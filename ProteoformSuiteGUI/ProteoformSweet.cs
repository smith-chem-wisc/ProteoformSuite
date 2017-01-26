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

namespace ProteoformSuite
{
    public partial class ProteoformSweet : Form
    {
        //  Initialize Forms START
        LoadResults loadResults = new LoadResults();
        RawExperimentalComponents rawExperimentalComponents = new RawExperimentalComponents();
        NeuCodePairs neuCodePairs = new NeuCodePairs();
        AggregatedProteoforms aggregatedProteoforms = new AggregatedProteoforms();
        TheoreticalDatabase theoreticalDatabase = new TheoreticalDatabase();
        ExperimentTheoreticalComparison experimentalTheoreticalComparison = new ExperimentTheoreticalComparison();
        ExperimentExperimentComparison experimentExperimentComparison = new ExperimentExperimentComparison();
        ProteoformFamilies proteoformFamilies = new ProteoformFamilies();
        TopDown topDown = new TopDown();
        Quantification quantification = new Quantification();
        ResultsSummary resultsSummary = new ResultsSummary();
        List<Form> forms;
        //  Initialize Forms END

        FolderBrowserDialog resultsFolderOpen = new FolderBrowserDialog();
        OpenFileDialog methodFileOpen = new OpenFileDialog();
        SaveFileDialog methodFileSave = new SaveFileDialog();
        SaveFileDialog saveDialog = new SaveFileDialog();

        Form current_form;

        public static bool run_when_form_loads = true;

        public ProteoformSweet()
        {
            InitializeComponent();
            InitializeForms();
            this.WindowState = FormWindowState.Maximized;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            showForm(loadResults);
            methodFileOpen.Filter = "Method TXT File (*.txt)| *.txt";
        }

        public void InitializeForms()
        {
            forms = new List<Form>(new Form[] {
                loadResults, rawExperimentalComponents, neuCodePairs, aggregatedProteoforms,
                theoreticalDatabase, experimentalTheoreticalComparison, experimentExperimentComparison,
                proteoformFamilies, quantification
            });
        }

        private void showForm(Form form)
        {
            form.MdiParent = this;
            form.Show();
            form.WindowState = FormWindowState.Maximized;
            current_form = form;
        }
        

        // RESULTS TOOL STRIP
        public void loadResultsToolStripMenuItem_Click(object sender, EventArgs e) { showForm(loadResults); }
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
        private void topdownResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(topDown);
            topDown.load_topdown();
        }

