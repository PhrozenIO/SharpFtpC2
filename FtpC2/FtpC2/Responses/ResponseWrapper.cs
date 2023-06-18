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
