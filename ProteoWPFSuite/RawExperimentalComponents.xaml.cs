using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProteoformSuiteInternal;

namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for RawExperimentalComponents.xaml
    /// </summary>
    public partial class RawExperimentalComponents : UserControl, ISweetForm, ITabbedMDI, System.ComponentModel.INotifyPropertyChanged
    {
        #region Public Constructor
        public RawExperimentalComponents()
        {
            InitializeComponent();
            InitializeParameterSet();
            this.DataContext = this;
        }
        #endregion Public Constructor

        #region Private fields
        private bool? ck_Identi;
        private bool? ck_Quanti;
        #endregion
        
        #region Public Property
        public bool? CK_rb_displayIdentificationComponents
        {
            get
            {
                return ck_Identi;
            }
            set
            {
                if (ck_Identi == value)
                {
                    return;
                }
                ck_Identi = value;
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs("CK_rb_displayIdentificationComponents"));
                FillTablesAndCharts();
                dgv_chargeStates.DataSource = null;
            }
        }
        public bool? CK_rb_displayQuantificationComponents
        {
            get
            {
                return ck_Quanti;
            }
            set
            {
                if (ck_Quanti == value)
                {
                    return;
                }
                ck_Quanti = value;
                PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs("CK_rb_displayQuantificationComponents"));
                //nothing changes here
            }
        }
        public List<DataTable> DataTables { get; private set; }
        public ProteoformSweet MDIParent { get; set; }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        #endregion Public Property

        #region Public Methods

        public bool ReadyToRunTheGamut()
        {
            return true;
        }

        public void RunTheGamut(bool full_run)
        {
            ClearListsTablesFigures(true);
            Sweet.lollipop.getConditionBiorepFractionLabels(Sweet.lollipop.neucode_labeled, Sweet.lollipop.input_files); //examines the conditions and bioreps to determine the maximum number of observations to require for quantification

            
            this.MDIParent.quantification.InitializeConditionsParameters();
            this.MDIParent.aggregatedProteoforms.InitializeParameterSet();
            Parallel.Invoke
            (
                () => Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files, Sweet.lollipop.raw_experimental_components, Purpose.Identification, true),
                () => Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files, Sweet.lollipop.raw_quantification_components, Purpose.Quantification, true)
            );
            if (ComponentReader.components_with_errors.Count > 0)
            {
                MessageBox.Show("Error in Deconvolution Results File: " + String.Join(", ", ComponentReader.components_with_errors));
            }

            FillTablesAndCharts();
        }

        public void InitializeParameterSet()
        {
            rb_displayQuantificationComponents.IsEnabled = Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Count() > 0;
            nUD_mass_tolerance.Value = (decimal)Sweet.lollipop.raw_component_mass_tolerance;
            nUD_max_fit.Value = (decimal)Sweet.lollipop.max_fit;
            nUD_min_liklihood_ratio.Value = (decimal)Sweet.lollipop.min_likelihood_ratio;
            CK_rb_displayIdentificationComponents = true;
            CK_rb_displayQuantificationComponents = false;
            FillTablesAndCharts();
        }

        public void ClearListsTablesFigures(bool clear_following_forms)
        {
            Sweet.lollipop.raw_experimental_components.Clear();
            Sweet.lollipop.raw_quantification_components.Clear();
            foreach (InputFile f in Sweet.lollipop.input_files)
            {
                f.reader.Clear();
            }

            if (clear_following_forms)
            {
               
                for (int i = this.MDIParent.forms.IndexOf(this) + 1; i < (this.MDIParent).forms.Count; i++)
                {
                    ISweetForm sweet = (this.MDIParent).forms[i];
                    if (sweet as TopDown == null)
                        sweet.ClearListsTablesFigures(false);
                }
            }

            dgv_fileList.Rows.Clear();
            dgv_rawComponents.Rows.Clear();
            dgv_chargeStates.Rows.Clear();

            dgv_fileList.DataSource = null;
            dgv_rawComponents.DataSource = null;
            dgv_chargeStates.DataSource = null;
        }

        public List<DataTable> SetTables()
        {
            DataTables = new List<DataTable>
            {
                DisplayComponent.FormatComponentsTable(Sweet.lollipop.raw_experimental_components.Select(c => new DisplayComponent(c)).ToList(), "RawExperimentalComponents"),
                DisplayComponent.FormatComponentsTable(Sweet.lollipop.raw_quantification_components.Select(c => new DisplayComponent(c)).ToList(), "RawQuantificationComponents"),
            };
            return DataTables;
        }

        public void FillTablesAndCharts()
        {
            DisplayUtility.FillDataGridView(dgv_fileList, Sweet.lollipop.get_files(Sweet.lollipop.input_files, new Purpose[] { Purpose.Identification, Purpose.Quantification }).Select(c => new DisplayInputFile(c)));
            DisplayInputFile.FormatInputFileTable(dgv_fileList, new Purpose[] { Purpose.Identification, Purpose.Quantification });
            dgv_fileList.ReadOnly = true;

            if (rb_displayIdentificationComponents.IsChecked.HasValue && (bool)rb_displayIdentificationComponents.IsChecked.Value && Sweet.lollipop.raw_experimental_components.Count > 0)
                DisplayUtility.FillDataGridView(dgv_rawComponents, Sweet.lollipop.raw_experimental_components.Select(c => new DisplayComponent(c)));

            if (rb_displayQuantificationComponents.IsChecked.HasValue && (bool)rb_displayQuantificationComponents.IsChecked.Value && Sweet.lollipop.raw_quantification_components.Count > 0)
                DisplayUtility.FillDataGridView(dgv_rawComponents, Sweet.lollipop.raw_quantification_components.Select(c => new DisplayComponent(c)));

            DisplayComponent.FormatComponentsTable(dgv_rawComponents);

            rtb_raw_components_counts.Text = ResultsSummaryGenerator.raw_components_report();

        }

        #endregion Public Methods

        #region Private Methods

        private void dgv_RawExpComp_MI_masses_CellContentClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                ProteoformSuiteInternal.Component c = ((Component)((DisplayComponent)this.dgv_rawComponents.Rows[e.RowIndex].DataBoundItem).display_object);
                DisplayUtility.FillDataGridView(dgv_chargeStates, c.charge_states.Select(cs => new DisplayChargeState(cs)));
                DisplayChargeState.FormatChargeStateTable(dgv_chargeStates);
            }
        }


        private void dgv_RawQuantComp_MI_masses_CellContentClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Component c = ((Component)((DisplayComponent)this.dgv_rawComponents.Rows[e.RowIndex].DataBoundItem).display_object);
                DisplayUtility.FillDataGridView(dgv_chargeStates, c.charge_states.Select(cs => new DisplayChargeState(cs)));
                DisplayChargeState.FormatChargeStateTable(dgv_chargeStates);
            }
        }

        private void rb_displayIdentificationComponents_CheckedChanged(object sender, EventArgs e)
        {
            FillTablesAndCharts();
            dgv_chargeStates.DataSource = null; 
        }

        private void bt_recalculate_Click(object sender, RoutedEventArgs e)
        {
            //Cursor = Cursors.Wait; only works for this particular page, so suggested to be replaced by override
            Mouse.OverrideCursor = Cursors.Wait;
            RunTheGamut(false);
            Mouse.OverrideCursor = null;
        }

        private void nUD_mass_tolerance_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.raw_component_mass_tolerance = Convert.ToDouble(nUD_mass_tolerance.Value);
        }

        private void nUD_min_liklihood_ratio_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_likelihood_ratio = Convert.ToDouble(nUD_min_liklihood_ratio.Value);
        }

        private void nUD_max_fit_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.max_fit = Convert.ToDouble(nUD_max_fit.Value);
        }

        #endregion Private Methods
        
    }
}
