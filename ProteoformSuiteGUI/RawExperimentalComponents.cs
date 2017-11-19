using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public partial class RawExperimentalComponents : Form, ISweetForm
    {

        #region Public Constructor

        public RawExperimentalComponents()
        {
            InitializeComponent();
            this.AutoScroll = true;
            this.AutoScrollMinSize = this.ClientSize;
        }

        #endregion Public Constructor

        #region Public Property

        public List<DataTable> DataTables { get; private set; }

        #endregion Public Property

        #region Public Methods

        public bool ReadyToRunTheGamut()
        {
            return true;
        }

        public void RunTheGamut(bool full_run)
        {
            ClearListsTablesFigures(true);

            //if unlabeled, copy over identification files to quant files...
            if(!Sweet.lollipop.neucode_labeled)
            {
                List<InputFile> new_files = new List<InputFile>();
                foreach(InputFile f in Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Identification))
                {
                    InputFile new_f = new InputFile(f);
                    new_f.purpose = Purpose.Quantification;
                    new_files.Add(new_f);
                }
                Sweet.lollipop.input_files.AddRange(new_files);
            }

            Sweet.lollipop.getConditionBiorepFractionLabels(Sweet.lollipop.neucode_labeled, Sweet.lollipop.input_files); //examines the conditions and bioreps to determine the maximum number of observations to require for quantification
            (MdiParent as ProteoformSweet).quantification.InitializeConditionsParameters();

            if (cb_deconvolute.Checked)
            {
                Parallel.Invoke
                (
                    () => Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files.Where(f => !f.quantitative).ToList(), Sweet.lollipop.raw_experimental_components, Purpose.RawFile, true),
                    () => Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files.Where(f => f.quantitative).ToList(), Sweet.lollipop.raw_quantification_components, Purpose.RawFile, true)
                );
            }
            else
            {
                Parallel.Invoke
                (
                    () => Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files, Sweet.lollipop.raw_experimental_components, Purpose.Identification, true),
                    () => Sweet.lollipop.process_raw_components(Sweet.lollipop.input_files, Sweet.lollipop.raw_quantification_components, Purpose.Quantification, true)
                );
            }

            FillTablesAndCharts();
        }

        public void InitializeParameterSet()
        {
            rb_displayQuantificationComponents.Enabled = Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.Quantification).Count() > 0 
                || Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.RawFile).Where(f => f.quantitative).Count() > 0;
            nUD_mass_tolerance.Value = (decimal)Sweet.lollipop.raw_component_mass_tolerance;
            nUD_min_RT.Value = (decimal)Sweet.lollipop.min_RT;
            nUD_max_RT.Value = (decimal)Sweet.lollipop.max_RT;
            nUD_agg_tolerance.Value = (decimal)Sweet.lollipop.aggregation_tolerance_ppm;
            nUD_decon_tolerance.Value = (decimal)Sweet.lollipop.deconvolution_tolerance_ppm;
            nUD_max_cs.Value = Sweet.lollipop.max_assumed_cs;
            nUD_intensity_ratio_limit.Value = (decimal)Sweet.lollipop.intensity_ratio_limit;
            nUD_min_num_CS.Value = Sweet.lollipop.min_num_cs_deconvolution_component;
            nUD_min_num_scans.Value = Sweet.lollipop.min_num_scans_deconvolution_component;
            nUD_min_cs.Value = Sweet.lollipop.min_assumed_cs;
            nUD_min_rel_ab.Value = (decimal)Sweet.lollipop.min_relative_abundance;
            nUD_scans_to_average.Value = Sweet.lollipop.num_scans_average;
            nUD_min_sig_noise.Value = (decimal)Sweet.lollipop.min_S_N;
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
                for (int i = ((ProteoformSweet)MdiParent).forms.IndexOf(this) + 1; i < ((ProteoformSweet)MdiParent).forms.Count; i++)
                {
                    ISweetForm sweet = ((ProteoformSweet)MdiParent).forms[i];
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

            if (rb_displayIdentificationComponents.Checked && Sweet.lollipop.raw_experimental_components.Count > 0)
                DisplayUtility.FillDataGridView(dgv_rawComponents, Sweet.lollipop.raw_experimental_components.Select(c => new DisplayComponent(c)));

            if (rb_displayQuantificationComponents.Checked && Sweet.lollipop.raw_quantification_components.Count > 0)
                DisplayUtility.FillDataGridView(dgv_rawComponents, Sweet.lollipop.raw_quantification_components.Select(c => new DisplayComponent(c)));

            DisplayComponent.FormatComponentsTable(dgv_rawComponents);

            rtb_raw_components_counts.Text = ResultsSummaryGenerator.raw_components_report();

            NeuCodePairs pairs_form = (MdiParent as ProteoformSweet).neuCodePairs;
            if (Sweet.lollipop.neucode_labeled && pairs_form.ReadyToRunTheGamut())
                pairs_form.RunTheGamut(false);
        }

        #endregion Public Methods

        #region Private Methods

        private void dgv_RawExpComp_MI_masses_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                Component c = ((Component)((DisplayComponent)this.dgv_rawComponents.Rows[e.RowIndex].DataBoundItem).display_object);
                DisplayUtility.FillDataGridView(dgv_chargeStates, c.charge_states.Select(cs => new DisplayChargeState(cs)));
                DisplayChargeState.FormatChargeStateTable(dgv_chargeStates);
            }
        }


        private void dgv_RawQuantComp_MI_masses_CellContentClick(object sender, DataGridViewCellEventArgs e)
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

        private void bt_recalculate_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            RunTheGamut(false);
            Cursor = Cursors.Default;
        }

        #endregion Private Methods

        private void nUD_mass_tolerance_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.raw_component_mass_tolerance = Convert.ToDouble(nUD_mass_tolerance.Value);
        }

        private void nUD_min_RT_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_RT = Convert.ToDouble(nUD_min_RT.Value);
        }

        private void nUD_max_RT_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.max_RT = Convert.ToDouble(nUD_max_RT.Value);
        }

        private void nUD_agg_tolerance_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.aggregation_tolerance_ppm = Convert.ToDouble(nUD_agg_tolerance.Value);
        }

        private void nUD_decon_tolerance_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.deconvolution_tolerance_ppm = Convert.ToDouble(nUD_decon_tolerance.Value);
        }

        private void nUD_max_cs_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.max_assumed_cs = Convert.ToInt32(nUD_max_cs.Value);
        }

        private void nUD_intensity_ratio_limit_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.intensity_ratio_limit = Convert.ToDouble(nUD_intensity_ratio_limit.Value);
        }

        private void nUD_min_num_CS_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_num_cs_deconvolution_component = Convert.ToInt32(nUD_min_num_CS.Value);
        }

        private void nUD_min_num_scans_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_num_scans_deconvolution_component = Convert.ToInt32(nUD_min_num_scans.Value);
        }

        private void cb_deconvolute_CheckedChanged(object sender, EventArgs e)
        {
            nUD_min_RT.Visible = cb_deconvolute.Checked;
            nUD_max_RT.Visible = cb_deconvolute.Checked;
            nUD_agg_tolerance.Visible = cb_deconvolute.Checked;
            nUD_decon_tolerance.Visible = cb_deconvolute.Checked;
            nUD_max_cs.Visible = cb_deconvolute.Checked;
            nUD_intensity_ratio_limit.Visible = cb_deconvolute.Checked;
            nUD_min_num_CS.Visible = cb_deconvolute.Checked;
            nUD_min_num_scans.Visible = cb_deconvolute.Checked;
            nUD_min_cs.Visible = cb_deconvolute.Checked;
            nUD_min_rel_ab.Visible = cb_deconvolute.Checked;
            nUD_scans_to_average.Visible = cb_deconvolute.Checked;
            nUD_min_sig_noise.Visible = cb_deconvolute.Checked;
            label2.Visible = cb_deconvolute.Checked;
            label3.Visible = cb_deconvolute.Checked;
            label4.Visible = cb_deconvolute.Checked;
            label5.Visible = cb_deconvolute.Checked;
            label6.Visible = cb_deconvolute.Checked;
            label7.Visible = cb_deconvolute.Checked;
            label8.Visible = cb_deconvolute.Checked;
            label9.Visible = cb_deconvolute.Checked;
            label10.Visible = cb_deconvolute.Checked;
            label11.Visible = cb_deconvolute.Checked;
            label12.Visible = cb_deconvolute.Checked;
            label13.Visible = cb_deconvolute.Checked;
        }

        private void nUD_min_cs_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_assumed_cs = Convert.ToInt32(nUD_min_cs.Value);
        }

        private void nUD_min_rel_ab_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_relative_abundance = Convert.ToDouble(nUD_min_rel_ab.Value);
        }

        private void nUD_scans_to_average_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.num_scans_average = Convert.ToInt32(nUD_scans_to_average.Value);
        }

        private void nUD_min_sig_noise_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_S_N = Convert.ToDouble(nUD_min_sig_noise.Value);
        }
    }
}
