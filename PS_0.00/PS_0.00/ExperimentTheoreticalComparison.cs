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
            GraphETHistogram();
            FillETPeakListTable();
            FillETPairsGridView();
            GraphETPairsList();
        }

        public void run_comparison()
        {
            InitializeParameterSet();
            UpdateFiguresOfMerit();
        }

        private void RunTheGamut()
        {
            this.Cursor = Cursors.WaitCursor;
            ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Clear();
            GraphETHistogram();
            FillETPeakListTable();
            FillETPairsGridView();
            GraphETPairsList();
            UpdateFiguresOfMerit();
            this.Cursor = Cursors.Default;
        }

        private void FillETPairsGridView()
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = Lollipop.et_relations;
            dgv_ET_Pairs.DataSource = bs;
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
           // string colName = "Delta Mass";
           // string direction = "DESC";
           // DataTable dt = Lollipop.experimentTheoreticalPairs;
           // dt.DefaultView.Sort = colName + " " + direction;
           // dt = dt.DefaultView.ToTable();
           // Lollipop.experimentTheoreticalPairs = dt;

           // ct_ET_peakList.Series["etPeakList"].XValueMember = "Delta Mass";
           // ct_ET_peakList.Series["etPeakList"].YValueMembers = "Running Sum";
           // ct_ET_peakList.DataSource = Lollipop.experimentTheoreticalPairs;
           // ct_ET_peakList.DataBind();
           // ct_ET_peakList.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";

           // ct_ET_peakList.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(dgv_ET_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) - Convert.ToDouble(nUD_PeakWidthBase.Value);
           // ct_ET_peakList.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(dgv_ET_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) + Convert.ToDouble(nUD_PeakWidthBase.Value);
           //// ct_ET_peakList.Series["etPeakList"].ToolTip = "#VALX{#.##}" + " , " + "#VALY{#.##}";
           // ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
           // {
           //     BorderColor = Color.Red,
           //     IntervalOffset = Convert.ToDouble(dgv_ET_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) + 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
           // });

           // ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Add(new StripLine()
           // {
           //     BorderColor = Color.Red,
           //     IntervalOffset = Convert.ToDouble(dgv_ET_Peak_List.Rows[0].Cells["Average Delta Mass"].Value.ToString()) - 0.5 * Convert.ToDouble((nUD_PeakWidthBase.Value)),
           // });
           // ct_ET_peakList.ChartAreas[0].AxisX.Title = "Delta m/z";
           // ct_ET_peakList.ChartAreas[0].AxisY.Title = "Peak Count";
        }

        private void dgv_ET_Peak_List_CellClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int clickedRow = dgv_ET_Peak_List.HitTest(e.X, e.Y).RowIndex;
                if (clickedRow >= 0 && clickedRow < Lollipop.et_relations.Count)
                    ETPeakListGraphParameters(clickedRow);
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
            //try
            //{
            //    int peakSum = 0;
            //    int peakCount = 0;
            //    DataRow[] bigPeaks = etPeaksList.Select("[Acceptable] = true");
            //    foreach (DataRow row in bigPeaks)
            //    {
            //        peakSum = peakSum + Convert.ToInt32(row["Peak Count"]);
            //        peakCount++;
            //    }
            //    tb_IdentifiedProteoforms.Text = peakSum.ToString();
            //    tb_TotalPeaks.Text = peakCount.ToString();
            //}
            //catch
            //{

            //}
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
            BindingSource bs = new BindingSource();
            bs.DataSource = Lollipop.et_peaks;
            dgv_ET_Peak_List.DataSource = bs;
            ////dgv_ET_Pairs.ReadOnly = true;
            //dgv_ET_Peak_List.Columns["Average Delta Mass"].ReadOnly = true;
            //dgv_ET_Peak_List.Columns["Peak Count"].ReadOnly = true;
            //dgv_ET_Peak_List.Columns["Average Delta Mass"].DefaultCellStyle.Format = "0.#####";
            //dgv_ET_Peak_List.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            //dgv_ET_Peak_List.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
            dgv_ET_Peak_List.EndEdit();
            dgv_ET_Peak_List.Refresh();

        }

        private void propagatePeakListAcceptedPeakChangeToPairsTable(object sender, DataGridViewCellEventArgs e)
        {
            //double averageDeltaMass = Convert.ToDouble(dgv_ET_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex - 2].Value);
            //int peakCount = Convert.ToInt32(dgv_ET_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex - 1].Value);
            //dgv_ET_Peak_List.EndEdit();
            //dgv_ET_Peak_List.Update();

            //double lowMass = averageDeltaMass - Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;
            //double highMass = averageDeltaMass + Convert.ToDouble(nUD_PeakWidthBase.Value) / 2;

            //string expression = "[Average Delta Mass] > " + lowMass + " and [Average Delta Mass] < " + highMass;
            //DataRow[] selectedPeaks = etPeaksList.Select(expression);

            //foreach (DataRow row in selectedPeaks)
            //{
            //    row["Acceptable"] = Convert.ToBoolean(dgv_ET_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
            //}
            //etPeaksList.AcceptChanges();
            //Lollipop.etPeakList = etPeaksList;
            //dgv_ET_Peak_List.Update();
            //dgv_ET_Peak_List.Refresh();

            //expression = "[Peak Center Mass] > " + lowMass + " and [Peak Center Mass] < " + highMass;

            //selectedPeaks = etPairsList.Select(expression);

            //foreach (DataRow row in selectedPeaks)
            //{
            //    row["Proteoform Family"] = Convert.ToBoolean(dgv_ET_Peak_List.Rows[e.RowIndex].Cells[e.ColumnIndex].Value);
            //}
            //etPairsList.AcceptChanges();

            //Lollipop.experimentTheoreticalPairs = etPairsList;
            //dgv_ET_Pairs.Update();
            //dgv_ET_Pairs.Refresh();

            //UpdateFiguresOfMerit();
        }

        private void peakListSpecificPeakAcceptanceChanged(object sender, EventArgs e)
        {
            if (dgv_ET_Peak_List.IsCurrentCellDirty)
            {                
                dgv_ET_Peak_List.EndEdit();
                dgv_ET_Peak_List.Update();
            }
        }

        private void GraphETHistogram()
        {
            //try
            //{
            //    string colName = "Delta Mass";
            //    string direction = "DESC";



            //    ct_ET_Histogram.Series["etHistogram"].XValueMember = Lollipop.et_relations.OrderByDescending(r => r.group_adjusted_deltaM).Select(r => r.group_adjusted_deltaM);
            //    ct_ET_Histogram.Series["etHistogram"].YValueMembers = "Running Sum";
            //    ct_ET_Histogram.DataBind();

            //    ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Clear();

            //    ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Add(new StripLine()
            //    {
            //        BorderColor = Color.Red,
            //        IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value),
            //    });


            //    //  ct_ET_Histogram.Series["etHistogram"].ToolTip = "#VALX{#.##}" + " , " + "#VALY{#.##}";
            //    ct_ET_Histogram.ChartAreas[0].AxisX.Title = "Delta m/z";
            //    ct_ET_Histogram.ChartAreas[0].AxisY.Title = "Peak Count";
            //}
            //catch
            //{

            //}
            
        }

        private void splitContainer3_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void nUD_ET_Lower_Bound_ValueChanged(object sender, EventArgs e) // maximum delta mass for theoretical proteoform that has mass LOWER than the experimental protoform mass
        {
            if (true) RunTheGamut();
        }

        private void nUD_ET_Upper_Bound_ValueChanged(object sender, EventArgs e) // maximum delta mass for theoretical proteoform that has mass HIGHER than the experimental protoform mass
        {
            if (true) RunTheGamut();
        }

        private void yMaxET_ValueChanged(object sender, EventArgs e) // scaling for y-axis of displayed ET Histogram of all ET pairs
        {
            if (true) ct_ET_Histogram.ChartAreas[0].AxisY.Maximum = double.Parse(yMaxET.Value.ToString());
        }

        private void yMinET_ValueChanged(object sender, EventArgs e) // scaling for y-axis of displayed ET Histogram of all ET pairs
        {
            if (true) ct_ET_Histogram.ChartAreas[0].AxisY.Minimum = double.Parse(yMinET.Value.ToString());
        }

        private void xMinET_ValueChanged(object sender, EventArgs e) // scaling for x-axis of displayed ET Histogram of all ET pairs
        {
            if (true) ct_ET_Histogram.ChartAreas[0].AxisX.Minimum = double.Parse(xMinET.Value.ToString());
        }

        private void xMaxET_ValueChanged(object sender, EventArgs e) // scaling for x-axis of displayed ET Histogram of all ET pairs
        {
            if (true) ct_ET_Histogram.ChartAreas[0].AxisX.Maximum = double.Parse(xMaxET.Value.ToString());
        }

        private void nUD_NoManLower_ValueChanged(object sender, EventArgs e) // lower bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts
        {
            Lollipop.no_mans_land_lowerBound = Convert.ToDouble(nUD_NoManLower.Value);
            if (true) RunTheGamut();
        }

        private void nUD_NoManUpper_ValueChanged(object sender, EventArgs e)// upper bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts
        {
            Lollipop.no_mans_land_upperBound = Convert.ToDouble(nUD_NoManUpper.Value);
            if (true) RunTheGamut();
        }

        private void nUD_PeakWidthBase_ValueChanged(object sender, EventArgs e) // bin size used for including individual ET pairs in one 'Peak Center Mass' and peak with for one ET peak
        {
            Lollipop.peak_width_base = Convert.ToDouble(nUD_PeakWidthBase.Value);
            if (true) RunTheGamut();
        }

        private void nUD_PeakCountMinThreshold_ValueChanged(object sender, EventArgs e) // ET pairs with [Peak Center Count] AND ET peaks with [Peak Count] above this value are considered acceptable for use in proteoform family. this will be eventually set following ED analysis.
        {
            Lollipop.min_peak_count = Convert.ToDouble(nUD_PeakCountMinThreshold.Value);
            if (true)
            {
                GraphETHistogram(); //we do this hear because the redline threshold needs to be redrawn
                UpdateFiguresOfMerit();
            }
        }

        Point? ct_ET_Histogram_prevPosition = null;
        ToolTip ct_ET_Histogram_tt = new ToolTip();

        private void ct_ET_Histogram_MouseMove(object sender, MouseEventArgs e)
        {
            tooltip_graph_display(ct_ET_peakList_tt, e, ct_ET_Histogram, ct_ET_Histogram_prevPosition);
        }

        Point? ct_ET_peakList_prevPosition = null;
        ToolTip ct_ET_peakList_tt = new ToolTip();

        private void ct_ET_peakList_MouseMove(object sender, MouseEventArgs e)
        {
            tooltip_graph_display(ct_ET_peakList_tt, e, ct_ET_peakList, ct_ET_peakList_prevPosition);
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
            nUD_NoManLower.Value = Convert.ToDecimal(Lollipop.no_mans_land_lowerBound); // lower bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts

            nUD_NoManUpper.Minimum = 0.50m;
            nUD_NoManUpper.Maximum = 1.00m;
            nUD_NoManUpper.Value = Convert.ToDecimal(Lollipop.no_mans_land_upperBound); // upper bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts

            nUD_PeakWidthBase.Minimum = 0.001m;
            nUD_PeakWidthBase.Maximum = 0.5000m;
            nUD_PeakWidthBase.Value = Convert.ToDecimal(Lollipop.peak_width_base); // bin size used for including individual ET pairs in one 'Peak Center Mass' and peak with for one ET peak

            nUD_PeakCountMinThreshold.Minimum = 0;
            nUD_PeakCountMinThreshold.Maximum = 1000;
            nUD_PeakCountMinThreshold.Value = Convert.ToDecimal(Lollipop.min_peak_count); // ET pairs with [Peak Center Count] AND ET peaks with [Peak Count] above this value are considered acceptable for use in proteoform family. this will be eventually set following ED analysis.
        }

        private void ET_Update_Click(object sender, EventArgs e)
        {
            RunTheGamut();
        }
    }
}
