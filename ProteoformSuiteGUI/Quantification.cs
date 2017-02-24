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
using System.IO;
using Proteomics;

namespace ProteoformSuite
{
    public partial class Quantification : Form
    {
        // FORM OPERATION
        public Quantification()
        {
            InitializeComponent();
        }
        private void Quantification_Load(object sender, EventArgs e) //I would rather that load event code be here than somewhere else. it makes it very hard to know what is being called on form load.....
        { }

        public DataGridView Get_quant_results_DGV()
        {
            return dgv_quantification_results;
        }

        public DataGridView Get_GoTerms_DGV()
        {
            return dgv_goAnalysis;
        }

        public void perform_calculations() //this is the first thing that gets run on form load
        {
            if (Lollipop.get_files(Lollipop.input_files, Purpose.Quantification).Count() > 0 && Lollipop.proteoform_community.experimental_proteoforms.Length > 0 && Lollipop.qVals.Count <= 0)
            {
                initialize();
                Lollipop.quantify();
                Lollipop.GO_analysis();
                fillGuiTablesAndGraphs();
            }
        }

        public void fillGuiTablesAndGraphs()
        {
            plotObservedVsExpectedRelativeDifference();
            DisplayUtility.FillDataGridView(dgv_quantification_results, Lollipop.qVals);
            volcanoPlot();
            plotBiorepIntensities();
            updateGoTermsTable();           
        }

        private void btn_refreshCalculation_Click(object sender, EventArgs e)
        {
            Lollipop.quantify();
            fillGuiTablesAndGraphs();
        }

        public void initialize_every_time()
        {
            this.tb_familyBuildFolder.Text = Lollipop.family_build_folder_path;
        }

