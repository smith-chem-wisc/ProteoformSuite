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

namespace ProteoformSuite
{
    public partial class Quantification : Form
    {
        // FORM OPERATION
        public Quantification()
        {
            InitializeComponent();
        }
        private void Quantification_Load(object sender, EventArgs e)
        { }

        public DataGridView Get_quant_results_DGV()
        {
            return dgv_quantification_results;
        }

        public DataGridView Get_GoTerms_DGV()
        {
            return dgv_goAnalysis;
        }

        private void quantify()
        {
            this.Cursor = Cursors.WaitCursor;
            computeBiorepIntensities();
            defineAllObservedIntensityDistribution();
            determineProteoformsMeetingCriteria();
            defineSelectObservedIntensityDistribution();
            defineSelectBackgroundIntensityDistribution();
            plotBiorepIntensities();
            proteoformQuantification();
            DisplayUtility.FillDataGridView(dgv_quantification_results, qVals);
            volcanoPlot();
            interestingProteins = getInterestingProteins(qVals);
            goTermNumbers = getGoTermNumbers(interestingProteins);
            fillGoTermsTable();
            this.Cursor = Cursors.Default;
        }

        public void perform_calculations()
        {
            if (Lollipop.quantification_files().Count() > 0 && Lollipop.proteoform_community.experimental_proteoforms.Length > 0 && qVals.Count <= 0)
            {
                initialize();
                quantify();
            }
        }

        private void btn_refreshCalculation_Click(object sender, EventArgs e)
        {
            quantify();
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
            nud_bkgdShift.Value = (decimal)-2.0;
            nud_bkgdWidth.Value = (decimal)0.5;

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

            cmbx_goAspect.Items.Add(Aspect.biologicalProcess);
            cmbx_goAspect.Items.Add(Aspect.cellularComponent);
            cmbx_goAspect.Items.Add(Aspect.molecularFunction);

            cmbx_goAspect.SelectedIndexChanged -= cmbx_goAspect_SelectedIndexChanged; //disable event on load to prevent premature firing
            cmbx_goAspect.SelectedIndex = 0;
            cmbx_goAspect.SelectedIndexChanged += cmbx_goAspect_SelectedIndexChanged;

            rb_allSampleGOTerms.Enabled = false;
            rb_allSampleGOTerms.Checked = true;
            rb_allSampleGOTerms.Enabled = true;

            rb_allSampleGOTerms.CheckedChanged += new EventHandler(goTermBackgroundChanged);
            //rb_allTheoreticalGOTerms.CheckedChanged += new EventHandler(goTermBackgroundChanged); // this is disabled to prevent two method calls.

            goMasterSet = getDatabaseGoNumbers();
        }


        // CALCULATING DISTRIBUTION OF OBSERVED AND IMPUTED PROTEOFORM INTENSITIES
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

