using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

//Adapted from the class by the same name from Morpheus (http://cwenger.github.io/Morpheus) by Craig Wenger
namespace PS_0._00
{
    class ProteomeDatabaseReader
    {
        private static Dictionary<string, char> aminoAcidCodes;
        public static string oldPtmFilePath;
        public Dictionary<string, Modification> ModTable { get; set; }

        public Dictionary<string, Modification> ReadUniprotPtmlist()
        {
            string ptmFilePath = GetPtmlistPath_AfterFileRefresh();
            int modCount = File.ReadAllText(ptmFilePath).Split(new string[] { "//" }, StringSplitOptions.None).Length - 2; //There will be an extra element from the legend
            InitializeAminoAcidCodes();
            Dictionary<string, Modification> ModTable = new Dictionary<string, Modification>();
            ModTable = LoadUniprotModifications(ptmFilePath, modCount);
            return ModTable;
        }

        private static void InitializeAminoAcidCodes()
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

        static Dictionary<string, Modification> LoadUniprotModifications(string path, int numDiffMods)
        {
            Dictionary<string, Modification> uniprotModifications = new Dictionary<string, Modification>();
            using (StreamReader uniprot_mods = new StreamReader(path))
            {
                int count = 0;
                string description = "";
                string accession = "";
                string featureType = "";
                string position = "";
                double monoisotopicMassShift = 0;
                double averageMassShift = 0;
                char[] targetAAs = new char[2];

                //Read lines and enter ptm information into the dictionary
                while (uniprot_mods.Peek() != -1)
                {
                    string line = uniprot_mods.ReadLine();
                    if (line.Length >= 2)
                    {
                        switch (line.Substring(0, 2))
                        {
                            case "ID":
                                description = line.Substring(5);
                                break;
                            case "AC":
                                accession = line.Substring(5);
                                break;
                            case "FT":
                                featureType = line.Substring(5);
                                break;
                            case "PP":
                                position = line.Substring(5);
                                break;
                            case "MM":
                                monoisotopicMassShift = double.Parse(line.Substring(5));
                                break;
                            case "MA":
                                averageMassShift = double.Parse(line.Substring(5));
                                break;
                            case "TG": 
                                string amino_acids = line.Substring(5).TrimEnd('.');
                                string[] acids = amino_acids.Split(new string[] { " or ", "-" }, StringSplitOptions.None);
                                char[] bases = new char[acids.Length];
                                for (int i = 0; i < acids.Length; i++)
                                {
                                    bases[i] = aminoAcidCodes[acids[i]];
                                }
                                targetAAs = bases;
                                break;
                            case "//":
                                uniprotModifications.Add(
                                    description,  new Modification(description, accession, featureType, position, targetAAs, monoisotopicMassShift, averageMassShift));
                                count++;
                                break;
                        }
                    }
                }
            }
            return uniprotModifications;
        }

        static string GetPtmlistPath_AfterFileRefresh()
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
            catch { }
            return oldPtmFilePath;
        }
    }
}
