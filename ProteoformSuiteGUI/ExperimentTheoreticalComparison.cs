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
        private List<ProteoformRelation> et_histogram_from_unmod = new List<ProteoformRelation>();

        #endregion Private Fields

        #region Public Constructor

        public ExperimentTheoreticalComparison()
        {
            relationUtility = new RelationUtility();
            InitializeComponent();
            this.AutoScroll = true;
            this.AutoScrollMinSize = this.ClientSize;
            dgv_ET_Peak_List.MouseClick += new MouseEventHandler(dgv_ET_Peak_List_CellClick);
            ct_ET_Histogram.MouseClick += new MouseEventHandler(ct_ET_Histogram_MouseClick);
            ct_ET_peakList.MouseClick += new MouseEventHandler(ct_ET_peakList_MouseClick);
            dgv_ET_Peak_List.CurrentCellDirtyStateChanged += new EventHandler(ET_Peak_List_DirtyStateChanged); // makes the change immediate and automatic
            InitializeParameterSet();
        }

        #endregion Public Constructor

        #region Public Property

        public List<DataTable> DataTables { get; private set; }

        #endregion Public Property

        #region Public Methods

        public bool ReadyToRunTheGamut()
        {
            return Sweet.lollipop.target_proteoform_community.has_e_and_t_proteoforms;
        }

        public void RunTheGamut(bool full_run)
        {
            shift_masses();  //check for shifts from GUI
            ClearListsTablesFigures(true);
            Sweet.lollipop.et_relations = Sweet.lollipop.target_proteoform_community.relate(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms, ProteoformComparison.ExperimentalTheoretical, true, Environment.CurrentDirectory, true);
            Sweet.lollipop.relate_ed();
            Sweet.lollipop.et_peaks = Sweet.lollipop.target_proteoform_community.accept_deltaMass_peaks(Sweet.lollipop.et_relations, Sweet.lollipop.ed_relations);
            shift_masses(); //check for shifts from presets (need to have peaks formed first)
            FillTablesAndCharts();
        }

        public void FillTablesAndCharts()
        {
            dgv_ET_Peak_List.CurrentCellDirtyStateChanged -= ET_Peak_List_DirtyStateChanged;//remove event handler on form load and table refresh event
            DisplayUtility.FillDataGridView(dgv_ET_Peak_List, Sweet.lollipop.et_peaks.OrderByDescending(p => p.peak_relation_group_count).Select(p => new DisplayDeltaMassPeak(p)).ToList());
            DisplayDeltaMassPeak.FormatPeakListGridView(dgv_ET_Peak_List, false);
            DisplayUtility.FillDataGridView(dgv_ET_Relations, Sweet.lollipop.et_relations.Select(r => new DisplayProteoformRelation(r)).ToList());
            DisplayProteoformRelation.FormatRelationsGridView(dgv_ET_Relations, true, false, false);
            GraphETRelations();
            GraphETPeaks();
            if (cb_Graph_lowerThreshold.Checked) ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Add(new StripLine() { BorderColor = Color.Red, IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value) });
            else ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            update_figures_of_merit();
            cb_discoveryHistogram.Checked = false;
            dgv_ET_Peak_List.CurrentCellDirtyStateChanged += ET_Peak_List_DirtyStateChanged;//re-instate event handler after form load and table refresh event
        }

        public List<DataTable> SetTables()
        {
            DataTables = new List<DataTable>
            {
                DisplayProteoformRelation.FormatRelationsGridView(Sweet.lollipop.et_relations.OfType<ProteoformRelation>().Select(p => new DisplayProteoformRelation(p)).ToList(), "ETRelations", true, false, cb_discoveryHistogram.Checked),
                DisplayDeltaMassPeak.FormatPeakListGridView(Sweet.lollipop.et_peaks.Select(p => new DisplayDeltaMassPeak(p)).ToList(), "ETPeaks", false)
            };
            return DataTables;
        }

        public void ClearListsTablesFigures(bool clear_following)
        {
            //clear all save acceptance actions --> will re-add save actions from loaded actions if peak still exists
            Sweet.save_actions.RemoveAll(x => x.StartsWith("accept ExperimentalTheoretical") || x.StartsWith("unaccept ExperimentalTheoretical"));
            //if in this round or others haven't ever shifted a mass, clear them all. Need to be careful because rerun the gamut whenever shift peaks.
            if (!Sweet.lollipop.raw_experimental_components.Any(c => c.manual_mass_shift > 0))
                Sweet.save_actions.RemoveAll(x => x.StartsWith("shift "));

            Sweet.lollipop.clear_et();
            et_histogram_from_unmod.Clear();

            foreach (var series in ct_ET_Histogram.Series) series.Points.Clear();
            foreach (var series in ct_ET_peakList.Series) series.Points.Clear();
            dgv_ET_Relations.DataSource = null;
            dgv_ET_Peak_List.DataSource = null;
            dgv_ET_Relations.Rows.Clear();
            dgv_ET_Peak_List.Rows.Clear();
            tb_IdentifiedProteoforms.Clear();
            tb_max_accepted_fdr.Clear();
            tb_peakTableFilter.Clear();
            tb_relationTableFilter.Clear();
            tb_TotalPeaks.Clear();

            if (clear_following)
            {
                for (int i = ((ProteoformSweet)MdiParent).forms.IndexOf(this) + 1; i < ((ProteoformSweet)MdiParent).forms.Count; i++)
                {
                    ISweetForm sweet = ((ProteoformSweet)MdiParent).forms[i];
                    if (sweet as ExperimentExperimentComparison == null && sweet as TopDown == null)
                        sweet.ClearListsTablesFigures(false);
                }
            }
        }

        public void InitializeParameterSet()
        {
            //MASS WINDOW
            //only do this if ET hasn't already been run
            nUD_ET_Lower_Bound.Minimum = -2000;
            nUD_ET_Lower_Bound.Maximum = 0;
            nUD_ET_Lower_Bound.Value = Convert.ToDecimal(Sweet.lollipop.et_low_mass_difference); // maximum delta mass for theoretical proteoform that has mass LOWER than the experimental protoform mass

            nUD_ET_Upper_Bound.Minimum = 0;
            nUD_ET_Upper_Bound.Maximum = 2000;
            nUD_ET_Upper_Bound.Value = Convert.ToDecimal(Sweet.lollipop.et_high_mass_difference); // maximum delta mass for theoretical proteoform that has mass HIGHER than the experimental protoform mass

            //Other stuff
            yMaxET.Minimum = 0;
            yMaxET.Maximum = 1000;
            yMaxET.Value = 100; // scaling for y-axis of displayed ET Histogram of all ET pairs

            yMinET.Minimum = -100;
            yMinET.Maximum = yMaxET.Maximum;
            yMinET.Value = 0; // scaling for y-axis of displayed ET Histogram of all ET pairs

            xMaxET.Minimum = xMinET.Value;
            xMaxET.Maximum = nUD_ET_Upper_Bound.Maximum;
            xMaxET.Value = nUD_ET_Upper_Bound.Value; // scaling for x-axis of displayed ET Histogram of all ET pairs

            xMinET.Minimum = -500;
            xMinET.Maximum = xMaxET.Value;
            xMinET.Value = nUD_ET_Lower_Bound.Value; // scaling for x-axis of displayed ET Histogram of all ET pairs

            nUD_PeakWidthBase.Minimum = 0.001m;
            nUD_PeakWidthBase.Maximum = 0.5000m;
            nUD_PeakWidthBase.Value = Convert.ToDecimal(Sweet.lollipop.peak_width_base_et); // bin size used for including individual ET pairs in one 'Peak Center Mass' and peak with for one ET peak

            nUD_PeakCountMinThreshold.ValueChanged -= nUD_PeakCountMinThreshold_ValueChanged;
            nUD_PeakCountMinThreshold.Minimum = 0;
            nUD_PeakCountMinThreshold.Maximum = 1000;
            nUD_PeakCountMinThreshold.Value = Convert.ToDecimal(Sweet.lollipop.min_peak_count_et); // ET pairs with [Peak Center Count] AND ET peaks with [Peak Count] above this value are considered acceptable for use in proteoform family. this will be eventually set following ED analysis.
            nUD_PeakCountMinThreshold.ValueChanged += nUD_PeakCountMinThreshold_ValueChanged;

            tb_peakTableFilter.TextChanged -= tb_peakTableFilter_TextChanged;
            tb_peakTableFilter.Text = "";
            tb_peakTableFilter.TextChanged += tb_peakTableFilter_TextChanged;

            tb_relationTableFilter.TextChanged -= tb_relationTableFilter_TextChanged;
            tb_relationTableFilter.Text = "";
            tb_relationTableFilter.TextChanged += tb_relationTableFilter_TextChanged;

            cb_et_peak_accept_rank.Checked = Sweet.lollipop.et_accept_peaks_based_on_rank;
        }

        #endregion Public Methods

        #region Other Private Methods

        private void update_figures_of_merit()
        {
            relationUtility.updateFiguresOfMerit(Sweet.lollipop.et_peaks, tb_IdentifiedProteoforms, tb_TotalPeaks, tb_max_accepted_fdr);
        }

        private void bt_compare_et_Click(object sender, EventArgs e)
        {
            if (ReadyToRunTheGamut())
            {
                Cursor = Cursors.WaitCursor;
                RunTheGamut(false);
                xMaxET.Value = (decimal)Sweet.lollipop.et_high_mass_difference;
                xMinET.Value = (decimal)Sweet.lollipop.et_low_mass_difference;
                Cursor = Cursors.Default;
            }
            else if (Sweet.lollipop.target_proteoform_community.has_e_proteoforms)
                MessageBox.Show("Go back and create a theoretical database.");
            else
                MessageBox.Show("Go back and aggregate experimental proteoforms.");
        }

        private void shift_masses()
        {
            List<DeltaMassPeak> peaks_to_shift = Sweet.lollipop.et_peaks.Where(p => p.mass_shifter != "0" && p.mass_shifter != "").ToList();
            if (peaks_to_shift.Count > 0)
            {
                //before making shifts, make sure all mass shifters are integers
                foreach (DeltaMassPeak peak in peaks_to_shift)
                {
                    if (!Int32.TryParse(peak.mass_shifter, out int ok))
                    {
                        MessageBox.Show("Could not convert mass shift for peak at delta mass " + peak.DeltaMass + ". Please enter an integer.");
                        return;
                    }
                }
                foreach (DeltaMassPeak peak in peaks_to_shift)
                {
                    int int_mass_shifter = Convert.ToInt32(peak.mass_shifter);
                    peak.shift_experimental_masses(int_mass_shifter, Sweet.lollipop.neucode_labeled);
                }

                ((ProteoformSweet)MdiParent).rawExperimentalComponents.FillTablesAndCharts();
                if (Sweet.lollipop.neucode_labeled)
                {
                    Sweet.lollipop.raw_neucode_pairs.Clear();
                    Sweet.lollipop.process_neucode_components(Sweet.lollipop.raw_neucode_pairs);
                    ((ProteoformSweet)MdiParent).neuCodePairs.FillTablesAndCharts();
                }
                ((ProteoformSweet)MdiParent).aggregatedProteoforms.RunTheGamut(false);
                RunTheGamut(false); //will need to rerun the Gamut if peaks shifted from preset.
            }
        }

        private void GraphETPeaks()
        {
            DisplayUtility.GraphDeltaMassPeaks(ct_ET_peakList, Sweet.lollipop.et_peaks, "Peak Count", "Median Decoy Count", Sweet.lollipop.et_relations, "Nearby Relations");
        }

        #endregion Other Private Methods

        #region ET Peak List Private Methods

        private void tb_peakTableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_peaks = tb_peakTableFilter.Text == "" ?
                Sweet.lollipop.et_peaks :
                ExtensionMethods.filter(Sweet.lollipop.et_peaks, tb_peakTableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_ET_Peak_List, selected_peaks.OfType<DeltaMassPeak>().Select(p => new DisplayDeltaMassPeak(p)));
            DisplayDeltaMassPeak.FormatPeakListGridView(dgv_ET_Peak_List, false);
        }

        private void dgv_ET_Peak_List_CellClick(object sender, MouseEventArgs e)
        {
            int clickedRow = dgv_ET_Peak_List.HitTest(e.X, e.Y).RowIndex;
            int clickedCol = dgv_ET_Peak_List.HitTest(e.X, e.Y).ColumnIndex;
            if (clickedRow < Sweet.lollipop.et_relations.Count && clickedRow >= 0 && clickedCol >= 0 && clickedCol < dgv_ET_Peak_List.ColumnCount)
            {
                if (e.Button == MouseButtons.Left)
                {
                    ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Clear();
                    DeltaMassPeak selected_peak = (dgv_ET_Peak_List.Rows[clickedRow].DataBoundItem as DisplayObject).display_object as DeltaMassPeak;
                    DisplayUtility.GraphSelectedDeltaMassPeak(ct_ET_peakList, selected_peak, Sweet.lollipop.et_relations);
                }
                else
                {
                    if (e.Button == MouseButtons.Right && clickedRow >= 0 && clickedRow < Sweet.lollipop.et_relations.Count)
                    {
                        ContextMenuStrip ET_peak_List_Menu = new ContextMenuStrip();
                        int position_xy_mouse_row = dgv_ET_Peak_List.HitTest(e.X, e.Y).RowIndex;

                        DisplayDeltaMassPeak selected_peak = dgv_ET_Peak_List.Rows[clickedRow].DataBoundItem as DisplayDeltaMassPeak;

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
        private void ET_peak_List_Menu_ItemClicked(object sender, ToolStripItemClickedEventArgs e, DisplayDeltaMassPeak peak)
        {
            int int_mass_shifter = 0;
            try
            {
                int_mass_shifter = Convert.ToInt32(peak.MassShifter);
            }
            catch
            {
                MessageBox.Show("Oops, this mass shifter " + peak.MassShifter + " is not an integer.");
                return;
            }

            switch (e.ClickedItem.Name.ToString())
            {
                case "IncreaseMass":
                    peak.MassShifter = (int_mass_shifter + 1).ToString();
                    break;

                case "DecreaseMass":
                    peak.MassShifter = (int_mass_shifter - 1).ToString();
                    break;
            }
            dgv_ET_Peak_List.Refresh();
        }

        private void ET_Peak_List_DirtyStateChanged(object sender, EventArgs e)
        {
            dgv_ET_Relations.Refresh();
            dgv_ET_Peak_List.Refresh();
            update_figures_of_merit();
            (MdiParent as ProteoformSweet).proteoformFamilies.ClearListsTablesFigures(true);
        }

        #endregion ET Peak List Private Methods

        #region Histogram Private Methods

        private void tb_relationTableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_relations = tb_relationTableFilter.Text == "" ?
                (cb_discoveryHistogram.Checked ? et_histogram_from_unmod.OfType<ProteoformRelation>().Select(p => new DisplayProteoformRelation(p)) : Sweet.lollipop.et_relations.OfType<ProteoformRelation>().Select(p => new DisplayProteoformRelation(p)))
                : (ExtensionMethods.filter((cb_discoveryHistogram.Checked ? et_histogram_from_unmod.OfType<ProteoformRelation>().Select(p => new DisplayProteoformRelation(p)) : Sweet.lollipop.et_relations.OfType<ProteoformRelation>().Select(p => new DisplayProteoformRelation(p))), tb_relationTableFilter.Text));
            DisplayUtility.FillDataGridView(dgv_ET_Relations, selected_relations);
            DisplayProteoformRelation.FormatRelationsGridView(dgv_ET_Relations, true, false, cb_discoveryHistogram.Checked);
        }

        private void GraphETRelations()
        {
            DisplayUtility.GraphRelationsChart(ct_ET_Histogram, Sweet.lollipop.et_relations, "relations", true);
            ct_ET_Histogram.Series["relations"].Enabled = true;
            if (Sweet.lollipop.ed_relations.Count > 0)
            {
                DisplayUtility.GraphRelationsChart(ct_ET_Histogram, Sweet.lollipop.ed_relations[Sweet.lollipop.decoy_community_name_prefix + "0"], "decoys", true);
                ct_ET_Histogram.Series["decoys"].Enabled = false;
                cb_view_decoy_histogram.Enabled = true;
            }
            else cb_view_decoy_histogram.Enabled = false;
            cb_view_decoy_histogram.Checked = false;

            DisplayUtility.GraphDeltaMassPeaks(ct_ET_peakList, Sweet.lollipop.et_peaks, "Peak Count", "Median Decoy Count", Sweet.lollipop.et_relations, "Nearby Relations");
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
            Sweet.lollipop.et_low_mass_difference = Convert.ToDouble(nUD_ET_Lower_Bound.Value);
        }

        private void nUD_ET_Upper_Bound_ValueChanged(object sender, EventArgs e) // maximum delta mass for theoretical proteoform that has mass HIGHER than the experimental protoform mass
        {
            Sweet.lollipop.et_high_mass_difference = Convert.ToDouble(nUD_ET_Upper_Bound.Value);
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

        //Special histogram counting relations of mass difference from unmodified
        private void cb_discoveryHistogram_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_discoveryHistogram.Checked)
            {
                Cursor = Cursors.WaitCursor;
                if (et_histogram_from_unmod.Count == 0)
                {
                    ProteoformCommunity community = new ProteoformCommunity();
                    et_histogram_from_unmod = community.relate(Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Where(ex => ex.accepted).ToArray(), Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(t => t.ptm_set.mass == 0).ToArray(), ProteoformComparison.ExperimentalTheoretical, false, Environment.CurrentDirectory, false);
                }
                DisplayUtility.GraphRelationsChart(ct_ET_Histogram, et_histogram_from_unmod, "relations", true);

                // Show the raw relations in the table
                tb_relationTableFilter.TextChanged -= tb_relationTableFilter_TextChanged;
                tb_relationTableFilter.Text = "";
                tb_relationTableFilter.TextChanged += tb_relationTableFilter_TextChanged;

                DisplayUtility.FillDataGridView(dgv_ET_Relations, et_histogram_from_unmod.Select(r => new DisplayProteoformRelation(r)));

                // Get rid of the stripline by default
                cb_Graph_lowerThreshold.Checked = false;
                Cursor = Cursors.Default;
            }
            else
            {
                DisplayUtility.GraphRelationsChart(ct_ET_Histogram, Sweet.lollipop.et_relations, "relations", true);
                DisplayUtility.FillDataGridView(dgv_ET_Relations, Sweet.lollipop.et_relations.Select(r => new DisplayProteoformRelation(r)).ToList());
                cb_Graph_lowerThreshold.Checked = true;
                tb_relationTableFilter.TextChanged -= tb_relationTableFilter_TextChanged;
                tb_relationTableFilter.Text = "";
                tb_relationTableFilter.TextChanged += tb_relationTableFilter_TextChanged;
            }
        }

        #endregion Histogram Private Methods

        #region Parameters Private Methods

        // bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts
        // not modifiable currently

        // bin size used for including individual ET pairs in one 'Peak Center Mass' and peak with for one ET peak
        private void nUD_PeakWidthBase_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.peak_width_base_et = Convert.ToDouble(nUD_PeakWidthBase.Value);
        }

        // ET pairs with [Peak Center Count] AND ET peaks with [Peak Count] above this value are considered acceptable for use in proteoform family. this will be eventually set following ED analysis.
        private void nUD_PeakCountMinThreshold_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_peak_count_et = Convert.ToDouble(nUD_PeakCountMinThreshold.Value);
            change_peak_acceptance();
        }

        private void change_peak_acceptance()
        {
            Parallel.ForEach(Sweet.lollipop.et_peaks, p =>
            {
                p.Accepted = p.peak_relation_group_count >= Sweet.lollipop.min_peak_count_et && (!Sweet.lollipop.et_accept_peaks_based_on_rank || (p.possiblePeakAssignments.Count > 0 && p.possiblePeakAssignments.Any(a => a.ptm_rank_sum < Sweet.lollipop.mod_rank_first_quartile)));
                Parallel.ForEach(p.grouped_relations, r => r.Accepted = p.Accepted);
            });
            Parallel.ForEach(Sweet.lollipop.ed_relations.Values.SelectMany(v => v).Where(r => r.peak != null), pRelation => pRelation.Accepted = pRelation.peak.Accepted);
            dgv_ET_Relations.Refresh();
            dgv_ET_Peak_List.Refresh();
            ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            StripLine lowerCountBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = Sweet.lollipop.min_peak_count_et };
            ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Add(lowerCountBound_stripline);
            update_figures_of_merit();
            (MdiParent as ProteoformSweet).proteoformFamilies.ClearListsTablesFigures(true);
        }

        #endregion Parameters Private Methods

        #region Tooltip Private Methods

        private Point? ct_ET_Histogram_prevPosition = null;
        private Point? ct_ET_peakList_prevPosition = null;
        private ToolTip ct_ET_Histogram_tt = new ToolTip();
        private ToolTip ct_ET_peakList_tt = new ToolTip();

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

        #endregion Tooltip Private Methods

        private void cb_et_peak_accept_rank_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.et_accept_peaks_based_on_rank = cb_et_peak_accept_rank.Checked;
            change_peak_acceptance();
        }
    }
}