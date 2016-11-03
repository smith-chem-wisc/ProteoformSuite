using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using System.Windows.Forms;


namespace ProteoformSuiteInternal
{
    class DGVExcelWriter
    {
        public void ExportToExcel(List<DataGridView> dgvs, string filename)
        {
            Microsoft.Office.Interop.Excel.Application excel_file = new Microsoft.Office.Interop.Excel.Application();
            Microsoft.Office.Interop.Excel.Workbook workbook = excel_file.Workbooks.Add(Type.Missing);

            foreach (DataGridView dgv in dgvs)
            {
                int cell_col_index = 1;
                Microsoft.Office.Interop.Excel.Worksheet worksheet = workbook.Worksheets.Add();
                worksheet.Name = dgv.Name;

                for (int dgv_col_index = 0; dgv_col_index < dgv.Columns.Count; dgv_col_index++) //can't be parallelized-- index depends on other columns
                {
                    if (!dgv.Columns[dgv_col_index].Visible) { continue; }
                    worksheet.Cells[1, cell_col_index] = dgv.Columns[dgv_col_index].HeaderText;  //line headings
                    Parallel.ForEach(dgv.Rows.Cast<DataGridViewRow>().Where(r => dgv.Rows[r.Index].Cells[dgv_col_index].Value != null).ToList(), row =>
                    {
                        worksheet.Cells[(row.Index + 2), cell_col_index] = dgv.Rows[row.Index].Cells[dgv_col_index].Value.ToString();
                    });
                    cell_col_index++;
                }
            }
            excel_file.DisplayAlerts = false; //if file already exists, save file dialog took care of it
            workbook.SaveAs(filename);
            workbook.Close(true, Type.Missing, Type.Missing);
            excel_file.Quit();
        }
    }
}
