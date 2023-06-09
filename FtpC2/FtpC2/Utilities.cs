using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FtpC2
{
    internal class Utilities
    {
        public static void CheckIntegerArgument(CommandOption argument)
        {
            if (argument.Value() == null)
                throw new ArgumentNullException(argument.ValueName);

            if (!uint.TryParse(argument.Value(), out uint value))
                throw new FormatException($"`{argument.Value()}` is not a valid positive integer value.");
        }

        public static string[] SplitEx(string value, string needle = @"\+&")
        {
            string pattern = $"([^{needle}]+)";

            MatchCollection matches = Regex.Matches(value, pattern);

            return matches.Cast<Match>().Select(m => m.Value.Trim()).ToArray();
        }

        public static string TimeSince(DateTime dateTime)
        {
            var timeSpan = DateTime.Now.Subtract(dateTime);
            if (timeSpan.TotalSeconds < 60)
            {
                return $"{timeSpan.Seconds} seconds ago";
            }
            else if (timeSpan.TotalMinutes < 60)
            {
                return $"{timeSpan.Minutes} minutes ago";
            }
            else if (timeSpan.TotalHours < 24)
            {
                return $"{timeSpan.Hours} hours ago";
            }
            else if (timeSpan.TotalDays < 30)
            {
                return $"{timeSpan.Days} days ago";
            }
            else if (timeSpan.TotalDays < 365)
            {
                return $"{timeSpan.Days / 30} months ago";
            }
            else
            {
                return $"{timeSpan.Days / 365} years ago";
            }
        }
    }
}
