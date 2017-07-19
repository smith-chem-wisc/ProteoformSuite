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
            DisplayUtility.FillDataGridView(dgv_TD_proteoforms, Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Select(t => new DisplayTopDownProteoform(t)));
            DisplayTopDownProteoform.FormatTopDownProteoformTable(dgv_TD_proteoforms);
            load_colors();
            load_ptm_colors();
        }

        public void load_topdown()
        {
            if (ReadyToRunTheGamut())
            {
                RunTheGamut();
            }
        }

        public void RunTheGamut()
        {
            ClearListsTablesFigures(true);
            AggregateTdHits();
            if (Sweet.lollipop.topdownReader.topdown_ptms.Count > 0)
            {
                MessageBox.Show("Warning: Top-down proteoforms with the following modifications were not matched to a modification in the theoretical PTM list: "
                  + String.Join(", ", Sweet.lollipop.topdownReader.topdown_ptms.Select(m => m.id + " at " + m.motif.Motif)));
            }
            Sweet.lollipop.td_relations = Sweet.lollipop.target_proteoform_community.relate_td();
            Parallel.ForEach(Sweet.lollipop.decoy_proteoform_communities.Values, c =>
            {
                c.relate_td();
            });
            if (Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Count(t => t.ttd_match_count == 0) > 0)
            {
                MessageBox.Show("Warning: Top-down proteoforms with the following accessions were not matched to a theoretical proteoform in the theoretical database: "
                     + String.Join(", ", Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Where(t => t.ttd_match_count == 0).Select(t => t.accession.Split('_')[0]).Distinct()));
            }
            FillTablesAndCharts();
        }

        public void ClearListsTablesFigures(bool clear_following)
        {
            Sweet.lollipop.top_down_hits.Clear();
            Sweet.lollipop.td_relations.Clear();
            Sweet.lollipop.topdownReader.topdown_ptms.Clear();
            foreach (ProteoformCommunity community in Sweet.lollipop.decoy_proteoform_communities.Values.Concat(new List<ProteoformCommunity> { Sweet.lollipop.target_proteoform_community }))
            {
                community.topdown_proteoforms = new TopDownProteoform[0];
                foreach (Proteoform p in community.experimental_proteoforms) p.relationships.RemoveAll(r => r.RelationType == ProteoformComparison.ExperimentalTopDown);
                foreach (Proteoform p in community.theoretical_proteoforms) p.relationships.RemoveAll(r => r.RelationType == ProteoformComparison.TheoreticalTopDown);
                foreach (Proteoform p in community.topdown_proteoforms) p.relationships.RemoveAll(r => r.RelationType == ProteoformComparison.ExperimentalTopDown || r.RelationType == ProteoformComparison.TheoreticalTopDown);
            }
            dgv_TD_proteoforms.DataSource = null;
            dgv_TD_proteoforms.Rows.Clear();
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
            if (!Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.TopDown))
            {
                MessageBox.Show("Go back and load in top-down results.");
                return false;
            }
            if (Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length == 0)
            {
                MessageBox.Show("Go back and create a theoretical proteoform database.");
                return false;
            }
            return true;
        }

        private void AggregateTdHits()
        {
             Sweet.lollipop.read_in_td_hits();
            if (Sweet.lollipop.top_down_hits.Count > 0)
            {
                List<TopDownProteoform> topdown_proteoforms = Sweet.lollipop.AggregateTdHits(Sweet.lollipop.top_down_hits);
                Sweet.lollipop.target_proteoform_community.topdown_proteoforms = topdown_proteoforms.Where(p => p != null).ToArray();
                foreach(ProteoformCommunity community in Sweet.lollipop.decoy_proteoform_communities.Values)
                {
                    community.topdown_proteoforms = Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Select(e => new TopDownProteoform(e)).ToArray();
                }
                tb_tdProteoforms.Text = Sweet.lollipop.target_proteoform_community.topdown_proteoforms.Length.ToString();
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


        private void dgv_TD_proteoforms_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                dgv_TD_family.DataSource = null;

                TopDownProteoform p = (TopDownProteoform)((DisplayObject)this.dgv_TD_proteoforms.Rows[e.RowIndex].DataBoundItem).display_object;
                if (p.relationships != null)
                    {
                        DisplayUtility.FillDataGridView(dgv_TD_family, p.relationships.Select(r => new DisplayProteoformRelation(r)));  //show T-TD and E-TD relations
                        DisplayProteoformRelation.FormatRelationsGridView(dgv_TD_family, false, false);
                    }
                    get_proteoform_sequence(p);
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

            foreach (Ptm ptm in p.ptm_set.ptm_combination.Where(m => m.position > 0))
            {
                int i;
                try { i = mods.IndexOf(ptm.modification.id); }
                catch { i = 0; } //just make color blue if > 20 unique PTMs
                Color color = colors[i];

                rtb_sequence.SelectionStart = ptm.position - 1;
                rtb_sequence.SelectionLength = 1;
                rtb_sequence.SelectionColor = color;
          
                rtb_sequence.AppendText("\n" + ptm.modification.id);
                rtb_sequence.SelectionStart = length;
                rtb_sequence.SelectionLength = ptm.modification.id.Length + 1;
                rtb_sequence.SelectionColor = colors[i];
                length += ptm.modification.id.Length + 1;
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
            foreach (TopDownProteoform p in Sweet.lollipop.target_proteoform_community.topdown_proteoforms)
            {
                ptm.AddRange(p.ptm_set.ptm_combination);
            }
            IEnumerable<string> unique_ptm = ptm.Select(p => p.modification.id).Distinct();
            mods = unique_ptm.ToList();
        }

        private void TopDown_Load(object sender, EventArgs e)
        {

        }

        private void nUD_min_RT_td_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_RT_td = Convert.ToDouble(nUD_min_RT_td.Value);
        }

        private void nUD_max_RT_td_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.max_RT_td = Convert.ToDouble(nUD_max_RT_td.Value);
        }

        private void nUD_min_score_td_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_score_td = Convert.ToDouble(nUD_min_score_td.Value);
        }

        private void cb_tight_abs_mass_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.tight_abs_mass = cb_tight_abs_mass.Checked;
        }

        private void cb_biomarker_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.biomarker = cb_biomarker.Checked;
        }
    }
}
