using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpC2.Tasks
{
    public class TaskWrapper
    {
        public Guid Id { get; set; }     
        public Guid AgentId { get; set; }
        public string? TaskType { get; set; }
    }
}
