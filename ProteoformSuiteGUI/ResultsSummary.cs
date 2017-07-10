using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
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
        }

        #endregion Public Constructor

        #region Public Methods

        public List<DataGridView> GetDGVs()
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

        public void RunTheGamut()
        {
            create_summary();
        }

        public void InitializeParameterSet()
        {
            tb_summarySaveFolder.Text = Sweet.lollipop.results_folder;
        }

        public void ClearListsTablesFigures(bool x)
        {
            rtb_summary.Text = "";
            tb_summarySaveFolder.Text = "";
            Sweet.lollipop.results_folder = "";
        }

        public void FillTablesAndCharts()
        {
            create_summary();
        }

        #endregion Public Methods

        #region Private Fields

        FolderBrowserDialog folderBrowser = new FolderBrowserDialog();

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
            ResultsSummaryGenerator.save_all(Sweet.lollipop.results_folder, timestamp);
            ((ProteoformSweet)MdiParent).save_all_plots(Sweet.lollipop.results_folder, timestamp);
        }

        #endregion Private Methods

    }
}
