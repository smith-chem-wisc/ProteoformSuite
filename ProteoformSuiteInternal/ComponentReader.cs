using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class ComponentReader
    {
        private List<Component> raw_components_in_file = new List<Component>();
        private static List<NeuCodePair> neucodePairs_in_file = new List<NeuCodePair>();
        public List<Component> final_components = new List<Component>();
        public HashSet<string> scan_ranges = new HashSet<string>();

        public List<Component> read_components_from_xlsx(InputFile file, IEnumerable<Correction>correctionFactors)
        {
            this.raw_components_in_file.Clear();
            string absolute_path = file.directory + "\\" + file.filename + file.extension;
            try
            {
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(absolute_path, false))
                {
                    // Get Data in Sheet1 of Excel file
                    IEnumerable<Sheet> sheetcollection = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>(); // Get all sheets in spread sheet document 
                    WorksheetPart worksheet_1 = (WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(sheetcollection.First().Id.Value); // Get sheet1 Part of Spread Sheet Document
                    SheetData sheet_1 = worksheet_1.Worksheet.Elements<SheetData>().First();
                    List<Row> rowcollection = worksheet_1.Worksheet.Descendants<Row>().ToList();

                    Component new_component = new Component();
                    int charge_row_index = 0;
                    string scan_range = "";
                    for (int i = 0; i < rowcollection.Count(); i++)
                    {
                        if (i == 0) continue; //skip component header
                        
                        IEnumerable<Cell> cells = rowcollection[i].Descendants<Cell>();

                        List<string> cellStrings = new List<string>();

                        for (int k = 0; k < rowcollection[i].Descendants<Cell>().Count(); k++)
                        {
                            cellStrings.Add(GetCellValue(spreadsheetDocument, rowcollection[i].Descendants<Cell>().ElementAt(k)));
                        }
                        
                        if (cellStrings.Count > 4) //component row
                        {
                            if (i > 1) add_component(new_component); // here we're adding the previously read component
                            new_component = new Component(cellStrings, file); // starting fresh here with a newly created componet.
                            charge_row_index = 0;
                            scan_range = cellStrings[8];
                        }
                        else if (cellStrings.Count == 4) //charge-state row
                        {
                            if (charge_row_index == 0)
                            {
                                charge_row_index += 1;
                                continue; //skip charge state headers
                            }
                            else new_component.add_charge_state(cellStrings, GetCorrectionFactor(file.filename, scan_range, correctionFactors));
                        }
                    }
                    add_component(new_component); //add the final component
                }
                this.final_components = remove_monoisotopic_duplicates_harmonics_from_same_scan(raw_components_in_file);
                this.scan_ranges = new HashSet<string>(this.final_components.Select(c => c.scan_range).ToList());
                return final_components;
            }
            catch (IOException ex)
            {
                throw new IOException(ex.Message);
            }
        }

        private void add_component(Component c)
        {
            c.calculate_properties();
            this.raw_components_in_file.Add(c);
        }

        //public List<Component> scanComps = new List<Component>();
        private List<Component> remove_monoisotopic_duplicates_harmonics_from_same_scan(List<Component> raw_components)
        {
            List<string> scans = raw_components.Select(c => c.scan_range).Distinct().ToList();
            List<Component> removeThese = new List<Component>();
            List<NeuCodePair> ncPairsInScan = new List<NeuCodePair>();

            foreach (string scan in scans)
            { // here

                List<Component> scanComps = raw_components.Where(c => c.scan_range == scan).OrderByDescending(i => i.intensity_sum).ToList();
                foreach (Component sc in scanComps) // this loop compresses missed monoisotopics into a single component. components are sorted by mass. in one scan, there should only be one missed mono per 
                {
                    if (removeThese.Contains(sc)) continue;

                    List<double> possibleMissedMonoisotopicsList =
                        Enumerable.Range(-3, 7).Select(x =>
                        sc.weighted_monoisotopic_mass + ((double)x) * Lollipop.MONOISOTOPIC_UNIT_MASS).ToList();

                    foreach (double missedMonoMass in possibleMissedMonoisotopicsList)
                    {
                        double massTolerance = missedMonoMass / 1000000d * (double)Lollipop.mass_tolerance;
                        List<Component> missedMonoisotopics = scanComps.Except(removeThese).Where(
                            cp => cp.weighted_monoisotopic_mass >= (missedMonoMass - massTolerance)
                            && cp.weighted_monoisotopic_mass <= (missedMonoMass + massTolerance)
                            ).ToList(); // this is a list of harmonics to hc

                        foreach (Component c in missedMonoisotopics.Where(m => m.id != sc.id).ToList())
                        {
                            sc.mergeTheseComponents(c);
                            removeThese.Add(c);
                        }
                    }
                }

                //this.scanComps = raw_components.Where(c => c.scan_range == scan).OrderByDescending(i => i.intensity_sum).ToList();

                //Component root = this.scanComps[0];
                //List<Component> running = new List<Component>();
                //List<Thread> active = new List<Thread>();
                //while (this.scanComps.Count > 0 || active.Count > 0)
                //{
                //    while (root != null && active.Count < Environment.ProcessorCount)
                //    {
                //        Thread t = new Thread(new ThreadStart(root.combine_missed_monoisotopics));
                //        t.Start();
                //        running.Add(root);
                //        active.Add(t);
                //        root = Lollipop.find_next_root(this.scanComps.Except(removeThese).ToList(), running);
                //    }

                //    foreach (Thread t in active)
                //    {
                //        t.Join();
                //    }

                //    foreach (Component c in running)
                //    {
                //        removeThese.AddRange(c.incorporated_missed_monoisotopics);
                //    }

                //    running.Clear();
                //    active.Clear();
                //    root = Lollipop.find_next_root(this.scanComps.Except(removeThese).ToList(), running);
                //}

                if (Lollipop.neucode_labeled && raw_components.FirstOrDefault().input_file.purpose == Purpose.Identification) //before we compress harmonics, we have to determine if they are neucode labeled and lysine count 14. these have special considerations
                    ncPairsInScan = ComponentReader.find_neucode_pairs(scanComps.Except(removeThese)).ToList(); // these are not the final neucode pairs, just a temp list
                List<string> lysFourteenComponents = new List<string>();
                foreach (NeuCodePair ncp in ncPairsInScan)
                {
                    if (ncp.lysine_count == 14)
                    {
                        lysFourteenComponents.Add(ncp.neuCodeLight.id);
                        lysFourteenComponents.Add(ncp.neuCodeHeavy.id);
                    }
                }

                List<Component> someComponents = scanComps.OrderByDescending(w => w.weighted_monoisotopic_mass).ToList();
                foreach (Component hc in someComponents)
                {
                    if (removeThese.Contains(hc)) continue;

                    List<double> possibleHarmonicList = // 2 missed on the top means up to 4 missed monos on the 2nd harmonic and 6 missed monos on the 3rd harmonic
                        Enumerable.Range(-4, 9).Select(x => (hc.weighted_monoisotopic_mass + ((double)x) * Lollipop.MONOISOTOPIC_UNIT_MASS) / 2d).Concat(
                            Enumerable.Range(-6, 13).Select(x => (hc.weighted_monoisotopic_mass + ((double)x) * Lollipop.MONOISOTOPIC_UNIT_MASS) / 3d)).ToList();

                    foreach (double harmonicMass in possibleHarmonicList)
                    {
                        double massTolerance = harmonicMass / 1000000d * (double)Lollipop.mass_tolerance;
                        List<Component> harmonics = scanComps.Except(removeThese).Where(
                            cp => cp.weighted_monoisotopic_mass >= (harmonicMass - massTolerance)
                            && cp.weighted_monoisotopic_mass <= (harmonicMass + massTolerance)
                            ).ToList(); // this is a list of harmonics to hc
                        List<Component> someHarmonics = harmonics.Where(harmonicComponent => harmonicComponent.id != hc.id).ToList();
                        foreach (Component h in someHarmonics) // now that we have a list of harmonics to hc, we have to figure out what to do with them
                        {
                            if (removeThese.Contains(h) || removeThese.Contains(hc)) continue;
                            int parCsCount = hc.charge_states.Count();
                            int cldCsCount = h.charge_states.Count();
                            double parMass = hc.weighted_monoisotopic_mass;
                            double cldMass = h.weighted_monoisotopic_mass;
                            bool fourteen = lysFourteenComponents.Contains(h.id);

                            if (lysFourteenComponents.Contains(h.id))
                            {
                                h.mergeTheseComponents(hc);
                                removeThese.Add(hc);
                            }
                            else
                            {
                                if (hc.charge_states.Count >= 4 && h.charge_states.Count >= 4)
                                    continue;

                                if (hc.charge_states.Count == h.charge_states.Count)
                                {
                                    //throw out the lower mass component
                                    if (hc.weighted_monoisotopic_mass > h.weighted_monoisotopic_mass)
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
                                else
                                {
                                    if (hc.charge_states.Count() > h.charge_states.Count())
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
                            }
                        }
                    }
                }
            }
            return raw_components.Except(removeThese).ToList();
        }

        public static List<NeuCodePair> find_neucode_pairs(IEnumerable<Component> components_in_file_scanrange)
        {
            List<NeuCodePair> pairsInScanRange = new List<NeuCodePair>();
            //Add putative neucode pairs. Must be in same spectrum, mass must be within 6 Da of each other
            List<Component> components = components_in_file_scanrange.OrderBy(c => c.weighted_monoisotopic_mass).ToList();

            //List<Tuple<Component,Component>> running = new List<Tuple<Component, Component>>();
            //Tuple<Component, Component> duple = Lollipop.find_next_pair(components, running);
            //List<Thread> active = new List<Thread>();
            //while (components.Count > 0 || active.Count > 0)
            //{
            //    while (duple != null && active.Count < Environment.ProcessorCount)
            //    {
            //        Component lower_component = duple.Item1;
            //        Component higher_component = duple.Item2;
            //        double mass_difference = higher_component.weighted_monoisotopic_mass - lower_component.weighted_monoisotopic_mass;
            //        if (mass_difference < 6)
            //        {
            //            List<int> lower_charges = lower_component.charge_states.Select(charge_state => charge_state.charge_count).ToList<int>();
            //            List<int> higher_charges = higher_component.charge_states.Select(charge_states => charge_states.charge_count).ToList<int>();
            //            List<int> overlapping_charge_states = lower_charges.Intersect(higher_charges).ToList();
            //            double lower_intensity = lower_component.calculate_sum_intensity_olcs(overlapping_charge_states);
            //            double higher_intensity = higher_component.calculate_sum_intensity_olcs(overlapping_charge_states);
            //            bool light_is_lower = true; //calculation different depending on if neucode light is the heavier/lighter component
            //            if (lower_intensity > 0 && higher_intensity > 0)
            //            {
            //                NeuCodePair pair = lower_intensity > higher_intensity ?
            //                    new NeuCodePair(lower_component, higher_component, mass_difference, overlapping_charge_states, light_is_lower) : //lower mass is neucode light
            //                    new NeuCodePair(higher_component, lower_component, mass_difference, overlapping_charge_states, !light_is_lower); //higher mass is neucode light

            //                Thread t = new Thread(new ThreadStart(pair.verify));
            //                if ((pair.weighted_monoisotopic_mass <= (pair.neuCodeHeavy.weighted_monoisotopic_mass + Lollipop.MONOISOTOPIC_UNIT_MASS)) // the heavy should be at higher mass. Max allowed is 1 dalton less than light.                                    
            //                    && !neucodePairs_in_file.Any(p => p.id_heavy == pair.id_light && p.neuCodeLight.intensity_sum > pair.neuCodeLight.intensity_sum)) // we found that any component previously used as a heavy, which has higher intensity is probably correct and that that component should not get reuused as a light.
            //                    lock (pairsInScanRange) pairsInScanRange.Add(pair);
            //            }
            //        }
            //        Thread t = new Thread(new ThreadStart(duple.combine_missed_monoisotopics));
            //        t.Start();
            //        running.Add(duple);
            //        active.Add(t);
            //        duple = Lollipop.find_next_root(this.scanComps.Except(removeThese).ToList(), running);
            //    }

            //    foreach (Thread t in active)
            //    {
            //        t.Join();
            //    }

            //    foreach (Component c in running)
            //    {
            //        removeThese.AddRange(c.incorporated_missed_monoisotopics);
            //    }

            //    running.Clear();
            //    active.Clear();
            //    duple = Lollipop.find_next_root(this.scanComps.Except(removeThese).ToList(), running);
            //}

            Parallel.ForEach(components, lower_component =>
            {
                IEnumerable<Component> higher_mass_components = components.Where(higher_component => higher_component != lower_component && higher_component.weighted_monoisotopic_mass > lower_component.weighted_monoisotopic_mass);
                foreach (Component higher_component in higher_mass_components)
                {
                    lock (lower_component) lock (higher_component) // two locks for thread-unsafe linq queries
                    {
                        double mass_difference = higher_component.weighted_monoisotopic_mass - lower_component.weighted_monoisotopic_mass;
                        if (mass_difference < 6)
                        {
                            List<int> lower_charges = lower_component.charge_states.Select(charge_state => charge_state.charge_count).ToList<int>();
                            List<int> higher_charges = higher_component.charge_states.Select(charge_states => charge_states.charge_count).ToList<int>();
                            List<int> overlapping_charge_states = lower_charges.Intersect(higher_charges).ToList();
                            double lower_intensity = lower_component.calculate_sum_intensity_olcs(overlapping_charge_states);
                            double higher_intensity = higher_component.calculate_sum_intensity_olcs(overlapping_charge_states);
                            bool light_is_lower = true; //calculation different depending on if neucode light is the heavier/lighter component
                            if (lower_intensity > 0 && higher_intensity > 0)
                            {
                                NeuCodePair pair = lower_intensity > higher_intensity ?
                                    new NeuCodePair(lower_component, higher_component, mass_difference, overlapping_charge_states, light_is_lower) : //lower mass is neucode light
                                    new NeuCodePair(higher_component, lower_component, mass_difference, overlapping_charge_states, !light_is_lower); //higher mass is neucode light

                                lock (pairsInScanRange)
                                    if ((pair.weighted_monoisotopic_mass <= (pair.neuCodeHeavy.weighted_monoisotopic_mass + Lollipop.MONOISOTOPIC_UNIT_MASS)) // the heavy should be at higher mass. Max allowed is 1 dalton less than light.                                    
                                        && !neucodePairs_in_file.Any(p => p.id_heavy == pair.id_light && p.neuCodeLight.intensity_sum > pair.neuCodeLight.intensity_sum)) // we found that any component previously used as a heavy, which has higher intensity is probably correct and that that component should not get reuused as a light.
                                        pairsInScanRange.Add(pair);
                            }
                        }
                    }
                }
            });

            return pairsInScanRange;
        }

        private static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue.InnerXml;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString && value != null)
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            else
                return value;
        }


        public double GetCorrectionFactor(string filename, string scan_range, IEnumerable<Correction> correctionFactors)
        {
            if(correctionFactors == null) return 0D;

            int[] scans = new int[2] { 0, 0 };
            try
            {
                scans = Array.ConvertAll<string, int>(scan_range.Split('-').ToArray(), int.Parse);
            }
            catch
            { }

            if (scans[0] <= 0 || scans[1] <= 0) return 0D;

            IEnumerable<double> allCorrectionFactors = 
                (from s in correctionFactors
                 where s.file_name == filename
                 where s.scan_number >= scans[0]
                where s.scan_number <= scans[1]
                select s.correction).ToList();

            if (allCorrectionFactors.Count() <= 0) return 0D;

            return allCorrectionFactors.Average();                
        }
    }
}
