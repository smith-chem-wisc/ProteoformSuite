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
    public partial class ExperimentTheoreticalComparison : Form, ISweetForm
    {

        #region Private Fields

        private RelationUtility relationUtility;
        private List<DisplayProteoformRelation> displayRelations = new List<DisplayProteoformRelation>();

        #endregion Private Fields

        #region Public Constructor

        public ExperimentTheoreticalComparison()
        {
            relationUtility = new RelationUtility();
            InitializeComponent();
            dgv_ET_Peak_List.MouseClick += new MouseEventHandler(dgv_ET_Peak_List_CellClick);
            ct_ET_Histogram.MouseClick += new MouseEventHandler(ct_ET_Histogram_MouseClick);
            ct_ET_peakList.MouseClick += new MouseEventHandler(ct_ET_peakList_MouseClick);
            dgv_ET_Peak_List.CurrentCellDirtyStateChanged += new EventHandler(ET_Peak_List_DirtyStateChanged); //makes the change immediate and automatic
            InitializeParameterSet();
        }

        #endregion Public Constructor

        #region Public Methods

        public bool ReadyToRunTheGamut()
        {
            return SaveState.lollipop.et_relations.Count == 0 && SaveState.lollipop.target_proteoform_community.has_e_and_t_proteoforms;
        }

        public void RunTheGamut()
        {
            Cursor = Cursors.WaitCursor;
            SaveState.lollipop.et_relations = SaveState.lollipop.target_proteoform_community.relate(SaveState.lollipop.target_proteoform_community.experimental_proteoforms, SaveState.lollipop.target_proteoform_community.theoretical_proteoforms, ProteoformComparison.ExperimentalTheoretical, true);
            SaveState.lollipop.relate_ed();
            SaveState.lollipop.et_peaks = SaveState.lollipop.target_proteoform_community.accept_deltaMass_peaks(SaveState.lollipop.et_relations, SaveState.lollipop.ed_relations);
            ((ProteoformSweet)MdiParent).proteoformFamilies.ClearListsTablesFigures();
            FillTablesAndCharts();
            Cursor = Cursors.Default;
        }

        public void FillTablesAndCharts()
        {
            dgv_ET_Peak_List.CurrentCellDirtyStateChanged -= ET_Peak_List_DirtyStateChanged;//remove event handler on form load and table refresh event
            FillETPeakListTable();
            FillETRelationsGridView();
            DisplayProteoformRelation.FormatRelationsGridView(dgv_ET_Relations, true, false);
            DisplayUtility.FormatPeakListGridView(dgv_ET_Peak_List, false);
            GraphETRelations();
            GraphETPeaks();
            update_figures_of_merit();
            dgv_ET_Peak_List.CurrentCellDirtyStateChanged += ET_Peak_List_DirtyStateChanged;//re-instate event handler after form load and table refresh event 
        }

        public List<DataGridView> GetDGVs()
        {
            return new List<DataGridView>() { dgv_ET_Relations, dgv_ET_Peak_List };
        }

        public void ClearListsTablesFigures()
        {
            SaveState.lollipop.clear_et();

            foreach (var series in ct_ET_Histogram.Series) series.Points.Clear();
            foreach (var series in ct_ET_peakList.Series) series.Points.Clear();
            dgv_ET_Relations.DataSource = null;
            dgv_ET_Peak_List.DataSource = null;
            dgv_ET_Relations.Rows.Clear();
            dgv_ET_Peak_List.Rows.Clear();
        }

        public void InitializeParameterSet()
        {
            yMaxET.Minimum = 0;
            yMaxET.Maximum = 1000;
            yMaxET.Value = 100; // scaling for y-axis of displayed ET Histogram of all ET pairs

            yMinET.Minimum = -100;
            yMinET.Maximum = yMaxET.Maximum;
            yMinET.Value = 0; // scaling for y-axis of displayed ET Histogram of all ET pairs

            xMaxET.Minimum = xMinET.Value;
            xMaxET.Maximum = 500;
            xMaxET.Value = nUD_ET_Upper_Bound.Value; // scaling for x-axis of displayed ET Histogram of all ET pairs

            xMinET.Minimum = -500;
            xMinET.Maximum = xMaxET.Value;
            xMinET.Value = nUD_ET_Lower_Bound.Value; // scaling for x-axis of displayed ET Histogram of all ET pairs

            nUD_NoManLower.Minimum = 00m;
            nUD_NoManLower.Maximum = 0.49m;
            nUD_NoManLower.Value = Convert.ToDecimal(SaveState.lollipop.no_mans_land_lowerBound); // lower bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts

            nUD_NoManUpper.Minimum = 0.50m;
            nUD_NoManUpper.Maximum = 1.00m;
            nUD_NoManUpper.Value = Convert.ToDecimal(SaveState.lollipop.no_mans_land_upperBound); // upper bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts

            nUD_PeakWidthBase.Minimum = 0.001m;
            nUD_PeakWidthBase.Maximum = 0.5000m;
            nUD_PeakWidthBase.Value = Convert.ToDecimal(SaveState.lollipop.peak_width_base_et); // bin size used for including individual ET pairs in one 'Peak Center Mass' and peak with for one ET peak

            nUD_PeakCountMinThreshold.Minimum = 0;
            nUD_PeakCountMinThreshold.Maximum = 1000;
            nUD_PeakCountMinThreshold.Value = Convert.ToDecimal(SaveState.lollipop.min_peak_count_et); // ET pairs with [Peak Center Count] AND ET peaks with [Peak Count] above this value are considered acceptable for use in proteoform family. this will be eventually set following ED analysis.

            tb_peakTableFilter.TextChanged -= tb_peakTableFilter_TextChanged;
            tb_peakTableFilter.Text = "";
            tb_peakTableFilter.TextChanged += tb_peakTableFilter_TextChanged;

            tb_relationTableFilter.TextChanged -= tb_relationTableFilter_TextChanged;
            tb_relationTableFilter.Text = "";
            tb_relationTableFilter.TextChanged += tb_relationTableFilter_TextChanged;

            //MASS WINDOW
            //only do this if ET hasn't already been run
            nUD_ET_Lower_Bound.Minimum = -500;
            nUD_ET_Lower_Bound.Maximum = 0;
            if (!SaveState.lollipop.neucode_labeled) SaveState.lollipop.et_low_mass_difference = -50;
            nUD_ET_Lower_Bound.Value = Convert.ToDecimal(SaveState.lollipop.et_low_mass_difference); // maximum delta mass for theoretical proteoform that has mass LOWER than the experimental protoform mass

            nUD_ET_Upper_Bound.Minimum = 0;
            nUD_ET_Upper_Bound.Maximum = 500;
            if (!SaveState.lollipop.neucode_labeled) SaveState.lollipop.et_high_mass_difference = 150;
            nUD_ET_Upper_Bound.Value = Convert.ToDecimal(SaveState.lollipop.et_high_mass_difference); // maximum delta mass for theoretical proteoform that has mass HIGHER than the experimental protoform mass
        }

        #endregion Public Methods

        #region Other Private Methods

        private void update_figures_of_merit()
        {
            relationUtility.updateFiguresOfMerit(SaveState.lollipop.et_peaks, tb_IdentifiedProteoforms, tb_TotalPeaks, tb_max_accepted_fdr);
        }

        private void bt_compare_et_Click(object sender, EventArgs e)
        {
            if (SaveState.lollipop.target_proteoform_community.has_e_and_t_proteoforms)
            {
                shift_masses();
                ClearListsTablesFigures();
                RunTheGamut();
                xMaxET.Value = (decimal)SaveState.lollipop.et_high_mass_difference;
                xMinET.Value = (decimal)SaveState.lollipop.et_low_mass_difference;
            }
            else if (SaveState.lollipop.target_proteoform_community.has_e_proteoforms) MessageBox.Show("Go back and create a theoretical database.");
            else MessageBox.Show("Go back and aggregate experimental proteoforms.");
        }

        //shifts any mass shifts that have been changed from 0 in dgv
        private void shift_masses()
        {
            List<DeltaMassPeak> peaks_to_shift = SaveState.lollipop.et_peaks.Where(p => p.mass_shifter != "0" && p.mass_shifter != "").ToList();
            if (peaks_to_shift.Count > 0)
            {
                foreach (DeltaMassPeak peak in peaks_to_shift)
                {
                    int int_mass_shifter = 0;
                    try
                    {
                        int_mass_shifter = Convert.ToInt32(peak.mass_shifter);
                    }
                    catch
                    {
                        MessageBox.Show("Could not convert mass shift for peak at delta mass " + peak.DeltaMass + ". Please enter an integer.");
                        return;
                    }
                    peak.shift_experimental_masses(int_mass_shifter, SaveState.lollipop.neucode_labeled);
                }
                ((ProteoformSweet)MdiParent).aggregatedProteoforms.ClearListsTablesFigures();
                ((ProteoformSweet)MdiParent).experimentExperimentComparison.ClearListsTablesFigures();
                ((ProteoformSweet)MdiParent).proteoformFamilies.ClearListsTablesFigures();
                ((ProteoformSweet)MdiParent).quantification.ClearListsTablesFigures();
                SaveState.lollipop.regroup_components(SaveState.lollipop.neucode_labeled, SaveState.lollipop.validate_proteoforms, SaveState.lollipop.input_files, SaveState.lollipop.raw_neucode_pairs, SaveState.lollipop.raw_experimental_components, SaveState.lollipop.raw_quantification_components, SaveState.lollipop.min_num_CS, SaveState.lollipop.min_relative_abundance);
            }
        }

        private void GraphETPeaks()
        {
            DisplayUtility.GraphDeltaMassPeaks(ct_ET_peakList, SaveState.lollipop.et_peaks, "Peak Count", "Median Decoy Count", SaveState.lollipop.et_relations, "Nearby Relations");
        }

        #endregion Other Private Methods

        #region ET Peak List Private Methods

        private void tb_peakTableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_peaks = tb_peakTableFilter.Text == "" ?
                SaveState.lollipop.et_peaks :
                ExtensionMethods.filter(SaveState.lollipop.et_peaks, tb_peakTableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_ET_Peak_List, selected_peaks.OfType<DeltaMassPeak>());
            DisplayUtility.FormatPeakListGridView(dgv_ET_Peak_List, false);
        }

        private void FillETPeakListTable()
        {
            DisplayUtility.FillDataGridView(dgv_ET_Peak_List, SaveState.lollipop.et_peaks.OrderByDescending(p => p.peak_relation_group_count).ToList());
        }

        private void dgv_ET_Peak_List_CellClick(object sender, MouseEventArgs e)
        {
            int clickedRow = dgv_ET_Peak_List.HitTest(e.X, e.Y).RowIndex;
            int clickedCol = dgv_ET_Peak_List.HitTest(e.X, e.Y).ColumnIndex;
            if (clickedRow < SaveState.lollipop.et_relations.Count && clickedRow >= 0 && clickedCol >= 0 && clickedCol < dgv_ET_Peak_List.ColumnCount)
            {
                if (e.Button == MouseButtons.Left)
                {
                    ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Clear();
                    DeltaMassPeak selected_peak = (DeltaMassPeak)dgv_ET_Peak_List.Rows[clickedRow].DataBoundItem;
                    DisplayUtility.GraphSelectedDeltaMassPeak(ct_ET_peakList, selected_peak, SaveState.lollipop.et_relations);
                }
                else
                {
                    if (e.Button == MouseButtons.Right && clickedRow >= 0 && clickedRow < SaveState.lollipop.et_relations.Count)
                    {
                        ContextMenuStrip ET_peak_List_Menu = new ContextMenuStrip();
                        int position_xy_mouse_row = dgv_ET_Peak_List.HitTest(e.X, e.Y).RowIndex;

                        DeltaMassPeak selected_peak = (DeltaMassPeak)this.dgv_ET_Peak_List.Rows[clickedRow].DataBoundItem;

                        if (position_xy_mouse_row > 0)
                        {
                            ET_peak_List_Menu.Items.Add("Increase Experimental Mass " + Lollipop.MONOISOTOPIC_UNIT_MASS + " Da").Name = "IncreaseMass";
                            ET_peak_List_Menu.Items.Add("Decrease Experimental Mass " + Lollipop.MONOISOTOPIC_UNIT_MASS + " Da").Name = "DecreaseMass";
                        }
                        ET_peak_List_Menu.Show(dgv_ET_Peak_List, new Point(e.X, e.Y));

                        //event menu click
                        ET_peak_List_Menu.ItemClicked += new ToolStripItemClickedEventHandler((s, ev) => ET_peak_List_Menu_ItemClicked(s, ev, selected_peak));
                    }
                }
            }
        }

        //will leave option to change one at a time by right clicking
        private void ET_peak_List_Menu_ItemClicked(object sender, ToolStripItemClickedEventArgs e, DeltaMassPeak peak)
        {
            int int_mass_shifter = 0;
            try
            {
                int_mass_shifter = Convert.ToInt32(peak.mass_shifter);
            }
            catch
            {
                MessageBox.Show("Oops, this mass shifter " + peak.mass_shifter + " is not an integer.");
                return;
            }

            switch (e.ClickedItem.Name.ToString())
            {
                case "IncreaseMass":
                    peak.mass_shifter = (int_mass_shifter + 1).ToString();
                    break;
                case "DecreaseMass":
                    peak.mass_shifter = (int_mass_shifter - 1).ToString();
                    break;
            }
            dgv_ET_Peak_List.Refresh();
        }

        #endregion ET Peak List Private Methods

        #region Histogram Private Methods

        private void tb_relationTableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_peaks = tb_relationTableFilter.Text == "" ?
                displayRelations :
                ExtensionMethods.filter(displayRelations.ToList(), tb_relationTableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_ET_Relations, selected_peaks);
            DisplayProteoformRelation.FormatRelationsGridView(dgv_ET_Relations, true, false);
        }

        private void FillETRelationsGridView()
        {
            displayRelations = SaveState.lollipop.et_relations.Select(r => new DisplayProteoformRelation(r)).ToList();
            DisplayUtility.FillDataGridView(dgv_ET_Relations, displayRelations);
        }

        private void GraphETRelations()
        {
            DisplayUtility.GraphRelationsChart(ct_ET_Histogram, SaveState.lollipop.et_relations, "relations");
            ct_ET_Histogram.Series["relations"].Enabled = true;
            if (SaveState.lollipop.ed_relations.Count > 0)
            {
                DisplayUtility.GraphRelationsChart(ct_ET_Histogram, SaveState.lollipop.ed_relations[SaveState.lollipop.decoy_community_name_prefix + "0"], "decoys");
                ct_ET_Histogram.Series["decoys"].Enabled = false;
                cb_view_decoy_histogram.Enabled = true;
            }
            else cb_view_decoy_histogram.Enabled = false;
            cb_view_decoy_histogram.Checked = false;

            DisplayUtility.GraphDeltaMassPeaks(ct_ET_peakList, SaveState.lollipop.et_peaks, "Peak Count", "Median Decoy Count", SaveState.lollipop.et_relations, "Nearby Relations");
            ct_ET_Histogram.ChartAreas[0].RecalculateAxesScale();
            ct_ET_Histogram.ChartAreas[0].RecalculateAxesScale();
        }

        private void cb_view_decoy_histogram_CheckedChanged(object sender, EventArgs e)
        {
            ct_ET_Histogram.Series["relations"].Enabled = !cb_view_decoy_histogram.Checked;
            ct_ET_Histogram.Series["decoys"].Enabled = cb_view_decoy_histogram.Checked;
        }

        private void nUD_ET_Lower_Bound_ValueChanged(object sender, EventArgs e) // maximum delta mass for theoretical proteoform that has mass LOWER than the experimental protoform mass
        {
            SaveState.lollipop.et_low_mass_difference = Convert.ToDouble(nUD_ET_Lower_Bound.Value);
        }

        private void nUD_ET_Upper_Bound_ValueChanged(object sender, EventArgs e) // maximum delta mass for theoretical proteoform that has mass HIGHER than the experimental protoform mass
        {
            SaveState.lollipop.et_high_mass_difference = Convert.ToDouble(nUD_ET_Upper_Bound.Value);
        }

        // scaling for axes of displayed ET Histogram of all ET pairs
        private void yMaxET_ValueChanged(object sender, EventArgs e)
        {
            ct_ET_Histogram.ChartAreas[0].AxisY.Maximum = Convert.ToDouble(yMaxET.Value);
        }

        private void yMinET_ValueChanged(object sender, EventArgs e)
        {
            ct_ET_Histogram.ChartAreas[0].AxisY.Minimum = Convert.ToDouble(yMinET.Value);
        }

        private void xMinET_ValueChanged(object sender, EventArgs e)
        {
            ct_ET_Histogram.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(xMinET.Value);
        }

        private void xMaxET_ValueChanged(object sender, EventArgs e)
        {
            ct_ET_Histogram.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(xMaxET.Value);
        }

        //Stripline
        private void cb_Graph_lowerThreshold_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_Graph_lowerThreshold.Checked)
                ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Add(new StripLine() { BorderColor = Color.Red, IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value) });
            else if (!cb_Graph_lowerThreshold.Checked) ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
        }

        #endregion Histogram Private Methods

        #region Parameters Private Methods

        // bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts
        private void nUD_NoManLower_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.no_mans_land_lowerBound = Convert.ToDouble(nUD_NoManLower.Value);
        }

        private void nUD_NoManUpper_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.no_mans_land_upperBound = Convert.ToDouble(nUD_NoManUpper.Value);
        }

        // bin size used for including individual ET pairs in one 'Peak Center Mass' and peak with for one ET peak
        private void nUD_PeakWidthBase_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.peak_width_base_et = Convert.ToDouble(nUD_PeakWidthBase.Value);
        }


        //Check box to allow whether theoreticals used to create ET pairs must have at least one BU PSM or have been osbsreved in TD 
        private void cb_TDBUpsm_CheckedChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.limit_theoreticals_to_BU_or_TD_observed = cb_TDBUpsm.Checked;
        }

        // ET pairs with [Peak Center Count] AND ET peaks with [Peak Count] above this value are considered acceptable for use in proteoform family. this will be eventually set following ED analysis.
        private void nUD_PeakCountMinThreshold_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.min_peak_count_et = Convert.ToDouble(nUD_PeakCountMinThreshold.Value);
            Parallel.ForEach(SaveState.lollipop.et_peaks, p =>
            {
                p.Accepted = p.peak_relation_group_count >= SaveState.lollipop.min_peak_count_et;
                Parallel.ForEach(p.grouped_relations, r => r.Accepted = p.Accepted);
            });
            Parallel.ForEach(SaveState.lollipop.ed_relations.Values.SelectMany(v => v).Where(r => r.peak != null), pRelation => pRelation.Accepted = pRelation.peak.Accepted);
            dgv_ET_Relations.Refresh();
            dgv_ET_Peak_List.Refresh();
            ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            StripLine lowerCountBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = SaveState.lollipop.min_peak_count_et };
            ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Add(lowerCountBound_stripline);
            update_figures_of_merit();
        }

        #endregion Parameters Private Methods

        #region Tooltip Private Methods

        Point? ct_ET_Histogram_prevPosition = null;
        Point? ct_ET_peakList_prevPosition = null;
        ToolTip ct_ET_Histogram_tt = new ToolTip();
        ToolTip ct_ET_peakList_tt = new ToolTip();
        private void ct_ET_Histogram_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                DisplayUtility.tooltip_graph_display(ct_ET_peakList_tt, e, ct_ET_Histogram, ct_ET_Histogram_prevPosition);
        }

        private void ct_ET_peakList_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                DisplayUtility.tooltip_graph_display(ct_ET_peakList_tt, e, ct_ET_peakList, ct_ET_peakList_prevPosition);
        }

        private void ET_Peak_List_DirtyStateChanged(object sender, EventArgs e)
        {
            relationUtility.peak_acceptability_change(dgv_ET_Peak_List);
            Parallel.ForEach(SaveState.lollipop.ed_relations.Values.SelectMany(v => v).Where(r => r.peak != null), pRelation => pRelation.Accepted = pRelation.peak.Accepted);
            dgv_ET_Relations.Refresh();
            dgv_ET_Peak_List.Refresh();
            update_figures_of_merit();
        }

        #endregion Tooltip Private Methods

    }
}
