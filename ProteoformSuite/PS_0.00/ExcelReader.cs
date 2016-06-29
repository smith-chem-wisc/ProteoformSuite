using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.ComponentModel; // needed for bindinglist
using System.Windows.Forms;

namespace ProteoformSuite
{
    public class ExcelReader
    {
        private List<Component> raw_components_in_file = new List<Component>();

        public List<Component> read_components_from_xlsx(string filename)
        {
            this.raw_components_in_file.Clear();
            try
            {
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filename, false))
                {
                    // Get Data in Sheet1 of Excel file
                    IEnumerable<Sheet> sheetcollection = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>(); // Get all sheets in spread sheet document 
                    WorksheetPart worksheet_1 = (WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(sheetcollection.First().Id.Value); // Get sheet1 Part of Spread Sheet Document
                    SheetData sheet_1 = worksheet_1.Worksheet.Elements<SheetData>().First();
                    List<Row> rowcollection = sheet_1.Descendants<Row>().ToList();

                    Component new_component = new Component();
                    int charge_row_index = 0;
                    for (int i = 0; i < rowcollection.Count(); i++)
                    {
                        if (i == 0) continue; //skip component header
                        List<Cell> cells = rowcollection[i].Descendants<Cell>().ToList();
                        if (cells.Count > 4) //component row
                        {
                            if (i > 1) add_component(new_component);
                            new_component = new Component(cells, filename);
                            charge_row_index = 0;
                        }
                        else if (cells.Count == 4) //charge-state row
                        {
                            if (charge_row_index == 0)
                            {
                                charge_row_index += 1;
                                continue; //skip charge state headers
                            }
                            else new_component.add_charge_state(cells);
                        }
                    }
                    add_component(new_component); //add the final component
                }
                return raw_components_in_file;
            }
            catch (IOException ex) { throw new IOException(ex.Message); }
        }

        private void add_component(Component c)
        {
            c.calculate_sum_intensity();
            c.calculate_weighted_monoisotopic_mass();
            this.raw_components_in_file.Add(c);
        }
    }
}
