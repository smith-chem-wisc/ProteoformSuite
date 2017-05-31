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

namespace ProteoformSuiteGUI
{
    public partial class TopDown : Form, ISweetForm
    {
        private static Color[] colors = new Color[20];
        private static List<string> mods = new List<string>();

        public TopDown()
        {
            InitializeComponent();
        }

        public void InitializeParameterSet()
        {

        }

        public List<DataGridView> GetDGVs()
        {
            return new List<DataGridView> { dgv_TD_proteoforms };
        }


        public void FillTablesAndCharts()
        {
            DisplayUtility.FillDataGridView(dgv_TD_proteoforms, SaveState.lollipop.target_proteoform_community.topdown_proteoforms.Where(p => !p.targeted).ToList());
            load_colors();
            load_ptm_colors();
        }

        public void load_topdown()
        {
            if (ReadyToRunTheGamut())
            {
                RunTheGamut();
                cmbx_td_or_e_proteoforms.DataSource = new BindingList<string>() { "TopDown Proteoforms", "Experimental Proteoforms" };
                cmbx_td_or_e_proteoforms.SelectedIndex = 0;
            }
        }

        public void RunTheGamut()
        {
            ClearListsTablesFigures(true);
            AggregateTdHits();
            if (SaveState.lollipop.topdownReader.topdown_ptms.Count > 0)
            {
                MessageBox.Show("Warning: Top-down proteoforms with the following modifications were not matched to a modification in the theoretical PTM list: "
                  + String.Join(", ", SaveState.lollipop.topdownReader.topdown_ptms.Select(m => m.id + " at " + m.motif.Motif)));
            }
            SaveState.lollipop.td_relations = SaveState.lollipop.target_proteoform_community.relate_td(SaveState.lollipop.target_proteoform_community.experimental_proteoforms.ToList(), SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.ToList(), SaveState.lollipop.target_proteoform_community.topdown_proteoforms.Where(p => !p.targeted).ToList());
            foreach (ProteoformCommunity c in SaveState.lollipop.decoy_proteoform_communities.Values)
            { 
                c.relate_td(c.experimental_proteoforms.ToList(), c.theoretical_proteoforms.ToList(), c.topdown_proteoforms.Where(p => !p.targeted).ToList());

            }
            if (SaveState.lollipop.target_proteoform_community.topdown_proteoforms.Count(t => t.ttd_match_count == 0) > 0)
            {
                MessageBox.Show("Warning: Top-down proteoforms with the following accessions were not matched to a theoretical proteoform in the theoretical database: "
                     + String.Join(", ", SaveState.lollipop.target_proteoform_community.topdown_proteoforms.Where(t => t.ttd_match_count == 0).Select(t => t.accession.Split('_')[0]).Distinct()));
            }
            tb_exp_proteoforms.Text = SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Count(exp => exp.accepted).ToString();
            FillTablesAndCharts();
        }

        public void ClearListsTablesFigures(bool clear_following)
        {
            SaveState.lollipop.td_relations.Clear();
            SaveState.lollipop.topdownReader.topdown_ptms.Clear();
            foreach (ProteoformCommunity community in SaveState.lollipop.decoy_proteoform_communities.Values.Concat(new List<ProteoformCommunity> { SaveState.lollipop.target_proteoform_community }))
            {
                community.topdown_proteoforms = new TopDownProteoform[0];
                foreach (Proteoform p in community.experimental_proteoforms) p.relationships.RemoveAll(r => r.RelationType == ProteoformComparison.ExperimentalTopDown);
                foreach (Proteoform p in community.theoretical_proteoforms) p.relationships.RemoveAll(r => r.RelationType == ProteoformComparison.TheoreticalTopDown);
                foreach (Proteoform p in community.topdown_proteoforms) p.relationships.RemoveAll(r => r.RelationType == ProteoformComparison.ExperimentalTopDown || r.RelationType == ProteoformComparison.TheoreticalTopDown);
            }
            dgv_TD_proteoforms.DataSource = null;
            dgv_TD_proteoforms.Rows.Clear();
            tb_exp_proteoforms.Text = "";
            tb_tdProteoforms.Text = "";
            
            if (clear_following)
            {
                for (int i = ((ProteoformSweet)MdiParent).forms.IndexOf(this) + 1; i < ((ProteoformSweet)MdiParent).forms.Count; i++)
                {
                    ISweetForm sweet = ((ProteoformSweet)MdiParent).forms[i];
                    if (sweet as ExperimentExperimentComparison == null)
                        sweet.ClearListsTablesFigures(false);
                }
            }
        }

