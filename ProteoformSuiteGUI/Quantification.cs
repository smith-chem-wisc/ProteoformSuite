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
using System.Windows.Forms.DataVisualization.Charting;

namespace ProteoformSuite
{
    public partial class Quantification : Form
    {
        DataSet quantTables = new DataSet();
        Dictionary<goTerm, int> goMasterSet = new Dictionary<goTerm, int>();
        List<Protein> interestingProteins = new List<Protein>();
        List<goTermNumber> goTermNumbers = new List<goTermNumber>();

        public Quantification()
        {
            InitializeComponent();
        }

        private void Quantification_Load(object sender, EventArgs e)
        {
            initializeForm();

            computeInputFileIntensities();
            createQuantificationTables();
            runTheGamut();
        }

        private void initializeForm()
        {
            rbtn_neucodeRatio.CheckedChanged += new EventHandler(datatableSelection_CheckedChange);
            rbtn_totalIntensity.CheckedChanged += new EventHandler(datatableSelection_CheckedChange);
            rbtn_variance.CheckedChanged += new EventHandler(datatableSelection_CheckedChange);
            rbtn_pValue.CheckedChanged += new EventHandler(datatableSelection_CheckedChange);
          
            nud_pValue.Value = 0.1m;
            nud_ratio.Value = 1.0m;
            nud_intensity.Value = 10000;

            nud_pValue.ValueChanged += new EventHandler(updateGoTermsTable);
            nud_ratio.ValueChanged += new EventHandler(updateGoTermsTable);
            nud_intensity.ValueChanged += new EventHandler(updateGoTermsTable);

            cmbx_goAspect.Items.Add(aspect.biologicalProcess);
            cmbx_goAspect.Items.Add(aspect.cellularComponent);
            cmbx_goAspect.Items.Add(aspect.molecularFunction);
            cmbx_goAspect.SelectedIndex = 0 ;

            goMasterSet = getDatabaseGoNumbers(Lollipop.proteins);
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
            foreach (DataTable dt in quantTables.Tables)
                dt.Reset();
            addColumnsToDataSet();
            addRowsToDataSet();
            rbtn_neucodeRatio.Checked  = true;
            dgv_quantification_results.DataSource = quantTables.Tables["ratio"];
            interestingProteins = getInterestingProteins();
            goTermNumbers = getGoTermNumbers(interestingProteins);
            fillGoTermsTable();
        }

        private  void fillGoTermsTable()
        {
            DisplayUtility.FillDataGridView(dgv_goAnalysis, goTermNumbers.Where(x=>x.goTerm.aspect.ToString() == cmbx_goAspect.SelectedItem.ToString()));
        }

        private void updateGoTermsTable(object s, EventArgs e)
        {
            if (!IsEmpty(quantTables))
            {
                interestingProteins = getInterestingProteins();
                goTermNumbers = getGoTermNumbers(interestingProteins);
                fillGoTermsTable();
            }        
        }

        private bool IsEmpty(DataSet dataSet)
        {
            foreach (DataTable table in dataSet.Tables)
                if (table.Rows.Count != 0) return false;

            return true;
        }

        private void createQuantificationTables()
        {
            DataTable dt_ratio = new DataTable();
            dt_ratio.TableName = "ratio";
            DataTable dt_intensity = new DataTable();
            dt_intensity.TableName = "intensity";
            DataTable dt_variance = new DataTable();
            dt_variance.TableName = "variance";
            DataTable dt_pValue = new DataTable();
            dt_pValue.TableName = "pValue";

            quantTables.Tables.Add(dt_ratio);
            quantTables.Tables.Add(dt_intensity);
            quantTables.Tables.Add(dt_variance);
            quantTables.Tables.Add(dt_pValue);

        }

        private void addColumnsToDataSet()
        {
            foreach (DataTable dt in quantTables.Tables)
            {
                dt.Columns.Add("Experimental_Proteoform", typeof(String));
                dt.Columns.Add("Theoretical_Proteoforms", typeof(String));

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

                cmbx_quantColumns.Items.Clear();

                foreach (string lbl in columnLabels.Distinct())
                {
                    dt.Columns.Add(lbl, typeof(Double));
                    cmbx_quantColumns.Items.Add(lbl.ToString());
                }

                cmbx_quantColumns.SelectedIndex = 0;
            }
        }

