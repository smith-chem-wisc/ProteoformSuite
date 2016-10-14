using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;


//Inspired by the class by the same name from Morpheus (http://cwenger.github.io/Morpheus) by Craig Wenger
namespace ProteoformSuiteInternal
{
    public class ProteomeDatabaseReader
    {
        public static string oldPtmlistFilePath;
        public Dictionary<string, Modification> ModTable { get; set; }
        private static Dictionary<string, char> aminoAcidCodes = new Dictionary<string, char>()
        {
            {"Alanine", 'A'},
            {"Arginine", 'R'},
            {"Asparagine", 'N'},
            {"Aspartate", 'D'},
            {"Cysteine", 'C'},
            {"Glutamate", 'E'},
            {"Glutamine", 'Q'},
            {"Glycine", 'G'},
            {"Histidine", 'H'},
            {"Isoleucine", 'I'},
            {"Leucine", 'L'},
            {"Lysine", 'K'},
            {"Methionine", 'M'},
            {"Phenylalanine", 'F'},
            {"Proline", 'P'},
            {"Serine", 'S'},
            {"Threonine", 'T'},
            {"Tryptophan", 'W'},
            {"Tyrosine", 'Y'},
            {"Valine", 'V'},
            {"Undefined", 'X'}, // we'll have to deal with this at some point.
            {"Selenocysteine", 'U'}
        };

        public Dictionary<string, Modification> ReadUniprotPtmlist()
        {
            string ptmFilePath = GetPtmlistPath_AfterFileRefresh();
            int modCount = File.ReadAllText(ptmFilePath).Split(new string[] { "//" }, StringSplitOptions.None).Length - 2; //There will be an extra element from the legend
            return LoadUniprotModifications(ptmFilePath, modCount);
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
                                description = line.Substring(5); break;
                            case "AC":
                                accession = line.Substring(5); break;
                            case "FT":
                                featureType = line.Substring(5); break;
                            case "PP":
                                position = line.Substring(5); break;
                            case "MM":
                                monoisotopicMassShift = double.Parse(line.Substring(5)); break;
                            case "MA":
                                averageMassShift = double.Parse(line.Substring(5)); break;
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
                    client.DownloadFile("http://www.uniprot.org/docs/ptmlist.txt", new_ptmlist_filepath);
                string old_ptmlist = File.ReadAllText(oldPtmlistFilePath);
                string new_ptmlist = File.ReadAllText(new_ptmlist_filepath);
                if (string.Equals(old_ptmlist, new_ptmlist))
                    File.Delete(new_ptmlist_filepath);
                else
                {
                    File.Delete(oldPtmlistFilePath);
                    File.Move(new_ptmlist_filepath, oldPtmlistFilePath);
                }
            }
            catch { }
            return oldPtmlistFilePath;
        }

