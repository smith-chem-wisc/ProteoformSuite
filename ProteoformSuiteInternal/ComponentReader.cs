﻿using System.Collections.Generic;
using System.Linq;
using System;
using Chemistry;

namespace ProteoformSuiteInternal
{
    public class ComponentReader
    {
        #region Private Fields

        private readonly List<Component> raw_components_in_file = new List<Component>();
        private static List<NeuCodePair> neucodePairs_in_file = new List<NeuCodePair>();
        private static Dictionary<Component, List<NeuCodePair>> heavy_hashed_pairs_in_file = new Dictionary<Component, List<NeuCodePair>>();

        #endregion Private Fields

        #region Public Fields

        public List<Component> final_components = new List<Component>();
        public List<string> scan_ranges = new List<string>();
        public int unprocessed_components;
        public int missed_mono_merges;
        public int harmonic_merges;

        public static List<string> components_with_errors = new List<string>(); //show warning with line number

        #endregion Public Fields

        #region Public Method

        public List<Component> read_components_from_xlsx(InputFile file, bool remove_missed_monos_and_harmonics)
        {
            this.raw_components_in_file.Clear();

            Component new_component = new Component();
            int charge_row_index = 0;
            string scan_range = "";
            List<List<string>> cells = ExcelReader.get_cell_strings(file, false);
            for (int i = 0; i < cells.Count; i++)
            {
                if (i == 0)
                {
                    continue; //skip component header
                }
                List<string> cellStrings = cells[i];
                if (cellStrings.Count > 10) //component row
                {
                    if (i > 1)
                    {
                        if (acceptable_component(new_component))
                        {
                            add_component(new_component); // here we're adding the previously read component
                        }
                        else
                        {
                            Clear();
                            return new List<Component>();
                        }
                    }
                    new_component = new Component(cellStrings, file); // starting fresh here with a newly created componet.
                    charge_row_index = 0;
                    scan_range = cellStrings[8];
                }
                else if (cellStrings.Count == 4) //charge-state row
                {
                    if (charge_row_index == 0)
                    {
                        charge_row_index += 1;
                        continue; //skip charge state headers=
                    }
                    else
                    {
                        new_component.charge_states.Add(new ChargeState(cellStrings));
                    }
                }
            }
            if (acceptable_component(new_component))
            {
                add_component(new_component);
            }
            else
            {
                Clear();
                return new List<Component>();
            }
            unprocessed_components += raw_components_in_file.Count;
            final_components = remove_missed_monos_and_harmonics ? remove_monoisotopic_duplicates_harmonics_from_same_scan(raw_components_in_file) : raw_components_in_file;
            scan_ranges = new HashSet<string>(final_components.Select(c => c.min_scan + "-" + c.max_scan)).ToList();
            return final_components;
        }

