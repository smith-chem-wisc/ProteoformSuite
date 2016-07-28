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
        public ExperimentExperimentComparison()
        {
            InitializeComponent();
            this.dgv_EE_Peaks.MouseClick += new MouseEventHandler(dgv_EE_Peak_List_CellClick);
            this.ct_EE_Histogram.MouseMove += new MouseEventHandler(ct_EE_Histogram_MouseMove);
            this.ct_EE_peakList.MouseMove += new MouseEventHandler(ct_EE_peakList_MouseMove);
            dgv_EE_Peaks.CurrentCellDirtyStateChanged += new EventHandler(peakListSpecificPeakAcceptanceChanged); //makes the change immediate and automatic
            dgv_EE_Peaks.CellValueChanged += new DataGridViewCellEventHandler(propagatePeakListAcceptedPeakChangeToPairsTable); //when 'acceptance' of an ET peak gets changed, we change the ET pairs table.
        }

        bool initial_load = true;
        public void ExperimentExperimentComparison_Load(object sender, EventArgs e)
        {
            InitializeParameterSet();
            if (Lollipop.ee_relations.Count == 0) Lollipop.make_ee_relationships();
            this.FillTablesAndCharts();
            initial_load = false;
        }

        public void FillTablesAndCharts()
        {
            FillEEPeakListTable();
            FillEEPairsGridView();
            FormatEERelationsGridView();
            FormatEEPeakListGridView();
            GraphEERelations();
            GraphEEPeaks();
            updateFiguresOfMerit();
        }

        private void RunTheGamut()
        {
            this.Cursor = Cursors.WaitCursor;
            ClearListsAndTables();
            Lollipop.make_ee_relationships();
            this.FillTablesAndCharts();
            this.Cursor = Cursors.Default;
        }

        private void ClearListsAndTables()
        {
            Lollipop.ee_relations.Clear();
            Lollipop.ee_peaks.Clear();
            Lollipop.ef_relations.Clear();
            Lollipop.proteoform_community.relations_in_peaks.Clear();
            Lollipop.proteoform_community.delta_mass_peaks.Clear();

            dgv_EE_Relations.DataSource = null;
            dgv_EE_Peaks.DataSource = null;
            dgv_EE_Relations.Rows.Clear();
            dgv_EE_Peaks.Rows.Clear();
        }
        
        private void updateFiguresOfMerit()
        {
            List<DeltaMassPeak> big_peaks = Lollipop.ee_peaks.Where(p => p.peak_accepted).ToList();
            tb_IdentifiedProteoforms.Text = big_peaks.Select(p => p.mass_difference_group.Count).Sum().ToString();
            tb_TotalPeaks.Text = big_peaks.Count.ToString();
        }

        private void FillEEPairsGridView()
        {
            DisplayUtility.FillDataGridView(dgv_EE_Relations, Lollipop.ee_relations);
        }
        private void FillEEPeakListTable()
        {
            DisplayUtility.FillDataGridView(dgv_EE_Peaks, Lollipop.ee_peaks);
        }
        private void GraphEERelations()
        {
            DisplayUtility.GraphRelationsChart(ct_EE_Histogram, Lollipop.ee_relations, "relations");
        }
        private void GraphEEPeaks()
        {
            DisplayUtility.GraphDeltaMassPeaks(ct_EE_peakList, Lollipop.ee_peaks, Lollipop.ee_relations);
        }

        private void dgv_EE_Peak_List_CellClick(object sender, MouseEventArgs e)
        {
            int clickedRow = dgv_EE_Peaks.HitTest(e.X, e.Y).RowIndex;
            if (e.Button == MouseButtons.Left && clickedRow >= 0 && clickedRow < Lollipop.ee_relations.Count)
            {
                ct_EE_peakList.ChartAreas[0].AxisX.StripLines.Clear();
                DeltaMassPeak selected_peak = (DeltaMassPeak)this.dgv_EE_Peaks.Rows[clickedRow].DataBoundItem;
                DisplayUtility.GraphSelectedDeltaMassPeak(ct_EE_peakList, selected_peak);
                ct_EE_peakList.ChartAreas[0].AxisY.Maximum = Convert.ToInt32(selected_peak.group_count * 1.2); //this automatically scales the vertical axis to the peak height plus 20%
            }
        }

        private void FormatEERelationsGridView()
        {
            //round table values
            dgv_EE_Relations.Columns["group_adjusted_deltaM"].DefaultCellStyle.Format = "0.#####";
            dgv_EE_Relations.Columns["proteoform_mass_1"].DefaultCellStyle.Format = "0.#####";
            dgv_EE_Relations.Columns["proteoform_mass_2"].DefaultCellStyle.Format = "0.#####";
            dgv_EE_Relations.Columns["agg_intensity_1"].DefaultCellStyle.Format = "0.##";
            dgv_EE_Relations.Columns["agg_intensity_2"].DefaultCellStyle.Format = "0.##";
            dgv_EE_Relations.Columns["agg_RT_1"].DefaultCellStyle.Format = "0.##";
            dgv_EE_Relations.Columns["agg_RT_2"].DefaultCellStyle.Format = "0.##";
            dgv_EE_Relations.Columns["delta_mass"].DefaultCellStyle.Format = "0.#####";


            //set column header
            dgv_EE_Relations.Columns["group_adjusted_deltaM"].HeaderText = "Peak Center Delta Mass";
            dgv_EE_Relations.Columns["group_count"].HeaderText = "Peak Center Count";
            dgv_EE_Relations.Columns["proteoform_mass_1"].HeaderText = "Heavy Experimental Aggregated Proteoform Mass";
            dgv_EE_Relations.Columns["proteoform_mass_2"].HeaderText = "Light Experimental Aggregarted Proteoform Mass";
            dgv_EE_Relations.Columns["agg_intensity_1"].HeaderText = "Heavy Experimental Aggregated Intensity";
            dgv_EE_Relations.Columns["agg_intensity_2"].HeaderText = "Light Experimental Aggregated Intensity";
            dgv_EE_Relations.Columns["agg_RT_1"].HeaderText = "Heavy Experimental Aggregated RT";
            dgv_EE_Relations.Columns["agg_RT_2"].HeaderText = "Light Experimental Aggregated RT";
            dgv_EE_Relations.Columns["lysine_count"].HeaderText = "Lysine Count";
            dgv_EE_Relations.Columns["num_observations_1"].HeaderText = "Number Heavy Experimental Observations";
            dgv_EE_Relations.Columns["num_observations_2"].HeaderText = "Number Light Experimental Observations";
            dgv_EE_Relations.Columns["delta_mass"].HeaderText = "Delta Mass";

            dgv_EE_Relations.Columns["accepted"].DisplayIndex = 19;
            dgv_EE_Relations.Columns["delta_mass"].DisplayIndex = 17; //column ordering
            dgv_EE_Relations.Columns["unadjusted_group_count"].HeaderText = "Unadjusted Group Count";
            dgv_EE_Relations.Columns["outside_no_mans_land"].HeaderText = "Outside No Man's Land";
            dgv_EE_Relations.Columns["accepted"].HeaderText = "Accepted";


            //making these columns invisible
            dgv_EE_Relations.Columns["accession"].Visible = false;
            dgv_EE_Relations.Columns["peak"].Visible = false;
            dgv_EE_Relations.Columns["name"].Visible = false;
            dgv_EE_Relations.Columns["fragment"].Visible = false;
            dgv_EE_Relations.Columns["ptm_list"].Visible = false;
            if (!Lollipop.neucode_labeled) { dgv_EE_Relations.Columns["lysine_count"].Visible = false; }

            dgv_EE_Relations.AllowUserToAddRows = false;
        }

        private void InitializeParameterSet()
        {
            nUD_EE_Upper_Bound.Minimum = 0;
            nUD_EE_Upper_Bound.Maximum = 500;
            nUD_EE_Upper_Bound.Value = (decimal)Lollipop.ee_max_mass_difference; // maximum mass difference in Da allowed between experimental pairs

            yMaxEE.Minimum = 0;
            yMaxEE.Maximum = 1000;
            yMaxEE.Value = 100; // scaling for y-axis maximum in the histogram of all EE pairs

            yMinEE.Minimum = -100;
            yMinEE.Maximum = yMaxEE.Maximum;
            yMinEE.Value = 0; // scaling for y-axis minimum in the histogram of all EE pairs

            xMaxEE.Minimum = xMinEE.Value;
            xMaxEE.Maximum = 500;
            xMaxEE.Value = (decimal)Lollipop.ee_max_mass_difference; // scaling for x-axis maximum in the histogram of all EE pairs

            xMinEE.Minimum = 0;
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
            nUD_PeakWidthBase.Value = Convert.ToDecimal(Lollipop.peak_width_base);

            nUD_PeakCountMinThreshold.Minimum = 0;
            nUD_PeakCountMinThreshold.Maximum = 1000;
            nUD_PeakCountMinThreshold.Value = Convert.ToDecimal(Lollipop.min_peak_count);
        }

        private void propagatePeakListAcceptedPeakChangeToPairsTable(object sender, DataGridViewCellEventArgs e)
        {
            updateFiguresOfMerit();
        }

        private void FormatEEPeakListGridView()
        {
            //making all columns invisible first - faster
            foreach (DataGridViewColumn column in dgv_EE_Peaks.Columns) { column.Visible = false; }

            dgv_EE_Peaks.Columns["group_count"].Visible = true;
            dgv_EE_Peaks.Columns["group_adjusted_deltaM"].Visible = true;
            dgv_EE_Peaks.Columns["peak_accepted"].Visible = true;
            dgv_EE_Peaks.Columns["possiblePeakAssignments_string"].Visible = true;

            dgv_EE_Peaks.Columns["group_count"].HeaderText = "Peak Center Count";
            dgv_EE_Peaks.Columns["group_adjusted_deltaM"].HeaderText = "Peak Center Delta Mass";
            dgv_EE_Peaks.Columns["peak_accepted"].HeaderText = "Peak Accepted";
            dgv_EE_Peaks.Columns["possiblePeakAssignments_string"].HeaderText = "Peak Assignment";

            dgv_EE_Peaks.AllowUserToAddRows = false;
        }

        private void peakListSpecificPeakAcceptanceChanged(object sender, EventArgs e)
        {
            if (dgv_EE_Peaks.IsCurrentCellDirty)
            {
                dgv_EE_Peaks.EndEdit();
                dgv_EE_Peaks.Update();
            }
        }

        private void xMaxEE_ValueChanged(object sender, EventArgs e) // scaling for x-axis maximum in the histogram of all EE pairs
        {
            if (!initial_load)
            {
                double newXMaxEE = double.Parse(xMaxEE.Value.ToString());
                if (newXMaxEE > double.Parse(xMinEE.Value.ToString()))
                {
                    ct_EE_Histogram.ChartAreas[0].AxisX.Maximum = newXMaxEE;
                }
            }
        }

        private void yMaxEE_ValueChanged(object sender, EventArgs e) // scaling for y-axis maximum in the histogram of all EE pairs
        {
            if (!initial_load)
            {
                double newYMaxEE = double.Parse(yMaxEE.Value.ToString());
                if (newYMaxEE > double.Parse(yMinEE.Value.ToString()))
                {
                    ct_EE_Histogram.ChartAreas[0].AxisY.Maximum = double.Parse(yMaxEE.Value.ToString());
                }
            }
        }

        private void yMinEE_ValueChanged(object sender, EventArgs e) // scaling for y-axis minimum in the histogram of all EE pairs
        {
            if (!initial_load)
            {
                double newYMinEE = double.Parse(yMinEE.Value.ToString());
                if (newYMinEE < double.Parse(yMaxEE.Value.ToString())) ct_EE_Histogram.ChartAreas[0].AxisY.Minimum = double.Parse(yMinEE.Value.ToString());
            }
        }

        private void xMinEE_ValueChanged(object sender, EventArgs e) // scaling for x-axis maximum in the histogram of all EE pairs
        {
            if (!initial_load)
            {
                double newXMinEE = double.Parse(xMinEE.Value.ToString());
                if (newXMinEE < double.Parse(xMaxEE.Value.ToString())) ct_EE_Histogram.ChartAreas[0].AxisX.Minimum = newXMinEE;
            }
        }


        private void cb_Graph_lowerThreshold_CheckedChanged(object sender, EventArgs e)
        {
            if (!initial_load)
            {
                if (cb_Graph_lowerThreshold.Checked)
                    ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Add(new StripLine() { BorderColor = Color.Red, IntervalOffset = Convert.ToDouble(nUD_PeakCountMinThreshold.Value) });
                else if (!cb_Graph_lowerThreshold.Checked) ct_EE_Histogram.ChartAreas[0].AxisY.StripLines.Clear();
            }
        }

        Point? ct_EE_Histogram_prevPosition = null;
        Point? ct_EE_peakList_prevPosition = null;
        ToolTip ct_EE_Histogram_tt = new ToolTip();
        ToolTip ct_EE_peakList_tt = new ToolTip();
        private void ct_EE_Histogram_MouseMove(object sender, MouseEventArgs e)
        {
            DisplayUtility.tooltip_graph_display(ct_EE_peakList_tt, e, ct_EE_Histogram, ct_EE_Histogram_prevPosition);
        }
        private void ct_EE_peakList_MouseMove(object sender, MouseEventArgs e)
        {
            DisplayUtility.tooltip_graph_display(ct_EE_peakList_tt, e, ct_EE_peakList, ct_EE_peakList_prevPosition);
        }

        private void EE_update_Click(object sender, EventArgs e)
        {
            RunTheGamut();
            xMaxEE.Value = (decimal)Lollipop.ee_max_mass_difference;
        }

        private void nUD_EE_Upper_Bound_ValueChanged(object sender, EventArgs e)
        {
            if (!initial_load)
            {
                Lollipop.ee_max_mass_difference = Convert.ToDouble(nUD_EE_Upper_Bound.Value);
                //RunTheGamut();
            }
        }

        private void nUD_PeakWidthBase_ValueChanged(object sender, EventArgs e)
        {
            if (!initial_load)
            {
                Lollipop.peak_width_base = Convert.ToDouble(nUD_PeakWidthBase.Value);
               // RunTheGamut();
            }
        }

        private void nUD_PeakCountMinThreshold_ValueChanged(object sender, EventArgs e)
        {
            if (!initial_load)
            {
                Lollipop.min_peak_count = Convert.ToDouble(nUD_PeakCountMinThreshold.Value);
               // RunTheGamut();
            }
        }

        private void nUD_NoManLower_ValueChanged(object sender, EventArgs e)
        {
            if (!initial_load)
            {
                Lollipop.no_mans_land_lowerBound = Convert.ToDouble(nUD_NoManLower.Value);
               // RunTheGamut();
            }
        }

        private void nUD_NoManUpper_ValueChanged(object sender, EventArgs e)
        {
            if (!initial_load)
            {
                Lollipop.no_mans_land_upperBound = Convert.ToDouble(nUD_NoManUpper.Value); 
               // RunTheGamut();
            }
        }
    }
}