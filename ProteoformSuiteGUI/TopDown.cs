using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ProteoformSuiteInternal;
using System.IO;

namespace ProteoformSuite
{
    public partial class TopDown : Form
    {
        private static Color[] colors = new Color[20];
        private static List<string> mods = new List<string>();

        public TopDown()
        {
            InitializeComponent();
        }

        public void load_dgv()
        {
            DisplayUtility.FillDataGridView(dgv_TD_proteoforms, Lollipop.proteoform_community.topdown_proteoforms);
            load_colors();
            load_ptm_colors();
        }

        public void load_topdown()
        {
            if (Lollipop.top_down_hits.Count == 0 && Lollipop.input_files.Any(f => f.purpose == Purpose.TopDown))
            {
                Lollipop.process_td_results(Lollipop.top_down_hits, false);
                Lollipop.process_td_results(Lollipop.top_down_hits_targeted_results, true);
            }
            cmbx_td_or_e_proteoforms.DataSource = new BindingList<string>() { "TopDown Proteoforms", "Identified Experimental Proteoforms" };
            cmbx_td_or_e_proteoforms.SelectedIndex = 0;
        }

        private void bt_load_td_Click(object sender, EventArgs e)
        {
            Lollipop.proteoform_community.topdown_proteoforms = new TopDownProteoform[0];
            Lollipop.proteoform_community.targeted_topdown_proteoforms.Clear();
            clear_lists();
            Lollipop.aggregate_td_hits(Lollipop.top_down_hits, false);
            Lollipop.aggregate_td_hits(Lollipop.top_down_hits_targeted_results, true);
            if (Lollipop.proteoform_community.targeted_topdown_proteoforms.Count > 0) bt_targeted_td_relations.Enabled = true;
            tb_tdProteoforms.Text = Lollipop.proteoform_community.topdown_proteoforms.Length.ToString();
            load_dgv();
        }

        private void clear_lists()
        {
            Lollipop.td_relations.Clear();
            foreach (Proteoform p in Lollipop.proteoform_community.experimental_proteoforms) p.relationships.RemoveAll(r => r.relation_type == ProteoformComparison.etd);
            foreach (Proteoform p in Lollipop.proteoform_community.theoretical_proteoforms) p.relationships.RemoveAll(r => r.relation_type == ProteoformComparison.ttd);
            foreach (Proteoform p in Lollipop.proteoform_community.experimental_proteoforms) p.relationships.RemoveAll(r => r.relation_type == ProteoformComparison.ettd);
            dgv_TD_proteoforms.DataSource = null;
            dgv_TD_proteoforms.Rows.Clear();
        }

        private void bt_td_relations_Click(object sender, EventArgs e)
        {
            if (Lollipop.proteoform_community.experimental_proteoforms.Length > 0 && Lollipop.proteoform_community.topdown_proteoforms.Length > 0)
            {
                clear_lists();
                Lollipop.make_td_relationships();
                tb_td_relations.Text = Lollipop.td_relations.Where(r => r.relation_type == ProteoformComparison.etd).Count().ToString();
                load_dgv();
            }
            else
            {
                if (Lollipop.proteoform_community.experimental_proteoforms.Length > 0) MessageBox.Show("Go back and load in topdown results.");
                else if (Lollipop.proteoform_community.topdown_proteoforms.Length > 0) MessageBox.Show("Go back and aggregate experimental proteoforms.");
            }
        }

        private void bt_targeted_td_relations_Click(object sender, EventArgs e)
        {
            Lollipop.make_targeted_td_relationships();
        }

        private void dgv_TD_proteoforms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dgv_TD_family.DataSource = null;
                if (this.dgv_TD_proteoforms.Rows[e.RowIndex].DataBoundItem is TopDownProteoform)
                {
                    TopDownProteoform p = (TopDownProteoform)this.dgv_TD_proteoforms.Rows[e.RowIndex].DataBoundItem;
                    if (p.relationships != null)
                    {
                        DisplayUtility.FillDataGridView(dgv_TD_family, p.relationships);  //show T-TD and E-TD relationsj
                    }
                    get_proteoform_sequence(p);
                }

