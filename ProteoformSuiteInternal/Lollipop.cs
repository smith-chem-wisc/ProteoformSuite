using Accord.Math;
using Proteomics;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UsefulProteomicsDatabases;

namespace ProteoformSuiteInternal
{
    [Serializable]
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

        [NonSerialized]
        public ProteoformCommunity proteoform_community = new ProteoformCommunity();

        [NonSerialized]
        public List<ExperimentalProteoform> vetted_proteoforms = new List<ExperimentalProteoform>();

        public Component[] ordered_components = new Component[0];
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
                        // e.accepted = true; this is set based on the e properties
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

        #endregion AGGREGATED PROTEOFORMS

        #region THEORETICAL DATABASE Public Fields

        public bool methionine_oxidation = true;
        public bool carbamidomethylation = true;
        public bool methionine_cleavage = true;
        public bool natural_lysine_isotope_abundance = false;
        public bool neucode_light_lysine = true;
        public bool neucode_heavy_lysine = false;
        public int max_ptms = 3;
        public int decoy_databases = 0;
        public string decoy_database_name_prefix = "DecoyDatabase_";
        public int min_peptide_length = 7;
        public double ptmset_mass_tolerance = 0.00001;
        public bool combine_identical_sequences = true;
        public bool combine_theoretical_proteoforms_byMass = true;
        public string[] mod_types_to_exclude = new string[] { "Metal", "PeptideTermMod", "TrypticProduct" };

        [NonSerialized]
        public Dictionary<InputFile, Protein[]> theoretical_proteins = new Dictionary<InputFile, Protein[]>();

        [NonSerialized]
        public ProteinWithGoTerms[] expanded_proteins = new ProteinWithGoTerms[0];

        [NonSerialized]
        public Dictionary<string, IList<Modification>> uniprotModifications = new Dictionary<string, IList<Modification>>();

        [NonSerialized]
        public List<ModificationWithMass> variableModifications = new List<ModificationWithMass>();

        [NonSerialized]
        public List<PtmSet> all_possible_ptmsets;

        [NonSerialized]
        public Dictionary<double, List<PtmSet>> possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>();

        [NonSerialized]
        public List<ModificationWithMass> all_mods_with_mass = new List<ModificationWithMass>();

        public Dictionary<double, int> modification_ranks = new Dictionary<double, int>();
        public int rank_sum_threshold = 0; // set to the maximum rank of any single modification
        public int rank_first_quartile = 0; // approximate quartiles used for heuristics with unranked modifications
        public int rank_second_quartile = 0;
        public int rank_third_quartile = 0;

        [NonSerialized]
        public List<PtmSet> acceptable_ptm_sets = new List<PtmSet>();
        Dictionary<char, double> aaIsotopeMassList;

        #endregion THEORETICAL DATABASE Public Fields

        #region THEORETICAL DATABASE

