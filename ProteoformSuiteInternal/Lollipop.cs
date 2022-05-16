﻿using Accord.Math;
using Chemistry;
using IO.MzML;
using ThermoRawFileReader;
using MassSpectrometry;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UsefulProteomicsDatabases;
using System.Text;
using Proteomics;
using MzLibUtil;

namespace ProteoformSuiteInternal
{
    public class Lollipop
    { 
        #region Constants

        public static double MONOISOTOPIC_UNIT_MASS = 1.0023; // updated 161007
        public static double NEUCODE_LYSINE_MASS_SHIFT = 0.036015372;
        public static double CYSTAG_MASS_SHIFT = 0.045258;
        public static double PROTON_MASS = 1.007276474;

        #endregion Constants

        public Lollipop()
        {
            Loaders.LoadElements();
        }

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

        public static readonly string[] file_lists = new[]
        {
            "Deconvolution Results for Identification (.xlsx, .tsv, .txt)",
            "Deconvolution Results for Quantification (.xlsx, .tsv. txt)",
            "Protein Databases (.xml, .xml.gz, .fasta)",
            "Top-Down Hit Results (.xlsx, .psmtsv )",
            "Spectra Files (.raw, .mzML)",
            "Uncalibrated Deconvolution Results (.xlsx, .tsv)",
            "Uncalibrated TDPortal Top-Down Hit Results (Unlabeled) (.xlsx)",
            "MetaMorpheus Bottom-Up Unique Peptides (.psmtsv)"

        };

        public static readonly List<string>[] acceptable_extensions = new[]
        {
            new List<string> { ".xlsx", ".tsv", ".txt" },
            new List<string> { ".xlsx", ".tsv", ".txt"  },
            new List<string> { ".xml", ".gz", ".fasta" },
            new List<string> { ".xlsx" , ".psmtsv"},
            new List<string> {".raw", ".mzML", ".mzml", ".MZML"},
            new List<string> { ".xlsx", ".tsv" },
            new List<string> { ".xlsx", ".psmtsv" },
            new List<string> { ".psmtsv"}

        };

        public static readonly string[] file_filters = new[]
        {
            "Deconvolution Files (*.xlsx, *.tsv, *.txt) | *.xlsx;*.tsv;*.txt",
            "Deconvolution Files (*.xlsx, *.tsv, *.txt) | *.xlsx;*.tsv;*.txt",
            "Protein Databases (*.xml, *.xml.gz, *.fasta) | *.xml;*.xml.gz;*.fasta",
            "Top-Down Hit Files (*.xlsx, *.psmtsv) | *.xlsx;*.psmtsv",
            "Spectra Files (*.raw, *.mzML) | *.raw;*.mzML",
            "Deconvolution Files (*.xlsx, *.tsv) | *.xlsx;*.tsv",
            "Top-Down Hit Files (*.xlsx, *.psmtsv) | *.xlsx;*.psmtsv",
            "MetaMorpheus Bottom-Up Unique Peptides (*.psmtsv) | *.psmtsv",
        };

        public static readonly List<Purpose>[] file_types = new[]
        {
            new List<Purpose> { Purpose.Identification },
            new List<Purpose> { Purpose.Quantification },
            new List<Purpose> { Purpose.ProteinDatabase },
            new List<Purpose> { Purpose.TopDown },
            new List<Purpose> { Purpose.SpectraFile },
            new List<Purpose> { Purpose.CalibrationIdentification },
            new List<Purpose> { Purpose.CalibrationTopDown },
            new List<Purpose> { Purpose.BottomUp }

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
                Labeling label;
                if (neucode_labeled)
                    label = Labeling.NeuCode;
                else if (cystag_labeled)
                    label = Labeling.Cystag;
                else
                    label = Labeling.Unlabeled;

                if (acceptable_extensions.Contains(extension) && !destination.Where(f => purposes.Contains(f.purpose)).Any(f => f.filename == filename))
                {
                    InputFile file;
                    if (!purposes.Contains(Purpose.ProteinDatabase))
                    {
                        file = new InputFile(complete_path, label, purposes.FirstOrDefault());
                    }
                    else
                    {
                        file = new InputFile(complete_path, Purpose.ProteinDatabase);
                        file.ContaminantDB = file.filename.Contains("cRAP");
                    }
                    destination.Add(file);
                    if (user_directed)
                    {
                        Sweet.add_file_action(file);
                    }
                }
            }

            Sweet.update_files_from_presets(destination);
        }

        #endregion Input Files

        #region RAW EXPERIMENTAL COMPONENTS Public Fields

        public List<Component> raw_experimental_components = new List<Component>();
        public List<Component> raw_quantification_components = new List<Component>();
        public bool neucode_labeled = false;
        public bool cystag_labeled = false;
        public double raw_component_mass_tolerance = 5;
        public double minIC = 0.7;
        public double minCC = 0.7;

        #endregion RAW EXPERIMENTAL COMPONENTS Public Fields

        #region RAW EXPERIMENTAL COMPONENTS

        public void process_raw_components(List<InputFile> input_files, List<Component> destination, Purpose purpose, bool remove_missed_monos_and_harmonics)
        {
            ComponentReader.components_with_errors.Clear();
            Parallel.ForEach(input_files.Where(f => f.purpose == purpose).ToList(), file =>
            {
                List<Component> someComponents = file.extension == ".xlsx" ? file.reader.read_components_from_xlsx(file, remove_missed_monos_and_harmonics)
                        : file.reader.read_components_from_tsv(file, false);
                lock (destination) destination.AddRange(someComponents);
            });

            if ((neucode_labeled || cystag_labeled) && purpose == Purpose.Identification)
            {
                process_neucode_components(raw_neucode_pairs);
            }
        }

        /* This currently only pairs components that have the same scan range, however I feel like it would be more robust to require that the components are within some retention time tolerance, especially considering
         that FLASHDeconv does not provide scan numbers for the components. -@JGP */
        public void process_neucode_components(List<NeuCodePair> raw_neucode_pairs)
        {
            foreach (InputFile inputFile in get_files(input_files, Purpose.Identification).ToList())
            {
                foreach(string scan_range in inputFile.reader.scan_ranges)
                {
                    find_neucode_pairs(inputFile.reader.final_components.Where(c => c.min_scan + "-" + c.max_scan == scan_range), raw_neucode_pairs, heavy_hashed_pairs);
                }
            }
            //raw_neucode_pairs = findMissing_ExtraLabels(raw_neucode_pairs).ToList();
        }

        #endregion RAW EXPERIMENTAL COMPONENTS

        #region DECONVOLUTION

        public string flash_deconv(int maxcharge, int mincharge, string directory)
        {
            int successfully_deconvoluted_files = 0;
            Loaders.LoadElements();

            string flashdeconv_location = directory + @"\FLASHDeconv\FLASHDeconv";

            foreach (InputFile f in input_files.Where(f => f.purpose == Purpose.SpectraFile))
            {
                string filelocation = Path.Combine(Path.GetDirectoryName(f.complete_path), Path.GetFileNameWithoutExtension(f.complete_path));

                if(f.extension == ".raw")
                {
                    //var myMsDataFile = ThermoRawFileReaderData.LoadAllStaticData(f.complete_path);
                    //MzmlMethods.CreateAndWriteMyMzmlWithCalibratedSpectra(myMsDataFile,
                    //f.directory + "\\" + f.filename + "PS.mzML", false);
                    //filelocation = f.directory + "\\" + f.filename + "PS";
                     return "Error: please convert .raw files to .mzML";
                }

                Process proc = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                if (File.Exists(Path.Combine(filelocation + ".tsv")))
                {
                    File.Delete(Path.Combine(filelocation + ".tsv"));
                }

                if (File.Exists(@"C:\WINDOWS\system32\cmd.exe"))
                {
                    startInfo.FileName = @"C:\WINDOWS\system32\cmd.exe";
                }
                else
                {
                    return "Please ensure that the command line executable is in " + @"C:\WINDOWS\system32\cmd.exe" + flashdeconv_location;
                }

                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardInput = true;
                startInfo.RedirectStandardOutput = false;
                startInfo.CreateNoWindow = true;
                proc.StartInfo = startInfo;

                proc.Start();

                proc.StandardInput.WriteLine("cd " + flashdeconv_location);
                string flash_deconv_string = ("FLASHDeconv.exe -in " + f.complete_path + " -out " + filelocation + " -minC " + mincharge + " -maxC "
                    + maxcharge);

                proc.StandardInput.WriteLine(flash_deconv_string);
                proc.StandardInput.Close();
                proc.WaitForExit();
                proc.Close();
                successfully_deconvoluted_files++;
            }

            if (successfully_deconvoluted_files == 1)
            {
                return "Successfully deconvoluted " + successfully_deconvoluted_files + " raw file.";
            }
            else if (successfully_deconvoluted_files > 1)
            {
                return "Successfully deconvoluted " + successfully_deconvoluted_files + " raw files.";
            }
            else
            {
                return "No files deconvoluted. Ensure correct file locations and try again.";
            }
        }

