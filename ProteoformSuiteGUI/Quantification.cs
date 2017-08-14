using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
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
            this.AutoScroll = true;
            this.AutoScrollMinSize = this.ClientSize;
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
            Sweet.lollipop.quantify();
            Sweet.lollipop.Log2FoldChangeAnalysis.calculate_log2fc_statistics();
            get_go_analysis().GoAnalysis.GO_analysis(Sweet.lollipop.significance_by_log2FC ? Sweet.lollipop.Log2FoldChangeAnalysis.inducedOrRepressedProteins : get_go_analysis().inducedOrRepressedProteins);
            FillTablesAndCharts();
        }

        public void ClearListsTablesFigures(bool clear_following)
        {
            Sweet.lollipop.TusherAnalysis1.QuantitativeDistributions.logIntensityHistogram.Clear();
            Sweet.lollipop.TusherAnalysis1.QuantitativeDistributions.logSelectIntensityHistogram.Clear();
            Sweet.lollipop.TusherAnalysis2.QuantitativeDistributions.logIntensityHistogram.Clear();
            Sweet.lollipop.TusherAnalysis2.QuantitativeDistributions.logSelectIntensityHistogram.Clear();
            Sweet.lollipop.satisfactoryProteoforms.Clear();
            Sweet.lollipop.qVals.Clear();
            Sweet.lollipop.quantifiedProteins.Clear();
            Sweet.lollipop.TusherAnalysis1.inducedOrRepressedProteins.Clear();
            Sweet.lollipop.TusherAnalysis2.inducedOrRepressedProteins.Clear();
            Sweet.lollipop.Log2FoldChangeAnalysis.inducedOrRepressedProteins.Clear();
            Sweet.lollipop.TusherAnalysis1.GoAnalysis.goTermNumbers.Clear();
            Sweet.lollipop.TusherAnalysis2.GoAnalysis.goTermNumbers.Clear();
            Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.goTermNumbers.Clear();

            foreach (var series in ct_proteoformIntensities.Series) series.Points.Clear();
            foreach (var series in ct_relativeDifference.Series) series.Points.Clear();
            foreach (var series in ct_volcano_logFold_logP.Series) series.Points.Clear();

            dgv_goAnalysis.DataSource = null;
            dgv_quantification_results.DataSource = null;
            dgv_goAnalysis.Rows.Clear();
            dgv_quantification_results.Rows.Clear();
            tb_avgIntensity.Clear();
            tb_FDR.Clear();
            tb_stdevIntensity.Clear();

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
            return Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Count() > 0 
                && Sweet.lollipop.target_proteoform_community.families.Count > 0
                // and all of the files need to have unique bio/fract/tech replicate annotations for logFC analysis
                && Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Select(f => f.biological_replicate + f.fraction + f.technical_replicate).Distinct().Count() == Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Count();
        }

        public void FillTablesAndCharts()
        {
            plots();
            TusherAnalysis analysis = get_tusher_analysis();

            tb_avgIntensity.Text = Math.Round(get_tusher_analysis().QuantitativeDistributions.selectAverageIntensity, 1).ToString();
            tb_stdevIntensity.Text = Math.Round(get_tusher_analysis().QuantitativeDistributions.selectStDev, 3).ToString();
            tb_FDR.Text = Math.Round(get_tusher_analysis().relativeDifferenceFDR, 4).ToString();
            plotBiorepIntensities();
            updateGoTermsTable();
        }

        public void initialize_every_time()
        {
            tb_familyBuildFolder.Text = Sweet.lollipop.family_build_folder_path;
            if (cmbx_geneLabel.Items.Count > 0)
                cmbx_geneLabel.SelectedIndex = Lollipop.gene_name_labels.IndexOf(Lollipop.preferred_gene_label);
            cb_geneCentric.Checked = Lollipop.gene_centric_families;

            int selection = cmbx_relativeDifferenceChartSelection.SelectedIndex;

            string[] relative_difference_selections = new string[]
            {
                "Observed vs. Expected (" + Sweet.lollipop.TusherAnalysis1.sortedPermutedRelativeDifferences.Count.ToString() + " Permutations)",
                "Observed vs. Scatter (" + Sweet.lollipop.TusherAnalysis1.sortedPermutedRelativeDifferences.Count.ToString() + " Permutations)",
                "All Permuted vs. Average Permuted (" + Sweet.lollipop.TusherAnalysis1.sortedPermutedRelativeDifferences.Count.ToString() + " Permutations)",
                "Observed vs. Expected (" + Sweet.lollipop.TusherAnalysis2.sortedPermutedRelativeDifferences.Count.ToString() + " Permutations)",
                "Observed vs. Scatter (" + Sweet.lollipop.TusherAnalysis2.sortedPermutedRelativeDifferences.Count.ToString() + " Permutations)",
                "All Permuted vs. Average Permuted (" + Sweet.lollipop.TusherAnalysis2.sortedPermutedRelativeDifferences.Count.ToString() + " Permutations)",
            };

            cmbx_relativeDifferenceChartSelection.Items.Clear();
            cmbx_relativeDifferenceChartSelection.Items.AddRange(relative_difference_selections);
            cmbx_relativeDifferenceChartSelection.SelectedIndex = selection;

            fill_quantitative_values_table();
        }

        public void InitializeConditionsParameters()
        {
            //Initialize conditions -- need to do after files entered
            List<string> conditions = Sweet.lollipop.ltConditionsBioReps.Keys.ToList();
            conditions.AddRange(Sweet.lollipop.hvConditionsBioReps.Keys.ToList());
            conditions = conditions.Distinct().ToList();
            cmbx_ratioNumerator.Items.AddRange(conditions.ToArray());
            cmbx_ratioDenominator.Items.AddRange(conditions.ToArray());
            cmbx_inducedCondition.Items.AddRange(conditions.ToArray());
            cmbx_ratioDenominator.SelectedIndex = 0;
            cmbx_ratioNumerator.SelectedIndex = Convert.ToInt32(conditions.Count > 1);
            cmbx_inducedCondition.SelectedIndex = Convert.ToInt32(conditions.Count > 1);
            Sweet.lollipop.numerator_condition = cmbx_ratioNumerator.SelectedItem.ToString();
            Sweet.lollipop.denominator_condition = cmbx_ratioDenominator.SelectedItem.ToString();
            Sweet.lollipop.induced_condition = cmbx_inducedCondition.SelectedItem.ToString();
            cmbx_edgeLabel.Items.AddRange(Lollipop.edge_labels);
        }

        public void InitializeParameterSet()
        {
            //Initialize display options
            cmbx_volcanoChartSelection.Items.AddRange(volcano_selections);
            cmbx_colorScheme.Items.AddRange(CytoscapeScript.color_scheme_names);
            cmbx_nodeLayout.Items.AddRange(Lollipop.node_positioning);
            cmbx_nodeLabelPositioning.Items.AddRange(CytoscapeScript.node_label_positions);
            cmbx_edgeLabel.Items.AddRange(Lollipop.edge_labels.ToArray());
            cmbx_nodeLabel.Items.AddRange(Lollipop.node_labels.ToArray());
            cmbx_geneLabel.Items.AddRange(Lollipop.gene_name_labels.ToArray());
            cb_redBorder.Checked = true;
            cb_boldLabel.Checked = true;

            cmbx_volcanoChartSelection.SelectedIndex = 0;
            cmbx_colorScheme.SelectedIndex = 1;
            cmbx_nodeLayout.SelectedIndex = 1;
            cmbx_nodeLabelPositioning.SelectedIndex = 0;
            cmbx_geneLabel.SelectedIndex = 1;
            cmbx_nodeLabel.SelectedIndex = 1;
            cmbx_edgeLabel.SelectedIndex = 1;
            Lollipop.preferred_gene_label = cmbx_geneLabel.SelectedItem.ToString();
            Lollipop.gene_centric_families = cb_geneCentric.Checked;

            string[] relative_difference_selections = new string[]
            {
                "Observed vs. Expected" ,
                "Observed vs. Scatter",
                "All Permuted vs. Average Permuted",
                "Observed vs. Expected",
                "Observed vs. Scatter",
                "All Permuted vs. Average Permuted",
            };

            cmbx_relativeDifferenceChartSelection.Items.AddRange(relative_difference_selections);
            cmbx_relativeDifferenceChartSelection.SelectedIndexChanged -= cmbx_relativeDifferenceChartSelection_SelectedIndexChanged;
            cmbx_relativeDifferenceChartSelection.SelectedIndex = 3; //start with obs vs exp
            cmbx_relativeDifferenceChartSelection.SelectedIndexChanged += cmbx_relativeDifferenceChartSelection_SelectedIndexChanged;

            cmbx_intensityDistributionChartSelection.Items.AddRange(biorepintensity_selections);
            cmbx_intensityDistributionChartSelection.SelectedIndexChanged -= cmbx_relativeDifferenceChartSelection_SelectedIndexChanged;
            cmbx_intensityDistributionChartSelection.SelectedIndex = 3; //start with projected
            cmbx_intensityDistributionChartSelection.SelectedIndexChanged += cmbx_relativeDifferenceChartSelection_SelectedIndexChanged;

            cmbx_quantitativeValuesTableSelection.Items.AddRange(quantitative_table_selections);
            cmbx_quantitativeValuesTableSelection.SelectedIndexChanged -= cmbx_quantitativeValuesTableSelection_SelectedIndexChanged;
            cmbx_quantitativeValuesTableSelection.SelectedIndex = 0;
            cmbx_quantitativeValuesTableSelection.SelectedIndexChanged += cmbx_quantitativeValuesTableSelection_SelectedIndexChanged;

            //Set parameters
            cb_significanceByFoldChange.CheckedChanged -= cb_significanceByFoldChange_CheckedChanged;
            cb_significanceByFoldChange.Checked = Sweet.lollipop.significance_by_log2FC;
            cb_significanceByFoldChange.CheckedChanged += cb_significanceByFoldChange_CheckedChanged;

            cb_significanceByPermutation.CheckedChanged -= cb_significanceByPermutation_CheckedChanged;
            cb_significanceByPermutation.Checked = Sweet.lollipop.significance_by_permutation;
            cb_significanceByPermutation.CheckedChanged += cb_significanceByPermutation_CheckedChanged;

            cb_useAveragePermutationFoldChange.CheckedChanged -= cb_useAveragePermutationFoldChange_CheckedChanged;
            cb_useAveragePermutationFoldChange.Checked = Sweet.lollipop.useAveragePermutationFoldChange;
            cb_useAveragePermutationFoldChange.CheckedChanged += cb_useAveragePermutationFoldChange_CheckedChanged;

            cb_useFoldChangeCutoff.CheckedChanged -= cb_useFoldChangeCutoff_CheckedChanged;
            cb_useFoldChangeCutoff.Checked = Sweet.lollipop.useFoldChangeCutoff;
            cb_useFoldChangeCutoff.CheckedChanged += cb_useFoldChangeCutoff_CheckedChanged;

            cb_useBiorepPermutationFoldChange.CheckedChanged -= cb_useBiorepPermutationFoldChange_CheckedChanged;
            cb_useBiorepPermutationFoldChange.Checked = Sweet.lollipop.useBiorepPermutationFoldChange;
            cb_useBiorepPermutationFoldChange.CheckedChanged += cb_useBiorepPermutationFoldChange_CheckedChanged;

            nud_foldChangeCutoff.ValueChanged -= nud_permutationFoldChangeCutoff_ValueChanged;
            nud_foldChangeCutoff.Value = Sweet.lollipop.foldChangeCutoff;
            nud_foldChangeCutoff.ValueChanged += nud_permutationFoldChangeCutoff_ValueChanged;

            nud_benjiHochFDR.ValueChanged -= nud_benjiHochFDR_ValueChanged;
            nud_benjiHochFDR.Value = (decimal)Sweet.lollipop.Log2FoldChangeAnalysis.benjiHoch_fdr;
            nud_benjiHochFDR.ValueChanged += nud_benjiHochFDR_ValueChanged;

            nud_bkgdShift.ValueChanged -= nud_bkgdShift_ValueChanged;
            nud_bkgdShift.Value = Sweet.lollipop.backgroundShift;
            nud_bkgdShift.ValueChanged += nud_bkgdShift_ValueChanged;
            nud_bkgdShift.ValueChanged += new EventHandler(plotBiorepIntensitiesEvent);

            nud_bkgdWidth.ValueChanged -= nud_bkgdWidth_ValueChanged;
            nud_bkgdWidth.Value = Sweet.lollipop.backgroundWidth;
            nud_bkgdWidth.ValueChanged += nud_bkgdWidth_ValueChanged;
            nud_bkgdWidth.ValueChanged += new EventHandler(plotBiorepIntensitiesEvent);

            nud_FDR.ValueChanged -= new EventHandler(updateGoTermsTable);
            nud_FDR.Value = get_go_analysis().GoAnalysis.maxGoTermFDR;
            nud_FDR.ValueChanged += new EventHandler(updateGoTermsTable);

            nud_ratio.ValueChanged -= new EventHandler(updateGoTermsTable);
            nud_ratio.Value = get_go_analysis().GoAnalysis.minProteoformFoldChange;
            nud_ratio.ValueChanged += new EventHandler(updateGoTermsTable);

            nud_intensity.ValueChanged -= new EventHandler(updateGoTermsTable);
            nud_intensity.Value = get_go_analysis().GoAnalysis.minProteoformIntensity;
            nud_intensity.ValueChanged += new EventHandler(updateGoTermsTable);


            // selecting proteoforms for quantification
            cmbx_observationsTypeRequired.SelectedIndexChanged -= cmbx_observationsTypeRequired_SelectedIndexChanged;
            cmbx_observationsTypeRequired.Items.AddRange(Lollipop.observation_requirement_possibilities);
            cmbx_observationsTypeRequired.SelectedIndex = Sweet.lollipop.observation_requirement == new Lollipop().observation_requirement ? // check that the default has not been changed (haven't loaded presets)
                0 :
                Lollipop.observation_requirement_possibilities.ToList().IndexOf(Sweet.lollipop.observation_requirement);
            Sweet.lollipop.observation_requirement = cmbx_observationsTypeRequired.SelectedItem.ToString();
            cmbx_observationsTypeRequired.SelectedIndexChanged += cmbx_observationsTypeRequired_SelectedIndexChanged;

            nud_minObservations.Minimum = 1;
            if (Sweet.lollipop.minBiorepsWithObservations == new Lollipop().minBiorepsWithObservations) // check that the default has not been changed (haven't loaded presets)
            {
                nud_minObservations.Maximum = Sweet.lollipop.countOfBioRepsInOneCondition;
                nud_minObservations.Value = Sweet.lollipop.countOfBioRepsInOneCondition;
            }
            else
            {
                set_nud_minObs_maximum();
                nud_minObservations.Value = Sweet.lollipop.minBiorepsWithObservations;
            }
            Sweet.lollipop.minBiorepsWithObservations = (int)nud_minObservations.Value;


            // permutation fold change requirement
            nud_foldChangeObservations.Minimum = 1;
            if (Sweet.lollipop.minBiorepsWithFoldChange == new Lollipop().minBiorepsWithFoldChange) // check that the default has not been changed (haven't loaded presets)
            {
                nud_foldChangeObservations.Maximum = Sweet.lollipop.countOfBioRepsInOneCondition;
                nud_foldChangeObservations.Value = Sweet.lollipop.countOfBioRepsInOneCondition;
            }
            else
            {
                set_nud_minObs_maximum();
                nud_foldChangeObservations.Value = Sweet.lollipop.minBiorepsWithFoldChange;
            }
            Sweet.lollipop.minBiorepsWithFoldChange = (int)nud_foldChangeObservations.Value;

            cmbx_foldChangeConjunction.SelectedIndexChanged -= cmbx_foldChangeConjunction_SelectedIndexChanged;
            cmbx_foldChangeConjunction.Items.AddRange(Lollipop.fold_change_conjunction_options);
            cmbx_foldChangeConjunction.SelectedIndex = Lollipop.fold_change_conjunction_options.ToList().IndexOf(Sweet.lollipop.fold_change_conjunction);
            cmbx_foldChangeConjunction.SelectedIndexChanged += cmbx_foldChangeConjunction_SelectedIndexChanged;

            cb_useAveragePermutationFoldChange.CheckedChanged -= cb_useAveragePermutationFoldChange_CheckedChanged;
            cb_useAveragePermutationFoldChange.Checked = Sweet.lollipop.useAveragePermutationFoldChange;
            cb_useAveragePermutationFoldChange.CheckedChanged += cb_useAveragePermutationFoldChange_CheckedChanged;

            cb_useBiorepPermutationFoldChange.CheckedChanged -= cb_useBiorepPermutationFoldChange_CheckedChanged;
            cb_useBiorepPermutationFoldChange.Checked = Sweet.lollipop.useBiorepPermutationFoldChange;
            cb_useBiorepPermutationFoldChange.CheckedChanged += cb_useBiorepPermutationFoldChange_CheckedChanged;


            nud_Offset.ValueChanged -= nud_Offset_ValueChanged;
            nud_Offset.Value = Sweet.lollipop.offsetTestStatistics;
            nud_Offset.ValueChanged += nud_Offset_ValueChanged;

            nud_localFdrCutoff.ValueChanged -= nud_localFdrCutoff_ValueChanged;
            nud_localFdrCutoff.Value = Sweet.lollipop.localFdrCutoff;
            nud_localFdrCutoff.ValueChanged += nud_localFdrCutoff_ValueChanged;
            nud_localFdrCutoff.Enabled = cb_useLocalFdrCutoff.Checked;

            cmbx_goAspect.Items.Add(Aspect.BiologicalProcess);
            cmbx_goAspect.Items.Add(Aspect.CellularComponent);
            cmbx_goAspect.Items.Add(Aspect.MolecularFunction);

            cmbx_goAspect.SelectedIndexChanged -= cmbx_goAspect_SelectedIndexChanged; //disable event on load to prevent premature firing
            cmbx_goAspect.SelectedIndex = 0;
            cmbx_goAspect.SelectedIndexChanged += cmbx_goAspect_SelectedIndexChanged;

            rb_quantifiedSampleSet.Enabled = false;
            rb_quantifiedSampleSet.Checked = !get_go_analysis().GoAnalysis.allTheoreticalProteins; //initiallizes the background for GO analysis to the set of observed proteins. not the set of theoretical proteins.
            rb_quantifiedSampleSet.Enabled = true;

            rb_allTheoreticalProteins.Enabled = false;
            rb_allTheoreticalProteins.Checked = get_go_analysis().GoAnalysis.allTheoreticalProteins; //initiallizes the background for GO analysis to the set of observed proteins. not the set of theoretical proteins.
            rb_allTheoreticalProteins.Enabled = true;

            cb_useRandomSeed.Checked = Sweet.lollipop.useRandomSeed;
            nud_foldChangeCutoff.Enabled = Sweet.lollipop.useFoldChangeCutoff;
            nud_randomSeed.Enabled = Sweet.lollipop.useRandomSeed;

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
            else if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Count() <= 0)
                MessageBox.Show("Please load quantification results in Load Deconvolution Results.", "Quantification");
            else if (Sweet.lollipop.raw_experimental_components.Count <= 0)
                MessageBox.Show("Please load deconvolution results.", "Quantification");
            else if (Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Length <= 0)
                MessageBox.Show("Please aggregate proteoform observations.", "Quantification");
            else if (Sweet.lollipop.target_proteoform_community.families.Count <= 0)
                MessageBox.Show("Please construct proteoform families.", "Quantification");
            else if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Select(f => f.biological_replicate + f.fraction + f.technical_replicate).Distinct().Count() != Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Count())
                MessageBox.Show("Please label each quantification input file with a unique set of labels, i.e. biological replicate, fraction, and technical replicate.", "Quantification");
        }

        private void cmbx_ratioNumerator_SelectedIndexChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.numerator_condition = cmbx_ratioNumerator.SelectedItem.ToString();
        }

        private void cmbx_ratioDenominator_SelectedIndexChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.denominator_condition = cmbx_ratioDenominator.SelectedItem.ToString();
        }

        private void cmbx_inducedCondition_SelectedIndexChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.induced_condition = cmbx_inducedCondition.SelectedItem.ToString();
        }

        private void nud_bkgdShift_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.backgroundShift = nud_bkgdShift.Value;
            if (Sweet.lollipop.qVals.Count <= 0)
                return;
            get_tusher_analysis().QuantitativeDistributions.defineAllObservedIntensityDistribution(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, get_tusher_analysis().QuantitativeDistributions.logIntensityHistogram);
            get_tusher_analysis().QuantitativeDistributions.defineBackgroundIntensityDistribution(Sweet.lollipop.quantBioFracCombos, Sweet.lollipop.satisfactoryProteoforms, Sweet.lollipop.condition_count, Sweet.lollipop.backgroundShift, Sweet.lollipop.backgroundWidth);
        }

        private void nud_bkgdWidth_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.backgroundWidth = nud_bkgdWidth.Value;
            if (Sweet.lollipop.qVals.Count <= 0)
                return;
            get_tusher_analysis().QuantitativeDistributions.defineAllObservedIntensityDistribution(Sweet.lollipop.target_proteoform_community.experimental_proteoforms, get_tusher_analysis().QuantitativeDistributions.logIntensityHistogram);
            get_tusher_analysis().QuantitativeDistributions.defineBackgroundIntensityDistribution(Sweet.lollipop.quantBioFracCombos, Sweet.lollipop.satisfactoryProteoforms, Sweet.lollipop.condition_count, Sweet.lollipop.backgroundShift, Sweet.lollipop.backgroundWidth);
        }

        private void cb_useRandomSeed_CheckedChanged(object sender, EventArgs e)
        {
            nud_randomSeed.Enabled = cb_useRandomSeed.Checked;
            Sweet.lollipop.useRandomSeed = cb_useRandomSeed.Checked;
        }

        private void nud_randomSeed_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.randomSeed = Convert.ToInt32(nud_randomSeed.Value);
        }

        #endregion Quantification Private Methods

        #region Quantification Table Methods

        private void cmbx_observationsTypeRequired_SelectedIndexChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.observation_requirement = cmbx_observationsTypeRequired.SelectedItem.ToString();
            set_nud_minObs_maximum();
            nud_minObservations.Value = nud_minObservations.Maximum;
        }

        private void set_nud_minObs_maximum()
        {
            if (Sweet.lollipop.observation_requirement == Lollipop.observation_requirement_possibilities[1]) // From any condition
                nud_minObservations.Maximum = Sweet.lollipop.conditionsBioReps.Sum(kv => kv.Value.Count);
            else if (Lollipop.observation_requirement_possibilities.ToList().IndexOf(Sweet.lollipop.observation_requirement) < 3)
                nud_minObservations.Maximum = Sweet.lollipop.countOfBioRepsInOneCondition;
            else if (Sweet.lollipop.observation_requirement == Lollipop.observation_requirement_possibilities[4]) // From any condition
                nud_minObservations.Maximum = Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Select(x => x.lt_condition + x.biological_replicate + x.technical_replicate).Distinct().Count() * (2 * Convert.ToInt32(Sweet.lollipop.neucode_labeled));
            else
                nud_minObservations.Maximum = Math.Min(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Where(x => x.lt_condition == Sweet.lollipop.numerator_condition).Concat(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Where(x => x.hv_condition == Sweet.lollipop.numerator_condition)).Select(x => x.biological_replicate + x.technical_replicate).Distinct().Count(),
                    Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Where(x => x.lt_condition == Sweet.lollipop.denominator_condition).Concat(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Where(x => x.hv_condition == Sweet.lollipop.denominator_condition)).Select(x => x.biological_replicate + x.technical_replicate).Distinct().Count());
        }

        private void nud_minObservations_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.minBiorepsWithObservations = (int)nud_minObservations.Value;
        }

        private string[] quantitative_table_selections = new string[]
        {
            "Quantitative Values", // 0
            "Results Table", // 1
            "Biorep Intensity Sums (Normalized with Imputation) for Selected Proteoforms", // 2
            "Biorep Intensity Sums (Normalized with Imputation) for All Proteoforms", // 3
            "Biorep Intensity Sums (Normalized without Imputation) for Selected Proteoforms", // 4
            "Biorep Intensity Sums (Normalized without Imputation) for All Proteoforms", // 5
            "Biorep-Techrep Intensity Sums (Normalized with Imputation) for Selected Proteoforms",  // 6
            "Biorep-Techrep Intensity Sums (Normalized with Imputation) for All Proteoforms", // 7
            "Biorep-Techrep Intensity Sums (Normalized without Imputation) for Selected Proteoforms", // 8
            "Biorep-Techrep Intensity Sums (Normalized without Imputation) for All Proteoforms", // 9
            "File-Condition Intensity Sums (Normalized with Imputation) for Selected Proteoforms", // 10
            "File-Condition Intensity Sums (Normalized with Imputation) for All Proteoforms", // 11
            "File-Condition Intensity Sums (Normalized without Imputation) for Selected Proteoforms", // 12
            "File-Condition Intensity Sums (Normalized without Imputation) for All Proteoforms" // 13
        };


        private void fill_quantitative_values_table()
        {
            TusherAnalysis tusher = get_tusher_analysis();

            if (cmbx_quantitativeValuesTableSelection.SelectedIndex == 0)
            {
                DisplayUtility.FillDataGridView(dgv_quantification_results, Sweet.lollipop.qVals.Select(q => new DisplayQuantitativeValues(q, tusher)));
                DisplayQuantitativeValues.FormatGridView(dgv_quantification_results);
                return;
            }

            if (cmbx_quantitativeValuesTableSelection.SelectedIndex == 1)
            {
                DisplayUtility.FillDataGridView(dgv_quantification_results, ResultsSummaryGenerator.results_dataframe(tusher));
                return;
            }

            IGoAnalysis analysis = new int[] { 2,3,4,5 }.Contains(cmbx_quantitativeValuesTableSelection.SelectedIndex) 
                ? Sweet.lollipop.TusherAnalysis1 as IGoAnalysis 
                : new int[] { 6,7,8,9 }.Contains(cmbx_quantitativeValuesTableSelection.SelectedIndex) 
                    ? Sweet.lollipop.TusherAnalysis2 as IGoAnalysis
                    : Sweet.lollipop.Log2FoldChangeAnalysis as IGoAnalysis;

            IEnumerable<ExperimentalProteoform> proteoforms = new int[] { 2,4,6,8,10,12 }.Contains(cmbx_quantitativeValuesTableSelection.SelectedIndex) 
                ? Sweet.lollipop.satisfactoryProteoforms as IEnumerable<ExperimentalProteoform> 
                : Sweet.lollipop.target_proteoform_community.experimental_proteoforms as IEnumerable<ExperimentalProteoform>;

            bool include_imputation = new int[] { 2, 3, 6, 7, 10, 11 }.Contains(cmbx_quantitativeValuesTableSelection.SelectedIndex);

            DisplayUtility.FillDataGridView(dgv_quantification_results, ResultsSummaryGenerator.biological_replicate_intensities(analysis, proteoforms, Sweet.lollipop.input_files, Sweet.lollipop.conditionsBioReps, include_imputation));
        }

        private void cmbx_quantitativeValuesTableSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            fill_quantitative_values_table();
        }

        #endregion Quantification Table Methods

        #region Volcano Plot Methods

        private void volcanoPlot()
        {
            int plot_selection = cmbx_volcanoChartSelection.SelectedIndex;

            ct_volcano_logFold_logP.Series.Clear();
            ct_volcano_logFold_logP.Series.Add("logFold_logP");
            ct_volcano_logFold_logP.Series["logFold_logP"].ChartType = SeriesChartType.Point; // these are the actual experimental proteoform intensities
            ct_volcano_logFold_logP.Series.Add("significantlogFold_logP");
            ct_volcano_logFold_logP.Series["significantlogFold_logP"].ChartType = SeriesChartType.Point; // these are the actual experimental proteoform intensities

            ct_volcano_logFold_logP.ChartAreas[0].AxisX.Title = "Log (Base 2) Fold Change (" + Sweet.lollipop.numerator_condition + "/" + Sweet.lollipop.denominator_condition + ")";
            ct_volcano_logFold_logP.ChartAreas[0].AxisY.Title = "Log (Base 10) p-Value";

            foreach (QuantitativeProteoformValues qValue in Sweet.lollipop.qVals)
            {
                if (get_tusher_values(qValue).significant && Sweet.lollipop.significance_by_permutation || qValue.Log2FoldChangeValues.significant && Sweet.lollipop.significance_by_log2FC)
                    ct_volcano_logFold_logP.Series["significantlogFold_logP"].Points.AddXY(plot_selection == 0 ? qValue.Log2FoldChangeValues.average_log2fc : (double)qValue.logFoldChange, -Math.Log10(qValue.Log2FoldChangeValues.pValue_uncorrected));
                else
                    ct_volcano_logFold_logP.Series["logFold_logP"].Points.AddXY(plot_selection == 0 ? qValue.Log2FoldChangeValues.average_log2fc : (double)qValue.logFoldChange, -Math.Log10(qValue.Log2FoldChangeValues.pValue_uncorrected));
            }

            if (Sweet.lollipop.qVals.Count > 0)
            {
                ct_volcano_logFold_logP.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(Math.Floor(Sweet.lollipop.qVals.Min(q => plot_selection == 0 ? q.Log2FoldChangeValues.average_log2fc : (double)q.logFoldChange)));
                ct_volcano_logFold_logP.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(Math.Ceiling(Sweet.lollipop.qVals.Max(q => plot_selection == 0 ? q.Log2FoldChangeValues.average_log2fc : (double)q.logFoldChange)));
                ct_volcano_logFold_logP.ChartAreas[0].AxisY.Minimum = Math.Floor(Sweet.lollipop.qVals.Min(q => -Math.Log10(q.Log2FoldChangeValues.pValue_uncorrected)));
                ct_volcano_logFold_logP.ChartAreas[0].AxisY.Maximum = Math.Ceiling(Sweet.lollipop.qVals.Max(q => -Math.Log10(q.Log2FoldChangeValues.pValue_uncorrected)));
                ct_volcano_logFold_logP.ChartAreas[0].AxisY.Maximum = ct_volcano_logFold_logP.ChartAreas[0].AxisX.Maximum > 30 ? 30 : ct_volcano_logFold_logP.ChartAreas[0].AxisX.Maximum;
            }
        }

        Point? ct_volcano_logFold_logP_prevPosition = null;
        ToolTip ct_volcano_logFold_logP_tt = new ToolTip();
        private void ct_volcano_logFold_logP_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                DisplayUtility.tooltip_graph_display(ct_volcano_logFold_logP_tt, e, ct_volcano_logFold_logP, ct_volcano_logFold_logP_prevPosition);
        }

        private string[] volcano_selections = new string[]
        {
            "Fold-Change Tests Within Files (NeuCode Labeling)",
        };

        private void cmbx_volcanoChartSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            volcanoPlot();
        }

        private void nud_benjiHochFDR_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.Log2FoldChangeAnalysis.benjiHoch_fdr = (double)nud_benjiHochFDR.Value;
            Sweet.lollipop.Log2FoldChangeAnalysis.establish_benjiHoch_significance();
            volcanoPlot();
        }

        #endregion Volcano Plot Methods

        #region Relative Difference Chart Methods     

        private TusherAnalysis get_tusher_analysis()
        {
            int selection = cmbx_relativeDifferenceChartSelection.SelectedIndex;
            TusherAnalysis tusher = selection < 3 ? Sweet.lollipop.TusherAnalysis1 as TusherAnalysis : Sweet.lollipop.TusherAnalysis2 as TusherAnalysis;
            return tusher;
        }

        private TusherValues get_tusher_values(QuantitativeProteoformValues q)
        {
            int selection = cmbx_relativeDifferenceChartSelection.SelectedIndex;
            TusherValues tusher = selection < 3 ? q.TusherValues1 as TusherValues : q.TusherValues2 as TusherValues;
            return tusher;
        }

        private void plotObservedVsExpectedRelativeDifference()
        {
            ct_relativeDifference.ChartAreas[0].AxisX.IsLogarithmic = false;
            ct_relativeDifference.Series.Clear();
            ct_relativeDifference.Series.Add("Quantified");
            ct_relativeDifference.Series["Quantified"].ChartType = SeriesChartType.Point;
            ct_relativeDifference.Series.Add("Significant");
            ct_relativeDifference.Series["Significant"].ChartType = SeriesChartType.Point;
            ct_relativeDifference.Series.Add("d(i) = dE(i)");
            ct_relativeDifference.Series["d(i) = dE(i)"].ChartType = SeriesChartType.Line;
            ct_relativeDifference.Series.Add("Positive Offset");
            ct_relativeDifference.Series["Positive Offset"].ChartType = SeriesChartType.Line;
            ct_relativeDifference.Series.Add("Negative Offset");
            ct_relativeDifference.Series["Negative Offset"].ChartType = SeriesChartType.Line;
            ct_relativeDifference.ChartAreas[0].AxisX.Title = "Expected Relative Difference dE(i)";
            ct_relativeDifference.ChartAreas[0].AxisY.Title = "Observed Relative Difference d(i)";

            int max_test_stat_unit = 0;
            foreach (ExperimentalProteoform pf in Sweet.lollipop.satisfactoryProteoforms)
            {
                if (get_tusher_values(pf.quant).significant && Sweet.lollipop.significance_by_permutation || pf.quant.Log2FoldChangeValues.significant && Sweet.lollipop.significance_by_log2FC)
                    ct_relativeDifference.Series["Significant"].Points.AddXY(get_tusher_values(pf.quant).correspondingAvgSortedRelDiff, get_tusher_values(pf.quant).relative_difference);
                else
                    ct_relativeDifference.Series["Quantified"].Points.AddXY(get_tusher_values(pf.quant).correspondingAvgSortedRelDiff, get_tusher_values(pf.quant).relative_difference);
                if (Math.Ceiling(Math.Abs(get_tusher_values(pf.quant).correspondingAvgSortedRelDiff)) > max_test_stat_unit)
                    max_test_stat_unit = (int)Math.Ceiling(Math.Abs(get_tusher_values(pf.quant).correspondingAvgSortedRelDiff));
                if (Math.Ceiling(Math.Abs(get_tusher_values(pf.quant).relative_difference)) > max_test_stat_unit)
                    max_test_stat_unit = (int)Math.Ceiling(Math.Abs(get_tusher_values(pf.quant).relative_difference));
            }

            if (get_tusher_analysis().avgSortedPermutationRelativeDifferences.Count > 0 && get_tusher_analysis().sortedProteoformRelativeDifferences.Count > 0)
            {
                //ct_relativeDifference.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(Math.Floor(SaveState.lollipop.sortedAvgPermutationTestStatistics.First()));
                //ct_relativeDifference.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(Math.Ceiling(SaveState.lollipop.sortedAvgPermutationTestStatistics.Last()));
                //ct_relativeDifference.ChartAreas[0].AxisY.Minimum = Math.Min(Convert.ToDouble(Math.Floor(negativeOffsetFunction(SaveState.lollipop.sortedAvgPermutationTestStatistics.First()))), Convert.ToDouble(Math.Floor(SaveState.lollipop.sortedProteoformTestStatistics.First())));
                //ct_relativeDifference.ChartAreas[0].AxisY.Maximum = Math.Max(Convert.ToDouble(Math.Ceiling(positiveOffsetFunction(SaveState.lollipop.sortedAvgPermutationTestStatistics.Last()))), Convert.ToDouble(Math.Ceiling(SaveState.lollipop.sortedProteoformTestStatistics.Last())));
                ct_relativeDifference.ChartAreas[0].AxisX.Minimum = -max_test_stat_unit;
                ct_relativeDifference.ChartAreas[0].AxisX.Maximum = max_test_stat_unit;
                ct_relativeDifference.ChartAreas[0].AxisY.Minimum = -max_test_stat_unit;
                ct_relativeDifference.ChartAreas[0].AxisY.Maximum = max_test_stat_unit;

            }

            plotObservedVsExpectedOffsets();
        }

        private void plotObservedRelativeDifferenceVsScatter()
        {
            int selection = cmbx_relativeDifferenceChartSelection.SelectedIndex;

            ct_relativeDifference.Series.Clear();
            ct_relativeDifference.Series.Add("Quantified");
            ct_relativeDifference.Series["Quantified"].ChartType = SeriesChartType.Point;
            ct_relativeDifference.Series.Add("Significant");
            ct_relativeDifference.Series["Significant"].ChartType = SeriesChartType.Point;
            ct_relativeDifference.ChartAreas[0].AxisX.Title = "Scatter s(i)";
            ct_relativeDifference.ChartAreas[0].AxisY.Title = "Observed Relative Difference d(i)";

            decimal min_scatter = Decimal.MaxValue;
            decimal max_scatter = Decimal.MinValue;
            decimal max_stat = Decimal.MinValue;

            foreach (ExperimentalProteoform pf in Sweet.lollipop.satisfactoryProteoforms)
            {
                if (get_tusher_values(pf.quant).significant && Sweet.lollipop.significance_by_permutation || pf.quant.Log2FoldChangeValues.significant && Sweet.lollipop.significance_by_log2FC)
                    ct_relativeDifference.Series["Significant"].Points.AddXY(get_tusher_values(pf.quant).scatter, get_tusher_values(pf.quant).relative_difference);
                else
                    ct_relativeDifference.Series["Quantified"].Points.AddXY(get_tusher_values(pf.quant).scatter, get_tusher_values(pf.quant).relative_difference);

                if (get_tusher_values(pf.quant).scatter < min_scatter) min_scatter = get_tusher_values(pf.quant).scatter;
                if (get_tusher_values(pf.quant).scatter > max_scatter) max_scatter = get_tusher_values(pf.quant).scatter;
                if (Math.Abs(get_tusher_values(pf.quant).relative_difference) > max_stat) max_stat = Math.Abs(get_tusher_values(pf.quant).relative_difference);
            }

            ct_relativeDifference.ChartAreas[0].AxisX.Minimum = 1;
            ct_relativeDifference.ChartAreas[0].AxisX.IsLogarithmic = true;
            ct_relativeDifference.ChartAreas[0].AxisX.Maximum = Math.Pow(Math.Ceiling(Math.Log10((double)max_scatter)), 10);
            ct_relativeDifference.ChartAreas[0].AxisY.Minimum = -(double)Math.Ceiling(max_stat);
            ct_relativeDifference.ChartAreas[0].AxisY.Maximum = (double)Math.Ceiling(max_stat);
        }

        private void plotAllPermutedTestStatistics()
        {
            ct_relativeDifference.ChartAreas[0].AxisX.IsLogarithmic = false;
            ct_relativeDifference.Series.Clear();
            ct_relativeDifference.Series.Add("Permuted");
            ct_relativeDifference.Series["Permuted"].ChartType = SeriesChartType.Point;
            ct_relativeDifference.Series.Add("Passing Permuted");
            ct_relativeDifference.Series["Passing Permuted"].ChartType = SeriesChartType.Point;
            ct_relativeDifference.ChartAreas[0].AxisX.Title = "Expected Relative Difference dE(i)";
            ct_relativeDifference.ChartAreas[0].AxisY.Title = "Observed Relative Difference d(i)";

            int max_test_stat_unit = 0;
            for (int i = 0; i < get_tusher_analysis().sortedProteoformRelativeDifferences.Count; i++)
            {
                decimal avg = get_tusher_analysis().avgSortedPermutationRelativeDifferences[i];
                foreach (TusherStatistic stat in get_tusher_analysis().sortedPermutedRelativeDifferences.Select(sorted => sorted[i]))
                {
                    if (stat.is_passing_real(get_tusher_analysis().minimumPassingNegativeTestStatistic, get_tusher_analysis().minimumPassingPositiveTestStatisitic, Sweet.lollipop.fold_change_conjunction, Sweet.lollipop.useFoldChangeCutoff, Sweet.lollipop.foldChangeCutoff, Sweet.lollipop.useAveragePermutationFoldChange, Sweet.lollipop.useBiorepPermutationFoldChange, Sweet.lollipop.minBiorepsWithFoldChange))
                    {
                        ct_relativeDifference.Series["Passing Permuted"].Points.AddXY(avg, stat.relative_difference);
                    }
                    else
                    {
                        ct_relativeDifference.Series["Permuted"].Points.AddXY(avg, stat.relative_difference);
                    }

                    if (Math.Ceiling(Math.Abs(avg)) > max_test_stat_unit)
                        max_test_stat_unit = (int)Math.Ceiling(Math.Abs(avg));
                    if (Math.Ceiling(Math.Abs(stat.relative_difference)) > max_test_stat_unit)
                        max_test_stat_unit = (int)Math.Ceiling(Math.Abs(stat.relative_difference));
                }
            }

            if (get_tusher_analysis().avgSortedPermutationRelativeDifferences.Count > 0 && get_tusher_analysis().sortedProteoformRelativeDifferences.Count > 0)
            {
                ct_relativeDifference.ChartAreas[0].AxisX.Minimum = -max_test_stat_unit;
                ct_relativeDifference.ChartAreas[0].AxisX.Maximum = max_test_stat_unit;
                ct_relativeDifference.ChartAreas[0].AxisY.Minimum = -max_test_stat_unit;
                ct_relativeDifference.ChartAreas[0].AxisY.Maximum = max_test_stat_unit;
            }
        }

        private void plots()
        {
            volcanoPlot();
            if (cb_useLocalFdrCutoff.Checked)
                plotObservedRelativeDifferenceVsFdr();
            else if (new int[] { 0, 3 }.Contains(cmbx_relativeDifferenceChartSelection.SelectedIndex))
                plotObservedVsExpectedRelativeDifference();
            else if (new int[] { 1, 4 }.Contains(cmbx_relativeDifferenceChartSelection.SelectedIndex))
                plotObservedRelativeDifferenceVsScatter();
            else if (new int[] { 2, 5 }.Contains(cmbx_relativeDifferenceChartSelection.SelectedIndex))
                plotAllPermutedTestStatistics();
        }

        private void cmbx_relativeDifferenceChartSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            get_tusher_analysis().reestablishSignficance(get_go_analysis());
            plots();
            plotBiorepIntensities();
            tb_FDR.Text = Math.Round(get_tusher_analysis().relativeDifferenceFDR, 4).ToString();
        }

        private void plotObservedRelativeDifferenceVsFdr()
        {
            int selection = cmbx_relativeDifferenceChartSelection.SelectedIndex;

            ct_relativeDifference.ChartAreas[0].AxisX.IsLogarithmic = false;
            ct_relativeDifference.Series.Clear();
            ct_relativeDifference.Series.Add("Quantified");
            ct_relativeDifference.Series["Quantified"].ChartType = SeriesChartType.Point;
            ct_relativeDifference.Series.Add("Significant");
            ct_relativeDifference.Series["Significant"].ChartType = SeriesChartType.Point;
            ct_relativeDifference.ChartAreas[0].AxisX.Title = "Local FDR";
            ct_relativeDifference.ChartAreas[0].AxisY.Title = "Observed Relative Difference d(i)";

            decimal min_fdr = Decimal.MaxValue;
            decimal max_fdr = Decimal.MinValue;
            decimal min_stat = Decimal.MaxValue;
            decimal max_stat = Decimal.MinValue;

            foreach (ExperimentalProteoform pf in Sweet.lollipop.satisfactoryProteoforms)
            {
                decimal rel_diff = get_tusher_values(pf.quant).relative_difference;
                if (get_tusher_values(pf.quant).significant && Sweet.lollipop.significance_by_permutation || pf.quant.Log2FoldChangeValues.significant && Sweet.lollipop.significance_by_log2FC)
                    ct_relativeDifference.Series["Significant"].Points.AddXY(get_tusher_values(pf.quant).roughSignificanceFDR, rel_diff);
                else
                    ct_relativeDifference.Series["Quantified"].Points.AddXY(get_tusher_values(pf.quant).roughSignificanceFDR, rel_diff);

                if (get_tusher_values(pf.quant).roughSignificanceFDR < min_fdr) min_fdr = get_tusher_values(pf.quant).roughSignificanceFDR;
                if (get_tusher_values(pf.quant).roughSignificanceFDR > max_fdr) max_fdr = get_tusher_values(pf.quant).roughSignificanceFDR;
                if (get_tusher_values(pf.quant).relative_difference < min_stat) min_stat = rel_diff;
                if (get_tusher_values(pf.quant).relative_difference > max_stat) max_stat = rel_diff;
            }

            ct_relativeDifference.ChartAreas[0].AxisX.Minimum = (double)Math.Floor(min_fdr);
            ct_relativeDifference.ChartAreas[0].AxisX.Maximum = (double)Math.Ceiling(max_fdr);
            ct_relativeDifference.ChartAreas[0].AxisY.Minimum = (double)Math.Floor(min_stat);
            ct_relativeDifference.ChartAreas[0].AxisY.Maximum = (double)Math.Ceiling(max_stat);
        }

        private void plotObservedVsExpectedOffsets()
        {
            ct_relativeDifference.Series["d(i) = dE(i)"].Points.Clear();
            ct_relativeDifference.Series["Positive Offset"].Points.Clear();
            ct_relativeDifference.Series["Negative Offset"].Points.Clear();

            for (double xvalue = ct_relativeDifference.ChartAreas[0].AxisX.Minimum; xvalue <= ct_relativeDifference.ChartAreas[0].AxisX.Maximum; xvalue += (ct_relativeDifference.ChartAreas[0].AxisX.Maximum - ct_relativeDifference.ChartAreas[0].AxisX.Minimum) / 100d)
            {
                ct_relativeDifference.Series["d(i) = dE(i)"].Points.AddXY(xvalue, xvalue);
                ct_relativeDifference.Series["Positive Offset"].Points.AddXY(xvalue, positiveOffsetFunction(xvalue));
                ct_relativeDifference.Series["Negative Offset"].Points.AddXY(xvalue, negativeOffsetFunction(xvalue));
            }

            tb_FDR.Text = Math.Round(get_tusher_analysis().relativeDifferenceFDR, 4).ToString();
        }

        private void nud_Offset_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.offsetTestStatistics = nud_Offset.Value;
            if (Sweet.lollipop.satisfactoryProteoforms.Count <= 0)
                return;
            get_tusher_analysis().reestablishSignficance(get_go_analysis());
            plots();
            tb_FDR.Text = Math.Round(get_tusher_analysis().relativeDifferenceFDR, 4).ToString();
        }

        private double positiveOffsetFunction(double x)
        {
            return (x + (double)nud_Offset.Value);
        }

        private double negativeOffsetFunction(double x)
        {
            return (x - (double)nud_Offset.Value);
        }

        Point? ct_relativeDifference_prevPosition = null;
        ToolTip ct_relativeDifference_tt = new ToolTip();
        private void ct_relativeDifference_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                DisplayUtility.tooltip_graph_display(ct_relativeDifference_tt, e, ct_relativeDifference, ct_relativeDifference_prevPosition);
        }

        private void cb_useLocalFdrCutoff_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.useLocalFdrCutoff = cb_useLocalFdrCutoff.Checked;
            get_tusher_analysis().reestablishSignficance(get_go_analysis());
            plots();
            nud_Offset.Enabled = !cb_useLocalFdrCutoff.Checked;
            nud_localFdrCutoff.Enabled = cb_useLocalFdrCutoff.Checked;
        }

        private void nud_localFdrCutoff_ValueChanged(object sender, EventArgs e)
        {
            int selection = cmbx_relativeDifferenceChartSelection.SelectedIndex;
            TusherAnalysis tusher_analysis = selection < 3 ? Sweet.lollipop.TusherAnalysis1 as TusherAnalysis : Sweet.lollipop.TusherAnalysis2 as TusherAnalysis;

            Sweet.lollipop.localFdrCutoff = nud_localFdrCutoff.Value;
            tusher_analysis.reestablishSignficance(get_go_analysis());
            if (cb_useLocalFdrCutoff.Checked)
                plotObservedRelativeDifferenceVsFdr();
        }

        #endregion Relative Difference Chart Methods

        #region Significance Checkbox Methods

        private void cb_significanceByPermutation_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.significance_by_permutation = cb_significanceByPermutation.Checked;
            if (cb_significanceByPermutation.Checked)
            {
                cb_significanceByFoldChange.Checked = !cb_significanceByPermutation.Checked;
                plots();
            }
        }

        private void cb_significanceByFoldChange_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.significance_by_log2FC = cb_significanceByFoldChange.Checked;
            if (cb_significanceByFoldChange.Checked)
            {
                cb_significanceByPermutation.Checked = !cb_significanceByFoldChange.Checked;
                plots();
            }
        }

        #endregion Significance Checkbox Methods

        #region Permutation Fold Change Cutoff Methods

        private void cb_useFoldChangeCutoff_CheckedChanged(object sender, EventArgs e)
        {
            nud_foldChangeCutoff.Enabled = cb_useFoldChangeCutoff.Checked;
            cb_useAveragePermutationFoldChange.Enabled = cb_useFoldChangeCutoff.Checked;
            cb_useBiorepPermutationFoldChange.Enabled = cb_useFoldChangeCutoff.Checked;
            Sweet.lollipop.useFoldChangeCutoff = cb_useFoldChangeCutoff.Checked;
            get_tusher_analysis().reestablishSignficance(get_go_analysis());
            plots();
        }

    
        private void nud_permutationFoldChangeCutoff_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.foldChangeCutoff = nud_foldChangeCutoff.Value;
            get_tusher_analysis().reestablishSignficance(get_go_analysis());
            plots();
        }

        private void nud_foldChangeObservations_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.minBiorepsWithFoldChange = Convert.ToInt32(nud_foldChangeObservations.Value);
            get_tusher_analysis().reestablishSignficance(get_go_analysis());
            plots();
        }

        private void cmbx_foldChangeConjunction_SelectedIndexChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.fold_change_conjunction = cmbx_foldChangeConjunction.SelectedItem.ToString();
            get_tusher_analysis().reestablishSignficance(get_go_analysis());
            plots();
        }

        private void cb_useAveragePermutationFoldChange_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_useAveragePermutationFoldChange.Checked)
            {
                cb_useBiorepPermutationFoldChange.Checked = !cb_useAveragePermutationFoldChange.Checked;
                Sweet.lollipop.useBiorepPermutationFoldChange = !cb_useAveragePermutationFoldChange.Checked;
                Sweet.lollipop.useAveragePermutationFoldChange = cb_useAveragePermutationFoldChange.Checked;
                get_tusher_analysis().reestablishSignficance(get_go_analysis());
                plots();
            }
        }

        private void cb_useBiorepPermutationFoldChange_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_useBiorepPermutationFoldChange.Checked)
            {
                cb_useAveragePermutationFoldChange.Checked = !cb_useBiorepPermutationFoldChange.Checked;
                Sweet.lollipop.useAveragePermutationFoldChange = !cb_useBiorepPermutationFoldChange.Checked;
                Sweet.lollipop.useBiorepPermutationFoldChange = cb_useBiorepPermutationFoldChange.Checked;
                get_tusher_analysis().reestablishSignficance(get_go_analysis());
                plots();
            }
        }

        #endregion Permutation Fold Change Cutoff Methods

        #region Biorep Intensities Plot Methods

        private void plotBiorepIntensitiesEvent(object s, EventArgs e)
        {
            plotBiorepIntensities();
        }

        private string[] biorepintensity_selections = new string[]
        {
            "All Observed (Log2 Intensities)", //0
            "Before Imputation (Log2 Intensities)", //1
            "Projected (Log2 Intensities)", //2
            "After Imputation (Log2 Intensities)", //3
            "All Linear Intensities", //4
            "Selected Linear Intensities" //4
        };

        private void plotBiorepIntensities()
        {
            int selection = cmbx_intensityDistributionChartSelection.SelectedIndex;
            int tusher_selection = cmbx_relativeDifferenceChartSelection.SelectedIndex;

            ct_proteoformIntensities.Series.Clear();
            ct_proteoformIntensities.Series.Add("Intensities");
            ct_proteoformIntensities.Series["Intensities"].ChartType = SeriesChartType.Point; // these are the actual experimental proteoform intensities
            if (new int[] { 4, 5 }.Contains(selection))
            {
                ct_proteoformIntensities.ChartAreas[0].AxisX.Title = "Avg. Intensity, " + Sweet.lollipop.numerator_condition;
                ct_proteoformIntensities.ChartAreas[0].AxisY.Title = "Avg. Intensity, " + Sweet.lollipop.denominator_condition;

                List<ExperimentalProteoform> pfs = (selection == 4 ? Sweet.lollipop.target_proteoform_community.experimental_proteoforms.ToList() : Sweet.lollipop.satisfactoryProteoforms)
                    .Where(pf => tusher_selection < 3 ?
                        pf.quant.TusherValues1.numeratorOriginalIntensities != null && pf.quant.TusherValues1.denominatorOriginalIntensities != null :
                        pf.quant.TusherValues2.numeratorOriginalIntensities != null && pf.quant.TusherValues2.denominatorOriginalIntensities != null
                        ).ToList();

                foreach (ExperimentalProteoform pf in pfs)
                {
                    int yeah = tusher_selection < 3 ?
                        ct_proteoformIntensities.Series["Intensities"].Points.AddXY(pf.quant.TusherValues1.numeratorOriginalIntensities == null || pf.quant.TusherValues1.numeratorOriginalIntensities.Count == 0 ? 0 : pf.quant.TusherValues1.numeratorOriginalIntensities.Average(x => x.intensity_sum), pf.quant.TusherValues1.denominatorOriginalIntensities == null || pf.quant.TusherValues1.denominatorOriginalIntensities.Count == 0 ? 0 : pf.quant.TusherValues1.denominatorOriginalIntensities.Average(x => x.intensity_sum)) :
                        ct_proteoformIntensities.Series["Intensities"].Points.AddXY(pf.quant.TusherValues2.numeratorOriginalIntensities == null || pf.quant.TusherValues2.numeratorOriginalIntensities.Count == 0 ? 0 : pf.quant.TusherValues2.numeratorOriginalIntensities.Average(x => x.intensity_sum), pf.quant.TusherValues2.denominatorOriginalIntensities == null || pf.quant.TusherValues2.denominatorOriginalIntensities.Count == 0 ? 0 : pf.quant.TusherValues2.denominatorOriginalIntensities.Average(x => x.intensity_sum));
                }
                return;
            }

            ct_proteoformIntensities.Series.Add("Fit");
            ct_proteoformIntensities.Series["Fit"].ChartType = SeriesChartType.Line; // this is a gaussian best fit to the experimental proteoform intensities.
            if (selection == 2)
            {
                ct_proteoformIntensities.Series.Add("Bkgd. Projected");
                ct_proteoformIntensities.Series["Bkgd. Projected"].ChartType = SeriesChartType.Line; // this is a gaussian line representing the distribution of missing values.
                ct_proteoformIntensities.Series.Add("Fit + Projected");
                ct_proteoformIntensities.Series["Fit + Projected"].ChartType = SeriesChartType.Line; // this is the sum of the gaussians for observed and missing values
            }
            ct_proteoformIntensities.ChartAreas[0].AxisX.Title = "Log (Base 2) Intensity";
            ct_proteoformIntensities.ChartAreas[0].AxisY.Title = "Count";

            foreach (KeyValuePair<decimal, int> entry in selection == 3 ?
                get_tusher_analysis().QuantitativeDistributions.logSelectIntensityWithImputationHistogram : 
                new int[] { 1, 2 }.Contains(selection) ? get_tusher_analysis().QuantitativeDistributions.logSelectIntensityHistogram :
                    get_tusher_analysis().QuantitativeDistributions.logIntensityHistogram)
            {
                ct_proteoformIntensities.Series["Intensities"].Points.AddXY(entry.Key, entry.Value);

                decimal gaussian_height = selection == 3 ?
                    get_tusher_analysis().QuantitativeDistributions.selectWithImputationGaussianHeight :
                    new int[] { 1, 2 }.Contains(selection) ? get_tusher_analysis().QuantitativeDistributions.selectWithImputationGaussianHeight :
                        get_tusher_analysis().QuantitativeDistributions.allObservedGaussianHeight;
                decimal average_intensity = selection == 3 ?
                    get_tusher_analysis().QuantitativeDistributions.selectWithImputationAverageIntensity :
                    new int[] { 1, 2 }.Contains(selection) ? get_tusher_analysis().QuantitativeDistributions.selectWithImputationAverageIntensity :
                        get_tusher_analysis().QuantitativeDistributions.allObservedAverageIntensity;
                decimal std_dev = selection == 3 ?
                    get_tusher_analysis().QuantitativeDistributions.selectWithImputationStDev :
                    new int[] { 1, 2 }.Contains(selection) ? get_tusher_analysis().QuantitativeDistributions.selectWithImputationStDev :
                        get_tusher_analysis().QuantitativeDistributions.allObservedStDev;
                double gaussIntensity = ((double)gaussian_height) * Math.Exp(-Math.Pow(((double)entry.Key - (double)average_intensity), 2) / (2d * Math.Pow((double)std_dev, 2)));
                double bkgd_gaussIntensity = ((double)get_tusher_analysis().QuantitativeDistributions.bkgdGaussianHeight) * Math.Exp(-Math.Pow(((double)entry.Key - (double)get_tusher_analysis().QuantitativeDistributions.bkgdAverageIntensity), 2) / (2d * Math.Pow((double)get_tusher_analysis().QuantitativeDistributions.bkgdStDev, 2)));
                double sumIntensity = gaussIntensity + bkgd_gaussIntensity;
                ct_proteoformIntensities.Series["Fit"].Points.AddXY(entry.Key, gaussIntensity);

                if (selection == 2)
                {
                    ct_proteoformIntensities.Series["Bkgd. Projected"].Points.AddXY(entry.Key, bkgd_gaussIntensity);
                    ct_proteoformIntensities.Series["Fit + Projected"].Points.AddXY(entry.Key, sumIntensity);
                }
            }
        }

        private void cmbx_intensityDistributionChartSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            plotBiorepIntensities();
        }

        Point? ct_proteoformIntensities_prevPosition = null;
        ToolTip ct_proteoformIntensities_tt = new ToolTip();
        private void ct_proteoformIntensities_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                DisplayUtility.tooltip_graph_display(ct_proteoformIntensities_tt, e, ct_proteoformIntensities, ct_proteoformIntensities_prevPosition);
        }

        #endregion Biorep Intensities Plot Methods

        #region GO-Term Analysis Private Methods

        private IGoAnalysis get_go_analysis()
        {
            int selection = cmbx_relativeDifferenceChartSelection.SelectedIndex;
            IGoAnalysis go = Sweet.lollipop.significance_by_log2FC ?
                Sweet.lollipop.Log2FoldChangeAnalysis as IGoAnalysis :
                selection < 3 ? Sweet.lollipop.TusherAnalysis1 as IGoAnalysis : Sweet.lollipop.TusherAnalysis2 as IGoAnalysis;
            return go;
        }

        private void fillGoTermsTable()
        {
            IGoAnalysis gg = get_go_analysis();
            DisplayUtility.FillDataGridView(dgv_goAnalysis, gg.GoAnalysis.goTermNumbers.Where(x => x.Aspect.ToString() == cmbx_goAspect.SelectedItem.ToString()).Select(g => new DisplayGoTermNumber(g)));
            DisplayGoTermNumber.FormatGridView(dgv_goAnalysis);
        }

        private void updateGoTermsTable(object s, EventArgs e)
        {
            IGoAnalysis g = get_go_analysis();
            g.GoAnalysis.maxGoTermFDR = nud_FDR.Value;
            g.GoAnalysis.minProteoformFoldChange = nud_ratio.Value;
            g.GoAnalysis.minProteoformIntensity = nud_intensity.Value;
            g.inducedOrRepressedProteins = Sweet.lollipop.getInducedOrRepressedProteins(get_tusher_analysis() as TusherAnalysis, Sweet.lollipop.satisfactoryProteoforms, g.GoAnalysis.minProteoformFoldChange, g.GoAnalysis.maxGoTermFDR, g.GoAnalysis.minProteoformIntensity);
            g.GoAnalysis.GO_analysis(g.inducedOrRepressedProteins);
            fillGoTermsTable();
        }

        private void updateGoTermsTable()
        {
            IGoAnalysis g = get_go_analysis();
            g.GoAnalysis.maxGoTermFDR = nud_FDR.Value;
            g.GoAnalysis.minProteoformFoldChange = nud_ratio.Value;
            g.GoAnalysis.minProteoformIntensity = nud_intensity.Value;
            g.inducedOrRepressedProteins = Sweet.lollipop.getInducedOrRepressedProteins(get_tusher_analysis() as TusherAnalysis, Sweet.lollipop.satisfactoryProteoforms, g.GoAnalysis.minProteoformFoldChange, g.GoAnalysis.maxGoTermFDR, g.GoAnalysis.minProteoformIntensity);
            g.GoAnalysis.GO_analysis(g.inducedOrRepressedProteins);
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
                get_go_analysis().GoAnalysis.GO_analysis(get_go_analysis().inducedOrRepressedProteins);
                fillGoTermsTable();
            }
            backgroundUpdated = true;
        }

        private void rb_quantifiedSampleSet_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_quantifiedSampleSet.Checked)
            {
                get_go_analysis().GoAnalysis.backgroundProteinsList = "";
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
            get_go_analysis().GoAnalysis.allDetectedProteins = rb_detectedSampleSet.Checked;
            if (rb_detectedSampleSet.Checked)
            {
                get_go_analysis().GoAnalysis.backgroundProteinsList = "";
                tb_goTermCustomBackground.Text = "";
                backgroundUpdated = false;
            }
        }

        private void rb_allTheoreticalProteins_CheckedChanged(object sender, EventArgs e)
        {
            get_go_analysis().GoAnalysis.allTheoreticalProteins = rb_allTheoreticalProteins.Checked;
            if (rb_allTheoreticalProteins.Checked)
            {
                get_go_analysis().GoAnalysis.backgroundProteinsList = "";
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
                get_go_analysis().GoAnalysis.backgroundProteinsList = fileOpen.FileName;
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
            Sweet.lollipop.family_build_folder_path = path;
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
            string time_stamp = Sweet.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            string message = CytoscapeScript.write_cytoscape_script(Sweet.lollipop.target_proteoform_community.families, Sweet.lollipop.target_proteoform_community.families,
                Sweet.lollipop.family_build_folder_path, "", time_stamp, get_go_analysis(), cb_redBorder.Checked, cb_boldLabel.Checked,
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), Sweet.lollipop.deltaM_edge_display_rounding,
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void btn_buildFamiliesWithSignificantChange_Click(object sender, EventArgs e)
        {
            List<ProteoformFamily> families = Sweet.lollipop.getInterestingFamilies(get_tusher_analysis() as TusherAnalysis, Sweet.lollipop.satisfactoryProteoforms, get_go_analysis().GoAnalysis.minProteoformFoldChange, get_go_analysis().GoAnalysis.maxGoTermFDR, get_go_analysis().GoAnalysis.minProteoformIntensity).Distinct().ToList();
            string time_stamp = Sweet.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            string message = CytoscapeScript.write_cytoscape_script(families, Sweet.lollipop.target_proteoform_community.families,
                Sweet.lollipop.family_build_folder_path, "", time_stamp, get_go_analysis(), cb_redBorder.Checked, cb_boldLabel.Checked,
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), Sweet.lollipop.deltaM_edge_display_rounding,
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void btn_buildSelectedQuantFamilies_Click(object sender, EventArgs e)
        {
            string time_stamp = Sweet.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            object[] selected = DisplayUtility.get_selected_objects(dgv_quantification_results);
            string message = CytoscapeScript.write_cytoscape_script(selected, Sweet.lollipop.target_proteoform_community.families,
                Sweet.lollipop.family_build_folder_path, "", time_stamp, get_go_analysis(), cb_redBorder.Checked, cb_boldLabel.Checked,
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), Sweet.lollipop.deltaM_edge_display_rounding,
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void btn_buildFamiliesAllGO_Click(object sender, EventArgs e)
        {
            Aspect a = (Aspect)cmbx_goAspect.SelectedItem;
            List<ProteoformFamily> go_families = Sweet.lollipop.getInterestingFamilies(get_go_analysis().GoAnalysis.goTermNumbers.Where(n => n.Aspect == a).Distinct().ToList(), Sweet.lollipop.target_proteoform_community.families);
            string time_stamp = Sweet.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            string message = CytoscapeScript.write_cytoscape_script(go_families, Sweet.lollipop.target_proteoform_community.families,
                Sweet.lollipop.family_build_folder_path, "", time_stamp, get_go_analysis(), cb_redBorder.Checked, cb_boldLabel.Checked, 
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), Sweet.lollipop.deltaM_edge_display_rounding,
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void btn_buildFromSelectedGoTerms_Click(object sender, EventArgs e)
        {
            List<GoTermNumber> selected_gos = (DisplayUtility.get_selected_objects(dgv_goAnalysis).Select(o => (GoTermNumber)o)).ToList();
            List<ProteoformFamily> selected_families = Sweet.lollipop.getInterestingFamilies(selected_gos, Sweet.lollipop.target_proteoform_community.families).Distinct().ToList();
            string time_stamp = Sweet.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            string message = CytoscapeScript.write_cytoscape_script(selected_families, Sweet.lollipop.target_proteoform_community.families,
                Sweet.lollipop.family_build_folder_path, "", time_stamp, get_go_analysis(), cb_redBorder.Checked, cb_boldLabel.Checked,
                cmbx_colorScheme.SelectedItem.ToString(), cmbx_edgeLabel.SelectedItem.ToString(), cmbx_nodeLabel.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), Sweet.lollipop.deltaM_edge_display_rounding,
                cb_geneCentric.Checked, cmbx_geneLabel.SelectedItem.ToString());
            MessageBox.Show(message, "Cytoscape Build");
        }

        private void cmbx_geneLabel_SelectedIndexChanged(object sender, EventArgs e)
        {
            Lollipop.preferred_gene_label = cmbx_geneLabel.SelectedItem.ToString();
        }

        private void cb_geneCentric_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.gene_centric_families = cb_geneCentric.Checked;
        }

        private void cmbx_empty_TextChanged(object sender, EventArgs e) { }

        #endregion Cytoscape Visualization Private Methods

    }
}