using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace ProteoformSuiteGUI
{
    public partial class Quantification : Form, ISweetForm
    {

        #region Constructor

        public Quantification()
        {
            InitializeComponent();
        }

        #endregion Constructor

        #region Public Methods

        public List<DataGridView> GetDGVs()
        {
            return new List<DataGridView>() { dgv_quantification_results, dgv_goAnalysis };
        }

        public void RunTheGamut()
        {
            ClearListsTablesFigures(true);
            SaveState.lollipop.quantify();
            SaveState.lollipop.GO_analysis();
            FillTablesAndCharts();
        }

        public void ClearListsTablesFigures(bool clear_following)
        {
            SaveState.lollipop.logIntensityHistogram.Clear();
            SaveState.lollipop.logSelectIntensityHistogram.Clear();
            SaveState.lollipop.satisfactoryProteoforms.Clear();
            SaveState.lollipop.qVals.Clear();
            SaveState.lollipop.quantifiedProteins.Clear();
            SaveState.lollipop.inducedOrRepressedProteins.Clear();
            SaveState.lollipop.goTermNumbers.Clear();

            foreach (var series in ct_proteoformIntensities.Series) series.Points.Clear();
            foreach (var series in ct_relativeDifference.Series) series.Points.Clear();
            foreach (var series in ct_volcano_logFold_logP.Series) series.Points.Clear();

            dgv_goAnalysis.DataSource = null;
            dgv_quantification_results.DataSource = null;
            dgv_goAnalysis.Rows.Clear();
            dgv_quantification_results.Rows.Clear();

            if (clear_following)
            {
                for (int i = ((ProteoformSweet)MdiParent).forms.IndexOf(this) + 1; i < ((ProteoformSweet)MdiParent).forms.Count; i++)
                {
                    ISweetForm sweet = ((ProteoformSweet)MdiParent).forms[i];
                    sweet.ClearListsTablesFigures(false);
                }
            }
        }

        public bool ReadyToRunTheGamut()
        {
            return SaveState.lollipop.get_files(SaveState.lollipop.input_files, Purpose.Quantification).Count() > 0 
                && SaveState.lollipop.target_proteoform_community.families.Count > 0;
        }

        public void FillTablesAndCharts()
        {
            plotObservedVsExpectedRelativeDifference();
            DisplayUtility.FillDataGridView(dgv_quantification_results, SaveState.lollipop.qVals.Select(q => new DisplayQuantitativeValues(q)));
            DisplayQuantitativeValues.FormatGridView(dgv_quantification_results);
            tb_avgIntensity.Text = Math.Round(SaveState.lollipop.selectAverageIntensity, 1).ToString();
            tb_stdevIntensity.Text = Math.Round(SaveState.lollipop.selectStDev, 3).ToString();
            volcanoPlot();
            plotBiorepIntensities();
            updateGoTermsTable();
        }

        public void initialize_every_time()
        {
            tb_familyBuildFolder.Text = SaveState.lollipop.family_build_folder_path;
            if (cmbx_geneLabel.Items.Count > 0)
                cmbx_geneLabel.SelectedIndex = Lollipop.gene_name_labels.IndexOf(ProteoformCommunity.preferred_gene_label);
            cb_geneCentric.Checked = ProteoformCommunity.gene_centric_families;
        }

        public void InitializeParameterSet()
        {
            //Initialize conditions
            List<string> conditions = SaveState.lollipop.ltConditionsBioReps.Keys.ToList();
            conditions.AddRange(SaveState.lollipop.hvConditionsBioReps.Keys.ToList());
            conditions = conditions.Distinct().ToList();
            cmbx_ratioNumerator.Items.AddRange(conditions.ToArray());
            cmbx_ratioDenominator.Items.AddRange(conditions.ToArray());
            cmbx_ratioNumerator.SelectedIndex = 0;
            cmbx_ratioDenominator.SelectedIndex = Convert.ToInt32(conditions.Count() > 1);
            SaveState.lollipop.numerator_condition = cmbx_ratioNumerator.SelectedItem.ToString();
            SaveState.lollipop.denominator_condition = cmbx_ratioDenominator.SelectedItem.ToString();
            cmbx_edgeLabel.Items.AddRange(Lollipop.edge_labels);

            //Initialize display options
            cmbx_colorScheme.Items.AddRange(CytoscapeScript.color_scheme_names);
            cmbx_nodeLayout.Items.AddRange(Lollipop.node_positioning);
            cmbx_nodeLabelPositioning.Items.AddRange(CytoscapeScript.node_label_positions);
            cmbx_edgeLabel.Items.AddRange(Lollipop.edge_labels.ToArray());
            cmbx_nodeLabel.Items.AddRange(Lollipop.node_labels.ToArray());
            cmbx_geneLabel.Items.AddRange(Lollipop.gene_name_labels.ToArray());
            cb_redBorder.Checked = true;
            cb_boldLabel.Checked = true;

            cmbx_colorScheme.SelectedIndex = 1;
            cmbx_nodeLayout.SelectedIndex = 1;
            cmbx_nodeLabelPositioning.SelectedIndex = 0;
            cmbx_geneLabel.SelectedIndex = 1;
            cmbx_nodeLabel.SelectedIndex = 1;
            cmbx_edgeLabel.SelectedIndex = 1;
            ProteoformCommunity.preferred_gene_label = cmbx_geneLabel.SelectedItem.ToString();
            ProteoformCommunity.gene_centric_families = cb_geneCentric.Checked;

            //Set parameters
            nud_bkgdShift.ValueChanged -= nud_bkgdShift_ValueChanged;
            nud_bkgdShift.Value = (decimal)-1.8;
            SaveState.lollipop.backgroundShift = nud_bkgdShift.Value;
            nud_bkgdShift.ValueChanged += nud_bkgdShift_ValueChanged;

            nud_bkgdWidth.ValueChanged -= nud_bkgdWidth_ValueChanged;
            nud_bkgdWidth.Value = (decimal)0.5;
            SaveState.lollipop.backgroundWidth = nud_bkgdWidth.Value;
            nud_bkgdWidth.ValueChanged += nud_bkgdWidth_ValueChanged;

            nud_bkgdShift.ValueChanged += new EventHandler(plotBiorepIntensitiesEvent);
            nud_bkgdWidth.ValueChanged += new EventHandler(plotBiorepIntensitiesEvent);

            nud_FDR.ValueChanged -= new EventHandler(updateGoTermsTable);
            nud_ratio.ValueChanged -= new EventHandler(updateGoTermsTable);
            nud_intensity.ValueChanged -= new EventHandler(updateGoTermsTable);

            nud_FDR.Value = SaveState.lollipop.minProteoformFDR;
            nud_ratio.Value = SaveState.lollipop.minProteoformFoldChange;
            nud_intensity.Value = SaveState.lollipop.minProteoformIntensity;

            nud_FDR.ValueChanged += new EventHandler(updateGoTermsTable);
            nud_ratio.ValueChanged += new EventHandler(updateGoTermsTable);
            nud_intensity.ValueChanged += new EventHandler(updateGoTermsTable);

            //Lollipop.getObservationParameters(); //examines the conditions and bioreps to determine the maximum number of observations to require for quantification
            nud_minObservations.Minimum = 1;
            nud_minObservations.Maximum = SaveState.lollipop.countOfBioRepsInOneCondition;
            nud_minObservations.Value = SaveState.lollipop.countOfBioRepsInOneCondition;
            SaveState.lollipop.minBiorepsWithObservations = (int)nud_minObservations.Value;

            cmbx_observationsTypeRequired.SelectedIndexChanged -= cmbx_observationsTypeRequired_SelectedIndexChanged;
            cmbx_observationsTypeRequired.Items.AddRange(Lollipop.observation_requirement_possibilities);
            cmbx_observationsTypeRequired.SelectedIndex = 0;
            SaveState.lollipop.observation_requirement = cmbx_observationsTypeRequired.SelectedItem.ToString();
            cmbx_observationsTypeRequired.SelectedIndexChanged += cmbx_observationsTypeRequired_SelectedIndexChanged;

            nud_sKnot_minFoldChange.ValueChanged -= nud_sKnot_minFoldChange_ValueChanged;
            nud_sKnot_minFoldChange.Value = SaveState.lollipop.sKnot_minFoldChange;
            nud_sKnot_minFoldChange.ValueChanged += nud_sKnot_minFoldChange_ValueChanged;

            nud_Offset.ValueChanged -= nud_Offset_ValueChanged;
            nud_Offset.Value = SaveState.lollipop.offsetTestStatistics;
            nud_Offset.ValueChanged += nud_Offset_ValueChanged;

            cmbx_goAspect.Items.Add(Aspect.BiologicalProcess);
            cmbx_goAspect.Items.Add(Aspect.CellularComponent);
            cmbx_goAspect.Items.Add(Aspect.MolecularFunction);

            cmbx_goAspect.SelectedIndexChanged -= cmbx_goAspect_SelectedIndexChanged; //disable event on load to prevent premature firing
            cmbx_goAspect.SelectedIndex = 0;
            cmbx_goAspect.SelectedIndexChanged += cmbx_goAspect_SelectedIndexChanged;

            rb_quantifiedSampleSet.Enabled = false;
            rb_quantifiedSampleSet.Checked = !SaveState.lollipop.allTheoreticalProteins; //initiallizes the background for GO analysis to the set of observed proteins. not the set of theoretical proteins.
            rb_quantifiedSampleSet.Enabled = true;

            rb_allTheoreticalProteins.Enabled = false;
            rb_allTheoreticalProteins.Checked = SaveState.lollipop.allTheoreticalProteins; //initiallizes the background for GO analysis to the set of observed proteins. not the set of theoretical proteins.
            rb_allTheoreticalProteins.Enabled = true;

            rb_quantifiedSampleSet.CheckedChanged += new EventHandler(goTermBackgroundChanged);
            rb_detectedSampleSet.CheckedChanged += new EventHandler(goTermBackgroundChanged);
            rb_customBackgroundSet.CheckedChanged += new EventHandler(goTermBackgroundChanged);
            rb_allTheoreticalProteins.CheckedChanged += new EventHandler(goTermBackgroundChanged);
        }

        #endregion Public Methods

        #region Quantification Private Methods

        private void btn_refreshCalculation_Click(object sender, EventArgs e)
        {
            if (ReadyToRunTheGamut())
            {
                Cursor = Cursors.WaitCursor;
                RunTheGamut();
                Cursor = Cursors.Default;
            }
            else if (SaveState.lollipop.get_files(SaveState.lollipop.input_files, Purpose.Quantification).Count() <= 0)
                MessageBox.Show("Please load quantification results in Load Deconvolution Results.", "Quantification");
            else if (SaveState.lollipop.raw_experimental_components.Count <= 0)
                MessageBox.Show("Please load deconvolution results.", "Quantification");
            else if (SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Length <= 0)
                MessageBox.Show("Please aggregate proteoform observations.", "Quantification");
            else if (SaveState.lollipop.target_proteoform_community.families.Count <= 0)
                MessageBox.Show("Please construct proteoform families.", "Quantification");
        }

        private void plotObservedVsExpectedRelativeDifference()
        {
            ct_relativeDifference.Series.Clear();
            ct_relativeDifference.Series.Add("obsVSexp");
            ct_relativeDifference.Series["obsVSexp"].ChartType = SeriesChartType.Point;
            ct_relativeDifference.Series.Add("positiveOffset");
            ct_relativeDifference.Series["positiveOffset"].ChartType = SeriesChartType.Line;
            ct_relativeDifference.Series.Add("negativeOffset");
            ct_relativeDifference.Series["negativeOffset"].ChartType = SeriesChartType.Line;
            ct_relativeDifference.ChartAreas[0].AxisX.Title = "Expected Relative Difference dE(i)";
            ct_relativeDifference.ChartAreas[0].AxisY.Title = "Observed Relative Difference d(i)";

            ct_relativeDifference.Series["obsVSexp"].Points.DataBindXY(SaveState.lollipop.sortedAvgPermutationTestStatistics.ToList(), SaveState.lollipop.sortedProteoformTestStatistics.ToList());

            if (SaveState.lollipop.sortedAvgPermutationTestStatistics.Count > 0 && SaveState.lollipop.sortedProteoformTestStatistics.Count > 0)
            {
                ct_relativeDifference.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(Math.Floor(SaveState.lollipop.sortedAvgPermutationTestStatistics.First()));
                ct_relativeDifference.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(Math.Ceiling(SaveState.lollipop.sortedAvgPermutationTestStatistics.Last()));
                ct_relativeDifference.ChartAreas[0].AxisY.Minimum = Math.Min(Convert.ToDouble(Math.Floor(negativeOffsetFunction(SaveState.lollipop.sortedProteoformTestStatistics.First()))), Convert.ToDouble(Math.Floor(SaveState.lollipop.sortedProteoformTestStatistics.First())));
                ct_relativeDifference.ChartAreas[0].AxisY.Maximum = Math.Max(Convert.ToDouble(Math.Ceiling(positiveOffsetFunction(SaveState.lollipop.sortedProteoformTestStatistics.Last()))), Convert.ToDouble(Math.Ceiling(SaveState.lollipop.sortedProteoformTestStatistics.Last())));
            }

            plotObservedVsExpectedOffsets();
        }

        private decimal positiveOffsetFunction(decimal x)
        {
            return (x + nud_Offset.Value);
        }

        private decimal negativeOffsetFunction(decimal x)
        {
            return (x - nud_Offset.Value);
        }

        private void plotObservedVsExpectedOffsets()
        {
            ct_relativeDifference.Series["positiveOffset"].Points.Clear();
            ct_relativeDifference.Series["negativeOffset"].Points.Clear();

            foreach (decimal xValue in SaveState.lollipop.sortedAvgPermutationTestStatistics)
            {
                ct_relativeDifference.Series["positiveOffset"].Points.AddXY(xValue, positiveOffsetFunction(xValue));
                ct_relativeDifference.Series["negativeOffset"].Points.AddXY(xValue, negativeOffsetFunction(xValue));
            }

            if (SaveState.lollipop.sortedAvgPermutationTestStatistics.Count <= 0 && SaveState.lollipop.sortedProteoformTestStatistics.Count <= 0)
            {
                ct_relativeDifference.ChartAreas[0].AxisX.Minimum = -10;
                ct_relativeDifference.ChartAreas[0].AxisX.Maximum = 10;
                ct_relativeDifference.ChartAreas[0].AxisY.Minimum = -10;
                ct_relativeDifference.ChartAreas[0].AxisY.Maximum = 10;
            }

            tb_FDR.Text = SaveState.lollipop.offsetFDR.ToString();
        }

        private void plotBiorepIntensitiesEvent(object s, EventArgs e)
        {
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
            ct_proteoformIntensities.ChartAreas[0].AxisX.Title = "Log (Base 2) Intensity";
            ct_proteoformIntensities.ChartAreas[0].AxisY.Title = "Count";


            foreach (KeyValuePair<decimal, int> entry in SaveState.lollipop.logSelectIntensityHistogram)
            {
                ct_proteoformIntensities.Series["Observed Intensities"].Points.AddXY(entry.Key, entry.Value);

                double gaussIntensity = ((double)SaveState.lollipop.selectGaussianHeight) * Math.Exp(-Math.Pow(((double)entry.Key - (double)SaveState.lollipop.selectAverageIntensity), 2) / (2d * Math.Pow((double)SaveState.lollipop.selectStDev, 2)));
                double bkgd_gaussIntensity = ((double)SaveState.lollipop.bkgdGaussianHeight) * Math.Exp(-Math.Pow(((double)entry.Key - (double)SaveState.lollipop.bkgdAverageIntensity), 2) / (2d * Math.Pow((double)SaveState.lollipop.bkgdStDev, 2)));
                double sumIntensity = gaussIntensity + bkgd_gaussIntensity;
                ct_proteoformIntensities.Series["Observed Fit"].Points.AddXY(entry.Key, gaussIntensity);
                ct_proteoformIntensities.Series["Background Projected"].Points.AddXY(entry.Key, bkgd_gaussIntensity);
                ct_proteoformIntensities.Series["Fit + Projected"].Points.AddXY(entry.Key, sumIntensity);
            }
        }

        private void volcanoPlot()
        {
            ct_volcano_logFold_logP.Series.Clear();
            ct_volcano_logFold_logP.Series.Add("logFold_logP");
            ct_volcano_logFold_logP.Series["logFold_logP"].ChartType = SeriesChartType.Point; // these are the actual experimental proteoform intensities

            ct_volcano_logFold_logP.ChartAreas[0].AxisX.Title = "Log (Base 2) Fold Change (" + SaveState.lollipop.numerator_condition + "/" + SaveState.lollipop.denominator_condition + ")";
            ct_volcano_logFold_logP.ChartAreas[0].AxisY.Title = "Log (Base 10) p-Value";

            foreach (QuantitativeProteoformValues qValue in SaveState.lollipop.qVals)
            {
                ct_volcano_logFold_logP.Series["logFold_logP"].Points.AddXY(qValue.logFoldChange, -Math.Log10((double)qValue.pValue));
            }

            if (SaveState.lollipop.qVals.Count > 0)
            {
                ct_volcano_logFold_logP.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(Math.Floor(SaveState.lollipop.qVals.Min(q => q.logFoldChange)));
                ct_volcano_logFold_logP.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(Math.Ceiling(SaveState.lollipop.qVals.Max(q => q.logFoldChange)));
                ct_volcano_logFold_logP.ChartAreas[0].AxisY.Minimum = Math.Floor(SaveState.lollipop.qVals.Min(q => -Math.Log10((double)q.pValue)));
                ct_volcano_logFold_logP.ChartAreas[0].AxisY.Maximum = Math.Ceiling(SaveState.lollipop.qVals.Max(q => -Math.Log10((double)q.pValue)));
            }
        }

        private void cmbx_ratioNumerator_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.numerator_condition = cmbx_ratioNumerator.SelectedItem.ToString();
        }

        private void cmbx_ratioDenominator_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.denominator_condition = cmbx_ratioDenominator.SelectedItem.ToString();
        }

        private void nud_bkgdShift_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.backgroundShift = nud_bkgdShift.Value;
            if (SaveState.lollipop.qVals.Count <= 0)
                return;
            SaveState.lollipop.defineAllObservedIntensityDistribution(SaveState.lollipop.target_proteoform_community.experimental_proteoforms, SaveState.lollipop.logIntensityHistogram);
            SaveState.lollipop.defineBackgroundIntensityDistribution(SaveState.lollipop.neucode_labeled, SaveState.lollipop.quantBioFracCombos, SaveState.lollipop.satisfactoryProteoforms, SaveState.lollipop.backgroundShift, SaveState.lollipop.backgroundWidth);
        }

        private void nud_bkgdWidth_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.backgroundWidth = nud_bkgdWidth.Value;
            if (SaveState.lollipop.qVals.Count <= 0)
                return;
            SaveState.lollipop.defineAllObservedIntensityDistribution(SaveState.lollipop.target_proteoform_community.experimental_proteoforms, SaveState.lollipop.logIntensityHistogram);
            SaveState.lollipop.defineBackgroundIntensityDistribution(SaveState.lollipop.neucode_labeled, SaveState.lollipop.quantBioFracCombos, SaveState.lollipop.satisfactoryProteoforms, SaveState.lollipop.backgroundShift, SaveState.lollipop.backgroundWidth);
        }

        private void cmbx_observationsTypeRequired_SelectedIndexChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.observation_requirement = cmbx_observationsTypeRequired.SelectedItem.ToString();
            if (SaveState.lollipop.observation_requirement == Lollipop.observation_requirement_possibilities[1]) // From any condition
                nud_minObservations.Maximum = SaveState.lollipop.conditionsBioReps.Sum(kv => kv.Value.Count);
            else
                nud_minObservations.Maximum = SaveState.lollipop.countOfBioRepsInOneCondition;
            nud_minObservations.Value = nud_minObservations.Maximum;
        }

        private void nud_minObservations_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.minBiorepsWithObservations = (int)nud_minObservations.Value;
        }

        private void nud_sKnot_minFoldChange_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.sKnot_minFoldChange = nud_sKnot_minFoldChange.Value;
            if (SaveState.lollipop.satisfactoryProteoforms.Count <= 0)
                return;
            SaveState.lollipop.computeProteoformTestStatistics(SaveState.lollipop.neucode_labeled, SaveState.lollipop.satisfactoryProteoforms, SaveState.lollipop.bkgdAverageIntensity, SaveState.lollipop.bkgdStDev, SaveState.lollipop.numerator_condition, SaveState.lollipop.denominator_condition, SaveState.lollipop.sKnot_minFoldChange);
            SaveState.lollipop.computeSortedTestStatistics(SaveState.lollipop.satisfactoryProteoforms);
            SaveState.lollipop.computeFoldChangeFDR(SaveState.lollipop.sortedAvgPermutationTestStatistics, SaveState.lollipop.sortedProteoformTestStatistics, SaveState.lollipop.satisfactoryProteoforms, SaveState.lollipop.permutedTestStatistics, SaveState.lollipop.offsetTestStatistics);
            SaveState.lollipop.computeIndividualExperimentalProteoformFDRs(SaveState.lollipop.satisfactoryProteoforms, SaveState.lollipop.sortedProteoformTestStatistics, SaveState.lollipop.minProteoformFoldChange, SaveState.lollipop.minProteoformFDR, SaveState.lollipop.minProteoformIntensity);
            plotObservedVsExpectedOffsets();
        }

        private void nud_Offset_ValueChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.offsetTestStatistics = nud_Offset.Value;
            if (SaveState.lollipop.satisfactoryProteoforms.Count <= 0)
                return;
            SaveState.lollipop.computeFoldChangeFDR(SaveState.lollipop.sortedAvgPermutationTestStatistics, SaveState.lollipop.sortedProteoformTestStatistics, SaveState.lollipop.satisfactoryProteoforms, SaveState.lollipop.permutedTestStatistics, SaveState.lollipop.offsetTestStatistics);
            plotObservedVsExpectedOffsets();
        }

        #endregion Quantification Private Methods

        #region GO-Term Analysis Private Methods

        private void fillGoTermsTable()
        {
            DisplayUtility.FillDataGridView(dgv_goAnalysis, SaveState.lollipop.goTermNumbers.Where(x => x.Aspect.ToString() == cmbx_goAspect.SelectedItem.ToString()).Select(g => new DisplayGoTermNumber(g)));
            DisplayGoTermNumber.FormatGridView(dgv_goAnalysis);
        }

        private void updateGoTermsTable(object s, EventArgs e)
        {
            SaveState.lollipop.minProteoformFDR = nud_FDR.Value;
            SaveState.lollipop.minProteoformFoldChange = nud_ratio.Value;
            SaveState.lollipop.minProteoformIntensity = nud_intensity.Value;
            SaveState.lollipop.inducedOrRepressedProteins = SaveState.lollipop.getInducedOrRepressedProteins(SaveState.lollipop.satisfactoryProteoforms, SaveState.lollipop.minProteoformFoldChange, SaveState.lollipop.minProteoformFDR, SaveState.lollipop.minProteoformIntensity);
            SaveState.lollipop.GO_analysis();
            fillGoTermsTable();
        }

        private void updateGoTermsTable()
        {
            SaveState.lollipop.minProteoformFDR = nud_FDR.Value;
            SaveState.lollipop.minProteoformFoldChange = nud_ratio.Value;
            SaveState.lollipop.minProteoformIntensity = nud_intensity.Value;
            SaveState.lollipop.inducedOrRepressedProteins = SaveState.lollipop.getInducedOrRepressedProteins(SaveState.lollipop.satisfactoryProteoforms, SaveState.lollipop.minProteoformFoldChange, SaveState.lollipop.minProteoformFDR, SaveState.lollipop.minProteoformIntensity);
            SaveState.lollipop.GO_analysis();
            fillGoTermsTable();
        }

        private void cmbx_goAspect_SelectedIndexChanged(object sender, EventArgs e)
        {
            fillGoTermsTable();
        }

        bool backgroundUpdated = true;
        private void goTermBackgroundChanged(object s, EventArgs e)
        {
            if (!backgroundUpdated)
            {
                SaveState.lollipop.GO_analysis();
                fillGoTermsTable();
            }
            backgroundUpdated = true;
        }

        private void rb_quantifiedSampleSet_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_quantifiedSampleSet.Checked)
            {
                SaveState.lollipop.backgroundProteinsList = "";
                tb_goTermCustomBackground.Text = "";
                backgroundUpdated = false;
            }
        }

        private void rb_customBackgroundSet_CheckedChanged(object sender, EventArgs e)
        {
            tb_goTermCustomBackground.Enabled = rb_customBackgroundSet.Checked;
            btn_customBackgroundBrowse.Enabled = rb_customBackgroundSet.Checked;
            if (rb_customBackgroundSet.Checked) btn_customBackgroundBrowse_Click(new object(), new EventArgs());
        }

        private void rb_detectedSampleSet_CheckedChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.allDetectedProteins = rb_detectedSampleSet.Checked;
            if (rb_detectedSampleSet.Checked)
            {
                SaveState.lollipop.backgroundProteinsList = "";
                tb_goTermCustomBackground.Text = "";
                backgroundUpdated = false;
            }
        }

        private void rb_allTheoreticalProteins_CheckedChanged(object sender, EventArgs e)
        {
            SaveState.lollipop.allTheoreticalProteins = rb_allTheoreticalProteins.Checked;
            if (rb_allTheoreticalProteins.Checked)
            {
                SaveState.lollipop.backgroundProteinsList = "";
                tb_goTermCustomBackground.Text = "";
                backgroundUpdated = false;
            }
        }

        OpenFileDialog fileOpen = new OpenFileDialog();
        private void btn_customBackgroundBrowse_Click(object sender, EventArgs e)
        {
            fileOpen.Filter = "Protein accession list (*.txt)|*.txt";
            fileOpen.FileName = "";
            DialogResult dr = this.fileOpen.ShowDialog();
            if (dr == DialogResult.OK && File.Exists(fileOpen.FileName))
            {
                SaveState.lollipop.backgroundProteinsList = fileOpen.FileName;
                tb_goTermCustomBackground.Text = fileOpen.FileName;
                if (rb_customBackgroundSet.Checked)
                {
                    backgroundUpdated = false;
                    goTermBackgroundChanged(new object(), new EventArgs());
                }
            }
        }

        #endregion GO-Term Analysis Private Methods

        #region Cytoscape Visualization Private Methods

        OpenFileDialog fileOpener = new OpenFileDialog();
        FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
        bool got_cyto_temp_folder = false;

        private void btn_browseTempFolder_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.folderBrowser.ShowDialog();
            if (dr == DialogResult.OK)
            {
                string temp_folder_path = folderBrowser.SelectedPath;
                tb_familyBuildFolder.Text = temp_folder_path; //triggers TextChanged method
            }
        }

        private void tb_familyBuildFolder_TextChanged(object sender, EventArgs e)
        {
            string path = tb_familyBuildFolder.Text;
            SaveState.lollipop.family_build_folder_path = path;
            got_cyto_temp_folder = true;
            enable_buildAllFamilies_button();
            enable_buildSelectedFamilies_button();
        }

        private void enable_buildAllFamilies_button()
        {
            if (got_cyto_temp_folder)
                btn_buildAllFamilies.Enabled = true;
        }

        private void enable_buildSelectedFamilies_button()
        {
            if (got_cyto_temp_folder && dgv_quantification_results.SelectedRows.Count > 0)
                btn_buildSelectedQuantFamilies.Enabled = true;
        }

        private void btn_buildAllQuantifiedFamilies_Click(object sender, EventArgs e)
        {
            string time_stamp = SaveState.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            string message = CytoscapeScript.write_cytoscape_script(SaveState.lollipop.target_proteoform_community.families, SaveState.lollipop.target_proteoform_community.families,
                SaveState.lollipop.family_build_folder_path, "", time_stamp, true, cb_redBorder.Checked, cb_boldLabel.Checked,
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), SaveState.lollipop.deltaM_edge_display_rounding,
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void btn_buildFamiliesWithSignificantChange_Click(object sender, EventArgs e)
        {
            List<ProteoformFamily> families = SaveState.lollipop.getInterestingFamilies(SaveState.lollipop.satisfactoryProteoforms, SaveState.lollipop.minProteoformFoldChange, SaveState.lollipop.minProteoformFDR, SaveState.lollipop.minProteoformIntensity).Distinct().ToList();
            string time_stamp = SaveState.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            string message = CytoscapeScript.write_cytoscape_script(families, SaveState.lollipop.target_proteoform_community.families,
                SaveState.lollipop.family_build_folder_path, "", time_stamp, true, cb_redBorder.Checked, cb_boldLabel.Checked,
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), SaveState.lollipop.deltaM_edge_display_rounding,
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void btn_buildSelectedQuantFamilies_Click(object sender, EventArgs e)
        {
            string time_stamp = SaveState.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            object[] selected = DisplayUtility.get_selected_objects(dgv_quantification_results);
            string message = CytoscapeScript.write_cytoscape_script(selected, SaveState.lollipop.target_proteoform_community.families,
                SaveState.lollipop.family_build_folder_path, "", time_stamp, true, cb_redBorder.Checked, cb_boldLabel.Checked,
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), SaveState.lollipop.deltaM_edge_display_rounding,
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void btn_buildFamiliesAllGO_Click(object sender, EventArgs e)
        {
            Aspect a = (Aspect)cmbx_goAspect.SelectedItem;
            List<ProteoformFamily> go_families = SaveState.lollipop.getInterestingFamilies(SaveState.lollipop.goTermNumbers.Where(n => n.Aspect == a).Distinct().ToList(), SaveState.lollipop.target_proteoform_community.families);
            string time_stamp = SaveState.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            string message = CytoscapeScript.write_cytoscape_script(go_families, SaveState.lollipop.target_proteoform_community.families,
                SaveState.lollipop.family_build_folder_path, "", time_stamp, true, cb_redBorder.Checked, cb_boldLabel.Checked, 
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), SaveState.lollipop.deltaM_edge_display_rounding,
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void btn_buildFromSelectedGoTerms_Click(object sender, EventArgs e)
        {
            List<GoTermNumber> selected_gos = (DisplayUtility.get_selected_objects(dgv_goAnalysis).Select(o => (GoTermNumber)o)).ToList();
            List<ProteoformFamily> selected_families = SaveState.lollipop.getInterestingFamilies(selected_gos, SaveState.lollipop.target_proteoform_community.families).Distinct().ToList();
            string time_stamp = SaveState.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            string message = CytoscapeScript.write_cytoscape_script(selected_families, SaveState.lollipop.target_proteoform_community.families,
                SaveState.lollipop.family_build_folder_path, "", time_stamp, true, cb_redBorder.Checked, cb_boldLabel.Checked,
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), SaveState.lollipop.deltaM_edge_display_rounding,
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void cmbx_geneLabel_SelectedIndexChanged(object sender, EventArgs e)
        {
            ProteoformCommunity.preferred_gene_label = cmbx_geneLabel.SelectedItem.ToString();
        }

        private void cb_geneCentric_CheckedChanged(object sender, EventArgs e)
        {
            ProteoformCommunity.gene_centric_families = cb_geneCentric.Checked;
        }

        private void cmbx_empty_TextChanged(object sender, EventArgs e) { }

        #endregion Cytoscape Visualization Private Methods

    }
}