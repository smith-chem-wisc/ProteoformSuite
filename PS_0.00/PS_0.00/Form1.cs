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
            showForm(neuCodePairs);
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
                String filename = methodFileOpen.FileName;
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
                            if (GlobalData.deconResultsFileNames.Count > 0)
                            {
                                var response = MessageBox.Show("Would you like to use the files specified in LoadDeconvolution rather than those referenced in the method file?",
                                    "Multiple Deconvolution File References", MessageBoxButtons.YesNoCancel);
                                if (response == DialogResult.Yes) { break; }
                                if (response == DialogResult.No) { GlobalData.deconResultsFileNames.Clear(); }
                                if (response == DialogResult.Cancel) { return; }
                            }
                            loadDeconvolutionResults.loadSetting(setting_specs);
                            break;
                        case "RawExperimentalComponents":
                            rawExperimentalComponents.loadSetting(setting_specs);
                            break;
                        case "NeuCodePairs":
                            neuCodePairs.loadSetting(setting_specs);
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
                MessageBox.Show("Successfully loaded method. Will run the method now.\n\nWill show as non-responsive.");
                rawExperimentalComponents.pull_raw_experimental_components();
                neuCodePairs.NeuCodePairs_Load(neuCodePairs, null);
                aggregatedProteoforms.AggregatedProteoforms_Load(aggregatedProteoforms, null);
                theoreticalDatabase.make_databases();
                experimentalTheoreticalComparison.run_comparison();
                experimentDecoyComparison.run_comparison();
                experimentExperimentComparison.run_comparison();
                decoyDecoyComparison.run_comparison();
                proteoformFamilyAssignment.ProteoformFamilyAssignment_Load(proteoformFamilyAssignment, null);
                MessageBox.Show("Successfully ran method. Feel free to explore using the Processing Phase menu.");
            }
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