        private void initialize()
        {
            //Initialize conditions
            List<string> conditions = Lollipop.ltConditionsBioReps.Keys.ToList();
            conditions.AddRange(Lollipop.hvConditionsBioReps.Keys.ToList());
            conditions = conditions.Distinct().ToList();
            cmbx_ratioNumerator.Items.AddRange(conditions.ToArray());
            cmbx_ratioDenominator.Items.AddRange(conditions.ToArray());
            cmbx_ratioNumerator.SelectedIndex = 0;
            cmbx_ratioDenominator.SelectedIndex = Convert.ToInt32(conditions.Count() > 1);
            Lollipop.numerator_condition = cmbx_ratioNumerator.SelectedItem.ToString();
            Lollipop.denominator_condition = cmbx_ratioDenominator.SelectedItem.ToString();
            cmbx_edgeLabel.Items.AddRange(Lollipop.edge_labels);

            //Initialize display options
            cmbx_colorScheme.Items.AddRange(CytoscapeScript.color_scheme_names);
            cmbx_nodeLayout.Items.AddRange(Lollipop.node_positioning);
            cmbx_nodeLabelPositioning.Items.AddRange(CytoscapeScript.node_label_positions);
            cb_redBorder.Checked = true;
            cb_boldLabel.Checked = true;
            cb_moreOpacity.Checked = false;

            cmbx_colorScheme.SelectedIndex = 0;
            cmbx_nodeLayout.SelectedIndex = 0;
            cmbx_nodeLabelPositioning.SelectedIndex = 0;

            //Set parameters
            nud_bkgdShift.ValueChanged -= nud_bkgdShift_ValueChanged;
            nud_bkgdShift.Value = (decimal)-2.0;
            Lollipop.backgroundShift = nud_bkgdShift.Value;
            nud_bkgdShift.ValueChanged += nud_bkgdShift_ValueChanged;

            nud_bkgdWidth.ValueChanged -= nud_bkgdWidth_ValueChanged;
            nud_bkgdWidth.Value = (decimal)0.5;
            Lollipop.backgroundWidth = nud_bkgdWidth.Value;
            nud_bkgdWidth.ValueChanged += nud_bkgdWidth_ValueChanged;

            nud_bkgdShift.ValueChanged += new EventHandler(plotBiorepIntensitiesEvent);
            nud_bkgdWidth.ValueChanged += new EventHandler(plotBiorepIntensitiesEvent);

            nud_FDR.ValueChanged -= new EventHandler(updateGoTermsTable);
            nud_ratio.ValueChanged -= new EventHandler(updateGoTermsTable);
            nud_intensity.ValueChanged -= new EventHandler(updateGoTermsTable);

            nud_FDR.Value = Lollipop.minProteoformFDR;
            nud_ratio.Value = Lollipop.minProteoformFoldChange;
            nud_intensity.Value = Lollipop.minProteoformIntensity;

            nud_FDR.ValueChanged += new EventHandler(updateGoTermsTable);
            nud_ratio.ValueChanged += new EventHandler(updateGoTermsTable);
            nud_intensity.ValueChanged += new EventHandler(updateGoTermsTable);

            //Lollipop.getObservationParameters(); //examines the conditions and bioreps to determine the maximum number of observations to require for quantification
            nud_minObservations.Minimum = 1;
            nud_minObservations.Maximum = Lollipop.countOfBioRepsInOneCondition;
            nud_minObservations.Value = Lollipop.countOfBioRepsInOneCondition;
            Lollipop.minBiorepsWithObservations = (int)nud_minObservations.Value;

            cmbx_observationsTypeRequired.SelectedIndexChanged -= cmbx_observationsTypeRequired_SelectedIndexChanged;
            cmbx_observationsTypeRequired.Items.AddRange(Lollipop.observation_requirement_possibilities);
            cmbx_observationsTypeRequired.SelectedIndex = 0;
            Lollipop.observation_requirement = cmbx_observationsTypeRequired.SelectedItem.ToString();
            cmbx_observationsTypeRequired.SelectedIndexChanged += cmbx_observationsTypeRequired_SelectedIndexChanged;


            nud_sKnot_minFoldChange.ValueChanged -= nud_sKnot_minFoldChange_ValueChanged;
            nud_sKnot_minFoldChange.Value = Lollipop.sKnot_minFoldChange;
            nud_sKnot_minFoldChange.ValueChanged += nud_sKnot_minFoldChange_ValueChanged;

            nud_Offset.ValueChanged -= nud_Offset_ValueChanged;
            nud_Offset.Value = Lollipop.offsetTestStatistics;
            nud_Offset.ValueChanged += nud_Offset_ValueChanged;

            cmbx_goAspect.Items.Add(Aspect.biologicalProcess);
            cmbx_goAspect.Items.Add(Aspect.cellularComponent);
            cmbx_goAspect.Items.Add(Aspect.molecularFunction);

            cmbx_goAspect.SelectedIndexChanged -= cmbx_goAspect_SelectedIndexChanged; //disable event on load to prevent premature firing
            cmbx_goAspect.SelectedIndex = 0;
            cmbx_goAspect.SelectedIndexChanged += cmbx_goAspect_SelectedIndexChanged;

            rb_allSampleGOTerms.Enabled = false;
            rb_allSampleGOTerms.Checked = !Lollipop.allTheoreticalProteins; //initiallizes the background for GO analysis to the set of observed proteins. not the set of theoretical proteins.
            rb_allSampleGOTerms.Enabled = true;

            rb_allSampleGOTerms.CheckedChanged += new EventHandler(goTermBackgroundChanged);
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
            ct_relativeDifference.ChartAreas[0].AxisX.Title = "expected relative difference dE(i)";
            ct_relativeDifference.ChartAreas[0].AxisY.Title = "observed relative difference d(i)";

            ct_relativeDifference.Series["obsVSexp"].Points.DataBindXY(Lollipop.sortedAvgPermutationTestStatistics.ToList(), Lollipop.sortedProteoformTestStatistics.ToList());

            plotObservedVsExpectedOffsets();
        }

        decimal positiveOffsetFunction(decimal x)
        {
            return (x + nud_Offset.Value);
        }

        decimal negativeOffsetFunction(decimal x)
        {
            return (x - nud_Offset.Value);
        }

