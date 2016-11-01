using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class ExcelReader
    {
        private List<Component> raw_components_in_file = new List<Component>();
        private List<string> MS1_scans = new List<string>();

        public List<Component> read_components_from_xlsx(InputFile file, IEnumerable<Correction>correctionFactors, List<string> MS1_scans)
        {
            this.MS1_scans.Clear();
            this.MS1_scans = MS1_scans;
            this.raw_components_in_file.Clear();
            string absolute_path = file.path + "\\" + file.filename + file.extension;
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
                            if (i > 1) add_component(new_component);
                            new_component = new Component(cellStrings, file);
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
                return raw_components_in_file;
            }
            catch (IOException ex) { throw new IOException(ex.Message); }
        }

        private bool acceptable_td_file_component(Component c)
        {
            string[] scans = c.scan_range.Split('-');
            bool same_scan = scans[0] == scans[1];
            bool MS1_scan = MS1_scans.Contains(scans[0]);
            bool not_a_repeat = this.raw_components_in_file.Where(r => r.monoisotopic_mass == c.monoisotopic_mass && r.intensity_sum == c.intensity_sum).ToList().Count == 0;
            if (same_scan && MS1_scan && not_a_repeat) return true;
            else return false;
        }

        public static string GetCellValue(SpreadsheetDocument document, Cell cell)
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

        private void add_component(Component c)
        {
            c.calculate_sum_intensity();
            c.calculate_weighted_monoisotopic_mass();
            if (!Lollipop.td_results || acceptable_td_file_component(c)) this.raw_components_in_file.Add(c);
        }
    }
}
