using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FtpAgent;

public static class UX
{
    public static void DisplayBanner()
    {
        Console.WriteLine("                    ^?P?                                                        ");
        Console.WriteLine("                   ^! :Y^                               ^!~                     ");
        Console.WriteLine("                   ?P   :~                       :~~   ~YP##^                   ");
        Console.WriteLine("                   ^#7   !G!                   ~5Y~  ^PP?!7B~                   ");
        Console.WriteLine("                    ^P5:  ~G5:                ~@Y   ^J~                         ");
        Console.WriteLine("                      7?    JG^               !@7                               ");
        Console.WriteLine("                             BP              !P5                                ");
        Console.WriteLine("                        ^   ^&7            :PJ^ Y!                              ");
        Console.WriteLine("                        J^  7@:            !J  5&                               ");
        Console.WriteLine("                        ^PJ~:BY                #@^                              ");
        Console.WriteLine("                          ?BP?@Y              ?@#^                              ");
        Console.WriteLine("                            G@@@5:G####BB!!#GB@J                                ");
        Console.WriteLine("                            J&@@P:P&@@@@B~7@@@G:                                ");
        Console.WriteLine("                          !G^^&@@G?7J5Y7?5&@@? 5J                               ");
        Console.WriteLine("                         J@Y:G@@@@@@#?G&@@@@@#~!@G:                             ");
        Console.WriteLine("                        ?@@J 5@@@@@@@@@@@@@@@B^!&@P                             ");
        Console.WriteLine("                        G@@?^#@@@@@@@@@@@@@@@@7^&@@^                            ");
        Console.WriteLine("                        Y@&:!&&&@&#@@@@@&#@&&&Y G@B                             ");
        Console.WriteLine("                        :G@!^YYJYJ?7&@@J?YJYJY!:#&~                             ");
        Console.WriteLine("                          ~ !YYJJP&^G@@^BBJJJYJ ~^                              ");
        Console.WriteLine("                         ~!:&@@@#Y7:75Y:!?B&@@@?:7                              ");
        Console.WriteLine("                        ^@&!JB#&&@#:5@#:P@@&#B5!G@?                             ");
        Console.WriteLine("                        :YB#P5555J?:5BG^7J5555PB#P~                             ");
        Console.WriteLine("                           :^~~7YYYJ7:775YY?!~^:                                ");
        Console.WriteLine("                              7^Y@@@@B&@@@B:?                                   ");
        Console.WriteLine("                              ^Y?5#@@@@@&P?Y~                                   ");
        Console.WriteLine("                                YJP?PJ5YYYY~                                    ");
        Console.WriteLine("                                 ~?5@?#B!7                                      ");
        Console.WriteLine("                                                                                ");                        
        Console.WriteLine("                     https://www.github.com/DarkCoderSc                         ");        
        Console.WriteLine("                                                                                ");
    }

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

    // TODO write as an extension of DataTable
    public static void DisplayTableToConsole(DataTable table)
    {
        if (table.Rows.Count == 0)
            return;

        int[] columnWidths = new int[table.Columns.Count];

        // Prepare Padding Information

        // Header
        for (int i = 0; i < table.Columns.Count; i++)
        {
            string columnName = table.Columns[i].ColumnName;

            if (columnName.Length > columnWidths[i])
                columnWidths[i] = columnName.Length;
        }

        // Body
        for (int i = 0; i < table.Rows.Count; i++)
        {
            DataRow row = table.Rows[i];
            string[] values = row.ItemArray.Cast<string>().ToArray();

            for (int n = 0; n < values.Length; n++)
            {
                if (values[n].Length > columnWidths[n])
                    columnWidths[n] = values[n].Length;
            }
        }

        // Sum of column widths +
        // Extra padding of two chars for each columns +
        // Borders between columns
        int headerLength = columnWidths.Sum() + (columnWidths.Length * 2) + (columnWidths.Length - 1);
        //var drawLine = () => Console.WriteLine("|" + new string('-', headerLength) + "|");
        Action drawLine = () => Console.WriteLine("|" + new string('-', headerLength) + "|");

        // Display Content

        drawLine();

        // Header                       
        int index = 0;
        foreach (DataColumn column in table.Columns)
        {
            if (index == 0)
                Console.Write("|");

            Console.Write(" ");

            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.Write(column.ColumnName.PadRight(columnWidths[index++] + 1));

            Console.ResetColor();

            Console.Write("|");
        }

        Console.WriteLine();

        drawLine();


        // Body            
        foreach (DataRow row in table.Rows)
        {
            string[] values = row.ItemArray.Cast<string>().ToArray();

            index = 0;
            foreach (string value in values)
            {
                if (index == 0)
                    Console.Write("|");

                Console.Write(" ");
                Console.Write(value.PadRight(columnWidths[index++] + 1));

                Console.Write("|");
            }

            Console.WriteLine();
        }

        drawLine();
    }

    public static void DisplayControllerPrompt()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;

        Console.Write("controller");

        Console.ResetColor();

        Console.Write(" > ");
    }

    public static void DisplayAgentPrompt(Agent? agent)
    {
        if (agent == null)
            return;

        Console.Write($"{agent.User}@");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(agent.Computer);
        Console.ResetColor();
        Console.Write($"({agent.Domain})[");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(agent.WorkDir);
        Console.ResetColor();
        Console.Write("] > ");
    }
}

