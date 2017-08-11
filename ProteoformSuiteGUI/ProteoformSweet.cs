using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        public List<ISweetForm> forms = new List<ISweetForm>();
        public static bool run_when_form_loads;

        #endregion Public Fields

        #region Private Fields

        FolderBrowserDialog resultsFolderOpen = new FolderBrowserDialog();
        OpenFileDialog methodFileOpen = new OpenFileDialog();
        SaveFileDialog methodFileSave = new SaveFileDialog();
        OpenFileDialog openResults = new OpenFileDialog();
        SaveFileDialog saveResults = new SaveFileDialog();
        SaveFileDialog saveExcelDialog = new SaveFileDialog();
        ISweetForm current_form;

        #endregion Private Fields

        #region Public Constructor

        public ProteoformSweet()
        {
            InitializeComponent();
            InitializeForms();
            WindowState = FormWindowState.Maximized;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
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
            forms = new List<ISweetForm>
            {
                loadDeconvolutionResults,
                rawExperimentalComponents,
                neuCodePairs,
                aggregatedProteoforms,
                theoreticalDatabase,
                experimentalTheoreticalComparison,
                experimentExperimentComparison,
                proteoformFamilies,
                quantification,
                resultsSummary
            };

            foreach (Form form in forms)
            {
                form.MdiParent = this;
            }
        }

        private void showForm(Form form)
        {
            form.Show();
            form.WindowState = FormWindowState.Maximized;
            current_form = form as ISweetForm;
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
            rawExperimentalComponents.InitializeParameterSet();
        }

        private void neuCodeProteoformPairsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(neuCodePairs);
            if (neuCodePairs.ReadyToRunTheGamut())
                neuCodePairs.RunTheGamut(); // There's no update/run button in NeuCodePairs, so just fill the tables
        }

        private void aggregatedProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(aggregatedProteoforms);
        }

        private void theoreticalProteoformDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theoreticalDatabase.reload_database_list();
            showForm(theoreticalDatabase);
        }

        private void experimentTheoreticalComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(experimentalTheoreticalComparison);
        }

        private void experimentExperimentComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(experimentExperimentComparison);
        }

        private void proteoformFamilyAssignmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(proteoformFamilies);
            proteoformFamilies.initialize_every_time();
        }

        private void quantificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            quantification.initialize_every_time();
            showForm(quantification);
        }

        private void resultsSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resultsSummary.InitializeParameterSet();
            resultsSummary.create_summary();
            showForm(resultsSummary);
        }

        #endregion RESULTS TOOL STRIP Private Methods

        #region FILE TOOL STRIP Private Methods

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
                file.WriteLine(Sweet.save_method());
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
                DialogResult d4 = MessageBox.Show("Add files at the listed paths if they still exist?", "Full Run", MessageBoxButtons.YesNoCancel);
                if (d4 == DialogResult.Cancel) return false;
                if (!open_method(File.ReadAllLines(method_filename), d4 == DialogResult.Yes))
                {
                    MessageBox.Show("Method file was not loaded succesffully.");
                    return false;
                };
                loadDeconvolutionResults.InitializeParameterSet(); // updates the textbox
                if (loadDeconvolutionResults.ReadyToRunTheGamut())
                    loadDeconvolutionResults.RunTheGamut(); // updates the dgvs
                return true;
            }
            return false;
        }

        public bool open_method(string[] lines, bool add_files)
        {
            bool method_file_success = Sweet.open_method(String.Join(Environment.NewLine, lines), add_files, out string warning);
            if (warning.Length > 0 && MessageBox.Show("WARNING" + Environment.NewLine + Environment.NewLine + warning, "Open Method", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                return false;
            foreach (ISweetForm form in forms) form.InitializeParameterSet();
            return method_file_success;
        }

        public Stopwatch full_run()
        {
            forms[1].ClearListsTablesFigures(true); // clear forms following load deconvolution results

            DialogResult d3 = MessageBox.Show("Use presets for this Full Run?", "Full Run", MessageBoxButtons.YesNoCancel);
            if (d3 == DialogResult.Yes)
            {
                DialogResult dr = methodFileOpen.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    string filepath = methodFileOpen.FileName;
                    DialogResult d4 = MessageBox.Show("Add files at the listed paths if they still exist?", "Full Run", MessageBoxButtons.YesNoCancel);
                    if (d4 == DialogResult.Cancel) return null;

                    if (!open_method(File.ReadAllLines(filepath), d4 == DialogResult.Yes))
                    {
                        MessageBox.Show("Error in method file. Generate a new method file.");
                        return null;
                    };
                }
                else if (dr == DialogResult.Cancel) return null;
            }
            else if (d3 == DialogResult.Cancel) return null;

            loadDeconvolutionResults.FillTablesAndCharts(); // updates the filelists in form

            // Check that there are input files
            if (Sweet.lollipop.input_files.Count == 0)
            {
                MessageBox.Show("Please load in deconvolution result files in order to use load and run.", "Full Run");
                return null;
            }

            // Check that theoretical database(s) are present
            if (!Sweet.lollipop.theoretical_database.ready_to_make_database(Environment.CurrentDirectory))
            {
                if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.ProteinDatabase).Count() <= 0)
                {
                    MessageBox.Show("Please list at least one protein database.", "Full Run");
                    return null;
                }
                else
                {
                    DialogResult d1 = MessageBox.Show("No PTM list is listed.\n\nWill now download the default PTM list from UniProt and use it for the Full Run.", "Full Run", MessageBoxButtons.OKCancel);
                    if (d1 == DialogResult.OK)
                    {
                        Sweet.lollipop.enter_uniprot_ptmlist(Environment.CurrentDirectory);
                        if (loadDeconvolutionResults.ReadyToRunTheGamut())
                            loadDeconvolutionResults.RunTheGamut(); // updates the dgvs
                    }
                    else return null;
                }
            }

            // Option to choose a result folder
            if (Sweet.lollipop.results_folder == "")
            {
                DialogResult d2 = MessageBox.Show("Choose a results folder for this Full Run?", "Full Run", MessageBoxButtons.YesNoCancel);
                if (d2 == DialogResult.Yes)
                {
                    FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
                    DialogResult dr = folderBrowser.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        string temp_folder_path = folderBrowser.SelectedPath;
                        Sweet.lollipop.results_folder = temp_folder_path;
                        loadDeconvolutionResults.InitializeParameterSet(); // updates the textbox
                    }
                    else if (dr == DialogResult.Cancel) return null;
                }
                else if (d2 == DialogResult.Cancel) return null;
            }
            else
            {
                DialogResult d2 = MessageBox.Show("Would you like to save results of this Full Run to " + Sweet.lollipop.results_folder + "?", "Full Run", MessageBoxButtons.YesNoCancel);
                if (d2 == DialogResult.No)
                    Sweet.lollipop.results_folder = "";
                else if (d2 == DialogResult.Cancel)
                    return null;
            }

            // Run the program
            Cursor = Cursors.WaitCursor;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (ISweetForm sweet in forms)
            {
                if (sweet.ReadyToRunTheGamut())
                    sweet.RunTheGamut();
            }

            // Save the results
            resultsSummary.InitializeParameterSet();
            if (Sweet.lollipop.results_folder != "")
            {
                string timestamp = Sweet.time_stamp();
                ResultsSummaryGenerator.save_all(Sweet.lollipop.results_folder, timestamp, resultsSummary.get_go_analysis(), resultsSummary.get_tusher_analysis());
                save_all_plots(Sweet.lollipop.results_folder, timestamp);
                using (StreamWriter file = new StreamWriter(Path.Combine(Sweet.lollipop.results_folder, "presets_" + timestamp + ".xml")))
                    file.WriteLine(Sweet.save_method());
            }

            //Program ran successfully
            stopwatch.Stop();
            Cursor = Cursors.Default;
            return stopwatch;
        }

        #endregion METHOD TOOL STRIP Private Methods

        #region Export Table as Excel File -- Private Methods

        private void exportTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<DataGridView> grid_views = current_form.GetDGVs();

            if (grid_views == null)
            {
                MessageBox.Show("There is no table on this page to export. Please navigate to another page with the Results tab.");
                return;
            }
            
            DGVExcelWriter writer = new DGVExcelWriter();
            writer.ExportToExcel(grid_views, (current_form as Form).Name);
            SaveExcelFile(writer, (current_form as Form).Name + "_table.xlsx");
        }


        private void exportAllTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DGVExcelWriter writer = new DGVExcelWriter();
            Parallel.ForEach(forms, form =>
            {
                List<DataGridView> grid_views = form.GetDGVs();
                writer.ExportToExcel(grid_views, (form as Form).Name);
            });
            SaveExcelFile(writer, (current_form as Form).MdiParent.Name + "_table.xlsx");
        }

        private void SaveExcelFile(DGVExcelWriter writer, string filename)
        {
            saveExcelDialog.FileName = filename;
            DialogResult dr = saveExcelDialog.ShowDialog();
            if (dr == DialogResult.OK)
            {
                MessageBox.Show(writer.SaveToExcel(saveExcelDialog.FileName));
            }
            else return;
        }

        #endregion Export Table as Excel File -- Private Methods

        #region Results Summary Methods

        public void save_all_plots(string folder, string timestamp)
        {
            if (Sweet.lollipop.raw_neucode_pairs.Count > 0) save_as_png(neuCodePairs.ct_IntensityRatio, folder, "NeuCode_IntensityRatios_", timestamp);
            if (Sweet.lollipop.raw_neucode_pairs.Count > 0) save_as_png(neuCodePairs.ct_LysineCount, folder, "NeuCode_LysineCounts_", timestamp);
            if (Sweet.lollipop.et_relations.Count > 0) save_as_png(experimentalTheoreticalComparison.ct_ET_Histogram, folder, "ExperimentalTheoretical_MassDifferences_", timestamp);
            if (Sweet.lollipop.ee_relations.Count > 0) save_as_png(experimentExperimentComparison.ct_EE_Histogram, folder, "ExperimentalExperimental_MassDifferences_", timestamp);
            if (Sweet.lollipop.qVals.Count > 0) save_as_png(quantification.ct_proteoformIntensities, folder, "QuantifiedProteoform_Intensities_", timestamp);
            if (Sweet.lollipop.qVals.Count > 0) save_as_png(quantification.ct_relativeDifference, folder, "QuantifiedProteoform_Tusher2001Plot_", timestamp);
            if (Sweet.lollipop.qVals.Count > 0) save_as_png(quantification.ct_volcano_logFold_logP, folder, "QuantifiedProteoform_VolcanoPlot_", timestamp);
        }

        private void save_as_png(Chart ct, string folder, string prefix, string timestamp)
        {
            ct.SaveImage(Path.Combine(folder, prefix + timestamp + ".png"), ChartImageFormat.Png);
        }

        #endregion Results Summary Methods

    }
}
