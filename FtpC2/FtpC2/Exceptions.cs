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

namespace FtpC2
{
    internal class ExitProgramExceptions : Exception { };

    internal class HttpError405 : Exception
    {
        public HttpError405() : base("405 Method Not Allowed") { }
    };

    internal class HttpError404 : Exception
    {
        public HttpError404() : base("404 Not Found") { }
    };
}
