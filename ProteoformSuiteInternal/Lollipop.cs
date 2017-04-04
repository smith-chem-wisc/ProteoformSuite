using Accord.Math;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UsefulProteomicsDatabases;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class Lollipop
    {
        // CONSTANTS
        public const double MONOISOTOPIC_UNIT_MASS = 1.0023; // updated 161007
        public const double NEUCODE_LYSINE_MASS_SHIFT = 0.036015372;
        public const double PROTON_MASS = 1.007276474;


        // OPENING RESULTS
        public static bool opening_results = false; //set to true if previously saved tsv's are read into program
        public static bool updated_theoretical = false;
        public static bool updated_agg = false;
        public static bool opened_results_originally = false; //stays true if results ever opened
        public static bool opened_raw_comps = false;

        public static List<InputFile> input_files = new List<InputFile>();

        public static IEnumerable<InputFile> get_files(IEnumerable<InputFile> files, Purpose purpose)
        {
            return files.Where(f => f.purpose == purpose);
        }

        public static IEnumerable<InputFile> get_files(List<InputFile> files, List<Purpose> purposes)
        {
            return files.Where(f => purposes.Contains(f.purpose));
        }

        public static string[] file_lists = new string[]
        {
            "Proteoform Identification Results (.xlsx)",
            "Proteoform Quantification Results (.xlsx)",
            "Protein Databases and PTM Lists (.xml, .xml.gz, .fasta, .txt)",
            "Uncalibrated Proteoform Identification Results (.xlsx)",
            "Raw Files (.raw)",
             "Uncalibrated ProSight Top-Down Results (.xlsx)",
            "ProSight Top-Down Results (.xlsx)",
            "Bottom-Up Results MzIdentML (.mzid)"
        };

        public static List<string>[] acceptable_extensions = new List<string>[]
        {
            new List<string> { ".xlsx" },
            new List<string> { ".xlsx" },
            new List<string> { ".xml", ".gz", ".fasta", ".txt" },
            new List<string> { ".xlsx" },
            new List<string> {".raw"},
            new List<string> { ".xlsx" },
            new List<string> { ".xlsx" },
            new List<string> { ".mzid" }
        };

        public static string[] file_filters = new string[]
        {
            "Excel Files (*.xlsx) | *.xlsx",
            "Excel Files (*.xlsx) | *.xlsx",
            "Protein Databases and PTM Text Files (*.xml, *.xml.gz, *.fasta, *.txt) | *.xml;*.xml.gz;*.fasta;*.txt",
            "Excel Files (*.xlsx) | *.xlsx",
            "Raw Files (*.raw) | *.raw",
            "Excel Files (*.xlsx) | *.xlsx",
            "Excel Files (*.xlsx) | *.xlsx",
            "MZIdentML Files (*.mzid) | *.mzid"
        };

        public static List<Purpose>[] file_types = new List<Purpose>[]
        {
            new List<Purpose> { Purpose.Identification },
            new List<Purpose> { Purpose.Quantification },
            new List<Purpose> { Purpose.ProteinDatabase, Purpose.PtmList },
            new List<Purpose> { Purpose.CalibrationIdentification },
            new List<Purpose> {Purpose.RawFile },
            new List<Purpose> { Purpose.CalibrationTopDown },
            new List<Purpose> { Purpose.TopDown },
            new List<Purpose> { Purpose.BottomUp }
        };

        public static void enter_input_files(string[] files, IEnumerable<string> acceptable_extensions, List<Purpose> purposes, List<InputFile> destination)
        {
            foreach (string complete_path in files)
            {
                FileAttributes a = File.GetAttributes(complete_path);
                if ((a & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    enter_input_files(Directory.GetFiles(complete_path), acceptable_extensions, purposes, destination);
                    enter_input_files(Directory.GetDirectories(complete_path), acceptable_extensions, purposes, destination);
                    continue;
                }

                string filename = Path.GetFileNameWithoutExtension(complete_path);
                string extension = Path.GetExtension(complete_path);
                Labeling label = neucode_labeled ? Labeling.NeuCode : Labeling.Unlabeled;

                if (acceptable_extensions.Contains(extension) && !destination.Where(f => purposes.Contains(f.purpose)).Any(f => f.filename == filename))
                {
                    InputFile file;
                    if (!purposes.Contains(Purpose.ProteinDatabase))
                        file = new InputFile(complete_path, label, purposes.FirstOrDefault());
                    else if (extension == ".txt")
                        file = new InputFile(complete_path, Purpose.PtmList);
                    else
                    {
                        file = new InputFile(complete_path, Purpose.ProteinDatabase);
                        file.ContaminantDB = file.filename.Contains("cRAP");
                    }
                    destination.Add(file);
                }
            }
        }

        //raw files used for calibration
        public static string match_calibration_files()
        {
            string return_message = "";

            // Look for results files with the same filename as a calibration file, and show that they're matched
            foreach (InputFile file in Lollipop.get_files(Lollipop.input_files, Purpose.RawFile))
            {
                if (Lollipop.input_files.Where(f => f.purpose != Purpose.RawFile).Select(f => f.filename).Contains(file.filename))
                {
                    IEnumerable<InputFile> matching_files = Lollipop.input_files.Where(f => f.purpose != Purpose.RawFile && f.filename == file.filename);
                    InputFile matching_file = matching_files.First();
                    if (matching_files.Count() != 1)
                        return_message += "Warning: There is more than one results file named " + file.filename + ". Will only match calibration to the first one from " + matching_file.purpose.ToString() + "." + Environment.NewLine;
                    file.matchingCalibrationFile = true;
                    matching_file.matchingCalibrationFile = true;
                }
            }

            if (Lollipop.get_files(Lollipop.input_files, Purpose.RawFile).Count() > 0 && !Lollipop.get_files(Lollipop.input_files, Purpose.RawFile).Any(f => f.matchingCalibrationFile))
                return_message += "To use calibration files, please give them the same filenames as the deconvolution results to which they correspond.";

            return return_message;
        }


        //RAW EXPERIMENTAL COMPONENTS
        public static List<Component> raw_experimental_components = new List<Component>();
        public static List<Component> raw_quantification_components = new List<Component>();
        public static bool neucode_labeled = true;
        public static void process_raw_components(List<InputFile> input_files, List<Component> destination, Purpose purpose, bool remove_missed_monos_and_harmonics)
        {
            Parallel.ForEach(input_files.Where(f => f.purpose == purpose).ToList(), file =>
            {
                List<Component> someComponents = file.reader.read_components_from_xlsx(file, remove_missed_monos_and_harmonics);
                lock (destination) destination.AddRange(someComponents);
            });

            if (neucode_labeled && purpose == Purpose.Identification) process_neucode_components(Lollipop.raw_neucode_pairs);
        }

        private static void process_neucode_components(List<NeuCodePair> raw_neucode_pairs)
        {
            foreach (InputFile inputFile in get_files(Lollipop.input_files, Purpose.Identification).ToList())
            {
                foreach (string scan_range in inputFile.reader.scan_ranges)
                {
                    find_neucode_pairs(inputFile.reader.final_components.Where(c => c.scan_range == scan_range), raw_neucode_pairs);
                }
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
        }

        //AGGREGATED PROTEOFORMS
        public static ProteoformCommunity proteoform_community = new ProteoformCommunity();
        public static List<ExperimentalProteoform> vetted_proteoforms = new List<ExperimentalProteoform>();
        public static Component[] ordered_components;
        public static List<Component> remaining_components = new List<Component>();
        public static List<Component> remaining_verification_components = new List<Component>();
        public static List<Component> remaining_quantification_components = new List<Component>();
        public static bool validate_proteoforms = true;
        public static decimal mass_tolerance = 5; //ppm
        public static decimal retention_time_tolerance = 5; //min
        public static decimal missed_monos = 3;
        public static decimal missed_lysines = 2;
        public static double min_rel_abundance = 0;
        public static int min_agg_count = 1;
        public static int min_num_CS = 1;
        public static double RT_tol_NC = 10;
        public static int min_num_bioreps = 0;
        public static double min_signal_to_noise = 0;
        public static bool merge_by_RT = false;

        public static List<ExperimentalProteoform> aggregate_proteoforms(bool two_pass_validation, IEnumerable<NeuCodePair> raw_neucode_pairs, IEnumerable<Component> raw_experimental_components, IEnumerable<Component> raw_quantification_components, double min_rel_abundance, int min_num_CS)
        {
            List<ExperimentalProteoform> candidateExperimentalProteoforms = createProteoforms(raw_neucode_pairs, raw_experimental_components, min_rel_abundance, min_num_CS);
            if (two_pass_validation) vetted_proteoforms = vetExperimentalProteoforms(candidateExperimentalProteoforms, raw_experimental_components, vetted_proteoforms);
            else vetted_proteoforms = candidateExperimentalProteoforms;
            if (merge_by_RT) vetted_proteoforms = merge_experimentals_by_RT(vetted_proteoforms);
            else Parallel.ForEach(vetted_proteoforms, e => e.all_RTs.Add(e.agg_rt));
            proteoform_community.experimental_proteoforms = vetted_proteoforms.ToArray();
            if (Lollipop.neucode_labeled && get_files(input_files, Purpose.Quantification).Count() > 0) assignQuantificationComponents(vetted_proteoforms, raw_quantification_components);
            return vetted_proteoforms;
         }

            //Rooting each experimental proteoform is handled in addition of each NeuCode pair.
            //If no NeuCodePairs exist, e.g. for an experiment without labeling, the raw components are used instead.
            //Uses an ordered list, so that the proteoform with max intensity is always chosen first
            //Lollipop.raw_neucode_pairs = Lollipop.raw_neucode_pairs.Where(p => p != null).ToList();
            public static List<ExperimentalProteoform> createProteoforms(IEnumerable<NeuCodePair> raw_neucode_pairs, IEnumerable<Component> raw_experimental_components, double min_rel_abundance, int min_num_CS)
        {
            List<ExperimentalProteoform> candidateExperimentalProteoforms = new List<ExperimentalProteoform>();
            ordered_components = new Component[0];
            remaining_components.Clear();
            remaining_verification_components.Clear();
            vetted_proteoforms.Clear();

            // Only aggregate acceptable components (and neucode pairs). Intensity sum from overlapping charge states includes all charge states if not a neucode pair.
            ordered_components = neucode_labeled ?
                raw_neucode_pairs.OrderByDescending(p => p.intensity_sum_olcs).Where(p => p.accepted == true && p.relative_abundance >= min_rel_abundance && p.num_charge_states >= min_num_CS).ToArray() :
                raw_experimental_components.OrderByDescending(p => p.intensity_sum).Where(p => p.accepted == true && p.relative_abundance >= min_rel_abundance && p.num_charge_states >= min_num_CS).ToArray();
            Lollipop.remaining_components = new List<Component>(ordered_components);

            Component root = ordered_components.FirstOrDefault();
            List<ExperimentalProteoform> running = new List<ExperimentalProteoform>();
            List<Thread> active = new List<Thread>();
            while (Lollipop.remaining_components.Count > 0 || active.Count > 0)
            {
                while (root != null && active.Count < Environment.ProcessorCount)
                {
                    ExperimentalProteoform new_pf = new ExperimentalProteoform("tbd", root, true);
                    Thread t = new Thread(new ThreadStart(new_pf.aggregate));
                    t.Start();
                    candidateExperimentalProteoforms.Add(new_pf);
                    running.Add(new_pf);
                    active.Add(t);
                    root = find_next_root(Lollipop.remaining_components, running);
                }
                foreach (Thread t in active) t.Join();
                foreach (ExperimentalProteoform e in running) Lollipop.remaining_components = Lollipop.remaining_components.Except(e.aggregated_components).ToList();

                running.Clear();
                active.Clear();
                root = find_next_root(Lollipop.remaining_components, running);
            }

            for (int i = 0; i < candidateExperimentalProteoforms.Count; i++) candidateExperimentalProteoforms[i].accession = "E" + i;
            return candidateExperimentalProteoforms;
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

        public static List<ExperimentalProteoform> vetExperimentalProteoforms(IEnumerable<ExperimentalProteoform> candidateExperimentalProteoforms, IEnumerable<Component> raw_experimental_components, List<ExperimentalProteoform> vetted_proteoforms) // eliminating candidate proteoforms that were mistakenly created
        {
            List<ExperimentalProteoform> candidates = candidateExperimentalProteoforms.OrderByDescending(p => p.agg_intensity).ToList();
            Lollipop.remaining_verification_components = new List<Component>(raw_experimental_components);

            ExperimentalProteoform candidate = candidates.FirstOrDefault();
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
                    if (e.lt_verification_components.Count > 0 || neucode_labeled && e.lt_verification_components.Count > 0 && e.hv_verification_components.Count > 0)
                    {
                        vetted_proteoforms.Add(e);
                    }
                    Lollipop.remaining_verification_components = Lollipop.remaining_verification_components.Except(e.lt_verification_components.Concat(e.hv_verification_components)).ToList();
                    candidates.Remove(e);
                }

                running.Clear();
                active.Clear();
                candidate = find_next_root(candidates, running);
            }
            return vetted_proteoforms;
        }

        public static List<ExperimentalProteoform> assignQuantificationComponents(List<ExperimentalProteoform> vetted_proteoforms, IEnumerable<Component> raw_quantification_components)  // this is only need for neucode labeled data. quantitative components for unlabelled are assigned elsewhere "vetExperimentalProteoforms"
        {
            List<ExperimentalProteoform> proteoforms = vetted_proteoforms.OrderByDescending(x => x.agg_intensity).ToList();
            Lollipop.remaining_quantification_components = new List<Component>(raw_quantification_components);

            ExperimentalProteoform p = proteoforms.FirstOrDefault();
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
                    Lollipop.remaining_quantification_components = Lollipop.remaining_quantification_components.Except(e.lt_quant_components.Concat(e.hv_quant_components)).ToList();
                    proteoforms.Remove(e);
                }

                running.Clear();
                active.Clear();
                p = find_next_root(proteoforms, running);
            }
            return vetted_proteoforms;
        }

        //Could be improved. Used for manual mass shifting.
        //Idea 1: Start with Components -- have them find the most intense nearby component. Then, go through and correct edge cases that aren't correct.
        //Idea 2: Use the assumption that proteoforms distant to the manual shift will not regroup.
        //Idea 2.1: Put the shifted proteoforms, plus some range from the min and max masses in there, and reaggregate the components with the aggregate_proteoforms algorithm.
        public static List<ExperimentalProteoform> regroup_components(bool neucode_labeled, bool two_pass_validation, IEnumerable<InputFile> input_files, List<NeuCodePair> raw_neucode_pairs, IEnumerable<Component> raw_experimental_components, IEnumerable<Component> raw_quantification_components, double min_rel_abundance, int min_num_CS)
        {
            if (neucode_labeled)
            {
                raw_neucode_pairs.Clear();
                process_neucode_components(raw_neucode_pairs);
            }
            return aggregate_proteoforms(two_pass_validation, raw_neucode_pairs, raw_experimental_components, raw_quantification_components, min_rel_abundance, min_num_CS);
        }

        public static List<ExperimentalProteoform> merge_experimentals_by_RT(List<ExperimentalProteoform> experimentals_to_merge)
        {
            List<ExperimentalProteoform> experimentals = experimentals_to_merge.OrderByDescending(p => p.observation_count).ToList() ;
            List<ExperimentalProteoform> refined_experimentals = new List<ExperimentalProteoform>();
            while (experimentals.Count > 0)
            {
                ExperimentalProteoform e = experimentals[0];
                refined_experimentals.Add(e);
                if (!e.all_RTs.Contains(e.agg_rt)) e.all_RTs.Add(e.agg_rt);
                experimentals.Remove(e);
                List<ExperimentalProteoform> e_same_mass =  experimentals.Where(p => p != e && Math.Abs(p.agg_mass - e.agg_mass) <= e.agg_mass / 1000000 * (double)Lollipop.mass_tolerance / 2 ).ToList();
                if (e_same_mass.Count > 0)
                {
                    e.all_RTs.AddRange(e_same_mass.Select(p => p.agg_rt));
                    //foreach(ExperimentalProteoform exp in e_same_mass) 
                    e.aggregated_components.AddRange(e_same_mass.SelectMany(p => p.aggregated_components));
                    experimentals = experimentals.Except(e_same_mass).ToList();
                    //try recalculating mass see if improved or worse
                }
            }
            Parallel.ForEach(refined_experimentals, e =>
            {
                e.agg_intensity = e.aggregated_components.Select(p => p.intensity_sum).Sum();
                e.agg_mass = e.aggregated_components.Select(p => (p.weighted_monoisotopic_mass - Math.Round(p.weighted_monoisotopic_mass - e.root.weighted_monoisotopic_mass, 0) * Lollipop.MONOISOTOPIC_UNIT_MASS) * p.intensity_sum / e.agg_intensity).Sum(); //remove the monoisotopic errors before aggregating masses
                e.modified_mass = e.agg_mass;
                e.accepted = e.aggregated_components.Count() >= Lollipop.min_agg_count;
            });
            return refined_experimentals;
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
        public static string accessions_of_interest_list_filepath = "";
        public static string interest_type = "Of interest"; //label for proteins of interest. can be changed 
        public static string[] mod_types_to_exclude = new string[] { "Metal", "PeptideTermMod", "TrypticProduct" };
        public static Dictionary<InputFile, Protein[]> theoretical_proteins = new Dictionary<InputFile, Protein[]>();
        public static ProteinWithGoTerms[] expanded_proteins = new ProteinWithGoTerms[0];
        public static List<Psm> psm_list = new List<Psm>();
        public static Dictionary<string, IList<Modification>> uniprotModificationTable = new Dictionary<string, IList<Modification>>();
        static Dictionary<char, double> aaIsotopeMassList;
        public static List<ModificationWithLocation> defaultMods = new List<ModificationWithLocation>();

        public static void get_theoretical_proteoforms()
        {
            updated_theoretical = true;
            //Clear out data from potential previous runs
            proteoform_community.decoy_proteoforms.Clear();
            psm_list.Clear();
            theoretical_proteins.Clear();

            //Read the UniProt-XML and ptmlist
            Loaders.LoadElements(Path.Combine(Environment.CurrentDirectory, "elements.dat"));
            List<ModificationWithLocation> all_known_modifications = defaultMods;
            all_known_modifications.AddRange(get_files(Lollipop.input_files, Purpose.PtmList).SelectMany(file => PtmListLoader.ReadModsFromFile(file.complete_path)).ToList());
            all_known_modifications = new HashSet<ModificationWithLocation>(all_known_modifications).ToList();
            read_mods(all_known_modifications);
            Dictionary<string, Modification> um;
            Parallel.ForEach(get_files(Lollipop.input_files, Purpose.ProteinDatabase).ToList(), database =>
            {
                lock (theoretical_proteins) theoretical_proteins.Add(database, ProteinDbLoader.LoadProteinXML(database.complete_path, false, all_known_modifications, database.ContaminantDB, new string[] { "GO" }, mod_types_to_exclude, out um).ToArray());
                lock (all_known_modifications) all_known_modifications.AddRange(ProteinDbLoader.GetPtmListFromProteinXml(database.complete_path).OfType<ModificationWithLocation>());
            });
            all_known_modifications = new HashSet<ModificationWithLocation>(all_known_modifications).ToList();
            read_mods(all_known_modifications);
            expanded_proteins = expand_protein_entries(theoretical_proteins.Values.SelectMany(p => p).ToArray());
            aaIsotopeMassList = new AminoAcidMasses(methionine_oxidation, carbamidomethylation, Lollipop.natural_lysine_isotope_abundance, Lollipop.neucode_light_lysine, Lollipop.neucode_heavy_lysine).AA_Masses;
            if (combine_identical_sequences) expanded_proteins = group_proteins_by_sequence(expanded_proteins);

            //Read the Morpheus BU data into PSM list
            foreach (InputFile file in Lollipop.input_files.Where(f => f.purpose == Purpose.BottomUp).ToList())
            {
                List<Psm> psm_from_file = TdBuReader.ReadBUFile(file.complete_path);
                psm_list.AddRange(psm_from_file);
            }

            //PARALLEL PROBLEM
            process_entries(expanded_proteins);
            process_decoys(expanded_proteins);

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
                tp.psm_list = Lollipop.psm_list.Where(p => p.protein_accession == tp.accession.Split('_')[0]).ToList();
            });
            for (int i = 0; i < decoy_databases; i++)
            {
                Parallel.ForEach<TheoreticalProteoform>(Lollipop.proteoform_community.decoy_proteoforms["DecoyDatabase_" + i], dp =>
                 {
                     dp.psm_list = Lollipop.psm_list.Where(p => p.protein_accession == dp.accession.Split('_')[0]).ToList();
                 });
            }
        }

        public static void read_mods_folder()
        {
            Loaders.LoadElements(Path.Combine(Environment.CurrentDirectory, "elements.dat"));
            defaultMods = Directory.GetFiles(@"Mods").SelectMany(file => PtmListLoader.ReadModsFromFile(Path.GetFullPath(file))).ToList();
            read_mods(defaultMods);
        }

        public static void read_mods(List<ModificationWithLocation> all_modifications)
        {
            uniprotModificationTable.Clear();
            foreach (var nice in all_modifications)
            {
                IList<Modification> val;
                if (uniprotModificationTable.TryGetValue(nice.id, out val))
                    val.Add(nice);
                else
                    uniprotModificationTable.Add(nice.id, new List<Modification> { nice });
            }
        }
        
        public static void read_mods()
        {
            Loaders.LoadElements(Path.Combine(Environment.CurrentDirectory, "elements.dat"));
            List<ModificationWithLocation> all_modifications = get_files(Lollipop.input_files, Purpose.PtmList).SelectMany(file => PtmListLoader.ReadModsFromFile(file.complete_path)).ToList();
            read_mods(all_modifications);
        }

        private static ProteinWithGoTerms[] expand_protein_entries(Protein[] proteins)
        {
            List<ProteinWithGoTerms> expanded_prots = new List<ProteinWithGoTerms>();
            foreach (Protein p in proteins)
            {
                List<ProteinWithGoTerms> new_prots = new List<ProteinWithGoTerms>();
                int begin = 1;
                int end = p.BaseSequence.Length;
                List<GoTerm> goTerms = p.DatabaseReferences.Where(reference => reference.Type == "GO").Select(reference => new GoTerm(reference)).ToList();
                new_prots.Add(new ProteinWithGoTerms(p.BaseSequence, p.Accession + "_" + begin.ToString() + "full" + end.ToString(), p.GeneNames, p.OneBasedPossibleLocalizedModifications, new int?[] { begin }, new int?[] { end }, new string[] { methionine_cleavage ? "full-met-cleaved" : "full" }, p.Name, p.FullName, p.IsDecoy, p.IsContaminant, p.DatabaseReferences, goTerms));
                List<ProteolysisProduct> products = p.ProteolysisProducts.ToList();
                foreach (ProteolysisProduct product in p.ProteolysisProducts)
                {
                    string feature_type = product.Type.Replace(' ', '-');
                    if (!(feature_type == "peptide" || feature_type == "propeptide" || feature_type == "chain" || feature_type == "signal-peptide") ||
                            !product.OneBasedBeginPosition.HasValue || !product.OneBasedEndPosition.HasValue)
                        continue;
                    int feature_begin = (int)product.OneBasedBeginPosition;
                    int feature_end = (int)product.OneBasedEndPosition;
                    if (feature_begin < 1 || feature_end < 1)
                        continue;
                    bool just_met_cleavage = methionine_cleavage && feature_begin == begin + 1 && feature_end == end;
                    string subsequence = p.BaseSequence.Substring(feature_begin - 1, feature_end - feature_begin + 1);
                    Dictionary<int, List<Modification>> segmented_ptms = p.OneBasedPossibleLocalizedModifications.Where(kv => kv.Key >= feature_begin && kv.Key <= feature_end).ToDictionary(kv => kv.Key, kv => kv.Value);
                    if (!just_met_cleavage && subsequence.Length != p.BaseSequence.Length && subsequence.Length >= Lollipop.min_peptide_length)
                        new_prots.Add(new ProteinWithGoTerms(subsequence, p.Accession + "_" + feature_begin.ToString() + "frag" + feature_end.ToString(), p.GeneNames, segmented_ptms, new int?[] { feature_begin }, new int?[] { feature_end }, new string[] { feature_type }, p.Name, p.FullName, p.IsDecoy, p.IsContaminant, p.DatabaseReferences, goTerms));
                }
                expanded_prots.AddRange(new_prots);
            }
            return expanded_prots.ToArray();
        }

        private static ProteinSequenceGroup[] group_proteins_by_sequence(ProteinWithGoTerms[] proteins)
        {
            Dictionary<string, List<ProteinWithGoTerms>> sequence_groupings = new Dictionary<string, List<ProteinWithGoTerms>>();
            foreach (ProteinWithGoTerms p in proteins)
            {
                if (sequence_groupings.ContainsKey(p.BaseSequence)) sequence_groupings[p.BaseSequence].Add(p);
                else sequence_groupings.Add(p.BaseSequence, new List<ProteinWithGoTerms> { p });
            }
            return sequence_groupings.Select(kv => new ProteinSequenceGroup(kv.Value.OrderByDescending(p => p.IsContaminant ? 1 : 0))).ToArray();
        }

        private static TheoreticalProteoformGroup[] group_proteoforms_byMass(IEnumerable<TheoreticalProteoform> theoreticals)
        {
            Dictionary<double, List<TheoreticalProteoform>> mass_groupings = new Dictionary<double, List<TheoreticalProteoform>>();
            foreach (TheoreticalProteoform t in theoreticals)
            {
                if (mass_groupings.ContainsKey(t.modified_mass)) mass_groupings[t.modified_mass].Add(t);
                else mass_groupings.Add(t.modified_mass, new List<TheoreticalProteoform> { t });
            }
            return mass_groupings.Select(kv => new TheoreticalProteoformGroup(kv.Value.OrderByDescending(t => t.contaminant ? 1 : 0))).ToArray();
        }

        private static void process_entries(IEnumerable<ProteinWithGoTerms> expanded_proteins)
        {
            List<TheoreticalProteoform> theoretical_proteoforms = new List<TheoreticalProteoform>();
            //foreach (Protein p in expanded_proteins)
            Parallel.ForEach<ProteinWithGoTerms>(expanded_proteins, p =>
            {
                bool isMetCleaved = (methionine_cleavage && p.ProteolysisProducts.Select(prod => prod.OneBasedBeginPosition).FirstOrDefault() == 1 && p.BaseSequence.FirstOrDefault() == 'M');
                int startPosAfterCleavage = Convert.ToInt32(isMetCleaved);
                string seq = p.BaseSequence.Substring(startPosAfterCleavage, (p.BaseSequence.Length - startPosAfterCleavage));
                EnterTheoreticalProteformFamily(seq, p, p.Accession, isMetCleaved, theoretical_proteoforms, -100);
            });
            Lollipop.proteoform_community.theoretical_proteoforms = theoretical_proteoforms.ToArray();
        }

        private static void process_decoys(ProteinWithGoTerms[] expanded_proteins)
        {
            for (int decoyNumber = 0; decoyNumber < Lollipop.decoy_databases; decoyNumber++)
            {
                List<TheoreticalProteoform> decoy_proteoforms = new List<TheoreticalProteoform>();
                string giantProtein = GetOneGiantProtein(expanded_proteins, methionine_cleavage); //Concatenate a giant protein out of all protein read from the UniProt-XML, and construct target and decoy proteoform databases
                string decoy_database_name = decoy_database_name_prefix + decoyNumber;
                ProteinWithGoTerms[] shuffled_proteins = new ProteinWithGoTerms[expanded_proteins.Length];
                shuffled_proteins = expanded_proteins;
                new Random().Shuffle(shuffled_proteins); //randomize order of protein array

                int prevLength = 0;
                Parallel.ForEach<ProteinWithGoTerms>(shuffled_proteins, p =>
                {
                    bool isMetCleaved = (methionine_cleavage && p.ProteolysisProducts.Select(prod => prod.OneBasedBeginPosition).FirstOrDefault() == 1 && p.BaseSequence.FirstOrDefault() == 'M'); // methionine cleavage of N-terminus specified
                    int startPosAfterCleavage = Convert.ToInt32(isMetCleaved);

                    //From the concatenated proteome, cut a decoy sequence of a randomly selected length
                    int hunkLength = p.BaseSequence.Length - startPosAfterCleavage;
                    string hunk = giantProtein.Substring(prevLength, hunkLength);
                    prevLength += hunkLength;

                    EnterTheoreticalProteformFamily(hunk, p, p.Accession + "_DECOY_" + decoyNumber, isMetCleaved, decoy_proteoforms, decoyNumber);
                });
                Lollipop.proteoform_community.decoy_proteoforms.Add(decoy_database_name, decoy_proteoforms.ToArray());
            }
        }

        private static void EnterTheoreticalProteformFamily(string seq, ProteinWithGoTerms prot, string accession, bool isMetCleaved, List<TheoreticalProteoform> theoretical_proteoforms, int decoy_number)
        {
            //Calculate the properties of this sequence
            double unmodified_mass = TheoreticalProteoform.CalculateProteoformMass(seq, aaIsotopeMassList);
            int lysine_count = seq.Split('K').Length - 1;
            List<PtmSet> unique_ptm_groups = new PtmCombos(prot.OneBasedPossibleLocalizedModifications).get_combinations(max_ptms);
            bool check_contaminants = theoretical_proteins.Any(item => item.Key.ContaminantDB);

            //Enumerate the ptm combinations with _P# to distinguish from the counts in ProteinSequenceGroups (_#G) and TheoreticalPfGps (_#T)
            int ptm_set_counter = 1;
            foreach (PtmSet ptm_set in unique_ptm_groups)
            {
                lock (theoretical_proteoforms) theoretical_proteoforms.Add(
                    new TheoreticalProteoform(
                        accession + "_P" + ptm_set_counter.ToString(), 
                        prot.FullDescription + "_P" + ptm_set_counter.ToString() + (decoy_number < 0 ? "" : "_DECOY_" + decoy_number.ToString()), 
                        new ProteinWithGoTerms[] { prot }, 
                        isMetCleaved,
                        unmodified_mass, 
                        lysine_count, 
                        ptm_set, 
                        decoy_number < 0, 
                        check_contaminants, 
                        theoretical_proteins)
                    );
                ptm_set_counter++;
            } 
        }

        private static string GetOneGiantProtein(IEnumerable<Protein> proteins, bool methionine_cleavage)
        {
            StringBuilder giantProtein = new StringBuilder(5000000); // this set-aside is autoincremented to larger values when necessary.
            foreach (Protein protein in proteins)
            {
                string sequence = protein.BaseSequence;
                bool isMetCleaved = methionine_cleavage && (sequence.Substring(0, 1) == "M");
                int startPosAfterMetCleavage = Convert.ToInt32(isMetCleaved);
                switch (protein.ProteolysisProducts.Select(p => p.Type).FirstOrDefault())
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

        private static void mark_accessions_of_interest()
        {
            string[] lines = File.ReadAllLines(Lollipop.accessions_of_interest_list_filepath);
            Parallel.ForEach<string>(lines, accession =>
            {
                List<TheoreticalProteoform> theoreticals = Lollipop.proteoform_community.theoretical_proteoforms.Where(p => p.accession.Contains(accession)).ToList();
                foreach (TheoreticalProteoform theoretical in theoreticals) { theoretical.of_interest = Lollipop.interest_type; }
            });
        }


        //ET,ED,EE,EF COMPARISONS
        public static double ee_max_mass_difference = 300;
        public static double ee_max_RetentionTime_difference = 2.5;
        public static double et_low_mass_difference = -300;
        public static double et_high_mass_difference = 300;
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
        public static Dictionary<string, List<ProteoformRelation>> ef_relations = new Dictionary<string, List<ProteoformRelation>>();
        public static List<DeltaMassPeak> et_peaks = new List<DeltaMassPeak>();
        public static List<DeltaMassPeak> ee_peaks = new List<DeltaMassPeak>();
        public static List<ProteoformRelation> td_relations = new List<ProteoformRelation>(); //td data
        public static bool notch_search_et = false;
        public static bool notch_search_ee = false;
        public static List<double> notch_masses_et = new List<double>();
        public static List<double> notch_masses_ee = new List<double>();


        public static void make_et_relationships(ProteoformCommunity community)
        {
            if (!limit_TD_BU_theoreticals) Lollipop.et_relations = Lollipop.proteoform_community.relate_et(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToList().ToArray(), Lollipop.proteoform_community.theoretical_proteoforms, ProteoformComparison.et);
            else Lollipop.et_relations = Lollipop.proteoform_community.relate_et(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToList().ToArray(), Lollipop.proteoform_community.theoretical_proteoforms.Where(t => t.psm_count_BU > 0 || t.relationships.Where(r => r.relation_type == ProteoformComparison.ttd).ToList().Count > 0).ToArray(), ProteoformComparison.et);
            Lollipop.ed_relations = Lollipop.proteoform_community.relate_ed();
            Lollipop.et_peaks = Lollipop.proteoform_community.accept_deltaMass_peaks(Lollipop.et_relations, Lollipop.ed_relations);
        }



        public static double test = 0;
        public static void make_ee_relationships(ProteoformCommunity community)
        {
            Lollipop.ee_relations = Lollipop.proteoform_community.relate_ee(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToArray(), Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToArray(), ProteoformComparison.ee);
            Lollipop.ef_relations = Lollipop.proteoform_community.relate_ef();
            Lollipop.ee_peaks = Lollipop.proteoform_community.accept_deltaMass_peaks(Lollipop.ee_relations, Lollipop.ef_relations);
            using (var writer = new StreamWriter("C:\\users\\lschaffer2\\Desktop\\testingef.txt"))
            {
                writer.WriteLine(test);
                for (int i = 0; i < 10; i++)
                foreach (List<ProteoformRelation> ef in ef_relations.Values)
                {
                    writer.WriteLine(ef.Count);
                }

                foreach (DeltaMassPeak peak in ee_peaks)
                {
                    foreach (List<ProteoformRelation> ef in ef_relations.Values)
                    {
                        List<ProteoformRelation> relations = ef.Where(r => r.delta_mass >= peak.peak_deltaM_average - peak_width_base_ee / 2 && r.delta_mass <= peak.peak_deltaM_average + peak_width_base_ee / 2).ToList();
                        writer.WriteLine(peak.peak_deltaM_average + "\t" + relations.Count);
                    }
                }
            }

        }

        //TOPDOWN DATA
        public static List<TopDownHit> top_down_hits = new List<TopDownHit>();

        public static void read_in_td_hits()
        {
            foreach (InputFile file in Lollipop.input_files.Where(f => f.purpose == Purpose.TopDown).ToList())
            {
                top_down_hits.AddRange(TdBuReader.ReadTDFile(file));
            }
        }

        public static void aggregate_td_hits()
        {
            //group hits into topdown proteoforms by accession/theoretical AND observed mass
            List<TopDownProteoform> topdown_proteoforms = new List<TopDownProteoform>();
            //TopDownHit[] remaining_td_hits = new TopDownHit[0];
            List<TopDownHit> remaining_td_hits = new List<TopDownHit>();
            //aggregate to td hit w/ highest C score
            remaining_td_hits = top_down_hits.OrderBy(t => Math.Abs(t.corrected_mass - t.theoretical_mass - Math.Round(t.corrected_mass - t.theoretical_mass, 0))).ToList();
            while (remaining_td_hits.Count > 0)
            {
                TopDownHit root = remaining_td_hits[0];
                //candiate topdown hits are those with the same theoretical accession and PTMs --> need to also be within RT tolerance used for agg
                TopDownProteoform new_pf = new TopDownProteoform(root.accession, root, remaining_td_hits.Where(h => h != root && h.targeted == root.targeted && h.accession == root.accession && h.sequence == root.sequence && h.same_ptm_hits(root)).ToList());
                topdown_proteoforms.Add(new_pf);
                foreach (TopDownHit hit in new_pf.topdown_hits) remaining_td_hits.Remove(hit);
            }
            topdown_proteoforms = topdown_proteoforms.Where(p => p != null).OrderByDescending(p => p.topdown_hits.Count).ToList();
            //aggregate all RT's
            List<TopDownProteoform> refined_topdown_proteoforms = new List<TopDownProteoform>();
            if (Lollipop.merge_by_RT)
            {
                while (topdown_proteoforms.Count > 0)
                {
                    TopDownProteoform p = topdown_proteoforms[0];
                    refined_topdown_proteoforms.Add(p);
                    topdown_proteoforms.Remove(p);
                    List<TopDownProteoform> p_diff_RT = topdown_proteoforms.Where(t => t != p && t.accession == p.accession && p.sequence == t.sequence && p.topdown_hits[0].same_ptm_hits(t.topdown_hits[0])).ToList();
                    p.all_RTs.AddRange(p_diff_RT.Select(t => t.agg_rt));
                    //foreach(ExperimentalProteoform exp in e_same_mass) 
                    p.topdown_hits.AddRange(p_diff_RT.SelectMany(t => t.topdown_hits));
                    topdown_proteoforms = topdown_proteoforms.Except(p_diff_RT).ToList();
                }
                Parallel.ForEach(refined_topdown_proteoforms, p =>
                {
                    p.modified_mass = p.topdown_hits.Average(h => h.corrected_mass - Math.Round(h.corrected_mass - p.root.corrected_mass));
                    p.monoisotopic_mass = p.modified_mass;
                });
            }
            else refined_topdown_proteoforms = topdown_proteoforms;
            Lollipop.proteoform_community.topdown_proteoforms = refined_topdown_proteoforms.ToArray();
        }

        public static void make_td_relationships()
        {
            Lollipop.td_relations.Clear();
            //all experimental proteoforms - mark accepted if not already and in match
            Lollipop.td_relations = Lollipop.proteoform_community.relate_td(Lollipop.proteoform_community.experimental_proteoforms.ToList(), Lollipop.proteoform_community.theoretical_proteoforms.ToList(), Lollipop.proteoform_community.topdown_proteoforms.Where(p => !p.targeted).ToList());
        }

        public static void make_targeted_td_relationships()
        {
             td_relations.AddRange(Lollipop.proteoform_community.relate_targeted_td(Lollipop.proteoform_community.experimental_proteoforms.Where(p => p.accepted).ToList(), Lollipop.proteoform_community.topdown_proteoforms.Where(p => p.targeted).ToList()));
        }

        //CALIBRATION
        public static bool calibrate_lock_mass = false;
        public static bool calibrate_td_results = true;
        public static List<TopDownHit> td_hits_calibration = new List<TopDownHit>();
        public static List<MsScan> Ms_scans = new List<MsScan>();
        public static Dictionary<string, Func<double[], double>> td_calibration_functions = new Dictionary<string, Func<double[], double>>();
        public static List<Correction> correctionFactors = new List<Correction>();
        public static Dictionary<Tuple<string, double, double>, double> file_mz_correction = new Dictionary<Tuple<string, double, double>, double>();
        public static Dictionary<Tuple<string, int, double>, double> td_hit_correction = new Dictionary<Tuple<string, int, double>, double>();
        public static List<Component> calibration_components = new List<Component>();

        public static void read_in_calibration_td_hits()
        {
            foreach (InputFile file in Lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationTopDown))
            {
                td_hits_calibration.AddRange(TdBuReader.ReadTDFile(file));
            }
        }

        public static void calibrate_files()
        {
            //add distinct filenames from both deconvolution results and topdown results
            List<string> filenames = new List<string>();
            filenames.AddRange(input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).Select(f => f.filename).Distinct());
            filenames.AddRange(Lollipop.td_hits_calibration.Select(f => f.filename).Distinct());
            //get calibration points and calibrate deconvolution files
            Parallel.ForEach(filenames.Distinct(), filename =>
                {
                    get_calibration_points(filename);
                    calibrate_td_hits(filename);
                    determine_component_shift(input_files.Where(f => f.purpose == Purpose.CalibrationIdentification && f.filename == filename).ToList());
                    InputFile file = input_files.Where(f => f.purpose == Purpose.CalibrationIdentification && f.filename == filename).FirstOrDefault();
                    if (file != null) Calibration.calibrate_components_in_xlsx(file);
                });
            foreach (InputFile file in Lollipop.input_files.Where(f => f.purpose == Purpose.CalibrationTopDown))
            {
                Calibration.calibrate_td_hits_file(file);
            }
        }
    

    public static void get_calibration_points(string filename)
        {
            List<InputFile> raw_files = Lollipop.input_files.Where(f => f.purpose == Purpose.RawFile && f.filename == filename).ToList();
            if (raw_files.Count > 0)
            {
                //get calibration points
                InputFile raw_file = raw_files.First();
                RawFileReader.get_ms_scans(filename, raw_file.complete_path);

                if (Lollipop.calibrate_lock_mass)
                {
                    Calibration.raw_lock_mass(filename, raw_file.complete_path);
                    lock(correctionFactors) correctionFactors.AddRange(Correction.CorrectionFactorInterpolation(Ms_scans.Where(s => s.filename == filename).Select(s => (new Correction(s.filename, s.scan_number, s.lock_mass_shift)))));
                }

                if (Lollipop.calibrate_td_results)
                {
                    Func<double[], double> bestCf = Calibration.Run_TdMzCal(filename, td_hits_calibration.Where(h => h.filename == filename).ToList());
                    if (bestCf != null)
                    {
                        lock(td_calibration_functions) td_calibration_functions.Add(filename, bestCf);
                    }
                }
            }
        }

        public static void determine_component_shift(List<InputFile> input_files)
        {
            process_raw_components(input_files, calibration_components, Purpose.CalibrationIdentification, false);
            Parallel.ForEach(calibration_components, c =>
            {
                if (!calibrate_td_results || td_calibration_functions.ContainsKey(c.input_file.filename))
                {
                    foreach (ChargeState cs in c.charge_states)
                    {
                        double corrected_mz = cs.mz_centroid + (Lollipop.calibrate_td_results ? -1 * td_calibration_functions[c.input_file.filename](new double[] { cs.mz_centroid, Convert.ToDouble(c.rt_range.Split('-')[0]) }) : -1 * Calibration.get_correction_factor(c.input_file.filename, c.scan_range));
                        var key = new Tuple<string, double, double>(c.input_file.filename, Math.Round(cs.mz_centroid, 0), Math.Round(cs.intensity, 0));
                        lock (file_mz_correction) if (!file_mz_correction.ContainsKey(key)) file_mz_correction.Add(key, corrected_mz);
                    }
                }
            });
        }

        public static void calibrate_td_hits(string filename)
        {
            if (Lollipop.calibrate_td_results && Lollipop.td_calibration_functions.ContainsKey(filename))
            {
                Func<double[], double> bestCf = Lollipop.td_calibration_functions[filename];
                //need to calibrate all the others
                foreach (TopDownHit hit in td_hits_calibration.Where(h => h.filename == filename))
                {
                    Tuple<string, int, double> key = new Tuple<string, int, double>(filename, hit.scan, hit.reported_mass);
                   if (!td_hit_correction.ContainsKey(key)) lock(td_hit_correction) td_hit_correction.Add(key, (hit.mz - bestCf(new double[] { hit.mz, hit.retention_time })) * hit.charge - hit.charge * PROTON_MASS);
                }
            }
            else if (!Lollipop.calibrate_td_results && Lollipop.calibrate_lock_mass)
            {
                foreach (TopDownHit hit in td_hits_calibration.Where(h => h.filename == filename))
                {
                    double correction = 0;
                    try { correction = correctionFactors.Where(c => c.file_name == filename && c.scan_number == hit.scan).First().correction; }
                    catch { }
                    Tuple<string, int, double> key = new Tuple<string, int, double>(filename, hit.scan, hit.reported_mass);
                    if(!td_hit_correction.ContainsKey(key)) lock(td_hit_correction) td_hit_correction.Add(key, (hit.mz - correction) * hit.charge - hit.charge * PROTON_MASS);
                }
            }
        }

        //PROTEOFORM FAMILIES -- see ProteoformCommunity
        public static string family_build_folder_path = "";
        public static int deltaM_edge_display_rounding = 2;
        public static string[] node_positioning = new string[] { "Arbitrary Circle", "Mass X-Axis", "Circle by Mass" };
        public static string[] node_labels = new string[] { "Experimental ID", "Inferred Theoretical ID" };
        public static string[] edge_labels = new string[] { "Mass Difference", "Modification IDs (omits edges with null IDs)" };
        public static List<string> gene_name_labels = new List<string> { "Primary, e.g. HOG1", "Ordered Locus, e.g. YLR113W" };


        //QUANTIFICATION SETUP
        public static int countOfBioRepsInOneCondition; //need this in quantification to select which proteoforms to perform calculations on.
        public static int condition_count;
        public static Dictionary<string, List<int>> ltConditionsBioReps = new Dictionary<string, List<int>>(); //key is the condition and value is the number of bioreps (not the list of bioreps)
        public static Dictionary<string, List<int>> hvConditionsBioReps = new Dictionary<string, List<int>>(); //key is the condition and value is the number of bioreps (not the list of bioreps)
        public static Dictionary<int, List<int>> quantBioFracCombos; //this dictionary has an integer list of bioreps with an integer list of observed fractions. this way we can be missing reps and fractions.
        public static List<Tuple<int, int, double>> normalizationFactors;

        public static void getBiorepsFractionsList(List<InputFile> input_files)  //this should be moved to the appropriate location. somewhere at the start of raw component/end of load component.
        {
            if (!input_files.Any(f => f.purpose == Purpose.Quantification)) return;
            quantBioFracCombos = new Dictionary<int, List<int>>();
            List<int> bioreps = input_files.Where(q => q.purpose == Purpose.Quantification).Select(b => b.biological_replicate).Distinct().ToList();
            List<int> fractions = new List<int>();
            foreach (int b in bioreps)
            {
                fractions = input_files.Where(q => q.purpose == Purpose.Quantification).Where(rep => rep.biological_replicate == b).Select(f => f.fraction).ToList();
                if (fractions != null)
                    fractions = fractions.Distinct().ToList();
                quantBioFracCombos.Add(b, fractions);
            }
        }

        public static void getObservationParameters(bool neucode_labeled, List<InputFile> input_files) //examines the conditions and bioreps to determine the maximum number of observations to require for quantification
        {
            if (!input_files.Any(f => f.purpose == Purpose.Quantification)) return;
            List<string> ltConditions = get_files(input_files, Purpose.Quantification).Select(f => f.lt_condition).Distinct().ToList();
            List<string> hvConditions = neucode_labeled ? 
                get_files(input_files, Purpose.Quantification).Select(f => f.hv_condition).Distinct().ToList() : 
                new List<string>();
            ltConditionsBioReps.Clear();
            hvConditionsBioReps.Clear();

            foreach (string condition in ltConditions)
            {
                //ltConditionsBioReps.Add(condition, Lollipop.get_files(Purpose.Quantification).Where(f => f.lt_condition == condition).Select(b => b.biological_replicate).ToList().Distinct().Count()); // this gives the count of bioreps in the specified condition
                List<int> bioreps = get_files(input_files, Purpose.Quantification).Where(f => f.lt_condition == condition).Select(b => b.biological_replicate).ToList();
                bioreps = bioreps.Distinct().ToList();
                ltConditionsBioReps.Add(condition, bioreps);
            }

            foreach (string condition in hvConditions)
            {
                //hvConditionsBioReps.Add(condition, Lollipop.get_files(Purpose.Quantification).Where(f => f.hv_condition == condition).Select(b => b.biological_replicate).ToList().Distinct().Count()); // this gives the count of bioreps in the specified condition
                List<int> bioreps = get_files(input_files, Purpose.Quantification).Where(f => f.hv_condition == condition).Select(b => b.biological_replicate).ToList();
                bioreps = bioreps.Distinct().ToList();
                hvConditionsBioReps.Add(condition, bioreps);
            }

            condition_count = ltConditions.Count + hvConditions.Count;

            int minLt = ltConditionsBioReps.Values.Min(v => v.Count);
            int minHv = 0;
            if (hvConditionsBioReps.Values.Count() > 0)
            {
                minHv = hvConditionsBioReps.Values.Min(v => v.Count);
                countOfBioRepsInOneCondition = Math.Min(minLt, minHv);
            }
            else
                countOfBioRepsInOneCondition = minLt;
            minBiorepsWithObservations = countOfBioRepsInOneCondition > 0 ? countOfBioRepsInOneCondition : 1;
        }


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
        public static decimal bkgdStDev;
        public static decimal bkgdGaussianHeight;
        public static decimal backgroundShift;
        public static decimal backgroundWidth;
        public static List<ExperimentalProteoform> satisfactoryProteoforms = new List<ExperimentalProteoform>(); // these are proteoforms meeting the required number of observations.
        public static IEnumerable<decimal> permutedTestStatistics;
        public static string[] observation_requirement_possibilities = new string[] { "Minimum Bioreps with Observations From Any Single Condition", "Minimum Bioreps with Observations From Any Condition", "Minimum Bioreps with Observations From Each Condition" };
        public static string observation_requirement = observation_requirement_possibilities[0];
        public static int minBiorepsWithObservations = 1;
        public static decimal selectGaussianHeight;
        public static List<ExperimentalProteoform.quantitativeValues> qVals = new List<ExperimentalProteoform.quantitativeValues>();
        public static decimal sKnot_minFoldChange = 1m;
        public static List<decimal> sortedProteoformTestStatistics = new List<decimal>();
        public static List<decimal> sortedAvgPermutationTestStatistics = new List<decimal>();
        public static decimal offsetTestStatistics = 1m;
        //public static decimal negativeOffsetTestStatistics = -1m;
        public static decimal offsetFDR;

        public static List<ProteinWithGoTerms> observedProteins = new List<ProteinWithGoTerms>();//This is the complete list of proteins included in any accepted proteoform family
        public static List<ProteinWithGoTerms> inducedOrRepressedProteins = new List<ProteinWithGoTerms>();//This is the of proteins from proteoforms that underwent significant induction or repression
        public static decimal minProteoformIntensity = 500000m;
        public static decimal minProteoformFoldChange = 1m;
        public static decimal minProteoformFDR = 0.05m;

        public static void quantify()
        {
            IEnumerable<string> ltconditions = ltConditionsBioReps.Keys;
            IEnumerable<string> hvconditions = hvConditionsBioReps.Keys;
            List<string> conditions = ltconditions.Concat(hvconditions).Distinct().ToList();

            computeBiorepIntensities(proteoform_community.experimental_proteoforms, ltconditions, hvconditions);
            defineAllObservedIntensityDistribution(proteoform_community.experimental_proteoforms, logIntensityHistogram);
            satisfactoryProteoforms = determineProteoformsMeetingCriteria(conditions, proteoform_community.experimental_proteoforms, observation_requirement, minBiorepsWithObservations);
            defineSelectObservedIntensityDistribution(satisfactoryProteoforms, logSelectIntensityHistogram);
            defineBackgroundIntensityDistribution(neucode_labeled, quantBioFracCombos, satisfactoryProteoforms, backgroundShift, backgroundWidth);
            computeProteoformTestStatistics(neucode_labeled, satisfactoryProteoforms, bkgdAverageIntensity, bkgdStDev, numerator_condition, denominator_condition, sKnot_minFoldChange);
            computeSortedTestStatistics(satisfactoryProteoforms);
            offsetFDR = computeFoldChangeFDR(sortedAvgPermutationTestStatistics, sortedProteoformTestStatistics, satisfactoryProteoforms, permutedTestStatistics, offsetTestStatistics);
            computeIndividualExperimentalProteoformFDRs(satisfactoryProteoforms, sortedProteoformTestStatistics, minProteoformFoldChange, minProteoformFDR, minProteoformIntensity);
            observedProteins = getObservedProteins(satisfactoryProteoforms);
            inducedOrRepressedProteins = getInducedOrRepressedProteins(satisfactoryProteoforms, minProteoformFoldChange, minProteoformFDR, minProteoformIntensity);
        }

        public static void computeBiorepIntensities(IEnumerable<ExperimentalProteoform> experimental_proteoforms, IEnumerable<string> ltconditions, IEnumerable<string> hvconditions)
        {
            Parallel.ForEach(experimental_proteoforms, eP => eP.make_biorepIntensityList(eP.lt_quant_components, eP.hv_quant_components, ltconditions, hvconditions));
        }

        public static List<ExperimentalProteoform> determineProteoformsMeetingCriteria(List<string> conditions, IEnumerable<ExperimentalProteoform> experimental_proteoforms, string observation_requirement, int minBiorepsWithObservations)
        {
            List<ExperimentalProteoform> satisfactory_proteoforms = new List<ExperimentalProteoform>();
            if (observation_requirement.Contains("From Any Single Condition"))
                satisfactory_proteoforms = experimental_proteoforms.Where(eP => conditions.Any(c => eP.biorepIntensityList.Where(bc => bc.condition == c).Select(bc => bc.biorep).Distinct().Count() >= minBiorepsWithObservations)).ToList();
            if (observation_requirement.Contains("From Each Condition"))
                satisfactory_proteoforms = experimental_proteoforms.Where(eP => conditions.All(c => eP.biorepIntensityList.Where(bc => bc.condition == c).Select(bc => bc.biorep).Distinct().Count() >= minBiorepsWithObservations)).ToList();
            if (observation_requirement.Contains("From Any Condition"))
                satisfactory_proteoforms = experimental_proteoforms.Where(eP => eP.biorepIntensityList.Select(bc => bc.condition + bc.biorep.ToString()).Distinct().Count() >= minBiorepsWithObservations).ToList();
            return satisfactory_proteoforms;
        }

        public static void defineAllObservedIntensityDistribution(IEnumerable<ExperimentalProteoform> experimental_proteoforms, SortedDictionary<decimal, int> logIntensityHistogram) // the distribution of all observed experimental proteoform biorep intensities
        {
            IEnumerable<decimal> allIntensities = define_intensity_distribution(experimental_proteoforms, logIntensityHistogram).Where(i => i > 1); //these are log2 values
            observedAverageIntensity = allIntensities.Average();
            observedStDev = (decimal)Math.Sqrt(allIntensities.Average(v => Math.Pow((double)(v - observedAverageIntensity), 2))); //population stdev calculation, rather than sample
            observedGaussianArea = get_gaussian_area(logIntensityHistogram);
            observedGaussianHeight = observedGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)observedStDev, 2));
        }

        public static void defineSelectObservedIntensityDistribution(IEnumerable<ExperimentalProteoform> satisfactory_proteoforms, SortedDictionary<decimal, int> logSelectIntensityHistogram)
        {
            IEnumerable<decimal> allRoundedIntensities = define_intensity_distribution(satisfactory_proteoforms, logSelectIntensityHistogram).Where(i => i > 1); //these are log2 values
            selectAverageIntensity = allRoundedIntensities.Average(); 
            selectStDev = (decimal)Math.Sqrt(allRoundedIntensities.Average(v => Math.Pow((double)(v - selectAverageIntensity), 2))); //population stdev calculation, rather than sample
            selectGaussianArea = get_gaussian_area(logSelectIntensityHistogram);
            selectGaussianHeight = selectGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)selectStDev, 2));
        }

        public static List<decimal> define_intensity_distribution(IEnumerable<ExperimentalProteoform> proteoforms, SortedDictionary<decimal, int> histogram)
        {
            histogram.Clear();

            List<decimal> rounded_intensities = (
                from p in proteoforms
                from i in p.biorepIntensityList
                select Math.Round((decimal)Math.Log(i.intensity, 2), 1))
                .ToList();

            foreach (decimal roundedIntensity in rounded_intensities)
            {
                if (histogram.ContainsKey(roundedIntensity))
                    histogram[roundedIntensity]++;
                else
                    histogram.Add(roundedIntensity, 1);
            }

            return rounded_intensities;
        }

        public static decimal get_gaussian_area(SortedDictionary<decimal, int> histogram)
        {
            decimal gaussian_area = 0;
            bool first = true;
            decimal x1 = 0;
            decimal y1 = 0;
            foreach (KeyValuePair<decimal, int> entry in histogram)
            {
                if (first)
                {
                    x1 = entry.Key;
                    y1 = (decimal)entry.Value;
                    first = false;
                }
                else
                {
                    gaussian_area += (entry.Key - x1) * (y1 + ((decimal)entry.Value - y1) / 2);
                    x1 = entry.Key;
                    y1 = (decimal)entry.Value;
                }
            }
            return gaussian_area;
        }

        public static void defineBackgroundIntensityDistribution(bool neucode_labeled, Dictionary<int, List<int>> quantBioFracCombos, List<ExperimentalProteoform> satisfactoryProteoforms, decimal backgroundShift, decimal backgroundWidth)
        {
            bkgdAverageIntensity = selectAverageIntensity + backgroundShift * selectStDev;
            bkgdStDev = selectStDev * backgroundWidth;

            int numMeasurableIntensities = quantBioFracCombos.Keys.Count * condition_count * satisfactoryProteoforms.Count; // all bioreps, all light conditions + all heavy conditions, all satisfactory proteoforms
            int numMeasuredIntensities = satisfactoryProteoforms.Sum(eP => eP.biorepIntensityList.Count); //biorep intensities are created to be unique to the light/heavy + condition + biorep
            int numMissingIntensities = numMeasurableIntensities - numMeasuredIntensities; //this could be negative if there were tons more quantitative intensities

            decimal bkgdGaussianArea = selectGaussianArea / (decimal)numMeasuredIntensities * (decimal)numMissingIntensities;
            bkgdGaussianHeight = bkgdGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)bkgdStDev, 2));
        }

        public static void computeProteoformTestStatistics(bool neucode_labeled, List<ExperimentalProteoform> satisfactoryProteoforms, decimal bkgdAverageIntensity, decimal bkgdStDev, string numerator_condition, string denominator_condition, decimal sKnot_minFoldChange)
        {
            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                eP.quant.determine_biorep_intensities_and_test_statistics(neucode_labeled, eP.biorepIntensityList, bkgdAverageIntensity, bkgdStDev, numerator_condition, denominator_condition, sKnot_minFoldChange);
            }
            qVals = satisfactoryProteoforms.Where(eP => eP.accepted == true).Select(e => e.quant).ToList();
            permutedTestStatistics = satisfactoryProteoforms.SelectMany(eP => eP.quant.permutedTestStatistics);
        }

        public static void computeSortedTestStatistics(List<ExperimentalProteoform> satisfactoryProteoforms)
        {
            sortedProteoformTestStatistics = satisfactoryProteoforms.Select(eP => eP.quant.testStatistic).ToList();
            sortedAvgPermutationTestStatistics = satisfactoryProteoforms.Select(eP => eP.quant.permutedTestStatistics.Average()).ToList();
            sortedProteoformTestStatistics.Sort();
            sortedAvgPermutationTestStatistics.Sort();
        }

        public static decimal computeFoldChangeFDR(List<decimal> sortedAvgPermutationTestStatistics, List<decimal> sortedProteoformTestStatistics, List<ExperimentalProteoform> satisfactoryProteoforms, IEnumerable<decimal> permutedTestStatistics, decimal offsetTestStatistics)
        {
            decimal minimumPositivePassingTestStatistic = sortedProteoformTestStatistics[Enumerable.Range(0, sortedAvgPermutationTestStatistics.Count).FirstOrDefault(i => sortedProteoformTestStatistics[i] >= sortedAvgPermutationTestStatistics[i] + offsetTestStatistics)]; //first time the test statistic exceeds the cap so we're good.
            decimal minimumNegativePassingTestStatistic = sortedProteoformTestStatistics[Enumerable.Range(0, sortedAvgPermutationTestStatistics.Count).LastOrDefault(i => sortedProteoformTestStatistics[i] <= sortedAvgPermutationTestStatistics[i] - offsetTestStatistics)]; //last time the test statistic is below the minimum

            int totalFalsePermutedPositiveValues = permutedTestStatistics.Count(p => p >= minimumPositivePassingTestStatistic);
            int totalFalsePermutedNegativeValues = permutedTestStatistics.Count(p => p <= minimumNegativePassingTestStatistic);

            decimal averagePermuted = (decimal)(totalFalsePermutedPositiveValues + totalFalsePermutedNegativeValues) / (decimal)satisfactoryProteoforms.Count;
            return averagePermuted / ((decimal)(sortedProteoformTestStatistics.Count(s => s >= minimumPositivePassingTestStatistic) + sortedProteoformTestStatistics.Count(s => s <= minimumNegativePassingTestStatistic)));
        }


        public static void computeIndividualExperimentalProteoformFDRs(List<ExperimentalProteoform> satisfactoryProteoforms, List<decimal> sortedProteoformTestStatistics, decimal minProteoformFoldChange, decimal minProteoformFDR, decimal minProteoformIntensity)
        {
            List<List<decimal>> permutedTestStatistics = satisfactoryProteoforms.Select(eP => eP.quant.permutedTestStatistics).ToList();
            Parallel.ForEach(satisfactoryProteoforms, eP =>
            {
                eP.quant.FDR = ExperimentalProteoform.quantitativeValues.computeExperimentalProteoformFDR(eP.quant.testStatistic, permutedTestStatistics, satisfactoryProteoforms.Count, sortedProteoformTestStatistics);
                eP.quant.significant = Math.Abs(eP.quant.logFoldChange) > minProteoformFoldChange && eP.quant.FDR < minProteoformFDR && eP.quant.intensitySum > minProteoformIntensity;
            });
        }

        public static List<ProteinWithGoTerms> getObservedProteins(List<ExperimentalProteoform> satisfactoryProteoforms) // these are all observed proteins in any of the proteoform families.
        {
            return satisfactoryProteoforms.Select(p => p.family).SelectMany(pf => pf.theoretical_proteoforms).SelectMany(t => t.proteinList).ToList();
        }

        public static List<ProteinWithGoTerms> getInducedOrRepressedProteins(List<ExperimentalProteoform> satisfactoryProteoforms, decimal minProteoformAbsLogFoldChange, decimal maxProteoformFDR, decimal minProteoformIntensity)
        {
            return getInterestingProteoforms(satisfactoryProteoforms, minProteoformAbsLogFoldChange, maxProteoformFDR, minProteoformIntensity).Select(p => p.family).SelectMany(pf => pf.theoretical_proteoforms).SelectMany(t => t.proteinList).ToList();
        }

        public static List<ProteoformFamily> getInterestingFamilies(IEnumerable<ExperimentalProteoform> proteoforms, decimal minProteoformFoldChange, decimal minProteoformFDR, decimal minProteoformIntensity)
        {
            return getInterestingProteoforms(proteoforms, minProteoformFoldChange, minProteoformFDR, minProteoformIntensity).Select(e => e.family).ToList();
        }

        public static List<ProteoformFamily> getInterestingFamilies(List<GoTermNumber> go_terms_numbers, List<ProteoformFamily> families)
        {
            return
                (from fam in families
                 from theo in fam.theoretical_proteoforms
                 from p in theo.proteinList
                 from g in p.GoTerms
                 where go_terms_numbers.Select(gtn => gtn.Id).Contains(g.Id)
                 select fam)
                 .Distinct()
                 .ToList();
        }

        public static IEnumerable<ExperimentalProteoform> getInterestingProteoforms(IEnumerable<ExperimentalProteoform> proteoforms, decimal minProteoformAbsLogFoldChange, decimal maxProteoformFDR, decimal minProteoformIntensity)
        {
            return proteoforms.Where(
                p => Math.Abs(p.quant.logFoldChange) > minProteoformAbsLogFoldChange
                && p.quant.FDR < maxProteoformFDR
                && p.quant.intensitySum > minProteoformIntensity);
        }


        // GO TERMS AND GO TERM SIGNIFICANCE
        public static List<GoTermNumber> goTermNumbers = new List<GoTermNumber>();//these are the count and enrichment values
        public static bool allTheoreticalProteins = false; // this sets the group used for background. True if all Proteins in the theoretical database are used. False if only proteins observed in the study are used.
        public static string backgroundProteinsList = "";

        public static void GO_analysis()
        {
            List<ProteinWithGoTerms> backgroundProteinsForGoAnalysis;
            if (backgroundProteinsList != null && backgroundProteinsList != "")
            {
                string[] protein_accessions = File.ReadAllLines(backgroundProteinsList).Select(acc => acc.Trim()).ToArray();
                backgroundProteinsForGoAnalysis = expanded_proteins.Where(p => p.AccessionList.Any(acc => protein_accessions.Contains(acc))).ToList();
            }
            else
            {
                backgroundProteinsForGoAnalysis = allTheoreticalProteins ? expanded_proteins.ToList() : observedProteins;
            }
            goTermNumbers = getGoTermNumbers(inducedOrRepressedProteins, backgroundProteinsForGoAnalysis);
            calculateGoTermFDR(goTermNumbers);
        }

        public static List<GoTermNumber> getGoTermNumbers(List<ProteinWithGoTerms> inducedOrRepressedProteins, List<ProteinWithGoTerms> backgroundProteinSet) //These are only for "interesting proteins", which is the set of proteins induced or repressed beyond a specified fold change, intensity and below FDR.
        {
            Dictionary<string, int> goSignificantCounts = fillGoDictionary(inducedOrRepressedProteins);
            Dictionary<string, int> goBackgroundCounts = fillGoDictionary(backgroundProteinSet);
            return inducedOrRepressedProteins.SelectMany(p => p.GoTerms).DistinctBy(g => g.Id).Select(g => 
                new GoTermNumber(
                    g, 
                    goSignificantCounts.ContainsKey(g.Id) ? goSignificantCounts[g.Id] : 0, 
                    inducedOrRepressedProteins.Count, 
                    goBackgroundCounts.ContainsKey(g.Id) ? goBackgroundCounts[g.Id] : 0, 
                    backgroundProteinSet.Count)
                ).ToList();
        }

        private static Dictionary<string, int> fillGoDictionary(List<ProteinWithGoTerms> proteinSet)
        {
            Dictionary<string, int> goCounts = new Dictionary<string, int>();
            foreach (ProteinWithGoTerms p in proteinSet)
            {
                foreach (string goId in p.GoTerms.Select(g => g.Id).Distinct())
                {
                    if (goCounts.ContainsKey(goId))
                        goCounts[goId]++;
                    else
                        goCounts.Add(goId, 1);
                }
            }
            return goCounts;
        }

        public static void calculateGoTermFDR(List<GoTermNumber> gtns)
        {
            List<double> pvals = gtns.Select(g => g.p_value).ToList();
            pvals.Sort();
            Parallel.ForEach<GoTermNumber>(gtns, g => g.by = GoTermNumber.benjaminiYekutieli(gtns.Count, pvals, g.p_value)); 
        }
    }
}
