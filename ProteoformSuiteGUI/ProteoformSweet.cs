using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using ProteoformSuiteInternal;
using System.Windows.Forms.DataVisualization.Charting;

namespace ProteoformSuiteGUI
{
    public partial class ProteoformSweet : Form
    {

        #region Public Fields

        public LoadDeconvolutionResults loadDeconvolutionResults = new LoadDeconvolutionResults();
        public RawExperimentalComponents rawExperimentalComponents = new RawExperimentalComponents();
        public NeuCodePairs neuCodePairs = new NeuCodePairs();
        public AggregatedProteoforms aggregatedProteoforms = new AggregatedProteoforms();
        public TheoreticalDatabase theoreticalDatabase = new TheoreticalDatabase();
        public ExperimentTheoreticalComparison experimentalTheoreticalComparison = new ExperimentTheoreticalComparison();
        public ExperimentExperimentComparison experimentExperimentComparison = new ExperimentExperimentComparison();
        public ProteoformFamilies proteoformFamilies = new ProteoformFamilies();
        public Quantification quantification = new Quantification();
        public ResultsSummary resultsSummary = new ResultsSummary();
        public static bool run_when_form_loads;

        #endregion Public Fields

        #region Private Fields

        FolderBrowserDialog resultsFolderOpen = new FolderBrowserDialog();
        OpenFileDialog methodFileOpen = new OpenFileDialog();
        SaveFileDialog methodFileSave = new SaveFileDialog();
        OpenFileDialog openResults = new OpenFileDialog();
        SaveFileDialog saveResults = new SaveFileDialog();
        SaveFileDialog saveExcelDialog = new SaveFileDialog();
        List<Form> forms;
        Form current_form;

        #endregion Private Fields

        #region Public Constructor

        public ProteoformSweet()
        {
            InitializeComponent();
            InitializeForms();
            this.WindowState = FormWindowState.Maximized;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            showForm(loadDeconvolutionResults);
            methodFileOpen.Filter = "Method XML File (*.xml)| *.xml";
            methodFileSave.DefaultExt = ".xml";
            methodFileSave.Filter = "Method XML File (*.xml)| *.xml";
            saveExcelDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            saveExcelDialog.DefaultExt = ".xlsx";
            openResults.Filter = "Proteoform Suite Save State (*.sweet)| *.sweet";
            saveResults.Filter = "Proteoform Suite Save State (*.sweet)| *.sweet";
            saveResults.DefaultExt = ".sweet";
        }

        #endregion Public Constructor

        #region Private Setup Methods

        private void InitializeForms()
        {
            forms = new List<Form>(new Form[] {
                loadDeconvolutionResults, rawExperimentalComponents, neuCodePairs, aggregatedProteoforms,
                theoreticalDatabase, experimentalTheoreticalComparison, experimentExperimentComparison,
                proteoformFamilies, quantification, resultsSummary
            });

            foreach (Form form in forms)
            {
                form.MdiParent = this;
            }
        }

        private void showForm(Form form)
        {
            form.Show();
            form.WindowState = FormWindowState.Maximized;
            current_form = form;
        }

        #endregion Private Setup Methods

        #region RESULTS TOOL STRIP Public Method

        public void enable_neuCodeProteoformPairsToolStripMenuItem(bool setting)
        {
            neuCodeProteoformPairsToolStripMenuItem.Enabled = setting;
        }

        #endregion RESULTS TOOL STRIP Public Method

        #region RESULTS TOOL STRIP Private Methods

        private void loadDeconvolutionResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(loadDeconvolutionResults);
        }

