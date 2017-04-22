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
        #region Public Fields

        public bool preloaded = false;

        #endregion Public Fields

        #region Public Constructor

        public RawExperimentalComponents()
        {
            InitializeComponent();
        }

        #endregion Public Constructor
        
        #region Public Methods

        public void load_raw_components()
        {
            if (!preloaded)
            {
                SaveState.lollipop.getBiorepsFractionsList(SaveState.lollipop.input_files); // list of bioreps with a list of fractions for each biorep
                SaveState.lollipop.getObservationParameters(SaveState.lollipop.neucode_labeled, SaveState.lollipop.input_files); //examines the conditions and bioreps to determine the maximum number of observations to require for quantification

                Parallel.Invoke
                (
                () => 
                    {
                        if (SaveState.lollipop.raw_experimental_components.Count <= 0)
                            SaveState.lollipop.process_raw_components(SaveState.lollipop.input_files, SaveState.lollipop.raw_experimental_components, Purpose.Identification);
                    }, //Includes reading correction factors if present,
                () => 
                    {
                        if (SaveState.lollipop.raw_quantification_components.Count <= 0)
                            SaveState.lollipop.process_raw_components(SaveState.lollipop.input_files, SaveState.lollipop.raw_quantification_components, Purpose.Quantification);
                    },
                () => 
                    {
                        if (SaveState.lollipop.get_files(SaveState.lollipop.input_files, Purpose.ProteinDatabase).Count() > 0 && SaveState.lollipop.proteoform_community.theoretical_proteoforms.Length <= 0)
                            SaveState.lollipop.get_theoretical_proteoforms(Path.Combine(Path.Combine(Environment.CurrentDirectory)));
                    }
                );

                FillRawExpComponentsTable();
                FillRawQuantificationComponentsTable();
                if (SaveState.lollipop.neucode_labeled) (MdiParent as ProteoformSweet).neuCodePairs.display_neucode_pairs();
                    (MdiParent as ProteoformSweet).theoreticalDatabase.FillDataBaseTable("Target");
            }
            preloaded = false;
        }

        public DataGridView GetDGV()
        {
            return dgv_RawExpComp_MI_masses;
        }

        public void FillRawExpComponentsTable()
        {
            if (SaveState.lollipop.raw_experimental_components.Count > 0)
            {
                DisplayUtility.FillDataGridView(dgv_RawExpComp_MI_masses, SaveState.lollipop.raw_experimental_components.Select(c => new DisplayComponent(c)));
                DisplayComponent.FormatComponentsTable(dgv_RawExpComp_MI_masses, false);
            }
        }

        public void FillRawQuantificationComponentsTable()
        {
            if (SaveState.lollipop.raw_quantification_components.Count > 0)
            {
                DisplayUtility.FillDataGridView(dgv_RawQuantComp_MI_masses, SaveState.lollipop.raw_quantification_components.Select(c => new DisplayComponent(c)));
                DisplayComponent.FormatComponentsTable(dgv_RawQuantComp_MI_masses, true);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private void dgv_RawExpComp_MI_masses_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Component c = ((Component)((DisplayComponent)this.dgv_RawExpComp_MI_masses.Rows[e.RowIndex].DataBoundItem).display_object);
                DisplayUtility.FillDataGridView(dgv_RawExpComp_IndChgSts, c.charge_states.Select(cs => new DisplayChargeState(cs)));
                DisplayChargeState.FormatChargeStateTable(dgv_RawExpComp_IndChgSts, false);
            }
        }


        private void dgv_RawQuantComp_MI_masses_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Component c = ((Component)((DisplayComponent)this.dgv_RawQuantComp_MI_masses.Rows[e.RowIndex].DataBoundItem).display_object);
                DisplayUtility.FillDataGridView(dgv_RawQuantComp_IndChgSts, c.charge_states.Select(cs => new DisplayChargeState(cs)));
                DisplayChargeState.FormatChargeStateTable(dgv_RawQuantComp_IndChgSts, true);
            }
        }
        #endregion Private Methods
    }
}
