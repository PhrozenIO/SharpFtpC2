using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace FtpAgent
{
    public class Agent
    {
        public Guid Id { get; set; }
        public DateTime DateTime { get; set; }        
        public string? User { get; set; }
        public string? Computer { get; set; }
        public string? Domain { get; set; }
        public string? WorkPath { get; set; }
        public string? WorkDir { get; set; }
        public int ProcessId { get; set; }
        public bool Is64BitProcess { get; set; }
        public string? OSVersion { get; set; }
        public string? ImagePath { get; set; }

        public string DisplayName()
        {
            return $"{this.User ?? "Unknown"}@{this.Computer ?? "Unknown"}";
        }

        public void Refresh(Guid? Id)
        {
            if (Id == null) 
                throw new ArgumentNullException("id");

            this.Id = Id.Value;
            this.DateTime = DateTime.Now;
            this.User = Environment.UserName;
            this.Computer = Environment.MachineName;
            this.Domain = Environment.UserDomainName;
            this.WorkPath = Environment.CurrentDirectory;
            this.WorkDir = Path.GetFileName(Path.GetDirectoryName(this.WorkPath));
            this.Is64BitProcess = Environment.Is64BitProcess;
            this.OSVersion = Environment.OSVersion.VersionString;

            this.ImagePath = Process.GetCurrentProcess()?.MainModule?.FileName ?? "";
            this.ProcessId = Process.GetCurrentProcess().Id;         
        }
    }
}
