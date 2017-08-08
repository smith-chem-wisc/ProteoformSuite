using Accord.Math;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UsefulProteomicsDatabases;

namespace ProteoformSuiteInternal
{
    public class Lollipop
    {

        #region Public Default Constructor

        public Lollipop()
        { }

        #endregion Public Default Constructor

        #region Constants

        public const double MONOISOTOPIC_UNIT_MASS = 1.0023; // updated 161007
        public const double NEUCODE_LYSINE_MASS_SHIFT = 0.036015372;
        public const double PROTON_MASS = 1.007276474;

        #endregion Constants

        #region Input Files

        public List<InputFile> input_files = new List<InputFile>();

        public IEnumerable<InputFile> get_files(IEnumerable<InputFile> files, Purpose purpose)
        {
            return files.Where(f => f.purpose == purpose);
        }

        public IEnumerable<InputFile> get_files(List<InputFile> files, IEnumerable<Purpose> purposes)
        {
            return files.Where(f => purposes.Contains(f.purpose));
        }

        public static string[] file_lists = new string[]
        {
            "Proteoform Identification Results (.xlsx)",
            "Proteoform Quantification Results (.xlsx)",
            "Protein Databases and PTM Lists (.xml, .xml.gz, .fasta, .txt)",
            "Deconvolution Calibration Files (.txt, .tsv)",
        };

        public static List<string>[] acceptable_extensions = new List<string>[]
        {
            new List<string> { ".xlsx" },
            new List<string> { ".xlsx" },
            new List<string> { ".xml", ".gz", ".fasta", ".txt" },
            new List<string> { ".txt", ".tsv" },
        };

        public static string[] file_filters = new string[]
        {
            "Excel Files (*.xlsx) | *.xlsx",
            "Excel Files (*.xlsx) | *.xlsx",
            "Protein Databases and PTM Text Files (*.xml, *.xml.gz, *.fasta, *.txt) | *.xml;*.xml.gz;*.fasta;*.txt",
            "Text Files (*.txt, *.tsv) | *.tsv;*.txt",
        };

        public static List<Purpose>[] file_types = new List<Purpose>[]
        {
            new List<Purpose> { Purpose.Identification },
            new List<Purpose> { Purpose.Quantification },
            new List<Purpose> { Purpose.ProteinDatabase, Purpose.PtmList },
            new List<Purpose> { Purpose.Calibration },
        };

        public void enter_input_files(string[] files, IEnumerable<string> acceptable_extensions, List<Purpose> purposes, List<InputFile> destination, bool user_directed)
        {
            foreach (string complete_path in files)
            {
                FileAttributes a = File.GetAttributes(complete_path);
                if ((a & FileAttributes.Directory) == FileAttributes.Directory)
                {
                    enter_input_files(Directory.GetFiles(complete_path), acceptable_extensions, purposes, destination, true);
                    enter_input_files(Directory.GetDirectories(complete_path), acceptable_extensions, purposes, destination, true);
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
                    if (user_directed) Sweet.add_file_action(file);
                }
            }

            Sweet.update_files_from_presets(destination);
        }

