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
            if (GlobalData.aggregatedProteoforms.Count == 0) AggregateNeuCodeLightProteoforms();
        }

        private void FillAggregatesTable()
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = GlobalData.aggregatedProteoforms;
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
            nUP_mass_tolerance.Value = 3;

            nUD_RetTimeToleranace.Minimum = 0;
            nUD_RetTimeToleranace.Maximum = 10;
            nUD_RetTimeToleranace.Value = 3;

            nUD_Missed_Monos.Minimum = 0;
            nUD_Missed_Monos.Maximum = 5;
            nUD_Missed_Monos.Value = 3;

            nUD_Missed_Ks.Minimum = 0;
            nUD_Missed_Ks.Maximum = 3;
            nUD_Missed_Ks.Value = 1;

        }

        public void AggregateNeuCodeLightProteoforms()
        {
            if (GlobalData.aggregatedProteoforms.Count > 0) GlobalData.aggregatedProteoforms.Clear();

            List<Proteoform> remaining_acceptableProteoforms = GlobalData.rawNeuCodePairs.Where(p => p.accepted).ToList().
                OrderByDescending(p => p.light_intensity).ToList(); 
            //ordered list, so that the proteoform with max intensity is always chosen first

            while (remaining_acceptableProteoforms.Count > 0)
            {
                Proteoform root = remaining_acceptableProteoforms[0];
                remaining_acceptableProteoforms.Remove(root);
                List<Proteoform> pf_to_aggregate = new List<Proteoform>() { root };

                Parallel.ForEach<Proteoform>(remaining_acceptableProteoforms, p =>
                {
                    if (tolerable_rt(root, p) && tolerable_lysCt(root, p) && tolerable_mass(root, p))
                        pf_to_aggregate.Add(p);
                });
                foreach(Proteoform p in pf_to_aggregate)
                {
                    remaining_acceptableProteoforms.Remove(p);
                }

                if (pf_to_aggregate.Count > 0)
                    GlobalData.aggregatedProteoforms.Add(new AggregatedProteoform(pf_to_aggregate));
            }
            FillAggregatesTable();
        }

        private bool tolerable_rt(Proteoform root, Proteoform candidate)
        {
            return candidate.light_apexRt >= root.light_apexRt - Convert.ToDouble(nUD_RetTimeToleranace.Value) &&
                candidate.light_apexRt <= root.light_apexRt + Convert.ToDouble(nUD_RetTimeToleranace.Value);
        }

        private bool tolerable_lysCt(Proteoform root, Proteoform candidate)
        {
            int max_missed_lysines = Convert.ToInt32(nUD_Missed_Ks.Value);
            List<int> acceptable_lysineCts = Enumerable.Range(root.lysine_count - max_missed_lysines, root.lysine_count + max_missed_lysines).ToList();
            return acceptable_lysineCts.Contains(candidate.lysine_count);
        }

        private bool tolerable_mass(Proteoform root, Proteoform candidate)
        {
            int max_missed_monoisotopics = Convert.ToInt32(nUD_Missed_Monos.Value);
            List<int> missed_monoisotopics = Enumerable.Range(-max_missed_monoisotopics, max_missed_monoisotopics).ToList();
            foreach(int m in missed_monoisotopics)
            {
                double shift = m * 1.0015;
                double mass_tolerance = (root.light_corrected_mass + shift) / 1000000 * Convert.ToInt32(nUP_mass_tolerance.Value);
                double low = root.light_corrected_mass + shift - mass_tolerance;
                double high = root.light_corrected_mass + shift + mass_tolerance;
                bool tolerable_mass = candidate.light_corrected_mass >= low && candidate.light_corrected_mass <= high;
                if (tolerable_mass) return true; //Return a true result immediately; acts as an OR between these conditions
            }
            return false;
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
            AggregateNeuCodeLightProteoforms();
        }

        private void nUD_RetTimeToleranace_ValueChanged(object sender, EventArgs e)
        {
            AggregateNeuCodeLightProteoforms();
        }

        private void nUD_Missed_Monos_ValueChanged(object sender, EventArgs e)
        {
            AggregateNeuCodeLightProteoforms();
        }

        private void nUD_Missed_Ks_ValueChanged(object sender, EventArgs e)
        {
            AggregateNeuCodeLightProteoforms();
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
