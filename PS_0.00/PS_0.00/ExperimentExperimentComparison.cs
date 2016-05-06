using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Odbc;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace PS_0._00
{ 
    public partial class ExperimentExperimentComparison : Form
    {
        DataTable eePairsList = new DataTable(); // this is a list of all individual EE pairs with mass difference smaller than the threshold
        DataTable eePeakList = new DataTable(); // these are the aggregated peaks coming from groups of individual EE pairs.
        //DataTableHandler dataTableHandler = new DataTableHandler();
        Boolean formLoadEvent; // this is needed to prevent firing of ParameterSet events from firing on form load and let them fire only when the values are actually changed

        public ExperimentExperimentComparison()
        {
            InitializeComponent();
            this.dgv_EE_Peak_List.MouseClick += new MouseEventHandler(dgv_EE_Peak_List_CellClick);
            dgv_EE_Peak_List.CurrentCellDirtyStateChanged += new EventHandler(peakListSpecificPeakAcceptanceChanged); //makes the change immediate and automatic
            dgv_EE_Peak_List.CellValueChanged += new DataGridViewCellEventHandler(propagatePeakListAcceptedPeakChangeToPairsTable); //when 'acceptance' of an ET peak gets changed, we change the ET pairs table.
        }

        private void ExperimentExperimentComparison_Load(object sender, EventArgs e)

        {
            this.Cursor = Cursors.WaitCursor;
            formLoadEvent = true;
            InitializeParameterSet();
            FindAllEEPairs();
            CalculateRunningSums();
            GraphEEHistogram();
            eePeakList = InitializeEEPeakListTable();
            FillEEPeakListTable();
            FillEEPairsGridView();
            GraphEEPairsList();
            UpdateFiguresOfMerit();
            formLoadEvent = false;
            this.Cursor = Cursors.Default;
        }

        private void RunTheGamut()
        {
            this.Cursor = Cursors.WaitCursor;
            ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            ct_EE_peakList.ChartAreas[0].AxisX.StripLines.Clear();
            FindAllEEPairs();
            CalculateRunningSums();
            GraphEEHistogram();
            FillEEPeakListTable();
            FillEEPairsGridView();
            GraphEEPairsList();
            UpdateFiguresOfMerit();
            xMaxEE.Value = nUD_EE_Upper_Bound.Value;
            this.Cursor = Cursors.Default;
        }

        private void dgv_EE_Peak_List_CellClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int clickedRow = dgv_EE_Peak_List.HitTest(e.X, e.Y).RowIndex;
                if (clickedRow >= 0 && clickedRow < GlobalData.eePeakList.Rows.Count)
                {
                    EEPeakListGraphParameters(clickedRow);
                }
            }
        }

        private void GraphEEPairsList()
        {
            string colName = "Delta Mass";
            string direction = "DESC";
            DataTable dt = eePairsList;
            dt.DefaultView.Sort = colName + " " + direction;
            dt = dt.DefaultView.ToTable();
            eePairsList = dt;

            ct_EE_peakList.Series["eePeakList"].XValueMember = "Delta Mass";
            ct_EE_peakList.Series["eePeakList"].YValueMembers = "Running Sum";
            ct_EE_peakList.DataSource = eePairsList;
            ct_EE_peakList.DataBind();
            ct_EE_peakList.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";

            ct_EE_peakList.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(dgv_EE_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) - Convert.ToDouble(nUD_PeakWidthBase.Value);
            ct_EE_peakList.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(dgv_EE_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) + Convert.ToDouble(nUD_PeakWidthBase.Value);
            ct_EE_peakList.Series["eePeakList"].ToolTip = "#VALX{#.##}" + " , " + "#VALY{#.##}";
            ct_EE_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
            {
                BorderColor = Color.Red,
                IntervalOffset = Convert.ToDouble(dgv_EE_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) + 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
            });

            ct_EE_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
            {
                BorderColor = Color.Red,
                IntervalOffset = Convert.ToDouble(dgv_EE_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) - 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
            });
            ct_EE_peakList.ChartAreas[0].AxisX.Title = "Delta m/z";
            ct_EE_peakList.ChartAreas[0].AxisY.Title = "Peak Count";
        }


        private void EEPeakListGraphParameters(int clickedRow)
        {
            ct_EE_peakList.ChartAreas[0].AxisX.StripLines.Clear();
            double graphMax = Convert.ToDouble(dgv_EE_Peak_List.Rows[clickedRow].Cells["Average Delta Mass"].Value.ToString()) + Convert.ToDouble((nUD_PeakWidthBase.Value));
            double graphMin = Convert.ToDouble(dgv_EE_Peak_List.Rows[clickedRow].Cells["Average Delta Mass"].Value.ToString()) - Convert.ToDouble((nUD_PeakWidthBase.Value));

            if (graphMin < graphMax)
            {
                ct_EE_peakList.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(graphMax);

                ct_EE_peakList.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(graphMin);
            }

            if (graphMin < 0){
                ct_EE_peakList.ChartAreas[0].AxisX.Minimum = 0;
            }

            if (graphMax > Convert.ToDouble(nUD_EE_Upper_Bound.Value))
            {
                ct_EE_peakList.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(nUD_EE_Upper_Bound.Value);
            }

            ct_EE_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
                {
                     BorderColor = Color.Red,
                     IntervalOffset = Convert.ToDouble(dgv_EE_Peak_List.Rows[clickedRow].Cells["Average Delta Mass"].Value.ToString()) + 0.5*Convert.ToDouble((nUD_PeakWidthBase.Value)),
                });

            ct_EE_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
            {
                BorderColor = Color.Red,
                IntervalOffset = Convert.ToDouble(dgv_EE_Peak_List.Rows[clickedRow].Cells["Average Delta Mass"].Value.ToString()) - 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
            });
        }

        //private void MarkEEPairsForProteoformFamilies()
        //{
        //    foreach (DataRow row in eePairsList.Rows)
        //    {
        //        if (Convert.ToInt32(row["Peak Center Count"].ToString()) >= nUD_PeakCountMinThreshold.Value)
        //        {
        //            row["Proteoform Family"] = true;
        //        }
        //        else
        //        {
        //            row["Proteoform Family"] = false;
        //        }
        //    }
        //    eePairsList.AcceptChanges();
        //    GlobalData.experimentExperimentPairs = eePairsList;
        //    dgv_EE_Pairs.Update();
        //}

        //private void MarkEEPeaksAsAcceptable()
        //{
        //    foreach (DataRow row in eePeakList.Rows)
        //    {
        //        if (Convert.ToInt32(row["Peak Count"].ToString()) >= nUD_PeakCountMinThreshold.Value)
        //        {
        //            row["Acceptable"] = true;
        //        }
        //        else
        //        {
        //            row["Acceptable"] = false;
        //        }
        //    }
        //    eePeakList.AcceptChanges();
        //    GlobalData.eePeakList = eePeakList;
        //    dgv_EE_Peak_List.Update();
        //}

        private void UpdateFiguresOfMerit()
        {
            try
            {
                int peakSum = 0;
                int peakCount = 0;
                DataRow[] bigPeaks = eePeakList.Select("[Acceptable] = true");
                foreach (DataRow row in bigPeaks)
                {
                    peakSum = peakSum + Convert.ToInt32(row["Peak Count"]);
                    peakCount++;
                }

                tb_TotalPeaks.Text = peakCount.ToString();
                tb_IdentifiedProteoforms.Text = peakSum.ToString();
            }
            catch
            {
                MessageBox.Show("catch in update figures of merit");
            }

        }

        private void ZeroEEPairsTableValues()
        {
            foreach (DataRow row in GlobalData.experimentExperimentPairs.Rows)
            {
                row["Acceptable Peak"] = false;
                row["Peak Center Count"] = 0;
            }
        }

        private void ClearEEPeakListTable()
        {
            eePeakList.Clear();
        }

        private void ClearEEGridView()
        {
            GlobalData.experimentExperimentPairs.Clear();
        }

        private void FillEEPeakListTable()
        {
            eePeakList.Clear();
            GlobalData.eePeakList.Clear();

            string colName = "Running Sum";
            string direction = "DESC";

            DataTable dt = eePairsList;
            dt.DefaultView.Sort = colName + " " + direction;
            dt = dt.DefaultView.ToTable();
            eePairsList = dt;

            foreach (DataRow row in eePairsList.Rows)
            {
                if (
                    Convert.ToBoolean(row["Out of Range Decimal"].ToString()) == false &&
                    Convert.ToBoolean(row["Acceptable Peak"].ToString()) == false)
                {
                    double deltaMass = Convert.ToDouble(row["Delta Mass"].ToString());
                    double lower = deltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    double upper = deltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    string expression = "[Delta Mass] >= " + lower + " and [Delta Mass] <=" + upper +
                        "and [Acceptable Peak] = false";
                    DataRow[] firstSet = eePairsList.Select(expression);
                    var firstAverage = firstSet.Average(fsRow => fsRow.Field<double>("Delta Mass"));

                    lower = firstAverage - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    upper = firstAverage + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                    expression = "[Delta Mass] >= " + lower + " and [Delta Mass] <= " + upper +
                        "and [Acceptable Peak] = false";
                    DataRow[] secondSet = eePairsList.Select(expression);
                    var secondAverage = secondSet.Average(ssRow => ssRow.Field<double>("Delta Mass"));

                    foreach (DataRow rutrow in secondSet)
                    {
                        rutrow["Peak Center Count"] = secondSet.Length;
                        rutrow["Peak Center Mass"] = secondAverage;
                        rutrow["Acceptable Peak"] = true;

                        if (secondSet.Length >= Convert.ToInt32(nUD_PeakCountMinThreshold.Value))
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
                        eePeakList.Rows.Add(secondAverage, secondSet.Length, true);
                    }
                    else
                    {
                        eePeakList.Rows.Add(secondAverage, secondSet.Length, false);
                    }
                }
                eePairsList.AcceptChanges();
                GlobalData.experimentExperimentPairs = eePairsList;
            }

            GlobalData.eePeakList = eePeakList;

            dgv_EE_Peak_List.DataSource = eePeakList;
            dgv_EE_Peak_List.Columns["Average Delta Mass"].ReadOnly = true;
            dgv_EE_Peak_List.Columns["Peak Count"].ReadOnly = true;
            dgv_EE_Peak_List.Columns["Average Delta Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_EE_Peak_List.EndEdit();
            dgv_EE_Peak_List.Refresh();

        }

        private DataTable InitializeEEPeakListTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Average Delta Mass", typeof(double));
            dt.Columns.Add("Peak Count", typeof(int));
            dt.Columns.Add("Acceptable", typeof(bool));

            return dt;
        }


        private void GraphEEHistogram()

        {
            try
            {
                string colName = "Delta Mass";
                string direction = "DESC";

                DataTable dt = eePairsList;
                dt.DefaultView.Sort = colName + " " + direction;
                dt = dt.DefaultView.ToTable();
                eePairsList = dt;

                ct_EE_Histogram.Series["eeHistogram"].XValueMember = "Delta Mass";
                ct_EE_Histogram.Series["eeHistogram"].YValueMembers = "Running Sum";
                ct_EE_Histogram.DataSource = eePairsList;
                ct_EE_Histogram.DataBind();
                ct_EE_Histogram.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
                ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Add(
                    new StripLine()
                    {
                        BorderColor = Color.Red,
                        IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value),
                    });

                // ct_EE_Histogram.Series["eeHistogram"].ToolTip = "#VALX{#.##}" + " , " + "#VALY{#.##}";
                ct_EE_Histogram.ChartAreas[0].AxisX.Title = "Delta m/z";
                ct_EE_Histogram.ChartAreas[0].AxisY.Title = "Peak Count";
            }
            catch
            {

            }
            
        }

        private void FillEEPairsGridView()
        {
            dgv_EE_Pairs.DataSource = eePairsList;
            dgv_EE_Pairs.ReadOnly = true;
            dgv_EE_Pairs.Columns["Acceptable Peak"].ReadOnly = false;
            dgv_EE_Pairs.Columns["Aggregated Mass Light"].DefaultCellStyle.Format = "0.#####";
            dgv_EE_Pairs.Columns["Aggregated Mass Heavy"].DefaultCellStyle.Format = "0.#####";
            dgv_EE_Pairs.Columns["Retention Time Light"].DefaultCellStyle.Format = "0.##";
            dgv_EE_Pairs.Columns["Retention Time Heavy"].DefaultCellStyle.Format = "0.##";
            dgv_EE_Pairs.Columns["Delta Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_EE_Pairs.Columns["Peak Center Mass"].DefaultCellStyle.Format = "0.#####";
        }

        private DataTable CreateEEPairsDataTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Aggregated Mass Light", typeof(double));
            dt.Columns.Add("Aggregated Mass Heavy", typeof(double));
            dt.Columns.Add("Aggregated Intensity Light", typeof(double));
            dt.Columns.Add("Aggregated Intensity Heavy", typeof(double));
            dt.Columns.Add("Retention Time Light", typeof(double));
            dt.Columns.Add("Retention Time Heavy", typeof(double));
            dt.Columns.Add("Lysine Count", typeof(int));
            dt.Columns.Add("Delta Mass", typeof(double));
            dt.Columns.Add("Running Sum", typeof(int));
            dt.Columns.Add("Peak Center Count", typeof(int));
            dt.Columns.Add("Peak Center Mass", typeof(double));
            dt.Columns.Add("Out of Range Decimal", typeof(bool));
            dt.Columns.Add("Acceptable Peak", typeof(bool));
            dt.Columns.Add("Proteoform Family", typeof(bool));

            return dt;
        }


        private void FindAllEEPairs()
        {
            eePairsList.Clear();
            eePairsList = CreateEEPairsDataTable();
            GlobalData.experimentExperimentPairs.Clear();

            int numRows = GlobalData.aggregatedProteoforms.Rows.Count;

            for (int index1 = 0; index1 < numRows; index1++)
            {
                for (int index2 = 0; index2 < numRows; index2++)
                {

                    double massLight = Convert.ToDouble(GlobalData.aggregatedProteoforms.Rows[index1]["Aggregated Mass"]);
                    double massHeavy = Convert.ToDouble(GlobalData.aggregatedProteoforms.Rows[index2]["Aggregated Mass"]);
                    int lysineLight = Convert.ToInt16(GlobalData.aggregatedProteoforms.Rows[index1]["Lysine Count"]);
                    int lysineHeavy = Convert.ToInt16(GlobalData.aggregatedProteoforms.Rows[index2]["Lysine Count"]);

                    if (massHeavy > massLight)
                    {
                        if (lysineLight == lysineHeavy)
                        {
                            double deltaMass = massHeavy - massLight;

                            if (deltaMass < Convert.ToDouble(nUD_EE_Upper_Bound.Value))
                            {
                                double afterDecimal = Math.Abs(deltaMass - Math.Truncate(deltaMass));
                                bool oOR = true;
                                if (afterDecimal <= Convert.ToDouble(nUD_NoManLower.Value) ||
                                    afterDecimal >= Convert.ToDouble(nUD_NoManUpper.Value))
                                {
                                    oOR = false;
                                }

                                eePairsList.Rows.Add(massLight, massHeavy, GlobalData.aggregatedProteoforms.Rows[index1]["Aggregated Intensity"], GlobalData.aggregatedProteoforms.Rows[index2]["Aggregated Intensity"], GlobalData.aggregatedProteoforms.Rows[index1]["Aggregated Retention Time"], GlobalData.aggregatedProteoforms.Rows[index2]["Aggregated Retention Time"], lysineLight, deltaMass, 0, 0, deltaMass, oOR, false, false);

                            }
                        }
                    }
                }
            }

            GlobalData.experimentExperimentPairs = eePairsList;
        }

        private void CalculateRunningSums()
        {
            foreach (DataRow row in eePairsList.Rows)
            {
                double deltaMass = Convert.ToDouble(row["Delta Mass"].ToString());
                double lower = deltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                double upper = deltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
                string expression = "[Delta Mass] >= " + lower + "and [Delta Mass] <= " + upper;
                row["Running Sum"] = eePairsList.Select(expression).Length;
            }
            eePairsList.AcceptChanges();
            GlobalData.experimentExperimentPairs = eePairsList;
        }


        private void InitializeParameterSet()
        {
            nUD_EE_Upper_Bound.Minimum = 0;
            nUD_EE_Upper_Bound.Maximum = 500;
            //nUD_EE_Upper_Bound.Value = 500; // maximum mass difference in Da allowed between experimental pairs

            yMaxEE.Minimum = 0;
            yMaxEE.Maximum = 1000;
            yMaxEE.Value = 100; // scaling for y-axis maximum in the histogram of all EE pairs

            yMinEE.Minimum = -100;
            yMinEE.Maximum = yMaxEE.Maximum;
            yMinEE.Value = 0; // scaling for y-axis minimum in the histogram of all EE pairs

            xMaxEE.Minimum = xMinEE.Value;
            xMaxEE.Maximum = nUD_EE_Upper_Bound.Value;
            xMaxEE.Value = nUD_EE_Upper_Bound.Value; // scaling for x-axis maximum in the histogram of all EE pairs

            xMinEE.Minimum = 0;
            xMinEE.Maximum = xMaxEE.Value;
            xMinEE.Value = 0; // scaling for x-axis minimum in the histogram of all EE pairs

            nUD_NoManLower.Minimum = 00m;
            nUD_NoManLower.Maximum = 0.49m;
            //  nUD_NoManLower.Value = 0.22m;


            nUD_NoManUpper.Minimum = 0.50m;
            nUD_NoManUpper.Maximum = 1.00m;
            //   nUD_NoManUpper.Value = 0.88m;


            nUD_PeakWidthBase.Minimum = 0.001m;
            nUD_PeakWidthBase.Maximum = 0.5000m;
            //   nUD_PeakWidthBase.Value = 0.0150m;

            nUD_PeakCountMinThreshold.Minimum = 0;
            nUD_PeakCountMinThreshold.Maximum = 1000;
            nUD_PeakCountMinThreshold.Value = 10;
        }

        private void propagatePeakListAcceptedPeakChangeToPairsTable(object sender, DataGridViewCellEventArgs e)
        {

            double averageDeltaMass = Convert.ToDouble(dgv_EE_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex - 2].Value);
            int peakCount = Convert.ToInt32(dgv_EE_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex - 1].Value);
            dgv_EE_Peak_List.EndEdit();
            dgv_EE_Peak_List.Update();

            double lowMass = averageDeltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
            double highMass = averageDeltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;

            string expression = "[Average Delta Mass] > " + lowMass + " and [Average Delta Mass] < " + highMass;

            DataRow[] selectedPeaks = eePeakList.Select(expression);

            foreach (DataRow row in selectedPeaks)
            {
                row["Acceptable"] = Convert.ToBoolean(dgv_EE_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
            }
            eePeakList.AcceptChanges();
            GlobalData.etPeakList = eePeakList;
            dgv_EE_Peak_List.Update();
            dgv_EE_Peak_List.Refresh();

            expression = "[Peak Center Mass] > " + lowMass + " and [Peak Center Mass] < " + highMass;

            selectedPeaks = eePairsList.Select(expression);

            foreach (DataRow row in selectedPeaks)
            {

                row["Proteoform Family"] = Convert.ToBoolean(dgv_EE_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);

            }
            eePairsList.AcceptChanges();

            GlobalData.experimentExperimentPairs = eePairsList;
            dgv_EE_Pairs.DataSource = null;
            FillEEPairsGridView();

            UpdateFiguresOfMerit();

        }

        private void peakListSpecificPeakAcceptanceChanged(object sender, EventArgs e)
        {
            if (dgv_EE_Peak_List.IsCurrentCellDirty)
            {
                dgv_EE_Peak_List.EndEdit();
                dgv_EE_Peak_List.Update();
            }
        }

        private void xMaxEE_ValueChanged(object sender, EventArgs e) // scaling for x-axis maximum in the histogram of all EE pairs
        {
            if (!formLoadEvent)
            {
                double newXMaxEE = double.Parse(xMaxEE.Value.ToString());
                if (newXMaxEE > double.Parse(xMinEE.Value.ToString()))
                {
                    ct_EE_Histogram.ChartAreas[0].AxisX.Maximum = newXMaxEE;
                }
            }          
        }

        private void yMaxEE_ValueChanged(object sender, EventArgs e) // scaling for y-axis maximum in the histogram of all EE pairs
        {
            if (!formLoadEvent)
            {
                double newYMaxEE = double.Parse(yMaxEE.Value.ToString());
                if (newYMaxEE > double.Parse(yMinEE.Value.ToString()))
                {
                    ct_EE_Histogram.ChartAreas[0].AxisY.Maximum = double.Parse(yMaxEE.Value.ToString());
                }
            }           
        }

        private void yMinEE_ValueChanged(object sender, EventArgs e) // scaling for y-axis minimum in the histogram of all EE pairs
        {
            if (!formLoadEvent)
            {
                double newYMinEE = double.Parse(yMinEE.Value.ToString());
                if (newYMinEE < double.Parse(yMaxEE.Value.ToString()))
                {
                    ct_EE_Histogram.ChartAreas[0].AxisY.Minimum = double.Parse(yMinEE.Value.ToString());
                }
            }           
        }

        private void xMinEE_ValueChanged(object sender, EventArgs e) // scaling for x-axis maximum in the histogram of all EE pairs
        {
            if (!formLoadEvent)
            {
                double newXMinEE = double.Parse(xMinEE.Value.ToString());
                if (newXMinEE < double.Parse(xMaxEE.Value.ToString()))
                {
                    ct_EE_Histogram.ChartAreas[0].AxisX.Minimum = newXMinEE;
                }
            }           
        }


        private void cb_Graph_lowerThreshold_CheckedChanged(object sender, EventArgs e)
        {
            if (!formLoadEvent)
            {
                if (cb_Graph_lowerThreshold.Checked)
                {
                    ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Add(
                  new StripLine()
                  {
                      BorderColor = Color.Red,
                      IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value),
                  });
                }

                else if (!cb_Graph_lowerThreshold.Checked)
                {
                    ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Clear();

                }
            }         
        }

        private void EE_update_Click(object sender, EventArgs e)
        {
            RunTheGamut();
        }
    }
}