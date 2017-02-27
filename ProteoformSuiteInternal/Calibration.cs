using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IO.Thermo;
using Spectra;
using Chemistry;
using MathNet.Numerics.Statistics;
using Proteomics;
using MassSpectrometry;
using System.IO;
using MathNet.Numerics;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;

namespace ProteoformSuiteInternal
{
    public class Calibration
    {
        //parameters
        public static double fineResolution = 0.1;

        //RAW LOCK MASS 
        public static void raw_lock_mass(string filename, string raw_file_path)
        {
            UsefulProteomicsDatabases.Loaders.LoadElements("elements.dat");
            var tol = .01;

            var Compound1 = new Peptide("NNNNN");

            var regularMZ = IsotopicDistribution.GetDistribution(Compound1.GetChemicalFormula(), 0.1, 0.001).Masses.Select(b => b.ToMz(1)).ToList();
            var withAmmoniaLoss = Compound1.GetChemicalFormula();
            withAmmoniaLoss.Add(ChemicalFormula.ParseFormula("N-1H-2"));
            var withAmmoniaLossMZ = IsotopicDistribution.GetDistribution(withAmmoniaLoss, 0.1, 0.001).Masses.Select(b => b.ToMz(1)).ToList();

            var deamidated = Compound1.GetChemicalFormula();
            deamidated.Add(ChemicalFormula.ParseFormula("H-1N-1O"));
            var deamidatedMZ = IsotopicDistribution.GetDistribution(deamidated, 0.1, 0.001).Masses.Select(b => b.ToMz(1)).ToList();

            List<List<double>> allDistributions = new List<List<double>>() { regularMZ, withAmmoniaLossMZ, deamidatedMZ };
            using (ThermoDynamicData myMsDataFile = ThermoDynamicData.InitiateDynamicConnection(raw_file_path))
            {
                foreach (var scan in myMsDataFile)
                {
                    if (scan.MsnOrder == 1)
                    {
                        double bestIntensity = 0;
                        double monoError = double.NaN;
                        foreach (var dist in allDistributions)
                        {
                            ThermoMzPeak monoisotopicPeak = null;
                            try { monoisotopicPeak = scan.MassSpectrum.Extract(dist[0] - tol, dist[0] + tol).OrderByDescending(p => p.Y).First(); }
                            catch { }
                            if (monoisotopicPeak != null && bestIntensity < monoisotopicPeak.Intensity)
                            {
                                bestIntensity = monoisotopicPeak.Intensity;
                                monoError = monoisotopicPeak.Mz - dist[0];
                            }
                        }
                        MsScan get_scan = Lollipop.Ms_scans.Where(s => s.filename == filename && s.scan_number == scan.OneBasedScanNumber).ToList().First();
                        get_scan.lock_mass_shift = monoError;
                    }
                }
            }
        }



