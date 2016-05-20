using System;
using System.Collections.Generic;
using System.ComponentModel; // needed for bindinglist
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS_0._00
{
    public class Lollipop
    {
        public static DataTable experimentTheoreticalPairs = new DataTable();
        public static DataTable etPeakList = new DataTable();
        public static DataSet experimentDecoyPairs = new DataSet();
        public static DataTable edList = new DataTable();
        public static DataTable experimentExperimentPairs = new DataTable();
        public static DataTable eePeakList = new DataTable();
        public static DataTable EE_Parent = new DataTable();
        public static DataSet ProteoformFamiliesET = new DataSet();
        public static DataSet ProteoformFamiliesEE = new DataSet();
        public static DataTable ProteoformFamilyMetrics = new DataTable();        

        //RAW EXPERIMENTAL COMPONENTS
        public static ExcelReader excelReader = new ExcelReader();
        public static BindingList<string> deconResultsFileNames = new BindingList<string>();
        public static List<DataTable> deconResultsFiles = new List<DataTable>();
        public static BindingList<Component> rawExperimentalComponents = new BindingList<Component>();
        public static void GetDeconResults()
        {
            List<DataTable> decon_results = new List<DataTable>();

            Parallel.ForEach<string>(Lollipop.deconResultsFileNames, filename =>
            {
                DataTable dt = excelReader.ReadExcelFile(filename);
                DataTable dc = dt.Clone();
                dc.Columns[0].DataType = typeof(int);
                foreach (DataRow row in dt.Rows)
                {
                    int number;
                    bool result = int.TryParse(row[dt.Columns[0].ColumnName].ToString(), out number);
                    if (result)
                    {
                        dc.ImportRow(row);
                    }
                    else
                    {
                        row[dt.Columns[0].ColumnName] = "-1";
                        dc.ImportRow(row);
                    }

                }
                dc.Columns.Add("Filename", typeof(string));

                foreach (DataRow row in dc.Rows)
                {
                    row["Filename"] = Path.GetFileName(filename);
                }

                decon_results.Add(dc);
            });
            Lollipop.deconResultsFiles = decon_results;
        }

        public static void GetRawComponents()
        {
            BindingList<Component> raw_components = new BindingList<Component>();

            Parallel.ForEach<DataTable>(Lollipop.deconResultsFiles, table =>
            {
                DataRow[] component_rows = table.Select("[" + table.Columns[0].ColumnName + "] > 0"); //Checking that it's a component row, not a charge state one

                Parallel.ForEach<DataRow>(component_rows, component_row =>
                {
                    Component raw_component = new Component(component_row);

                    string charge_states_for_this_id = "[" + table.Columns[5].ColumnName
                        + "] is null AND [" + table.Columns[0].ColumnName + "] = " + raw_component.id
                        + " AND [" + table.Columns[1].ColumnName + "] <> 'Charge State'";
                    DataRow[] charge_rows = table.Select(charge_states_for_this_id);
                    Parallel.ForEach<DataRow>(charge_rows, charge_row => { raw_component.add_charge_state(charge_row); });

                    raw_component.calculate_sum_intensity();
                    raw_component.calculate_weighted_monoisotopic_mass();
                    raw_components.Add(raw_component);
                });
            });
            Lollipop.rawExperimentalComponents = raw_components;
        }

        //NEUCODE PAIRS
        public static List<NeuCodePair> rawNeuCodePairs = new List<NeuCodePair>();
        public static decimal max_intensity_ratio = 6;
        public static decimal min_intensity_ratio = 1.4m;
        public static decimal max_lysine_ct = 26.2m;
        public static decimal min_lysine_ct = 1.5m;
        public static void FillRawNeuCodePairsDataTable()
        {
            Dictionary<string, HashSet<string>>  filename_scanRange = Lollipop.get_scanranges_by_filename();
            Parallel.ForEach<KeyValuePair<string, HashSet<string>>>(filename_scanRange, entry =>
            {
                string filename = entry.Key;
                Parallel.ForEach<string>(entry.Value, scanRange =>
                {
                    List<Component> components_in_file_scanrange = new List<Component>();

                    //select all components in file and this particular scanrange
                    Parallel.ForEach<Component>(Lollipop.rawExperimentalComponents, c =>
                    {
                        if (c.file_origin == filename && c.scan_range == scanRange)
                            components_in_file_scanrange.Add(c);
                    });

                    components_in_file_scanrange.OrderBy(c => c.weighted_monoisotopic_mass);

                    //Add putative neucode pairs. Must be in same spectrum, mass must be within 6 Da of each other
                    int lower_mass_index = 0;
                    Parallel.For(lower_mass_index, components_in_file_scanrange.Count - 2, lower_index =>
                    {
                        Component lower_component = components_in_file_scanrange[lower_index];
                        int higher_mass_index = lower_mass_index + 1;
                        double apexRT = lower_component.rt_apex;

                        Parallel.For(higher_mass_index, components_in_file_scanrange.Count - 1, higher_index =>
                        {
                            Component higher_component = components_in_file_scanrange[higher_index];
                            double mass_difference = higher_component.weighted_monoisotopic_mass - lower_component.weighted_monoisotopic_mass; //changed from decimal; it doesn't seem like that should make a difference
                            if (mass_difference < 6)
                            {
                                NeuCodePair p = new NeuCodePair(lower_component, higher_component);
                                if (p.accepted) { Lollipop.rawNeuCodePairs.Add(p); }
                            }
                        });
                    });
                });
            });
        }

        private static Dictionary<string, HashSet<string>> get_scanranges_by_filename()
        {
            Dictionary<string, HashSet<string>> filename_scanRanges = new Dictionary<string, HashSet<string>>();
            Parallel.ForEach<string>(Lollipop.deconResultsFileNames, filename =>
            {
                if (!filename_scanRanges.ContainsKey(filename))
                    filename_scanRanges.Add(filename, new HashSet<string>());
            });

            Parallel.ForEach<Component>(Lollipop.rawExperimentalComponents, c =>
            {
                filename_scanRanges[c.file_origin].Add(c.scan_range);
            });
            return filename_scanRanges;
        }

        //AGGREGATED PROTEOFORMS
        public static decimal mass_tolerance = 3;
        public static decimal retention_time_tolerance = 3;
        public static decimal missed_monos = 3;
        public static decimal missed_lysines = 1;
        public static List<ExperimentalProteoform> aggregatedProteoforms = new List<ExperimentalProteoform>();
        public static void AggregateNeuCodeLightProteoforms()
        {
            if (Lollipop.aggregatedProteoforms.Count > 0)
                Lollipop.aggregatedProteoforms.Clear();

            List<NeuCodePair> remaining_acceptableProteoforms = Lollipop.rawNeuCodePairs.Where(p => p.accepted).ToList().
                OrderByDescending(p => p.light_intensity).ToList(); //ordered list, so that the proteoform with max intensity is always chosen first

            while (remaining_acceptableProteoforms.Count > 0)
            {
                NeuCodePair root = remaining_acceptableProteoforms[0];
                remaining_acceptableProteoforms.Remove(root);
                List<NeuCodePair> pf_to_aggregate = new List<NeuCodePair>() { root };

                Parallel.ForEach<NeuCodePair>(remaining_acceptableProteoforms, p =>
                {
                    if (tolerable_rt(root, p) && tolerable_lysCt(root, p) && tolerable_mass(root, p))
                        pf_to_aggregate.Add(p);
                });
                foreach (NeuCodePair p in pf_to_aggregate)
                {
                    remaining_acceptableProteoforms.Remove(p); //Can removal be parallelized? seems unstable
                }

                if (pf_to_aggregate.Count > 0)
                    Lollipop.aggregatedProteoforms.Add(new ExperimentalProteoform(pf_to_aggregate));
            }
        }

        private static bool tolerable_rt(NeuCodePair root, NeuCodePair candidate)
        {
            return candidate.light_apexRt >= root.light_apexRt - Convert.ToDouble(Lollipop.retention_time_tolerance) &&
                candidate.light_apexRt <= root.light_apexRt + Convert.ToDouble(Lollipop.retention_time_tolerance);
        }

        private static bool tolerable_lysCt(NeuCodePair root, NeuCodePair candidate)
        {
            int max_missed_lysines = Convert.ToInt32(Lollipop.missed_lysines);
            List<int> acceptable_lysineCts = Enumerable.Range(root.lysine_count - max_missed_lysines, root.lysine_count + max_missed_lysines).ToList();
            return acceptable_lysineCts.Contains(candidate.lysine_count);
        }

        private static bool tolerable_mass(NeuCodePair root, NeuCodePair candidate)
        {
            int max_missed_monoisotopics = Convert.ToInt32(Lollipop.missed_monos);
            List<int> missed_monoisotopics = Enumerable.Range(-max_missed_monoisotopics, max_missed_monoisotopics).ToList();
            foreach (int m in missed_monoisotopics)
            {
                double shift = m * 1.0015;
                double mass_tolerance = (root.light_corrected_mass + shift) / 1000000 * Convert.ToInt32(Lollipop.mass_tolerance);
                double low = root.light_corrected_mass + shift - mass_tolerance;
                double high = root.light_corrected_mass + shift + mass_tolerance;
                bool tolerable_mass = candidate.light_corrected_mass >= low && candidate.light_corrected_mass <= high;
                if (tolerable_mass) return true; //Return a true result immediately; acts as an OR between these conditions
            }
            return false;
        }

        //THEORETICAL DATABASE
        public static bool methionine_oxidation = false;
        public static bool carbamidomethylation = true;
        public static bool methionine_cleavage = true;
        public static bool natural_lysine_isotope_abundance = false;
        public static bool neucode_light_lysine = true;
        public static bool neucode_heavy_lysine = false;
        public static int max_ptms = 3;
        public static int decoy_databases = 1;
        public static int min_peptide_length = 7;
        public static bool combine_identical_sequences = true;
        public static string uniprot_xml_filepath = "";
        public static string ptmlist_filepath = "";
        public static List<TheoreticalProteoform> theoretical_proteoforms = new List<TheoreticalProteoform>();
        public static Dictionary<string, List<TheoreticalProteoform>> decoy_proteoforms = new Dictionary<string, List<TheoreticalProteoform>>();
        static Protein[] proteins;

        static ProteomeDatabaseReader proteomeDatabaseReader = new ProteomeDatabaseReader();
        static Dictionary<string, Modification> uniprotModificationTable;
        static Dictionary<char, double> aaIsotopeMassList;

        public static void make_databases()
        {
            //Clear out data from potential previous runs
            Lollipop.theoretical_proteoforms = new List<TheoreticalProteoform>();
            Lollipop.decoy_proteoforms = new Dictionary<string, List<TheoreticalProteoform>>();
            Lollipop.experimentTheoreticalPairs = new DataTable();
            Lollipop.experimentDecoyPairs = new DataSet();
            Lollipop.experimentExperimentPairs = new DataTable();
            Lollipop.ProteoformFamiliesEE = new DataSet();
            Lollipop.ProteoformFamiliesET = new DataSet();
            ProteomeDatabaseReader.oldPtmlistFilePath = ptmlist_filepath;
            uniprotModificationTable = proteomeDatabaseReader.ReadUniprotPtmlist();
            aaIsotopeMassList = new AminoAcidMasses(methionine_oxidation, carbamidomethylation, WhichLysineIsotopeComposition()).AA_Masses;

            //Read the UniProt-XML and ptmlist
            proteins = ProteomeDatabaseReader.ReadUniprotXml(uniprot_xml_filepath, min_peptide_length, methionine_cleavage);

            if (combine_identical_sequences) //used aggregated proteoforms in the creation of the theoretical database
            {
                //consolodate proteins that have identical sequences into protein groups. this also aggregates ptms
                List<string> sequences = ProteinSequenceGroups.uniqueProteinSequences(proteins);
                proteins = ProteinSequenceGroups.consolidateProteins(proteins, sequences);
            }

            //Concatenate a giant protein out of all protein read from the UniProt-XML, and construct target and decoy proteoform databases
            string giantProtein = GetOneGiantProtein(proteins, methionine_cleavage);
            processEntries();
            processDecoys(giantProtein);
        }

        private static string WhichLysineIsotopeComposition()
        {
            if (natural_lysine_isotope_abundance) return "n";
            else if (neucode_light_lysine) return "l";
            else if (neucode_heavy_lysine) return "h";
            else return "";
        }

        private static DataTable GenerateProteoformDatabaseDataTable(string title)
        {
            DataTable dt = new DataTable(title);//datatable name goes in parentheses.
            dt.Columns.Add("Accession", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Fragment", typeof(string));
            dt.Columns.Add("Begin", typeof(int));
            dt.Columns.Add("End", typeof(int));
            dt.Columns.Add("Mass", typeof(double));
            dt.Columns.Add("Lysine Count", typeof(int));
            dt.Columns.Add("PTM List", typeof(string));
            dt.Columns.Add("PTM Group Mass", typeof(double));
            dt.Columns.Add("Proteoform Mass", typeof(double));
            return dt;
        }

        private static void processEntries()
        {
            List<TheoreticalProteoform> targets = new List<TheoreticalProteoform>();
            Parallel.ForEach<Protein>(proteins, p =>
            {
                bool isMetCleaved = (methionine_cleavage && p.Begin == 0 && p.Sequence.Substring(0, 1) == "M"); // methionine cleavage of N-terminus specified
                int startPosAfterCleavage = Convert.ToInt32(isMetCleaved);
                string seq = p.Sequence.Substring(startPosAfterCleavage, (p.Sequence.Length - startPosAfterCleavage));
                EnterTheoreticalProteformFamily(targets, seq, p, p.Accession, max_ptms, isMetCleaved, aaIsotopeMassList, uniprotModificationTable);
            });
            Lollipop.theoretical_proteoforms = targets;
        }

        private static void processDecoys(string giantProtein)
        {
            Random rng = new Random();
            Parallel.For(0, Lollipop.decoy_databases, decoyNumber =>
            {
                List<TheoreticalProteoform> decoys = new List<TheoreticalProteoform>();
                Protein[] shuffled_proteins = new Protein[proteins.Count()];
                shuffled_proteins = proteins.ToArray();
                new Random().Shuffle<Protein>(shuffled_proteins); //Randomize Order of Protein Array

                int prevLength = 0;
                Parallel.ForEach<Protein>(shuffled_proteins.ToList(), p =>
                {
                    bool isMetCleaved = (methionine_cleavage && p.Begin == 0 && p.Sequence.Substring(0, 1) == "M"); // methionine cleavage of N-terminus specified
                    int startPosAfterCleavage = Convert.ToInt32(isMetCleaved);

                    //From the concatenated proteome, cut a decoy sequence of a randomly selected length
                    int hunkLength = p.Sequence.Length - startPosAfterCleavage;
                    string hunk = giantProtein.Substring(prevLength, hunkLength);
                    prevLength += hunkLength;

                    EnterTheoreticalProteformFamily(decoys, hunk, p, p.Accession +
                        "_DECOY_" + decoyNumber, max_ptms, isMetCleaved, aaIsotopeMassList, uniprotModificationTable);
                });
                decoy_proteoforms.Add("DecoyDatabase_" + decoyNumber, decoys);
            });
        }

        private static void EnterTheoreticalProteformFamily(List<TheoreticalProteoform> proteoforms, string seq, Protein prot, string accession, int maxPTMsPerProteoform, bool isMetCleaved,
            Dictionary<char, double> aaIsotopeMassList, Dictionary<string, Modification> uniprotModificationTable)
        {
            //Calculate the properties of this sequence
            double mass = CalculateProteoformMass(seq);
            int kCount = seq.Split('K').Length - 1;
            
            //Initialize a PTM combination list with "unmodified," and then add other PTMs 
            List<OneUniquePtmGroup> aupg = new List<OneUniquePtmGroup>(new OneUniquePtmGroup[] { new OneUniquePtmGroup(0, new List<string>(new string[] { "unmodified" })) });
            bool addPtmCombos = maxPTMsPerProteoform > 0 && prot.PositionsAndPtms.Count() > 0;
            if (addPtmCombos)
            {
                aupg.AddRange(new PtmCombos().combos(maxPTMsPerProteoform, uniprotModificationTable, prot.PositionsAndPtms));
            }

            foreach (OneUniquePtmGroup group in aupg)
            {
                List<string> ptm_list = group.unique_ptm_combinations;
                //if (!isMetCleaved) { MessageBox.Show("PTM Combinations: " + String.Join("; ", ptm_list)); }
                Double ptm_mass = group.mass;
                Double proteoform_mass = mass + group.mass;
                table.Rows.Add(accession, prot.Name, prot.Fragment, prot.Begin + Convert.ToInt32(isMetCleaved), prot.End, mass, kCount, string.Join("; ", ptm_list), ptm_mass, proteoform_mass);
            }
        }

        private static double CalculateProteoformMass(string pForm)
        {
            double proteoformMass = 18.010565; // start with water
            char[] aminoAcids = pForm.ToCharArray();
            List<double> aaMasses = new List<double>();
            Parallel.For(0, pForm.Length, i =>
            {
                if (aaIsotopeMassList.ContainsKey(aminoAcids[i]))
                    aaMasses.Add(aaIsotopeMassList[aminoAcids[i]]);
            });
            return proteoformMass + aaMasses.Sum();
        }

        private static string GetOneGiantProtein(IEnumerable<Protein> proteins, bool mC)
        {
            StringBuilder giantProtein = new StringBuilder(5000000); // this set-aside is autoincremented to larger values when necessary.
            foreach (Protein protein in proteins)
            {
                string sequence = protein.Sequence;
                bool isMetCleaved = mC && (sequence.Substring(0, 1) == "M");
                int startPosAfterMetCleavage = Convert.ToInt32(isMetCleaved);
                switch (protein.Fragment)
                {
                    case "chain":
                    case "signal peptide":
                    case "propeptide":
                    case "peptide":
                        giantProtein.Append(".");
                        break;
                    default:
                        giantProtein.Append("-");
                        break;
                }
                giantProtein.Append(sequence.Substring(startPosAfterMetCleavage));
            }
            return giantProtein.ToString();
        }


        //METHOD FILE
        public static string method_toString()
        {
            return String.Join(System.Environment.NewLine, new string[] {
                "NeuCodePairs|max_intensity_ratio\t" + max_intensity_ratio.ToString(),
                "NeuCodePairs|min_intensity_ratio\t" + min_intensity_ratio.ToString(),
                "NeuCodePairs|max_lysine_ct\t" + max_lysine_ct.ToString(),
                "NeuCodePairs|min_lysine_ct\t" + min_lysine_ct.ToString(),
                "AggregatedProteoforms|mass_tolerance\t" + mass_tolerance.ToString(),
                "AggregatedProteoforms|retention_time_tolerance\t" + retention_time_tolerance.ToString(),
                "AggregatedProteoforms|missed_monos\t" + missed_monos.ToString(),
                "AggregatedProteoforms|missed_lysines\t" + missed_lysines.ToString(),
                "TheoreticalDatabase|uniprot_xml_filepath\t" + uniprot_xml_filepath,
                "TheoreticalDatabase|ptmlist_filepath\t" + ptmlist_filepath,
                "TheoreticalDatabase|methionine_oxidation\t" + methionine_oxidation.ToString(),
                "TheoreticalDatabase|carbamidomethylation\t" + carbamidomethylation.ToString(),
                "TheoreticalDatabase|methionine_cleavage\t" + methionine_cleavage.ToString(),
                "TheoreticalDatabase|neucode_light_lysine\t" + neucode_light_lysine.ToString(),
                "TheoreticalDatabase|neucode_heavy_lysine\t" + neucode_heavy_lysine.ToString(),
                "TheoreticalDatabase|natural_lysine_isotope_abundance\t" + natural_lysine_isotope_abundance.ToString(),
                "TheoreticalDatabase|max_ptms\t" + max_ptms.ToString(),
                "TheoreticalDatabase|decoy_databases\t" + decoy_databases.ToString(),
                "TheoreticalDatabase|min_peptide_length\t" + min_peptide_length.ToString(),
                "TheoreticalDatabase|combine_identical_sequences\t" + combine_identical_sequences.ToString()
            });
        }

        public static void load_setting(string setting_spec)
        {
            string[] fields = setting_spec.Split('\t');
            switch (fields[0])
            {
                case "NeuCodePairs|max_intensity_ratio":
                    max_intensity_ratio = Convert.ToDecimal(fields[1]);
                    break;
                case "NeuCodePairs|min_intensity_ratio":
                    min_intensity_ratio = Convert.ToDecimal(fields[1]);
                    break;
                case "NeuCodePairs|max_lysine_ct":
                    max_lysine_ct = Convert.ToDecimal(fields[1]);
                    break;
                case "NeuCodePairs|min_lysine_ct":
                    min_lysine_ct = Convert.ToDecimal(fields[1]);
                    break;
                case "AggregatedProteoforms|mass_tolerance":
                    mass_tolerance = Convert.ToDecimal(fields[1]);
                    break;
                case "AggregatedProteoforms|retention_time_tolerance":
                    retention_time_tolerance = Convert.ToDecimal(fields[1]);
                    break;
                case "AggregatedProteoforms|missed_monos":
                    missed_monos = Convert.ToDecimal(fields[1]);
                    break;
                case "AggregatedProteoforms|missed_lysines":
                    missed_lysines = Convert.ToDecimal(fields[1]);
                    break;
                case "TheoreticalDatabase|uniprot_xml_filepath":
                    uniprot_xml_filepath = fields[1];
                    break;
                case "TheoreticalDatabase|ptmlist_filepath":
                    ptmlist_filepath = fields[1];
                    break;
                case "TheoreticalDatabase|methionine_oxidation":
                    methionine_oxidation = Convert.ToBoolean(fields[1]);
                    break;
                case "TheoreticalDatabase|carbamidomethylation":
                    carbamidomethylation = Convert.ToBoolean(fields[1]);
                    break;
                case "TheoreticalDatabase|methionine_cleavage":
                    methionine_cleavage = Convert.ToBoolean(fields[1]);
                    break;
                case "TheoreticalDatabase|neucode_light_lysine":
                    neucode_light_lysine = Convert.ToBoolean(fields[1]);
                    break;
                case "TheoreticalDatabase|neucode_heavy_lysine":
                    neucode_heavy_lysine = Convert.ToBoolean(fields[1]);
                    break;
                case "TheoreticalDatabase|natural_lysine_isotope_abundance":
                    natural_lysine_isotope_abundance = Convert.ToBoolean(fields[1]);
                    break;
                case "TheoreticalDatabase|combine_identical_sequences":
                    combine_identical_sequences = Convert.ToBoolean(fields[1]);
                    break;
                case "TheoreticalDatabase|max_ptms":
                    max_ptms = Convert.ToInt32(fields[1]);
                    break;
                case "TheoreticalDatabase|decoy_databases":
                    decoy_databases = Convert.ToInt32(fields[1]);
                    break;
                case "TheoreticalDatabase|min_peptide_length":
                    min_peptide_length = Convert.ToInt32(fields[1]);
                    break;
            }
        }
    }
}