        public void get_theoretical_proteoforms(string current_directory)
        {
            //Clear out data from potential previous runs
            proteoform_community.decoy_proteoforms.Clear();
            theoretical_proteins.Clear();

            //Read the UniProt-XML and ptmlist
            Loaders.LoadElements(Path.Combine(current_directory, "elements.dat"));
            List<ModificationWithLocation> all_known_modifications = get_files(input_files, Purpose.PtmList).SelectMany(file => PtmListLoader.ReadModsFromFile(file.complete_path)).ToList();
            uniprotModifications = make_modification_dictionary(all_known_modifications);

            Dictionary<string, Modification> um;
            Parallel.ForEach(get_files(input_files, Purpose.ProteinDatabase).ToList(), database =>
            {
                lock (theoretical_proteins) theoretical_proteins.Add(database, ProteinDbLoader.LoadProteinXML(database.complete_path, false, all_known_modifications, database.ContaminantDB, mod_types_to_exclude, out um).ToArray());
                lock (all_known_modifications) all_known_modifications.AddRange(ProteinDbLoader.GetPtmListFromProteinXml(database.complete_path).OfType<ModificationWithLocation>().Where(m => !mod_types_to_exclude.Contains(m.modificationType)));
            });

            foreach (string filename in Directory.GetFiles(Path.Combine(current_directory, "Mods")))
            {
                IEnumerable<ModificationWithLocation> new_mods = !filename.EndsWith("variable.txt") || methionine_oxidation ?
                    PtmListLoader.ReadModsFromFile(filename) :
                    new List<ModificationWithLocation>(); // Empty variable modifications if not selected
                if (filename.EndsWith("variable.txt")) variableModifications = new_mods.OfType<ModificationWithMass>().ToList();
                all_known_modifications.AddRange(new_mods);
            }

            all_known_modifications = new HashSet<ModificationWithLocation>(all_known_modifications).ToList();
            uniprotModifications = make_modification_dictionary(all_known_modifications);
            all_mods_with_mass = uniprotModifications.SelectMany(kv => kv.Value).OfType<ModificationWithMass>().Concat(variableModifications).ToList();

            modification_ranks = rank_mods(theoretical_proteins, variableModifications, all_mods_with_mass);
            
            // Generate all ptm sets if not done already
            if (all_possible_ptmsets == null)
            {
                //Generate all two-member sets and all three-member (or greater) sets of the same modification (three-member combinitorics gets out of hand for assignment)
                all_possible_ptmsets = PtmCombos.generate_all_ptmsets(Math.Min(2, max_ptms), all_mods_with_mass, modification_ranks, rank_first_quartile / 2).ToList();
                for (int i = 3; i < max_ptms + 1; i++)
                {
                    all_possible_ptmsets.AddRange(all_mods_with_mass.Select(m => new PtmSet(Enumerable.Repeat(new Ptm(-1, m), i).ToList(), modification_ranks, rank_first_quartile / 2)));
                }
            }

            //Generate lookup table for ptm sets based on rounded mass of eligible PTMs -- used in forming ET relations
            if (possible_ptmset_dictionary.Count == 0)
                possible_ptmset_dictionary = make_ptmset_dictionary();

            expanded_proteins = expand_protein_entries(theoretical_proteins.Values.SelectMany(p => p).ToArray());
            aaIsotopeMassList = new AminoAcidMasses(carbamidomethylation, natural_lysine_isotope_abundance, neucode_light_lysine, neucode_heavy_lysine).AA_Masses;
            if (combine_identical_sequences) expanded_proteins = group_proteins_by_sequence(expanded_proteins);

            expanded_proteins = expanded_proteins.OrderBy(x => x.OneBasedPossibleLocalizedModifications.Count).ToArray(); // Take on harder problems first to use parallelization more effectively
            process_entries(expanded_proteins, variableModifications);
            process_decoys(expanded_proteins, variableModifications);

            if (combine_theoretical_proteoforms_byMass)
            {
                proteoform_community.theoretical_proteoforms = group_proteoforms_by_mass(proteoform_community.theoretical_proteoforms);
                proteoform_community.decoy_proteoforms = proteoform_community.decoy_proteoforms.ToDictionary(kv => kv.Key, kv => group_proteoforms_by_mass(kv.Value) as TheoreticalProteoform[]);
            }
        }

        //Generate lookup table for ptm sets based on rounded mass of eligible PTMs -- used in forming ET relations
        public Dictionary<double, List<PtmSet>> make_ptmset_dictionary()
        {
            Dictionary<double, List<PtmSet>> possible_ptmsets = new Dictionary<double, List<PtmSet>>();
            foreach (PtmSet set in all_possible_ptmsets.Where(s => s.ptm_combination.Count == 1 || !s.ptm_combination.Select(ptm => ptm.modification).Any(m => m.monoisotopicMass == 0)).ToList())
            {
                for (int i = 0; i < 10; i++)
                {
                    double midpoint = Math.Round(set.mass, 1) - 0.5 + i * 0.1;
                    if (possible_ptmsets.TryGetValue(midpoint, out List<PtmSet> a)) a.Add(set);
                    else possible_ptmsets.Add(midpoint, new List<PtmSet> { set });
                }
            }
            return possible_ptmsets;
        }