                else
                {
                    ExperimentalProteoform exp = (ExperimentalProteoform)this.dgv_TD_proteoforms.Rows[e.RowIndex].DataBoundItem;
                    if (exp.family != null)
                    {
                        List<Proteoform> proteoforms = new List<Proteoform>();
                        proteoforms.AddRange(exp.family.topdown_proteoforms);
                        proteoforms.AddRange(exp.family.theoretical_proteoforms);
                        proteoforms.AddRange(exp.relationships.Where(r => r.relation_type == ProteoformComparison.ettd).Select(r => r.connected_proteoforms[0]));
                        DisplayUtility.FillDataGridView(dgv_TD_family, proteoforms);  //show E-TD or ET relations (identified)
                    }
                }
            }
        }

        private void get_proteoform_sequence(TopDownProteoform p)
        {
            rtb_sequence.Text = p.sequence + "\n";
            rtb_sequence.SelectionStart = 0;
            rtb_sequence.SelectionLength = p.sequence.Length;
            rtb_sequence.SelectionColor = Color.Black;
            rtb_sequence.ZoomFactor = 3;

            int length = p.sequence.Length + 1;

            foreach (Ptm ptm in p.ptm_list)
            {
                int i;
                try { i = mods.IndexOf(ptm.modification.description); }
                catch { i = 0; } //just make color blue if > 20 unique PTMs
                Color color = colors[i];

                rtb_sequence.SelectionStart = ptm.position;
                rtb_sequence.SelectionLength = 1;
                rtb_sequence.SelectionColor = color;
            }

            foreach (string description in p.ptm_list.Select(ptm => ptm.modification.description).Distinct())
            {
                int i;
                try { i = mods.IndexOf(description); }
                catch { i = 0; }
                Color color = colors[i];

                rtb_sequence.AppendText("\n" + description);
                rtb_sequence.SelectionStart = length;
                rtb_sequence.SelectionLength = description.Length + 1;
                rtb_sequence.SelectionColor = colors[i];
                length += description.Length + 1;
            }

        }

        private static void load_colors()
        {
            colors[0] = Color.Blue;
            colors[1] = Color.Red;
            colors[2] = Color.Orange;
            colors[3] = Color.Green;
            colors[4] = Color.Purple;
            colors[5] = Color.Gold;
            colors[6] = Color.DeepPink;
            colors[7] = Color.ForestGreen;
            colors[8] = Color.DarkBlue;
            colors[9] = Color.DarkOrange;
            colors[10] = Color.OrangeRed;
            colors[11] = Color.Magenta;
            colors[12] = Color.DeepSkyBlue;
            colors[13] = Color.DarkSlateGray;
            colors[14] = Color.DarkSalmon;
            colors[15] = Color.DarkTurquoise;
            colors[16] = Color.Aqua;
            colors[17] = Color.DarkOliveGreen;
            colors[18] = Color.Fuchsia;
            colors[19] = Color.HotPink;
        }

        private static void load_ptm_colors()
        {
            List<Ptm> ptm = new List<Ptm>();
            foreach (TopDownProteoform p in Lollipop.proteoform_community.topdown_proteoforms)
            {
                ptm.AddRange(p.ptm_list);
            }
            IEnumerable<string> unique_ptm = ptm.Select(p => p.modification.description).Distinct();
            mods = unique_ptm.ToList();
        }

        private void TopDown_Load(object sender, EventArgs e)
        {

        }

        private void cmbx_td_or_e_proteoforms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(cmbx_td_or_e_proteoforms.SelectedItem.ToString() == "TopDown Proteoforms")
            {
                DisplayUtility.FillDataGridView(dgv_TD_proteoforms, Lollipop.proteoform_community.topdown_proteoforms);

            }
            else
            {
                if (Lollipop.proteoform_community.families.Count > 0)
                {
                    if (Lollipop.proteoform_community.experimental_proteoforms.Where(exp => exp.family.relations.Where(r => r.relation_type == ProteoformComparison.etd || (r.relation_type == ProteoformComparison.et && r.accepted)).Count() > 0).Count() > 0)
                    {
                        DisplayUtility.FillDataGridView(dgv_TD_proteoforms, Lollipop.proteoform_community.experimental_proteoforms.Where(exp => exp.family.relations.Where(r => (r.relation_type == ProteoformComparison.et && r.accepted) || r.relation_type == ProteoformComparison.etd).ToList().Count > 0).ToList());
                    }
                }
                else MessageBox.Show("Go back and construct proteoform families.");
            }
        }
    }
}
