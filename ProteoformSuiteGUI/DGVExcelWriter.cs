using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using System.Linq;

namespace ProteoformSuiteInternal
{
    class DGVExcelWriter
    {

        private XLWorkbook workbook = new XLWorkbook();

        public void ExportToExcel(List<DataGridView> dgvs, string sheet_prefix)
        {
            if (dgvs == null || dgvs.Count(d => d.DataSource != null) == 0)
                return;

            foreach (DataGridView dgv in dgvs.Where(d => d.DataSource != null))
            {
                DataTable dt = new DataTable();
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    dt.Columns.Add(col.HeaderText);
                }


                foreach(DataGridViewRow row in dgv.Rows)
                {
                    DataRow new_row = dt.NewRow();
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (dgv.Columns[cell.ColumnIndex].Visible)
                            new_row[cell.ColumnIndex] = cell.Value == null || cell.Value.ToString() == "NaN" ? "" : cell.Value;
                    }
                    lock (dt) dt.Rows.Add(new_row);
                }

                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    if (!col.Visible)
                        dt.Columns.Remove(col.HeaderText);
                }

                IXLWorksheet worksheet = null;
                lock (workbook)
                {
                    worksheet = workbook.Worksheets.Add(dt, sheet_prefix.Substring(0, Math.Min(sheet_prefix.Length, 30 - dgv.Name.Length)) + "_" + dgv.Name);
                }

                foreach (var col in worksheet.Columns())
                {
                    try
                    {
                        col.Cells(2, worksheet.LastRowUsed().RowNumber()).DataType = Double.TryParse(worksheet.Row(2).Cell(col.ColumnNumber()).Value.ToString(), out double is_number) ?
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

        public void SaveToExcel(string filename)
        {
            if (workbook.Worksheets.Count > 0)
                workbook.SaveAs(filename);
        }
    }
}
