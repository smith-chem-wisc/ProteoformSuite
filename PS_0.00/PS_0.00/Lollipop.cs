using System;
using System.Collections.Generic;
using System.ComponentModel; // needed for bindinglist
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Packaging;
using System.Windows.Forms;

namespace PS_0._00
{
    public class Lollipop
    {
        public static ProteoformCommunity proteoform_community = new ProteoformCommunity();
        public static DataSet experimentDecoyPairs = new DataSet();
        public static DataTable edList = new DataTable();
        public static DataTable eePeakList = new DataTable();
        public static DataTable EE_Parent = new DataTable();
        public static DataSet ProteoformFamiliesET = new DataSet();
        public static DataSet ProteoformFamiliesEE = new DataSet();
        public static DataTable ProteoformFamilyMetrics = new DataTable();

        public static void get_experimental_proteoforms()
        {
            Lollipop.process_raw_components();
            Lollipop.aggregate_neucode_light_proteoforms();
        }

        //RAW EXPERIMENTAL COMPONENTS
        public static ExcelReader excel_reader = new ExcelReader();
        public static BindingList<string> deconResultsFileNames = new BindingList<string>();
        public static List<Component> raw_experimental_components = new List<Component>();
        public static void process_raw_components()
        {
            Parallel.ForEach<string>(Lollipop.deconResultsFileNames, filename =>
            {
                List<Component> raw_components = excel_reader.read_components_from_xlsx(filename);
                raw_experimental_components.AddRange(raw_components);

                HashSet<string> scan_ranges = new HashSet<string>(raw_components.Select(c => c.scan_range));
                Parallel.ForEach<string>(scan_ranges, scan_range =>
                {
                    find_neucode_pairs(raw_components.Where(c => c.scan_range == scan_range));
                });
            });
        }

        //NEUCODE PAIRS
        public static List<NeuCodePair> raw_neucode_pairs = new List<NeuCodePair>();
        public static decimal max_intensity_ratio = 6;
        public static decimal min_intensity_ratio = 1.4m;
        public static decimal max_lysine_ct = 26.2m;
        public static decimal min_lysine_ct = 1.5m;
        public static void find_neucode_pairs(IEnumerable<Component> components_in_file_scanrange)
        {
            //Add putative neucode pairs. Must be in same spectrum, mass must be within 6 Da of each other
            List<Component> components = components_in_file_scanrange.OrderBy(c => c.weighted_monoisotopic_mass).ToList();
            Parallel.ForEach<Component>(components, lower_component =>
            {
                IEnumerable<Component> higher_mass_components = components.Where(higher_component => higher_component != lower_component && higher_component.weighted_monoisotopic_mass >= lower_component.weighted_monoisotopic_mass);
                Parallel.ForEach<Component>(higher_mass_components, higher_component =>
                {
                    double mass_difference = higher_component.weighted_monoisotopic_mass - lower_component.weighted_monoisotopic_mass; //changed from decimal; it doesn't seem like that should make a difference
                    if (mass_difference < 6)
                    {
                        NeuCodePair pair = new NeuCodePair(lower_component, higher_component);
                        if (pair.accepted) Lollipop.raw_neucode_pairs.Add(pair);
                    }
                });
            });
        }

        //public static void pair_neucode_components() //Legacy for form-load runs, or and starting over at NeuCodePairs
        //{
        //    Dictionary<string, HashSet<string>> filename_scanRange = Lollipop.get_scanranges_by_filename();
        //    Parallel.ForEach<KeyValuePair<string, HashSet<string>>>(filename_scanRange, entry =>
        //    {
        //        string filename = entry.Key;
        //        Parallel.ForEach<string>(entry.Value, scanRange =>
        //        {
        //            List<Component> components_in_file_scanrange = new List<Component>();

        //            //select all components in file and this particular scanrange
        //            Parallel.ForEach<Component>(Lollipop.rawExperimentalComponents, c =>
        //            {
        //                if (c.file_origin == filename && c.scan_range == scanRange)
        //                    components_in_file_scanrange.Add(c);
        //            });

