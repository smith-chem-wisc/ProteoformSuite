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

namespace ProteoformSuite
{ 
    public partial class ExperimentExperimentComparison : Form
    {
        public ExperimentExperimentComparison()
        {
            InitializeComponent();
            this.dgv_EE_Peak_List.MouseClick += new MouseEventHandler(dgv_EE_Peak_List_CellClick);
            this.ct_EE_Histogram.MouseMove += new MouseEventHandler(ct_EE_Histogram_MouseMove);
            this.ct_EE_peakList.MouseMove += new MouseEventHandler(ct_EE_peakList_MouseMove);
            dgv_EE_Peak_List.CurrentCellDirtyStateChanged += new EventHandler(peakListSpecificPeakAcceptanceChanged); //makes the change immediate and automatic
            dgv_EE_Peak_List.CellValueChanged += new DataGridViewCellEventHandler(propagatePeakListAcceptedPeakChangeToPairsTable); //when 'acceptance' of an ET peak gets changed, we change the ET pairs table.
        }

        public void ExperimentExperimentComparison_Load(object sender, EventArgs e)
        {
            if (Lollipop.ee_relations.Count == 0)
            {
                run_comparison();
            }
            GraphEEHistogram();
            FillEEPeakListTable();
            FillEEPairsGridView();
            GraphEEPairsList();
        }

        public void run_comparison()
        {
            this.Cursor = Cursors.WaitCursor;
            InitializeParameterSet();
            UpdateFiguresOfMerit();
            this.Cursor = Cursors.Default;
        }

        private void RunTheGamut()
        {
            this.Cursor = Cursors.WaitCursor;
            ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            ct_EE_peakList.ChartAreas[0].AxisX.StripLines.Clear();
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
                if (clickedRow >= 0 && clickedRow < Lollipop.eePeakList.Rows.Count)
                {
                    EEPeakListGraphParameters(clickedRow);
                }
            }
        }

