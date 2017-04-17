﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using ProteoformSuiteInternal;
using Proteomics;

namespace ProteoformSuiteGUI
{
    public partial class ExperimentExperimentComparison : Form
    {
        private RelationUtility relationUtility;

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

        public void compare_ee()
        {
            if (Lollipop.ee_relations.Count == 0 && Lollipop.proteoform_community.has_e_proteoforms)
            {
                ClearListsAndTables();
                run_the_gamut();
            }
            else if (Lollipop.ee_relations.Count == 0) MessageBox.Show("Go back and aggregate experimental proteoforms.");
        }

        public void run_the_gamut()
        {
            if (Lollipop.notch_search_ee)
            {
                Lollipop.notch_masses_ee = relationUtility.get_notch_masses(tb_notch_masses);
                if (Lollipop.notch_masses_ee == null) return;
            }
            this.Cursor = Cursors.WaitCursor;
            Lollipop.ee_relations = Lollipop.proteoform_community.relate(Lollipop.proteoform_community.experimental_proteoforms, Lollipop.proteoform_community.experimental_proteoforms, ProteoformComparison.ExperimentalExperimental, true);
            Lollipop.ef_relations = Lollipop.proteoform_community.relate_ef(Lollipop.proteoform_community.experimental_proteoforms, Lollipop.proteoform_community.experimental_proteoforms);
            Lollipop.ee_peaks = Lollipop.proteoform_community.accept_deltaMass_peaks(Lollipop.ee_relations, Lollipop.ef_relations);
            ((ProteoformSweet)MdiParent).proteoformFamilies.ClearListsAndTables();

            Parallel.Invoke
            (
                () => this.FillTablesAndCharts(),
                () => { if (Lollipop.neucode_labeled) Lollipop.proteoform_community.construct_families(); }
            );

            if (Lollipop.neucode_labeled)
            {
                ((ProteoformSweet)this.MdiParent).proteoformFamilies.fill_proteoform_families("");
                ((ProteoformSweet)this.MdiParent).proteoformFamilies.update_figures_of_merit();
            }
            else { this.FillTablesAndCharts(); }
            relationUtility.updateFiguresOfMerit(Lollipop.ee_peaks);
            this.Cursor = Cursors.Default;
        }

        public DataGridView GetEERelationDGV()
        {
            return dgv_EE_Relations;
        }

        public DataGridView GetEEPeaksDGV()
        {
            return dgv_EE_Peaks;
        }

        public void ClearListsAndTables()
        {
            Lollipop.proteoform_community.clear_ee();

            foreach (var series in ct_EE_Histogram.Series) series.Points.Clear();
            foreach (var series in ct_EE_peakList.Series) series.Points.Clear();

            dgv_EE_Relations.DataSource = null;
            dgv_EE_Peaks.DataSource = null;
            dgv_EE_Relations.Rows.Clear();
            dgv_EE_Peaks.Rows.Clear();

            cb_automate_peak_acceptance.Checked = false;

        }

        public void FillTablesAndCharts()
        {
            this.dgv_EE_Peaks.CurrentCellDirtyStateChanged -= this.EE_Peak_List_DirtyStateChanged;//remove event handler on form load and table refresh event
            FillEEPeakListTable();
            FillEEPairsGridView();
            DisplayProteoformRelation.FormatRelationsGridView(dgv_EE_Relations, false, true);
            DisplayUtility.FormatPeakListGridView(dgv_EE_Peaks, true);
            GraphEERelations();
            GraphEEPeaks();
            relationUtility.updateFiguresOfMerit(Lollipop.ee_peaks);
            this.dgv_EE_Peaks.CurrentCellDirtyStateChanged += this.EE_Peak_List_DirtyStateChanged;//re-instate event handler after form load and table refresh event 
        }

        private void FillEEPairsGridView()
        {
            DisplayUtility.FillDataGridView(dgv_EE_Relations, Lollipop.ee_relations.Select(r => new DisplayProteoformRelation(r)));
        }
        private void FillEEPeakListTable()
        {
            DisplayUtility.FillDataGridView(dgv_EE_Peaks, Lollipop.ee_peaks.OrderByDescending(p => p.peak_relation_group_count).ToList());
        }
        private void GraphEERelations()
        {
            DisplayUtility.GraphRelationsChart(ct_EE_Histogram, Lollipop.ee_relations, "relations");
            ct_EE_Histogram.Series["relations"].Enabled = true;
            if (Lollipop.ef_relations.Count > 0)
            {
                DisplayUtility.GraphRelationsChart(ct_EE_Histogram, Lollipop.ef_relations["EF_relations_0"], "decoys");
                ct_EE_Histogram.Series["decoys"].Enabled = false;
                cb_view_decoy_histogram.Enabled = true;
            }
            else cb_view_decoy_histogram.Enabled = false;
            cb_view_decoy_histogram.Checked = false;
        }
        private void GraphEEPeaks()
        {
            DisplayUtility.GraphDeltaMassPeaks(ct_EE_peakList, Lollipop.ee_peaks, "Peak Count", "Decoy Count", Lollipop.ee_relations, "Nearby Relations");
        }

