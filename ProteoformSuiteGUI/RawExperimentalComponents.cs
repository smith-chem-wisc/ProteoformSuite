using ProteoformSuiteInternal;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;

namespace ProteoformSuiteGUI
{
    public partial class RawExperimentalComponents : Form, ISweetForm
    {

        #region Public Constructor

        public RawExperimentalComponents()
        {
            InitializeComponent();
        }

        #endregion Public Constructor
        
        #region Public Methods

        public void load_raw_components()
        {
            SaveState.lollipop.getBiorepsFractionsList(SaveState.lollipop.input_files); // list of bioreps with a list of fractions for each biorep
            SaveState.lollipop.getObservationParameters(SaveState.lollipop.neucode_labeled, SaveState.lollipop.input_files); //examines the conditions and bioreps to determine the maximum number of observations to require for quantification

            Parallel.Invoke
            (
                () => SaveState.lollipop.process_raw_components(SaveState.lollipop.input_files, SaveState.lollipop.raw_experimental_components, Purpose.Identification),
                () => SaveState.lollipop.process_raw_components(SaveState.lollipop.input_files, SaveState.lollipop.raw_quantification_components, Purpose.Quantification),
                () => 
                {
                    if ((MdiParent as ProteoformSweet).theoreticalDatabase.ReadyToRunTheGamut())
                        (MdiParent as ProteoformSweet).theoreticalDatabase.RunTheGamut();
                }
            );

            FillTablesAndCharts();
            NeuCodePairs pairs_form = (MdiParent as ProteoformSweet).neuCodePairs;
            if (SaveState.lollipop.neucode_labeled && pairs_form.ReadyToRunTheGamut())
                pairs_form.RunTheGamut();
        }

        public List<DataGridView> GetDGVs()
        {
            return new List<DataGridView>() { dgv_rawComponents };
        }

        public void FillTablesAndCharts()
        {
            if (rb_displayIdentificationComponents.Checked && SaveState.lollipop.raw_experimental_components.Count > 0)
                DisplayUtility.FillDataGridView(dgv_rawComponents, SaveState.lollipop.raw_experimental_components.Select(c => new DisplayComponent(c)));

            if (rb_displayQuantificationComponents.Checked && SaveState.lollipop.raw_quantification_components.Count > 0)
                DisplayUtility.FillDataGridView(dgv_rawComponents, SaveState.lollipop.raw_quantification_components.Select(c => new DisplayComponent(c)));

            DisplayComponent.FormatComponentsTable(dgv_rawComponents, true);
        }

        public void initialize_every_time()
        {
            rb_displayQuantificationComponents.Enabled = SaveState.lollipop.get_files(SaveState.lollipop.input_files, Purpose.Quantification).Count() > 0;
            DisplayUtility.FillDataGridView(dgv_fileList, SaveState.lollipop.get_files(SaveState.lollipop.input_files, new Purpose[] { Purpose.Identification, Purpose.Quantification }).Select(c => new DisplayInputFile(c)));
            DisplayInputFile.FormatInputFileTable(dgv_fileList, new Purpose[] { Purpose.Identification, Purpose.Quantification });
            dgv_fileList.ReadOnly = true;
        }

        public bool ReadyToRunTheGamut()
        {
            return true;
        }

        public void RunTheGamut()
        {
            load_raw_components();
        }

        public void InitializeParameterSet()
        {
            initialize_every_time();
        }

        public void ClearListsTablesFigures()
        {
            dgv_fileList.DataSource = null;
            dgv_fileList.Rows.Clear();
            dgv_rawComponents.DataSource = null;
            dgv_rawComponents.Rows.Clear();
            dgv_chargeStates.DataSource = null;
            dgv_chargeStates.Rows.Clear();

            ((ProteoformSweet)MdiParent).neuCodePairs.ClearListsTablesFigures();
            ((ProteoformSweet)MdiParent).aggregatedProteoforms.ClearListsTablesFigures();

        }

        #endregion Public Methods

        #region Private Methods

        private void dgv_RawExpComp_MI_masses_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Component c = ((Component)((DisplayComponent)this.dgv_rawComponents.Rows[e.RowIndex].DataBoundItem).display_object);
                DisplayUtility.FillDataGridView(dgv_chargeStates, c.charge_states.Select(cs => new DisplayChargeState(cs)));
                DisplayChargeState.FormatChargeStateTable(dgv_chargeStates, false);
            }
        }


        private void dgv_RawQuantComp_MI_masses_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Component c = ((Component)((DisplayComponent)this.dgv_rawComponents.Rows[e.RowIndex].DataBoundItem).display_object);
                DisplayUtility.FillDataGridView(dgv_chargeStates, c.charge_states.Select(cs => new DisplayChargeState(cs)));
                DisplayChargeState.FormatChargeStateTable(dgv_chargeStates, true);
            }
        }

        private void rb_displayIdentificationComponents_CheckedChanged(object sender, EventArgs e)
        {
            FillTablesAndCharts();
            dgv_chargeStates.DataSource = null;
        }

        private void bt_recalculate_Click(object sender, EventArgs e)
        {
            RunTheGamut();
        }

        #endregion Private Methods
    }
}
