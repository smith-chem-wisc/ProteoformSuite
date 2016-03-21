using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel; //Right click Solution/Explorer/References. Then Add  "Reference". Assemblis/Extension/Microsoft.Office.Interop.Excel
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace PS_0._00
{
    public partial class RawExperimentalProteoforms : Form
    {

        public RawExperimentalProteoforms()
        {
            InitializeComponent();
        }

        private void RawExperimentalProteoforms_Load(object sender, EventArgs e)
        {
            GlobalData.deconResultsFiles = GetDeconResults();
            GlobalData.rawExperimentalProteoforms = GetRawProteoforms();
            GlobalData.rawExperimentalChargeStateData = GetRawChargeStates();

            BindingSource dgv_rep_BS = new BindingSource();
            dgv_rep_BS.DataSource = GlobalData.rawExperimentalProteoforms;
            dgv_RawExpProt_MI_masses.DataSource = dgv_rep_BS;
            dgv_RawExpProt_MI_masses.AutoGenerateColumns = true;
            dgv_RawExpProt_MI_masses.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_RawExpProt_MI_masses.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;

            CalculateWeightedMonoisotopicMass();
        }

        private void CalculateWeightedMonoisotopicMass()
        {
            foreach (DataTable table in GlobalData.rawExperimentalChargeStateData.Tables)
            {
                string Filename = "";
                int entryNumber = 0;
                object sumObject; 
                sumObject = table.Compute("Sum(Intensity)", "");
                double intensitySum = Convert.ToDouble(sumObject);
                double weightedMonoisotopicMass = 0;
                foreach (DataRow row in table.Rows)
                {
                    Filename = row["Filename"].ToString();
                    entryNumber = int.Parse(row["No#"].ToString());
                    weightedMonoisotopicMass = weightedMonoisotopicMass + (double.Parse(row["intensity"].ToString())/intensitySum*(double.Parse(row["Calculated Mass"].ToString())));
                }
                string expression = GlobalData.rawExperimentalProteoforms.Columns[11].ColumnName + " = '"+ Filename +"'"
                    + " AND [" + GlobalData.rawExperimentalProteoforms.Columns[0].ColumnName + "] = " + entryNumber;// you gotta have single quotes on the filename or this don't work. took me forever to figure that out.

                DataRow[] rawProteoformRows = GlobalData.rawExperimentalProteoforms.Select(expression);

                foreach (DataRow row in rawProteoformRows)
                {
                    row["Weighted Monoisotopic Mass"] = weightedMonoisotopicMass;
                }

            }
        }

        private DataSet GetRawChargeStates()
        {

            DataSet rCST = new DataSet();
            
            foreach (DataTable table in GlobalData.deconResultsFiles.Tables)
            {
                int entryNumber = 0;
                int maxEntry = 0;
                foreach (DataRow row in table.Rows)
                {
                    bool result = int.TryParse(row[0].ToString(), out entryNumber);
                    if (entryNumber > maxEntry)
                    {
                        maxEntry = entryNumber;
                    }
                    else
                    {
                        row[0] = maxEntry;
                    }
                }

                for (int i = 1; i <= maxEntry; i++)
                {

                    string expression = "[" + table.Columns[5].ColumnName
                        + "] is null AND [" + table.Columns[0].ColumnName + "] = " + i
                        + " AND [" + table.Columns[1].ColumnName + "] <> 'Charge State'";

                    DataRow[] chargeStates = table.Select(expression);

                    string tableName = chargeStates[0]["Filename"].ToString() + "_" + i.ToString();
                    DataTable csTable = new DataTable(tableName);
                    csTable.Columns.Add("Filename", typeof(string));
                    csTable.Columns.Add("No#", typeof(int));
                    csTable.Columns.Add("Charge State", typeof(int));
                    csTable.Columns.Add("Intensity", typeof(double));
                    csTable.Columns.Add("MZ Centroid", typeof(double));
                    csTable.Columns.Add("Calculated Mass", typeof(double));

                    foreach (DataRow row in chargeStates)
                    {
                        csTable.Rows.Add(
                            row[11].ToString(),
                            int.Parse(row[0].ToString()),
                            int.Parse(row[1].ToString()),
                            double.Parse(row[2].ToString()),
                            double.Parse(row[3].ToString()),
                            double.Parse(row[4].ToString())
                            );
                    }
                    rCST.Tables.Add(csTable);
                }
            }
            return rCST;
        }


        private DataTable GetRawProteoforms()
        {
            DataTable dc = GlobalData.deconResultsFiles.Tables[0].Clone();
            
            dc.Columns[0].DataType = typeof(int);//No.
            dc.Columns[1].DataType = typeof(double);//Monoisotopic Mass
            dc.Columns[2].DataType = typeof(double);//Sum Intensity
            dc.Columns[3].DataType = typeof(int);//Number of Charge States
            dc.Columns[4].DataType = typeof(int);//Number of Detected Intervals
            dc.Columns[5].DataType = typeof(double);//Delta Mass
            dc.Columns[6].DataType = typeof(double);//Relative Abundance
            dc.Columns[7].DataType = typeof(double);//Fractional Abundance
            dc.Columns[8].DataType = typeof(string);//Scan Range
            dc.Columns[9].DataType = typeof(string);//RT Range
            dc.Columns[10].DataType = typeof(double);//Apex RT
            dc.Columns[11].DataType = typeof(string);//filename

            foreach (DataTable table in GlobalData.deconResultsFiles.Tables)
            {
                DataRow[] rows = table.Select("[" + table.Columns[0].ColumnName + "] > 0");

                foreach (DataRow row in rows)
                {
                    dc.ImportRow(row);
                }
            }

            dc.Columns.Add("Weighted Monoisotopic Mass", typeof(double));

            foreach (DataRow dr in dc.Rows)
            {

                dr["Weighted Monoisotopic Mass"] = -1;   // this will get changed later.
            }
            return dc;
        }

        private DataSet GetDeconResults()
        {
            DataSet ds = new DataSet();

            foreach (string file in GlobalData.deconResultsFileNames)
            {
                DataTable dt = new DataTable();
                dt = ReadExcelFile(file);
                DataTable dc = dt.Clone();
                dc.Columns[0].DataType = typeof(int);
                foreach (DataRow row in dt.Rows)
                {
                    int number;
                    bool result = int.TryParse(row[dt.Columns[0].ColumnName].ToString(), out number);
                    if (result)
                    {
                        dc.ImportRow(row);
                    }
                    else
                    {
                        row[dt.Columns[0].ColumnName] = "-1";
                        dc.ImportRow(row);
                    }
                    
                }
                dc.Columns.Add("Filename", typeof(string));

                foreach (DataRow row in dc.Rows)
                {
                    row["Filename"] = Path.GetFileName(file);
                }

                ds.Tables.Add(dc);
            }
            return ds;
        }

        private DataTable ReadExcelFile(string filename)
        {
            // Initialize an instance of DataTable
            DataTable dt = new DataTable();

            try
            {
                // Use SpreadSheetDocument class of Open XML SDK to open excel file
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filename, false))
                {
                    // Get Workbook Part of Spread Sheet Document
                    WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

                    // Get all sheets in spread sheet document 
                    IEnumerable<Sheet> sheetcollection = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();

                    // Get relationship Id
                    string relationshipId = sheetcollection.First().Id.Value;

                    // Get sheet1 Part of Spread Sheet Document
                    WorksheetPart worksheetPart = (WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(relationshipId);

                    // Get Data in Excel file
                    SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                    IEnumerable<Row> rowcollection = sheetData.Descendants<Row>();

                    if (rowcollection.Count() == 0)
                    {
                        return dt;
                    }

                    // Add columns
                    foreach (Cell cell in rowcollection.ElementAt(0))
                    {
                        dt.Columns.Add(GetValueOfCell(spreadsheetDocument, cell));
                    }

                    // Add rows into DataTable
                    foreach (Row row in rowcollection)
                    {
                        DataRow temprow = dt.NewRow();
                        int columnIndex = 0;
                        foreach (Cell cell in row.Descendants<Cell>())
                        {
                            // Get Cell Column Index
                            int cellColumnIndex = GetColumnIndex(GetColumnName(cell.CellReference));

                            if (columnIndex < cellColumnIndex)
                            {
                                do
                                {
                                    temprow[columnIndex] = string.Empty;
                                    columnIndex++;
                                }

                                while (columnIndex < cellColumnIndex);
                            }

                            temprow[columnIndex] = GetValueOfCell(spreadsheetDocument, cell);
                            columnIndex++;
                        }

                        // Add the row to DataTable
                        // the rows include header row
                        dt.Rows.Add(temprow);
                    }
                }

                // Here remove header row
                dt.Rows.RemoveAt(0);
                return dt;
            }
            catch (IOException ex)
            {
                throw new IOException(ex.Message);
            }
        }

        /// <summary>
        ///  Get Value of Cell 
        /// </summary>
        /// <param name="spreadsheetdocument">SpreadSheet Document Object</param>
        /// <param name="cell">Cell Object</param>
        /// <returns>The Value in Cell</returns>
        private static string GetValueOfCell(SpreadsheetDocument spreadsheetdocument, Cell cell)
        {
            // Get value in Cell
            SharedStringTablePart sharedString = spreadsheetdocument.WorkbookPart.SharedStringTablePart;
            if (cell.CellValue == null)
            {
                return string.Empty;
            }

            string cellValue = cell.CellValue.InnerText;

            // The condition that the Cell DataType is SharedString
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return sharedString.SharedStringTable.ChildElements[int.Parse(cellValue)].InnerText;
            }
            else
            {
                return cellValue;
            }
        }

        /// <summary>
        /// Get Column Name From given cell name
        /// </summary>
        /// <param name="cellReference">Cell Name(For example,A1)</param>
        /// <returns>Column Name(For example, A)</returns>
        private string GetColumnName(string cellReference)
        {
            // Create a regular expression to match the column name of cell
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellReference);
            return match.Value;
        }

        /// <summary>
        /// Get Index of Column from given column name
        /// </summary>
        /// <param name="columnName">Column Name(For Example,A or AA)</param>
        /// <returns>Column Index</returns>
        private int GetColumnIndex(string columnName)
        {
            int columnIndex = 0;
            int factor = 1;

            // From right to left
            for (int position = columnName.Length - 1; position >= 0; position--)
            {
                // For letters
                if (Char.IsLetter(columnName[position]))
                {
                    columnIndex += factor * ((columnName[position] - 'A') + 1) - 1;
                    factor *= 26;
                }
            }

            return columnIndex;
        }

        private void dgv_RawExpProt_MI_masses_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string one;
            string two;

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dgv_RawExpProt_MI_masses.Rows[e.RowIndex];
                one = row.Cells["Filename"].Value.ToString();
                two = row.Cells[0].Value.ToString();

                BindingSource dgv_cs_BS = new BindingSource();
                dgv_cs_BS.DataSource = GlobalData.rawExperimentalChargeStateData.Tables[one + "_" + two];
                dgv_RawExpProt_IndChgSts.DataSource = dgv_cs_BS;
                dgv_RawExpProt_IndChgSts.AutoGenerateColumns = true;
                dgv_RawExpProt_IndChgSts.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
                dgv_RawExpProt_IndChgSts.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;

            }
        }
    }
}
