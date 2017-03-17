using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public partial class TheoreticalDatabase : Form
    {
        OpenFileDialog openAccessionListDialog = new OpenFileDialog();
        bool initial_load = true;

        public TheoreticalDatabase()
        {
            InitializeComponent();
        }

        public void TheoreticalDatabase_Load(object sender, EventArgs e)
        {
            InitializeAccessionListDialog();
            InitializeSettings();
            if (Lollipop.opened_results_originally) load_dgv();
            initial_load = false;
        }

        public void load_dgv()
        {
            DisplayUtility.FillDataGridView(dgv_Database, Lollipop.proteoform_community.theoretical_proteoforms);
            this.initialize_table_bindinglist();
            DisplayUtility.FormatTheoreticalProteoformTable(dgv_Database);
        }

        public DataGridView GetDGV()
        {
            return dgv_Database;
        }

        private void InitializeSettings()
        {
            if (Lollipop.neucode_labeled)
                btn_NeuCode_Lt.Checked = true;
            else
            {
                btn_NeuCode_Lt.Checked = false;
                btn_NaturalIsotopes.Checked = true;
            }
            
            tb_interest_label.Text = "Type a label here. Ex: mitochondrial proteins";

            ckbx_OxidMeth.Checked = Lollipop.methionine_oxidation;
            ckbx_Carbam.Checked = Lollipop.carbamidomethylation;
            ckbx_Meth_Cleaved.Checked = Lollipop.methionine_cleavage;

            nUD_MaxPTMs.Minimum = 0;
            nUD_MaxPTMs.Maximum = 5;
            nUD_MaxPTMs.Value = Lollipop.max_ptms;

            nUD_NumDecoyDBs.Minimum = 0;
            nUD_NumDecoyDBs.Maximum = 50;
            nUD_NumDecoyDBs.Value = Lollipop.decoy_databases;

            nUD_MinPeptideLength.Minimum = 0;
            nUD_MinPeptideLength.Maximum = 20;
            nUD_MinPeptideLength.Value = Lollipop.min_peptide_length;

            ckbx_combineIdenticalSequences.Checked = Lollipop.combine_identical_sequences;
            ckbx_combineTheoreticalsByMass.Checked = Lollipop.combine_theoretical_proteoforms_byMass;

            tb_tableFilter.TextChanged -= tb_tableFilter_TextChanged;
            tb_tableFilter.Text = "";
            tb_tableFilter.TextChanged += tb_tableFilter_TextChanged;
        }


        public void FillDataBaseTable(string table)
        {
            if (table == "Target")
                DisplayUtility.FillDataGridView(dgv_Database, Lollipop.proteoform_community.theoretical_proteoforms);
            else if (Lollipop.proteoform_community.decoy_proteoforms.ContainsKey(table))
                DisplayUtility.FillDataGridView(dgv_Database, Lollipop.proteoform_community.decoy_proteoforms[table]);
        }

        // GENES OF INTEREST
        private void InitializeAccessionListDialog()
        {
            this.openAccessionListDialog.Filter = "List of Proteins of Interest (*.txt)|*.txt";
            this.openAccessionListDialog.Multiselect = false;
            this.openAccessionListDialog.Title = "List of Proteins of Interest";
        }

        private void bt_genes_of_interest_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Please select a text file with the accession numbers of your proteins of interest, one per line.");
            DialogResult dr = this.openAccessionListDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string accessions_path = openAccessionListDialog.FileName;
                try
                {
                    tb_proteins_of_interest_path.Text = accessions_path;
                    Lollipop.accessions_of_interest_list_filepath = accessions_path;
                    tb_interest_label.Visible = true;
                }
                catch (SecurityException ex)
                {
                    // The user lacks appropriate permissions to read files, discover paths, etc.
                    MessageBox.Show("Security error. Please contact your administrator for details.\n\nError message: " + ex.Message + "\n\n" +
                        "Details (send to Support):\n\n" + ex.StackTrace);
                }
                catch (Exception ex)
                {
                    // Could not load the result file - probably related to Windows file system permissions.
                    MessageBox.Show("Cannot display the file: " + accessions_path.Substring(accessions_path.LastIndexOf('\\'))
                        + ". You may not have permission to read the file, or it may be corrupt.\n\nReported error: " + ex.Message);
                }
            }
        }

        private void set_Make_Database_Button()
        {
            btn_Make_Databases.Enabled = Lollipop.get_files(Lollipop.input_files, Purpose.PtmList).Count() > 0 && Lollipop.get_files(Lollipop.input_files, Purpose.ProteinDatabase).Count() > 0;
        }

        private void btn_Make_Databases_Click(object sender, EventArgs e)
        {
            make_databases(); 
            DisplayUtility.FillDataGridView(dgv_Database, Lollipop.proteoform_community.theoretical_proteoforms);
            this.initialize_table_bindinglist();
            DisplayUtility.FormatTheoreticalProteoformTable(dgv_Database);
        }

        public void make_databases()
        {
            Lollipop.get_theoretical_proteoforms();
        }

        public void initialize_table_bindinglist()
        {
            List<string> databases = new List<string> { "Target" };
            if (Lollipop.proteoform_community.decoy_proteoforms.Keys.Count > 0)
                foreach (string name in Lollipop.proteoform_community.decoy_proteoforms.Keys)
                    databases.Add(name);
            cmbx_DisplayWhichDB.DataSource = new BindingList<string>(databases.ToList());
        }

        private void cmbx_DisplayWhichDB_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!initial_load)
            {
                string table = cmbx_DisplayWhichDB.SelectedItem.ToString();
                if (table == "Target")
                    DisplayUtility.FillDataGridView(dgv_Database, Lollipop.proteoform_community.theoretical_proteoforms);
                else
                    DisplayUtility.FillDataGridView(dgv_Database, Lollipop.proteoform_community.decoy_proteoforms[table]);
            }
        }


        // ADD AND CLEAR
        private void btn_addFiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = cmb_loadTable.SelectedItem.ToString();
            openFileDialog.Filter = Lollipop.file_filters[cmb_loadTable.SelectedIndex];
            openFileDialog.Multiselect = true;

            DialogResult dr = openFileDialog.ShowDialog();
            if (dr == DialogResult.OK)
                Lollipop.enter_input_files(openFileDialog.FileNames, Lollipop.acceptable_extensions[cmb_loadTable.SelectedIndex], Lollipop.file_types[cmb_loadTable.SelectedIndex], Lollipop.input_files);

            DisplayUtility.FillDataGridView(dgv_loadFiles, Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb_loadTable.SelectedIndex]));
            set_Make_Database_Button();
        }

        private void btn_clearFiles_Click(object sender, EventArgs e)
        {
            Lollipop.input_files = Lollipop.input_files.Except(Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb_loadTable.SelectedIndex])).ToList();
            DisplayUtility.FillDataGridView(dgv_loadFiles, Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb_loadTable.SelectedIndex]));
            set_Make_Database_Button();
        }


        // CHECKBOXES
        private void ckbx_combineIdenticalSequences_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.combine_identical_sequences = ckbx_combineIdenticalSequences.Checked;
        }

        private void ckbx_combineTheoreticalsByMass_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.combine_theoretical_proteoforms_byMass = ckbx_combineTheoreticalsByMass.Checked;
        }

        private void ckbx_OxidMeth_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.methionine_oxidation = ckbx_OxidMeth.Checked;
        }

        private void ckbx_Carbam_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.carbamidomethylation = ckbx_Carbam.Checked;
        }

        private void ckbx_Meth_Cleaved_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.methionine_cleavage = ckbx_Meth_Cleaved.Checked;
        }

        private void btn_NaturalIsotopes_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.natural_lysine_isotope_abundance = btn_NaturalIsotopes.Checked;
        }

        private void btn_NeuCode_Lt_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.neucode_light_lysine = btn_NeuCode_Lt.Checked;
        }

        private void btn_NeuCode_Hv_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.neucode_heavy_lysine = btn_NeuCode_Hv.Checked;
        }

        private void nUD_MaxPTMs_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.max_ptms = Convert.ToInt32(nUD_MaxPTMs.Value);
        }

        private void nUD_NumDecoyDBs_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.decoy_databases = Convert.ToInt32(nUD_NumDecoyDBs.Value);
        }

        private void nUD_MinPeptideLength_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.min_peptide_length = Convert.ToInt32(nUD_MinPeptideLength.Value);
        }

        private void tb_interest_label_TextChanged(object sender, EventArgs e)
        {
            if (!initial_load)
                Lollipop.interest_type = tb_interest_label.Text;
        }

        private void dgv_ptmLists_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            enter_input_files(files, new List<string> { ".txt" }, Purpose.Identification);

            //DisplayUtility.FillDataGridView(dgv_identificationFiles, Lollipop.identification_files());
        }
        private void enter_input_files(string[] files, IEnumerable<string> acceptable_extensions, Purpose purpose)
        {
            foreach (string enteredFile in files)
            {
                //string path = Path.GetDirectoryName(enteredFile);
                //string filename = Path.GetFileNameWithoutExtension(enteredFile);
                //string extension = Path.GetExtension(enteredFile);
                //Labeling label = Labeling.Unlabeled;
                //if (btn_neucode.Checked) label = Labeling.NeuCode;

                //if (acceptable_extensions.Contains(extension) && !Lollipop.input_files.Where(f => f.purpose == purpose).Any(f => f.filename == filename))
                //{
                //    reload_dgvs();

                //    InputFile file = new InputFile(path, filename, extension, label, purpose);
                //    Lollipop.input_files.Add(file);
                //}
            }
        }


        // LOAD DATABASES GRID VIEW
        private void cmb_loadTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_loadTable.SelectedIndex != 2) MessageBox.Show("Use the Load Deconvolution Results page to load data.");
            cmb_loadTable.SelectedIndex = 2;
        }

        private void dgv_loadFiles_DragDrop(object sender, DragEventArgs e)
        {
            drag_drop(e, cmb_loadTable, dgv_loadFiles);
        }

        private void drag_drop(DragEventArgs e, ComboBox cmb, DataGridView dgv)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Lollipop.enter_input_files(files, Lollipop.acceptable_extensions[cmb.SelectedIndex], Lollipop.file_types[cmb.SelectedIndex], Lollipop.input_files);
            DisplayUtility.FillDataGridView(dgv, Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb.SelectedIndex]));
        }

        public void reload_database_list()
        {
            cmb_loadTable.Items.Clear();
            cmb_loadTable.Items.AddRange(Lollipop.file_lists);
            cmb_loadTable.SelectedIndex = 2;
            DisplayUtility.FillDataGridView(dgv_loadFiles, Lollipop.get_files(Lollipop.input_files, Lollipop.file_types[cmb_loadTable.SelectedIndex]));
            set_Make_Database_Button();
        }

        private void tb_tableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_theoreticals = tb_tableFilter.Text == "" ?
                Lollipop.proteoform_community.theoretical_proteoforms :
                ExtensionMethods.filter(Lollipop.proteoform_community.theoretical_proteoforms, tb_tableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_Database, selected_theoreticals);
            if (selected_theoreticals.Count() > 0) DisplayUtility.FormatTheoreticalProteoformTable(dgv_Database);
        }
    }
}
