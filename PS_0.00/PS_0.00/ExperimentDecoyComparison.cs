﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace PS_0._00
{
    public partial class ExperimentDecoyComparison : Form
    {
        DataTable edList = new DataTable();

        public ExperimentDecoyComparison()
        {
            InitializeComponent();
            this.dgv_ED_Peak_List.MouseClick += new MouseEventHandler(dgv_ED_Peak_List_CellClick);
        }

        public void ExperimentDecoyComparison_Load(object sender, EventArgs e)
        {
            //this was causing as exception to be thrown, will look into later -LVS
           // if (!GlobalData.experimentTheoreticalPairs.Columns.Contains("Acceptable Peak"))
            //{
                run_comparison();
           // }
            GraphEDHistogram();
            FillEDListTable();
            FillEDGridView("DecoyDatabase_0");
            GraphETPeakList();
            GraphEDList();
        }

        public void run_comparison()
        {
            InitializeParameterSet();
            FindAllEDPairs();
            CalculateRunningSums();
            InitializeEDListTable();
            UpdateFiguresOfMerit();
        }

        //private void RunTheGamut()
        //{
        //    FindAllETPairs();
        //    CalculateRunningSums();
        //    FillETGridView();
        //    GraphETHistogram();
        //    InitializeETPeakListTable();
        //    FillETPeakListTable();
        //    FillETGridView();
        //    UpdateFiguresOfMerit();
        //}

        private void FindAllEDPairs()
        {
            for (int i = 0; i < GlobalData.numDecoyDatabases; i++)
            {
                string tableName = "DecoyDatabase_" + i;
                DataTable eD = GetNewED_DataTable(tableName);
                GlobalData.experimentDecoyPairs.Tables.Add(eD);

                foreach (DataRow agRow in GlobalData.aggregatedProteoforms.Rows)
                {
                    double lowMass = Convert.ToDouble(agRow["Aggregated Mass"]) + Convert.ToDouble(nUD_ED_Lower_Bound.Value);
                    double highMass = Convert.ToDouble(agRow["Aggregated Mass"]) + Convert.ToDouble(nUD_ED_Upper_Bound.Value);

                    string expression = "[Proteoform Mass] >= " + lowMass + " and [Proteoform Mass] <= " + highMass;
                    expression = expression + "and [Lysine Count] >= " + agRow["Lysine Count"];

                    DataRow[] closeDecoys = GlobalData.theoreticalAndDecoyDatabases.Tables[tableName].Select(expression);

                    foreach (DataRow row in closeDecoys)
                    {
                        double deltaMass = Convert.ToDouble(agRow["Aggregated Mass"]) - Convert.ToDouble(row["Proteoform Mass"]);
                        double afterDecimal = Math.Abs(deltaMass - Math.Truncate(deltaMass));
                        bool oOR = true;
                        if (afterDecimal <= Convert.ToDouble(nUD_NoManLower.Value) || afterDecimal >= Convert.ToDouble(nUD_NoManUpper.Value))
                        {
                            oOR = false;
                        }
                        else
                        {
                            oOR = true;
                        }
                        eD.Rows.Add(row["Accession"], row["Name"], row["Fragment"], row["PTM List"], row["Proteoform Mass"], agRow["Aggregated Mass"], agRow["Aggregated Intensity"], agRow["Aggregated Retention Time"], agRow["Lysine Count"], deltaMass, 0, 0, deltaMass, oOR, false);
                        //set out of range variable
                    }

                }
            }

            // GlobalData.experimentDecoyPairs = eD;
        }

        private void CalculateRunningSums()
        {
            for (int i = 0; i < GlobalData.numDecoyDatabases; i++)
            {
                string tableName = "DecoyDatabase_" + i;
                foreach (DataRow row in GlobalData.experimentDecoyPairs.Tables[tableName].Rows)
                {
                    double deltaMass = Convert.ToDouble(row["Delta Mass"].ToString());
                    double lower = deltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    double upper = deltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    string expression = "[Delta Mass] >= " + lower + " and [Delta Mass] <= " + upper;
                    row["Running Sum"] = GlobalData.experimentDecoyPairs.Tables[tableName].Select(expression).Length;
                }

            }

           
        }

        private void GraphETPeakList()
        {
            string colNameET = "Delta Mass";
            string directionET = "DESC";
            DataTable et = GlobalData.experimentTheoreticalPairs;
            et.DefaultView.Sort = colNameET + " " + directionET;
            et = et.DefaultView.ToTable();
            GlobalData.experimentTheoreticalPairs = et;

            foreach (DataRow row in GlobalData.experimentTheoreticalPairs.Rows)
            {
                ct_ED_peakList.Series["etPeakList"].Points.AddXY(row["Delta Mass"], row["Running Sum"]);
            }
           
        }

        private void GraphEDList()
        {
            int i = (int)nud_Decoy_Database.Value - 1;
            string colNameED = "Delta Mass";
            string directionED = "DESC";
            string tableName = "DecoyDatabase_" + i;

            DataTable ed = GlobalData.experimentDecoyPairs.Tables[tableName];
            ed.DefaultView.Sort = colNameED + " " + directionED;
            ed = ed.DefaultView.ToTable();

            foreach (DataRow row in ed.Rows)
            {
                ct_ED_peakList.Series["edPeakList"].Points.AddXY(row["Delta Mass"], row["Running Sum"]);
            }

            ct_ED_peakList.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";


            ct_ED_peakList.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(dgv_ED_Peak_List.Rows[0].Cells["ED Delta Mass"].Value.ToString()) - Convert.ToDouble(nUD_PeakWidthBase.Value);
            ct_ED_peakList.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(dgv_ED_Peak_List.Rows[0].Cells["ED Delta Mass"].Value.ToString()) + Convert.ToDouble(nUD_PeakWidthBase.Value);
            ct_ED_peakList.Series["edPeakList"].ToolTip = "#VALX{#.##}" + " , " + "#VALY{#.##}";
            ct_ED_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
            {
                BorderColor = Color.Red,
                IntervalOffset = Convert.ToDouble(dgv_ED_Peak_List.Rows[0].Cells["ED Delta Mass"].Value.ToString()) + 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
            });

            ct_ED_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
            {
                BorderColor = Color.Red,
                IntervalOffset = Convert.ToDouble(dgv_ED_Peak_List.Rows[0].Cells["ED Delta Mass"].Value.ToString()) - 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
            });
        }

        private void dgv_ED_Peak_List_CellClick(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                int clickedRow = dgv_ED_Peak_List.HitTest(e.X, e.Y).RowIndex;
                if (clickedRow >= 0 && clickedRow < GlobalData.edList.Rows.Count)
                {
                    EDListGraphParameters(clickedRow);
                }
            }
        }

        private void EDListGraphParameters(int clickedRow)
        {
            ct_ED_peakList.ChartAreas[0].AxisX.StripLines.Clear();
            double graphMax = Convert.ToDouble(dgv_ED_Peak_List.Rows[clickedRow].Cells["ED Delta Mass"].Value.ToString()) + Convert.ToDouble((nUD_PeakWidthBase.Value));
            double graphMin = Convert.ToDouble(dgv_ED_Peak_List.Rows[clickedRow].Cells["ED Delta Mass"].Value.ToString()) - Convert.ToDouble((nUD_PeakWidthBase.Value));

            if (graphMin < graphMax)
            {
                ct_ED_peakList.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(graphMin);
                ct_ED_peakList.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(graphMax);
            }

            ct_ED_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
            {
                BorderColor = Color.Red,
                IntervalOffset = Convert.ToDouble(dgv_ED_Peak_List.Rows[clickedRow].Cells["ED Delta Mass"].Value.ToString()) + 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
            });

            ct_ED_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
            {
                BorderColor = Color.Red,
                IntervalOffset = Convert.ToDouble(dgv_ED_Peak_List.Rows[clickedRow].Cells["ED Delta Mass"].Value.ToString()) - 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
            });
        }

        private void UpdateFiguresOfMerit()
        {
            int eDCount = 0;
            foreach (DataRow row in edList.Rows)
            {
                eDCount += Convert.ToInt32(row["ED count"]);
            }

            tb_TotalPeaks.Text = eDCount.ToString();
        }

        //private void ZeroETPairsTableValues()
        //{
        //    foreach (DataRow row in GlobalData.experimentTheoreticalPairs.Rows)
        //    {
        //        row["Acceptable Peak"] = false;
        //        row["Peak Center Count"] = 0;
        //    }
        //}

        //private void ClearETPeakListTable()
        //{
        //    etPeakList.Clear();
        //    InitializeETPeakListTable();
        //}

        private void FillEDListTable()
        {
          
            foreach (DataRow row in GlobalData.etPeakList.Rows)
            {
                DataTable decoyTotals = new DataTable();
                decoyTotals.Columns.Add("Decoy Hits", typeof(int));

                double deltaMass = Convert.ToDouble(row["Average Delta Mass"].ToString());
                int peakCount = Convert.ToInt16(row["Peak Count"].ToString());

                double lower = deltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                double upper = deltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                string expression = "[Delta Mass] >= " + lower + " and [Delta Mass] <= " + upper;

                for (int i = 0; i < GlobalData.numDecoyDatabases; i++)
                {
                    string colName = "Running Sum";
                    string direction = "DESC";
                    string tableName = "DecoyDatabase_" + i;
                    DataTable dt = GlobalData.experimentDecoyPairs.Tables[tableName];
                    dt.DefaultView.Sort = colName + " " + direction;
                    dt = dt.DefaultView.ToTable();
                    //if (Convert.ToBoolean(row["Out of Range Decimal"].ToString()) == false && Convert.ToBoolean(row["Acceptable Peak"].ToString()) == false)
                    DataRow[] decoyHits = dt.Select(expression);


                    decoyTotals.Rows.Add(decoyHits.Length);
                }

                //calculate average of decoy hits for given protein
                int sum = 0;
                for (int i = 0; i < decoyTotals.Rows.Count; i++)
                {
                    sum = sum + Convert.ToInt16(decoyTotals.Rows[i]["Decoy Hits"]);
                }

                decimal average = sum / (decoyTotals.Rows.Count);
                edList.Rows.Add(deltaMass, peakCount, average);


                //calculate median of decoy hits for given protein
                //string colName2 = "Decoy Hits";
                //decoyTotals.DefaultView.Sort = colName2 + " " + "ASC";
                //decoyTotals = decoyTotals.DefaultView.ToTable();
                
                //int indexMedian = (decoyTotals.Rows.Count)/ 2;
                //int median = Convert.ToInt16(decoyTotals.Rows[indexMedian][0]);
            
                //edList.Rows.Add(deltaMass, peakCount, median);
            }

            GlobalData.edList = edList;

            //Round before displaying ED peak list
            dgv_ED_Peak_List.DataSource = edList;
            dgv_ED_Peak_List.ReadOnly = true;
            dgv_ED_Peak_List.Columns["ED Delta Mass"].DefaultCellStyle.Format = "0.####";
            dgv_ED_Peak_List.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_ED_Peak_List.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }   



        private void InitializeEDListTable()
        {
            edList.Columns.Add("ED Delta Mass", typeof(double)); //I changed this from ET Delta Mass -AC
            edList.Columns.Add("ED Peak Count", typeof(int)); //I changed this from ET Peak Count -AC
            edList.Columns.Add("ED count", typeof(int));
        }

        private void FillEDGridView(string table)
        {
            DataTable displayTable = GlobalData.experimentDecoyPairs.Tables[table];

            dgv_ED_Pairs.DataSource = displayTable;
            dgv_ED_Pairs.ReadOnly = true;
            dgv_ED_Pairs.Columns["Acceptable Peak"].ReadOnly = false;
            dgv_ED_Pairs.Columns["Aggregated Retention Time"].DefaultCellStyle.Format = "0.##";
            dgv_ED_Pairs.Columns["Proteoform Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_ED_Pairs.Columns["Aggregated Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_ED_Pairs.Columns["Delta Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_ED_Pairs.Columns["Peak Center Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_ED_Pairs.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_ED_Pairs.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }

        private void GraphEDHistogram()
        {
            int i = (int)nud_Decoy_Database.Value - 1;
            string tableName = "DecoyDatabase_" + i;
            string colName = "Delta Mass";
            string direction = "DESC";
            DataTable dt = GlobalData.experimentDecoyPairs.Tables[tableName];

            dt.DefaultView.Sort = colName + " " + direction;
            dt = dt.DefaultView.ToTable();


            ct_ED_Histogram.Series["edHistogram"].XValueMember = "Delta Mass";
            ct_ED_Histogram.Series["edHistogram"].YValueMembers = "Running Sum";

            ct_ED_Histogram.DataSource = dt;
            ct_ED_Histogram.DataBind();

            ct_ED_Histogram.Series["edHistogram"].ToolTip = "#VALX{#.##}" + " , " + "#VALY{#.##}";

        }

        private DataTable GetNewED_DataTable(String title)
        {
            DataTable dt = new DataTable(title);
            dt.Columns.Add("Accession", typeof(string));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("Fragment", typeof(string));
            dt.Columns.Add("PTM List", typeof(string));
            dt.Columns.Add("Proteoform Mass", typeof(double));
            dt.Columns.Add("Aggregated Mass", typeof(double));
            dt.Columns.Add("Aggregated Intensity", typeof(double));
            dt.Columns.Add("Aggregated Retention Time", typeof(double));
            dt.Columns.Add("Lysine Count", typeof(int));
            dt.Columns.Add("Delta Mass", typeof(double));
            dt.Columns.Add("Running Sum", typeof(int));
            dt.Columns.Add("Peak Center Count", typeof(int));
            dt.Columns.Add("Peak Center Mass", typeof(double));
            dt.Columns.Add("Out of Range Decimal", typeof(bool));
            dt.Columns.Add("Acceptable Peak", typeof(bool));

            return dt;
        }

        private void InitializeParameterSet()
        {
            nUD_ED_Lower_Bound.Minimum = -500;
            nUD_ED_Lower_Bound.Maximum = 0;
            nUD_ED_Lower_Bound.Value = -250;

            nUD_ED_Upper_Bound.Minimum = 0;
            nUD_ED_Upper_Bound.Maximum = 500;
            nUD_ED_Upper_Bound.Value = 250;

            yMaxED.Minimum = 0;
            yMaxED.Maximum = 1000;
            yMaxED.Value = 100;

            yMinED.Minimum = -100;
            yMinED.Maximum = xMaxED.Value;
            yMinED.Value = 0;

            xMinED.Minimum = nUD_ED_Lower_Bound.Value;
            xMinED.Maximum = xMaxED.Value;
            xMinED.Value = nUD_ED_Lower_Bound.Value;

            xMaxED.Minimum = xMinED.Value;
            xMaxED.Maximum = nUD_ED_Upper_Bound.Value;
            xMaxED.Value = nUD_ED_Upper_Bound.Value;

            nUD_NoManLower.Minimum = 00m;
            nUD_NoManLower.Maximum = 0.49m;
            nUD_NoManLower.Value = 0.22m;//add the m suffix to treat the double as decimal

            nUD_NoManUpper.Minimum = 0.50m;
            nUD_NoManUpper.Maximum = 1.00m;
            nUD_NoManUpper.Value = 0.88m;

            nUD_PeakWidthBase.Minimum = 0.001m;
            nUD_PeakWidthBase.Maximum = 0.5000m;
            nUD_PeakWidthBase.Value = 0.0150m;

            nud_Decoy_Database.Minimum = 1;
            nud_Decoy_Database.Maximum = GlobalData.numDecoyDatabases;
            nud_Decoy_Database.Value = 1;
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void yMaxED_ValueChanged(object sender, EventArgs e)
        {
            ct_ED_Histogram.ChartAreas[0].AxisY.Maximum = double.Parse(yMaxED.Value.ToString());
        }

        private void yMinED_ValueChanged(object sender, EventArgs e)
        {
            ct_ED_Histogram.ChartAreas[0].AxisY.Minimum = double.Parse(yMinED.Value.ToString());

        }

        private void xMinED_ValueChanged(object sender, EventArgs e)
        {
            ct_ED_Histogram.ChartAreas[0].AxisX.Minimum = double.Parse(xMinED.Value.ToString());

        }

        private void xMaxED_ValueChanged(object sender, EventArgs e)
        {
            ct_ED_Histogram.ChartAreas[0].AxisX.Maximum = double.Parse(xMaxED.Value.ToString());

        }

        private void nud_Decoy_Database_ValueChanged(object sender, EventArgs e)
        {
            ct_ED_peakList.Series["edPeakList"].Points.Clear();
            int i = (int)nud_Decoy_Database.Value - 1;
            string table = "DecoyDatabase_" + i;
            FillEDGridView(table);
            GraphEDHistogram();
            GraphEDList();
        }

        public override string ToString()
        {
            return String.Join(System.Environment.NewLine, new string[] {
                    "ExperimentDecoyComparison|nUD_NoManLower.Value\t" + nUD_NoManLower.Value.ToString(),
                    "ExperimentDecoyComparison|nUD_NoManUpper.Value\t" + nUD_NoManUpper.Value.ToString(),
                    "ExperimentDecoyComparison|nUD_PeakWidthBase.Value\t" + nUD_PeakWidthBase.Value.ToString(),
                    "ExperimentDecoyComparison|nud_Decoy_Database.Value\t" + nud_Decoy_Database.Value.ToString()
            });
        }

        public void loadSetting(string setting_specs)
        {
            string[] fields = setting_specs.Split('\t');
            switch (fields[0].Split('|')[1])
            {
                case "nUD_NoManLower.Value":
                    nUD_NoManLower.Value = Convert.ToDecimal(fields[1]);
                    break;
                case "nUD_NoManUpper.Value":
                    nUD_NoManUpper.Value = Convert.ToDecimal(fields[1]);
                    break;
                case "nUD_PeakWidthBase.Value":
                    nUD_PeakWidthBase.Value = Convert.ToDecimal(fields[1]);
                    break;
                case "nud_Decoy_Database.Value":
                    nud_Decoy_Database.Value = Convert.ToDecimal(fields[1]);
                    break;
            }
        }
    }
}
