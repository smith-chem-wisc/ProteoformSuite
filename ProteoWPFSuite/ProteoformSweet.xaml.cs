using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ProteoformSuiteInternal;
using System.IO;
using System.Diagnostics;
using System.Windows.Input;
using System.Text.RegularExpressions;

namespace ProteoWPFSuite
{
    public partial class ProteoformSweet : UserControl
    {

        #region Public Fields
        public LoadResults loadResults = new LoadResults(); //finished

        public NeuCodePairs neuCodePairs = new NeuCodePairs(); //finished
        public RawExperimentalComponents rawExperimentalComponents = new RawExperimentalComponents(); //finished
        public TheoreticalDatabase theoreticalDatabase = new TheoreticalDatabase(); //finished
        public TopDown topDown = new TopDown();//finished
        public AggregatedProteoforms aggregatedProteoforms = new AggregatedProteoforms(); //finished
        public ExperimentTheoreticalComparison experimentTheoreticalComparison = new ExperimentTheoreticalComparison(); //Finished
        public ExperimentExperimentComparison experimentExperimentComparison = new ExperimentExperimentComparison(); //Finished
        public ProteoformFamilies proteoformFamilies = new ProteoformFamilies();//Finished
        public Quantification quantification = new Quantification();//Finished
        public IdentifiedProteoforms identifiedProteoforms = new IdentifiedProteoforms(); //Finished
        public ResultsSummary resultsSummary = new ResultsSummary();//No
        public List<ISweetForm> forms = new List<ISweetForm>(); //contains a list of user controls
        #endregion Public Fields

        #region Private Fields
        System.Windows.Forms.FolderBrowserDialog resultsFolderOpen = new System.Windows.Forms.FolderBrowserDialog();
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
            showTabs(forms);
            
