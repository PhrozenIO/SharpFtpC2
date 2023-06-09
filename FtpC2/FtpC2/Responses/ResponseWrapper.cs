using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpC2.Responses
{
    public class ResponseWrapper
    {
        public Guid TaskId { get; set; }
        public Guid AgentId { get; set; }
        public DateTime DateTime { get; set; }

        public string? ResponseType { get; set; }

        public virtual string DisplayName() { return ""; }
    }
}
