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
using ProteoformSuiteInternal;

namespace ProteoformSuite
{ 
    public partial class ExperimentExperimentComparison : Form
    {
        private bool compared_ee = false;

        public ExperimentExperimentComparison()
        {
            InitializeComponent();
            this.dgv_EE_Peaks.MouseClick += new MouseEventHandler(dgv_EE_Peak_List_CellClick);
            this.ct_EE_Histogram.MouseClick += new MouseEventHandler(ct_EE_Histogram_MouseClick);
            this.ct_EE_peakList.MouseClick += new MouseEventHandler(ct_EE_peakList_MouseClick);
            dgv_EE_Peaks.CurrentCellDirtyStateChanged += new EventHandler(peakListSpecificPeakAcceptanceChanged); //makes the change immediate and automatic
            dgv_EE_Peaks.CellValueChanged += new DataGridViewCellEventHandler(propagatePeakListAcceptedPeakChangeToPairsTable); //when 'acceptance' of an ET peak gets changed, we change the ET pairs table.
            InitializeParameterSet();
            InitializeMassWindow();
        }

        public void ExperimentExperimentComparison_Load(object sender, EventArgs e)
        { }

        public void compare_ee()
        {
            if (Lollipop.proteoform_community.has_e_proteoforms)
            {
                this.Cursor = Cursors.WaitCursor;
                ClearListsAndTables();
                Lollipop.make_ee_relationships();
                this.FillTablesAndCharts();
                this.Cursor = Cursors.Default;
                compared_ee = true;
            }
            else MessageBox.Show("Go back and aggregate experimental proteoforms.");
        }

        public DataGridView GetEERelationDGV()
        {
            return dgv_EE_Relations;
        }

        public DataGridView GetEEPeaksDGV()
        {
            return dgv_EE_Peaks;
        }

        private void ClearListsAndTables()
        {
            Lollipop.ee_relations.Clear();
            Lollipop.ee_peaks.Clear();
            Lollipop.ef_relations.Clear();
            Lollipop.proteoform_community.families.Clear();
            foreach (Proteoform p in Lollipop.proteoform_community.experimental_proteoforms) p.relationships.RemoveAll(r => r.relation_type == ProteoformComparison.ee || r.relation_type == ProteoformComparison.ef);
            foreach (Proteoform p in Lollipop.proteoform_community.theoretical_proteoforms) p.relationships.RemoveAll(r => r.relation_type == ProteoformComparison.ee || r.relation_type == ProteoformComparison.ef);
            Lollipop.proteoform_community.relations_in_peaks.RemoveAll(r => r.relation_type == ProteoformComparison.ee || r.relation_type == ProteoformComparison.ef);
            Lollipop.proteoform_community.delta_mass_peaks.RemoveAll(k => k.relation_type == ProteoformComparison.ee || k.relation_type == ProteoformComparison.ef);

            dgv_EE_Relations.DataSource = null;
            dgv_EE_Peaks.DataSource = null;
            dgv_EE_Relations.Rows.Clear();
            dgv_EE_Peaks.Rows.Clear();
        }

        public void FillTablesAndCharts()
        {
            FillEEPeakListTable();
            FillEEPairsGridView();
            DisplayUtility.FormatRelationsGridView(dgv_EE_Relations, false, true);
            DisplayUtility.FormatPeakListGridView(dgv_EE_Peaks, true);
            GraphEERelations();
            GraphEEPeaks();
            updateFiguresOfMerit();
        }

        private void updateFiguresOfMerit()
        {
            List<DeltaMassPeak> big_peaks = Lollipop.ee_peaks.Where(p => p.peak_accepted).ToList();
            tb_IdentifiedProteoforms.Text = big_peaks.Select(p => p.grouped_relations.Count).Sum().ToString();
            tb_TotalPeaks.Text = big_peaks.Count.ToString();
        }

        private void FillEEPairsGridView()
        {
            DisplayUtility.FillDataGridView(dgv_EE_Relations, Lollipop.ee_relations);
        }
        private void FillEEPeakListTable()
        {
            DisplayUtility.FillDataGridView(dgv_EE_Peaks, Lollipop.ee_peaks.OrderByDescending(p => p.peak_relation_group_count).ToList());
        }
        private void GraphEERelations()
        {
            DisplayUtility.GraphRelationsChart(ct_EE_Histogram, Lollipop.ee_relations, "relations");
        }
        private void GraphEEPeaks()
        {
            DisplayUtility.GraphDeltaMassPeaks(ct_EE_peakList, Lollipop.ee_peaks, "Peak Count", "Decoy Count", Lollipop.ee_relations, "Nearby Relations");
        }

        private void dgv_EE_Peak_List_CellClick(object sender, MouseEventArgs e)
        {
            int clickedRow = dgv_EE_Peaks.HitTest(e.X, e.Y).RowIndex;
            int clickedCol = dgv_EE_Peaks.HitTest(e.X, e.Y).ColumnIndex;
            if (e.Button == MouseButtons.Left && clickedRow >= 0 && clickedRow < Lollipop.ee_relations.Count 
                && clickedCol < dgv_EE_Peaks.ColumnCount && clickedCol >= 0)
            {
                ct_EE_peakList.ChartAreas[0].AxisX.StripLines.Clear();
                DeltaMassPeak selected_peak = (DeltaMassPeak)this.dgv_EE_Peaks.Rows[clickedRow].DataBoundItem;
                DisplayUtility.GraphSelectedDeltaMassPeak(ct_EE_peakList, selected_peak, Lollipop.ee_relations);
            }
        }

