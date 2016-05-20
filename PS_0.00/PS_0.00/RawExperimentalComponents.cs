﻿using System;
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

            if (GlobalData.rawExperimentalComponents.Count == 0)
            {
                pull_raw_experimental_components();
            }

            FillRawExpComponentsTable();

        }

        public void pull_raw_experimental_components()
        {
            GlobalData.deconResultsFiles = GetDeconResults();
            GlobalData.rawExperimentalComponents = GetRawComponents();
        }

        private List<DataTable> GetDeconResults()
        {
            List<DataTable> decon_results = new List<DataTable>();

            Parallel.ForEach<string>(GlobalData.deconResultsFileNames, filename =>
            {
                DataTable dt = ReadExcelFile(filename);
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
                    row["Filename"] = Path.GetFileName(filename);
                }

                decon_results.Add(dc);
            });

            return decon_results;
        }

        private BindingList<Component> GetRawComponents()
        {
            BindingList<Component> raw_components = new BindingList<Component>();

            foreach (DataTable table in GlobalData.deconResultsFiles)
            {
                DataRow[] component_rows = table.Select("[" + table.Columns[0].ColumnName + "] > 0"); //Checking that it's a component row, not a charge state one

                Parallel.ForEach<DataRow>(component_rows, component_row =>
               {
                   int id = component_row.Field<int>(0);
                   double monoisotopic_mass = component_row.Field<double>(1);
                   double sum_intensity = component_row.Field<double>(2);
                   int num_charge_states = component_row.Field<int>(3);
                   int num_detected_intervals = component_row.Field<int>(4);
                   double delta_mass = component_row.Field<double>(5);
                   double relative_abundance = component_row.Field<double>(6);
                   double fract_abundance = component_row.Field<double>(7);
                   string scan_range = component_row.Field<string>(8);
                   string rt_range = component_row.Field<string>(9);
                   double rt_apex = component_row.Field<double>(10);
                   string filename = component_row.Field<string>(11);

                   Component raw_component = new Component(id, monoisotopic_mass, sum_intensity, num_charge_states,
                       num_detected_intervals, delta_mass, relative_abundance, fract_abundance, scan_range,
                       rt_range, rt_apex, filename);

                   string charge_states_for_this_id = "[" + table.Columns[5].ColumnName
                       + "] is null AND [" + table.Columns[0].ColumnName + "] = " + id
                       + " AND [" + table.Columns[1].ColumnName + "] <> 'Charge State'";
                   DataRow[] charge_rows = table.Select(charge_states_for_this_id);
                   foreach (DataRow charge_row in charge_rows)
                   {
                       int charge_state = charge_row.Field<int>(1);
                       double intensity = charge_row.Field<double>(2);
                       double mz_centroid = charge_row.Field<double>(3);
                       double calculated_mass = charge_row.Field<double>(4);
                       raw_component.add_charge_state(charge_state, intensity, mz_centroid, calculated_mass);
                   }
                   raw_component.calculate_sum_intensity();
                   raw_component.calculate_weighted_monoisotopic_mass();

                   raw_components.Add(raw_component);
               });
            }
            return raw_components;
        }

        private void FillRawExpComponentsTable()
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = GlobalData.rawExperimentalComponents;
            dgv_RawExpComp_MI_masses.DataSource = bs;
            dgv_RawExpComp_MI_masses.ReadOnly = true;
            //dgv_RawExpComp_MI_masses.Columns["Monoisotopic Mass"].DefaultCellStyle.Format = "0.####";
            //dgv_RawExpComp_MI_masses.Columns["Delta Mass"].DefaultCellStyle.Format = "0.####";
            //dgv_RawExpComp_MI_masses.Columns["Weighted Monoisotopic Mass"].DefaultCellStyle.Format = "0.####";
            //dgv_RawExpComp_MI_masses.Columns["Apex RT"].DefaultCellStyle.Format = "0.##";
            //dgv_RawExpComp_MI_masses.Columns["Relative Abundance"].DefaultCellStyle.Format = "0.####";
            //dgv_RawExpComp_MI_masses.Columns["Fractional Abundance"].DefaultCellStyle.Format = "0.####";
            //dgv_RawExpComp_MI_masses.Columns["Sum Intensity"].DefaultCellStyle.Format = "0";
            dgv_RawExpComp_MI_masses.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_RawExpComp_MI_masses.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }

        private void dgv_RawExpComp_MI_masses_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Component c = (Component)this.dgv_RawExpComp_MI_masses.Rows[e.RowIndex].DataBoundItem;

                //Round doubles before displaying
                BindingSource bs = new BindingSource();
                bs.DataSource = c.get_chargestates();
                dgv_RawExpComp_IndChgSts.DataSource = bs;
                dgv_RawExpComp_IndChgSts.ReadOnly = true;
                //dgv_RawExpComp_IndChgSts.Columns["MZ Centroid"].DefaultCellStyle.Format = "0.####";
                //dgv_RawExpComp_IndChgSts.Columns["Calculated Mass"].DefaultCellStyle.Format = "0.####";
                //dgv_RawExpComp_IndChgSts.Columns["Intensity"].DefaultCellStyle.Format = "0";
                dgv_RawExpComp_IndChgSts.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
                dgv_RawExpComp_IndChgSts.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
            }
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