        public Dictionary<string, IList<Modification>> make_modification_dictionary(IEnumerable<ModificationWithLocation> all_modifications)
        {
            Dictionary<string, IList<Modification>> mod_dict = new Dictionary<string, IList<Modification>>();
            foreach (var nice in all_modifications)
            {
                if (mod_dict.TryGetValue(nice.id, out IList<Modification> val)) val.Add(nice);
                else mod_dict.Add(nice.id, new List<Modification> { nice });
            }
            return mod_dict;
        }

        public Dictionary<double, int> rank_mods(Dictionary<InputFile, Protein[]> theoretical_proteins, IEnumerable<ModificationWithMass> variable_modifications, IEnumerable<ModificationWithMass> all_mods_with_mass)
        {
            Dictionary<double, int> mod_counts = new Dictionary<double, int>();

            foreach (ModificationWithMass m in theoretical_proteins.SelectMany(kv => kv.Value).SelectMany(p => p.OneBasedPossibleLocalizedModifications).SelectMany(kv => kv.Value).OfType<ModificationWithMass>().ToList())
            {
                if (!mod_counts.TryGetValue(m.monoisotopicMass, out int b)) mod_counts.Add(m.monoisotopicMass, 1);
                else mod_counts[m.monoisotopicMass]++;
            }
            List<KeyValuePair<double, int>> ordered_mod_counts = mod_counts.OrderByDescending(kv => kv.Value).ToList();

            int rank = 3;
            int last_count = 0;
            Dictionary<double, int> mod_ranks = new Dictionary<double, int>();
            foreach (KeyValuePair<double, int> mod_count in ordered_mod_counts)
            {
                mod_ranks.Add(mod_count.Key, rank);
                if (mod_count.Value < last_count) rank++;
                last_count = mod_count.Value;
            }

            //Give unmodified the best rank
            if (!mod_ranks.TryGetValue(0, out int a)) mod_ranks.Add(0, 0);
            else mod_ranks[0] = 0;

            //Give variable mods a good score
            foreach (ModificationWithMass m in variable_modifications)
            {
                if (!mod_ranks.TryGetValue(m.monoisotopicMass, out int b)) mod_ranks.Add(m.monoisotopicMass, 2);
                else mod_ranks[m.monoisotopicMass] = 2;
            }

            List<int> ranks = mod_ranks.Values.OrderBy(x => x).ToList();
            rank_first_quartile = ranks[ranks.Count / 4];
            rank_second_quartile = ranks[2 * ranks.Count / 4];
            rank_third_quartile = ranks[3 * ranks.Count / 4];
            rank_sum_threshold = ranks.Max();

            //Give the remaining mods the threshold value
            foreach (ModificationWithMass m in all_mods_with_mass)
            {
                if (!mod_ranks.TryGetValue(m.monoisotopicMass, out int lkj))
                    mod_ranks.Add(m.monoisotopicMass, rank_sum_threshold);
            }

            return mod_ranks;
        }

