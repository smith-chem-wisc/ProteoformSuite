using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

//Inspired by the class by the same name from Morpheus (http://cwenger.github.io/Morpheus) by Craig Wenger
namespace PS_0._00
{
    class ProteomeDatabaseReader
    {
        private static Dictionary<string, char> aminoAcidCodes;
        public static string oldPtmlistFilePath;
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
                                    description, new Modification(description, accession, featureType, position, targetAAs, monoisotopicMassShift, averageMassShift));
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
                string new_ptmlist_filepath = Path.Combine(Path.GetDirectoryName(oldPtmlistFilePath), "ptmlist.new.txt");
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile("http://www.uniprot.org/docs/ptmlist.txt", new_ptmlist_filepath);
                }
                string old_ptmlist = File.ReadAllText(oldPtmlistFilePath);
                string new_ptmlist = File.ReadAllText(new_ptmlist_filepath);
                if (string.Equals(old_ptmlist, new_ptmlist))
                {
                    File.Delete(new_ptmlist_filepath);
                }
                else
                {
                    File.Delete(oldPtmlistFilePath);
                    File.Move(new_ptmlist_filepath, oldPtmlistFilePath);
                }
            }
            catch { }
            return oldPtmlistFilePath;
        }

        public static IEnumerable<Protein> ReadUniprotXml(string uniprotXmlFile, int minPeptideLength, bool fixedMethionineCleavage)
        {
            using (FileStream uniprotXmlStream = new FileStream(uniprotXmlFile, FileMode.Open))
            {
                using (XmlReader xml = XmlReader.Create(uniprotXmlStream))
                {
                    string[] nodes = new string[6];

                    string dataset = null;
                    string accession = null;
                    string name = null;
                    string full_name = null;
                    string fragment = null;
                    string organism = null;
                    string gene_name = null;
                    string protein_existence = null;
                    string sequence_version = null;
                    string sequence = null;
                    string feature_type = null;
                    string feature_description = null;
                    int feature_position = -1;
                    string feature_position_status = null;
                    int feature_begin = -1;
                    string feature_begin_status = null;
                    int feature_end = -1;
                    string feature_end_status = null;

                    int begin = 0;
                    int end = -1;

                    Dictionary<int, List<string>> positionsAndPtms = new Dictionary<int, List<string>>();
                    List<SubsequenceVariant> subsequenceVariants = new List<SubsequenceVariant>();
                    while (xml.Read())
                    {
                        switch (xml.NodeType)
                        {
                            case XmlNodeType.Element: //start of an element <name>
                                nodes[xml.Depth] = xml.Name;
                                switch (xml.Name)
                                {
                                    case "entry":
                                        dataset = xml.GetAttribute("dataset");
                                        break;
                                    case "accession":
                                        if (accession == null)
                                        {
                                            accession = xml.ReadElementString();
                                        }
                                        break;
                                    case "name":
                                        if (xml.Depth == 2)
                                        {
                                            name = xml.ReadElementString();
                                        }
                                        else if (nodes[2] == "gene")
                                        {
                                            if (gene_name == null)
                                            {
                                                gene_name = xml.ReadElementString();
                                            }
                                        }
                                        else if (nodes[2] == "organism")
                                        {
                                            if (organism == null)
                                            {
                                                organism = xml.ReadElementString();
                                            }
                                        }
                                        break;
                                    case "fullName": //recommendedName > submittedName > alternativeName; because these appear in this order in uniprotxmls, this is true
                                        if (full_name == null)
                                        {
                                            full_name = xml.ReadElementString();
                                        }
                                        break;
                                    case "proteinExistence":
                                        protein_existence = xml.GetAttribute("type");
                                        break;
                                    case "feature":
                                        feature_type = xml.GetAttribute("type");
                                        feature_description = xml.GetAttribute("description");
                                        break;
                                    //positionType elements have default 'status' of certain
                                    case "position":
                                        if (xml.GetAttribute("position") != null)
                                        {
                                            feature_position = int.Parse(xml.GetAttribute("position")) - 1;
                                        }
                                        feature_position_status = xml.GetAttribute("status");
                                        break;
                                    case "begin":
                                        if (xml.GetAttribute("position") != null)
                                        {
                                            feature_begin = int.Parse(xml.GetAttribute("position")) - 1;
                                        }
                                        feature_position_status = xml.GetAttribute("status");
                                        break;
                                    case "end":
                                        if (xml.GetAttribute("position") != null)
                                        {
                                            feature_end = int.Parse(xml.GetAttribute("position")) - 1;
                                        }
                                        feature_position_status = xml.GetAttribute("status");
                                        break;
                                    case "sequence":
                                        fragment = xml.GetAttribute("fragment");
                                        sequence_version = xml.GetAttribute("version");
                                        sequence = xml.ReadElementString().Replace("\r", null).Replace("\n", null);
                                        end = sequence.Length - 1;
                                        break;
                                }
                                break;
                            case XmlNodeType.EndElement: //reached an end element </name>
                                switch (xml.Name)
                                {
                                    case "feature":
                                        switch (feature_type)
                                        {
                                            case "modified residue":
                                                //Add the modification to the dictionary binned by position
                                                List<string> modListAtPos;
                                                feature_description = feature_description.Split(';')[0];
                                                if (feature_position != -1)
                                                {
                                                    if (positionsAndPtms.TryGetValue(feature_position, out modListAtPos))
                                                    {
                                                        modListAtPos.Add(feature_description);
                                                    }
                                                    else
                                                    {
                                                        modListAtPos = new List<string> { feature_description };
                                                        positionsAndPtms.Add(feature_position, modListAtPos);
                                                    }
                                                }
                                                break;
                                            case "chain":
                                            case "signal peptide":
                                            case "propeptide":
                                            case "peptide":
                                                if (feature_begin != -1 && feature_end != -1 && feature_begin_status != null && feature_end_status != null)
                                                {
                                                    subsequenceVariants.Add(new SubsequenceVariant(feature_type, feature_begin, feature_end));
                                                }
                                                break;
                                            case "splice variant":
                                            case "sequence variant":
                                                break;
                                        }

                                        //reset feature values
                                        feature_type = null;
                                        feature_description = null;
                                        feature_position = -1;
                                        feature_position_status = null;
                                        feature_begin = -1;
                                        feature_begin_status = null;
                                        feature_end = -1;
                                        feature_end_status = null;
                                        break;

                                    case "entry":
                                        yield return new Protein(accession, name, fragment, begin, end, sequence, positionsAndPtms);

                                        foreach (SubsequenceVariant subseq_v in subsequenceVariants)
                                        {
                                            bool justMetCleavage = fixedMethionineCleavage && subseq_v.begin - 1 == begin && subseq_v.end == end;
                                            int subseq_length = subseq_v.end - subseq_v.begin + 1;
                                            if (!justMetCleavage && subseq_length != sequence.Length && subseq_length >= minPeptideLength)
                                            {
                                                yield return new Protein(accession, name, subseq_v.type, subseq_v.begin, subseq_v.end,
                                                    sequence.Substring(subseq_v.begin, subseq_v.end - subseq_v.begin + 1),
                                                    SegmentPtms(positionsAndPtms, subseq_v.begin, subseq_v.end));
                                            }
                                        }

                                        //reset values
                                        dataset = null;
                                        accession = null;
                                        name = null;
                                        full_name = null;
                                        fragment = null;
                                        organism = null;
                                        gene_name = null;
                                        protein_existence = null;
                                        sequence_version = null;
                                        sequence = null;
                                        feature_type = null;
                                        feature_description = null;
                                        feature_position = -1;
                                        feature_position_status = null;
                                        feature_begin = -1;
                                        feature_begin_status = null;
                                        feature_end = -1;
                                        feature_end_status = null;
                                        subsequenceVariants = new List<SubsequenceVariant>();
                                        positionsAndPtms = new Dictionary<int, List<string>>();
                                        break;
                                }
                                break;
                        }
                        uniprotXmlStream.Seek(0, SeekOrigin.Begin);
                    }
                }
            }
        }

        struct SubsequenceVariant
        {
            public string type;
            public int begin;
            public int end;
            public SubsequenceVariant(string feature_type, int feature_begin, int feature_end)
            {
                type = feature_type;
                begin = feature_begin;
                end = feature_end;
            }
        }

        static Dictionary<int, List<string>> SegmentPtms(Dictionary<int, List<string>> allPosPTMs, int begin, int end)
        {
            Dictionary<int, List<string>> segPosPTMs = new Dictionary<int, List<string>>();
            foreach (int position in allPosPTMs.Keys)
            {
                if (position >= begin && position <= end)
                {
                    segPosPTMs.Add(position, allPosPTMs[position]);
                }
            }
            return segPosPTMs;// the int is the amino acid position and the string[] are the different ptms at that position
        }
    }
}
