using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManyConsole;

namespace ProteoformSuite
{
    class UserInterfaceCommand : ConsoleCommand
    {
        public UserInterfaceCommand()
        {
            this.IsCommand("User_Interface", "Guided console command setup.");
        }

        public override int Run(string[] remainingArguments)
        {
            var commands = Program.GetCommands();

            // USER PROMPTS: commands
            List<string> commandList = new List<string> { "help", "run", "proteoform_database", "exit" };
            string commandPrompt = String.Join(Environment.NewLine, ["Choose a command:", "help -- Display help menu for commandline arguments", "run -- Execute the full ProteoformSuite pipeline.", "proteoform_database -- Construct a proteoform XML.", "exit -- Exit this program"]);

            // User prompts: proteoform_database parameters
            List<string> trueFalseList = new List<string> { "T", "t", "True", "true", "TRUE", "F", "f", "False", "false", "FALSE" };
            List<string> trueList = new List<string> { "T", "t", "True", "true", "TRUE" };
            List<string> falseList = new List<string> { "F", "f", "False", "false", "FALSE" };
            List<string> lysineOptList = new List<string> { "n", "l", "h" };
            string methionineOxidationPrompt = "Methionine Oxidation? (T/F)";
            string carbamPrompt = "Carbamidomethylation of cysteine? (T/F)";
            string cleavedNTermMetPrompt = "Cleaved N-terminal methionine? (T/F)";
            string lysineIsotopesPrompt = "Lysine isotopes: natural; neucode light; neucode heavy? (" + lysineOptList + ")";
            string maxPtmsPrompt = "Enter the maximum number of PTMs to allow per proteoform. (integer <= " + DatabaseGenerator.maxMaxPtms;

            //User prompts: proteoform_database files
            string proteoformXmlPrompt = "Please enter the path for a proteoform XML. Default: "; //TODO: choose and implement default pfXML path
            string uniprotXmlPrompt = "Please enter the path for a UniProt XML. Default: "; //TODO: choose and implement default uXML path
            string ensemblFaaPrompt = "Please enter the path for an Ensemble protein fasta. Default: ";
            // TODO: choose and implement default ensembl fasta path
            // TODO: Consider other commands to implement
            // TODO: Consider which parameters to specify with flags following the command

            // RUN COMMANDS after prompting and parsing arguments
            string command = GetUserResponse(commandPrompt);
            if (command == "exit") { }
            else if (command == "help") { ConsoleCommandDispatcher.DispatchCommand(commands, args, Console.Out); }
            else
            {
                string[] newargs = new string[0];

                if (command == "proteoform_database")
                {
                    bool methionineOxidation = trueList.Contains(GetUserResponse(methionineOxidationPrompt, trueFalseList));
                    bool carbam = trueList.Contains(GetUserResponse(carbamPrompt, trueFalseList));
                    bool cleaved_met = trueList.Contains(GetUserResponse(cleavedNTermMetPrompt, trueFalseList));
                    string lysine_isotopes = GetUserResponse(lysineIsotopesPrompt, lysineOptList);
                    int maxPtms = GetUserResponse(maxPtmsPrompt, Enumerable.Range(0, DatabaseGenerator.maxMaxPtms).ToList());

                    string proteoformXml = GetUserResponse(proteoformXmlPrompt);
                    string uniprotXml = GetUserResponse(uniprotXmlPrompt);
                    string ensemblFaa = GetUserResponse(ensemblFaaPrompt);
                    newargs = new string[7] { command, "-f", proteoformXml, "-u", uniprotXml, "-e", ensemblFaa }; //TODO: test that this works to specify the correct args for ManyConsole
                }
                else if (command == "run")
                {
                    string proteoformXml = GetUserResponse(proteoformXmlPrompt);
                    string uniprotXml = GetUserResponse(uniprotXmlPrompt);
                    string ensemblFaa = GetUserResponse(ensemblFaaPrompt);
                    //TODO: implement with all of the prompted 
                }

                ConsoleCommandDispatcher.DispatchCommand(commands, newargs, Console.Out); // TODO: implement error notification if empty newargs at this point
            }

            return 0;

        }

        //User response methods (+2 overload)
        public string GetUserResponse(string prompt, List<string> acceptableAnswers)
        {
            string response = null;
            do
            {
                Console.WriteLine(prompt);
                response = Console.ReadLine().Trim();
            } while (!acceptableAnswers.Contains(response));
            return response;
        }
        public int GetUserResponse(string prompt, List<int> acceptableAnswers)
        {
            int response = Int32.MinValue;
            do
            {
                try
                {
                    Console.WriteLine(prompt);
                    response = Int32.Parse(Console.ReadLine().Trim());
                } catch { }
            } while (!acceptableAnswers.Contains(response));
            return response;
        }
        public string GetUserResponse(string prompt)
        {
            Console.WriteLine(prompt);
            return Console.ReadLine().Trim();
        }
    }
}
