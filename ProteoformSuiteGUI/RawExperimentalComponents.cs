using ProteoformSuiteInternal;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace ProteoformSuite
{
    public partial class RawExperimentalComponents : Form
    {
        public RawExperimentalComponents()
        {
            InitializeComponent();
        }

        public void RawExperimentalComponents_Load(object sender, EventArgs e)
        {
            load_raw_components();
        }

        public void load_raw_components()
        {
            if (Lollipop.raw_experimental_components.Count == 0)
                Lollipop.process_raw_components(); //Includes reading correction factors if present
            this.FillRawExpComponentsTable();
        }

        public void FillRawExpComponentsTable()
        {
            DisplayUtility.FillDataGridView(dgv_RawExpComp_MI_masses, Lollipop.raw_experimental_components);
            this.FormatRawExpComponentsTable();
        }

        private void FormatRawExpComponentsTable()
        {
            //round table values
            dgv_RawExpComp_MI_masses.Columns["monoisotopic_mass"].DefaultCellStyle.Format = "0.####";
            dgv_RawExpComp_MI_masses.Columns["delta_mass"].DefaultCellStyle.Format = "0.####";
            dgv_RawExpComp_MI_masses.Columns["weighted_monoisotopic_mass"].DefaultCellStyle.Format = "0.####";
            dgv_RawExpComp_MI_masses.Columns["corrected_mass"].DefaultCellStyle.Format = "0.####";
            dgv_RawExpComp_MI_masses.Columns["rt_apex"].DefaultCellStyle.Format = "0.##";
            dgv_RawExpComp_MI_masses.Columns["relative_abundance"].DefaultCellStyle.Format = "0.####";
            dgv_RawExpComp_MI_masses.Columns["fract_abundance"].DefaultCellStyle.Format = "0.####";
            dgv_RawExpComp_MI_masses.Columns["intensity_sum"].DefaultCellStyle.Format = "0.####";

            //set column header
            dgv_RawExpComp_MI_masses.Columns["monoisotopic_mass"].HeaderText = "Monoisotopic Mass";
            dgv_RawExpComp_MI_masses.Columns["delta_mass"].HeaderText = "Delta Mass";
            dgv_RawExpComp_MI_masses.Columns["weighted_monoisotopic_mass"].HeaderText = "Weighted Monoisotopic Mass";
            dgv_RawExpComp_MI_masses.Columns["rt_apex"].HeaderText = "Apex RT";
            dgv_RawExpComp_MI_masses.Columns["relative_abundance"].HeaderText = "Relative Abundance";
            dgv_RawExpComp_MI_masses.Columns["fract_abundance"].HeaderText = "Fractional Abundance";
            dgv_RawExpComp_MI_masses.Columns["intensity_sum"].HeaderText = "Intensity Sum";
            dgv_RawExpComp_MI_masses.Columns["file_origin"].HeaderText = "Filename";
            dgv_RawExpComp_MI_masses.Columns["id"].HeaderText = "ID";
            dgv_RawExpComp_MI_masses.Columns["scan_range"].HeaderText = "Scan Range";
            dgv_RawExpComp_MI_masses.Columns["rt_range"].HeaderText = "RT Range";
            dgv_RawExpComp_MI_masses.Columns["num_charge_states"].HeaderText = "No. Charge States";
            dgv_RawExpComp_MI_masses.Columns["accepted"].HeaderText = "Accepted";
            dgv_RawExpComp_MI_masses.Columns["manual_mass_shift"].HeaderText = "Manual Mass Shift";

            dgv_RawExpComp_MI_masses.AllowUserToAddRows = false;
            dgv_RawExpComp_MI_masses.Columns["corrected_mass"].Visible = false;
            dgv_RawExpComp_MI_masses.Columns["intensity_sum_olcs"].Visible = false;
            dgv_RawExpComp_MI_masses.Columns["_manual_mass_shift"].Visible = false;
        }

        private void dgv_RawExpComp_MI_masses_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && !Lollipop.opened_results_originally)
            {
                ProteoformSuiteInternal.Component c = (ProteoformSuiteInternal.Component)this.dgv_RawExpComp_MI_masses.Rows[e.RowIndex].DataBoundItem;
                DisplayUtility.FillDataGridView(dgv_RawExpComp_IndChgSts, c.charge_states);
                Format_IndChgSts();
            }
        }

        private void Format_IndChgSts()
        {
            //round table values
            dgv_RawExpComp_IndChgSts.Columns["intensity"].DefaultCellStyle.Format = "0.####";
            dgv_RawExpComp_IndChgSts.Columns["mz_centroid"].DefaultCellStyle.Format = "0.####";
            dgv_RawExpComp_IndChgSts.Columns["calculated_mass"].DefaultCellStyle.Format = "0.####";

            //set column header
            dgv_RawExpComp_IndChgSts.Columns["intensity"].HeaderText = "Intensity";
            dgv_RawExpComp_IndChgSts.Columns["mz_centroid"].HeaderText = "Centroid m/z";
            dgv_RawExpComp_IndChgSts.Columns["mz_correction"].HeaderText = "Lock-Mass Correction (m/z)";
            dgv_RawExpComp_IndChgSts.Columns["calculated_mass"].HeaderText = "Calculated Mass";
            dgv_RawExpComp_IndChgSts.Columns["charge_count"].HeaderText = "Charge Count";

            if (Lollipop.correctionFactorFilenames.Count == 0) dgv_RawExpComp_IndChgSts.Columns["mz_correction"].Visible = false;
            dgv_RawExpComp_IndChgSts.AllowUserToAddRows = false;
        }
    }
}