        private void plotObservedVsExpectedOffsets()
        {
            ct_relativeDifference.Series["positiveOffset"].Points.Clear();
            ct_relativeDifference.Series["negativeOffset"].Points.Clear();

            foreach (decimal xValue in Lollipop.sortedAvgPermutationTestStatistics)
            {
                ct_relativeDifference.Series["positiveOffset"].Points.AddXY(xValue, positiveOffsetFunction(xValue));
                ct_relativeDifference.Series["negativeOffset"].Points.AddXY(xValue, negativeOffsetFunction(xValue));
            }

            ct_relativeDifference.ChartAreas[0].AxisX.Minimum = -10;
            ct_relativeDifference.ChartAreas[0].AxisX.Maximum = 10;
            ct_relativeDifference.ChartAreas[0].AxisY.Minimum = -10;
            ct_relativeDifference.ChartAreas[0].AxisY.Maximum = 10;

            tb_FDR.Text = Lollipop.offsetFDR.ToString();
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
            ct_proteoformIntensities.ChartAreas[0].AxisX.Title = "log Intensity (base 2)";
            ct_proteoformIntensities.ChartAreas[0].AxisY.Title = "count";

           
            foreach (KeyValuePair<decimal, int> entry in Lollipop.logSelectIntensityHistogram)
            {
                ct_proteoformIntensities.Series["Observed Intensities"].Points.AddXY(entry.Key, entry.Value);

                double gaussIntensity = ((double)Lollipop.selectGaussianHeight) * Math.Exp(-Math.Pow(((double)entry.Key - (double)Lollipop.selectAverageIntensity), 2) / (2d * Math.Pow((double)Lollipop.selectStDev, 2)));
                double bkgd_gaussIntensity = ((double)Lollipop.bkgdGaussianHeight) * Math.Exp(-Math.Pow(((double)entry.Key - (double)Lollipop.bkgdAverageIntensity), 2) / (2d * Math.Pow((double)Lollipop.bkgdStDev, 2)));
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

            ct_proteoformIntensities.ChartAreas[0].AxisX.Title = "log (base 2) fold change (light/heavy)";
            ct_proteoformIntensities.ChartAreas[0].AxisY.Title = "log (base 10) pValue";

            foreach (ExperimentalProteoform.quantitativeValues qValue in Lollipop.qVals)
            {
                ct_volcano_logFold_logP.Series["logFold_logP"].Points.AddXY(qValue.logFoldChange, -Math.Log10((double)qValue.pValue));
            }
        }


        private void goTermBackgroundChanged(object s, EventArgs e)
        {
            Lollipop.allTheoreticalProteins = !Lollipop.allTheoreticalProteins;
            Lollipop.GO_analysis();
        }

        private void fillGoTermsTable()
        {
            DisplayUtility.FillDataGridView(dgv_goAnalysis, Lollipop.goTermNumbers.Where(x => x.goTerm.aspect.ToString() == cmbx_goAspect.SelectedItem.ToString()));
        }

        private void updateGoTermsTable(object s, EventArgs e)
        {
            Lollipop.minProteoformFDR = nud_FDR.Value;
            Lollipop.minProteoformFoldChange = nud_ratio.Value;
            Lollipop.minProteoformIntensity = nud_intensity.Value;
            Lollipop.inducedOrRepressedProteins = Lollipop.getInducedOrRepressedProteins(Lollipop.satisfactoryProteoforms, Lollipop.expanded_proteins, Lollipop.minProteoformFoldChange, Lollipop.minProteoformFDR, Lollipop.minProteoformIntensity);
            Lollipop.GO_analysis();
            fillGoTermsTable();
        }

        private void updateGoTermsTable()
        {
            Lollipop.minProteoformFDR = nud_FDR.Value;
            Lollipop.minProteoformFoldChange = nud_ratio.Value;
            Lollipop.minProteoformIntensity = nud_intensity.Value;
            Lollipop.inducedOrRepressedProteins = Lollipop.getInducedOrRepressedProteins(Lollipop.satisfactoryProteoforms, Lollipop.expanded_proteins, Lollipop.minProteoformFoldChange, Lollipop.minProteoformFDR, Lollipop.minProteoformIntensity);
            Lollipop.GO_analysis();
            fillGoTermsTable();
        }

        private void cmbx_goAspect_SelectedIndexChanged(object sender, EventArgs e)
        {
            fillGoTermsTable();
        }

        // CYTOSCAPE VISUALIZATION
        OpenFileDialog fileOpener = new OpenFileDialog();
        FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
        bool got_cyto_temp_folder = false;

        private void btn_browseTempFolder_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.folderBrowser.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string temp_folder_path = folderBrowser.SelectedPath;
                tb_familyBuildFolder.Text = temp_folder_path; //triggers TextChanged method
            }
        }

        private void tb_familyBuildFolder_TextChanged(object sender, EventArgs e)
        {
            string path = tb_familyBuildFolder.Text;
            Lollipop.family_build_folder_path = path;
            got_cyto_temp_folder = true;
            enable_buildAllFamilies_button();
            enable_buildSelectedFamilies_button();
        }