        //CALIBRATION WITH TD HITS
        public static Func<double[], double> Run_TdMzCal(string filename, string raw_file_path, List<TopDownHit> identifications)
        {
            Func<double[], double> calibration_function = null;
            using (ThermoDynamicData myMsDataFile = ThermoDynamicData.InitiateDynamicConnection(raw_file_path))
            {
                if (identifications.Where(h => h.result_set == Result_Set.tight_absolute_mass).ToList().Count >= 5)
                {
                    List<TopDownHit> identifications_tight_mass = identifications.Where(h => h.result_set == Result_Set.tight_absolute_mass).ToList();
                    //filter out 5% outliers
                    List<double> mass_errors = identifications_tight_mass.Select(h => (h.theoretical_mass - h.reported_mass) - Math.Round(h.theoretical_mass - h.reported_mass, 0)).ToList().OrderBy(m => m).ToList();
                    double percent_in_window = 1;
                    int start = 0; //start index
                    int count = identifications_tight_mass.Count; //end index
                    while (percent_in_window > .95)  //decrease window until ~95% of points in it
                    {
                        List<double> mass_errors_start = mass_errors.GetRange(start + 1, count - 1);
                        double start_range = mass_errors_start.Max() - mass_errors_start.Min();
                        List<double> mass_errors_end = mass_errors.GetRange(start, count - 1);
                        double end_range = mass_errors_end.Max() - mass_errors_end.Min();
                        if (start_range < end_range) start++; //if window smaller from removing first delta m, keep this window
                        count--; //either way count fewer
                        percent_in_window = (double)mass_errors.GetRange(start, count).ToList().Count / mass_errors.Count;
                    }
                    List<TopDownHit> identifications_to_use = identifications_tight_mass.OrderBy(h => ((h.theoretical_mass - h.reported_mass) - Math.Round(h.theoretical_mass - h.reported_mass, 0))).ToList().GetRange(start, count).ToList();

                    List<LabeledDataPoint> pointList = new List<LabeledDataPoint>();

                    //get data points
                    UsefulProteomicsDatabases.Loaders.LoadElements("elements.dat");
                    foreach (TopDownHit hit in identifications_to_use)
                    {
                        double[] inputs = new double[2] { hit.mz, hit.retention_time };
                        var a = new LabeledDataPoint(inputs, (hit.reported_mass - hit.theoretical_mass - Math.Round(hit.reported_mass - hit.theoretical_mass, 0)) / hit.charge);
                        pointList.Add(a);
                    }

                    ////if lock mass, add lock mass peptide to training points
                    if (Lollipop.calibrate_lock_mass)
                    {
                        foreach (MsScan scan in Lollipop.Ms_scans.Where(s => s.filename == filename && s.lock_mass_shift > 0 || s.lock_mass_shift < 0))
                        {
                            pointList.Add(new LabeledDataPoint(new double[2] { 589.2, scan.retention_time }, scan.lock_mass_shift));
                        }
                    }

                    //linear fit, mz correction as a function of retention time and m/z value
                    double[][] variables = new double[pointList.Count()][];
                    int k = 0;
                    foreach (LabeledDataPoint p in pointList)
                    {
                        variables[k] = p.inputs;
                        k++;
                    }

                    var mz_errors = pointList.Select(p => p.output).ToArray();
                    var functions = new Func<double[], double>[3];
                    functions[0] = a => 1;
                    for (int i = 0; i < 2; i++)
                    {
                        int j = i;
                        functions[j + 1] = a => a[j];
                    }
                    calibration_function = Fit.LinearMultiDimFunc(variables, mz_errors, functions);
                }
            }
            return calibration_function;
        }


        //READ AND WRITE NEW CALIBRATED TD HITS FILE 
        public static void calibrate_td_hits_file(InputFile file)
        {
            //Copy file to new worksheet
            string old_absolute_path = file.complete_path;
            string new_absolute_path = file.directory + "\\" + file.filename + "_calibrated" + file.extension;

            //create copy of excel file
            byte[] byteArray = File.ReadAllBytes(old_absolute_path);
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(byteArray, 0, (int)byteArray.Length);
                File.WriteAllBytes(new_absolute_path, stream.ToArray());
            }

            //update m/z for each charge state with calibrated value
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(new_absolute_path, true))
            {
                // Get Data in Sheet1 of Excel file
                IEnumerable<Sheet> sheetcollection = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>(); // Get all sheets in spread sheet document 
                WorksheetPart worksheet_1 = (WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(sheetcollection.First().Id.Value); // Get sheet1 Part of Spread Sheet Document
                IEnumerable<Row> rowcollection = worksheet_1.Worksheet.Descendants<Row>().ToList();
                uint rowindex = 2;
                foreach (Row row in rowcollection)
                {
                    if (row.RowIndex.Value == 1) continue;
                    IEnumerable<Cell> cells = TdBuReader.GetRowCells(row);
                    List<string> cellStrings = new List<string>();
                    foreach (Cell cell in cells)
                    {
                        cellStrings.Add(TdBuReader.GetCellValue(spreadsheetDocument, cell));
                    }
                    if (Lollipop.td_calibration_functions.ContainsKey(cellStrings[14].Split('.')[0]))
                    {
                        TopDownHit hit = Lollipop.td_hits_calibration.Where(h => h.file == file && h.reported_mass == Convert.ToDouble(cellStrings[16])).ToList().First();
                        cells.ElementAt(16).CellValue = new CellValue(hit.corrected_mass.ToString());
                        cells.ElementAt(16).DataType = new EnumValue<CellValues>(CellValues.Number);
                        foreach (Cell cell in cells)
                        {
                            string cellReference = cell.CellReference.Value;
                            cell.CellReference = new DocumentFormat.OpenXml.StringValue(cellReference.Replace(row.RowIndex.Value.ToString(), rowindex.ToString()));
                        }
                        row.RowIndex = rowindex;
                        rowindex++;
                    }
                    //if not calibrated, remove from list
                    else
                    {
                        row.Remove();
                    }
                }
                worksheet_1.Worksheet.Save();
                spreadsheetDocument.Close();
            }
        }

