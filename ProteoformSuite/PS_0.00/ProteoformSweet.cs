﻿using System;
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


namespace ProteoformSuite
{
    public partial class ProteoformSweet : Form
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
        //ProteoformFamilyAssignment proteoformFamilyAssignment = new ProteoformFamilyAssignment();
        List<Form> forms;
        //  Initialize Forms END

        FolderBrowserDialog resultsFolderOpen = new FolderBrowserDialog();
        OpenFileDialog methodFileOpen = new OpenFileDialog();
        SaveFileDialog methodFileSave = new SaveFileDialog();

        public ProteoformSweet()
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
                //proteoformFamilyAssignment
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
            if (Lollipop.decoy_databases > 0) showForm(experimentDecoyComparison);
            else MessageBox.Show("Create at least 1 decoy database in Theoretical Proteoform Database in order to view Experiment - Decoy Comparison.");
        }

        private void experimentExperimentComparisonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showForm(experimentExperimentComparison);
        }

        private void proteoformFamilyAssignmentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //showForm(proteoformFamilyAssignment);
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

        private void saveMethod()
        {
            DialogResult dr = this.methodFileSave.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string method_filename = methodFileSave.FileName;
                using (StreamWriter file = new StreamWriter(method_filename))
                    file.WriteLine(Lollipop.method_toString());
            }
        }

        private void loadRunToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.methodFileOpen.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string method_filename = methodFileOpen.FileName;
                string[] lines = File.ReadAllLines(method_filename);
                foreach (string line in lines)
                {
                    string setting_spec = line.Trim();
                    Lollipop.load_setting(setting_spec);
                }

                string working_directory;
                MessageBox.Show("Choose a results folder.");
                DialogResult results_folder = this.resultsFolderOpen.ShowDialog();
                if (results_folder == DialogResult.OK)
                    working_directory = this.resultsFolderOpen.SelectedPath;
                else
                    working_directory = Path.GetDirectoryName(Lollipop.deconResultsFileNames[0]);

                MessageBox.Show("Successfully loaded method. Will run the method now.\n\nWill show as non-responsive.");

                Parallel.Invoke( 
                    () => Lollipop.get_experimental_proteoforms(),
                    () => Lollipop.get_theoretical_proteoforms()
                );
                File.WriteAllText(working_directory + "\\raw_experimental_components.csv", Lollipop.raw_component_results());
                File.WriteAllText(working_directory + "\\raw_neucode_pairs.csv", Lollipop.raw_neucode_pair_results());
                File.WriteAllText(working_directory + "\\aggregated_experimental_proteoforms.csv", Lollipop.aggregated_experimental_proteoform_results());

                Lollipop.make_et_relationships();
                File.WriteAllText(working_directory + "\\experimental_theoretical_relationships.csv", Lollipop.et_relations_results());
                File.WriteAllText(working_directory + "\\experimental_theoretical_peaks.csv", Lollipop.et_peak_results());
                Lollipop.et_relations.Clear(); Lollipop.et_relations.Capacity = 0;
                Lollipop.et_peaks.Clear(); Lollipop.et_peaks.Capacity = 0;
                Lollipop.ed_relations.Clear();

                Lollipop.make_ee_relationships();
                File.WriteAllText(working_directory + "\\experimental_experimental_relationships.csv", Lollipop.ee_relations_results());
                File.WriteAllText(working_directory + "\\experimental_experimental_peaks.csv", Lollipop.ee_peak_results());
                Lollipop.ee_relations.Clear(); Lollipop.ee_relations.Capacity = 0;
                Lollipop.ee_peaks.Clear(); Lollipop.ee_peaks.Capacity = 0;
                Lollipop.ef_relations.Clear(); Lollipop.ef_relations.Capacity = 0;

                //Lollipop.proteoform_community.construct_families();
                MessageBox.Show("Successfully ran method. Feel free to explore using the Processing Phase menu.");
            }
        }

        private void prepare_tables()
        {
            Parallel.Invoke(
                () => rawExperimentalComponents.FillRawExpComponentsTable(),
                () => neuCodePairs.FillNeuCodePairsDGV(),
                () => aggregatedProteoforms.FillAggregatesTable(),
                () => theoreticalDatabase.FillDataBaseTable("Target")
            );
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
