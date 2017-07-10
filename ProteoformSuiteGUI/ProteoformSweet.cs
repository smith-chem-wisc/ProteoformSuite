using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
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
                Sweet.open_method(File.ReadAllLines(method_filename));
                return true;
            }
            return false;
        }

        private void loadRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Sweet.lollipop.input_files.Count == 0)
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
            if (!Sweet.lollipop.theoretical_database.ready_to_make_database(Environment.CurrentDirectory))
            {
                if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.ProteinDatabase).Count() <= 0)
                {
                    MessageBox.Show("Please list at least one protein database.", "Full Run");
                    return false;
                }
                else
                {
                    DialogResult d = MessageBox.Show("No PTM list is listed.\n\nWill now download the default PTM list from UniProt and use it for the Full Run.", "Full Run", MessageBoxButtons.OKCancel);
                    if (d == DialogResult.OK)
                    {
                        Sweet.lollipop.enter_uniprot_ptmlist();
                        if (loadDeconvolutionResults.ReadyToRunTheGamut())
                            loadDeconvolutionResults.RunTheGamut(); // updates the dgvs
                    }
                    else return false;
                }
                
            }

            if (Sweet.lollipop.results_folder == "")
            {
                DialogResult d = MessageBox.Show("Choose a results folder for this Full Run?", "Full Run", MessageBoxButtons.YesNoCancel);
                if (d == DialogResult.Yes)
                {
                    FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
                    DialogResult dr = folderBrowser.ShowDialog();
                    if (dr == DialogResult.OK)
                    {
                        string temp_folder_path = folderBrowser.SelectedPath;
                        Sweet.lollipop.results_folder = temp_folder_path;
                        loadDeconvolutionResults.InitializeParameterSet(); // updates the textbox
                    }
                    else if (dr == DialogResult.Cancel) return false;
                }
                else if (d == DialogResult.Cancel) return false;
            }

            Cursor = Cursors.WaitCursor;
            foreach (ISweetForm sweet in forms)
            {
                if (sweet.ReadyToRunTheGamut())
                    sweet.RunTheGamut();
            }
            string timestamp = Sweet.time_stamp();
            ResultsSummaryGenerator.save_all(Sweet.lollipop.results_folder, timestamp);
            save_all_plots(Sweet.lollipop.results_folder, timestamp);
            Cursor = Cursors.Default;
            return true;
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
                writer.SaveToExcel(saveExcelDialog.FileName);
                MessageBox.Show("Successfully exported table.");
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

        #region Public Method

        public void clear_lists()
        {
            Sweet.lollipop.raw_experimental_components.Clear();
            Sweet.lollipop.raw_neucode_pairs.Clear();
            Sweet.lollipop.target_proteoform_community = new ProteoformCommunity();
            Sweet.lollipop.decoy_proteoform_communities.Clear();
            Sweet.lollipop.et_relations.Clear();
            Sweet.lollipop.et_peaks.Clear();
            Sweet.lollipop.ee_relations.Clear();
            Sweet.lollipop.ee_peaks.Clear();
            Sweet.lollipop.ed_relations.Clear();
            Sweet.lollipop.ef_relations.Clear();
        }

        #endregion Public Method

    }
}