        public ProteinWithGoTerms[] expand_protein_entries(Protein[] proteins)
        {
            List<ProteinWithGoTerms> expanded_prots = new List<ProteinWithGoTerms>();
            foreach (Protein p in proteins)
            {
                List<ProteinWithGoTerms> new_prots = new List<ProteinWithGoTerms>();

                //Add full length product
                int begin = 1;
                int end = p.BaseSequence.Length;
                List<GoTerm> goTerms = p.DatabaseReferences.Where(reference => reference.Type == "GO").Select(reference => new GoTerm(reference)).ToList();
                int startPosAfterCleavage = Convert.ToInt32(methionine_cleavage && p.BaseSequence.StartsWith("M"));
                new_prots.Add(new ProteinWithGoTerms(
                    p.BaseSequence.Substring(begin + startPosAfterCleavage - 1, end - (begin + startPosAfterCleavage) + 1),
                    p.Accession + "_" + (begin + startPosAfterCleavage).ToString() + "full" + end.ToString(),
                    p.GeneNames,
                    p.OneBasedPossibleLocalizedModifications,
                    new int?[] { begin + startPosAfterCleavage },
                    new int?[] { end },
                    new string[] { methionine_cleavage && p.BaseSequence.StartsWith("M") ? "full-met-cleaved" : "full" },
                    p.Name, p.FullName, p.IsDecoy, p.IsContaminant, p.DatabaseReferences, goTerms));

                //Add fragments
                List<ProteolysisProduct> products = p.ProteolysisProducts.ToList();
                foreach (ProteolysisProduct product in p.ProteolysisProducts)
                {
                    string feature_type = product.Type.Replace(' ', '-');
                    if (!(feature_type == "peptide" || feature_type == "propeptide" || feature_type == "chain" || feature_type == "signal-peptide") 
                            || !product.OneBasedBeginPosition.HasValue 
                            || !product.OneBasedEndPosition.HasValue)
                        continue;
                    int feature_begin = (int)product.OneBasedBeginPosition;
                    int feature_end = (int)product.OneBasedEndPosition;
                    if (feature_begin < 1 || feature_end < 1)
                        continue;
                    bool feature_is_just_met_cleavage = methionine_cleavage && feature_begin == begin + 1 && feature_end == end;
                    string subsequence = p.BaseSequence.Substring(feature_begin - 1, feature_end - feature_begin + 1);
                    Dictionary<int, List<Modification>> segmented_ptms = p.OneBasedPossibleLocalizedModifications.Where(kv => kv.Key >= feature_begin && kv.Key <= feature_end).ToDictionary(kv => kv.Key, kv => kv.Value);
                    if (!feature_is_just_met_cleavage && subsequence.Length != p.BaseSequence.Length && subsequence.Length >= min_peptide_length)
                        new_prots.Add(new ProteinWithGoTerms(
                            subsequence,
                            p.Accession + "_" + feature_begin.ToString() + "frag" + feature_end.ToString(),
                            p.GeneNames,
                            segmented_ptms,
                            new int?[] { feature_begin },
                            new int?[] { feature_end },
                            new string[] { feature_type },
                            p.Name, p.FullName, p.IsDecoy, p.IsContaminant, p.DatabaseReferences, goTerms));
                }
                expanded_prots.AddRange(new_prots);
            }
            return expanded_prots.ToArray();
        }

        public ProteinSequenceGroup[] group_proteins_by_sequence(IEnumerable<ProteinWithGoTerms> proteins)
        {
            Dictionary<string, List<ProteinWithGoTerms>> sequence_groupings = new Dictionary<string, List<ProteinWithGoTerms>>();
            foreach (ProteinWithGoTerms p in proteins)
            {
                if (sequence_groupings.ContainsKey(p.BaseSequence)) sequence_groupings[p.BaseSequence].Add(p);
                else sequence_groupings.Add(p.BaseSequence, new List<ProteinWithGoTerms> { p });
            }
            return sequence_groupings.Select(kv => new ProteinSequenceGroup(kv.Value.OrderByDescending(p => p.IsContaminant ? 1 : 0))).ToArray();
        }

        public TheoreticalProteoformGroup[] group_proteoforms_by_mass(IEnumerable<TheoreticalProteoform> theoreticals)
        {
            Dictionary<double, List<TheoreticalProteoform>> mass_groupings = new Dictionary<double, List<TheoreticalProteoform>>();
            foreach (TheoreticalProteoform t in theoreticals)
            {
                if (mass_groupings.ContainsKey(t.modified_mass)) mass_groupings[t.modified_mass].Add(t);
                else mass_groupings.Add(t.modified_mass, new List<TheoreticalProteoform> { t });
            }
            return mass_groupings.Select(kv => new TheoreticalProteoformGroup(kv.Value.OrderByDescending(t => t.contaminant ? 1 : 0))).ToArray();
        }

        private void process_entries(IEnumerable<ProteinWithGoTerms> expanded_proteins, IEnumerable<ModificationWithMass> variableModifications)
        {
            List<TheoreticalProteoform> theoretical_proteoforms = new List<TheoreticalProteoform>();
            Parallel.ForEach(expanded_proteins, p => EnterTheoreticalProteformFamily(p.BaseSequence, p, p.Accession, theoretical_proteoforms, -100, variableModifications));
            proteoform_community.theoretical_proteoforms = theoretical_proteoforms.ToArray();
        }

