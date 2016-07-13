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
            this.FillAggregatesTable();
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
            DataGridViewDisplayUtility.FillDataGridView(dgv_AggregatedProteoforms, Lollipop.proteoform_community.experimental_proteoforms);
        }

        private void dgv_AggregatedProteoforms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex >= 0)
            {
                ExperimentalProteoform selected_pf = (ExperimentalProteoform)this.dgv_AggregatedProteoforms.Rows[e.RowIndex].DataBoundItem;
                DataGridViewDisplayUtility.FillDataGridView(dgv_AcceptNeuCdLtProteoforms, selected_pf.aggregated_components);
            }
        }

        private void nUP_mass_tolerance_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.mass_tolerance = nUP_mass_tolerance.Value;
            Lollipop.aggregate_proteoforms();
        }

        private void nUD_RetTimeToleranace_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.retention_time_tolerance = nUD_RetTimeToleranace.Value;
            Lollipop.aggregate_proteoforms();
        }

        private void nUD_Missed_Monos_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.missed_monos = nUD_Missed_Monos.Value;
            Lollipop.aggregate_proteoforms();
        }

        private void nUD_Missed_Ks_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.missed_lysines = nUD_Missed_Ks.Value;
            Lollipop.aggregate_proteoforms();
        }

        private void dgv_AcceptNeuCdLtProteoforms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
