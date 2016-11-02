﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel; // needed for bindinglist
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class Lollipop
    {
        public const double MONOISOTOPIC_UNIT_MASS = 1.0023; // updated 161007
        public const double NEUCODE_LYSINE_MASS_SHIFT = 0.036015372;

        //needed for functioning open results - user can update/rerun modules and program doesn't crash.
        public static bool opening_results = false; //set to true if previously saved tsv's are read into program
        public static bool updated_theoretical = false;
        public static bool updated_agg = false;
        public static bool opened_results_originally = false; //stays true if results ever opened
        public static bool opened_raw_comps = false;

        //RAW EXPERIMENTAL COMPONENTS
        public static List<InputFile> input_files = new List<InputFile>();
        public static IEnumerable<InputFile> identification_files() { return input_files.Where(f => f.purpose == Purpose.Identification); }
        public static IEnumerable<InputFile> quantification_files() { return input_files.Where(f => f.purpose == Purpose.Quantification); }
        public static IEnumerable<InputFile> calibration_files() { return input_files.Where(f => f.purpose == Purpose.Calibration); }
        public static IEnumerable<InputFile> bottomup_files() { return input_files.Where(f => f.purpose == Purpose.BottomUp); }
        public static IEnumerable<InputFile> topdown_files() { return input_files.Where(f => f.purpose == Purpose.TopDown); }
        public static IEnumerable<InputFile> topdownID_files() { return input_files.Where(f => f.purpose == Purpose.TopDownIDResults); }
        public static List<Correction> correctionFactors = null;
        public static List<int> MS1_scans = new List<int>();
        public static List<Component> raw_experimental_components = new List<Component>();
        public static List<Component> reduced_raw_exp_components = new List<Component>(); //for td data
        public static List<Component> raw_quantification_components = new List<Component>();
        public static bool neucode_labeled = true;
        public static bool td_results = false;

        public static void process_raw_components()
        {
            if (input_files.Any(f => f.purpose == Purpose.Calibration))
                correctionFactors = calibration_files().SelectMany(file => Correction.CorrectionFactorInterpolation(read_corrections(file))).ToList();
            object sync = new object();

            Parallel.ForEach(input_files.Where(f=>f.purpose==Purpose.Identification), file =>
            {
                ExcelReader componentReader = new ExcelReader();
                List<Component> someComponents = remove_monoisotopic_duplicates_from_same_scan(componentReader.read_components_from_xlsx(file, correctionFactors));
                lock (sync)
                {                   
                    raw_experimental_components.AddRange(someComponents);
                }
            });
            if (neucode_labeled)
            {               
                HashSet<string> input_files = new HashSet<string>(raw_experimental_components.Select(c => c.input_file.UniqueId.ToString()));
                Parallel.ForEach(input_files, inputFile => 
                {
                    HashSet<string> scan_ranges = new HashSet<string>();
                    lock (sync)
                    {
                        scan_ranges = new HashSet<string>(from comp in raw_experimental_components where comp.input_file.UniqueId.ToString() == inputFile select comp.scan_range);
                        foreach (string scan_range in scan_ranges)
                            find_neucode_pairs(raw_experimental_components.Where(c => c.input_file.UniqueId.ToString() == inputFile && c.scan_range == scan_range));
                    }                                  
                });
            }

            //if (td_results)
            //{
            //    raw_experimental_components.Clear();
            //    raw_experimental_components = reduced_raw_exp_components;
            //}

        }

        private static void delete_MS2_and_repeats(string filename)
        {
            int i = 1;
            List<Component> reduced_raw_exp_comps = new List<Component>();
            foreach (Component comp in raw_experimental_components.Where(f => (f.input_file.path + "\\" + f.input_file.filename + f.input_file.extension) == filename).ToList())
            {
                string[] scans = comp.scan_range.Split('-');
                if (scans[0].Equals(scans[1]) && MS1_scans.Contains(Convert.ToInt32(scans[0])))  //make sure same scan # in range (one scan) and that it's MS1 scan
                    {
                        //if it has the same monoisotopic mass and intensity sum as something else, it's probably a repeat - don't add. 
                        if (reduced_raw_exp_comps.Where(r => r.monoisotopic_mass == comp.monoisotopic_mass && r.intensity_sum == comp.intensity_sum).ToList().Count == 0)
                        {
                            comp.id = i.ToString();  //new id
                            reduced_raw_exp_comps.Add(comp);
                            i++;
                        }   
                    }
           }
            Lollipop.reduced_raw_exp_components.AddRange(reduced_raw_exp_comps);
        }


        public static void process_raw_quantification_components()
        {
            if (input_files.Any(f => f.purpose == Purpose.Quantification))
                correctionFactors = calibration_files().SelectMany(file => Correction.CorrectionFactorInterpolation(read_corrections(file))).ToList();
            object sync = new object();
            Parallel.ForEach(quantification_files(), file => 
            {
                ExcelReader componentReader = new ExcelReader();
                List<Component> someComponents = remove_monoisotopic_duplicates_from_same_scan(componentReader.read_components_from_xlsx(file, correctionFactors));
                lock (sync)
                {
                    raw_quantification_components.AddRange(someComponents);
                }
            });

        }

        private static List<Component> remove_monoisotopic_duplicates_from_same_scan(List<Component> raw_components)
        {
            IEnumerable<string> scans = raw_components.Select(c => c.scan_range).Distinct();
            List<Component> removeThese = new List<Component>();

            foreach (string scan in scans)
            {
                IEnumerable<Component> scanComps = raw_components.Where(c => c.scan_range == scan).OrderBy(w => w.weighted_monoisotopic_mass);
                foreach (Component sc in scanComps)
                {
                    IEnumerable<Component> mmc = scanComps.Where(cp => cp.weighted_monoisotopic_mass >= sc.weighted_monoisotopic_mass + (Lollipop.MONOISOTOPIC_UNIT_MASS - 0.0003) && cp.weighted_monoisotopic_mass <= sc.weighted_monoisotopic_mass + (Lollipop.MONOISOTOPIC_UNIT_MASS + 0.0003)); //missed monoisotopic that is one dalton larger
                    removeThese.AddRange(mmc);
                    foreach (Component c in mmc) sc.mergeTheseComponents(c);
                 }
            }

            return raw_components.Except(removeThese).ToList();
        }

        public static IEnumerable<Correction> read_corrections(InputFile file)
        {
            string filepath = file.path + "\\" + file.filename + file.extension;
            string filename = file.filename;

            string[] correction_lines = File.ReadAllLines(filepath);
            for (int i = 1; i < correction_lines.Length; i++)
            {
                string[] parts = correction_lines[i].Split('\t');
                if (parts.Length < 3) continue;
                int scan_number = Convert.ToInt32(parts[0]);
                double correction = Double.NaN;
                //two corrections can be available for each scan. The correction in column 3 is preferred
                //if column three is NaN, then column 2 is selected.
                //if column 2 is also NaN, then the correction for the scan will be interpolated from adjacent scans
                correction = Convert.ToDouble(parts[2]);
                if (Double.IsNaN(correction)) correction = Convert.ToDouble(parts[1]);
                yield return new Correction(filename, scan_number, correction);
            }
        }


        //NEUCODE PAIRS
        public static List<NeuCodePair> raw_neucode_pairs = new List<NeuCodePair>();
        public static decimal max_intensity_ratio = 6;
        public static decimal min_intensity_ratio = 1.4m;
        public static decimal max_lysine_ct = 26.2m;
        public static decimal min_lysine_ct = 1.5m;

        public static void find_neucode_pairs(IEnumerable<Component> components_in_file_scanrange)
        {
            object sync = new object();
            //Add putative neucode pairs. Must be in same spectrum, mass must be within 6 Da of each other
            List<Component> components = components_in_file_scanrange.OrderBy(c => c.corrected_mass).ToList();
            foreach (Component lower_component in components)
            {
                IEnumerable<Component> higher_mass_components = components.Where(higher_component => higher_component != lower_component && higher_component.corrected_mass > lower_component.corrected_mass);
                foreach (Component higher_component in higher_mass_components)
                {
                    double mass_difference = higher_component.corrected_mass - lower_component.corrected_mass;
                    if (mass_difference < 6)
                    {
                        List<int> lower_charges = lower_component.charge_states.Select(charge_state => charge_state.charge_count).ToList<int>();
                        List<int> higher_charges = higher_component.charge_states.Select(charge_states => charge_states.charge_count).ToList<int>();
                        List<int> overlapping_charge_states = lower_charges.Intersect(higher_charges).ToList();
                        double lower_intensity;
                        double higher_intensity;
                        if (opened_raw_comps)
                        {
                            lower_intensity = lower_component.intensity_sum_olcs;
                            higher_intensity = higher_component.intensity_sum_olcs;
                        }
                        else
                        {
                             lower_intensity = lower_component.calculate_sum_intensity_olcs(overlapping_charge_states);
                             higher_intensity = higher_component.calculate_sum_intensity_olcs(overlapping_charge_states);
                        }
                        bool light_is_lower = true; //calculation different depending on if neucode light is the heavier/lighter component
                        if (lower_intensity > 0 && higher_intensity > 0)
                        {
                            NeuCodePair pair;
                            if (lower_intensity > higher_intensity)
                                pair = new NeuCodePair(lower_component, higher_component, mass_difference, overlapping_charge_states, light_is_lower); //lower mass is neucode light
                            else
                                pair = new NeuCodePair(higher_component, lower_component, mass_difference, overlapping_charge_states, !light_is_lower); //higher mass is neucode light
                            if ((pair.corrected_mass <= (pair.neuCodeHeavy.corrected_mass + Lollipop.MONOISOTOPIC_UNIT_MASS)) // the heavy should be at higher mass. Max allowed is 1 dalton less than light.                                    
                                && !Lollipop.raw_neucode_pairs.Any(p => p.id_heavy == pair.id_light && p.neuCodeLight.intensity_sum > pair.neuCodeLight.intensity_sum)) // we found that any component previously used as a heavy, which has higher intensity is probably correct and that that component should not get reuused as a light.
                            {
                                lock (sync) Lollipop.raw_neucode_pairs.Add(pair);
                            }
                        }
                    }
                }
            }
        }


        //AGGREGATED PROTEOFORMS
        public static ProteoformCommunity proteoform_community = new ProteoformCommunity();
        public static decimal mass_tolerance = 3; //ppm
        public static decimal retention_time_tolerance = 3; //min
        public static decimal missed_monos = 3;
        public static decimal missed_lysines = 1;
        public static double min_rel_abundance = 0;
        public static int min_agg_count = 1;
        public static int min_num_CS = 1;

        public static void aggregate_proteoforms()
        {
            //Rooting each experimental proteoform is handled in addition of each NeuCode pair.
            //If no NeuCodePairs exist, e.g. for an experiment without labeling, the raw components are used instead.
            //Uses an ordered list, so that the proteoform with max intensity is always chosen first
            //Lollipop.raw_neucode_pairs = Lollipop.raw_neucode_pairs.Where(p => p != null).ToList();

            // Only aggregate acceptable components (and neucode pairs). Intensity sum from overlapping charge states includes all charge states if not a neucode pair.
            Component[] remaining_proteoforms = new Component[0];
            List<Component> remaining_quant_components = new List<Component>();

            if (Lollipop.neucode_labeled)
            {
                remaining_proteoforms = Lollipop.raw_neucode_pairs.OrderByDescending(p => p.intensity_sum_olcs).Where(p => p.accepted == true && p.relative_abundance >= Lollipop.min_rel_abundance && p.num_charge_states >= Lollipop.min_num_CS).ToArray();
                remaining_quant_components = Lollipop.raw_quantification_components; 
            }
            else
            {
                remaining_proteoforms = Lollipop.raw_experimental_components.OrderByDescending(p => p.intensity_sum).Where(p => p.accepted == true && p.relative_abundance >= Lollipop.min_rel_abundance && p.num_charge_states >= Lollipop.min_num_CS).ToArray();
                remaining_quant_components = Lollipop.raw_experimental_components; // there are no extra quantitative files for unlableled
            }

            int count = 1;
            List<ExperimentalProteoform> experimental_proteoforms = new List<ExperimentalProteoform>();
            while (remaining_proteoforms.Length > 0)
            {
                Component root = remaining_proteoforms[0];
                List<Component> tmp_remaining_proteoforms = remaining_proteoforms.ToList();
                List<Component> tmp_remaining_quant_proteoforms = remaining_quant_components;
                ExperimentalProteoform new_pf = new ExperimentalProteoform("E_" + count, root, tmp_remaining_proteoforms, tmp_remaining_quant_proteoforms, true);
                experimental_proteoforms.Add(new_pf);
                remaining_proteoforms = tmp_remaining_proteoforms.Except(new_pf.aggregated_components).ToArray();
                remaining_quant_components = tmp_remaining_quant_proteoforms.Except(new_pf.lt_quant_components).Except(new_pf.hv_quant_components).ToList();
                count += 1;
            }
            Lollipop.proteoform_community.experimental_proteoforms = experimental_proteoforms.ToArray();
        } 

        //Could be improved. Used for manual mass shifting.
        //Idea 1: Start with Components -- have them find the most intense nearby component. Then, go through and correct edge cases that aren't correct.
        //Idea 2: Use the assumption that proteoforms distant to the manual shift will not regroup.
        //Idea 2.1: Put the shifted proteoforms, plus some range from the min and max masses in there, and reaggregate the components with the aggregate_proteoforms algorithm.
        public static void regroup_components()
        {
            if (Lollipop.neucode_labeled)
            {
                Lollipop.raw_neucode_pairs.Clear();
                HashSet<string> scan_ranges = new HashSet<string>(Lollipop.raw_experimental_components.Select(c => c.scan_range));
                foreach (string scan_range in scan_ranges)
                    Lollipop.find_neucode_pairs(Lollipop.raw_experimental_components.Where(c => c.scan_range == scan_range));
            }
            Lollipop.aggregate_proteoforms();
        }


        //THEORETICAL DATABASE
        public static bool methionine_oxidation = false;
        public static bool carbamidomethylation = true;
        public static bool methionine_cleavage = true;
        public static bool natural_lysine_isotope_abundance = false;
        public static bool neucode_light_lysine = true;
        public static bool neucode_heavy_lysine = false;
        public static int max_ptms = 3;
        public static int decoy_databases = 0;
        public static string decoy_database_name_prefix = "DecoyDatabase_";
        public static int min_peptide_length = 7;
        public static double ptmset_mass_tolerance = 0.00001;
        public static bool combine_identical_sequences = true;
        public static bool combine_theoretical_proteoforms_byMass = true;
        public static string uniprot_xml_filepath = "";
        public static string ptmlist_filepath = "";
        public static string accessions_of_interest_list_filepath = "";
        public static string interest_type = "Of interest"; //label for proteins of interest. can be changed 
        public static Protein[] proteins;
        public static List<Psm> psm_list = new List<Psm>();

        public static ProteomeDatabaseReader proteomeDatabaseReader = new ProteomeDatabaseReader();
        public static Dictionary<string, Modification> uniprotModificationTable;
        static Dictionary<char, double> aaIsotopeMassList;

        public static void get_theoretical_proteoforms()
        {
            updated_theoretical = true;
            //Clear out data from potential previous runs
            Lollipop.proteoform_community.decoy_proteoforms = new Dictionary<string, TheoreticalProteoform[]>();
            Lollipop.psm_list.Clear();

            ProteomeDatabaseReader.oldPtmlistFilePath = ptmlist_filepath;
            uniprotModificationTable = proteomeDatabaseReader.ReadUniprotPtmlist();
            aaIsotopeMassList = new AminoAcidMasses(methionine_oxidation, carbamidomethylation).AA_Masses;

            //Read the UniProt-XML and ptmlist
            proteins = ProteomeDatabaseReader.ReadUniprotXml(uniprot_xml_filepath, uniprotModificationTable, min_peptide_length, methionine_cleavage);
            if (combine_identical_sequences) proteins = group_proteins_by_sequence(proteins);

            //Read the Morpheus BU data into PSM list
            foreach (InputFile file in Lollipop.bottomup_files())
            {
                List<Psm> psm_from_file = Lollipop.ReadBUFile(file.path + "\\" + file.filename + file.extension);
                psm_list.AddRange(psm_from_file);
            }

            //Read TD data into PSM list
            foreach (InputFile file in Lollipop.topdown_files())
            {
                List<Psm> psm_from_file = ExcelReader.ReadTDFile(file.path + "\\" + file.filename + file.extension, file.td_program);
                psm_list.AddRange(psm_from_file);
            }

            //PARALLEL PROBLEM
            process_entries();
            process_decoys();

            if (combine_theoretical_proteoforms_byMass)
            {
                Lollipop.proteoform_community.theoretical_proteoforms = group_proteoforms_byMass(Lollipop.proteoform_community.theoretical_proteoforms);
                Lollipop.proteoform_community.decoy_proteoforms = Lollipop.proteoform_community.decoy_proteoforms.ToDictionary(kv => kv.Key, kv => (TheoreticalProteoform[])group_proteoforms_byMass(kv.Value));
            }

            if (psm_list.Count > 0)
                match_psms_and_theoreticals();   //if BU data loaded in, match PSMs to theoretical accessions
            if (Lollipop.accessions_of_interest_list_filepath.Length > 0)
                mark_accessions_of_interest();
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
                            Convert.ToDouble(parts[10]), Convert.ToDouble(parts[6]), Convert.ToDouble(parts[25]), Convert.ToInt32(parts[1]),
                            parts[13].ToString(), Convert.ToDouble(parts[5]), Convert.ToInt32(parts[7]), Convert.ToDouble(parts[18]), PsmType.BottomUp);
                        psm_list.Add(new_psm);
                    }
                    i++;
                }
                else { qLessThan1 = false; }
            }
            return psm_list;
        }

        private static void match_psms_and_theoreticals()
        {
            Parallel.ForEach<TheoreticalProteoform>(Lollipop.proteoform_community.theoretical_proteoforms, tp =>
            {
                //PSMs in BU data with that protein accession
                string[] accession_to_search = tp.accession.Split('_');
                tp.psm_list = Lollipop.psm_list.Where(p => p.protein_description.Contains(accession_to_search[0])).ToList();
            });
        }

        private static void mark_accessions_of_interest()
        {
            string[] lines = File.ReadAllLines(Lollipop.accessions_of_interest_list_filepath);
            Parallel.ForEach<string>(lines, accession =>
            {
                List<TheoreticalProteoform> theoreticals = Lollipop.proteoform_community.theoretical_proteoforms.Where(p => p.accession.Contains(accession)).ToList();
                foreach(TheoreticalProteoform theoretical in theoreticals) { theoretical.of_interest = Lollipop.interest_type; }
            });
        }

        private static ProteinSequenceGroup[] group_proteins_by_sequence(IEnumerable<Protein> proteins)
        {
            List<ProteinSequenceGroup> protein_sequence_groups = new List<ProteinSequenceGroup>();
            HashSet<string> unique_sequences = new HashSet<string>(proteins.Select(p => p.sequence));
            foreach (string sequence in unique_sequences) protein_sequence_groups.Add(new ProteinSequenceGroup(proteins.Where(p => p.sequence == sequence).ToList()));
            return protein_sequence_groups.ToArray();
        }

        private static TheoreticalProteoformGroup[] group_proteoforms_byMass(IEnumerable<TheoreticalProteoform> theoreticals)
        {
            List<TheoreticalProteoformGroup> theoretical_proteoform_groups = new List<TheoreticalProteoformGroup>();
            HashSet<double> unique_modified_masses = new HashSet<double>(theoreticals.Select(p => p.modified_mass));
            foreach (double modified_mass in unique_modified_masses) theoretical_proteoform_groups.Add(new TheoreticalProteoformGroup(theoreticals.Where(p => p.modified_mass == modified_mass).ToList()));
            return theoretical_proteoform_groups.ToArray();
        }

        private static void process_entries()
        {
            List<TheoreticalProteoform> theoretical_proteoforms = new List<TheoreticalProteoform>();
            foreach (Protein p in proteins)
            {
                bool isMetCleaved = (methionine_cleavage && p.begin == 0 && p.sequence.Substring(0, 1) == "M");
                int startPosAfterCleavage = Convert.ToInt32(isMetCleaved);
                string seq = p.sequence.Substring(startPosAfterCleavage, (p.sequence.Length - startPosAfterCleavage));
                EnterTheoreticalProteformFamily(seq, p, p.accession, isMetCleaved, theoretical_proteoforms, -100);
            }
            Lollipop.proteoform_community.theoretical_proteoforms = theoretical_proteoforms.ToArray();
        }

        private static void process_decoys()
        {
            for (int decoyNumber = 0; decoyNumber < Lollipop.decoy_databases; decoyNumber++)
            {
                List<TheoreticalProteoform> decoy_proteoforms = new List<TheoreticalProteoform>();
                string giantProtein = GetOneGiantProtein(proteins, methionine_cleavage); //Concatenate a giant protein out of all protein read from the UniProt-XML, and construct target and decoy proteoform databases
                string decoy_database_name = decoy_database_name_prefix + decoyNumber;
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

                    EnterTheoreticalProteformFamily(hunk, p, p.accession + "_DECOY_" + decoyNumber, isMetCleaved, decoy_proteoforms, decoyNumber);
                }
                Lollipop.proteoform_community.decoy_proteoforms.Add(decoy_database_name, decoy_proteoforms.ToArray());
            }
        }

        private static void EnterTheoreticalProteformFamily(string seq, Protein prot, string accession, bool isMetCleaved, List<TheoreticalProteoform> theoretical_proteoforms, int decoy_number)
        {
            //Calculate the properties of this sequence
            double unmodified_mass = TheoreticalProteoform.CalculateProteoformMass(seq, aaIsotopeMassList);
            int lysine_count = seq.Split('K').Length - 1;
            List<PtmSet> unique_ptm_groups = new List<PtmSet>();
            unique_ptm_groups.AddRange(new PtmCombos(prot.ptms_by_position).get_combinations(max_ptms));

            int listMemberNumber = 1;

            foreach (PtmSet ptm_set in unique_ptm_groups)
            {
                double proteoform_mass = unmodified_mass + ptm_set.mass;
                string protein_description = prot.description + "_" + listMemberNumber.ToString();

                if (decoy_number < 0 )
                    theoretical_proteoforms.Add(new TheoreticalProteoform(accession, protein_description, prot.name, prot.fragment, prot.begin + Convert.ToInt32(isMetCleaved), prot.end, 
                        unmodified_mass, lysine_count, prot.goTerms, ptm_set, proteoform_mass, true));
                else
                    theoretical_proteoforms.Add(new TheoreticalProteoform(accession, protein_description + "_DECOY" + "_" + decoy_number.ToString(), prot.name, prot.fragment, prot.begin + Convert.ToInt32(isMetCleaved), prot.end, 
                        unmodified_mass, lysine_count, prot.goTerms , ptm_set, proteoform_mass, false));
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
        public static double ee_max_RetentionTime_difference = 2.5;
        public static double et_low_mass_difference = -250;
        public static double et_high_mass_difference = 250;
        public static double no_mans_land_lowerBound = 0.22;
        public static double no_mans_land_upperBound = 0.88;
        public static double peak_width_base_ee = 0.015;
        public static double peak_width_base_et = 0.015; //need to be separate so you can change one and not other. 
        public static double min_peak_count_ee = 10;
        public static double min_peak_count_et = 10;
        public static int relation_group_centering_iterations = 2;  // is this just arbitrary? whys is it specified here?
        public static List<ProteoformRelation> et_relations = new List<ProteoformRelation>();
        public static List<ProteoformRelation> ee_relations = new List<ProteoformRelation>();
        public static Dictionary<string, List<ProteoformRelation>> ed_relations = new Dictionary<string, List<ProteoformRelation>>();
        public static List<ProteoformRelation> ef_relations = new List<ProteoformRelation>();
        public static List<DeltaMassPeak> et_peaks = new List<DeltaMassPeak>();
        public static List<DeltaMassPeak> ee_peaks = new List<DeltaMassPeak>();

        public static void make_et_relationships()
        {
            Lollipop.et_relations = Lollipop.proteoform_community.relate_et(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToList().ToArray(), Lollipop.proteoform_community.theoretical_proteoforms.ToArray(), ProteoformComparison.et);
            Lollipop.ed_relations = Lollipop.proteoform_community.relate_ed();
            Lollipop.et_peaks = Lollipop.proteoform_community.accept_deltaMass_peaks(Lollipop.et_relations, Lollipop.ed_relations);
        }

        public static void make_ee_relationships()
        {
            Lollipop.ee_relations = Lollipop.proteoform_community.relate_ee(Lollipop.proteoform_community.experimental_proteoforms.ToArray(), Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToList().ToArray(), ProteoformComparison.ee);
            Lollipop.ef_relations = Lollipop.proteoform_community.relate_unequal_ee_lysine_counts();
            Lollipop.ee_peaks = Lollipop.proteoform_community.accept_deltaMass_peaks(Lollipop.ee_relations, Lollipop.ef_relations);
        }

        //PROTEOFORM FAMILIES -- see ProteoformCommunity
        public static string family_build_folder_path = "";
        public static int deltaM_edge_display_rounding = 2;

        //METHOD FILE
        public static string method_toString()
        {
            string method = String.Join(System.Environment.NewLine, new string[] {            
                "NeuCodePairs|max_intensity_ratio\t" + max_intensity_ratio.ToString(),
                "NeuCodePairs|min_intensity_ratio\t" + min_intensity_ratio.ToString(),
                "NeuCodePairs|max_lysine_ct\t" + max_lysine_ct.ToString(),
                "NeuCodePairs|min_lysine_ct\t" + min_lysine_ct.ToString(),
                "AggregatedProteoforms|mass_tolerance\t" + mass_tolerance.ToString(),
                "AggregatedProteoforms|retention_time_tolerance\t" + retention_time_tolerance.ToString(),
                "AggregatedProteoforms|missed_monos\t" + missed_monos.ToString(),
                "AggregatedProteoforms|missed_lysines\t" + missed_lysines.ToString(),
                "AggregatedProteoforms|min_rel_abundance\t" + min_rel_abundance.ToString(),
                "AggregatedProteoforms|min_num_CS\t" + min_num_CS.ToString(),
                "AggregatedProteoforms|min_agg_count\t" + min_agg_count.ToString(),
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
                "Comparisons|peak_width_base_ee\t" + peak_width_base_ee.ToString(),
                "Comparisons|peak_width_base_et\t" + peak_width_base_et.ToString(),
                "Comparisons|min_peak_count_ee\t" + min_peak_count_ee.ToString(),
                "Comparisons|min_peak_count_et\t" + min_peak_count_et.ToString(),
                "Comparisons|ee_max_RetentionTime_difference\t" + ee_max_RetentionTime_difference.ToString(),
                "Families|family_build_folder_path\t" + family_build_folder_path, 
                "Families|deltaM_edge_display_rounding\t" + deltaM_edge_display_rounding.ToString()    
            });
            return method; 
        }

        public static bool use_method_files = true;
        public static void load_setting(string setting_spec)
        {
            string[] fields = setting_spec.Split('\t');
            switch (fields[0])
            {
                case "NeuCodePairs|max_intensity_ratio": max_intensity_ratio = Convert.ToDecimal(fields[1]); break;
                case "NeuCodePairs|min_intensity_ratio": min_intensity_ratio = Convert.ToDecimal(fields[1]); break;
                case "NeuCodePairs|max_lysine_ct": max_lysine_ct = Convert.ToDecimal(fields[1]); break;
                case "NeuCodePairs|min_lysine_ct": min_lysine_ct = Convert.ToDecimal(fields[1]); break;
                case "AggregatedProteoforms|mass_tolerance": mass_tolerance = Convert.ToDecimal(fields[1]); break;
                case "AggregatedProteoforms|retention_time_tolerance": retention_time_tolerance = Convert.ToDecimal(fields[1]); break;
                case "AggregatedProteoforms|missed_monos": missed_monos = Convert.ToDecimal(fields[1]); break;
                case "AggregatedProteoforms|missed_lysines": missed_lysines = Convert.ToDecimal(fields[1]); break;
                case "AggregatedProteoforms|min_rel_abundance": min_rel_abundance = Convert.ToDouble(fields[1]); break;
                case "AggregatedProteoforms|min_num_CS": min_num_CS = Convert.ToInt16(fields[1]); break;
                case "AggregatedProteoforms|min_agg_count": min_agg_count = Convert.ToInt16(fields[1]); break;
                case "TheoreticalDatabase|uniprot_xml_filepath": if (fields.Length > 1) uniprot_xml_filepath = fields[1]; break; //make sure path listed
                case "TheoreticalDatabase|ptmlist_filepath": if (fields.Length > 1) ptmlist_filepath = fields[1]; break;
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
                case "Comparisons|peak_width_base_ee": peak_width_base_ee = Convert.ToDouble(fields[1]); break;
                case "Comparisons|peak_width_base_et": peak_width_base_et = Convert.ToDouble(fields[1]); break;
                case "Comparisons|min_peak_count_ee": min_peak_count_ee = Convert.ToDouble(fields[1]); break;
                case "Comparisons|min_peak_count_et": min_peak_count_et = Convert.ToDouble(fields[1]); break;
                case "Comparisons|ee_max_RetentionTime_difference": ee_max_RetentionTime_difference = Convert.ToDouble(fields[1]); break;
                case "Comparisons|ee_max_mass_difference": ee_max_mass_difference = Convert.ToDouble(fields[1]); break;
                case "Comprisons|et_high_mass_diffrence": et_high_mass_difference = Convert.ToDouble(fields[1]); break;
                case "Comparisons|et_low_mass_difference": et_low_mass_difference = Convert.ToDouble(fields[1]); break;
                case "Families|family_build_folder_path": if (fields.Length > 1) family_build_folder_path = fields[1]; break; //still works if no path
                case "Families|deltaM_edge_display_rounding": deltaM_edge_display_rounding = Convert.ToInt32(fields[1]); break;
            }
        }
    }
}