        public static Protein[] ReadUniprotXml(string uniprotXmlFile, Dictionary<string, Modification> uniprot_modification_table, int minPeptideLength, bool fixedMethionineCleavage)
        {
            ConcurrentBag<Protein> bag_protein_list = new ConcurrentBag<Protein>();
            using (var stream = new FileStream(uniprotXmlFile, FileMode.Open))
            {
                Stream uniprotXmlFileStream;
                if (uniprotXmlFile.EndsWith(".gz"))
                    uniprotXmlFileStream = new GZipStream(stream, CompressionMode.Decompress);
                else uniprotXmlFileStream = stream;

                List<XElement> entries = new List<XElement>();
                using (XmlReader uniprotXmlReader = XmlReader.Create(uniprotXmlFileStream))
                {
                    uniprotXmlReader.MoveToContent();
                    while (uniprotXmlReader.Read())
                    {
                        if (uniprotXmlReader.NodeType == XmlNodeType.Element && uniprotXmlReader.Name == "entry")
                            entries.Add(XElement.ReadFrom(uniprotXmlReader) as XElement);
                    }
                }

                //PARALLEL PROBLEM
               // Parallel.ForEach<XElement>(entries, entry =>
                foreach (XElement entry in entries)
                {
                    //Used fields
                    string dataset = GetAttribute(entry, "dataset");
                    string accession = GetChild(entry, "accession").Value;
                    string full_name = GetDescendant(entry, "fullName").Value;
                    IEnumerable<XElement> features = from node in entry.Elements() where node.Name.LocalName == "feature" select node;
                    IEnumerable<XElement> dbReferences = from node in entry.Elements() where node.Name.LocalName == "dbReference" select node;
                    List<GoTerm> goTerms = new List<GoTerm>();
                    XElement sequence_elem = GetChild(entry, "sequence");
                    string sequence = sequence_elem.Value.Replace("\r", null).Replace("\n", null);
                    string fragment = GetAttribute(sequence_elem, "fragment");
                    if (fragment == "" || fragment == null)
                    {
                        fragment = "full";
                        if (fixedMethionineCleavage) fragment += "-met-cleaved";
                    }
                    int begin = 0;
                    int end = sequence.Length - 1;
                    Dictionary<int, List<Modification>> positionsAndPtms = new Dictionary<int, List<Modification>>();

                    //Other fields
                    //Full name follows desired order: recommendedName > submittedName > alternativeName because these appear in this order in UniProt-XMLs
                    string name = GetChild(entry, "name").Value;
                    string organism = GetChild(GetChild(entry, "organism"), "name").Value;
                    string gene_name = GetChild(GetChild(entry, "gene"), "name").Value;
                    string protein_existence = GetAttribute(GetChild(entry, "proteinExistence"), "type");
                    string sequence_version = GetAttribute(sequence_elem, "version");

                    //Process dbReferences to retrieve Gene Ontology Terms
                    foreach (XElement dbReference in dbReferences)
                    {
                        string dbReference_type = GetAttribute(dbReference, "type");
                        if(dbReference_type == "GO")
                        {
                            GoTerm go = new GoTerm();
                            string ID = GetAttribute(dbReference, "id");
                            go.id = ID.Split(':')[1].ToString();
                            IEnumerable<XElement> dbProperties = from XElement in dbReference.Elements() where XElement.Name.LocalName == "property" select XElement;

                            foreach (XElement property in dbProperties)
                            {
                                string type = GetAttribute(property, "type");
                                if (type == "term")
                                {
                                    string description = GetAttribute(property, "value");
                                    switch (description.Split(':')[0].ToString())
                                    {
                                        case "C":
                                            go.aspect = aspect.cellularComponent;
                                            go.description = description.Split(':')[1].ToString();
                                            break;
                                        case "F":
                                            go.aspect = aspect.molecularFunction;
                                            go.description = description.Split(':')[1].ToString();
                                            break;
                                        case "P":
                                            go.aspect = aspect.biologicalProcess;
                                            go.description = description.Split(':')[1].ToString();
                                            break;
                                    }
                                    goTerms.Add(go);
                                }
                            }                            
                        }
                    }

                    //Process the modified residues
                    foreach (XElement feature in features)
                    {
                        string feature_type = GetAttribute(feature, "type");
                        if (feature_type == "modified residue")
                        {
                            XElement feature_position_elem = GetDescendant(feature, "position");
                            int feature_position = ConvertPositionElem(feature_position_elem);
                            string feature_description = GetAttribute(feature, "description").Split(';')[0];
                            if (feature_position >= 0 && uniprot_modification_table.ContainsKey(feature_description))
                            {
                                List<Modification> modListAtPos;
                                if (positionsAndPtms.TryGetValue(feature_position, out modListAtPos))
                                    modListAtPos.Add(uniprot_modification_table[feature_description]);
                                else
                                {
                                    modListAtPos = new List<Modification> { uniprot_modification_table[feature_description] };
                                    positionsAndPtms.Add(feature_position, modListAtPos);
                                }
                            }
                        }
                    }

                    //Add the full length protein, and then add the fragments with segments of the above modification dictionary
                    bag_protein_list.Add(new Protein(accession, full_name, fragment, begin, end, sequence, goTerms, positionsAndPtms));
                    //MessageBox.Show("added " + new Protein(accession, name, fragment, begin, end, sequence, positionsAndPtms).ToString());

                    //PARALLEL PROBLEM
                    //Parallel.ForEach<XElement>(features, feature =>
                    foreach (XElement feature in features)
                    {
                        string feature_type = GetAttribute(feature, "type");
                        switch (feature_type)
                        {
                            case "chain":
                            case "signal peptide":
                            case "propeptide":
                            case "peptide":
                                XElement feature_begin_elem = GetDescendant(feature, "begin");
                                XElement feature_end_elem = GetDescendant(feature, "end");
                                int feature_begin = ConvertPositionElem(feature_begin_elem);
                                int feature_end = ConvertPositionElem(feature_end_elem);
                                if (feature_begin != -1 && feature_end != -1)
                                {
                                    bool justMetCleavage = fixedMethionineCleavage && feature_begin - 1 == begin && feature_end == end;
                                    string subsequence = sequence.Substring(feature_begin, feature_end - feature_begin + 1);
                                    if (!justMetCleavage && subsequence.Length != sequence.Length && subsequence.Length >= minPeptideLength)
                                        bag_protein_list.Add(new Protein(accession, full_name, feature_type, feature_begin, feature_end, subsequence, goTerms,
                                            SegmentPtms(positionsAndPtms, feature_begin, feature_end)));
                                }
                                break;
                            case "splice variant":
                            case "sequence variant":
                                break;
                        }
                    } //});
                } //}); 
                
            }

            //List<Protein> proteins = new List<Protein>();
            //foreach (Protein p in bag_protein_list) proteins.Add(p);
            return bag_protein_list.ToArray();
        }

