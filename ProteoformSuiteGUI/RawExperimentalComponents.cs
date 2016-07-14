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
            if (Lollipop.raw_experimental_components.Count == 0)
                Lollipop.process_raw_components((b) => new ExcelReader().read_components_from_xlsx(b));
            this.FillRawExpComponentsTable();
            this.FormatRawExpComponentsTable();
        }

        public void FillRawExpComponentsTable()
        {
            DataGridViewDisplayUtility.FillDataGridView(dgv_RawExpComp_MI_masses, Lollipop.raw_experimental_components);
        }

        public void FormatRawExpComponentsTable()
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
            dgv_RawExpComp_MI_masses.Columns["corrected_mass"].HeaderText = "Corrected Mass";
            dgv_RawExpComp_MI_masses.Columns["rt_apex"].HeaderText = "Apex RT";
            dgv_RawExpComp_MI_masses.Columns["relative_abundance"].HeaderText = "Relative Abundance";
            dgv_RawExpComp_MI_masses.Columns["fract_abundance"].HeaderText = "Fractional Abundance";
            dgv_RawExpComp_MI_masses.Columns["intensity_sum"].HeaderText = "Intensity Sum";
            dgv_RawExpComp_MI_masses.Columns["file_origin"].HeaderText = "Filename";
            dgv_RawExpComp_MI_masses.Columns["id"].HeaderText = "ID";
            dgv_RawExpComp_MI_masses.Columns["scan_range"].HeaderText = "Scan Range";
            dgv_RawExpComp_MI_masses.Columns["rt_range"].HeaderText = "RT Range";
            dgv_RawExpComp_MI_masses.Columns["num_charge_states"].HeaderText = "No. Charge States";
        }

        private void dgv_RawExpComp_MI_masses_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                ProteoformSuiteInternal.Component c = (ProteoformSuiteInternal.Component)this.dgv_RawExpComp_MI_masses.Rows[e.RowIndex].DataBoundItem;
                DataGridViewDisplayUtility.FillDataGridView(dgv_RawExpComp_IndChgSts, c.charge_states);
            }
        }
    }
}
