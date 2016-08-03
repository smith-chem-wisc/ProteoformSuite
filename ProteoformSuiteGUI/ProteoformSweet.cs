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
        //ProteoformFamilyAssignment proteoformFamilyAssignment = new ProteoformFamilyAssignment();
        List<Form> forms;
        //  Initialize Forms END

        FolderBrowserDialog resultsFolderOpen = new FolderBrowserDialog();
        OpenFileDialog methodFileOpen = new OpenFileDialog();
        SaveFileDialog methodFileSave = new SaveFileDialog();

        Form current_form; 

        public ProteoformSweet()
        {
            InitializeComponent();
            InitializeForms();
            this.WindowState = FormWindowState.Maximized;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        public void InitializeForms()
        {
            forms = new List<Form>(new Form[] {
                loadDeconvolutionResults, rawExperimentalComponents, neuCodePairs, aggregatedProteoforms,
                theoreticalDatabase, experimentalTheoreticalComparison, experimentExperimentComparison,
                //proteoformFamilyAssignment
            });
        }

        private void showForm(Form form)
        {
            form.MdiParent = this;
            form.Show();
            form.WindowState = FormWindowState.Maximized;
            current_form = form;
        }
        
        // tool strip clicks
        public void loadDeconvolutionResultsToolStripMenuItem_Click(object sender, EventArgs e) { showForm(loadDeconvolutionResults); }
        private void rawExperimentalProteoformsToolStripMenuItem_Click(object sender, EventArgs e) { showForm(rawExperimentalComponents); }
        private void neuCodeProteoformPairsToolStripMenuItem_Click(object sender, EventArgs e) { showForm(neuCodePairs); }
        private void aggregatedProteoformsToolStripMenuItem_Click(object sender, EventArgs e) { showForm(aggregatedProteoforms); }
        private void theoreticalProteoformDatabaseToolStripMenuItem_Click(object sender, EventArgs e) { showForm(theoreticalDatabase); }
        private void experimentTheoreticalComparisonToolStripMenuItem_Click(object sender, EventArgs e) { showForm(experimentalTheoreticalComparison); }
        private void experimentExperimentComparisonToolStripMenuItem_Click(object sender, EventArgs e) { showForm(experimentExperimentComparison); }
        private void proteoformFamilyAssignmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //showForm(proteoformFamilyAssignment);
        }

        private void resultsSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResultsSummary resultsSummary = new ResultsSummary();
            showForm(resultsSummary);
        }

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
                    var response = MessageBox.Show("Would you like to use the files specified in LoadDeconvolution rather than those referenced in the method file?", "Multiple Deconvolution File References", MessageBoxButtons.YesNoCancel);
                    if (response == DialogResult.Yes) { Lollipop.use_method_files = false; }
                    if (response == DialogResult.No) { Lollipop.deconResultsFileNames.Clear(); Lollipop.use_method_files = true; }
                    if (response == DialogResult.Cancel) { return; }
                }

                foreach (string line in lines)
                {
                    string setting_spec = line.Trim();
                    Lollipop.load_setting(setting_spec);
                }

                // For connectivity testing --
                string working_directory;
                MessageBox.Show("Choose a results folder.");
                DialogResult results_folder = this.resultsFolderOpen.ShowDialog();
                if (results_folder == DialogResult.OK)
                    working_directory = this.resultsFolderOpen.SelectedPath;
                else
                    working_directory = Path.GetDirectoryName(Lollipop.deconResultsFileNames[0]);

                MessageBox.Show("Successfully loaded method. Will run the method now.\n\nWill show as non-responsive.");

                Parallel.Invoke( 
                    () => Lollipop.get_experimental_proteoforms((b)=>new ExcelReader().read_components_from_xlsx(b)),
                    () => Lollipop.get_theoretical_proteoforms()
                );
                Parallel.Invoke(
                    () => Lollipop.make_et_relationships(),
                    () => Lollipop.make_ee_relationships()
                );

                save_tsv_files(working_directory, true);
                prepare_figures_and_tables();
                this.enable_neuCodeProteoformPairsToolStripMenuItem(Lollipop.neucode_labeled);
                MessageBox.Show("Successfully ran method. Feel free to explore using the Processing Phase menu.");
            }
        }

        private void save_tsv_files (string working_directory, bool save_all)
        {
            if (current_form == rawExperimentalComponents || save_all)
                File.WriteAllText(working_directory + "\\raw_experimental_components.tsv", Results.raw_component_results());
            if (current_form == neuCodePairs || save_all)
                File.WriteAllText(working_directory + "\\raw_neucode_pairs.tsv", Results.raw_neucode_pair_results());
            if (current_form == aggregatedProteoforms || save_all)
                File.WriteAllText(working_directory + "\\aggregated_experimental_proteoforms.tsv", Results.aggregated_experimental_proteoform_results());
            if (current_form == theoreticalDatabase || save_all)
            {
                File.WriteAllText(working_directory + "\\theoretical_proteoforms.tsv", Results.theoretical_proteoforms_results());
                File.WriteAllText(working_directory + "\\decoy_proteoforms.tsv", Results.decoy_proteoforms_results());
            }
            if (current_form == experimentalTheoreticalComparison || save_all)
            {
                File.WriteAllText(working_directory + "\\experimental_theoretical_relationships.tsv", Results.et_relations_results());
                File.WriteAllText(working_directory + "\\experimental_decoy_relationships.tsv", Results.ed_relations_results());
                File.WriteAllText(working_directory + "\\experimental_theoretical_peaks.tsv", Results.et_peak_results());
            }
            if (current_form == experimentExperimentComparison || save_all)
            {
                File.WriteAllText(working_directory + "\\experimental_experimental_relationships.tsv", Results.ee_relations_results());
                File.WriteAllText(working_directory + "\\experimental_false_relationships.tsv", Results.ef_relations_results());
                File.WriteAllText(working_directory + "\\experimental_experimental_peaks.tsv", Results.ee_peak_results());
            }
            else { return; }
        }

        private void prepare_figures_and_tables()
        {
            Parallel.Invoke(
                () => rawExperimentalComponents.FillRawExpComponentsTable(),
                () => aggregatedProteoforms.FillAggregatesTable(),
                () => theoreticalDatabase.FillDataBaseTable("Target"),
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
            string working_directory;
            MessageBox.Show("Please select a folder with saved results files.");
            DialogResult results_folder = this.resultsFolderOpen.ShowDialog();
            if (results_folder == DialogResult.OK)
                working_directory = this.resultsFolderOpen.SelectedPath;
            else
                working_directory = Path.GetDirectoryName(Lollipop.deconResultsFileNames[0]);

            DialogResult response = MessageBox.Show("Are these results neucode labeled?", "Neucode Labeled", MessageBoxButtons.YesNo);
            if (response == DialogResult.Yes) { Lollipop.neucode_labeled = true; }
            if (response == DialogResult.No) { Lollipop.neucode_labeled = false; }
            this.enable_neuCodeProteoformPairsToolStripMenuItem(Lollipop.neucode_labeled);

            Lollipop.opened_results = true;
            Lollipop.opened_results_originally = true;

            //cannot parallelize bc results dependent on one another for certain objects
            Results.read_raw_components(working_directory);

            if (Lollipop.neucode_labeled) Results.read_raw_neucode_pairs(working_directory);
            Results.read_aggregated_proteoforms(working_directory);
            Results.read_theoretical_proteoforms(working_directory);
            Results.read_decoy_proteoforms(working_directory);
            Results.read_experimental_decoy_relationships(working_directory);
            Results.read_experimental_theoretical_relationships(working_directory);
            Results.read_peaks(working_directory, ProteoformComparison.et);
            Results.read_experimental_false_relationships(working_directory);
            Results.read_experimental_experimental_relationships(working_directory);
            Results.read_peaks(working_directory, ProteoformComparison.ee);
            MessageBox.Show("Files successfully read in.");

            Lollipop.opened_results = false;
        }


        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string working_directory;
            if (current_form == loadDeconvolutionResults) { return; }
            MessageBox.Show("Choose a results folder.");
            DialogResult results_folder = this.resultsFolderOpen.ShowDialog();
            if (results_folder == DialogResult.OK)
                working_directory = this.resultsFolderOpen.SelectedPath;
            else
                working_directory = Path.GetDirectoryName(Lollipop.deconResultsFileNames[0]);
            save_tsv_files(working_directory, false);
            MessageBox.Show("Successfully saved the currently displayed page.");
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string working_directory;
            MessageBox.Show("Choose a results folder.");
            DialogResult results_folder = this.resultsFolderOpen.ShowDialog();
            if (results_folder == DialogResult.OK)
                working_directory = this.resultsFolderOpen.SelectedPath;
            else
                working_directory = Path.GetDirectoryName(Lollipop.deconResultsFileNames[0]);
            save_tsv_files(working_directory, true);
            MessageBox.Show("Successfully saved all pages.");
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