        private void quantificationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (run_when_form_loads) quantification.perform_calculations();
            quantification.initialize_every_time();
            showForm(quantification);
        }
        private void resultsSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resultsSummary.createResultsSummary();
            resultsSummary.displayResultsSummary();
            showForm(resultsSummary);
        }


        // FILE TOOL STRIP
        private void openAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string working_directory;
            MessageBox.Show("Please choose a folder with saved results files.");
            DialogResult results_folder = this.resultsFolderOpen.ShowDialog();
            if (results_folder == DialogResult.OK) working_directory = this.resultsFolderOpen.SelectedPath;
            else if (results_folder == DialogResult.Cancel) return;
            else return;

            SaveState.open_method(File.ReadAllLines(working_directory + "\\_method.xml"));
            
            Lollipop.opening_results = true;
            Lollipop.opened_results_originally = true;
            ResultsSummary.loadDescription = working_directory;

            var result = MessageBox.Show("Add to the existing results?", "Results Clear", MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.No) clear_lists();

            //cannot parallelize bc results dependent on one another for certain objects
            MessageBox.Show("Will load in results now.\n\nMay show as non-responsive.");
            Results.read_input_files(File.ReadAllLines(working_directory + "\\input_files.tsv"));
            Results.read_raw_components(File.ReadAllLines(working_directory + "\\raw_experimental_components.tsv"));
            if (Lollipop.neucode_labeled) Results.read_raw_neucode_pairs(File.ReadAllLines(working_directory + "\\raw_neucode_pairs.tsv"));
            Results.read_aggregated_proteoforms(File.ReadAllLines(working_directory + "\\aggregated_experimental_proteoforms.tsv"));
            //need to read in ptm list
            try
            {
                ProteomeDatabaseReader.oldPtmlistFilePath = working_directory + "\\ptmlist.txt";
                Lollipop.uniprotModificationTable = Lollipop.proteomeDatabaseReader.ReadUniprotPtmlist();
            }
            catch
            {
                try
                {
                    ProteomeDatabaseReader.oldPtmlistFilePath = working_directory + "\\ptmlist_new.txt";
                    Lollipop.uniprotModificationTable = Lollipop.proteomeDatabaseReader.ReadUniprotPtmlist();
                }
                catch
                {
                    get_ptm_list();
                }
            }
            Results.read_theoretical_proteoforms(File.ReadAllLines(working_directory + "\\theoretical_proteoforms.tsv"), true);
            Results.read_theoretical_proteoforms(File.ReadAllLines(working_directory + "\\decoy_proteoforms.tsv"), false);
            Results.read_relationships(File.ReadAllLines(working_directory + "\\experimental_theoretical_relationships.tsv"), ProteoformComparison.et);
            Results.read_relationships(File.ReadAllLines(working_directory + "\\experimental_decoy_relationships.tsv"), ProteoformComparison.ed);
            Results.read_relationships(File.ReadAllLines(working_directory + "\\experimental_experimental_relationships.tsv"), ProteoformComparison.ee);
            Results.read_relationships(File.ReadAllLines(working_directory + "\\experimental_false_relationships.tsv"), ProteoformComparison.ef);
            Results.read_peaks(File.ReadAllLines(working_directory + "\\experimental_theoretical_peaks.tsv"), ProteoformComparison.et);
            Results.read_peaks(File.ReadAllLines(working_directory + "\\experimental_experimental_peaks.tsv"), ProteoformComparison.ee);
            Results.read_families(File.ReadAllLines(working_directory + "\\proteoform_families.tsv"));
            MessageBox.Show("Files successfully read in.");

            Lollipop.opening_results = false;
        }

        private void get_ptm_list()
        {
            MessageBox.Show("Please select a Uniprot ptm list.");
            DialogResult dr = this.methodFileOpen.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string ptm_list = methodFileOpen.FileName;
                ProteomeDatabaseReader.oldPtmlistFilePath = ptm_list;
                Lollipop.ptmlist_filepath = ptm_list;
            }
            else if (dr == DialogResult.Cancel) return;
            else return;
        }

        OpenFileDialog openXmlDialog = new OpenFileDialog();
     
        private void get_uniprot_xml()
        {
            MessageBox.Show("Please select a Uniprot database.");
            openXmlDialog.Filter = "UniProt XML (*.xml, *.xml.gz)|*.xml;*.xml.gz";
            openXmlDialog.Multiselect = false;
            openXmlDialog.Title = "UniProt XML Format Database";
            DialogResult dr = this.openXmlDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
                Lollipop.uniprot_xml_filepath = openXmlDialog.FileName;
            else return;
        }

        private void openCurrentPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (current_form == loadResults)
            {
                MessageBox.Show("Please select a raw experimental components file to open.");
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Title = "Raw Experimental Components";
                openFileDialog1.Filter = "Raw Experimental Components (.tsv) | *.tsv";
                DialogResult dr = openFileDialog1.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    Labeling label = Labeling.NeuCode;
                    if (!Lollipop.neucode_labeled) label = Labeling.Unlabeled;
                    InputFile inpfile = new InputFile(1, false, 1, 1, 1, "no_condition", "no_condition", openFileDialog1.FileName, label, Purpose.Identification);
                    Lollipop.input_files.Add(inpfile);
                    try
                    {
                        Results.read_raw_components(File.ReadAllLines(openFileDialog1.FileName));
                        Lollipop.opened_raw_comps = true;
                        Lollipop.min_num_CS = 0;
                        if (Lollipop.neucode_labeled)
                        {
                            HashSet<string> scan_ranges = new HashSet<string>(Lollipop.raw_experimental_components.Select(c => c.scan_range));
                            foreach (string scan_range in scan_ranges)
                                Lollipop.find_neucode_pairs(Lollipop.raw_experimental_components.Where(c => c.scan_range == scan_range));
                        }
                    }
                    catch
                    {
                        MessageBox.Show("File format incorrect.");
                        return;
                    }
                    MessageBox.Show("Successfully read in raw experimental components.");
                }
                else { return; }
            }
            else if (current_form == theoreticalDatabase)
            {
                MessageBox.Show("Please select a theoretical proteoforms file to open.");
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Title = "Theoretical Proteoforms";
                openFileDialog1.Filter = "Theoretical Proteoforms (.tsv) | *.tsv";
                DialogResult dr = openFileDialog1.ShowDialog();
                if (dr == DialogResult.OK)
                {
                    try
                    {
                        Results.read_theoretical_proteoforms(File.ReadAllLines(openFileDialog1.FileName), true);
                    }
                    catch
                    {
                        MessageBox.Show("File format incorrect.");
                        return;
                    }
                    theoreticalDatabase.load_dgv();
                    MessageBox.Show("Successfully read in theoretical proteoforms."); 
                }
                else return; 
            }
            else
            {
                MessageBox.Show("Current page cannot be opened. Try Open All.");
            }
        }

        private void saveCurrentPageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string working_directory;
            if (current_form == loadResults) { return; }
            MessageBox.Show("Choose a results folder.");
            DialogResult results_folder = this.resultsFolderOpen.ShowDialog();
            if (results_folder == DialogResult.OK) working_directory = this.resultsFolderOpen.SelectedPath;
            else return;
            save_tsv(working_directory, false);
            MessageBox.Show("Successfully saved the currently displayed page.");
        }

        private void saveAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string working_directory;
            MessageBox.Show("Choose a folder for the method and results files.");
            DialogResult results_folder = this.resultsFolderOpen.ShowDialog();
            if (results_folder == DialogResult.OK) working_directory = this.resultsFolderOpen.SelectedPath;
            else return;
            saveMethod(working_directory + "\\_method.xml");
            save_tsv(working_directory, true);
            File.Copy(ProteomeDatabaseReader.oldPtmlistFilePath, working_directory + "\\ptmlist.txt", true);
            MessageBox.Show("Successfully saved all pages.");
        }

        private void save_tsv(string working_directory, bool save_all)
        {
            if (current_form == loadResults || save_all)
            {
                File.WriteAllText(working_directory + "\\input_files.tsv", Results.input_file_results());
            }
            if (current_form == rawExperimentalComponents || save_all)
            {
                File.WriteAllText(working_directory + "\\raw_experimental_components.tsv", Results.raw_component_results());
            }
            if (current_form == neuCodePairs || save_all)
            {
                File.WriteAllText(working_directory + "\\raw_neucode_pairs.tsv", Results.raw_neucode_pair_results());
            }
            if (current_form == aggregatedProteoforms || save_all)
            {
                File.WriteAllText(working_directory + "\\aggregated_experimental_proteoforms.tsv", Results.aggregated_experimental_proteoform_results());
            }
            if (current_form == theoreticalDatabase || save_all)
            {
                File.WriteAllText(working_directory + "\\theoretical_proteoforms.tsv", Results.theoretical_proteoforms_results(true));
                File.WriteAllText(working_directory + "\\decoy_proteoforms.tsv", Results.theoretical_proteoforms_results(false));
            }
            if (current_form == experimentalTheoreticalComparison || save_all)
            {
                File.WriteAllText(working_directory + "\\experimental_theoretical_relationships.tsv", Results.relation_results(ProteoformComparison.et));
                File.WriteAllText(working_directory + "\\experimental_decoy_relationships.tsv", Results.relation_results(ProteoformComparison.ed));
                File.WriteAllText(working_directory + "\\experimental_theoretical_peaks.tsv", Results.peak_results(ProteoformComparison.et));
            }
            if (current_form == experimentExperimentComparison || save_all)
            {
                File.WriteAllText(working_directory + "\\experimental_experimental_relationships.tsv", Results.relation_results(ProteoformComparison.ee));
                File.WriteAllText(working_directory + "\\experimental_false_relationships.tsv", Results.relation_results(ProteoformComparison.ef));
                File.WriteAllText(working_directory + "\\experimental_experimental_peaks.tsv", Results.peak_results(ProteoformComparison.ee));
            }
            if (current_form == proteoformFamilies || save_all)
            {
                File.WriteAllText(working_directory + "\\proteoform_families.tsv", Results.family_results());
            }
        }

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
                ResultsSummary.loadDescription = method_filename;
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
            if (!File.Exists(Lollipop.uniprot_xml_filepath)) get_uniprot_xml();
            if (!File.Exists(Lollipop.uniprot_xml_filepath)) return false; //user hit cancel
            if (!File.Exists(Lollipop.ptmlist_filepath)) get_ptm_list();
            if (!File.Exists(Lollipop.ptmlist_filepath)) return false; //user hit cancel
            this.Cursor = Cursors.WaitCursor;
            rawExperimentalComponents.load_raw_components();
            aggregatedProteoforms.aggregate_proteoforms();
            theoreticalDatabase.make_databases();
            Lollipop.make_et_relationships();
            Lollipop.make_ee_relationships();
            if (Lollipop.neucode_labeled) proteoformFamilies.construct_families();
            quantification.perform_calculations();
            prepare_figures_and_tables();
            this.enable_neuCodeProteoformPairsToolStripMenuItem(Lollipop.neucode_labeled);
            this.Cursor = Cursors.Default;
            return true;
        }

        private void prepare_figures_and_tables()
        {
            Parallel.Invoke
            (
                () => rawExperimentalComponents.FillRawExpComponentsTable(),
                () => aggregatedProteoforms.FillAggregatesTable(),
                () => theoreticalDatabase.FillDataBaseTable("Target"),
                () => experimentalTheoreticalComparison.FillTablesAndCharts(),
                () => experimentExperimentComparison.FillTablesAndCharts()
            );
            if (Lollipop.neucode_labeled) neuCodePairs.GraphNeuCodePairs();
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
            List<DataGridView> dgvs = new List<DataGridView>();
            if (current_form == rawExperimentalComponents)
            {
                dgvs.Add(rawExperimentalComponents.GetDGV());
                SaveExcelFile(dgvs, "raw_experimental_components_table.xlsx");
            }
            if (current_form == neuCodePairs)
            {
                dgvs.Add(neuCodePairs.GetDGV());
                SaveExcelFile(dgvs, "neucode_pairs_table.xlsx");
            }
            if (current_form == aggregatedProteoforms)
            {
                dgvs.Add(aggregatedProteoforms.GetDGV());
                SaveExcelFile(dgvs, "aggregated_proteoforms_table.xlsx");
           }
            if (current_form == theoreticalDatabase)
            {
                dgvs.Add(theoreticalDatabase.GetDGV());
                SaveExcelFile(dgvs, "theoretical_database_table.xlsx");
            }
            if ( current_form == experimentalTheoreticalComparison)
            {
                dgvs.Add(experimentalTheoreticalComparison.GetETPeaksDGV());
                dgvs.Add(experimentalTheoreticalComparison.GetETRelationsDGV());
                SaveExcelFile(dgvs, "experimental_theoretical_comparison_table.xlsx");
            }
            if ( current_form == experimentExperimentComparison)
            {
                dgvs.Add(experimentExperimentComparison.GetEEPeaksDGV());
                dgvs.Add(experimentExperimentComparison.GetEERelationDGV());
                SaveExcelFile(dgvs, "experiment_experiment_comparison_table.xlsx");
            }
            if (current_form == proteoformFamilies)
            {
                dgvs.Add(proteoformFamilies.GetDGV());
                SaveExcelFile(dgvs, "proteoform_families_table.xlsx");
            }
            if (current_form == quantification)
            {
                dgvs.Add(quantification.Get_GoTerms_DGV());
                dgvs.Add(quantification.Get_quant_results_DGV());
                SaveExcelFile(dgvs, "quantification_table.xlsx");
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
    }
}
