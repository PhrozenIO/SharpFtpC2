using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpC2.Responses
{
    internal class ResponseNotification : ResponseWrapper
    {
        public enum NotificationKind
        {
            AgentTerminated,
        }

        public NotificationKind Kind { get; set; }

        public override string DisplayName()
        {
            return Kind.ToString();
        }
    }
}
