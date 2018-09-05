using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ProteoformSuiteInternal
{
    public class ExcelReader
    {
        public static List<List<string>> get_cell_strings(InputFile file, bool blankRows)
        {
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(file.complete_path, false))
            {
                // Get Data in Sheet1 of Excel file
                IEnumerable<Sheet> sheetcollection = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>(); // Get all sheets in spread sheet document 
                WorksheetPart worksheet_1 = (WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(sheetcollection.First().Id.Value); // Get sheet1 Part of Spread Sheet Document
                SheetData sheet_1 = worksheet_1.Worksheet.Elements<SheetData>().First();
                IEnumerable<Row> rowcollection = worksheet_1.Worksheet.Descendants<Row>().ToList();
                List<List<string>> list_cells = new List<List<string>>();
                foreach (Row row in rowcollection)
                {
                    List<string> cellStrings = new List<string>();
                    if (blankRows) //topdown files have blank cells in between need to handle
                    {
                        if (row.RowIndex.Value == 1) continue;
                        IEnumerable<Cell> cells = GetRowCells(row);
                        foreach (Cell cell in cells)
                        {
                            cellStrings.Add(GetCellValue(spreadsheetDocument, cell));
                        }
                    }
                    else
                    {  //deconvolution results -- blank cells should be ignored
                        for(int k = 0; k < row.Descendants<Cell>().Count(); k++)
                        {
                            cellStrings.Add(GetCellValue(spreadsheetDocument, row.Descendants<Cell>().ElementAt(k)));
                        }
                    }
                    list_cells.Add(cellStrings);
                }
                return list_cells;
            }
        }

        //TD files have blank spaces in middle of rows/columns -- need this to account for those. 
        private static IEnumerable<Cell> GetRowCells(Row row)
        {
            int currentCount = 0;

            foreach (DocumentFormat.OpenXml.Spreadsheet.Cell cell in
                row.Descendants<DocumentFormat.OpenXml.Spreadsheet.Cell>())
            {
                string columnName = GetColumnName(cell.CellReference);

                int currentColumnIndex = ConvertColumnNameToNumber(columnName);

                for (; currentCount < currentColumnIndex; currentCount++)
                {
                    yield return new DocumentFormat.OpenXml.Spreadsheet.Cell();
                }

                yield return cell;
                currentCount++;
            }
        }

        private static string GetColumnName(string cellReference)
        {
            // Match the column name portion of the cell name.
            var regex = new System.Text.RegularExpressions.Regex("[A-Za-z]+");
            var match = regex.Match(cellReference);

            return match.Value;
        }

        private static int ConvertColumnNameToNumber(string columnName)
        {
            var alpha = new System.Text.RegularExpressions.Regex("^[A-Z]+$");
            if (!alpha.IsMatch(columnName)) throw new ArgumentException();

            char[] colLetters = columnName.ToCharArray();
            Array.Reverse(colLetters);

            int convertedValue = 0;
            for (int i = 0; i < colLetters.Length; i++)
            {
                char letter = colLetters[i];
                int current = i == 0 ? letter - 65 : letter - 64;
                convertedValue += current * (int)Math.Pow(26, i);
            }
            return convertedValue;
        }

        private static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            try
            {
                string value = cell.CellValue.InnerXml;
                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString && value != null)
                    return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
                else
                    return value;
            }
            catch { return ""; }
        }
    }
}