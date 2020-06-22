﻿using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;

namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for ExperimentExperimentComparison.xaml
    /// </summary>
    public partial class ExperimentExperimentComparison : UserControl,ISweetForm, ITabbedMDI, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool? is_cb_view_decoy_histogram;
        private bool? is_cb_Graph_lowerThreshold;
        private bool? is_cb_ee_peak_accept_rank;
        private bool? cbusenotch;
        private bool? rbdaltons;
        private bool? rbppm;


        /// <summary>
        /// binding for cb_ee_peak_accept_rank_check;
        /// </summary>
        public bool? CBUSENOTCH
        {
            get
            {
                return cbusenotch;
            }
            set
            {
                if (cbusenotch == value)// || MDIParent==null)
                    return;
                cbusenotch = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CBUSENOTCH"));
                Sweet.lollipop.ee_use_notch = (bool)cbusenotch;
                NotchStack.Visibility = ((bool)cbusenotch) ? Visibility.Visible : Visibility.Collapsed;
                NotchNUD.Visibility = ((bool)cbusenotch) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public bool? RBDALTONS
        {
            get
            {
                return rbdaltons;
            }
            set
            {
                if (rbdaltons == value) //|| MDIParent == null)
                {
                    return;
                }
                rbdaltons = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RBDALTONS"));
                RBPPM = !value;
                Sweet.lollipop.ee_notch_ppm = !(bool)value;
            }
        }
        public bool? RBPPM
        {
            get
            {
                return rbppm;
            }
            set
            {
                if (rbppm == value) //|| MDIParent == null)
                {
                    return;
                }
                rbppm = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RBPPM"));
                RBDALTONS = !value;
                Sweet.lollipop.ee_notch_ppm = (bool)rbppm;
            }
        }

        public bool? CK_Auto
        {
            get
            {
                return is_cb_ee_peak_accept_rank;
            }
            set
            {
                if(is_cb_ee_peak_accept_rank == value) //|| MDIParent==null)
                {
                    return;
                }
                is_cb_ee_peak_accept_rank = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CK_Auto"));
                Sweet.lollipop.ee_accept_peaks_based_on_rank = (bool)is_cb_ee_peak_accept_rank;
                if(MDIParent == null)
                {
                    return;
                }
                change_peak_acceptance();
            }
        }
        public bool? CK_View
        {
            get
            {
                return is_cb_view_decoy_histogram;
            }
            set
            {
                if(is_cb_view_decoy_histogram == value) //|| MDIParent==null)
                {
                    return;
                }
                is_cb_view_decoy_histogram = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CK_View"));
                ct_EE_Histogram.Series["relations"].Enabled = !(bool)is_cb_view_decoy_histogram;
                ct_EE_Histogram.Series["decoys"].Enabled = (bool)is_cb_view_decoy_histogram;
            }
        }
        public bool? CK_Graph
        {
            get
            {
                return is_cb_Graph_lowerThreshold;
            }
            set
            {
                if(is_cb_Graph_lowerThreshold == value) //|| MDIParent==null)
                {
                    return;
                }
                is_cb_Graph_lowerThreshold = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CK_Graph"));
                ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
                if ((bool) is_cb_Graph_lowerThreshold)
                    ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Add(new StripLine() { BorderColor = Color.Red, IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value) });
                  
            }
        }

        public ExperimentExperimentComparison()
        {
            InitializeComponent();
            this.DataContext = this;
            this.relationUtility = new RelationUtility();
            this.dgv_EE_Peaks.MouseClick += new System.Windows.Forms.MouseEventHandler(dgv_EE_Peak_List_CellClick);
            this.ct_EE_Histogram.MouseClick += new System.Windows.Forms.MouseEventHandler(ct_EE_Histogram_MouseClick);
            this.ct_EE_peakList.MouseClick += new System.Windows.Forms.MouseEventHandler(ct_EE_peakList_MouseClick);
            dgv_EE_Peaks.CurrentCellDirtyStateChanged += new EventHandler(EE_Peak_List_DirtyStateChanged); //makes the change immediate and automatic
            CK_Graph = true;
            CK_View = false;
            InitializeParameterSet();
            
        }

        public List<DataTable> DataTables { get; private set; }
        public ProteoformSweet MDIParent { get; set; }

       
        #region Private Field

        private RelationUtility relationUtility;

        #endregion Private Field

        

        #region Public Methods

        public bool ReadyToRunTheGamut()
        {
            return Sweet.lollipop.target_proteoform_community.has_e_and_t_proteoforms; // Need the ptm dictionary for peak assignment from theoretical database
        }

        public void RunTheGamut(bool full_run)
        {
            ClearListsTablesFigures(true);
            Sweet.lollipop.ee_relations = Sweet.lollipop.target_proteoform_community.relate(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, Sweet.lollipop.target_proteoform_community.experimental_proteoforms, ProteoformComparison.ExperimentalExperimental, Environment.CurrentDirectory, true);
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
                for (int i = (MDIParent).forms.IndexOf(this) + 1; i < (MDIParent).forms.Count; i++)
                {
                    ISweetForm sweet = (MDIParent).forms[i];
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
            CK_View = false;
            GraphEERelations();
            GraphEEPeaks();
            ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            if (is_cb_Graph_lowerThreshold.HasValue && (bool)is_cb_Graph_lowerThreshold)
                ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Add(new StripLine() { BorderColor = Color.Red, IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value) });
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
            yMaxEE.Maximum = 5000;
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

            CK_Auto = Sweet.lollipop.ee_accept_peaks_based_on_rank;

            CBUSENOTCH = Sweet.lollipop.ee_use_notch;
            RBPPM = Sweet.lollipop.ee_notch_ppm;
            RBDALTONS = !Sweet.lollipop.ee_notch_ppm;

            nUD_notch_tolerance.Minimum = 0;
            nUD_notch_tolerance.Maximum = 30;
            nUD_notch_tolerance.Value = Convert.ToDecimal(Sweet.lollipop.notch_tolerance_ee);

        }

        #endregion Public Methods

        #region Other Private Methods

        private void GraphEEPeaks()
        {
            DisplayUtility.GraphDeltaMassPeaks(ct_EE_peakList, Sweet.lollipop.ee_peaks, "Peak Count", "Decoy Count", Sweet.lollipop.ee_relations, "Nearby Relations");
        }

        private void update_figures_of_merit()
        {
            relationUtility.updateFiguresOfMerit(Sweet.lollipop.ee_peaks, tb_totalAcceptedEERelations, tb_TotalEEPeaks, tb_max_accepted_fdr);
        }

        #endregion Other Private Methods

        #region EE Peak List Private Methods

        private void tb_peakTableFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            IEnumerable<object> selected_peaks = tb_peakTableFilter.Text == "" ?
            Sweet.lollipop.ee_peaks :
            ExtensionMethods.filter(Sweet.lollipop.ee_peaks, tb_peakTableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_EE_Peaks, selected_peaks.OfType<DeltaMassPeak>().Select(p => new DisplayDeltaMassPeak(p)));
            DisplayDeltaMassPeak.FormatPeakListGridView(dgv_EE_Peaks, true);
        }
        //changed
        private void dgv_EE_Peak_List_CellClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int clickedRow = dgv_EE_Peaks.HitTest(e.X, e.Y).RowIndex;
            int clickedCol = dgv_EE_Peaks.HitTest(e.X, e.Y).ColumnIndex;
            if (e.Button == System.Windows.Forms.MouseButtons.Left && clickedRow >= 0 && clickedRow < Sweet.lollipop.ee_relations.Count
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
            (MDIParent).proteoformFamilies.ClearListsTablesFigures(true);
        }

        #endregion EE Peak List Private Methods

        #region Histogram Private Methods

        private void tb_relationTableFilter_TextChanged(object sender, TextChangedEventArgs e)
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
                cb_view_decoy_histogram.IsEnabled = true;
            }
            else cb_view_decoy_histogram.IsEnabled = false;
            is_cb_view_decoy_histogram = false;
        }
        /*
        private void cb_view_decoy_histogram_CheckedChanged(object sender, EventArgs e)
        {
            ct_EE_Histogram.Series["relations"].Enabled = !cb_view_decoy_histogram.Checked;
            ct_EE_Histogram.Series["decoys"].Enabled = cb_view_decoy_histogram.Checked;
        }*/

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

        private void nUD_ppm_tolerance_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.notch_tolerance_ee = Convert.ToDouble(nUD_notch_tolerance.Value);
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
            if (is_cb_Graph_lowerThreshold.HasValue && (bool)is_cb_Graph_lowerThreshold)
            {
                StripLine lowerCountBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = Sweet.lollipop.min_peak_count_ee };
                ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Add(lowerCountBound_stripline);
            }
            update_figures_of_merit();
            (MDIParent).proteoformFamilies.ClearListsTablesFigures(true);
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
        /*
        private void cb_Graph_lowerThreshold_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_Graph_lowerThreshold.Checked)
                ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Add(new StripLine() { BorderColor = Color.Red, IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value) });
            else
                ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
        }*/

        #endregion Histogram Private Methods

        #region Parameters Private Methods

        private void nUD_MaxRetTimeDifference_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.ee_max_RetentionTime_difference = Convert.ToDouble(nUD_MaxRetTimeDifference.Value);
        }

        #endregion Parameters Private Methods

        #region Tooltip Private Methods

        private System.Drawing.Point? ct_EE_Histogram_prevPosition = null;
        private System.Drawing.Point? ct_EE_peakList_prevPosition = null;

        private System.Windows.Forms.ToolTip ct_EE_Histogram_tt = new System.Windows.Forms.ToolTip();
        private System.Windows.Forms.ToolTip ct_EE_peakList_tt = new System.Windows.Forms.ToolTip();

        

        //changed
        private void ct_EE_Histogram_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                DisplayUtility.tooltip_graph_display(ct_EE_Histogram_tt, e, ct_EE_Histogram, ct_EE_Histogram_prevPosition);
        }

        private void ct_EE_peakList_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                DisplayUtility.tooltip_graph_display(ct_EE_peakList_tt, e, ct_EE_peakList, ct_EE_peakList_prevPosition);
        }

        #endregion Tooltip Private Methods
        /*
        private void cb_ee_peak_accept_rank_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.ee_accept_peaks_based_on_rank = cb_ee_peak_accept_rank.Checked;
            change_peak_acceptance();
        }
        */
    }
}