        private void enable_buildAllFamilies_button()
        {
            if (got_cyto_temp_folder) btn_buildAllFamilies.Enabled = true;
        }

        private void enable_buildSelectedFamilies_button()
        {
            if (got_cyto_temp_folder && dgv_quantification_results.SelectedRows.Count > 0) btn_buildSelectedQuantFamilies.Enabled = true;
        }

        private void btn_buildAllQuantifiedFamilies_Click(object sender, EventArgs e)
        {
            bool built = build_families(Lollipop.getInterestingFamilies(Lollipop.qVals, Lollipop.proteoform_community.families, Lollipop.minProteoformFoldChange, Lollipop.minProteoformFDR, Lollipop.minProteoformIntensity).Distinct().ToList());
            if (!built) return;
            MessageBox.Show("Finished building all families.\n\nPlease load them into Cytoscape 3.0 or later using \"Tools\" -> \"Execute Command File\" and choosing the script_[TIMESTAMP].txt file in your specified directory.");
        }

        private void btn_buildSelectedQuantFamilies_Click(object sender, EventArgs e)
        {
            List<ExperimentalProteoform.quantitativeValues> selected_qvals = (DisplayUtility.get_selected_objects(dgv_quantification_results).Select(o => (ExperimentalProteoform.quantitativeValues)o)).ToList();
            List<ProteoformFamily> selected_families = Lollipop.getInterestingFamilies(selected_qvals, Lollipop.proteoform_community.families, Lollipop.minProteoformFoldChange, Lollipop.minProteoformFDR, Lollipop.minProteoformIntensity).Distinct().ToList();
            bool built = build_families(selected_families);
            if (!built) return;

            string selected_family_string = "Finished building selected famil";
            if (selected_families.Count() == 1) selected_family_string += "y :";
            else selected_family_string += "ies :";
            if (selected_families.Count() > 3) selected_family_string = String.Join(", ", selected_families.Select(f => f.family_id).ToList().Take(3)) + ". . .";
            else selected_family_string = String.Join(", ", selected_families.Select(f => f.family_id));
            MessageBox.Show(selected_family_string + ".\n\nPlease load them into Cytoscape 3.0 or later using \"Tools\" -> \"Execute Command File\" and choosing the script_[TIMESTAMP].txt file in your specified directory.");
        }

        private void btn_buildFamiliesAllGO_Click(object sender, EventArgs e)
        {
            Aspect a = (Aspect)cmbx_goAspect.SelectedItem;
            bool built = build_families(Lollipop.getInterestingFamilies(Lollipop.goTermNumbers.Where(n=>n.goTerm.aspect == a).Distinct().ToList(), Lollipop.proteoform_community.families));
            if (!built) return;
            MessageBox.Show("Finished building all families.\n\nPlease load them into Cytoscape 3.0 or later using \"Tools\" -> \"Execute Command File\" and choosing the script_[TIMESTAMP].txt file in your specified directory.");
        }

        private void btn_buildFromSelectedGoTerms_Click(object sender, EventArgs e)
        {
            List<GoTermNumber> selected_gos = (DisplayUtility.get_selected_objects(dgv_goAnalysis).Select(o => (GoTermNumber)o)).ToList();
            List<ProteoformFamily> selected_families = Lollipop.getInterestingFamilies(selected_gos, Lollipop.proteoform_community.families).Distinct().ToList();
            bool built = build_families(selected_families);
            if (!built) return;

            string selected_family_string = "Finished building selected famil";
            if (selected_families.Count() == 1) selected_family_string += "y :";
            else selected_family_string += "ies :";
            if (selected_families.Count() > 3) selected_family_string = String.Join(", ", selected_families.Select(f => f.family_id).ToList().Take(3)) + ". . .";
            else selected_family_string = String.Join(", ", selected_families.Select(f => f.family_id));
            MessageBox.Show(selected_family_string + ".\n\nPlease load them into Cytoscape 3.0 or later using \"Tools\" -> \"Execute Command File\" and choosing the script_[TIMESTAMP].txt file in your specified directory.");
        }

