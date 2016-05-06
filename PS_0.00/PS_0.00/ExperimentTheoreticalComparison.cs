using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;


namespace PS_0._00
{
    public partial class ExperimentTheoreticalComparison : Form
    {
        DataTable etPeakList = new DataTable();
        DataTableHandler dataTableHandler = new DataTableHandler();

        public ExperimentTheoreticalComparison()
        {
            InitializeComponent();
            this.dgv_ET_Peak_List.MouseClick += new MouseEventHandler(dgv_ET_Peak_List_CellClick);
            this.ct_ET_Histogram.MouseMove += new MouseEventHandler(ct_ET_Histogram_MouseMove);
            this.ct_ET_peakList.MouseMove += new MouseEventHandler(ct_ET_peakList_MouseMove);
        }

        private void ExperimentTheoreticalComparison_Load(object sender, EventArgs e)
        {
            InitializeParameterSet();
            FindAllETPairs();
            CalculateRunningSums();
            FillETGridView();
            GraphETHistogram();
            InitializeETPeakListTable();
            FillETPeakListTable();
            FillETGridView(); //Why are there two of these? -AC
            GraphETPeakList();
            UpdateFiguresOfMerit();
        }


        private void RunTheGamut()
        {
            this.Cursor = Cursors.WaitCursor;
            ClearETGridView();
            ZeroETPairsTableValues();
            ClearETPeakListTable();
            ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Clear();
            FindAllETPairs();
            CalculateRunningSums();
            FillETGridView();
            GraphETHistogram();
            FillETPeakListTable();
            UpdateFiguresOfMerit();
            xMaxET.Value = nUD_ET_Upper_Bound.Value;
            xMinET.Value = nUD_ET_Lower_Bound.Value;
            GraphETPeakList();
            this.Cursor = Cursors.Default;
        }

        Point? prevPosition = null;
        ToolTip tooltip = new ToolTip();

        void ct_ET_Histogram_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.Location;
            if (prevPosition.HasValue && pos == prevPosition.Value)
                return;
            tooltip.RemoveAll();
            prevPosition = pos;
            var results = ct_ET_Histogram.HitTest(pos.X, pos.Y, false,
                                            ChartElementType.DataPoint);
            foreach (var result in results)
            {
                if (result.ChartElementType == ChartElementType.DataPoint)
                {
                    var prop = result.Object as DataPoint;
                    if (prop != null)
                    {
                        var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                        // check if the cursor is really close to the point (2 pixels around the point)
                        if (Math.Abs(pos.X - pointXPixel) < 2) //&&
                           // Math.Abs(pos.Y - pointYPixel) < 2)
                        {
                            tooltip.Show("X=" + prop.XValue + ", Y=" + prop.YValues[0], this.ct_ET_Histogram,
                                            pos.X, pos.Y - 15);
                        }
                    }
                }
            }
        }

        Point? prevPosition2 = null;
        ToolTip tooltip2 = new ToolTip();