        //READ AND WRITE NEW CALIBRATED RAW EXPERIMENTAL COMPONENTS FILE
        public static void calibrate_components_in_xlsx(InputFile file)
        {
            Func<double[], double> bestCf = null;
            if (Lollipop.calibrate_td_results && Lollipop.td_calibration_functions.ContainsKey(file.filename)) bestCf = Lollipop.td_calibration_functions[file.filename];
            else if (Lollipop.calibrate_td_results) return; //if no calibration function, don't calibrate components file

            //Copy file to new worksheet
            string old_absolute_path = file.complete_path;
            string new_absolute_path = file.directory + "\\" + file.filename + "_calibrated" + file.extension;

            //create copy of excel file
            byte[] byteArray = File.ReadAllBytes(old_absolute_path);
            using (MemoryStream stream = new MemoryStream())
            {
                stream.Write(byteArray, 0, (int)byteArray.Length);
                File.WriteAllBytes(new_absolute_path, stream.ToArray());
            }

            //update m/z for each charge state with calibrated value
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(new_absolute_path, true))
            {
                // Get Data in Sheet1 of Excel file
                IEnumerable<Sheet> sheetcollection = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>(); // Get all sheets in spread sheet document 
                WorksheetPart worksheet_1 = (WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(sheetcollection.First().Id.Value); // Get sheet1 Part of Spread Sheet Document
                List<Row> rowcollection = worksheet_1.Worksheet.Descendants<Row>().ToList();

                int charge_row_index = 0;
                string rt_range = "";
                string scan_range = "";

                for (int i = 0; i < rowcollection.Count(); i++)
                {
                    List<string> cellStrings = new List<string>();
                    if (i == 0) continue; //skip component header
                    for (int k = 0; k < rowcollection[i].Descendants<Cell>().Count(); k++)
                    {
                        cellStrings.Add(GetCellValue(spreadsheetDocument, rowcollection[i].Descendants<Cell>().ElementAt(k)));
                    }

                    if (cellStrings.Count > 4) //component row
                    {
                        charge_row_index = 0;
                        rt_range = cellStrings[9];
                        scan_range = cellStrings[8];
                    }

                    else if (cellStrings.Count == 4) //charge-state row
                    {
                        if (charge_row_index == 0)
                        {
                            charge_row_index += 1;
                            continue; //skip charge state headers
                        }
                        else
                        {
                            double correction = 0;
                            //if calibrate td results only, use td results' function
                            //if calibrate td results & lock mass, lock mass peptide was used to make td results' function
                            //if lock mass only, use lock mass correction factors
                            if (Lollipop.calibrate_td_results) correction = -1 * bestCf(new double[] { Convert.ToDouble(cellStrings[2]), Convert.ToDouble(rt_range.Split('-')[0]) });
                            if (Lollipop.calibrate_lock_mass && !Lollipop.calibrate_td_results) correction = -1 * get_correction_factor(file.filename, scan_range);
                            worksheet_1.Worksheet.Descendants<Row>().ToList()[i].Descendants<Cell>().ElementAt(2).CellValue = new CellValue((Convert.ToDouble(cellStrings[2]) + correction).ToString());
                            worksheet_1.Worksheet.Descendants<Row>().ToList()[i].Descendants<Cell>().ElementAt(2).DataType = new EnumValue<CellValues>(CellValues.Number);
                        }
                    }
                }
                worksheet_1.Worksheet.Save();
                spreadsheetDocument.Close();
            }
        }

        private static double get_correction_factor(string filename, string scan_range)
        {
            if (Lollipop.correctionFactors == null) return 0D;
            int[] scans = new int[2] { 0, 0 };
            try
            {
                scans = Array.ConvertAll<string, int>(scan_range.Split('-').ToArray(), int.Parse);
            }
            catch
            { }

            if (scans[0] <= 0 || scans[1] <= 0) return 0D;

            IEnumerable<double> allCorrectionFactors =
                (from s in Lollipop.correctionFactors
                 where s.file_name == filename
                 where s.scan_number >= scans[0]
                 where s.scan_number <= scans[1]
                 select s.correction).ToList();

            if (allCorrectionFactors.Count() <= 0) return 0D;
            return allCorrectionFactors.Average();
        }

        //for deconvolution results excel file (want to ignore blank spaces)
        public static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue.InnerXml;
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString && value != null)
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            else
                return value;
        }
    }
}