        //READING IN BOTTOM-UP MORPHEUS FILE
        public static List<Psm> ReadBUFile(string filename)
        {
            List<Psm> psm_list = new List<Psm>();
            string[] lines = File.ReadAllLines(filename);

            int i = 1;
            bool qLessThan1 = true;
            //only add PSMs with q less than 1. this assumes the tsv is in increasing order of q-value! 
            while (qLessThan1)
          {
                string[] parts = lines[i].Split('\t');
                //only read in with Q-value < 1%
                if (Convert.ToDouble(parts[30]) < 1)
                {
                    if (Convert.ToBoolean(parts[26]))
                    {
                        Psm new_psm = new Psm(parts[11].ToString(), parts[0].ToString(), Convert.ToInt32(parts[14]), Convert.ToInt32(parts[15]),
                            Convert.ToDouble(parts[10]), Convert.ToDouble(parts[6]), Convert.ToDouble(parts[25]),Convert.ToInt32(parts[1]),
                            parts[13].ToString(), Convert.ToDouble(parts[5]), Convert.ToInt32(parts[7]), Convert.ToDouble(parts[18]), PsmType.BottomUp);
                        psm_list.Add(new_psm);
                    }
                    i++;
                }
                else { qLessThan1 = false; } 
            }
            return psm_list;
        }

        private static string GetAttribute(XElement element, string attribute_name)
        {
            XAttribute attribute = element.Attributes().FirstOrDefault(a => a.Name.LocalName == attribute_name);
            return attribute == null ? "" : attribute.Value;
        }

        private static XElement GetChild(XElement element, string name)
        {
            XElement e = element.Elements().FirstOrDefault(elem => elem.Name.LocalName == name);
            if (e != null) return e;
            else return new XElement("dummy_node");
        }

        private static XElement GetDescendant(XElement element, string name)
        {
            XElement e = element.Descendants().FirstOrDefault(elem => elem.Name.LocalName == name);
            if (e != null) return e;
            else return new XElement("dummy_node");
        }

        private static int ConvertPositionElem(XElement position_elem)
        {
            string feature_position = GetAttribute(position_elem, "position");
            string feature_position_status = GetAttribute(position_elem, "status"); //positionType elements have default 'status' of certain
            if (feature_position != "" && feature_position_status == "")
                return Convert.ToInt32(feature_position) - 1;
            else
                return -1;
        }

        private static Dictionary<int, List<Modification>> SegmentPtms(Dictionary<int, List<Modification>> allPosPTMs, int begin, int end)
        {
            Dictionary<int, List<Modification>> segPosPTMs = new Dictionary<int, List<Modification>>();
            foreach (int position in allPosPTMs.Keys)
            {
                if (position >= begin && position <= end)
                    segPosPTMs.Add(position, allPosPTMs[position]);
            }

            return segPosPTMs;// the int is the amino acid position and the string[] are the different ptms at that position
        }
    }
}