        private void process_decoys(ProteinWithGoTerms[] expanded_proteins, IEnumerable<ModificationWithMass> variableModifications)
        {
            Parallel.For(0, decoy_databases, decoyNumber =>
            {
                List<TheoreticalProteoform> decoy_proteoforms = new List<TheoreticalProteoform>();
                string giantProtein = GetOneGiantProtein(expanded_proteins, methionine_cleavage); //Concatenate a giant protein out of all protein read from the UniProt-XML, and construct target and decoy proteoform databases
                string decoy_database_name = decoy_database_name_prefix + decoyNumber;
                ProteinWithGoTerms[] shuffled_proteins = new ProteinWithGoTerms[expanded_proteins.Length];
                Array.Copy(expanded_proteins, shuffled_proteins, expanded_proteins.Length);
                new Random().Shuffle(shuffled_proteins); //randomize order of protein array

                int prevLength = 0;
                Parallel.ForEach(shuffled_proteins, p =>
                {
                    string hunk = giantProtein.Substring(prevLength, p.BaseSequence.Length);
                    prevLength += p.BaseSequence.Length;
                    EnterTheoreticalProteformFamily(hunk, p, p.Accession + "_DECOY_" + decoyNumber, decoy_proteoforms, decoyNumber, variableModifications);
                });

                lock (proteoform_community)
                    proteoform_community.decoy_proteoforms.Add(decoy_database_name, decoy_proteoforms.ToArray());
            });
        }

