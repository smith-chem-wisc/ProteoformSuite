using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS_0._00
{
    class ReadUniprotPtmList
    {
        private static Dictionary<string, char> aminoAcidCodes;

        public static string oldPtmFilePath;

        public Dictionary<string, ModData> ModTable
        {
            get
            {
                return this.ModTable;
            }
            set
            {
                ModTable = value;
            }
        }

        public Dictionary<string, ModData> rd_unip_ptms()
        {

            string ptmFilePath = GetPtmListFilePath();

            int modCount = CountMods(ptmFilePath);

            InitializeDictionaries();

            Dictionary<string, ModData> ModTable = new Dictionary<string, ModData>();

            ModTable = LoadModificationTable(ptmFilePath, modCount);

            return ModTable;
        }

        //static void WriteData(Dictionary<string, modData> modTable)
        //{
        //    foreach (KeyValuePair<string, modData> entry in modTable)
        //    {
        //        //Console.WriteLine("ID: {0} DATA: {1}", entry.Key, entry.Value.ToString());
        //    }
        //}

        static Dictionary<string, ModData> LoadModificationTable(string path, int numDiffMods)
        {
            Dictionary<string, ModData> mT = new Dictionary<string, ModData>();

            using (StreamReader uniprot_mods = new StreamReader(path))
            {

                ModData thisMod = new ModData();

                string modID = null;
                int count = 0;

                while (uniprot_mods.Peek() != -1)
                {

                    string line = uniprot_mods.ReadLine();
                    if (line.Length >= 2)
                    {
                        switch (line.Substring(0, 2))
                        {
                            case "ID":
                                thisMod = new ModData();
                                modID = line.Substring(5);
                                //Console.WriteLine("modID: " + modID);
                                break;
                            case "AC":
                                thisMod.AC = line.Substring(5);
                                //Console.WriteLine("AC: " + thisMod.AC);
                                break;
                            case "FT":
                                thisMod.FT = line.Substring(5);
                                //Console.WriteLine("FT: " + thisMod.FT);
                                break;
                            case "TG":
                                string amino_acids = line.Substring(5).TrimEnd('.');
                                string[] acids = new string[2];
                                char[] bases = new char[2];
                                if (amino_acids.Contains(" or "))
                                {
                                    amino_acids = amino_acids.Replace(" or ", "-"); // this handles blocked amino end (Asx) incorrectly for now.
                                }
                                if (amino_acids.Contains("-"))
                                {
                                    acids = amino_acids.Split('-');
                                    bases[0] = aminoAcidCodes[acids[0]];
                                    bases[1] = aminoAcidCodes[acids[1]];
                                }
                                else
                                {
                                    acids[0] = amino_acids;
                                    acids[1] = null;
                                    bases[0] = aminoAcidCodes[acids[0]];
                                    bases[1] = '\0';
                                }

                                thisMod.TG[0] = bases[0];
                                thisMod.TG[1] = bases[1];
                                //Console.WriteLine("TG0: {0} TG1: {1} ", thisMod.TG[0], thisMod.TG[1]);
                                break;
                            case "PP":
                                thisMod.PP = line.Substring(5);
                                break;
                            case "MM":
                                thisMod.MM = double.Parse(line.Substring(5));
                                break;
                            case "MA":
                                thisMod.MA = double.Parse(line.Substring(5));
                                break;
                            case "//":
                                mT.Add(modID, thisMod);
                                modID = null;
                                count++;

                                break;
                        }
                    }
                }
            }

            return mT;
        }

        private static void InitializeDictionaries()
        {
            aminoAcidCodes = new Dictionary<string, char>();
            aminoAcidCodes.Add("Alanine", 'A');
            aminoAcidCodes.Add("Arginine", 'R');
            aminoAcidCodes.Add("Asparagine", 'N');
            aminoAcidCodes.Add("Aspartate", 'D');
            aminoAcidCodes.Add("Cysteine", 'C');
            aminoAcidCodes.Add("Glutamate", 'E');
            aminoAcidCodes.Add("Glutamine", 'Q');
            aminoAcidCodes.Add("Glycine", 'G');
            aminoAcidCodes.Add("Histidine", 'H');
            aminoAcidCodes.Add("Isoleucine", 'I');
            aminoAcidCodes.Add("Leucine", 'L');
            aminoAcidCodes.Add("Lysine", 'K');
            aminoAcidCodes.Add("Methionine", 'M');
            aminoAcidCodes.Add("Phenylalanine", 'F');
            aminoAcidCodes.Add("Proline", 'P');
            aminoAcidCodes.Add("Serine", 'S');
            aminoAcidCodes.Add("Threonine", 'T');
            aminoAcidCodes.Add("Tryptophan", 'W');
            aminoAcidCodes.Add("Tyrosine", 'Y');
            aminoAcidCodes.Add("Valine", 'V');
            aminoAcidCodes.Add("Undefined", 'X'); // we'll have to deal with this at some point.
            aminoAcidCodes.Add("Selenocysteine", 'U');
        }

        static int CountMods(string path)
        {
            int count = 0;

            using (StreamReader modCounter = new StreamReader(path))
            {
                while (modCounter.Peek() != -1)
                {
                    string line = modCounter.ReadLine();
                    if (line.Length >= 2)
                    {
                        switch (line.Substring(0, 2))
                        {
                            case "//":
                                count++;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            return count;
        }

        static string GetPtmListFilePath()
        {

            try
            {
                string new_ptmlist_filepath = Path.Combine(Path.GetDirectoryName(oldPtmFilePath), "ptmlist.new.txt");
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile("http://www.uniprot.org/docs/ptmlist.txt", new_ptmlist_filepath);
                }
                string old_ptmlist = File.ReadAllText(oldPtmFilePath);
                string new_ptmlist = File.ReadAllText(new_ptmlist_filepath);
                if (string.Equals(old_ptmlist, new_ptmlist))
                {
                    File.Delete(new_ptmlist_filepath);
                }
                else
                {
                    File.Delete(oldPtmFilePath);
                    File.Move(new_ptmlist_filepath, oldPtmFilePath);
                }
            }
            catch
            {

            }

            return oldPtmFilePath;
        }
    }
}
