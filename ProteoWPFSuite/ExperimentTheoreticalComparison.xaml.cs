using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using ProteoformSuiteInternal;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for ExperimentTheoreticalComparison.xaml
    /// </summary>
    public partial class ExperimentTheoreticalComparison : UserControl, ISweetForm, ITabbedMDI, INotifyPropertyChanged
    {
        #region Private Fields
        private RelationUtility relationUtility;
        private List<ProteoformRelation> et_histogram_from_unmod = new List<ProteoformRelation>();

        private bool? cbuseppmnotch;
        private bool? cbbestetpaironly;
        private bool? rbdaltons;
        private bool? rbppm;
        private bool? cbviewdecoyhistogram;
        private bool? cbdiscoveryhistogram;
        private bool? cbgraphlowerthreshold;
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Public Constructor
        public ExperimentTheoreticalComparison()
        {
            relationUtility = new RelationUtility();
            InitializeComponent();
            //following set in XAML
            //dgv_ET_Peak_List.MouseClick += new MouseEventHandler(dgv_ET_Peak_List_CellClick);
            //ct_ET_Histogram.MouseClick += new MouseEventHandler(ct_ET_Histogram_MouseClick);
            //ct_ET_peakList.MouseClick += new MouseEventHandler(ct_ET_peakList_MouseClick);
            //dgv_ET_Peak_List.CurrentCellDirtyStateChanged += new EventHandler(ET_Peak_List_DirtyStateChanged); // makes the change immediate and automatic
            InitializeParameterSet();
            CBGRAPHLOWERTHRESHOLD = true;
            //initialize properties
            this.DataContext = this;
        }
        #endregion

        #region Public Property
        public List<DataTable> DataTables { get; private set; }
        //binding

        /// <summary>
        /// binding for cb_et_peak_accept_rank_check;
        /// </summary>
        public bool? CBUSEPPMNOTCH
        {
            get
            {
                return cbuseppmnotch;
            }
            set
            {
                if (cbuseppmnotch == value || MDIParent==null)
                    return;
                cbuseppmnotch = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CBUSEPPMNOTCH"));
                Sweet.lollipop.et_use_notch = (bool)cbuseppmnotch;
                NotchStack.Visibility = ((bool) cbuseppmnotch)? Visibility.Visible : Visibility.Collapsed;
                NotchNUD.Visibility = ((bool)cbuseppmnotch) ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        public bool? CBBESTETPAIRONLY
        {
            get
            {
                return cbbestetpaironly;
            }
            set
            {
                if (cbbestetpaironly ==value || MDIParent == null)
                    return;
                cbbestetpaironly = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CBBESTETPAIRONLY"));
                Sweet.lollipop.et_bestETRelationOnly = (bool)cbbestetpaironly;
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
                if(rbdaltons==value || MDIParent == null)
                {
                    return;
                }
                rbdaltons = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RBDALTONS"));
                RBPPM = !value;
                Sweet.lollipop.et_notch_ppm = !(bool)value;
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
                if (rbppm == value || MDIParent == null)
                {
                    return;
                }
                rbppm = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RBPPM"));
                RBDALTONS = !value;
                Sweet.lollipop.et_notch_ppm = (bool)rbppm;
            }
        }
        /// <summary>
        /// binding for cb_view_decoy_histogram_check;
        /// </summary>
        public bool? CBVIEWDECOYHISTOGRAM
        {
            get
            {
                return cbviewdecoyhistogram;
            }
            set
            {
                if (cbviewdecoyhistogram == value || MDIParent==null)
                    return;
                cbviewdecoyhistogram = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CBVIEWDECOYHISTOGRAM"));
                ct_ET_Histogram.Series["relations"].Enabled = !(bool)cb_view_decoy_histogram.IsChecked;
                ct_ET_Histogram.Series["decoys"].Enabled = (bool) cb_view_decoy_histogram.IsChecked;
            }
        }

        /// <summary>
        /// binding for cb_discoveryHistogram_check;
        /// </summary>
        public bool? CBDISCOVERYHISTOGRAM
        {
            get
            {
                return cbdiscoveryhistogram;
            }
            set
            {
                if (cbdiscoveryhistogram == value || MDIParent==null)
                    return;
                cbdiscoveryhistogram = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CBDISCOVERYHISTOGRAM"));
                if ((bool)cbdiscoveryhistogram)
                {
                    System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;

                    if (et_histogram_from_unmod.Count == 0)
                    {
                        ProteoformCommunity community = new ProteoformCommunity();
                        et_histogram_from_unmod = community.relate(Sweet.lollipop.target_proteoform_community.experimental_proteoforms.ToArray(), Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Where(t => t.ptm_set.mass == 0).ToArray(), ProteoformComparison.ExperimentalTheoretical, Environment.CurrentDirectory, false);
                    }
                    DisplayUtility.GraphRelationsChart(ct_ET_Histogram, et_histogram_from_unmod, "relations", true);

                    // Show the raw relations in the table
                    tb_relationTableFilter.TextChanged -= tb_relationTableFilter_TextChanged;
                    tb_relationTableFilter.Text = "";
                    tb_relationTableFilter.TextChanged += tb_relationTableFilter_TextChanged;

                    DisplayUtility.FillDataGridView(dgv_ET_Relations, et_histogram_from_unmod.Select(r => new DisplayProteoformRelation(r)));

                    // Get rid of the stripline by default
                    CBGRAPHLOWERTHRESHOLD = false;
                    System.Windows.Input.Mouse.OverrideCursor =null;

                }
                else
                {
                    DisplayUtility.GraphRelationsChart(ct_ET_Histogram, Sweet.lollipop.et_relations, "relations", true);
                    DisplayUtility.FillDataGridView(dgv_ET_Relations, Sweet.lollipop.et_relations.Select(r => new DisplayProteoformRelation(r)).ToList());
                    CBGRAPHLOWERTHRESHOLD = true;
                    tb_relationTableFilter.TextChanged -= tb_relationTableFilter_TextChanged;
                    tb_relationTableFilter.Text = "";
                    tb_relationTableFilter.TextChanged += tb_relationTableFilter_TextChanged;
                }
            }
        }

        /// <summary>
        /// binding for cb_Graph_lowerThreshold_check
        /// </summary>
        public bool? CBGRAPHLOWERTHRESHOLD
        {
            get
            {
                return cbgraphlowerthreshold;
            }
            set
            {
                if (cbgraphlowerthreshold == value)
                    return;
                cbgraphlowerthreshold = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CBGRAPHLOWERTHRESHOLD"));
                /*if (cb_Graph_lowerThreshold.Checked)
                ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Add(new StripLine() { BorderColor = Color.Red, IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value) });
            else if (!cb_Graph_lowerThreshold.Checked) ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Clear();*/
            }
        }

        public ProteoformSweet MDIParent { get; set; }
        #endregion

        #region Public Methods
        public bool ReadyToRunTheGamut()
        {
            return Sweet.lollipop.target_proteoform_community.has_e_and_t_proteoforms;
        }

        public void RunTheGamut(bool full_run)
        {
            shift_masses();  //check for shifts from GUI
            ClearListsTablesFigures(true);
            Sweet.lollipop.et_relations = Sweet.lollipop.target_proteoform_community.relate(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, Sweet.lollipop.target_proteoform_community.theoretical_proteoforms, ProteoformComparison.ExperimentalTheoretical, Environment.CurrentDirectory, Sweet.lollipop.et_bestETRelationOnly);
            Sweet.lollipop.relate_ed();
            Sweet.lollipop.et_peaks = Sweet.lollipop.target_proteoform_community.accept_deltaMass_peaks(Sweet.lollipop.et_relations, Sweet.lollipop.ed_relations);
            if (full_run)
            {
                shift_masses(); //check for shifts from presets (need to have peaks formed first)
                RunTheGamut(false);
            }
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
            if ((bool)cbgraphlowerthreshold) ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Add(new System.Windows.Forms.DataVisualization.Charting.StripLine() { BorderColor = System.Drawing.Color.Red, IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value) });
            else ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            update_figures_of_merit();
            CBDISCOVERYHISTOGRAM = false;
            dgv_ET_Peak_List.CurrentCellDirtyStateChanged += ET_Peak_List_DirtyStateChanged;//re-instate event handler after form load and table refresh event
        }


        public List<DataTable> SetTables()
        {
            DataTables = new List<DataTable>
            {
                DisplayProteoformRelation.FormatRelationsGridView(Sweet.lollipop.et_relations.OfType<ProteoformRelation>().Select(p => new DisplayProteoformRelation(p)).ToList(), "ETRelations", true, false, (bool)cbdiscoveryhistogram),
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
                for (int i = ((ProteoformSweet)this.MDIParent).forms.IndexOf(this) + 1; i < ((ProteoformSweet)this.MDIParent).forms.Count; i++)
                {
                    ISweetForm sweet = ((ProteoformSweet)this.MDIParent).forms[i];
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
            nUD_PeakWidthBase.Maximum = 10;
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

            CBUSEPPMNOTCH = Sweet.lollipop.et_use_notch;
            RBPPM = Sweet.lollipop.et_notch_ppm;
            RBDALTONS = !Sweet.lollipop.et_notch_ppm;
            CBBESTETPAIRONLY = Sweet.lollipop.et_bestETRelationOnly;

            nUD_notch_tolerance.Minimum = 0;
            nUD_notch_tolerance.Maximum = 30;
            nUD_notch_tolerance.Value = Convert.ToDecimal(Sweet.lollipop.notch_tolerance_et);
        }
        #endregion

        #region Other Private Methods

        private void update_figures_of_merit()
        {
            relationUtility.updateFiguresOfMerit(Sweet.lollipop.et_peaks, tb_IdentifiedProteoforms, tb_TotalPeaks, tb_max_accepted_fdr);
        }

        private void bt_compare_et_Click(object sender, EventArgs e)
        {
            if (ReadyToRunTheGamut())
            {
                System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                RunTheGamut(false);
                xMaxET.Value = (decimal)Sweet.lollipop.et_high_mass_difference;
                xMinET.Value = (decimal)Sweet.lollipop.et_low_mass_difference;
                System.Windows.Input.Mouse.OverrideCursor = null;
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
                    Sweet.shift_peak_action(peak);
                }

                ((ProteoformSweet)this.MDIParent).rawExperimentalComponents.FillTablesAndCharts();
                if (Sweet.lollipop.neucode_labeled)
                {
                    Sweet.lollipop.raw_neucode_pairs.Clear();
                    Sweet.lollipop.process_neucode_components(Sweet.lollipop.raw_neucode_pairs);
                    ((ProteoformSweet)this.MDIParent).neuCodePairs.FillTablesAndCharts();
                }
                ((ProteoformSweet)this.MDIParent).aggregatedProteoforms.RunTheGamut(false);
                //RunTheGamut(false); //will need to rerun the Gamut if peaks shifted from preset.
            }
        }

        private void GraphETPeaks()
        {
            DisplayUtility.GraphDeltaMassPeaks(ct_ET_peakList, Sweet.lollipop.et_peaks, "Peak Count", "Median Decoy Count", Sweet.lollipop.et_relations, "Nearby Relations");
        }

        #endregion Other Private Methods

        #region ET Peak List Private Methods

        private void tb_peakTableFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            IEnumerable<object> selected_peaks = tb_peakTableFilter.Text == "" ?
                Sweet.lollipop.et_peaks :
                ExtensionMethods.filter(Sweet.lollipop.et_peaks, tb_peakTableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_ET_Peak_List, selected_peaks.OfType<DeltaMassPeak>().Select(p => new DisplayDeltaMassPeak(p)));
            DisplayDeltaMassPeak.FormatPeakListGridView(dgv_ET_Peak_List, false);
        }

        private void dgv_ET_Peak_List_CellClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int clickedRow = dgv_ET_Peak_List.HitTest(e.X, e.Y).RowIndex;
            int clickedCol = dgv_ET_Peak_List.HitTest(e.X, e.Y).ColumnIndex;
            if (clickedRow < Sweet.lollipop.et_relations.Count && clickedRow >= 0 && clickedCol >= 0 && clickedCol < dgv_ET_Peak_List.ColumnCount)
            {
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Clear();
                    DeltaMassPeak selected_peak = (dgv_ET_Peak_List.Rows[clickedRow].DataBoundItem as DisplayObject).display_object as DeltaMassPeak;
                    DisplayUtility.GraphSelectedDeltaMassPeak(ct_ET_peakList, selected_peak, Sweet.lollipop.et_relations);
                }
                else
                {
                    if (e.Button == System.Windows.Forms.MouseButtons.Right && clickedRow >= 0 && clickedRow < Sweet.lollipop.et_relations.Count)
                    {
                        System.Windows.Forms.ContextMenuStrip ET_peak_List_Menu = new System.Windows.Forms.ContextMenuStrip();
                        int position_xy_mouse_row = dgv_ET_Peak_List.HitTest(e.X, e.Y).RowIndex;

                        DisplayDeltaMassPeak selected_peak = dgv_ET_Peak_List.Rows[clickedRow].DataBoundItem as DisplayDeltaMassPeak;

                        if (position_xy_mouse_row > 0)
                        {
                            ET_peak_List_Menu.Items.Add("Increase Experimental Mass " + Lollipop.MONOISOTOPIC_UNIT_MASS + " Da").Name = "IncreaseMass";
                            ET_peak_List_Menu.Items.Add("Decrease Experimental Mass " + Lollipop.MONOISOTOPIC_UNIT_MASS + " Da").Name = "DecreaseMass";
                        }
                        ET_peak_List_Menu.Show(dgv_ET_Peak_List, new System.Drawing.Point(e.X, e.Y));

                        //event menu click
                        ET_peak_List_Menu.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler((s, ev) => ET_peak_List_Menu_ItemClicked(s, ev, selected_peak));
                    }
                }
            }
        }

        //will leave option to change one at a time by right clicking
        private void ET_peak_List_Menu_ItemClicked(object sender, System.Windows.Forms.ToolStripItemClickedEventArgs e, DisplayDeltaMassPeak peak)
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
            (this.MDIParent as ProteoformSweet).proteoformFamilies.ClearListsTablesFigures(true);
        }

        #endregion ET Peak List Private Methods

        #region Private Methods

        private void tb_relationTableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_relations = tb_relationTableFilter.Text == "" ?
                ((bool)cbdiscoveryhistogram ? et_histogram_from_unmod.OfType<ProteoformRelation>().Select(p => new DisplayProteoformRelation(p)) : Sweet.lollipop.et_relations.OfType<ProteoformRelation>().Select(p => new DisplayProteoformRelation(p)))
                : (ExtensionMethods.filter(((bool)cbdiscoveryhistogram ? et_histogram_from_unmod.OfType<ProteoformRelation>().Select(p => new DisplayProteoformRelation(p)) : Sweet.lollipop.et_relations.OfType<ProteoformRelation>().Select(p => new DisplayProteoformRelation(p))), tb_relationTableFilter.Text));
            DisplayUtility.FillDataGridView(dgv_ET_Relations, selected_relations);
            DisplayProteoformRelation.FormatRelationsGridView(dgv_ET_Relations, true, false, (bool)cbdiscoveryhistogram);
        }

        private void GraphETRelations()
        {
            DisplayUtility.GraphRelationsChart(ct_ET_Histogram, Sweet.lollipop.et_relations, "relations", true);
            ct_ET_Histogram.Series["relations"].Enabled = true;
            if (Sweet.lollipop.ed_relations.Count > 0)
            {
                DisplayUtility.GraphRelationsChart(ct_ET_Histogram, Sweet.lollipop.ed_relations[Sweet.lollipop.decoy_community_name_prefix + "0"], "decoys", true);
                ct_ET_Histogram.Series["decoys"].Enabled = false;
                cb_view_decoy_histogram.IsEnabled = true;
            }
            else cb_view_decoy_histogram.IsEnabled = false;
            CBVIEWDECOYHISTOGRAM = false;

            DisplayUtility.GraphDeltaMassPeaks(ct_ET_peakList, Sweet.lollipop.et_peaks, "Peak Count", "Median Decoy Count", Sweet.lollipop.et_relations, "Nearby Relations");
            ct_ET_Histogram.ChartAreas[0].RecalculateAxesScale();
            ct_ET_Histogram.ChartAreas[0].RecalculateAxesScale();
        }
        
        private void cb_view_decoy_histogram_CheckedChanged(object sender, EventArgs e)
        {
            ct_ET_Histogram.Series["relations"].Enabled = !(bool)cbviewdecoyhistogram;
            ct_ET_Histogram.Series["decoys"].Enabled = (bool)cbviewdecoyhistogram;
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
     

        private void nUD_PeakWidthBase_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.peak_width_base_et = Convert.ToDouble(nUD_PeakWidthBase.Value);
        }

        private void nUD_PeakCountMinThreshold_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_peak_count_et = Convert.ToDouble(nUD_PeakCountMinThreshold.Value);
            change_peak_acceptance();
        }

        private void change_peak_acceptance()
        {
            Parallel.ForEach(Sweet.lollipop.et_peaks, p =>
            {
                p.Accepted = p.peak_relation_group_count >= Sweet.lollipop.min_peak_count_et;
                Parallel.ForEach(p.grouped_relations, r => r.Accepted = p.Accepted);
            });
            Parallel.ForEach(Sweet.lollipop.ed_relations.Values.SelectMany(v => v).Where(r => r.peak != null), pRelation => pRelation.Accepted = pRelation.peak.Accepted);
            dgv_ET_Relations.Refresh();
            dgv_ET_Peak_List.Refresh();
            ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            System.Windows.Forms.DataVisualization.Charting.StripLine lowerCountBound_stripline = new System.Windows.Forms.DataVisualization.Charting.StripLine() { BorderColor = Color.Red, IntervalOffset = Sweet.lollipop.min_peak_count_et };
            ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Add(lowerCountBound_stripline);
            update_figures_of_merit();
            (this.MDIParent).proteoformFamilies.ClearListsTablesFigures(true);
        }

        #region Tooltip Private Methods

        private System.Drawing.Point? ct_ET_Histogram_prevPosition = null;
        private System.Drawing.Point? ct_ET_peakList_prevPosition = null;
        private ToolTip ct_ET_Histogram_tt = new ToolTip();
        private System.Windows.Forms.ToolTip ct_ET_peakList_tt = new System.Windows.Forms.ToolTip();

        private void ct_ET_Histogram_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                DisplayUtility.tooltip_graph_display(ct_ET_peakList_tt, e, ct_ET_Histogram, ct_ET_Histogram_prevPosition);
        }

        private void ct_ET_peakList_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
                DisplayUtility.tooltip_graph_display(ct_ET_peakList_tt, e, ct_ET_peakList, ct_ET_peakList_prevPosition);
        }

        #endregion Tooltip Private Methods

        #endregion Private Methods

        private void nUD_ppm_tolerance_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.notch_tolerance_et = Convert.ToDouble(nUD_notch_tolerance.Value);
        }
    }

}