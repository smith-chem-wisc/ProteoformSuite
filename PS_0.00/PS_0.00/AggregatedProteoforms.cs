using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PS_0._00
{
    public partial class AggregatedProteoforms : Form
    {
        public AggregatedProteoforms()
        {
            InitializeComponent();
        }

        public void AggregatedProteoforms_Load(object sender, EventArgs e)
        {
            InitializeSettings();
            if (Lollipop.aggregatedProteoforms.Count == 0)
            {
                Lollipop.AggregateNeuCodeLightProteoforms();
                FillAggregatesTable();
            }
        }

        private void FillAggregatesTable()
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = Lollipop.aggregatedProteoforms;
            dgv_AggregatedProteoforms.DataSource = bs;
            dgv_AggregatedProteoforms.ReadOnly = true;
            //dgv_AggregatedProteoforms.Columns["Aggregated Mass"].DefaultCellStyle.Format = "0.####";
            //dgv_AggregatedProteoforms.Columns["Aggregated Retention Time"].DefaultCellStyle.Format = "0.##";
            //dgv_AggregatedProteoforms.Columns["Aggregated Intensity"].DefaultCellStyle.Format = "0";
            dgv_AggregatedProteoforms.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
            dgv_AggregatedProteoforms.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
        }

        private void RoundDoubleColumn(DataTable table, string column_name, int num_decimal_places)
        {
            table.AsEnumerable().ToList().ForEach(p => p.SetField<Double>(column_name, Math.Round(p.Field<Double>(column_name), num_decimal_places)));
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

        private void dgv_AggregatedProteoforms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex >= 0)
            {
                AggregatedProteoform selected_pf = (AggregatedProteoform)this.dgv_AggregatedProteoforms.Rows[e.RowIndex].DataBoundItem;

                BindingSource bs = new BindingSource();
                bs.DataSource = selected_pf.proteoforms;
                dgv_AcceptNeuCdLtProteoforms.DataSource = bs;
                dgv_AcceptNeuCdLtProteoforms.ReadOnly = true;
                //dgv_AcceptNeuCdLtProteoforms.Columns["Aggregated Mass"].DefaultCellStyle.Format = "0.####";
                //dgv_AcceptNeuCdLtProteoforms.Columns["Light Mass"].DefaultCellStyle.Format = "0.####";
                //dgv_AcceptNeuCdLtProteoforms.Columns["Light Mass Corrected"].DefaultCellStyle.Format = "0.####";
                //dgv_AcceptNeuCdLtProteoforms.Columns["Aggregated Retention Time"].DefaultCellStyle.Format = "0.##";
                //dgv_AcceptNeuCdLtProteoforms.Columns["Light Retention Time"].DefaultCellStyle.Format = "0.##";
                //dgv_AcceptNeuCdLtProteoforms.Columns["Aggregated Intensity"].DefaultCellStyle.Format = "0";
                //dgv_AcceptNeuCdLtProteoforms.Columns["Light Retention Time"].DefaultCellStyle.Format = "0";
                dgv_AcceptNeuCdLtProteoforms.DefaultCellStyle.BackColor = System.Drawing.Color.LightGray;
                dgv_AcceptNeuCdLtProteoforms.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.Color.DarkGray;
            }
        }

        private void nUP_mass_tolerance_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.AggregateNeuCodeLightProteoforms();
        }

        private void nUD_RetTimeToleranace_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.AggregateNeuCodeLightProteoforms();
        }

        private void nUD_Missed_Monos_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.AggregateNeuCodeLightProteoforms();
        }

        private void nUD_Missed_Ks_ValueChanged(object sender, EventArgs e)
        {
            Lollipop.AggregateNeuCodeLightProteoforms();
        }

        public override string ToString()
        {
            return String.Join(System.Environment.NewLine, new string[] {
                "AggregatedProteoforms|nUP_mass_tolerance.Value\t" + nUP_mass_tolerance.Value.ToString(),
                "AggregatedProteoforms|nUD_RetTimeToleranace.Value\t" + nUD_RetTimeToleranace.Value.ToString(),
                "AggregatedProteoforms|nUD_Missed_Monos.Value\t" + nUD_Missed_Monos.Value.ToString(),
                "AggregatedProteoforms|nUD_Missed_Ks.Value\t" + nUD_Missed_Ks.Value.ToString(),
            });
        }

        public void loadSetting(string setting_specs)
        {
            string[] fields = setting_specs.Split('\t');
            switch (fields[0].Split('|')[1])
            {
                case "nUP_mass_tolerance.Value":
                    nUP_mass_tolerance.Value = Convert.ToDecimal(fields[1]);
                    break;
                case "nUD_RetTimeToleranace.Value":
                    nUD_RetTimeToleranace.Value = Convert.ToDecimal(fields[1]);
                    break;
                case "nUD_Missed_Monos.Value":
                    nUD_Missed_Monos.Value = Convert.ToDecimal(fields[1]);
                    break;
                case "nUD_Missed_Ks.Value":
                    nUD_Missed_Ks.Value = Convert.ToDecimal(fields[1]);
                    break;
            }
        }

        private void dgv_AcceptNeuCdLtProteoforms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