        private bool build_families(List<ProteoformFamily> families)
        {
            //Check if valid folder
            if (Lollipop.family_build_folder_path == "" || !Directory.Exists(Lollipop.family_build_folder_path))
            {
                MessageBox.Show("Please choose a folder in which the families will be built, so you can load them into Cytoscape.");
                return false;
            }
            string time_stamp = SaveState.time_stamp();
            tb_recentTimeStamp.Text = time_stamp;
            CytoscapeScript c = new CytoscapeScript(families, time_stamp, true, cb_redBorder.Checked, cb_boldLabel.Checked, cb_moreOpacity.Checked, cmbx_colorScheme.SelectedItem.ToString(), cmbx_nodeLabelPositioning.SelectedItem.ToString());
            File.WriteAllText(c.edges_path, c.edge_table);
            File.WriteAllText(c.nodes_path, c.node_table);
            File.WriteAllText(c.script_path, c.script);
            c.write_styles(); //cmbx_colorScheme.SelectedItem.ToString(), cmbx_nodeLayout.SelectedItem.ToString(), "");
            return true;
        }

        private void cmbx_ratioNumerator_SelectedIndexChanged(object sender, EventArgs e)
        {
            Lollipop.numerator_condition = cmbx_ratioNumerator.SelectedItem.ToString();
        }

        private void cmbx_ratioDenominator_SelectedIndexChanged(object sender, EventArgs e)
        {
            Lollipop.denominator_condition = cmbx_ratioDenominator.SelectedItem.ToString();
        }

        private void nud_bkgdShift_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.backgroundShift = nud_bkgdShift.Value;
            Lollipop.defineAllObservedIntensityDistribution(Lollipop.proteoform_community.experimental_proteoforms, Lollipop.logIntensityHistogram);
            Lollipop.defineBackgroundIntensityDistribution(Lollipop.neucode_labeled, Lollipop.quantBioFracCombos, Lollipop.satisfactoryProteoforms, Lollipop.backgroundShift, Lollipop.backgroundWidth);
        }

        private void nud_bkgdWidth_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.backgroundWidth = nud_bkgdWidth.Value;
            Lollipop.defineAllObservedIntensityDistribution(Lollipop.proteoform_community.experimental_proteoforms, Lollipop.logIntensityHistogram);
            Lollipop.defineBackgroundIntensityDistribution(Lollipop.neucode_labeled, Lollipop.quantBioFracCombos, Lollipop.satisfactoryProteoforms, Lollipop.backgroundShift, Lollipop.backgroundWidth);
        }

        private void cmbx_observationsTypeRequired_SelectedIndexChanged(object sender, EventArgs e)
        {
            Lollipop.observation_requirement = cmbx_observationsTypeRequired.SelectedItem.ToString();
        }

        private void nud_minObservations_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.minBiorepsWithObservations = (int)nud_minObservations.Value;
        }

        private void nud_sKnot_minFoldChange_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.sKnot_minFoldChange = nud_sKnot_minFoldChange.Value;
            Lollipop.computeProteoformTestStatistics(Lollipop.neucode_labeled, Lollipop.satisfactoryProteoforms, Lollipop.bkgdAverageIntensity, Lollipop.bkgdStDev, Lollipop.numerator_condition, Lollipop.denominator_condition, Lollipop.sKnot_minFoldChange);
            Lollipop.computeSortedTestStatistics(Lollipop.satisfactoryProteoforms);
            Lollipop.computeFoldChangeFDR(Lollipop.sortedAvgPermutationTestStatistics, Lollipop.sortedProteoformTestStatistics, Lollipop.satisfactoryProteoforms, Lollipop.permutedTestStatistics, Lollipop.offsetTestStatistics);
            Lollipop.computeIndividualExperimentalProteoformFDRs(Lollipop.satisfactoryProteoforms, Lollipop.sortedProteoformTestStatistics, Lollipop.minProteoformFoldChange, Lollipop.minProteoformFDR, Lollipop.minProteoformIntensity);
            plotObservedVsExpectedOffsets();
        }

        private void nud_Offset_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.offsetTestStatistics = nud_Offset.Value;
            Lollipop.computeFoldChangeFDR(Lollipop.sortedAvgPermutationTestStatistics, Lollipop.sortedProteoformTestStatistics, Lollipop.satisfactoryProteoforms, Lollipop.permutedTestStatistics, Lollipop.offsetTestStatistics);
            plotObservedVsExpectedOffsets();
        }

        private void rb_allTheoreticalProteins_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.GO_analysis();
            fillGoTermsTable();
        }
    }
}