        public void enter_uniprot_ptmlist(string current_directory)
        {
            Loaders.LoadUniprot(Path.Combine(current_directory, "ptmlist.txt"), Loaders.GetFormalChargesDictionary(Loaders.LoadPsiMod(Path.Combine(current_directory, "PSI-MOD.obo2.xml"))));
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(Environment.CurrentDirectory, "ptmlist.txt") }, acceptable_extensions[2], file_types[2], Sweet.lollipop.input_files, true);
        }

        public string match_calibration_files()
        {
            string return_message = "";

            // Look for results files with the same filename as a calibration file, and show that they're matched
            foreach (InputFile file in get_files(input_files, Purpose.Calibration))
            {
                if (input_files.Where(f => f.purpose != Purpose.Calibration).Select(f => f.filename).Contains(file.filename))
                {
                    IEnumerable<InputFile> matching_files = input_files.Where(f => f.purpose != Purpose.Calibration && f.filename == file.filename);
                    InputFile matching_file = matching_files.First();
                    if (matching_files.Count() != 1)
                        return_message += "Warning: There is more than one results file named " + file.filename + ". Will only match calibration to the first one from " + matching_file.purpose.ToString() + "." + Environment.NewLine;
                    file.matchingCalibrationFile = true;
                    matching_file.matchingCalibrationFile = true;
                }
            }

            if (get_files(input_files, Purpose.Calibration).Count() > 0 && !get_files(input_files, Purpose.Calibration).Any(f => f.matchingCalibrationFile))
                return_message += "To use calibration files, please give them the same filenames as the deconvolution results to which they correspond.";

            return return_message;
        }

        #endregion Input Files

        #region RAW EXPERIMENTAL COMPONENTS Public Fields

        public List<Correction> correctionFactors = null;
        public List<Component> raw_experimental_components = new List<Component>();
        public List<Component> raw_quantification_components = new List<Component>();
        public bool neucode_labeled = true;
        public double raw_component_mass_tolerance = 10;
        public int unprocessed_exp_components = 0;
        public int unprocessed_quant_components = 0;
        public int missed_mono_merges_exp = 0;
        public int missed_mono_merges_quant = 0;
        public int harmonic_merges_exp = 0;
        public int harmonic_merges_quant = 0;

        #endregion RAW EXPERIMENTAL COMPONENTS Public Fields

        #region RAW EXPERIMENTAL COMPONENTS

        public void process_raw_components(List<InputFile> input_files, List<Component> destination, Purpose purpose)
        {
            if (get_files(input_files, Purpose.Calibration).Count() > 0)
            {
                correctionFactors = get_files(input_files, Purpose.Calibration).SelectMany(file => Correction.CorrectionFactorInterpolation(read_corrections(file))).ToList();
            }

            Parallel.ForEach(input_files.Where(f => f.purpose == purpose).ToList(), file =>
            {
                List<Component> someComponents = file.reader.read_components_from_xlsx(file, correctionFactors);
                lock (destination) destination.AddRange(someComponents);
            });

            if (neucode_labeled && purpose == Purpose.Identification) process_neucode_components(raw_neucode_pairs);
        }

        public void process_neucode_components(List<NeuCodePair> raw_neucode_pairs)
        {
            foreach (InputFile inputFile in get_files(input_files, Purpose.Identification).ToList())
            {
                foreach (string scan_range in inputFile.reader.scan_ranges)
                {
                    find_neucode_pairs(inputFile.reader.final_components.Where(c => c.scan_range == scan_range), raw_neucode_pairs, heavy_hashed_pairs);
                }
            }
        }

        public IEnumerable<Correction> read_corrections(InputFile file)
        {
            string filepath = file.directory + "\\" + file.filename + file.extension;
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

        #endregion RAW EXPERIMENTAL COMPONENTS

        #region NEUCODE PAIRS Public Fields

        public List<NeuCodePair> raw_neucode_pairs = new List<NeuCodePair>();
        public Dictionary<Component, List<NeuCodePair>> heavy_hashed_pairs = new Dictionary<Component, List<NeuCodePair>>();
        public decimal max_intensity_ratio = 6m;
        public decimal min_intensity_ratio = 1.4m;
        public decimal max_lysine_ct = 26.2m;
        public decimal min_lysine_ct = 1.5m;

        #endregion NEUCODE PAIRS Public Fields

        #region NEUCODE PAIRS

        public List<NeuCodePair> find_neucode_pairs(IEnumerable<Component> components_in_file_scanrange, List<NeuCodePair> destination, Dictionary<Component, List<NeuCodePair>> heavy_hashed_pairs)
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
                                List<int> lower_charges = lower_component.charge_states.Select(charge_state => charge_state.charge_count).ToList();
                                List<int> higher_charges = higher_component.charge_states.Select(charge_states => charge_states.charge_count).ToList();
                                List<int> overlapping_charge_states = lower_charges.Intersect(higher_charges).ToList();
                                lower_component.calculate_sum_intensity_olcs(overlapping_charge_states);
                                higher_component.calculate_sum_intensity_olcs(overlapping_charge_states);
                                double lower_intensity = lower_component.intensity_sum_olcs;
                                double higher_intensity = higher_component.intensity_sum_olcs;
                                bool light_is_lower = true; //calculation different depending on if neucode light is the heavier/lighter component
                                if (lower_intensity > 0 && higher_intensity > 0)
                                {
                                    NeuCodePair pair = lower_intensity > higher_intensity ?
                                        new NeuCodePair(lower_component, higher_component, mass_difference, overlapping_charge_states, light_is_lower) : //lower mass is neucode light
                                        new NeuCodePair(higher_component, lower_component, mass_difference, overlapping_charge_states, !light_is_lower); //higher mass is neucode light

                                    if (pair.weighted_monoisotopic_mass <= pair.neuCodeHeavy.weighted_monoisotopic_mass + MONOISOTOPIC_UNIT_MASS) // the heavy should be at higher mass. Max allowed is 1 dalton less than light.                                    
                                        lock (pairsInScanRange) pairsInScanRange.Add(pair);
                                }
                            }
                        }
                }
            });

            foreach (NeuCodePair pair in pairsInScanRange
                .OrderBy(p => Math.Min(p.neuCodeLight.weighted_monoisotopic_mass, p.neuCodeHeavy.weighted_monoisotopic_mass)) //lower_component
                .ThenBy(p => Math.Max(p.neuCodeLight.weighted_monoisotopic_mass, p.neuCodeHeavy.weighted_monoisotopic_mass)) //higher_component
                .ToList())
            {
                lock (heavy_hashed_pairs)
                {
                    if (!heavy_hashed_pairs.TryGetValue(pair.neuCodeLight, out List<NeuCodePair> these_pairs)
                        || !these_pairs.Any(p => p.neuCodeLight.intensity_sum > pair.neuCodeLight.intensity_sum)) // we found that any component previously used as a heavy, which has higher intensity, is probably correct, and that that component should not get reuused as a light.)
                    {
                        lock (destination)
                            destination.Add(pair);
                        if (heavy_hashed_pairs.TryGetValue(pair.neuCodeHeavy, out List<NeuCodePair> paired))
                            lock (paired)
                                paired.Add(pair);
                        else
                            heavy_hashed_pairs.Add(pair.neuCodeHeavy, new List<NeuCodePair> { pair }); // already locked
                    }

                    else
                    {
                        lock (pairsInScanRange)
                            pairsInScanRange.Remove(pair);
                    }
                }
            }
            return pairsInScanRange;
        }

        #endregion NEUCODE PAIRS

        #region AGGREGATED PROTEOFORMS Public Fields

        public ProteoformCommunity target_proteoform_community = new ProteoformCommunity();
        public Dictionary<string, ProteoformCommunity> decoy_proteoform_communities = new Dictionary<string, ProteoformCommunity>();
        public string decoy_community_name_prefix = "Decoy_Proteoform_Community_";
        public List<Component> remaining_components = new List<Component>();
        public HashSet<Component> remaining_verification_components = new HashSet<Component>();
        public HashSet<Component> remaining_quantification_components = new HashSet<Component>();
        public bool validate_proteoforms = true;
        public double mass_tolerance = 10; //ppm
        public double retention_time_tolerance = 5; //min
        public int maximum_missed_monos = 3;
        public List<int> missed_monoisotopics_range = new List<int>();
        public int maximum_missed_lysines = 2;
        public int min_agg_count = 1;
        public int min_num_CS = 1;
        public int min_num_bioreps = 1;

        #endregion AGGREGATED PROTEOFORMS Public Fields

        #region AGGREGATED PROTEOFORMS Private Fields

        private List<ExperimentalProteoform> vetted_proteoforms = new List<ExperimentalProteoform>();

        private Component[] ordered_components = new Component[0];

        #endregion AGGREGATED PROTEOFORMS Private Fields

        #region AGGREGATED PROTEOFORMS

        public List<ExperimentalProteoform> aggregate_proteoforms(bool two_pass_validation, IEnumerable<NeuCodePair> raw_neucode_pairs, IEnumerable<Component> raw_experimental_components, IEnumerable<Component> raw_quantification_components, int min_num_CS)
        {
            missed_monoisotopics_range = Enumerable.Range(-maximum_missed_monos, maximum_missed_monos * 2 + 1).ToList();
            List<ExperimentalProteoform> candidateExperimentalProteoforms = createProteoforms(raw_neucode_pairs, raw_experimental_components, min_num_CS);
            vetted_proteoforms = two_pass_validation ?
                vetExperimentalProteoforms(candidateExperimentalProteoforms, raw_experimental_components, vetted_proteoforms) :
                candidateExperimentalProteoforms;
            target_proteoform_community.experimental_proteoforms = vetted_proteoforms.ToArray();
            foreach (ProteoformCommunity community in decoy_proteoform_communities.Values)
            {
                community.experimental_proteoforms = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Select(e => new ExperimentalProteoform(e)).ToArray();
            }
            if (neucode_labeled && get_files(input_files, Purpose.Quantification).Count() > 0)
            {
                assignQuantificationComponents(vetted_proteoforms, raw_quantification_components);
            }
            return vetted_proteoforms;
        }

        public void assign_best_components_for_manual_validation(IEnumerable<ExperimentalProteoform> experimental_proteoforms)
        {
            foreach (ExperimentalProteoform pf in experimental_proteoforms)
            {
                pf.manual_validation_id = pf.find_manual_inspection_component(pf.aggregated_components);
                pf.manual_validation_verification = pf.find_manual_inspection_component(pf.lt_verification_components.Concat(pf.hv_verification_components));
                pf.manual_validation_quant = pf.find_manual_inspection_component(pf.lt_quant_components.Concat(pf.hv_quant_components));
            }
        }

        //Rooting each experimental proteoform is handled in addition of each NeuCode pair.
        //If no NeuCodePairs exist, e.g. for an experiment without labeling, the raw components are used instead.
        //Uses an ordered list, so that the proteoform with max intensity is always chosen first
        //raw_neucode_pairs = raw_neucode_pairs.Where(p => p != null).ToList();
        public List<ExperimentalProteoform> createProteoforms(IEnumerable<NeuCodePair> raw_neucode_pairs, IEnumerable<Component> raw_experimental_components, int min_num_CS)
        {
            List<ExperimentalProteoform> candidateExperimentalProteoforms = new List<ExperimentalProteoform>();

            // Only aggregate acceptable components (and neucode pairs). Intensity sum from overlapping charge states includes all charge states if not a neucode pair.
            ordered_components = neucode_labeled ?
                raw_neucode_pairs.OrderByDescending(p => p.intensity_sum_olcs).Where(p => p.accepted == true && p.charge_states.Count >= min_num_CS).ToArray() :
                raw_experimental_components.OrderByDescending(p => p.intensity_sum).Where(p => p.accepted == true && p.num_charge_states >= min_num_CS).ToArray();
            remaining_components = new List<Component>(ordered_components);

            Component root = ordered_components.FirstOrDefault();
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

            for (int i = 0; i < candidateExperimentalProteoforms.Count; i++)
            {
                candidateExperimentalProteoforms[i].accession = "E" + i;
            }

            return candidateExperimentalProteoforms;
        }

        public Component find_next_root(List<Component> ordered, List<Component> running)
        {
            return ordered.FirstOrDefault(c =>
                running.All(d =>
                    c.weighted_monoisotopic_mass < d.weighted_monoisotopic_mass - 20 || c.weighted_monoisotopic_mass > d.weighted_monoisotopic_mass + 20));
        }

        public Component find_next_root(List<Component> ordered, List<ExperimentalProteoform> running)
        {
            return ordered.FirstOrDefault(c =>
                running.All(d =>
                    c.weighted_monoisotopic_mass < d.root.weighted_monoisotopic_mass - 20 || c.weighted_monoisotopic_mass > d.root.weighted_monoisotopic_mass + 20));
        }

        public ExperimentalProteoform find_next_root(List<ExperimentalProteoform> ordered, List<ExperimentalProteoform> running)
        {
            return ordered.FirstOrDefault(e =>
                running.All(f =>
                    e.agg_mass < f.agg_mass - 20 || e.agg_mass > f.agg_mass + 20));
        }

        public List<ExperimentalProteoform> vetExperimentalProteoforms(IEnumerable<ExperimentalProteoform> candidateExperimentalProteoforms, IEnumerable<Component> raw_experimental_components, List<ExperimentalProteoform> vetted_proteoforms) // eliminating candidate proteoforms that were mistakenly created
        {
            List<ExperimentalProteoform> candidates = candidateExperimentalProteoforms.OrderByDescending(p => p.agg_intensity).ToList();
            remaining_verification_components = new HashSet<Component>(raw_experimental_components);

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
                        // e.accepted = true; this is set based on the e properties
                        vetted_proteoforms.Add(e);
                    }
                    foreach (Component c in e.lt_verification_components.Concat(e.hv_verification_components).ToList())
                    {
                        remaining_verification_components.Remove(c); // O(1) complexity with HashSet
                    }
                    candidates.Remove(e);
                }

                running.Clear();
                active.Clear();
                candidate = find_next_root(candidates, running);
            }
            return vetted_proteoforms;
        }

        public List<ExperimentalProteoform> assignQuantificationComponents(List<ExperimentalProteoform> vetted_proteoforms, IEnumerable<Component> raw_quantification_components)  // this is only need for neucode labeled data. quantitative components for unlabelled are assigned elsewhere "vetExperimentalProteoforms"
        {
            List<ExperimentalProteoform> proteoforms = vetted_proteoforms.OrderByDescending(x => x.agg_intensity).ToList();
            remaining_quantification_components = new HashSet<Component>(raw_quantification_components);

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
                    foreach (Component c in e.lt_quant_components.Concat(e.hv_quant_components).ToList())
                    {
                        remaining_quantification_components.Remove(c);
                    }
                    proteoforms.Remove(e);
                }

                running.Clear();
                active.Clear();
                p = find_next_root(proteoforms, running);
            }
            return vetted_proteoforms;
        }

        public void clear_aggregation()
        {
            foreach (ProteoformCommunity community in decoy_proteoform_communities.Values.Concat(new List<ProteoformCommunity> { target_proteoform_community }))
            {
                community.experimental_proteoforms = new ExperimentalProteoform[0];
            }
            Sweet.lollipop.vetted_proteoforms.Clear();
            Sweet.lollipop.ordered_components = new Component[0];
            Sweet.lollipop.remaining_components.Clear();
            Sweet.lollipop.remaining_verification_components.Clear();
            Sweet.lollipop.remaining_quantification_components.Clear();
        }

        #endregion AGGREGATED PROTEOFORMS

        #region THEORETICAL DATABASE Public Fields

        public bool methionine_oxidation = true;
        public bool carbamidomethylation = true;
        public bool methionine_cleavage = true;
        public bool natural_lysine_isotope_abundance = false;
        public bool neucode_light_lysine = true;
        public bool neucode_heavy_lysine = false;
        public int max_ptms = 4;
        public int decoy_databases = 10;
        public int min_peptide_length = 7;
        public double ptmset_mass_tolerance = 0.00001;
        public bool combine_identical_sequences = true;
        public bool combine_theoretical_proteoforms_byMass = true;
        public string[] mod_types_to_exclude = new string[] { "Metal", "PeptideTermMod", "TrypticProduct", "TrypsinDigestedMod" };
        public Dictionary<double, int> modification_ranks = new Dictionary<double, int>();
        public int mod_rank_sum_threshold = 0; // set to the maximum rank of any single modification
        public int mod_rank_first_quartile = 0; // approximate quartiles used for heuristics with unranked modifications
        public int mod_rank_second_quartile = 0;
        public int mod_rank_third_quartile = 0;
        public TheoreticalProteoformDatabase theoretical_database = new TheoreticalProteoformDatabase();

        #endregion THEORETICAL DATABASE Public Fields

        #region ET,ED,EE,EF COMPARISONS Public Fields

        public double ee_max_mass_difference = 1100;
        public double ee_max_RetentionTime_difference = 2.5;
        public double et_low_mass_difference = -300;
        public double et_high_mass_difference = 350;
        public double no_mans_land_lowerBound = 0.22;
        public double no_mans_land_upperBound = 0.88;
        public double peak_width_base_et = 0.03; //need to be separate so you can change one and not other. 
        public double peak_width_base_ee = 0.03;
        public double min_peak_count_et = 4;
        public double min_peak_count_ee = 10;
        public int relation_group_centering_iterations = 2;  // is this just arbitrary? whys is it specified here?
        public List<ProteoformRelation> et_relations = new List<ProteoformRelation>();
        public List<ProteoformRelation> ee_relations = new List<ProteoformRelation>();
        public Dictionary<string, List<ProteoformRelation>> ed_relations = new Dictionary<string, List<ProteoformRelation>>();
        public Dictionary<string, List<ProteoformRelation>> ef_relations = new Dictionary<string, List<ProteoformRelation>>();
        public List<DeltaMassPeak> et_peaks = new List<DeltaMassPeak>();
        public List<DeltaMassPeak> ee_peaks = new List<DeltaMassPeak>();

        public void relate_ed()
        {
            Sweet.lollipop.ed_relations.Clear();
            for (int i = 0; i < Sweet.lollipop.decoy_proteoform_communities.Count; i++)
            {
                string key = decoy_community_name_prefix + i;
                Sweet.lollipop.ed_relations.Add(key, Sweet.lollipop.decoy_proteoform_communities[key].relate(Sweet.lollipop.decoy_proteoform_communities[key].experimental_proteoforms, Sweet.lollipop.decoy_proteoform_communities[key].theoretical_proteoforms, ProteoformComparison.ExperimentalDecoy, true, Environment.CurrentDirectory, true));
                if (i == 0)
                    ProteoformCommunity.count_nearby_relations(Sweet.lollipop.ed_relations[key]); //count from first decoy database (for histogram)
            }

            foreach (ProteoformRelation mass_difference in ed_relations.Values.SelectMany(v => v).ToList())
            {
                foreach (Proteoform p in mass_difference.connected_proteoforms)
                {
                    p.relationships.Add(mass_difference);
                }
            }
        }

        public void relate_ef()
        {
            Sweet.lollipop.ef_relations.Clear();
            for (int i = 0; i < Sweet.lollipop.decoy_proteoform_communities.Count; i++)
            {
                string key = decoy_community_name_prefix + i;
                Sweet.lollipop.ef_relations.Add(key, Sweet.lollipop.decoy_proteoform_communities[key].relate_ef(Sweet.lollipop.decoy_proteoform_communities[key].experimental_proteoforms, Sweet.lollipop.decoy_proteoform_communities[key].experimental_proteoforms));
                if (i == 0)
                    ProteoformCommunity.count_nearby_relations(Sweet.lollipop.ef_relations[key]); //count from first decoy database (for histogram)
            }

            foreach (ProteoformRelation mass_difference in ef_relations.Values.SelectMany(v => v).ToList())
            {
                foreach (Proteoform p in mass_difference.connected_proteoforms)
                {
                    p.relationships.Add(mass_difference);
                }
            }
        }

        public void change_peak_acceptance(DeltaMassPeak peak, bool accepted, bool add_action)
        {
            peak.Accepted = accepted;
            Parallel.ForEach(peak.grouped_relations, r => r.Accepted = accepted);

            if (peak.RelationType == ProteoformComparison.ExperimentalTheoretical)
                Parallel.ForEach(Sweet.lollipop.ed_relations.Values.SelectMany(v => v).Where(r => r.peak != null), pRelation => pRelation.Accepted = pRelation.peak.Accepted);
            else
                Parallel.ForEach(Sweet.lollipop.ef_relations.Values.SelectMany(v => v).Where(r => r.peak != null), pRelation => pRelation.Accepted = pRelation.peak.Accepted);

            if (accepted && add_action)
                Sweet.accept_peak_action(peak);
            else if (add_action)
                Sweet.unaccept_peak_action(peak);
        }

        #endregion ET,ED,EE,EF COMPARISONS Public Fields

        #region PROTEOFORM FAMILIES Public Fields

        public string family_build_folder_path = "";

        public int deltaM_edge_display_rounding = 2;

        public static string[] node_positioning = new string[]
        {
            "Arbitrary Circle",
            "Mass-Based Spiral",
            "Circle by Mass",
            //"Mass X-Axis" 
        };

        public static string[] node_labels = new string[]
        {
            "Experimental ID",
            "Inferred Theoretical ID"
        };

        public static string[] edge_labels = new string[]
        {
            "Mass Difference",
            "Modification IDs (omits edges with null IDs)"
        };

        public static List<string> gene_name_labels = new List<string>
        {
            "Primary, e.g. HOG1",
            "Ordered Locus, e.g. YLR113W"
        };

        public string[] likely_cleavages = new string[]
        {
            "I",
            "L",
            "A"
        };

        public void construct_target_and_decoy_families()
        {
            target_proteoform_community.construct_families();
            foreach (var decoys in decoy_proteoform_communities.Values) decoys.construct_families();
        }

        #endregion PROTEOFORM FAMILIES Public Fields

        #region QUANTIFICATION SETUP Public Fields

        public int countOfBioRepsInOneCondition; //need this in quantification to select which proteoforms to perform calculations on.
        public int condition_count;
        public Dictionary<string, List<string>> conditionsBioReps = new Dictionary<string, List<string>>();
        public Dictionary<string, List<string>> ltConditionsBioReps = new Dictionary<string, List<string>>(); //key is the condition and value is the number of bioreps (not the list of bioreps)
        public Dictionary<string, List<string>> hvConditionsBioReps = new Dictionary<string, List<string>>(); //key is the condition and value is the number of bioreps (not the list of bioreps)
        public Dictionary<string, List<string>> quantBioFracCombos = new Dictionary<string, List<string>>(); //this dictionary has an integer list of bioreps with an integer list of observed fractions. this way we can be missing reps and fractions.

        #endregion QUANTIFICATION SETUP Public Fields

        #region QUANTIFICATION SETUP

        public void getConditionBiorepFractionLabels(bool neucode_labeled, List<InputFile> input_files) //examines the conditions and bioreps to determine the maximum number of observations to require for quantification
        {
            if (!input_files.Any(f => f.purpose == Purpose.Quantification))
                return;

            List<string> ltConditions = get_files(input_files, Purpose.Quantification).Select(f => f.lt_condition).Distinct().ToList();
            List<string> hvConditions = neucode_labeled ?
                get_files(input_files, Purpose.Quantification).Select(f => f.hv_condition).Distinct().ToList() :
                new List<string>();
            conditionsBioReps.Clear();
            ltConditionsBioReps.Clear();
            hvConditionsBioReps.Clear();

            foreach (string condition in ltConditions.Concat(hvConditions).Distinct().ToList())
            {
                List<string> allbioreps = get_files(input_files, Purpose.Quantification).Where(f => f.lt_condition == condition || f.hv_condition == condition).Select(b => b.biological_replicate).Distinct().ToList();
                conditionsBioReps.Add(condition, allbioreps);
            }

            foreach (string condition in ltConditions)
            {
                List<string> ltbioreps = get_files(input_files, Purpose.Quantification).Where(f => f.lt_condition == condition).Select(b => b.biological_replicate).Distinct().ToList();
                ltConditionsBioReps.Add(condition, ltbioreps);
            }

            foreach (string condition in hvConditions)
            {
                List<string> hvbioreps = get_files(input_files, Purpose.Quantification).Where(f => f.hv_condition == condition).Select(b => b.biological_replicate).Distinct().ToList();
                hvConditionsBioReps.Add(condition, hvbioreps);
            }

            condition_count = conditionsBioReps.Count;

            int minLt = ltConditionsBioReps.Values.Min(v => v.Count);

            int minHv = hvConditionsBioReps.Values.Count > 0 ?
                hvConditionsBioReps.Values.Min(v => v.Count) :
                0;

            countOfBioRepsInOneCondition = conditionsBioReps.Min(kv => kv.Value.Count);
            minBiorepsWithObservations = minBiorepsWithObservations > 0 ? 
                minBiorepsWithObservations : // keep presets (default is -1, which should be erased)
                    countOfBioRepsInOneCondition > 0 ? 
                        countOfBioRepsInOneCondition : 
                        1;

            //getBiorepsFractionsList
            List<string> bioreps = input_files.Where(q => q.purpose == Purpose.Quantification).Select(b => b.biological_replicate).Distinct().ToList();
            quantBioFracCombos = bioreps.ToDictionary(b => b, b => input_files.Where(q => q.purpose == Purpose.Quantification && q.biological_replicate == b).Select(f => f.fraction).Distinct().ToList());
        }

        #endregion QUANTIFICATION SETUP

        #region QUANTIFICATION Public Fields

        // Condition labels
        public string numerator_condition = ""; // numerator for fold change calculation
        public string denominator_condition = ""; // denominator for fold change calculation
        public string induced_condition = ""; // induced condition for relative difference calculation

        // Selecting proteoforms for quantification
        public static string[] observation_requirement_possibilities = new string[] 
        {
            "Minimum Bioreps with Observations From Any Single Condition",
            "Minimum Bioreps with Observations From Any Condition",
            "Minimum Bioreps with Observations From Each Condition",
            "Minimum Biorep+Techreps with Observations From Any Single Condition",
            "Minimum Biorep+Techreps with Observations From Any Condition",
            "Minimum Biorep+Techreps with Observations From Each Condition",
        };
        public string observation_requirement = observation_requirement_possibilities[0];
        public int minBiorepsWithObservations = -1;
        public List<ExperimentalProteoform> satisfactoryProteoforms = new List<ExperimentalProteoform>(); // these are proteoforms meeting the required number of observations.
        public List<QuantitativeProteoformValues> qVals = new List<QuantitativeProteoformValues>(); // quantitative values associated with each selected proteoform
        public bool significance_by_log2FC = false;
        public bool significance_by_permutation = true;

        // Imputation
        public decimal backgroundShift = -1.8m;
        public decimal backgroundWidth = 0.5m;
        public bool useRandomSeed = false;
        public int randomSeed = 1;

        // Log2FC statistics
        public Log2FoldChangeAnalysis Log2FoldChangeAnalysis = new Log2FoldChangeAnalysis();

        // Relative difference calculations with balanced permutations
        public TusherAnalysis1 TusherAnalysis1 = new TusherAnalysis1();
        public TusherAnalysis2 TusherAnalysis2 = new TusherAnalysis2();
        public List<ProteinWithGoTerms> observedProteins = new List<ProteinWithGoTerms>(); // This is the complete list of observed proteins
        public List<ProteinWithGoTerms> quantifiedProteins = new List<ProteinWithGoTerms>(); // This is the complete list of proteins that were quantified and included in any accepted proteoform family
        public decimal offsetTestStatistics = 1m; // offset from expected relative differences (avgSortedPermutationRelativeDifferences); used to call significance with minimumPassingNegativeTestStatistic & minimumPassingPositiveTestStatisitic
        public decimal sKnot_minFoldChange = 1m; // this is used in the original paper to correct for artificially high relative differences calculated at low intensities. Mass spec intensities are high enough in general that this is not a problem, so this is not customized by the user.
        public bool useFoldChangeCutoff = false;
        public decimal foldChangeCutoff = 1.5m;
        public Random seeded;

        // Permutation fold change criteria
        public static string[] fold_change_conjunction_options = new string[] 
        {
            "AND",
            "OR"
        };
        public string fold_change_conjunction = fold_change_conjunction_options[0];
        public int minBiorepsWithFoldChange = -1;
        public bool useAveragePermutationFoldChange = true;
        public bool useBiorepPermutationFoldChange = false;

        // "Local FDR" calculated using the relative difference of each proteoform as both minimumPassingNegativeTestStatistic & minimumPassingPositiveTestStatisitic. This is an unofficial extension of the statisitical analysis above.
        public bool useLocalFdrCutoff = false;
        public decimal localFdrCutoff = 0.05m;

        #endregion QUANTIFICATION Public Fields

        #region QUANTIFICATION

        public void quantify()
        {
            IEnumerable<string> ltconditions = ltConditionsBioReps.Keys;
            IEnumerable<string> hvconditions = hvConditionsBioReps.Keys;
            List<string> conditions = ltconditions.Concat(hvconditions).Distinct().ToList();

            if (useRandomSeed)
                seeded = new Random(randomSeed);

            computeBiorepIntensities(target_proteoform_community.experimental_proteoforms, ltconditions, hvconditions);
            satisfactoryProteoforms = determineProteoformsMeetingCriteria(conditions, target_proteoform_community.experimental_proteoforms, observation_requirement, minBiorepsWithObservations);
            observedProteins = getProteins(target_proteoform_community.experimental_proteoforms.Where(x => x.accepted));
            quantifiedProteins = getProteins(satisfactoryProteoforms);
            qVals = satisfactoryProteoforms.Where(pf => pf.accepted).Select(pf => pf.quant).ToList();

            TusherAnalysis1.QuantitativeDistributions.defineAllObservedIntensityDistribution(target_proteoform_community.experimental_proteoforms, TusherAnalysis1.QuantitativeDistributions.logIntensityHistogram);
            TusherAnalysis1.QuantitativeDistributions.defineSelectObservedIntensityDistribution(satisfactoryProteoforms, TusherAnalysis1.QuantitativeDistributions.logSelectIntensityHistogram);
            TusherAnalysis1.QuantitativeDistributions.defineBackgroundIntensityDistribution(quantBioFracCombos, satisfactoryProteoforms, condition_count, backgroundShift, backgroundWidth);

            TusherAnalysis2.QuantitativeDistributions.defineAllObservedIntensityDistribution(target_proteoform_community.experimental_proteoforms, TusherAnalysis2.QuantitativeDistributions.logIntensityHistogram);
            TusherAnalysis2.QuantitativeDistributions.defineSelectObservedIntensityDistribution(satisfactoryProteoforms, TusherAnalysis2.QuantitativeDistributions.logSelectIntensityHistogram);
            TusherAnalysis2.QuantitativeDistributions.defineBackgroundIntensityDistribution(quantBioFracCombos, satisfactoryProteoforms, condition_count, backgroundShift, backgroundWidth);

            TusherAnalysis1.compute_proteoform_statistics(satisfactoryProteoforms, TusherAnalysis1.QuantitativeDistributions.bkgdAverageIntensity, TusherAnalysis1.QuantitativeDistributions.bkgdStDev, conditionsBioReps, numerator_condition, denominator_condition, induced_condition, sKnot_minFoldChange, true); // includes normalization
            TusherAnalysis1.permutedRelativeDifferences = TusherAnalysis1.compute_balanced_biorep_permutation_relativeDifferences(conditionsBioReps, induced_condition, satisfactoryProteoforms, sKnot_minFoldChange);
            TusherAnalysis1.flattenedPermutedRelativeDifferences = TusherAnalysis1.permutedRelativeDifferences.SelectMany(x => x).ToList();
            TusherAnalysis1.computeSortedRelativeDifferences(satisfactoryProteoforms, TusherAnalysis1.permutedRelativeDifferences);
            TusherAnalysis1.relativeDifferenceFDR = TusherAnalysis1.computeRelativeDifferenceFDR(TusherAnalysis1.avgSortedPermutationRelativeDifferences, TusherAnalysis1.sortedProteoformRelativeDifferences, satisfactoryProteoforms, TusherAnalysis1.flattenedPermutedRelativeDifferences, offsetTestStatistics);
            TusherAnalysis1.computeIndividualExperimentalProteoformFDRs(satisfactoryProteoforms, TusherAnalysis1.flattenedPermutedRelativeDifferences, TusherAnalysis1.sortedProteoformRelativeDifferences);
            TusherAnalysis1.inducedOrRepressedProteins = getInducedOrRepressedProteins(TusherAnalysis1 as TusherAnalysis, satisfactoryProteoforms, TusherAnalysis1.GoAnalysis.minProteoformFoldChange, TusherAnalysis1.GoAnalysis.maxGoTermFDR, TusherAnalysis1.GoAnalysis.minProteoformIntensity);

            TusherAnalysis2.compute_proteoform_statistics(satisfactoryProteoforms, TusherAnalysis2.QuantitativeDistributions.bkgdAverageIntensity, TusherAnalysis2.QuantitativeDistributions.bkgdStDev, conditionsBioReps, numerator_condition, denominator_condition, induced_condition, sKnot_minFoldChange, true); // includes normalization
            TusherAnalysis2.permutedRelativeDifferences = TusherAnalysis2.compute_balanced_biorep_permutation_relativeDifferences(conditionsBioReps, input_files, induced_condition, satisfactoryProteoforms, sKnot_minFoldChange);
            TusherAnalysis2.flattenedPermutedRelativeDifferences = TusherAnalysis2.permutedRelativeDifferences.SelectMany(x => x).ToList();
            TusherAnalysis2.computeSortedRelativeDifferences(satisfactoryProteoforms, TusherAnalysis2.permutedRelativeDifferences);
            TusherAnalysis2.relativeDifferenceFDR = TusherAnalysis2.computeRelativeDifferenceFDR(TusherAnalysis2.avgSortedPermutationRelativeDifferences, TusherAnalysis2.sortedProteoformRelativeDifferences, satisfactoryProteoforms, TusherAnalysis2.flattenedPermutedRelativeDifferences, offsetTestStatistics);
            TusherAnalysis2.computeIndividualExperimentalProteoformFDRs(satisfactoryProteoforms, TusherAnalysis2.flattenedPermutedRelativeDifferences, TusherAnalysis2.sortedProteoformRelativeDifferences);
            TusherAnalysis2.inducedOrRepressedProteins = getInducedOrRepressedProteins(TusherAnalysis2 as TusherAnalysis, satisfactoryProteoforms, TusherAnalysis2.GoAnalysis.minProteoformFoldChange, TusherAnalysis2.GoAnalysis.maxGoTermFDR, TusherAnalysis2.GoAnalysis.minProteoformIntensity);
        }

        public void computeBiorepIntensities(IEnumerable<ExperimentalProteoform> experimental_proteoforms, IEnumerable<string> ltconditions, IEnumerable<string> hvconditions)
        {
            Parallel.ForEach(experimental_proteoforms, eP => eP.make_biorepIntensityList(eP.lt_quant_components, eP.hv_quant_components, ltconditions, hvconditions));
        }

        public List<ExperimentalProteoform> determineProteoformsMeetingCriteria(List<string> conditions, IEnumerable<ExperimentalProteoform> experimental_proteoforms, string observation_requirement, int minBiorepsWithObservations)
        {
            List<ExperimentalProteoform> satisfactory_proteoforms = new List<ExperimentalProteoform>();

            if (observation_requirement.Contains("Bioreps") && observation_requirement.Contains("From Any Single Condition"))
                satisfactory_proteoforms = experimental_proteoforms.Where(eP => conditions.Any(c => eP.biorepIntensityList.Where(bc => bc.condition == c).Select(bc => bc.biorep).Distinct().Count() >= minBiorepsWithObservations)).ToList();
            if (observation_requirement.Contains("Bioreps") && observation_requirement.Contains("From Each Condition"))
                satisfactory_proteoforms = experimental_proteoforms.Where(eP => conditions.All(c => eP.biorepIntensityList.Where(bc => bc.condition == c).Select(bc => bc.biorep).Distinct().Count() >= minBiorepsWithObservations)).ToList();
            if (observation_requirement.Contains("Bioreps") && observation_requirement.Contains("From Any Condition"))
                satisfactory_proteoforms = experimental_proteoforms.Where(eP => eP.biorepIntensityList.Select(bc => bc.condition + bc.biorep.ToString()).Distinct().Count() >= minBiorepsWithObservations).ToList();

            if (observation_requirement.Contains("Biorep+Techreps") && observation_requirement.Contains("From Any Single Condition"))
                satisfactory_proteoforms = experimental_proteoforms.Where(eP => conditions.Any(c => eP.biorepTechrepIntensityList.Where(bc => bc.condition == c).Select(bc => bc.biorep + bc.techrep).Distinct().Count() >= minBiorepsWithObservations)).ToList();
            if (observation_requirement.Contains("Biorep+Techreps") && observation_requirement.Contains("From Each Condition"))
                satisfactory_proteoforms = experimental_proteoforms.Where(eP => conditions.All(c => eP.biorepTechrepIntensityList.Where(bc => bc.condition == c).Select(bc => bc.biorep + bc.techrep).Distinct().Count() >= minBiorepsWithObservations)).ToList();
            if (observation_requirement.Contains("Biorep+Techreps") && observation_requirement.Contains("From Any Condition"))
                satisfactory_proteoforms = experimental_proteoforms.Where(eP => eP.biorepTechrepIntensityList.Select(bc => bc.condition + bc.biorep + bc.techrep).Distinct().Count() >= minBiorepsWithObservations).ToList();

            return satisfactory_proteoforms;
        }

        public List<ProteinWithGoTerms> getProteins(IEnumerable<ExperimentalProteoform> proteoforms) // these are all observed proteins in any of the proteoform families.
        {
            return proteoforms
                .Select(p => p.family)
                .SelectMany(pf => pf.theoretical_proteoforms)
                .SelectMany(t => t.ExpandedProteinList)
                .DistinctBy(pwg => pwg.Accession.Split('_')[0])
                .ToList();
        }

        public IEnumerable<ExperimentalProteoform> getInterestingProteoforms(TusherAnalysis analysis, IEnumerable<ExperimentalProteoform> proteoforms, decimal minProteoformAbsLogFoldChange, decimal maxProteoformFDR, decimal minProteoformIntensity)
        {
            return proteoforms.Where(p =>
                (significance_by_permutation && analysis != null && (analysis as TusherAnalysis1 != null ? p.quant.TusherValues1.significant : p.quant.TusherValues2.significant) 
                || significance_by_log2FC && p.quant.Log2FoldChangeValues.significant)
                && Math.Abs(p.quant.logFoldChange) > minProteoformAbsLogFoldChange
                && p.quant.intensitySum > minProteoformIntensity);
        }

        public List<ProteinWithGoTerms> getInducedOrRepressedProteins(TusherAnalysis analysis, IEnumerable<ExperimentalProteoform> satisfactoryProteoforms, decimal minProteoformAbsLogFoldChange, decimal maxProteoformFDR, decimal minProteoformIntensity)
        {
            return getInterestingProteoforms(analysis, satisfactoryProteoforms, minProteoformAbsLogFoldChange, maxProteoformFDR, minProteoformIntensity)
                .Where(pf => pf.linked_proteoform_references != null && pf.linked_proteoform_references.FirstOrDefault() as TheoreticalProteoform != null)
                .Select(pf => pf.linked_proteoform_references.First() as TheoreticalProteoform)
                .SelectMany(t => t.ExpandedProteinList)
                .DistinctBy(pwg => pwg.Accession.Split('_')[0])
                .ToList();
        }

        public List<ProteoformFamily> getInterestingFamilies(TusherAnalysis analysis, IEnumerable<ExperimentalProteoform> proteoforms, decimal minProteoformFoldChange, decimal minProteoformFDR, decimal minProteoformIntensity)
        {
            return getInterestingProteoforms(analysis, proteoforms, minProteoformFoldChange, minProteoformFDR, minProteoformIntensity)
                .Select(e => e.family).ToList();
        }

        public List<ProteoformFamily> getInterestingFamilies(List<GoTermNumber> go_terms_numbers, List<ProteoformFamily> families)
        {
            return
                (from fam in families
                 from theo in fam.theoretical_proteoforms
                 from p in theo.ExpandedProteinList
                 from g in p.GoTerms
                 where go_terms_numbers.Select(gtn => gtn.Id).Contains(g.Id)
                 select fam)
                 .Distinct()
                 .ToList();
        }

        #endregion QUANTIFICATION

        #region RESULTS Public Field

        public string results_folder = "";

        #endregion RESULTS Public Field

        #region CLEAR METHODS

        public void clear_et()
        {
            Sweet.lollipop.et_relations.Clear();
            Sweet.lollipop.et_peaks.Clear();
            Sweet.lollipop.ed_relations.Clear();
            Sweet.lollipop.target_proteoform_community.families.Clear();
            Sweet.lollipop.decoy_proteoform_communities.Values.Select(c => c.families).ToList().Clear();

            foreach (ProteoformCommunity community in decoy_proteoform_communities.Values.Concat(new List<ProteoformCommunity> { target_proteoform_community }))
            {
                foreach (Proteoform p in community.experimental_proteoforms)
                {
                    p.relationships.RemoveAll(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical || r.RelationType == ProteoformComparison.ExperimentalDecoy);
                    p.family = null;
                    p.ptm_set = new PtmSet(new List<Ptm>());
                    p.linked_proteoform_references = null;
                    p.gene_name = null;
                }

                foreach (Proteoform p in community.theoretical_proteoforms)
                {
                    p.relationships.RemoveAll(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical || r.RelationType == ProteoformComparison.ExperimentalDecoy);
                    p.family = null;
                }

                et_peaks.RemoveAll(k => k.RelationType == ProteoformComparison.ExperimentalTheoretical || k.RelationType == ProteoformComparison.ExperimentalDecoy);
            }
        }

        public void clear_ee()
        {
            Sweet.lollipop.ee_relations.Clear();
            Sweet.lollipop.ee_peaks.Clear();
            Sweet.lollipop.ef_relations.Clear();
            foreach (ProteoformCommunity community in decoy_proteoform_communities.Values.Concat(new List<ProteoformCommunity> { target_proteoform_community }))
            {
                community.families.Clear();
                foreach (Proteoform p in community.experimental_proteoforms)
                {
                    p.relationships.RemoveAll(r => r.RelationType == ProteoformComparison.ExperimentalExperimental || r.RelationType == ProteoformComparison.ExperimentalFalse);
                    p.family = null;
                    p.ptm_set = new PtmSet(new List<Ptm>());
                    p.linked_proteoform_references = null;
                    p.gene_name = null;
                }

                ee_peaks.RemoveAll(k => k.RelationType == ProteoformComparison.ExperimentalExperimental || k.RelationType == ProteoformComparison.ExperimentalFalse);
            }
        }

        public void clear_all_families()
        {
            foreach (ProteoformCommunity community in decoy_proteoform_communities.Values.Concat(new List<ProteoformCommunity> { target_proteoform_community }))
            {
                community.clear_families();
            }
        }

        #endregion CLEAR METHODS

    }
}

