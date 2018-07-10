using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Controls;
using System.Drawing;
using ProteoformSuiteInternal;
using System.Windows;
namespace ProteoWPFSuite
{
    /// <summary>
    /// Interaction logic for TopDown.xaml
    /// </summary>
    public partial class TopDown : UserControl, ISweetForm
    {
        private static Color[] colors = new Color[20];
        private static List<string> mods = new List<string>();
        private ProteoformSweet parMDI;
        public TopDown()
        {
            InitializeComponent();
            this.parMDI = ((MainWindow)MDIHelpers.getParentWindow(this)).MDIParentControl;
        }

        public void InitializeParameterSet()
        {
            tb_tableFilter.TextChanged -= tb_tableFilter_TextChanged;
            tb_tableFilter.Text = "";
            tb_tableFilter.TextChanged += tb_tableFilter_TextChanged;

            nUD_min_score_td.Value = (decimal)Sweet.lollipop.min_score_td;
            cb_biomarker.IsChecked = Sweet.lollipop.biomarker;
            cb_tight_abs_mass.IsChecked = Sweet.lollipop.tight_abs_mass;
            nUD_td_rt_tolerance.Value = (decimal)Sweet.lollipop.td_retention_time_tolerance;
        }

        public List<DataTable> DataTables { get; private set; }
        public List<DataTable> SetTables()
        {
            DataTables = new List<DataTable>
            {
                DisplayTopDownProteoform.FormatTopDownTable(Sweet.lollipop.topdown_proteoforms.Select(e => new DisplayTopDownProteoform(e)).ToList(), "TopdownProteoforms", false)
            };
            return DataTables;
        }
        public void FillTablesAndCharts()
        {
            DisplayUtility.FillDataGridView(dgv_TD_proteoforms, Sweet.lollipop.topdown_proteoforms.Select(t => new DisplayTopDownProteoform(t)));
            DisplayTopDownProteoform.FormatTopDownTable(dgv_TD_proteoforms, false);
            load_colors();
            mods = Sweet.lollipop.topdown_proteoforms.SelectMany(p => p.topdown_ptm_set.ptm_combination).Select(m => m.modification.id).Distinct().ToList();
            tb_tdProteoforms.Text = Sweet.lollipop.topdown_proteoforms.Count.ToString();
            tb_td_hits.Text = Sweet.lollipop.top_down_hits.Count.ToString();
            tb_unique_PFRs.Text = Sweet.lollipop.topdown_proteoforms.Select(p => p.pfr_accession).Distinct().Count().ToString();
        }
        public void RunTheGamut(bool full_run)
        {
            if (!full_run)
            {
                if (Sweet.lollipop.top_down_hits.Count == 0)
                {
                    ClearListsTablesFigures(true);
                    if (!Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.TopDown))
                    {
                        MessageBox.Show("Go back and load in top-down results.");
                        return;
                    }
                    if (Sweet.lollipop.target_proteoform_community.theoretical_proteoforms.Length == 0)
                    {
                        MessageBox.Show("Go back and construct a theoretical database.");
                        return;
                    }
                    Sweet.lollipop.read_in_td_hits();
                    tb_td_hits.Text = Sweet.lollipop.top_down_hits.Count.ToString();
                }
            }
            else
            {
                Sweet.lollipop.read_in_td_hits();
            }
            ClearListsTablesFigures(true);
            AggregateTdHits();
            if (!full_run)
            {
                List<string> warning_methods = new List<string>() { "Warning:" };
                if (Sweet.lollipop.topdownReader.topdown_ptms.Count > 0)
                {
                    warning_methods.Add("Top-down proteoforms with the following modifications were not matched to a modification in the theoretical PTM list: ");
                    warning_methods.Add(String.Join(", ", Sweet.lollipop.topdownReader.topdown_ptms.Distinct()));
                }
                if (Sweet.lollipop.topdown_proteoforms.Count(t => !t.accepted) > 0)
                {
                    warning_methods.Add("Top-down proteoforms with the following accessions were not matched to a theoretical proteoform in the theoretical database: ");
                    warning_methods.Add(String.Join(", ", Sweet.lollipop.topdown_proteoforms.Where(t => !t.accepted).Select(t => t.accession.Split('_')[0]).Distinct()));
                }
                if (warning_methods.Count > 1)
                {
                    MessageBox.Show(String.Join("\n\n", warning_methods));
                }
            }
            //need to refill theo database --> added theoreticsl
            
