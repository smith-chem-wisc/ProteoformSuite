using ProteoformSuiteInternal;
using System;
using System.Collections.Concurrent;
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
        //DataSet quantTables = new DataSet();
        Dictionary<GoTerm, int> goMasterSet = new Dictionary<GoTerm, int>();
        List<Protein> interestingProteins = new List<Protein>();
        List<GoTermNumber> goTermNumbers = new List<GoTermNumber>();
        List<ExperimentalProteoform.quantitativeValues> qVals = new List<ExperimentalProteoform.quantitativeValues>();

        //Variables Describing the Distribution of Observed and Imputed Proteoform Intensities
        decimal observedAverageIntensity; //log base 2
        decimal selectAverageIntensity; //log base 2
        decimal observedStDev;
        decimal selectStDev;
        decimal observedGaussianArea;
        decimal selectGaussianArea;
        decimal observedGaussianHeight;
        decimal selectGaussianHeight;
        int numSelectMissingIntensities;
        int numSelectProteoformIntensities;
        SortedDictionary<decimal, int> logIntensityHistogram = new SortedDictionary<decimal, int>();
        SortedDictionary<decimal, int> logSelectIntensityHistogram = new SortedDictionary<decimal, int>();

        decimal bkgdAverageIntensity; //log base 2
        decimal bkgdSelectAverageIntensity; //log base 2
        decimal bkgdStDev;
        decimal bkgdSelectStDev;
        //decimal bkgdGaussianArea;
        decimal bkgdSelectGaussianArea;
        //decimal bkgdGaussianHeight;
        decimal bkgdSelectGaussianHeight;
        int numMeasuredProteoformIntensities;
        //End -- Variables Describing the Distribution of Observed and Imputed Proteoform Intensities

        int satisfactoryProteoformsCount;
        List<ExperimentalProteoform> satisfactoryProteoforms = new List<ExperimentalProteoform>(); // these are proteoforms meeting the required number of observations.

        public Quantification()
        {
            InitializeComponent();
        }

        private void Quantification_Load(object sender, EventArgs e)
        {
            initializeForm();
            runTheGamut();
        }

        public DataGridView Get_quant_results_DGV()
        {
            return dgv_quantification_results;
        }

        public DataGridView Get_GoTerms_DGV()
        {
            return dgv_goAnalysis;
        }

        private void initializeForm()
        {
            List<string> conditions = Lollipop.ltConditionsBioReps.Keys.ToList();
            conditions.AddRange(Lollipop.hvConditionsBioReps.Keys.ToList());
            conditions = conditions.Distinct().ToList();
            cmbx_ratioNumerator.Items.AddRange(conditions.ToArray());
            cmbx_ratioDenominator.Items.AddRange(conditions.ToArray());
            cmbx_ratioNumerator.SelectedIndex = 0;
            if (conditions.Count() > 1)
                cmbx_ratioDenominator.SelectedIndex = 1;
            else
                cmbx_ratioDenominator.SelectedIndex = 0;

            nud_bkgdShift.Value = (decimal)-2.0;
            nud_bkgdWidth.Value = (decimal) 0.5;

            nud_bkgdShift.ValueChanged += new EventHandler(plotBiorepIntensitiesEvent);
            nud_bkgdWidth.ValueChanged += new EventHandler(plotBiorepIntensitiesEvent);

            nud_pValue.Value = 0.01m;
            nud_ratio.Value = 3.0m;
            nud_intensity.Value = 500000;

            nud_pValue.ValueChanged += new EventHandler(updateGoTermsTable);
            nud_ratio.ValueChanged += new EventHandler(updateGoTermsTable);
            nud_intensity.ValueChanged += new EventHandler(updateGoTermsTable);

            //Lollipop.getObservationParameters(); //examines the conditions and bioreps to determine the maximum number of observations to require for quantification
            nud_minObservations.Minimum = 1;
            nud_minObservations.Maximum = Lollipop.countOfBioRepsInOneCondition;
            nud_minObservations.Value = Lollipop.countOfBioRepsInOneCondition;

            cmbx_observationsTypeRequired.Items.Add("Minimum Total from A Single Condition");
            cmbx_observationsTypeRequired.Items.Add("Minimum Total from Any Condition");
            cmbx_observationsTypeRequired.SelectedIndex = 0;

            cmbx_goAspect.Items.Add(aspect.biologicalProcess);
            cmbx_goAspect.Items.Add(aspect.cellularComponent);
            cmbx_goAspect.Items.Add(aspect.molecularFunction);

            cmbx_goAspect.SelectedIndexChanged -= cmbx_goAspect_SelectedIndexChanged; //disable event on load to prevent premature firing
            cmbx_goAspect.SelectedIndex = 0;
            cmbx_goAspect.SelectedIndexChanged += cmbx_goAspect_SelectedIndexChanged;

            goMasterSet = getDatabaseGoNumbers(Lollipop.proteins);
        }

        private void runTheGamut()
        {
            computeBiorepIntensities();
            defineAllObservedIntensityDistribution();
            determineProteoformsMeetingCriteria();
            defineSelectObservedIntensityDistribution();
            defineSelectBackgroundIntensityDistribution();
            plotBiorepIntensities();
            proteoformQuantification();
            DisplayUtility.FillDataGridView(dgv_quantification_results, qVals);
            volcanoPlot();
            interestingProteins = getInterestingProteins();
            goTermNumbers = getGoTermNumbers(interestingProteins);
            fillGoTermsTable();
        }

        private void fillGoTermsTable()
        {
            DisplayUtility.FillDataGridView(dgv_goAnalysis, goTermNumbers.Where(x => x.goTerm.aspect.ToString() == cmbx_goAspect.SelectedItem.ToString()));
        }

        private void updateGoTermsTable(object s, EventArgs e)
        {
            //if (!IsEmpty(quantTables))
            //{
            //    interestingProteins = getInterestingProteins();
            //    goTermNumbers = getGoTermNumbers(interestingProteins);
            //    fillGoTermsTable();
            //}
        }

        private bool IsEmpty(DataSet dataSet)
        {
            foreach (DataTable table in dataSet.Tables)
                if (table.Rows.Count != 0) return false;

            return true;
        }

        private void createQuantificationTables()
        {
            //DataTable dt_ratio = new DataTable();
            //dt_ratio.TableName = "ratio";
            //DataTable dt_intensity = new DataTable();
            //dt_intensity.TableName = "intensity";
            //DataTable dt_variance = new DataTable();
            //dt_variance.TableName = "variance";
            //DataTable dt_pValue = new DataTable();
            //dt_pValue.TableName = "pValue";

            //quantTables.Tables.Add(dt_ratio);
            //quantTables.Tables.Add(dt_intensity);
            //quantTables.Tables.Add(dt_variance);
            //quantTables.Tables.Add(dt_pValue);

        }

        private void addColumnsToDataSet()
        {
            //foreach (DataTable dt in quantTables.Tables)
            //{
            //    dt.Columns.Add("Experimental_Proteoform", typeof(String));
            //    dt.Columns.Add("Theoretical_Proteoforms", typeof(String));

            //    List<string> columnLabels = new List<string>();                    
            //    if (Lollipop.neucode_labeled)
            //    {
            //        foreach (InputFile inputFile in Lollipop.input_files.Where(s => s.purpose == Purpose.Quantification))
            //        {
            //            columnLabels.Add("Ratio: " + inputFile.lt_condition + "/" + inputFile.hv_condition + "; BR-" + inputFile.biological_replicate + "; FR-" + inputFile.fraction + "; TR-" + inputFile.technical_replicate);
            //        }
            //    }
            //    else
            //    {
            //        foreach (InputFile inputFile in Lollipop.input_files)
            //        {
            //            columnLabels.Add("Condition: " + inputFile.lt_condition + "; BR-" + inputFile.biological_replicate + "; FR-" + inputFile.fraction + "; TR-" + inputFile.technical_replicate);
            //        }
            //    }
            //    cmbx_quantColumns.Items.Clear();

            //    foreach (string lbl in columnLabels.Distinct())
            //    {
            //        dt.Columns.Add(lbl, typeof(Double));
            //        cmbx_quantColumns.Items.Add(lbl.ToString());
            //    }

            //    cmbx_quantColumns.SelectedIndex = 0;
            //}
        }

        private void addRowsToDataSet()
        {
            //string[] columnNames = quantTables.Tables["ratio"].Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            //List<InputFile>[] inputFileLists = new List<InputFile>[columnNames.Count()];

            //if (Lollipop.neucode_labeled)
            //{
            //    inputFileLists[0] = null;
            //    inputFileLists[1] = null;
            //    for (int i = 2; i < columnNames.Count(); i++)
            //    {
            //        string[] columnNamePieces = columnNames[i].Split('-').ToArray();
            //        switch (columnNamePieces.Count())
            //        {
            //            case 1:
            //                inputFileLists[i] = (from s in Lollipop.input_files where s.purpose == Purpose.Quantification select s).ToList();
            //                break;
            //            case 2:
            //                inputFileLists[i] = (from s in Lollipop.input_files where s.purpose == Purpose.Quantification where s.biological_replicate == Convert.ToInt32(columnNamePieces[1]) select s).ToList();
            //                break;
            //            case 3:
            //                inputFileLists[i] = (from s in Lollipop.input_files where s.purpose == Purpose.Quantification where s.biological_replicate == Convert.ToInt32(columnNamePieces[1].Split(';').ToList().First()) where s.fraction == Convert.ToInt32(columnNamePieces[2].Split(';').ToList().First()) select s).ToList();
            //                break;
            //            case 4:
            //                inputFileLists[i] = (from s in Lollipop.input_files where s.purpose == Purpose.Quantification where s.biological_replicate == Convert.ToInt32(columnNamePieces[1].Split(';').ToList().First()) where s.fraction == Convert.ToInt32(columnNamePieces[2].Split(';').ToList().First()) where s.technical_replicate == Convert.ToInt32(columnNamePieces[3]) select s).ToList();
            //                break;
            //        }

            //    }
            //}
            //else
            //{
            //    inputFileLists[0] = null;
            //    inputFileLists[1] = null;
            //    for (int i = 2; i < columnNames.Count(); i++)
            //    {
            //        string[] columnNamePieces = columnNames[i].Split('-').ToArray();
            //        switch (columnNamePieces.Count())
            //        {
            //            case 1:
            //                inputFileLists[i] = (from s in Lollipop.input_files select s).ToList();
            //                break;
            //            case 2:
            //                inputFileLists[i] = (from s in Lollipop.input_files where s.biological_replicate == Convert.ToInt32(columnNamePieces[1]) select s).ToList();
            //                break;
            //            case 3:
            //                inputFileLists[i] = (from s in Lollipop.input_files where s.biological_replicate == Convert.ToInt32(columnNamePieces[1].Split(';').ToList().First()) where s.fraction == Convert.ToInt32(columnNamePieces[2].Split(';').ToList().First()) select s).ToList();
            //                break;
            //            case 4:
            //                inputFileLists[i] = (from s in Lollipop.input_files where s.biological_replicate == Convert.ToInt32(columnNamePieces[1].Split(';').ToList().First()) where s.fraction == Convert.ToInt32(columnNamePieces[2].Split(';').ToList().First()) where s.technical_replicate == Convert.ToInt32(columnNamePieces[3]) select s).ToList();
            //                break;
            //        }
            //    }
            //}

            //foreach (ExperimentalProteoform eP in Lollipop.proteoform_community.experimental_proteoforms.Where(eP=>eP.accepted == true).ToList()) // we may want to limit this to select proteoforms
            //{
            //    object[] ratio = new object[columnNames.Count()];
            //    object[] intensity = new object[columnNames.Count()];
            //    object[] variance = new object[columnNames.Count()];
            //    object[] pValue = new object[columnNames.Count()];

            //    ratio[0] = eP.accession;
            //    intensity[0] = eP.accession;
            //    variance[0] = eP.accession;
            //    pValue[0] = eP.accession;

            //    ratio[1] = string.Join("; ", eP.family.theoretical_proteoforms.Select(t => t.accession).ToList());
            //    intensity[1] = string.Join("; ", eP.family.theoretical_proteoforms.Select(t => t.accession).ToList());
            //    variance[1] = string.Join("; ", eP.family.theoretical_proteoforms.Select(t => t.accession).ToList());
            //    pValue[1] = string.Join("; ", eP.family.theoretical_proteoforms.Select(t => t.accession).ToList());

            //    for (int i = 2; i < inputFileLists.Count(); i++)
            //    {
            //        ratio[i] = eP.weightedRatioAndWeightedVariance(inputFileLists[i].DistinctBy(x => x.UniqueId).ToList()).ratio;
            //        intensity[i] = eP.weightedRatioAndWeightedVariance(inputFileLists[i].DistinctBy(x => x.UniqueId).ToList()).intensity;
            //        variance[i] = eP.weightedRatioAndWeightedVariance(inputFileLists[i].DistinctBy(x => x.UniqueId).ToList()).variance;
            //        pValue[i] = eP.weightedRatioAndWeightedVariance(inputFileLists[i].DistinctBy(x => x.UniqueId).ToList()).pValue;
            //    }

            //    quantTables.Tables["ratio"].Rows.Add(ratio);
            //    quantTables.Tables["intensity"].Rows.Add(intensity);
            //    quantTables.Tables["variance"].Rows.Add(variance);
            //    quantTables.Tables["pValue"].Rows.Add(pValue);
            //}
        }

        private void computeBiorepIntensities()
        {
            if (Lollipop.proteoform_community.experimental_proteoforms.Count() > 0)
            {
                Parallel.ForEach(Lollipop.proteoform_community.experimental_proteoforms, eP =>
                {
                    eP.make_biorepIntensityList();
                });
            }
        }

        private void defineAllObservedIntensityDistribution() // the distribution of all observed experimental proteoform biorep intensities
        {          
            List<decimal> allIntensities = new List<decimal>();
            numMeasuredProteoformIntensities = 0;
            foreach (List<biorepIntensity> biorepIntensityList in Lollipop.proteoform_community.experimental_proteoforms.Select(b => b.biorepIntensityList))
            {
                foreach (double intensity in biorepIntensityList.Select(i => i.intensity))
                {
                    numMeasuredProteoformIntensities++;
                    decimal roundedIntensity = Math.Round((decimal)Math.Log(intensity, 2), 1);
                    allIntensities.Add(roundedIntensity);
                    if (logIntensityHistogram.ContainsKey(roundedIntensity))
                        logIntensityHistogram[roundedIntensity]++;
                    else
                        logIntensityHistogram.Add(roundedIntensity, 1);
                }
            }

            observedAverageIntensity = allIntensities.Where(i => i > 1).ToList().Average(); //these are log2 values
            observedStDev = (decimal)Math.Sqrt(allIntensities.Average(v => Math.Pow((double)v - (double)(observedAverageIntensity), 2)));
            observedGaussianArea = 0;
            bool first = true;
            decimal x1 = 0;
            decimal y1 = 0;
            foreach (KeyValuePair<decimal, int> entry in logIntensityHistogram)
            {
                if (first)
                {
                    x1 = entry.Key;
                    y1 = (decimal)entry.Value;
                    first = false;
                }
                else
                {
                    observedGaussianArea += (entry.Key - x1) * (y1 + ((decimal)entry.Value - y1) / 2);
                    x1 = entry.Key;
                    y1 = (decimal)entry.Value;
                }
            }
            observedGaussianHeight = observedGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)observedStDev, 2));

            bkgdAverageIntensity = observedAverageIntensity + nud_bkgdShift.Value * observedStDev;
            bkgdStDev = observedStDev * nud_bkgdWidth.Value;
        }

        private void defineSelectObservedIntensityDistribution()
        {
            List<decimal> allIntensities = new List<decimal>();
            numSelectProteoformIntensities = 0;
            foreach (List<biorepIntensity> biorepIntensityList in satisfactoryProteoforms.Select(b => b.biorepIntensityList))
            {
                foreach (double intensity in biorepIntensityList.Select(i => i.intensity))
                {
                    numSelectProteoformIntensities++;
                    decimal roundedIntensity = Math.Round((decimal)Math.Log(intensity, 2), 1);
                    allIntensities.Add(roundedIntensity);
                    if (logSelectIntensityHistogram.ContainsKey(roundedIntensity))
                        logSelectIntensityHistogram[roundedIntensity]++;
                    else
                        logSelectIntensityHistogram.Add(roundedIntensity, 1);
                }
            }

            selectAverageIntensity = allIntensities.Where(i => i > 1).ToList().Average(); //these are log2 values
            selectStDev = (decimal)Math.Sqrt(allIntensities.Average(v => Math.Pow((double)v - (double)(selectAverageIntensity), 2)));
            selectGaussianArea = 0;
            bool first = true;
            decimal x1 = 0;
            decimal y1 = 0;
            foreach (KeyValuePair<decimal, int> entry in logSelectIntensityHistogram)
            {
                if (first)
                {
                    x1 = entry.Key;
                    y1 = (decimal)entry.Value;
                    first = false;
                }
                else
                {
                    selectGaussianArea += (entry.Key - x1) * (y1 + ((decimal)entry.Value - y1) / 2);
                    x1 = entry.Key;
                    y1 = (decimal)entry.Value;
                }
            }
            selectGaussianHeight = selectGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)observedStDev, 2));
        }

        //private void defineAllBackgroundIntensityDistribution()
        //{
        //    bkgdAverageIntensity = observedAverageIntensity + nud_bkgdShift.Value * observedStDev;
        //    bkgdStDev = observedStDev * nud_bkgdWidth.Value;
        //    int numMeasurableIntensities = Lollipop.quantBioFracCombos.Keys.Count() * Lollipop.proteoform_community.experimental_proteoforms.Count();
        //    if (Lollipop.neucode_labeled)
        //        numMeasurableIntensities = numMeasurableIntensities * 2;
        //    numMissingIntensities = numMeasurableIntensities - numMeasuredProteoformIntensities;
        //    bkgdGaussianArea = observedGaussianArea / numMeasuredProteoformIntensities * numMissingIntensities;
        //    bkgdGaussianHeight = bkgdGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)bkgdStDev, 2));
        //}

        private void determineProteoformsMeetingCriteria()
        {
            List<string> conditions = Lollipop.ltConditionsBioReps.Keys.ToList();
            satisfactoryProteoformsCount = 0;
            satisfactoryProteoforms.Clear();
            ConcurrentBag<ExperimentalProteoform> sP = new ConcurrentBag<ExperimentalProteoform>();
            conditions.AddRange(Lollipop.hvConditionsBioReps.Keys.ToList());
            conditions = conditions.Distinct().ToList();

            object sync = new object();

            if (cmbx_observationsTypeRequired.SelectedIndex == 0)//single condition
                Parallel.ForEach(Lollipop.proteoform_community.experimental_proteoforms, eP => 
                {
                    foreach (string c in conditions)
                    {
                        if (eP.biorepIntensityList.Where(bc => bc.condition == c).Select(b=>b.biorep).ToList().Count() == nud_minObservations.Value)
                        {
                            lock (sync)
                            {
                                satisfactoryProteoformsCount++;
                                sP.Add(eP);
                            }                           
                            break;
                        }                           
                    }
                });
            else //any condition
                Parallel.ForEach(Lollipop.proteoform_community.experimental_proteoforms, eP =>
                {
                    if (eP.biorepIntensityList.Select(c => c.condition).Distinct().ToList().Count() == nud_minObservations.Value)
                    {
                        lock (sync)
                        {
                            satisfactoryProteoformsCount++;
                            sP.Add(eP);
                        }
                    }
                });
            satisfactoryProteoforms = sP.ToList();
        }

        private void defineSelectBackgroundIntensityDistribution()
        {
            bkgdSelectAverageIntensity = observedAverageIntensity + nud_bkgdShift.Value * observedStDev;
            bkgdSelectStDev = observedStDev * nud_bkgdWidth.Value;
            int numSelectMeasurableIntensities = Lollipop.quantBioFracCombos.Keys.Count() * satisfactoryProteoforms.Count();
            if (Lollipop.neucode_labeled)
                numSelectMeasurableIntensities = numSelectMeasurableIntensities * 2;
            int numSelectMeasuredIntensities = 0;
            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                numSelectMeasuredIntensities += eP.biorepIntensityList.Select(s => s.biorep).ToList().Count();
            }
            numSelectMissingIntensities = numSelectMeasurableIntensities - numSelectMeasuredIntensities;
            bkgdSelectGaussianArea = selectGaussianArea / numSelectMeasuredIntensities * numSelectMissingIntensities;
            bkgdSelectGaussianHeight = bkgdSelectGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)bkgdSelectStDev, 2));
        }

        private void plotBiorepIntensitiesEvent(object s, EventArgs e)
        {
            //defineAllBackgroundIntensityDistribution();
            defineSelectBackgroundIntensityDistribution();
            plotBiorepIntensities();
        }

        private void plotBiorepIntensities()
        {
            ct_proteoformIntensities.Series.Clear();
            ct_proteoformIntensities.Series.Add("Observed Intensities");
            ct_proteoformIntensities.Series["Observed Intensities"].ChartType = SeriesChartType.Point; // these are the actual experimental proteoform intensities
            ct_proteoformIntensities.Series.Add("Observed Fit");
            ct_proteoformIntensities.Series["Observed Fit"].ChartType = SeriesChartType.Line; // this is a gaussian best fit to the experimental proteoform intensities.
            ct_proteoformIntensities.Series.Add("Background Projected");
            ct_proteoformIntensities.Series["Background Projected"].ChartType = SeriesChartType.Line; // this is a gaussian line representing the distribution of missing values.
            ct_proteoformIntensities.Series.Add("Fit + Projected");
            ct_proteoformIntensities.Series["Fit + Projected"].ChartType = SeriesChartType.Line; // this is the sum of the gaussians for observed and missing values
            ct_proteoformIntensities.ChartAreas[0].AxisX.Title = "log Intensity (base 2)";
            ct_proteoformIntensities.ChartAreas[0].AxisY.Title = "count";

            foreach (KeyValuePair<decimal, int> entry in logSelectIntensityHistogram)
            {
                ct_proteoformIntensities.Series["Observed Intensities"].Points.AddXY(entry.Key, entry.Value);

                double gaussIntensity = ((double)selectGaussianHeight) * Math.Exp(-Math.Pow(((double)entry.Key - (double)selectAverageIntensity), 2) / (2d * Math.Pow((double)selectStDev, 2)));
                double bkgd_gaussIntensity = ((double)bkgdSelectGaussianHeight) * Math.Exp(-Math.Pow(((double)entry.Key - (double)bkgdSelectAverageIntensity), 2) / (2d * Math.Pow((double)bkgdSelectStDev, 2)));
                double sumIntensity = gaussIntensity + bkgd_gaussIntensity;
                ct_proteoformIntensities.Series["Observed Fit"].Points.AddXY(entry.Key, gaussIntensity);
                ct_proteoformIntensities.Series["Background Projected"].Points.AddXY(entry.Key, bkgd_gaussIntensity);
                ct_proteoformIntensities.Series["Fit + Projected"].Points.AddXY(entry.Key, sumIntensity);
            }
        }

        private void proteoformQuantification()
        {
            qVals.Clear();
            object sync = new object();
            string numerator = cmbx_ratioNumerator.SelectedItem.ToString();
            string denominator = cmbx_ratioDenominator.SelectedItem.ToString();

            Parallel.ForEach(satisfactoryProteoforms.Where(eP => eP.accepted == true).ToList(), eP =>
            {
                lock (sync)
                {
                    qVals.Add(new ExperimentalProteoform.quantitativeValues(eP, bkgdAverageIntensity, bkgdStDev, numerator, denominator)); // those are log2 intensities
                }               
            }); 
        }

        private void volcanoPlot()
        {
            ct_volcano_logFold_logP.Series.Clear();
            ct_volcano_logFold_logP.Series.Add("logFold_logP");
            ct_volcano_logFold_logP.Series["logFold_logP"].ChartType = SeriesChartType.Point; // these are the actual experimental proteoform intensities

            ct_proteoformIntensities.ChartAreas[0].AxisX.Title = "log (base 2) fold change (light/heavy)";
            ct_proteoformIntensities.ChartAreas[0].AxisY.Title = "log (base 10) pValue";

            foreach (ExperimentalProteoform.quantitativeValues qValue in qVals)
            {
                ct_volcano_logFold_logP.Series["logFold_logP"].Points.AddXY(qValue.logFoldChange, -Math.Log10((double)qValue.pValue));
            }
        }

        //private Color HeatMapColor(double value, double min, double max)//(double value, double min, double max)
        //{
        //    double val;
        //    int r = 0;
        //    int g = 0;
        //    int b = 0;
        //    double middleValue = (max - min) / 2 + min;

        //    if (value > (min + (max - min) / 2)) // positive - green
        //    {
        //        val = (Math.Min(value, max) - middleValue) / (max - middleValue);
        //        g = Convert.ToByte(255 * val);
        //    }
        //    else // negative red
        //    {
        //        val = (middleValue - Math.Max(value, min)) / (middleValue - min);
        //        r = Convert.ToByte(255 * val);
        //    }
        //    return Color.FromArgb(255, r, g, b);
        //}

        //private void dgv_quantification_results_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        //{
        //    foreach (DataGridViewRow dataRow in dgv_quantification_results.Rows)
        //    {
        //        foreach (DataGridViewCell oneCell in dataRow.Cells)
        //        {
        //            double d = 0;
        //            if (oneCell.Value != null)
        //            {
        //                Double.TryParse(oneCell.Value.ToString(), out d);
        //            }
        //            if (d == 0)
        //            {
        //                dgv_quantification_results.Rows[oneCell.RowIndex].Cells[oneCell.ColumnIndex].Style.BackColor = Color.White;
        //            }
        //            else
        //            {
        //                dgv_quantification_results.Rows[oneCell.RowIndex].Cells[oneCell.ColumnIndex].Style.BackColor = HeatMapColor(d, -1, 1);
        //            }
        //        }
        //    }
        //}


        private void cmbx_goAspect_SelectedIndexChanged(object sender, EventArgs e)
        {
            fillGoTermsTable();
        }

        private List<Protein> getInterestingProteins()
        {
            List<ExperimentalProteoform.quantitativeValues> interestingQuantValues = new List<ExperimentalProteoform.quantitativeValues>();
            interestingQuantValues.AddRange(qVals.Where(q => q.intensitySum > nud_intensity.Value && Math.Abs(q.logFoldChange) > nud_ratio.Value && q.pValue < nud_pValue.Value).ToList());
            List<string> distinctAccessions = new List<string>();
            distinctAccessions.AddRange(interestingQuantValues.Select(a => a.accession).ToList());

            foreach (string acc in distinctAccessions) // here were getting the accession numbers of the proteins linked to interesting experimental proteoforms
            {

                ExperimentalProteoform interestingProteoform = Lollipop.proteoform_community.experimental_proteoforms.Where(iP => iP.accession == acc).FirstOrDefault();
                List<ProteoformFamily> famlist = new List<ProteoformFamily>();
                if (interestingProteoform != null)
                    famlist = Lollipop.proteoform_community.families.Where(fam => fam.experimental_proteoforms.Contains(interestingProteoform)).ToList(); // proteoform families containing the selected experimental proteoform
                List<TheoreticalProteoform> tpList = new List<TheoreticalProteoform>();
                if (famlist.Count() > 0)
                {
                    foreach (ProteoformFamily pf in famlist)
                    {
                        if (pf.theoretical_proteoforms.Count() > 0)
                            tpList.AddRange(famlist.SelectMany(t => t.theoretical_proteoforms));
                    }
                }
                List<string> tlist = new List<string>();
                if (tpList.Count() > 0)
                    tlist = tpList.Select(tacc => tacc.accession).ToList();
                foreach (string accession in tlist)
                {
                    string someJunk = accession.Replace("_T", "!").Split('!').FirstOrDefault();
                    Protein p = Lollipop.proteins.FirstOrDefault(protein => protein.accession == someJunk);
                    if (p != null)
                        if (!interestingProteins.Any(theoreticalProtein => theoreticalProtein.accession == p.accession))
                            interestingProteins.Add(p);
                }
                
            }
            return interestingProteins;
        }

        private Dictionary<GoTerm, int> getDatabaseGoNumbers(Protein[] proteinList)
        {
            Dictionary<GoTerm, int> numbers = new Dictionary<GoTerm, int>();

            List<GoTerm> completeGoTermList = new List<GoTerm>();
            List<GoTerm> uniqueGoTermList = new List<GoTerm>();
            foreach (Protein p in proteinList)
            {
                completeGoTermList.AddRange(p.goTerms);
            }

            foreach (GoTerm t in completeGoTermList)
            {
                if (!uniqueGoTermList.Any(item => item.id == t.id))
                    uniqueGoTermList.Add(t);
            }

            foreach (GoTerm term in uniqueGoTermList)
            {
                numbers.Add(term, completeGoTermList.Where(t => t.id == term.id).Count());
            }

            return numbers;
        }

        private List<GoTermNumber> getGoTermNumbers(List<Protein> interestingProteins)
        {
            List<GoTermNumber> numbers = new List<GoTermNumber>();
            List<GoTerm> terms = new List<GoTerm>();
            foreach (Protein p in interestingProteins)
            {
                foreach (GoTerm g in p.goTerms)
                {
                    if (!terms.Any(item => item.id == g.id))
                        terms.Add(g);
                }
            }
            foreach (GoTerm g in terms)
            {
                GoTermNumber gTN = new GoTermNumber(g, interestingProteins, goMasterSet);
                numbers.Add(gTN);
            }
            return numbers;
        }

        private void btn_refreshCalculation_Click(object sender, EventArgs e)
        {
            runTheGamut();
        }
    }
}
