using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public partial class ResultsSummary : Form, ISweetForm
    {
        #region Public Constructor

        public ResultsSummary()
        {
            InitializeComponent();
            this.AutoScroll = true;
            this.AutoScrollMinSize = this.ClientSize;
        }

        #endregion Public Constructor

        #region Public Property

        public List<DataTable> DataTables { get; private set; }

        #endregion Public Property

        #region Public Methods

        public List<DataTable> SetTables()
        {
            return null;
        }

        public void create_summary()
        {
            rtb_summary.Text = ResultsSummaryGenerator.generate_full_report();
        }

        public bool ReadyToRunTheGamut()
        {
            return true;
        }

        public void RunTheGamut(bool full_run)
        {
            create_summary();
        }

        public void InitializeParameterSet()
        {
            tb_summarySaveFolder.Text = Sweet.lollipop.results_folder;
            cmbx_analysis.Items.Clear();
            cmbx_analysis.Items.AddRange(new string[]
            {
                "Tusher Analysis (" + Sweet.lollipop.TusherAnalysis1.sortedPermutedRelativeDifferences.Count.ToString() + " Permutations)",
                "Tusher Analysis (" + Sweet.lollipop.TusherAnalysis2.sortedPermutedRelativeDifferences.Count.ToString() + " Permutations)",
                "Log2 Fold Change Analysis (" + Sweet.lollipop.Log2FoldChangeAnalysis.benjiHoch_fdr.ToString() + " FDR)"
            });
            cmbx_analysis.SelectedIndex = 1;
        }

        public void ClearListsTablesFigures(bool x)
        {
            rtb_summary.Text = "";
            tb_summarySaveFolder.Text = "";
            //Sweet.lollipop.results_folder = "";
        }

        public void FillTablesAndCharts()
        {
            create_summary();
        }

        public IGoAnalysis get_go_analysis()
        {
            return cmbx_analysis.SelectedIndex == 0 ? Sweet.lollipop.TusherAnalysis1 as IGoAnalysis : cmbx_analysis.SelectedIndex == 1 ? Sweet.lollipop.TusherAnalysis2 as IGoAnalysis : Sweet.lollipop.Log2FoldChangeAnalysis as IGoAnalysis;
        }

        public TusherAnalysis get_tusher_analysis()
        {
            return cmbx_analysis.SelectedIndex == 0 ? Sweet.lollipop.TusherAnalysis1 as TusherAnalysis : cmbx_analysis.SelectedIndex == 1 ? Sweet.lollipop.TusherAnalysis2 as TusherAnalysis : null;
        }

        #endregion Public Methods

        #region Private Fields

        private FolderBrowserDialog folderBrowser = new FolderBrowserDialog();

        #endregion Private Fields

        #region Private Methods

        private void btn_browseSummarySaveFolder_Click(object sender, EventArgs e)
        {
            DialogResult dr = folderBrowser.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string temp_folder_path = folderBrowser.SelectedPath;
                tb_summarySaveFolder.Text = temp_folder_path;
                Sweet.lollipop.results_folder = temp_folder_path;
            }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Sweet.lollipop.results_folder)) return;
            string timestamp = Sweet.time_stamp();
            ResultsSummaryGenerator.save_all(Sweet.lollipop.results_folder, timestamp, get_go_analysis(), get_tusher_analysis());
            ((ProteoformSweet)MdiParent).save_all_plots(Sweet.lollipop.results_folder, timestamp);
            using (StreamWriter file = new StreamWriter(Path.Combine(Sweet.lollipop.results_folder, "presets_" + timestamp + ".xml")))
                file.WriteLine(Sweet.save_method());
        }

        #endregion Private Methods
    }
}