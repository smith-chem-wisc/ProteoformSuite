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
            if (Lollipop.et_relations.Count == 0)
            {
                Lollipop.make_et_relationships();
                this.FillTablesAndCharts();
            }
            initial_load = false;
        }

        private void RunTheGamut()
        {
            this.Cursor = Cursors.WaitCursor;
            ClearListsAndTables();
            Lollipop.make_et_relationships();
            this.FillTablesAndCharts();
            this.Cursor = Cursors.Default;
        }

        public void FillTablesAndCharts()
        {
            FillETPeakListTable();
            FillETRelationsGridView();
            DisplayUtility.FormatRelationsGridView(dgv_ET_Pairs, true, false);
            DisplayUtility.FormatPeakListGridView(dgv_ET_Peak_List);
            GraphETRelations();
            GraphETPeaks();
            updateFiguresOfMerit();       
        }

        private void ClearListsAndTables()
        {
            Lollipop.et_relations.Clear();
            Lollipop.et_peaks.Clear();
            Lollipop.ed_relations.Clear();
            Lollipop.proteoform_community.relations_in_peaks.Clear();
            Lollipop.proteoform_community.delta_mass_peaks.Clear();

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
            DisplayUtility.GraphDeltaMassPeaks(ct_ET_peakList, Lollipop.et_peaks, "Peak Count", "Median Decoy Count", Lollipop.et_relations, "Nearby Relations");
        }

        private void dgv_ET_Peak_List_CellClick(object sender, MouseEventArgs e)
        {
            int clickedRow = dgv_ET_Peak_List.HitTest(e.X, e.Y).RowIndex;
            if (e.Button == MouseButtons.Left && clickedRow >= 0 && clickedRow < Lollipop.et_relations.Count)
            {
                ct_ET_peakList.ChartAreas[0].AxisX.StripLines.Clear();
                DeltaMassPeak selected_peak = (DeltaMassPeak)this.dgv_ET_Peak_List.Rows[clickedRow].DataBoundItem;
                DisplayUtility.GraphSelectedDeltaMassPeak(ct_ET_peakList, selected_peak, Lollipop.et_relations);
            }
            else
            {
                if(e.Button == MouseButtons.Right && clickedRow >= 0 && clickedRow < Lollipop.et_relations.Count)
                {
                    ContextMenuStrip ET_peak_List_Menu = new ContextMenuStrip();
                    int position_xy_mouse_row = dgv_ET_Peak_List.HitTest(e.X, e.Y).RowIndex;

                    DeltaMassPeak selected_peak = (DeltaMassPeak)this.dgv_ET_Peak_List.Rows[clickedRow].DataBoundItem;

                    if (position_xy_mouse_row > 0)
                    {
                        ET_peak_List_Menu.Items.Add("Increase Experimenal Mass 1.0015 Da").Name = "IncreaseMass";
                        ET_peak_List_Menu.Items.Add("Decrease Experimenal Mass 1.0015 Da").Name = "DecreaseMass";
                    }
                    ET_peak_List_Menu.Show(dgv_ET_Peak_List, new Point(e.X, e.Y));

                    //event menu click
                    ET_peak_List_Menu.ItemClicked += new ToolStripItemClickedEventHandler((s,ev)=> ET_peak_List_Menu_ItemClicked(s,ev,selected_peak));
                }
            }
        }
        
        void ET_peak_List_Menu_ItemClicked(object sender,ToolStripItemClickedEventArgs e, DeltaMassPeak peak)
        {
            switch (e.ClickedItem.Name.ToString())
            {
                case "IncreaseMass":
                    massShifter(peak, 1);
                    break;
                case "DecreaseMass":
                    massShifter(peak, -1);
                    break;
            }
        }

        private void massShifter(DeltaMassPeak peak, int shift)
        {
            List<ExperimentalProteoform> expProtList = new List<ExperimentalProteoform>();
            foreach (ProteoformRelation relation in Lollipop.et_relations.Where(p => p.peak == peak).ToList())
            {
                if (relation.connected_proteoforms[0] is ExperimentalProteoform)
                {
                    expProtList.Add(relation.connected_proteoforms[0] as ExperimentalProteoform);
                }
            }

            foreach (ExperimentalProteoform ep in Lollipop.proteoform_community.experimental_proteoforms.Where(ex => expProtList.Contains(ex)))
            {
                List<ProteoformSuiteInternal.Component> epComponents = new List<ProteoformSuiteInternal.Component>();

                foreach (ProteoformSuiteInternal.Component comp in ep.aggregated_components)
                {
                    epComponents.Add(comp);
                }

                if (Lollipop.neucode_labeled)
                {
                    List<ProteoformSuiteInternal.Component> neuCodeComponents = new List<ProteoformSuiteInternal.Component>();

                    foreach (ProteoformSuiteInternal.Component rawComponent in Lollipop.raw_neucode_pairs.Where(p => epComponents.Contains(p)).Select(p => p.neuCodeLight))
                    {
                        neuCodeComponents.Add(rawComponent);
                    }

                    foreach (ProteoformSuiteInternal.Component rawComponent in Lollipop.raw_neucode_pairs.Where(p => epComponents.Contains(p)).Select(p => p.neuCodeHeavy))
                    {
                        neuCodeComponents.Add(rawComponent);
                    }

                    foreach (ProteoformSuiteInternal.Component rawComponent in Lollipop.raw_experimental_components.Where(cp => neuCodeComponents.Contains(cp)).ToList())
                    {
                        rawComponent.manual_mass_shift = rawComponent.manual_mass_shift + (shift * Lollipop.MONOISOTOPIC_UNIT_MASS);
                    }
                }
                else //unlabeled
                {
                    foreach (ProteoformSuiteInternal.Component rawComponent in Lollipop.raw_experimental_components.Where(p => epComponents.Contains(p)).ToList())
                    {
                        rawComponent.manual_mass_shift = rawComponent.manual_mass_shift + (shift * Lollipop.MONOISOTOPIC_UNIT_MASS);
                    }
                }
            }

            Lollipop.raw_neucode_pairs.Clear();
            HashSet<string> scan_ranges = new HashSet<string>(Lollipop.raw_experimental_components.Select(c => c.scan_range));
            foreach (string scan_range in scan_ranges)
                Lollipop.find_neucode_pairs(Lollipop.raw_experimental_components.Where(c => c.scan_range == scan_range));

            Lollipop.aggregate_proteoforms();
            RunTheGamut();
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
            updateFiguresOfMerit(); //I'm not sure if this is necessary here.

            //boolean accepted in proteoform relation must change in response to DeltaMassPeak change.
        }

        private void peakListSpecificPeakAcceptanceChanged(object sender, EventArgs e)
        {
            if (dgv_ET_Peak_List.IsCurrentCellDirty)
            {
                dgv_ET_Peak_List.EndEdit();
                dgv_ET_Peak_List.Update();
            }
        }

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
                //RunTheGamut();
            }          
        }
        private void nUD_ET_Upper_Bound_ValueChanged(object sender, EventArgs e) // maximum delta mass for theoretical proteoform that has mass HIGHER than the experimental protoform mass
        {
            if (!initial_load)
            {
                Lollipop.et_high_mass_difference = Convert.ToDouble(nUD_ET_Upper_Bound.Value);
               // RunTheGamut();
            }
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
            if (!initial_load) Lollipop.no_mans_land_lowerBound = Convert.ToDouble(nUD_NoManLower.Value);          
        }
        private void nUD_NoManUpper_ValueChanged(object sender, EventArgs e)
        {
            if (!initial_load) Lollipop.no_mans_land_upperBound = Convert.ToDouble(nUD_NoManUpper.Value);     
        }
        // bin size used for including individual ET pairs in one 'Peak Center Mass' and peak with for one ET peak
        private void nUD_PeakWidthBase_ValueChanged(object sender, EventArgs e) 
        {
            if (!initial_load) Lollipop.peak_width_base = Convert.ToDouble(nUD_PeakWidthBase.Value);          
        }
        // ET pairs with [Peak Center Count] AND ET peaks with [Peak Count] above this value are considered acceptable for use in proteoform family. this will be eventually set following ED analysis.
        private void nUD_PeakCountMinThreshold_ValueChanged(object sender, EventArgs e) 
        {     
            if (!initial_load) Lollipop.min_peak_count = Convert.ToDouble(nUD_PeakCountMinThreshold.Value);  
        }

        private void ET_update_Click(object sender, EventArgs e)
        {
            if (!initial_load) RunTheGamut();
            xMaxET.Value = (decimal)Lollipop.et_high_mass_difference;
            xMinET.Value = (decimal)Lollipop.et_low_mass_difference;
        }
    }
}
