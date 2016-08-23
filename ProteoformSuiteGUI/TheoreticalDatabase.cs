using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace ProteoformSuite
{
    public partial class TheoreticalDatabase : Form
    {
        OpenFileDialog openXmlDialog = new OpenFileDialog();
        OpenFileDialog openPtmlistDialog = new OpenFileDialog();
        OpenFileDialog openAccessionListDialog = new OpenFileDialog();
        public bool gotPTMlistFilePath = false;
        public bool gotXMLFilePath = false;
        bool initial_load = true;

        public TheoreticalDatabase()
        {
            InitializeComponent();
        }

        public void TheoreticalDatabase_Load(object sender, EventArgs e)
        {
            btn_Make_Databases.Enabled = false;
            InitializeOpenXmlDialog();
            InitializeOpenPtmlistDialog();
            InitializeSettings();

            if (Lollipop.opened_results_originally)
            {
                DisplayUtility.FillDataGridView(dgv_Database, Lollipop.proteoform_community.theoretical_proteoforms);
                this.initialize_table_bindinglist();
                DisplayUtility.FormatTheoreticalProteoformTable(dgv_Database);
            }
            initial_load = false;
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

            ckbx_aggregateProteoforms.Checked = Lollipop.combine_identical_sequences;
        }

        private void InitializeOpenXmlDialog()
        {
            this.openXmlDialog.Filter = "UniProt XML (*.xml, *.xml.gz)|*.xml;*.xml.gz";
            this.openXmlDialog.Multiselect = false;
            this.openXmlDialog.Title = "UniProt XML Format Database";
        }

        private void InitializeOpenPtmlistDialog()
        {
            this.openPtmlistDialog.Filter = "UniProt PTM List (*.txt)|*.txt";
            this.openPtmlistDialog.Multiselect = false;
            this.openPtmlistDialog.Title = "UniProt PTM List";
        }

        private void InitializeGeneListDialog()
        {
            this.openAccessionListDialog.Filter = "List of Genes of Interest (*.txt)|*.txt";
            this.openAccessionListDialog.Multiselect = false;
            this.openAccessionListDialog.Title = "List of Genes of Interest";
        }

        public void FillDataBaseTable(string table)
        {
            if (table == "Target")
                DisplayUtility.FillDataGridView(dgv_Database, Lollipop.proteoform_community.theoretical_proteoforms);
            else if (Lollipop.proteoform_community.decoy_proteoforms.ContainsKey(table))
                DisplayUtility.FillDataGridView(dgv_Database, Lollipop.proteoform_community.decoy_proteoforms[table]);
        }

        private void btn_GetUniProtXML_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openXmlDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string uniprotXmlFile = openXmlDialog.FileName;
                try
                {
                    tb_UniProtXML_Path.Text = uniprotXmlFile;
                    Lollipop.uniprot_xml_filepath = uniprotXmlFile;
                    if (File.Exists(uniprotXmlFile))
                    {
                        gotXMLFilePath = true;
                        enable_Make_Database_Button();
                    }
                }
                catch (SecurityException ex)
                {
                    // The user lacks appropriate permissions to read files, discover paths, etc.
                    MessageBox.Show("Security error. Please contact your administrator for details.\n\nError message: " + ex.Message + "\n\n" +
                        "Details (send to Support):\n\n" + ex.StackTrace);
                    tb_UniProtXML_Path.Text = "";
                }
                catch (Exception ex)
                {
                    // Could not load the result file - probably related to Windows file system permissions.
                    MessageBox.Show("Cannot display the file: " + uniprotXmlFile.Substring(uniprotXmlFile.LastIndexOf('\\'))
                        + ". You may not have permission to read the file, or it may be corrupt.\n\nReported error: " + ex.Message);
                    tb_UniProtXML_Path.Text = "";
                }
            }
        }

        private void btn_UniPtPtmList_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openPtmlistDialog.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                string ptmlist_filename = openPtmlistDialog.FileName;
                try
                {
                    tb_UniProtPtmList_Path.Text = ptmlist_filename;
                    Lollipop.ptmlist_filepath = ptmlist_filename;
                    if (File.Exists(ptmlist_filename))
                    {
                        gotPTMlistFilePath = true;
                        enable_Make_Database_Button();
                    } 
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
                    MessageBox.Show("Cannot display the file: " + ptmlist_filename.Substring(ptmlist_filename.LastIndexOf('\\'))
                        + ". You may not have permission to read the file, or it may be corrupt.\n\nReported error: " + ex.Message);
                }
            }
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

        private void enable_Make_Database_Button()
        {
            if (gotPTMlistFilePath && gotXMLFilePath) btn_Make_Databases.Enabled = true;
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

        private void ckbx_aggregateProteoforms_CheckedChanged(object sender, EventArgs e)
        {
            Lollipop.combine_identical_sequences = ckbx_aggregateProteoforms.Checked;
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
    }
}