        private void dgv_EE_Peak_List_CellClick(object sender, MouseEventArgs e)
        {
            int clickedRow = dgv_EE_Peaks.HitTest(e.X, e.Y).RowIndex;
            int clickedCol = dgv_EE_Peaks.HitTest(e.X, e.Y).ColumnIndex;
            if (e.Button == MouseButtons.Left && clickedRow >= 0 && clickedRow < dgv_EE_Peaks.RowCount
                && clickedCol < dgv_EE_Peaks.ColumnCount && clickedCol >= 0)
            {
                ct_EE_peakList.ChartAreas[0].AxisX.StripLines.Clear();
                DeltaMassPeak selected_peak = (DeltaMassPeak)this.dgv_EE_Peaks.Rows[clickedRow].DataBoundItem;
                DisplayUtility.GraphSelectedDeltaMassPeak(ct_EE_peakList, selected_peak, Lollipop.ee_relations);
            }
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

            //MASS WINDOW
            nUD_EE_Upper_Bound.Minimum = 0;
            nUD_EE_Upper_Bound.Maximum = 500;
            if (!Lollipop.neucode_labeled) Lollipop.ee_max_mass_difference = 150;
            nUD_EE_Upper_Bound.Value = (decimal)Lollipop.ee_max_mass_difference; // maximum mass difference in Da allowed between experimental pair
        }

        private void EE_Peak_List_DirtyStateChanged(object sender, EventArgs e)
        {
            relationUtility.peak_acceptability_change(dgv_EE_Peaks);
            dgv_EE_Peaks.Refresh();
            dgv_EE_Relations.Refresh();
            relationUtility.updateFiguresOfMerit(Lollipop.ee_peaks);
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
            {
                ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Add(new StripLine() { BorderColor = Color.Red, IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value) });
                ct_EE_Histogram.ChartAreas[0].AxisX.MajorGrid.Enabled = true;
                ct_EE_Histogram.ChartAreas[0].AxisY.MajorGrid.Enabled = true;
            }
            else
            {
                ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
                ct_EE_Histogram.ChartAreas[0].AxisX.MajorGrid.Enabled = false;
                ct_EE_Histogram.ChartAreas[0].AxisY.MajorGrid.Enabled = false;
            }
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
            if (Lollipop.proteoform_community.has_e_proteoforms && Lollipop.all_possible_ptmsets != null)
            {
                ClearListsAndTables();
                run_the_gamut();
                xMaxEE.Value = Convert.ToDecimal(Lollipop.ee_max_mass_difference);
            }
            else if (Lollipop.all_possible_ptmsets == null) MessageBox.Show("Go back and load in theoretical proteoforms (Need a ptm list).");
            else MessageBox.Show("Go back and aggregate experimental proteoforms.");
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
            cb_automate_peak_acceptance.Checked = false;
            Lollipop.min_peak_count_ee = Convert.ToDouble(nUD_PeakCountMinThreshold.Value);
            Parallel.ForEach(Lollipop.ee_peaks, p =>
            {
                p.peak_accepted = p.peak_relation_group_count >= Lollipop.min_peak_count_ee;
                Parallel.ForEach(p.grouped_relations, r => r.accepted = p.peak_accepted);
            });
            dgv_EE_Peaks.Refresh();
            dgv_EE_Relations.Refresh();
            ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            StripLine lowerCountBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = Lollipop.min_peak_count_ee };
            ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Add(lowerCountBound_stripline);
            relationUtility.updateFiguresOfMerit(Lollipop.ee_peaks);
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

        private void cb_notch_search_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.notch_search_ee = cb_notch_search.Checked;
            tb_notch_masses.Enabled = cb_notch_search.Checked;
            if (cb_notch_search.Checked) tb_notch_masses.Text = "Enter notches to search, separated by semi-colon.";
        }
        private void cb_automate_peak_acceptance_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_automate_peak_acceptance.Checked)
            {
                foreach (DeltaMassPeak peak in Lollipop.ee_peaks.Where(p => p.peak_relation_group_count >= Lollipop.min_peak_count_ee))
                {
                        if (Lollipop.all_possible_ptmsets.Where(s => s.ptm_combination.Count == 1 || !s.ptm_combination.Select(ptm => ptm.modification).Any(m => m.monoisotopicMass == 0)).Where(m => Math.Abs(m.mass - (peak.peak_deltaM_average)) <= Lollipop.peak_width_base_ee / 2).Count() > 0)
                        {
                            peak.peak_accepted = true;
                        }
                        else peak.peak_accepted = false;
                    Parallel.ForEach(peak.grouped_relations, r => r.accepted = peak.peak_accepted);
                }
                dgv_EE_Peaks.Refresh();
            }
        }
        private void cb_view_decoy_histogram_CheckedChanged(object sender, EventArgs e)
        {
            ct_EE_Histogram.Series["relations"].Enabled = !cb_view_decoy_histogram.Checked;
            ct_EE_Histogram.Series["decoys"].Enabled = cb_view_decoy_histogram.Checked;
        }
    }
}