using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Excel = Microsoft.Office.Interop.Excel; //Right click Solution/Explorer/References. Then Add  "Reference". Assemblies/Extension/Microsoft.Office.Interop.Excel



namespace ProteoformSuite
{
    public partial class CalibrateResults : Form
    {
        public CalibrateResults()
        {
            InitializeComponent();
        }
        // DGV DRAG AND DROP EVENTS
        private void dgv_deconResults_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            enter_input_files(files, new List<string> { ".xlsx" }, Purpose.CalibrationIdentification);
            match_files();
            DisplayUtility.FillDataGridView(dgv_identificationFiles, Lollipop.calibration_identification_files());
        }
        private void dgv_rawFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            enter_input_files(files, new List<string> { "raw" }, Purpose.RawFile);
            match_files();

            DisplayUtility.FillDataGridView(dgv_rawFiles, Lollipop.raw_files());
        }
        private void dgv_tdFiles_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            enter_input_files(files, new List<string> { "xlsx" }, Purpose.CalibrationTopDown);
            match_files();
            DisplayUtility.FillDataGridView(dgv_rawFiles, Lollipop.calibration_topdown_files());
        }
        private void dgv_deconResults_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }
        private void dgv_rawFiles_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }
        private void dgv_tdFiles_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }
        private void enter_input_files(string[] files, IEnumerable<string> acceptable_extensions, Purpose purpose)
        {
            foreach (string enteredFile in files)
            {
                string path = Path.GetDirectoryName(enteredFile);
                string filename = Path.GetFileNameWithoutExtension(enteredFile);
                string extension = Path.GetExtension(enteredFile);
                Labeling label = Labeling.Unlabeled;
                if (acceptable_extensions.Contains(extension) && !Lollipop.input_files.Where(f => f.purpose == purpose).Any(f => f.filename == filename))
                {
                    reload_dgvs();
                    InputFile file = new InputFile(path, filename, extension, label, purpose);
                    Lollipop.input_files.Add(file);
                }
            }
        }

        private void match_files() //for dgv
        {
            // Look for results files with the same filename as a calibration file, and show that they're matched
                foreach (InputFile file in Lollipop.raw_files())
                    if (Lollipop.input_files.Where(f => f.purpose != Purpose.RawFile).Select(f => f.filename).Contains(file.filename))
                    {
                        IEnumerable<InputFile> matching_files = Lollipop.input_files.Where(f => f.purpose != Purpose.RawFile && f.filename == file.filename);
                        InputFile matching_file = matching_files.First();
                        if (matching_files.Count() != 1) MessageBox.Show("Warning: There is more than one results file named " + file.filename + ". Will only match calibration to the first one from " + matching_file.purpose.ToString() + ".");
                        file.matchingCalibrationFile = true;
                        matching_file.matchingCalibrationFile = true;
                    }
            refresh_dgvs();
            if (Lollipop.raw_files().Count() > 0 && !Lollipop.raw_files().Any(f => f.matchingCalibrationFile))
                MessageBox.Show("To calibration deconvolution results, please make sure the raw files have the same filenames as the deconvolution results to which they correspond.", "Orphaned Calibration Files", MessageBoxButtons.OK);
        }

        private void refresh_dgvs()
        {
            foreach (DataGridView dgv in new List<DataGridView> { dgv_identificationFiles, dgv_rawFiles, dgv_tdFiles })
            {
                dgv.Refresh();
            }
        }

        private void reload_dgvs()
        {
            DisplayUtility.FillDataGridView(dgv_identificationFiles, Lollipop.calibration_identification_files());
            DisplayUtility.FillDataGridView(dgv_rawFiles, Lollipop.raw_files());
            DisplayUtility.FillDataGridView(dgv_tdFiles, Lollipop.calibration_topdown_files());
        }

        // CELL FORMATTING EVENTS
        private void dgv_deconResults_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((dgv_identificationFiles.Rows[e.RowIndex].DataBoundItem != null) && (dgv_identificationFiles.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
                e.Value = BindProperty(dgv_identificationFiles.Rows[e.RowIndex].DataBoundItem, dgv_identificationFiles.Columns[e.ColumnIndex].DataPropertyName);
        }
        private void dgv_tdFiles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((dgv_tdFiles.Rows[e.RowIndex].DataBoundItem != null) && (dgv_tdFiles.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
                e.Value = BindProperty(dgv_tdFiles.Rows[e.RowIndex].DataBoundItem, dgv_tdFiles.Columns[e.ColumnIndex].DataPropertyName);
        }
        private void dgv_rawFiles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if ((dgv_rawFiles.Rows[e.RowIndex].DataBoundItem != null) && (dgv_rawFiles.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
                e.Value = BindProperty(dgv_rawFiles.Rows[e.RowIndex].DataBoundItem, dgv_rawFiles.Columns[e.ColumnIndex].DataPropertyName);
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

        private void btn_protIdResultsAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "My Deconvolution 4.0 Results Files";
            openFileDialog1.Filter = "Excel Files(.xlsx) | *.xlsx";
            openFileDialog1.Multiselect = true;

            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
                enter_input_files(openFileDialog1.FileNames, new List<string> { ".xlsx" }, Purpose.CalibrationIdentification);

            DisplayUtility.FillDataGridView(dgv_identificationFiles, Lollipop.calibration_identification_files());
            match_files();
        }

        private void bt_tdResultsAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "ProSight Top-Down Results Files";
            openFileDialog1.Filter = "Excel Files(.xlsx) | *.xlsx";
            openFileDialog1.Multiselect = true;

            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                enter_input_files(openFileDialog1.FileNames, new List<string> { ".xlsx" }, Purpose.CalibrationTopDown);
                DisplayUtility.FillDataGridView(dgv_tdFiles, Lollipop.calibration_topdown_files());
            }
            else return;
            if (Lollipop.ptmlist_filepath.Length == 0) get_ptm_list();
            ProteomeDatabaseReader.oldPtmlistFilePath = Lollipop.ptmlist_filepath;
            Lollipop.uniprotModificationTable = Lollipop.proteomeDatabaseReader.ReadUniprotPtmlist(); //need for reading in TD hit PTMs
        }

        private void bt_rawFilesAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Title = "My Thermo Raw Files";
            openFileDialog1.Filter = "Raw Files (*.raw) | *.raw";
            openFileDialog1.Multiselect = true;

            DialogResult dr = openFileDialog1.ShowDialog();
            if (dr == DialogResult.OK)
            {
                enter_input_files(openFileDialog1.FileNames, new List<string> { ".raw" }, Purpose.RawFile);
                DisplayUtility.FillDataGridView(dgv_rawFiles, Lollipop.raw_files());
            }
            else return;
            match_files();
        }

        private void bt_rawFilesClear_Click(object sender, EventArgs e)
        {
            Lollipop.input_files = Lollipop.input_files.Except(Lollipop.raw_files()).ToList();
            DisplayUtility.FillDataGridView(dgv_rawFiles, Lollipop.raw_files());
        }

        private void btn_protIdResultsClear_Click(object sender, EventArgs e)
        {
            Lollipop.input_files = Lollipop.input_files.Except(Lollipop.calibration_identification_files()).ToList();
            DisplayUtility.FillDataGridView(dgv_identificationFiles, Lollipop.calibration_identification_files());
        }

        private void bt_tdResultsClear_Click(object sender, EventArgs e)
        {
            Lollipop.input_files = Lollipop.input_files.Except(Lollipop.calibration_topdown_files()).ToList();
            DisplayUtility.FillDataGridView(dgv_tdFiles, Lollipop.calibration_topdown_files());
        }

        private void get_ptm_list()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            MessageBox.Show("Please select a Uniprot ptm list.");
            DialogResult dr = ofd.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string ptm_list = ofd.FileName;
                Lollipop.ptmlist_filepath = ptm_list;
            }
            else return;
        }

        private void cb_calibrate_lock_mass_peptide_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.calibrate_lock_mass = cb_calibrate_lock_mass_peptide.Checked;
            if (!cb_calibrate_lock_mass_peptide.Checked && !cb_calibrate_with_td_results.Checked)
            {
                MessageBox.Show("Check at least one of the calibration options.");
                cb_calibrate_lock_mass_peptide.Checked = true;
                Lollipop.calibrate_lock_mass = true;
            }
        }

        private void cb_calibrate_with_td_results_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.calibrate_td_results = cb_calibrate_with_td_results.Checked;
            if (!cb_calibrate_lock_mass_peptide.Checked && !cb_calibrate_with_td_results.Checked)
            {
                MessageBox.Show("Check at least one of the calibration options.");
                cb_calibrate_with_td_results.Checked = true;
                Lollipop.calibrate_td_results = true;
            }
        }

        private void bt_calibrate_Click(object sender, EventArgs e)
        {
            Lollipop.read_in_calibration_td_hits();
            Lollipop.get_calibration_points();
            Lollipop.calibrate_td_hits();
            Lollipop.calibrate_components();
        }
    }
}
