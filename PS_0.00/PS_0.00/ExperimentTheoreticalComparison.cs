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
        DataTable etPeaksList = new DataTable();  // These are the aggregated peaks that are integrated from the histogram of the individual ET pairs.
        DataTable etPairsList = new DataTable(); // These are the individual experiment to theoretical pairs with delta mass less than the threshold
        Boolean formLoadEvent = true;

        public ExperimentTheoreticalComparison()
        {
            InitializeComponent();
            this.dgv_ET_Peak_List.MouseClick += new MouseEventHandler(dgv_ET_Peak_List_CellClick);
            this.ct_ET_Histogram.MouseMove += new MouseEventHandler(ct_ET_Histogram_MouseMove);
            this.ct_ET_peakList.MouseMove += new MouseEventHandler(ct_ET_peakList_MouseMove);
            dgv_ET_Peak_List.CurrentCellDirtyStateChanged += new EventHandler(peakListSpecificPeakAcceptanceChanged); //makes the change immediate and automatic
            dgv_ET_Peak_List.CellValueChanged += new DataGridViewCellEventHandler(propagatePeakListAcceptedPeakChangeToPairsTable); //when 'acceptance' of an ET peak gets changed, we change the ET pairs table.
        }

        public void ExperimentTheoreticalComparison_Load(object sender, EventArgs e)
        {
            if (GlobalData.experimentTheoreticalPairs.Columns.Count == 0)
            {
                run_comparison();
            }
            GraphETHistogram();
            FillETPeakListTable();
            FillETPairsGridView();
            GraphETPairsList();
        }

        public void run_comparison()
        {
            formLoadEvent = true;
            InitializeParameterSet();
            FindAllETPairs();
            CalculateRunningSums();
            etPeaksList = InitializeETPeakListTable();
            UpdateFiguresOfMerit();
            formLoadEvent = false;
        }

        private void RunTheGamut()
        {
            this.Cursor = Cursors.WaitCursor;
            ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Clear();
            FindAllETPairs();
            CalculateRunningSums();       
            GraphETHistogram();
            FillETPeakListTable();
            FillETPairsGridView();
            GraphETPairsList();
            UpdateFiguresOfMerit();
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

        private void MarkETPairsForProteoformFamilies()
        {
            foreach (DataRow row in etPairsList.Rows)
            {
                if (Convert.ToInt32(row["Peak Center Count"].ToString()) >= nUD_PeakCountMinThreshold.Value)
                {
                    row["Proteoform Family"] = true;
                }
                else
                {
                    row["Proteoform Family"] = false;
                }
            }
            etPairsList.AcceptChanges();
            GlobalData.experimentTheoreticalPairs = etPairsList;
            dgv_ET_Pairs.Update();
        }

        private void MarkETPeaksAsAcceptable()
        {
            foreach (DataRow row in etPeaksList.Rows)
            {
                if (Convert.ToInt32(row["Peak Count"].ToString()) >= nUD_PeakCountMinThreshold.Value)
                {
                    row["Acceptable"] = true;
                }
                else
                {
                    row["Acceptable"] = false;
                }
            }
            etPeaksList.AcceptChanges();
            GlobalData.etPeakList = etPeaksList;
            dgv_ET_Peak_List.Update();
        }

        private void ClearAllETPairs()
        {
            GlobalData.experimentTheoreticalPairs.Clear();
            etPairsList.Clear();
        }

        private void ClearAllETPeaks()
        {
            GlobalData.etPeakList.Clear();
            etPeaksList.Clear();
        }

        private void FindAllETPairs()
        {

            etPairsList.Clear();
            etPairsList = CreateETPairsDataTable();
            GlobalData.experimentTheoreticalPairs.Clear();

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

                    etPairsList.Rows.Add(row["Accession"], row["Name"], row["Fragment"], row["PTM List"], row["Proteoform Mass"], agRow["Aggregated Mass"], agRow["Aggregated Intensity"], agRow["Aggregated Retention Time"], agRow["Lysine Count"], deltaMass, 0, 0, deltaMass, oOR, false, false);

                }
            }
            etPairsList.AcceptChanges();
            GlobalData.experimentTheoreticalPairs = etPairsList;
        }

        private void CalculateRunningSums()
        {
            foreach (DataRow row in etPairsList.Rows)
            {
                double deltaMass = Convert.ToDouble(row["Delta Mass"].ToString());
                double lower = deltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                double upper = deltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                string expression = "[Delta Mass] >= " + lower + " and [Delta Mass] <= " + upper;
                row["Running Sum"] = etPairsList.Select(expression).Length;
            }
            etPairsList.AcceptChanges();
            GlobalData.experimentTheoreticalPairs = etPairsList;
        }

        private void FillETPairsGridView()
        {
            dgv_ET_Pairs.DataSource = etPairsList;
            dgv_ET_Pairs.ReadOnly = true;
            dgv_ET_Pairs.Columns["Acceptable Peak"].ReadOnly = false;
            dgv_ET_Pairs.Columns["Aggregated Retention Time"].DefaultCellStyle.Format = "0.##";
            dgv_ET_Pairs.Columns["Proteoform Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_ET_Pairs.Columns["Aggregated Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_ET_Pairs.Columns["Delta Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_ET_Pairs.Columns["Peak Center Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_ET_Pairs.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_ET_Pairs.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;

        }

        private void GraphETPairsList()
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
            try
            {
                int peakSum = 0;
                int peakCount = 0;
                DataRow[] bigPeaks = etPeaksList.Select("[Acceptable] = true");
                foreach (DataRow row in bigPeaks)
                {
                    peakSum = peakSum + Convert.ToInt32(row["Peak Count"]);
                    peakCount++;
                }
                tb_IdentifiedProteoforms.Text = peakSum.ToString();
                tb_TotalPeaks.Text = peakCount.ToString();
            }
            catch
            {

            }
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
            etPeaksList.Clear();
            GlobalData.etPeakList.Clear();

            string colName = "Running Sum";
            string direction = "DESC";

            DataTable dt = etPairsList;
            dt.DefaultView.Sort = colName + " " + direction;
            dt = dt.DefaultView.ToTable();
            etPairsList = dt;

            foreach (DataRow row in etPairsList.Rows)
            {
                if (Convert.ToBoolean(row["Out of Range Decimal"].ToString())==false && Convert.ToBoolean(row["Acceptable Peak"].ToString()) == false)
                {
                    double deltaMass = Convert.ToDouble(row["Delta Mass"].ToString());
                    double lower = deltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    double upper = deltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    string expression = "[Delta Mass] >= " + lower + " and [Delta Mass] <= " + upper + " and [Acceptable Peak] = false";
                    DataRow[] firstSet = etPairsList.Select(expression);
                    double firstAverage = firstSet.Average(fsRow => fsRow.Field<double>("Delta Mass"));

                    lower = firstAverage - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    upper = firstAverage + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    expression = "[Delta Mass] >= " + lower + " and [Delta Mass] <= " + upper + " and [Acceptable Peak] = false";
                    DataRow[] secondSet = etPairsList.Select(expression);
                    double secondAverage = secondSet.Average(ssRow => ssRow.Field<double>("Delta Mass"));

                    foreach (DataRow rutrow in secondSet)
                    {
                        rutrow["Peak Center Count"] = secondSet.Length;
                        rutrow["Peak Center Mass"] = secondAverage;
                        rutrow["Acceptable Peak"] = true;

                        if(secondSet.Length >= Convert.ToInt32(nUD_PeakCountMinThreshold.Value))
                        {
                            rutrow["Proteoform Family"] = true;
                        }
                        else
                        {
                            rutrow["Proteoform Family"] = false;
                        }
                        
                        rutrow.EndEdit();
                    }

                    if (secondSet.Length >= nUD_PeakCountMinThreshold.Value)
                    {
                        etPeaksList.Rows.Add(secondAverage, secondSet.Length, true);
                    }
                    else
                    {
                        etPeaksList.Rows.Add(secondAverage, secondSet.Length, false);
                    }                  

                }
                etPairsList.AcceptChanges();
                GlobalData.experimentTheoreticalPairs = etPairsList;
            }
            GlobalData.etPeakList = etPeaksList;


            dgv_ET_Peak_List.DataSource = etPeaksList;
            //dgv_ET_Peak_List.ReadOnly = true;
            dgv_ET_Peak_List.Columns["Average Delta Mass"].ReadOnly = true;
            dgv_ET_Peak_List.Columns["Peak Count"].ReadOnly = true;
            dgv_ET_Peak_List.Columns["Average Delta Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_ET_Peak_List.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_ET_Peak_List.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
            dgv_ET_Peak_List.EndEdit();
            dgv_ET_Peak_List.Refresh();

        }

        private void propagatePeakListAcceptedPeakChangeToPairsTable(object sender, DataGridViewCellEventArgs e)
        {
            double averageDeltaMass = Convert.ToDouble(dgv_ET_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex - 2].Value);
            int peakCount = Convert.ToInt32(dgv_ET_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex - 1].Value);
            dgv_ET_Peak_List.EndEdit();
            dgv_ET_Peak_List.Update();

            double lowMass = averageDeltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
            double highMass = averageDeltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;

            string expression = "[Average Delta Mass] > " + lowMass + " and [Average Delta Mass] < " + highMass;
            DataRow[] selectedPeaks = etPeaksList.Select(expression);

            foreach (DataRow row in selectedPeaks)
            {
                row["Acceptable"] = Convert.ToBoolean(dgv_ET_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
            }
            etPeaksList.AcceptChanges();
            GlobalData.etPeakList = etPeaksList;
            dgv_ET_Peak_List.Update();
            dgv_ET_Peak_List.Refresh();

            expression = "[Peak Center Mass] > " + lowMass + " and [Peak Center Mass] < " + highMass;

            selectedPeaks = etPairsList.Select(expression);

            foreach (DataRow row in selectedPeaks)
            {
                row["Proteoform Family"] = Convert.ToBoolean(dgv_ET_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
            }
            etPairsList.AcceptChanges();

            GlobalData.experimentTheoreticalPairs = etPairsList;
            dgv_ET_Pairs.Update();
            dgv_ET_Pairs.Refresh();

            UpdateFiguresOfMerit();
            
        }

        private void peakListSpecificPeakAcceptanceChanged(object sender, EventArgs e)
        {
            if (dgv_ET_Peak_List.IsCurrentCellDirty)
            {                
                dgv_ET_Peak_List.EndEdit();
                dgv_ET_Peak_List.Update();
            }
        }

        private DataTable InitializeETPeakListTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Average Delta Mass", typeof(double));
            dt.Columns.Add("Peak Count", typeof(int));
            dt.Columns.Add("Acceptable", typeof(bool));

            return dt;
        }

        private void GraphETHistogram()
        {

            try
            {
                string colName = "Delta Mass";
                string direction = "DESC";

                DataTable dt = etPairsList;
                dt.DefaultView.Sort = colName + " " + direction;
                dt = dt.DefaultView.ToTable();
                etPairsList = dt;

                ct_ET_Histogram.Series["etHistogram"].XValueMember = "Delta Mass";
                ct_ET_Histogram.Series["etHistogram"].YValueMembers = "Running Sum";

                ct_ET_Histogram.DataSource = etPairsList;
                ct_ET_Histogram.DataBind();

                ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Clear();

                ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Add(new StripLine()
                {
                    BorderColor = Color.Red,
                    IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value),
                });


                //  ct_ET_Histogram.Series["etHistogram"].ToolTip = "#VALX{#.##}" + " , " + "#VALY{#.##}";
                ct_ET_Histogram.ChartAreas[0].AxisX.Title = "Delta m/z";
                ct_ET_Histogram.ChartAreas[0].AxisY.Title = "Peak Count";
            }
            catch
            {

            }
            
        }

        private DataTable CreateETPairsDataTable()
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
            dt.Columns.Add("Acceptable Peak", typeof(bool)); //place holder really for pairs that get included in what will eventually be ET Peaks. Kind of meaningless to show user
            dt.Columns.Add("Proteoform Family", typeof(bool)); //true for pairs that should be used in the constructionof proteoform families

            return dt;
        }

        private void splitContainer3_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void nUD_ET_Lower_Bound_ValueChanged(object sender, EventArgs e) // maximum delta mass for theoretical proteoform that has mass LOWER than the experimental protoform mass
        {
            if (!formLoadEvent)
            {
                RunTheGamut();
            }
            
        }

        private void nUD_ET_Upper_Bound_ValueChanged(object sender, EventArgs e) // maximum delta mass for theoretical proteoform that has mass HIGHER than the experimental protoform mass
        {
            if (!formLoadEvent)
            {
                RunTheGamut();
            }
        }

        private void yMaxET_ValueChanged(object sender, EventArgs e) // scaling for y-axis of displayed ET Histogram of all ET pairs
        {

            if (!formLoadEvent)
            {
                ct_ET_Histogram.ChartAreas[0].AxisY.Maximum = double.Parse(yMaxET.Value.ToString());
            }

        }

        private void yMinET_ValueChanged(object sender, EventArgs e) // scaling for y-axis of displayed ET Histogram of all ET pairs
        {
            if (!formLoadEvent)
            {
                ct_ET_Histogram.ChartAreas[0].AxisY.Minimum = double.Parse(yMinET.Value.ToString());
            }         
        }

        private void xMinET_ValueChanged(object sender, EventArgs e) // scaling for x-axis of displayed ET Histogram of all ET pairs
        {
            if (!formLoadEvent)
            {
                ct_ET_Histogram.ChartAreas[0].AxisX.Minimum = double.Parse(xMinET.Value.ToString());
            }
            
        }

        private void xMaxET_ValueChanged(object sender, EventArgs e) // scaling for x-axis of displayed ET Histogram of all ET pairs
        {
            if (!formLoadEvent)
            {
                ct_ET_Histogram.ChartAreas[0].AxisX.Maximum = double.Parse(xMaxET.Value.ToString());
            }
            
        }

        private void nUD_NoManLower_ValueChanged(object sender, EventArgs e) // lower bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts
        {
            if (!formLoadEvent)
            {
                RunTheGamut();
            }
        }

        private void nUD_NoManUpper_ValueChanged(object sender, EventArgs e)// upper bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts
        {
            if (!formLoadEvent)
            {
                RunTheGamut();
            }
        }

        private void nUD_PeakWidthBase_ValueChanged(object sender, EventArgs e) // bin size used for including individual ET pairs in one 'Peak Center Mass' and peak with for one ET peak
        {
            if (!formLoadEvent)
            {
                RunTheGamut();
            }
        }

        private void nUD_PeakCountMinThreshold_ValueChanged(object sender, EventArgs e) // ET pairs with [Peak Center Count] AND ET peaks with [Peak Count] above this value are considered acceptable for use in proteoform family. this will be eventually set following ED analysis.
        {

            if (!formLoadEvent)
            {
                MarkETPairsForProteoformFamilies();
                MarkETPeaksAsAcceptable();
                GraphETHistogram(); //we do this hear because the redline threshold needs to be redrawn
                UpdateFiguresOfMerit();
            }
            
        }

        private void InitializeParameterSet()
        {
            nUD_ET_Lower_Bound.Minimum = -500;
            nUD_ET_Lower_Bound.Maximum = 0;
            nUD_ET_Lower_Bound.Value = -250; // maximum delta mass for theoretical proteoform that has mass LOWER than the experimental protoform mass

            nUD_ET_Upper_Bound.Minimum = 0;
            nUD_ET_Upper_Bound.Maximum = 500;
            nUD_ET_Upper_Bound.Value = 250; // maximum delta mass for theoretical proteoform that has mass HIGHER than the experimental protoform mass

            yMaxET.Minimum = 0;
            yMaxET.Maximum = 1000;
            yMaxET.Value = 100; // scaling for y-axis of displayed ET Histogram of all ET pairs

            yMinET.Minimum = -100;
            yMinET.Maximum = yMaxET.Maximum;
            yMinET.Value = 0; // scaling for y-axis of displayed ET Histogram of all ET pairs

            xMinET.Minimum = nUD_ET_Lower_Bound.Value;
            xMinET.Maximum = xMaxET.Value;
            xMinET.Value = nUD_ET_Lower_Bound.Value; // scaling for x-axis of displayed ET Histogram of all ET pairs

            xMaxET.Minimum = xMinET.Value;
            xMaxET.Maximum = nUD_ET_Upper_Bound.Value;
            xMaxET.Value = nUD_ET_Upper_Bound.Value; // scaling for x-axis of displayed ET Histogram of all ET pairs

            nUD_NoManLower.Minimum = 00m;
            nUD_NoManLower.Maximum = 0.49m;
            nUD_NoManLower.Value = 0.22m; // lower bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts

            nUD_NoManUpper.Minimum = 0.50m;
            nUD_NoManUpper.Maximum = 1.00m;
            nUD_NoManUpper.Value = 0.88m; // upper bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts

            nUD_PeakWidthBase.Minimum = 0.001m;
            nUD_PeakWidthBase.Maximum = 0.5000m;
            nUD_PeakWidthBase.Value = 0.0150m; // bin size used for including individual ET pairs in one 'Peak Center Mass' and peak with for one ET peak

            nUD_PeakCountMinThreshold.Minimum = 0;
            nUD_PeakCountMinThreshold.Maximum = 1000;
            nUD_PeakCountMinThreshold.Value = 10; // ET pairs with [Peak Center Count] AND ET peaks with [Peak Count] above this value are considered acceptable for use in proteoform family. this will be eventually set following ED analysis.
        }

        private void ET_Update_Click(object sender, EventArgs e)
        {
            RunTheGamut();
        }

        public override string ToString()
        {
            return String.Join(System.Environment.NewLine, new string[] {
                "ExperimentTheoreticalComparison|nUD_NoManLower.Value\t" + nUD_NoManLower.Value.ToString(),
                "ExperimentTheoreticalComparison|nUD_NoManUpper.Value\t" + nUD_NoManUpper.Value.ToString(),
                "ExperimentTheoreticalComparison|nUD_PeakWidthBase.Value\t" + nUD_PeakWidthBase.Value.ToString(),
                "ExperimentTheoreticalComparison|nUD_PeakCountMinThreshold.Value\t" + nUD_PeakCountMinThreshold.Value.ToString()
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
                case "nUD_PeakCountMinThreshold.Value":
                    nUD_PeakCountMinThreshold.Value = Convert.ToDecimal(fields[1]);
                    break;
            }
        }
    }
}
