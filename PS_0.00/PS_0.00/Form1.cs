using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace PS_0._00
{
    public partial class Form1 : Form
    {
        //  Initialize Forms START
        LoadDeconvolutionResults loadDeconvolutionResults = new LoadDeconvolutionResults();
        RawExperimentalComponents rawExperimentalComponents = new RawExperimentalComponents();
        NeuCodePairs neuCodePairs = new NeuCodePairs();
        AggregatedProteoforms aggregatedProteoforms = new AggregatedProteoforms();
        TheoreticalDatabase theoreticalDatabase = new TheoreticalDatabase();
        ExperimentTheoreticalComparison experimentalTheoreticalComparison = new ExperimentTheoreticalComparison();
        ExperimentDecoyComparison experimentDecoyComparison = new ExperimentDecoyComparison();
        DecoyDecoyComparison decoyDecoyComparison = new DecoyDecoyComparison();
        ExperimentExperimentComparison experimentExperimentComparison = new ExperimentExperimentComparison();
        ProteoformFamilyAssignment proteoformFamilyAssignment = new ProteoformFamilyAssignment();
        List<Form> forms;
        //  Initialize Forms END

        OpenFileDialog methodFileOpen = new OpenFileDialog();
        FolderBrowserDialog resultsFolderOpen = new FolderBrowserDialog();
        SaveFileDialog methodFileSave = new SaveFileDialog();

        public Form1()
        {
            InitializeComponent();
            InitializeForms();
            this.WindowState = FormWindowState.Maximized;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        public void InitializeForms()
        {
            forms = new List<Form>(new Form[] {
                loadDeconvolutionResults, rawExperimentalComponents, neuCodePairs, aggregatedProteoforms,
                theoreticalDatabase, experimentalTheoreticalComparison, experimentDecoyComparison, experimentExperimentComparison,
                decoyDecoyComparison, proteoformFamilyAssignment
            });
        }

        private void showForm(Form form)
        {
            form.MdiParent = this;
            form.Show();
            form.WindowState = FormWindowState.Maximized;
        }

        public void loadDeconvolutionResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(loadDeconvolutionResults);
        }

        private void rawExperimentalProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(rawExperimentalComponents);
        }

        private void neuCodeProteoformPairsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GlobalData.neucodeLabeled == true)
            {
                showForm(neuCodePairs);
            }
            else
            {
                MessageBox.Show("Samples must be neucode labeled in order to view Neucode Proteoform Pairs.");
            }
        }

        private void aggregatedProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(aggregatedProteoforms);
        }

        private void theoreticalProteoformDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(theoreticalDatabase);
        }

        private void experimentTheoreticalComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(experimentalTheoreticalComparison);
        }
        
        private void experimentDecoyComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GlobalData.numDecoyDatabases > 0)
            {
                showForm(experimentDecoyComparison);
            }
            else
            {
                MessageBox.Show("Create at least 1 decoy database in Theoretical Proteoform Database in order to view Experiment - Decoy Comparison.");
            }
        }

        private void decoyDecoyComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GlobalData.numDecoyDatabases > 0)
            {
                showForm(decoyDecoyComparison);
            }
            else
            {
                MessageBox.Show("Create at least 1 decoy database in Theoretical Proteoform Database in order to view Decoy - Decoy Comparison.");
            }
        }

        private void experimentExperimentComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(experimentExperimentComparison);
        }

        private void proteoformFamilyAssignmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(proteoformFamilyAssignment);
        }
        
        private void resultsSummaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //want it to actully load each time so results are up to date with GlobalData.
            ResultsSummary resultsSummary = new ResultsSummary();
            showForm(resultsSummary);
        }

        private void saveMethod()
        {
            DialogResult dr = this.methodFileSave.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                String filename = methodFileSave.FileName;
                using (StreamWriter file = new StreamWriter(filename))
                {
                    foreach (Form form in forms)
                    {
                        file.WriteLine(form.ToString());
                    }
                }
            }
        }

        private void generateMethodToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var response = MessageBox.Show("This option does a dry run of the program.\n\nIt allows you to select input files and parameters to generate a method file.\n\nThen, select Load & Run from the Method drop-down menu to then perform the full run of ProteoformSuite.", "Generate Method", MessageBoxButtons.OKCancel);
            //if (response == DialogResult.OK)
            //{
            //    foreach (Form form in forms)
            //    {
            //        Show(Form);
            //        Thread t2 = new Thread(wait_message);
            //        t2.Start();
            //        t2.Join();
            //    }
            //}
        }

        //private void wait_message() { MessageBox.Show("Click OK when you are satisfied with the settings."); }

        private void saveMethodToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            saveMethod();
        }

        private void loadRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.methodFileOpen.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                bool loadDeconDialog = false;   //only want to ask for user feedback once
                bool useMethod = true;  //true if user wants to use deconvolution files specified in method
                String filename = methodFileOpen.FileName;
                GlobalData.loadAndRunMethod = filename;
                string[] lines = File.ReadAllLines(filename);
                foreach (string line in lines)
                {
                    string setting_specs = line.Trim();
                    string[] fields = line.Split('\t');
                    switch (fields[0].Split('|')[0])
                    {
                        case "AggregatedProteoforms":
                            aggregatedProteoforms.loadSetting(setting_specs);
                            break;
                        case "LoadDeconvolutionResults":
                                if (!loadDeconDialog)
                                {
                                loadDeconDialog = true; //won't ask again
                                if (GlobalData.deconResultsFileNames.Count > 0)
                                    { 
                                    var response = MessageBox.Show("Would you like to use the files specified in LoadDeconvolution rather than those referenced in the method file?",
                                        "Multiple Deconvolution File References", MessageBoxButtons.YesNoCancel);
                                    if (response == DialogResult.Yes) { useMethod = false; break; }
                                    if (response == DialogResult.No) { useMethod = true; GlobalData.deconResultsFileNames.Clear(); }
                                    if (response == DialogResult.Cancel) { useMethod = false; return; }
                                     }
                                 }
                            if (useMethod == true)
                            {
                                loadDeconvolutionResults.loadSetting(setting_specs);
                            }           
                            break;
                        case "RawExperimentalComponents":
                            rawExperimentalComponents.loadSetting(setting_specs);
                            break;
                        case "NeuCodePairs":
                            if (GlobalData.neucodeLabeled == true)
                            {
                                neuCodePairs.loadSetting(setting_specs);
                            }
                            break;
                        case "ProteoformFamilyAssignment":
                            proteoformFamilyAssignment.loadSetting(setting_specs);
                            break;
                        case "ExperimentExperimentComparison":
                            experimentExperimentComparison.loadSetting(setting_specs);
                            break;
                        case "ExperimentTheoreticalComparison":
                            experimentalTheoreticalComparison.loadSetting(setting_specs);
                            break;
                        case "ExperimentDecoyComparison":
                            experimentDecoyComparison.loadSetting(setting_specs);
                            break;
                        case "DecoyDecoyComparison":
                            decoyDecoyComparison.loadSetting(setting_specs);
                            break;
                        case "TheoreticalDatabase":
                            theoreticalDatabase.loadSetting(setting_specs);
                            break;
                    }
                }

                string working_directory;
                MessageBox.Show("Choose a results folder.");
                DialogResult results_folder = this.resultsFolderOpen.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                    working_directory = this.resultsFolderOpen.SelectedPath;
                else
                    working_directory = Path.GetDirectoryName(GlobalData.deconResultsFileNames[0]);

                MessageBox.Show("Successfully loaded method. Will run the method now.\n\nWill show as non-responsive.");
                Parallel.Invoke(
                    () => process_raw_components(working_directory),
                    () => theoreticalDatabase.make_databases()
                );
                DataToCSV(GlobalData.theoreticalAndDecoyDatabases.Tables["Target"], working_directory + "\\theoretical_target_database.csv");
                Parallel.Invoke(
                    () => experimentalTheoreticalComparison.run_comparison(),
                    () => experimentExperimentComparison.run_comparison()
                    //() => experimentDecoyComparison.run_comparison(),
                    //() => decoyDecoyComparison.run_comparison()
                );
                DataToCSV(GlobalData.experimentTheoreticalPairs, working_directory + "\\experimental_theoretical_pairs.csv");
                DataToCSV(GlobalData.etPeakList, working_directory + "\\experimental_theoretical_deltaM_peaks.csv");
                DataToCSV(GlobalData.experimentExperimentPairs, working_directory + "\\experimental_experimental_pairs.csv");
                DataToCSV(GlobalData.eePeakList, working_directory + "\\experimental_experimental_deltaM_peaks.csv");
                proteoformFamilyAssignment.AssignColumns();
                proteoformFamilyAssignment.assign_families();
                DataToCSV(GlobalData.ProteoformFamilyMetrics, working_directory + "\\proteoform_family_metrics.csv");
                MessageBox.Show("Successfully ran method and exported results to CSV files. Feel free to explore using the Processing Phase menu.");
            }
        }

        private void process_raw_components(string working_directory)
        {
            rawExperimentalComponents.pull_raw_experimental_components();
            DataToCSV(GlobalData.rawExperimentalComponents, working_directory + "\\raw_experimental_components.csv");
            if (GlobalData.neucodeLabeled == true)
            { 
                neuCodePairs.find_neucode_pairs();
                DataToCSV(GlobalData.rawNeuCodePairs, working_directory + "\\raw_neucode_pairs.csv");
            }
            aggregatedProteoforms.aggregate_proteoforms();
            DataToCSV(GlobalData.acceptableNeuCodeLightProteoforms, working_directory + "\\acceptable_neucode_light_proteoforms.csv");
            DataToCSV(GlobalData.aggregatedProteoforms, working_directory + "\\aggregated_proteoforms.csv");
        }

        private void DataToCSV(DataTable dt, string out_filename)
        {
            StringBuilder sb = new StringBuilder();

            foreach (DataColumn col in dt.Columns)
            {
                sb.Append(col.ColumnName + ',');
            }

            sb.Remove(sb.Length - 1, 1);
            sb.Append(Environment.NewLine);

            foreach (DataRow row in dt.Rows)
            {
                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    sb.Append(row[i].ToString() + ",");
                }
                sb.Append(Environment.NewLine);
            }

            // This text is added only once to the file.
            if (!File.Exists(out_filename))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(out_filename))
                {
                    sw.WriteLine("");
                }
            }

            //foreach (string tN in sb.ToString())
            //{
            //    // This text is always added, making the file longer over time
            //    // if it is not deleted.
            //    using (StreamWriter sw = File.AppendText(path))
            //    {
            //        sw.WriteLine(tN);
            //    }
            //}

            File.WriteAllText(out_filename, sb.ToString());
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("openToolStripMenuItem_Click");
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("saveToolStripMenuItem_Click");
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("saveAsToolStripMenuItem_Click");
        }

        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("printToolStripMenuItem_Click");
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
