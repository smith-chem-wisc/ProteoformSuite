using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class ComponentReader
    {

        #region Fields

        private List<Component> raw_components_in_file = new List<Component>();
        private static List<NeuCodePair> neucodePairs_in_file = new List<NeuCodePair>();
        public List<Component> final_components = new List<Component>();
        public HashSet<string> scan_ranges = new HashSet<string>();

        #endregion Fields

        #region Public Method

        public List<Component> read_components_from_xlsx(InputFile file, IEnumerable<Correction> correctionFactors)
        {
            raw_components_in_file.Clear();
            string absolute_path = file.directory + "\\" + file.filename + file.extension;

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
                        else new_component.add_charge_state(cellStrings, Correction.GetCorrectionFactor(file.filename, scan_range, correctionFactors));
                    }
                }
                add_component(new_component); //add the final component
            }
            final_components = remove_monoisotopic_duplicates_harmonics_from_same_scan(raw_components_in_file);
            scan_ranges = new HashSet<string>(this.final_components.Select(c => c.scan_range).ToList());
            return final_components;
        }

        #endregion Public Method

        #region Private Methods

        private void add_component(Component c)
        {
            c.calculate_properties();
            raw_components_in_file.Add(c);
        }

        private List<Component> remove_monoisotopic_duplicates_harmonics_from_same_scan(List<Component> raw_components)
        {
            List<string> scans = raw_components.Select(c => c.scan_range).Distinct().ToList();
            List<Component> removeThese = new List<Component>();
            List<NeuCodePair> ncPairsInScan = new List<NeuCodePair>();

            foreach (string scan in scans)
            {
                List<Component> scanComps = raw_components.Where(c => c.scan_range == scan).OrderByDescending(i => i.intensity_sum).ToList();
                foreach (Component sc in scanComps) // this loop compresses missed monoisotopics into a single component. components are sorted by mass. in one scan, there should only be one missed mono per 
                {
                    if (removeThese.Contains(sc)) continue;

                    List<double> possibleMissedMonoisotopicsList =
                        Enumerable.Range(-3, 7).Select(x =>
                        sc.weighted_monoisotopic_mass + ((double)x) * Lollipop.MONOISOTOPIC_UNIT_MASS).ToList();

                    foreach (double missedMonoMass in possibleMissedMonoisotopicsList)
                    {
                        double massTolerance = missedMonoMass / 1000000d * (double)SaveState.lollipop.mass_tolerance;
                        List<Component> missedMonoisotopics = scanComps.Except(removeThese).Where(cp => cp.weighted_monoisotopic_mass >= (missedMonoMass - massTolerance) && cp.weighted_monoisotopic_mass <= (missedMonoMass + massTolerance)).ToList(); // this is a list of harmonics to hc

                        foreach (Component c in missedMonoisotopics.Where(m => m.id != sc.id).ToList())
                        {
                            sc.mergeTheseComponents(c);
                            removeThese.Add(c);
                        }
                    }
                }


                if (SaveState.lollipop.neucode_labeled && raw_components.FirstOrDefault().input_file.purpose == Purpose.Identification) //before we compress harmonics, we have to determine if they are neucode labeled and lysine count 14. these have special considerations
                {
                    ncPairsInScan = SaveState.lollipop.find_neucode_pairs(scanComps.Except(removeThese), neucodePairs_in_file); // these are not the final neucode pairs, just a temp list
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

                List<Component> someComponents = scanComps.OrderByDescending(w => w.weighted_monoisotopic_mass).ToList();
                foreach (Component hc in someComponents)
                {
                    if (removeThese.Contains(hc))
                        continue;

                    List<double> possibleHarmonicList = // 2 missed on the top means up to 4 missed monos on the 2nd harmonic and 6 missed monos on the 3rd harmonic
                        Enumerable.Range(-4, 9).Select(x => (hc.weighted_monoisotopic_mass + ((double)x) * Lollipop.MONOISOTOPIC_UNIT_MASS) / 2d).Concat(
                            Enumerable.Range(-6, 13).Select(x => (hc.weighted_monoisotopic_mass + ((double)x) * Lollipop.MONOISOTOPIC_UNIT_MASS) / 3d)).ToList();

                    foreach (double harmonicMass in possibleHarmonicList)
                    {
                        double massTolerance = harmonicMass / 1000000d * (double)SaveState.lollipop.mass_tolerance;
                        List<Component> harmonics = scanComps.Except(removeThese).Where(cp => cp.weighted_monoisotopic_mass >= (harmonicMass - massTolerance) && cp.weighted_monoisotopic_mass <= (harmonicMass + massTolerance)).ToList(); // this is a list of harmonics to hc
                        List<Component> someHarmonics = harmonics.Where(harmonicComponent => harmonicComponent.id != hc.id).ToList();
                        foreach (Component h in someHarmonics) // now that we have a list of harmonics to hc, we have to figure out what to do with them
                        {
                            if (removeThese.Contains(h) || removeThese.Contains(hc))
                                continue;

                            int parCsCount = hc.charge_states.Count;
                            int cldCsCount = h.charge_states.Count;
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
                            }
                        }
                    }
                }
            }
            return raw_components.Except(removeThese).ToList();
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

        #endregion Private Methods

    }
}