        private void rawExperimentalProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(rawExperimentalComponents);
            rawExperimentalComponents.initialize_every_time();
        }

        private void neuCodeProteoformPairsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(neuCodePairs);
            neuCodePairs.display_neucode_pairs();
        }

        private void aggregatedProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(aggregatedProteoforms);
            if (run_when_form_loads) aggregatedProteoforms.aggregate_proteoforms();
        }

        private void theoreticalProteoformDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theoreticalDatabase.reload_database_list();
            showForm(theoreticalDatabase);
        }

        private void experimentTheoreticalComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(experimentalTheoreticalComparison);
            if (run_when_form_loads) experimentalTheoreticalComparison.compare_et();
        }

        private void experimentExperimentComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(experimentExperimentComparison);
            if (run_when_form_loads) experimentExperimentComparison.compare_ee();
        }

        private void proteoformFamilyAssignmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(proteoformFamilies);
            proteoformFamilies.construct_families();
        }

        private void quantificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (run_when_form_loads) quantification.perform_calculations();
            quantification.initialize_every_time();
            showForm(quantification);
        }

        private void resultsSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resultsSummary.create_summary();
            showForm(resultsSummary);
        }

        #endregion RESULTS TOOL STRIP Private Methods

        #region FILE TOOL STRIP Private Methods

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            if (openResults.ShowDialog() == DialogResult.OK)
                SaveState.load_all_results(openResults.FileName);

            loadDeconvolutionResults.InitializeSettings();

            rawExperimentalComponents.FillComponentsTable();

            aggregatedProteoforms.InitializeSettings();
            aggregatedProteoforms.FillAggregatesTable();

            Cursor = Cursors.Default;
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            if (saveResults.ShowDialog() == DialogResult.OK)
                SaveState.save_all_results(saveResults.FileName);
            Cursor = Cursors.Default;
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("printToolStripMenuItem_Click");
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        #endregion FILE TOOL STRIP Private Methods

        #region METHOD TOOL STRIP Private Methods

        private void saveMethodToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (methodFileSave.ShowDialog() == DialogResult.OK)
                saveMethod(methodFileSave.FileName);
        }

        private void saveMethod(string method_filename)
        {
            using (StreamWriter file = new StreamWriter(method_filename))
                file.WriteLine(SaveState.save_method());
        }

        private void loadSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            load_method();
        }

        private bool load_method()
        {
            DialogResult dr = methodFileOpen.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string method_filename = methodFileOpen.FileName;
                SaveState.open_method(File.ReadAllLines(method_filename));
                return true;
            }
            return false;
        }

        private void loadRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SaveState.lollipop.input_files.Count == 0)
            {
                MessageBox.Show("Please load in deconvolution result files in order to use load and run.");
                return;
            }
            var result = MessageBox.Show("Choose a method file.", "Method Load and Run", MessageBoxButtons.OKCancel);
            if (result == DialogResult.Cancel) return;

            if (!load_method()) return;
            MessageBox.Show("Successfully loaded method. Will run the method now.");

            if (full_run()) MessageBox.Show("Successfully ran method. Feel free to explore using the Results menu.");
            else MessageBox.Show("Method did not successfully run.");
        }

        public bool full_run()
        {
            clear_lists();
            if (!SaveState.lollipop.theoretical_database.ready_to_make_database(Environment.CurrentDirectory))
            {
                MessageBox.Show("Please list at least one protein database. Also, please make sure it has modifications listed (mzLibXml format) or to include and at least one PTM list.");
                return false;
            }

            this.Cursor = Cursors.WaitCursor;
            rawExperimentalComponents.load_raw_components(); //also loads the theoretical database, now
            neuCodePairs.preloaded = true;
            aggregatedProteoforms.aggregate_proteoforms();
            this.enable_neuCodeProteoformPairsToolStripMenuItem(SaveState.lollipop.neucode_labeled);
            this.Cursor = Cursors.Default;
            return true;
        }

        #endregion METHOD TOOL STRIP Private Methods

        #region Export Table as Excel File -- Private Methods

        private void exportTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            export_table();
        }

        private void export_table()
        {
            if (current_form == rawExperimentalComponents)
            {
                SaveExcelFile(new List<DataGridView>() { rawExperimentalComponents.GetDGV() }, "raw_experimental_components_table.xlsx");
            }

            if (current_form == neuCodePairs)
            {
                SaveExcelFile(new List<DataGridView>() { neuCodePairs.GetDGV() }, "neucode_pairs_table.xlsx");
            }

            if (current_form == aggregatedProteoforms)
            {
                SaveExcelFile(new List<DataGridView>() { aggregatedProteoforms.GetDGV() }, "aggregated_proteoforms_table.xlsx");
            }

            if (current_form == theoreticalDatabase)
            {
                SaveExcelFile(new List<DataGridView>() { theoreticalDatabase.GetDGV() }, "theoretical_database_table.xlsx");
            }

            if (current_form == experimentalTheoreticalComparison)
            {
                SaveExcelFile(new List<DataGridView>() { experimentalTheoreticalComparison.GetETRelationsDGV(), experimentalTheoreticalComparison.GetETPeaksDGV() }, "experimental_theoretical_comparison_table.xlsx");
            }

            if (current_form == experimentExperimentComparison)
            {
                SaveExcelFile(new List<DataGridView>() { experimentExperimentComparison.GetEERelationDGV(), experimentExperimentComparison.GetEEPeaksDGV() }, "experiment_experiment_comparison_table.xlsx");
            }

            if (current_form == proteoformFamilies)
            {
                SaveExcelFile(new List<DataGridView>() { proteoformFamilies.GetDGV() }, "proteoform_families_table.xlsx");
            }

            if (current_form == quantification)
            {
                SaveExcelFile(new List<DataGridView>() { quantification.Get_GoTerms_DGV(), quantification.Get_quant_results_DGV() }, "quantification_table.xlsx");
            }
        }

        private void SaveExcelFile(List<DataGridView> dgvs, string filename)
        {
            saveExcelDialog.FileName = filename;
            DialogResult dr = this.saveExcelDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                DGVExcelWriter writer = new DGVExcelWriter();
                writer.ExportToExcel(dgvs, saveExcelDialog.FileName);
                MessageBox.Show("Successfully exported table.");
            }
            else return;
        }

        #endregion Export Table as Excel File -- Private Methods

        #region Results Summary Methods

        public void save_all_plots(string folder, string timestamp)
        {
            if (SaveState.lollipop.raw_neucode_pairs.Count > 0) save_as_png(neuCodePairs.ct_IntensityRatio, folder, "NeuCode_IntensityRatios_", timestamp);
            if (SaveState.lollipop.raw_neucode_pairs.Count > 0) save_as_png(neuCodePairs.ct_LysineCount, folder, "NeuCode_LysineCounts_", timestamp);
            if (SaveState.lollipop.et_relations.Count > 0) save_as_png(experimentalTheoreticalComparison.ct_ET_Histogram, folder, "ExperimentalTheoretical_MassDifferences_", timestamp);
            if (SaveState.lollipop.ee_relations.Count > 0) save_as_png(experimentExperimentComparison.ct_EE_Histogram, folder, "ExperimentalExperimental_MassDifferences_", timestamp);
            if (SaveState.lollipop.qVals.Count > 0) save_as_png(quantification.ct_proteoformIntensities, folder, "QuantifiedProteoform_Intensities_", timestamp);
            if (SaveState.lollipop.qVals.Count > 0) save_as_png(quantification.ct_relativeDifference, folder, "QuantifiedProteoform_Tusher2001Plot_", timestamp);
            if (SaveState.lollipop.qVals.Count > 0) save_as_png(quantification.ct_volcano_logFold_logP, folder, "QuantifiedProteoform_VolcanoPlot_", timestamp);
        }

        private void save_as_png(Chart ct, string folder, string prefix, string timestamp)
        {
            ct.SaveImage(Path.Combine(folder, prefix + timestamp + ".png"), ChartImageFormat.Png);
        }

        #endregion Results Summary Methods

        #region Public Method

        public void clear_lists()
        {
            SaveState.lollipop.raw_experimental_components.Clear();
            SaveState.lollipop.raw_neucode_pairs.Clear();
            SaveState.lollipop.proteoform_community.experimental_proteoforms = new ExperimentalProteoform[0];
            SaveState.lollipop.proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            SaveState.lollipop.et_relations.Clear();
            SaveState.lollipop.et_peaks.Clear();
            SaveState.lollipop.ee_relations.Clear();
            SaveState.lollipop.ee_peaks.Clear();
            SaveState.lollipop.proteoform_community.families.Clear();
            SaveState.lollipop.ed_relations.Clear();
            SaveState.lollipop.proteoform_community.relations_in_peaks.Clear();
            SaveState.lollipop.proteoform_community.delta_mass_peaks.Clear();
            SaveState.lollipop.ef_relations.Clear();
            SaveState.lollipop.proteoform_community.decoy_proteoforms.Clear();
        }

        #endregion Public Method


    }
}
