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
        bool initial_load = true;

        public ExperimentTheoreticalComparison()
        {
            InitializeComponent();
            this.dgv_ET_Peak_List.MouseClick += new MouseEventHandler(dgv_ET_Peak_List_CellClick);
            this.ct_ET_Histogram.MouseMove += new MouseEventHandler(ct_ET_Histogram_MouseMove);
            this.ct_ET_peakList.MouseMove += new MouseEventHandler(ct_ET_peakList_MouseMove);
            dgv_ET_Peak_List.CurrentCellDirtyStateChanged += new EventHandler(peakListSpecificPeakAcceptanceChanged); //makes the change immediate and automatic
            dgv_ET_Peak_List.CellValueChanged += new DataGridViewCellEventHandler(propagatePeakListAcceptedPeakChangeToPairsTable); //when 'acceptance' of an ET peak gets changed, we change the ET pairs table.
        }

        
        public void ExperimentTheoreticalComparison_Load(object sender, EventArgs e)
        {
            InitializeParameterSet();
            if (Lollipop.et_relations.Count == 0) Lollipop.make_et_relationships();
            this.FillTablesAndGraphs();
            initial_load = false;
        }

        private void RunTheGamut()
        {
            this.Cursor = Cursors.WaitCursor;
            ClearListsTablesAndGraphs();
            Lollipop.make_et_relationships();
            this.FillTablesAndGraphs();
            this.Cursor = Cursors.Default;
        }

        public void FillTablesAndGraphs()
        {
            FillETPeakListTable();
            FillETRelationsGridView();
            FormatETRelationsGridView();
            FormatETPeakListGridView();
            GraphETRelations();
            GraphETPeaks();

            List<DeltaMassPeak> big_peaks = Lollipop.et_peaks.Where(p => p.accepted).ToList();
            tb_IdentifiedProteoforms.Text = big_peaks.Select(p => p.mass_difference_group.Count).Sum().ToString();
            tb_TotalPeaks.Text = big_peaks.Count.ToString();
        }

        private void ClearListsTablesAndGraphs()
        {
            Lollipop.et_relations.Clear();
            Lollipop.et_peaks.Clear();
            Lollipop.edList.Clear();
            Lollipop.ed_relations.Clear();
            Lollipop.proteoform_community.relations_in_peaks.Clear();
            Lollipop.proteoform_community.delta_mass_peaks.Clear();

            dgv_ET_Pairs.DataSource = null;
            dgv_ET_Peak_List.DataSource = null;
            dgv_ET_Pairs.Rows.Clear();
            dgv_ET_Peak_List.Rows.Clear();
        }

        private void FillETRelationsGridView()
        {
            DisplayUtility.FillDataGridView(dgv_ET_Pairs, Lollipop.et_relations);
        }
        private void FillETPeakListTable()
        {
            DisplayUtility.FillDataGridView(dgv_ET_Peak_List, Lollipop.et_peaks);
        }
        private void GraphETRelations()
        {
            DisplayUtility.GraphRelationsChart(ct_ET_Histogram, Lollipop.et_relations, "relations");
        }
        private void GraphETPeaks()
        {
            DisplayUtility.GraphDeltaMassPeaks(ct_ET_peakList, Lollipop.et_peaks, Lollipop.et_relations);

        }
        private void dgv_ET_Peak_List_CellClick(object sender, MouseEventArgs e)
        {
            int clickedRow = dgv_ET_Peak_List.HitTest(e.X, e.Y).RowIndex;
            if (e.Button == MouseButtons.Left && clickedRow >= 0 && clickedRow < Lollipop.et_relations.Count)
            {
                ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Clear();
                DeltaMassPeak selected_peak = (DeltaMassPeak)this.dgv_ET_Peak_List.Rows[clickedRow].DataBoundItem;
                DisplayUtility.GraphSelectedDeltaMassPeak(ct_ET_peakList, selected_peak);
            }
        }
        
        private void FormatETRelationsGridView()
        {
            //round table values
            dgv_ET_Pairs.Columns["group_adjusted_deltaM"].DefaultCellStyle.Format = "0.####";
            dgv_ET_Pairs.Columns["proteoform_mass_1"].DefaultCellStyle.Format = "0.####";
            dgv_ET_Pairs.Columns["proteoform_mass_2"].DefaultCellStyle.Format = "0.####";
            dgv_ET_Pairs.Columns["agg_intensity_1"].DefaultCellStyle.Format = "0.##";
            dgv_ET_Pairs.Columns["agg_RT_1"].DefaultCellStyle.Format = "0.##";
            dgv_ET_Pairs.Columns["delta_mass"].DefaultCellStyle.Format = "0.####";

            //set column header
            dgv_ET_Pairs.Columns["group_adjusted_deltaM"].HeaderText = "Peak Center Delta Mass";
            dgv_ET_Pairs.Columns["group_count"].HeaderText = "Peak Center Count";
            dgv_ET_Pairs.Columns["accession"].HeaderText = "Accession";
            dgv_ET_Pairs.Columns["fragment"].HeaderText = "Fragment";
            dgv_ET_Pairs.Columns["ptm_list"].HeaderText = "PTM Description";
            dgv_ET_Pairs.Columns["proteoform_mass_1"].HeaderText = "Experimental Aggregated Proteoform Mass";
            dgv_ET_Pairs.Columns["proteoform_mass_2"].HeaderText = "Theoretical Proteoform Mass";
            dgv_ET_Pairs.Columns["agg_intensity_1"].HeaderText = "Experimental Aggregated Intensity";
            dgv_ET_Pairs.Columns["agg_RT_1"].HeaderText = "Experimental Aggregated RT";
            dgv_ET_Pairs.Columns["lysine_count"].HeaderText = "Lysine Count";
            dgv_ET_Pairs.Columns["num_observations_1"].HeaderText = "Number Experimental Observations";
            dgv_ET_Pairs.Columns["delta_mass"].HeaderText = "Delta Mass";
            dgv_ET_Pairs.Columns["delta_mass"].DisplayIndex = 18;
            dgv_ET_Pairs.Columns["name"].HeaderText = "Name";
            dgv_ET_Pairs.Columns["unadjusted_group_count"].HeaderText = "Unadjusted Group Count";
            dgv_ET_Pairs.Columns["outside_no_mans_land"].HeaderText = "Outside No Man's Land";
            dgv_ET_Pairs.Columns["accepted"].HeaderText = "Accepeted";

            //making these columns invisible
            dgv_ET_Pairs.Columns["peak"].Visible = false;
            dgv_ET_Pairs.Columns["agg_intensity_2"].Visible = false;
            dgv_ET_Pairs.Columns["agg_RT_2"].Visible = false;
            dgv_ET_Pairs.Columns["num_observations_2"].Visible = false;
            if (!Lollipop.neucode_labeled) { dgv_ET_Pairs.Columns["lysine_count"].Visible = false; }

            dgv_ET_Pairs.AllowUserToAddRows = false;
        }

        private void FormatETPeakListGridView()
        {
            //making all columns invisible first - faster
            foreach (DataGridViewColumn column in dgv_ET_Peak_List.Columns) { column.Visible = false; }

            dgv_ET_Peak_List.Columns["group_count"].Visible = true;
            dgv_ET_Peak_List.Columns["group_adjusted_deltaM"].Visible = true;
            dgv_ET_Peak_List.Columns["peak_accepted"].Visible = true;
            dgv_ET_Peak_List.Columns["possiblePeakAssignments_string"].Visible = true;

            dgv_ET_Peak_List.Columns["group_count"].HeaderText = "Peak Center Count";
            dgv_ET_Peak_List.Columns["group_adjusted_deltaM"].HeaderText = "Peak Center Delta Mass";
            dgv_ET_Peak_List.Columns["peak_accepted"].HeaderText = "Peak Accepted";
            dgv_ET_Peak_List.Columns["possiblePeakAssignments_string"].HeaderText = "Peak Assignment";

            dgv_ET_Peak_List.AllowUserToAddRows = false;
        }

        Point? ct_ET_Histogram_prevPosition = null;
        Point? ct_ET_peakList_prevPosition = null;
        ToolTip ct_ET_Histogram_tt = new ToolTip();
        ToolTip ct_ET_peakList_tt = new ToolTip();
        private void ct_ET_Histogram_MouseMove(object sender, MouseEventArgs e)
        {
            DisplayUtility.tooltip_graph_display(ct_ET_peakList_tt, e, ct_ET_Histogram, ct_ET_Histogram_prevPosition);
        }
        private void ct_ET_peakList_MouseMove(object sender, MouseEventArgs e)
        {
            DisplayUtility.tooltip_graph_display(ct_ET_peakList_tt, e, ct_ET_peakList, ct_ET_peakList_prevPosition);
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

        private void InitializeParameterSet()
        {
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

        private void nUD_ET_Lower_Bound_ValueChanged(object sender, EventArgs e) // maximum delta mass for theoretical proteoform that has mass LOWER than the experimental protoform mass
        {
            if (!initial_load)
            {
                Lollipop.et_low_mass_difference = Convert.ToDouble(nUD_ET_Lower_Bound.Value);
                RunTheGamut();
                xMinET.Value = Convert.ToDecimal(nUD_ET_Lower_Bound.Value);
            }          
        }
        private void nUD_ET_Upper_Bound_ValueChanged(object sender, EventArgs e) // maximum delta mass for theoretical proteoform that has mass HIGHER than the experimental protoform mass
        {
            if (!initial_load)
            {
                Lollipop.et_high_mass_difference = Convert.ToDouble(nUD_ET_Upper_Bound.Value);
                RunTheGamut();
                xMaxET.Value = Convert.ToDecimal(nUD_ET_Upper_Bound.Value);
            }
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
            if (!initial_load)
            {
                Lollipop.no_mans_land_lowerBound = Convert.ToDouble(nUD_NoManLower.Value);
                RunTheGamut();
            }              
        }
        private void nUD_NoManUpper_ValueChanged(object sender, EventArgs e)
        {
            if (!initial_load)
            {
                Lollipop.no_mans_land_upperBound = Convert.ToDouble(nUD_NoManUpper.Value);
                RunTheGamut();
            }            
        }

        // bin size used for including individual ET pairs in one 'Peak Center Mass' and peak with for one ET peak
        private void nUD_PeakWidthBase_ValueChanged(object sender, EventArgs e) 
        {
            if (!initial_load)
            {
                Lollipop.peak_width_base = Convert.ToDouble(nUD_PeakWidthBase.Value);
                RunTheGamut();
            }               
        }

        // ET pairs with [Peak Center Count] AND ET peaks with [Peak Count] above this value are considered acceptable for use in proteoform family. this will be eventually set following ED analysis.
        private void nUD_PeakCountMinThreshold_ValueChanged(object sender, EventArgs e) 
        {     
            if (!initial_load)
            {
                Lollipop.min_peak_count = Convert.ToDouble(nUD_PeakCountMinThreshold.Value);
                //GraphETRelations(); //we do this hear because the redline threshold needs to be redrawn
                // FillTablesAndGraphs();
                RunTheGamut(); //changes what pairs are acceptable... should rerun
            }     
        }

        private void ET_update_Click(object sender, EventArgs e)
        {
            if (!initial_load) RunTheGamut();
        }
    }
}
