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
using MathNet.Numerics;

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
        public static List<Correction> correctionFactors = new List<Correction>();
        public static List<Component> raw_experimental_components = new List<Component>();
        public static List<Component> raw_quantification_components = new List<Component>();
        public static bool neucode_labeled = true;

        //input file auxillary methods
        public static IEnumerable<InputFile> identification_files() { return input_files.Where(f => f.purpose == Purpose.Identification); }
        public static IEnumerable<InputFile> quantification_files() { return input_files.Where(f => f.purpose == Purpose.Quantification); }
        public static IEnumerable<InputFile> bottomup_files() { return input_files.Where(f => f.purpose == Purpose.BottomUp); }
        public static IEnumerable<InputFile> topdown_files() { return input_files.Where(f => f.purpose == Purpose.TopDown); }
        public static IEnumerable<InputFile> raw_files() { return input_files.Where(f => f.purpose == Purpose.RawFile); }
        public static IEnumerable<InputFile> calibration_topdown_files() { return input_files.Where(f => f.purpose == Purpose.CalibrationTopDown); }
        public static IEnumerable<InputFile> calibration_identification_files() { return input_files.Where(f => f.purpose == Purpose.CalibrationIdentification); }

        //quantification
        public static int countOfBioRepsInOneCondition; //need this in quantification to select which proteoforms to perform calculations on.
        public static Dictionary<string, List<int>> ltConditionsBioReps = new Dictionary<string, List<int>>(); //key is the condition and value is the number of bioreps (not the list of bioreps)
        public static Dictionary<string, List<int>> hvConditionsBioReps = new Dictionary<string, List<int>>(); //key is the condition and value is the number of bioreps (not the list of bioreps)
        public static Dictionary<int, List<int>> quantBioFracCombos; //this dictionary has an integer list of bioreps with an integer list of observed fractions. this way we can be missing reps and fractions.
        public static List<Tuple<int, int, double>> normalizationFactors;


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
            object sync = new object();
            Parallel.ForEach(input_files.Where(f => f.purpose == Purpose.Identification), file =>
            {
                List<Component> someComponents = file.reader.read_components_from_xlsx(file);
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
           Parallel.ForEach(quantification_files(), file => 
            {
                List<Component> someComponents = file.reader.read_components_from_xlsx(file);
                lock (raw_quantification_components) raw_quantification_components.AddRange(someComponents);
            });
        }

        //NEUCODE PAIRS
        public static List<NeuCodePair> raw_neucode_pairs = new List<NeuCodePair>();
        public static decimal max_intensity_ratio = 6;
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
        public static Component[] ordered_components;
        public static List<Component> remaining_components = new List<Component>();
        public static List<Component> remaining_verification_components = new List<Component>();
        public static decimal mass_tolerance = 3; //ppm
        public static decimal retention_time_tolerance = 3; //min
        public static decimal missed_monos = 3;
        public static decimal missed_lysines = 2;
        public static double min_rel_abundance = 0;
        public static int min_agg_count = 1;
        public static int min_num_CS = 1;
        public static double RT_tol_NC = 10;
        public static int min_num_bioreps = 0;
        public static double min_signal_to_noise = 0;

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
            ordered_components = new Component[0];
            remaining_components.Clear();
            remaining_verification_components.Clear();
            vetted_proteoforms.Clear();

            // Only aggregate acceptable components (and neucode pairs). Intensity sum from overlapping charge states includes all charge states if not a neucode pair.
            ordered_components = Lollipop.neucode_labeled ?
                Lollipop.raw_neucode_pairs.OrderByDescending(p => p.intensity_sum_olcs).Where(p => p.accepted == true && p.relative_abundance >= Lollipop.min_rel_abundance && p.num_charge_states >= Lollipop.min_num_CS).ToArray() :
                Lollipop.raw_experimental_components.OrderByDescending(p => p.intensity_sum).Where(p => p.max_signal_to_noise >= min_signal_to_noise && p.accepted == true && p.relative_abundance >= Lollipop.min_rel_abundance && p.num_charge_states >= Lollipop.min_num_CS).ToArray();
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
                    theoretical_proteoforms.Add(new TheoreticalProteoform(accession, protein_description, prot, isMetCleaved, 
                        unmodified_mass, lysine_count, prot.goTerms, ptm_set, proteoform_mass, prot.gene_id, true));
                else
                    theoretical_proteoforms.Add(new TheoreticalProteoform(accession, protein_description + "_DECOY" + "_" + decoy_number.ToString(), prot, isMetCleaved, 
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
        public static bool notch_search_et = false;
        public static bool notch_search_ee = false;
        public static List<double> notch_masses_et = new List<double>();
        public static List<double> notch_masses_ee = new List<double>();


        public static void make_et_relationships()
        {
            if (!limit_TD_BU_theoreticals) Lollipop.et_relations = Lollipop.proteoform_community.relate_et(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToList().ToArray(), Lollipop.proteoform_community.theoretical_proteoforms, ProteoformComparison.et);
            else Lollipop.et_relations = Lollipop.proteoform_community.relate_et(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToList().ToArray(), Lollipop.proteoform_community.theoretical_proteoforms.Where(t => t.psm_count_BU > 0 || t.relationships.Where(r => r.relation_type == ProteoformComparison.ttd).ToList().Count > 0).ToArray(), ProteoformComparison.et);
            Lollipop.ed_relations = Lollipop.proteoform_community.relate_ed();

            foreach (double mass in notch_masses_et)
            {
                List<ProteoformRelation> relations_in_peak = et_relations.Where(r => r.delta_mass >= mass - Lollipop.peak_width_base_et && r.delta_mass <= mass + Lollipop.peak_width_base_et).ToList();
                List<ProteoformRelation> decoy_relations = ed_relations["DecoyDatabase_0"].Where(r => r.delta_mass >= mass - Lollipop.peak_width_base_et && r.delta_mass <= mass + Lollipop.peak_width_base_et).ToList();
                if (relations_in_peak.Count > 0)
                {
                    DeltaMassPeak peak = new DeltaMassPeak(true, relations_in_peak[0], relations_in_peak);
                    peak.peak_group_fdr =  decoy_relations.Count / relations_in_peak.Count ;
                    et_peaks.Add(peak);
                    proteoform_community.delta_mass_peaks.Add(peak);
                    proteoform_community.relations_in_peaks.AddRange(relations_in_peak);
                }
            }
            //Lollipop.et_peaks = Lollipop.proteoform_community.accept_deltaMass_peaks(Lollipop.et_relations, Lollipop.ed_relations);
        }

        public static void make_ee_relationships()
        {
            Lollipop.ee_relations = Lollipop.proteoform_community.relate_ee(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToArray(), Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToArray(), ProteoformComparison.ee);
            Lollipop.ef_relations = Lollipop.proteoform_community.relate_ef();
            foreach (double mass in notch_masses_ee)
            {
                List<ProteoformRelation> relations_in_peak = ee_relations.Where(r => r.delta_mass >= mass - Lollipop.peak_width_base_ee && r.delta_mass <= mass + Lollipop.peak_width_base_ee).ToList();
                List<ProteoformRelation> decoy_relations = ef_relations.Where(r => r.delta_mass >= mass - Lollipop.peak_width_base_ee && r.delta_mass <= mass + Lollipop.peak_width_base_ee).ToList();
                if (relations_in_peak.Count > 0)
                {
                    DeltaMassPeak peak = new DeltaMassPeak(true, relations_in_peak[0], relations_in_peak);
                    peak.peak_group_fdr = decoy_relations.Count / relations_in_peak.Count;
                    ee_peaks.Add(peak);
                    proteoform_community.delta_mass_peaks.Add(peak);
                    proteoform_community.relations_in_peaks.AddRange(relations_in_peak);
                }
            }
            // Lollipop.ee_peaks = Lollipop.proteoform_community.accept_deltaMass_peaks(Lollipop.ee_relations, Lollipop.ef_relations);
        }
           
        //TOPDOWN DATA
        public static List<TopDownHit> top_down_hits = new List<TopDownHit>();

        public static void read_in_td_hits()
        {
            foreach (InputFile file in Lollipop.topdown_files())
            {
                top_down_hits.AddRange(TdBuReader.ReadTDFile(file));
            }
        }

        public static void aggregate_td_hits(bool targeted)
        {
            foreach(TopDownHit hit in top_down_hits) if (hit.result_set == Result_Set.find_unexpected_mods) hit.corrected_mass = hit.theoretical_mass; //observed mass isn't mass of proteoform if find unexpected mods search (used fragments)
            //group hits into topdown proteoforms by accession/theoretical AND observed mass
            List<TopDownProteoform> topdown_proteoforms = new List<TopDownProteoform>();
            //TopDownHit[] remaining_td_hits = new TopDownHit[0];
            List<TopDownHit> remaining_td_hits = new List<TopDownHit>();
            //aggregate to td hit w/ highest C score
            remaining_td_hits = top_down_hits.Where(h => h.targeted == targeted).OrderBy(t => Math.Abs(t.corrected_mass - t.theoretical_mass)).ToList();
            while (remaining_td_hits.Count > 0)
            {
                TopDownHit root = remaining_td_hits[0];
                //candiate topdown hits are those with the same theoretical accession and PTMs --> need to also be within RT tolerance used for agg
                TopDownProteoform new_pf = new TopDownProteoform(root.accession, root, remaining_td_hits.Where(h => h != root && h.accession == root.accession && h.theoretical_mass == root.theoretical_mass && h.same_ptm_hits(root)).ToList());
                topdown_proteoforms.Add(new_pf);
                foreach (TopDownHit hit in new_pf.topdown_hits) remaining_td_hits.Remove(hit);
            }
            topdown_proteoforms = topdown_proteoforms.Where(p => p != null).ToList();
            Lollipop.proteoform_community.topdown_proteoforms = topdown_proteoforms.ToArray();
        }

        public static void make_td_relationships()
        {
            Lollipop.td_relations.Clear();
            Lollipop.td_relations = Lollipop.proteoform_community.relate_td(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToList(), Lollipop.proteoform_community.theoretical_proteoforms.ToList(), Lollipop.proteoform_community.topdown_proteoforms.ToList());

        }

        public static void make_targeted_td_relationships()
        {
             td_relations.AddRange(Lollipop.proteoform_community.relate_targeted_td(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted && p.etd_match_count == 0).ToList(), Lollipop.proteoform_community.topdown_proteoforms.Where(p => p.targeted).ToList()));
        }

        //CALIBRATION
        public static bool calibrate_lock_mass = false;
        public static bool calibrate_td_results = true;
        public static List<TopDownHit> td_hits_calibration = new List<TopDownHit>();
        public static List<Component> uncalibrated_components = new List<Component>();
        public static List<MsScan> Ms_scans = new List<MsScan>();
        public static Dictionary<string, Func<double[], double>> td_calibration_functions = new Dictionary<string, Func<double[], double>>();

        public static void read_in_calibration_td_hits()
        {
            foreach (InputFile file in Lollipop.calibration_topdown_files())
            {
                td_hits_calibration.AddRange(TdBuReader.ReadTDFile(file));
            }
        }

        public static void calibrate_files()
        {
            foreach (InputFile file in calibration_identification_files())
            {
                get_calibration_points(file.filename);
                calibrate_td_hits(file.filename);
                Calibration.calibrate_components_in_xlsx(file);
            }

            foreach (InputFile file in calibration_topdown_files())
            {
                Calibration.calibrate_td_hits_file(file);
            }
        }

       
        public static void get_calibration_points(string filename)
        {
            List<InputFile> raw_files = Lollipop.raw_files().Where(f => f.filename == filename).ToList();
            if (raw_files.Count > 0)
            {
                InputFile raw_file = raw_files.First();
                RawFileReader.get_ms_scans(filename, raw_file.path + "\\" + raw_file.filename + raw_file.extension);

                if (Lollipop.calibrate_lock_mass)
                {
                    Calibration.raw_lock_mass(filename, raw_file.path + "\\" + raw_file.filename + raw_file.extension);
                    correctionFactors.AddRange(Correction.CorrectionFactorInterpolation(Ms_scans.Where(s => s.filename == filename).Select(s => (new Correction(s.filename, s.scan_number, s.lock_mass_shift)))));
                }

                if (Lollipop.calibrate_td_results)
                {
                    Func<double[], double> bestCf = Calibration.Run_TdMzCal(filename, raw_file.path + "\\" + raw_file.filename + raw_file.extension, td_hits_calibration.Where(h => h.filename == filename).ToList());
                    if (bestCf != null)
                    {
                        td_calibration_functions.Add(filename, bestCf);
                    }
                }
            }
        }

        public static void calibrate_td_hits(string filename)
        {
            if (Lollipop.calibrate_td_results && Lollipop.td_calibration_functions.ContainsKey(filename))
            {
                Func<double[], double> bestCf = Lollipop.td_calibration_functions[filename];
                //need to calibrate all the others
                foreach (TopDownHit hit in td_hits_calibration.Where(h => h.filename == filename).ToList())
                {
                    hit.corrected_mass = (hit.mz - bestCf(new double[] { hit.mz, hit.retention_time }))*hit.charge - hit.charge*PROTON_MASS;
                }
            }
            else if (!Lollipop.calibrate_td_results && Lollipop.calibrate_lock_mass)
            {
                foreach (TopDownHit hit in td_hits_calibration.Where(h => h.filename == filename).ToList())
                {
                    double correction = 0;
                    try { correction = correctionFactors.Where(c => c.file_name == filename && c.scan_number == hit.scan).First().correction; }
                    catch { }
                    hit.corrected_mass = (hit.mz - correction) * hit.charge  - hit.charge*PROTON_MASS;
                }
            }
        }

        //PROTEOFORM FAMILIES -- see ProteoformCommunity
        public static string family_build_folder_path = "";
        public static int deltaM_edge_display_rounding = 2;
        public static string[] node_positioning = new string[] { "Arbitrary Circle", "Mass X-Axis", "Circle by Mass" };
        public static string[] edge_labels = new string[] { "Mass Difference" };

        //QUANTIFICATION
        public static string numerator_condition = "";
        public static string denominator_condition = "";
        public static void getNormalizationFactors() //data is too noisy at present to make use of this
        {
            ////get the complete list of bioreps
            //List<int> allBioReps = quantBioFracCombos.Keys.Distinct().ToList();
            //List<int> allFractions = new List<int>();
            //foreach (int b in allBioReps)
            //{
            //    allFractions.AddRange(quantBioFracCombos[b]);
            //} 
            //allBioReps.Sort();
            //allFractions = allFractions.Distinct().ToList();
            //allFractions.Sort();
            ////find experimental proteoforms that were expressed in all bioreps
            //List<ExperimentalProteoform> ubiquitousLightProteoforms = new List<ExperimentalProteoform>();
            //List<ExperimentalProteoform> ubiquitousHeavyProteoforms = new List<ExperimentalProteoform>();

            //object sync = new object();
            //Parallel.ForEach(Lollipop.proteoform_community.experimental_proteoforms, eP =>
            //{
            //    List<int> ltBioRepsObserved = eP.lt_quant_components.Select(c => c.input_file.biological_replicate).Distinct().ToList();
            //    if(allBioReps.Intersect(ltBioRepsObserved).ToList().Count == allBioReps.Count)
            //    {
            //        eP.make_bftList();
            //        lock (sync)
            //        {
            //            ubiquitousLightProteoforms.Add(eP);
            //        }
            //    }
                
            //});

            //if (neucode_labeled)
            //    Parallel.ForEach(Lollipop.proteoform_community.experimental_proteoforms, eP =>
            //    {
            //        List<int> hvBioRepsObserved = eP.hv_quant_components.Select(c => c.input_file.biological_replicate).Distinct().ToList();
            //        if (allBioReps.Intersect(hvBioRepsObserved).ToList().Count == allBioReps.Count)
            //        {
            //            if (eP.bftIntensityList.Count == 0)
            //                eP.make_bftList();
            //            lock (sync)
            //            {
            //                ubiquitousHeavyProteoforms.Add(eP);
            //            }
            //        }

            //    });

            //int ltCount = ubiquitousLightProteoforms.Count;
            //int hvCount = ubiquitousHeavyProteoforms.Count;
            ////compute normalization factors

            //var a = new double[ubiquitousLightProteoforms.Count + ubiquitousHeavyProteoforms.Count, allBioReps.Count, allFractions.Count];

            //for (int b = 0; b < allBioReps.Count; b++)
            //    for (int f = 0; f < allFractions.Count; f++)
            //    {
            //        for (int pl = 0; pl < (ubiquitousLightProteoforms.Count); pl++)
            //            a[pl, b, f] = ubiquitousLightProteoforms[pl].bftAggIntensityValue(allBioReps[b], allFractions[f], -1, true);

            //        for (int ph = 0; ph < (ubiquitousHeavyProteoforms.Count); ph++)
            //            a[ubiquitousLightProteoforms.Count + ph, b, f] = ubiquitousHeavyProteoforms[ph].bftAggIntensityValue(allBioReps[b], allFractions[f], -1, false);                 
            //    }


            //var coefs = new double[allBioReps.Count * allFractions.Count, allBioReps.Count * allFractions.Count];

            //// Populate the coefs matrix
            //for (int b_ = 0; b_ < allBioReps.Count; b_++)
            //{
            //    for (int f_ = 0; f_ < allFractions.Count; f_++)
            //    {
            //        // Working on specific row! This row is the result of taking the gradient with respect to N{b_,f_}

            //        // For b = b_
            //        for (int f = 0; f < allFractions.Count; f++)
            //            for (int p = 0; p < (ubiquitousLightProteoforms.Count + ubiquitousHeavyProteoforms.Count); p++)
            //                coefs[b_ * allFractions.Count + f_, b_ * allFractions.Count + f] += a[p, b_, f] * a[p, b_, f_] * (1d-1d/allBioReps.Count);

            //        // For b != b_
            //        for (int b = 0; b < allBioReps.Count; b++)
            //            if (b != b_)
            //                for (int f = 0; f < allFractions.Count; f++)
            //                    for (int p = 0; p < (ubiquitousLightProteoforms.Count + ubiquitousHeavyProteoforms.Count); p++)
            //                        coefs[b_ * allFractions.Count + f_, b * allFractions.Count + f] += a[p, b, f] * a[p, b_, f_] * (-1d/allBioReps.Count);

            //    }
            //}

            //coefs[0, 0] += 1;

            //// Vector of right-hand-sides...
            //var v = new double[allBioReps.Count * allFractions.Count];
            //v[0] = 1;
            //var ye = coefs.Solve(v);

            //int sumnum = allFractions.Count;

        }

    }
}
