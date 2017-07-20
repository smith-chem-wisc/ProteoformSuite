using Accord.Math;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UsefulProteomicsDatabases;
using Chemistry;
using MassSpectrometry;
using IO.Thermo;

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
            "Top-Down Results (Unlabeled) (.xlsx)",
            "Bottom-Up Results MzIdentML (.mzid)",
            "Uncalibrated Proteoform Identification Results (.xlsx)",
            "Raw Files (.raw)",
             "Uncalibrated Top-Down Results (Unlabeled) (.xlsx)"
        };

        public static List<string>[] acceptable_extensions = new List<string>[]
        {
            new List<string> { ".xlsx" },
            new List<string> { ".xlsx" },
            new List<string> { ".xml", ".gz", ".fasta", ".txt" },
            new List<string> { ".xlsx" },
            new List<string> { ".mzid" },
            new List<string> { ".xlsx" },
            new List<string> {".raw"},
            new List<string> { ".xlsx" }

        };

        public static string[] file_filters = new string[]
        {
            "Excel Files (*.xlsx) | *.xlsx",
            "Excel Files (*.xlsx) | *.xlsx",
            "Protein Databases and PTM Text Files (*.xml, *.xml.gz, *.fasta, *.txt) | *.xml;*.xml.gz;*.fasta;*.txt",
            "Excel Files (*.xlsx) | *.xlsx",
            "MZIdentML Files (*.mzid) | *.mzid",
            "Excel Files (*.xlsx) | *.xlsx",
            "Raw Files (*.raw) | *.raw",
            "Excel Files (*.xlsx) | *.xlsx",
  
        };

        public static List<Purpose>[] file_types = new List<Purpose>[]
        {
            new List<Purpose> { Purpose.Identification },
            new List<Purpose> { Purpose.Quantification },
            new List<Purpose> { Purpose.ProteinDatabase, Purpose.PtmList },
            new List<Purpose> { Purpose.TopDown },
            new List<Purpose> { Purpose.BottomUp },
            new List<Purpose> { Purpose.CalibrationIdentification },
            new List<Purpose> {Purpose.RawFile },
            new List<Purpose> { Purpose.CalibrationTopDown }

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
       

        #endregion Input Files

        #region RAW EXPERIMENTAL COMPONENTS Public Fields

        public List<Component> raw_experimental_components = new List<Component>();
        public List<Component> raw_quantification_components = new List<Component>();
        public bool neucode_labeled = true;

        #endregion RAW EXPERIMENTAL COMPONENTS Public Fields

        #region RAW EXPERIMENTAL COMPONENTS

        public void process_raw_components(List<InputFile> input_files, List<Component> destination, Purpose purpose, bool remove_missed_monos_and_harmonics)
        {
            Parallel.ForEach(input_files.Where(f => f.purpose == purpose).ToList(), file =>
            {
                List<Component> someComponents = file.reader.read_components_from_xlsx(file, remove_missed_monos_and_harmonics);
                lock (destination) destination.AddRange(someComponents);
            });

            if (neucode_labeled && purpose == Purpose.Identification) process_neucode_components(raw_neucode_pairs);
        }

        private void process_neucode_components(List<NeuCodePair> raw_neucode_pairs)
        {
            foreach (InputFile inputFile in get_files(input_files, Purpose.Identification).ToList())
            {
                foreach (string scan_range in inputFile.reader.scan_ranges)
                {
                    find_neucode_pairs(inputFile.reader.final_components.Where(c => c.scan_range == scan_range), raw_neucode_pairs, heavy_hashed_pairs);
                }
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
            target_proteoform_community.community_number = -100;
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
            ordered_components = new Component[0];
            remaining_components.Clear();
            remaining_verification_components.Clear();
            vetted_proteoforms.Clear();

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

        //Could be improved. Used for manual mass shifting.
        //Idea 1: Start with Components -- have them find the most intense nearby component. Then, go through and correct edge cases that aren't correct.
        //Idea 2: Use the assumption that proteoforms distant to the manual shift will not regroup.
        //Idea 2.1: Put the shifted proteoforms, plus some range from the min and max masses in there, and reaggregate the components with the aggregate_proteoforms algorithm.
        public List<ExperimentalProteoform> regroup_components(bool neucode_labeled, bool two_pass_validation, IEnumerable<InputFile> input_files, List<NeuCodePair> raw_neucode_pairs, IEnumerable<Component> raw_experimental_components, IEnumerable<Component> raw_quantification_components, int min_num_CS)
        {
            if (neucode_labeled)
            {
                raw_neucode_pairs.Clear();
                process_neucode_components(raw_neucode_pairs);
            }
            List<ExperimentalProteoform> new_exps = aggregate_proteoforms(two_pass_validation, raw_neucode_pairs, raw_experimental_components, raw_quantification_components, min_num_CS);
            assign_best_components_for_manual_validation(new_exps);
            return new_exps;
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
        public int decoy_databases = 1;
        public int min_peptide_length = 7;
        public double ptmset_mass_tolerance = 0.00001;
        public bool combine_identical_sequences = true;
        public bool combine_theoretical_proteoforms_byMass = true;
        public string[] mod_types_to_exclude = new string[] { "Metal", "PeptideTermMod", "TrypticProduct" };
        public Dictionary<double, int> modification_ranks = new Dictionary<double, int>();
        public int mod_rank_sum_threshold = 0; // set to the maximum rank of any single modification
        public int mod_rank_first_quartile = 0; // approximate quartiles used for heuristics with unranked modifications
        public int mod_rank_second_quartile = 0;
        public int mod_rank_third_quartile = 0;
        public TheoreticalProteoformDatabase theoretical_database = new TheoreticalProteoformDatabase();
        public List<BottomUpPSM> BottomUpPSMList = new List<BottomUpPSM>();
        #endregion THEORETICAL DATABASE Public Fields

        #region ET,ED,EE,EF COMPARISONS Public Fields

        public double ee_max_mass_difference = 300;
        public double ee_max_RetentionTime_difference = 2.5;
        public double et_low_mass_difference = -300;
        public double et_high_mass_difference = 350;
        public double no_mans_land_lowerBound = 0.22;
        public double no_mans_land_upperBound = 0.88;
        public double peak_width_base_et = 0.03; //need to be separate so you can change one and not other. 
        public double peak_width_base_ee = 0.03;
        public double min_peak_count_et = 4;
        public double min_peak_count_ee = 10;
        public bool limit_theoreticals_to_BU_or_TD_observed = false;
        public int relation_group_centering_iterations = 2;  // is this just arbitrary? whys is it specified here?
        public List<ProteoformRelation> et_relations = new List<ProteoformRelation>();
        public List<ProteoformRelation> ee_relations = new List<ProteoformRelation>();
        public Dictionary<string, List<ProteoformRelation>> ed_relations = new Dictionary<string, List<ProteoformRelation>>();
        public Dictionary<string, List<ProteoformRelation>> ef_relations = new Dictionary<string, List<ProteoformRelation>>();
        public List<DeltaMassPeak> et_peaks = new List<DeltaMassPeak>();
        public List<DeltaMassPeak> ee_peaks = new List<DeltaMassPeak>();
        public List<ProteoformRelation> td_relations = new List<ProteoformRelation>(); //td data

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

        #region TOPDOWN 
        public double min_RT_td = 40.0;
        public double max_RT_td = 90.0;
        public double min_score_td = 3.0;
        public bool biomarker = true;
        public bool tight_abs_mass = true;
        public List<TopDownHit> top_down_hits = new List<TopDownHit>();
        public TopDownReader topdownReader = new TopDownReader();
        //C-score > 40: proteoform is both identified and fully characterized; 
        //3 ≤ Cscore≤ 40: proteoform is identified, but only partially characterized; 
        //C-score < 3: proteoform is neither identified nor characterized.

        public void read_in_td_hits()
        {
            topdownReader.topdown_ptms.Clear();
            foreach (InputFile file in input_files.Where(f => f.purpose == Purpose.TopDown).ToList())
            {
                top_down_hits.AddRange(topdownReader.ReadTDFile(file));
            }
        }

        public List<TopDownProteoform> AggregateTdHits(List<TopDownHit> top_down_hits)
        {
            //group hits into topdown proteoforms by accession/theoretical AND observed mass
            List<TopDownProteoform> topdown_proteoforms = new List<TopDownProteoform>();
            List<TopDownHit> unprocessed_td_hits = top_down_hits.Where(h => h.score >= min_score_td && h.retention_time >= min_RT_td && h.retention_time <= max_RT_td && ((biomarker && h.tdResultType == TopDownResultType.Biomarker) || (tight_abs_mass && h.tdResultType == TopDownResultType.TightAbsoluteMass))).OrderByDescending(h => h.score).ToList();

            List<TopDownHit> remaining_td_hits = new List<TopDownHit>();
            while(unprocessed_td_hits.Count > 0)
            {
                TopDownHit root = unprocessed_td_hits[0];
                remaining_td_hits.Add(root);
                unprocessed_td_hits = unprocessed_td_hits.Except(unprocessed_td_hits.Where(h => h.pvalue == root.pvalue && h.ms2ScanNumber == root.ms2ScanNumber && h.filename == root.filename)).ToList();
            }

            List<string> PFRs = remaining_td_hits.Select(h => h.pfr).Distinct().ToList();
            Parallel.ForEach(PFRs, pfr =>
            {
                List<TopDownHit> hits_by_pfr = remaining_td_hits.Where(h => h.pfr == pfr).ToList();
                List<TopDownProteoform> first_aggregation = new List<TopDownProteoform>();
                //aggregate to td hit w/ highest c-score as root - 1st average for retention time
                while (hits_by_pfr.Count > 0)
                {
                    TopDownHit root = hits_by_pfr[0];
                    //ambiguous results - only include higher scoring one (if same scan, file, and p-value)
                    //find topdownhits within RT tol --> first average
                    double first_RT_average = hits_by_pfr.Where(h => Math.Abs(h.retention_time - root.retention_time) <= Convert.ToDouble(retention_time_tolerance) && h.pfr == root.pfr).Select(h => h.retention_time).Average();
                    List<TopDownHit> hits_to_aggregate = hits_by_pfr.Where(h => Math.Abs(h.retention_time - first_RT_average) <= Convert.ToDouble(retention_time_tolerance) && h.pfr == root.pfr).OrderByDescending(h => h.score).ToList();
                    root = hits_to_aggregate[0];
                    //candiate topdown hits are those with the same theoretical accession and PTMs --> need to also be within RT tolerance used for agg
                    TopDownProteoform new_pf = new TopDownProteoform(root.accession, root, hits_to_aggregate);
                    hits_by_pfr = hits_by_pfr.Except(new_pf.topdown_hits).ToList();

                    //could have cases where topdown proteoforms same accession, mass, diff PTMs --> need a diff accession
                    lock (topdown_proteoforms)
                    {
                        int count = topdown_proteoforms.Count(t => t.accession.Contains(new_pf.accession));
                        if (count > 0)
                        {
                            string[] old_accession = new_pf.accession.Split('_');
                            new_pf.accession = old_accession[0] + "_TD" + count + "_" + old_accession[2] + old_accession[3] + "_" + old_accession[4];
                        }
                        topdown_proteoforms.Add(new_pf);
                    }
                }

            });
            return topdown_proteoforms;
        }
     
        #endregion TOPDOWN 

        #region PROTEOFORM FAMILIES Public Fields
        public bool count_adducts_as_identifications = false;
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
        public Dictionary<string, List<int>> conditionsBioReps = new Dictionary<string, List<int>>();
        public Dictionary<string, List<int>> ltConditionsBioReps = new Dictionary<string, List<int>>(); //key is the condition and value is the number of bioreps (not the list of bioreps)
        public Dictionary<string, List<int>> hvConditionsBioReps = new Dictionary<string, List<int>>(); //key is the condition and value is the number of bioreps (not the list of bioreps)
        public Dictionary<int, List<int>> quantBioFracCombos; //this dictionary has an integer list of bioreps with an integer list of observed fractions. this way we can be missing reps and fractions.
        public List<Tuple<int, int, double>> normalizationFactors;

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
                List<int> allbioreps = get_files(input_files, Purpose.Quantification).Where(f => f.lt_condition == condition || f.hv_condition == condition).Select(b => b.biological_replicate).Distinct().ToList();
                conditionsBioReps.Add(condition, allbioreps);
            }

            foreach (string condition in ltConditions)
            {
                List<int> ltbioreps = get_files(input_files, Purpose.Quantification).Where(f => f.lt_condition == condition).Select(b => b.biological_replicate).Distinct().ToList();
                ltConditionsBioReps.Add(condition, ltbioreps);
            }

            foreach (string condition in hvConditions)
            {
                List<int> hvbioreps = get_files(input_files, Purpose.Quantification).Where(f => f.hv_condition == condition).Select(b => b.biological_replicate).Distinct().ToList();
                hvConditionsBioReps.Add(condition, hvbioreps);
            }

            condition_count = conditionsBioReps.Count;

            int minLt = ltConditionsBioReps.Values.Min(v => v.Count);

            int minHv = hvConditionsBioReps.Values.Count > 0 ?
                hvConditionsBioReps.Values.Min(v => v.Count) :
                0;

            countOfBioRepsInOneCondition = conditionsBioReps.Min(kv => kv.Value.Count);
            minBiorepsWithObservations = countOfBioRepsInOneCondition > 0 ? countOfBioRepsInOneCondition : 1;

            //getBiorepsFractionsList
            quantBioFracCombos = new Dictionary<int, List<int>>();
            List<int> bioreps = input_files.Where(q => q.purpose == Purpose.Quantification).Select(b => b.biological_replicate).Distinct().ToList();
            List<int> fractions = new List<int>();
            foreach (int b in bioreps)
            {
                //fractions = input_files.Where(q => q.purpose == Purpose.Quantification).Where(rep => rep.biological_replicate == b).Select(f => f.fraction).Distinct().ToList();
                quantBioFracCombos.Add(b, fractions);
            }
        }

        #endregion QUANTIFICATION SETUP

        #region QUANTIFICATION Public Fields

        // Histograms
        public SortedDictionary<decimal, int> logIntensityHistogram = new SortedDictionary<decimal, int>(); // all intensities
        public SortedDictionary<decimal, int> logSelectIntensityHistogram = new SortedDictionary<decimal, int>(); // selected intensities
        public SortedDictionary<decimal, int> logSelectIntensityWithImputationHistogram = new SortedDictionary<decimal, int>(); // selected intensities & imputed intensities

        // Gaussian fit to histograms
        public decimal allObservedAverageIntensity; //log base 2
        public decimal allObservedStDev;
        public decimal allObservedGaussianArea;
        public decimal allObservedGaussianHeight;
        public decimal selectAverageIntensity; //log base 2
        public decimal selectStDev;
        public decimal selectGaussianArea;
        public decimal selectGaussianHeight;
        public decimal selectWithImputationAverageIntensity;
        public decimal selectWithImputationStDev;
        public decimal selectWithImputationGaussianArea;
        public decimal selectWithImputationGaussianHeight;
        public decimal bkgdAverageIntensity; //log base 2
        public decimal bkgdStDev;
        public decimal bkgdGaussianHeight;
        public decimal backgroundShift = -1.8m;
        public decimal backgroundWidth = 0.5m;

        // Condition labels
        public string numerator_condition = ""; // numerator for fold change calculation
        public string denominator_condition = ""; // denominator for fold change calculation
        public string induced_condition = ""; // induced condition for relative difference calculation

        // Selecting proteoforms for quantification
        public static string[] observation_requirement_possibilities = new string[] { "Minimum Bioreps with Observations From Any Single Condition", "Minimum Bioreps with Observations From Any Condition", "Minimum Bioreps with Observations From Each Condition" };
        public string observation_requirement = observation_requirement_possibilities[0];
        public int minBiorepsWithObservations = 1;
        public List<ExperimentalProteoform> satisfactoryProteoforms = new List<ExperimentalProteoform>(); // these are proteoforms meeting the required number of observations.
        public List<QuantitativeProteoformValues> qVals = new List<QuantitativeProteoformValues>(); // quantitative values associated with each selected proteoform

        // Relative difference calculations with balanced permutations
        public decimal sKnot_minFoldChange = 1m; // this is used in the original paper to correct for artificially high relative differences calculated at low intensities. Mass spec intensities are high enough in general that this is not a problem, so this is not customized by the user.
        public List<decimal> sortedProteoformRelativeDifferences = new List<decimal>(); // real relative differences for each selected proteoform; sorted
        public List<List<decimal>> permutedRelativeDifferences; // relative differences for each proteoform for each balanced permutation
        public List<List<decimal>> sortedPermutedRelativeDifferences; // sorted relative differences for each balanced permutation
        public List<decimal> avgSortedPermutationRelativeDifferences = new List<decimal>(); // average relative difference across sorted values for each balanced permutation
        public List<decimal> flattenedPermutedRelativeDifferences; // all relative differences from permutations
        public decimal offsetTestStatistics = 1m; // offset from expected relative differences (avgSortedPermutationRelativeDifferences); used to call significance with minimumPassingNegativeTestStatistic & minimumPassingPositiveTestStatisitic
        public decimal minimumPassingNegativeTestStatistic; // the first NEGATIVE relative difference from a selected proteoform that exceeded the offset BELOW the expected relative differences (avg sorted permuted); everything equal to or BELOW this value is considered significant
        public decimal minimumPassingPositiveTestStatisitic; // the first POSITIVE relative difference from a selected proteoform that exceeded the offset ABOVE the expected relative differences (avg sorted permuted); everything equal to or ABOVE this value is considered significant
        public double relativeDifferenceFDR; // average # of permuted relative differences that pass minimumPassingNegativeTestStatistic & minimumPassingPositiveTestStatisitic, divided by the number of selected proteoforms that passed
        public List<ProteinWithGoTerms> observedProteins = new List<ProteinWithGoTerms>(); // This is the complete list of observed proteins
        public List<ProteinWithGoTerms> quantifiedProteins = new List<ProteinWithGoTerms>(); // This is the complete list of proteins that were quantified and included in any accepted proteoform family
        public List<ProteinWithGoTerms> inducedOrRepressedProteins = new List<ProteinWithGoTerms>(); // This is the list of proteins from proteoforms that underwent significant induction or repression

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

            computeBiorepIntensities(target_proteoform_community.experimental_proteoforms, ltconditions, hvconditions);
            defineAllObservedIntensityDistribution(target_proteoform_community.experimental_proteoforms, logIntensityHistogram);

            satisfactoryProteoforms = determineProteoformsMeetingCriteria(conditions, target_proteoform_community.experimental_proteoforms, observation_requirement, minBiorepsWithObservations);

            defineSelectObservedIntensityDistribution(satisfactoryProteoforms, logSelectIntensityHistogram);
            defineBackgroundIntensityDistribution(quantBioFracCombos, satisfactoryProteoforms, backgroundShift, backgroundWidth);

            compute_proteoform_statistics(satisfactoryProteoforms, bkgdAverageIntensity, bkgdStDev, conditionsBioReps, numerator_condition, denominator_condition, induced_condition, sKnot_minFoldChange);
            defineSelectObservedWithImputedIntensityDistribution(satisfactoryProteoforms, logSelectIntensityWithImputationHistogram);

            permutedRelativeDifferences = compute_balanced_biorep_permutation_relativeDifferences(conditionsBioReps, induced_condition, satisfactoryProteoforms, sKnot_minFoldChange);
            flattenedPermutedRelativeDifferences = permutedRelativeDifferences.SelectMany(x => x).ToList();
            computeSortedRelativeDifferences(satisfactoryProteoforms, permutedRelativeDifferences);

            relativeDifferenceFDR = computeRelativeDifferenceFDR(avgSortedPermutationRelativeDifferences, sortedProteoformRelativeDifferences, satisfactoryProteoforms, flattenedPermutedRelativeDifferences, offsetTestStatistics);
            computeIndividualExperimentalProteoformFDRs(satisfactoryProteoforms, flattenedPermutedRelativeDifferences, sortedProteoformRelativeDifferences);

            observedProteins = getProteins(target_proteoform_community.experimental_proteoforms.Where(x => x.accepted));

            quantifiedProteins = getProteins(satisfactoryProteoforms);

            inducedOrRepressedProteins = getInducedOrRepressedProteins(satisfactoryProteoforms, minProteoformFoldChange, maxGoTermFDR, minProteoformIntensity);

        }

        public void computeBiorepIntensities(IEnumerable<ExperimentalProteoform> experimental_proteoforms, IEnumerable<string> ltconditions, IEnumerable<string> hvconditions)
        {
            Parallel.ForEach(experimental_proteoforms, eP => eP.make_biorepIntensityList(eP.lt_quant_components, eP.hv_quant_components, ltconditions, hvconditions));
        }

        public List<ExperimentalProteoform> determineProteoformsMeetingCriteria(List<string> conditions, IEnumerable<ExperimentalProteoform> experimental_proteoforms, string observation_requirement, int minBiorepsWithObservations)
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

        public void defineAllObservedIntensityDistribution(IEnumerable<ExperimentalProteoform> experimental_proteoforms, SortedDictionary<decimal, int> logIntensityHistogram) // the distribution of all observed experimental proteoform biorep intensities
        {
            IEnumerable<decimal> allIntensities = define_intensity_distribution(experimental_proteoforms.SelectMany(pf => pf.biorepIntensityList), logIntensityHistogram).Where(i => i > 1); //these are log2 values
            allObservedAverageIntensity = allIntensities.Average();
            allObservedStDev = (decimal)Math.Sqrt(allIntensities.Average(v => Math.Pow((double)(v - allObservedAverageIntensity), 2))); //population stdev calculation, rather than sample
            allObservedGaussianArea = get_gaussian_area(logIntensityHistogram);
            allObservedGaussianHeight = allObservedGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)allObservedStDev, 2));
        }

        public void defineSelectObservedIntensityDistribution(IEnumerable<ExperimentalProteoform> satisfactory_proteoforms, SortedDictionary<decimal, int> logSelectIntensityHistogram)
        {
            IEnumerable<decimal> allRoundedIntensities = define_intensity_distribution(satisfactory_proteoforms.SelectMany(pf => pf.biorepIntensityList), logSelectIntensityHistogram).Where(i => i > 1); //these are log2 values
            selectAverageIntensity = allRoundedIntensities.Average();
            selectStDev = (decimal)Math.Sqrt(allRoundedIntensities.Average(v => Math.Pow((double)(v - selectAverageIntensity), 2))); //population stdev calculation, rather than sample
            selectGaussianArea = get_gaussian_area(logSelectIntensityHistogram);
            selectGaussianHeight = selectGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)selectStDev, 2));
        }

        public void defineSelectObservedWithImputedIntensityDistribution(IEnumerable<ExperimentalProteoform> satisfactory_proteoforms, SortedDictionary<decimal, int> logSelectIntensityHistogram)
        {
            IEnumerable<decimal> allRoundedIntensities = define_intensity_distribution(satisfactoryProteoforms.SelectMany(pf => pf.quant.allIntensities.Values).ToList(), logSelectIntensityWithImputationHistogram);
            selectWithImputationAverageIntensity = allRoundedIntensities.Average();
            selectWithImputationStDev = (decimal)Math.Sqrt(allRoundedIntensities.Average(v => Math.Pow((double)(v - selectAverageIntensity), 2))); //population stdev calculation, rather than sample
            selectWithImputationGaussianArea = get_gaussian_area(logSelectIntensityHistogram);
            selectWithImputationGaussianHeight = selectGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)selectStDev, 2));
        }

        public List<decimal> define_intensity_distribution(IEnumerable<BiorepIntensity> intensities, SortedDictionary<decimal, int> histogram)
        {
            histogram.Clear();

            List<decimal> rounded_intensities = (
                from i in intensities
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

        public decimal get_gaussian_area(SortedDictionary<decimal, int> histogram)
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

        public void defineBackgroundIntensityDistribution(Dictionary<int, List<int>> quantBioFracCombos, List<ExperimentalProteoform> satisfactoryProteoforms, decimal backgroundShift, decimal backgroundWidth)
        {
            bkgdAverageIntensity = selectAverageIntensity + backgroundShift * selectStDev;
            bkgdStDev = selectStDev * backgroundWidth;

            int numMeasurableIntensities = quantBioFracCombos.Keys.Count * condition_count * satisfactoryProteoforms.Count; // all bioreps, all light conditions + all heavy conditions, all satisfactory proteoforms
            int numMeasuredIntensities = satisfactoryProteoforms.Sum(eP => eP.biorepIntensityList.Count); //biorep intensities are created to be unique to the light/heavy + condition + biorep
            int numMissingIntensities = numMeasurableIntensities - numMeasuredIntensities; //this could be negative if there were tons more quantitative intensities

            decimal bkgdGaussianArea = selectGaussianArea / (decimal)numMeasuredIntensities * (decimal)numMissingIntensities;
            bkgdGaussianHeight = bkgdGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)bkgdStDev, 2));
        }

        public void compute_proteoform_statistics(List<ExperimentalProteoform> satisfactoryProteoforms, decimal bkgdAverageIntensity, decimal bkgdStDev, Dictionary<string, List<int>> conditionsBioReps, string numerator_condition, string denominator_condition, string induced_condition, decimal sKnot_minFoldChange)
        {
            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                eP.quant.determine_biorep_intensities_and_test_statistics(eP.biorepIntensityList, conditionsBioReps, numerator_condition, denominator_condition, induced_condition, bkgdAverageIntensity, bkgdStDev, sKnot_minFoldChange);
            }
            qVals = satisfactoryProteoforms.Where(eP => eP.accepted == true).Select(e => e.quant).ToList();
        }


        /// <summary>
        /// Gets the relative differences of permuted bioreps.
        /// In the Tusher et al. (2001) paper, they made all 36 balanced permutations of the 2 cell lines. Here, we have one control that is orthogonal to the treatment: bioreps.
        /// Therefore, we can make 3 balanced permutations (see Example 3).
        /// </summary>
        /// <param name="conditionsBioReps"></param>
        /// <returns></returns>
        /// 
        /// Example 1 (triples):
        /// Normal: 1, 2, 3 | Stress: 4, 5, 6
        /// Nine balanced permutations for normal (technically these are "nearly balanced")
        /// *4*, 2, 3
        /// *5*, 2, 3
        /// *6*, 2, 3
        /// 1, *4*, 3
        /// 1, *5*, 3
        /// 1, *6*, 3
        /// 1, 2, *4*
        /// 1, 2, *5*
        /// 1, 2, *6*
        /// 
        /// Example 2 (quadruples, like in Tusher et al.):
        /// Normal: 1, 2, 3, 4 | Stress: 5, 6, 7, 8
        /// Thirty-six balanced permutations for normal
        /// *5*, *6*, 3, 4 ;; 1, *5*, *6*, 4 ;; 1, 2, *5*, *6* ;; *5*, 2, *6*, 4 ;; *5*, 2, 3 *6* ;; 1, *5*, 3, *6*
        /// *5*, *7*, 3, 4 ;; 1, *5*, *7*, 4 ;; 1, 2, *5*, *7* ;; *5*, 2, *7*, 4 ;; *5*, 2, 3 *7* ;; 1, *5*, 3, *7*
        /// *5*, *8*, 3, 4 ;; 1, *5*, *8*, 4 ;; 1, 2, *5*, *8* ;; *5*, 2, *8*, 4 ;; *5*, 2, 3 *8* ;; 1, *5*, 3, *8*
        /// *6*, *7*, 3, 4 ;; 1, *6*, *7*, 4 ;; 1, 2, *6*, *7* ;; *6*, 2, *7*, 4 ;; *6*, 2, 3 *7* ;; 1, *6*, 3, *7*
        /// *6*, *8*, 3, 4 ;; 1, *6*, *8*, 4 ;; 1, 2, *6*, *8* ;; *6*, 2, *8*, 4 ;; *6*, 2, 3 *8* ;; 1, *6*, 3, *8*
        /// *7*, *8*, 3, 4 ;; 1, *7*, *8*, 4 ;; 1, 2, *7*, *8* ;; *7*, 2, *8*, 4 ;; *7*, 2, 3 *8* ;; 1, *7*, 3, *8*
        /// 
        /// Example 3 (duples):
        /// Normal: n_1, n_2, n_3 | Stress: s_1, s_2, s_3
        /// Duple 1: n_1, s_1 ;; Duple 2: n_2, s_2 ;; Duple 3: n_3, s_3
        /// Three balanced permutations for "normal." Balanced when 2 from the original set.
        /// *s_1*, n_2, n_3
        /// n_1, *s_2*, n_3
        /// n_1, n_2, *s_3*
        /// 
        /// Example 4 (duples):
        /// Normal: n_1, n_2, n_3, n_4 | Stress: s_1, s_2, s_3, s_4
        /// Duple 1: n_1, s_1 ;; Duple 2: n_2, s_2 ;; Duple 3: n_3, s_3 ;; Duple 4: n_4, s_4
        /// Six balanced permutations for "normal." Balanced when 2 from the original set.
        /// *s_1*, *s_2*, n_3, n_4
        /// *s_1*, s_2, *s_3*, n_4
        /// *s_1*, s_2, n_3, *s_4*
        /// n_1, *s_2*, *s_3*, n_4
        /// n_1, *s_2*, n_3, *s_4*
        /// n_1, n_2, *s_3*, *s_4*
        /// 
        public List<List<decimal>> compute_balanced_biorep_permutation_relativeDifferences(Dictionary<string, List<int>> conditionsBioReps, string induced_condition, List<ExperimentalProteoform> satisfactoryProteoforms, decimal sKnot_minFoldChange)
        {
            if (!conditionsBioReps.All(x => x.Value.OrderBy(y => y).SequenceEqual(conditionsBioReps.First().Value.OrderBy(z => z))))
                throw new ArgumentException("Error: Permutation analysis doesn't currently handle unbalanced experimental designs.");
            if (conditionsBioReps.Any(x => x.Value.Count > x.Value.Distinct().Count()))
                throw new ArgumentException("Error: Permutation analysis doesn't currently handle experimental designs with more than one dimension of controls. It on");
            if (conditionsBioReps.Count > 2)
                throw new ArgumentException("Error: Permutation analysis doesn't currently handle experimental  designs with more than 2 conditions.");

            List<int> bioreps = conditionsBioReps.SelectMany(kv => kv.Value).Distinct().ToList();
            List<Tuple<string, int>> allInduced = conditionsBioReps[induced_condition].Select(v => new Tuple<string, int>(induced_condition, v)).ToList();
            List<Tuple<string, int>> allUninduced = conditionsBioReps.Where(kv => kv.Key != induced_condition).SelectMany(kv => kv.Value.Select(v => new Tuple<string, int>(kv.Key, v))).ToList();
            List<Tuple<string, int>> all = allInduced.Concat(allUninduced).ToList();
            List<IEnumerable<Tuple<string, int>>> permutations = ExtensionMethods.Combinations(all, allInduced.Count).ToList();
            List<IEnumerable<Tuple<string, int>>> balanced_permutations_induced = permutations.Where(p =>
                !p.SequenceEqual(allInduced) // not the original set
                && bioreps.All(rep => p.Any(x => x.Item2 == rep)) // all bioreps are represented
                && p.Count(x => x.Item1 != induced_condition) == allInduced.Count / 2) // there should be n/2 (int division) from the uninduced set to satisfy balanced and nearly balanced permutations
                .ToList();

            List<List<decimal>> permutedRelativeDifferences = new List<List<decimal>>(); // each internal list is sorted
            foreach (IEnumerable<Tuple<string, int>> induced in balanced_permutations_induced)
            {
                List<decimal> relativeDifferences = new List<decimal>();
                foreach (ExperimentalProteoform pf in satisfactoryProteoforms)
                {
                    List<BiorepIntensity> induced_intensities = induced.Select(x => pf.quant.allIntensities[x]).ToList();
                    List<BiorepIntensity> uninduced_intensities = pf.quant.allIntensities.Values.Except(induced_intensities).ToList();
                    relativeDifferences.Add(pf.quant.getSingleTestStatistic(induced_intensities, uninduced_intensities, pf.quant.StdDev(induced_intensities, uninduced_intensities), sKnot_minFoldChange));
                }
                permutedRelativeDifferences.Add(relativeDifferences);
            }
            return permutedRelativeDifferences;
        }

        public void computeSortedRelativeDifferences(List<ExperimentalProteoform> satisfactoryProteoforms, List<List<decimal>> permutedRelativeDifferences)
        {
            sortedProteoformRelativeDifferences = satisfactoryProteoforms.Select(eP => eP.quant.relative_difference).OrderBy(reldiff => reldiff).ToList();
            sortedPermutedRelativeDifferences = permutedRelativeDifferences.Select(list => list.OrderBy(reldiff => reldiff).ToList()).ToList();
            avgSortedPermutationRelativeDifferences = Enumerable.Range(0, sortedProteoformRelativeDifferences.Count).Select(i => sortedPermutedRelativeDifferences.Average(sorted => sorted[i])).OrderBy(x => x).ToList();
            int ct = 0;
            foreach (ExperimentalProteoform p in satisfactoryProteoforms.OrderBy(eP => eP.quant.relative_difference).ToList())
            {
                p.quant.correspondingAvgSortedRelDiff = avgSortedPermutationRelativeDifferences[ct++];
            }
        }

        /// <summary>
        /// Calculates the FDR and establishes significance for proteoforms with a method published by Tusher, et al. (2001)
        /// 
        /// First, thresholds are established on either side of the line where the test statistic is equal to the average permuted test statistic of the same rank.
        /// Every proteoform test statistic that passes that threshold is considered significant.
        /// 
        /// The average number of proteoforms that pass by chance is used to calculate the FDR. 
        /// This calculated as the proportion of all permuted tests that pass the thresholds, times the number of proteoforms quantified.
        /// The denominator is the number of proteoforms that passed. 
        /// Thus, the FDR is (# pfs passing by chance / # pfs passing).
        /// </summary>
        /// <param name="sortedAvgPermutationTestStatistics">
        /// The averages of test statistics calculated from permuted intensities for each proteoform. These are sorted independently of the real proteoform test statistics.
        /// </param>
        /// <param name="sortedProteoformTestStatistics">
        /// The test statistics calculated for each proteoform. These are sorted independently of the avg permuted test statistics.
        /// </param>
        /// <param name="satisfactoryProteoforms">
        /// All proteoforms selected for quantification.
        /// </param>
        /// <param name="permutedTestStatistics">
        /// All test statistics calculated from permuted proteoform intensities.
        /// </param>
        /// <param name="significanceTestStatOffset">
        /// Offset from the line where the proteoform test statistsic is equal to the average permuted test statistic: d(i) = dE(i). This is used as a threshold for significance for positive or negative test statistics. 
        /// </param>
        /// <returns></returns>
        public double computeRelativeDifferenceFDR(List<decimal> sortedAvgPermutationTestStatistics, List<decimal> sortedProteoformTestStatistics, List<ExperimentalProteoform> satisfactoryProteoforms, List<decimal> permutedTestStatistics, decimal significanceTestStatOffset)
        {
            minimumPassingNegativeTestStatistic = Decimal.MinValue;
            minimumPassingPositiveTestStatisitic = Decimal.MaxValue;

            for (int i = 0; i < satisfactoryProteoforms.Count; i++)
            {
                decimal lower_threshold = sortedAvgPermutationTestStatistics[i] - significanceTestStatOffset;
                decimal higher_threshold = sortedAvgPermutationTestStatistics[i] + significanceTestStatOffset;
                if (sortedProteoformTestStatistics[i] < lower_threshold)
                    minimumPassingNegativeTestStatistic = sortedProteoformTestStatistics[i]; // last one below
                if (sortedProteoformTestStatistics[i] > higher_threshold)
                {
                    minimumPassingPositiveTestStatisitic = sortedProteoformTestStatistics[i]; //first one above
                    break;
                }
            }

            double avgFalsePermutedPassingProteoforms = (double)permutedTestStatistics.Count(v => v <= minimumPassingNegativeTestStatistic && v <= 0 || minimumPassingPositiveTestStatisitic <= v && v >= 0) / (double)permutedTestStatistics.Count * (double)satisfactoryProteoforms.Count;

            int totalPassingProteoforms = 0;
            foreach (ExperimentalProteoform pf in satisfactoryProteoforms)
            {
                decimal test_statistic = pf.quant.relative_difference;
                pf.quant.significant = test_statistic <= minimumPassingNegativeTestStatistic && test_statistic <= 0 || minimumPassingPositiveTestStatisitic <= test_statistic && test_statistic >= 0;
                totalPassingProteoforms += Convert.ToInt32(pf.quant.significant);
            }

            if (totalPassingProteoforms == 0)
                return Double.NaN;

            double fdr = (double)avgFalsePermutedPassingProteoforms / (double)totalPassingProteoforms;
            return fdr;
        }

        public void computeIndividualExperimentalProteoformFDRs(List<ExperimentalProteoform> satisfactoryProteoforms, List<decimal> permutedTestStatistics, List<decimal> sortedProteoformTestStatistics)
        {
            Parallel.ForEach(satisfactoryProteoforms, eP =>
            {
                eP.quant.roughSignificanceFDR = QuantitativeProteoformValues.computeExperimentalProteoformFDR(eP.quant.relative_difference, permutedTestStatistics, satisfactoryProteoforms.Count, sortedProteoformTestStatistics);
            });
        }

        public void reestablishSignficance()
        {
            if (!useLocalFdrCutoff)
                relativeDifferenceFDR = computeRelativeDifferenceFDR(avgSortedPermutationRelativeDifferences, sortedProteoformRelativeDifferences, satisfactoryProteoforms, flattenedPermutedRelativeDifferences, offsetTestStatistics);
            else
                Parallel.ForEach(satisfactoryProteoforms, eP => { eP.quant.significant = eP.quant.roughSignificanceFDR <= localFdrCutoff; });
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

        public List<ProteinWithGoTerms> getInducedOrRepressedProteins(IEnumerable<ExperimentalProteoform> satisfactoryProteoforms, decimal minProteoformAbsLogFoldChange, decimal maxProteoformFDR, decimal minProteoformIntensity)
        {
            return getInterestingProteoforms(satisfactoryProteoforms, minProteoformAbsLogFoldChange, maxProteoformFDR, minProteoformIntensity)
                .Where(pf => pf.linked_proteoform_references != null && pf.linked_proteoform_references.FirstOrDefault() as TheoreticalProteoform != null)
                .Select(pf => pf.linked_proteoform_references.First() as TheoreticalProteoform)
                .SelectMany(t => t.ExpandedProteinList)
                .DistinctBy(pwg => pwg.Accession.Split('_')[0])
                .ToList();
        }

        public List<ProteoformFamily> getInterestingFamilies(IEnumerable<ExperimentalProteoform> proteoforms, decimal minProteoformFoldChange, decimal minProteoformFDR, decimal minProteoformIntensity)
        {
            return getInterestingProteoforms(proteoforms, minProteoformFoldChange, minProteoformFDR, minProteoformIntensity)
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

        public IEnumerable<ExperimentalProteoform> getInterestingProteoforms(IEnumerable<ExperimentalProteoform> proteoforms, decimal minProteoformAbsLogFoldChange, decimal maxProteoformFDR, decimal minProteoformIntensity)
        {
            return proteoforms.Where(p => 
                p.quant.significant
                && Math.Abs(p.quant.logFoldChange) > minProteoformAbsLogFoldChange
                && p.quant.intensitySum > minProteoformIntensity);
        }

        #endregion QUANTIFICATION

        #region GO-TERMS AND GO-TERM SIGNIFICANCE Public Fields

        public List<GoTermNumber> goTermNumbers = new List<GoTermNumber>();//these are the count and enrichment values
        public bool allTheoreticalProteins = false; // this sets the group used for background. True if all Proteins in the theoretical database are used. False if only proteins observed in the study are used.
        public bool allDetectedProteins = false; // this sets the group used for background. True if all Proteins in the theoretical database are used. False if only proteins observed in the study are used.
        public string backgroundProteinsList = "";
        public decimal maxGoTermFDR = 0.05m;
        public decimal minProteoformIntensity = 0; // 500000m;
        public decimal minProteoformFoldChange = 0; // 1m;

        #endregion GO-TERMS AND GO-TERM SIGNIFICANCE Public Fields

        #region GO-TERMS AND GO-TERM SIGNIFICANCE

        public void GO_analysis()
        {
            List<ProteinWithGoTerms> backgroundProteinsForGoAnalysis;
            if (backgroundProteinsList != null && backgroundProteinsList != "")
            {
                string[] protein_accessions = File.ReadAllLines(backgroundProteinsList).Select(acc => acc.Trim()).ToArray();
                backgroundProteinsForGoAnalysis = theoretical_database.expanded_proteins.Where(p => p.AccessionList.Any(acc => protein_accessions.Contains(acc.Split('_')[0]))).DistinctBy(pwg => pwg.Accession.Split('_')[0]).ToList();
            }
            else
            {
                backgroundProteinsForGoAnalysis = allTheoreticalProteins ?
                    theoretical_database.expanded_proteins.DistinctBy(pwg => pwg.Accession.Split('_')[0]).ToList() :
                    allDetectedProteins ?
                        observedProteins :
                        quantifiedProteins;
            }
            goTermNumbers = getGoTermNumbers(inducedOrRepressedProteins, backgroundProteinsForGoAnalysis);
            calculateGoTermFDR(goTermNumbers);
        }

        public List<GoTermNumber> getGoTermNumbers(List<ProteinWithGoTerms> inducedOrRepressedProteins, List<ProteinWithGoTerms> backgroundProteinSet) //These are only for "interesting proteins", which is the set of proteins induced or repressed beyond a specified fold change, intensity and below FDR.
        {
            Dictionary<string, int> goSignificantCounts = fillGoDictionary(inducedOrRepressedProteins);
            Dictionary<string, int> goBackgroundCounts = fillGoDictionary(backgroundProteinSet);
            return inducedOrRepressedProteins.SelectMany(p => p.GoTerms).DistinctBy(g => g.Id).Select(g =>
                new GoTermNumber(
                    g,
                    goSignificantCounts.ContainsKey(g.Id) ? goSignificantCounts[g.Id] : 0,
                    inducedOrRepressedProteins.Count,
                    goBackgroundCounts.ContainsKey(g.Id) ? goBackgroundCounts[g.Id] : 0,
                    backgroundProteinSet.Count
                )).ToList();
        }

        private Dictionary<string, int> fillGoDictionary(List<ProteinWithGoTerms> proteinSet)
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
            Parallel.ForEach(gtns, g => g.by = GoTermNumber.benjaminiYekutieli(gtns.Count, pvals, g.p_value));
        }

        #endregion GO-TERMS AND GO-TERM SIGNIFICANCE

        #region CALIBRATION
        public List<TopDownHit> td_hits_calibration = new List<TopDownHit>();
        public Dictionary<Tuple<string, double>, double> file_mz_correction = new Dictionary<Tuple<string, double>, double>();
        public Dictionary<Tuple<string, int, double>, double> td_hit_correction = new Dictionary<Tuple<string, int, double>, double>();
        public List<Component> calibration_components = new List<Component>();
        public string[] file_descriptions; 

        public void read_in_calibration_td_hits()
        {
            td_hits_calibration.Clear();
            file_mz_correction.Clear();
            td_hit_correction.Clear();
            calibration_components.Clear();
            foreach (InputFile file in input_files.Where(f => f.purpose == Purpose.CalibrationTopDown))
            {
                td_hits_calibration.AddRange(topdownReader.ReadTDFile(file));
            }
        }

        public string calibrate_files()
        {
            //determine biorep, techrep, fraction of each raw file, component file, and topdown hit
            foreach (string line in file_descriptions.Get(1, file_descriptions.Length))
            {
                int biological_replicate;
                int fraction;
                int technical_replicate;
                string[] file_description = line.Split('\t');
                if (file_descriptions.Count(d => d.Split('\t')[0] == file_description[0]) > 1)
                    return "Error in file descriptions file - multiple entries for one filename";
                try
                {
                    biological_replicate = Convert.ToInt32(file_description[1]);
                    fraction = Convert.ToInt32(file_description[2]);
                    technical_replicate = Convert.ToInt32(file_description[3]);
                }
                catch
                {
                    return "Error in file descriptions file values";
                }
                Parallel.ForEach(td_hits_calibration.Where(h => h.filename == file_description[0].Split('.')[0]), h =>
                {
                    h.biological_replicate = biological_replicate;
                    h.fraction = fraction;
                    h.technical_replicate = technical_replicate;
                });
            }
            if (td_hits_calibration.Any(h => h.fraction == 0))
                return "Error in file descriptions file - top-down hit(s) with no matching description.";
            if (input_files.Where(f => f.purpose == Purpose.RawFile || f.purpose == Purpose.CalibrationIdentification).Any(f => f.fraction == 0))
                return "Label fraction, biological replicate, and techincal replicate of input files.";
            foreach (InputFile raw_file in input_files.Where(f => f.purpose == Purpose.RawFile && f.topdown_file))
            {
                IMsDataFile<IMsDataScan<IMzSpectrum<IMzPeak>>> myMsDataFile = ThermoStaticData.LoadAllStaticData(raw_file.complete_path);
                Parallel.ForEach(Sweet.lollipop.td_hits_calibration.Where(f => f.filename == raw_file.filename).ToList(), hit =>
                {
                    int scanNum = hit.ms2ScanNumber;
                    while (scanNum < myMsDataFile.NumSpectra && myMsDataFile.GetOneBasedScan(scanNum).MsnOrder > 1) scanNum--;
                    hit.charge = Convert.ToInt16(Math.Round(hit.reported_mass / (double)(myMsDataFile.GetOneBasedScan(hit.ms2ScanNumber) as ThermoScanWithPrecursor).IsolationMz, 0)); //m / (m/z)  round to get charge 
                    hit.mz = hit.reported_mass.ToMz(hit.charge);
                    hit.retention_time = myMsDataFile.GetOneBasedScan(scanNum).RetentionTime;
                });
            }
            if (td_hits_calibration.Any(h => h.charge == 0)) return "Error: need to input all raw files for top-down hits.";

            foreach (int biological_replicate in input_files.Select(f => f.biological_replicate).Distinct())
            {
                foreach (int fraction in input_files.Where(f => f.biological_replicate == biological_replicate).Select(f => f.fraction).Distinct())
                {
                    foreach (int technical_replicate in input_files.Where(f => f.biological_replicate == biological_replicate && f.fraction == fraction).Select(f => f.technical_replicate).Distinct())
                    {
                        Calibration calibration = new Calibration();
                        foreach(InputFile file in input_files.Where(f => f.purpose == Purpose.RawFile && f.biological_replicate == biological_replicate && f.fraction == fraction && f.technical_replicate == technical_replicate))
                        {
                            process_raw_components(input_files.Where(f => f.purpose == Purpose.CalibrationIdentification && f.topdown_file == file.topdown_file && f.biological_replicate == biological_replicate && f.fraction == fraction && f.technical_replicate == technical_replicate).ToList(), calibration_components, Purpose.CalibrationIdentification, false);
                            bool calibrated = get_calibration_points(calibration, file);
                            if (calibrated)
                            {
                                determine_component_shift(calibration, file.topdown_file, biological_replicate, fraction, technical_replicate);
                                List<InputFile> files = input_files.Where(f => f.topdown_file == file.topdown_file && f.purpose == Purpose.CalibrationIdentification && f.biological_replicate == biological_replicate && f.fraction == fraction && f.technical_replicate == technical_replicate).ToList();
                                foreach (InputFile f in files) calibration.calibrate_components_in_xlsx(f);
                            }
                        }
                    }
                }
            }
            Calibration cali = new Calibration();
            foreach (InputFile file in input_files.Where(f => f.purpose == Purpose.CalibrationTopDown))
            {
                cali.calibrate_td_hits_file(file);
            }
            return "Successfully calibrated files.";
        }


        public bool get_calibration_points(Calibration calibration, InputFile raw_file)
        {
            List<TopDownHit> hits = td_hits_calibration.Where(h => h.biological_replicate == raw_file.biological_replicate && h.fraction == raw_file.fraction && (!raw_file.topdown_file || h.technical_replicate == raw_file.technical_replicate) && h.tdResultType == TopDownResultType.TightAbsoluteMass && h.score >= 40).ToList();
            return calibration.Run_TdMzCal(raw_file, hits, raw_file.topdown_file, raw_file.biological_replicate, raw_file.fraction, raw_file.technical_replicate);
        }

        public void determine_component_shift(Calibration calibration, bool topdown_file, int biological_replicate, int fraction, int technical_replicate)
        {
            Parallel.ForEach(calibration_components.Where(c =>c.input_file.topdown_file == topdown_file && c.input_file.biological_replicate == biological_replicate && c.input_file.fraction == fraction && c.input_file.technical_replicate == technical_replicate), c =>
            {
                foreach (ChargeState cs in c.charge_states)
                {
                    var key = new Tuple<string, double>(c.input_file.filename, Math.Round(cs.intensity, 0));
                    lock (file_mz_correction)
                    {
                        if (!file_mz_correction.ContainsKey(key))
                        {
                            file_mz_correction.Add(key, cs.mz_centroid);
                        }
                    }
                }

            });
        }

        #endregion CALIBRATION 

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

        public void clear_td()
        {
            Sweet.lollipop.top_down_hits.Clear();
            Sweet.lollipop.td_relations.Clear();
            topdownReader.topdown_ptms.Clear();
            foreach (ProteoformCommunity community in Sweet.lollipop.decoy_proteoform_communities.Values.Concat(new List<ProteoformCommunity> { Sweet.lollipop.target_proteoform_community }))
            {
                community.topdown_proteoforms = new TopDownProteoform[0];
                foreach (Proteoform p in community.experimental_proteoforms) p.relationships.RemoveAll(r => r.RelationType == ProteoformComparison.ExperimentalTopDown);
                foreach (Proteoform p in community.theoretical_proteoforms) p.relationships.RemoveAll(r => r.RelationType == ProteoformComparison.TheoreticalTopDown);
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

