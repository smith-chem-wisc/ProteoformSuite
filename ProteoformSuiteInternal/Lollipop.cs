using Accord.Math;
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
        public static bool neucode_labeled = true;
        public static bool td_results = false;

        //input file auxillary methods
        public static IEnumerable<InputFile> identification_files() { return input_files.Where(f => f.purpose == Purpose.Identification); }
        public static IEnumerable<InputFile> quantification_files() { return input_files.Where(f => f.purpose == Purpose.Quantification); }
        public static IEnumerable<InputFile> calibration_files() { return input_files.Where(f => f.purpose == Purpose.Calibration); }
        public static IEnumerable<InputFile> bottomup_files() { return input_files.Where(f => f.purpose == Purpose.BottomUp); }
        public static IEnumerable<InputFile> topdown_files() { return input_files.Where(f => f.purpose == Purpose.TopDown); }

        //quantification
        public static int countOfBioRepsInOneCondition; //need this in quantification to select which proteoforms to perform calculations on.
        public static Dictionary<string, List<int>> ltConditionsBioReps = new Dictionary<string, List<int>>(); //key is the condition and value is the number of bioreps (not the list of bioreps)
        public static Dictionary<string, List<int>> hvConditionsBioReps = new Dictionary<string, List<int>>(); //key is the condition and value is the number of bioreps (not the list of bioreps)
        public static Dictionary<int, List<int>> quantBioFracCombos; //this dictionary has an integer list of bioreps with an integer list of observed fractions. this way we can be missing reps and fractions.
        public static List<Tuple<int, int, double>> normalizationFactors;

        //theoretical database


        public static void getBiorepsFractionsList()  //this should be moved to the appropriate location. somewhere at the start of raw component/end of load component.
        {
            quantBioFracCombos = new Dictionary<int, List<int>>();
            List<int> bioreps = Lollipop.input_files.Where(q => q.purpose == Purpose.Quantification).Select(b => b.biological_replicate).Distinct().ToList();
            List<int> fractions = new List<int>();
            foreach (int b in bioreps)
            {
                fractions = Lollipop.input_files.Where(q => q.purpose == Purpose.Quantification).Where(rep => rep.biological_replicate == b).Select(f => f.fraction).ToList();
                if (fractions != null)
                    fractions = fractions.Distinct().ToList();
                quantBioFracCombos.Add(b, fractions);
            }           
        }

        public static void getObservationParameters() //examines the conditions and bioreps to determine the maximum number of observations to require for quantification
        {
            List<string> ltConditions = new List<string>();
            List<string> hvConditions = new List<string>();
            ltConditionsBioReps.Clear();
            hvConditionsBioReps.Clear();

            foreach (InputFile inFile in Lollipop.quantification_files().ToList())
            {
                ltConditions.Add(inFile.lt_condition);
                if (Lollipop.neucode_labeled)
                    hvConditions.Add(inFile.hv_condition);
            }

            ltConditions = ltConditions.Distinct().ToList();
            if (hvConditions.Count > 0)
                hvConditions = hvConditions.Distinct().ToList();

            foreach (string condition in ltConditions)
            {
                //ltConditionsBioReps.Add(condition, Lollipop.quantification_files().Where(f => f.lt_condition == condition).Select(b => b.biological_replicate).ToList().Distinct().Count()); // this gives the count of bioreps in the specified condition
                List<int> bioreps = Lollipop.quantification_files().Where(f => f.lt_condition == condition).Select(b => b.biological_replicate).ToList();
                bioreps = bioreps.Distinct().ToList();
                ltConditionsBioReps.Add(condition, bioreps);

            }

            foreach (string condition in hvConditions)
            {
                //hvConditionsBioReps.Add(condition, Lollipop.quantification_files().Where(f => f.hv_condition == condition).Select(b => b.biological_replicate).ToList().Distinct().Count()); // this gives the count of bioreps in the specified condition
                List<int> bioreps = Lollipop.quantification_files().Where(f => f.hv_condition == condition).Select(b => b.biological_replicate).ToList();
                bioreps = bioreps.Distinct().ToList();
                hvConditionsBioReps.Add(condition, bioreps);
                
            }

            int minLt = ltConditionsBioReps.Values.Select(v => v.Count).ToList().Min();
            int minHv = 0;
            if (hvConditionsBioReps.Values.Count() > 0)
            {
                minHv = hvConditionsBioReps.Values.Select(v => v.Count).ToList().Min();
                countOfBioRepsInOneCondition = Math.Min(minLt, minHv);
            }
            else
                countOfBioRepsInOneCondition = minLt;
        }

        public static void process_raw_components()
        {
            if (input_files.Any(f => f.purpose == Purpose.Calibration))
                correctionFactors = calibration_files().SelectMany(file => Correction.CorrectionFactorInterpolation(read_corrections(file))).ToList();
            Parallel.ForEach(input_files.Where(f => f.purpose == Purpose.Identification), file =>
            {
                List<Component> someComponents = file.reader.read_components_from_xlsx(file, correctionFactors);
                lock (raw_experimental_components) raw_experimental_components.AddRange(someComponents);
            });

            if (neucode_labeled) process_neucode_components();
        }

        private static void process_neucode_components()
        {
            foreach (InputFile inputFile in identification_files().ToList())
            {
                foreach (string scan_range in inputFile.reader.scan_ranges)
                {
                    find_neucode_pairs(inputFile.reader.final_components.Where(c => c.scan_range == scan_range), Lollipop.raw_neucode_pairs);
                }
            }
        }

        public static void process_raw_quantification_components()
        {
            if (input_files.Any(f => f.purpose == Purpose.Quantification))
                correctionFactors = calibration_files().SelectMany(file => Correction.CorrectionFactorInterpolation(read_corrections(file))).ToList();
            Parallel.ForEach(quantification_files(), file => 
            {
                List<Component> someComponents = file.reader.read_components_from_xlsx(file, correctionFactors);
                lock (raw_quantification_components) raw_quantification_components.AddRange(someComponents);
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
        public static decimal max_intensity_ratio = 6m;
        public static decimal min_intensity_ratio = 1.4m;
        public static decimal max_lysine_ct = 26.2m;
        public static decimal min_lysine_ct = 1.5m;

        public static List<NeuCodePair> find_neucode_pairs(IEnumerable<Component> components_in_file_scanrange, List<NeuCodePair> destination)
        {
            List<NeuCodePair> pairsInScanRange = new List<NeuCodePair>();
            //Add putative neucode pairs. Must be in same spectrum, mass must be within 6 Da of each other
            List<Component> components = components_in_file_scanrange.OrderBy(c => c.weighted_monoisotopic_mass).ToList();
            Parallel.ForEach(components, lower_component =>
            {
                List<Component> higher_mass_components = components.Where(higher_component => higher_component != lower_component && higher_component.weighted_monoisotopic_mass > lower_component.weighted_monoisotopic_mass).ToList();
                foreach (Component higher_component in higher_mass_components)
                {
                    lock (lower_component) lock (higher_component) // Turns out the LINQ queries in here, especially for overlapping_charge_states, aren't thread safe
                    {
                        double mass_difference = higher_component.weighted_monoisotopic_mass - lower_component.weighted_monoisotopic_mass;
                        if (mass_difference < 6)
                        {
                            List<int> lower_charges = lower_component.charge_states.Select(charge_state => charge_state.charge_count).ToList<int>();
                            List<int> higher_charges = higher_component.charge_states.Select(charge_states => charge_states.charge_count).ToList<int>();
                            List<int> overlapping_charge_states = lower_charges.Intersect(higher_charges).ToList();
                            double lower_intensity = opened_raw_comps ? lower_component.intensity_sum_olcs : lower_component.calculate_sum_intensity_olcs(overlapping_charge_states);
                            double higher_intensity = opened_raw_comps ? higher_component.intensity_sum_olcs : higher_component.calculate_sum_intensity_olcs(overlapping_charge_states);
                            bool light_is_lower = true; //calculation different depending on if neucode light is the heavier/lighter component
                            if (lower_intensity > 0 && higher_intensity > 0)
                            {
                                NeuCodePair pair = lower_intensity > higher_intensity ?
                                    new NeuCodePair(lower_component, higher_component, mass_difference, overlapping_charge_states, light_is_lower) : //lower mass is neucode light
                                    new NeuCodePair(higher_component, lower_component, mass_difference, overlapping_charge_states, !light_is_lower); //higher mass is neucode light

                                lock (pairsInScanRange) pairsInScanRange.Add(pair);
                            }
                        }
                    }
                }
            });

            foreach (NeuCodePair pair in pairsInScanRange
                .OrderBy(p => Math.Min(p.neuCodeLight.weighted_monoisotopic_mass, p.neuCodeHeavy.weighted_monoisotopic_mass)) //lower_component
                .ThenBy(p => Math.Max(p.neuCodeLight.weighted_monoisotopic_mass, p.neuCodeHeavy.weighted_monoisotopic_mass)).ToList()) //higher_component
            {
                lock (destination)
                {
                    if (pair.weighted_monoisotopic_mass <= pair.neuCodeHeavy.weighted_monoisotopic_mass + Lollipop.MONOISOTOPIC_UNIT_MASS // the heavy should be at higher mass. Max allowed is 1 dalton less than light.                                    
                    && !destination.Any(p => p.id_heavy == pair.id_light && p.neuCodeLight.intensity_sum > pair.neuCodeLight.intensity_sum)) // we found that any component previously used as a heavy, which has higher intensity, is probably correct, and that that component should not get reuused as a light.)
                    {
                        destination.Add(pair);
                    }
                    else
                    {
                        lock (pairsInScanRange) pairsInScanRange.Remove(pair);
                    }
                }
            }
            return pairsInScanRange;

            //return pairsInScanRange.Where(pair =>
            //    pair.weighted_monoisotopic_mass <= pair.neuCodeHeavy.weighted_monoisotopic_mass + Lollipop.MONOISOTOPIC_UNIT_MASS // the heavy should be at higher mass. Max allowed is 1 dalton less than light.                                    
            //        && !Lollipop.raw_neucode_pairs.Concat(pairsInScanRange).Any(p => p.id_heavy == pair.id_light && p.neuCodeLight.intensity_sum > pair.neuCodeLight.intensity_sum)) // we found that any component previously used as a heavy, which has higher intensity, is probably correct, and that that component should not get reuused as a light.)
            //            .ToList();
        }

        public static Tuple<Component, Component> find_next_pair(List<Component> ordered, List<Tuple<Component,Component>> running)
        {
            Component first = ordered.FirstOrDefault(c => running.All(d => c.id != d.Item1.id && c.id != d.Item2.id));
            if (first == null) return null;
            IEnumerable<Component> higher_mass_components = ordered.Where(higher_component => higher_component != first && higher_component.weighted_monoisotopic_mass > first.weighted_monoisotopic_mass);
            Component second = higher_mass_components.FirstOrDefault(c => c.id != first.id && running.All(d => c.id != d.Item1.id && c.id != d.Item2.id));
            if (second == null) return null;
            return new Tuple<Component, Component>( first, second );
        }

        //AGGREGATED PROTEOFORMS
        public static ProteoformCommunity proteoform_community = new ProteoformCommunity();
        public static List<ExperimentalProteoform> vetted_proteoforms = new List<ExperimentalProteoform>();
        public static Component[] ordered_components = new Component[0];
        public static List<Component> remaining_components = new List<Component>();
        public static List<Component> remaining_verification_components = new List<Component>();
        public static decimal mass_tolerance = 3; //ppm
        public static decimal retention_time_tolerance = 3; //min
        public static decimal missed_monos = 3;
        public static decimal missed_lysines = 2;
        public static double min_rel_abundance = 0;
        public static int min_agg_count = 1;
        public static int min_num_CS = 1;

        public static void aggregate_proteoforms()
        {
            List<ExperimentalProteoform> candidateExperimentalProteoforms = createProteoforms();
            vetExperimentalProteoforms(candidateExperimentalProteoforms);
            proteoform_community.experimental_proteoforms = vetted_proteoforms.ToArray();
            if (Lollipop.neucode_labeled && quantification_files().Count() > 0) assignQuantificationComponents();
        }

        //Rooting each experimental proteoform is handled in addition of each NeuCode pair.
        //If no NeuCodePairs exist, e.g. for an experiment without labeling, the raw components are used instead.
        //Uses an ordered list, so that the proteoform with max intensity is always chosen first
        //Lollipop.raw_neucode_pairs = Lollipop.raw_neucode_pairs.Where(p => p != null).ToList();
        public static List<ExperimentalProteoform> createProteoforms()
        {
            List<ExperimentalProteoform> candidateExperimentalProteoforms = new List<ExperimentalProteoform>();

            // Only aggregate acceptable components (and neucode pairs). Intensity sum from overlapping charge states includes all charge states if not a neucode pair.
            ordered_components = Lollipop.neucode_labeled ?
                Lollipop.raw_neucode_pairs.OrderByDescending(p => p.intensity_sum_olcs).Where(p => p.accepted == true && p.relative_abundance >= Lollipop.min_rel_abundance && p.num_charge_states >= Lollipop.min_num_CS).ToArray() :
                Lollipop.raw_experimental_components.OrderByDescending(p => p.intensity_sum).Where(p => p.accepted == true && p.relative_abundance >= Lollipop.min_rel_abundance && p.num_charge_states >= Lollipop.min_num_CS).ToArray();
            remaining_components = ordered_components.ToList();

            Component root = ordered_components[0];
            List<ExperimentalProteoform> running = new List<ExperimentalProteoform>();
            List<Thread> active = new List<Thread>();
            while (remaining_components.Count > 0 || active.Count > 0)
            {
                while (root != null && active.Count < Environment.ProcessorCount)
                {
                    ExperimentalProteoform new_pf = new ExperimentalProteoform("tbd", root, true);
                    Thread t = new Thread(new ThreadStart(new_pf.aggregate));
                    t.Start();
                    candidateExperimentalProteoforms.Add(new_pf);
                    running.Add(new_pf);
                    active.Add(t);
                    root = find_next_root(remaining_components, running);
                }

                foreach (Thread t in active) t.Join();
                foreach (ExperimentalProteoform e in running) remaining_components = remaining_components.Except(e.aggregated_components).ToList();

                running.Clear();
                active.Clear();
                root = find_next_root(remaining_components, running);
            }

            for (int i = 0; i < candidateExperimentalProteoforms.Count; i++) candidateExperimentalProteoforms[i].accession = "E" + i;
            return candidateExperimentalProteoforms;
        }

        private static ExperimentalProteoform aggregate(Component c, List<Component> remaining)
        {
            ExperimentalProteoform temp_pf = new ExperimentalProteoform("tbd", c, remaining, true); //first pass returns temporary proteoform
            ExperimentalProteoform new_pf = new ExperimentalProteoform("tbd", temp_pf, remaining, true); //second pass uses temporary protoeform from first pass.
            return new_pf;
        }

        public static Component find_next_root(List<Component> ordered, List<Component> running)
        {
            return ordered.FirstOrDefault(c =>
                running.All(d =>
                    c.weighted_monoisotopic_mass < d.weighted_monoisotopic_mass - 20 || c.weighted_monoisotopic_mass > d.weighted_monoisotopic_mass + 20));
        }
        public static Component find_next_root(List<Component> ordered, List<ExperimentalProteoform> running)
        {
            return ordered.FirstOrDefault(c =>
                running.All(d =>
                    c.weighted_monoisotopic_mass < d.root.weighted_monoisotopic_mass - 20 || c.weighted_monoisotopic_mass > d.root.weighted_monoisotopic_mass + 20));
        }
        public static ExperimentalProteoform find_next_root(List<ExperimentalProteoform> ordered, List<ExperimentalProteoform> running)
        {
            return ordered.FirstOrDefault(e =>
                running.All(f =>
                    e.agg_mass < f.agg_mass - 20 || e.agg_mass > f.agg_mass + 20));
        }

        public static void vetExperimentalProteoforms(List<ExperimentalProteoform> candidateExperimentalProteoforms) // eliminating candidate proteoforms that were mistakenly created
        {
            List<ExperimentalProteoform> candidates = candidateExperimentalProteoforms.OrderByDescending(p => p.agg_intensity).ToList();
            Lollipop.remaining_verification_components = new List<Component>(Lollipop.raw_experimental_components);

            ExperimentalProteoform candidate = candidates[0];
            List<ExperimentalProteoform> running = new List<ExperimentalProteoform>();
            List<Thread> active = new List<Thread>();
            while (candidates.Count > 0 || active.Count > 0)
            {
                while (candidate != null && active.Count < Environment.ProcessorCount)
                {
                    Thread t = new Thread(new ThreadStart(candidate.verify));
                    t.Start();
                    running.Add(candidate);
                    active.Add(t);
                    candidate = find_next_root(candidates, running);
                }

                foreach (Thread t in active)
                {
                    t.Join();
                }

                foreach (ExperimentalProteoform e in running)
                {
                    if (e.lt_verification_components.Count > 0 || Lollipop.neucode_labeled && e.lt_verification_components.Count > 0 && e.hv_verification_components.Count > 0)
                    {
                       // e.accepted = true; this is set based on the e properties
                        Lollipop.vetted_proteoforms.Add(e);
                    }
                    Lollipop.remaining_verification_components = Lollipop.remaining_verification_components.Except(e.lt_verification_components.Concat(e.hv_verification_components)).ToList();
                    candidates.Remove(e);
                }

                running.Clear();
                active.Clear();
                candidate = find_next_root(candidates, running);
            }
        }

        public static void assignQuantificationComponents()  // this is only need for neucode labeled data. quantitative components for unlabelled are assigned elsewhere "vetExperimentalProteoforms"
        {
            List<ExperimentalProteoform> proteoforms = vetted_proteoforms.OrderByDescending(x => x.agg_intensity).ToList();
            remaining_components = new List<Component>(Lollipop.raw_quantification_components);

            ExperimentalProteoform p = proteoforms[0];
            List<ExperimentalProteoform> running = new List<ExperimentalProteoform>();
            List<Thread> active = new List<Thread>();
            while (proteoforms.Count > 0 || active.Count > 0)
            {
                while (p != null && active.Count < Environment.ProcessorCount)
                {
                    Thread t = new Thread(new ThreadStart(p.assign_quantitative_components));
                    t.Start();
                    running.Add(p);
                    active.Add(t);
                    p = find_next_root(proteoforms, running);
                }

                foreach (Thread t in active)
                {
                    t.Join();
                }

                foreach (ExperimentalProteoform e in running)
                {
                    remaining_components = remaining_components.Except(e.lt_quant_components.Concat(e.hv_quant_components)).ToList();
                    proteoforms.Remove(e);
                }

                running.Clear();
                active.Clear();
                p = find_next_root(proteoforms, running);
            }
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
        public static Protein[] proteins;//these are all the proteins read from the xml
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
                else qLessThan1 = false;
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
                    theoretical_proteoforms.Add(new TheoreticalProteoform(accession, protein_description, prot, isMetCleaved, 
                        unmodified_mass, lysine_count, prot.goTerms, ptm_set, proteoform_mass, true));
                else
                    theoretical_proteoforms.Add(new TheoreticalProteoform(accession, protein_description + "_DECOY" + "_" + decoy_number.ToString(), prot, isMetCleaved, 
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
            Lollipop.et_relations = Lollipop.proteoform_community.relate_et(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToArray(), Lollipop.proteoform_community.theoretical_proteoforms, ProteoformComparison.et);
            Lollipop.ed_relations = Lollipop.proteoform_community.relate_ed();
            Lollipop.et_peaks = Lollipop.proteoform_community.accept_deltaMass_peaks(Lollipop.et_relations, Lollipop.ed_relations);
        }

        public static void make_ee_relationships()
        {
            Lollipop.ee_relations = Lollipop.proteoform_community.relate_ee(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToArray(), Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToArray(), ProteoformComparison.ee);
            Lollipop.ef_relations = Lollipop.proteoform_community.relate_unequal_ee_lysine_counts();
            Lollipop.ee_peaks = Lollipop.proteoform_community.accept_deltaMass_peaks(Lollipop.ee_relations, Lollipop.ef_relations);
        }

        //PROTEOFORM FAMILIES -- see ProteoformCommunity
        public static string family_build_folder_path = "";
        public static int deltaM_edge_display_rounding = 2;
        public static string[] node_positioning = new string[] { "Arbitrary Circle", "Mass X-Axis", "Circle by Mass" };
        
        public static string[] edge_labels = new string[] { "Mass Difference" };

        //QUANTIFICATION
        public static string numerator_condition = "";
        public static string denominator_condition = "";
        
        public static SortedDictionary<decimal, int> logIntensityHistogram = new SortedDictionary<decimal, int>();
        public static SortedDictionary<decimal, int> logSelectIntensityHistogram = new SortedDictionary<decimal, int>();
        public static decimal observedAverageIntensity; //log base 2
        public static decimal selectAverageIntensity; //log base 2
        public static decimal observedStDev;
        public static decimal selectStDev;
        public static decimal observedGaussianArea;
        public static decimal selectGaussianArea;
        public static decimal observedGaussianHeight;
        public static decimal bkgdAverageIntensity; //log base 2
        public static decimal bkgdSelectAverageIntensity; //log base 2
        public static decimal bkgdStDev;
        public static decimal bkgdSelectStDev;
        public static decimal bkgdSelectGaussianArea;
        public static decimal bkgdSelectGaussianHeight;
        public static int numMeasuredProteoformIntensities;
        public static decimal backgroundShift;
        public static decimal backgroundWidth;
        public static List<ExperimentalProteoform> satisfactoryProteoforms = new List<ExperimentalProteoform>(); // these are proteoforms meeting the required number of observations.
        public static int satisfactoryProteoformsCount;
        public static string observationsTypeRequired = "";
        public static int minObservationsRequired;
        public static int numSelectProteoformIntensities;
        public static decimal selectGaussianHeight;
        public static int numSelectMissingIntensities;
        public static List<ExperimentalProteoform.quantitativeValues> qVals = new List<ExperimentalProteoform.quantitativeValues>();
        public static decimal sKnot_minFoldChange = 1m;
        public static List<decimal> sortedProteoformTestStatistics = new List<decimal>();
        public static List<decimal> sortedGroupTestStatistics = new List<decimal>();
        public static decimal offsetTestStatistics = 1m;
        //public static decimal negativeOffsetTestStatistics = -1m;
        public static decimal minimumPositivePassingTestStatistic;
        public static decimal minimumNegativePassingTestStatistic;
        public static decimal offsetFDR;

        public static List<Protein> observedProteins = new List<Protein>();//This is the complete list of proteins included in any accepted proteoform family
        public static List<Protein> inducedOrRepressedProteins = new List<Protein>();//This is the of proteins from proteoforms that underwent significant induction or repression
        public static decimal minProteoformIntensity = 500000m;
        public static decimal minProteoformFoldChange = 1m;
        public static decimal minProteoformFDR = 0.05m;

        public static void quantify()
        {
            computeBiorepIntensities();
            defineAllObservedIntensityDistribution();
            determineProteoformsMeetingCriteria();
            defineSelectObservedIntensityDistribution();
            defineSelectBackgroundIntensityDistribution();
            computeProteoformTestStatistics();
            computeSortedTestStatistics();
            computeFoldChangeFDR();
            computeIndividualExperimentalProteoformFDR();
            fill_qValsList();
            getObservedProteins();
            getInducedOrRepressedProteins();
        }

        private static void computeBiorepIntensities()
        {
            if (Lollipop.proteoform_community.experimental_proteoforms.Count() > 0)
            {
                Parallel.ForEach(Lollipop.proteoform_community.experimental_proteoforms, eP =>
                {
                    eP.make_biorepIntensityList();
                });
            }
        }

        public static void defineAllObservedIntensityDistribution() // the distribution of all observed experimental proteoform biorep intensities
        {
            List<decimal> allIntensities = new List<decimal>();
            numMeasuredProteoformIntensities = 0;
            foreach (List<biorepIntensity> biorepIntensityList in Lollipop.proteoform_community.experimental_proteoforms.Select(b => b.biorepIntensityList))
            {
                foreach (double intensity in biorepIntensityList.Select(i => i.intensity))
                {
                    numMeasuredProteoformIntensities++;
                    decimal roundedIntensity = Math.Round((decimal)Math.Log(intensity, 2), 1);
                    allIntensities.Add(roundedIntensity);
                    if (logIntensityHistogram.ContainsKey(roundedIntensity))
                        logIntensityHistogram[roundedIntensity]++;
                    else
                        logIntensityHistogram.Add(roundedIntensity, 1);
                }
            }

            observedAverageIntensity = allIntensities.Where(i => i > 1).ToList().Average(); //these are log2 values
            observedStDev = (decimal)Math.Sqrt(allIntensities.Average(v => Math.Pow((double)v - (double)(observedAverageIntensity), 2)));
            observedGaussianArea = 0;
            bool first = true;
            decimal x1 = 0;
            decimal y1 = 0;
            foreach (KeyValuePair<decimal, int> entry in logIntensityHistogram)
            {
                if (first)
                {
                    x1 = entry.Key;
                    y1 = (decimal)entry.Value;
                    first = false;
                }
                else
                {
                    observedGaussianArea += (entry.Key - x1) * (y1 + ((decimal)entry.Value - y1) / 2);
                    x1 = entry.Key;
                    y1 = (decimal)entry.Value;
                }
            }
            observedGaussianHeight = observedGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)observedStDev, 2));

            bkgdAverageIntensity = observedAverageIntensity + backgroundShift * observedStDev;
            bkgdStDev = observedStDev * backgroundWidth;
        }

        private static void determineProteoformsMeetingCriteria()
        {
            List<string> conditions = Lollipop.ltConditionsBioReps.Keys.ToList();
            satisfactoryProteoformsCount = 0;
            satisfactoryProteoforms.Clear();
            ConcurrentBag<ExperimentalProteoform> sP = new ConcurrentBag<ExperimentalProteoform>();
            conditions.AddRange(Lollipop.hvConditionsBioReps.Keys.ToList());
            conditions = conditions.Distinct().ToList();

            object sync = new object();

            if (observationsTypeRequired == "Minimum Total from A Single Condition")//single condition
            {
                Parallel.ForEach(Lollipop.proteoform_community.experimental_proteoforms, eP =>
                {
                    foreach (string c in conditions)
                    {
                        if (eP.biorepIntensityList.Where(bc => bc.condition == c).Select(b => b.biorep).ToList().Count() == minObservationsRequired)
                        {
                            lock (sync)
                            {
                                satisfactoryProteoformsCount++;
                                sP.Add(eP);
                            }
                            break;
                        }
                    }
                });
            }

            else //any condition
            {
                Parallel.ForEach(Lollipop.proteoform_community.experimental_proteoforms, eP =>
                {
                    if (eP.biorepIntensityList.Select(c => c.condition).Distinct().ToList().Count() == minObservationsRequired)
                    {
                        lock (sync)
                        {
                            satisfactoryProteoformsCount++;
                            sP.Add(eP);
                        }
                    }
                });
            }

            satisfactoryProteoforms = sP.ToList();
        }

        private static void defineSelectObservedIntensityDistribution()
        {
            List<decimal> allIntensities = new List<decimal>();
            numSelectProteoformIntensities = 0;
            foreach (List<biorepIntensity> biorepIntensityList in satisfactoryProteoforms.Select(b => b.biorepIntensityList))
            {
                foreach (double intensity in biorepIntensityList.Select(i => i.intensity))
                {
                    numSelectProteoformIntensities++;
                    decimal roundedIntensity = Math.Round((decimal)Math.Log(intensity, 2), 1);
                    allIntensities.Add(roundedIntensity);
                    if (logSelectIntensityHistogram.ContainsKey(roundedIntensity))
                        logSelectIntensityHistogram[roundedIntensity]++;
                    else
                        logSelectIntensityHistogram.Add(roundedIntensity, 1);
                }
            }

            selectAverageIntensity = allIntensities.Where(i => i > 1).ToList().Average(); //these are log2 values
            selectStDev = (decimal)Math.Sqrt(allIntensities.Average(v => Math.Pow((double)v - (double)(selectAverageIntensity), 2)));
            selectGaussianArea = 0;
            bool first = true;
            decimal x1 = 0;
            decimal y1 = 0;
            foreach (KeyValuePair<decimal, int> entry in logSelectIntensityHistogram)
            {
                if (first)
                {
                    x1 = entry.Key;
                    y1 = (decimal)entry.Value;
                    first = false;
                }
                else
                {
                    selectGaussianArea += (entry.Key - x1) * (y1 + ((decimal)entry.Value - y1) / 2);
                    x1 = entry.Key;
                    y1 = (decimal)entry.Value;
                }
            }
            selectGaussianHeight = selectGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)observedStDev, 2));
        }

        public static void defineSelectBackgroundIntensityDistribution()
        {
            bkgdSelectAverageIntensity = observedAverageIntensity + backgroundShift * observedStDev;
            bkgdSelectStDev = observedStDev * backgroundWidth;
            int numSelectMeasurableIntensities = Lollipop.quantBioFracCombos.Keys.Count() * satisfactoryProteoforms.Count();
            if (Lollipop.neucode_labeled) numSelectMeasurableIntensities = numSelectMeasurableIntensities * 2;
            int numSelectMeasuredIntensities = 0;
            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                numSelectMeasuredIntensities += eP.biorepIntensityList.Select(s => s.biorep).ToList().Count();
            }
            numSelectMissingIntensities = numSelectMeasurableIntensities - numSelectMeasuredIntensities;
            bkgdSelectGaussianArea = selectGaussianArea / (decimal)numSelectMeasuredIntensities * (decimal)numSelectMissingIntensities;
            bkgdSelectGaussianHeight = bkgdSelectGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)bkgdSelectStDev, 2));
        }

        public static void computeProteoformTestStatistics()
        {
            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                ExperimentalProteoform.quantitativeValues q = new ExperimentalProteoform.quantitativeValues(eP, Lollipop.bkgdAverageIntensity, Lollipop.bkgdStDev, numerator_condition, denominator_condition, sKnot_minFoldChange);
            }

            qVals = satisfactoryProteoforms.Where(eP => eP.accepted == true).Select(e => e.quant).ToList();
        }

        public static void computeSortedTestStatistics()
        {
            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                sortedGroupTestStatistics.Add(eP.quant.permutedTestStatistics.Average());
                sortedProteoformTestStatistics.Add(eP.quant.testStatistic);
            }
            sortedProteoformTestStatistics.Sort();
            sortedGroupTestStatistics.Sort();
        }

        public static void computeFoldChangeFDR()
        {
            for (int i = 0; i < sortedGroupTestStatistics.Count; i++)
            {
                if (sortedProteoformTestStatistics[i] >= sortedGroupTestStatistics[i]+offsetTestStatistics)
                { 
                    minimumPositivePassingTestStatistic = sortedProteoformTestStatistics[i];
                    break; // breaks the first time the difference exceeds the cap so we're good. 
                }
            }

            for (int i = 0; i < sortedGroupTestStatistics.Count; i++)
            {
                if (sortedProteoformTestStatistics[i] <= sortedGroupTestStatistics[i] + offsetTestStatistics)
                {
                    minimumNegativePassingTestStatistic = sortedProteoformTestStatistics[i];
                }
                else
                    break;
            }

            int totalFalsePermutedPositiveValues = 0;
            int totalFalsePermutedNegativeValues = 0;
            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                totalFalsePermutedPositiveValues += eP.quant.permutedTestStatistics.Count(p => p >= minimumPositivePassingTestStatistic);
                totalFalsePermutedNegativeValues += eP.quant.permutedTestStatistics.Count(p => p <= minimumNegativePassingTestStatistic);
            }

            decimal avergePermuted = (decimal)(totalFalsePermutedPositiveValues + totalFalsePermutedNegativeValues) / (decimal)satisfactoryProteoforms.Count;

            offsetFDR = avergePermuted / ((decimal)(sortedProteoformTestStatistics.Count(s => s >= minimumPositivePassingTestStatistic)+ sortedProteoformTestStatistics.Count(s => s <= minimumNegativePassingTestStatistic)));
        }

        public static void computeIndividualExperimentalProteoformFDR()
        {
            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                eP.quant.computeExperimentalProteoformFDR();
            }
        }

        public static void fill_qValsList()
        {
            qVals.Clear();
            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                qVals.Add(eP.quant);
            }
        }


        public static void getObservedProteins()//these are all observed proteins in any of the proteoform families.
        {
            observedProteins.Clear();
            List<ProteoformFamily> pfList = satisfactoryProteoforms.Select(p => p.family).ToList();
            List<TheoreticalProteoform> tp = new List<TheoreticalProteoform>();
            foreach (ProteoformFamily pf in pfList)
            {
                tp.AddRange(pf.theoretical_proteoforms);
            }
            List<string> completeAccessionList = tp.Select(a => a.accession).ToList();
            List<string> truncAccession = new List<string>();
            foreach (string acc in completeAccessionList)
            {
                truncAccession.Add(acc.Replace("_T", "!").Split('!').FirstOrDefault());
            }
            truncAccession.Distinct();
            foreach (string acc in truncAccession)
            {
                observedProteins.AddRange(proteins.Where(p => p.accession == acc).ToList());
            }
            observedProteins.DistinctBy(a => a.accession);
        }


        public static void getInducedOrRepressedProteins()
        {
            List<ExperimentalProteoform> inducedOrRepressedProteoforms = satisfactoryProteoforms.Where(
                p => p.quant.logFoldChange > minProteoformFoldChange || 
                p.quant.logFoldChange < -minProteoformFoldChange 
                && p.quant.FDR < minProteoformFDR 
                && p.quant.intensitySum > minProteoformIntensity).ToList();

            inducedOrRepressedProteins.Clear();
            List<ProteoformFamily> pfList = inducedOrRepressedProteoforms.Select(p => p.family).ToList();
            List<TheoreticalProteoform> tp = new List<TheoreticalProteoform>();
            foreach (ProteoformFamily pf in pfList)
            {
                tp.AddRange(pf.theoretical_proteoforms);
            }
            List<string> completeAccessionList = tp.Select(a => a.accession).ToList();
            List<string> truncAccession = new List<string>();
            foreach (string acc in completeAccessionList)
            {
                truncAccession.Add(acc.Replace("_T", "!").Split('!').FirstOrDefault());
            }
            truncAccession.Distinct();
            foreach (string acc in truncAccession)
            {
                inducedOrRepressedProteins.AddRange(proteins.Where(p => p.accession == acc).ToList());
            }
            inducedOrRepressedProteins.DistinctBy(a => a.accession);
        }

        public static List<ProteoformFamily> getInterestingFamilies(List<ExperimentalProteoform.quantitativeValues> qvals)
        {
            IEnumerable<ProteoformFamily> interesting_families =
                from exp in getInterestingProteoforms(qvals)
                from fam in Lollipop.proteoform_community.families
                where fam.experimental_proteoforms.Contains(exp)
                select fam;
            return interesting_families.ToList();
        }

        public static List<ProteoformFamily> getInterestingFamilies(List<GoTermNumber> go_terms_numbers)
        {
            IEnumerable<ProteoformFamily> interesting_families =
                from fam in Lollipop.proteoform_community.families
                where fam.theoretical_proteoforms.Any(t => t.proteinList.Any(p => p.goTerms.Any(g => go_terms_numbers.Select(n => n.goTerm).Contains(g))))
                select fam;
            return interesting_families.ToList();
        }

        public static List<ExperimentalProteoform> getInterestingProteoforms(List<ExperimentalProteoform.quantitativeValues> qvals)
        {
            List<ExperimentalProteoform.quantitativeValues> interestingQuantValues = qvals.Where(q => q.intensitySum > minProteoformIntensity && Math.Abs(q.logFoldChange) > minProteoformFoldChange && q.FDR < minProteoformFDR).ToList();
            List<string> distinctAccessions = interestingQuantValues.Select(a => a.accession).ToList();
            List<ExperimentalProteoform> interestingProteoforms = Lollipop.proteoform_community.experimental_proteoforms.Where(p => distinctAccessions.Contains(p.accession)).ToList();
            interestingProteoforms.ForEach(e => e.quant.significant = true);
            return interestingProteoforms;
        }


        // GO TERMS AND GO TERM SIGNIFICANCE
        public static List<Protein> GO_ProteinBackgroundSet = new List<Protein>();//Created originally with list of all theoretical proteins but usually changed to the set of observed proteins.
        public static Dictionary<GoTerm, int> goMasterSet = new Dictionary<GoTerm, int>(); //dictionary of goterms with count of proteins in background
        public static List<GoTermNumber> goTermNumbers = new List<GoTermNumber>();//these are the count and enrichment values


        public static bool allTheoreticalProteins = false; // this sets the group used for background. True if all Proteins in the theoretical database are used. False if only proteins observed in the study are used.

        public static void GO_analysis()
        {
            establishBackgroundForGoTermAnalysis();
            fillGO_MasterSet();
            getGoTermNumbers();
            calculateGoTermFDR();
        }

        public static void establishBackgroundForGoTermAnalysis()
        {
            if (allTheoreticalProteins)
                GO_ProteinBackgroundSet = proteins.ToList();//These are proteins in the theoretical database
            else
            {
                if (observedProteins.Count() <= 0) getObservedProteins();
                else
                    GO_ProteinBackgroundSet = observedProteins;
            }
        }

        public static void fillGO_MasterSet()
        {
            goMasterSet.Clear();
            foreach (Protein p in GO_ProteinBackgroundSet)
            {
                foreach (GoTerm g in p.goTerms)
                {
                    if (goMasterSet.ContainsKey(g))
                        goMasterSet[g]++;
                    else
                        goMasterSet.Add(g, 1);
                }
            }
        }

        public static void getGoTermNumbers() //These are only for "interesting proteins", which is the set of proteins induced or repressed beyond a specified fold change, intensity and below FDR.
        {
            goTermNumbers.Clear();

            foreach (Protein p in inducedOrRepressedProteins)
            {
                foreach (GoTerm g in p.goTerms)
                {
                    if (!goTermNumbers.Select(t => t.goTerm.id).Contains(g.id))
                        goTermNumbers.Add(new GoTermNumber(g));
                }
            }
        }

        public static void calculateGoTermFDR()
        {
            foreach (GoTermNumber g in goTermNumbers)
            {
                g.benjaminiYekutieli();
            }
        }
    }
}
