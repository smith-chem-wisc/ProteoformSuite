using ProteoformSuiteInternal;
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


namespace ProteoformSuite
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

        bool initial_load = true;
        public void ExperimentTheoreticalComparison_Load(object sender, EventArgs e)
        {
            Lollipop.make_et_relationships();
            InitializeParameterSet();
            initial_load = false;
            //GraphETRelations();
            //FillETPeakListTable();
            FillETRelationsGridView();
            //GraphETPeaks();
        }

        private void RunTheGamut()
        {
            this.Cursor = Cursors.WaitCursor;
            //GraphETRelations();
            //FillETPeakListTable();
            FillETRelationsGridView();
            //GraphETPeaks();
            UpdateFiguresOfMerit();
            this.Cursor = Cursors.Default;
        }

        private void FillETRelationsGridView()
        {
            DataGridViewDisplayUtility.FillDataGridView(dgv_ET_Pairs, Lollipop.et_relations);
        }

        private void FillETPeakListTable()
        {
            DataGridViewDisplayUtility.FillDataGridView(dgv_ET_Peak_List, Lollipop.et_peaks);
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

        private void GraphETRelations()
        {
            List<ProteoformRelation> et_relations_ordered = Lollipop.et_relations.OrderByDescending(r => r.group_adjusted_deltaM).ToList();
            ct_ET_Histogram.DataSource = et_relations_ordered;
            ct_ET_peakList.DataBind();
            ct_ET_Histogram.Series["etHistogram"].XValueMember = "group_adjusted_deltaM";
            ct_ET_Histogram.Series["etHistogram"].YValueMembers = "group_count";
            ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Clear();

            ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            StripLine lowerCountBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value) };
            ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Add(lowerCountBound_stripline);

            ct_ET_Histogram.ChartAreas[0].AxisX.Title = "Adjusted Delta m/z";
            ct_ET_Histogram.ChartAreas[0].AxisY.Title = "Nearby Count";
        }

        private void GraphETPeaks()
        {
            List<DeltaMassPeak> et_peaks_ordered = Lollipop.et_peaks.OrderByDescending(r => r.group_adjusted_deltaM).ToList();
            ct_ET_peakList.DataSource = et_peaks_ordered;
            ct_ET_peakList.DataBind();
            ct_ET_peakList.Series["etPeakList"].XValueMember = "group_adjusted_deltaM";
            ct_ET_peakList.Series["etPeakList"].YValueMembers = "group_count";
            ct_ET_peakList.ChartAreas[0].AxisX.LabelStyle.Format = "{0:0.00}";

            ct_ET_peakList.ChartAreas[0].AxisX.Minimum = et_peaks_ordered[0].group_adjusted_deltaM - Convert.ToDouble(nUD_PeakWidthBase.Value);
            ct_ET_peakList.ChartAreas[0].AxisX.Maximum = et_peaks_ordered[0].group_adjusted_deltaM + Convert.ToDouble(nUD_PeakWidthBase.Value);

            ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Clear();
            double stripline_tolerance = Convert.ToDouble(nUD_PeakWidthBase.Value) * 0.5;
            StripLine lowerPeakBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = et_peaks_ordered[0].group_adjusted_deltaM - stripline_tolerance };
            StripLine upperPeakBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = et_peaks_ordered[0].group_adjusted_deltaM + stripline_tolerance };
            ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Add(lowerPeakBound_stripline);
            ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Add(upperPeakBound_stripline);

            ct_ET_peakList.ChartAreas[0].AxisX.Title = "Adjusted Delta m/z";
            ct_ET_peakList.ChartAreas[0].AxisY.Title = "Peak Group Count";
        }

        private void dgv_ET_Peak_List_CellClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int clickedRow = dgv_ET_Peak_List.HitTest(e.X, e.Y).RowIndex;
                if (clickedRow >= 0 && clickedRow < Lollipop.et_relations.Count)
                {
                    ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Clear();
                    DeltaMassPeak selected_peak = (DeltaMassPeak)this.dgv_ET_Peak_List.Rows[clickedRow].DataBoundItem;
                    double graphMax = selected_peak.group_adjusted_deltaM + Convert.ToDouble((nUD_PeakWidthBase.Value));
                    double graphMin = selected_peak.group_adjusted_deltaM - Convert.ToDouble((nUD_PeakWidthBase.Value));
                    if (graphMin < graphMax)
                    {
                        ct_ET_peakList.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(graphMin);
                        ct_ET_peakList.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(graphMax);
                    }

                    ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Clear();
                    double stripline_tolerance = Convert.ToDouble(nUD_PeakWidthBase.Value) * 0.5;
                    StripLine lowerPeakBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = selected_peak.group_adjusted_deltaM - stripline_tolerance };
                    StripLine upperPeakBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = selected_peak.group_adjusted_deltaM + stripline_tolerance };
                    ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Add(lowerPeakBound_stripline);
                    ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Add(upperPeakBound_stripline);
                }
            }
        }

        Point? ct_ET_Histogram_prevPosition = null;
        ToolTip ct_ET_Histogram_tt = new ToolTip();

        private void ct_ET_Histogram_MouseMove(object sender, MouseEventArgs e)
        {
            DataGridViewDisplayUtility.tooltip_graph_display(ct_ET_peakList_tt, e, ct_ET_Histogram, ct_ET_Histogram_prevPosition);
        }

        Point? ct_ET_peakList_prevPosition = null;
        ToolTip ct_ET_peakList_tt = new ToolTip();

        private void ct_ET_peakList_MouseMove(object sender, MouseEventArgs e)
        {
            DataGridViewDisplayUtility.tooltip_graph_display(ct_ET_peakList_tt, e, ct_ET_peakList, ct_ET_peakList_prevPosition);
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

        private void splitContainer3_Panel2_Paint(object sender, PaintEventArgs e)
        { }

        private void nUD_ET_Lower_Bound_ValueChanged(object sender, EventArgs e) // maximum delta mass for theoretical proteoform that has mass LOWER than the experimental protoform mass
        {
            if (!initial_load) RunTheGamut();
        }

        private void nUD_ET_Upper_Bound_ValueChanged(object sender, EventArgs e) // maximum delta mass for theoretical proteoform that has mass HIGHER than the experimental protoform mass
        {
            if (!initial_load) RunTheGamut();
        }

        // scaling for axes of displayed ET Histogram of all ET pairs
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

        // bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts
        private void nUD_NoManLower_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.no_mans_land_lowerBound = Convert.ToDouble(nUD_NoManLower.Value);
            if (!initial_load) RunTheGamut();
        }
        private void nUD_NoManUpper_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.no_mans_land_upperBound = Convert.ToDouble(nUD_NoManUpper.Value);
            if (!initial_load) RunTheGamut();
        }

        // bin size used for including individual ET pairs in one 'Peak Center Mass' and peak with for one ET peak
        private void nUD_PeakWidthBase_ValueChanged(object sender, EventArgs e) 
        {
            Lollipop.peak_width_base = Convert.ToDouble(nUD_PeakWidthBase.Value);
            if (!initial_load) RunTheGamut();
        }

        // ET pairs with [Peak Center Count] AND ET peaks with [Peak Count] above this value are considered acceptable for use in proteoform family. this will be eventually set following ED analysis.
        private void nUD_PeakCountMinThreshold_ValueChanged(object sender, EventArgs e) 
        {
            Lollipop.min_peak_count = Convert.ToDouble(nUD_PeakCountMinThreshold.Value);
            GraphETRelations(); //we do this hear because the redline threshold needs to be redrawn
            UpdateFiguresOfMerit();
        }

        private void ET_Update_Click(object sender, EventArgs e)
        {
            RunTheGamut();
        }
    }
}
