using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Controls;
using ProteoformSuiteInternal;
using System.Threading.Tasks;
using System.Linq;

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
            InitializeParameterSet();
        }
        #endregion Public Constructor

        #region Public Property

        public List<DataTable> DataTables { get; private set; }
        public ProteoformSweet MDIParent { get; set; }//tracking the mdi parent of current user control

        #endregion Public Property

        #region Public Methods
        public bool ReadyToRunTheGamut()
        {
            throw new NotImplementedException();
        }
        public void RunTheGamut(bool full_run)
        {
            //ClearListsTablesFigures(true); No need to clear tables for graphing and filling tables
            GraphLysineCount();
            GraphIntensityRatio();
            FillNeuCodePairsDGV();
        }
        public void FillTablesAndCharts()
        {
            if (!ReadyToRunTheGamut())
            {
                return;
            }
            GraphLysineCount();
            GraphIntensityRatio();
            FillNeuCodePairsDGV();
        }

        public void ClearListsTablesFigures(bool clear_following)
        {
            foreach (var series in ct_IntensityRatio.Series) { series.Points.Clear(); }
            foreach (var series in ct_LysineCount.Series) { series.Points.Clear(); }

            dgv_RawExpNeuCodePairs.DataSource = null;
            dgv_RawExpNeuCodePairs.Rows.Clear();

            if (clear_following)
            {
                for (int i = this.MDIParent.forms.IndexOf(this) + 1; i < this.MDIParent.forms.Count; i++)
                {
                    ISweetForm sweet = this.MDIParent.forms[i];
                    if (sweet as TheoreticalDatabase == null)
                    {
                        sweet.ClearListsTablesFigures(false);
                    }
                }
            }
        }

        public List<DataTable> SetTables()
        {
            DataTables = new List<DataTable> { DisplayNeuCodePair.FormatNeuCodeTable(Sweet.lollipop.raw_neucode_pairs.Select(x => new DisplayNeuCodePair(x)).ToList(), "NeuCodePairs") };
            return DataTables;
        }

        public void InitializeParameterSet()
        {
            IRatMaxAcceptable.ValueChanged -= IRatMaxAcceptable_ValueChanged;
            IRatMinAcceptable.ValueChanged -= IRatMinAcceptable_ValueChanged;
            KMaxAcceptable.ValueChanged -= KMaxAcceptable_ValueChanged;
            KMinAcceptable.ValueChanged -= KMinAcceptable_ValueChanged;

            IRatMaxAcceptable.Minimum = -20; IRatMaxAcceptable.Maximum = 20; IRatMaxAcceptable.Value = Sweet.lollipop.max_intensity_ratio;
            IRatMinAcceptable.Minimum = -20; IRatMinAcceptable.Maximum = 20; IRatMinAcceptable.Value = Sweet.lollipop.min_intensity_ratio;

            KMaxAcceptable.Minimum = -28; KMaxAcceptable.Maximum = 28; KMaxAcceptable.Value = Sweet.lollipop.max_lysine_ct;
            KMinAcceptable.Minimum = -28; KMinAcceptable.Maximum = 28; KMinAcceptable.Value = Sweet.lollipop.min_lysine_ct;

            IRatMaxAcceptable.ValueChanged += IRatMaxAcceptable_ValueChanged;
            IRatMinAcceptable.ValueChanged += IRatMinAcceptable_ValueChanged;
            KMaxAcceptable.ValueChanged += KMaxAcceptable_ValueChanged;
            KMinAcceptable.ValueChanged += KMinAcceptable_ValueChanged;
        }
        #endregion Public Methods

        #region Private Methods
        private void FillNeuCodePairsDGV()
        {
            DisplayUtility.FillDataGridView(dgv_RawExpNeuCodePairs, Sweet.lollipop.raw_neucode_pairs.Select(n => new DisplayNeuCodePair(n)));
            DisplayNeuCodePair.FormatNeuCodeTable(dgv_RawExpNeuCodePairs);
        }
        private void GraphIntensityRatio()
        {
            DataTable intensityRatioHistogram = new DataTable();
            intensityRatioHistogram.Columns.Add("intRatio", typeof(double));
            intensityRatioHistogram.Columns.Add("numPairsAtThisIntRatio", typeof(int));

            int ymax = 0;
            for (double i = 0; i <= 20; i = i + 0.05)
            {
                List<NeuCodePair> proteoforms_by_intensityRatio = Sweet.lollipop.raw_neucode_pairs.Where(p => p.intensity_ratio >= i - 0.025 && p.intensity_ratio < i + 0.025).ToList();
                if (proteoforms_by_intensityRatio.Count > ymax)
                {
                    ymax = proteoforms_by_intensityRatio.Count;
                }
                intensityRatioHistogram.Rows.Add(i, proteoforms_by_intensityRatio.Count);
            }
            ct_IntensityRatio.DataSource = intensityRatioHistogram;
            ct_IntensityRatio.DataBind();

            ct_IntensityRatio.Series["intensityRatio"].XValueMember = "intRatio";
            ct_IntensityRatio.Series["intensityRatio"].YValueMembers = "numPairsAtThisIntRatio";

            yMaxIRat.Minimum = -ymax; yMaxIRat.Maximum = ymax; yMaxIRat.Value = ymax;
            yMinIRat.Minimum = -ymax; yMinIRat.Maximum = ymax; yMinIRat.Value = 0;
            xMaxIRat.Minimum = -20; xMaxIRat.Maximum = 20; xMaxIRat.Value = 20;
            xMinIRat.Minimum = -20; xMinIRat.Maximum = 20; xMinIRat.Value = 0;

            ct_IntensityRatio.ChartAreas[0].AxisX.Title = "Intensity Ratio of a Pair";
            ct_IntensityRatio.ChartAreas[0].AxisY.Title = "Number of NeuCode Pairs";
        }
        private void GraphLysineCount()
        {
            DataTable lysCtHistogram = new DataTable();
            lysCtHistogram.Columns.Add("numLysines", typeof(int));
            lysCtHistogram.Columns.Add("numPairsAtThisLysCt", typeof(int));

            int ymax = 0;
            for (int i = 0; i <= 28; i++)
            {
                List<NeuCodePair> pf_by_lysCt = Sweet.lollipop.raw_neucode_pairs.Where(p => p.lysine_count == i).ToList();
                if (pf_by_lysCt.Count > ymax)
                {
                    ymax = pf_by_lysCt.Count;
                }
                lysCtHistogram.Rows.Add(i, pf_by_lysCt.Count);
            }
            ct_LysineCount.DataSource = lysCtHistogram;
            ct_LysineCount.DataBind();

            ct_LysineCount.Series["lysineCount"].XValueMember = "numLysines";
            ct_LysineCount.Series["lysineCount"].YValueMembers = "numPairsAtThisLysCt";

            yMaxKCt.Minimum = -ymax; yMaxKCt.Maximum = ymax; yMaxKCt.Value = ymax;
            yMinKCt.Minimum = -ymax; yMinKCt.Maximum = ymax; yMinKCt.Value = 0;
            xMaxKCt.Minimum = -28; xMaxKCt.Value = 28;
            xMinKCt.Minimum = -28; xMinKCt.Maximum = 28; xMinKCt.Value = 0;

            ct_LysineCount.ChartAreas[0].AxisX.Title = "Lysine Count";
            ct_LysineCount.ChartAreas[0].AxisY.Title = "Number of NeuCode Pairs";
        }
        private void yMaxKCt_ValueChanged(object sender, EventArgs e)
        {
            ct_LysineCount.ChartAreas[0].AxisY.Maximum = double.Parse(yMaxKCt.Value.ToString());
        }
        private void yMinKCt_ValueChanged(object sender, EventArgs e)
        {
            ct_LysineCount.ChartAreas[0].AxisY.Minimum = double.Parse(yMinKCt.Value.ToString());
        }
        private void xMinKCt_ValueChanged(object sender, EventArgs e)
        {
            ct_LysineCount.ChartAreas[0].AxisX.Minimum = double.Parse(xMinKCt.Value.ToString());
        }
        private void xMaxKCt_ValueChanged(object sender, EventArgs e)
        {
            ct_LysineCount.ChartAreas[0].AxisX.Maximum = double.Parse(xMaxKCt.Value.ToString());
        }
        private void yMaxIRat_ValueChanged(object sender, EventArgs e)
        {
            ct_IntensityRatio.ChartAreas[0].AxisY.Maximum = double.Parse(yMaxIRat.Value.ToString());
        }
        private void yMinIRat_ValueChanged(object sender, EventArgs e)
        {
            ct_IntensityRatio.ChartAreas[0].AxisY.Minimum = double.Parse(yMinIRat.Value.ToString());
        }
        private void xMinIRat_ValueChanged(object sender, EventArgs e)
        {
            ct_IntensityRatio.ChartAreas[0].AxisX.Maximum = double.Parse(xMaxIRat.Value.ToString());
        }
        private void xMaxIRat_ValueChanged(object sender, EventArgs e)
        {
            ct_IntensityRatio.ChartAreas[0].AxisX.Maximum = double.Parse(xMaxIRat.Value.ToString());
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
        private void IRatMinAcceptable_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_intensity_ratio = IRatMinAcceptable.Value;
            Parallel.ForEach(Sweet.lollipop.raw_neucode_pairs, p => p.set_accepted());
            dgv_RawExpNeuCodePairs.Refresh();
            this.MDIParent.aggregatedProteoforms.ClearListsTablesFigures(true);
        }
        private void IRatMaxAcceptable_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.max_intensity_ratio = IRatMaxAcceptable.Value;
            Parallel.ForEach(Sweet.lollipop.raw_neucode_pairs, p => p.set_accepted());
            dgv_RawExpNeuCodePairs.Refresh();
            this.MDIParent.aggregatedProteoforms.ClearListsTablesFigures(true);
        }
        #endregion Private Methods

        #region Tooltip Private Methods
        System.Drawing.Point? ct_intensityRatio_prevPosition = null;
        System.Windows.Forms.ToolTip ct_intensityRatio_tt = new System.Windows.Forms.ToolTip();
        private void ct_IntensityRatio_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
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
        #endregion Tooltip Private Methods
    }
}