        public List<Component> read_components_from_tsv(InputFile file, bool remove_missed_monos_and_harmonics)
        {
            this.raw_components_in_file.Clear();
            Dictionary<int, List<string[]>> components_by_feature_ID = new Dictionary<int, List<string[]>>();
            string[] lines = System.IO.File.ReadAllLines(file.complete_path);
            for (int i = 1; i < lines.Length; i++)
            {
                string[] row = lines[i].Split('\t');
                if (row.Length == 20)
                {
                    if (Double.TryParse(row[16], out double likelihood_ratio) && likelihood_ratio >= Sweet.lollipop.min_likelihood_ratio
                        && Double.TryParse(row[19], out double fit) && fit <= Sweet.lollipop.max_fit)
                    {
                        List<string> charge_column = row[17].Split(',').ToList();
                        List<string> intensity_column = row[18].Split(',').ToList();
                        if (charge_column.Count != intensity_column.Count) continue;
                        List<int> charges = new List<int>();
                        List<double> intensities = new List<double>();
                        for(int a = 0; a < charge_column.Count; a++)
                        {
                            int asdfg = Int32.TryParse(charge_column[a], out int ok) ? ok : 0;
                            if (asdfg > 0)
                            {
                                charges.Add(ok);
                                intensities.Add(Double.TryParse(intensity_column[a], out double intensity) ? intensity : 0);
                            }
                        }
                        if (charges.Count < 1) continue;
                        List<string> cellStrings = new List<string>();
                        cellStrings.Add(row[0]); //id
                        cellStrings.Add(row[5]); //monoisotopic mass
                        cellStrings.Add(row[9]); //intensity
                        cellStrings.Add(charges.Count().ToString()); //num charges 
                        cellStrings.Add("0"); //num detected intervals
                        cellStrings.Add("0"); //reported delta mass
                        cellStrings.Add("0"); //relative abundance
                        cellStrings.Add("0"); //fractional abundance

                        cellStrings.Add(row[1] + "-" + row[2]); //scanrange
                        cellStrings.Add(row[12] + "-" + row[13]); //rt range
                        cellStrings.Add(row[10]);

                        Component c = new Component(cellStrings, file);

                        for(int a = 0; a < charges.Count; a++)
                        {
                            //must use monoisotopic mass reported to get m/z --> m/z reported in each row is NOT monoisotopic!! 
                            //multiply by charge for intensity because constructor divides (thermo is not charge state normalized!)
                            c.charge_states.Add(new ChargeState(new List<string>() { charges[a].ToString(), (intensities[a] * charges[a]).ToString(), (c.reported_monoisotopic_mass.ToMz(charges[a])).ToString(), c.reported_monoisotopic_mass.ToString() }));
                        }

                        c.calculate_properties();

                        if (acceptable_component(c))
                        {
                            add_component(c);
                        }
                        else
                        {
                            Clear();
                            return new List<Component>();
                        }
                    }
                }
            }

            unprocessed_components += raw_components_in_file.Count;
            final_components = remove_missed_monos_and_harmonics ? remove_monoisotopic_duplicates_harmonics_from_same_scan(raw_components_in_file) : raw_components_in_file;
            scan_ranges = new HashSet<string>(final_components.Select(c => c.min_scan + "-" + c.max_scan)).ToList();
            return final_components;
        }

        public bool acceptable_component(Component c)
        {
            if (c.min_rt <= 0 || c.max_rt <= 0 || c.max_scan <= 0 || c.min_scan <= 0 || c.charge_states.Count <= 0 || c.rt_apex <= 0)
            {
                lock (components_with_errors) components_with_errors.Add(c.input_file.filename + " component " + c.id.Split('_')[1]);
                return false;
            }
            foreach (ChargeState cs in c.charge_states)
            {
                if (cs.calculated_mass <= 0 || cs.mz_centroid <= 0 || cs.intensity <= 0 || cs.charge_count <= 0 )
                {
                    lock (components_with_errors) components_with_errors.Add(c.input_file.filename + " component " + c.id.Split('_')[1]);
                    return false;
                }
            }
            return true;
        }

        public void Clear()
        {
            final_components.Clear();
            scan_ranges.Clear();
            unprocessed_components = 0;
            missed_mono_merges = 0;
            harmonic_merges = 0;
        }

        #endregion Public Method

        #region Private Methods

        private void add_component(Component c)
        {
            c.calculate_properties();
            raw_components_in_file.Add(c);
        }

