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

        public List<Component> read_components_from_xlsx(InputFile file, IEnumerable<Correction>correctionFactors)
        {
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
                 where s.file_name == filename //the tsv and xlsx weren't matching which is why it wasn't working. use filename w/out path/extensions -LVS
                 where s.scan_number >= scans[0]
                where s.scan_number <= scans[1]
                select s.correction).ToList();

            if (allCorrectionFactors.Count() <= 0) return 0D;

            return allCorrectionFactors.Average();                
        }

        //Reading in Top-down excel
        public static List<Psm> ReadTDFile(string filename, TDProgram td_program)
        {
            List<Psm> psm_list = new List<Psm>();
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filename, false))
            {
                // Get Data in Sheet1 of Excel file
                IEnumerable<Sheet> sheetcollection = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>(); // Get all sheets in spread sheet document 
                WorksheetPart worksheet_1 = (WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(sheetcollection.First().Id.Value); // Get sheet1 Part of Spread Sheet Document
                SheetData sheet_1 = worksheet_1.Worksheet.Elements<SheetData>().First();
                List<Row> rowcollection = worksheet_1.Worksheet.Descendants<Row>().ToList();
                for (int i = 1; i < rowcollection.Count; i++)   //skip first row (headers)
                {
                    List<string> cellStrings = new List<string>();
                    for (int k = 0; k < rowcollection[i].Descendants<Cell>().Count(); k++)
                    {
                        cellStrings.Add(GetCellValue(spreadsheetDocument, rowcollection[i].Descendants<Cell>().ElementAt(k)));
                    }

                    if (td_program == TDProgram.NRTDP)
                    {
                        Psm new_psm = new Psm(cellStrings[8] + cellStrings[4], filename, Convert.ToInt16(cellStrings[5]), Convert.ToInt16(cellStrings[6]),
                            0, 0, Convert.ToDouble(cellStrings[14]), 0, cellStrings[2], Convert.ToDouble(cellStrings[12]), 0, 0, PsmType.TopDown);
                        psm_list.Add(new_psm);
                    }

                    else if (td_program == TDProgram.ProSight)
                    {
                        Psm new_psm = new Psm(cellStrings[6] + cellStrings[8], cellStrings[14], 0, 0, 0, 0, Convert.ToDouble(cellStrings[20]), 0, cellStrings[4], Convert.ToDouble(cellStrings[9]), 0, Convert.ToDouble(cellStrings[11]), PsmType.TopDown);
                        psm_list.Add(new_psm);
                    }
                }
            }
            return psm_list;
        }

        private void add_component(Component c)
        {
            c.calculate_sum_intensity();
            c.calculate_weighted_monoisotopic_mass();
            this.raw_components_in_file.Add(c);
        }
    }
}
