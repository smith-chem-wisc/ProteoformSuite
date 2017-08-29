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

namespace ProteoformSuiteGUI
{
    public partial class IdentifiedProteoforms : Form , ISweetForm
    {
        public IdentifiedProteoforms()
        {
            InitializeComponent();
        }

        public bool ReadyToRunTheGamut()
        {
            return true;
        }

        public void RunTheGamut(bool full_run)
        {
            ClearListsTablesFigures(true);
            FillTablesAndCharts();
        }


        public void ClearListsTablesFigures(bool clear_following)
        {
            dgv_identified_experimentals.DataSource = null;
            dgv_identified_experimentals.Rows.Clear();
            dgv_td_proteoforms.DataSource = null;
            dgv_td_proteoforms.Rows.Clear();
        }

        public void InitializeParameterSet()
        {
            //not applicable
        }

        public void FillTablesAndCharts()
        {
            DisplayUtility.FillDataGridView(dgv_identified_experimentals, Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.experimental_proteoforms)
                .Where(e => !e.topdown_id && e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct)).Select(e => new DisplayExperimentalProteoform(e)));
            DisplayExperimentalProteoform.FormatAggregatesTable(dgv_identified_experimentals);
            DisplayUtility.FillDataGridView(dgv_td_proteoforms, Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.experimental_proteoforms.Where(e => e.topdown_id && e.linked_proteoform_references != null)).Select(e => new DisplayTopDownProteoform(e as TopDownProteoform)));
            DisplayTopDownProteoform.FormatTopDownTable(dgv_td_proteoforms, true);
            tb_not_td.Text = "Identified Experimental Proteoforms Not in Top-Down";
            tb_topdown.Text = "Top-Down Proteoforms";
        }

        public List<DataTable> DataTables { get; private set; }
        public List<DataTable> SetTables()
        {
            DataTables = new List<DataTable>
            {
                DisplayTopDownProteoform.FormatTopDownTable( Sweet.lollipop.target_proteoform_community.families.SelectMany(f => f.experimental_proteoforms.Where(e => e.topdown_id && e.linked_proteoform_references != null)).Select(e => new DisplayTopDownProteoform(e as TopDownProteoform)).ToList(), "TopdownProteoforms", true),
                DisplayExperimentalProteoform.FormatAggregatesTable(Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Where(e => !e.topdown_id && e.linked_proteoform_references != null && (Sweet.lollipop.count_adducts_as_identifications || !e.adduct)).Select(e => new DisplayExperimentalProteoform(e)).ToList(), "IdentifiedExperimentals")
            };
            return DataTables;
        }
    }
}