        #endregion DECONVOLUTION

        #region METAMORPHEUS TOPDOWN SEARCH

        public string metamorpheus_topdown(string timestamp, string directory, bool carbamidomethyl, double precursor_mass_tolerance, double product_mass_tolerance, DissociationType dissocation_type)
        {
            //set toml with new parameters
            string[] toml_params = File.ReadAllLines(Path.Combine(directory + "\\MetaMorpheus_CommandLine\\Task1-SearchTaskconfig.toml"));
            toml_params[45] = carbamidomethyl ?  "ListOfModsFixed = \"Common Fixed\tCarbamidomethyl on C\t\tCommon Fixed\tCarbamidomethyl on U\""
                : "ListOfModsFixed = \"\"";
            toml_params[53] = "ProductMassTolerance = \"±" + Math.Round(product_mass_tolerance, 4) + " PPM\"";
            toml_params[54] = "PrecursorMassTolerance = \"±" + Math.Round(precursor_mass_tolerance, 4) + " PPM\"";
            toml_params[69] = "DissociationType = \"" + dissocation_type + "\"";
            File.WriteAllLines(Path.Combine(directory + "\\MetaMorpheus_CommandLine\\Task1-SearchTaskconfig.toml"), toml_params);

            Loaders.LoadElements();
            Process proc = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();

            string metaMorpheusBuild = directory + @"\MetaMorpheus_CommandLine";

            if (File.Exists(@"C:\WINDOWS\system32\cmd.exe"))
            {
                startInfo.FileName = @"C:\WINDOWS\system32\cmd.exe";
            }
            else
            {
                return "Please ensure that the command line executable is in " + @"C:\WINDOWS\system32\cmd.exe";
            }

            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = false;
            startInfo.CreateNoWindow = true;
            proc.StartInfo = startInfo;

            proc.Start();

            proc.StandardInput.WriteLine("cd " + metaMorpheusBuild);

            string command = "dotnet CMD.dll -t Task1-SearchTaskconfig.toml -s ";
            foreach (var file in input_files.Where(f => f.purpose == Purpose.SpectraFile))
            {
                command += file.complete_path + " ";
            }
            command += "-d ";
            foreach (var file in input_files.Where(f => f.purpose == Purpose.ProteinDatabase))
            {
                command += file.complete_path + " ";
            }
            command += "-o " + Path.Combine(input_files.Where(f => f.purpose == Purpose.SpectraFile).First().directory, timestamp);

            proc.StandardInput.WriteLine(command);
            proc.StandardInput.WriteLine("y");

            proc.StandardInput.Close();
            proc.WaitForExit();
            proc.Close();
            return "Successfully ran MetaMorpheus top-down search.";
        }

        #endregion METAMORPHEUS TOPDOWN SEARCH

        #region NEUCODE PAIRS Public Fields

