using System;
using System.Linq;
using System.Windows;
using ProteoformSuiteInternal;
using System.Data;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Reflection;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;
using System.ComponentModel;

/// <Problems>
/// - Should convert window to proteoform sweet?
/// - Currently keeping main window but not sure if still keeping it in the future
/// - Split Container not addressed (What is this for? What should be used instead?)
/// </Problems>
/// <remarks>
/// - Dragevents are using forms version in order to be consistent with datagridview
/// - Comments with three slashes are for future implementation
/// - Controls are default to be WPF except for data Grid view
/// - WPF window close takes care of all dispose things
/// - selection changed doesnot change the text of combo box in the first place
/// </remarks>
/// <TODO>
/// - Implement ProteoFormSweat
/// - Remove the three-slash comments 
/// </TODO>

namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for loadResults.xaml
    /// </summary>
    public partial class LoadResults : UserControl, ITabbedMDI, ISweetForm, INotifyPropertyChanged
    {
        public LoadResults()
        {
            InitializeComponent();
            this.DataContext = this;
            this.PropertyChanged += tb_filter1_TextChanged;
            populate_file_lists();
        }

        #region Public Property
        public List<DataTable> DataTables { get; private set; }
        public ProteoformSweet MDIParent { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public String LabelTxt
        {
            get
            {
                return _labeltxt;
            }
            set
            {
                if (_labeltxt==value)
                    return;
                _labeltxt = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("LabelTxt"));
            }
        }
        #endregion

        #region Private Property
        private String _labeltxt;
        #endregion Private Property

        #region Public Methods
        public void InitializeParameterSet()
        {   
            //tb_resultsFolder.Text = Sweet.lollipop.results_folder;
            rb_neucode.IsChecked = Sweet.lollipop.neucode_labeled;
            rb_unlabeled.IsChecked = !rb_neucode.IsChecked;
            cb_calibrate_td_files.IsChecked = Sweet.lollipop.calibrate_td_files;
            cb_calibrate_raw_files.IsChecked = Sweet.lollipop.calibrate_raw_files;
            ProteoformSweet parMDI = ((MainWindow)MDIHelpers.getParentWindow(this)).MDIParentControl; //get the parent control of the form;
            ///parMDI.enable_neuCodeProteoformPairsToolStripMenuItem(Sweet.lollipop.neucode_labeled);
            ///parMDI.enable_quantificationToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.Quantification));
            ///parMDI.enable_topDownToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.TopDown));
        }
        public List<DataTable> SetTables()
        {
            DataTables = new List<DataTable>();
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Count() > 0)
                DataTables.Add(DisplayInputFile.FormatInputFileTable(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification).Select(x => new DisplayInputFile(x)).ToList(), "IdentificationFiles", new List<Purpose> { Purpose.Identification }));
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.CalibrationIdentification).Count() > 0)
                DataTables.Add(DisplayInputFile.FormatInputFileTable(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.CalibrationIdentification).Select(x => new DisplayInputFile(x)).ToList(), "CalibrationIdentificationFiles", new List<Purpose> { Purpose.CalibrationIdentification }));
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.CalibrationTopDown).Count() > 0)
                DataTables.Add(DisplayInputFile.FormatInputFileTable(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.CalibrationTopDown).Select(x => new DisplayInputFile(x)).ToList(), "CalibrationTopDownFiles", new List<Purpose> { Purpose.CalibrationTopDown }));
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.TopDown).Count() > 0)
                DataTables.Add(DisplayInputFile.FormatInputFileTable(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.TopDown).Select(x => new DisplayInputFile(x)).ToList(), "TopDown", new List<Purpose> { Purpose.TopDown }));
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.BottomUp).Count() > 0)
                DataTables.Add(DisplayInputFile.FormatInputFileTable(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.BottomUp).Select(x => new DisplayInputFile(x)).ToList(), "BottomUp", new List<Purpose> { Purpose.BottomUp }));
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Count() > 0)
                DataTables.Add(DisplayInputFile.FormatInputFileTable(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Select(x => new DisplayInputFile(x)).ToList(), "QuantificationFiles", new List<Purpose> { Purpose.Quantification }));
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, new List<Purpose> { Purpose.PtmList, Purpose.ProteinDatabase }).Count() > 0)
                DataTables.Add(DisplayInputFile.FormatInputFileTable(Sweet.lollipop.get_files(Sweet.lollipop.input_files, new List<Purpose> { Purpose.PtmList, Purpose.ProteinDatabase }).Select(x => new DisplayInputFile(x)).ToList(), "ProteinDatabases", new List<Purpose> { Purpose.ProteinDatabase, Purpose.PtmList }));
            return DataTables;
        }
        public void ClearListsTablesFigures(bool clear_following)
        {
            Sweet.lollipop.input_files.Clear();
            Sweet.save_actions.Clear();
            Sweet.loaded_actions.Clear();
            Sweet.lollipop.results_folder = "";
            //tb_resultsFolder.Text = "";

            ProteoformSweet parMDI = ((MainWindow)MDIHelpers.getParentWindow(this)).MDIParentControl; //get the parent control of the form;
            if (clear_following)
            {
                for (int i = parMDI.forms.IndexOf(this) + 1; i < parMDI.forms.Count; i++)
                {
                    ISweetForm sweet = parMDI.forms[i];
                    sweet.ClearListsTablesFigures(false);
                    sweet.FillTablesAndCharts();
                }
            }
            FillTablesAndCharts();
        }
        public bool ReadyToRunTheGamut()
        {
            return true;
        }

        public void RunTheGamut(bool full_run)
        {
            populate_file_lists();
        }
        public void FillTablesAndCharts()
        {
            populate_file_lists();
        }
        #endregion Public Methods

        #region GENERAL TABLE OPTIONS Private Methods
        private void rb_neucode_CheckedChanged(object sender, RoutedEventArgs e)
        {
            
            /*MDIParent.enable_neuCodeProteoformPairsToolStripMenuItem(rb_neucode.Checked);
            Sweet.lollipop.neucode_labeled = (bool)rb_neucode.IsChecked;
            Sweet.lollipop.neucode_light_lysine = (bool)rb_neucode.IsChecked;
            Sweet.lollipop.natural_lysine_isotope_abundance = !(bool)rb_neucode.IsChecked;

            foreach (InputFile f in Sweet.lollipop.input_files)
            {
                if ((bool)rb_neucode.IsChecked) f.label = Labeling.NeuCode;
                if ((bool)rb_unlabeled.IsChecked) f.label = Labeling.Unlabeled;
            }
            populate_file_lists();*/
        }
        private void rb_unlabeled_CheckedChanged(object sender, RoutedEventArgs e)
        { }
        private void rb_standardOptions_CheckedChanged(object sender, RoutedEventArgs e)
        {
            populate_file_lists();
        }
        private void rb_chemicalCalibration_CheckedChanged(object sender, RoutedEventArgs e)
        {
            populate_file_lists();
        }

        private void rb_advanced_user_CheckedChanged(object sender, EventArgs e)
        {
            populate_file_lists();
        }

        private void rb_deconvolution_CheckedChanged(object sender, RoutedEventArgs e)
        {
            populate_file_lists();
        }
        private void cmb_loadTable1_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            LabelTxt = (sender as ComboBox).SelectedItem.ToString();
        }
        private void populate_file_lists()
        {
            cmb_loadTable1.Items.Clear();

            if ((bool)rb_standardOptions.IsChecked)
            {
                for (int i = 0; i < 4; i++) cmb_loadTable1.Items.Add(Lollipop.file_lists[i]);

                cmb_loadTable1.SelectedIndex = 0;

                cmb_loadTable1.IsEnabled = true;


                bt_calibrate.Visibility = Visibility.Collapsed;
                cb_calibrate_raw_files.Visibility = Visibility.Collapsed;
                cb_calibrate_td_files.Visibility = Visibility.Collapsed;
                bt_deconvolute.Visibility = Visibility.Collapsed;
                bt_stepthru.Visibility = Visibility.Visible;
                bt_fullrun.Visibility = Visibility.Visible;
                bt_calibrate.Visibility = Visibility.Collapsed;
                panel_deconv_calib.Visibility = Visibility.Collapsed;
                panel_step.Visibility = Visibility.Visible;
                nud_maxcharge.Visibility = Visibility.Collapsed;
                nud_mincharge.Visibility = Visibility.Collapsed;
                label_maxcharge.Visibility = Visibility.Collapsed;
                label_mincharge.Visibility = Visibility.Collapsed;
                label_maxRT.Visibility = Visibility.Collapsed;
                label_minRT.Visibility = Visibility.Collapsed;
                rb_neucode.Visibility = Visibility.Visible;
                rb_unlabeled.Visibility = Visibility.Visible;
                calib_stand_splitContainer.Visibility = Visibility.Visible;
                fullrun_groupbox.Visibility = Visibility.Visible;

            }

            else if ((bool)rb_chemicalCalibration.IsChecked)
            {
                for (int i = 4; i < 7; i++) cmb_loadTable1.Items.Add(Lollipop.file_lists[i]);
                bt_calibrate.Visibility = Visibility.Visible;
                cb_calibrate_td_files.Visibility = Visibility.Visible;
                cb_calibrate_raw_files.Visibility = Visibility.Visible;
                bt_deconvolute.Visibility = Visibility.Collapsed;
                bt_stepthru.Visibility = Visibility.Collapsed;
                bt_fullrun.Visibility = Visibility.Collapsed;
                bt_calibrate.Visibility = Visibility.Visible;
                panel_deconv_calib.Visibility = Visibility.Visible;
                panel_step.Visibility = Visibility.Collapsed;
                nud_maxcharge.Visibility = Visibility.Collapsed;
                nud_mincharge.Visibility = Visibility.Collapsed;
                label_maxcharge.Visibility = Visibility.Collapsed;
                label_mincharge.Visibility = Visibility.Collapsed;
                label_maxRT.Visibility = Visibility.Collapsed;
                label_minRT.Visibility = Visibility.Collapsed;
                rb_neucode.Visibility = Visibility.Visible;
                rb_unlabeled.Visibility = Visibility.Visible;
                calib_stand_splitContainer.Visibility = Visibility.Visible;
                fullrun_groupbox.Visibility = Visibility.Collapsed;

                cmb_loadTable1.SelectedIndex = 0;

                cmb_loadTable1.IsEnabled = true;
            }

            else if ((bool)rb_deconvolution.IsChecked)
            {

                cmb_loadTable1.Items.Add(Lollipop.file_lists[4]);

                cmb_loadTable1.SelectedIndex = 0;

                cmb_loadTable1.IsEnabled = false;

                bt_calibrate.Visibility = Visibility.Collapsed;
                cb_calibrate_raw_files.Visibility = Visibility.Collapsed;
                cb_calibrate_td_files.Visibility = Visibility.Collapsed;
                bt_stepthru.Visibility = Visibility.Collapsed;
                bt_fullrun.Visibility = Visibility.Collapsed;
                bt_calibrate.Visibility = Visibility.Collapsed;
                bt_deconvolute.Visibility = Visibility.Visible;
                panel_deconv_calib.Visibility = Visibility.Visible;
                panel_step.Visibility = Visibility.Collapsed;
                nud_maxcharge.Visibility = Visibility.Visible;
                nud_mincharge.Visibility = Visibility.Visible;
                label_maxcharge.Visibility = Visibility.Visible;
                label_mincharge.Visibility = Visibility.Visible;
                label_maxRT.Visibility = Visibility.Visible;
                label_minRT.Visibility = Visibility.Visible;
                rb_neucode.Visibility = Visibility.Collapsed;
                rb_unlabeled.Visibility = Visibility.Collapsed;
                calib_stand_splitContainer.Visibility = Visibility.Collapsed;
                fullrun_groupbox.Visibility = Visibility.Collapsed;

                cmb_loadTable1.SelectedIndex = 0;

                cmb_loadTable1.IsEnabled = false;
            }
            LabelTxt = cmb_loadTable1.SelectedItem.ToString();

            reload_dgvs();
            refresh_dgvs();
        }
        #endregion GENERAL TABLE OPTIONS Private Methods

        #region DGV DRAG AND DROP Private Methods

        private void dgv_deconResults_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable1, dgv_loadFiles1);
        }

        private void dgv_quantResults_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable1, dgv_loadFiles1);
        }

        private void dgv_calibrationResults_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable1, dgv_loadFiles1);
        }

        private void dgv_deconResults_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            e.Effect = System.Windows.Forms.DragDropEffects.All;
        }

        private void dgv_calibrationResults_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            e.Effect = System.Windows.Forms.DragDropEffects.All;
        }

        private void dgv_quantResults_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            e.Effect = System.Windows.Forms.DragDropEffects.All;
        }

        private void drag_drop(System.Windows.Forms.DragEventArgs e, ComboBox cmb, System.Windows.Forms.DataGridView dgv)
        {
            int selected_index = Lollipop.file_lists.ToList().IndexOf(cmb.Text);

            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (DisplayUtility.CheckForProteinFastas(cmb, files)) return; // todo: implement protein fasta usage
            Sweet.lollipop.enter_input_files(files, Lollipop.acceptable_extensions[selected_index], Lollipop.file_types[selected_index], Sweet.lollipop.input_files, true);
            refresh_dgvs();
            DisplayUtility.FillDataGridView(dgv, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv, Lollipop.file_types[selected_index]);
        }

        private void refresh_dgvs()
        {
            
            foreach (System.Windows.Forms.DataGridView dgv in new List<System.Windows.Forms.DataGridView> { dgv_loadFiles1 })
            {
                dgv.Refresh();
            }
            //ProteoformSweet parMDI = ((MainWindow)MDIHelpers.getParentWindow(this)).MDIParentControl; //get the parent control of the form;
            /*if (parMDI != null) //doesn't work first time
            {
                ///parMDI.enable_quantificationToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.Quantification));
                ///parMDI.enable_topDownToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.TopDown));
            }*/
        }

        private void reload_dgvs()
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles1, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[Lollipop.file_lists.ToList().IndexOf(cmb_loadTable1.Text)]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles1, Lollipop.file_types[Lollipop.file_lists.ToList().IndexOf(cmb_loadTable1.Text)]);
        }

        #endregion DGV DRAG AND DROP Private Methods
        
        #region CELL FORMATTING Private Methods

        private void dgv_loadFiles1_CellFormatting(object sender, System.Windows.Forms.DataGridViewCellFormattingEventArgs e)
        {
            if ((dgv_loadFiles1.Rows[e.RowIndex].DataBoundItem != null) && e.ColumnIndex >= 0 && (dgv_loadFiles1.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
                e.Value = BindProperty(dgv_loadFiles1.Rows[e.RowIndex].DataBoundItem, dgv_loadFiles1.Columns[e.ColumnIndex].DataPropertyName);
        }

        private string BindProperty(object property, string propertyName)
        {
            if (propertyName.Contains("."))
            {
                PropertyInfo[] arrayProperties = property.GetType().GetProperties();
                string firstPropertyName = propertyName.Substring(0, propertyName.IndexOf("."));
                PropertyInfo firstProperty = arrayProperties.Where(p => p.Name == firstPropertyName).First();
                return BindProperty(firstProperty.GetValue(property, null), propertyName.Substring(propertyName.IndexOf(".") + 1));
            }
            else
            {
                Type propertyType = property.GetType();
                PropertyInfo propertyInfo = propertyType.GetProperty(propertyName);
                return propertyInfo.GetValue(property, null).ToString();
            }
        }

        #endregion CELL FORMATTING Private Methods

        #region ADD BUTTONS Private Methods
        private void btn_addFiles1_Click(object sender, RoutedEventArgs e)
        {
            add_files(cmb_loadTable1, dgv_loadFiles1);
        }

        private void add_files(ComboBox cmb, System.Windows.Forms.DataGridView dgv)
        {
            int selected_index = Lollipop.file_lists.ToList().IndexOf(cmb.Text);

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = cmb.SelectedItem.ToString();
            openFileDialog.Filter = Lollipop.file_filters[selected_index];
            openFileDialog.Multiselect = true;

            bool dr = (bool)openFileDialog.ShowDialog();
            if (dr)
            {
                if (DisplayUtility.CheckForProteinFastas(cmb, openFileDialog.FileNames)) return; // todo: implement protein fasta usage
                Sweet.lollipop.enter_input_files(openFileDialog.FileNames, Lollipop.acceptable_extensions[selected_index], Lollipop.file_types[selected_index], Sweet.lollipop.input_files, true);
                refresh_dgvs();
                if (openFileDialog.FileNames.Any(f => Path.GetExtension(f) == ".raw")) ValidateThermoMsFileReaderVersion();
            }

            DisplayUtility.FillDataGridView(dgv, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv, Lollipop.file_types[selected_index]);
        }

        private const string AssumedThermoMsFileReaderDllPath = @"C:\Program Files\Thermo\MSFileReader";
        private const string DesiredFileIoVersion = "3.0";
        private const string DesiredFregistryVersion = "3.0";
        private const string DesiredXRawFileVersion = "3.0.29.0";

        public static void ValidateThermoMsFileReaderVersion()
        {
            string fileIoAssumedPath = Path.Combine(AssumedThermoMsFileReaderDllPath, "Fileio_x64.dll");
            string fregistryAssumedPath = Path.Combine(AssumedThermoMsFileReaderDllPath, "fregistry_x64.dll");
            string xRawFileAssumedPath = Path.Combine(AssumedThermoMsFileReaderDllPath, "XRawfile2_x64.dll");

            if (File.Exists(fileIoAssumedPath) && File.Exists(fregistryAssumedPath) && File.Exists(xRawFileAssumedPath))
            {
                string fileIoVersion = FileVersionInfo.GetVersionInfo(fileIoAssumedPath).FileVersion;
                string fregistryVersion = FileVersionInfo.GetVersionInfo(fregistryAssumedPath).FileVersion;
                string xRawFileVersion = FileVersionInfo.GetVersionInfo(xRawFileAssumedPath).FileVersion;

                if (fileIoVersion.Equals(DesiredFileIoVersion) && fregistryVersion.Equals(DesiredFregistryVersion) && xRawFileVersion.Equals(DesiredXRawFileVersion))
                {
                    return;
                }
                else
                {
                    MessageBox.Show("Warning!Thermo MSFileReader is not version 3.0 SP2; a crash may result from searching this .raw file.");
                    return;
                }
            }
            MessageBox.Show("Warning! Cannot find Thermo MSFileReader (v3.0 SP2 is preferred); a crash may result from searching this .raw file");
        }



        #endregion ADD BUTTONS Private Methods

        #region CLEAR BUTTONS Private Methods
        private void btn_clearFiles1_Click(object sender, RoutedEventArgs e)
        {
            clear_files(cmb_loadTable1, dgv_loadFiles1);
        }

        private void clear_files(ComboBox cmb, System.Windows.Forms.DataGridView dgv)
        {
            int selected_index = Lollipop.file_lists.ToList().IndexOf(cmb.Text);
            List<InputFile> files_to_remove = Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]).ToList();
            Sweet.save_actions.RemoveAll(a => files_to_remove.Any(f => a.Contains(f.complete_path)));
            Sweet.lollipop.input_files = Sweet.lollipop.input_files.Except(files_to_remove).ToList();
            refresh_dgvs();
            DisplayUtility.FillDataGridView(dgv, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv, Lollipop.file_types[selected_index]);
        }
        #endregion CLEAR BUTTONS Private Methods

        #region FULL RUN & STEP THROUGH Private Methods

        private void btn_fullRun_Click(object sender, RoutedEventArgs e)
        {
            ProteoformSweet parMDI = ((MainWindow)MDIHelpers.getParentWindow(this)).MDIParentControl; //get the parent control of the form;
            ///Stopwatch successful_run = parMDI.full_run();
            ///if (successful_run != null) MessageBox.Show("Successfully ran method in "
            ///    + String.Format("{0:00}:{1:00}:{2:00}.{3:00}", successful_run.Elapsed.Hours, successful_run.Elapsed.Minutes, successful_run.Elapsed.Seconds, successful_run.Elapsed.Milliseconds / 10)
            ///    + ". Feel free to explore using the Results menu.", "Full Run");
            ///else MessageBox.Show("Method did not successfully run.", "Full Run");
        }
        private void bt_clearResults_Click(object sender, RoutedEventArgs e)
        {
            Sweet.lollipop = new Lollipop();
            ClearListsTablesFigures(true);
        }

        private void bt_stepthru_Click(object sender, RoutedEventArgs e)
        {
            ProteoformSweet parMDI = ((MainWindow)MDIHelpers.getParentWindow(this)).MDIParentControl; //get the parent control of the form;
            ///parMDI.resultsToolStripMenuItem.ShowDropDown();
            MessageBox.Show("Use the Results menu to step through processing results.\n\n" +
                "Load results and databases in this panel, and then proceed to Raw Experimental Components.", "Step Through Introduction.");
        }

        System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();


        private void btn_browseSummarySaveFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.DialogResult dr = folderBrowser.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string temp_folder_path = folderBrowser.SelectedPath;
                tb_resultsFolder.Text = temp_folder_path;
                Sweet.lollipop.results_folder = temp_folder_path;
            }
        }

        #endregion FULL RUN & STEP THROUGH Private Methods

        #region FILTERS Private Methods
        private void tb_filter1_TextChanged(object sender, PropertyChangedEventArgs e)
        {
            int selected_index = Lollipop.file_lists.ToList().IndexOf(cmb_loadTable1.SelectedItem.ToString());
            //problem here!!!!!!!!!
            DisplayUtility.FillDataGridView(dgv_loadFiles1, ExtensionMethods.filter(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]), tb_filter1.Text).OfType<InputFile>().Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles1, Lollipop.file_types[selected_index]);
        }
        #endregion FILTERS Private Methods

        #region CHANGED TABLE SELECTION Private Methods

        private void bt_calibrate_Click(object sender, RoutedEventArgs e)
        {
            //if (Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.SpectraFile).Count() == 0)
            //{
            //    MessageBox.Show("Please enter raw files to calibrate."); return;
            //}
            if (Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length == 0)
            {
                MessageBox.Show("First create a theoretical proteoform database. On the Results tab, select Theoretical Proteoform Database.");
                return;
            }
            Sweet.lollipop.read_in_calibration_td_hits();
            MessageBox.Show(Sweet.lollipop.calibrate_files());
        }

        private void bt_deconvolute_Click(object sender, RoutedEventArgs e)
        {
            if (Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.SpectraFile).Count() == 0)
            {
                MessageBox.Show("Please enter raw files to deconvolute."); return;
            }
            string deconv_results = Sweet.lollipop.promex_deconvolute(Convert.ToInt32(nud_maxcharge.Value), Convert.ToInt32(nud_mincharge.Value), Environment.CurrentDirectory);
            MessageBox.Show(deconv_results);
        }

        #endregion CHANGED TABLE SELECTION Private Methods

        #region CHANGE ALL CELLS private methods

        private void dgv_loadFiles1_CellMouseClick(object sender, System.Windows.Forms.DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                change_all_selected_cells(dgv_loadFiles1);
        }

        

        private void change_all_selected_cells(System.Windows.Forms.DataGridView dgv)
        {
            InputBox testdialog = new InputBox();
            if (testdialog.ShowDialog() == true)
            {
                foreach (System.Windows.Forms.DataGridViewTextBoxCell cell in dgv.SelectedCells.OfType<System.Windows.Forms.DataGridViewTextBoxCell>())
                {
                    cell.Value = testdialog.tb.Text;
                }
            }
            testdialog.Close();
        }

        #endregion

        #region Cell Validation Methods

        private void dgv_loadFiles1_CellValidating(object sender, System.Windows.Forms.DataGridViewCellValidatingEventArgs e)
        {
            validate(dgv_loadFiles1, e);
        }

        private void validate(System.Windows.Forms.DataGridView dgv, System.Windows.Forms.DataGridViewCellValidatingEventArgs e)
        {
            if (dgv.Rows[e.RowIndex].IsNewRow)
                return;
            if (e.FormattedValue.ToString() == "" && dgv.IsCurrentCellInEditMode)
            {
                e.Cancel = true;
                MessageBox.Show("Please enter text for each label.");
            }
            if (dgv[e.ColumnIndex, e.RowIndex].ValueType == typeof(int) && (!int.TryParse(e.FormattedValue.ToString(), out int x) || x < 1))
            {
                e.Cancel = true;
                MessageBox.Show("Please use positive integers for biological replicate labels.");
            }
        }

        #endregion Cell Validation Methods

        private void cb_calibrate_raw_files_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Sweet.lollipop.calibrate_raw_files = (bool)cb_calibrate_raw_files.IsChecked;
        }

        private void cb_calibrate_td_files_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Sweet.lollipop.calibrate_td_files = (bool)cb_calibrate_td_files.IsChecked;
        }
        
        /*private void topbar_splitcontainer_SplitterMoved(object sender, SplitterEventArgs e)
{

}*/
    }
}