            parMDI.theoreticalDatabase.FillTablesAndCharts();
            FillTablesAndCharts();
        }
        public void ClearListsTablesFigures(bool clear_following)
        {
            if (!clear_following)
            {
                tb_td_hits.Clear();
                Sweet.lollipop.top_down_hits.Clear(); //only want to clear if cleared theo database
            }
            Sweet.lollipop.clear_td();
            dgv_TD_proteoforms.DataSource = null;
            dgv_TD_proteoforms.Rows.Clear();
            tb_tdProteoforms.Clear();
            tb_unique_PFRs.Clear();
            tb_tableFilter.Clear();
            rtb_sequence.Clear();
            if (clear_following)
            {
                for (int i = parMDI.forms.IndexOf(this) + 1; i < parMDI.forms.Count; i++)
                {
                    ISweetForm sweet = parMDI.forms[i];
                    if (sweet as RawExperimentalComponents == null)
                        sweet.ClearListsTablesFigures(false);
                }
            }
        }
        public bool ReadyToRunTheGamut()
        {
            return true;
        }

        private void AggregateTdHits()
        {
            if (Sweet.lollipop.top_down_hits.Count > 0)
            {
                Sweet.lollipop.clear_td();
                Sweet.lollipop.topdown_proteoforms = Sweet.lollipop.aggregate_td_hits(Sweet.lollipop.top_down_hits, Sweet.lollipop.min_score_td, Sweet.lollipop.biomarker, Sweet.lollipop.tight_abs_mass);
                Sweet.lollipop.theoretical_database.make_theoretical_proteoforms();
            }
        }

        private void bt_td_relations_Click(object sender, EventArgs e)
        {
            if (ReadyToRunTheGamut())
            {
                RunTheGamut(false);
            }
        }

        public System.Windows.Forms.DataGridView GetTopDownDGV()
        {
            return dgv_TD_proteoforms;
        }

        
        private void dgv_TD_proteoforms_CellContentClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                TopDownProteoform p = (TopDownProteoform)((DisplayObject)this.dgv_TD_proteoforms.Rows[e.RowIndex].DataBoundItem).display_object;
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
            foreach (Ptm ptm in p.topdown_ptm_set.ptm_combination)
            {
                int position_in_sequence = ptm.position + 1 - p.topdown_begin;
                int i = 0;
                Color color;
                try
                {
                    i = mods.IndexOf(ptm.modification.id);
                    color = colors[i];
                }
                catch
                {
                    i = 0;
                    color = colors[0];
                } //just make color blue if > 20 unique PTMs

                rtb_sequence.SelectionStart = position_in_sequence - 1;
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

        private void TopDown_Load(object sender, EventArgs e)
        {

        }

        private void nUD_min_score_td_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.min_score_td = Convert.ToDouble(nUD_min_score_td.Value);
        }

        private void cb_tight_abs_mass_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.tight_abs_mass = (bool)cb_tight_abs_mass.IsChecked;
        }

        private void cb_biomarker_CheckedChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.biomarker = (bool)cb_biomarker.IsChecked;
        }
        private void tb_tableFilter_TextChanged(object sender, EventArgs e)
        {
            IEnumerable<object> selected_td = tb_tableFilter.Text == "" ?
               Sweet.lollipop.topdown_proteoforms :
               ExtensionMethods.filter(Sweet.lollipop.topdown_proteoforms, tb_tableFilter.Text);
            DisplayUtility.FillDataGridView(dgv_TD_proteoforms, selected_td.OfType<TopDownProteoform>().Select(t => new DisplayTopDownProteoform(t)));
            DisplayTopDownProteoform.FormatTopDownTable(dgv_TD_proteoforms, false);

        }

        private void nUD_td_rt_tolerance_ValueChanged(object sender, EventArgs e)
        {
            Sweet.lollipop.td_retention_time_tolerance = (double)nUD_td_rt_tolerance.Value;
        }
        
    }
}
