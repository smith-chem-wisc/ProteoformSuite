using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using ProteoformSuiteInternal;

namespace ProteoformSuite
{
    public partial class ProteoformSweet : Form
    {
        //  Initialize Forms START
        LoadDeconvolutionResults loadDeconvolutionResults = new LoadDeconvolutionResults();
        RawExperimentalComponents rawExperimentalComponents = new RawExperimentalComponents();
        NeuCodePairs neuCodePairs = new NeuCodePairs();
        AggregatedProteoforms aggregatedProteoforms = new AggregatedProteoforms();
        TheoreticalDatabase theoreticalDatabase = new TheoreticalDatabase();
        ExperimentTheoreticalComparison experimentalTheoreticalComparison = new ExperimentTheoreticalComparison();
        ExperimentExperimentComparison experimentExperimentComparison = new ExperimentExperimentComparison();
        ProteoformFamilies proteoformFamilies = new ProteoformFamilies();
        ResultsSummary resultsSummary = new ResultsSummary();
        List<Form> forms;
        //  Initialize Forms END

        FolderBrowserDialog resultsFolderOpen = new FolderBrowserDialog();
        OpenFileDialog methodFileOpen = new OpenFileDialog();
        SaveFileDialog methodFileSave = new SaveFileDialog();

        public ProteoformSweet()
        {
            InitializeComponent();
            InitializeForms();
            this.WindowState = FormWindowState.Maximized;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            showForm(loadDeconvolutionResults);
        }

        public void InitializeForms()
        {
            forms = new List<Form>(new Form[] {
                loadDeconvolutionResults, rawExperimentalComponents, neuCodePairs, aggregatedProteoforms,
                theoreticalDatabase, experimentalTheoreticalComparison, experimentExperimentComparison,
                proteoformFamilies, resultsSummary
            });
        }

        private void showForm(Form form)
        {
            form.MdiParent = this;
            form.Show();
            form.WindowState = FormWindowState.Maximized;
        }
        
        // tool strip clicks
        public void loadDeconvolutionResultsToolStripMenuItem_Click(object sender, EventArgs e) { showForm(loadDeconvolutionResults); }
        private void rawExperimentalProteoformsToolStripMenuItem_Click(object sender, EventArgs e) { showForm(rawExperimentalComponents); }
        private void neuCodeProteoformPairsToolStripMenuItem_Click(object sender, EventArgs e) { showForm(neuCodePairs); }
        private void aggregatedProteoformsToolStripMenuItem_Click(object sender, EventArgs e) { showForm(aggregatedProteoforms); }
        private void theoreticalProteoformDatabaseToolStripMenuItem_Click(object sender, EventArgs e) { showForm(theoreticalDatabase); }
        private void experimentTheoreticalComparisonToolStripMenuItem_Click(object sender, EventArgs e) { showForm(experimentalTheoreticalComparison); }
        private void experimentExperimentComparisonToolStripMenuItem_Click(object sender, EventArgs e) { showForm(experimentExperimentComparison); }
        private void proteoformFamilyAssignmentToolStripMenuItem_Click(object sender, EventArgs e) { showForm(proteoformFamilies); }
        private void resultsSummaryToolStripMenuItem_Click(object sender, EventArgs e) { showForm(resultsSummary); }

        private void generateMethodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var response = MessageBox.Show("This option does a dry run of the program.\n\nIt allows you to select input files and parameters to generate a method file.\n\nThen, select Load & Run from the Method drop-down menu to then perform the full run of ProteoformSuite.", "Generate Method", MessageBoxButtons.OKCancel);
            //if (response == DialogResult.OK)
            //{
            //    foreach (Form form in forms)
            //    {
            //        Show(Form);
            //        Thread t2 = new Thread(wait_message);
            //        t2.Start();
            //        t2.Join();
            //    }
            //}
        }

        //private void wait_message() { MessageBox.Show("Click OK when you are satisfied with the settings."); }

