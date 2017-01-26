using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel; // needed for bindinglist
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MathNet.Numerics;
using Chemistry;

namespace ProteoformSuiteInternal
{
    public class Lollipop
    {
        public const double MONOISOTOPIC_UNIT_MASS = 1.0023; // updated 161007
        public const double NEUCODE_LYSINE_MASS_SHIFT = 0.036015372;
        public const double PROTON_MASS = 1.007276474;


        //needed for functioning open results - user can update/rerun modules and program doesn't crash.
        public static bool opening_results = false; //set to true if previously saved tsv's are read into program
        public static bool updated_theoretical = false;
        public static bool updated_agg = false;
        public static bool opened_results_originally = false; //stays true if results ever opened
        public static bool opened_raw_comps = false;

        //RAW EXPERIMENTAL COMPONENTS
        public static List<InputFile> input_files = new List<InputFile>();
        public static List<Correction> correctionFactors = null;
        public static List<Component> raw_experimental_components = new List<Component>();
        public static List<Component> raw_quantification_components = new List<Component>();
        public static List<TopDownHit> top_down_hits = new List<TopDownHit>();
        public static bool neucode_labeled = true;
        public static bool td_results = false;
        public static bool td_famlies = false;
        public static bool calibrate_td_results = false;

        //input file auxillary methods
        public static IEnumerable<InputFile> identification_files() { return input_files.Where(f => f.purpose == Purpose.Identification); }
        public static IEnumerable<InputFile> quantification_files() { return input_files.Where(f => f.purpose == Purpose.Quantification); }
        public static IEnumerable<InputFile> calibration_files() { return input_files.Where(f => f.purpose == Purpose.Calibration); }
        public static IEnumerable<InputFile> bottomup_files() { return input_files.Where(f => f.purpose == Purpose.BottomUp); }
        public static IEnumerable<InputFile> topdown_files() { return input_files.Where(f => f.purpose == Purpose.TopDown); }
        public static IEnumerable<InputFile> raw_files() { return input_files.Where(f => f.purpose == Purpose.RawFile); }

        public static void process_raw_components()
        {

            if (input_files.Any(f => f.purpose == Purpose.Calibration))
                correctionFactors = calibration_files().SelectMany(file => Correction.CorrectionFactorInterpolation(read_corrections(file))).ToList();
            object sync = new object();
            // Parallel.ForEach(input_files.Where(f => f.purpose == Purpose.Identification).ToList(), file =>
            // using (var writer = new StreamWriter("C:\\Users\\LeahSchaffer\\Desktop\\A17ABC_correction_attributes_mass.tsv"))
            // {
            // writer.WriteLine("filename\tmz_centroid\tretention_time\tintensity\tTIC\tinjection_time\tmz_correction\tmass_correction");
            foreach (InputFile file in input_files.Where(f => f.purpose == Purpose.Identification).ToList())
            {
                ComponentReader componentReader = new ComponentReader();
                List<Component> someComponents = componentReader.read_components_from_xlsx(file, correctionFactors, Lollipop.Ms_scans.Where(s => s.filename == file.filename && s.ms_order == 1).ToList().Select(s => s.scan_number).ToList());
                {
                    if (Lollipop.calibrate_td_results && td_calibration_functions.ContainsKey(file.filename))
                    {
                        Func<double[], double> bestCf = td_calibration_functions[file.filename];
                        foreach (Component comp in someComponents)
                        {
                            double rt = Convert.ToDouble(comp.rt_range.Split('-')[0]);
                            MsScan scan = Ms_scans.Where(s => s.filename == file.filename && s.scan_number == Convert.ToInt16(comp.scan_range.Split('-')[0])).First();
                            TdMzCal.get_signal_to_noise(comp);

                            //Func<ChargeState, double> theFUnc = x => x.mz_centroid - bestCf.Predict(new double[6] { 1, x.mz_centroid, rt, x.intensity, scan.TIC, scan.injection_time });
                            foreach (ChargeState cs in comp.charge_states)
                            {
                                double correction = -1 * bestCf(new double[] { cs.mz_centroid, rt });
                                cs.calculated_mass = cs.correct_calculated_mass(correction);
                                //writer.WriteLine(file.filename + "\t" + cs.mz_centroid + "\t" + rt + "\t" + cs.intensity + "\t" + scan.TIC + "\t" + scan.injection_time + "\t" + correction + "\t" + correction * cs.charge_count);
                            }
                            // double correction = td_corrections[comp.input_file.filename](Convert.ToDouble(comp.rt_range.Split('-')[0]));
                            comp.topdown_correction = comp.weighted_monoisotopic_mass - comp.uncalibrated_monoisotopic_mass; //helpful to see topdown correction
                        }
                    }
                    //don't add components if calibrating td results and no calibration function for that file 
                    if (!Lollipop.calibrate_td_results || (Lollipop.calibrate_td_results && td_calibration_functions.ContainsKey(file.filename)))
                        lock (sync)
                        {
                            raw_experimental_components.AddRange(someComponents);
                        }
                    //);
                    // }
                    if (neucode_labeled)
                    {
                        process_neucode_components();
                    }
                }
            }
        }

