using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using ProteoformSuiteInternal;
using System.Windows.Forms.DataVisualization.Charting;

namespace ProteoformSuiteGUI
{
    public partial class ProteoformSweet : Form
    {
        //  Initialize Forms START
        public LoadDeconvolutionResults loadDeconvolutionResults = new LoadDeconvolutionResults();
        public RawExperimentalComponents rawExperimentalComponents = new RawExperimentalComponents();
        public NeuCodePairs neuCodePairs = new NeuCodePairs();
        public AggregatedProteoforms aggregatedProteoforms = new AggregatedProteoforms();
        public TheoreticalDatabase theoreticalDatabase = new TheoreticalDatabase();
        public ExperimentTheoreticalComparison experimentalTheoreticalComparison = new ExperimentTheoreticalComparison();
        public ExperimentExperimentComparison experimentExperimentComparison = new ExperimentExperimentComparison();
        public ProteoformFamilies proteoformFamilies = new ProteoformFamilies();
        public Quantification quantification = new Quantification();
        public ResultsSummary resultsSummary = new ResultsSummary();
        List<Form> forms;
        //  Initialize Forms END

        FolderBrowserDialog resultsFolderOpen = new FolderBrowserDialog();
        OpenFileDialog methodFileOpen = new OpenFileDialog();
        SaveFileDialog methodFileSave = new SaveFileDialog();
        SaveFileDialog saveDialog = new SaveFileDialog();

        Form current_form;

        public static bool run_when_form_loads;

        public ProteoformSweet()
        {
            InitializeComponent();
            InitializeForms();
            this.WindowState = FormWindowState.Maximized;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            showForm(loadDeconvolutionResults);
            methodFileOpen.Filter = "Method XML File (*.xml)| *.xml";
            methodFileSave.DefaultExt = ".xml";
            methodFileSave.Filter = "Method XML File (*.xml)| *.xml";
        }

        public void InitializeForms()
        {
            forms = new List<Form>(new Form[] {
                loadDeconvolutionResults, rawExperimentalComponents, neuCodePairs, aggregatedProteoforms,
                theoreticalDatabase, experimentalTheoreticalComparison, experimentExperimentComparison,
                proteoformFamilies, quantification, resultsSummary
            });
            foreach (Form form in forms)
            {
                form.MdiParent = this;
            }
        }

        private void showForm(Form form)
        {
            form.Show();
            form.WindowState = FormWindowState.Maximized;
            current_form = form;
        }
        

