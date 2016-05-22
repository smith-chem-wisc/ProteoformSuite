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

namespace PS_0._00
{
    public partial class TheoreticalDatabase : Form
    {
        OpenFileDialog openFileDialog2 = new OpenFileDialog();
        OpenFileDialog openFileDialog3 = new OpenFileDialog();
        Protein[] proteinRawInfo = null;

        public TheoreticalDatabase()
        {
            InitializeComponent();
        }

        public void TheoreticalDatabase_Load(object sender, EventArgs e)
        {
            InitializeOpenFileDialog2();
            InitializeOpenFileDialog3();
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            ckbx_OxidMeth.Checked = Lollipop.methionine_oxidation;
            ckbx_Carbam.Checked = Lollipop.carbamidomethylation;
            ckbx_Meth_Cleaved.Checked = Lollipop.methionine_cleavage;

            btn_NeuCode_Lt.Checked = Lollipop.neucode_light_lysine;

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

        private void InitializeOpenFileDialog2()
        {
            // Set the file dialog to filter for graphics files.
            this.openFileDialog2.Filter = "UniProt XML (*.xml, *.xml.gz)|*.xml;*.xml.gz";
            // Allow the user to select multiple images.
            this.openFileDialog2.Multiselect = false;
            this.openFileDialog2.Title = "UniProt XML Format Database";
        }

        private void InitializeOpenFileDialog3()
        {
            // Set the file dialog to filter for graphics files.
            this.openFileDialog3.Filter = "UniProt PTM List (*.txt)|*.txt";
            // Allow the user to select multiple images.
            this.openFileDialog3.Multiselect = false;
            this.openFileDialog3.Title = "UniProt PTM List";
        }

        private void FillDataBaseTable(string table)
        {
            BindingSource bs = new BindingSource();
            if (table == "Target") bs.DataSource = Lollipop.theoretical_proteoforms;
            else if (Lollipop.decoy_proteoforms.ContainsKey(table)) bs.DataSource = Lollipop.decoy_proteoforms[table];
            dgv_Database.DataSource = bs;
            dgv_Database.ReadOnly = true;
            //dgv_Database.Columns["Mass"].DefaultCellStyle.Format = "0.####";
            //dgv_Database.Columns["PTM Group Mass"].DefaultCellStyle.Format = "0.####";
            //dgv_Database.Columns["Proteoform Mass"].DefaultCellStyle.Format = "0.####";
            dgv_Database.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_Database.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }

        private void btn_GetUniProtXML_Click(object sender, EventArgs e)
        {
            DialogResult dr = this.openFileDialog2.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                String uniprotXmlFile = openFileDialog2.FileName;
                try
                {
                    tb_UniProtXML_Path.Text = uniprotXmlFile;
                    Lollipop.uniprot_xml_filepath = uniprotXmlFile;
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
            DialogResult dr = this.openFileDialog3.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                String ptmlist_filename = openFileDialog3.FileName;
                try
                {
                    tb_UniProtPtmList_Path.Text = ptmlist_filename;
                    Lollipop.ptmlist_filepath = ptmlist_filename;
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

        private void btn_Make_Databases_Click(object sender, EventArgs e)
        {
            Lollipop.get_theoretical_proteoforms();

            BindingList<string> bindinglist = new BindingList<string>() { "Target" };
            BindingSource bindingSource = new BindingSource();
            bindingSource.DataSource = bindinglist;
            cmbx_DisplayWhichDB.DataSource = bindingSource;
            //Add the new proteoform databases to the bindingList, and then display
            foreach (string decoy_tablename in Lollipop.decoy_proteoforms.Keys)
            {
                bindinglist.Add(decoy_tablename);
                //cmbx_DisplayWhichDB.Items.Add(dt.TableName[0].ToString());
            }

            FillDataBaseTable(cmbx_DisplayWhichDB.SelectedItem.ToString());
        }

        public void loadSetting(string setting_specs)
        {
            string[] fields = setting_specs.Split('\t');
            switch (fields[0].Split('|')[1])
            {
                case "tb_UniProtXML_Path.Text":
                    tb_UniProtXML_Path.Text = fields[1];
                    break;
                case "tb_UniProtPtmList_Path.Text":
                    tb_UniProtPtmList_Path.Text = fields[1];
                    break;
                case "ckbx_OxidMeth.Checked":
                    ckbx_OxidMeth.Checked = Convert.ToBoolean(fields[1]);
                    break;
                case "ckbx_Carbam.Checked":
                    ckbx_Carbam.Checked = Convert.ToBoolean(fields[1]);
                    break;
                case "ckbx_Meth_Cleaved.Checked":
                    ckbx_Meth_Cleaved.Checked = Convert.ToBoolean(fields[1]);
                    break;
                case "btn_NeuCode_Lt.Checked":
                    btn_NeuCode_Lt.Checked = Convert.ToBoolean(fields[1]);
                    break;
                case "btn_NeuCode_Hv.Checked":
                    btn_NeuCode_Hv.Checked = Convert.ToBoolean(fields[1]);
                    break;
                case "btn_NaturalIsotopes.Checked":
                    btn_NaturalIsotopes.Checked = Convert.ToBoolean(fields[1]);
                    break;
                case "ckbx_aggregateProteoforms.Checked":
                    ckbx_aggregateProteoforms.Checked = Convert.ToBoolean(fields[1]);
                    break;
                case "nUD_MaxPTMs.Value":
                    nUD_MaxPTMs.Value = Convert.ToDecimal(fields[1]);
                    break;
                case "nUD_NumDecoyDBs.Value":
                    nUD_NumDecoyDBs.Value = Convert.ToDecimal(fields[1]);
                    break;
                case "nUD_MinPeptideLength.Value":
                    nUD_MinPeptideLength.Value = Convert.ToDecimal(fields[1]);
                    break;
            }
        }

        private void cmbx_DisplayWhichDB_SelectedIndexChanged(object sender, EventArgs e)
        {
            FillDataBaseTable(cmbx_DisplayWhichDB.SelectedItem.ToString());
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
    }
}
