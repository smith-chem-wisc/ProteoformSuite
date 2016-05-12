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
            FillLocation();
            this.WindowState = FormWindowState.Maximized;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        }

        public void InitializeForms()
        {
            forms = new List<Form>(new Form[] {
                loadDeconvolutionResults, rawExperimentalComponents, neuCodePairs, aggregatedProteoforms,
                theoreticalDatabase, experimentalTheoreticalComparison, experimentDecoyComparison, experimentExperimentComparison,
                proteoformFamilyAssignment
            });
        }

        public void FillLocation()
        {
            GlobalData.ModuleList[0] = "Load Deconvolution Results";
            GlobalData.ModuleList[1] = "Raw Experimental Components";
            GlobalData.ModuleList[2] = "NeuCode Proteoform Pairs";
            GlobalData.ModuleList[3] = "Aggregated Proteoforms";
//            GlobalData.ModuleList[4] = "Theoretical Proteoform Database";
  //          GlobalData.ModuleList[5] = "Experiment - Theoretical Comparison";
    //        GlobalData.ModuleList[6] = "Experiment - Experiment Comparison";
      //      GlobalData.ModuleList[7] = "Proteoform Family Assignment";
        }

        private void showForm(Form form)
        {
            form.MdiParent = this;
            form.Show();
            form.WindowState = FormWindowState.Maximized;
        }

        public void loadDeconvolutionResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GlobalData.Location==0)
            {
                GlobalData.Location++;
            }
            showForm(loadDeconvolutionResults);
        }

        private void rawExperimentalProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            if (GlobalData.Location < 1)
            {
                MessageBox.Show("Oops! Looks like you missed a step! Please go back to '" + GlobalData.ModuleList[GlobalData.Location] + "' and try again.");
                return;
            }

            if (GlobalData.deconResultsFileNames.Count().Equals(0))
            {
                MessageBox.Show("Oops! We didn't find any data... Did you forget to load your Deconvolution Results?");
                return;
            }

            if (GlobalData.Location == 1)
            {
                GlobalData.Location++;
            }

            showForm(rawExperimentalComponents);
        }

        private void neuCodeProteoformPairsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GlobalData.Location < 2)
            {
                MessageBox.Show("Oops! Looks like you missed a step! Please go back to '" + GlobalData.ModuleList[GlobalData.Location] + "' and try again.");
                return;
            }

            if (GlobalData.Location == 2)
            {
                GlobalData.Location++;
            }
            showForm(neuCodePairs);
        }

        private void aggregatedProteoformsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GlobalData.Location < 3)
            {
                MessageBox.Show("Oops! Looks like you missed a step! Please go back to '" + GlobalData.ModuleList[GlobalData.Location] + "' and try again.");
                return;
            }

            if (GlobalData.Location == 3)
            {
                GlobalData.Location++;
            }
            showForm(aggregatedProteoforms);
        }

        private void theoreticalProteoformDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (GlobalData.Location == 4)
            {
                GlobalData.Location++;
                if (GlobalData.theoreticalAndDecoyDatabases.Tables["Target"].Rows.Count != 0)
                {
                    GlobalData.Location++;
                }
            }
            showForm(theoreticalDatabase);
        }

        private void experimentTheoreticalComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (GlobalData.theoreticalAndDecoyDatabases.Tables["Target"].Rows.Count == 0)
                {
                    MessageBox.Show("Oops! Looks like you missed a step! Please go back to 'Theoretical Proteoform Database' and try again.");
                    return;
                }
            }
            catch
            {
                    MessageBox.Show("Oops! Looks like you missed a step! Please go back to 'Theoretical Proteoform Database' and try again.");
                    return;
            }
            if (GlobalData.Location < 5)
            {
                MessageBox.Show("Oops! Looks like you missed a step! Please go back to '" + GlobalData.ModuleList[GlobalData.Location] + "' and try again.");
                return;
            }

            showForm(experimentalTheoreticalComparison);
        }
        
        private void experimentDecoyComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (GlobalData.theoreticalAndDecoyDatabases.Tables["DecoyDatabase_0"].Rows.Count == 0)
                {
                    MessageBox.Show("Oops! Looks like you missed a step! Please go back to 'Theoretical Proteoform Database' and try again.");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Oops! Looks like you missed a step! Please go back to 'Theoretical Proteoform Database' and try again.");
                return;
            }
            if (GlobalData.Location < 5)
            {
                MessageBox.Show("Oops! Looks like you missed a step! Please go back to '" + GlobalData.ModuleList[GlobalData.Location] + "' and try again.");
                return;
            }
            if (GlobalData.numDecoyDatabases > 0)
            {
                showForm(experimentDecoyComparison);
            }
            else
            {
                MessageBox.Show("Create at least 1 decoy database in Theoretical Proteoform Database in order to view Experiment - Decoy Comparison.");
            }
        }

        private void experimentExperimentComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (GlobalData.theoreticalAndDecoyDatabases.Tables["Target"].Rows.Count == 0)
                {
                    MessageBox.Show("Oops! Looks like you missed a step! Please go back to 'Theoretical Proteoform Database' and try again.");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Oops! Looks like you missed a step! Please go back to 'Theoretical Proteoform Database' and try again.");
                return;
            }
            if (GlobalData.Location < 5)
            {
                MessageBox.Show("Oops! Looks like you missed a step! Please go back to '" + GlobalData.ModuleList[GlobalData.Location] + "' and try again.");
                return;
            }
            showForm(experimentExperimentComparison);
        }

        private void proteoformFamilyAssignmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (GlobalData.theoreticalAndDecoyDatabases.Tables["Target"].Rows.Count == 0)
                {
                    MessageBox.Show("Oops! Looks like you missed a step! Please go back to 'Theoretical Proteoform Database' and try again.");
                    return;
                }
            }
            catch
            {
                MessageBox.Show("Oops! Looks like you missed a step! Please go back to 'Theoretical Proteoform Database' and try again.");
                return;
            }
            if (GlobalData.Location < 5)
            {
                MessageBox.Show("Oops! Looks like you missed a step! Please go back to '" + GlobalData.ModuleList[GlobalData.Location] + "' and try again.");
                return;
            }
            if (GlobalData.experimentTheoreticalPairs.Rows.Count == 0)
            {
                MessageBox.Show("Oops! Looks like you missed a step! Please go back to '" + "Experiment - Theoretical Comparison" + "' and try again.");
                return;
            }
            if (GlobalData.experimentExperimentPairs.Rows.Count == 0)
            {
                MessageBox.Show("Oops! Looks like you missed a step! Please go back to '" + "Experiment - Experiment Comparison" + "' and try again.");
                return;
            }
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