        private void GraphEEPairsList()
        {
           // string colName = "Delta Mass";
           // string direction = "DESC";
           // DataTable dt = eePairsList;
           // dt.DefaultView.Sort = colName + " " + direction;
           // dt = dt.DefaultView.ToTable();
           // eePairsList = dt;

           // ct_EE_peakList.Series["eePeakList"].XValueMember = "Delta Mass";
           // ct_EE_peakList.Series["eePeakList"].YValueMembers = "Running Sum";
           // ct_EE_peakList.DataSource = eePairsList;
           // ct_EE_peakList.DataBind();
           // ct_EE_peakList.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";

           // ct_EE_peakList.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(dgv_EE_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) - Convert.ToDouble(nUD_PeakWidthBase.Value);
           // ct_EE_peakList.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(dgv_EE_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) + Convert.ToDouble(nUD_PeakWidthBase.Value);
           //// ct_EE_peakList.Series["eePeakList"].ToolTip = "#VALX{#.##}" + " , " + "#VALY{#.##}";
           // ct_EE_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
           // {
           //     BorderColor = Color.Red,
           //     IntervalOffset = Convert.ToDouble(dgv_EE_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) + 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
           // });

           // ct_EE_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
           // {
           //     BorderColor = Color.Red,
           //     IntervalOffset = Convert.ToDouble(dgv_EE_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) - 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
           // });
           // ct_EE_peakList.ChartAreas[0].AxisX.Title = "Delta m/z";
           // ct_EE_peakList.ChartAreas[0].AxisY.Title = "Peak Count";
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

        private void UpdateFiguresOfMerit()
        {
            //try
            //{
            //    int peakSum = 0;
            //    int peakCount = 0;
            //    DataRow[] bigPeaks = eePeakList.Select("[Acceptable] = true");
            //    foreach (DataRow row in bigPeaks)
            //    {
            //        peakSum = peakSum + Convert.ToInt32(row["Peak Count"]);
            //        peakCount++;
            //    }

            //    tb_TotalPeaks.Text = peakCount.ToString();
            //    tb_IdentifiedProteoforms.Text = peakSum.ToString();
            //}
            //catch
            //{
            //    MessageBox.Show("catch in update figures of merit");
            //}

        }

        private void FillEEPeakListTable()
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = Lollipop.ee_peaks;
            dgv_EE_Peak_List.DataSource = bs;
            dgv_EE_Peak_List.Columns["Average Delta Mass"].ReadOnly = true;
            dgv_EE_Peak_List.Columns["Peak Count"].ReadOnly = true;
            dgv_EE_Peak_List.Columns["Average Delta Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_EE_Peak_List.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_EE_Peak_List.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
            dgv_EE_Peak_List.EndEdit();
            dgv_EE_Peak_List.Refresh();

        }

        private void GraphEEHistogram()
        {
            //string colName = "Delta Mass";
            //string direction = "DESC";

            //DataTable dt = eePairsList;
            //dt.DefaultView.Sort = colName + " " + direction;
            //dt = dt.DefaultView.ToTable();
            //eePairsList = dt;

            //ct_EE_Histogram.Series["eeHistogram"].XValueMember = "Delta Mass";
            //ct_EE_Histogram.Series["eeHistogram"].YValueMembers = "Running Sum";
            //ct_EE_Histogram.DataSource = eePairsList;
            //ct_EE_Histogram.DataBind();
            //ct_EE_Histogram.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";
            //ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Add(
            //    new StripLine()
            //    {
            //        BorderColor = Color.Red,
            //        IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value),
            //    });

            //// ct_EE_Histogram.Series["eeHistogram"].ToolTip = "#VALX{#.##}" + " , " + "#VALY{#.##}";
            //ct_EE_Histogram.ChartAreas[0].AxisX.Title = "Delta m/z";
            //ct_EE_Histogram.ChartAreas[0].AxisY.Title = "Peak Count";
        }

        private void FillEEPairsGridView()
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = Lollipop.ee_relations;
            dgv_EE_Pairs.DataSource = bs;
            dgv_EE_Pairs.ReadOnly = true;
            dgv_EE_Pairs.Columns["Acceptable Peak"].ReadOnly = false;
            dgv_EE_Pairs.Columns["Aggregated Mass Light"].DefaultCellStyle.Format = "0.#####";
            dgv_EE_Pairs.Columns["Aggregated Mass Heavy"].DefaultCellStyle.Format = "0.#####";
            dgv_EE_Pairs.Columns["Retention Time Light"].DefaultCellStyle.Format = "0.##";
            dgv_EE_Pairs.Columns["Retention Time Heavy"].DefaultCellStyle.Format = "0.##";
            dgv_EE_Pairs.Columns["Delta Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_EE_Pairs.Columns["Peak Center Mass"].DefaultCellStyle.Format = "0.#####";
            dgv_EE_Pairs.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_EE_Pairs.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
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
            nUD_NoManLower.Value = Convert.ToDecimal(Lollipop.no_mans_land_lowerBound);

            nUD_NoManUpper.Minimum = 0.50m;
            nUD_NoManUpper.Maximum = 1.00m;
            nUD_NoManUpper.Value = Convert.ToDecimal(Lollipop.no_mans_land_upperBound);

            nUD_PeakWidthBase.Minimum = 0.001m;
            nUD_PeakWidthBase.Maximum = 0.5000m;
            nUD_PeakWidthBase.Value = Convert.ToDecimal(Lollipop.peak_width_base);

            nUD_PeakCountMinThreshold.Minimum = 0;
            nUD_PeakCountMinThreshold.Maximum = 1000;
            nUD_PeakCountMinThreshold.Value = Convert.ToDecimal(Lollipop.min_peak_count);
        }

        private void propagatePeakListAcceptedPeakChangeToPairsTable(object sender, DataGridViewCellEventArgs e)
        {

            //double averageDeltaMass = Convert.ToDouble(dgv_EE_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex - 2].Value);
            //int peakCount = Convert.ToInt32(dgv_EE_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex - 1].Value);
            //dgv_EE_Peak_List.EndEdit();
            //dgv_EE_Peak_List.Update();

            //double lowMass = averageDeltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
            //double highMass = averageDeltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;

            //string expression = "[Average Delta Mass] > " + lowMass + " and [Average Delta Mass] < " + highMass;

            //DataRow[] selectedPeaks = eePeakList.Select(expression);

            //foreach (DataRow row in selectedPeaks)
            //{
            //    row["Acceptable"] = Convert.ToBoolean(dgv_EE_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
            //}
            //eePeakList.AcceptChanges();
            //Lollipop.etPeakList = eePeakList;
            //dgv_EE_Peak_List.Update();
            //dgv_EE_Peak_List.Refresh();

            //expression = "[Peak Center Mass] > " + lowMass + " and [Peak Center Mass] < " + highMass;

            //selectedPeaks = eePairsList.Select(expression);

            //foreach (DataRow row in selectedPeaks)
            //{

            //    row["Proteoform Family"] = Convert.ToBoolean(dgv_EE_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);

            //}
            //eePairsList.AcceptChanges();

            //Lollipop.experimentExperimentPairs = eePairsList;
            //dgv_EE_Pairs.DataSource = null;
            //FillEEPairsGridView();

            //UpdateFiguresOfMerit();

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
            if (true)
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
            if (true)
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
            if (true)
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
            if (true)
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
            if (true)
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

        Point? ct_EE_Histogram_prevPosition = null;
        ToolTip ct_EE_Histogram_tt = new ToolTip();

        private void ct_EE_Histogram_MouseMove(object sender, MouseEventArgs e)
        {
            tooltip_graph_display(ct_EE_peakList_tt, e, ct_EE_Histogram, ct_EE_Histogram_prevPosition);
        }

        Point? ct_EE_peakList_prevPosition = null;
        ToolTip ct_EE_peakList_tt = new ToolTip();

        private void ct_EE_peakList_MouseMove(object sender, MouseEventArgs e)
        {
            tooltip_graph_display(ct_EE_peakList_tt, e, ct_EE_peakList, ct_EE_peakList_prevPosition);
        }

        private void tooltip_graph_display(ToolTip t, MouseEventArgs e, Chart c, Point? p)
        {
            var pos = e.Location;
            if (p.HasValue && pos == p.Value) return;
            t.RemoveAll();
            p = pos;
            var results = c.HitTest(pos.X, pos.Y, false, ChartElementType.DataPoint);
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
                            t.Show("X=" + prop.XValue + ", Y=" + prop.YValues[0], c, pos.X, pos.Y - 15);
                        }
                    }
                }
            }
        }

        private void EE_update_Click(object sender, EventArgs e)
        {
            RunTheGamut();
        }
    }
}