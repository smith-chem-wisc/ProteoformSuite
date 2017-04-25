using ProteoformSuiteInternal;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ProteoformSuiteGUI
{
    public partial class ExperimentExperimentComparison : Form, ISweetForm
    {

        #region Private Field

        private RelationUtility relationUtility;

        #endregion Private Field

        #region Public Constructors

        public ExperimentExperimentComparison()
        {
            this.relationUtility = new RelationUtility();
            InitializeComponent();
            this.dgv_EE_Peaks.MouseClick += new MouseEventHandler(dgv_EE_Peak_List_CellClick);
            this.ct_EE_Histogram.MouseClick += new MouseEventHandler(ct_EE_Histogram_MouseClick);
            this.ct_EE_peakList.MouseClick += new MouseEventHandler(ct_EE_peakList_MouseClick);
            dgv_EE_Peaks.CurrentCellDirtyStateChanged += new EventHandler(EE_Peak_List_DirtyStateChanged); //makes the change immediate and automatic
            InitializeParameterSet();
        }

        #endregion Public Constructors

        #region Public Methods

        public bool ReadyToRunTheGamut()
        {
            return SaveState.lollipop.ee_relations.Count == 0 && SaveState.lollipop.proteoform_community.has_e_proteoforms;
        }

        public void RunTheGamut()
        {
            Cursor = Cursors.WaitCursor;
            SaveState.lollipop.ee_relations = SaveState.lollipop.proteoform_community.relate(SaveState.lollipop.proteoform_community.experimental_proteoforms, SaveState.lollipop.proteoform_community.experimental_proteoforms, ProteoformComparison.ExperimentalExperimental, true);
            SaveState.lollipop.ef_relations = SaveState.lollipop.proteoform_community.relate_ef(SaveState.lollipop.proteoform_community.experimental_proteoforms, SaveState.lollipop.proteoform_community.experimental_proteoforms);
            SaveState.lollipop.ee_peaks = SaveState.lollipop.proteoform_community.accept_deltaMass_peaks(SaveState.lollipop.ee_relations, SaveState.lollipop.ef_relations);
            ((ProteoformSweet)MdiParent).proteoformFamilies.ClearListsTablesFigures();
            ((ProteoformSweet)MdiParent).quantification.ClearListsTablesFigures();

            Parallel.Invoke
            (
                () => FillTablesAndCharts(),
                () => { if (SaveState.lollipop.neucode_labeled) SaveState.lollipop.proteoform_community.construct_families(); }
            );

            if (SaveState.lollipop.neucode_labeled)
            {
                ((ProteoformSweet)MdiParent).proteoformFamilies.fill_proteoform_families("");
                ((ProteoformSweet)MdiParent).proteoformFamilies.update_figures_of_merit();
            }

            Cursor = Cursors.Default;
        }

        public DataGridView GetEERelationDGV()
        {
            return dgv_EE_Relations;
        }

        public DataGridView GetEEPeaksDGV()
        {
            return dgv_EE_Peaks;
        }

        public void ClearListsTablesFigures()
        {
            SaveState.lollipop.proteoform_community.clear_ee();

            foreach (var series in ct_EE_Histogram.Series) series.Points.Clear();
            foreach (var series in ct_EE_peakList.Series) series.Points.Clear();

            dgv_EE_Relations.DataSource = null;
            dgv_EE_Peaks.DataSource = null;
            dgv_EE_Relations.Rows.Clear();
            dgv_EE_Peaks.Rows.Clear();
        }

        public void FillTablesAndCharts()
        {
            dgv_EE_Peaks.CurrentCellDirtyStateChanged -= EE_Peak_List_DirtyStateChanged;//remove event handler on form load and table refresh event
            FillEEPeakListTable();
            FillEEPairsGridView();
            DisplayProteoformRelation.FormatRelationsGridView(dgv_EE_Relations, false, true);
            DisplayUtility.FormatPeakListGridView(dgv_EE_Peaks, true);
            GraphEERelations();
            GraphEEPeaks();
            update_figures_of_merit();
            dgv_EE_Peaks.CurrentCellDirtyStateChanged += EE_Peak_List_DirtyStateChanged;//re-instate event handler after form load and table refresh event 
        }

        public void InitializeParameterSet()
        {
            yMaxEE.Minimum = 0;
            yMaxEE.Maximum = 1000;
            yMaxEE.Value = 100; // scaling for y-axis maximum in the histogram of all EE pairs

            yMinEE.Minimum = -100;
            yMinEE.Maximum = yMaxEE.Maximum;
            yMinEE.Value = 0; // scaling for y-axis minimum in the histogram of all EE pairs

            xMaxEE.Minimum = xMinEE.Value;
            xMaxEE.Maximum = 500;
            xMaxEE.Value = (decimal)SaveState.lollipop.ee_max_mass_difference; // scaling for x-axis maximum in the histogram of all EE pairs

            xMinEE.Minimum = -100;
            xMinEE.Maximum = xMaxEE.Value;
            xMinEE.Value = 0; // scaling for x-axis minimum in the histogram of all EE pairs

            nUD_NoManLower.Minimum = 00m;
            nUD_NoManLower.Maximum = 0.49m;
            nUD_NoManLower.Value = Convert.ToDecimal(SaveState.lollipop.no_mans_land_lowerBound);

            nUD_NoManUpper.Minimum = 0.50m;
            nUD_NoManUpper.Maximum = 1.00m;
            nUD_NoManUpper.Value = Convert.ToDecimal(SaveState.lollipop.no_mans_land_upperBound);

            nUD_PeakWidthBase.Minimum = 0.001m;
            nUD_PeakWidthBase.Maximum = 0.5000m;
            nUD_PeakWidthBase.Value = Convert.ToDecimal(SaveState.lollipop.peak_width_base_ee);

            nUD_PeakCountMinThreshold.Minimum = 0;
            nUD_PeakCountMinThreshold.Maximum = 1000;
            nUD_PeakCountMinThreshold.Value = Convert.ToDecimal(SaveState.lollipop.min_peak_count_ee);

            nUD_MaxRetTimeDifference.Minimum = 0;
            nUD_MaxRetTimeDifference.Maximum = 60;
            nUD_MaxRetTimeDifference.Value = Convert.ToDecimal(SaveState.lollipop.ee_max_RetentionTime_difference);

            //MASS WINDOW
            nUD_EE_Upper_Bound.Minimum = 0;
            nUD_EE_Upper_Bound.Maximum = 500;
            if (!SaveState.lollipop.neucode_labeled) SaveState.lollipop.ee_max_mass_difference = 150;
            nUD_EE_Upper_Bound.Value = (decimal)SaveState.lollipop.ee_max_mass_difference; // maximum mass difference in Da allowed between experimental pair
        }

        #endregion Public Methods

        #region Other Private Methods

        private void GraphEEPeaks()
        {
            DisplayUtility.GraphDeltaMassPeaks(ct_EE_peakList, SaveState.lollipop.ee_peaks, "Peak Count", "Decoy Count", SaveState.lollipop.ee_relations, "Nearby Relations");
        }

        private void bt_compare_EE_Click(object sender, EventArgs e)
        {
            if (SaveState.lollipop.proteoform_community.has_e_proteoforms && SaveState.lollipop.theoretical_database.all_possible_ptmsets != null)
            {
                ClearListsTablesFigures();
                RunTheGamut();
                xMaxEE.Value = Convert.ToDecimal(SaveState.lollipop.ee_max_mass_difference);
            }
            else if (SaveState.lollipop.theoretical_database.all_possible_ptmsets == null) MessageBox.Show("Go back and load in a theoretical database.");
            else MessageBox.Show("Go back and aggregate experimental proteoforms.");
        }

        private void update_figures_of_merit()
        {
            relationUtility.updateFiguresOfMerit(SaveState.lollipop.ee_peaks, tb_totalAcceptedEERelations, tb_TotalEEPeaks, tb_max_accepted_fdr);
        }

        #endregion Other Private Methods

        #region EE Peak List Private Methods

        private void FillEEPeakListTable()
        {
            DisplayUtility.FillDataGridView(dgv_EE_Peaks, SaveState.lollipop.ee_peaks.OrderByDescending(p => p.peak_relation_group_count).ToList());
        }

        private void dgv_EE_Peak_List_CellClick(object sender, MouseEventArgs e)
        {
            int clickedRow = dgv_EE_Peaks.HitTest(e.X, e.Y).RowIndex;
            int clickedCol = dgv_EE_Peaks.HitTest(e.X, e.Y).ColumnIndex;
            if (e.Button == MouseButtons.Left && clickedRow >= 0 && clickedRow < SaveState.lollipop.ee_relations.Count
                && clickedCol < dgv_EE_Peaks.ColumnCount && clickedCol >= 0)
            {
                ct_EE_peakList.ChartAreas[0].AxisX.StripLines.Clear();
                DeltaMassPeak selected_peak = (DeltaMassPeak)this.dgv_EE_Peaks.Rows[clickedRow].DataBoundItem;
                DisplayUtility.GraphSelectedDeltaMassPeak(ct_EE_peakList, selected_peak, SaveState.lollipop.ee_relations);
            }
        }

        private void EE_Peak_List_DirtyStateChanged(object sender, EventArgs e)
        {
            relationUtility.peak_acceptability_change(dgv_EE_Peaks);
            dgv_EE_Peaks.Refresh();
            dgv_EE_Relations.Refresh();
            update_figures_of_merit();
        }

        #endregion EE Peak List Private Methods
        
        #region Histogram Private Methods

        private void FillEEPairsGridView()
        {
            DisplayUtility.FillDataGridView(dgv_EE_Relations, SaveState.lollipop.ee_relations.Select(r => new DisplayProteoformRelation(r)));
        }

        private void GraphEERelations()
        {
            DisplayUtility.GraphRelationsChart(ct_EE_Histogram, SaveState.lollipop.ee_relations, "relations");
            ct_EE_Histogram.Series["relations"].Enabled = true;
            if (SaveState.lollipop.ef_relations.Count > 0)
            {
                DisplayUtility.GraphRelationsChart(ct_EE_Histogram, SaveState.lollipop.ef_relations["EF_relations_0"], "decoys");
                ct_EE_Histogram.Series["decoys"].Enabled = false;
                cb_view_decoy_histogram.Enabled = true;
            }
            else cb_view_decoy_histogram.Enabled = false;
            cb_view_decoy_histogram.Checked = false;
        }


        private void cb_view_decoy_histogram_CheckedChanged(object sender, EventArgs e)
        {
            ct_EE_Histogram.Series["relations"].Enabled = !cb_view_decoy_histogram.Checked;
            ct_EE_Histogram.Series["decoys"].Enabled = cb_view_decoy_histogram.Checked;
        }

        private void nUD_EE_Upper_Bound_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.ee_max_mass_difference = Convert.ToDouble(nUD_EE_Upper_Bound.Value);
        }

        private void nUD_PeakWidthBase_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.peak_width_base_ee = Convert.ToDouble(nUD_PeakWidthBase.Value);
        }

        private void nUD_PeakCountMinThreshold_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.min_peak_count_ee = Convert.ToDouble(nUD_PeakCountMinThreshold.Value);
            Parallel.ForEach(SaveState.lollipop.ee_peaks, p =>
            {
                p.Accepted = p.peak_relation_group_count >= SaveState.lollipop.min_peak_count_ee;
                Parallel.ForEach(p.grouped_relations, r => r.Accepted = p.Accepted);
            });
            dgv_EE_Peaks.Refresh();
            dgv_EE_Relations.Refresh();
            ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            StripLine lowerCountBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = SaveState.lollipop.min_peak_count_ee };
            ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Add(lowerCountBound_stripline);
            update_figures_of_merit();
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

        #endregion Histogram Private Methods

        #region Parameters Private Methods

        private void nUD_NoManLower_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.no_mans_land_lowerBound = Convert.ToDouble(nUD_NoManLower.Value);
        }

        private void nUD_NoManUpper_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.no_mans_land_upperBound = Convert.ToDouble(nUD_NoManUpper.Value);
        }

        private void nUD_MaxRetTimeDifference_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.ee_max_RetentionTime_difference = Convert.ToDouble(nUD_MaxRetTimeDifference.Value);
        }

        #endregion Parameters Private Methods

        #region Tooltip Private Methods

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

        #endregion Tooltip Private Methods

    }
}