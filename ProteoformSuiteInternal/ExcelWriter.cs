using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;

namespace ProteoformSuiteGUI
{
    public class ExcelWriter
    {
        #region Private Field

        private XLWorkbook workbook = new XLWorkbook();

        #endregion Private Field

        #region Public Methods

        public void ExportToExcel(List<DataTable> datatables, string sheet_prefix)
        {
            if (datatables == null)
                return;

            foreach (DataTable dt in datatables)
            {
                if (dt.Rows.Count == 0)
                    continue;

                IXLWorksheet worksheet = null;
                lock (workbook)
                {
                    worksheet = workbook.Worksheets.Add(sheet_name(sheet_prefix, dt.TableName));
                }

                // speedup by doing this in parallel
                worksheet.Cell(1, 1).InsertTable(dt);

                foreach (var col in worksheet.Columns())
                {
                    try
                    {
                        col.Cells(2, worksheet.LastRowUsed().RowNumber()).DataType =
                            Double.TryParse(worksheet.Row(2).Cell(col.ColumnNumber()).Value.ToString(), out double is_number) ?
                            XLCellValues.Number :
                            XLCellValues.Text;
                    }
                    catch
                    {
                        col.Cells(2, worksheet.LastRowUsed().RowNumber()).DataType = XLCellValues.Text;
                    }
                }
            }
        }

        public void BuildHyperlinkSheet(List<Tuple<string, List<DataTable>>> sheetNameAndTables)
        {
            var ws = workbook.Worksheets.Add("Contents");
            int row = 1;
            foreach (Tuple<string, List<DataTable>> x in sheetNameAndTables)
            {
                if (x.Item2 == null) continue;
                foreach (DataTable dt in x.Item2)
                {
                    ws.Cell(row, 1).Value = "Table S" + row.ToString();
                    ws.Cell(row, 2).Value = sheet_name(x.Item1, dt.TableName);
                    ws.Cell(row++, 2).Hyperlink = new XLHyperlink("'" + sheet_name(x.Item1, dt.TableName) + "'!A1");
                }
            }
        }

        public string SaveToExcel(string filename)
        {
            if (workbook.Worksheets.Count > 0)
            {
                workbook.SaveAs(filename);
                return "Successfully exported table.";
            }
            else
            {
                return "There were no tables to export.";
            }
        }

        #endregion Public Methods

        #region Private Method

        private static string sheet_name(string sheet_prefix, string table_name)
        {
            return sheet_prefix.Substring(0, Math.Min(sheet_prefix.Length, 30 - table_name.Length)) + "_" + table_name;
        }

        #endregion Private Method
    }
}