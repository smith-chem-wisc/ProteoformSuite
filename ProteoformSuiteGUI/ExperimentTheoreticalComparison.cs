using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
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
        public event ETPeakAcceptabilityChangedEventHandler ETPeakAcceptabilityChanged;
        private bool compared_et;

        public ExperimentTheoreticalComparison()
        {
            InitializeComponent();
            this.dgv_ET_Peak_List.MouseClick += new MouseEventHandler(dgv_ET_Peak_List_CellClick);
            this.dgv_ET_Pairs.CellMouseClick += new DataGridViewCellMouseEventHandler(dgv_ET_Pairs_CellClick);
            this.ct_ET_Histogram.MouseClick += new MouseEventHandler(ct_ET_Histogram_MouseClick);
            this.ct_ET_peakList.MouseClick += new MouseEventHandler(ct_ET_peakList_MouseClick);
            dgv_ET_Peak_List.CurrentCellDirtyStateChanged += new EventHandler(ET_Peak_List_DirtyStateChanged); //makes the change immediate and automatic
            ETPeakAcceptabilityChanged += ExperimentTheoreticalComparison_ETPeakAcceptabilityChanged;
            InitializeParameterSet();
            InitializeMassWindow();
        }

        public void ExperimentTheoreticalComparison_Load(object sender, EventArgs e)
        { }

        public void compare_et()
        {
            if (Lollipop.et_relations.Count == 0 && Lollipop.proteoform_community.has_e_and_t_proteoforms)
            {
                run_the_gamut();
            }
            else if (Lollipop.et_relations.Count == 0 && Lollipop.proteoform_community.has_e_proteoforms) MessageBox.Show("Go back and create a theoretical database.");
            else if (Lollipop.et_relations.Count == 0) MessageBox.Show("Go back and aggregate experimental proteoforms.");
        }

        private void run_the_gamut()
        {
            this.Cursor = Cursors.WaitCursor;
            Lollipop.make_et_relationships();
            this.FillTablesAndCharts();
            this.Cursor = Cursors.Default;
            compared_et = true;
        }

        public void FillTablesAndCharts()
        {
            this.dgv_ET_Peak_List.CurrentCellDirtyStateChanged -= this.ET_Peak_List_DirtyStateChanged;//remove event handler on form load and table refresh event
            FillETPeakListTable();
            FillETRelationsGridView();
            DisplayUtility.FormatRelationsGridView(dgv_ET_Pairs, true, false);
            DisplayUtility.FormatPeakListGridView(dgv_ET_Peak_List, false);
            GraphETRelations();
            GraphETPeaks();
            updateFiguresOfMerit();
            this.dgv_ET_Peak_List.CurrentCellDirtyStateChanged += this.ET_Peak_List_DirtyStateChanged;//re-instate event handler after form load and table refresh event 
        }

        public DataGridView GetETRelationsDGV()
        {
            return dgv_ET_Pairs;
        }

        public DataGridView GetETPeaksDGV()
        {
            return dgv_ET_Peak_List;
        }

        private void ClearListsAndTables()
        {
            Lollipop.et_relations.Clear();
            Lollipop.et_peaks.Clear();
            Lollipop.ed_relations.Clear();
            Lollipop.proteoform_community.families.Clear();
            foreach (Proteoform p in Lollipop.proteoform_community.experimental_proteoforms) p.relationships.RemoveAll(r => r.relation_type == ProteoformComparison.et || r.relation_type == ProteoformComparison.ed);
            foreach (Proteoform p in Lollipop.proteoform_community.theoretical_proteoforms) p.relationships.RemoveAll(r => r.relation_type == ProteoformComparison.et || r.relation_type == ProteoformComparison.ed);
            foreach (Proteoform p in Lollipop.proteoform_community.decoy_proteoforms.Values.SelectMany(d => d)) p.relationships.Clear();
            Lollipop.proteoform_community.relations_in_peaks.RemoveAll(r => r.relation_type == ProteoformComparison.et || r.relation_type == ProteoformComparison.ed);
            Lollipop.proteoform_community.delta_mass_peaks.RemoveAll(k => k.relation_type == ProteoformComparison.et || k.relation_type == ProteoformComparison.ed);

            dgv_ET_Pairs.DataSource = null;
            dgv_ET_Peak_List.DataSource = null;
            dgv_ET_Pairs.Rows.Clear();
            dgv_ET_Peak_List.Rows.Clear();
        }

        private void updateFiguresOfMerit()
        {
            List<DeltaMassPeak> big_peaks = Lollipop.et_peaks.Where(p => p.peak_accepted).ToList();
            tb_IdentifiedProteoforms.Text = big_peaks.Select(p => p.grouped_relations.Count).Sum().ToString();
            tb_TotalPeaks.Text = big_peaks.Count.ToString();
            if (Lollipop.ed_relations.Count > 0 && big_peaks.Count > 0) tb_max_accepted_fdr.Text = Math.Round( big_peaks.Max(p => p.peak_group_fdr) , 3).ToString();
        }

        private void FillETRelationsGridView()
        {
            DisplayUtility.FillDataGridView(dgv_ET_Pairs, Lollipop.et_relations);
        }
        private void FillETPeakListTable()
        {
            DisplayUtility.FillDataGridView(dgv_ET_Peak_List, Lollipop.et_peaks.OrderByDescending(p => p.peak_relation_group_count).ToList());
        }
        private void GraphETRelations()
        {
            DisplayUtility.GraphRelationsChart(ct_ET_Histogram, Lollipop.et_relations, "relations");

            ct_ET_Histogram.ChartAreas[0].RecalculateAxesScale();
        }
        private void GraphETPeaks()
        {
            DisplayUtility.GraphDeltaMassPeaks(ct_ET_peakList, Lollipop.et_peaks, "Peak Count", "Median Decoy Count", Lollipop.et_relations, "Nearby Relations");
        }

        protected void ExperimentTheoreticalComparison_ETPeakAcceptabilityChanged(object sender, ETPeakAcceptabilityChangedEventArgs e)
        {
            foreach (ProteoformRelation pRelation in Lollipop.et_relations.Where(p => e.ETPeak.grouped_relations.Contains(p)))
            {
                pRelation.accepted = e.IsPeakAcceptable;
            }
            FillTablesAndCharts();
        }

        private void dgv_ET_Pairs_CellClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && Lollipop.psm_list.Count > 0)
            {
                dgv_psmList.Visible = true;
                display_psm_list(e.RowIndex);
            }
        }
        private void display_psm_list(int row_index)
        {
            ProteoformRelation selected_pf = (ProteoformRelation)this.dgv_ET_Pairs.Rows[row_index].DataBoundItem;
            DisplayUtility.FillDataGridView(dgv_psmList, ((TheoreticalProteoform)selected_pf.connected_proteoforms[1]).psm_list);
        }

        private void dgv_ET_Peak_List_CellClick(object sender, MouseEventArgs e)
        {
            int clickedRow = dgv_ET_Peak_List.HitTest(e.X, e.Y).RowIndex;
            int clickedCol = dgv_ET_Peak_List.HitTest(e.X, e.Y).ColumnIndex;
            if (clickedRow < Lollipop.et_relations.Count && clickedRow >= 0 && clickedCol >=0 && clickedCol < dgv_ET_Peak_List.ColumnCount)
            { 
                if (e.Button == MouseButtons.Left)
                {
                    ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Clear();
                    DeltaMassPeak selected_peak = (DeltaMassPeak)this.dgv_ET_Peak_List.Rows[clickedRow].DataBoundItem;
                    DisplayUtility.GraphSelectedDeltaMassPeak(ct_ET_peakList, selected_peak, Lollipop.et_relations);
                }
                else
                {
                    if (e.Button == MouseButtons.Right && clickedRow >= 0 && clickedRow < Lollipop.et_relations.Count)
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

        private void bt_compare_et_Click(object sender, EventArgs e)
        {
            shift_masses();
            ClearListsAndTables();
            run_the_gamut();
            xMaxET.Value = (decimal)Lollipop.et_high_mass_difference;
            xMinET.Value = (decimal)Lollipop.et_low_mass_difference;
        }

        //shifts any mass shifts that have been changed from 0 in dgv
        private void shift_masses()
        {
            List<DeltaMassPeak> peaks_to_shift = Lollipop.et_peaks.Where(p => p.mass_shifter != "0" && p.mass_shifter != "").ToList();
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
                        MessageBox.Show("Could not convert mass shift for peak at delta mass " + peak.delta_mass + ". Please enter an integer.");
                        return;
                    }
                    peak.shift_experimental_masses(int_mass_shifter, Lollipop.neucode_labeled);
                }
                Lollipop.regroup_components(Lollipop.neucode_labeled, Lollipop.validate_proteoforms, Lollipop.input_files, Lollipop.raw_neucode_pairs, Lollipop.raw_experimental_components, Lollipop.raw_quantification_components, Lollipop.min_rel_abundance, Lollipop.min_num_CS);
            }
            tb_max_accepted_fdr.Text = Lollipop.et_peaks.Where(p => p.peak_accepted).Max(p => p.peak_group_fdr).ToString();
        }

        //will leave option to change one at a time by right clicking
        void ET_peak_List_Menu_ItemClicked(object sender, ToolStripItemClickedEventArgs e, DeltaMassPeak peak)
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

        protected virtual void ONETAcceptibilityChanged(ETPeakAcceptabilityChangedEventArgs e)
        {
            if (ETPeakAcceptabilityChanged != null) ETPeakAcceptabilityChanged(this, e);
        }

        private void InitializeMassWindow()
        {
            //only do this if ET hasn't already been run
            nUD_ET_Lower_Bound.Minimum = -500;
            nUD_ET_Lower_Bound.Maximum = 0;
            if (Lollipop.neucode_labeled) Lollipop.et_low_mass_difference = -250;
            else Lollipop.et_low_mass_difference = -50;
            nUD_ET_Lower_Bound.Value = Convert.ToDecimal(Lollipop.et_low_mass_difference); // maximum delta mass for theoretical proteoform that has mass LOWER than the experimental protoform mass

            nUD_ET_Upper_Bound.Minimum = 0;
            nUD_ET_Upper_Bound.Maximum = 500;
            if (Lollipop.neucode_labeled) Lollipop.et_high_mass_difference = 250;
            else Lollipop.et_high_mass_difference = 150;
            nUD_ET_Upper_Bound.Value = Convert.ToDecimal(Lollipop.et_high_mass_difference); // maximum delta mass for theoretical proteoform that has mass HIGHER than the experimental protoform mass
        }

        private void InitializeParameterSet()
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
            nUD_NoManLower.Value = Convert.ToDecimal(Lollipop.no_mans_land_lowerBound); // lower bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts

            nUD_NoManUpper.Minimum = 0.50m;
            nUD_NoManUpper.Maximum = 1.00m;
            nUD_NoManUpper.Value = Convert.ToDecimal(Lollipop.no_mans_land_upperBound); // upper bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts

            nUD_PeakWidthBase.Minimum = 0.001m;
            nUD_PeakWidthBase.Maximum = 0.5000m;
            nUD_PeakWidthBase.Value = Convert.ToDecimal(Lollipop.peak_width_base_et); // bin size used for including individual ET pairs in one 'Peak Center Mass' and peak with for one ET peak

            nUD_PeakCountMinThreshold.Minimum = 0;
            nUD_PeakCountMinThreshold.Maximum = 1000;
            nUD_PeakCountMinThreshold.Value = Convert.ToDecimal(Lollipop.min_peak_count_et); // ET pairs with [Peak Center Count] AND ET peaks with [Peak Count] above this value are considered acceptable for use in proteoform family. this will be eventually set following ED analysis.
        }

        private void nUD_ET_Lower_Bound_ValueChanged(object sender, EventArgs e) // maximum delta mass for theoretical proteoform that has mass LOWER than the experimental protoform mass
        {
            Lollipop.et_low_mass_difference = Convert.ToDouble(nUD_ET_Lower_Bound.Value);
        }
        private void nUD_ET_Upper_Bound_ValueChanged(object sender, EventArgs e) // maximum delta mass for theoretical proteoform that has mass HIGHER than the experimental protoform mass
        {
            Lollipop.et_high_mass_difference = Convert.ToDouble(nUD_ET_Upper_Bound.Value);
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

        // bound for the range of decimal values that is impossible to achieve chemically. these would be artifacts
        private void nUD_NoManLower_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.no_mans_land_lowerBound = Convert.ToDouble(nUD_NoManLower.Value);
        }
        private void nUD_NoManUpper_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.no_mans_land_upperBound = Convert.ToDouble(nUD_NoManUpper.Value);
        }
        // bin size used for including individual ET pairs in one 'Peak Center Mass' and peak with for one ET peak
        private void nUD_PeakWidthBase_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.peak_width_base_et = Convert.ToDouble(nUD_PeakWidthBase.Value);
        }
        // ET pairs with [Peak Center Count] AND ET peaks with [Peak Count] above this value are considered acceptable for use in proteoform family. this will be eventually set following ED analysis.
        private void nUD_PeakCountMinThreshold_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.min_peak_count_et = Convert.ToDouble(nUD_PeakCountMinThreshold.Value);
            if (compared_et)
            { 
                Parallel.ForEach(Lollipop.et_peaks, p =>
                    p.peak_accepted = p.peak_relation_group_count >= Lollipop.min_peak_count_et);
                dgv_ET_Pairs.Refresh();
                dgv_ET_Peak_List.Refresh();
            }
            ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            StripLine lowerCountBound_stripline = new StripLine() { BorderColor = Color.Red, IntervalOffset = Lollipop.min_peak_count_et };
            ct_ET_Histogram.ChartAreas[0].AxisY.StripLines.Add(lowerCountBound_stripline);
            this.updateFiguresOfMerit();
        }

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
            if (dgv_ET_Peak_List.IsCurrentCellDirty)
            {
                dgv_ET_Peak_List.CommitEdit(DataGridViewDataErrorContexts.Commit);

                int columnIndex = dgv_ET_Peak_List.CurrentCell.ColumnIndex;
                int rowIndex = dgv_ET_Peak_List.CurrentCell.RowIndex;

                if (columnIndex < 0) return;
                string columnName = dgv_ET_Peak_List.Columns[columnIndex].Name;

                if (columnName == "peak_accepted")
                {
                    bool acceptibilityStatus = Convert.ToBoolean(dgv_ET_Peak_List.Rows[rowIndex].Cells[columnIndex].Value);
                    DeltaMassPeak selected_peak = (DeltaMassPeak)this.dgv_ET_Peak_List.Rows[rowIndex].DataBoundItem;
                    ETPeakAcceptabilityChangedEventArgs ETAcceptabilityChangedEventData = new ETPeakAcceptabilityChangedEventArgs(acceptibilityStatus, selected_peak);
                    ONETAcceptibilityChanged(ETAcceptabilityChangedEventData);
                }
            }
        }
    }

    public class ETPeakAcceptabilityChangedEventArgs : EventArgs
    {
        private bool _isPeakAcceptable;
        public bool IsPeakAcceptable
        {
            get
            {
                return this._isPeakAcceptable;
            }
        }

        private DeltaMassPeak _ETPeak;
        public DeltaMassPeak ETPeak
        {
            get
            {
                return this._ETPeak;
            }
        }

        public ETPeakAcceptabilityChangedEventArgs(bool IsPeakAcceptable, DeltaMassPeak ETPeak)
        {
            this._isPeakAcceptable = IsPeakAcceptable; //True if peak is acceptable
            this._ETPeak = ETPeak;
        }
    }

    public delegate void ETPeakAcceptabilityChangedEventHandler(object sender, ETPeakAcceptabilityChangedEventArgs e);

}

