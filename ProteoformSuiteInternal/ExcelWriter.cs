using System;
using System.Collections.Generic;
using System.Data;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml;

namespace ProteoformSuiteInternal
{
    public class ExcelWriter
    {
        #region Public Methods
        public void ExportToExcel(string filename, List<DataTable> datatables)
        {
            SpreadsheetDocument new_document = SpreadsheetDocument.Create(filename, DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook);
            WorkbookPart workbookPart = new_document.AddWorkbookPart();
            workbookPart.Workbook = new Workbook();
            var sheets = workbookPart.Workbook.AppendChild(new Sheets());

            int sheetID = 1; 
            foreach (DataTable dt in datatables)
            {
                if (dt.Rows.Count == 0)
                    continue;
                WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = (uint)sheetID, Name = sheet_name(dt.TableName) };
                sheets.Append(sheet);
                SheetData sheetData = new SheetData();
                worksheetPart.Worksheet = new Worksheet(sheetData);

               Row headerRow = new Row();

                List<String> columns = new List<string>();
                foreach (System.Data.DataColumn column in dt.Columns)
                {
                    columns.Add(column.ColumnName);

                    Cell cell = new Cell();
                    cell.DataType = CellValues.String;
                    cell.CellValue = new CellValue(column.ColumnName);
                    headerRow.AppendChild(cell);
                }

                sheetData.AppendChild(headerRow);

                foreach (DataRow dsrow in dt.Rows)
                {
                    Row newRow = new Row();
                    foreach (String col in columns)
                    {
                        Cell cell = new Cell();
                        cell.DataType = double.TryParse(dsrow[col].ToString(), out double d) ? CellValues.Number : CellValues.String;
                        cell.CellValue = new CellValue(dsrow[col].ToString());
                        newRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(newRow);
                }
                sheetID++;
            }

            new_document.WorkbookPart.Workbook.Save();
            new_document.Save();
            new_document.Close();
        }

        #endregion Public Methods

        #region Private Method

        private static string sheet_name(string table_name)
        {
            return table_name.Length > 30 ? table_name.Substring(0, 30) : table_name;
        }

        #endregion Private Method
    }
}