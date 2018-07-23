using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;
using ProteoformSuiteInternal;
using System.Threading.Tasks;
namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for NeuCodePairs.xaml
    /// </summary>
    public partial class NeuCodePairs : UserControl, ISweetForm,ITabbedMDI
    {
        #region Public Constructor
        public NeuCodePairs()
        {
            InitializeComponent();
            
            //chart1 init
            this.ct_LysineCount.Cursor = System.Windows.Forms.Cursors.No;

            //InitializeParameterSet();
        }
        #endregion Public Constructor

        #region Public Property

        public List<DataTable> DataTables { get; private set; }

        public ProteoformSweet MDIParent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        #endregion Public Property


        public void ClearListsTablesFigures(bool clear_following_forms)
        {
            throw new NotImplementedException();
        }

        public void FillTablesAndCharts()
        {
            throw new NotImplementedException();
        }

        public void InitializeParameterSet()
        {
            throw new NotImplementedException();
        }

        public bool ReadyToRunTheGamut()
        {
            throw new NotImplementedException();
        }

        public void RunTheGamut(bool full_run)
        {
            throw new NotImplementedException();
        }

        public List<DataTable> SetTables()
        {
            throw new NotImplementedException();
        }

        #region Private Methods
        private void IRatMinAcceptable_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_intensity_ratio = IRatMinAcceptable.Value;
            Parallel.ForEach(Sweet.lollipop.raw_neucode_pairs, p => p.set_accepted());
            dgv_RawExpNeuCodePairs.Refresh();
            this.MDIParent.aggregatedProteoforms.ClearListsTablesFigures(true);
        }
        private void yMinIRat_ValueChanged(object sender, EventArgs e)
        {
            ct_IntensityRatio.ChartAreas[0].AxisY.Minimum = double.Parse(yMinIRat.Value.ToString());
        }

        private void yMaxIRat_ValueChanged(object sender, EventArgs e)
        {
            ct_IntensityRatio.ChartAreas[0].AxisY.Maximum = double.Parse(yMaxIRat.Value.ToString());
        }

        private void xMaxIRat_ValueChanged(object sender, EventArgs e)
        {
            ct_IntensityRatio.ChartAreas[0].AxisX.Maximum = double.Parse(xMaxIRat.Value.ToString());
        }

        private void xMinIRat_ValueChanged(object sender, EventArgs e)
        {
            ct_IntensityRatio.ChartAreas[0].AxisX.Maximum = double.Parse(xMaxIRat.Value.ToString());
        }

        private void IRatMaxAcceptable_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.max_intensity_ratio = IRatMaxAcceptable.Value;
            Parallel.ForEach(Sweet.lollipop.raw_neucode_pairs, p => p.set_accepted());
            dgv_RawExpNeuCodePairs.Refresh();
            this.MDIParent.aggregatedProteoforms.ClearListsTablesFigures(true);
        }

        private void xMaxKCt_ValueChanged(object sender, EventArgs e)
        {
            ct_LysineCount.ChartAreas[0].AxisX.Maximum = double.Parse(xMaxKCt.Value.ToString());
        }

        private void xMinKCt_ValueChanged(object sender, EventArgs e)
        {
            ct_LysineCount.ChartAreas[0].AxisX.Minimum = double.Parse(xMinKCt.Value.ToString());
        }

        private void yMaxKCt_ValueChanged(object sender, EventArgs e)
        {
            ct_LysineCount.ChartAreas[0].AxisY.Maximum = double.Parse(yMaxKCt.Value.ToString());
        }

        private void yMinKCt_ValueChanged(object sender, EventArgs e)
        {
            ct_LysineCount.ChartAreas[0].AxisY.Minimum = double.Parse(yMinKCt.Value.ToString());
        }

        private void KMinAcceptable_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_lysine_ct = KMinAcceptable.Value;
            Parallel.ForEach(Sweet.lollipop.raw_neucode_pairs, p => p.set_accepted());
            dgv_RawExpNeuCodePairs.Refresh();
            this.MDIParent.aggregatedProteoforms.ClearListsTablesFigures(true);
        }

        private void KMaxAcceptable_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.max_lysine_ct = KMaxAcceptable.Value;
            Parallel.ForEach(Sweet.lollipop.raw_neucode_pairs, p => p.set_accepted());
            dgv_RawExpNeuCodePairs.Refresh();
            this.MDIParent.aggregatedProteoforms.ClearListsTablesFigures(true);
        }
        #endregion Private Methods

        System.Drawing.Point? ct_intensityRatio_prevPosition = null;
        System.Windows.Forms.ToolTip ct_intensityRatio_tt = new System.Windows.Forms.ToolTip();
        void ct_IntensityRatio_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                DisplayUtility.tooltip_graph_display(ct_intensityRatio_tt, e, this.ct_IntensityRatio, ct_intensityRatio_prevPosition);
            }
        }

        System.Drawing.Point? ct_LysineCount_prevPosition = null;
        System.Windows.Forms.ToolTip ct_LysineCount_tt = new System.Windows.Forms.ToolTip();
        

        private void ct_LysineCount_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                
                DisplayUtility.tooltip_graph_display(ct_LysineCount_tt, e, ct_LysineCount, ct_LysineCount_prevPosition);
            }
        }
    }
}
