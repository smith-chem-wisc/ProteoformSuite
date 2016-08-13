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
    public partial class LoadDeconvolutionResults : Form
    {

        public LoadDeconvolutionResults()
        {
            InitializeComponent();
        }

        public void LoadDeconvolutionResults_Load(object sender, EventArgs e)
        {

            clb_deconResults.Sorted = true;
            clb_quantResults.Sorted = true;
            clb_calibResults.Sorted = true;
            //formatDataGridview();
            btn_neucode.Checked = true;
            //DisplayUtility.formatDataFileInputGridView(dgv_deconResults);
        }

        private bool FirstLineOK (string fileName)
        {
            bool fileOK = true;

            return fileOK;
        }

        private void clb_deconResults_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string enteredFile in files)
            {
                string _path = Path.GetDirectoryName(enteredFile);
                string _filename = Path.GetFileNameWithoutExtension(enteredFile);
                string _extension = Path.GetExtension(enteredFile);

                switch (_extension)
                {
                    case ".xlsx":
                        InputFile f = new InputFile();
                        f.path = _path;
                        f.filename = _filename;
                        f.extension = _extension;
                        if (btn_neucode.Checked) f.lbl = label.neuCode;
                        else f.lbl = label.unlabeled;
                        f.inputFileType = inputFileType.id;
                        if (!Lollipop.deconResultsFiles.Any(item => item.filename == f.filename)) Lollipop.deconResultsFiles.Add(f);

                        break;
                    default:
                        break;
                }
            }
            clb_deconResults.DataSource = (from s in Lollipop.deconResultsFiles select s.filename).ToList();
            matchFiles();
        }

        private void clb_deconResults_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void clb_quantResults_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string enteredFile in files)
            {
                string _path = Path.GetDirectoryName(enteredFile);
                string _filename = Path.GetFileNameWithoutExtension(enteredFile);
                string _extension = Path.GetExtension(enteredFile);

                switch (_extension)
                {
                    case ".xlsx":
                        InputFile f = new InputFile();
                        f.path = _path;
                        f.filename = _filename;
                        f.extension = _extension;
                        if (btn_neucode.Checked) f.lbl = label.neuCode;
                        else f.lbl = label.unlabeled;
                        f.inputFileType = inputFileType.id;
                        if (!Lollipop.quantResultsFiles.Any(item => item.filename == f.filename)) Lollipop.quantResultsFiles.Add(f);

                        break;
                    default:
                        break;
                }
            }
            clb_quantResults.DataSource = (from s in Lollipop.quantResultsFiles select s.filename).ToList();
            matchFiles();
        }

        private void clb_quantResults_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void clb_calibResults_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string enteredFile in files)
            {
                string _path = Path.GetDirectoryName(enteredFile);
                string _filename = Path.GetFileNameWithoutExtension(enteredFile);
                string _extension = Path.GetExtension(enteredFile);

                switch (_extension)
                {
                    case ".txt":
                    case ".tsv":
                        InputFile f = new InputFile();
                        f.path = _path;
                        f.filename = _filename;
                        f.extension = _extension;
                        if (btn_neucode.Checked) f.lbl = label.neuCode;
                        else f.lbl = label.unlabeled;
                        f.inputFileType = inputFileType.id;
                        if (!Lollipop.calResultsFiles.Any(item => item.filename == f.filename)) Lollipop.calResultsFiles.Add(f);

                        break;
                    default:
                        break;
                }
            }
            clb_calibResults.DataSource = (from s in Lollipop.calResultsFiles select s.filename).ToList();
            matchFiles();
        }

        private void clb_calibResults_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private void btn_protIdResultsClear_Click(object sender, EventArgs e)
        {
            //Lollipop.deconResultsFileNames.Clear();
            Lollipop.deconResultsFiles.Clear();

            clb_deconResults.DataSource = (from s in Lollipop.deconResultsFiles select s.filename).ToList();
            matchFiles();
        }

        private void btn_neucode_CheckedChanged(object sender, EventArgs e)
        {
            ((ProteoformSweet)MdiParent).enable_neuCodeProteoformPairsToolStripMenuItem(btn_neucode.Checked);
            Lollipop.neucode_labeled = btn_neucode.Checked;
            Lollipop.neucode_light_lysine = btn_neucode.Checked;
            Lollipop.natural_lysine_isotope_abundance = !btn_neucode.Checked;

            foreach (InputFile f in Lollipop.deconResultsFiles)
            {
                if (btn_neucode.Checked)
                    f.lbl = label.neuCode;
                else
                    f.lbl = label.unlabeled;
            }

            foreach (InputFile f in Lollipop.quantResultsFiles)
            {
                if (btn_neucode.Checked)
                    f.lbl = label.neuCode;
                else
                    f.lbl = label.unlabeled;
            }

            foreach (InputFile f in Lollipop.calResultsFiles)
            {
                if (btn_neucode.Checked)
                    f.lbl = label.neuCode;
                else
                    f.lbl = label.unlabeled;
            }

            clb_deconResults.DataSource = null;
            clb_quantResults.DataSource = null;
            clb_calibResults.DataSource = null;

            clb_deconResults.DataSource = (from s in Lollipop.deconResultsFiles select s.filename).ToList();
            clb_quantResults.DataSource = (from s in Lollipop.quantResultsFiles select s.filename).ToList(); ;
            clb_calibResults.DataSource = (from s in Lollipop.calResultsFiles select s.filename).ToList();

            matchFiles();

        }

        private void btn_protIdResultsAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Excel Files(.xlsx) | *.xlsx";

            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "My Deconvolution 4.0 Results Files";

            DialogResult dr = openFileDialog1.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                foreach (String enteredFile in openFileDialog1.FileNames)
                {
                    try
                    {
                        string _path = Path.GetDirectoryName(enteredFile);
                        string _filename = Path.GetFileNameWithoutExtension(enteredFile);
                        string _extension = Path.GetExtension(enteredFile);

                        InputFile f = new InputFile();
                        f.path = _path;
                        f.filename = _filename;
                        f.extension = _extension;
                        if (btn_neucode.Checked) f.lbl = label.neuCode;
                        else f.lbl = label.unlabeled;
                        f.inputFileType = inputFileType.id;

                        if(!Lollipop.deconResultsFiles.Any(item => item.filename == f.filename)) Lollipop.deconResultsFiles.Add(f);
                    }
                    catch
                    {
                        MessageBox.Show("something went wrong with the input");
                    }

                }
            }

            clb_deconResults.DataSource = (from s in Lollipop.deconResultsFiles select s.filename).ToList();
            matchFiles();
        }

        private void btn_protQuantResultsAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Excel Files(.xlsx) | *.xlsx";

            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "My Deconvolution 4.0 Results Files";

            DialogResult dr = openFileDialog1.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                foreach (String enteredFile in openFileDialog1.FileNames)
                {
                    try
                    {
                        string _path = Path.GetDirectoryName(enteredFile);
                        string _filename = Path.GetFileNameWithoutExtension(enteredFile);
                        string _extension = Path.GetExtension(enteredFile);

                        InputFile f = new InputFile();
                        f.path = _path;
                        f.filename = _filename;
                        f.extension = _extension;
                        if (btn_neucode.Checked) f.lbl = label.neuCode;
                        else f.lbl = label.unlabeled;
                        f.inputFileType = inputFileType.quant;

                        if (!Lollipop.quantResultsFiles.Any(item => item.filename == f.filename)) Lollipop.quantResultsFiles.Add(f);
                    }
                    catch
                    {
                        MessageBox.Show("something went wrong with the input");
                    }

                }
            }

            clb_quantResults.DataSource = (from s in Lollipop.quantResultsFiles select s.filename).ToList();
            matchFiles();
        }

        private void btn_protQuantResultsClear_Click(object sender, EventArgs e)
        {
            Lollipop.quantResultsFiles.Clear();
            clb_quantResults.DataSource = Lollipop.quantResultsFiles;

            matchFiles();
        }

        private void btn_protCalibResultsAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Text Files (*.txt, *.tsv) | *.txt; *.tsv";

            openFileDialog1.Multiselect = true;
            openFileDialog1.Title = "My Calibration Results Files";

            DialogResult dr = openFileDialog1.ShowDialog();

            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                foreach (String enteredFile in openFileDialog1.FileNames)
                {
                    try
                    {
                        string _path = Path.GetDirectoryName(enteredFile);
                        string _filename = Path.GetFileNameWithoutExtension(enteredFile);
                        string _extension = Path.GetExtension(enteredFile);

                        InputFile f = new InputFile();
                        f.path = _path;
                        f.filename = _filename;
                        f.extension = _extension;
                        if (btn_neucode.Checked) f.lbl = label.neuCode;
                        else f.lbl = label.unlabeled;
                        f.inputFileType = inputFileType.calibration;

                        if (!Lollipop.calResultsFiles.Any(item => item.filename == f.filename)) Lollipop.calResultsFiles.Add(f);
                    }
                    catch
                    {
                        MessageBox.Show("something went wrong with the input");
                    }

                }
            }

            clb_calibResults.DataSource = (from s in Lollipop.calResultsFiles select s.filename).ToList();
            matchFiles();
        }

        private void btn_protCalibResultsClear_Click(object sender, EventArgs e)
        {
            Lollipop.calResultsFiles.Clear();
            Lollipop.correctionFactorFilenames.Clear();
            clb_calibResults.DataSource = Lollipop.calResultsFiles;

            matchFiles();
        }

        private void matchFiles()
        {
            foreach (string filename in Lollipop.deconResultsFiles.Select(item => item.filename).ToList())
            {
                if (clb_calibResults.Items.Contains(filename))
                {
                    int index = clb_deconResults.Items.IndexOf(filename);
                    clb_deconResults.SetItemCheckState(index, CheckState.Checked);
                }
                else
                {
                    int index = clb_deconResults.Items.IndexOf(filename);
                    clb_deconResults.SetItemCheckState(index, CheckState.Unchecked);
                }
            }

            foreach (string filename in Lollipop.calResultsFiles.Select(item => item.filename).ToList())
            {
                if (clb_deconResults.Items.Contains(filename))
                {
                    int index = clb_calibResults.Items.IndexOf(filename);
                    clb_calibResults.SetItemCheckState(index, CheckState.Checked);
                }
                else
                {
                    int index = clb_calibResults.Items.IndexOf(filename);
                    clb_calibResults.SetItemCheckState(index, CheckState.Unchecked);
                }
            }
        }

        private void dgv_deconResults_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (string enteredFile in files)
            {
                string _path = Path.GetDirectoryName(enteredFile);
                string _filename = Path.GetFileNameWithoutExtension(enteredFile);
                string _extension = Path.GetExtension(enteredFile);

                switch (_extension)
                {
                    case ".xlsx":
                        InputFile f = new InputFile();
                        f.path = _path;
                        f.filename = _filename;
                        f.extension = _extension;
                        if (btn_neucode.Checked) f.lbl = label.neuCode;
                        else f.lbl = label.unlabeled;
                        f.inputFileType = inputFileType.id;
                        if (!Lollipop.deconResultsFiles.Any(item => item.filename == f.filename)) Lollipop.deconResultsFiles.Add(f);

                        break;
                    default:
                        break;
                }
            }

            BindingSource _bindingSource = new BindingSource();
            dgv_deconResults.DataSource = _bindingSource;
            _bindingSource.DataSource = Lollipop.deconResultsFiles;

            //DisplayUtility.formatDataFileInputGridView(dgv_deconResults, Lollipop.deconResultsFiles);
            //dgv_deconResults.DataSource = (from s in Lollipop.deconResultsFiles select s.filename).ToList();
            //matchFiles();
        }

        private void dgv_deconResults_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.All;
        }

        private string BindProperty(object property, string propertyName)
        {
            string retValue;

            retValue = "";

            if (propertyName.Contains("."))
            {
                PropertyInfo[] arrayProperties;
                string leftPropertyName;

                leftPropertyName = propertyName.Substring(0, propertyName.IndexOf("."));
                arrayProperties = property.GetType().GetProperties();

                foreach (PropertyInfo propertyInfo in arrayProperties)
                {
                    if (propertyInfo.Name == leftPropertyName)
                    {
                        retValue = BindProperty(propertyInfo.GetValue(property, null), propertyName.Substring(propertyName.IndexOf(".") + 1));
                        break;
                    }
                }
            }
            else
            {
                Type propertyType;
                PropertyInfo propertyInfo;

                propertyType = property.GetType();
                propertyInfo = propertyType.GetProperty(propertyName);
                retValue = propertyInfo.GetValue(property, null).ToString();
            }

            return retValue;
        }

        private void dgv_deconResults_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            //MessageBox.Show("cell formatting" + dgv_deconResults.Columns[e.ColumnIndex].DataPropertyName.ToString());
            if ((dgv_deconResults.Rows[e.RowIndex].DataBoundItem != null) && (dgv_deconResults.Columns[e.ColumnIndex].DataPropertyName.Contains(".")))
                e.Value = BindProperty(dgv_deconResults.Rows[e.RowIndex].DataBoundItem, dgv_deconResults.Columns[e.ColumnIndex].DataPropertyName);
        }
    }
}
