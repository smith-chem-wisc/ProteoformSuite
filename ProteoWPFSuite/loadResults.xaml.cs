using System;
using System.Linq;
using System.Windows;
using ProteoformSuiteInternal;
using System.Data;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Reflection;
/// <Problems>
/// - Should convert window to proteoform sweet?
/// - Currently keeping main window but not sure if still keeping it in the future
/// </Problems>
/// <remarks>
/// - Dragevents are using forms version in order to be consistent with datagridview
/// - Comments with three slashes are for future implementation
/// - Controls are default to be WPF except for data Grid view
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
    public partial class LoadResults : UserControl, ITabbedMDI, ISweetForm
    {
        #region Interface Area
        public string UniqueTabName { get; set; }
        public event delClosed BeingClosed;
        public void OnClosing(ITabbedMDI sender) { 
            if (BeingClosed != null)
                {
                    BeingClosed(this, new EventArgs());
                }
        }
        #endregion Interface Area

        public LoadResults()
        {
            InitializeComponent();
            populate_file_lists();
        }

        #region Public Property
        public List<DataTable> DataTables { get; private set; }
        #endregion

        #region Private Property
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
        private void rb_neucode_CheckedChanged(object sender, EventArgs e)
        {
            ProteoformSweet parMDI = ((MainWindow)MDIHelpers.getParentWindow(this)).MDIParentControl; //get the parent control of the form;
            ///parMDI.enable_neuCodeProteoformPairsToolStripMenuItem(rb_neucode.Checked);
            Sweet.lollipop.neucode_labeled = (bool)rb_neucode.IsChecked;
            Sweet.lollipop.neucode_light_lysine = (bool)rb_neucode.IsChecked;
            Sweet.lollipop.natural_lysine_isotope_abundance = !(bool)rb_neucode.IsChecked;

            foreach (InputFile f in Sweet.lollipop.input_files)
            {
                if ((bool)rb_neucode.IsChecked) f.label = Labeling.NeuCode;
                if ((bool)rb_unlabeled.IsChecked) f.label = Labeling.Unlabeled;
            }
            populate_file_lists();
        }
        private void rb_unlabeled_CheckedChanged(object sender, EventArgs e)
        { }
        private void rb_standardOptions_CheckedChanged(object sender, EventArgs e)
        {
            populate_file_lists();
        }
        private void rb_chemicalCalibration_CheckedChanged(object sender, EventArgs e)
        {
            populate_file_lists();
        }

        private void rb_advanced_user_CheckedChanged(object sender, EventArgs e)
        {
            populate_file_lists();
        }

        private void rb_deconvolution_CheckedChanged(object sender, EventArgs e)
        {
            populate_file_lists();
        }
        private void cmb_loadTable1_SelectedIndexChanged(object sender, EventArgs e)
        {
            lb_filter1.Content = cmb_loadTable1.SelectedItem.ToString();
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

            lb_filter1.Content = cmb_loadTable1.SelectedItem.ToString();

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
            ProteoformSweet parMDI = ((MainWindow)MDIHelpers.getParentWindow(this)).MDIParentControl; //get the parent control of the form;
            if (parMDI != null) //doesn't work first time
            {
                ///parMDI.enable_quantificationToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.Quantification));
                ///parMDI.enable_topDownToolStripMenuItem(Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.TopDown));
            }
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
        private void btn_addFiles1_Click(object sender, EventArgs e)
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

            DialogResult dr = openFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
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

    }
}
