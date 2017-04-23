using ProteoformSuiteInternal;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace ProteoformSuiteGUI
{
    public partial class RawExperimentalComponents : Form
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
                    if (SaveState.lollipop.get_files(SaveState.lollipop.input_files, Purpose.ProteinDatabase).Count() > 0 && SaveState.lollipop.proteoform_community.theoretical_proteoforms.Length <= 0)
                        SaveState.lollipop.get_theoretical_proteoforms(Path.Combine(Path.Combine(Environment.CurrentDirectory)));
                }
            );

            FillComponentsTable();
            if (SaveState.lollipop.neucode_labeled) (MdiParent as ProteoformSweet).neuCodePairs.display_neucode_pairs();
                (MdiParent as ProteoformSweet).theoreticalDatabase.FillDataBaseTable("Target");
        }

        public DataGridView GetDGV()
        {
            return dgv_rawComponents;
        }

        public void FillComponentsTable()
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
            FillComponentsTable();
            dgv_chargeStates.DataSource = null;
        }

        private void bt_recalculate_Click(object sender, EventArgs e)
        {
            load_raw_components();
        }

        #endregion Private Methods
    }
}
