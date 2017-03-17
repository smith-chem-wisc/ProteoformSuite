using System;
using ProteoformSuiteInternal;
using System.Windows.Forms;
using System.IO;

namespace ProteoformSuiteGUI
{
    public partial class ResultsSummary : Form
    {
        public ResultsSummary()
        {
            InitializeComponent();
        }

        private void ResultsSummary_Load(object sender, EventArgs e)
        { }

        public void create_summary()
        {
            rtb_summary.Text = ResultsSummaryGenerator.generate_full_report();
        }

        FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
        private void btn_browseSummarySaveFolder_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.folderBrowser.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string temp_folder_path = folderBrowser.SelectedPath;
                tb_summarySaveFolder.Text = temp_folder_path; //triggers TextChanged method
            }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            string timestamp = SaveState.time_stamp();
            using (StreamWriter writer = new StreamWriter(Path.Combine(tb_summarySaveFolder.Text, "summary_" + timestamp + ".txt"))) writer.Write(ResultsSummaryGenerator.generate_full_report());
            if (cb_savePlots.Checked && Directory.Exists(tb_summarySaveFolder.Text)) ((ProteoformSweet)MdiParent).save_all_plots(tb_summarySaveFolder.Text, timestamp);
        }
    }
}