        // RESULTS TOOL STRIP
        public void loadDeconvolutionResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(loadDeconvolutionResults);
        }
        private void rawExperimentalProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(rawExperimentalComponents);
            rawExperimentalComponents.load_raw_components();
        }
        private void neuCodeProteoformPairsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(neuCodePairs);
            neuCodePairs.display_neucode_pairs();
        }
        private void aggregatedProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(aggregatedProteoforms);
            if (run_when_form_loads) aggregatedProteoforms.aggregate_proteoforms();
        }
        private void theoreticalProteoformDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            theoreticalDatabase.reload_database_list();
            showForm(theoreticalDatabase);
        }
        private void experimentTheoreticalComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(experimentalTheoreticalComparison);
            if (run_when_form_loads) experimentalTheoreticalComparison.compare_et();
        }
        private void experimentExperimentComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(experimentExperimentComparison);
            if (run_when_form_loads) experimentExperimentComparison.compare_ee();
        }
        private void proteoformFamilyAssignmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(proteoformFamilies);
            proteoformFamilies.construct_families();
        }
        private void quantificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (run_when_form_loads) quantification.perform_calculations();
            quantification.initialize_every_time();
            showForm(quantification);
        }
        private void resultsSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resultsSummary.create_summary();
            showForm(resultsSummary);
        }


        // FILE TOOL STRIP
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("printToolStripMenuItem_Click");
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


        // METHOD TOOL STRIP
        private void saveMethodToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.methodFileSave.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
                saveMethod(methodFileSave.FileName);
        }

        private void saveMethod(string method_filename)
        {
            using (StreamWriter file = new StreamWriter(method_filename))
                file.WriteLine(SaveState.save_method());
        }

        private void loadSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            load_method();
        }

        private bool load_method()
        {
            DialogResult dr = this.methodFileOpen.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string method_filename = methodFileOpen.FileName;
                SaveState.open_method(File.ReadAllLines(method_filename));
                return true;
            }
            return false;
        }

        private void loadRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Lollipop.input_files.Count == 0)
            {
                MessageBox.Show("Please load in deconvolution result files in order to use load and run.");
                return;
            }
            var result = MessageBox.Show("Choose a method file.", "Method Load and Run", MessageBoxButtons.OKCancel);
            if (result == DialogResult.Cancel) return;

            if (!load_method()) return;
            MessageBox.Show("Successfully loaded method. Will run the method now.");

            if (full_run()) MessageBox.Show("Successfully ran method. Feel free to explore using the Results menu.");
            else MessageBox.Show("Method did not successfully run.");
         }

        public bool full_run()
        {
            clear_lists();
            if (Lollipop.get_files(Lollipop.input_files, Purpose.ProteinDatabase).Count() <= 0)
            {
                MessageBox.Show("Please list at least one protein database and at least one PTM list.");
                return false;
            }

            this.Cursor = Cursors.WaitCursor;
            rawExperimentalComponents.load_raw_components(); //also loads the theoretical database, now
            rawExperimentalComponents.preloaded = true;
            neuCodePairs.preloaded = true;
            aggregatedProteoforms.aggregate_proteoforms();
            this.enable_neuCodeProteoformPairsToolStripMenuItem(Lollipop.neucode_labeled);
            this.Cursor = Cursors.Default;
            return true;
        }
    

        // MISCELLANEOUS
        public void clear_lists()
        {
            Lollipop.raw_experimental_components.Clear();
            Lollipop.raw_neucode_pairs.Clear();
            Lollipop.proteoform_community.experimental_proteoforms = new ExperimentalProteoform[0];
            Lollipop.proteoform_community.theoretical_proteoforms = new TheoreticalProteoform[0];
            Lollipop.et_relations.Clear();
            Lollipop.et_peaks.Clear();
            Lollipop.ee_relations.Clear();
            Lollipop.ee_peaks.Clear();
            Lollipop.proteoform_community.families.Clear();
            Lollipop.ed_relations.Clear();
            Lollipop.proteoform_community.relations_in_peaks.Clear();
            Lollipop.proteoform_community.delta_mass_peaks.Clear();
            Lollipop.ef_relations.Clear();
            Lollipop.proteoform_community.decoy_proteoforms.Clear();
        }

        public void enable_neuCodeProteoformPairsToolStripMenuItem(bool setting)
        {
            neuCodeProteoformPairsToolStripMenuItem.Enabled = setting;
        }

        public void display_resultsMenu()
        {
            resultsToolStripMenuItem.ShowDropDown();
        }

        public void display_methodMenu()
        {
            runMethodToolStripMenuItem.ShowDropDown();
        }

        private void exportTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            export_table();
        }

        private void export_table()
        {
            if (current_form == rawExperimentalComponents)
            {
                SaveExcelFile(new List<DataGridView> (){ rawExperimentalComponents.GetDGV() }, "raw_experimental_components_table.xlsx");
            }
            if (current_form == neuCodePairs)
            {
                SaveExcelFile(new List<DataGridView>() { neuCodePairs.GetDGV() }, "neucode_pairs_table.xlsx");
            }
            if (current_form == aggregatedProteoforms)
            {
                SaveExcelFile(new List<DataGridView>() { aggregatedProteoforms.GetDGV() } , "aggregated_proteoforms_table.xlsx");
           }
            if (current_form == theoreticalDatabase)
            {
                SaveExcelFile(new List<DataGridView>() { theoreticalDatabase.GetDGV() }, "theoretical_database_table.xlsx");
            }
            if ( current_form == experimentalTheoreticalComparison)
            {
                SaveExcelFile(new List<DataGridView>() { experimentalTheoreticalComparison.GetETRelationsDGV(), experimentalTheoreticalComparison.GetETPeaksDGV() }, "experimental_theoretical_comparison_table.xlsx");
            }
            if ( current_form == experimentExperimentComparison)
            {
                SaveExcelFile(new List<DataGridView>(){ experimentExperimentComparison.GetEERelationDGV(), experimentExperimentComparison.GetEEPeaksDGV()}, "experiment_experiment_comparison_table.xlsx");
            }
            if (current_form == proteoformFamilies)
            {
                SaveExcelFile(new List<DataGridView>() { proteoformFamilies.GetDGV() }, "proteoform_families_table.xlsx");
            }
            if (current_form == quantification)
            {
                SaveExcelFile(new List<DataGridView>() { quantification.Get_GoTerms_DGV(), quantification.Get_quant_results_DGV() }, "quantification_table.xlsx");
            }
        }

        public void SaveExcelFile(List<DataGridView> dgvs, string filename)
        {
            saveDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            saveDialog.FileName = filename;
            DialogResult dr = this.saveDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                DGVExcelWriter writer = new DGVExcelWriter();
                writer.ExportToExcel(dgvs, saveDialog.FileName);
                MessageBox.Show("Successfully exported table.");
            }
            else return; 
        }

        public void save_all_plots(string folder, string timestamp)
        {
            if (Lollipop.raw_neucode_pairs.Count > 0) save_as_png(neuCodePairs.ct_IntensityRatio, folder, "NeuCode_IntensityRatios_", timestamp);
            if (Lollipop.raw_neucode_pairs.Count > 0) save_as_png(neuCodePairs.ct_LysineCount, folder, "NeuCode_LysineCounts_", timestamp);
            if (Lollipop.et_relations.Count > 0) save_as_png(experimentalTheoreticalComparison.ct_ET_Histogram, folder, "ExperimentalTheoretical_MassDifferences_", timestamp);
            if (Lollipop.ee_relations.Count > 0) save_as_png(experimentExperimentComparison.ct_EE_Histogram, folder, "ExperimentalExperimental_MassDifferences_", timestamp);
            if (Lollipop.qVals.Count > 0) save_as_png(quantification.ct_proteoformIntensities, folder, "QuantifiedProteoform_Intensities_", timestamp);
            if (Lollipop.qVals.Count > 0) save_as_png(quantification.ct_relativeDifference, folder, "QuantifiedProteoform_Tusher2001Plot_", timestamp);
            if (Lollipop.qVals.Count > 0) save_as_png(quantification.ct_volcano_logFold_logP, folder, "QuantifiedProteoform_VolcanoPlot_", timestamp);
        }

        private void save_as_png(Chart ct, string folder, string prefix, string timestamp)
        {
            ct.SaveImage(Path.Combine(folder, prefix + timestamp + ".png"), ChartImageFormat.Png);
        }
    }
}