        public List<NeuCodePair> raw_neucode_pairs = new List<NeuCodePair>();
        public Dictionary<Component, List<NeuCodePair>> heavy_hashed_pairs = new Dictionary<Component, List<NeuCodePair>>();
        public decimal max_intensity_ratio = 6m;
        public decimal min_intensity_ratio = 1.4m;
        public decimal max_lysine_ct = 26.2m;
        public decimal min_lysine_ct = 1.5m;
        public decimal max_cysteine_ct = 20;
        public decimal min_cysteine_ct = 1m;

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
                    lock (lower_component)
                    {
                        lock (higher_component) // Turns out the LINQ queries in here, especially for overlapping_charge_states, aren't thread safe
                        {
                            double mass_difference = higher_component.weighted_monoisotopic_mass - lower_component.weighted_monoisotopic_mass;
                            if (mass_difference < 6)
                            {
                                List<int> lower_charges = lower_component.charge_states.Select(charge_state => charge_state.charge_count).ToList();
                                List<int> higher_charges = higher_component.charge_states.Select(charge_states => charge_states.charge_count).ToList();
                                HashSet<int> overlapping_charge_states = new HashSet<int>(lower_charges.Intersect(higher_charges));
                                double lower_intensity = NeuCodePair.calculate_sum_intensity_olcs(lower_component.charge_states, overlapping_charge_states);
                                double higher_intensity = NeuCodePair.calculate_sum_intensity_olcs(higher_component.charge_states, overlapping_charge_states);
                                bool light_is_lower = true; //calculation different depending on if neucode light is the heavier/lighter component
                                if (neucode_labeled && lower_intensity > 0 && higher_intensity > 0)
                                {
                                    NeuCodePair pair = lower_intensity > higher_intensity ?
                                        new NeuCodePair(lower_component, lower_intensity, higher_component, higher_intensity, mass_difference, overlapping_charge_states, light_is_lower) : //lower mass is neucode light
                                        new NeuCodePair(higher_component, higher_intensity, lower_component, lower_intensity, mass_difference, overlapping_charge_states, !light_is_lower); //higher mass is neucode light

                                    if (pair.weighted_monoisotopic_mass <= pair.neuCodeHeavy.weighted_monoisotopic_mass + MONOISOTOPIC_UNIT_MASS) // the heavy should be at higher mass. Max allowed is 1 dalton less than light.
                                    {
                                        lock (pairsInScanRange) pairsInScanRange.Add(pair);
                                    }
                                }
                                else if (cystag_labeled && lower_intensity > 0 && higher_intensity > 0)
                                {
                                    NeuCodePair pair = new NeuCodePair(lower_component, lower_intensity, higher_component, higher_intensity, mass_difference, overlapping_charge_states, light_is_lower);

                                    if (pair.weighted_monoisotopic_mass <= pair.neuCodeHeavy.weighted_monoisotopic_mass + MONOISOTOPIC_UNIT_MASS) // the heavy should be at higher mass. Max allowed is 1 dalton less than light.
                                    {
                                        lock (pairsInScanRange) pairsInScanRange.Add(pair);
                                    }
                                }
                            }
                        }
                    }
                }
            });

            pairsInScanRange = findMissing_ExtraLabels(pairsInScanRange).ToList();

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
                            if(!checkForDups_Mislabeled(destination.ToHashSet(),pair))
                            destination.Add(pair);
                        if (heavy_hashed_pairs.TryGetValue(pair.neuCodeHeavy, out List<NeuCodePair> paired))
                        {
                            lock (paired)
                                paired.Add(pair);
                        }
                        else
                        {
                            heavy_hashed_pairs.Add(pair.neuCodeHeavy, new List<NeuCodePair> { pair }); // already locked
                        }
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
 
        public static HashSet<NeuCodePair> findMissing_ExtraLabels(List<NeuCodePair> pairs)
        {
            //Assumes that the highest intensity 
            List<NeuCodePair> dupRemovedPairs = new List<NeuCodePair>();
            foreach(NeuCodePair pair in pairs)
            {
                if (!checkForDups_Mislabeled(dupRemovedPairs.ToHashSet(), pair))
                    dupRemovedPairs.Add(pair);
            }
            dupRemovedPairs = dupRemovedPairs.OrderByDescending(p => p.intensity_sum).ToList();
            HashSet<NeuCodePair> vetted_pairs = new HashSet<NeuCodePair>();

            foreach(NeuCodePair pair1 in dupRemovedPairs)
            {
                if(!pair1.mislabeled)
                {
                    bool foundMislabeled = false;
                    foreach (NeuCodePair pair2 in dupRemovedPairs)
                    {
                        if (!pair2.mislabeled)
                        {
                            //Find the cysteine count difference, then check if the mass difference is explained by a missing or extra label.
                            int cysteine_count_difference = pair1.cysteine_count - pair2.cysteine_count;
                            double expected_mass_difference = cysteine_count_difference * 222.158975;
                            double predicted_mass = pair2.neuCodeLight.weighted_monoisotopic_mass + expected_mass_difference;
                            double experimental_mass_difference = pair1.neuCodeLight.weighted_monoisotopic_mass - pair2.neuCodeLight.weighted_monoisotopic_mass;
                            PpmTolerance merging_tol = new PpmTolerance(10);
                            if (cysteine_count_difference != 0 && merging_tol.Within(pair1.neuCodeLight.weighted_monoisotopic_mass, predicted_mass))
                            {
                                pair1.mislabeled_components.Add(pair2.neuCodeLight);
                                pair1.mislabeled_components.Add(pair2.neuCodeHeavy);
                                vetted_pairs.Add(pair1);
                                pair2.mislabeled = true;
                                foundMislabeled = true;
                            }
                        }
                    }
                    if (!foundMislabeled && !checkForDups_Mislabeled(vetted_pairs,pair1))
                        vetted_pairs.Add(pair1);
                }
            }

            return vetted_pairs;
        }

        public static bool checkForDups_Mislabeled(HashSet<NeuCodePair> vetted, NeuCodePair current)
        {
            bool contains = false;
            foreach(NeuCodePair pair in vetted)
            {
                double light_diff = Math.Abs(pair.weighted_monoisotopic_mass - current.weighted_monoisotopic_mass);
                if ((pair.cysteine_count == current.cysteine_count && light_diff < 0.5) || (pair.mislabeled_components.Contains(current.neuCodeLight) || pair.mislabeled_components.Contains(current.neuCodeHeavy)))
                    contains = true;
            }
            return contains;
        }

        #endregion NEUCODE PAIRS

        #region TOPDOWN

        public double min_score_td = 3.0;
        public bool biomarker = true;
        public bool tight_abs_mass = true;
        public double td_retention_time_tolerance = 5; //min
        public List<SpectrumMatch> top_down_hits = new List<SpectrumMatch>();
        public List<TopDownProteoform> topdown_proteoforms = new List<TopDownProteoform>();
        public List<TopDownProteoform> topdown_proteoforms_no_theoretical = new List<TopDownProteoform>();
        public TDBUReader topdownReader = new TDBUReader();
        //C-score > 40: proteoform is both identified and fully characterized;
        //3 ≤ Cscore≤ 40: proteoform is identified, but only partially characterized;
        //C-score < 3: proteoform is neither identified nor characterized.

        public void read_in_td_hits()
        {
            Sweet.lollipop.top_down_hits.Clear();
            topdownReader.bad_ptms.Clear();
            foreach (InputFile file in input_files.Where(f => f.purpose == Purpose.TopDown).ToList())
            {
                top_down_hits.AddRange(topdownReader.ReadTDFile(file));
            }
        }

        public List<TopDownProteoform> aggregate_td_hits(List<SpectrumMatch> top_down_hits, double min_score_td, bool biomarker, bool tight_abs_mass)
        {
            List<TopDownProteoform> topdown_proteoforms = new List<TopDownProteoform>();
            //get topdown hits that meet criteria
            List<SpectrumMatch> remaining_td_hits = top_down_hits.Where(h => h.score >= min_score_td && (h.tdResultType == TopDownResultType.MetaMorpheus || (biomarker && h.tdResultType == TopDownResultType.Biomarker) || (tight_abs_mass && h.tdResultType == TopDownResultType.TightAbsoluteMass))).OrderBy(h => h.ambiguous_matches.Count).ThenByDescending(h => h.score).ThenBy(h => h.qValue).ThenBy(h => h.reported_mass).ToList();

            List<string> unique_proteoform_ids = remaining_td_hits.Select(h => h.accession.Split('-')[0] + "_" + h.pfr_accession).Distinct().ToList();
            Parallel.ForEach(unique_proteoform_ids, pfr =>
            {
                List<SpectrumMatch> hits_by_pfr = remaining_td_hits.Where(h => h.accession.Split('-')[0] + "_" + h.pfr_accession == pfr).ToList();
                List<TopDownProteoform> first_aggregation = new List<TopDownProteoform>();
                //aggregate to td hit w/ highest c-score as root - 1st average for retention time
                while (hits_by_pfr.Count > 0)
                {
                    SpectrumMatch root = hits_by_pfr[0];
                    //ambiguous results - only include higher scoring one (if same scan, file, and p-value)
                    //find topdownhits within RT tol --> first average
                    double first_RT_average = hits_by_pfr.Where(h => Math.Abs(h.ms2_retention_time - root.ms2_retention_time) <= Convert.ToDouble(td_retention_time_tolerance)).Select(h => h.ms2_retention_time).Average();
                    List<SpectrumMatch> hits_to_aggregate = hits_by_pfr.Where(h => Math.Abs(h.ms2_retention_time - first_RT_average) <= Convert.ToDouble(td_retention_time_tolerance)).OrderByDescending(h => h.score).ToList();
                    root = hits_to_aggregate[0];
                    //candiate topdown hits are those with the same theoretical accession and PTMs --> need to also be within RT tolerance used for agg
                    TopDownProteoform new_pf = new TopDownProteoform(root.accession, hits_to_aggregate);
                    hits_by_pfr = hits_by_pfr.Except(new_pf.topdown_hits).ToList();

                    //could have cases where topdown proteoforms same accession, mass, diff PTMs --> need a diff accession
                    lock (topdown_proteoforms)
                    {
                        int count = topdown_proteoforms.Count(t => t.accession == new_pf.accession);
                        if (count > 0)
                        {
                            string[] old_accession = new_pf.accession.Split('_');
                            count += topdown_proteoforms.Count(t => t.accession.Split('_')[0] == old_accession[0] && t.accession.Split('_')[1] == old_accession[1]);
                            new_pf.accession = old_accession[0] + "_" + old_accession[1] + "_" + count + "TD";
                        }

                        topdown_proteoforms.Add(new_pf);
                    }
                }
            });

            List<TopDownProteoform> to_remove = new List<TopDownProteoform>();
            foreach (var proteoform in topdown_proteoforms)
            {
                //if all ambiguous matches are contained
                if (topdown_proteoforms.Where(p => p != proteoform && p.pfr_accession != proteoform.pfr_accession && Math.Abs(p.agg_rt - proteoform.agg_rt) <= Convert.ToDouble(td_retention_time_tolerance) && p.ambiguous_topdown_hits.Count <= proteoform.ambiguous_topdown_hits.Count)
                .Any(p => p.ambiguous_topdown_hits.Select(r => r.pfr_accession.Split('|')[0]).Concat(new List<string>() { p.pfr_accession.Split('|')[0] })
                .All(r => proteoform.ambiguous_topdown_hits.Select(h => h.pfr_accession.Split('|')[0]).Concat(new List<string>() { proteoform.pfr_accession.Split('|')[0] }).Contains(r))))
                {
                    to_remove.Add(proteoform);
                }
            }

            topdown_proteoforms = topdown_proteoforms.Except(to_remove).ToList();


            //add bottomup peptides
            if(Sweet.lollipop.theoretical_database.bottom_up_psm_by_accession.Count > 0)
            {
                Parallel.ForEach(topdown_proteoforms, p =>
                {
                    p.topdown_bottom_up_PSMs = Proteoform.get_possible_PSMs(p.accession, p.topdown_ptm_set, p.topdown_begin, p.topdown_end, true);
                    foreach(var ambiguous in p.ambiguous_topdown_hits)
                    {
                       ambiguous.bottom_up_PSMs = Proteoform.get_possible_PSMs(ambiguous.accession, new PtmSet(ambiguous.ptm_list), ambiguous.begin, ambiguous.end, true);
                    }
                    p.bu_PTMs_all_from_protein = p.setter_bu_PTMs_all_from_protein();
                });
            }
            return topdown_proteoforms;
        }

        //convert unlabeled mass to neucode light mass based on lysine count (used for topdown identifications)
        public double get_neucode_mass(double unlabeled_mass, int lysine_count, int cysteine_count)
        {
            if(neucode_labeled)
            {
                return unlabeled_mass - lysine_count * 128.094963 + lysine_count * 136.109162;
            }
            else
            {
                return unlabeled_mass - cysteine_count * 128.094963 + cysteine_count * 136.109162; //Update this eventually @JGP
            }
        }

        #endregion TOPDOWN

        #region AGGREGATED PROTEOFORMS Public Fields

        public ProteoformCommunity target_proteoform_community = new ProteoformCommunity();
        public Dictionary<string, ProteoformCommunity> decoy_proteoform_communities = new Dictionary<string, ProteoformCommunity>();
        public string decoy_community_name_prefix = "Decoy_Proteoform_Community_";
        public List<IAggregatable> remaining_to_aggregate = new List<IAggregatable>();
        public HashSet<Component> remaining_verification_components = new HashSet<Component>();
        public HashSet<Component> remaining_quantification_components = new HashSet<Component>();
        public bool validate_proteoforms = true;
        public double mass_tolerance = 5; //ppm
        public double retention_time_tolerance = 5; //min
        public int maximum_missed_monos = 3;
        public List<int> missed_monoisotopics_range = new List<int>();
        public int maximum_missed_lysines = 2;
        public int maximum_missed_cysteines = 2;
        public int min_num_CS = 1;
        public string agg_observation_requirement = observation_requirement_possibilities[0];
        public int agg_minBiorepsWithObservations = -1;
        public bool add_td_proteoforms = true;

        #endregion AGGREGATED PROTEOFORMS Public Fields

        #region AGGREGATED PROTEOFORMS Private Fields

        private List<ExperimentalProteoform> vetted_proteoforms = new List<ExperimentalProteoform>();

        private IAggregatable[] ordered_to_aggregate = new IAggregatable[0];

        #endregion AGGREGATED PROTEOFORMS Private Fields

        #region AGGREGATED PROTEOFORMS

        public List<ExperimentalProteoform> aggregate_proteoforms(bool two_pass_validation, IEnumerable<NeuCodePair> raw_neucode_pairs, IEnumerable<Component> raw_experimental_components, IEnumerable<Component> raw_quantification_components, int min_num_CS)
        {
            set_missed_monoisotopic_range();
            List<ExperimentalProteoform> candidateExperimentalProteoforms = createProteoforms(raw_neucode_pairs, raw_experimental_components, min_num_CS);
            vetted_proteoforms = two_pass_validation ?
                vetExperimentalProteoforms(candidateExperimentalProteoforms, raw_experimental_components, vetted_proteoforms) :
                candidateExperimentalProteoforms;
            List<string> conditions = input_files.Where(f => f.purpose == Purpose.Identification).Select(f => f.lt_condition).Concat(input_files.Select(f => f.hv_condition)).Distinct().ToList();
            vetted_proteoforms = determineAggProteoformsMeetingCriteria(conditions, vetted_proteoforms, agg_observation_requirement, agg_minBiorepsWithObservations);
            if (add_td_proteoforms) vetted_proteoforms = add_topdown_proteoforms(vetted_proteoforms, topdown_proteoforms);
            target_proteoform_community.experimental_proteoforms = vetted_proteoforms.ToArray();
            target_proteoform_community.community_number = -100;
            foreach (ProteoformCommunity community in decoy_proteoform_communities.Values)
            {
                community.experimental_proteoforms = Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Select(e => e.topdown_id ? new TopDownProteoform(e as TopDownProteoform) : new ExperimentalProteoform(e)).ToArray();
            }
            if (get_files(input_files, Purpose.Quantification).Count() > 0)
            {
                assignQuantificationComponents(vetted_proteoforms, raw_quantification_components);
            }
            return vetted_proteoforms;
        }

        public List<ExperimentalProteoform> determineAggProteoformsMeetingCriteria(List<string> conditions, IEnumerable<ExperimentalProteoform> experimental_proteoforms, string observation_requirement, int minBiorepsWithObservations)
        {
            List<ExperimentalProteoform> satisfactory_proteoforms = new List<ExperimentalProteoform>();

            if (observation_requirement.Contains("Bioreps") && observation_requirement.Contains("From Any Single Condition"))
                satisfactory_proteoforms = experimental_proteoforms.Where(eP => conditions.Any(c => eP.aggregated.Select(a => a.input_file).Where(bc => bc.lt_condition == c).Select(bc => bc.biological_replicate).Distinct().Count() >= minBiorepsWithObservations)).ToList();
            if (observation_requirement.Contains("Bioreps") && observation_requirement.Contains("From Each Condition"))
                satisfactory_proteoforms = experimental_proteoforms.Where(eP => conditions.All(c => eP.aggregated.Select(a => a.input_file).Where(bc => bc.lt_condition == c).Select(bc => bc.biological_replicate).Distinct().Count() >= minBiorepsWithObservations)).ToList();
            if (observation_requirement.Contains("Bioreps") && observation_requirement.Contains("From Any Condition"))
                satisfactory_proteoforms = experimental_proteoforms.Where(eP => eP.aggregated.Select(a => a.input_file).Select(bc => bc.lt_condition + bc.biological_replicate.ToString()).Distinct().Count() >= minBiorepsWithObservations).ToList();
            if (observation_requirement.Contains("Biorep+Techreps") && observation_requirement.Contains("From Any Single Condition"))
                satisfactory_proteoforms = experimental_proteoforms.Where(eP => conditions.Any(c => eP.aggregated.Select(a => a.input_file).Where(bc => bc.lt_condition == c).Select(bc => bc.biological_replicate + bc.technical_replicate).Distinct().Count() >= minBiorepsWithObservations)).ToList();
            if (observation_requirement.Contains("Biorep+Techreps") && observation_requirement.Contains("From Each Condition"))
                satisfactory_proteoforms = experimental_proteoforms.Where(eP => conditions.All(c => eP.aggregated.Select(a => a.input_file).Where(bc => bc.lt_condition == c).Select(bc => bc.biological_replicate + bc.technical_replicate).Distinct().Count() >= minBiorepsWithObservations)).ToList();
            if (observation_requirement.Contains("Biorep+Techreps") && observation_requirement.Contains("From Any Condition"))
                satisfactory_proteoforms = experimental_proteoforms.Where(eP => eP.aggregated.Select(a => a.input_file).Select(bc => bc.lt_condition + bc.biological_replicate + bc.technical_replicate).Distinct().Count() >= minBiorepsWithObservations).ToList();

            return satisfactory_proteoforms;
        }

        public void set_missed_monoisotopic_range()
        {
            missed_monoisotopics_range = Enumerable.Range(-maximum_missed_monos, maximum_missed_monos * 2 + 1).ToList();
        }

        public void assign_best_components_for_manual_validation(IEnumerable<ExperimentalProteoform> experimental_proteoforms)
        {
            foreach (ExperimentalProteoform pf in experimental_proteoforms)
            {
                if (pf as TopDownProteoform == null)
                {
                    pf.manual_validation_id = pf.find_manual_inspection_component(pf.aggregated);
                    pf.manual_validation_verification = pf.find_manual_inspection_component(pf.lt_verification_components.Concat(pf.hv_verification_components));
                }
                else
                {
                    SpectrumMatch best_hit = (pf as TopDownProteoform).topdown_hits.OrderByDescending(h => h.score).ThenBy(h => h.qValue).First();
                    pf.manual_validation_id = "File: " + best_hit.filename
                         + "; Scan: " + best_hit.ms2ScanNumber
                        + "; RT (min): " + best_hit.ms2_retention_time;
                }
                pf.manual_validation_quant = pf.find_manual_inspection_component(pf.lt_quant_components.Concat(pf.hv_quant_components));
            }
        }

        public List<ExperimentalProteoform> add_topdown_proteoforms(List<ExperimentalProteoform> vetted_proteoforms, List<TopDownProteoform> topdown_proteoforms)
        {
            foreach (TopDownProteoform topdown in topdown_proteoforms.OrderByDescending(t => t.topdown_hits.Max(h => h.score)).ThenBy(t => t.topdown_hits.Min(h => h.qValue)).ThenBy(t => t.topdown_hits.Count).ThenBy(t => t.agg_mass))
            {
                //only add nonambiguous...
                if (topdown.ambiguous_topdown_hits.Count > 0) continue;
                double mass = topdown.modified_mass;
                List<ProteoformRelation> all_td_relations = new List<ProteoformRelation>();
                List<ExperimentalProteoform> potential_matches = new List<ExperimentalProteoform>();
                //remove existing experimentals that correspond to this topdown
                foreach (int m in missed_monoisotopics_range)
                {
                    double shift = m * Lollipop.MONOISOTOPIC_UNIT_MASS;
                    double mass_tol = (mass + shift) / 1000000 * Convert.ToInt32(Sweet.lollipop.mass_tolerance);
                    double low = mass + shift - mass_tol;
                    double high = mass + shift + mass_tol;
                    potential_matches.AddRange(vetted_proteoforms.Where(ep => !ep.topdown_id && ep.modified_mass >= low && ep.modified_mass <= high && ((!neucode_labeled || ep.lysine_count == topdown.lysine_count) || (!cystag_labeled || ep.cysteine_count == topdown.cysteine_count))
                        && Math.Abs(ep.agg_rt - topdown.agg_rt) <= Convert.ToDouble(Sweet.lollipop.retention_time_tolerance)));
                }
                if (potential_matches.Count > 0)
                {
                    ExperimentalProteoform matching_experimental = potential_matches.OrderBy(p => Math.Abs(topdown.modified_mass - Math.Round(topdown.modified_mass - p.modified_mass, 0) * Lollipop.MONOISOTOPIC_UNIT_MASS)).ThenBy(p => Math.Round(topdown.modified_mass - p.modified_mass, 0)).First();
                    topdown.matching_experimental = matching_experimental;
                    vetted_proteoforms.Remove(matching_experimental);
                }
                else topdown.matching_experimental = null;
                vetted_proteoforms.Add(topdown);
            }
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

            ordered_to_aggregate = ((neucode_labeled || cystag_labeled) ? raw_neucode_pairs.OfType<IAggregatable>() : raw_experimental_components.OfType<IAggregatable>()).OrderByDescending(p => p.intensity_sum).Where(p => p.accepted == true && consecutive_charge_states(min_num_CS, p.charge_states)).ToArray();
            remaining_to_aggregate = new List<IAggregatable>(ordered_to_aggregate);

            IAggregatable root = ordered_to_aggregate.FirstOrDefault();
            List<ExperimentalProteoform> running = new List<ExperimentalProteoform>();
            List<Thread> active = new List<Thread>();
            while (remaining_to_aggregate.Count > 0 || active.Count > 0)
            {
                while (root != null && active.Count < Environment.ProcessorCount)
                {
                    ExperimentalProteoform new_pf = new ExperimentalProteoform("tbd", root, true);
                    Thread t = new Thread(new ThreadStart(new_pf.aggregate));
                    t.Start();
                    candidateExperimentalProteoforms.Add(new_pf);
                    running.Add(new_pf);
                    active.Add(t);
                    root = find_next_root(remaining_to_aggregate, running);
                }
                foreach (Thread t in active) t.Join();
                foreach (ExperimentalProteoform e in running) remaining_to_aggregate = remaining_to_aggregate.Except(e.aggregated).ToList();

                running.Clear();
                active.Clear();
                root = find_next_root(remaining_to_aggregate, running);
            }

            for (int i = 0; i < candidateExperimentalProteoforms.Count; i++)
            {
                candidateExperimentalProteoforms[i].accession = "E" + i;
            }

            return candidateExperimentalProteoforms;
        }

        public bool consecutive_charge_states(int num_charge_states, List<ChargeState> charge_states)
        {
            List<int> charges = charge_states.OrderBy(cs => cs.charge_count).Select(cs => cs.charge_count).ToList();
            if (charges.Count < num_charge_states) return false;
            foreach (int cs in charges)
            {
                int consecutive = 1;
                foreach (int next in charges)
                {
                    if (consecutive >= num_charge_states) return true;
                    if (next == cs || next < cs) continue;
                    if (next - cs == charges.IndexOf(next) - charges.IndexOf(cs)) consecutive++;
                    if (consecutive >= num_charge_states) return true;
                }
            }

           return false;
        }

        public IAggregatable find_next_root(List<IAggregatable> ordered, List<IAggregatable> running)
        {
            return ordered.FirstOrDefault(c =>
                running.All(d =>
                    c.weighted_monoisotopic_mass < d.weighted_monoisotopic_mass - 20 || c.weighted_monoisotopic_mass > d.weighted_monoisotopic_mass + 20));
        }

        public IAggregatable find_next_root(List<IAggregatable> ordered, List<ExperimentalProteoform> running)
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
                    if (e.lt_verification_components.Count > 0 || (neucode_labeled || cystag_labeled) && e.lt_verification_components.Count > 0 && e.hv_verification_components.Count > 0)
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
            Parallel.ForEach(vetted_proteoforms, e =>
            {
                e.lt_quant_components.Clear();
                e.hv_quant_components.Clear();
            });
            List<ExperimentalProteoform> proteoforms = vetted_proteoforms.OrderBy(e => e.topdown_id ? (e as TopDownProteoform).topdown_hits.Min(h => h.qValue) : 1e6).ThenByDescending(e => e.topdown_id ? (e as TopDownProteoform).topdown_hits.Max(h => h.score) : 0).ThenByDescending(e => e.agg_intensity).ToList();
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
            Sweet.lollipop.ordered_to_aggregate = new Component[0];
            Sweet.lollipop.remaining_to_aggregate.Clear();
            Sweet.lollipop.remaining_verification_components.Clear();
            Sweet.lollipop.remaining_quantification_components.Clear();
        }

        #endregion AGGREGATED PROTEOFORMS

        #region THEORETICAL DATABASE Public Fields

        public bool methionine_oxidation = false;
        public bool carbamidomethylation = false;
        public bool methionine_cleavage = true;
        public bool use_average_mass = false;
        public int max_ptms = 2;
        public int decoy_databases = 10;
        public int min_peptide_length = 7;
        public double ptmset_mass_tolerance = 0.00001;
        public bool combine_identical_sequences = true;
        public bool combine_theoretical_proteoforms_byMass = false;
        public string[] mod_types_to_exclude = new string[] { "Metal", "PeptideTermMod", "TrypticProduct", "TrypsinDigestedMod" };
        public Dictionary<double, int> modification_ranks = new Dictionary<double, int>();
        public int mod_rank_sum_threshold = 0; // set to the maximum rank of any single modification
        public int mod_rank_first_quartile = 0; // approximate quartiles used for heuristics with unranked modifications
        public int mod_rank_second_quartile = 0;
        public int mod_rank_third_quartile = 0;
        public TheoreticalProteoformDatabase theoretical_database = new TheoreticalProteoformDatabase();
        public TDBUReader bottomupReader = new TDBUReader();
        public double min_bu_peptides = 0;
        public bool limit_triples_and_greater = true;

        //public List<BottomUpPSM> BottomUpPSMList = new List<BottomUpPSM>();
        public bool useRandomSeed_decoys = true;

        public int randomSeed_decoys = 1;

        #endregion THEORETICAL DATABASE Public Fields

        #region ET,ED,EE,EF COMPARISONS Public Fields

        public bool ee_accept_peaks_based_on_rank = true;
        public bool et_use_notch = false;
        public bool ee_use_notch = false;
        public bool et_notch_ppm = true;
        public bool add_td_theoreticals = true;
        public bool et_bestETRelationOnly = false;
        public double notch_tolerance_et = 1;
        public bool ee_notch_ppm = true;
        public double notch_tolerance_ee = 1;
        public double ee_max_mass_difference = 300;
        public double ee_max_RetentionTime_difference = 2.5;
        public double et_low_mass_difference = -100;
        public double et_high_mass_difference = 100;
        public double peak_width_base_et = .02; //need to be separate so you can change one and not other.
        public double peak_width_base_ee = .02;
        public double min_peak_count_et = 50;
        public double min_peak_count_ee = 50;
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
                Sweet.lollipop.ed_relations.Add(key, Sweet.lollipop.decoy_proteoform_communities[key].relate(Sweet.lollipop.decoy_proteoform_communities[key].experimental_proteoforms, Sweet.lollipop.decoy_proteoform_communities[key].theoretical_proteoforms, ProteoformComparison.ExperimentalDecoy, Environment.CurrentDirectory, et_bestETRelationOnly));
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

        public bool count_adducts_as_identifications = false;
        public string family_build_folder_path = "";
        public bool gene_centric_families = true;
        public static string preferred_gene_label = "";
        public int deltaM_edge_display_rounding = 2;
        public bool only_assign_common_or_known_mods = true;
        public bool identify_from_td_nodes = false;
        public bool remove_bad_connections = false;
        public double id_ppm_tolerance = 10.0;
        public bool id_use_ppm_tolerance = false;
        public bool topdown_theoretical_reduce_ambiguity = false;
        public bool annotated_PTMs_reduce_ambiguity = false;

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

        public void construct_target_and_decoy_families()
        {
            //in case any were unaccepted before
            Parallel.ForEach(et_relations, r => r.Accepted = r.peak != null && r.peak.Accepted);
            Parallel.ForEach(ee_relations, r => r.Accepted = r.peak != null && r.peak.Accepted);
            Parallel.ForEach(ed_relations.Values.SelectMany(r => r), r => r.Accepted = r.peak != null && r.peak.Accepted);
            Parallel.ForEach(ef_relations.Values.SelectMany(r => r), r => r.Accepted = r.peak != null && r.peak.Accepted);

            Parallel.ForEach(et_relations, r => r.Identification = false);
            Parallel.ForEach(ee_relations, r => r.Identification = false);
            Parallel.ForEach(ef_relations.Values.SelectMany(r => r), r => r.Identification = false);
            Parallel.ForEach(ed_relations.Values.SelectMany(r => r), r => r.Identification = false);

            target_proteoform_community.construct_families();
            foreach (var decoys in decoy_proteoform_communities.Values) decoys.construct_families();

            if (remove_bad_connections)
            {
                clear_all_families();
                target_proteoform_community.construct_families();
                foreach (var decoys in decoy_proteoform_communities.Values) decoys.construct_families();
            }
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

        public void getConditionBiorepFractionLabels(bool neucode_labeled, bool cystag_labeled, List<InputFile> input_files) //examines the conditions and bioreps to determine the maximum number of observations to require for quantification
        {
            if (!input_files.Any(f => f.purpose == Purpose.Quantification))
                return;
            List<string> ltConditions = get_files(input_files, Purpose.Quantification).Select(f => f.lt_condition).Distinct().ToList();
            List<string> hvConditions = (neucode_labeled || cystag_labeled) ?
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

        // Imputation
        public decimal backgroundShift = -1.8m;

        public decimal backgroundWidth = 0.5m;
        public bool useRandomSeed_quant = false;
        public int randomSeed_quant = 1;

        // Log2FC statistics
        public Log2FoldChangeAnalysis Log2FoldChangeAnalysis = new Log2FoldChangeAnalysis();
        public double benjiHoch_fdr = 0.05;
        public double minFoldChange = 1.0;

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
        public int minBiorepsWithFoldChange = 1;
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

            if (useRandomSeed_quant)
                seeded = new Random(randomSeed_quant);

            computeBiorepIntensities(target_proteoform_community.experimental_proteoforms, ltconditions, hvconditions);
            satisfactoryProteoforms = determineProteoformsMeetingCriteria(conditions, target_proteoform_community.experimental_proteoforms, observation_requirement, minBiorepsWithObservations);
            observedProteins = getProteins(target_proteoform_community.experimental_proteoforms);
            quantifiedProteins = getProteins(satisfactoryProteoforms);
            qVals = satisfactoryProteoforms.Select(pf => pf.quant).ToList();

            TusherAnalysis1.QuantitativeDistributions.defineAllObservedIntensityDistribution(target_proteoform_community.experimental_proteoforms.SelectMany(pf => pf.biorepIntensityList).ToList<IBiorepIntensity>(), TusherAnalysis1.QuantitativeDistributions.logIntensityHistogram);
            TusherAnalysis1.QuantitativeDistributions.defineSelectObservedIntensityDistribution(satisfactoryProteoforms.SelectMany(pf => pf.biorepIntensityList), TusherAnalysis1.QuantitativeDistributions.logSelectIntensityHistogram);
            TusherAnalysis1.QuantitativeDistributions.defineBackgroundIntensityDistribution(quantBioFracCombos, satisfactoryProteoforms, condition_count, backgroundShift, backgroundWidth);

            TusherAnalysis2.QuantitativeDistributions.defineAllObservedIntensityDistribution(target_proteoform_community.experimental_proteoforms.SelectMany(pf => pf.biorepTechrepIntensityList).ToList<IBiorepIntensity>(), TusherAnalysis2.QuantitativeDistributions.logIntensityHistogram);
            TusherAnalysis2.QuantitativeDistributions.defineSelectObservedIntensityDistribution(satisfactoryProteoforms.SelectMany(pf => pf.biorepTechrepIntensityList), TusherAnalysis2.QuantitativeDistributions.logSelectIntensityHistogram);
            TusherAnalysis2.QuantitativeDistributions.defineBackgroundIntensityDistribution(quantBioFracCombos, satisfactoryProteoforms, condition_count, backgroundShift, backgroundWidth);

            TusherAnalysis1.compute_proteoform_statistics(satisfactoryProteoforms, TusherAnalysis1.QuantitativeDistributions.bkgdAverageIntensity, TusherAnalysis1.QuantitativeDistributions.bkgdStDev, conditionsBioReps, numerator_condition, denominator_condition, induced_condition, sKnot_minFoldChange, true); // includes normalization
            TusherAnalysis1.permutedRelativeDifferences = TusherAnalysis1.compute_balanced_biorep_permutation_relativeDifferences(conditionsBioReps, induced_condition, satisfactoryProteoforms, sKnot_minFoldChange);
            TusherAnalysis1.flattenedPermutedRelativeDifferences = TusherAnalysis1.permutedRelativeDifferences.SelectMany(x => x).ToList();
            TusherAnalysis1.computeSortedRelativeDifferences(satisfactoryProteoforms, TusherAnalysis1.permutedRelativeDifferences);
            TusherAnalysis1.relativeDifferenceFDR = TusherAnalysis1.computeRelativeDifferenceFDR(TusherAnalysis1.avgSortedPermutationRelativeDifferences, TusherAnalysis1.sortedProteoformRelativeDifferences, satisfactoryProteoforms, TusherAnalysis1.flattenedPermutedRelativeDifferences, offsetTestStatistics);
            TusherAnalysis1.computeIndividualExperimentalProteoformFDRs(satisfactoryProteoforms, TusherAnalysis1.flattenedPermutedRelativeDifferences, TusherAnalysis1.sortedProteoformRelativeDifferences);
            TusherAnalysis1.inducedOrRepressedProteins = getInducedOrRepressedProteins(satisfactoryProteoforms.Where(p => p.quant.TusherValues1.significant), TusherAnalysis1.GoAnalysis);

            TusherAnalysis2.compute_proteoform_statistics(satisfactoryProteoforms, TusherAnalysis2.QuantitativeDistributions.bkgdAverageIntensity, TusherAnalysis2.QuantitativeDistributions.bkgdStDev, conditionsBioReps, numerator_condition, denominator_condition, induced_condition, sKnot_minFoldChange, true); // includes normalization
            TusherAnalysis2.permutedRelativeDifferences = TusherAnalysis2.compute_balanced_biorep_permutation_relativeDifferences(conditionsBioReps, input_files, induced_condition, satisfactoryProteoforms, sKnot_minFoldChange);
            TusherAnalysis2.flattenedPermutedRelativeDifferences = TusherAnalysis2.permutedRelativeDifferences.SelectMany(x => x).ToList();
            TusherAnalysis2.computeSortedRelativeDifferences(satisfactoryProteoforms, TusherAnalysis2.permutedRelativeDifferences);
            TusherAnalysis2.relativeDifferenceFDR = TusherAnalysis2.computeRelativeDifferenceFDR(TusherAnalysis2.avgSortedPermutationRelativeDifferences, TusherAnalysis2.sortedProteoformRelativeDifferences, satisfactoryProteoforms, TusherAnalysis2.flattenedPermutedRelativeDifferences, offsetTestStatistics);
            TusherAnalysis2.computeIndividualExperimentalProteoformFDRs(satisfactoryProteoforms, TusherAnalysis2.flattenedPermutedRelativeDifferences, TusherAnalysis2.sortedProteoformRelativeDifferences);
            TusherAnalysis2.inducedOrRepressedProteins = getInducedOrRepressedProteins(satisfactoryProteoforms.Where(p => p.quant.TusherValues2.significant), TusherAnalysis2.GoAnalysis);

            Log2FoldChangeAnalysis.compute_proteoform_statistics(satisfactoryProteoforms, conditionsBioReps, numerator_condition, denominator_condition, induced_condition, sKnot_minFoldChange, true); // includes normalization
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

        public IEnumerable<ExperimentalProteoform> getInterestingProteoforms(IEnumerable<ExperimentalProteoform> significantProteoforms, GoAnalysis goAnalysis)
        {
            return significantProteoforms.Where(p =>
                 Math.Abs(p.quant.tusherlogFoldChange) > goAnalysis.minProteoformFoldChange
                && p.quant.intensitySum > goAnalysis.minProteoformIntensity);
        }

        public List<ProteinWithGoTerms> getInducedOrRepressedProteins(IEnumerable<ExperimentalProteoform> significantProteoforms, GoAnalysis goAnalysis)
        {
            return getInterestingProteoforms(significantProteoforms, goAnalysis)
                .Where(pf => pf.linked_proteoform_references != null && pf.linked_proteoform_references.FirstOrDefault() as TheoreticalProteoform != null)
                .Select(pf => pf.linked_proteoform_references.First() as TheoreticalProteoform)
                .SelectMany(t => t.ExpandedProteinList)
                .DistinctBy(pwg => pwg.Accession.Split('_')[0])
                .ToList();
        }

        public List<ProteoformFamily> getInterestingFamilies(IEnumerable<ExperimentalProteoform> significantProteoforms, GoAnalysis goAnalysis)
        {
            return getInterestingProteoforms(significantProteoforms, goAnalysis)
                .Select(e => e.family).Distinct().ToList();
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

        #region CALIBRATION

        public List<SpectrumMatch> td_hits_calibration = new List<SpectrumMatch>();
        public Dictionary<Tuple<string, double, double>, double> component_mz_correction = new Dictionary<Tuple<string, double, double>, double>(); //key is file, intensity, reported mass, value is corrected mz
        public Dictionary<Tuple<string, double, double>, double> component_RT_correction = new Dictionary<Tuple<string, double, double>, double>(); //key is file, intensity, reported mass, value is corrected RT
        public Dictionary<Tuple<string, double, double>, double> td_hit_mz_correction = new Dictionary<Tuple<string, double, double>, double>(); //key is filename, hit scan #, hit reported mass, value is corrected mass
        public Dictionary<Tuple<string, double, double>, double> td_hit_RT_correction = new Dictionary<Tuple<string, double, double>, double>(); //key is filename, hit scan #, hit reported mass, value is corrected RT
        public List<Component> calibration_components = new List<Component>();
        public List<string> filenames_did_not_calibrate = new List<string>();
        public bool calibrate_raw_files = false;
        public bool calibrate_td_files = false;
        public bool mass_calibration = true;
        public bool retention_time_calibration = true;
        public double cali_mass_tolerance = 10;
        public double cali_rt_tolerance = 5;

        public void read_in_calibration_td_hits()
        {
            td_hits_calibration.Clear();
            component_mz_correction.Clear();
            td_hit_mz_correction.Clear();
            td_hit_RT_correction.Clear();
            calibration_components.Clear();
            foreach (InputFile file in input_files.Where(f => f.purpose == Purpose.CalibrationTopDown))
            {
                td_hits_calibration.AddRange(topdownReader.ReadTDFile(file));
            }
        }

        public void get_td_hit_chargestates()
        {
            foreach (InputFile raw_file in input_files.Where(f => f.purpose == Purpose.SpectraFile))
            {
                if (Sweet.lollipop.td_hits_calibration.Any(f => f.filename == raw_file.filename))
                {
                    MsDataFile myMsDataFile = Path.GetExtension(raw_file.complete_path) == ".raw" ?
                        ThermoRawFileReaderData.LoadAllStaticData(raw_file.complete_path) :
                        null;
                    if (myMsDataFile == null) myMsDataFile = Mzml.LoadAllStaticData(raw_file.complete_path);
                    Parallel.ForEach(Sweet.lollipop.td_hits_calibration.Where(f => f.filename == raw_file.filename).ToList(), hit =>
                    {
                        int scanNum = myMsDataFile.GetClosestOneBasedSpectrumNumber(hit.ms2_retention_time);
                        if (myMsDataFile.GetOneBasedScan(scanNum) as MsDataScan != null && scanNum <= myMsDataFile.NumSpectra)
                        {
                            hit.charge = Convert.ToInt16(Math.Round(hit.reported_mass / (double)(myMsDataFile.GetOneBasedScan(scanNum) as MsDataScan).IsolationMz, 0)); //m / (m/z)  round to get charge
                            if (hit.charge > 0)
                            {
                                hit.mz = hit.reported_mass.ToMz(hit.charge);
                            }
                            else
                            {
                                hit.score = -100;
                            }
                        }
                        while (myMsDataFile.GetOneBasedScan(scanNum).MsnOrder > 1) scanNum--;
                        hit.ms1_scan = myMsDataFile.GetOneBasedScan(scanNum);
                        hit.technical_replicate = raw_file.technical_replicate;
                        hit.biological_replicate = raw_file.biological_replicate;
                        hit.fraction = raw_file.fraction;
                        hit.condition = raw_file.lt_condition;
                        hit.calibrated_retention_time = hit.ms2_retention_time;
                    });
                }
            }
        }

        public string calibrate_files()
        {
            if (input_files.Where(f => f.purpose == Purpose.SpectraFile || f.purpose == Purpose.CalibrationIdentification).Any(f => f.fraction == "" || f.biological_replicate == "" || f.technical_replicate == "" || f.lt_condition == ""))
                return "Label condition, fraction, biological replicate, and techincal replicate of all Uncalibrated Proteoform Identification Results and Raw Files.";
            if (input_files.Where(f => f.purpose == Purpose.SpectraFile).Any(f1 => input_files.Where(f => f.purpose == Purpose.SpectraFile).Any(f2 => f2 != f1 && f2.biological_replicate == f1.biological_replicate && f2.fraction == f1.fraction && f2.technical_replicate == f1.technical_replicate && f2.lt_condition == f1.lt_condition)))
                return "Error: Multiple raw files have the same labels for biological replicate, technical replicate, and fraction.";
            get_td_hit_chargestates();
            if (td_hits_calibration.Any(h => h.fraction == "" || h.biological_replicate == "" || h.technical_replicate == "" || h.condition == ""))
                return "Error: need to input all raw files for top-down hits: " + string.Join(", ", td_hits_calibration.Where(h => h.fraction == "" || h.biological_replicate == "" || h.technical_replicate == "" || h.condition == "").Select(h => h.filename).Distinct());

            //calibrate topdown hits retention time
            if (retention_time_calibration)
            {
                Calibration calibration = new Calibration();
                calibration.RetentionTimeCalibrateTopDownHits(td_hits_calibration);
                foreach (SpectrumMatch hit in Sweet.lollipop.td_hits_calibration)
                {
                    Tuple<string, double, double> key = new Tuple<string, double, double>(hit.filename, hit.ms2ScanNumber, hit.reported_mass);
                    if (!Sweet.lollipop.td_hit_RT_correction.ContainsKey(key)) Sweet.lollipop.td_hit_RT_correction.Add(key, Math.Round(hit.calibrated_retention_time, 8));
                }
            }

            foreach (string condition in input_files.Select(f => f.lt_condition).Distinct())
            {
                foreach (string biological_replicate in input_files.Where(f => f.lt_condition == condition)
                    .Select(f => f.biological_replicate).Distinct())
                {
                    foreach (string fraction in input_files
                        .Where(f => f.lt_condition == condition && f.biological_replicate == biological_replicate)
                        .Select(f => f.fraction).Distinct())
                    {
                        Calibration calibration = new Calibration();
                        calibration_components.Clear();
                        ComponentReader.components_with_errors.Clear();
                        process_raw_components(
                            input_files.Where(f =>
                                f.purpose == Purpose.CalibrationIdentification &&
                                ((Sweet.lollipop.neucode_labeled || Sweet.lollipop.cystag_labeled) ||
                                 f.biological_replicate == biological_replicate) && f.fraction == fraction &&
                                f.lt_condition == condition).ToList(), calibration_components,
                            Purpose.CalibrationIdentification, false);

                        foreach (var c in calibration_components)
                        {
                            if (c.charge_states.Count == 1 && c.charge_states.First().charge_count == 1)
                            {
                                int new_charge = Convert.ToInt16(c.weighted_monoisotopic_mass / 1000);
                                c.charge_states.First().mz_centroid = c.weighted_monoisotopic_mass.ToMz(new_charge);
                                c.charge_states.First().charge_count = new_charge;
                            }
                        }

                        if (ComponentReader.components_with_errors.Count > 0)
                        {
                            return "Error in Deconvolution Results File: " +
                                   string.Join(", ", ComponentReader.components_with_errors);
                        }

                        foreach (InputFile raw_file in input_files.Where(f =>
                            f.purpose == Purpose.SpectraFile && f.biological_replicate == biological_replicate &&
                            f.fraction == fraction && f.lt_condition == condition))
                        {
                            if (!Sweet.lollipop.calibrate_td_files &&
                                td_hits_calibration.Any(h => h.filename == raw_file.filename)) continue;

                            bool calibrated = calibration.Run_TdMzCal(raw_file,
                                td_hits_calibration.Where(h =>
                                        ((Sweet.lollipop.neucode_labeled || Sweet.lollipop.cystag_labeled) ||
                                         h.biological_replicate == raw_file.biological_replicate) &&
                                        h.fraction == raw_file.fraction && h.condition == raw_file.lt_condition)
                                    .ToList());
                            if (calibrated)
                            {
                                //determine component and td hit shifts
                                determine_shifts(raw_file);
                                //calibrate component xlsx files
                                foreach (InputFile f in input_files.Where(f =>
                                    f.lt_condition == raw_file.lt_condition && f.purpose ==
                                                                            Purpose.CalibrationIdentification
                                                                            && f.biological_replicate ==
                                                                            raw_file.biological_replicate &&
                                                                            f.fraction == raw_file.fraction
                                                                            && f.technical_replicate ==
                                                                            raw_file.technical_replicate))
                                {
                                    Calibration.calibrate_components_in_xlsx(f);
                                }
                            }
                            else filenames_did_not_calibrate.Add(raw_file.filename);
                        }
                    }
                }

            }

            if (calibrate_td_files)
            {
                foreach (InputFile file in input_files.Where(f => f.purpose == Purpose.CalibrationTopDown &&
                                                                  (td_hits_calibration.Any(h =>
                                                                      h.file == f &&
                                                                      td_hit_RT_correction.ContainsKey(
                                                                          new Tuple<string, double, double>(
                                                                              h.filename, h.ms2ScanNumber,
                                                                              h.reported_mass)) ||
                                                                      td_hit_mz_correction.ContainsKey(
                                                                          new Tuple<string, double, double>(
                                                                              h.filename, h.ms2ScanNumber,
                                                                              h.reported_mass))))))
                {
                    Calibration.calibrate_td_hits_file(file);
                }
            }



            return "Successfully calibrated files." + ((filenames_did_not_calibrate.Count > 0) ? (" The following files did not calibrate due to not enough calibration points: " + string.Join(", ", filenames_did_not_calibrate.Distinct())) : "");
        }

        public void determine_shifts(InputFile raw_file)
        {
            //calibrate components with same topdown file, biological replicate, fraction, and technical replicate
            Parallel.ForEach(calibration_components.Where(c => c.input_file.lt_condition == raw_file.lt_condition && c.input_file.biological_replicate == raw_file.biological_replicate && c.input_file.fraction == raw_file.fraction && c.input_file.technical_replicate == raw_file.technical_replicate),
                c =>
                {
                    if (mass_calibration)
                    {
                        if (c.input_file.extension == ".xlsx")
                        {
                            foreach (ChargeState cs in c.charge_states)
                            {
                                Tuple<string, double, double> key = new Tuple<string, double, double>(c.input_file.filename,
                                    Math.Round(cs.intensity, 0), Math.Round(cs.reported_mass, 2));
                                lock (component_mz_correction)
                                {
                                    if (!component_mz_correction.ContainsKey(key))
                                    {
                                        component_mz_correction.Add(key, Math.Round(cs.mz_centroid, 5));
                                    }
                                }
                            }
                        }
                        else
                        {
                            var key = new Tuple<string, double, double>(c.input_file.filename,
                                Math.Round(c.intensity_reported, 0), Math.Round(c.reported_monoisotopic_mass, 2));
                            var best_cs = c.charge_states.OrderByDescending(cs => cs.intensity).First();
                            lock (component_mz_correction)
                            {
                                if (!component_mz_correction.ContainsKey(key))
                                {
                                    component_mz_correction.Add(key,
                                        Math.Round(best_cs.mz_centroid.ToMass(best_cs.charge_count), 5));
                                }
                            }
                        }
                    }

                    if (retention_time_calibration)
                    {
                        var RT_key = new Tuple<string, double, double>(c.input_file.filename,
                            Math.Round(c.intensity_reported, 0), Math.Round(c.reported_monoisotopic_mass, 2));
                        lock (component_RT_correction)
                        {
                            if (!component_RT_correction.ContainsKey(RT_key))
                            {
                                component_RT_correction.Add(RT_key, Math.Round(c.rt_apex, 5));
                            }
                        }
                    }
                });

            if (calibrate_td_files)
            {
                //get topdown shifts if this SpectraFile is the same name as the topdown hit's file
                foreach (SpectrumMatch hit in Sweet.lollipop.td_hits_calibration.Where(h => h.filename == raw_file.filename))
                {
                    Tuple<string, double, double> key = new Tuple<string, double, double>(hit.filename, hit.ms2ScanNumber, hit.reported_mass);
                    if (!Sweet.lollipop.td_hit_mz_correction.ContainsKey(key)) lock (Sweet.lollipop.td_hit_mz_correction) Sweet.lollipop.td_hit_mz_correction.Add(key, Math.Round(hit.mz.ToMass(hit.charge), 8));
                }
            }
        }

#endregion

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
                    (p as ExperimentalProteoform).ambiguous_identifications.Clear();
                    (p as ExperimentalProteoform).bottom_up_PSMs.Clear();
                    p.gene_name = null;
                    p.begin = 0;
                    p.end = 0;
                    ProteoformRelation relation = null;
                    p.relation_to_id = relation;
                }

                foreach (Proteoform p in community.theoretical_proteoforms)
                {
                    p.relationships.RemoveAll(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical || r.RelationType == ProteoformComparison.ExperimentalDecoy);
                    p.family = null;
                }
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
                    (p as ExperimentalProteoform).ambiguous_identifications.Clear();
                    (p as ExperimentalProteoform).bottom_up_PSMs.Clear();
                    p.gene_name = null;
                    ProteoformRelation relation = null;
                    p.relation_to_id = relation;
                    p.begin = 0;
                    p.end = 0;
                }
            }
        }

        public void clear_td()
        {
            Sweet.lollipop.top_down_hits.Clear();
            Sweet.lollipop.topdown_proteoforms.Clear();
            Sweet.lollipop.topdown_proteoforms_no_theoretical.Clear();
            foreach (ProteoformCommunity community in decoy_proteoform_communities.Values.Concat(new List<ProteoformCommunity> { target_proteoform_community }))
            {
                List<TheoreticalProteoform> topdown_theoreticals = community.theoretical_proteoforms.Where(t => t.new_topdown_proteoform).ToList();
                Sweet.lollipop.theoretical_database.expanded_proteins.ToList().RemoveAll(t => t.topdown_protein);
                community.theoretical_proteoforms = community.theoretical_proteoforms.Except(topdown_theoreticals).ToArray();
                if (theoretical_database.theoreticals_by_accession.ContainsKey(community.community_number))
                {
                    Parallel.ForEach(theoretical_database.theoreticals_by_accession[community.community_number], list =>
                    {
                        list.Value.RemoveAll(t => t.new_topdown_proteoform);
                    });
                }
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