        //            components_in_file_scanrange.OrderBy(c => c.weighted_monoisotopic_mass);

        //            //Add putative neucode pairs. Must be in same spectrum, mass must be within 6 Da of each other
        //            int lower_mass_index = 0;
        //            Parallel.For(lower_mass_index, components_in_file_scanrange.Count - 2, lower_index =>
        //            {
        //                Component lower_component = components_in_file_scanrange[lower_index];
        //                int higher_mass_index = lower_mass_index + 1;
        //                //double apexRT = lower_component.rt_apex;

        //                Parallel.For(higher_mass_index, components_in_file_scanrange.Count - 1, higher_index =>
        //                {
        //                    Component higher_component = components_in_file_scanrange[higher_index];
        //                    double mass_difference = higher_component.weighted_monoisotopic_mass - lower_component.weighted_monoisotopic_mass; //changed from decimal; it doesn't seem like that should make a difference
        //                    if (mass_difference < 6)
        //                    {
        //                        NeuCodePair p = new NeuCodePair(lower_component, higher_component);
        //                        if (p.accepted) { Lollipop.rawNeuCodePairs.Add(p); }
        //                    }
        //                });
        //            });
        //        });
        //    });
        //}

        //private static Dictionary<string, HashSet<string>> get_scanranges_by_filename()
        //{
        //    Dictionary<string, HashSet<string>> filename_scanRanges = new Dictionary<string, HashSet<string>>();
        //    Parallel.ForEach<string>(Lollipop.deconResultsFileNames, filename =>
        //    {
        //        if (!filename_scanRanges.ContainsKey(filename))
        //            filename_scanRanges.Add(filename, new HashSet<string>());
        //    });

        //    Parallel.ForEach<Component>(Lollipop.rawExperimentalComponents, c =>
        //    {
        //        filename_scanRanges[c.file_origin].Add(c.scan_range);
        //    });
        //    return filename_scanRanges;
        //}

