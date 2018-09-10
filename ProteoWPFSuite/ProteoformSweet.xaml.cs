using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using ProteoformSuiteInternal;
using System.IO;
using System.Diagnostics;
using System.Windows.Input;
/*
* - MDI Form replaced by user control
* - MDI Parent replaced by the getparentwindow (private)
*/
namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ProteoformSweet : UserControl
    {

        #region Public Fields
        
        //MDI Iterface
        public HashSet<String> MDIChildren
        {
            get;
            set;
        }
        
        //original
        public LoadResults loadResults = new LoadResults();
        public RawExperimentalComponents rawExperimentalComponents = new RawExperimentalComponents();
        public NeuCodePairs neuCodePairs = new NeuCodePairs();
        public AggregatedProteoforms aggregatedProteoforms = new AggregatedProteoforms();
        public TheoreticalDatabase theoreticalDatabase = new TheoreticalDatabase();
        public ExperimentTheoreticalComparison experimentalTheoreticalComparison = new ExperimentTheoreticalComparison();
        public ExperimentExperimentComparison experimentExperimentComparison = new ExperimentExperimentComparison();
        public ProteoformFamilies proteoformFamilies = new ProteoformFamilies();
        public Quantification quantification = new Quantification();
        public TopDown topDown = new TopDown();
        public IdentifiedProteoforms identifiedProteoforms = new IdentifiedProteoforms();
        public ResultsSummary resultsSummary = new ResultsSummary();
        public List<ISweetForm> forms = new List<ISweetForm>(); //contains a list of user controls
        #endregion

        #region Private Fields
        System.Windows.Forms.FolderBrowserDialog resultsFolderOpen = new System.Windows.Forms.FolderBrowserDialog();
        OpenFileDialog methodFileOpen = new OpenFileDialog();
        SaveFileDialog methodFileSave = new SaveFileDialog();
        OpenFileDialog openResults = new OpenFileDialog();
        SaveFileDialog saveResults = new SaveFileDialog();
        ISweetForm current_form;
        SaveFileDialog saveExcelDialog;
        #endregion

        #region Public Constructor

        public ProteoformSweet()
        {
            InitializeComponent();/*
            InitializeForms();
            WindowState = FormWindowState.Maximized;
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            loadResults.InitializeParameterSet();
            showForm(loadResults);
            methodFileOpen.Filter = "Method XML File (*.xml)| *.xml";
            methodFileSave.DefaultExt = ".xml";
            methodFileSave.Filter = "Method XML File (*.xml)| *.xml";
            saveExcelDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            saveExcelDialog.DefaultExt = ".xlsx";
            openResults.Filter = "Proteoform Suite Save State (*.sweet)| *.sweet";
            saveResults.Filter = "Proteoform Suite Save State (*.sweet)| *.sweet";
            saveResults.DefaultExt = ".sweet";
            loadResults.Focus();
            Form.ActiveForm.Show();
            LoadResults.ActiveForm.Show();*/

        }

        #endregion Public Constructor

        #region Private Setup Methods
        private void initializeForms()
        {
            forms = new List<ISweetForm>
            {
                loadResults,
                theoreticalDatabase,
                neuCodePairs
            };
            foreach(UserControl uc in forms)
            {
                (uc as ITabbedMDI).MDIParent = this; //set the mdi parent
            }
        }
        private void showForm(UserControl form)
        {
            /*form.Show();
            form.WindowState = FormWindowState.Maximized;*/
            (form as ITabbedMDI).MDIParent = this;
            Window wdo = new Window
            {
                Title = "Proteo Display",
                Content = new ProteoformSweet(),
                MinHeight = 450,
                MinWidth = 800
            };
            current_form = form as ISweetForm;
        }
        #endregion

        #region RESULTS TOOL STRIP Public Method

        public void enable_neuCodeProteoformPairsToolStripMenuItem(bool setting)
        {
            neuCodeProteoformPairsToolStripMenuItem.IsEnabled = setting;
        }

        public void enable_quantificationToolStripMenuItem(bool setting)
        {
            quantificationToolStripMenuItem.IsEnabled = setting;
        }

        public void enable_topDownToolStripMenuItem(bool setting)
        {
            topdownResultsToolStripMenuItem.IsEnabled = setting;
        }

        #endregion RESULTS TOOL STRIP Public Method

        #region RESULTS TOOL STRIP Private Methods

        private void LoadResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            loadResults.InitializeParameterSet();
            showForm(loadResults);
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
                neuCodePairs.RunTheGamut(false); // There's no update/run button in NeuCodePairs, so just fill the tables
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
        private void topdownResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(topDown);
        }

        private void identifiedProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(identifiedProteoforms);
            if (identifiedProteoforms.ReadyToRunTheGamut()) identifiedProteoforms.RunTheGamut(false);
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
            Window.GetWindow(this).Close();
        }

        #endregion FILE TOOL STRIP Private Methods

        #region METHOD TOOL STRIP Private Methods

        private void saveMethodToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if (methodFileSave.ShowDialog() == true)
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
            bool? dr = methodFileOpen.ShowDialog();
            if (dr == true)
            {
                string method_filename = methodFileOpen.FileName;
                MessageBoxResult d4 = MessageBox.Show("Add files at the listed paths if they still exist?", "Full Run", MessageBoxButton.YesNoCancel);
                if (d4 == MessageBoxResult.Cancel) return false;
                if (!open_method(method_filename, File.ReadAllLines(method_filename), d4 == MessageBoxResult.Yes))
                {
                    MessageBox.Show("Method file was not loaded succesffully.");
                    return false;
                };
                loadResults.InitializeParameterSet(); // updates the textbox
                if (loadResults.ReadyToRunTheGamut())
                    loadResults.RunTheGamut(false); // updates the dgvs
                return true;
            }
            return false;
        }

        public bool open_method(string methodFilePath, string[] lines, bool add_files)
        {
            bool method_file_success = Sweet.open_method(methodFilePath, string.Join(Environment.NewLine, lines), add_files, out string warning);
            if (warning.Length > 0 && MessageBox.Show("WARNING" + Environment.NewLine + Environment.NewLine + warning, "Open Method", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                return false;
            foreach (ISweetForm form in forms) form.InitializeParameterSet();
            return method_file_success;
        }

        public Stopwatch full_run()
        {
            forms[1].ClearListsTablesFigures(true); // clear forms following load deconvolution results

            MessageBoxResult d3 = MessageBox.Show("Use presets for this Full Run?", "Full Run", MessageBoxButton.YesNoCancel);
            if (d3 == MessageBoxResult.Yes)
            {
                bool? dr = methodFileOpen.ShowDialog();
                if (dr == true)
                {
                    string filepath = methodFileOpen.FileName;
                    MessageBoxResult d4 = MessageBox.Show("Add files at the listed paths if they still exist?", "Full Run", MessageBoxButton.YesNoCancel);
                    if (d4 == MessageBoxResult.Cancel) return null;

                    if (!open_method(filepath, File.ReadAllLines(filepath), d4 == MessageBoxResult.Yes))
                    {
                        return null;
                    };
                }
                else if (dr == false) return null;
            }
            else if (d3 == MessageBoxResult.Cancel) return null;

            loadResults.FillTablesAndCharts(); // updates the filelists in form

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
                    MessageBoxResult d1 = MessageBox.Show("No PTM list is listed.\n\nWill now download the default PTM list from UniProt and use it for the Full Run.", "Full Run", MessageBoxButton.OKCancel);
                    if (d1 == MessageBoxResult.OK)
                    {
                        Lollipop.enter_uniprot_ptmlist(Environment.CurrentDirectory);
                        if (loadResults.ReadyToRunTheGamut())
                            loadResults.RunTheGamut(true); // updates the dgvs
                    }
                    else return null;
                }
            }

            // Option to choose a result folder
            if (Sweet.lollipop.results_folder == "")
            {
                MessageBoxResult d2 = MessageBox.Show("Choose a results folder for this Full Run?", "Full Run", MessageBoxButton.YesNoCancel);
                if (d2 == MessageBoxResult.Yes)
                {
                    System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
                    System.Windows.Forms.DialogResult dr = folderBrowser.ShowDialog();
                    if (dr == System.Windows.Forms.DialogResult.OK)
                    {
                        string temp_folder_path = folderBrowser.SelectedPath;
                        Sweet.lollipop.results_folder = temp_folder_path;
                        loadResults.InitializeParameterSet(); // updates the textbox
                    }
                    else if (dr == System.Windows.Forms.DialogResult.Cancel) return null;
                }
                else if (d2 == MessageBoxResult.Cancel) return null;
            }
            else
            {
                MessageBoxResult d2 = MessageBox.Show("Would you like to save results of this Full Run to " + Sweet.lollipop.results_folder + "?", "Full Run", MessageBoxButton.YesNoCancel);
                if (d2 == MessageBoxResult.No)
                    Sweet.lollipop.results_folder = "";
                else if (d2 == MessageBoxResult.Cancel)
                    return null;
            }

            // Run the program
            Mouse.OverrideCursor = Cursors.Wait;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (ISweetForm sweet in forms)
            {
                if (sweet.ReadyToRunTheGamut())
                    sweet.RunTheGamut(true);
            }

            // Save the results
            resultsSummary.InitializeParameterSet();
            if (Sweet.lollipop.results_folder != "")
            {
                string timestamp = Sweet.time_stamp();
                ResultsSummaryGenerator.save_all(Sweet.lollipop.results_folder, timestamp, resultsSummary.get_go_analysis(), resultsSummary.get_tusher_analysis());
                //save_all_plots(Sweet.lollipop.results_folder, timestamp);
                using (StreamWriter file = new StreamWriter(Path.Combine(Sweet.lollipop.results_folder, "presets_" + timestamp + ".xml")))
                    file.WriteLine(Sweet.save_method());
            }
            List<string> warning_methods = new List<string>() { "Warning:" };
            if (BottomUpReader.bottom_up_PTMs_not_in_dictionary.Count() > 0)
            {
                warning_methods.Add("The following PTMs in the .mzid file were not matched with any PTMs in the theoretical database: ");
                warning_methods.Add(string.Join(", ", BottomUpReader.bottom_up_PTMs_not_in_dictionary.Distinct()));
            }
            if (Sweet.lollipop.topdownReader.topdown_ptms.Count > 0)
            {
                warning_methods.Add("Top-down proteoforms with the following modifications were not matched to a modification in the theoretical PTM list: ");
                warning_methods.Add(string.Join(", ", Sweet.lollipop.topdownReader.topdown_ptms.Distinct()));
            }
            if (Sweet.lollipop.topdown_proteoforms.Count(t => !t.accepted) > 0)
            {
                warning_methods.Add("Top-down proteoforms with the following accessions were not matched to a theoretical proteoform in the theoretical database: ");
                warning_methods.Add(string.Join(", ", Sweet.lollipop.topdown_proteoforms.Where(t => !t.accepted).Select(t => t.accession.Split('_')[0]).Distinct()));
            }
            if (warning_methods.Count > 1)
            {
                MessageBox.Show(string.Join("\n\n", warning_methods));
            }
            //Program ran successfully
            stopwatch.Stop();
            Mouse.OverrideCursor = null;
            return stopwatch;
        }

        #endregion METHOD TOOL STRIP Private Methods

        #region Export Table as Excel File -- Private Methods

        private void exportTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<DataTable> data_tables = current_form.SetTables();

            if (data_tables == null)
            {
                MessageBox.Show("There is no table on this page to export. Please navigate to another page with the Results tab.");
                return;
            }

            ProteoformSuiteGUI.ExcelWriter writer = new ProteoformSuiteGUI.ExcelWriter();
            writer.ExportToExcel(data_tables, (current_form as UserControl).Name);
            SaveExcelFile(writer, (current_form as UserControl).Name + "_table.xlsx");
        }


        private void exportAllTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ProteoformSuiteGUI.ExcelWriter writer = new ProteoformSuiteGUI.ExcelWriter();
            if (MessageBox.Show("Will prepare for export. This may take a while.", "Export Data", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
            Parallel.ForEach(forms, form => form.SetTables());
            writer.BuildHyperlinkSheet(forms.Select(sweet => new Tuple<string, List<DataTable>>((sweet as UserControl).Name, sweet.DataTables)).ToList());
            Parallel.ForEach(forms, form => writer.ExportToExcel(form.DataTables, (form as UserControl).Name));
            if (MessageBox.Show("Finished preparing. Ready to save? This may take a while.", "Export Data", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
            SaveExcelFile(writer, (current_form as ITabbedMDI).MDIParent.Name + "_table.xlsx");
        }

        private void SaveExcelFile(ProteoformSuiteGUI.ExcelWriter writer, string filename)
        {
            saveExcelDialog.FileName = filename;
            bool? dr = saveExcelDialog.ShowDialog();
            if (dr == true)
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
            //TODO: Finish the following
            //if (Sweet.lollipop.et_relations.Count > 0) save_as_png(experimentalTheoreticalComparison.ct_ET_Histogram, folder, "ExperimentalTheoretical_MassDifferences_", timestamp);
            //if (Sweet.lollipop.ee_relations.Count > 0) save_as_png(experimentExperimentComparison.ct_EE_Histogram, folder, "ExperimentalExperimental_MassDifferences_", timestamp);
            //if (Sweet.lollipop.qVals.Count > 0) save_as_png(quantification.ct_proteoformIntensities, folder, "QuantifiedProteoform_Intensities_", timestamp);
            //if (Sweet.lollipop.qVals.Count > 0) save_as_png(quantification.ct_relativeDifference, folder, "QuantifiedProteoform_Tusher2001Plot_", timestamp);
            //if (Sweet.lollipop.qVals.Count > 0) save_as_png(quantification.ct_volcano_logFold_logP, folder, "QuantifiedProteoform_VolcanoPlot_", timestamp);
        }
        private void save_as_png(System.Windows.Forms.DataVisualization.Charting.Chart ct, string folder, string prefix, string timestamp)
        {
            ct.SaveImage(Path.Combine(folder, prefix + timestamp + ".png"), System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Png);
        }

        #endregion Results Summary Methods

        private void resultsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void ExportTables_Click(object sender, RoutedEventArgs e)
        {
            List<DataTable> data_tables = current_form.SetTables();

            if (data_tables == null)
            {
                MessageBox.Show("There is no table on this page to export. Please navigate to another page with the Results tab.");
                return;
            }
            ProteoformSuiteGUI.ExcelWriter writer = new ProteoformSuiteGUI.ExcelWriter();
            writer.ExportToExcel(data_tables, (current_form as UserControl).Name);
            SaveExcelFile(writer, (current_form as UserControl).Name +"_table.xlsx");
        }
        
        private void exportAllTablesToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProteoformSuiteGUI.ExcelWriter writer = new ProteoformSuiteGUI.ExcelWriter();
            if (MessageBox.Show("Will prepare for export. This may take a while.", "Export Data", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
            Parallel.ForEach(forms, form => form.SetTables());
            writer.BuildHyperlinkSheet(forms.Select(sweet => new Tuple<string, List<DataTable>>((sweet as Window).Name, sweet.DataTables)).ToList());
            Parallel.ForEach(forms, form => writer.ExportToExcel(form.DataTables, (form as Window).Name));
            if (MessageBox.Show("Finished preparing. Ready to save? This may take a while.", "Export Data", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
            SaveExcelFile(writer, MDIHelpers.getParentWindow(current_form as UserControl).Name + "_table.xlsx"); //get the window hosting tabcontrol, which hosts usercontrol
        }
        

        private void printToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("printToolStripMenuItem_Click");
        }

        private void closeToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void testWin(object sender, RoutedEventArgs e)
        {
            testWin curr = new testWin();
            curr.Show();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in Enum.GetValues(typeof(MassSpectrometry.DissociationType)))
            {
                MessageBox.Show(item.ToString());
                
            }
        }

        private void LoadResultsToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            loadResults.InitializeParameterSet();
            showForm(loadResults);
        }
    }
}
