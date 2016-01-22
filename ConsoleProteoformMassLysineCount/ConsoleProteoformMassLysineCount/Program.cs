using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace ConsoleProteoformMassLysineCount
{
    struct protein
    {
        public string accession, name, fragment, sequence;
        public int begin, end;
        public Dictionary<int, List<string>> positionsAndPtms;
    }

    class Program
    {
        static void Main(string[] args)
        {
            string xmlInFilePath = GetXmlFilePath(); //Get path and filename for valid UniProt XML
            string myOutFilePath = GetOutFileName(xmlInFilePath); //Create path and filename for output. Defaults to input directory.
            string myDecoyOutfile = GetDecoyOutFileName(xmlInFilePath); // Path for decoy databases. Still need to append _number and .txt
            File.WriteAllBytes(myOutFilePath, new byte[0]);//Clear the output file before processing
            GlobalProteoformProperties gpp = new GlobalProteoformProperties(); //class of global paramters used in protoeform mass calculation

            bool mOx = gpp.MethionineOxidation;
            bool cBn = gpp.Carbmidomthylaton;
            bool mC = gpp.Cleaved_N_TerminalMethionine;
            string kI = gpp.LysineIsotopes;
            int maxPTMsPerProteoform = gpp.MaxPTMs;
            int numDecoyDatabases = gpp.NumberOfDecoyDatabases;

            Console.WriteLine("decoy databases: " + numDecoyDatabases);

            Dictionary<char, double> aaIsotopeMassList = new Dictionary<char, double>();
            aaIsotopeMassList = AminoAcidMasses.GetAA_Masses(mOx, cBn, kI);

            read_uniprot_ptmlist rup = new read_uniprot_ptmlist();

            Dictionary<string, modData> uniprotModificationTable = new Dictionary<string, modData>();

            uniprotModificationTable = rup.rd_unip_ptms();

            Console.WriteLine("We're gonna write your result in:  "+myOutFilePath);

            int totalNumEntries = NumberOfUniProtEntrys(xmlInFilePath);

            string giantProtein = getOneGiantProtein(xmlInFilePath, mC);
            //.WriteLine("GP Length: " + giantProtein.Length);
            //Console.Read();

            //writeGiantProtein(giantProtein, myOutFilePath);

            Console.WriteLine("Num Entries = "+totalNumEntries);

            protein[] proteinRawInfo = new protein[totalNumEntries];

            proteinRawInfo = GetProteinRawInfo(xmlInFilePath,totalNumEntries);

            WriteHeader(xmlInFilePath, mOx, cBn, mC, kI, maxPTMsPerProteoform, myOutFilePath);

            processEntries(proteinRawInfo, mC, totalNumEntries,aaIsotopeMassList, myOutFilePath, maxPTMsPerProteoform, uniprotModificationTable);

            processDecoys(numDecoyDatabases, giantProtein, proteinRawInfo, mC, totalNumEntries, aaIsotopeMassList, myDecoyOutfile, maxPTMsPerProteoform, uniprotModificationTable);

        } // end of main


        // methods start here
        static void WriteHeader(string xml_file, bool MOX, bool carbam, bool noMet, string lysine, int maxPTMs, string outFile)
        {
            StreamWriter myOutFile = new StreamWriter(@outFile, false);
            myOutFile.WriteLine("******************************************************************");
            myOutFile.WriteLine("*");
            myOutFile.WriteLine("*\tDatabase of Intact Proteoform Masses");
            myOutFile.WriteLine("*");
            myOutFile.WriteLine("*\tCreated:\t" + DateTime.Now.ToString());
            myOutFile.WriteLine("*");
            myOutFile.WriteLine("*\tXML File:\t" + xml_file);
            myOutFile.WriteLine("*");
            myOutFile.WriteLine("*\tMethionine Oxidation:\t" + MOX);
            myOutFile.WriteLine("*");
            myOutFile.WriteLine("*\tCarbamidomethylation:\t" + carbam);
            myOutFile.WriteLine("*");
            myOutFile.WriteLine("*\tCleaved N-terminal methionine:\t" + noMet);
            myOutFile.WriteLine("*");
            myOutFile.WriteLine("*\tLysine Isotopes:\t" + lysine);
            myOutFile.WriteLine("*");
            myOutFile.WriteLine("*\tMax PTMs per Proteoform:\t" + maxPTMs);
            myOutFile.WriteLine("*");
            myOutFile.WriteLine("*\toutfile:\t" + outFile);
            myOutFile.WriteLine("*");
            myOutFile.WriteLine("*\tNote:\tAmino acid numbering begins with '0'.  UniProt amino acid numbering begins with '1'.");
            myOutFile.WriteLine("*");
            myOutFile.WriteLine("******************************************************************");
            myOutFile.WriteLine("Accesion\tProtein\tFragment_Type\tBegin\tEnd\tBase_Mass\tLysine_Count\tModification_Type\tModification_Mass\tProteoform_Mass");
            myOutFile.Close();

        }

        static void WriteOutFile(string acc, string name, string frag, int begin, int end, double mass, int kCount, string outFile, List<string> ptm_list, double ptm_mass, double proteoform_mass)
        {
            StreamWriter myOutFile = new StreamWriter(@outFile, true);

            myOutFile.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}", acc, name, frag, begin, end, mass, kCount, String.Join("; ", ptm_list), ptm_mass, proteoform_mass);
            myOutFile.Close();

        }

        static int LysineCount(string text, string search)
        {
            int num = 0;
            int pos = 0;

            if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(search))
            {
                while ((pos = text.IndexOf(search, pos)) > -1)
                {
                    num++;
                    pos += search.Length;
                }
            }
            return num;
        }

        static void processEntries(protein[] pRD, bool mC, int num, Dictionary<char,double> aIML, string outFile, int maxPTMsPerProteoform, Dictionary<string, modData> uniprotModificationTable)
        {
            for (int i = 0; i < num; i++)
            {
                double mass;
                if (mC && pRD[i].begin == 0 && pRD[i].sequence.Substring(0,1) == "M") // methionine cleavage of N-terminus specified
                {
                    mass = CalculateProteoformMass(ref aIML, pRD[i].sequence.Substring(1,(pRD[i].sequence.Length-1)));

                    int kCount = LysineCount(pRD[i].sequence.Substring(1, (pRD[i].sequence.Length - 1)), "K");

                    ptm_combos pc = new ptm_combos();

                    List<one_unique_ptm_group> aupg = new List<one_unique_ptm_group>();

                    one_unique_ptm_group unmod = new one_unique_ptm_group();
                    unmod.mass = 0;
                    List<string> unmod_string = new List<string>();
                    unmod_string.Add("unmodified");
                    unmod.unique_ptm_combinations = unmod_string;
                    aupg.Add(unmod);

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("Accession: "+pRD[i].accession+"\tCount: " + pRD[i].positionsAndPtms.Count());

                    if (maxPTMsPerProteoform > 0 && pRD[i].positionsAndPtms.Count() > 0)
                    {
                        aupg.AddRange(pc.combos(maxPTMsPerProteoform, uniprotModificationTable, pRD[i].positionsAndPtms));
                    }

                    foreach (one_unique_ptm_group group in aupg)
                    {
                        List<string> ptm_list = group.unique_ptm_combinations;
                        Console.WriteLine("PTM Combinations: " + String.Join("; ", ptm_list));
                        Double ptm_mass = group.mass;
                        Double proteoform_mass = mass + group.mass;
                        WriteOutFile(pRD[i].accession, pRD[i].name, pRD[i].fragment, pRD[i].begin + 1, pRD[i].end, mass, kCount, outFile, ptm_list, ptm_mass, proteoform_mass);
                    }

                }
                else
                {
                    mass = CalculateProteoformMass(ref aIML, pRD[i].sequence);

                    //Console.WriteLine(pRD[i].accession);
                    //Console.WriteLine("     mass : " + mass);
                    int kCount = LysineCount(pRD[i].sequence, "K");
                    //Console.WriteLine("     lysine count : " + kCount);

                    //WriteOutFile(pRD[i].accession, pRD[i].name, pRD[i].fragment, pRD[i].begin, pRD[i].end, mass, kCount, outFile);

                    //*****************this was added

                    ptm_combos pc = new ptm_combos();

                    List<one_unique_ptm_group> aupg = new List<one_unique_ptm_group>();

                    one_unique_ptm_group unmod = new one_unique_ptm_group();
                    unmod.mass = 0;
                    List<string> unmod_string = new List<string>();
                    unmod_string.Add("unmodified");
                    unmod.unique_ptm_combinations = unmod_string;
                    aupg.Add(unmod);

                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("Accession: " + pRD[i].accession + "\tCount: " + pRD[i].positionsAndPtms.Count());

                    if (maxPTMsPerProteoform > 0 && pRD[i].positionsAndPtms.Count() > 0)
                    {
                        aupg.AddRange(pc.combos(maxPTMsPerProteoform, uniprotModificationTable, pRD[i].positionsAndPtms));
                    }

                    foreach (one_unique_ptm_group group in aupg)
                    {
                        List<string> ptm_list = group.unique_ptm_combinations;
                        Console.WriteLine("PTM Combinations: " + String.Join("; ", ptm_list));
                        Double ptm_mass = group.mass;
                        Double proteoform_mass = mass + group.mass;
                        WriteOutFile(pRD[i].accession, pRD[i].name, pRD[i].fragment, pRD[i].begin, pRD[i].end, mass, kCount, outFile, ptm_list, ptm_mass, proteoform_mass);
                    }

                    //*****************this was added



                }

            }
        }

        static void processDecoys(int numDb, string giantProtein, protein[] pRD, bool mC, int num, Dictionary<char, double> aIML, string outFile, int maxPTMsPerProteoform, Dictionary<string, modData> uniprotModificationTable)
        {
            Console.WriteLine("Processing decoys: " + numDb);

            for (int decoyNumber = 0; decoyNumber < numDb; decoyNumber++)
            {
                string myOutFile = outFile + "_" + decoyNumber + ".txt";

                StreamWriter myNewOutFile = new StreamWriter(@myOutFile, false);
                myNewOutFile.WriteLine("******************************************************************");
                myNewOutFile.WriteLine("*");
                myNewOutFile.WriteLine("*\tDecoy Database of Intact Proteoform Masses Number: " + decoyNumber);
                myNewOutFile.WriteLine("*");
                myNewOutFile.WriteLine("*\tCreated:\t" + DateTime.Now.ToString());
                myNewOutFile.WriteLine("*");
                myNewOutFile.WriteLine("*\toutfile:\t" + myOutFile);
                myNewOutFile.WriteLine("*");
                myNewOutFile.WriteLine("*\tNote:\tAmino acid numbering begins with '0'.  UniProt amino acid numbering begins with '1'.");
                myNewOutFile.WriteLine("*");
                myNewOutFile.WriteLine("******************************************************************");
                myNewOutFile.WriteLine("Accesion\tProtein\tFragment_Type\tBegin\tEnd\tBase_Mass\tLysine_Count\tModification_Type\tModification_Mass\tProteoform_Mass");
                myNewOutFile.Close();


                new Random().Shuffle(pRD); //Randomize Order of Protein Array

                for (int i = 0; i < num; i++)
                {
                    double mass;
                    if (mC && pRD[i].begin == 0 && pRD[i].sequence.Substring(0, 1) == "M") // methionine cleavage of N-terminus specified
                    {
                        int hunkLength = pRD[i].sequence.Length - 1;
                        string hunk = giantProtein.Substring(0, hunkLength);
                        giantProtein.Remove(0, hunkLength);

                        mass = CalculateProteoformMass(ref aIML, hunk);

                        int kCount = LysineCount(hunk, "K");

                        ptm_combos pc = new ptm_combos();

                        List<one_unique_ptm_group> aupg = new List<one_unique_ptm_group>();

                        one_unique_ptm_group unmod = new one_unique_ptm_group();
                        unmod.mass = 0;
                        List<string> unmod_string = new List<string>();
                        unmod_string.Add("unmodified");
                        unmod.unique_ptm_combinations = unmod_string;
                        aupg.Add(unmod);

                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("Accession: " + pRD[i].accession + "_DECOY_" + decoyNumber + "\tCount: " + pRD[i].positionsAndPtms.Count());

                        if (maxPTMsPerProteoform > 0 && pRD[i].positionsAndPtms.Count() > 0)
                        {
                            aupg.AddRange(pc.combos(maxPTMsPerProteoform, uniprotModificationTable, pRD[i].positionsAndPtms));
                        }

                        foreach (one_unique_ptm_group group in aupg)
                        {
                            List<string> ptm_list = group.unique_ptm_combinations;
                            Console.WriteLine("PTM Combinations: " + String.Join("; ", ptm_list));
                            Double ptm_mass = group.mass;
                            Double proteoform_mass = mass + group.mass;
                            WriteOutFile(pRD[i].accession + "_DECOY_" + decoyNumber, pRD[i].name, pRD[i].fragment, pRD[i].begin + 1, pRD[i].end, mass, kCount, myOutFile, ptm_list, ptm_mass, proteoform_mass);
                        }

                    }
                    else
                    {
                        int hunkLength = pRD[i].sequence.Length;
                        string hunk = giantProtein.Substring(0, hunkLength);
                        giantProtein.Remove(0, hunkLength);

                        mass = CalculateProteoformMass(ref aIML, hunk);


                        //Console.WriteLine(pRD[i].accession);
                        //Console.WriteLine("     mass : " + mass);
                        int kCount = LysineCount(hunk, "K");
                        //Console.WriteLine("     lysine count : " + kCount);

                        //WriteOutFile(pRD[i].accession, pRD[i].name, pRD[i].fragment, pRD[i].begin, pRD[i].end, mass, kCount, outFile);

                        //*****************this was added

                        ptm_combos pc = new ptm_combos();

                        List<one_unique_ptm_group> aupg = new List<one_unique_ptm_group>();

                        one_unique_ptm_group unmod = new one_unique_ptm_group();
                        unmod.mass = 0;
                        List<string> unmod_string = new List<string>();
                        unmod_string.Add("unmodified");
                        unmod.unique_ptm_combinations = unmod_string;
                        aupg.Add(unmod);

                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("Accession: " + pRD[i].accession + "_DECOY_" + decoyNumber + "\tCount: " + pRD[i].positionsAndPtms.Count());

                        if (maxPTMsPerProteoform > 0 && pRD[i].positionsAndPtms.Count() > 0)
                        {
                            aupg.AddRange(pc.combos(maxPTMsPerProteoform, uniprotModificationTable, pRD[i].positionsAndPtms));
                        }

                        foreach (one_unique_ptm_group group in aupg)
                        {
                            List<string> ptm_list = group.unique_ptm_combinations;
                            Console.WriteLine("PTM Combinations: " + String.Join("; ", ptm_list));
                            Double ptm_mass = group.mass;
                            Double proteoform_mass = mass + group.mass;
                            WriteOutFile(pRD[i].accession + "_DECOY_" + decoyNumber, pRD[i].name, pRD[i].fragment, pRD[i].begin, pRD[i].end, mass, kCount, myOutFile, ptm_list, ptm_mass, proteoform_mass);
                        }

                    }

                }

            }
        }

        static string GetProteinName(entry _ent)
        {
            string name = "";

            int proteinNameType = 1;
            try
            {
                if ((_ent.protein.recommendedName.fullName.Value) != null)
                {
                    proteinNameType = 1;
                }
            }
            catch
            {
                try
                {
                    if ((_ent.protein.submittedName[0].fullName.Value) != null)
                    {
                        proteinNameType = 2;
                    }
                }
                catch
                {
                    proteinNameType = 3;
                }
            }


            switch (proteinNameType)
            {
                case 1: //Recommended Name
                    {
                        name = _ent.protein.recommendedName.fullName.Value;
                        break;
                    }
                case 2: //Submitted Name
                    {
                        name = _ent.protein.submittedName[0].fullName.Value.ToString();
                        break;
                    }
                case 3: //Alternative Name
                    {
                        name = _ent.protein.alternativeName[0].fullName.Value.ToString();
                        break;
                    }
                default:
                    name = "";
                    break;
            }

            return name;
        }

        static Dictionary<int, List<string>> SegmentPTMs(Dictionary<int,List<string>> allPosPTMs, int begin, int end)
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

        static Dictionary<int, List<string>> GetPositionsPTMs(entry _ent)
        {
            Dictionary<int, List<string>> local_pAP = new Dictionary<int, List<string>>();

            foreach (var proteinFeature in _ent.feature)//process protein ptms
            {
                string type = proteinFeature.type.ToString();

                int position = 0;
                int begin = 0;
                int end = 0; 
                bool realFeature = false;

                int noPosition = 0;
                try
                {
                    if (proteinFeature.location.ItemsElementName[0].ToString() != "position") { noPosition = 1; }
                }
                catch
                {
                    noPosition = 0;
                }
                if (noPosition == 1) // has begin and end
                {
                    if (proteinFeature.location.Items[0].status.ToString() == "certain"
                        && proteinFeature.location.Items[0].positionSpecified)
                    {
                        realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out begin);
                        if (realFeature)
                        {
                            if (proteinFeature.location.Items[1].status.ToString() == "certain"
                                && proteinFeature.location.Items[1].positionSpecified)
                            {
                                realFeature = int.TryParse(proteinFeature.location.Items[1].position.ToString(), out end);
                            }  
                        }
                    }

                    if (realFeature)
                    {
                        begin = begin - 1;
                        end = end - 1;
                        if ((begin<0)||(end<0))
                        {
                            realFeature = false;
                        }
                    }

                }
                else // protein only has single position location
                {
                    if (proteinFeature.location.Items[0].status.ToString() == "certain"
                        && proteinFeature.location.Items[0].positionSpecified)
                    {
                        realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out position);
                        if (realFeature)
                        {
                            position = position - 1;
                            if (position < 0)
                            {
                                realFeature = false;
                            }
                        }
                    }

                }

                if (realFeature)
                {
                    switch (type)
                    {
                        case "modifiedresidue":
                            string description = proteinFeature.description.ToString();
                            //Console.WriteLine(_ent.accession[0] + "\t" + description + "\t" + position);
                            if (local_pAP.ContainsKey(position))
                            {
                                List<string> morePtms = new List<string>();
                                morePtms = local_pAP[position].ToList();
                                morePtms.Add(description.Split(';')[0]);//take description up to ';' if there is one
                                local_pAP[position] = morePtms;
                            }
                            else
                            {
                                List<string> ptms = new List<string>();
                                ptms.Add(description.Split(';')[0]);
                                local_pAP.Add(position, ptms);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }


            return local_pAP;
        }

        static protein[] GetProteinRawInfo(string inFile, int num)
        {
            protein[] pRI = new protein[num];

            using (FileStream xmlStream = new FileStream(inFile, FileMode.Open))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                using (XmlReader xmlReader = XmlReader.Create(xmlStream, settings))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(uniprot));
                    uniprot deserializeduniprot = serializer.Deserialize(xmlReader) as uniprot;
                    int count = 0;
                    foreach (var _entry in deserializeduniprot.entry)
                    {
                        Dictionary<int, List<string>> pAP = new Dictionary<int, List<string>>();

                        pAP = GetPositionsPTMs(_entry); // dictionary of positions and PTMs in complete entry

                        pRI[count].accession = _entry.accession[0];
                        //Console.WriteLine(_entry.accession[0]);
                        pRI[count].name = GetProteinName(_entry);
                        pRI[count].fragment = "full";

                        //this next bit eliminates return characters etc from sequence string
                        string fullSequence = _entry.sequence.Value;
                        char[] arr = fullSequence.ToCharArray();
                        arr = Array.FindAll<char>(arr,(c => (char.IsLetter(c))));
                        fullSequence = new string(arr);

                        pRI[count].sequence = fullSequence;
                        pRI[count].begin = 0;
                        pRI[count].end = fullSequence.Length-1;
                        int fullSequenceLength = _entry.sequence.length; 
                        pRI[count].positionsAndPtms = SegmentPTMs(pAP, pRI[count].begin, pRI[count].end);

                        count++;

                        foreach (var proteinFeature in _entry.feature)//process protein fragments
                        {
                            string type = proteinFeature.type.ToString();

                            int position = 0;
                            int begin = 0;
                            int end = 0;
                            bool realFeature = false;

                            int noPosition = 0;
                            try
                            {
                                if (proteinFeature.location.ItemsElementName[0].ToString() != "position") { noPosition = 1; }
                            }
                            catch
                            {
                                noPosition = 0;
                            }
                            if (noPosition == 1) // has begin and end
                            {
                                if (proteinFeature.location.Items[0].status.ToString() == "certain"
                                    && proteinFeature.location.Items[0].positionSpecified)
                                {
                                    realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out begin);
                                }
                                if (realFeature)
                                {
                                    if (proteinFeature.location.Items[1].status.ToString() == "certain"
                                        && proteinFeature.location.Items[1].positionSpecified)
                                    {
                                        realFeature = int.TryParse(proteinFeature.location.Items[1].position.ToString(), out end);
                                    }
                                }

                                if (realFeature)
                                {
                                    begin = begin - 1;
                                    end = end - 1;
                                    //Console.WriteLine("parse b: " + begin + "end: " + end);
                                    if ((begin<0)||(end<0))
                                    {
                                        realFeature = false;
                                    }
                                }
                            }
                            else // protein only as single position location
                            {
                                if (proteinFeature.location.Items[0].status.ToString() == "certain"
                                    && proteinFeature.location.Items[0].positionSpecified)
                                    {
                                        realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out position);
                                        if (realFeature)
                                        {
                                            position = position - 1;
                                            if (position < 0)
                                            {
                                                realFeature = false;
                                             }
                                        }
                                    }
                            }
                            if (realFeature)
                            {
                                switch (type)
                                {
                                    case "signalpeptide"://spaces are sometimes deleted in xml read
                                        pRI[count].accession = _entry.accession[0];
                                        pRI[count].name = GetProteinName(_entry);
                                        pRI[count].fragment = "signal peptide";
                                        pRI[count].sequence = fullSequence.Substring(begin, (end - begin + 1));
                                        pRI[count].begin = begin;
                                        pRI[count].end = end;
                                        pRI[count].positionsAndPtms = SegmentPTMs(pAP, begin, end);
                                        count++;
                                        break;
                                    case "chain":
                                        if ((end-begin+1)!=fullSequenceLength)
                                        {
                                            pRI[count].accession = _entry.accession[0];
                                            pRI[count].name = GetProteinName(_entry);
                                            pRI[count].fragment = "chain";
                                            pRI[count].sequence = fullSequence.Substring(begin, (end - begin + 1));
                                            pRI[count].begin = begin;
                                            pRI[count].end = end;
                                            pRI[count].positionsAndPtms = SegmentPTMs(pAP, begin, end);
                                            count++;
                                        }
                                        break;
                                    case "propeptide":
                                        pRI[count].accession = _entry.accession[0];
                                        pRI[count].name = GetProteinName(_entry);
                                        pRI[count].fragment = "propeptide";
                                        pRI[count].sequence = fullSequence.Substring(begin, (end - begin) + 1);
                                        pRI[count].begin = begin;
                                        pRI[count].end = end;
                                        pRI[count].positionsAndPtms = SegmentPTMs(pAP, begin, end);
                                        count++;
                                        break;
                                    case "peptide":
                                        pRI[count].accession = _entry.accession[0];
                                        pRI[count].name = GetProteinName(_entry);
                                        pRI[count].fragment = "peptide";
                                        pRI[count].sequence = fullSequence.Substring(begin, (end - begin) + 1);
                                        pRI[count].begin = begin;
                                        pRI[count].end = end;
                                        pRI[count].positionsAndPtms = SegmentPTMs(pAP, begin, end);
                                        count++;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            return pRI;
        }

        static void writeGiantProtein(string giantProtein, string myOutFilePath)
        {
            string newOutFile = myOutFilePath.Replace("_Mass_Kcount.txt", "_complete_protein.txt");

            File.WriteAllBytes(newOutFile, new byte[0]);//Clear the output file before processing

            StreamWriter myOutFile = new StreamWriter(@newOutFile, false);

            while (giantProtein.Length > 60)
            {
                myOutFile.WriteLine(giantProtein.Substring(0, 60));
                giantProtein = giantProtein.Substring(60);
            }
            myOutFile.WriteLine(giantProtein);

            myOutFile.Close();
        }

        static string getOneGiantProtein(string inFile, bool mC)//returns sum of full length, signal peptide, chain, propetide and peptide
        {
            StringBuilder giantProtein = new StringBuilder(5000000); // this set-aside is autoincremented to larger values when necessary.

            using (FileStream xmlStream = new FileStream(inFile, FileMode.Open))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                using (XmlReader xmlReader = XmlReader.Create(xmlStream, settings))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(uniprot));
                    uniprot deserializeduniprot = serializer.Deserialize(xmlReader) as uniprot;
                    foreach (var _entry in deserializeduniprot.entry)
                    {
                        string thisFullSequence = _entry.sequence.Value.Replace("\r", "").Replace("\n", "");
                        //Console.WriteLine(thisFullSequence.ToString());

                        if (mC && (thisFullSequence.Substring(0,1)=="M"))
                        {
                            //Console.WriteLine("MC: " + mC);
                            giantProtein.Append("-" + thisFullSequence.Substring(1)); // should be everything after the first character
                        }
                        else
                        {
                            Console.WriteLine("full");
                            giantProtein.Append("-" + thisFullSequence);
                        }
                        
                        

                        int fullSequenceLength = thisFullSequence.Length;

                        foreach (var proteinFeature in _entry.feature)//process protein fragments
                        {
                            string type = proteinFeature.type.ToString();

                            int position = 0;
                            int begin = 0;
                            int end = 0;
                            bool realFeature = false;

                            int noPosition = 0;
                            try
                            {
                                if (proteinFeature.location.ItemsElementName[0].ToString() != "position") { noPosition = 1; }
                            }
                            catch
                            {
                                noPosition = 0;
                            }
                            if (noPosition == 1) // has begin and end
                            {
                                if (proteinFeature.location.Items[0].status.ToString() == "certain"
                                    && proteinFeature.location.Items[0].positionSpecified)
                                {
                                    realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out begin);
                                }
                                if (realFeature)
                                {
                                    if (proteinFeature.location.Items[1].status.ToString() == "certain"
                                        && proteinFeature.location.Items[1].positionSpecified)
                                    {
                                        realFeature = int.TryParse(proteinFeature.location.Items[1].position.ToString(), out end);
                                    }
                                }

                                if (realFeature)
                                {
                                    begin = begin - 1;
                                    end = end - 1;
                                    //Console.WriteLine("parse b: " + begin + "end: " + end);
                                    if ((begin < 0) || (end < 0))
                                    {
                                        realFeature = false;
                                    }
                                }
                            }
                            else // protein only as single position location
                            {
                                if (proteinFeature.location.Items[0].status.ToString() == "certain"
                                    && proteinFeature.location.Items[0].positionSpecified)
                                {
                                    realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out position);
                                    if (realFeature)
                                    {
                                        position = position - 1;
                                        if (position < 0)
                                        {
                                            realFeature = false;
                                        }
                                    }
                                }
                            }
                            if (realFeature)
                            {
                                if (mC && (begin == 0) && end >= 1 && (thisFullSequence.Substring(0,1) == "M"))
                                {
                                    //Console.WriteLine("inside begin + 1");
                                    begin = begin + 1;
                                }

                                switch (type)
                                {
                                    case "signalpeptide"://spaces are sometimes deleted in xml read
                                        giantProtein.Append("." + thisFullSequence.Substring(begin, (end-begin+1)));
                                        //Console.WriteLine(thisFullSequence.Substring(begin, (end - begin + 1)));
                                        break;
                                    case "chain":
                                        if (mC == true) 
                                            {
                                            if ((end - begin + 1) != (fullSequenceLength-1))
                                                {
                                                    giantProtein.Append("." + thisFullSequence.Substring(begin, (end - begin + 1)));
                                                }
                                            }
                                            else
                                            {
                                            if ((end - begin + 1) != fullSequenceLength)
                                                {
                                                    giantProtein.Append("." + thisFullSequence.Substring(begin, (end - begin + 1)));
                                                }
                                            }
                                        break;
                                    case "propeptide":
                                        giantProtein.Append("." + thisFullSequence.Substring(begin, (end - begin + 1)));
                                        break;
                                    case "peptide":
                                        giantProtein.Append("." + thisFullSequence.Substring(begin, (end - begin + 1)));
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            return giantProtein.ToString();
        }

        static int NumberOfUniProtEntrys(string inFile)//returns sum of full length, signal peptide, chain, propetide and peptide
        {
            var nodeCount = 0;

            using (FileStream xmlStream = new FileStream(inFile, FileMode.Open))
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                using (XmlReader xmlReader = XmlReader.Create(xmlStream, settings))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(uniprot));
                    uniprot deserializeduniprot = serializer.Deserialize(xmlReader) as uniprot;
                    foreach (var _entry in deserializeduniprot.entry)
                    {
                        nodeCount++;//one for main entry
                        int fullSequenceLength = _entry.sequence.length;

                        foreach (var proteinFeature in _entry.feature)//process protein fragments
                        {
                            string type = proteinFeature.type.ToString();

                            int position = 0;
                            int begin = 0;
                            int end = 0;
                            bool realFeature = false;

                            int noPosition = 0;
                            try
                            {
                                if (proteinFeature.location.ItemsElementName[0].ToString() != "position") { noPosition = 1; }
                            }
                            catch
                            {
                                noPosition = 0;
                            }
                            if (noPosition == 1) // has begin and end
                            {
                                if (proteinFeature.location.Items[0].status.ToString() == "certain"
                                    && proteinFeature.location.Items[0].positionSpecified)
                                {
                                    realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out begin);
                                }
                                if (realFeature)
                                {
                                    if (proteinFeature.location.Items[1].status.ToString() == "certain"
                                        && proteinFeature.location.Items[1].positionSpecified)
                                    {
                                        realFeature = int.TryParse(proteinFeature.location.Items[1].position.ToString(), out end);
                                    }
                                }

                                if (realFeature)
                                {
                                    begin = begin - 1;
                                    end = end - 1;
                                    //Console.WriteLine("parse b: " + begin + "end: " + end);
                                    if ((begin < 0) || (end < 0))
                                    {
                                        realFeature = false;
                                    }
                                }
                            }
                            else // protein only as single position location
                            {
                                if (proteinFeature.location.Items[0].status.ToString() == "certain"
                                    && proteinFeature.location.Items[0].positionSpecified)
                                {
                                    realFeature = int.TryParse(proteinFeature.location.Items[0].position.ToString(), out position);
                                    if (realFeature)
                                    {
                                        position = position - 1;
                                        if (position < 0)
                                        {
                                            realFeature = false;
                                        }
                                    }
                                }
                            }
                            if (realFeature)
                            {
                                switch (type)
                                {
                                    case "signalpeptide"://spaces are sometimes deleted in xml read
                                        nodeCount++;
                                        break;
                                    case "chain":
                                        if ((end - begin + 1) != fullSequenceLength)
                                        {
                                            //Console.WriteLine("b: {0} e: {1} f: {2}", begin, end, fullSequenceLength);
                                            nodeCount++;
                                        }
                                        break;
                                    case "propeptide":
                                        nodeCount++;
                                        break;
                                    case "peptide":
                                        nodeCount++;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                }
            }

            return nodeCount;
        }

        static double CalculateProteoformMass(ref Dictionary<char, double> aaIsotopeMassList, string pForm)
        {
            double proteoformMass = 18.010565; // start with water

            char[] aminoAcids = pForm.ToCharArray(); 

            for (int i = 0; i < pForm.Length; i++)
            {
                double aMass = 0;
                try
                {
                    aMass = aaIsotopeMassList[aminoAcids[i]];
                }
                catch
                {
                    aMass = 0;
                }
                proteoformMass = proteoformMass + aMass;
            }

            return proteoformMass;
        }

        static string GetOutFileName(string inFile)
        {
            string myOutPath = Path.GetDirectoryName(inFile);
            string baseFileName = Path.GetFileNameWithoutExtension(inFile) + "_Mass_Kcount.txt";
            string myOutFilePath = myOutPath + "\\" + baseFileName;
            return myOutFilePath;
        }

        static string GetDecoyOutFileName (string inFile)
        {
            string myOutPath = Path.GetDirectoryName(inFile);
            string baseFileName = Path.GetFileNameWithoutExtension(inFile) + "_Decoy";
            string myOutFilePath = myOutPath + "\\" + baseFileName;
            return myOutFilePath;
        }

        static string GetXmlFilePath()
        {
            bool validXml = false;
            string extension = "";
            string fileName = "";
            do
            {
                Console.WriteLine("Enter Path.UniProt.xml database for mass and lysine count calculation:");
                fileName = Console.ReadLine();

                try
                {
                    extension = Path.GetExtension(fileName);
                }
                catch
                {
                    Console.WriteLine("You typed something weird. Try again.");
                }
                if (extension == ".xml")
                {
                    validXml = ValidateUniProtXML(fileName);
                }

            } while (validXml == false);

            return fileName;
        }

        static bool ValidateUniProtXML(string testXmlFile)
        {
            bool valid = false;
            string line1, line2;
            try
            {
                using (StreamReader reader = new StreamReader(testXmlFile))
                {
                    line1 = reader.ReadLine();
                    line2 = reader.ReadLine();
                    reader.DiscardBufferedData();
                    reader.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);

                    if (line2.Contains("uniprot"))
                    {
                        valid = true;
                    }
                    else
                    {
                        Console.WriteLine("This is not a valid uniprot .xml file. Try again.");
                    }
                }
            }
            catch
            {
                Console.WriteLine("Try again.");
            }
            return valid;
        }

    }
}
