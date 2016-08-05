using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProteoformSuite
{
    public partial class AggregatedProteoforms : Form
    {
        bool initial_load = true;

        public AggregatedProteoforms()
        {
            InitializeComponent();
        }

        public void AggregatedProteoforms_Load(object sender, EventArgs e)
        {
            this.InitializeSettings();
            if (Lollipop.proteoform_community.experimental_proteoforms.Count == 0) Lollipop.aggregate_proteoforms();
            updateFiguresOfMerit();
            this.FillAggregatesTable();
            DisplayUtility.FormatAggregatesTable(dgv_AggregatedProteoforms);
            initial_load = false;
        }

        private void RunTheGamut()
        {
            ClearListsAndTables();
            Lollipop.aggregate_proteoforms();
            this.FillAggregatesTable();
            DisplayUtility.FormatAggregatesTable(dgv_AggregatedProteoforms);
            updateFiguresOfMerit();
        }

        private void InitializeSettings()
        {
            nUP_mass_tolerance.Minimum = 0;
            nUP_mass_tolerance.Maximum = 10;
            nUP_mass_tolerance.Value = Lollipop.mass_tolerance;

            nUD_RetTimeToleranace.Minimum = 0;
            nUD_RetTimeToleranace.Maximum = 10;
            nUD_RetTimeToleranace.Value = Lollipop.retention_time_tolerance;

            nUD_Missed_Monos.Minimum = 0;
            nUD_Missed_Monos.Maximum = 5;
            nUD_Missed_Monos.Value = Lollipop.missed_monos;

            nUD_Missed_Ks.Minimum = 0;
            nUD_Missed_Ks.Maximum = 3;
            nUD_Missed_Ks.Value = Lollipop.missed_lysines;
        }

        public void FillAggregatesTable()
        {
            DisplayUtility.FillDataGridView(dgv_AggregatedProteoforms, Lollipop.proteoform_community.experimental_proteoforms);
        }
     
        private void dgv_AggregatedProteoforms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && !Lollipop.opened_results_originally) display_light_proteoforms(e.RowIndex);
        }
        private void dgv_AggregatedProteoforms_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex >= 0 && !Lollipop.opened_results_originally) display_light_proteoforms(e.RowIndex);
        }
        private void display_light_proteoforms(int row_index)
        {
            ExperimentalProteoform selected_pf = (ExperimentalProteoform)this.dgv_AggregatedProteoforms.Rows[row_index].DataBoundItem;
            DisplayUtility.FillDataGridView(dgv_AcceptNeuCdLtProteoforms, selected_pf.aggregated_components);
            Format_dgv_AcceptNeuCdLtProteoforms();
        }

        private void Format_dgv_AcceptNeuCdLtProteoforms()
        {
            //round table values
            dgv_AcceptNeuCdLtProteoforms.Columns["monoisotopic_mass"].DefaultCellStyle.Format = "0.####";
            dgv_AcceptNeuCdLtProteoforms.Columns["delta_mass"].DefaultCellStyle.Format = "0.####";
            dgv_AcceptNeuCdLtProteoforms.Columns["weighted_monoisotopic_mass"].DefaultCellStyle.Format = "0.####";
            dgv_AcceptNeuCdLtProteoforms.Columns["corrected_mass"].DefaultCellStyle.Format = "0.####";
            dgv_AcceptNeuCdLtProteoforms.Columns["rt_apex"].DefaultCellStyle.Format = "0.##";
            dgv_AcceptNeuCdLtProteoforms.Columns["relative_abundance"].DefaultCellStyle.Format = "0.####";
            dgv_AcceptNeuCdLtProteoforms.Columns["fract_abundance"].DefaultCellStyle.Format = "0.####";
            dgv_AcceptNeuCdLtProteoforms.Columns["intensity_sum"].DefaultCellStyle.Format = "0.####";
            if (Lollipop.neucode_labeled) { dgv_AcceptNeuCdLtProteoforms.Columns["intensity_ratio"].DefaultCellStyle.Format = "0.####"; }


            //set column header
            dgv_AcceptNeuCdLtProteoforms.Columns["monoisotopic_mass"].HeaderText = "Monoisotopic Mass";
            dgv_AcceptNeuCdLtProteoforms.Columns["delta_mass"].HeaderText = "Delta Mass";
            dgv_AcceptNeuCdLtProteoforms.Columns["weighted_monoisotopic_mass"].HeaderText = "Weighted Monoisotopic Mass";
            dgv_AcceptNeuCdLtProteoforms.Columns["corrected_mass"].HeaderText = "Corrected Mass";
            dgv_AcceptNeuCdLtProteoforms.Columns["rt_apex"].HeaderText = "Apex RT";
            dgv_AcceptNeuCdLtProteoforms.Columns["relative_abundance"].HeaderText = "Relative Abundance";
            dgv_AcceptNeuCdLtProteoforms.Columns["fract_abundance"].HeaderText = "Fractional Abundance";
            dgv_AcceptNeuCdLtProteoforms.Columns["intensity_sum"].HeaderText = "Intensity Sum";
            dgv_AcceptNeuCdLtProteoforms.Columns["file_origin"].HeaderText = "Filename";
            dgv_AcceptNeuCdLtProteoforms.Columns["scan_range"].HeaderText = "Scan Range";
            dgv_AcceptNeuCdLtProteoforms.Columns["rt_range"].HeaderText = "RT Range";
            dgv_AcceptNeuCdLtProteoforms.Columns["num_charge_states"].HeaderText = "No. Charge States";
            dgv_AcceptNeuCdLtProteoforms.Columns["accepted"].HeaderText = "Accepted";
            dgv_AcceptNeuCdLtProteoforms.Columns["intensity_sum_olcs"].HeaderText = "Intensity Sum for Overlapping Charge States";

            dgv_AcceptNeuCdLtProteoforms.AllowUserToAddRows = false;
            dgv_AcceptNeuCdLtProteoforms.Columns["id"].Visible = false;

            if (!Lollipop.neucode_labeled)
            {
                dgv_AcceptNeuCdLtProteoforms.Columns["intensity_sum_olcs"].Visible = false;
            }
            else
            {
                dgv_AcceptNeuCdLtProteoforms.Columns["lysine_count"].HeaderText = "Lysine Count";
                dgv_AcceptNeuCdLtProteoforms.Columns["intensity_ratio"].HeaderText = "Intensity Ratio";
                dgv_AcceptNeuCdLtProteoforms.Columns["id_light"].HeaderText = "ID Light";
                dgv_AcceptNeuCdLtProteoforms.Columns["id_heavy"].HeaderText = "ID Heavy";
            }
        }

        private void updateFiguresOfMerit()
        {
            tb_totalAggregatedProteoforms.Text = Lollipop.proteoform_community.experimental_proteoforms.Count.ToString();
        }

        private void ClearListsAndTables()
        {
            Lollipop.proteoform_community.experimental_proteoforms.Clear();

            dgv_AcceptNeuCdLtProteoforms.DataSource = null;
            dgv_AcceptNeuCdLtProteoforms.Rows.Clear();
        }

        private void dgv_AcceptNeuCdLtProteoforms_CellContentClick(object sender, EventArgs e)
        {
            //code for if acceptable boolean is changed by user. 
        }

        private void nUP_mass_tolerance_ValueChanged(object sender, EventArgs e)
        {
            if (!initial_load)
            {
                Lollipop.mass_tolerance = nUP_mass_tolerance.Value;
            }
        }
        private void nUD_RetTimeToleranace_ValueChanged(object sender, EventArgs e)
        {
            if (!initial_load)
            {
                Lollipop.retention_time_tolerance = nUD_RetTimeToleranace.Value;
            }
        }
        private void nUD_Missed_Monos_ValueChanged(object sender, EventArgs e)
        {
            if (!initial_load)
            {
                Lollipop.missed_monos = nUD_Missed_Monos.Value;
            }
        }
        private void nUD_Missed_Ks_ValueChanged(object sender, EventArgs e)
        {
            if (!initial_load)
            {
                Lollipop.missed_lysines = nUD_Missed_Ks.Value;
            }
        }

        private void button_update_Click(object sender, EventArgs e)
        {
            RunTheGamut();
        }
    }
}