            showForm(loadResults);
            loadResults.InitializeParameterSet();

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
                loadResults,
                theoreticalDatabase,
                topDown,
                rawExperimentalComponents,
                neuCodePairs,
                aggregatedProteoforms,
                experimentTheoreticalComparison,
                experimentExperimentComparison,
                proteoformFamilies,
                identifiedProteoforms,    
                quantification,
                resultsSummary
            };
            foreach (UserControl uc in forms)
            {
                (uc as ITabbedMDI).MDIParent = this; //set the mdi parent
            }
        }
        private void showForm(UserControl form)
        {
            /*UClosingTabItem temp = new ClosingTabItem();
            temp.Title = form.GetType().Name;
            temp.Content = form;
            MDIContainer.Items.Add(temp);
            MDIContainer.SelectedItem = temp;
            current_form = form as ISweetForm;*/
            MDIContainer.SelectedIndex = ClosingTabItem.tabTable[form.GetType().Name];
        }
        //Initial all tabs
        private void showTabs(List<ISweetForm> forms)
        {
            foreach (UserControl uc in forms)
            {
                
                ClosingTabItem temp = new ClosingTabItem();
                temp.Content = uc;

                // Format the header name with spaces
                string[] tokenizedName = Regex.Split(uc.GetType().Name, @"(?<!^)(?=[A-Z])");
                string cleanedHeaderName = string.Join(" ", tokenizedName);
                temp.Title = cleanedHeaderName;

                //if (!uc.IsEnabled)
                //{
                //temp.Focusable = false;//cannot be selected
                //}

                MDIContainer.Items.Add(temp);
                ClosingTabItem.tabTable.Add(uc.GetType().Name, MDIContainer.Items.Count - 1);//keep a record
                current_form = uc as ISweetForm;
            }
        }
        #endregion Private Setup Methods

        #region RESULTS TOOL STRIP Public Method

        public void enable_neuCodeProteoformPairsToolStripMenuItem(bool setting)
        {
            ClosingTabItem temp = (ClosingTabItem)MDIContainer.Items[ClosingTabItem.tabTable["NeuCodePairs"]];
            temp.Focusable=setting;
            temp.freeze = !setting;
            if(temp.freeze)
                temp.Background = System.Windows.Media.Brushes.Gray;
            else
                temp.ClearValue(BackgroundProperty);
        }

        public void enable_quantificationToolStripMenuItem(bool setting)
        {
            ClosingTabItem temp = (ClosingTabItem)MDIContainer.Items[ClosingTabItem.tabTable["Quantification"]];
            temp.Focusable = setting;
            temp.freeze = !setting;
            if (temp.freeze)
                temp.Background = System.Windows.Media.Brushes.Gray;
            else
                temp.ClearValue(BackgroundProperty);
        }

        public void enable_topDownToolStripMenuItem(bool setting)
        {
            ClosingTabItem temp = (ClosingTabItem)MDIContainer.Items[ClosingTabItem.tabTable["TopDown"]];
            temp.Focusable = setting;
            temp.freeze = !setting;
            if (temp.freeze)
                temp.Background = System.Windows.Media.Brushes.Gray;
            else
                temp.ClearValue(BackgroundProperty);
        }

        #endregion RESULTS TOOL STRIP Public Method

        #region FILE TOOL STRIP Private Methods

        private void printToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("printToolStripMenuItem_Click");
        }

        private void closeToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion FILE TOOL STRIP Private Methods

        #region METHOD TOOL STRIP Private Methods

        private void saveMethodToolStripMenuItem1_Click(object sender, RoutedEventArgs e)
        {
            if (methodFileSave.ShowDialog() == true)
                saveMethod(methodFileSave.FileName);
        }

        private void saveMethod(string method_filename)
        {
            using (StreamWriter file = new StreamWriter(method_filename))
                file.WriteLine(Sweet.save_method());
        }

        private void loadSettingsToolStripMenuItem_Click(object sender, RoutedEventArgs e)
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
            bool method_file_success = Sweet.open_method(methodFilePath, String.Join(Environment.NewLine, lines), add_files, out string warning);
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

            //  Check that there are input files
            if (Sweet.lollipop.input_files.Count == 0)
            {
                MessageBox.Show("Please load in deconvolution result files in order to use load and run.", "Full Run");
                return null;
            }

            // Check that theoretical database(s) are present

            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.ProteinDatabase).Count() <= 0)
            {
                MessageBox.Show("Please list at least one protein database.", "Full Run");
                return null;
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

            //Run the program
            Mouse.OverrideCursor = Cursors.Wait;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (ISweetForm sweet in forms)
            {
                if (sweet.ReadyToRunTheGamut())
                    sweet.RunTheGamut(true);
            }

            //  Save the results
            resultsSummary.InitializeParameterSet();
            if (Sweet.lollipop.results_folder != "")
            {
                string timestamp = Sweet.time_stamp();
                ResultsSummaryGenerator.save_all(Sweet.lollipop.results_folder, timestamp, resultsSummary.get_go_analysis(), resultsSummary.get_tusher_analysis());
                save_all_plots(Sweet.lollipop.results_folder, timestamp);
                using (StreamWriter file = new StreamWriter(Path.Combine(Sweet.lollipop.results_folder, "presets_" + timestamp + ".xml")))
                    file.WriteLine(Sweet.save_method());
            }
            List<string> warning_methods = new List<string>() { "Warning:" };
            if (Sweet.lollipop.bottomupReader.bad_ptms.Count > 0)
            {
                warning_methods.Add("The following PTMs in the bottom-up file were not matched with any PTMs in the theoretical database: ");
                warning_methods.Add(string.Join(", ", Sweet.lollipop.bottomupReader.bad_ptms.Distinct()));
            }
            if (Sweet.lollipop.topdownReader.bad_ptms.Count > 0)
            {
                warning_methods.Add("Top-down proteoforms with the following modifications were not matched to a modification in the theoretical PTM list: ");
                warning_methods.Add(string.Join(", ", Sweet.lollipop.topdownReader.bad_ptms.Distinct()));
            }
            if (Sweet.lollipop.topdown_proteoforms_no_theoretical.Count() > 0)
            {
                warning_methods.Add("Top-down proteoforms with the following accessions were not matched to a theoretical proteoform in the theoretical database: ");
                warning_methods.Add(string.Join(", ", Sweet.lollipop.topdown_proteoforms_no_theoretical.Select(t => t.accession.Split('_')[0]).Distinct()));
            }
            if (warning_methods.Count > 1)
            {
                MessageBox.Show(String.Join("\n\n", warning_methods));
            }
            //  Program ran successfully
            stopwatch.Stop();
            Mouse.OverrideCursor = null;
            return stopwatch;
        }

        #endregion METHOD TOOL STRIP Private Methods

        #region Export Table as Excel File -- Private Methods
        private void exportTablesToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            List<DataTable> data_tables = current_form.SetTables();

            if (data_tables == null)
            {
                MessageBox.Show("There is no table on this page to export. Please navigate to another page with the Results tab.");
                return;
            }

            ProteoformSuiteGUI.ExcelWriter writer = new ProteoformSuiteGUI.ExcelWriter();
            writer.ExportToExcel(data_tables, (current_form as UserControl).GetType().Name);
            SaveExcelFile(writer, (current_form as UserControl).GetType().Name + "_table.xlsx");
        }


        private void exportAllTablesToolStripMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ProteoformSuiteGUI.ExcelWriter writer = new ProteoformSuiteGUI.ExcelWriter();
            if (MessageBox.Show("Will prepare for export. This may take a while.", "Export Data", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
            Parallel.ForEach(forms, form => form.SetTables());
            writer.BuildHyperlinkSheet(forms.Select(sweet => new Tuple<string, List<DataTable>>((sweet as UserControl).GetType().Name, sweet.DataTables)).ToList());
            Parallel.ForEach(forms, form => writer.ExportToExcel(form.DataTables, (form as UserControl).GetType().Name));
            if (MessageBox.Show("Finished preparing. Ready to save? This may take a while.", "Export Data", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel) return;
            SaveExcelFile(writer, (current_form as ITabbedMDI).MDIParent.GetType().Name + "_table.xlsx"); //get the window hosting tabcontrol, which hosts usercontrol
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

            if (Sweet.lollipop.et_relations.Count > 0) save_as_png(experimentTheoreticalComparison.ct_ET_Histogram, folder, "ExperimentalTheoretical_MassDifferences_", timestamp);
            //if (Sweet.lollipop.ee_relations.Count > 0) save_as_png(experimentExperimentComparison.ct_EE_Histogram, folder, "ExperimentalExperimental_MassDifferences_", timestamp);
            if (Sweet.lollipop.qVals.Count > 0) save_as_png(quantification.ct_proteoformIntensities, folder, "QuantifiedProteoform_Intensities_", timestamp);
            if (Sweet.lollipop.qVals.Count > 0) save_as_png(quantification.ct_relativeDifference, folder, "QuantifiedProteoform_Tusher2001Plot_", timestamp);
            if (Sweet.lollipop.qVals.Count > 0) save_as_png(quantification.ct_volcano_logFold_logP, folder, "QuantifiedProteoform_VolcanoPlot_", timestamp);
        }
        private void save_as_png(System.Windows.Forms.DataVisualization.Charting.Chart ct, string folder, string prefix, string timestamp)
        {
            ct.SaveImage(Path.Combine(folder, prefix + timestamp + ".png"), System.Windows.Forms.DataVisualization.Charting.ChartImageFormat.Png);
        }

        #endregion Results Summary Methods

        #region Others
        private void resultsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void testWin(object sender, RoutedEventArgs e)
        {
            testWin curr = new testWin();
            curr.Show();
        }
        #endregion Others

        private void Button_Click_Left(object sender, RoutedEventArgs e)
        {
            int prev = MDIContainer.SelectedIndex - 1;

            while (prev >= 0 && !(MDIContainer.Items[prev] as TabItem).Focusable)
            {
                --prev;
            }
            if (prev >= 0)
            {
                MDIContainer.SelectedIndex = prev;
            }
        }

        private void Button_Click_Right(object sender, RoutedEventArgs e)
        {
            int nxt = MDIContainer.SelectedIndex + 1;

            while (nxt < MDIContainer.Items.Count && !(MDIContainer.Items[nxt] as TabItem).Focusable)
            {
                ++nxt;
            }
            if (nxt < MDIContainer.Items.Count)
            {
                MDIContainer.SelectedIndex = nxt;
            }
        }

        private void changeColor(object sender, MouseEventArgs e)
        {
            (sender as Border).Background = System.Windows.Media.Brushes.Gray;
        }

        private void restore(object sender, MouseEventArgs e)
        {
            Border temp = (Border)sender;
            temp.ClearValue(BackgroundProperty);
        }

        private void pressColor(object sender, MouseButtonEventArgs e)
        {
            (sender as Border).Background = System.Windows.Media.Brushes.White;
        }

        private void restore(object sender, MouseButtonEventArgs e)
        {
            (sender as Border).Background = System.Windows.Media.Brushes.Gray;
        }

        /**
         * When a tab a clicked, this runs the appropriate "initializing" for that TabItem.
         * Used by <TabControl x:Name="MDIContainer" ..> in ProteoformSweet.xaml
         * 
         * @param sender    is the TabControl object that triggers this function. Can be used to 
         *                  get reference to the selected tab by using the field SelectedValue.
         */
        private void MDIContainer_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // We do this to prevent firing TabControl's SelectionChanged event unintendedly
            // Reference: https://stackoverflow.com/questions/3659858/in-c-sharp-wpf-why-is-my-tabcontrols-selectionchanged-event-firing-too-often
            e.Handled = true;

            // Gets the reference to the selected tab
            TabControl tabControl = sender as TabControl;
            ClosingTabItem item = tabControl.SelectedValue as ClosingTabItem;

            // Before opening each tab, there are initializations that are specific
            // to each tab that needs to be done.
            switch (item.Title)
            {
                case "Load Results":
                    showForm(loadResults);
                    break;

                case "Theoretical Database":
                    theoreticalDatabase.reload_database_list();
                    showForm(theoreticalDatabase);
                    break;

                case "Top Down":
                    showForm(topDown);
                    break;

                case "Raw Experimental Components":
                    showForm(rawExperimentalComponents);
                    break;

                case "Neu Code Pairs":
                    showForm(neuCodePairs);
                    break;

                case "Aggregated Proteoforms":
                    showForm(aggregatedProteoforms);
                    break;

                case "Experiment Theoretical Comparison":
                    showForm(experimentTheoreticalComparison);
                    break;

                case "Experiment Experiment Comparison":
                    showForm(experimentExperimentComparison);
                    break;

                case "Proteoform Families":
                    showForm(proteoformFamilies);
                    proteoformFamilies.initialize_every_time();
                    break;

                case "Identified Proteoforms":
                    showForm(identifiedProteoforms);
                    if (identifiedProteoforms.ReadyToRunTheGamut()) identifiedProteoforms.RunTheGamut(false);
                    break;

                case "Quantification":
                    quantification.initialize_every_time();
                    showForm(quantification);
                    break;

                case "Results Summary":
                    resultsSummary.create_summary();
                    showForm(resultsSummary);
                    break;

                default:
                    MessageBox.Show("ERROR: Click unknown. You clicked: " + item.Title);
                    break;
            }
        }

        private void Btn_RunPage_Click(object sender, RoutedEventArgs e)
        {
            ClosingTabItem item = MDIContainer.SelectedValue as ClosingTabItem;
            // MessageBox.Show("This page is :" + item.Title);

            // Dynamically change the button's action according to page.
            // i.e. use the correct RunTheGamut that refers to page.
            switch (item.Title)
            {
                case "Load Results":
                    // not yet
                    break;

                case "Theoretical Database":
                    Mouse.OverrideCursor = Cursors.Wait;
                    theoreticalDatabase.RunTheGamut(false);
                    Mouse.OverrideCursor = null;
                    break;

                case "Top Down":
                    topDown.RunTheGamut(false);
                    break;

                case "Raw Experimental Components":
                    Mouse.OverrideCursor = Cursors.Wait;
                    rawExperimentalComponents.RunTheGamut(false);
                    Mouse.OverrideCursor = null;
                    break;

                case "Neu Code Pairs":
                    if (neuCodePairs.ReadyToRunTheGamut())
                        neuCodePairs.RunTheGamut(false);
                    break;

                case "Aggregated Proteoforms":
                    if (aggregatedProteoforms.ReadyToRunTheGamut())
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        aggregatedProteoforms.RunTheGamut(false);
                        Mouse.OverrideCursor = null;
                    }
                    else if (Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Length <= 0)
                    {
                        MessageBox.Show("Go back and load in deconvolution results.");
                    }
                    break;

                case "Experiment Theoretical Comparison":
                    if (experimentTheoreticalComparison.ReadyToRunTheGamut())
                    {
                        System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                        experimentTheoreticalComparison.RunTheGamut(false);
                        experimentTheoreticalComparison.xMaxET.Value = (decimal)Sweet.lollipop.et_high_mass_difference;
                        experimentTheoreticalComparison.xMinET.Value = (decimal)Sweet.lollipop.et_low_mass_difference;
                        System.Windows.Input.Mouse.OverrideCursor = null;
                    }
                    else if (Sweet.lollipop.target_proteoform_community.has_e_proteoforms)
                        MessageBox.Show("Go back and create a theoretical database.");
                    else
                        MessageBox.Show("Go back and aggregate experimental proteoforms.");
                    break;

                case "Experiment Experiment Comparison":
                    if (experimentExperimentComparison.ReadyToRunTheGamut())
                    {
                        System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                        experimentExperimentComparison.RunTheGamut(false);
                        experimentExperimentComparison.xMaxEE.Value = Convert.ToDecimal(Sweet.lollipop.ee_max_mass_difference);
                        System.Windows.Input.Mouse.OverrideCursor = null;
                    }
                    else if (Sweet.lollipop.target_proteoform_community.has_e_proteoforms)
                        MessageBox.Show("Go back and create the theoretical database.");
                    else
                        MessageBox.Show("Go back and aggregate experimental proteoforms.");
                    break;

                case "Proteoform Families":
                    if (proteoformFamilies.ReadyToRunTheGamut())
                    {
                        System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                        proteoformFamilies.RunTheGamut(false);
                        System.Windows.Input.Mouse.OverrideCursor = null;
                    }
                    break;

                case "Identified Proteoforms":
                    if (identifiedProteoforms.ReadyToRunTheGamut()) identifiedProteoforms.RunTheGamut(false);
                    break;

                case "Quantification":
                    if (quantification.ReadyToRunTheGamut())
                    {
                        Mouse.OverrideCursor = Cursors.Wait;
                        quantification.RunTheGamut(false);
                        Mouse.OverrideCursor = null;
                    }
                    else if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Count() <= 0)
                        MessageBox.Show("Please load quantification results in Load Deconvolution Results.", "Quantification");
                    else if (Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Where(f => f.purpose == Purpose.Quantification).Any(f => f.fraction == "" || f.biological_replicate == "" || f.technical_replicate == "" || f.lt_condition == ""))
                        MessageBox.Show("Please be sure the technical replicate, fraction, biological replicate, and condition are labeled for each quantification file.");
                    else if (Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Where(f => f.purpose == Purpose.Quantification).Select(f => f.lt_condition).Concat(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Select(f => Sweet.lollipop.neucode_labeled ? f.hv_condition : f.lt_condition)).Distinct().Count() != 2)
                        MessageBox.Show("Please be sure there are two conditions.");
                    else if (!Sweet.lollipop.neucode_labeled && !Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Select(f => f.lt_condition).Distinct().All(c => Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification && f.lt_condition == c).Select(f => f.biological_replicate + f.fraction + f.technical_replicate).Distinct().All(d => Sweet.lollipop.input_files.Where(f2 => f2.purpose == Purpose.Quantification && f2.lt_condition != c).
                            Select(f2 => f2.biological_replicate + f2.fraction + f2.technical_replicate).Distinct().ToList().Contains(d))))
                        MessageBox.Show("Please be sure there are the same number and labels for each biological replicate, fraction, and technical replicate for each condition.");
                    else if (!Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Select(f => f.lt_condition).Concat(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Select(f => Sweet.lollipop.neucode_labeled ? f.hv_condition : f.lt_condition)).Distinct().All(c => Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification && f.lt_condition == c).Select(f => f.biological_replicate).Distinct().Count() >= 2))
                        MessageBox.Show("Please be sure there are at least two biological replicates for each condition.");
                    else if (Sweet.lollipop.raw_quantification_components.Count == 0)
                        MessageBox.Show("Please process raw components.", "Quantification");
                    else if (Sweet.lollipop.raw_experimental_components.Count <= 0)
                        MessageBox.Show("Please load deconvolution results.", "Quantification");
                    else if (Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Length <= 0)
                        MessageBox.Show("Please aggregate proteoform observations.", "Quantification");
                    else if (Sweet.lollipop.target_proteoform_community.families.Count <= 0)
                        MessageBox.Show("Please construct proteoform families.", "Quantification");
                    break;

                case "Results Summary":
                    // not yet
                    break;

                default:
                    MessageBox.Show("ERROR: Click unknown. You clicked: " + item.Title);
                    break;
            }
        }
    }
}