        private void InitializeMassWindow()
        {
            nUD_EE_Upper_Bound.Minimum = 0;
            nUD_EE_Upper_Bound.Maximum = 500;
            if (!Lollipop.neucode_labeled) Lollipop.ee_max_mass_difference = 150;
            else Lollipop.ee_max_mass_difference = 250;
            nUD_EE_Upper_Bound.Value = (decimal)Lollipop.ee_max_mass_difference; // maximum mass difference in Da allowed between experimental pair
        }

        private void InitializeParameterSet()
        {
            yMaxEE.Minimum = 0;
            yMaxEE.Maximum = 1000;
            yMaxEE.Value = 100; // scaling for y-axis maximum in the histogram of all EE pairs

            yMinEE.Minimum = -100;
            yMinEE.Maximum = yMaxEE.Maximum;
            yMinEE.Value = 0; // scaling for y-axis minimum in the histogram of all EE pairs

            xMaxEE.Minimum = xMinEE.Value;
            xMaxEE.Maximum = 500;
            xMaxEE.Value = (decimal)Lollipop.ee_max_mass_difference; // scaling for x-axis maximum in the histogram of all EE pairs

            xMinEE.Minimum = -100;
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
            nUD_PeakWidthBase.Value = Convert.ToDecimal(Lollipop.peak_width_base_ee);

            nUD_PeakCountMinThreshold.Minimum = 0;
            nUD_PeakCountMinThreshold.Maximum = 1000;
            nUD_PeakCountMinThreshold.Value = Convert.ToDecimal(Lollipop.min_peak_count_ee);

            nUD_MaxRetTimeDifference.Minimum = 0;
            nUD_MaxRetTimeDifference.Maximum = 60;
            nUD_MaxRetTimeDifference.Value = Convert.ToDecimal(Lollipop.ee_max_RetentionTime_difference);

        }

        private void propagatePeakListAcceptedPeakChangeToPairsTable(object sender, DataGridViewCellEventArgs e)
        {
            updateFiguresOfMerit();
        }


        private void peakListSpecificPeakAcceptanceChanged(object sender, EventArgs e)
        {
            if (dgv_EE_Peaks.IsCurrentCellDirty)
            {
                dgv_EE_Peaks.EndEdit();
                dgv_EE_Peaks.Update();
                dgv_EE_Relations.Refresh();
            }
        }

        private void xMaxEE_ValueChanged(object sender, EventArgs e) // scaling for x-axis maximum in the histogram of all EE pairs
        {
            ct_EE_Histogram.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(xMaxEE.Value);
        }
        private void yMaxEE_ValueChanged(object sender, EventArgs e) // scaling for y-axis maximum in the histogram of all EE pairs
        {
            ct_EE_Histogram.ChartAreas[0].AxisY.Maximum = Convert.ToDouble(yMaxEE.Value);
        }
        private void yMinEE_ValueChanged(object sender, EventArgs e) // scaling for y-axis minimum in the histogram of all EE pairs
        {
            ct_EE_Histogram.ChartAreas[0].AxisY.Minimum = Convert.ToDouble(yMinEE.Value);
        }
        private void xMinEE_ValueChanged(object sender, EventArgs e) // scaling for x-axis maximum in the histogram of all EE pairs
        {
            ct_EE_Histogram.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(xMinEE.Value);
        }
        private void cb_Graph_lowerThreshold_CheckedChanged(object sender, EventArgs e)
        {
                if (cb_Graph_lowerThreshold.Checked)
                    ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Add(new StripLine() { BorderColor = Color.Red, IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value) });
                else if (!cb_Graph_lowerThreshold.Checked) ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
        }

        Point? ct_EE_Histogram_prevPosition = null;
        Point? ct_EE_peakList_prevPosition = null;
        ToolTip ct_EE_Histogram_tt = new ToolTip();
        ToolTip ct_EE_peakList_tt = new ToolTip();
        private void ct_EE_Histogram_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
               DisplayUtility.tooltip_graph_display(ct_EE_Histogram_tt, e, ct_EE_Histogram, ct_EE_Histogram_prevPosition);
        }
        private void ct_EE_peakList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
              DisplayUtility.tooltip_graph_display(ct_EE_peakList_tt, e, ct_EE_peakList, ct_EE_peakList_prevPosition);
        }

        private void bt_compare_EE_Click(object sender, EventArgs e)
        {
            compare_ee();
            xMaxEE.Value = Convert.ToDecimal(Lollipop.ee_max_mass_difference);
        }

        private void nUD_EE_Upper_Bound_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.ee_max_mass_difference = Convert.ToDouble(nUD_EE_Upper_Bound.Value);
        }

        private void nUD_PeakWidthBase_ValueChanged(object sender, EventArgs e)
        {
             Lollipop.peak_width_base_ee = Convert.ToDouble(nUD_PeakWidthBase.Value);
        }

        private void nUD_PeakCountMinThreshold_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.min_peak_count_ee = Convert.ToDouble(nUD_PeakCountMinThreshold.Value);
            if (compared_ee)
            {
                Parallel.ForEach(Lollipop.ee_peaks, p =>
                p.peak_accepted = p.peak_relation_group_count >= Lollipop.min_peak_count_ee);
                dgv_EE_Peaks.Refresh();
                dgv_EE_Relations.Refresh();
            }
                ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
                StripLine lowerCountBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = Lollipop.min_peak_count_ee };
                ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Add(lowerCountBound_stripline);
        }

        private void nUD_NoManLower_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.no_mans_land_lowerBound = Convert.ToDouble(nUD_NoManLower.Value);
        }

        private void nUD_NoManUpper_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.no_mans_land_upperBound = Convert.ToDouble(nUD_NoManUpper.Value); 
        }

        private void nUD_MaxRetTimeDifference_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.ee_max_RetentionTime_difference = Convert.ToDouble(nUD_MaxRetTimeDifference.Value);
        }
    }
}