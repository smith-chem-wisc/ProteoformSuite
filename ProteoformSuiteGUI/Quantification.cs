using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProteoformSuite
{
    public partial class Quantification : Form
    {
        DataTable dt_quantification = new DataTable();

        public Quantification()
        {
            InitializeComponent();
        }

        private void Quantification_Load(object sender, EventArgs e)
        {
            computeInputFileIntensities();
            runTheGamut();
        }

        private void computeInputFileIntensities()
        {
            foreach (InputFile inputFile in Lollipop.input_files)
            {
                 inputFile.totalIntensity = Lollipop.raw_experimental_components.Where(s => s.input_file == inputFile).ToList().Sum(a => a.intensity_sum);
            }
        }

        private void runTheGamut()
        {
            dt_quantification.Reset();
            addColumnsToDataTable();
            addRowsToDataTable();
            dgv_quantification_results.DataSource = dt_quantification;
        }

        private void addColumnsToDataTable()
        {
            
            dt_quantification.Columns.Add("Experimental_Proteoform", typeof(String));

            List<string> columnLabels = new List<string>();

            if (ckbx_compressBioReps.Checked)
            {
                if (Lollipop.neucode_labeled)
                {
                    foreach (InputFile inputFile in Lollipop.input_files.Where(s => s.purpose == Purpose.Quantification))
                    {
                        columnLabels.Add("Ratio: " + inputFile.lt_condition + "/" + inputFile.hv_condition);
                    }
                }
                else
                {
                    foreach (InputFile inputFile in Lollipop.input_files)
                    {
                        columnLabels.Add("Condition: " + inputFile.lt_condition);
                    }
                }
            }
            else
            {
                if (ckbx_compressTechReps.Checked)
                {
                    if (ckbx_compressFractions.Checked)
                    {
                        if (Lollipop.neucode_labeled)
                        {
                            foreach (InputFile inputFile in Lollipop.input_files.Where(s => s.purpose == Purpose.Quantification))
                            {
                                columnLabels.Add("Ratio: " + inputFile.lt_condition + "/" + inputFile.hv_condition + "; BR-" + inputFile.biological_replicate);
                            }
                        }
                        else
                        {
                            foreach (InputFile inputFile in Lollipop.input_files)
                            {
                                columnLabels.Add("Condition: " + inputFile.lt_condition + "; BR-" + inputFile.biological_replicate);
                            }
                        }
                    }
                    else
                    { //not compressing fractions
                        if (ckbx_compressTechReps.Checked)
                        {
                            if (Lollipop.neucode_labeled)
                            {
                                foreach (InputFile inputFile in Lollipop.input_files.Where(s => s.purpose == Purpose.Quantification))
                                {
                                    columnLabels.Add("Ratio: " + inputFile.lt_condition + "/" + inputFile.hv_condition + "; BR-" + inputFile.biological_replicate + "; FR:-" + inputFile.fraction);
                                }
                            }
                            else
                            {
                                foreach (InputFile inputFile in Lollipop.input_files)
                                {
                                    columnLabels.Add("Condition: " + inputFile.lt_condition + "; BR-" + inputFile.biological_replicate + "; FR:-" + inputFile.fraction);
                                }
                            }
                        }
                        else
                        {//not compressing fractions and not compressing tech reps
                            if (Lollipop.neucode_labeled)
                            {
                                foreach (InputFile inputFile in Lollipop.input_files.Where(s => s.purpose == Purpose.Quantification))
                                {
                                    columnLabels.Add("Ratio: " + inputFile.lt_condition + "/" + inputFile.hv_condition + "; BR-" + inputFile.biological_replicate + "; FR-" + inputFile.fraction + "; TR-" + inputFile.technical_replicate);
                                }
                            }
                            else
                            {
                                foreach (InputFile inputFile in Lollipop.input_files)
                                {
                                    columnLabels.Add("Condition: " + inputFile.lt_condition + "; BR-" + inputFile.biological_replicate + "; FR-" + inputFile.fraction + "; TR-" + inputFile.technical_replicate);
                                }
                            }
                        }
                    }
                }//not compressing technical reps
                else
                {
                    if (Lollipop.neucode_labeled)
                    {
                        foreach (InputFile inputFile in Lollipop.input_files.Where(s => s.purpose == Purpose.Quantification))
                        {
                            columnLabels.Add("Ratio: " + inputFile.lt_condition + "/" + inputFile.hv_condition + "; BR-" + inputFile.biological_replicate + "; FR-" + inputFile.fraction + "; TR-" + inputFile.technical_replicate);
                        }
                    }
                    else
                    {
                        foreach (InputFile inputFile in Lollipop.input_files)
                        {
                            columnLabels.Add("Condition: " + inputFile.lt_condition + "; BR-" + inputFile.biological_replicate + "; FR-" + inputFile.fraction + "; TR-" + inputFile.technical_replicate);
                        }
                    }
                }
            }

            foreach (string lbl in columnLabels.Distinct())
            {
                dt_quantification.Columns.Add(lbl, typeof(Double));
            }

        }

        private void addRowsToDataTable()
        {
            string[] columnNames = dt_quantification.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            List<InputFile>[] inputFileLists = new List<InputFile>[columnNames.Count()];

            if (Lollipop.neucode_labeled)
            {
                inputFileLists[0] = null;
                for (int i = 1; i < columnNames.Count(); i++)
                {
                    string[] columnNamePieces = columnNames[i].Split('-').ToArray();
                    switch (columnNamePieces.Count())
                    {
                        case 1:
                            inputFileLists[i] = (from s in Lollipop.input_files where s.purpose == Purpose.Quantification select s).ToList();
                            break;
                        case 2:
                            inputFileLists[i] = (from s in Lollipop.input_files where s.purpose == Purpose.Quantification where s.biological_replicate == Convert.ToInt32(columnNamePieces[1]) select s).ToList();
                            break;
                        case 3:
                            inputFileLists[i] = (from s in Lollipop.input_files where s.purpose == Purpose.Quantification where s.biological_replicate == Convert.ToInt32(columnNamePieces[1].Split(';').ToList().First()) where s.fraction == Convert.ToInt32(columnNamePieces[2].Split(';').ToList().First()) select s).ToList();
                            break;
                        case 4:
                            inputFileLists[i] = (from s in Lollipop.input_files where s.purpose == Purpose.Quantification where s.biological_replicate == Convert.ToInt32(columnNamePieces[1].Split(';').ToList().First()) where s.fraction == Convert.ToInt32(columnNamePieces[2].Split(';').ToList().First()) where s.technical_replicate == Convert.ToInt32(columnNamePieces[3]) select s).ToList();
                            break;
                    }

                }
            }
            else
            {
                inputFileLists[0] = null;
                for (int i = 1; i < columnNames.Count(); i++)
                {
                    string[] columnNamePieces = columnNames[i].Split('-').ToArray();
                    switch (columnNamePieces.Count())
                    {
                        case 1:
                            inputFileLists[i] = (from s in Lollipop.input_files select s).ToList();
                            break;
                        case 2:
                            inputFileLists[i] = (from s in Lollipop.input_files where s.biological_replicate == Convert.ToInt32(columnNamePieces[1]) select s).ToList();
                            break;
                        case 3:
                            inputFileLists[i] = (from s in Lollipop.input_files where s.biological_replicate == Convert.ToInt32(columnNamePieces[1].Split(';').ToList().First()) where s.fraction == Convert.ToInt32(columnNamePieces[2].Split(';').ToList().First()) select s).ToList();
                            break;
                        case 4:
                            inputFileLists[i] = (from s in Lollipop.input_files where s.biological_replicate == Convert.ToInt32(columnNamePieces[1].Split(';').ToList().First()) where s.fraction == Convert.ToInt32(columnNamePieces[2].Split(';').ToList().First()) where s.technical_replicate == Convert.ToInt32(columnNamePieces[3]) select s).ToList();
                            break;
                    }
                }
            }

            foreach (ExperimentalProteoform eP in Lollipop.proteoform_community.experimental_proteoforms)
            {
                object[] array = new object[columnNames.Count()];
                array[0] = eP.accession;
                for (int i = 1; i < inputFileLists.Count(); i++)
                {
                    array[i] = eP.weightedRatioAndWeightedVariance(inputFileLists[i]).Item1;//Item1 is weighted ratio; Item2 is weighted variance
                }
                dt_quantification.Rows.Add(array);
            }
        }

        private Color HeatMapColor(double value, double min, double max)//(double value, double min, double max)
        {
            double val;
            int r = 0;
            int g = 0;
            int b = 0;
            double middleValue = (max - min) / 2 + min;

            if (value > (min + (max - min) / 2)) // positive - green
            {
                val = (Math.Min(value, max)-middleValue)/(max-middleValue);
                g = Convert.ToByte(255 * val);
            }
            else // negative red
            {
                val = (middleValue - Math.Max(value, min)) / (middleValue - min);
                r = Convert.ToByte(255 * val);
            }
            return Color.FromArgb(255, r, g, b);
        }

        private void dgv_quantification_results_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow dataRow in dgv_quantification_results.Rows)
            {
                foreach (DataGridViewCell oneCell in dataRow.Cells)
                {
                    double d = 0;
                    if (oneCell.Value != null)
                    {
                        Double.TryParse(oneCell.Value.ToString(), out d);
                    }
                    if (d == 0)
                    {
                        dgv_quantification_results.Rows[oneCell.RowIndex].Cells[oneCell.ColumnIndex].Style.BackColor = Color.White;
                    }
                    else
                    {
                        dgv_quantification_results.Rows[oneCell.RowIndex].Cells[oneCell.ColumnIndex].Style.BackColor = HeatMapColor(d, -1, 1);
                    }                  
                }
            }
        }

        private void ckbx_columnNormalize_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void ckbx_compressTechReps_CheckedChanged(object sender, EventArgs e)
        {
            if (!ckbx_compressTechReps.Checked)
            {
                ckbx_compressBioReps.CheckedChanged -= new EventHandler(ckbx_compressBioReps_CheckedChanged);
                ckbx_compressFractions.CheckedChanged -= new EventHandler(ckbx_compressFractions_CheckedChanged);
                ckbx_compressBioReps.Checked = false;
                ckbx_compressFractions.Checked = false;
                ckbx_compressBioReps.CheckedChanged -= new EventHandler(ckbx_compressBioReps_CheckedChanged);
                ckbx_compressFractions.CheckedChanged -= new EventHandler(ckbx_compressFractions_CheckedChanged);
                runTheGamut();
            }
            else
                runTheGamut();
        }

        private void ckbx_compressFractions_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbx_compressFractions.Checked)
            {
                ckbx_compressTechReps.CheckedChanged -= new EventHandler(ckbx_compressTechReps_CheckedChanged);
                ckbx_compressTechReps.Checked = true;
                ckbx_compressTechReps.CheckedChanged += new EventHandler(ckbx_compressTechReps_CheckedChanged);
                runTheGamut();
            }
            else
            {
                ckbx_compressBioReps.CheckedChanged -= new EventHandler(ckbx_compressBioReps_CheckedChanged);
                ckbx_compressTechReps.Checked = false;
                ckbx_compressBioReps.CheckedChanged += new EventHandler(ckbx_compressBioReps_CheckedChanged);
                runTheGamut();
            }
        }
        private void ckbx_compressBioReps_CheckedChanged(object sender, EventArgs e)
        {
            if (ckbx_compressBioReps.Checked)
            {
                ckbx_compressFractions.CheckedChanged -= new EventHandler(ckbx_compressFractions_CheckedChanged);
                ckbx_compressTechReps.CheckedChanged -= new EventHandler(ckbx_compressTechReps_CheckedChanged);
                ckbx_compressTechReps.Checked = true;
                ckbx_compressFractions.Checked = true;
                ckbx_compressFractions.CheckedChanged += new EventHandler(ckbx_compressFractions_CheckedChanged);
                ckbx_compressTechReps.CheckedChanged += new EventHandler(ckbx_compressTechReps_CheckedChanged);
                runTheGamut();
            }
            else
                runTheGamut();
        }
    }
}
