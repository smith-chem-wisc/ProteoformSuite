﻿using System;
using System.Collections.Generic;
using System.ComponentModel; // needed for bindinglist
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
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

        public static void get_experimental_proteoforms(Func<string, IEnumerable<Component>> componentReader)
        {
            Lollipop.process_raw_components(componentReader);
            Lollipop.aggregate_proteoforms();
        }

        //RAW EXPERIMENTAL COMPONENTS
        public static BindingList<string> deconResultsFileNames = new BindingList<string>();
        public static List<Component> raw_experimental_components = new List<Component>();
        public static bool neucode_labeled = true;
        public static void process_raw_components(Func<string, IEnumerable<Component>> componentReader)
        {
            Parallel.ForEach<string>(Lollipop.deconResultsFileNames, filename =>
            {
                IEnumerable<Component> readComponents = componentReader(filename);
                List<Component> raw_components = new List<Component>(readComponents);
                raw_experimental_components.AddRange(raw_components);

                if (neucode_labeled)
                {
                    HashSet<string> scan_ranges = new HashSet<string>(raw_components.Select(c => c.scan_range));
                    Parallel.ForEach<string>(scan_ranges, scan_range =>
                    {
                        find_neucode_pairs(raw_components.Where(c => c.scan_range == scan_range));
                    });
                }
            });
            raw_experimental_components = raw_experimental_components.Where(c => c != null).ToList();
            raw_neucode_pairs = raw_neucode_pairs.Where(p => p != null).ToList();
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
            foreach (Component lower_component in components)
            {
                IEnumerable<Component> higher_mass_components = components.Where(higher_component => higher_component != lower_component && higher_component.weighted_monoisotopic_mass >= lower_component.weighted_monoisotopic_mass);
                foreach (Component higher_component in higher_mass_components)
                {
                    double mass_difference = higher_component.weighted_monoisotopic_mass - lower_component.weighted_monoisotopic_mass; //changed from decimal; it doesn't seem like that should make a difference
                    if (mass_difference < 6)
                    {
                        List<int> lower_charges = lower_component.charge_states.Select(charge_state => charge_state.charge_count).ToList<int>();
                        List<int> higher_charges = higher_component.charge_states.Select(charge_states => charge_states.charge_count).ToList<int>();
                        List<int> overlapping_charge_states = lower_charges.Intersect(higher_charges).ToList();
                        double lower_intensity = lower_component.calculate_sum_intensity(overlapping_charge_states);
                        double higher_intensity = higher_component.calculate_sum_intensity(overlapping_charge_states);

                        if (lower_intensity > 0 || higher_intensity > 0)
                        {
                            NeuCodePair pair;
                            if (lower_intensity > higher_intensity) pair = new NeuCodePair(lower_component, higher_component, mass_difference, overlapping_charge_states); //lower mass is neucode light
                            else pair = new NeuCodePair(higher_component, lower_component, mass_difference, overlapping_charge_states); //higher mass is neucode light
                            Lollipop.raw_neucode_pairs.Add(pair);
                        }
                    }
                }
            }
        }

        //AGGREGATED PROTEOFORMS
        public static decimal mass_tolerance = 3; //ppm
        public static decimal retention_time_tolerance = 3; //min
        public static decimal missed_monos = 3;
        public static decimal missed_lysines = 1;
        public static void aggregate_proteoforms()
        {
            if (Lollipop.proteoform_community.experimental_proteoforms.Count > 0)
                Lollipop.proteoform_community.experimental_proteoforms.Clear();

            //Rooting each experimental proteoform is handled in addition of each NeuCode pair.
            //If no NeuCodePairs exist, e.g. for an experiment without labeling, the raw components are used instead.
            //Uses an ordered list, so that the proteoform with max intensity is always chosen first
            Lollipop.raw_neucode_pairs = Lollipop.raw_neucode_pairs.Where(p => p != null).ToList();
            Component[] remaining_proteoforms;
            //only aggregate accepatable neucode pairs
            if (Lollipop.raw_neucode_pairs.Count > 0) remaining_proteoforms = Lollipop.raw_neucode_pairs.OrderByDescending(p => p.intensity_sum).Where(p => p.accepted == true).ToArray();
            else remaining_proteoforms = Lollipop.raw_experimental_components.OrderByDescending(p => p.intensity_sum).ToArray();

            int count = 1;
            while (remaining_proteoforms.Length > 0)
            {
                Component root = remaining_proteoforms[0];
                List<Component> tmp_remaining_proteoforms = remaining_proteoforms.ToList();
                tmp_remaining_proteoforms.Remove(root);
                ExperimentalProteoform new_pf = new ExperimentalProteoform("E_" + count, root, tmp_remaining_proteoforms, true);
                Lollipop.proteoform_community.add(new_pf);
                remaining_proteoforms = tmp_remaining_proteoforms.Except(new_pf.aggregated_components).ToArray();
                count += 1;
            }
            Lollipop.proteoform_community.experimental_proteoforms = Lollipop.proteoform_community.experimental_proteoforms.Where(p => p != null).ToList();
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
            Lollipop.proteoform_community.theoretical_proteoforms.Clear();
            Lollipop.proteoform_community.decoy_proteoforms.Clear();
            Lollipop.ProteoformFamiliesET = new DataSet();
            ProteomeDatabaseReader.oldPtmlistFilePath = ptmlist_filepath;
            uniprotModificationTable = proteomeDatabaseReader.ReadUniprotPtmlist();
            aaIsotopeMassList = new AminoAcidMasses(methionine_oxidation, carbamidomethylation).AA_Masses;

            //Read the UniProt-XML and ptmlist
            proteins = ProteomeDatabaseReader.ReadUniprotXml(uniprot_xml_filepath, uniprotModificationTable, min_peptide_length, methionine_cleavage);
            if (combine_identical_sequences) proteins = group_proteins_by_sequence(proteins);

            process_entries();
            process_decoys();


            //Parallel.Invoke(
            //    () => process_entries(),
            //    () => process_decoys()
            //);
            Lollipop.proteoform_community.theoretical_proteoforms = Lollipop.proteoform_community.theoretical_proteoforms.ToList();
            Parallel.ForEach<List<TheoreticalProteoform>>(Lollipop.proteoform_community.decoy_proteoforms.Values, decoys =>
                decoys = decoys.Where(d => d != null).ToList()
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
            foreach (Protein p in proteins)
            {
                bool isMetCleaved = (methionine_cleavage && p.begin == 0 && p.sequence.Substring(0, 1) == "M"); // methionine cleavage of N-terminus specified
                int startPosAfterCleavage = Convert.ToInt32(isMetCleaved);
                string seq = p.sequence.Substring(startPosAfterCleavage, (p.sequence.Length - startPosAfterCleavage));
                EnterTheoreticalProteformFamily(seq, p, p.accession, isMetCleaved, null);
            }

            //Parallel.ForEach<Protein>(proteins, p =>
            //{
            //    bool isMetCleaved = (methionine_cleavage && p.begin == 0 && p.sequence.Substring(0, 1) == "M"); // methionine cleavage of N-terminus specified
            //    int startPosAfterCleavage = Convert.ToInt32(isMetCleaved);
            //    string seq = p.sequence.Substring(startPosAfterCleavage, (p.sequence.Length - startPosAfterCleavage));
            //    EnterTheoreticalProteformFamily(seq, p, p.accession, isMetCleaved, null);
            //});

        }

        private static void process_decoys()
        {
            for (int decoyNumber = 0; decoyNumber < Lollipop.decoy_databases; decoyNumber++)
            {
                string giantProtein = GetOneGiantProtein(proteins, methionine_cleavage); //Concatenate a giant protein out of all protein read from the UniProt-XML, and construct target and decoy proteoform databases
                string decoy_database_name = "DecoyDatabase_" + decoyNumber;
                Lollipop.proteoform_community.decoy_proteoforms.Add(decoy_database_name, new List<TheoreticalProteoform>());
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
            }
        }

        private static void EnterTheoreticalProteformFamily(string seq, Protein prot, string accession, bool isMetCleaved, string decoy_database_name)
        {
            //Calculate the properties of this sequence
            double unmodified_mass = TheoreticalProteoform.CalculateProteoformMass(seq, aaIsotopeMassList);
            int lysine_count = seq.Split('K').Length - 1;
            List<PtmSet> unique_ptm_groups = new List<PtmSet>();
            unique_ptm_groups.AddRange(new PtmCombos(prot.ptms_by_position).get_combinations(max_ptms));

            int listMemberNumber = 1;
            foreach (PtmSet group in unique_ptm_groups)
            {
                List<Ptm> ptm_list = group.ptm_combination.ToList();
                double ptm_mass = group.mass;
                double proteoform_mass = unmodified_mass + group.mass;
                if (string.IsNullOrEmpty(decoy_database_name))
                    proteoform_community.add(new TheoreticalProteoform(accession + "_" + prot.fragment + "_" + listMemberNumber.ToString(), prot.name, prot.fragment, prot.begin + Convert.ToInt32(isMetCleaved), prot.end, unmodified_mass, lysine_count, ptm_list, ptm_mass, proteoform_mass, true));
                else
                    proteoform_community.add(new TheoreticalProteoform(accession + "_" + prot.fragment + "_" + listMemberNumber.ToString(), prot.name, prot.fragment, prot.begin + Convert.ToInt32(isMetCleaved), prot.end, unmodified_mass, lysine_count, ptm_list, ptm_mass, proteoform_mass, false), decoy_database_name);
                listMemberNumber++;
            }
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
        public static double ee_max_mass_difference = 250; //TODO: implement this in ProteoformFamilies and elsewhere
        public static double et_low_mass_difference = -100;
        public static double et_high_mass_difference = 200;

        public static double no_mans_land_lowerBound = 0.22;
        public static double no_mans_land_upperBound = 0.88;
        public static double peak_width_base = 0.0150;
        public static double min_peak_count = 8;
        public static int relation_group_centering_iterations = 2;  // is this just arbitrary? whys is it specified here?
        public static List<ProteoformRelation> et_relations = new List<ProteoformRelation>();
        public static List<ProteoformRelation> ee_relations = new List<ProteoformRelation>();
        public static Dictionary<string, List<ProteoformRelation>> ed_relations = new Dictionary<string, List<ProteoformRelation>>();
        public static List<ProteoformRelation> ef_relations = new List<ProteoformRelation>();
        public static List<DeltaMassPeak> et_peaks = new List<DeltaMassPeak>();
        public static List<DeltaMassPeak> ee_peaks = new List<DeltaMassPeak>();

        public static void make_et_relationships()
        {
            Parallel.Invoke(
                () => et_relations = Lollipop.proteoform_community.relate_et(Lollipop.proteoform_community.experimental_proteoforms.ToArray(), Lollipop.proteoform_community.theoretical_proteoforms.ToArray(), ProteoformComparison.et),
                () => ed_relations = Lollipop.proteoform_community.relate_ed()
            );
            et_peaks = Lollipop.proteoform_community.accept_deltaMass_peaks(Lollipop.et_relations, Lollipop.ed_relations);
        }

        public static void make_ee_relationships()
        {
            Parallel.Invoke(
                () => ee_relations = Lollipop.proteoform_community.relate_ee(Lollipop.proteoform_community.experimental_proteoforms.ToArray(), Lollipop.proteoform_community.experimental_proteoforms.ToArray(), ProteoformComparison.et),
                () => ef_relations = proteoform_community.relate_unequal_ee_lysine_counts()
            );
            ee_peaks = Lollipop.proteoform_community.accept_deltaMass_peaks(Lollipop.ee_relations, Lollipop.ef_relations);
        }

        //PROTEOFORM FAMILIES
        public static double maximum_delta_mass_peak_fdr = 25;

        //RESULTS OUTPUT
        public static string raw_component_results()
        {
            return Component.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, Lollipop.raw_experimental_components.Where(c => c != null).Select(c => c.as_tsv_row()));
        }
        public static string raw_neucode_pair_results()
        {
            return NeuCodePair.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, Lollipop.raw_neucode_pairs.Select(p => p.as_tsv_row()));
        }
        public static string aggregated_experimental_proteoform_results()
        {
            return ExperimentalProteoform.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, Lollipop.proteoform_community.experimental_proteoforms.Select(p => p.as_tsv_row()));
        }
        public static string et_relations_results()
        {
            return ProteoformRelation.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, et_relations.Select(r => r.as_tsv_row()));
        }
        public static string et_peak_results()
        {
            return DeltaMassPeak.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, et_peaks.Select(r => r.as_tsv_row()));
        }
        public static string ed_relations_results()
        {
            return ProteoformRelation.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, ed_relations.Values.ToList()[0].Select(r => r.as_tsv_row()));
        }
        public static string ee_relations_results()
        {
            return ProteoformRelation.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, ee_relations.Select(r => r.as_tsv_row()));
        }
        public static string ef_relations_results()
        {
            return ProteoformRelation.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, ef_relations.Select(r => r.as_tsv_row()));
        }
        public static string ee_peak_results()
        {
            return DeltaMassPeak.get_tsv_header() + Environment.NewLine + String.Join(Environment.NewLine, ee_peaks.Select(r => r.as_tsv_row()));
        }

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
                "Comparisons|ee_max_mass_difference\t" + ee_max_mass_difference.ToString(),
                "Comparisons|et_low_mass_difference\t" + et_low_mass_difference.ToString(),
                "Comparisons|et_high_mass_difference\t" + et_high_mass_difference.ToString(),
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
                case "LoadDeconvolutionResults|deconvolution_file_names":
                    //if (Lollipop.deconResultsFileNames.Count != 0)
                    //{
                    //    var response = MessageBox.Show("Would you like to use the files specified in LoadDeconvolution rather than those referenced in the method file?", "Multiple Deconvolution File References", MessageBoxButtons.YesNoCancel);
                    //    if (response == DialogResult.Yes) { break; }
                    //    if (response == DialogResult.No) { Lollipop.deconResultsFileNames.Clear(); }
                    //    if (response == DialogResult.Cancel) { return; }
                    //}
                    foreach (string filename in fields) { if (filename != "LoadDeconvolutionResults|deconvolution_file_names") Lollipop.deconResultsFileNames.Add(filename); }
                    break;
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
