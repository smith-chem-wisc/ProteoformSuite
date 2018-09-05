using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
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

        #region Public Property

        public List<DataTable> DataTables { get; private set; }

        #endregion Public Property

        #region Public Constructors

        public ExperimentExperimentComparison()
        {
            this.relationUtility = new RelationUtility();
            InitializeComponent();
            this.AutoScroll = true;
            this.AutoScrollMinSize = this.ClientSize;
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
            return Sweet.lollipop.target_proteoform_community.has_e_and_t_proteoforms; // Need the ptm dictionary for peak assignment from theoretical database
        }

        public void RunTheGamut(bool full_run)
        {
            ClearListsTablesFigures(true);
            Sweet.lollipop.ee_relations = Sweet.lollipop.target_proteoform_community.relate(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, Sweet.lollipop.target_proteoform_community.experimental_proteoforms, ProteoformComparison.ExperimentalExperimental, true, Environment.CurrentDirectory, true);
            Sweet.lollipop.relate_ef();
            Sweet.lollipop.ee_peaks = Sweet.lollipop.target_proteoform_community.accept_deltaMass_peaks(Sweet.lollipop.ee_relations, Sweet.lollipop.ef_relations);
            FillTablesAndCharts();
        }

        public List<DataTable> SetTables()
        {
            DataTables = new List<DataTable>
            {
                DisplayProteoformRelation.FormatRelationsGridView(Sweet.lollipop.ee_relations.OfType<ProteoformRelation>().Select(r => new DisplayProteoformRelation(r)).ToList(), "EERelations", false, true, false),
                DisplayDeltaMassPeak.FormatPeakListGridView(Sweet.lollipop.ee_peaks.Select(p => new DisplayDeltaMassPeak(p)).ToList(), "EEPeaks", true)
            };
            return DataTables;
        }

        public void ClearListsTablesFigures(bool clear_following)
        {
            //clear all save acceptance actions --> will re-add save actions from loaded actions if peak still exists
            Sweet.save_actions.RemoveAll(x => x.StartsWith("accept ExperimentalExperimental") || x.StartsWith("unaccept ExperimentalExperimental"));

            Sweet.lollipop.clear_ee();
            foreach (var series in ct_EE_Histogram.Series) series.Points.Clear();
            foreach (var series in ct_EE_peakList.Series) series.Points.Clear();

            dgv_EE_Relations.DataSource = null;
            dgv_EE_Peaks.DataSource = null;
            dgv_EE_Relations.Rows.Clear();
            dgv_EE_Peaks.Rows.Clear();
            tb_max_accepted_fdr.Clear();
            tb_peakTableFilter.Clear();
            tb_relationTableFilter.Clear();
            tb_totalAcceptedEERelations.Clear();
            tb_TotalEEPeaks.Clear();

            if (clear_following)
            {
                for (int i = ((ProteoformSweet)MdiParent).forms.IndexOf(this) + 1; i < ((ProteoformSweet)MdiParent).forms.Count; i++)
                {
                    ISweetForm sweet = ((ProteoformSweet)MdiParent).forms[i];
                    if (sweet as TopDown == null)
                    {
                        sweet.ClearListsTablesFigures(false);
                    }
                }
            }
        }

        public void FillTablesAndCharts()
        {
            dgv_EE_Peaks.CurrentCellDirtyStateChanged -= EE_Peak_List_DirtyStateChanged;//remove event handler on form load and table refresh event
            DisplayUtility.FillDataGridView(dgv_EE_Peaks, Sweet.lollipop.ee_peaks.OrderByDescending(p => p.peak_relation_group_count).Select(p => new DisplayDeltaMassPeak(p)));
            DisplayUtility.FillDataGridView(dgv_EE_Relations, Sweet.lollipop.ee_relations.Select(r => new DisplayProteoformRelation(r)));
            DisplayProteoformRelation.FormatRelationsGridView(dgv_EE_Relations, false, true, false);
            DisplayDeltaMassPeak.FormatPeakListGridView(dgv_EE_Peaks, true);
            GraphEERelations();
            GraphEEPeaks();
            if (cb_Graph_lowerThreshold.Checked) ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Add(new StripLine() { BorderColor = Color.Red, IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value) });
            else ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            update_figures_of_merit();
            dgv_EE_Peaks.CurrentCellDirtyStateChanged += EE_Peak_List_DirtyStateChanged;//re-instate event handler after form load and table refresh event
        }

        public void InitializeParameterSet()
        {
            //MASS WINDOW
            nUD_EE_Upper_Bound.Minimum = 0;
            nUD_EE_Upper_Bound.Maximum = 2000;
            xMaxEE.Maximum = nUD_EE_Upper_Bound.Maximum;
            xMinEE.Maximum = nUD_EE_Upper_Bound.Maximum;
            nUD_EE_Upper_Bound.Value = (decimal)Sweet.lollipop.ee_max_mass_difference; // maximum mass difference in Da allowed between experimental pair

            //Other stuff
            yMaxEE.Minimum = 0;
            yMaxEE.Maximum = 1000;
            yMaxEE.Value = 100; // scaling for y-axis maximum in the histogram of all EE pairs

            yMinEE.Minimum = -100;
            yMinEE.Maximum = yMaxEE.Maximum;
            yMinEE.Value = 0; // scaling for y-axis minimum in the histogram of all EE pairs

            xMaxEE.Minimum = xMinEE.Value;
            xMaxEE.Value = (decimal)Sweet.lollipop.ee_max_mass_difference; // scaling for x-axis maximum in the histogram of all EE pairs

            xMinEE.Minimum = -100;
            xMinEE.Value = 0; // scaling for x-axis minimum in the histogram of all EE pairs

            nUD_PeakWidthBase.Minimum = 0.001m;
            nUD_PeakWidthBase.Maximum = 0.5000m;
            nUD_PeakWidthBase.Value = Convert.ToDecimal(Sweet.lollipop.peak_width_base_ee);

            nUD_PeakCountMinThreshold.ValueChanged -= nUD_PeakCountMinThreshold_ValueChanged;
            nUD_PeakCountMinThreshold.Minimum = 0;
            nUD_PeakCountMinThreshold.Maximum = 1000;
            nUD_PeakCountMinThreshold.Value = Convert.ToDecimal(Sweet.lollipop.min_peak_count_ee);
            nUD_PeakCountMinThreshold.ValueChanged += nUD_PeakCountMinThreshold_ValueChanged;

            nUD_MaxRetTimeDifference.Minimum = 0;
            nUD_MaxRetTimeDifference.Maximum = 60;
            nUD_MaxRetTimeDifference.Value = Convert.ToDecimal(Sweet.lollipop.ee_max_RetentionTime_difference);

            tb_peakTableFilter.TextChanged -= tb_peakTableFilter_TextChanged;
            tb_peakTableFilter.Text = "";
            tb_peakTableFilter.TextChanged += tb_peakTableFilter_TextChanged;

            tb_relationTableFilter.TextChanged -= tb_relationTableFilter_TextChanged;
            tb_relationTableFilter.Text = "";
            tb_relationTableFilter.TextChanged += tb_relationTableFilter_TextChanged;

            cb_ee_peak_accept_rank.Checked = Sweet.lollipop.ee_accept_peaks_based_on_rank;
        }

        #endregion Public Methods

        #region Other Private Methods

        private void GraphEEPeaks()
        {
            DisplayUtility.GraphDeltaMassPeaks(ct_EE_peakList, Sweet.lollipop.ee_peaks, "Peak Count", "Decoy Count", Sweet.lollipop.ee_relations, "Nearby Relations");
        }

        private void bt_compare_EE_Click(object sender, EventArgs e)
        {
            if (ReadyToRunTheGamut())
            {
                Cursor = Cursors.WaitCursor;
                RunTheGamut(false);
                xMaxEE.Value = Convert.ToDecimal(Sweet.lollipop.ee_max_mass_difference);
                Cursor = Cursors.Default;
            }
            else if (Sweet.lollipop.target_proteoform_community.has_e_proteoforms)
                MessageBox.Show("Go back and create the theoretical database.");
            else
                MessageBox.Show("Go back and aggregate experimental proteoforms.");
        }

        private void update_figures_of_merit()
        {
            relationUtility.updateFiguresOfMerit(Sweet.lollipop.ee_peaks, tb_totalAcceptedEERelations, tb_TotalEEPeaks, tb_max_accepted_fdr);
        }

        #endregion Other Private Methods

        #region EE Peak List Private Methods

        private void tb_peakTableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_peaks = tb_peakTableFilter.Text == "" ?
                Sweet.lollipop.ee_peaks :
                ExtensionMethods.filter(Sweet.lollipop.ee_peaks, tb_peakTableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_EE_Peaks, selected_peaks.OfType<DeltaMassPeak>().Select(p => new DisplayDeltaMassPeak(p)));
            DisplayDeltaMassPeak.FormatPeakListGridView(dgv_EE_Peaks, true);
        }

        private void dgv_EE_Peak_List_CellClick(object sender, MouseEventArgs e)
        {
            int clickedRow = dgv_EE_Peaks.HitTest(e.X, e.Y).RowIndex;
            int clickedCol = dgv_EE_Peaks.HitTest(e.X, e.Y).ColumnIndex;
            if (e.Button == MouseButtons.Left && clickedRow >= 0 && clickedRow < Sweet.lollipop.ee_relations.Count
                && clickedCol < dgv_EE_Peaks.ColumnCount && clickedCol >= 0)
            {
                ct_EE_peakList.ChartAreas[0].AxisX.StripLines.Clear();
                DeltaMassPeak selected_peak = (DeltaMassPeak)(dgv_EE_Peaks.Rows[clickedRow].DataBoundItem as DisplayObject).display_object;
                DisplayUtility.GraphSelectedDeltaMassPeak(ct_EE_peakList, selected_peak, Sweet.lollipop.ee_relations);
            }
        }

        private void EE_Peak_List_DirtyStateChanged(object sender, EventArgs e)
        {
            dgv_EE_Peaks.Refresh();
            dgv_EE_Relations.Refresh();
            update_figures_of_merit();
            (MdiParent as ProteoformSweet).proteoformFamilies.ClearListsTablesFigures(true);
        }

        #endregion EE Peak List Private Methods

        #region Histogram Private Methods

        private void tb_relationTableFilter_TextChanged(object sender, EventArgs e)
        {
            List<DisplayProteoformRelation> display_relations = Sweet.lollipop.ee_relations.Select(p => new DisplayProteoformRelation(p)).ToList();
            IEnumerable<object> selected_relations = tb_relationTableFilter.Text == "" ?
                display_relations :
                ExtensionMethods.filter(display_relations, tb_relationTableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_EE_Relations, selected_relations.OfType<DisplayProteoformRelation>());
            DisplayProteoformRelation.FormatRelationsGridView(dgv_EE_Relations, false, true, false);
        }

        private void GraphEERelations()
        {
            DisplayUtility.GraphRelationsChart(ct_EE_Histogram, Sweet.lollipop.ee_relations, "relations", false);
            ct_EE_Histogram.Series["relations"].Enabled = true;
            if (Sweet.lollipop.ef_relations.Count > 0)
            {
                DisplayUtility.GraphRelationsChart(ct_EE_Histogram, Sweet.lollipop.ef_relations[Sweet.lollipop.decoy_community_name_prefix + "0"], "decoys", false);
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
            Sweet.lollipop.ee_max_mass_difference = Convert.ToDouble(nUD_EE_Upper_Bound.Value);
        }

        private void nUD_PeakWidthBase_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.peak_width_base_ee = Convert.ToDouble(nUD_PeakWidthBase.Value);
        }

        private void nUD_PeakCountMinThreshold_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_peak_count_ee = Convert.ToDouble(nUD_PeakCountMinThreshold.Value);
            change_peak_acceptance();
        }

        private void change_peak_acceptance()
        {
            Parallel.ForEach(Sweet.lollipop.ee_peaks, p =>
            {
                p.Accepted = p.peak_relation_group_count >= Sweet.lollipop.min_peak_count_ee && (!Sweet.lollipop.ee_accept_peaks_based_on_rank || (p.possiblePeakAssignments.Count > 0 && p.possiblePeakAssignments.Any(a => a.ptm_rank_sum < Sweet.lollipop.mod_rank_first_quartile)));
                Parallel.ForEach(p.grouped_relations, r => r.Accepted = p.Accepted);
            });
            Parallel.ForEach(Sweet.lollipop.ef_relations.Values.SelectMany(v => v).Where(r => r.peak != null), pRelation => pRelation.Accepted = pRelation.peak.Accepted);
            dgv_EE_Peaks.Refresh();
            dgv_EE_Relations.Refresh();
            ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            StripLine lowerCountBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = Sweet.lollipop.min_peak_count_ee };
            ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Add(lowerCountBound_stripline);
            update_figures_of_merit();
            (MdiParent as ProteoformSweet).proteoformFamilies.ClearListsTablesFigures(true);
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
            else
                ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
        }

        #endregion Histogram Private Methods

        #region Parameters Private Methods

        private void nUD_MaxRetTimeDifference_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.ee_max_RetentionTime_difference = Convert.ToDouble(nUD_MaxRetTimeDifference.Value);
        }

        #endregion Parameters Private Methods

        #region Tooltip Private Methods

        private Point? ct_EE_Histogram_prevPosition = null;
        private Point? ct_EE_peakList_prevPosition = null;

        private ToolTip ct_EE_Histogram_tt = new ToolTip();
        private ToolTip ct_EE_peakList_tt = new ToolTip();

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

        private void cb_ee_peak_accept_rank_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.ee_accept_peaks_based_on_rank = cb_ee_peak_accept_rank.Checked;
            change_peak_acceptance();
        }
    }
}