        private void EnterTheoreticalProteformFamily(string seq, ProteinWithGoTerms prot, string accession, List<TheoreticalProteoform> theoretical_proteoforms, int decoy_number, IEnumerable<ModificationWithMass> variableModifications)
        {
            //Calculate the properties of this sequence
            double unmodified_mass = TheoreticalProteoform.CalculateProteoformMass(seq, aaIsotopeMassList);
            int lysine_count = seq.Split('K').Length - 1;
            bool check_contaminants = theoretical_proteins.Any(item => item.Key.ContaminantDB);

            //Figure out the possible ptm sets
            Dictionary<int, List<Modification>> possibleLocalizedMods = new Dictionary<int, List<Modification>>(prot.OneBasedPossibleLocalizedModifications);
            foreach (ModificationWithMass m in variableModifications)
            {
                for (int i = 1; i <= prot.BaseSequence.Length; i++)
                {
                    if (prot.BaseSequence[i - 1].ToString() == m.motif.Motif)
                    {
                        if (!possibleLocalizedMods.TryGetValue(i, out List<Modification> a)) possibleLocalizedMods.Add(i, new List<Modification> { m });
                        else a.Add(m);
                    }
                }
            }

            List<PtmSet> unique_ptm_groups = PtmCombos.get_combinations(possibleLocalizedMods, max_ptms, modification_ranks, rank_first_quartile / 2);

            //Enumerate the ptm combinations with _P# to distinguish from the counts in ProteinSequenceGroups (_#G) and TheoreticalPfGps (_#T)
            int ptm_set_counter = 1;
            foreach (PtmSet ptm_set in unique_ptm_groups)
            {
                lock (theoretical_proteoforms) theoretical_proteoforms.Add(
                    new TheoreticalProteoform(
                        accession + "_P" + ptm_set_counter.ToString(),
                        prot.FullDescription + "_P" + ptm_set_counter.ToString() + (decoy_number < 0 ? "" : "_DECOY_" + decoy_number.ToString()),
                        new ProteinWithGoTerms[] { prot },
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

        private string GetOneGiantProtein(IEnumerable<Protein> proteins, bool methionine_cleavage)
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

        #endregion THEORETICAL DATABASE

        #region ET,ED,EE,EF COMPARISONS Public Fields

        public double ee_max_mass_difference = 300;
        public double ee_max_RetentionTime_difference = 2.5;
        public double et_low_mass_difference = -300;
        public double et_high_mass_difference = 300;
        public double no_mans_land_lowerBound = 0.22;
        public double no_mans_land_upperBound = 0.88;
        public double peak_width_base_et = 0.03; //need to be separate so you can change one and not other. 
        public double peak_width_base_ee = 0.03;
        public double min_peak_count_et = 5;
        public double min_peak_count_ee = 10;
        public int relation_group_centering_iterations = 2;  // is this just arbitrary? whys is it specified here?

        [NonSerialized]
        public List<ProteoformRelation> et_relations = new List<ProteoformRelation>();

        [NonSerialized]
        public List<ProteoformRelation> ee_relations = new List<ProteoformRelation>();

        [NonSerialized]
        public Dictionary<string, List<ProteoformRelation>> ed_relations = new Dictionary<string, List<ProteoformRelation>>();

        [NonSerialized]
        public Dictionary<string, List<ProteoformRelation>> ef_relations = new Dictionary<string, List<ProteoformRelation>>();

        [NonSerialized]
        public List<DeltaMassPeak> et_peaks = new List<DeltaMassPeak>();

        [NonSerialized]
        public List<DeltaMassPeak> ee_peaks = new List<DeltaMassPeak>();

        #endregion ET,ED,EE,EF COMPARISONS Public Fields

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

        public string numerator_condition = "";
        public string denominator_condition = "";

        [NonSerialized]
        public SortedDictionary<decimal, int> logIntensityHistogram = new SortedDictionary<decimal, int>();

        [NonSerialized]
        public SortedDictionary<decimal, int> logSelectIntensityHistogram = new SortedDictionary<decimal, int>();
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

        [NonSerialized]
        public List<ExperimentalProteoform> satisfactoryProteoforms = new List<ExperimentalProteoform>(); // these are proteoforms meeting the required number of observations.
        public IEnumerable<decimal> permutedTestStatistics;
        public static string[] observation_requirement_possibilities = new string[] { "Minimum Bioreps with Observations From Any Single Condition", "Minimum Bioreps with Observations From Any Condition", "Minimum Bioreps with Observations From Each Condition" };
        public string observation_requirement = observation_requirement_possibilities[0];
        public int minBiorepsWithObservations = 1;
        public decimal selectGaussianHeight;

        [NonSerialized]
        public List<ExperimentalProteoform.quantitativeValues> qVals = new List<ExperimentalProteoform.quantitativeValues>();
        public decimal sKnot_minFoldChange = 1m;
        public List<decimal> sortedProteoformTestStatistics = new List<decimal>();
        public List<decimal> sortedAvgPermutationTestStatistics = new List<decimal>();
        public decimal offsetTestStatistics = 1m;
        //public decimal negativeOffsetTestStatistics = -1m;
        public decimal offsetFDR;

        [NonSerialized]
        public List<ProteinWithGoTerms> observedProteins = new List<ProteinWithGoTerms>(); //This is the complete list of observed proteins

        [NonSerialized]
        public List<ProteinWithGoTerms> quantifiedProteins = new List<ProteinWithGoTerms>(); //This is the complete list of proteins that were quantified and included in any accepted proteoform family

        [NonSerialized]
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
            permutedTestStatistics = satisfactoryProteoforms.SelectMany(eP => eP.quant.permutedTestStatistics);
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
                eP.quant.FDR = ExperimentalProteoform.quantitativeValues.computeExperimentalProteoformFDR(eP.quant.testStatistic, permutedTestStatistics, satisfactoryProteoforms.Count, sortedProteoformTestStatistics);
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
                backgroundProteinsForGoAnalysis = expanded_proteins.Where(p => p.AccessionList.Any(acc => protein_accessions.Contains(acc.Split('_')[0]))).DistinctBy(pwg => pwg.Accession.Split('_')[0]).ToList();
            }
            else
            {
                backgroundProteinsForGoAnalysis = allTheoreticalProteins ? 
                    expanded_proteins.DistinctBy(pwg => pwg.Accession.Split('_')[0]).ToList() : 
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
    }
}