            private static void process_neucode_components()
        {
            object sync = new object();
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

        public static void process_raw_quantification_components()
        {
            if (input_files.Any(f => f.purpose == Purpose.Quantification))
                correctionFactors = calibration_files().SelectMany(file => Correction.CorrectionFactorInterpolation(read_corrections(file))).ToList();
            object sync = new object();
            Parallel.ForEach(quantification_files(), file =>
            {
                ComponentReader componentReader = new ComponentReader();
                List<Component> someComponents = componentReader.read_components_from_xlsx(file, correctionFactors, new List<int>());
                lock (sync)
                {
                    raw_quantification_components.AddRange(someComponents);
                }
            });
        }

        public static IEnumerable<Correction> read_corrections(InputFile file)
        {
            string filepath = file.path + "\\" + file.filename + file.extension;
            string filename = file.filename;

            string[] correction_lines = File.ReadAllLines(filepath);
            for (int i = 1; i < correction_lines.Length; i++)
            {
                string[] parts = correction_lines[i].Split('\t');
                if (parts.Length < 2) continue;
                int scan_number = Convert.ToInt32(parts[0]);
                double correction = Double.NaN;
                correction = Convert.ToDouble(parts[1]);
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
            List<Component> components = components_in_file_scanrange.OrderBy(c => c.weighted_monoisotopic_mass).ToList();
            foreach (Component lower_component in components)
            {
                IEnumerable<Component> higher_mass_components = components.Where(higher_component => higher_component != lower_component && higher_component.weighted_monoisotopic_mass > lower_component.weighted_monoisotopic_mass);
                foreach (Component higher_component in higher_mass_components)
                {
                    double mass_difference = higher_component.weighted_monoisotopic_mass - lower_component.weighted_monoisotopic_mass;
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
                            if ((pair.weighted_monoisotopic_mass <= (pair.neuCodeHeavy.weighted_monoisotopic_mass + Lollipop.MONOISOTOPIC_UNIT_MASS)) // the heavy should be at higher mass. Max allowed is 1 dalton less than light.                                    
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
        public static decimal missed_lysines = 2;
        public static double min_rel_abundance = 0;
        public static int min_agg_count = 1;
        public static int min_num_CS = 1;
        public static double RT_tol_NC = 10;
        public static int min_num_bioreps;

        public static void aggregate_proteoforms()
        {
            List<ExperimentalProteoform> candidateExperimentalProteoforms = createProteoforms();
            List<ExperimentalProteoform> vettedExperimentalProteoforms = vetExperimentalProteoforms(candidateExperimentalProteoforms);
            if (Lollipop.neucode_labeled)
                Lollipop.proteoform_community.experimental_proteoforms = assignQuantificationComponents(vettedExperimentalProteoforms).ToArray();
            else
                Lollipop.proteoform_community.experimental_proteoforms = vettedExperimentalProteoforms.ToArray();
           // using (var writer = new StreamWriter("C:\\Users\\LeahSchaffer\\Desktop\\file_number_components_experimentals.tsv"))
            {
             //   writer.WriteLine("filename\\components\\agg_3cs");
                foreach(InputFile file in identification_files())
                {
                    int comps = Lollipop.raw_experimental_components.Where(c => c.input_file == file).ToList().Count;
                    int exp = Lollipop.proteoform_community.experimental_proteoforms.Where(e => e.aggregated_components.Where(c => c.input_file == file).ToList().Count > 0).ToList().Count;
                  //  writer.WriteLine(file.filename + "\t" + comps + "\t" + exp);
                }
            }
        }

        public static List<ExperimentalProteoform> createProteoforms()
        {
            //Rooting each experimental proteoform is handled in addition of each NeuCode pair.
            //If no NeuCodePairs exist, e.g. for an experiment without labeling, the raw components are used instead.
            //Uses an ordered list, so that the proteoform with max intensity is always chosen first
            //Lollipop.raw_neucode_pairs = Lollipop.raw_neucode_pairs.Where(p => p != null).ToList();

            List<ExperimentalProteoform> candidateExperimentalProteoforms = new List<ExperimentalProteoform>();

            // Only aggregate acceptable components (and neucode pairs). Intensity sum from overlapping charge states includes all charge states if not a neucode pair.
            Component[] remaining_proteoforms = new Component[0];

            if (Lollipop.neucode_labeled)
                remaining_proteoforms = Lollipop.raw_neucode_pairs.OrderByDescending(p => p.intensity_sum_olcs).Where(p => p.accepted == true && p.relative_abundance >= Lollipop.min_rel_abundance && p.num_charge_states >= Lollipop.min_num_CS).ToArray();
            else
                remaining_proteoforms = Lollipop.raw_experimental_components.OrderByDescending(p => p.intensity_sum).Where(p => p.accepted == true && p.relative_abundance >= Lollipop.min_rel_abundance && p.num_charge_states >= Lollipop.min_num_CS).ToArray();
            int count = 1;
            while (remaining_proteoforms.Length > 0)
            {
                Component root = remaining_proteoforms[0];
                List<Component> tmp_remaining_proteoforms = remaining_proteoforms.ToList();
                ExperimentalProteoform temp_pf = new ExperimentalProteoform("E_" + count, root, tmp_remaining_proteoforms, true); //first pass returns temporary proteoform
                ExperimentalProteoform new_pf = new ExperimentalProteoform("E_" + count, temp_pf, tmp_remaining_proteoforms, true); //second pass uses temporary protoeform from first pass.
                candidateExperimentalProteoforms.Add(new_pf);
                remaining_proteoforms = tmp_remaining_proteoforms.Except(new_pf.aggregated_components).ToArray();
                count += 1;
            }
            return candidateExperimentalProteoforms;
        }

        public static List<ExperimentalProteoform> vetExperimentalProteoforms(List<ExperimentalProteoform> candidateExperimentalProteoforms) // eliminating candidate proteoforms that were mistakenly created
        {
            List<ExperimentalProteoform> vettedExperimentalProteoforms = new List<ExperimentalProteoform>();
            List<Component> usedQuantitativeComponents = new List<Component>();
            foreach (ExperimentalProteoform ep in candidateExperimentalProteoforms.OrderByDescending(p=>p.agg_intensity))
            {
                ExperimentalProteoform temp = ep;

                temp.lt_quant_components.AddRange(Lollipop.raw_experimental_components.Except(usedQuantitativeComponents).Where(r => temp.includes(r, temp, true)));
                temp.light_observation_count = temp.lt_quant_components.Count;
                if (Lollipop.neucode_labeled)
                {
                    temp.hv_quant_components.AddRange(Lollipop.raw_experimental_components.Except(usedQuantitativeComponents).Where(r => temp.includes(r, temp, false)));
                    temp.heavy_observation_count = temp.hv_quant_components.Count;
                }

                if (Lollipop.neucode_labeled)
                {
                    if (temp.light_observation_count > 0 && temp.heavy_observation_count > 0)
                    {
                        vettedExperimentalProteoforms.Add(ep);
                        usedQuantitativeComponents.AddRange(temp.lt_quant_components);
                        usedQuantitativeComponents.AddRange(temp.hv_quant_components);
                    }                        
                }
                else
                {
                    if (temp.light_observation_count > 0) // going ahead and assinging quantitative components here for unlabelled becuase we already have them in temp
                    {
                        ep.light_observation_count = temp.light_observation_count;
                        ep.lt_quant_components = temp.lt_quant_components;
                        vettedExperimentalProteoforms.Add(ep);
                        usedQuantitativeComponents.AddRange(temp.lt_quant_components);
                    }
                }
            }
            return vettedExperimentalProteoforms;
        }

        public static List<ExperimentalProteoform> assignQuantificationComponents(List<ExperimentalProteoform> vettedExperimentalProteoforms)  // this is only need for neucode labeled data. quantitative components for unlabelled are assigned elsewhere "vetExperimentalProteoforms"
        {
            List<Component> usedQuantitativeComponents = new List<Component>();
            foreach (ExperimentalProteoform ep in vettedExperimentalProteoforms.OrderByDescending(p => p.agg_intensity))
            {
                ep.lt_quant_components.AddRange(Lollipop.raw_quantification_components.Except(usedQuantitativeComponents).Where(r => ep.includes(r, ep, true)));
                ep.light_observation_count = ep.lt_quant_components.Count;
                ep.hv_quant_components.AddRange(Lollipop.raw_quantification_components.Except(usedQuantitativeComponents).Where(r => ep.includes(r, ep, false)));
                ep.heavy_observation_count = ep.hv_quant_components.Count;
            }
            return vettedExperimentalProteoforms;
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
                process_neucode_components();
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
        public static bool use_gene_ID = false;
        public static ProteomeDatabaseReader proteomeDatabaseReader = new ProteomeDatabaseReader();
        public static Dictionary<string, Modification> uniprotModificationTable;
        static Dictionary<char, double> aaIsotopeMassList;

        public static void get_theoretical_proteoforms()
        {
            updated_theoretical = true;
            //Clear out data from potential previous runs
            Lollipop.proteoform_community.decoy_proteoforms = new Dictionary<string, TheoreticalProteoform[]>();
            Lollipop.psm_list.Clear();

            uniprotModificationTable.Clear();
            ProteomeDatabaseReader.oldPtmlistFilePath = ptmlist_filepath;
            uniprotModificationTable = proteomeDatabaseReader.ReadUniprotPtmlist();
            aaIsotopeMassList = new AminoAcidMasses(methionine_oxidation, carbamidomethylation).AA_Masses;

            //Read the UniProt-XML and ptmlist
            proteins = ProteomeDatabaseReader.ReadUniprotXml(uniprot_xml_filepath, uniprotModificationTable, min_peptide_length, methionine_cleavage);
            if (combine_identical_sequences) proteins = group_proteins_by_sequence(proteins);

            //Read the Morpheus BU data into PSM list
            foreach (InputFile file in Lollipop.bottomup_files())
            {
                List<Psm> psm_from_file = TdBuReader.ReadBUFile(file.path + "\\" + file.filename + file.extension);
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

        private static void match_psms_and_theoreticals()
        {
            Parallel.ForEach<TheoreticalProteoform>(Lollipop.proteoform_community.theoretical_proteoforms, tp =>
            {
                //PSMs in BU data with that protein accession
                string[] accession_to_search = tp.accession.Split('_');
                tp.psm_list = Lollipop.psm_list.Where(p => p.protein_description.Contains(accession_to_search[0])).ToList();
            });
            for (int i = 0; i < decoy_databases; i++)
            {
                Parallel.ForEach<TheoreticalProteoform>(Lollipop.proteoform_community.decoy_proteoforms["DecoyDatabase_" + i], dp =>
                 {
                     string[] accession_to_search = dp.accession.Split('_');
                     dp.psm_list = Lollipop.psm_list.Where(p => p.protein_description.Contains(accession_to_search[0])).ToList();
 
                 });
            }
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
                        unmodified_mass, lysine_count, prot.goTerms, ptm_set, proteoform_mass, prot.gene_id, true));
                else
                    theoretical_proteoforms.Add(new TheoreticalProteoform(accession, protein_description + "_DECOY" + "_" + decoy_number.ToString(), prot.name, prot.fragment, prot.begin + Convert.ToInt32(isMetCleaved), prot.end, 
                        unmodified_mass, lysine_count, prot.goTerms , ptm_set, proteoform_mass, prot.gene_id, false));
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
        public static bool limit_TD_BU_theoreticals = false;
        public static List<ProteoformRelation> et_relations = new List<ProteoformRelation>();
        public static List<ProteoformRelation> ee_relations = new List<ProteoformRelation>();
        public static Dictionary<string, List<ProteoformRelation>> ed_relations = new Dictionary<string, List<ProteoformRelation>>();
        public static List<ProteoformRelation> ef_relations = new List<ProteoformRelation>();
        public static List<DeltaMassPeak> et_peaks = new List<DeltaMassPeak>();
        public static List<DeltaMassPeak> ee_peaks = new List<DeltaMassPeak>();
        public static List<ProteoformRelation> td_relations = new List<ProteoformRelation>(); //td data

        //for reading in neucode data for labelfree analysis
        public static List<ProteoformRelation> neucode_et_pairs = new List<ProteoformRelation>();
        public static List<ProteoformRelation> neucode_ee_pairs = new List<ProteoformRelation>();
        public static bool limit_NC_ee_pairs = false;
        public static bool limit_NC_et_pairs = false;
        public static double NC_et_mass_tol = 0.03;
        public static double NC_ee_mass_tol = 0.03;

        public static void make_et_relationships()
        {
            if (!limit_TD_BU_theoreticals) Lollipop.et_relations = Lollipop.proteoform_community.relate_et(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToList().ToArray(), Lollipop.proteoform_community.theoretical_proteoforms.ToArray(), ProteoformComparison.et);
            else Lollipop.et_relations = Lollipop.proteoform_community.relate_et(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToList().ToArray(), Lollipop.proteoform_community.theoretical_proteoforms.Where(t => t.psm_count_BU > 0 || t.TD_proteoforms.Count > 0 ).ToArray(), ProteoformComparison.et);
            Lollipop.ed_relations = Lollipop.proteoform_community.relate_ed();
            Lollipop.et_peaks = Lollipop.proteoform_community.accept_deltaMass_peaks(Lollipop.et_relations, Lollipop.ed_relations);
        }

        public static void read_neucode_et_relationships(string [] relationships)
        {
            neucode_et_pairs.Clear();
            for (int i = 1; i < relationships.Length; i++)
            {
                string[] e = relationships[i].Split('\t');
                double labeled_e_mass = Convert.ToDouble(e[0]);
                double rt = Convert.ToDouble(e[1]);
                double labeled_t_mass = Convert.ToDouble(e[2]);
                string accession = e[3];
                string mods = e[4];
                int lysine_count = Convert.ToInt16(e[5]);
                double delta_mass = Convert.ToDouble(e[6]);
                double e_mass = labeled_e_mass - (lysine_count * 136.109162) + (lysine_count * 128.094963);
                double t_mass = labeled_t_mass - (lysine_count * 136.109162) + (lysine_count * 128.094963);

                TheoreticalProteoform theo = new TheoreticalProteoform(accession, t_mass, lysine_count, mods);
                ExperimentalProteoform exp = new ExperimentalProteoform("E_" + i, e_mass, lysine_count, rt);
                ProteoformRelation pr = new ProteoformRelation(exp, theo, ProteoformComparison.et, delta_mass);
                neucode_et_pairs.Add(pr);
            }
        }


        public static void read_neucode_ee_relationships(string[] relationships)
        {
            neucode_ee_pairs.Clear();
            for (int i = 1; i < relationships.Length; i++)
            {
                string[] e = relationships[i].Split('\t');
                double labeled_e1_mass = Convert.ToDouble(e[0]);
                double e1_rt = Convert.ToDouble(e[1]);
                double labeled_e2_mass = Convert.ToDouble(e[2]);
                double e2_rt = Convert.ToDouble(e[3]);
                int lysine_count = Convert.ToInt16(e[4]);
                double delta_mass = Convert.ToDouble(e[5]);
                double e1_mass = labeled_e1_mass - (lysine_count * 136.109162) + (lysine_count * 128.094963);
                double e2_mass = labeled_e2_mass - (lysine_count * 136.109162) + (lysine_count * 128.094963);

                ExperimentalProteoform e1 = new ExperimentalProteoform("E_" + i, e1_mass, lysine_count, e1_rt);
                ExperimentalProteoform e2 = new ExperimentalProteoform("E_" + i, e2_mass, lysine_count, e2_rt);
                ProteoformRelation pr = new ProteoformRelation(e1, e2, ProteoformComparison.ee, delta_mass);
                neucode_ee_pairs.Add(pr);
            }
        }

        public static void make_ee_relationships()
        {
            Lollipop.ee_relations = Lollipop.proteoform_community.relate_ee(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToArray(), Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToArray(), ProteoformComparison.ee);
            Lollipop.ef_relations = Lollipop.proteoform_community.relate_unequal_ee_lysine_counts();
            Lollipop.ee_peaks = Lollipop.proteoform_community.accept_deltaMass_peaks(Lollipop.ee_relations, Lollipop.ef_relations);
        }

        //TOPDOWN DATA
        public static List<MsScan> Ms_scans = new List<MsScan>();
        public static Dictionary<string, Func<double[], double>> td_calibration_functions = new Dictionary<string, Func<double[], double>> ();
        public static int hits = 0;
        public static int tight_mass_hits = 0;
        public static int training_points = 0;
        public static List<TopDownHit> hits_used = new List<TopDownHit>();

        public static void process_td_results()
        {
            foreach (InputFile file in Lollipop.topdown_files())
            {
                List<TopDownHit> td_file_hits = TdBuReader.ReadTDFile(file.path + "\\" + file.filename + file.extension, file.td_software);
                top_down_hits.AddRange(td_file_hits);
            }
          //  using (var writer = new StreamWriter("C:\\Users\\LeahSchaffer\\Desktop\\hit_scan_attributes.tsv"))
            {
              //  writer.WriteLine("filename\thits\tabs_mass_hits\ttraining_points\treported_mass\ttheoretical_mass\tcorrected_mass\tretention_time\tmz_centroid\tinjection_time\tTIC\tintensity\tmass_error");
                //get MS1 scan numbers and corrections (if calibrate td results)
                foreach (string filename in top_down_hits.Select(h => h.filename).Distinct())
                {
                    List<InputFile> raw_files = Lollipop.raw_files().Where(f => f.filename == filename).ToList();
                    if (raw_files.Count > 0)
                    {
                        InputFile raw_file = raw_files.First();
                      //  first step opens raw file and gets MS1 scan numbers... if calibrating, goes on to calibrate. 
                        Func<double[], double> bestCf = TdMzCal.Run_TdMzCal(filename, raw_file.path + "\\" + raw_file.filename + raw_file.extension, top_down_hits.Where(h => h.filename == filename).ToList());
                        if (bestCf != null && Lollipop.calibrate_td_results)
                        {
                            td_calibration_functions.Add(filename, bestCf);
                            //need to calibrate all the others
                            foreach (TopDownHit hit in Lollipop.top_down_hits.Where(h =>  h.filename == filename).ToList())
                            {
                                hit.corrected_mass = (hit.mz - bestCf(new double[] { hit.mz, hit.retention_time })).ToMass(hit.charge);

                                if (hit.result_set == Result_Set.tight_absolute_mass && hits_used.Contains(hit))
                                {
                                    MsScan ms1_scan = Lollipop.Ms_scans.Where(s => s.ms_order == 1 && s.scan_number < hit.scan).ToList().OrderBy(m => hit.scan - m.scan_number).ToList().First();
                                    //writer.WriteLine(hit.filename +  "\t" + hits+ "\t" + tight_mass_hits + "\t" + training_points +  "\t" + hit.reported_mass + "\t" + hit.theoretical_mass + "\t" + hit.corrected_mass + "\t" +  hit.retention_time + "\t" + hit.mz + "\t" + ms1_scan.injection_time + "\t" + ms1_scan.TIC + "\t" + hit.intensity);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void aggregate_td_hits()
        {
            Lollipop.proteoform_community.topdown_proteoforms.Clear();
            TopDownHit[] remaining_td_hits = new TopDownHit[0];
            //aggregate to td hit w/ highest C score
            remaining_td_hits = Lollipop.top_down_hits.OrderByDescending(t => t.score).ToArray();

            while (remaining_td_hits.Length > 0)
            {
                TopDownHit root = remaining_td_hits[0];
                List<TopDownHit> tmp_remaining_td_hits = remaining_td_hits.ToList();
                //candiate topdown hits are those with the same theoretical accession and PTMs --> need to also be within mass tolerance used for agg
                TopDownProteoform new_pf = new TopDownProteoform(root.accession, root, tmp_remaining_td_hits.Where(h => h.accession == root.accession && h.theoretical_mass == root.theoretical_mass).ToList());
                Lollipop.proteoform_community.topdown_proteoforms.Add(new_pf);
                remaining_td_hits = tmp_remaining_td_hits.Except(new_pf.topdown_hits).ToArray();
            }
            Lollipop.proteoform_community.topdown_proteoforms = Lollipop.proteoform_community.topdown_proteoforms.Where(p => p != null).ToList();
        }

        public static void make_td_relationships()
        {
            Lollipop.td_relations.Clear();
            Lollipop.td_relations = Lollipop.proteoform_community.relate_td(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToList(), Lollipop.proteoform_community.theoretical_proteoforms.ToList(), Lollipop.proteoform_community.topdown_proteoforms);
        }


        //PROTEOFORM FAMILIES -- see ProteoformCommunity
        public static string family_build_folder_path = "";
        public static int deltaM_edge_display_rounding = 2;
    }
}
