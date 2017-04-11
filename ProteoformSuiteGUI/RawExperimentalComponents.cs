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
                Lollipop.getBiorepsFractionsList(Lollipop.input_files); // list of bioreps with a list of fractions for each biorep
                Lollipop.getObservationParameters(Lollipop.neucode_labeled, Lollipop.input_files); //examines the conditions and bioreps to determine the maximum number of observations to require for quantification

                Parallel.Invoke
                (
                    () => {
                        if (Lollipop.raw_experimental_components.Count <= 0)
                            Lollipop.process_raw_components(Lollipop.input_files, Lollipop.raw_experimental_components, Purpose.Identification);
                    }, //Includes reading correction factors if present,
                    () => {
                        if (Lollipop.raw_quantification_components.Count <= 0)
                            Lollipop.process_raw_components(Lollipop.input_files, Lollipop.raw_quantification_components, Purpose.Quantification);
                    },
                    () => { if (Lollipop.get_files(Lollipop.input_files, Purpose.ProteinDatabase).Count() > 0 && Lollipop.proteoform_community.theoretical_proteoforms.Length <= 0) Lollipop.get_theoretical_proteoforms(Path.Combine(Path.Combine(Environment.CurrentDirectory, "Mods"))); }
                );

                this.FillRawExpComponentsTable();
                this.FillRawQuantificationComponentsTable();
                if (Lollipop.neucode_labeled) ((ProteoformSweet)this.MdiParent).neuCodePairs.display_neucode_pairs();
                ((ProteoformSweet)this.MdiParent).theoreticalDatabase.FillDataBaseTable("Target");
            }
            preloaded = false;
        }

        public DataGridView GetDGV()
        {
            return dgv_RawExpComp_MI_masses;
        }

        public void FillRawExpComponentsTable()
        {
            if (Lollipop.raw_experimental_components.Count > 0)
            {
                DisplayUtility.FillDataGridView(dgv_RawExpComp_MI_masses, Lollipop.raw_experimental_components.Select(c => new DisplayComponent(c)));
                DisplayComponent.FormatComponentsTable(dgv_RawExpComp_MI_masses, false);
            }
        }

        public void FillRawQuantificationComponentsTable()
        {
            if (Lollipop.raw_quantification_components.Count > 0)
            {
                DisplayUtility.FillDataGridView(dgv_RawQuantComp_MI_masses, Lollipop.raw_quantification_components.Select(c => new DisplayComponent(c)));
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
