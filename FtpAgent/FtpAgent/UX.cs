using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FtpAgent
{
    internal class UX
    {
        public static void ColorBackTicks(string message)
        {

            int lastIndex = 0;

            for (int i = 0; i < message.Length; i++)
            {
                if (i + 1 < message.Length && message[i] == '`' && message[i + 1] != '`')
                {
                    Console.Write(message.Substring(lastIndex, i - lastIndex));
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("`");
                    i++;

                    int j = message.IndexOf("`", i);
                    Console.Write(message.Substring(i, j - i));
                    Console.Write("`");
                    Console.ResetColor();

                    i = j;
                    lastIndex = i + 1;
                }
            }

            Console.WriteLine(message.Substring(lastIndex));
        }

        public static void DisplayNotification(string message, char icon, ConsoleColor color)
        {
            Console.Write('[');
            Console.ForegroundColor = color;
            Console.Write(icon);
            Console.ResetColor();
            Console.Write($"] ");

            ColorBackTicks(message);
        }

        public static void DisplayInfo(string message)
        {
            DisplayNotification(message, '*', ConsoleColor.DarkCyan);
        }

        public static void DisplayWarning(string message)
        {
            DisplayNotification(message, '!', ConsoleColor.Yellow);
        }

        public static void DisplayError(string message)
        {
            DisplayNotification(message, 'x', ConsoleColor.Red);
        }

        public static void DisplaySuccess(string message)
        {
            DisplayNotification(message, '+', ConsoleColor.Green);
        }
    }
}
