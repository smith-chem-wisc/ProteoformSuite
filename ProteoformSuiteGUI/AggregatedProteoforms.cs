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
            FormatAggregatesTable();
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

        private void FormatAggregatesTable()
        {
            //round table values
            dgv_AggregatedProteoforms.Columns["agg_mass"].DefaultCellStyle.Format = "0.####";
            dgv_AggregatedProteoforms.Columns["agg_intensity"].DefaultCellStyle.Format = "0.####";
            dgv_AggregatedProteoforms.Columns["agg_rt"].DefaultCellStyle.Format = "0.##";
            dgv_AggregatedProteoforms.Columns["modified_mass"].DefaultCellStyle.Format = "0.####";
            
            //set column header
            dgv_AggregatedProteoforms.Columns["agg_mass"].HeaderText = "Aggregated Mass";
            dgv_AggregatedProteoforms.Columns["agg_intensity"].HeaderText = "Aggregated Intensity";
            dgv_AggregatedProteoforms.Columns["agg_rt"].HeaderText = "Aggregated RT";
            dgv_AggregatedProteoforms.Columns["observation_count"].HeaderText = "Observation Count";
            dgv_AggregatedProteoforms.Columns["accession"].HeaderText = "Experimental Proteoform ID";
            dgv_AggregatedProteoforms.Columns["lysine_count"].HeaderText = "Lysine Count";

            //making these columns invisible. (irrelevent for agg proteoforms)
            dgv_AggregatedProteoforms.Columns["is_target"].Visible = false; 
            dgv_AggregatedProteoforms.Columns["is_decoy"].Visible = false;
            dgv_AggregatedProteoforms.Columns["modified_mass"].Visible = false;
            if (!Lollipop.neucode_labeled) { dgv_AggregatedProteoforms.Columns["lysine_count"].Visible = false; }


            dgv_AggregatedProteoforms.AllowUserToAddRows = false;
        }

        private void dgv_AggregatedProteoforms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex >= 0)
            {
                ExperimentalProteoform selected_pf = (ExperimentalProteoform)this.dgv_AggregatedProteoforms.Rows[e.RowIndex].DataBoundItem;
                DisplayUtility.FillDataGridView(dgv_AcceptNeuCdLtProteoforms, selected_pf.aggregated_components);
                Format_dgv_AcceptNeuCdLtProteoforms();
            }
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
            dgv_AcceptNeuCdLtProteoforms.Columns["intensity_ratio"].HeaderText = "Intensity Ratio";
            dgv_AcceptNeuCdLtProteoforms.Columns["lysine_count"].HeaderText = "Lysine Count";
            dgv_AcceptNeuCdLtProteoforms.Columns["accepted"].HeaderText = "Accepted";
            dgv_AcceptNeuCdLtProteoforms.Columns["intensity_sum_olcs"].HeaderText = "Intensity Sum for Overlapping Charge States";
            dgv_AcceptNeuCdLtProteoforms.Columns["id_light"].HeaderText = "ID Light";
            dgv_AcceptNeuCdLtProteoforms.Columns["id_heavy"].HeaderText = "ID Heavy";

            dgv_AcceptNeuCdLtProteoforms.AllowUserToAddRows = false;

            if (!Lollipop.neucode_labeled)
            {
                dgv_AcceptNeuCdLtProteoforms.Columns["lysine_count"].Visible = false;
                dgv_AcceptNeuCdLtProteoforms.Columns["intensity_ratio"].Visible = false;
                dgv_AcceptNeuCdLtProteoforms.Columns["intensity_sum_olcs"].Visible = false;
                dgv_AcceptNeuCdLtProteoforms.Columns["id"].Visible = false;

            }
        }

        private void updateFiguresOfMerit()
        {
            tb_totalAggregatedProteoforms.Text = Lollipop.proteoform_community.experimental_proteoforms.Count.ToString();
        }

        private void updateFiguresOfMerit()
        {
            tb_totalAggregatedProteoforms.Text = Lollipop.proteoform_community.experimental_proteoforms.Count.ToString();
        }

        private void dgv_AcceptNeuCdLtProteoforms_CellContentClick(object sender, EventArgs e)
        {
            //code for if acceptable boolean is changed by user. 
        }

        private void nUP_mass_tolerance_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.mass_tolerance = nUP_mass_tolerance.Value;
            Lollipop.aggregate_proteoforms();
            updateFiguresOfMerit();
        }

        private void nUD_RetTimeToleranace_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.retention_time_tolerance = nUD_RetTimeToleranace.Value;
            Lollipop.aggregate_proteoforms();
            updateFiguresOfMerit();
        }

        private void nUD_Missed_Monos_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.missed_monos = nUD_Missed_Monos.Value;
            Lollipop.aggregate_proteoforms();
            updateFiguresOfMerit();
        }

        private void nUD_Missed_Ks_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.missed_lysines = nUD_Missed_Ks.Value;
            Lollipop.aggregate_proteoforms();
            updateFiguresOfMerit();
        }

    }
}
