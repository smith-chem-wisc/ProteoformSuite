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

        Form current_form; 

        public ProteoformSweet()
        {
            InitializeComponent();
            InitializeForms();
            this.WindowState = FormWindowState.Maximized;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            showForm(loadDeconvolutionResults);
            methodFileOpen.Filter = "Method TXT File (*.txt)| *.txt";
        }

        public void InitializeForms()
        {
            forms = new List<Form>(new Form[] {
                loadDeconvolutionResults, rawExperimentalComponents, neuCodePairs, aggregatedProteoforms,
                theoreticalDatabase, experimentalTheoreticalComparison, experimentExperimentComparison,
                proteoformFamilies
            });
        }

        private void showForm(Form form)
        {
            form.MdiParent = this;
            form.Show();
            form.WindowState = FormWindowState.Maximized;
            current_form = form;
        }
        

        // RESULTS TOOL STRIP
        public void loadDeconvolutionResultsToolStripMenuItem_Click(object sender, EventArgs e) { showForm(loadDeconvolutionResults); }
        private void rawExperimentalProteoformsToolStripMenuItem_Click(object sender, EventArgs e) { showForm(rawExperimentalComponents); }
        private void neuCodeProteoformPairsToolStripMenuItem_Click(object sender, EventArgs e) { showForm(neuCodePairs); }
        private void aggregatedProteoformsToolStripMenuItem_Click(object sender, EventArgs e) { showForm(aggregatedProteoforms); }
        private void theoreticalProteoformDatabaseToolStripMenuItem_Click(object sender, EventArgs e) { showForm(theoreticalDatabase); }
        private void experimentTheoreticalComparisonToolStripMenuItem_Click(object sender, EventArgs e) { showForm(experimentalTheoreticalComparison); }
        private void experimentExperimentComparisonToolStripMenuItem_Click(object sender, EventArgs e) { showForm(experimentExperimentComparison); }
        private void proteoformFamilyAssignmentToolStripMenuItem_Click(object sender, EventArgs e) { showForm(proteoformFamilies); }

        private void resultsSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resultsSummary.createResultsSummary();
            resultsSummary.displayResultsSummary();
            showForm(resultsSummary);
        }


        // FILE TOOL STRIP
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string working_directory;
            MessageBox.Show("Please choose a folder with saved results files.");
            DialogResult results_folder = this.resultsFolderOpen.ShowDialog();
            if (results_folder == DialogResult.OK) working_directory = this.resultsFolderOpen.SelectedPath;
            else if (results_folder == DialogResult.Cancel) return;
            else return;

            MessageBox.Show("Choose the method file corresponding to the results files.");
            openMethod();
            
            Lollipop.opened_results = true;
            Lollipop.opened_results_originally = true;
            ResultsSummary.loadDescription = working_directory;

            var result = MessageBox.Show("Add to the existing results?", "Results Clear", MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.No) clear_lists();

            //cannot parallelize bc results dependent on one another for certain objects
            MessageBox.Show("Will load in results now.\n\nMay show as non-responsive.");
            Results.read_raw_components(File.ReadAllLines(working_directory + "\\raw_experimental_components.tsv"));
            if (Lollipop.neucode_labeled) Results.read_raw_neucode_pairs(File.ReadAllLines(working_directory + "\\raw_neucode_pairs.tsv"));
            Results.read_aggregated_proteoforms(File.ReadAllLines(working_directory + "\\aggregated_experimental_proteoforms.tsv"));
            Results.read_theoretical_proteoforms(File.ReadAllLines(working_directory + "\\theoretical_proteoforms.tsv"), true);
            Results.read_theoretical_proteoforms(File.ReadAllLines(working_directory + "\\decoy_proteoforms.tsv"), false);
            Results.read_relationships(File.ReadAllLines(working_directory + "\\experimental_theoretical_relationships.tsv"), ProteoformComparison.et);
            Results.read_relationships(File.ReadAllLines(working_directory + "\\experimental_decoy_relationships.tsv"), ProteoformComparison.ed);
            Results.read_relationships(File.ReadAllLines(working_directory + "\\experimental_experimental_relationships.tsv"), ProteoformComparison.ee);
            Results.read_relationships(File.ReadAllLines(working_directory + "\\experimental_false_relationships.tsv"), ProteoformComparison.ef);
            Results.read_peaks(File.ReadAllLines(working_directory + "\\experimental_theoretical_peaks.tsv"), ProteoformComparison.et);
            Results.read_peaks(File.ReadAllLines(working_directory + "\\experimental_experimental_peaks.tsv"), ProteoformComparison.ee);
            Results.read_families(File.ReadAllLines(working_directory + "\\proteoform_families.tsv"));
            MessageBox.Show("Files successfully read in.");

            Lollipop.opened_results = false;
        }

        private void openMethod()
        {
            DialogResult dr = this.methodFileOpen.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string method_filename = methodFileOpen.FileName;
                ResultsSummary.loadDescription = method_filename;
                foreach (string setting_spec in File.ReadAllLines(method_filename))
                {
                    Lollipop.load_setting(setting_spec.Trim());
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string working_directory;
            if (current_form == loadDeconvolutionResults) { return; }
            MessageBox.Show("Choose a results folder.");
            DialogResult results_folder = this.resultsFolderOpen.ShowDialog();
            if (results_folder == DialogResult.OK) working_directory = this.resultsFolderOpen.SelectedPath;
            else return;
            save_tsv(working_directory, false);
            MessageBox.Show("Successfully saved the currently displayed page.");
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string working_directory;
            MessageBox.Show("Choose a folder for the method and results files.");
            DialogResult results_folder = this.resultsFolderOpen.ShowDialog();
            if (results_folder == DialogResult.OK) working_directory = this.resultsFolderOpen.SelectedPath;
            else return;
            saveMethod(working_directory + "\\_method.txt");
            save_tsv(working_directory, true);
            MessageBox.Show("Successfully saved all pages.");
        }

        private void save_tsv(string working_directory, bool save_all)
        {
            if (current_form == rawExperimentalComponents || save_all)
            {
                File.WriteAllText(working_directory + "\\raw_experimental_components.tsv", Results.raw_component_results());
            }
            if (current_form == neuCodePairs || save_all)
            {
                File.WriteAllText(working_directory + "\\raw_neucode_pairs.tsv", Results.raw_neucode_pair_results());
            }
            if (current_form == aggregatedProteoforms || save_all)
            {
                File.WriteAllText(working_directory + "\\aggregated_experimental_proteoforms.tsv", Results.aggregated_experimental_proteoform_results());
            }
            if (current_form == theoreticalDatabase || save_all)
            {
                File.WriteAllText(working_directory + "\\theoretical_proteoforms.tsv", Results.theoretical_proteoforms_results(true));
                File.WriteAllText(working_directory + "\\decoy_proteoforms.tsv", Results.theoretical_proteoforms_results(false));
            }
            if (current_form == experimentalTheoreticalComparison || save_all)
            {
                File.WriteAllText(working_directory + "\\experimental_theoretical_relationships.tsv", Results.relation_results(ProteoformComparison.et));
                File.WriteAllText(working_directory + "\\experimental_decoy_relationships.tsv", Results.relation_results(ProteoformComparison.ed));
                File.WriteAllText(working_directory + "\\experimental_theoretical_peaks.tsv", Results.peak_results(ProteoformComparison.et));
            }
            if (current_form == experimentExperimentComparison || save_all)
            {
                File.WriteAllText(working_directory + "\\experimental_experimental_relationships.tsv", Results.relation_results(ProteoformComparison.ee));
                File.WriteAllText(working_directory + "\\experimental_false_relationships.tsv", Results.relation_results(ProteoformComparison.ef));
                File.WriteAllText(working_directory + "\\experimental_experimental_peaks.tsv", Results.peak_results(ProteoformComparison.ee));
            }
            if (current_form == proteoformFamilies || save_all)
            {
                File.WriteAllText(working_directory + "\\proteoform_families.tsv", Results.family_results());
            }
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("printToolStripMenuItem_Click");
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        // METHOD TOOL STRIP
        private void saveMethodToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.methodFileSave.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                saveMethod(methodFileSave.FileName);
            }
        }

        private void saveMethod(string method_filename)
        {
            using (StreamWriter file = new StreamWriter(method_filename))
                file.WriteLine(Lollipop.method_toString());
        }

        private void loadRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Choose a method file.");
            if (Lollipop.deconResultsFileNames.Count != 0)
            {
                var response = MessageBox.Show("Would you like to use the files specified in LoadDeconvolution rather than those referenced in the method file?", "Multiple Deconvolution File References", MessageBoxButtons.YesNoCancel);
                if (response == DialogResult.Yes) { Lollipop.use_method_files = false; }
                if (response == DialogResult.No) { Lollipop.deconResultsFileNames.Clear(); Lollipop.use_method_files = true; }
                if (response == DialogResult.Cancel) { return; }
            }
            openMethod();
            MessageBox.Show("Successfully loaded method. Will run the method now.\n\nWill show as non-responsive.");

            clear_lists();
            rawExperimentalComponents.load_raw_components();
            Lollipop.get_theoretical_proteoforms();
            Parallel.Invoke(
                () => Lollipop.make_et_relationships(),
                () => Lollipop.make_ee_relationships());
            Lollipop.proteoform_community.construct_families();
            prepare_figures_and_tables();
            this.enable_neuCodeProteoformPairsToolStripMenuItem(Lollipop.neucode_labeled);
            MessageBox.Show("Successfully ran method. Feel free to explore using the Processing Phase menu.");
        }
        private void prepare_figures_and_tables()
        {
            Parallel.Invoke(
                () => rawExperimentalComponents.FillRawExpComponentsTable(),
                () => aggregatedProteoforms.FillAggregatesTable(),
                () => theoreticalDatabase.FillDataBaseTable("Target"),
                () => experimentalTheoreticalComparison.FillTablesAndCharts(),
                () => experimentExperimentComparison.FillTablesAndCharts(),
                () => proteoformFamilies.fill_proteoform_families()
            );
            if (Lollipop.neucode_labeled) neuCodePairs.GraphNeuCodePairs();
        }
    

        // MISCELLANEOUS
        private void clear_lists()
        {
            Lollipop.raw_experimental_components.Clear();
            Lollipop.raw_neucode_pairs.Clear();
            Lollipop.proteoform_community.experimental_proteoforms.Clear();
            Lollipop.proteoform_community.theoretical_proteoforms.Clear();
            Lollipop.et_relations.Clear();
            Lollipop.et_peaks.Clear();
            Lollipop.ee_relations.Clear();
            Lollipop.ee_peaks.Clear();
            Lollipop.proteoform_community.families.Clear();
            Lollipop.ed_relations.Clear();
            Lollipop.proteoform_community.relations_in_peaks.Clear();
            Lollipop.proteoform_community.delta_mass_peaks.Clear();
            Lollipop.ef_relations.Clear();
            Lollipop.proteoform_community.decoy_proteoforms.Clear();
        }

        public void enable_neuCodeProteoformPairsToolStripMenuItem(bool setting)
        {
            neuCodeProteoformPairsToolStripMenuItem.Enabled = setting;
        }
    }
}
