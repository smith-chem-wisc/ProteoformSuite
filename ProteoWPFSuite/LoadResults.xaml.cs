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
using MassSpectrometry;

namespace ProteoWPFSuite
{
    public partial class LoadResults : UserControl, ITabbedMDI, ISweetForm, INotifyPropertyChanged
    {
        #region Public Constructor
        public LoadResults()
        {
            InitializeComponent();
            this.DataContext = this;

            populate_file_lists();
        }
        #endregion Public Constructor

        #region Private Fields
        private String _labeltxt;
        private bool ck_rbneucode;
        private int cb_select;
        //private string[] cb_src;
        #endregion Private Fields

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
                
                // moved tb_filter1 to here
                int selected_index = Lollipop.file_lists.ToList().IndexOf(value);
                DisplayUtility.FillDataGridView(dgv_loadFiles1, ExtensionMethods.filter(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]), tb_filter1.Text).OfType<InputFile>().Select(f => new DisplayInputFile(f)));
                DisplayInputFile.FormatInputFileTable(dgv_loadFiles1, Lollipop.file_types[selected_index]);
                
            }
        }
        //for checked radio
        public bool CK_rbneucode
        {
            get
            {
                return ck_rbneucode;
            }
            set
            {   
                if (ck_rbneucode == value)
                    return;
                ck_rbneucode = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CK_rbneucode"));
                rb_neucode_CheckedChanged(this.rb_neucode, new RoutedEventArgs());
            }
        }
        public int CB_select
        {
            get
            {
                return cb_select;
            }
            set
            {
                if (value < 0)
                    return;
                cb_select = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CB_select"));
                LabelTxt = cmb_loadTable1.Items[value].ToString();
            }
        }
        #endregion Public Property

        #region Public Methods
        public void InitializeParameterSet()
        {
            // Initialize components in "2. Set Parameters"
            rb_neucode.IsChecked    = Sweet.lollipop.neucode_labeled;
            rb_unlabeled.IsChecked  = !rb_neucode.IsChecked;

            // Initialize components in "2. Set Parameters" that fall under "1. Choose Analysis->Chemical Calibration"
            cb_calibrate_raw_files.IsChecked        = Sweet.lollipop.calibrate_raw_files;
            cb_calibrate_td_files.IsChecked         = Sweet.lollipop.calibrate_td_files;
            cb_mass_calibration.IsChecked           = Sweet.lollipop.mass_calibration;
            cb_retention_time_calibration.IsChecked = Sweet.lollipop.retention_time_calibration;
            nud_cali_mass_tolerance.Value           = Convert.ToDecimal(Sweet.lollipop.cali_mass_tolerance);
            nud_cali_rt_tolerance.Value             = Convert.ToDecimal(Sweet.lollipop.cali_rt_tolerance);

            // Initialize components in "2. Set Parameters" that fall under "1. Choose Analysis->MetaMorpheus Top-Down Search"
            // p.s. Formerly named cmbx_dissociation_types
            cmb_dissociation_types.Items.Clear();
            cmb_dissociation_types.ItemsSource = new object[] { DissociationType.HCD, DissociationType.CID, DissociationType.ECD, DissociationType.ETD, DissociationType.EThcD };
            cmb_dissociation_types.SelectedIndex = 0;

            this.MDIParent.enable_neuCodeProteoformPairsToolStripMenuItem(Sweet.lollipop.neucode_labeled);
            this.MDIParent.enable_quantificationToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.Quantification));
            this.MDIParent.enable_topDownToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.TopDown));
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
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, new List<Purpose> {  Purpose.ProteinDatabase }).Count() > 0)
                DataTables.Add(DisplayInputFile.FormatInputFileTable(Sweet.lollipop.get_files(Sweet.lollipop.input_files, new List<Purpose> { Purpose.ProteinDatabase }).Select(x => new DisplayInputFile(x)).ToList(), "ProteinDatabases", new List<Purpose> { Purpose.ProteinDatabase}));
            return DataTables;
        }

        public void ClearListsTablesFigures(bool clear_following)
        {
            Sweet.lollipop.input_files.Clear();
            Sweet.save_actions.Clear();
            Sweet.loaded_actions.Clear();
            Sweet.lollipop.results_folder = "";
            //tb_resultsFolder.Text = "";
            if (clear_following)
            {
                for (int i = this.MDIParent.forms.IndexOf(this) + 1; i < this.MDIParent.forms.Count; i++)
                {
                    ISweetForm sweet = this.MDIParent.forms[i];
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
            if (ReadyToRunTheGamut() == true)
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

            this.MDIParent.enable_neuCodeProteoformPairsToolStripMenuItem((bool)rb_neucode.IsChecked);
            Sweet.lollipop.neucode_labeled = (bool)rb_neucode.IsChecked;

            foreach (InputFile f in Sweet.lollipop.input_files)
            {
                if ((bool)rb_neucode.IsChecked)
                    f.label = Labeling.NeuCode;
                if ((bool)rb_unlabeled.IsChecked)
                    f.label = Labeling.Unlabeled;
            }
            populate_file_lists();
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

        private void rb_deconvolution_CheckedChanged(object sender, RoutedEventArgs e)
        {
            populate_file_lists();
        }

        private void rb_topdown_search_CheckedChanged(object sender, EventArgs e)
        {
            populate_file_lists();
        }

        //Using property notification instead
        private void cmb_loadTable1_SelectedIndexChanged(object sender, SelectionChangedEventArgs e)
        {
            
            CB_select = cmb_loadTable1.SelectedIndex;
        }

        private void populate_file_lists()
        {
            cmb_loadTable1.Items.Clear();

            // Toggle visibility of components according to checked Radio Button in "1. Choose Analysis"
            if (rb_standardOptions.IsChecked == true) 
            {
                // In "2. Set Parameters"
                panel_neucode_labeled_unlabled.Visibility   = Visibility.Visible;
                panel_fullrun.Visibility                    = Visibility.Visible;
                panel_calibrate.Visibility                  = Visibility.Collapsed;
                panel_minmaxcharge.Visibility               = Visibility.Collapsed;
                panel_topdown_search.Visibility             = Visibility.Collapsed;

                // In "3. Load Data Using Drop Down Menu"
                cmb_loadTable1.IsEnabled = true;
                for (int i = 0; i < 4; i++)
                    cmb_loadTable1.Items.Add(Lollipop.file_lists[i]);
                cmb_loadTable1.SelectedIndex = 0;

                // In "4. Start Analysis"
                panel_btn_stepthru.Visibility        = Visibility.Visible;
                panel_btn_fullrun.Visibility         = Visibility.Visible;
                panel_btn_calib.Visibility           = Visibility.Collapsed;
                panel_btn_deconv.Visibility          = Visibility.Collapsed;
                panel_btn_topdown_search.Visibility  = Visibility.Collapsed;            
            }

            else if (rb_chemicalCalibration.IsChecked == true)
            {
                // In "2. Set Parameters"
                panel_neucode_labeled_unlabled.Visibility   = Visibility.Visible;
                panel_calibrate.Visibility                  = Visibility.Visible;
                panel_fullrun.Visibility                    = Visibility.Collapsed;
                panel_minmaxcharge.Visibility               = Visibility.Collapsed;
                panel_topdown_search.Visibility             = Visibility.Collapsed;

                // In "3. Load Data Using Drop Down Menu"
                cmb_loadTable1.IsEnabled = true;
                for (int i = 4; i < 7; i++)
                    cmb_loadTable1.Items.Add(Lollipop.file_lists[i]);
                cmb_loadTable1.SelectedIndex = 0;

                // In "4. Start Analysis"
                panel_btn_calib.Visibility           = Visibility.Visible;
                panel_btn_stepthru.Visibility        = Visibility.Collapsed;
                panel_btn_fullrun.Visibility         = Visibility.Collapsed;
                panel_btn_deconv.Visibility          = Visibility.Collapsed;
                panel_btn_topdown_search.Visibility  = Visibility.Collapsed;
            }

            else if (rb_deconvolution.IsChecked == true)
            {
                // In "2. Set Parameters"
                panel_minmaxcharge.Visibility               = Visibility.Visible;
                panel_neucode_labeled_unlabled.Visibility   = Visibility.Collapsed;
                panel_calibrate.Visibility                  = Visibility.Collapsed;
                panel_fullrun.Visibility                    = Visibility.Collapsed;
                panel_topdown_search.Visibility             = Visibility.Collapsed;

                // In "3. Load Data Using Drop Down Menu"
                cmb_loadTable1.Items.Add(Lollipop.file_lists[4]);
                cmb_loadTable1.SelectedIndex = 0;
                cmb_loadTable1.IsEnabled = false;

                // In "4. Start Analysis"
                panel_btn_deconv.Visibility          = Visibility.Visible;
                panel_btn_stepthru.Visibility        = Visibility.Collapsed;
                panel_btn_fullrun.Visibility         = Visibility.Collapsed;
                panel_btn_calib.Visibility           = Visibility.Collapsed;
                panel_btn_topdown_search.Visibility  = Visibility.Collapsed;
            }

            else if (rb_topdown_search.IsChecked == true)
            {
                // In "2. Set Parameters"
                panel_topdown_search.Visibility             = Visibility.Visible;
                panel_minmaxcharge.Visibility               = Visibility.Collapsed;
                panel_neucode_labeled_unlabled.Visibility   = Visibility.Collapsed;
                panel_calibrate.Visibility                  = Visibility.Collapsed;
                panel_fullrun.Visibility                    = Visibility.Collapsed;

                // In "3. Load Data Using Drop Down Menu"
                cmb_loadTable1.Items.Add(Lollipop.file_lists[4]);
                cmb_loadTable1.Items.Add(Lollipop.file_lists[2]);
                cmb_loadTable1.SelectedIndex = 0;
                cmb_loadTable1.IsEnabled = false;

                // In "4. Start Analysis"
                panel_btn_topdown_search.Visibility  = Visibility.Visible;
                panel_btn_deconv.Visibility          = Visibility.Collapsed;
                panel_btn_stepthru.Visibility        = Visibility.Collapsed;
                panel_btn_fullrun.Visibility         = Visibility.Collapsed;
                panel_btn_calib.Visibility           = Visibility.Collapsed;
            }

            CB_select = 0;
            cmb_loadTable1.SelectedItem = cmb_loadTable1.Items[cb_select];
            LabelTxt = cmb_loadTable1.Items[cb_select].ToString();
            //MessageBox.Show(LabelTxt + " , " + Lollipop.file_lists.Count());

            reload_dgvs();          
            refresh_dgvs();
        }
        #endregion GENERAL TABLE OPTIONS Private Methods

        #region DGV DRAG AND DROP Private Methods
        private void dgv_deconResults_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable1, dgv_loadFiles1);
        }
        //this one is not called
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

            if (this.MDIParent != null) //doesn't work first time
            {
                this.MDIParent.enable_quantificationToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.Quantification));
                this.MDIParent.enable_topDownToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.TopDown));
            }
        }

        private void reload_dgvs()
        {
            DisplayUtility.FillDataGridView(dgv_loadFiles1, Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[Lollipop.file_lists.ToList().IndexOf(LabelTxt)]).Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles1, Lollipop.file_types[Lollipop.file_lists.ToList().IndexOf(LabelTxt)]);
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
            if (dr == true)
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
        // formerly bt_stepthru_Click
        private void btn_stepthru_Click(object sender, RoutedEventArgs e)
        {
            if (rb_standardOptions.IsChecked == true)
            {
                this.MDIParent.resultsToolStripMenuItem.IsSubmenuOpen = true;
                MessageBox.Show("Use the Results menu to step through processing results.\n\n" +
                    "Load results and databases in this panel, and then proceed to Raw Experimental Components.", "Step Through Introduction.");
            }
        }

        private void btn_fullRun_Click(object sender, RoutedEventArgs e)
        {
            Stopwatch successful_run = this.MDIParent.full_run();
            if (successful_run != null) MessageBox.Show("Successfully ran method in "
                + String.Format("{0:00}:{1:00}:{2:00}.{3:00}", successful_run.Elapsed.Hours, successful_run.Elapsed.Minutes, successful_run.Elapsed.Seconds, successful_run.Elapsed.Milliseconds / 10)
                + ". Feel free to explore using the Results menu.", "Full Run");
            else MessageBox.Show("Method did not successfully run.", "Full Run");
        }

        // formerly bt_clearResults_Click
        private void btn_clearResults_Click(object sender, RoutedEventArgs e)
        {
            Sweet.lollipop = new Lollipop();
            ClearListsTablesFigures(true);
        }

        private System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();

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

        #region CHANGED TABLE SELECTION Private Methods
        // formerly bt_calibrate_Click
        private void btn_calibrate_Click(object sender, RoutedEventArgs e)
        {
            if (Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length == 0)
            {
                MessageBox.Show("First create a theoretical proteoform database. On the Results tab, select Theoretical Proteoform Database.");
                return;
            }
            Sweet.lollipop.read_in_calibration_td_hits();
            MessageBox.Show(Sweet.lollipop.calibrate_files());
        }

        // formerly bt_deconvolute_Click
        private void btn_deconvolute_Click(object sender, RoutedEventArgs e)
        {
            if (Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.SpectraFile).Count() == 0)
            {
                MessageBox.Show("Please enter raw files to deconvolute.");
                return;
            }
            string deconv_results = Sweet.lollipop.promex_deconvolute(Convert.ToInt32(nud_maxcharge.Value), Convert.ToInt32(nud_mincharge.Value), Environment.CurrentDirectory);
            MessageBox.Show(deconv_results);
        }

        // formerly bt_topdown_search_Click
        private void btn_topdown_search_Click(object sender, RoutedEventArgs e)
        {
            if (Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.SpectraFile).Count() == 0)
            {
                MessageBox.Show("Please enter at least one raw file to search."); return;
            }
            if (Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.ProteinDatabase).Count() == 0)
            {
                MessageBox.Show("Please enter at least one database files to search."); return;
            }
            MessageBox.Show(Sweet.lollipop.metamorpheus_topdown(Environment.CurrentDirectory, (bool)cb_carbamidomethylate.IsChecked,
                (double)nud_precursor_mass_tol.Value,
                (double)nud_product_mass_tol.Value, (DissociationType)cmb_dissociation_types.SelectedItem));
        }
        #endregion CHANGED TABLE SELECTION Private Methods

        #region FILTERS Private Methods
        //function moved to property get
        /*private void tb_filter1_TextChanged(object sender, PropertyChangedEventArgs e)
        {
            int selected_index = Lollipop.file_lists.ToList().IndexOf(cmb_loadTable1.SelectedItem.ToString());
            DisplayUtility.FillDataGridView(dgv_loadFiles1, ExtensionMethods.filter(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Lollipop.file_types[selected_index]), tb_filter1.Text).OfType<InputFile>().Select(f => new DisplayInputFile(f)));
            DisplayInputFile.FormatInputFileTable(dgv_loadFiles1, Lollipop.file_types[selected_index]);
        }*/
        #endregion FILTERS Private Methods

        #region CHANGE ALL CELLS Private methods
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
        #endregion CHANGE ALL CELLS Private methods

        #region Cell Validation Private Methods
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
        #endregion Cell Validation Private Methods

        #region Chemical Calibration Private Methods
        private void cb_calibrate_raw_files_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Sweet.lollipop.calibrate_raw_files = (bool)cb_calibrate_raw_files.IsChecked;
        }

        private void cb_calibrate_td_files_CheckedChanged(object sender, RoutedEventArgs e)
        {
            Sweet.lollipop.calibrate_td_files = (bool)cb_calibrate_td_files.IsChecked;
        }

        // Formerly cb_mass_cali_CheckedChanged
        private void cb_mass_calibration_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.mass_calibration = (bool)cb_mass_calibration.IsChecked;
        }

        // Formerly cb_rt_cali_CheckedChanged
        private void cb_retention_time_calibration_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.retention_time_calibration = (bool)cb_retention_time_calibration.IsChecked;
        }

        private void nud_cali_mass_tolerance_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.cali_mass_tolerance = Convert.ToDouble(nud_cali_mass_tolerance.Value);
        }

        private void nud_cali_rt_tolerance_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.cali_rt_tolerance = Convert.ToDouble(nud_cali_rt_tolerance.Value);
        }
        #endregion Chemical Calibration Private Methods
    }
}
