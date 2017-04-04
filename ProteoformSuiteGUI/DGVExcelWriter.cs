using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Forms;
using ClosedXML.Excel;
using System.Data;


namespace ProteoformSuiteInternal
{
    class DGVExcelWriter
    {
        public void ExportToExcel(List<DataGridView> dgvs, string filename)
        {
            var workbook = new XLWorkbook();
            foreach (DataGridView dgv in dgvs)
            {
                System.Data.DataTable dt = new System.Data.DataTable();
                foreach (DataGridViewColumn col in dgv.Columns)
                {
                    dt.Columns.Add(col.HeaderText);

                }
                foreach(DataGridViewRow row in dgv.Rows)
                {
                    DataRow new_row = dt.NewRow();
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (dgv.Columns[cell.ColumnIndex].Visible) new_row[cell.ColumnIndex] = (cell.Value == null  || cell.Value.ToString() == "NaN")? "" : cell.Value;
                    }
                    lock (dt) dt.Rows.Add(new_row);
                }
                foreach (DataGridViewColumn col in dgv.Columns) { if (!col.Visible) dt.Columns.Remove(col.HeaderText); }

                var worksheet = workbook.Worksheets.Add(dt, dgv.Name);
                foreach (var col in worksheet.Columns())
                {
                    double is_number;
                    col.Cells(2, worksheet.LastRowUsed().RowNumber()).DataType = Double.TryParse(worksheet.Row(2).Cell(col.ColumnNumber()).Value.ToString(), out is_number) ? XLCellValues.Number : XLCellValues.Text;
                }

            }
            workbook.SaveAs(filename);
        }
    }
}

