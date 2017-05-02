using Accord.Math;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public void enter_input_files(string[] files, IEnumerable<string> acceptable_extensions, List<Purpose> purposes, List<InputFile> destination)
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

        public string match_calibration_files()
        {
            string return_message = "";

            // Look for results files with the same filename as a calibration file, and show that they're matched
            foreach (InputFile file in get_files(input_files, Purpose.RawFile))
            {
                if (input_files.Where(f => f.purpose != Purpose.RawFile).Select(f => f.filename).Contains(file.filename))
                {
                    IEnumerable<InputFile> matching_files = input_files.Where(f => f.purpose != Purpose.RawFile && f.filename == file.filename);
                    InputFile matching_file = matching_files.First();
                    if (matching_files.Count() != 1)
                        return_message += "Warning: There is more than one results file named " + file.filename + ". Will only match calibration to the first one from " + matching_file.purpose.ToString() + "." + Environment.NewLine;
                    file.matchingCalibrationFile = true;
                    matching_file.matchingCalibrationFile = true;
                }
            }

            if (get_files(input_files, Purpose.RawFile).Count() > 0 && !get_files(input_files, Purpose.RawFile).Any(f => f.matchingCalibrationFile))
                return_message += "To use calibration files, please give them the same filenames as the deconvolution results to which they correspond.";

            return return_message;
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
                    find_neucode_pairs(inputFile.reader.final_components.Where(c => c.scan_range == scan_range), raw_neucode_pairs);
                }
            }
        }

        #endregion RAW EXPERIMENTAL COMPONENTS

        #region NEUCODE PAIRS Public Fields

        public List<NeuCodePair> raw_neucode_pairs = new List<NeuCodePair>();
        public decimal max_intensity_ratio = 6m;
        public decimal min_intensity_ratio = 1.4m;
        public decimal max_lysine_ct = 26.2m;
        public decimal min_lysine_ct = 1.5m;

        #endregion NEUCODE PAIRS Public Fields

        #region NEUCODE PAIRS

        public List<NeuCodePair> find_neucode_pairs(IEnumerable<Component> components_in_file_scanrange, List<NeuCodePair> destination)
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
                                double lower_intensity = lower_component.calculate_sum_intensity_olcs(overlapping_charge_states);
                                double higher_intensity = higher_component.calculate_sum_intensity_olcs(overlapping_charge_states);
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
                    if (pair.weighted_monoisotopic_mass <= pair.neuCodeHeavy.weighted_monoisotopic_mass + MONOISOTOPIC_UNIT_MASS // the heavy should be at higher mass. Max allowed is 1 dalton less than light.                                    
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

        #endregion NEUCODE PAIRS

        #region AGGREGATED PROTEOFORMS Public Fields

        public ProteoformCommunity proteoform_community = new ProteoformCommunity();
        public List<Component> remaining_components = new List<Component>();
        public List<Component> remaining_verification_components = new List<Component>();
        public List<Component> remaining_quantification_components = new List<Component>();
        public bool validate_proteoforms = true;
        public decimal mass_tolerance = 10; //ppm
        public decimal retention_time_tolerance = 5; //min
        public decimal missed_monos = 3;
        public decimal missed_lysines = 2;
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
            List<ExperimentalProteoform> candidateExperimentalProteoforms = createProteoforms(raw_neucode_pairs, raw_experimental_components, min_num_CS);
            if (two_pass_validation) vetted_proteoforms = vetExperimentalProteoforms(candidateExperimentalProteoforms, raw_experimental_components, vetted_proteoforms);
            else vetted_proteoforms = candidateExperimentalProteoforms;
            proteoform_community.experimental_proteoforms = vetted_proteoforms.ToArray();
            if (neucode_labeled && get_files(input_files, Purpose.Quantification).Count() > 0) assignQuantificationComponents(vetted_proteoforms, raw_quantification_components);
            return vetted_proteoforms;
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

            for (int i = 0; i < candidateExperimentalProteoforms.Count; i++) candidateExperimentalProteoforms[i].accession = "E" + i;
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
            remaining_verification_components = new List<Component>(raw_experimental_components);

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
                    remaining_verification_components = remaining_verification_components.Except(e.lt_verification_components.Concat(e.hv_verification_components)).ToList();
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
            remaining_quantification_components = new List<Component>(raw_quantification_components);

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
                    remaining_quantification_components = remaining_quantification_components.Except(e.lt_quant_components.Concat(e.hv_quant_components)).ToList();
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
            return aggregate_proteoforms(two_pass_validation, raw_neucode_pairs, raw_experimental_components, raw_quantification_components, min_num_CS);
        }

        public void clear_aggregation()
        {
            SaveState.lollipop.proteoform_community.experimental_proteoforms = new ExperimentalProteoform[0];
            SaveState.lollipop.vetted_proteoforms.Clear();
            SaveState.lollipop.ordered_components = new Component[0];
            SaveState.lollipop.remaining_components.Clear();
            SaveState.lollipop.remaining_verification_components.Clear();
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
        public string decoy_database_name_prefix = "DecoyDatabase_";
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

        #endregion THEORETICAL DATABASE Public Fields

        #region ET,ED,EE,EF COMPARISONS Public Fields

        public double ee_max_mass_difference = 350;
        public double ee_max_RetentionTime_difference = 2.5;
        public double et_low_mass_difference = -300;
        public double et_high_mass_difference = 350;
        public double no_mans_land_lowerBound = 0.22;
        public double no_mans_land_upperBound = 0.88;
        public double peak_width_base_et = 0.03; //need to be separate so you can change one and not other. 
        public double peak_width_base_ee = 0.03;
        public double min_peak_count_et = 5;
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

        #endregion ET,ED,EE,EF,TD COMPARISONS Public Fields

        #region TOPDOWN 

        public List<TopDownHit> top_down_hits = new List<TopDownHit>();
        public TopDownReader topdownReader = new TopDownReader();
        public double min_C_score = 3.0d;
                    //C-score > 40: proteoform is both identified and fully characterized; 
                    //3 ≤ Cscore≤ 40: proteoform is identified, but only partially characterized; 
                    //C-score < 3: proteoform is neither identified nor characterized.

        public void read_in_td_hits()
        {
            foreach (InputFile file in input_files.Where(f => f.purpose == Purpose.TopDown).ToList())
            {
                top_down_hits.AddRange(topdownReader.ReadTDFile(file));
            }
        }

        public void aggregate_td_hits()
        {
            //group hits into topdown proteoforms by accession/theoretical AND observed mass
            List<TopDownProteoform> topdown_proteoforms = new List<TopDownProteoform>();
            //TopDownHit[] remaining_td_hits = new TopDownHit[0];
            List<TopDownHit> remaining_td_hits = top_down_hits.Where(h => h.score > min_C_score).OrderByDescending(h => h.score).ToList();

            List<TopDownProteoform> first_aggregation = new List<TopDownProteoform>();
            //aggregate to td hit w/ highest c-score as root - 1st average for retention time
            while (remaining_td_hits.Count > 0)
            {
                TopDownHit root = remaining_td_hits[0];
                //find topdownhits within RT tol --> first average
                double first_RT_average = remaining_td_hits.Where(h => h.targeted == root.targeted && Math.Abs(h.retention_time - root.retention_time) <= Convert.ToDouble(retention_time_tolerance) && h.accession == root.accession && h.sequence == root.sequence && h.same_ptm_hits(root)).Select(h => h.retention_time).Average();
                List<TopDownHit> hits_to_aggregate = remaining_td_hits.Where(h => h.targeted == root.targeted && Math.Abs(h.retention_time - first_RT_average) <= Convert.ToDouble(retention_time_tolerance) && h.accession == root.accession && h.sequence == root.sequence && h.same_ptm_hits(root)).OrderByDescending(h => h.score).ToList();
                root = hits_to_aggregate[0];
                //candiate topdown hits are those with the same theoretical accession and PTMs --> need to also be within RT tolerance used for agg
                TopDownProteoform new_pf = new TopDownProteoform(root.accession, root, hits_to_aggregate);
                remaining_td_hits = remaining_td_hits.Except(new_pf.topdown_hits).ToList();

                //could have cases where topdown proteoforms same accession, mass, diff PTMs --> need a diff accession
                if (topdown_proteoforms.Select(t => t.accession).Contains(new_pf.accession))
                {
                    string[] old_accession = new_pf.accession.Split('_');
                    int num = Convert.ToInt16(old_accession[1].ElementAt(2).ToString()) + 1;
                    new_pf.accession = old_accession[0] + "_TD" + num + "_" + old_accession[2] + old_accession[3] + "_" + old_accession[4];
                }
                topdown_proteoforms.Add(new_pf);
            }
            SaveState.lollipop.proteoform_community.topdown_proteoforms = topdown_proteoforms.Where(p => p!= null).ToArray();
        }

        #endregion TOPDOWN 

        #region PROTEOFORM FAMILIES Public Fields

        public string family_build_folder_path = "";
        public int deltaM_edge_display_rounding = 2;
        public static string[] node_positioning = new string[] { "Arbitrary Circle", "Mass X-Axis", "Circle by Mass" };
        public static string[] node_labels = new string[] { "Experimental ID", "Inferred Theoretical ID" };
        public static string[] edge_labels = new string[] { "Mass Difference", "Modification IDs (omits edges with null IDs)" };
        public static List<string> gene_name_labels = new List<string> { "Primary, e.g. HOG1", "Ordered Locus, e.g. YLR113W" };
        public string[] likely_cleavages = new string[] { "I", "L", "A" };

        #endregion PROTEOFORM FAMILIES Public Fields

        #region QUANTIFICATION SETUP Public Fields

        public int countOfBioRepsInOneCondition; //need this in quantification to select which proteoforms to perform calculations on.
        public int condition_count;
        public Dictionary<string, List<int>> ltConditionsBioReps = new Dictionary<string, List<int>>(); //key is the condition and value is the number of bioreps (not the list of bioreps)
        public Dictionary<string, List<int>> hvConditionsBioReps = new Dictionary<string, List<int>>(); //key is the condition and value is the number of bioreps (not the list of bioreps)
        public Dictionary<int, List<int>> quantBioFracCombos; //this dictionary has an integer list of bioreps with an integer list of observed fractions. this way we can be missing reps and fractions.
        public List<Tuple<int, int, double>> normalizationFactors;

        #endregion QUANTIFICATION SETUP Public Fields

        #region QUANTIFICATION SETUP

        public void getBiorepsFractionsList(List<InputFile> input_files)  //this should be moved to the appropriate location. somewhere at the start of raw component/end of load component.
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

        public void getObservationParameters(bool neucode_labeled, List<InputFile> input_files) //examines the conditions and bioreps to determine the maximum number of observations to require for quantification
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
                //ltConditionsBioReps.Add(condition, get_files(Purpose.Quantification).Where(f => f.lt_condition == condition).Select(b => b.biological_replicate).ToList().Distinct().Count()); // this gives the count of bioreps in the specified condition
                List<int> bioreps = get_files(input_files, Purpose.Quantification).Where(f => f.lt_condition == condition).Select(b => b.biological_replicate).ToList();
                bioreps = bioreps.Distinct().ToList();
                ltConditionsBioReps.Add(condition, bioreps);
            }

            foreach (string condition in hvConditions)
            {
                //hvConditionsBioReps.Add(condition, get_files(Purpose.Quantification).Where(f => f.hv_condition == condition).Select(b => b.biological_replicate).ToList().Distinct().Count()); // this gives the count of bioreps in the specified condition
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

        #endregion QUANTIFICATION SETUP

        #region QUANTIFICATION Public Fields

        public SortedDictionary<decimal, int> logIntensityHistogram = new SortedDictionary<decimal, int>();
        public SortedDictionary<decimal, int> logSelectIntensityHistogram = new SortedDictionary<decimal, int>();

        public string numerator_condition = "";
        public string denominator_condition = "";
        public decimal observedAverageIntensity; //log base 2
        public decimal selectAverageIntensity; //log base 2
        public decimal observedStDev;
        public decimal selectStDev;
        public decimal observedGaussianArea;
        public decimal selectGaussianArea;
        public decimal observedGaussianHeight;
        public decimal bkgdAverageIntensity; //log base 2
        public decimal bkgdStDev;
        public decimal bkgdGaussianHeight;
        public decimal backgroundShift;
        public decimal backgroundWidth;
        public List<ExperimentalProteoform> satisfactoryProteoforms = new List<ExperimentalProteoform>(); // these are proteoforms meeting the required number of observations.
        public List<decimal> permutedTestStatistics;
        public static string[] observation_requirement_possibilities = new string[] { "Minimum Bioreps with Observations From Any Single Condition", "Minimum Bioreps with Observations From Any Condition", "Minimum Bioreps with Observations From Each Condition" };
        public string observation_requirement = observation_requirement_possibilities[0];
        public int minBiorepsWithObservations = 1;
        public decimal selectGaussianHeight;
        public List<QuantitativeProteoformValues> qVals = new List<QuantitativeProteoformValues>();
        public decimal sKnot_minFoldChange = 1m;
        public List<decimal> sortedProteoformTestStatistics = new List<decimal>();
        public List<decimal> sortedAvgPermutationTestStatistics = new List<decimal>();
        public decimal offsetTestStatistics = 1m;
        //public decimal negativeOffsetTestStatistics = -1m;
        public decimal offsetFDR;
        public List<ProteinWithGoTerms> observedProteins = new List<ProteinWithGoTerms>(); //This is the complete list of observed proteins
        public List<ProteinWithGoTerms> quantifiedProteins = new List<ProteinWithGoTerms>(); //This is the complete list of proteins that were quantified and included in any accepted proteoform family
        public List<ProteinWithGoTerms> inducedOrRepressedProteins = new List<ProteinWithGoTerms>(); //This is the list of proteins from proteoforms that underwent significant induction or repression
        public decimal minProteoformIntensity = 500000m;
        public decimal minProteoformFoldChange = 1m;
        public decimal minProteoformFDR = 0.05m;

        #endregion QUANTIFICATION Public Fields

        #region QUANTIFICATION

        public void quantify()
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
            observedProteins = getProteins(proteoform_community.experimental_proteoforms.Where(x => x.accepted));
            quantifiedProteins = getProteins(satisfactoryProteoforms);
            inducedOrRepressedProteins = getInducedOrRepressedProteins(satisfactoryProteoforms, minProteoformFoldChange, minProteoformFDR, minProteoformIntensity);
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
            IEnumerable<decimal> allIntensities = define_intensity_distribution(experimental_proteoforms, logIntensityHistogram).Where(i => i > 1); //these are log2 values
            observedAverageIntensity = allIntensities.Average();
            observedStDev = (decimal)Math.Sqrt(allIntensities.Average(v => Math.Pow((double)(v - observedAverageIntensity), 2))); //population stdev calculation, rather than sample
            observedGaussianArea = get_gaussian_area(logIntensityHistogram);
            observedGaussianHeight = observedGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)observedStDev, 2));
        }

        public void defineSelectObservedIntensityDistribution(IEnumerable<ExperimentalProteoform> satisfactory_proteoforms, SortedDictionary<decimal, int> logSelectIntensityHistogram)
        {
            IEnumerable<decimal> allRoundedIntensities = define_intensity_distribution(satisfactory_proteoforms, logSelectIntensityHistogram).Where(i => i > 1); //these are log2 values
            selectAverageIntensity = allRoundedIntensities.Average();
            selectStDev = (decimal)Math.Sqrt(allRoundedIntensities.Average(v => Math.Pow((double)(v - selectAverageIntensity), 2))); //population stdev calculation, rather than sample
            selectGaussianArea = get_gaussian_area(logSelectIntensityHistogram);
            selectGaussianHeight = selectGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)selectStDev, 2));
        }

        public List<decimal> define_intensity_distribution(IEnumerable<ExperimentalProteoform> proteoforms, SortedDictionary<decimal, int> histogram)
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

        public void defineBackgroundIntensityDistribution(bool neucode_labeled, Dictionary<int, List<int>> quantBioFracCombos, List<ExperimentalProteoform> satisfactoryProteoforms, decimal backgroundShift, decimal backgroundWidth)
        {
            bkgdAverageIntensity = selectAverageIntensity + backgroundShift * selectStDev;
            bkgdStDev = selectStDev * backgroundWidth;

            int numMeasurableIntensities = quantBioFracCombos.Keys.Count * condition_count * satisfactoryProteoforms.Count; // all bioreps, all light conditions + all heavy conditions, all satisfactory proteoforms
            int numMeasuredIntensities = satisfactoryProteoforms.Sum(eP => eP.biorepIntensityList.Count); //biorep intensities are created to be unique to the light/heavy + condition + biorep
            int numMissingIntensities = numMeasurableIntensities - numMeasuredIntensities; //this could be negative if there were tons more quantitative intensities

            decimal bkgdGaussianArea = selectGaussianArea / (decimal)numMeasuredIntensities * (decimal)numMissingIntensities;
            bkgdGaussianHeight = bkgdGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)bkgdStDev, 2));
        }

        public void computeProteoformTestStatistics(bool neucode_labeled, List<ExperimentalProteoform> satisfactoryProteoforms, decimal bkgdAverageIntensity, decimal bkgdStDev, string numerator_condition, string denominator_condition, decimal sKnot_minFoldChange)
        {
            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                eP.quant.determine_biorep_intensities_and_test_statistics(neucode_labeled, eP.biorepIntensityList, bkgdAverageIntensity, bkgdStDev, numerator_condition, denominator_condition, sKnot_minFoldChange);
            }
            qVals = satisfactoryProteoforms.Where(eP => eP.accepted == true).Select(e => e.quant).ToList();
            permutedTestStatistics = satisfactoryProteoforms.SelectMany(eP => eP.quant.permutedTestStatistics).ToList();
        }

        public void computeSortedTestStatistics(List<ExperimentalProteoform> satisfactoryProteoforms)
        {
            sortedProteoformTestStatistics = satisfactoryProteoforms.Select(eP => eP.quant.testStatistic).ToList();
            sortedAvgPermutationTestStatistics = satisfactoryProteoforms.Select(eP => eP.quant.permutedTestStatistics.Average()).ToList();
            sortedProteoformTestStatistics.Sort();
            sortedAvgPermutationTestStatistics.Sort();
        }

        public decimal computeFoldChangeFDR(List<decimal> sortedAvgPermutationTestStatistics, List<decimal> sortedProteoformTestStatistics, List<ExperimentalProteoform> satisfactoryProteoforms, IEnumerable<decimal> permutedTestStatistics, decimal offsetTestStatistics)
        {
            decimal minimumPositivePassingTestStatistic = sortedProteoformTestStatistics[Enumerable.Range(0, sortedAvgPermutationTestStatistics.Count).FirstOrDefault(i => sortedProteoformTestStatistics[i] >= sortedAvgPermutationTestStatistics[i] + offsetTestStatistics)]; //first time the test statistic exceeds the cap so we're good.
            decimal minimumNegativePassingTestStatistic = sortedProteoformTestStatistics[Enumerable.Range(0, sortedAvgPermutationTestStatistics.Count).LastOrDefault(i => sortedProteoformTestStatistics[i] <= sortedAvgPermutationTestStatistics[i] - offsetTestStatistics)]; //last time the test statistic is below the minimum

            int totalFalsePermutedPositiveValues = permutedTestStatistics.Count(p => p >= minimumPositivePassingTestStatistic);
            int totalFalsePermutedNegativeValues = permutedTestStatistics.Count(p => p <= minimumNegativePassingTestStatistic);

            decimal averagePermuted = (decimal)(totalFalsePermutedPositiveValues + totalFalsePermutedNegativeValues) / (decimal)satisfactoryProteoforms.Count;
            return averagePermuted / ((decimal)(sortedProteoformTestStatistics.Count(s => s >= minimumPositivePassingTestStatistic) + sortedProteoformTestStatistics.Count(s => s <= minimumNegativePassingTestStatistic)));
        }


        public void computeIndividualExperimentalProteoformFDRs(List<ExperimentalProteoform> satisfactoryProteoforms, List<decimal> sortedProteoformTestStatistics, decimal minProteoformFoldChange, decimal minProteoformFDR, decimal minProteoformIntensity)
        {
            List<List<decimal>> permutedTestStatistics = satisfactoryProteoforms.Select(eP => eP.quant.permutedTestStatistics).ToList();
            Parallel.ForEach(satisfactoryProteoforms, eP =>
            {
                eP.quant.FDR = QuantitativeProteoformValues.computeExperimentalProteoformFDR(eP.quant.testStatistic, permutedTestStatistics, satisfactoryProteoforms.Count, sortedProteoformTestStatistics);
                eP.quant.significant = Math.Abs(eP.quant.logFoldChange) > minProteoformFoldChange && eP.quant.FDR < minProteoformFDR && eP.quant.intensitySum > minProteoformIntensity;
            });
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
                .Select(p => p.family)
                .SelectMany(pf => pf.theoretical_proteoforms)
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
            return proteoforms.Where(
                p => Math.Abs(p.quant.logFoldChange) > minProteoformAbsLogFoldChange
                && p.quant.FDR < maxProteoformFDR
                && p.quant.intensitySum > minProteoformIntensity);
        }

        #endregion QUANTIFICATION

        #region GO-TERMS AND GO-TERM SIGNIFICANCE Public Fields

        public List<GoTermNumber> goTermNumbers = new List<GoTermNumber>();//these are the count and enrichment values
        public bool allTheoreticalProteins = false; // this sets the group used for background. True if all Proteins in the theoretical database are used. False if only proteins observed in the study are used.
        public bool allDetectedProteins = false; // this sets the group used for background. True if all Proteins in the theoretical database are used. False if only proteins observed in the study are used.
        public string backgroundProteinsList = "";

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
        public  bool calibrate_lock_mass = false;
        public  bool calibrate_td_results = true;
        public  bool calibrate_intact_with_td_ids = false;
        public  List<TopDownHit> td_hits_calibration = new List<TopDownHit>();
        public  List<MsScan> Ms_scans = new List<MsScan>();
        public  Dictionary<string, Func<double[], double>> td_calibration_functions = new Dictionary<string, Func<double[], double>>();
        public  List<Correction> correctionFactors = new List<Correction>();
        public  Dictionary<Tuple<string, double, double>, double> file_mz_correction = new Dictionary<Tuple<string, double, double>, double>();
        public  Dictionary<Tuple<string, int, double>, double> td_hit_correction = new Dictionary<Tuple<string, int, double>, double>();
        public  List<Component> calibration_components = new List<Component>();

        public void read_in_calibration_td_hits()
        {
            foreach (InputFile file in input_files.Where(f => f.purpose == Purpose.CalibrationTopDown))
            {
                td_hits_calibration.AddRange(topdownReader.ReadTDFile(file));
            }
        }

        public void calibrate_files()
        {
            //add distinct filenames from both deconvolution results and topdown results
            List<string> filenames = new List<string>();
            filenames.AddRange(input_files.Where(f => f.purpose == Purpose.CalibrationIdentification).Select(f => f.filename).Distinct());
            filenames.AddRange(td_hits_calibration.Select(f => f.filename).Distinct());
            //get calibration points and calibrate deconvolution files
            foreach (string filename in filenames.Distinct())
            {
                process_raw_components(input_files.Where(f => f.purpose == Purpose.CalibrationIdentification && f.filename == filename).ToList(), calibration_components, Purpose.CalibrationIdentification, false);
                get_calibration_points(filename);
                calibrate_td_hits(filename);
                determine_component_shift();
                InputFile file = input_files.Where(f => f.purpose == Purpose.CalibrationIdentification && f.filename == filename).FirstOrDefault();
                if (file != null) Calibration.calibrate_components_in_xlsx(file);
            }
            foreach (InputFile file in input_files.Where(f => f.purpose == Purpose.CalibrationTopDown))
            {
                Calibration.calibrate_td_hits_file(file);
            }
        }


        public void get_calibration_points(string filename)
        {
            List<InputFile> raw_files = input_files.Where(f => f.purpose == Purpose.RawFile && f.filename == filename).ToList();
            if (raw_files.Count > 0)
            {
                //get calibration points
                InputFile raw_file = raw_files.First();
                Ms_scans = RawFileReader.get_ms_scans(filename, raw_file.complete_path);

                if (calibrate_lock_mass)
                {
                    Calibration.raw_lock_mass(filename, raw_file.complete_path);
                    correctionFactors.AddRange(Correction.CorrectionFactorInterpolation(Ms_scans.Where(s => s.filename == filename).Select(s => (new Correction(s.filename, s.scan_number, s.lock_mass_shift)))));
                }

                if (calibrate_td_results || calibrate_intact_with_td_ids)
                {
                    Func<double[], double> bestCf = Calibration.Run_TdMzCal(filename, td_hits_calibration.Where(h => h.filename == filename && h.tdResultType == TopDownResultType.TightAbsoluteMass).ToList(), true);
                    if (bestCf != null) td_calibration_functions.Add(filename, bestCf);
                    if (calibrate_intact_with_td_ids)
                    {
                        bestCf = Calibration.Run_TdMzCal(filename, td_hits_calibration.Where(h => h.tdResultType == TopDownResultType.TightAbsoluteMass).ToList(), false);
                        if (bestCf != null) td_calibration_functions.Add(filename, bestCf);
                    }
                }
            }
        }

        public void determine_component_shift()
        {
            Parallel.ForEach(calibration_components, c =>
            {
                if ((!calibrate_td_results && !calibrate_intact_with_td_ids) || td_calibration_functions.ContainsKey(c.input_file.filename))
                {
                    foreach (ChargeState cs in c.charge_states)
                    {
                        double corrected_mz = (calibrate_td_results || calibrate_intact_with_td_ids) ? cs.mz_centroid - td_calibration_functions[c.input_file.filename](new double[] { cs.mz_centroid, Convert.ToDouble(c.rt_range.Split('-')[0]) }) : cs.mz_centroid - Calibration.get_correction_factor(c.input_file.filename, c.scan_range);
                        var key = new Tuple<string, double, double>(c.input_file.filename, Math.Round(cs.mz_centroid, 0), Math.Round(cs.intensity, 0));
                        if (!file_mz_correction.ContainsKey(key))
                        {
                            lock (file_mz_correction) file_mz_correction.Add(key, corrected_mz);
                        }
                    }
                }
            });
        }

        public void calibrate_td_hits(string filename)
        {
            if (calibrate_td_results || calibrate_intact_with_td_ids && td_calibration_functions.ContainsKey(filename))
            {
                Func<double[], double> bestCf = td_calibration_functions[filename];
                //need to calibrate all the others
                foreach (TopDownHit hit in td_hits_calibration.Where(h => h.filename == filename))
                {
                    Tuple<string, int, double> key = new Tuple<string, int, double>(filename, hit.scan, hit.reported_mass);
                    if (!td_hit_correction.ContainsKey(key)) lock (td_hit_correction) td_hit_correction.Add(key, (hit.mz - bestCf(new double[] { hit.mz, hit.retention_time })) * hit.charge - hit.charge * PROTON_MASS);
                }
            }
            else if (!calibrate_td_results && calibrate_lock_mass)
            {
                foreach (TopDownHit hit in td_hits_calibration.Where(h => h.filename == filename))
                {
                    double correction = 0;
                    try { correction = correctionFactors.Where(c => c.file_name == filename && c.scan_number == hit.scan).First().correction; }
                    catch { }
                    Tuple<string, int, double> key = new Tuple<string, int, double>(filename, hit.scan, hit.reported_mass);
                    if (!td_hit_correction.ContainsKey(key)) lock (td_hit_correction) td_hit_correction.Add(key, (hit.mz - correction) * hit.charge - hit.charge * PROTON_MASS);
                }
            }
        }

        #endregion CALIBRATION 
    }
}
