using System;
using System.Collections.Concurrent;
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
using Excel = Microsoft.Office.Interop.Excel; //Right click Solution/Explorer/References. Then Add  "Reference". Assemblies/Extension/Microsoft.Office.Interop.Excel
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace PS_0._00
{
    public partial class RawExperimentalComponents : Form
    {
        private void RoundDoubleColumn(DataTable table, string column_name, int num_decimal_places)
        {
            table.AsEnumerable().ToList().ForEach(p => p.SetField<Double>(column_name, Math.Round(p.Field<Double>(column_name), num_decimal_places)));
        }

        public RawExperimentalComponents()
        {
            InitializeComponent();
        }

        public void RawExperimentalComponents_Load(object sender, EventArgs e)
        {

            if (GlobalData.rawExperimentalComponents.Columns.Count == 0)
            {
                pull_raw_experimental_components();
            }

            FillRawExpComponentsTable();
        }

        public void pull_raw_experimental_components()
        {
            GlobalData.deconResultsFiles = GetDeconResults();
            GlobalData.rawExperimentalComponents = GetRawComponents();
            GlobalData.rawExperimentalChargeStateData = GetRawChargeStates();
            CalculateWeightedMonoisotopicMass();
        }

        private void FillRawExpComponentsTable()
        {
            dgv_RawExpComp_MI_masses.DataSource = GlobalData.rawExperimentalComponents;
            dgv_RawExpComp_MI_masses.ReadOnly = true;
            dgv_RawExpComp_MI_masses.Columns["Monoisotopic Mass"].DefaultCellStyle.Format = "0.####";
            dgv_RawExpComp_MI_masses.Columns["Delta Mass"].DefaultCellStyle.Format = "0.####";
            dgv_RawExpComp_MI_masses.Columns["Weighted Monoisotopic Mass"].DefaultCellStyle.Format = "0.####";
            dgv_RawExpComp_MI_masses.Columns["Apex RT"].DefaultCellStyle.Format = "0.##";
            dgv_RawExpComp_MI_masses.Columns["Relative Abundance"].DefaultCellStyle.Format = "0.####";
            dgv_RawExpComp_MI_masses.Columns["Fractional Abundance"].DefaultCellStyle.Format = "0.####";
            dgv_RawExpComp_MI_masses.Columns["Sum Intensity"].DefaultCellStyle.Format = "0";
            dgv_RawExpComp_MI_masses.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_RawExpComp_MI_masses.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }

        private DataSet GetDeconResults()
        {
            DataSet ds = new DataSet();
            List<DataTable> dtL = new List<DataTable>();

            Parallel.ForEach(GlobalData.deconResultsFileNames, (file) =>
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

                dtL.Add(dc);

            });

            foreach (DataTable tbl in dtL)
            {
                ds.Tables.Add(tbl);
            }

            dtL.Clear();

            return ds;
        }

        private DataTable GetRawComponents()
        {
            DataTable deconvolutionResults = GlobalData.deconResultsFiles.Tables[0].Clone();

            deconvolutionResults.Columns[0].DataType = typeof(int);//No.
            deconvolutionResults.Columns[1].DataType = typeof(double);//Monoisotopic Mass
            deconvolutionResults.Columns[2].DataType = typeof(double);//Sum Intensity
            deconvolutionResults.Columns[3].DataType = typeof(int);//Number of Charge States
            deconvolutionResults.Columns[4].DataType = typeof(int);//Number of Detected Intervals
            deconvolutionResults.Columns[5].DataType = typeof(double);//Delta Mass
            deconvolutionResults.Columns[6].DataType = typeof(double);//Relative Abundance
            deconvolutionResults.Columns[7].DataType = typeof(double);//Fractional Abundance
            deconvolutionResults.Columns[8].DataType = typeof(string);//Scan Range
            deconvolutionResults.Columns[9].DataType = typeof(string);//RT Range
            deconvolutionResults.Columns[10].DataType = typeof(double);//Apex RT
            deconvolutionResults.Columns[11].DataType = typeof(string);//filename

            foreach (DataTable table in GlobalData.deconResultsFiles.Tables)
            {
                DataRow[] rows = table.Select("[" + table.Columns[0].ColumnName + "] > 0");

                foreach (DataRow row in rows)
                {
                    deconvolutionResults.ImportRow(row);
                }
            }

            deconvolutionResults.Columns.Add("Weighted Monoisotopic Mass", typeof(decimal));

            foreach (DataRow dr in deconvolutionResults.Rows)
            {

                dr["Weighted Monoisotopic Mass"] = -1;   // this will get changed later.
            }
            return deconvolutionResults;
        }

        private void CalculateWeightedMonoisotopicMass()
        {

            //ConcurrentBag<List<Dictionary<string, decimal>>> dictList = new ConcurrentBag<List<Dictionary<string, decimal>>>();

            var dictList = new ConcurrentBag<string>();

            List<DataTable> rECSDs = new List<DataTable>();
            foreach (DataTable dt in GlobalData.rawExperimentalChargeStateData.Tables)
            {
                rECSDs.Add(dt);
            }

            ParallelLoopResult rlt = Parallel.ForEach(rECSDs, table => 
            {
                dictList.Add(expressionMonoIsotopicMass(table));
            });

            rECSDs.Clear();

            foreach (string exp in dictList)
            {
                string[] expMass = exp.Split('|');
                DataRow[] rawComponentRows = GlobalData.rawExperimentalComponents.Select(expMass[0]);

                foreach (DataRow row in rawComponentRows)
                {
                    row["Weighted Monoisotopic Mass"] = Convert.ToDecimal(expMass[1]);
                }

            }

        }

        private string expressionMonoIsotopicMass(DataTable table)
        {
            string result;

            string Filename = "";
            int entryNumber = 0;
            object sumObject;
            sumObject = table.Compute("Sum(Intensity)", "");
            decimal intensitySum = Convert.ToDecimal(sumObject);
            decimal weightedMonoisotopicMass = 0;
            foreach (DataRow row in table.Rows)
            {
                Filename = row["Filename"].ToString();
                entryNumber = int.Parse(row["No#"].ToString());
                weightedMonoisotopicMass = weightedMonoisotopicMass + (decimal.Parse(row["intensity"].ToString()) / intensitySum * (decimal.Parse(row["Calculated Mass"].ToString())));
            }
            string expression = GlobalData.rawExperimentalComponents.Columns[11].ColumnName + " = '" + Filename + "'"
                + " AND [" + GlobalData.rawExperimentalComponents.Columns[0].ColumnName + "] = " + entryNumber;// you gotta have single quotes on the filename or this don't work. took me forever to figure that out.

            //DataRow[] rawComponentRows = GlobalData.rawExperimentalComponents.Select(expression);

            //foreach (DataRow row in rawComponentRows)
            //{
            //    row["Weighted Monoisotopic Mass"] = weightedMonoisotopicMass;
            //}

            result = expression +"|"+ weightedMonoisotopicMass.ToString();

            return result;
        }


        private DataSet GetRawChargeStates()
        {
            DataSet rawChargeStateTables = new DataSet();

            ConcurrentBag<DataTable> rCSTL = new ConcurrentBag<DataTable>();
            //ConcurrentBag<string> debug_text = new ConcurrentBag<string>();

            //List<DataTable> rCSTL = new List<DataTable>();

            List<DataTable> bigTables = new List<DataTable>();

            foreach (DataTable table in GlobalData.deconResultsFiles.Tables)
            {
                bigTables.Add(table);
            }

            Parallel.ForEach(bigTables, (table) =>        
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

                DataTable tableCopy = table.Copy();

                var idNumberList = Enumerable.Range(1, maxEntry).ToList();

                //var csTableCollection = new ConcurrentBag<DataTable>();
                ParallelLoopResult rlt = Parallel.ForEach(idNumberList, id =>
                 {
                     //debug_text.Add(id.ToString());
                     rCSTL.Add(rCSTLParallel(id, tableCopy));
                 });
                tableCopy.Clear();
            });

            //string path = @"C:\Users\Michael\Downloads\garbage\debug_text.txt";
            //// This text is added only once to the file.
            //if (!File.Exists(path))
            //{
            //    // Create a file to write to.
            //    using (StreamWriter sw = File.CreateText(path))
            //    {
            //        sw.WriteLine("");
            //    }
            //}

            //foreach (string tN in debug_text)
            //{
            //    // This text is always added, making the file longer over time
            //    // if it is not deleted.
            //    using (StreamWriter sw = File.AppendText(path))
            //    {
            //        sw.WriteLine(tN);
            //    }
            //}


            foreach (DataTable tbl in rCSTL)
            {
                rawChargeStateTables.Tables.Add(tbl);
            }

            DataTable dtbl;
            while (rCSTL.TryTake(out dtbl)) ;

            bigTables.Clear();

            return rawChargeStateTables;
        }

        private DataTable rCSTLParallel(int i, DataTable tableCopy)
        {
            string expression = "[" + tableCopy.Columns[5].ColumnName
                        + "] is null AND [" + tableCopy.Columns[0].ColumnName + "] = " + i
                        + " AND [" + tableCopy.Columns[1].ColumnName + "] <> 'Charge State'";

            DataRow[] chargeStates = tableCopy.Select(expression);

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

            return csTable;
        }

        private DataTable ReadExcelFile(string filename)
        {
            // Initialize an instance of DataTable
            DataTable dt = new DataTable();

            try
            {
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(filename, false)) // Use SpreadSheetDocument class of Open XML SDK to open excel file
                {
                    // Open Excel workbook, and get Sheet1
                    WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart; // Get Workbook Part of Spread Sheet Document
                    IEnumerable<Sheet> sheetcollection = spreadsheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>(); // Get all sheets in spread sheet document 
                    string relationshipId = sheetcollection.First().Id.Value; // Get relationship Id
                    WorksheetPart worksheetPart = (WorksheetPart)spreadsheetDocument.WorkbookPart.GetPartById(relationshipId); // Get sheet1 Part of Spread Sheet Document

                    // Get Data in Sheet1 of Excel file
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

                        //Remove empty cells from beginning of row
                        int columnIndex = 0;
                        foreach (Cell cell in row.Descendants<Cell>())
                        {
                            // Get Cell Column Index
                            int cellColumnIndex = GetColumnIndex(GetColumnName(cell.CellReference));
                            while (columnIndex < cellColumnIndex)
                            {
                                temprow[columnIndex] = string.Empty;
                                columnIndex++;
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

        private void dgv_RawExpComp_MI_masses_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            string filename;
            string rawComponentNum;

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dgv_RawExpComp_MI_masses.Rows[e.RowIndex];
                filename = row.Cells["Filename"].Value.ToString();
                rawComponentNum = row.Cells[0].Value.ToString();

                //MessageBox.Show("dgv_RawExpComp_MI_masses_CellContentClick");

                //Round doubles before displaying
                DataTable displayTable = GlobalData.rawExperimentalChargeStateData.Tables[filename + "_" + rawComponentNum];
                dgv_RawExpComp_IndChgSts.DataSource = displayTable;
                dgv_RawExpComp_IndChgSts.ReadOnly = true;
                dgv_RawExpComp_IndChgSts.Columns["MZ Centroid"].DefaultCellStyle.Format = "0.####";
                dgv_RawExpComp_IndChgSts.Columns["Calculated Mass"].DefaultCellStyle.Format = "0.####";
                dgv_RawExpComp_IndChgSts.Columns["Intensity"].DefaultCellStyle.Format = "0";
                dgv_RawExpComp_IndChgSts.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
                dgv_RawExpComp_IndChgSts.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
            }
        }

        public override string ToString()
        {
            return "RawExperimentalComponents|";
        }

        public void loadSetting(string setting_specs)
        {
            string[] fields = setting_specs.Split('\t');
            switch (fields[0].Split('|')[1])
            {
                case "":
                    break;
            }
        }
    }
}
