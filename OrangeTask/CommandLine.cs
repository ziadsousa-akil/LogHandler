using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrangeTask
{
    public class CommandLine
    {
        public enum Status
        {
            Fail,
            Pass
        }
        public static void Write(string Message,string Variable,Status status,bool showStatus = true)
        {
            Console.ForegroundColor = (status == Status.Fail)? ConsoleColor.Red: ConsoleColor.Green;
            Console.WriteLine(Message + " " + Variable + ((showStatus)? $" [{status}]":""));
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
}