        private void saveMethodToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveMethod();
        }

        private void saveMethod()
        {
            DialogResult dr = this.methodFileSave.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string method_filename = methodFileSave.FileName;
                using (StreamWriter file = new StreamWriter(method_filename))
                    file.WriteLine(Lollipop.method_toString());
            }
        }

        private void loadRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.methodFileOpen.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string method_filename = methodFileOpen.FileName;
                string[] lines = File.ReadAllLines(method_filename);
                if (Lollipop.deconResultsFileNames.Count != 0)
                {
                    var response1 = MessageBox.Show("Would you like to use the files specified in LoadDeconvolution rather than those referenced in the method file?", "Multiple Deconvolution File References", MessageBoxButtons.YesNoCancel);
                    if (response1 == DialogResult.Cancel) return;
                    else if (response1 == DialogResult.Yes) Lollipop.use_method_files = false;
                    else if (response1 == DialogResult.No)
                    {
                        Lollipop.deconResultsFileNames.Clear();
                        Lollipop.use_method_files = true;
                    }
                }

                foreach (string line in lines)
                {
                    string setting_spec = line.Trim();
                    Lollipop.load_setting(setting_spec);
                }

                // For connectivity testing --
                //string working_directory;
                //MessageBox.Show("Choose a results folder.");
                //DialogResult results_folder = this.resultsFolderOpen.ShowDialog();
                //if (results_folder == DialogResult.OK)
                //    working_directory = this.resultsFolderOpen.SelectedPath;
                //else
                //    working_directory = Path.GetDirectoryName(Lollipop.deconResultsFileNames[0]);

                var response2 = MessageBox.Show("Successfully loaded method. Will run the method now.\n\nWill show as non-responsive.", "Method Load", MessageBoxButtons.OKCancel);
                if (response2 == DialogResult.Cancel) return;

                Parallel.Invoke( 
                    () => Lollipop.get_experimental_proteoforms((b)=>new ExcelReader().read_components_from_xlsx(b, Lollipop.correctionFactors)),
                    () => Lollipop.get_theoretical_proteoforms()
                );
                Parallel.Invoke(
                    () => Lollipop.make_et_relationships(),
                    () => Lollipop.make_ee_relationships()
                );
                Lollipop.proteoform_community.construct_families();

                // For connectivity testing --
                //File.WriteAllText(working_directory + "\\raw_experimental_components.tsv", Lollipop.raw_component_results());
                //File.WriteAllText(working_directory + "\\raw_neucode_pairs.tsv", Lollipop.raw_neucode_pair_results());
                //File.WriteAllText(working_directory + "\\aggregated_experimental_proteoforms.tsv", Lollipop.aggregated_experimental_proteoform_results());         
                //File.WriteAllText(working_directory + "\\experimental_theoretical_relationships.tsv", Lollipop.et_relations_results());
                //File.WriteAllText(working_directory + "\\experimental_decoy_relationships.tsv", Lollipop.ed_relations_results());
                //File.WriteAllText(working_directory + "\\experimental_experimental_relationships.tsv", Lollipop.ee_relations_results());
                //File.WriteAllText(working_directory + "\\experimental_false_relationships.tsv", Lollipop.ef_relations_results());
                //File.WriteAllText(working_directory + "\\experimental_theoretical_peaks.tsv", Lollipop.et_peak_results());
                //File.WriteAllText(working_directory + "\\experimental_experimental_peaks.tsv", Lollipop.ee_peak_results());

                prepare_figures_and_tables();
                MessageBox.Show("Successfully ran method. Feel free to explore using the Processing Phase menu.");
            }
        }

        private void prepare_figures_and_tables()
        {
            Parallel.Invoke(
                () => rawExperimentalComponents.FillRawExpComponentsTable(),
                () => aggregatedProteoforms.FillAggregatesTable(),
                () => theoreticalDatabase.FillDataBaseTable("Target"),
                () => theoreticalDatabase.initialize_table_bindinglist(),
                () => experimentalTheoreticalComparison.FillTablesAndCharts(),
                () => experimentExperimentComparison.FillTablesAndCharts()
            );
            if (Lollipop.neucode_labeled) neuCodePairs.GraphNeuCodePairs();

        }

        public void enable_neuCodeProteoformPairsToolStripMenuItem(bool setting)
        {
            neuCodeProteoformPairsToolStripMenuItem.Enabled = setting;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("openToolStripMenuItem_Click");
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("saveToolStripMenuItem_Click");
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("saveAsToolStripMenuItem_Click");
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("printToolStripMenuItem_Click");
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