        public List<Component> remove_monoisotopic_duplicates_harmonics_from_same_scan(List<Component> raw_components)
        {
            List<string> scans = raw_components.Select(c => c.min_scan + "-" + c.max_scan).Distinct().ToList();
            HashSet<Component> removeThese = new HashSet<Component>();
            List<NeuCodePair> ncPairsInScan = new List<NeuCodePair>();

            foreach (string scan in scans)
            {
                List<Component> scanComps = raw_components.Where(c => c.min_scan + "-" + c.max_scan == scan).OrderByDescending(i => i.intensity_sum).ToList();
                foreach (Component sc in scanComps) // this loop compresses missed monoisotopics into a single component. components are sorted by mass. in one scan, there should only be one missed mono per
                {
                    if (removeThese.Contains(sc))
                    {
                        continue;
                    }

                    List<double> possibleMissedMonoisotopicsList =
                        Enumerable.Range(-3, 7).Select(x =>
                        sc.weighted_monoisotopic_mass + ((double)x) * Lollipop.MONOISOTOPIC_UNIT_MASS).ToList();

                    foreach (double missedMonoMass in possibleMissedMonoisotopicsList)
                    {
                        double massTolerance = missedMonoMass / 1000000d * Sweet.lollipop.raw_component_mass_tolerance;
                        List<Component> missedMonoisotopics = scanComps.Where(cp =>
                            !removeThese.Contains(cp)
                            && cp.weighted_monoisotopic_mass >= (missedMonoMass - massTolerance)
                            && cp.weighted_monoisotopic_mass <= (missedMonoMass + massTolerance)).ToList(); // this is a list of harmonics to hc

                        foreach (Component c in missedMonoisotopics.Where(m => m.id != sc.id).ToList())
                        {
                            missed_mono_merges++;
                            sc.mergeTheseComponents(c);
                            removeThese.Add(c);
                        }
                    }
                }

                if (Sweet.lollipop.neucode_labeled && raw_components.FirstOrDefault().input_file.purpose == Purpose.Identification) //before we compress harmonics, we have to determine if they are neucode labeled and lysine count 14. these have special considerations
                {
                    ncPairsInScan = Sweet.lollipop.find_neucode_pairs(scanComps.Except(removeThese), neucodePairs_in_file, heavy_hashed_pairs_in_file); // these are not the final neucode pairs, just a temp list
                }

                List<string> lysFourteenComponents = new List<string>();
                foreach (NeuCodePair ncp in ncPairsInScan)
                {
                    if (ncp.lysine_count == 14)
                    {
                        lysFourteenComponents.Add(ncp.neuCodeLight.id);
                        lysFourteenComponents.Add(ncp.neuCodeHeavy.id);
                    }
                }

                List<Component> someComponents = scanComps.OrderByDescending(w => w.weighted_monoisotopic_mass).ToList(); // sorting by mass means that all harmonics are a component are lower on the list.
                foreach (Component hc in someComponents)
                {
                    if (removeThese.Contains(hc))
                    {
                        continue;
                    }

                    List<double> possibleHarmonicList = // 2 missed on the top means up to 4 missed monos on the 2nd harmonic and 6 missed monos on the 3rd harmonic
                        Enumerable.Range(-4, 9).Select(x => (hc.weighted_monoisotopic_mass + ((double)x) * Lollipop.MONOISOTOPIC_UNIT_MASS) / 2d).Concat(
                            Enumerable.Range(-6, 13).Select(x => (hc.weighted_monoisotopic_mass + ((double)x) * Lollipop.MONOISOTOPIC_UNIT_MASS) / 3d)).ToList();

                    foreach (double harmonicMass in possibleHarmonicList)
                    {
                        double massTolerance = harmonicMass / 1000000d * Sweet.lollipop.raw_component_mass_tolerance;
                        List<Component> harmonics = scanComps.Where(cp =>
                            !removeThese.Contains(cp)
                            && cp.weighted_monoisotopic_mass >= (harmonicMass - massTolerance)
                            && cp.weighted_monoisotopic_mass <= (harmonicMass + massTolerance)).ToList(); // this is a list of harmonics to hc
                        List<Component> someHarmonics = harmonics.Where(harmonicComponent => harmonicComponent.id != hc.id).ToList();
                        foreach (Component h in someHarmonics) // now that we have a list of harmonics to hc, we have to figure out what to do with them
                        {
                            if (removeThese.Contains(h) || removeThese.Contains(hc))
                            {
                                continue;
                            }

                            int parCsCount = hc.charge_states.Count;
                            int cldCsCount = h.charge_states.Count;
                            double parMass = hc.weighted_monoisotopic_mass;
                            double cldMass = h.weighted_monoisotopic_mass;
                            bool fourteen = lysFourteenComponents.Contains(h.id);

                            if (lysFourteenComponents.Contains(h.id))
                            {
                                harmonic_merges++;
                                h.mergeTheseComponents(hc);
                                removeThese.Add(hc);
                            }
                            else
                            {
                                if (hc.charge_states.Count >= 4 && h.charge_states.Count >= 4)
                                {
                                    continue;
                                }

                                if (hc.charge_states.Count == h.charge_states.Count)
                                {
                                    hc.mergeTheseComponents(h);
                                    removeThese.Add(h);
                                }
                                else
                                {
                                    if (hc.charge_states.Count > h.charge_states.Count)
                                    {
                                        hc.mergeTheseComponents(h);
                                        removeThese.Add(h);
                                    }
                                    else
                                    {
                                        h.mergeTheseComponents(hc);
                                        removeThese.Add(hc);
                                    }
                                }
                                harmonic_merges++;
                            }
                        }
                    }
                }
            }
            return raw_components.Except(removeThese).ToList();
        }

        #endregion Private Methods
    }
}