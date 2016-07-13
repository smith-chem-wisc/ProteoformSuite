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
        }

        public void FillRawExpComponentsTable()
        {
            DataGridViewDisplayUtility.FillDataGridView(dgv_RawExpComp_MI_masses, Lollipop.raw_experimental_components);
        }

        private void dgv_RawExpComp_MI_masses_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //DataGridViewDisplayUtility.sortDataGridViewColumn(dgv_RawExpComp_MI_masses, e.ColumnIndex);           
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