        public bool ReadyToRunTheGamut()
        {
            if (!SaveState.lollipop.input_files.Any(f => f.purpose == Purpose.TopDown))
            {
                MessageBox.Show("Go back and load in top-down results.");
                return false;
            }
            if (SaveState.lollipop.target_proteoform_community.theoretical_proteoforms.Length == 0)
            {
                MessageBox.Show("Go back and create a theoretical proteoform database.");
                return false;
            }
            return true;
        }

        private void AggregateTdHits()
        {
            if (SaveState.lollipop.top_down_hits.Count == 0) SaveState.lollipop.read_in_td_hits();
            if (SaveState.lollipop.top_down_hits.Count > 0)
            {
                SaveState.lollipop.AggregateTdHits();
                bt_targeted_td_relations.Enabled = (SaveState.lollipop.target_proteoform_community.topdown_proteoforms.Where(p => p.targeted).Count() > 0);
                tb_tdProteoforms.Text = SaveState.lollipop.target_proteoform_community.topdown_proteoforms.Count(p => !p.targeted).ToString();
            }
        }

        private void bt_td_relations_Click(object sender, EventArgs e)
        {
            if (ReadyToRunTheGamut())
            {
                RunTheGamut();
            }
        }

        public DataGridView GetTopDownDGV()
        {
            return dgv_TD_proteoforms;
        }

        private void bt_targeted_td_relations_Click(object sender, EventArgs e)
        {
            //TODO: check list of topdown id's, see if one that matches this experimental was identified
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
                        DisplayUtility.FillDataGridView(dgv_TD_family, exp.relationships.Where(r => r.RelationType == ProteoformComparison.ExperimentalTopDown || r.RelationType == ProteoformComparison.ExperimentalTheoretical).ToList());  //show E-TD or ET relations (identified)
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

            foreach (Ptm ptm in p.ptm_set.ptm_combination)
            {
                int i;
                try { i = mods.IndexOf(ptm.modification.id); }
                catch { i = 0; } //just make color blue if > 20 unique PTMs
                Color color = colors[i];

                rtb_sequence.SelectionStart = ptm.position - 1;
                rtb_sequence.SelectionLength = 1;
                rtb_sequence.SelectionColor = color;
            }

            foreach (string description in p.ptm_set.ptm_combination.Select(ptm => ptm.modification.id).Distinct())
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
            foreach (TopDownProteoform p in SaveState.lollipop.target_proteoform_community.topdown_proteoforms)
            {
                ptm.AddRange(p.ptm_set.ptm_combination);
            }
            IEnumerable<string> unique_ptm = ptm.Select(p => p.modification.id).Distinct();
            mods = unique_ptm.ToList();
        }

        private void TopDown_Load(object sender, EventArgs e)
        {

        }

        private void cmbx_td_or_e_proteoforms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbx_td_or_e_proteoforms.SelectedItem.ToString() == "TopDown Proteoforms" && SaveState.lollipop.target_proteoform_community.topdown_proteoforms.Count(p => !p.targeted) > 0)
                DisplayUtility.FillDataGridView(dgv_TD_proteoforms, SaveState.lollipop.target_proteoform_community.topdown_proteoforms.Where(p => !p.targeted).ToList());
            else if (cmbx_td_or_e_proteoforms.SelectedItem.ToString() == "Experimental Proteoforms" && SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Length > 0)
            DisplayUtility.FillDataGridView(dgv_TD_proteoforms, SaveState.lollipop.target_proteoform_community.experimental_proteoforms.Where(exp => exp.accepted).ToList());
        }
    }
}
