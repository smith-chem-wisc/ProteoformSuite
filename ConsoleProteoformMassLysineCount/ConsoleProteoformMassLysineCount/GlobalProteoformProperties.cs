using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleProteoformMassLysineCount
{
    class GlobalProteoformProperties
    {
        private bool methionineOxidation = true;
        private bool carbamidoMethylation = true;
        private bool cleaved_N_TerminalMethionine = true;
        private string lysineIsotopes = "n"; // default to natural abundance 'n'. NeuCode light 'l' and heavy'h' are options.
        private int maxPTMs = 0;
        private int numberOfDecoyDatabases = 0;

        public bool MethionineOxidation
        {
            get
            {
                bool validAnswer = false;
                string answer = null;
                do
                {
                    Console.WriteLine("Methionine Oxidation (T/F)?");
                    answer = Console.ReadLine();

                    switch (answer)
                    {
                        case "T":
                            validAnswer = true;
                            break;
                        case "t":
                            validAnswer = true;
                            break;
                        case "F":
                            validAnswer = true;
                            break;
                        case "f":
                            validAnswer = true;
                            break;
                        default:
                            break; 
                    }
                } while (validAnswer == false);

                if (answer == "T" || answer == "t")
                {
                    this.methionineOxidation = true;
                }
                else
                {
                    this.methionineOxidation = false;
                }
                return this.methionineOxidation;
            }

        }

        public bool Carbmidomthylaton
        {
            get
            {
                bool validAnswer = false;
                string answer = null;
                do
                {
                    Console.WriteLine("Cabamidomethylation of cysteine (T/F)?");
                    answer = Console.ReadLine();

                    switch (answer)
                    {
                        case "T":
                            validAnswer = true;
                            break;
                        case "t":
                            validAnswer = true;
                            break;
                        case "F":
                            validAnswer = true;
                            break;
                        case "f":
                            validAnswer = true;
                            break;
                        default:
                            break;
                    }
                } while (validAnswer == false);

                if (answer == "T" || answer == "t")
                {
                    this.carbamidoMethylation = true;
                }
                else
                {
                    this.carbamidoMethylation = false;
                }
                return this.carbamidoMethylation;
            }

        }

        public bool Cleaved_N_TerminalMethionine
        {
            get
            {
                bool validAnswer = false;
                string answer = null;
                do
                {
                    Console.WriteLine("Cleaved N-terminal methionine (T/F)?");
                    answer = Console.ReadLine();

                    switch (answer)
                    {
                        case "T":
                            validAnswer = true;
                            break;
                        case "t":
                            validAnswer = true;
                            break;
                        case "F":
                            validAnswer = true;
                            break;
                        case "f":
                            validAnswer = true;
                            break;
                        default:
                            break;
                    }
                } while (validAnswer == false);

                if (answer == "T" || answer == "t")
                {
                    this.cleaved_N_TerminalMethionine = true;
                }
                else
                {
                    this.cleaved_N_TerminalMethionine = false;
                }
                return this.cleaved_N_TerminalMethionine;
            }

        }

        public string LysineIsotopes
        {
            get
            {
                bool validAnswer = false;
                string answer = null;
                do
                {
                    Console.WriteLine("Lysine isotopes: (n)atural; neucode (l)ight; neucode (h) (n,l,h)?");
                    answer = Console.ReadLine();

                    switch (answer)
                    {
                        case "n":
                            validAnswer = true;
                            break;
                        case "l":
                            validAnswer = true;
                            break;
                        case "h":
                            validAnswer = true;
                            break;
                        default:
                            break;
                    }
                } while (validAnswer == false);

                if (answer == "n")
                {
                    this.lysineIsotopes = "n";
                }
                else if (answer == "l")
                {
                    this.lysineIsotopes = "l";
                }
                else
                {
                    this.lysineIsotopes = "h";
                }
                return this.lysineIsotopes;
            }

        }

        public int MaxPTMs
        {
            get
            {
                bool validAnswer = false;
                int answer = 0;
                const int max = 4;

                do
                {
                    Console.WriteLine("Enter maximum number of PTMs allowed per proteoform (integer<={0}):",max);
                    string consoleInput = Console.ReadLine();

                    try
                    {
                        answer = Convert.ToInt16(consoleInput);
                        if (answer <= max)
                        {
                            validAnswer = true;
                            this.maxPTMs = answer;
                        }
                    }
                    catch (Exception)
                    {

                        validAnswer = false;
                    }
                } while (validAnswer == false);

                return this.maxPTMs;
            }

        }

        public int NumberOfDecoyDatabases
        {
            get
            {
                bool validAnswer = false;
                int answer = 0;
                const int max = 99;

                do
                {
                    Console.WriteLine("Enter maximum number of decoy databses desired (integer<={0}):", max);
                    string consoleInput = Console.ReadLine();

                    try
                    {
                        answer = Convert.ToInt16(consoleInput);
                        if (answer <= max)
                        {
                            validAnswer = true;
                            this.numberOfDecoyDatabases = answer;
                        }
                    }
                    catch (Exception)
                    {

                        validAnswer = false;
                    }
                } while (validAnswer == false);

                return this.numberOfDecoyDatabases;
            }

        }

    }
}