        int satisfactoryProteoformsCount;
        List<ExperimentalProteoform> satisfactoryProteoforms = new List<ExperimentalProteoform>(); // these are proteoforms meeting the required number of observations.

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
            {
                Parallel.ForEach(Lollipop.proteoform_community.experimental_proteoforms, eP =>
                {
                    foreach (string c in conditions)
                    {
                        if (eP.biorepIntensityList.Where(bc => bc.condition == c).Select(b => b.biorep).ToList().Count() == nud_minObservations.Value)
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
            }

            else //any condition
            {
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
            }

            satisfactoryProteoforms = sP.ToList();
        }

        private void defineSelectBackgroundIntensityDistribution()
        {
            bkgdSelectAverageIntensity = observedAverageIntensity + nud_bkgdShift.Value * observedStDev;
            bkgdSelectStDev = observedStDev * nud_bkgdWidth.Value;
            int numSelectMeasurableIntensities = Lollipop.quantBioFracCombos.Keys.Count() * satisfactoryProteoforms.Count();
            if (Lollipop.neucode_labeled) numSelectMeasurableIntensities = numSelectMeasurableIntensities * 2;
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
            string numerator = cmbx_ratioNumerator.SelectedItem.ToString();
            string denominator = cmbx_ratioDenominator.SelectedItem.ToString();
            Parallel.ForEach(Lollipop.proteoform_community.experimental_proteoforms, eP => new ExperimentalProteoform.quantitativeValues(eP, bkgdAverageIntensity, bkgdStDev, numerator, denominator));
            qVals = satisfactoryProteoforms.Where(eP => eP.accepted == true).Select(e => e.quant).ToList();
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


        // GO TERM SIGNIFICANCE
        Dictionary<GoTerm, int> goMasterSet = new Dictionary<GoTerm, int>();
        List<GoTermNumber> goTermNumbers = new List<GoTermNumber>();
        List<GoTermNumber> interesting_go_terms = new List<GoTermNumber>();

        private Dictionary<GoTerm, int> getDatabaseGoNumbers()
        {
            Dictionary<GoTerm, int> numbers = new Dictionary<GoTerm, int>();
            List<Protein> proteinList = new List<Protein>();
            List<string> experimentalProteoformAcessionList = new List<string>();
            List<GoTerm> completeGoTermList = new List<GoTerm>();
            List<GoTerm> uniqueGoTermList = new List<GoTerm>();

            if (rb_allTheoreticalGOTerms.Checked == true)
                proteinList = Lollipop.proteins.ToList();
            else
            {
                List<TheoreticalProteoform> theoreticalProteoformList = new List<TheoreticalProteoform>();
                theoreticalProteoformList = Lollipop.proteoform_community.families.SelectMany(t => t.theoretical_proteoforms).ToList();

                List<string> theoreticalAccessionList = new List<string>();
                theoreticalAccessionList = theoreticalProteoformList.Select(a => a.accession).ToList();

                foreach (string acc in theoreticalAccessionList)
                {
                    string someJunk = acc.Replace("_T", "!").Split('!').FirstOrDefault();
                    Protein p = Lollipop.proteins.FirstOrDefault(protein => protein.accession == someJunk);
                    if (p != null && !proteinList.Any(theoreticalProtein => theoreticalProtein.accession == p.accession))
                        proteinList.Add(p);
                }
            }

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

        private void goTermBackgroundChanged(object s, EventArgs e)
        {
            goMasterSet = getDatabaseGoNumbers();
            if(interestingProteins.Count() <= 0) interestingProteins = getInterestingProteins(qVals);
            goTermNumbers = getGoTermNumbers(interestingProteins);
            fillGoTermsTable();
        }

        private void fillGoTermsTable()
        {
            this.interesting_go_terms = goTermNumbers.Where(x => x.goTerm.aspect.ToString() == cmbx_goAspect.SelectedItem.ToString()).ToList();
            DisplayUtility.FillDataGridView(dgv_goAnalysis, goTermNumbers.Where(x => x.goTerm.aspect.ToString() == cmbx_goAspect.SelectedItem.ToString()));
        }

        private void updateGoTermsTable(object s, EventArgs e)
        {
            interestingProteins = getInterestingProteins(qVals);
            goTermNumbers = getGoTermNumbers(interestingProteins);
            fillGoTermsTable();
        }

        private void cmbx_goAspect_SelectedIndexChanged(object sender, EventArgs e)
        {
            fillGoTermsTable();
        }


        // GETTING SIGNIFICANT PROTS
        List<Protein> interestingProteins = new List<Protein>();
        List<ExperimentalProteoform.quantitativeValues> qVals = new List<ExperimentalProteoform.quantitativeValues>();

        private List<ExperimentalProteoform> getInterestingProteoforms(List<ExperimentalProteoform.quantitativeValues> qvals)
        {
            List<ExperimentalProteoform.quantitativeValues> interestingQuantValues = qvals.Where(q => q.intensitySum > nud_intensity.Value && Math.Abs(q.logFoldChange) > nud_ratio.Value && q.pValue < nud_pValue.Value).ToList();
            List<string> distinctAccessions = interestingQuantValues.Select(a => a.accession).ToList();
            List<ExperimentalProteoform> interestingProteoforms = Lollipop.proteoform_community.experimental_proteoforms.Where(p => distinctAccessions.Contains(p.accession)).ToList();
            interestingProteoforms.ForEach(e => e.quant.significant = true);
            return interestingProteoforms;
        }

        private List<ProteoformFamily> getInterestingFamilies(List<ExperimentalProteoform.quantitativeValues> qvals)
        {
            IEnumerable<ProteoformFamily> interesting_families =
                from exp in this.getInterestingProteoforms(qvals)
                from fam in Lollipop.proteoform_community.families
                where fam.experimental_proteoforms.Contains(exp)
                select fam;
            return interesting_families.ToList();
        }

        private List<ProteoformFamily> getInterestingFamilies(List<GoTermNumber> go_terms_numbers)
        {
            IEnumerable<ProteoformFamily> interesting_families =
                from fam in Lollipop.proteoform_community.families
                where fam.theoretical_proteoforms.Any(t => t.proteinList.Any(p => p.goTerms.Any(g => go_terms_numbers.Select(n => n.goTerm).Contains(g))))
                select fam;
            return interesting_families.ToList();
        }

        private List<Protein> getInterestingProteins(List<ExperimentalProteoform.quantitativeValues> qvals)
        {
            IEnumerable<string> interesting_theo_accessions = getInterestingFamilies(qvals).SelectMany(f => f.theoretical_proteoforms).Select(theo => theo.accession);
            foreach (string accession in interesting_theo_accessions)
            {
                string someJunk = accession.Replace("_T", "!").Split('!').FirstOrDefault();
                Protein p = Lollipop.proteins.FirstOrDefault(protein => protein.accession == someJunk);
                if (p != null && !interestingProteins.Any(theoreticalProtein => theoreticalProtein.accession == p.accession))
                    interestingProteins.Add(p);
            }
            return interestingProteins;
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
            bool built = build_families(getInterestingFamilies(qVals).Distinct().ToList());
            if (!built) return;
            MessageBox.Show("Finished building all families.\n\nPlease load them into Cytoscape 3.0 or later using \"Tools\" -> \"Execute Command File\" and choosing the script_[TIMESTAMP].txt file in your specified directory.");
        }

        private void btn_buildSelectedQuantFamilies_Click(object sender, EventArgs e)
        {
            List<ExperimentalProteoform.quantitativeValues> selected_qvals = (DisplayUtility.get_selected_objects(dgv_quantification_results).Select(o => (ExperimentalProteoform.quantitativeValues)o)).ToList();
            List<ProteoformFamily> selected_families = getInterestingFamilies(selected_qvals).Distinct().ToList();
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
            bool built = build_families(getInterestingFamilies(interesting_go_terms).Distinct().ToList());
            if (!built) return;
            MessageBox.Show("Finished building all families.\n\nPlease load them into Cytoscape 3.0 or later using \"Tools\" -> \"Execute Command File\" and choosing the script_[TIMESTAMP].txt file in your specified directory.");
        }

        private void btn_buildFromSelectedGoTerms_Click(object sender, EventArgs e)
        {
            List<GoTermNumber> selected_gos = (DisplayUtility.get_selected_objects(dgv_goAnalysis).Select(o => (GoTermNumber)o)).ToList();
            List<ProteoformFamily> selected_families = getInterestingFamilies(selected_gos).Distinct().ToList();
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
    }
}



// METHODS NOT IN USE

//DataSet quantTables = new DataSet();

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

//private List<Protein> getInterestingProteins()
//{
//    List<ExperimentalProteoform.quantitativeValues> interestingQuantValues = qVals.Where(q => q.intensitySum > nud_intensity.Value && Math.Abs(q.logFoldChange) > nud_ratio.Value && q.pValue < nud_pValue.Value).ToList();
//    List<string> distinctAccessions = interestingQuantValues.Select(a => a.accession).ToList();

//    foreach (string acc in distinctAccessions) // here we're getting the accession numbers of the proteins linked to interesting experimental proteoforms
//    {
//        ExperimentalProteoform interestingProteoform = Lollipop.proteoform_community.experimental_proteoforms.Where(iP => iP.accession == acc).FirstOrDefault();
//        List<ProteoformFamily> famlist = new List<ProteoformFamily>();
//        if (interestingProteoform != null)
//            famlist = Lollipop.proteoform_community.families.Where(fam => fam.experimental_proteoforms.Contains(interestingProteoform)).ToList(); // proteoform families containing the selected experimental proteoform
//        List<TheoreticalProteoform> tpList = new List<TheoreticalProteoform>();
//        if (famlist.Count() > 0)
//        {
//            foreach (ProteoformFamily pf in famlist)
//            {
//                if (pf.theoretical_proteoforms.Count() > 0)
//                    tpList.AddRange(famlist.SelectMany(t => t.theoretical_proteoforms));
//            }
//        }

//        List<string> tlist = tlist = tpList.Select(tacc => tacc.accession).ToList();
//        foreach (string accession in tlist)
//        {
//            string someJunk = accession.Replace("_T", "!").Split('!').FirstOrDefault();
//            Protein p = Lollipop.proteins.FirstOrDefault(protein => protein.accession == someJunk);
//            if (p != null && !interestingProteins.Any(theoreticalProtein => theoreticalProtein.accession == p.accession))
//                interestingProteins.Add(p);
//        }

//    }
//    return interestingProteins;
//}