        //AGGREGATED PROTEOFORMS
        public static decimal mass_tolerance = 3; //ppm
        public static decimal retention_time_tolerance = 3; //min
        public static decimal missed_monos = 3;
        public static decimal missed_lysines = 1;
        //public static List<ExperimentalProteoform> experimental_proteoforms = new List<ExperimentalProteoform>();
        public static void aggregate_neucode_light_proteoforms()
        {
            if (Lollipop.proteoform_community.experimental_proteoforms.Count > 0)
                Lollipop.proteoform_community.experimental_proteoforms.Clear();

            //Rooting each experimental proteoform is handled in addition of each NeuCode pair.
            List<NeuCodePair> remaining_acceptableProteoforms = Lollipop.raw_neucode_pairs.Where(p => p.accepted) //Accepted NeuCode pairs
                    .OrderByDescending(p => p.light_intensity).ToList(); //ordered list, so that the proteoform with max intensity is always chosen first

            int count = 1;
            while (remaining_acceptableProteoforms.Count > 0)
            {
                NeuCodePair root = remaining_acceptableProteoforms[0];
                remaining_acceptableProteoforms.Remove(root);
                ExperimentalProteoform new_pf = new ExperimentalProteoform("E_" + count, root, true);
                Lollipop.proteoform_community.add(new_pf);
                Parallel.ForEach<NeuCodePair>(remaining_acceptableProteoforms, p => 
                    { if (new_pf.includes(p)) new_pf.add(p); });
                new_pf.calculate_properties();
                remaining_acceptableProteoforms = remaining_acceptableProteoforms.Except(new_pf.aggregated_neucode_pairs).ToList();
                count += 1;
            }
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
        public static double ptmset_mass_tolerance = 0.00001;
        public static bool combine_identical_sequences = true;
        public static string uniprot_xml_filepath = "";
        public static string ptmlist_filepath = "";
        //public static List<TheoreticalProteoform> theoretical_proteoforms = new List<TheoreticalProteoform>();
        //public static Dictionary<string, List<TheoreticalProteoform>> decoy_proteoforms = new Dictionary<string, List<TheoreticalProteoform>>();
        static Protein[] proteins;

        static ProteomeDatabaseReader proteomeDatabaseReader = new ProteomeDatabaseReader();
        static Dictionary<string, Modification> uniprotModificationTable;
        static Dictionary<char, double> aaIsotopeMassList;

        public static void get_theoretical_proteoforms()
        {
            //Clear out data from potential previous runs
            proteoform_community.Clear();
            Lollipop.ProteoformFamiliesEE = new DataSet();
            Lollipop.ProteoformFamiliesET = new DataSet();
            ProteomeDatabaseReader.oldPtmlistFilePath = ptmlist_filepath;
            uniprotModificationTable = proteomeDatabaseReader.ReadUniprotPtmlist();
            aaIsotopeMassList = new AminoAcidMasses(methionine_oxidation, carbamidomethylation).AA_Masses;

            //Read the UniProt-XML and ptmlist
            proteins = ProteomeDatabaseReader.ReadUniprotXml(uniprot_xml_filepath, uniprotModificationTable, min_peptide_length, methionine_cleavage);
            if (combine_identical_sequences) proteins = group_proteins_by_sequence(proteins);
            Parallel.Invoke(
                () => process_entries(), 
                () => process_decoys()
            );
        }

        private static ProteinSequenceGroup[] group_proteins_by_sequence(IEnumerable<Protein> proteins)
        {
            List<ProteinSequenceGroup> protein_sequence_groups = new List<ProteinSequenceGroup>();
            HashSet<string> unique_sequences = new HashSet<string>(proteins.Select(p => p.sequence));
            foreach (string sequence in unique_sequences) protein_sequence_groups.Add(new ProteinSequenceGroup(proteins.Where(p => p.sequence == sequence).ToList()));
            return protein_sequence_groups.ToArray();
        }

        private static void process_entries()
        {
            Parallel.ForEach<Protein>(proteins, p =>
            {
                bool isMetCleaved = (methionine_cleavage && p.begin == 0 && p.sequence.Substring(0, 1) == "M"); // methionine cleavage of N-terminus specified
                int startPosAfterCleavage = Convert.ToInt32(isMetCleaved);
                string seq = p.sequence.Substring(startPosAfterCleavage, (p.sequence.Length - startPosAfterCleavage));
                EnterTheoreticalProteformFamily(seq, p, p.accession, isMetCleaved, null);
            });
        }

        private static void process_decoys()
        {
            string giantProtein = GetOneGiantProtein(proteins, methionine_cleavage); //Concatenate a giant protein out of all protein read from the UniProt-XML, and construct target and decoy proteoform databases
            Parallel.For(0, Lollipop.decoy_databases, decoyNumber =>
            {
                string decoy_database_name = "DecoyDatabase_" + decoyNumber;
                proteoform_community.decoy_proteoforms.Add(decoy_database_name, new List<TheoreticalProteoform>());
                Protein[] shuffled_proteins = new Protein[proteins.Length];
                shuffled_proteins = proteins;
                new Random().Shuffle(shuffled_proteins); //randomize order of protein array

                int prevLength = 0;
                foreach (Protein p in shuffled_proteins)
                {
                    bool isMetCleaved = (methionine_cleavage && p.begin == 0 && p.sequence.Substring(0, 1) == "M"); // methionine cleavage of N-terminus specified
                    int startPosAfterCleavage = Convert.ToInt32(isMetCleaved);

                    //From the concatenated proteome, cut a decoy sequence of a randomly selected length
                    int hunkLength = p.sequence.Length - startPosAfterCleavage;
                    string hunk = giantProtein.Substring(prevLength, hunkLength);
                    prevLength += hunkLength;

                    EnterTheoreticalProteformFamily(hunk, p, p.accession + "_DECOY_" + decoyNumber, isMetCleaved, decoy_database_name);
                }
            });
        }

        private static void EnterTheoreticalProteformFamily(string seq, Protein prot, string accession, bool isMetCleaved, string decoy_database_name)
        {
            //Calculate the properties of this sequence
            double unmodified_mass = TheoreticalProteoform.CalculateProteoformMass(seq, aaIsotopeMassList);
            int lysine_count = seq.Split('K').Length - 1;
            
            //Initialize a PTM combination list with "unmodified," and then add other PTMs 
            List<PtmSet> unique_ptm_groups = new List<PtmSet>();
            bool addPtmCombos = max_ptms > 0 && prot.ptms_by_position.Count() > 0;
            if (addPtmCombos) unique_ptm_groups.AddRange(new PtmCombos(prot.ptms_by_position).get_combinations(max_ptms));

            Parallel.ForEach<PtmSet>( unique_ptm_groups, group =>
            {
                List<Ptm> ptm_list = group.ptm_combination.ToList();
                double ptm_mass = group.mass;
                double proteoform_mass = unmodified_mass + group.mass;
                if (string.IsNullOrEmpty(decoy_database_name))
                    proteoform_community.add(new TheoreticalProteoform(accession, prot.name, prot.fragment, prot.begin + Convert.ToInt32(isMetCleaved), prot.end, unmodified_mass, lysine_count, ptm_list, ptm_mass, proteoform_mass, true));
                else
                    proteoform_community.add(new TheoreticalProteoform(accession, prot.name, prot.fragment, prot.begin + Convert.ToInt32(isMetCleaved), prot.end, unmodified_mass, lysine_count, ptm_list, ptm_mass, proteoform_mass, false), decoy_database_name);
            });
        }

        private static string GetOneGiantProtein(IEnumerable<Protein> proteins, bool methionine_cleavage)
        {
            StringBuilder giantProtein = new StringBuilder(5000000); // this set-aside is autoincremented to larger values when necessary.
            foreach (Protein protein in proteins)
            {
                string sequence = protein.sequence;
                bool isMetCleaved = methionine_cleavage && (sequence.Substring(0, 1) == "M");
                int startPosAfterMetCleavage = Convert.ToInt32(isMetCleaved);
                switch (protein.fragment)
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

        //ET,ED,EE,EF COMPARISONS
        public static double max_mass_difference = 500; //TODO: implement this in ProteoformFamilies and elsewhere
        public static double no_mans_land_lowerBound = 0.22;
        public static double no_mans_land_upperBound = 0.88;
        public static double peak_width_base = 0.0150;
        public static double min_peak_count = 10;
        public static int relation_group_centering_iterations = 2;
        public static List<ProteoformRelation> et_relations = new List<ProteoformRelation>();
        public static List<ProteoformRelation> ee_relations = new List<ProteoformRelation>();
        public static Dictionary<string, List<ProteoformRelation>> ed_relations = new Dictionary<string, List<ProteoformRelation>>();
        public static List<ProteoformRelation> ef_relations = new List<ProteoformRelation>();
        public static List<DeltaMassPeak> et_peaks = new List<DeltaMassPeak>();
        public static List<DeltaMassPeak> ee_peaks = new List<DeltaMassPeak>();

        public static void make_et_relationships()
        {
            Parallel.Invoke(
                () => et_relations = Lollipop.proteoform_community.relate_et(),
                () => ed_relations = Lollipop.proteoform_community.relate_ed()
            );
            et_peaks = Lollipop.proteoform_community.accept_deltaMass_peaks(Lollipop.et_relations, Lollipop.ed_relations);
        }

        public static void make_ee_relationships()
        {
            Parallel.Invoke(
                () => ee_relations = proteoform_community.relate_ee(),
                () => ef_relations = proteoform_community.relate_unequal_ee_lysine_counts()
            );
            ee_peaks = Lollipop.proteoform_community.accept_deltaMass_peaks(Lollipop.ee_relations, Lollipop.ef_relations);
        }

        //PROTEOFORM FAMILIES
        public static double maximum_delta_mass_peak_fdr = 25;

        //METHOD FILE
        public static string method_toString()
        {
            return String.Join(System.Environment.NewLine, new string[] {
                "LoadDeconvolutionResults|deconvolution_file_names\t" + String.Join("\t", Lollipop.deconResultsFileNames.ToArray<string>()),
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
                "TheoreticalDatabase|combine_identical_sequences\t" + combine_identical_sequences.ToString(),
                "Comparisons|no_mans_land_lowerBound\t" + no_mans_land_lowerBound.ToString(),
                "Comparisons|no_mans_land_upperBound\t" + no_mans_land_upperBound.ToString(),
                "Comparisons|max_mass_difference\t" + max_mass_difference.ToString(),
                "Comparisons|relation_group_centering_iterations\t" + relation_group_centering_iterations.ToString(),
                "Comparisons|peak_width_base\t" + peak_width_base.ToString(),
                "Comparisons|min_peak_count\t" + min_peak_count.ToString()
            });
        }

        public static void load_setting(string setting_spec)
        {
            string[] fields = setting_spec.Split('\t');
            switch (fields[0])
            {
                case "LoadDeconvolutionResults|deconvolution_file_names": foreach (string filename in fields) { if (filename != "LoadDeconvolutionResults|deconvolution_file_names") Lollipop.deconResultsFileNames.Add(filename); } break;
                case "NeuCodePairs|max_intensity_ratio": max_intensity_ratio = Convert.ToDecimal(fields[1]); break;
                case "NeuCodePairs|min_intensity_ratio": min_intensity_ratio = Convert.ToDecimal(fields[1]); break;
                case "NeuCodePairs|max_lysine_ct": max_lysine_ct = Convert.ToDecimal(fields[1]); break;
                case "NeuCodePairs|min_lysine_ct": min_lysine_ct = Convert.ToDecimal(fields[1]); break;
                case "AggregatedProteoforms|mass_tolerance": mass_tolerance = Convert.ToDecimal(fields[1]); break;
                case "AggregatedProteoforms|retention_time_tolerance": retention_time_tolerance = Convert.ToDecimal(fields[1]); break;
                case "AggregatedProteoforms|missed_monos": missed_monos = Convert.ToDecimal(fields[1]); break;
                case "AggregatedProteoforms|missed_lysines": missed_lysines = Convert.ToDecimal(fields[1]); break;
                case "TheoreticalDatabase|uniprot_xml_filepath": uniprot_xml_filepath = fields[1]; break;
                case "TheoreticalDatabase|ptmlist_filepath": ptmlist_filepath = fields[1]; break;
                case "TheoreticalDatabase|methionine_oxidation": methionine_oxidation = Convert.ToBoolean(fields[1]); break;
                case "TheoreticalDatabase|carbamidomethylation": carbamidomethylation = Convert.ToBoolean(fields[1]); break;
                case "TheoreticalDatabase|methionine_cleavage": methionine_cleavage = Convert.ToBoolean(fields[1]); break;
                case "TheoreticalDatabase|neucode_light_lysine": neucode_light_lysine = Convert.ToBoolean(fields[1]); break;
                case "TheoreticalDatabase|neucode_heavy_lysine": neucode_heavy_lysine = Convert.ToBoolean(fields[1]); break;
                case "TheoreticalDatabase|natural_lysine_isotope_abundance": natural_lysine_isotope_abundance = Convert.ToBoolean(fields[1]); break;
                case "TheoreticalDatabase|combine_identical_sequences": combine_identical_sequences = Convert.ToBoolean(fields[1]); break;
                case "TheoreticalDatabase|max_ptms": max_ptms = Convert.ToInt32(fields[1]); break;
                case "TheoreticalDatabase|decoy_databases": decoy_databases = Convert.ToInt32(fields[1]); break;
                case "TheoreticalDatabase|min_peptide_length": min_peptide_length = Convert.ToInt32(fields[1]); break;
                case "Comparisons|no_mans_land_lowerBound": no_mans_land_lowerBound = Convert.ToDouble(fields[1]); break;
                case "Comparisons|no_mans_land_upperBound": no_mans_land_upperBound = Convert.ToDouble(fields[1]); break;
                case "Comparisons|peak_width_base": peak_width_base = Convert.ToDouble(fields[1]); break;
                case "Comparisons|min_peak_count": min_peak_count = Convert.ToDouble(fields[1]); break;
            }
        }
    }
}
