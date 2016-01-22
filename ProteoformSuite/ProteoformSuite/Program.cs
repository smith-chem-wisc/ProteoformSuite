using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ManyConsole;

namespace ProteoformSuite
{

    class Program
    {
        static void Main(string[] args)
        {

            // Command-line arguments: C:\\>ProteoformSuite.exe <command> <parameters>
            var commands = GetCommands();
            if (args.Length > 0)
            {
                ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out);
            }

            // TODO: create a GUI and place open-command here, i.e. double-click or no command selected
            // TODO: GUI encourages export of proteoformXML to repository/TDP before saving visualization and results
            else
            {
                ConsoleCommandDispatcher.DispatchCommand(commands, new string[] { "user_interface" }, Console.Out);
            }

        } // end of main

        //MAIN METHODS
        //Get the run command classes
        public static IEnumerable<ConsoleCommand> GetCommands()
        {
            return ConsoleCommandDispatcher.FindCommandsInSameAssemblyAs(typeof(Program));
        }

    }

}