        void ct_ET_peakList_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.Location;
            if (prevPosition2.HasValue && pos == prevPosition2.Value)
                return;
            tooltip2.RemoveAll();
            prevPosition2 = pos;
            var results = ct_ET_peakList.HitTest(pos.X, pos.Y, false,
                                            ChartElementType.DataPoint);
            foreach (var result in results)
            {
                if (result.ChartElementType == ChartElementType.DataPoint)
                {
                    var prop = result.Object as DataPoint;
                    if (prop != null)
                    {
                        var pointXPixel = result.ChartArea.AxisX.ValueToPixelPosition(prop.XValue);
                        var pointYPixel = result.ChartArea.AxisY.ValueToPixelPosition(prop.YValues[0]);

                        // check if the cursor is really close to the point (2 pixels around the point)
                        if (Math.Abs(pos.X - pointXPixel) < 2) //&&
                                                               // Math.Abs(pos.Y - pointYPixel) < 2)
                        {
                            tooltip2.Show("X=" + prop.XValue + ", Y=" + prop.YValues[0], this.ct_ET_peakList,
                                            pos.X, pos.Y - 15);
                        }
                    }
                }
            }
        }


        private void ZeroETPairsTableValues()
        {
            foreach (DataRow row in GlobalData.experimentTheoreticalPairs.Rows)
            {
                row["Acceptable Peak"] = false;
                row["Peak Center Count"] = 0;
            }
        }

        private void ClearETPeakListTable()
        {
            etPeakList.Clear();
        }

        private void ClearETGridView()
        {
            GlobalData.experimentTheoreticalPairs.Clear();
        }

        private void FindAllETPairs()
        {
            DataTable eT = new DataTable();
            eT = GetNewET_DataTable();
            foreach (DataRow agRow in GlobalData.aggregatedProteoforms.Rows)
            {
                double lowMass = Convert.ToDouble(agRow["Aggregated Mass"]) + Convert.ToDouble(nUD_ET_Lower_Bound.Value);
                double highMass = Convert.ToDouble(agRow["Aggregated Mass"]) + Convert.ToDouble(nUD_ET_Upper_Bound.Value);

                string expression = "[Proteoform Mass] >= " + lowMass + " and [Proteoform Mass] <= " + highMass;
                expression = expression + "and [Lysine Count] >= " + agRow["Lysine Count"];

                DataRow[] closeTheoreticals = GlobalData.theoreticalAndDecoyDatabases.Tables["Target"].Select(expression);

                foreach (DataRow row in closeTheoreticals)
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
                    eT.Rows.Add(row["Accession"], row["Name"], row["Fragment"], row["PTM List"], row["Proteoform Mass"], agRow["Aggregated Mass"], agRow["Aggregated Intensity"], agRow["Aggregated Retention Time"], agRow["Lysine Count"], deltaMass, 0, 0, deltaMass, oOR, false);
                    //set out of range variable
                }
            }
            GlobalData.experimentTheoreticalPairs = eT;
        }

        private void CalculateRunningSums()
        {
            foreach (DataRow row in GlobalData.experimentTheoreticalPairs.Rows)
            {
                double deltaMass = Convert.ToDouble(row["Delta Mass"].ToString());
                double lower = deltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                double upper = deltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                string expression = "[Delta Mass] >= " + lower + " and [Delta Mass] <= " + upper;
                row["Running Sum"] = GlobalData.experimentTheoreticalPairs.Select(expression).Length;
            }
        }

        private void FillETGridView()
        {
            //Round before displaying ET grid
            string[] rt_column_names = new string[] { "Aggregated Retention Time" };
            string[] intensity_column_names = new string[] { "Aggregated Intensity" };
            string[] abundance_column_names = new string[] { };
            string[] mass_column_names = new string[] { "Proteoform Mass", "Aggregated Mass", "Delta Mass", "Peak Center Mass" };
            //string[] dec_mass_column_names = new string[] { };
            DataTable displayTable = GlobalData.experimentTheoreticalPairs;
            BindingSource dgv_DT_BS = dataTableHandler.DisplayWithRoundedDoubles(dgv_ET_Pairs, displayTable,
                rt_column_names, intensity_column_names, abundance_column_names, mass_column_names, new string[] { });
        }

        private void GraphETPeakList()
        {
            string colName = "Delta Mass";
            string direction = "DESC";
            DataTable dt = GlobalData.experimentTheoreticalPairs;
            dt.DefaultView.Sort = colName + " " + direction;
            dt = dt.DefaultView.ToTable();
            GlobalData.experimentTheoreticalPairs = dt;

            ct_ET_peakList.Series["etPeakList"].XValueMember = "Delta Mass";
            ct_ET_peakList.Series["etPeakList"].YValueMembers = "Running Sum";
            ct_ET_peakList.DataSource = GlobalData.experimentTheoreticalPairs;
            ct_ET_peakList.DataBind();
            ct_ET_peakList.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";

            ct_ET_peakList.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(dgv_ET_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) - Convert.ToDouble(nUD_PeakWidthBase.Value);
            ct_ET_peakList.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(dgv_ET_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) + Convert.ToDouble(nUD_PeakWidthBase.Value);
           // ct_ET_peakList.Series["etPeakList"].ToolTip = "#VALX{#.##}" + " , " + "#VALY{#.##}";
            ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
            {
                BorderColor = Color.Red,
                IntervalOffset = Convert.ToDouble(dgv_ET_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) + 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
            });

            ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
            {
                BorderColor = Color.Red,
                IntervalOffset = Convert.ToDouble(dgv_ET_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) - 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
            });
            ct_ET_peakList.ChartAreas[0].AxisX.Title = "Delta m/z";
            ct_ET_peakList.ChartAreas[0].AxisY.Title = "Peak Count";
        }

        private void dgv_ET_Peak_List_CellClick(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Left)
            {
                int clickedRow = dgv_ET_Peak_List.HitTest(e.X, e.Y).RowIndex;
                if (clickedRow >= 0 && clickedRow < GlobalData.etPeakList.Rows.Count)
                {
                    ETPeakListGraphParameters(clickedRow);
                }
            }
        }

        


        private void ETPeakListGraphParameters(int clickedRow)
        {
            ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Clear();
            double graphMax = Convert.ToDouble(dgv_ET_Peak_List.Rows[clickedRow].Cells["Average Delta Mass"].Value.ToString()) + Convert.ToDouble((nUD_PeakWidthBase.Value));
            double graphMin = Convert.ToDouble(dgv_ET_Peak_List.Rows[clickedRow].Cells["Average Delta Mass"].Value.ToString()) - Convert.ToDouble((nUD_PeakWidthBase.Value));

            if (graphMin < graphMax)
            {
                ct_ET_peakList.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(graphMin);
                ct_ET_peakList.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(graphMax);
            }

            ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
            {
                BorderColor = Color.Red,
                IntervalOffset = Convert.ToDouble(dgv_ET_Peak_List.Rows[clickedRow].Cells["Average Delta Mass"].Value.ToString()) + 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
            });

            ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
            {
                BorderColor = Color.Red,
                IntervalOffset = Convert.ToDouble(dgv_ET_Peak_List.Rows[clickedRow].Cells["Average Delta Mass"].Value.ToString()) - 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
            });
        }

        private void UpdateFiguresOfMerit()
        {
            int peakSum = 0;
            int peakCount = 0;
            DataRow[] bigPeaks = etPeakList.Select("[Peak Count] >= " + nUD_PeakCountMinThreshold.Value);
            foreach (DataRow row in bigPeaks)
            {
                peakSum = peakSum + Convert.ToInt32(row["Peak Count"]);
                peakCount++;
            }
            tb_IdentifiedProteoforms.Text = peakSum.ToString();
            tb_TotalPeaks.Text = peakCount.ToString();
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

        private void FillETPeakListTable()
        {
            string colName = "Running Sum";
            string direction = "DESC";

            DataTable dt = GlobalData.experimentTheoreticalPairs;
            dt.DefaultView.Sort = colName + " " + direction;
            dt = dt.DefaultView.ToTable();
            GlobalData.experimentTheoreticalPairs = dt;

            foreach (DataRow row in GlobalData.experimentTheoreticalPairs.Rows)
            {
                if (Convert.ToBoolean(row["Out of Range Decimal"].ToString())==false && Convert.ToBoolean(row["Acceptable Peak"].ToString()) == false)
                {
                    double deltaMass = Convert.ToDouble(row["Delta Mass"].ToString());
                    double lower = deltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    double upper = deltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    string expression = "[Delta Mass] >= " + lower + " and [Delta Mass] <= " + upper + " and [Acceptable Peak] = false";
                    DataRow[] firstSet = GlobalData.experimentTheoreticalPairs.Select(expression);
                    var firstAverage = firstSet.Average(fsRow => fsRow.Field<double>("Delta Mass"));

                    lower = firstAverage - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    upper = firstAverage + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    expression = "[Delta Mass] >= " + lower + " and [Delta Mass] <= " + upper + " and [Acceptable Peak] = false";
                    DataRow[] secondSet = GlobalData.experimentTheoreticalPairs.Select(expression);
                    var secondAverage = secondSet.Average(ssRow => ssRow.Field<double>("Delta Mass"));

                    foreach (DataRow rutrow in secondSet)
                    {
                        rutrow["Peak Center Count"] = secondSet.Length;
                        rutrow["Peak Center Mass"] = secondAverage;
                        rutrow["Acceptable Peak"] = true;

                        rutrow.EndEdit();
                    }

                    if (secondSet.Length >= nUD_PeakCountMinThreshold.Value)
                    {
                        etPeakList.Rows.Add(secondAverage, secondSet.Length, true);
                    }
                    else
                    {
                        etPeakList.Rows.Add(secondAverage, secondSet.Length, false);
                    }                  

                }

                GlobalData.experimentTheoreticalPairs.AcceptChanges();
            }
            GlobalData.etPeakList = etPeakList;

            //Round before displaying ET peak list
            string[] other_columns = new string[] { };
            string[] mass_column_names = new string[] { "Average Delta Mass" };
            //string[] dec_mass_column_names = new string[] { };
            BindingSource dgv_ET_Peak_List_BS = dataTableHandler.DisplayWithRoundedDoubles(dgv_ET_Peak_List, etPeakList,
                other_columns, other_columns, other_columns, mass_column_names, new string[] { });
        }



        private void InitializeETPeakListTable()
        {
            etPeakList.Columns.Add("Average Delta Mass", typeof(double));
            etPeakList.Columns.Add("Peak Count", typeof(int));
            etPeakList.Columns.Add("Acceptable", typeof(bool));
        }

        private void GraphETHistogram()
        {
            string colName = "Delta Mass";
            string direction = "DESC";

            DataTable dt = GlobalData.experimentTheoreticalPairs;
            dt.DefaultView.Sort = colName + " " + direction;
            dt = dt.DefaultView.ToTable();
            GlobalData.experimentTheoreticalPairs = dt;

            ct_ET_Histogram.Series["etHistogram"].XValueMember = "Delta Mass";
            ct_ET_Histogram.Series["etHistogram"].YValueMembers = "Running Sum";

            ct_ET_Histogram.DataSource = GlobalData.experimentTheoreticalPairs;
            ct_ET_Histogram.DataBind();
            ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Add(new StripLine()
    {
        BorderColor = Color.Red,
        IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value),
    });
          //  ct_ET_Histogram.Series["etHistogram"].ToolTip = "#VALX{#.##}" + " , " + "#VALY{#.##}";
            ct_ET_Histogram.ChartAreas[0].AxisX.Title = "Delta m/z";
            ct_ET_Histogram.ChartAreas[0].AxisY.Title = "Peak Count";
        }

        private DataTable GetNewET_DataTable()
        {
            DataTable dt = new DataTable();
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

        private void splitContainer3_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void nUD_ET_Lower_Bound_ValueChanged(object sender, EventArgs e)
        {
            //RunTheGamut();
        }

        private void nUD_ET_Upper_Bound_ValueChanged(object sender, EventArgs e)
        {
            //RunTheGamut();
        }

        private void yMaxET_ValueChanged(object sender, EventArgs e)
        {
            ct_ET_Histogram.ChartAreas[0].AxisY.Maximum = double.Parse(yMaxET.Value.ToString());
        }

        private void yMinET_ValueChanged(object sender, EventArgs e)
        {
            ct_ET_Histogram.ChartAreas[0].AxisY.Minimum = double.Parse(yMinET.Value.ToString());
        }

        private void xMinET_ValueChanged(object sender, EventArgs e)
        {
            ct_ET_Histogram.ChartAreas[0].AxisX.Minimum = double.Parse(xMinET.Value.ToString());
        }

        private void xMaxET_ValueChanged(object sender, EventArgs e)
        {
            ct_ET_Histogram.ChartAreas[0].AxisX.Maximum = double.Parse(xMaxET.Value.ToString());
        }

        private void nUD_NoManLower_ValueChanged(object sender, EventArgs e)
        {
           // RunTheGamut();
        }

        private void nUD_NoManUpper_ValueChanged(object sender, EventArgs e)
        {
            //RunTheGamut();
        }

        private void nUD_PeakWidthBase_ValueChanged(object sender, EventArgs e)
        {
            //RunTheGamut();
        }

        private void nUD_PeakCountMinThreshold_ValueChanged(object sender, EventArgs e)
        {
            //ClearETPeakListTable();
            //ZeroETPairsTableValues();
            //FillETPeakListTable();
            //UpdateFiguresOfMerit();
        }

        private void InitializeParameterSet()
        {
            nUD_ET_Lower_Bound.Minimum = -500;
            nUD_ET_Lower_Bound.Maximum = 0;
           nUD_ET_Lower_Bound.Value = -250;

            nUD_ET_Upper_Bound.Minimum = 0;
            nUD_ET_Upper_Bound.Maximum = 500;
            nUD_ET_Upper_Bound.Value = 250;

            yMaxET.Minimum = 0;
            yMaxET.Maximum = 1000;
            yMaxET.Value = 100;

            yMinET.Minimum = -100;
            yMinET.Maximum = yMaxET.Maximum;
            yMinET.Value = 0;

            xMinET.Minimum = nUD_ET_Lower_Bound.Value;
            xMinET.Maximum = xMaxET.Value;
            xMinET.Value = nUD_ET_Lower_Bound.Value;

            xMaxET.Minimum = xMinET.Value;
            xMaxET.Maximum = nUD_ET_Upper_Bound.Value;
            xMaxET.Value = nUD_ET_Upper_Bound.Value;

            nUD_NoManLower.Minimum = 00m;
            nUD_NoManLower.Maximum = 0.49m;
            nUD_NoManLower.Value = 0.22m;//add the m suffix to treat the double as decimal

            nUD_NoManUpper.Minimum = 0.50m;
            nUD_NoManUpper.Maximum = 1.00m;
            nUD_NoManUpper.Value = 0.88m;

            nUD_PeakWidthBase.Minimum = 0.001m;
            nUD_PeakWidthBase.Maximum = 0.5000m;
            nUD_PeakWidthBase.Value = 0.0150m;

            nUD_PeakCountMinThreshold.Minimum = 0;
            nUD_PeakCountMinThreshold.Maximum = 1000;
            nUD_PeakCountMinThreshold.Value = 10;
        }

        private void ET_Update_Click(object sender, EventArgs e)
        {
            RunTheGamut();
        }
    }
}