        private void addRowsToDataSet()
        {
            string[] columnNames = quantTables.Tables["ratio"].Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            List<InputFile>[] inputFileLists = new List<InputFile>[columnNames.Count()];

            if (Lollipop.neucode_labeled)
            {
                inputFileLists[0] = null;
                inputFileLists[1] = null;
                for (int i = 2; i < columnNames.Count(); i++)
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
                inputFileLists[1] = null;
                for (int i = 2; i < columnNames.Count(); i++)
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

            foreach (ExperimentalProteoform eP in Lollipop.proteoform_community.experimental_proteoforms) // we may want to limit this to select proteoforms
            {
                object[] ratio = new object[columnNames.Count()];
                object[] intensity = new object[columnNames.Count()];
                object[] variance = new object[columnNames.Count()];
                object[] pValue = new object[columnNames.Count()];

                ratio[0] = eP.accession;
                intensity[0] = eP.accession;
                variance[0] = eP.accession;
                pValue[0] = eP.accession;

                ratio[1] = string.Join("; ", eP.family.theoretical_proteoforms.Select(t=> t.accession).ToList());
                intensity[1] = string.Join("; ", eP.family.theoretical_proteoforms.Select(t=> t.accession).ToList());
                variance[1] = string.Join("; ", eP.family.theoretical_proteoforms.Select(t=> t.accession).ToList());
                pValue[1] = string.Join("; ", eP.family.theoretical_proteoforms.Select(t=> t.accession).ToList());

                for (int i = 2; i < inputFileLists.Count(); i++)
                {
                        ratio[i] = eP.weightedRatioAndWeightedVariance(inputFileLists[i].DistinctBy(x=>x.UniqueId).ToList()).ratio;
                        intensity[i] = eP.weightedRatioAndWeightedVariance(inputFileLists[i].DistinctBy(x => x.UniqueId).ToList()).intensity;
                        variance[i] = eP.weightedRatioAndWeightedVariance(inputFileLists[i].DistinctBy(x => x.UniqueId).ToList()).variance;
                        pValue[i] = eP.weightedRatioAndWeightedVariance(inputFileLists[i].DistinctBy(x => x.UniqueId).ToList()).pValue;
                }

                quantTables.Tables["ratio"].Rows.Add(ratio);
                quantTables.Tables["intensity"].Rows.Add(intensity);
                quantTables.Tables["variance"].Rows.Add(variance);
                quantTables.Tables["pValue"].Rows.Add(pValue);
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

        private void datatableSelection_CheckedChange(object sender, EventArgs e)
        {
            RadioButton rb = new RadioButton();

            if (((RadioButton)sender).Checked)
            {
                // This is the correct control.
                rb = (RadioButton)sender;
            }

            switch (rb.Name)
            {
                case "rbtn_neucodeRatio":
                    dgv_quantification_results.DataSource = quantTables.Tables["ratio"];
                    break;
                case "rbtn_totalIntensity":
                    dgv_quantification_results.DataSource = quantTables.Tables["intensity"];
                    break;
                case "rbtn_variance":
                    dgv_quantification_results.DataSource = quantTables.Tables["variance"];
                    break;
                case "rbtn_pValue":
                    dgv_quantification_results.DataSource = quantTables.Tables["pValue"];
                    break;
            }

        }

        private void cmbx_quantColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            int quantTableLength = quantTables.Tables["ratio"].Rows.Count;
            ct_volcano_logFold_logP.Series.Clear();
            ct_volcano_logFold_logP.Series.Add("Series1");
            ct_volcano_logFold_logP.Series["Series1"].ChartType = SeriesChartType.Point;
            ct_volcano_logFold_logP.ChartAreas[0].AxisX.Title = "log Ratio (base 2)";
            ct_volcano_logFold_logP.ChartAreas[0].AxisY.Title = "-log p-Value (base 10)";

            foreach (DataRow drow in quantTables.Tables["ratio"].Rows)
            {
                string experimentalProteoform = drow["Experimental_Proteoform"].ToString();
                double xValue = Convert.ToDouble(drow[cmbx_quantColumns.SelectedItem.ToString()]);
                string searchExpression = "Experimental_Proteoform = '" + experimentalProteoform +"'";
                DataRow[] foundRows = quantTables.Tables["pValue"].Select(searchExpression);
                string foundValue = foundRows[0][cmbx_quantColumns.SelectedItem.ToString()].ToString();
                double yValue = Convert.ToDouble(foundValue);
                if (yValue > 0)
                    yValue = - Math.Log10(yValue);
                ct_volcano_logFold_logP.Series["Series1"].Points.AddXY(xValue, yValue);
            }
        }

        private void cmbx_goAspect_SelectedIndexChanged(object sender, EventArgs e)
        {
            fillGoTermsTable();
        }

        private List<Protein> getInterestingProteins()
        {
            List<string> rAccessions = new List<string>();
            List<string> pAccessions = new List<string>();
            List<string> iAccessions = new List<string>();
            List<string> allAccessions = new List<string>();
            List<Protein> interestingProteins = new List<Protein>();

            string expression = "[" + cmbx_quantColumns.SelectedItem.ToString() + "]";
            string ratioExpression = expression + ">=" + nud_ratio.Value.ToString() + " OR " + expression + "<= -" + nud_ratio.Value.ToString();
            string pValueExpression = expression + "<=" + nud_pValue.Value.ToString();
            string intensityExpression = expression + ">=" + nud_intensity.Value.ToString();

            DataRow[] rRows = quantTables.Tables["ratio"].Select(ratioExpression);
            DataRow[] pRows = quantTables.Tables["pValue"].Select(pValueExpression);
            DataRow[] iRows = quantTables.Tables["intensity"].Select(intensityExpression);

            foreach (DataRow rRow in rRows)
            {
                string value = rRow["Experimental_Proteoform"].ToString();
                if (value.Length > 0)
                {
                    List<string> accessions =  Array.ConvertAll(value.Split(';'), p => p.Trim()).ToList();
                    foreach (string a in accessions)
                    {
                        if(a.Length > 0)
                            rAccessions.Add(a);
                    }
                }
            }
            allAccessions.AddRange(rAccessions.Distinct());
            foreach (DataRow pRow in pRows)
            {
                string value = pRow["Experimental_Proteoform"].ToString();
                if (value.Length > 0)
                {
                    List<string> accessions = Array.ConvertAll(value.Split(';'), p => p.Trim()).ToList();
                    foreach (string a in accessions)
                    {
                        if (a.Length > 0)
                            pAccessions.Add(a);
                    }
                }
            }
            allAccessions.AddRange(pAccessions.Distinct());
            foreach (DataRow iRow in iRows)
            {
                string value = iRow["Experimental_Proteoform"].ToString();
                if (value.Length > 0)
                {
                    List<string> accessions = Array.ConvertAll(value.Split(';'), p => p.Trim()).ToList();
                    foreach (string a in accessions)
                    {
                        if (a.Length > 0)
                            iAccessions.Add(a);
                    }
                }
            }
            allAccessions.AddRange(iAccessions.Distinct());
            List<string> distinctAccessions = allAccessions.Distinct().ToList();

            foreach (string acc in distinctAccessions)
            {
                if (allAccessions.Where(a => a == acc).Count() == 3)
                {
                    List<ProteoformFamily> famlist = (from f in Lollipop.proteoform_community.families
                                from e in f.experimental_proteoforms
                                where e.accession == acc
                                select f).ToList();

                    List<string> tlist = famlist.SelectMany(t => t.theoretical_proteoforms.Select(a => a.accession)).ToList();

                    foreach (string accession in tlist)
                    {
                        interestingProteins.AddRange(Lollipop.proteins.Where(protein => protein.accession == accession).ToList());
                    }
                }
            
            }
            return interestingProteins.DistinctBy(p => p.accession).ToList();
        }

        private Dictionary<goTerm, int> getDatabaseGoNumbers(Protein[] proteinList)
        {
            Dictionary<goTerm, int> numbers = new Dictionary<goTerm, int>();

            List<goTerm> completeGoTermList = new List<goTerm>();
            List<goTerm> uniqueGoTermList = new List<goTerm>();
            foreach (Protein p in proteinList)
            {
                completeGoTermList.AddRange(p.goTerms);
            }

            foreach (goTerm t in completeGoTermList)
            {
                if (!uniqueGoTermList.Any(item => item.id == t.id))
                    uniqueGoTermList.Add(t);
            }

            foreach (goTerm term in uniqueGoTermList)
            {
                numbers.Add(term, completeGoTermList.Where(t => t.id == term.id).Count());
            }

            return numbers;
        }

        private List<goTermNumber> getGoTermNumbers(List<Protein> interestingProteins)
        {
            List<goTermNumber> numbers = new List<goTermNumber>();
            List<goTerm> terms = new List<goTerm>();
            foreach (Protein  p in interestingProteins)
            {
                foreach (goTerm g in p.goTerms)
                {
                    if (!terms.Any(item => item.id == g.id))
                        terms.Add(g);
                }
            }
            foreach (goTerm g in terms)
            {
                goTermNumber gTN = new goTermNumber(g, interestingProteins, goMasterSet);
                numbers.Add(gTN);
            }
            return numbers;
        }
    }
}
