using System;
using System.Linq;
using ProteoformSuiteInternal;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;

namespace ProteoformSuiteGUI
{
    public partial class ResultsSummary : Form
    {
        public ResultsSummary()
        {
            InitializeComponent();
        }

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
            if (!Directory.Exists(tb_summarySaveFolder.Text)) return;
            string timestamp = SaveState.time_stamp();
            Parallel.Invoke
            (
                () => save_summary(timestamp),
                () => save_dataframe(timestamp),
                () => save_cytoscripts(timestamp)
            );
            save_plots(timestamp);
        }

        private void save_summary(string timestamp)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(tb_summarySaveFolder.Text, "summary_" + timestamp + ".txt")))
                writer.Write(ResultsSummaryGenerator.generate_full_report());
        }

        private void save_dataframe(string timestamp)
        {
            using (StreamWriter writer = new StreamWriter(Path.Combine(tb_summarySaveFolder.Text, "results_" + timestamp + ".tsv")))
                if (cb_saveDataframe.Checked)
                    writer.Write(ResultsSummaryGenerator.results_dataframe());
        }

        private void save_plots(string timestamp)
        {
            if (cb_savePlots.Checked)
                ((ProteoformSweet)MdiParent).save_all_plots(tb_summarySaveFolder.Text, timestamp);
        }

        private void save_cytoscripts(string timestamp)
        {
            if (cb_saveCytoScripts.Checked)
            {
                string message = "";
                message += CytoscapeScript.write_cytoscape_script(SaveState.lollipop.proteoform_community.families, SaveState.lollipop.proteoform_community.families,
                    tb_summarySaveFolder.Text, "AllFamilies_", timestamp,
                    SaveState.lollipop.qVals.Count > 0, true, true, false,
                    CytoscapeScript.color_scheme_names[0], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], 2,
                    ProteoformCommunity.gene_centric_families, ProteoformCommunity.preferred_gene_label);
                message += Environment.NewLine;

                foreach (GoTermNumber gtn in SaveState.lollipop.goTermNumbers.Where(g => g.by < (double)SaveState.lollipop.minProteoformFDR).ToList())
                {
                    message += CytoscapeScript.write_cytoscape_script(new GoTermNumber[] { gtn }, SaveState.lollipop.proteoform_community.families,
                        tb_summarySaveFolder.Text, gtn.Aspect.ToString() + gtn.Description.Replace(" ", "_") + "_", timestamp,
                        true, true, true, false,
                        CytoscapeScript.color_scheme_names[0], Lollipop.edge_labels[1], Lollipop.node_labels[1], CytoscapeScript.node_label_positions[0], 2,
                        ProteoformCommunity.gene_centric_families, ProteoformCommunity.preferred_gene_label);
                    message += Environment.NewLine;
                }
                message += "Remember to install the package \"enhancedGraphics\" under App -> App Manager to view piechart nodes for quantitative data";
            }
        }
    }
}
