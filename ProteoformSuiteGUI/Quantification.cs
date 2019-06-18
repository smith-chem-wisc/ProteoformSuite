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
        }

        #endregion Constructor

        #region Private Fields

        private TusherAnalysis selected_tusher_analysis;
        private IGoAnalysis selected_analysis;

        #endregion Private Fields

        #region Public Property

        public List<DataTable> DataTables { get; private set; }

        #endregion Public Property

        #region Public Methods

        public List<DataTable> SetTables()
        {
            //if (selected_tusher_analysis == null) return new List<DataTable>();

            string suffix = selected_analysis as TusherAnalysis1 != null ?
                 Sweet.lollipop.TusherAnalysis1.sortedPermutedRelativeDifferences.Count.ToString() + "Perm" :
                 selected_analysis as TusherAnalysis2 != null ?
                 Sweet.lollipop.TusherAnalysis2.sortedPermutedRelativeDifferences.Count.ToString() + "Perm" :
                "Log2FC";

            DataTables = new List<DataTable>
            {
                DisplayQuantitativeValues.FormatGridView(Sweet.lollipop.qVals.Select(x => new DisplayQuantitativeValues(x, selected_analysis)).ToList(), "QuantValues_" + suffix)
            };

            //Results dataframe for selected tusher analysis
            DataTable results;
            results = ResultsSummaryGenerator.experimental_results_dataframe(selected_tusher_analysis);
            results.TableName = "Results_" + selected_tusher_analysis.sortedPermutedRelativeDifferences.Count.ToString() + "Perm";
            DataTables.Add(results);

            // Satisfactory proteoforms with and without imputation
            IEnumerable<ExperimentalProteoform> proteoforms;
            proteoforms = Sweet.lollipop.satisfactoryProteoforms as IEnumerable<ExperimentalProteoform>;
            results = ResultsSummaryGenerator.biological_replicate_intensities(selected_analysis, proteoforms, Sweet.lollipop.input_files, Sweet.lollipop.conditionsBioReps, true, false);
            results.TableName = "SelectWithImputation" + suffix;
            DataTables.Add(results);
            results = ResultsSummaryGenerator.biological_replicate_intensities(selected_analysis, proteoforms, Sweet.lollipop.input_files, Sweet.lollipop.conditionsBioReps, false, true);
            results.TableName = "SelectNoImputationNorm" + suffix;
            DataTables.Add(results);
            if (selected_analysis as Log2FoldChangeAnalysis != null)
            {
                results = ResultsSummaryGenerator.biological_replicate_intensities(selected_analysis, proteoforms, Sweet.lollipop.input_files, Sweet.lollipop.conditionsBioReps, false, false);
                results.TableName = "SelectNoImputation" + suffix;
                DataTables.Add(results);
            }
            // All proteoforms with and without imputation
            proteoforms = Sweet.lollipop.target_proteoform_community.experimental_proteoforms as IEnumerable<ExperimentalProteoform>;
            results = ResultsSummaryGenerator.biological_replicate_intensities(selected_analysis, proteoforms, Sweet.lollipop.input_files, Sweet.lollipop.conditionsBioReps, true, false);
            results.TableName = "AllWithImputation" + suffix;
            DataTables.Add(results);
            results = ResultsSummaryGenerator.biological_replicate_intensities(selected_analysis, proteoforms, Sweet.lollipop.input_files, Sweet.lollipop.conditionsBioReps, false, true);
            results.TableName = "AllNoImputationNorm" + suffix;
            DataTables.Add(results);
            if (selected_analysis as Log2FoldChangeAnalysis != null)
            {
                results = ResultsSummaryGenerator.biological_replicate_intensities(selected_analysis, proteoforms, Sweet.lollipop.input_files, Sweet.lollipop.conditionsBioReps, false, false);
                results.TableName = "AllNoImputation" + suffix;
                DataTables.Add(results);
            }
            // Go term table
            results = DisplayGoTermNumber.FormatGridView(selected_analysis.GoAnalysis.goTermNumbers.OrderBy(x => x.by).Select(x => new DisplayGoTermNumber(x)).ToList(), "GoTerms");
            DataTables.Add(results);

            return DataTables;
        }

        public void RunTheGamut(bool full_run)
        {
            ClearListsTablesFigures(true);
            Sweet.lollipop.quantify();
            Sweet.lollipop.TusherAnalysis1.GoAnalysis.GO_analysis(Sweet.lollipop.TusherAnalysis1.inducedOrRepressedProteins);
            Sweet.lollipop.TusherAnalysis2.GoAnalysis.GO_analysis(Sweet.lollipop.TusherAnalysis2.inducedOrRepressedProteins);
            Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis.GO_analysis(Sweet.lollipop.Log2FoldChangeAnalysis.inducedOrRepressedProteins);
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
            if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Count() == 0) return false;
            if (Sweet.lollipop.target_proteoform_community.families.Count == 0) return false;
            if (Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Where(f => f.purpose == Purpose.Quantification).Any(f => f.fraction == "" || f.biological_replicate == "" || f.technical_replicate == "" || f.lt_condition == "")) return false;
            if (Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Where(f => f.purpose == Purpose.Quantification).Select(f => f.lt_condition).Concat(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Select(f => Sweet.lollipop.neucode_labeled ? f.hv_condition : f.lt_condition)).Distinct().Count() != 2) return false;
            if (!Sweet.lollipop.neucode_labeled && !Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Select(f => f.lt_condition).Distinct().All(c => Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification && f.lt_condition == c).Select(f => f.biological_replicate + f.fraction + f.technical_replicate).Distinct().All(d => Sweet.lollipop.input_files.Where(f2 => f2.purpose == Purpose.Quantification && f2.lt_condition != c).
           Select(f2 => f2.biological_replicate + f2.fraction + f2.technical_replicate).Distinct().ToList().Contains(d)))) return false;
            if (!Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Select(f => f.lt_condition).Concat(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Select(f => Sweet.lollipop.neucode_labeled ? f.hv_condition : f.lt_condition)).Distinct().All(c => Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification && f.lt_condition == c || f.hv_condition == c).Select(f => f.biological_replicate).Distinct().Count() >= 2)) return false;
            if (Sweet.lollipop.raw_quantification_components.Count == 0) return false;
            return true;
        }

        public void FillTablesAndCharts()
        {
            plots();
            fill_quantitative_values_table();
            TusherAnalysis analysis = get_tusher_analysis();
            tb_avgIntensity.Text = Math.Round(analysis.QuantitativeDistributions.selectAverageIntensity, 1).ToString();
            tb_stdevIntensity.Text = Math.Round(analysis.QuantitativeDistributions.selectStDev, 3).ToString();
            tb_FDR.Text = Math.Round(analysis.relativeDifferenceFDR, 4).ToString();
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
            cmbx_relativeDifferenceChartSelection.SelectedIndexChanged -= cmbx_relativeDifferenceChartSelection_SelectedIndexChanged;
            cmbx_relativeDifferenceChartSelection.SelectedIndex = selection;
            cmbx_relativeDifferenceChartSelection.SelectedIndexChanged += cmbx_relativeDifferenceChartSelection_SelectedIndexChanged;
        }

        public void InitializeConditionsParameters()
        {
            cmbx_ratioNumerator.Items.Clear();
            cmbx_ratioDenominator.Items.Clear();
            cmbx_inducedCondition.Items.Clear();

            //Initialize conditions -- need to do after files entered
            List<string> conditions = Sweet.lollipop.ltConditionsBioReps.Keys.ToList();
            if (conditions.Count > 0)
            {
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
            }

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
        }

        public void InitializeParameterSet()
        {
            nud_FDR.ValueChanged -= new EventHandler(updateGoTermsTable);
            nud_FDR.Value = get_go_analysis().GoAnalysis.maxGoTermFDR;
            nud_FDR.ValueChanged += new EventHandler(updateGoTermsTable);

            nud_ratio.ValueChanged -= new EventHandler(updateGoTermsTable);
            nud_ratio.Value = get_go_analysis().GoAnalysis.minProteoformFoldChange;
            nud_ratio.ValueChanged += new EventHandler(updateGoTermsTable);

            nud_intensity.ValueChanged -= new EventHandler(updateGoTermsTable);
            nud_intensity.Value = get_go_analysis().GoAnalysis.minProteoformIntensity;
            nud_intensity.ValueChanged += new EventHandler(updateGoTermsTable);

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
            cmbx_relativeDifferenceChartSelection.Items.Clear();
            cmbx_relativeDifferenceChartSelection.Items.AddRange(relative_difference_selections);
            cmbx_relativeDifferenceChartSelection.SelectedIndexChanged -= cmbx_relativeDifferenceChartSelection_SelectedIndexChanged;
            cmbx_relativeDifferenceChartSelection.SelectedIndex = 3; //start with obs vs exp
            cmbx_relativeDifferenceChartSelection.SelectedIndexChanged += cmbx_relativeDifferenceChartSelection_SelectedIndexChanged;

            cmbx_intensityDistributionChartSelection.Items.Clear();
            cmbx_intensityDistributionChartSelection.Items.AddRange(biorepintensity_selections);
            cmbx_intensityDistributionChartSelection.SelectedIndexChanged -= cmbx_relativeDifferenceChartSelection_SelectedIndexChanged;
            cmbx_intensityDistributionChartSelection.SelectedIndex = 3; //start with projected
            cmbx_intensityDistributionChartSelection.SelectedIndexChanged += cmbx_relativeDifferenceChartSelection_SelectedIndexChanged;

            cmbx_quantitativeValuesTableSelection.Items.Clear();
            cmbx_quantitativeValuesTableSelection.Items.AddRange(quantitative_table_selections);
            cmbx_quantitativeValuesTableSelection.SelectedIndexChanged -= cmbx_quantitativeValuesTableSelection_SelectedIndexChanged;
            cmbx_quantitativeValuesTableSelection.SelectedIndex = 0;
            cmbx_quantitativeValuesTableSelection.SelectedIndexChanged += cmbx_quantitativeValuesTableSelection_SelectedIndexChanged;

            //Set parameters
            rb_significanceByFoldChange.CheckedChanged -= cb_significanceByFoldChange_CheckedChanged;
            rb_significanceByFoldChange.Checked = Sweet.lollipop.significance_by_log2FC;
            rb_significanceByFoldChange.CheckedChanged += cb_significanceByFoldChange_CheckedChanged;

            rb_signficanceByPermutation.CheckedChanged -= cb_significanceByPermutation_CheckedChanged;
            rb_signficanceByPermutation.Checked = Sweet.lollipop.significance_by_permutation;
            rb_signficanceByPermutation.CheckedChanged += cb_significanceByPermutation_CheckedChanged;

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

            nUD_min_fold_change.ValueChanged -= nUD_min_fold_change_ValueChanged;
            nUD_min_fold_change.Value = (decimal)Sweet.lollipop.Log2FoldChangeAnalysis.minFoldChange;
            nUD_min_fold_change.ValueChanged += nUD_min_fold_change_ValueChanged;

            nud_bkgdShift.ValueChanged -= nud_bkgdShift_ValueChanged;
            nud_bkgdShift.Value = Sweet.lollipop.backgroundShift;
            nud_bkgdShift.ValueChanged += nud_bkgdShift_ValueChanged;
            nud_bkgdShift.ValueChanged += new EventHandler(plotBiorepIntensitiesEvent);

            nud_bkgdWidth.ValueChanged -= nud_bkgdWidth_ValueChanged;
            nud_bkgdWidth.Value = Sweet.lollipop.backgroundWidth;
            nud_bkgdWidth.ValueChanged += nud_bkgdWidth_ValueChanged;
            nud_bkgdWidth.ValueChanged += new EventHandler(plotBiorepIntensitiesEvent);

            cmbx_foldChangeConjunction.Items.Clear();
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

            cmbx_goAspect.Items.Clear();
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

            cb_useRandomSeed.Checked = Sweet.lollipop.useRandomSeed_quant;
            nud_foldChangeCutoff.Enabled = Sweet.lollipop.useFoldChangeCutoff;
            nud_randomSeed.Enabled = Sweet.lollipop.useRandomSeed_quant;

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
                RunTheGamut(false);
                Cursor = Cursors.Default;
            }
            else if (Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Count() <= 0)
                MessageBox.Show("Please load quantification results in Load Deconvolution Results.", "Quantification");
            else if (Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Where(f => f.purpose == Purpose.Quantification).Any(f => f.fraction == "" || f.biological_replicate == "" || f.technical_replicate == "" || f.lt_condition == ""))
                MessageBox.Show("Please be sure the technical replicate, fraction, biological replicate, and condition are labeled for each quantification file.");
            else if (Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Where(f => f.purpose == Purpose.Quantification).Select(f => f.lt_condition).Concat(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Select(f => Sweet.lollipop.neucode_labeled ? f.hv_condition : f.lt_condition)).Distinct().Count() != 2)
                MessageBox.Show("Please be sure there are two conditions.");
            else if (!Sweet.lollipop.neucode_labeled && !Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Select(f => f.lt_condition).Distinct().All(c => Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification && f.lt_condition == c).Select(f => f.biological_replicate + f.fraction + f.technical_replicate).Distinct().All(d => Sweet.lollipop.input_files.Where(f2 => f2.purpose == Purpose.Quantification && f2.lt_condition != c).
                    Select(f2 => f2.biological_replicate + f2.fraction + f2.technical_replicate).Distinct().ToList().Contains(d))))
                MessageBox.Show("Please be sure there are the same number and labels for each biological replicate, fraction, and technical replicate for each condition.");
            else if (!Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Select(f => f.lt_condition).Concat(Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification).Select(f => Sweet.lollipop.neucode_labeled ? f.hv_condition : f.lt_condition)).Distinct().All(c => Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.Quantification && f.lt_condition == c).Select(f => f.biological_replicate).Distinct().Count() >= 2))
                MessageBox.Show("Please be sure there are at least two biological replicates for each condition.");
            else if (Sweet.lollipop.raw_quantification_components.Count == 0)
                MessageBox.Show("Please process raw components.", "Quantification");
            else if (Sweet.lollipop.raw_experimental_components.Count <= 0)
                MessageBox.Show("Please load deconvolution results.", "Quantification");
            else if (Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Length <= 0)
                MessageBox.Show("Please aggregate proteoform observations.", "Quantification");
            else if (Sweet.lollipop.target_proteoform_community.families.Count <= 0)
                MessageBox.Show("Please construct proteoform families.", "Quantification");
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
            BackgroundChanged();
        }

        private void nud_bkgdWidth_ValueChanged(object sender, EventArgs e)
        {
            BackgroundChanged();
        }

        private void BackgroundChanged()
        {
            Sweet.lollipop.backgroundShift = nud_bkgdShift.Value;
            if (Sweet.lollipop.qVals.Count <= 0)
            {
                return;
            }
            Sweet.lollipop.Log2FoldChangeAnalysis.QuantitativeDistributions.defineAllObservedIntensityDistribution(Sweet.lollipop.target_proteoform_community.experimental_proteoforms.SelectMany(pf => pf.biorepIntensityList), Sweet.lollipop.Log2FoldChangeAnalysis.QuantitativeDistributions.logIntensityHistogram);
            Sweet.lollipop.TusherAnalysis1.QuantitativeDistributions.defineAllObservedIntensityDistribution(Sweet.lollipop.target_proteoform_community.experimental_proteoforms.SelectMany(pf => pf.biorepIntensityList), Sweet.lollipop.TusherAnalysis1.QuantitativeDistributions.logIntensityHistogram);
            Sweet.lollipop.TusherAnalysis2.QuantitativeDistributions.defineAllObservedIntensityDistribution(Sweet.lollipop.target_proteoform_community.experimental_proteoforms.SelectMany(pf => pf.biorepTechrepIntensityList), Sweet.lollipop.TusherAnalysis2.QuantitativeDistributions.logIntensityHistogram);
            Sweet.lollipop.Log2FoldChangeAnalysis.QuantitativeDistributions.defineBackgroundIntensityDistribution(Sweet.lollipop.quantBioFracCombos, Sweet.lollipop.satisfactoryProteoforms, Sweet.lollipop.condition_count, Sweet.lollipop.backgroundShift, Sweet.lollipop.backgroundWidth);
            Sweet.lollipop.TusherAnalysis1.QuantitativeDistributions.defineBackgroundIntensityDistribution(Sweet.lollipop.quantBioFracCombos, Sweet.lollipop.satisfactoryProteoforms, Sweet.lollipop.condition_count, Sweet.lollipop.backgroundShift, Sweet.lollipop.backgroundWidth);
            Sweet.lollipop.TusherAnalysis2.QuantitativeDistributions.defineBackgroundIntensityDistribution(Sweet.lollipop.quantBioFracCombos, Sweet.lollipop.satisfactoryProteoforms, Sweet.lollipop.condition_count, Sweet.lollipop.backgroundShift, Sweet.lollipop.backgroundWidth);
        }

        private void cb_useRandomSeed_CheckedChanged(object sender, EventArgs e)
        {
            nud_randomSeed.Enabled = cb_useRandomSeed.Checked;
            Sweet.lollipop.useRandomSeed_quant = cb_useRandomSeed.Checked;
        }

        private void nud_randomSeed_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.randomSeed_quant = Convert.ToInt32(nud_randomSeed.Value);
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
            List<InputFile> files = Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).ToList();
            if (Sweet.lollipop.observation_requirement == Lollipop.observation_requirement_possibilities[1]) // From any condition
                nud_minObservations.Maximum = Sweet.lollipop.conditionsBioReps.Sum(kv => kv.Value.Count);
            else if (Lollipop.observation_requirement_possibilities.ToList().IndexOf(Sweet.lollipop.observation_requirement) < 3)
                nud_minObservations.Maximum = Sweet.lollipop.countOfBioRepsInOneCondition;
            else if (Sweet.lollipop.observation_requirement == Lollipop.observation_requirement_possibilities[4]) // From any condition
                nud_minObservations.Maximum = files.Select(x => x.lt_condition + x.biological_replicate + x.technical_replicate).Distinct().Count();
            else
                nud_minObservations.Maximum = Math.Min(files.Where(x => x.lt_condition == Sweet.lollipop.numerator_condition).Concat(files.Where(x => x.hv_condition == Sweet.lollipop.numerator_condition)).Select(x => x.biological_replicate + x.technical_replicate).Distinct().Count(),
                   files.Where(x => x.lt_condition == Sweet.lollipop.denominator_condition).Concat(files.Where(x => x.hv_condition == Sweet.lollipop.denominator_condition)).Select(x => x.biological_replicate + x.technical_replicate).Distinct().Count());
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
        };

        private void fill_quantitative_values_table()
        {
            if (Sweet.lollipop.significance_by_log2FC)
            {
                DisplayUtility.FillDataGridView(dgv_quantification_results, Sweet.lollipop.qVals.Select(q => new DisplayQuantitativeValues(q, Sweet.lollipop.Log2FoldChangeAnalysis)));
                DisplayQuantitativeValues.FormatGridView(dgv_quantification_results);
                return;
            }
            else
            {
                if (cmbx_quantitativeValuesTableSelection.SelectedIndex == 0)
                {
                    DisplayUtility.FillDataGridView(dgv_quantification_results, Sweet.lollipop.qVals.Select(q => new DisplayQuantitativeValues(q, get_tusher_analysis())));
                    DisplayQuantitativeValues.FormatGridView(dgv_quantification_results);
                    return;
                }

                if (cmbx_quantitativeValuesTableSelection.SelectedIndex == 1)
                {
                    DisplayUtility.FillDataGridView(dgv_quantification_results, ResultsSummaryGenerator.experimental_results_dataframe(get_tusher_analysis()));
                    return;
                }
            }

            IEnumerable<ExperimentalProteoform> proteoforms = new int[] { 2, 4, 6, 8 }.Contains(cmbx_quantitativeValuesTableSelection.SelectedIndex)
                ? Sweet.lollipop.satisfactoryProteoforms as IEnumerable<ExperimentalProteoform>
                : Sweet.lollipop.target_proteoform_community.experimental_proteoforms as IEnumerable<ExperimentalProteoform>;

            bool include_imputation = new int[] { 2, 3, 6, 7 }.Contains(cmbx_quantitativeValuesTableSelection.SelectedIndex);

            DisplayUtility.FillDataGridView(dgv_quantification_results, ResultsSummaryGenerator.biological_replicate_intensities(selected_analysis, proteoforms, Sweet.lollipop.input_files, Sweet.lollipop.conditionsBioReps, include_imputation, true));
        }

        private void cmbx_quantitativeValuesTableSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            fill_quantitative_values_table();
        }

        #endregion Quantification Table Methods

        #region Volcano Plot Methods

        private void volcanoPlot()
        {
            ct_volcano_logFold_logP.Series.Clear();
            ct_volcano_logFold_logP.Series.Add("logFold_logP");
            ct_volcano_logFold_logP.Series["logFold_logP"].ChartType = SeriesChartType.Point; // these are the actual experimental proteoform intensities
            ct_volcano_logFold_logP.Series.Add("significantlogFold_logP");
            ct_volcano_logFold_logP.Series["significantlogFold_logP"].ChartType = SeriesChartType.Point; // these are the actual experimental proteoform intensities

            ct_volcano_logFold_logP.ChartAreas[0].AxisX.Title = "Log (Base 2) Fold Change (" + Sweet.lollipop.numerator_condition + "/" + Sweet.lollipop.denominator_condition + ")";
            ct_volcano_logFold_logP.ChartAreas[0].AxisY.Title = "Log (Base 10) p-Value";

            foreach (QuantitativeProteoformValues qValue in Sweet.lollipop.qVals)
            {
                if ((get_tusher_values(qValue).significant && Sweet.lollipop.significance_by_permutation) || (qValue.Log2FoldChangeValues.significant && Sweet.lollipop.significance_by_log2FC))
                    ct_volcano_logFold_logP.Series["significantlogFold_logP"].Points.AddXY(qValue.Log2FoldChangeValues.logfold2change, -Math.Log10(qValue.Log2FoldChangeValues.pValue_uncorrected));
                else
                    ct_volcano_logFold_logP.Series["logFold_logP"].Points.AddXY(qValue.Log2FoldChangeValues.logfold2change, -Math.Log10(qValue.Log2FoldChangeValues.pValue_uncorrected));
            }

            if (Sweet.lollipop.qVals.Count > 0)
            {
                ct_volcano_logFold_logP.ChartAreas[0].AxisX.Minimum = Convert.ToDouble(Math.Floor(Sweet.lollipop.qVals.Min(q => q.Log2FoldChangeValues.logfold2change)));
                ct_volcano_logFold_logP.ChartAreas[0].AxisX.Maximum = Convert.ToDouble(Math.Ceiling(Sweet.lollipop.qVals.Max(q => q.Log2FoldChangeValues.logfold2change)));
                ct_volcano_logFold_logP.ChartAreas[0].AxisY.Minimum = Math.Floor(Sweet.lollipop.qVals.Min(q => -Math.Log10(q.Log2FoldChangeValues.pValue_uncorrected)));
                ct_volcano_logFold_logP.ChartAreas[0].AxisY.Maximum = Math.Ceiling(Sweet.lollipop.qVals.Max(q => -Math.Log10(q.Log2FoldChangeValues.pValue_uncorrected)));
            }
        }

        private Point? ct_volcano_logFold_logP_prevPosition = null;
        private ToolTip ct_volcano_logFold_logP_tt = new ToolTip();

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
            selected_tusher_analysis = tusher;
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
            SetRelativeDifferenceAxisMaxMin(max_test_stat_unit);
            plotObservedVsExpectedOffsets();
        }

        private void SetRelativeDifferenceAxisMaxMin(int max_test_stat_unit)
        {
            if (get_tusher_analysis().avgSortedPermutationRelativeDifferences.Count > 0 && get_tusher_analysis().sortedProteoformRelativeDifferences.Count > 0)
            {
                ct_relativeDifference.ChartAreas[0].AxisX.Minimum = -max_test_stat_unit;
                ct_relativeDifference.ChartAreas[0].AxisX.Maximum = max_test_stat_unit;
                ct_relativeDifference.ChartAreas[0].AxisY.Minimum = -max_test_stat_unit;
                ct_relativeDifference.ChartAreas[0].AxisY.Maximum = max_test_stat_unit;
            }
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
                    if (stat.is_passing_real(get_tusher_analysis().minimumPassingNegativeTestStatistic, get_tusher_analysis().minimumPassingPositiveTestStatisitic, Sweet.lollipop.fold_change_conjunction, Sweet.lollipop.useFoldChangeCutoff, Sweet.lollipop.foldChangeCutoff, Sweet.lollipop.useAveragePermutationFoldChange, Sweet.lollipop.useBiorepPermutationFoldChange, Sweet.lollipop.minBiorepsWithFoldChange, out bool a, out bool b))
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

            SetRelativeDifferenceAxisMaxMin(max_test_stat_unit);
        }

        private void plots()
        {
            plotBiorepIntensities();
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
            if (Sweet.lollipop.satisfactoryProteoforms.Count <= 0) return;
            get_tusher_analysis().reestablishSignficance(get_go_analysis());
            plots();
            updateGoTermsTable();
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

        private Point? ct_relativeDifference_prevPosition = null;
        private ToolTip ct_relativeDifference_tt = new ToolTip();

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
        }

        private void cb_significanceByFoldChange_CheckedChanged(object sender, EventArgs e)
        {
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
                    .Where(pf => Sweet.lollipop.significance_by_log2FC ?
                        pf.quant.Log2FoldChangeValues.numeratorOriginalIntensities != null && pf.quant.Log2FoldChangeValues.denominatorOriginalIntensities != null :
                        tusher_selection < 3 ?
                        pf.quant.TusherValues1.numeratorOriginalIntensities != null && pf.quant.TusherValues1.denominatorOriginalIntensities != null :
                        pf.quant.TusherValues2.numeratorOriginalIntensities != null && pf.quant.TusherValues2.denominatorOriginalIntensities != null
                        ).ToList();

                foreach (ExperimentalProteoform pf in pfs)
                {
                    int yeah = Sweet.lollipop.significance_by_log2FC ?
                        ct_proteoformIntensities.Series["Intensities"].Points.AddXY(pf.quant.Log2FoldChangeValues.numeratorOriginalIntensities == null || pf.quant.Log2FoldChangeValues.numeratorOriginalIntensities.Count == 0 ? 0 : pf.quant.Log2FoldChangeValues.numeratorOriginalIntensities.Average(x => x.intensity_sum), pf.quant.Log2FoldChangeValues.denominatorOriginalIntensities == null || pf.quant.Log2FoldChangeValues.denominatorOriginalIntensities.Count == 0 ? 0 : pf.quant.Log2FoldChangeValues.denominatorOriginalIntensities.Average(x => x.intensity_sum)) :
                        tusher_selection < 3 ?
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

            QuantitativeDistributions quant = Sweet.lollipop.significance_by_log2FC ? Sweet.lollipop.Log2FoldChangeAnalysis.QuantitativeDistributions : get_tusher_analysis().QuantitativeDistributions;
            foreach (KeyValuePair<decimal, int> entry in selection == 3 ?
                quant.logSelectIntensityWithImputationHistogram :
                new int[] { 1, 2 }.Contains(selection) ? quant.logSelectIntensityHistogram :
                    quant.logIntensityHistogram)
            {
                ct_proteoformIntensities.Series["Intensities"].Points.AddXY(entry.Key, entry.Value);

                decimal gaussian_height = selection == 3 ?
                   quant.selectWithImputationGaussianHeight :
                    new int[] { 1, 2 }.Contains(selection) ? quant.selectGaussianHeight :
                        quant.allObservedGaussianHeight;
                decimal average_intensity = selection == 3 ?
                    quant.selectWithImputationAverageIntensity :
                    new int[] { 1, 2 }.Contains(selection) ? quant.selectAverageIntensity :
                        quant.allObservedAverageIntensity;
                decimal std_dev = selection == 3 ?
                    quant.selectWithImputationStDev :
                    new int[] { 1, 2 }.Contains(selection) ? quant.selectStDev :
                       quant.allObservedStDev;
                double gaussIntensity = ((double)gaussian_height) * Math.Exp(-Math.Pow(((double)entry.Key - (double)average_intensity), 2) / (2d * Math.Pow((double)std_dev, 2)));
                double bkgd_gaussIntensity = (double)quant.bkgdGaussianHeight * Math.Exp(-Math.Pow(((double)entry.Key - (double)quant.bkgdAverageIntensity), 2) / (2d * Math.Pow((double)quant.bkgdStDev, 2)));
                double sumIntensity = gaussIntensity + bkgd_gaussIntensity;
                ct_proteoformIntensities.Series["Fit"].Points.AddXY(entry.Key, gaussIntensity);

                if (selection == 2)
                {
                    ct_proteoformIntensities.Series["Bkgd. Projected"].Points.AddXY(entry.Key, bkgd_gaussIntensity);
                    ct_proteoformIntensities.Series["Fit + Projected"].Points.AddXY(entry.Key, sumIntensity);
                }
            }
            tb_avgIntensity.Text = Math.Round(quant.selectAverageIntensity, 1).ToString();
            tb_stdevIntensity.Text = Math.Round(quant.selectStDev, 3).ToString();
        }

        private void cmbx_intensityDistributionChartSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            plotBiorepIntensities();
        }

        private Point? ct_proteoformIntensities_prevPosition = null;
        private ToolTip ct_proteoformIntensities_tt = new ToolTip();

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
            selected_analysis = go;
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
            updateGoTermsTable();
        }

        private void updateGoTermsTable()
        {
            IGoAnalysis g = get_go_analysis();
            g.GoAnalysis.maxGoTermFDR = nud_FDR.Value;
            g.GoAnalysis.minProteoformFoldChange = nud_ratio.Value;
            g.GoAnalysis.minProteoformIntensity = nud_intensity.Value;
            if (Sweet.lollipop.significance_by_log2FC) g.inducedOrRepressedProteins = Sweet.lollipop.getInducedOrRepressedProteins(Sweet.lollipop.satisfactoryProteoforms.Where(pf => pf.quant.Log2FoldChangeValues.significant), Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis);
            else g.inducedOrRepressedProteins = Sweet.lollipop.getInducedOrRepressedProteins(Sweet.lollipop.satisfactoryProteoforms.Where(pf => get_tusher_analysis() as TusherAnalysis1 != null ? pf.quant.TusherValues1.significant : pf.quant.TusherValues2.significant), get_tusher_analysis().GoAnalysis);
            g.GoAnalysis.GO_analysis(g.inducedOrRepressedProteins);
            fillGoTermsTable();
        }

        private void cmbx_goAspect_SelectedIndexChanged(object sender, EventArgs e)
        {
            fillGoTermsTable();
        }

        private bool backgroundUpdated = true;

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

        private OpenFileDialog fileOpen = new OpenFileDialog();

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

        private OpenFileDialog fileOpener = new OpenFileDialog();
        private FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
        private bool got_cyto_temp_folder = false;

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
            List<ProteoformFamily> families = Sweet.lollipop.significance_by_log2FC ? Sweet.lollipop.getInterestingFamilies(Sweet.lollipop.satisfactoryProteoforms.Where(pf => pf.quant.Log2FoldChangeValues.significant), Sweet.lollipop.Log2FoldChangeAnalysis.GoAnalysis) :
                    Sweet.lollipop.getInterestingFamilies(Sweet.lollipop.satisfactoryProteoforms.Where(pf => get_tusher_analysis() as TusherAnalysis1 != null ? pf.quant.TusherValues1.significant : pf.quant.TusherValues2.significant), get_tusher_analysis().GoAnalysis);
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

        private void cmbx_empty_TextChanged(object sender, EventArgs e)
        {
        }

        #endregion Cytoscape Visualization Private Methods

        private void nUD_min_fold_change_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.Log2FoldChangeAnalysis.minFoldChange = (double)nUD_min_fold_change.Value;
            Sweet.lollipop.Log2FoldChangeAnalysis.establish_benjiHoch_significance();
            volcanoPlot();
        }

        private void rb_significanceByFoldChange_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.significance_by_log2FC = rb_significanceByFoldChange.Checked;
            rb_signficanceByPermutation.Checked = !rb_significanceByFoldChange.Checked;
            if (rb_significanceByFoldChange.Checked)
            {
                plots();
            }
            updateGoTermsTable();
            fill_quantitative_values_table();
        }

        private void rb_signficanceByPermutation_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.significance_by_permutation = rb_signficanceByPermutation.Checked;
            rb_significanceByFoldChange.Checked = !rb_signficanceByPermutation.Checked;
            if (rb_signficanceByPermutation.Checked)
            {
                plots();
            }
            updateGoTermsTable();
            fill_quantitative_values_table();
        }
    }
}