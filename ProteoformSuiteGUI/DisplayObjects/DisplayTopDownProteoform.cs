using ProteoformSuiteInternal;
using System.Windows.Forms;
using System.Linq;

namespace ProteoformSuiteGUI
{
    public class DisplayTopDownProteoform : DisplayObject
    {

        #region Public Constructors

        public DisplayTopDownProteoform(TopDownProteoform t)
            : base(t)
        {
            this.t = t;
        }

        #endregion

        #region Private Fields

        private TopDownProteoform t;

        #endregion

        #region Public Properties

        public string Accession
        {
            get { return t.accession; }
        }

        public string Name
        {
            get { return t.name; }
        }

        public string Sequence
        {
            get { return t.sequence; }
        }

        public int Begin
        {
            get { return t.begin; }
        }

        public int End
        {
            get { return t.end; }
        }

        public string ptm_description
        {
            get { return t.topdown_ptm_description; }
        }

        public bool correct_id
        {
            get { return t.correct_id; }
        }

        public string theoretical_accession
        {
            get { return t.linked_proteoform_references.First().accession; }
        }

        public string theoretical_ptm_description
        {
            get { return t.linked_proteoform_references.First().ptm_description; }
        }

        public int theoretical_begin
        {
            get { return (t.linked_proteoform_references.First() as TheoreticalProteoform).begin; }
        }

        public int theoretical_end
        {
            get { return (t.linked_proteoform_references.First() as TheoreticalProteoform).end; }
        }

        public double modified_mass
        {
            get { return t.modified_mass; }
        }

        public double theoretical_mass
        {
            get { return t.theoretical_mass; }
        }

        public int bottomUpPSMcount
        {
            get
            {
                try
                {
                    return t.relationships.Sum(r => r.connected_proteoforms.OfType<TheoreticalProteoform>().SelectMany(t => t.psm_list).Distinct().Count());
                }
                catch { return 0; }
            }
        }

        public double retentionTime
        {
            get { return t.agg_rt; }
        }

        public int Observations
        {
            get { return t.topdown_hits.Count; }
        }


        public string PFR
        {
            get { return t.pfr; }
        }

        public int bottomup_PSMs
        {
            get { return (t.linked_proteoform_references.First() as TheoreticalProteoform).psm_list.Count; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatTopDownProteoformTable(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.ReadOnly = true;

            //round table values
            dgv.Columns[nameof(modified_mass)].DefaultCellStyle.Format = "0.####";

            //set column header
            dgv.Columns[nameof(ptm_description)].HeaderText = "PTM Description";
            dgv.Columns[nameof(modified_mass)].HeaderText = "Modified Mass";
            dgv.Columns[nameof(bottomUpPSMcount)].HeaderText = "Bottom-Up PSM Count";
            dgv.Columns[nameof(retentionTime)].HeaderText = "Retention Time";
            dgv.Columns[nameof(theoretical_mass)].HeaderText = "Theoretical Mass";
            dgv.AllowUserToAddRows = false;

            //VISIBILITY
            dgv.Columns[nameof(bottomup_PSMs)].Visible = false;
            dgv.Columns[nameof(theoretical_accession)].Visible = false;
            dgv.Columns[nameof(correct_id)].Visible = false;
            dgv.Columns[nameof(theoretical_ptm_description)].Visible = false;
            dgv.Columns[nameof(theoretical_begin)].Visible = false;
            dgv.Columns[nameof(theoretical_end)].Visible = false;

        }

        public static void FormatIdentifiedProteoformTable(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;

            dgv.Columns[nameof(modified_mass)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(theoretical_mass)].DefaultCellStyle.Format = "0.####";

            //set column header
            dgv.Columns[nameof(ptm_description)].HeaderText = "PTM Description";
            dgv.Columns[nameof(modified_mass)].HeaderText = "Modified Mass";
            dgv.Columns[nameof(bottomUpPSMcount)].HeaderText = "Bottom-Up PSM Count";
            dgv.Columns[nameof(retentionTime)].HeaderText = "Retention Time";
            dgv.Columns[nameof(theoretical_mass)].HeaderText = "Theoretical Mass";
            dgv.Columns[nameof(correct_id)].HeaderText = "Correct ID";
            dgv.Columns[nameof(theoretical_accession)].HeaderText = "Theoretical Accession";
            dgv.Columns[nameof(theoretical_ptm_description)].HeaderText = "Theoretical PTM Description";
            dgv.Columns[nameof(theoretical_begin)].HeaderText = "Theoretical Begin";
            dgv.Columns[nameof(theoretical_end)].HeaderText = "Theoretical End";
            dgv.Columns[nameof(bottomup_PSMs)].HeaderText = "BottomUp PSMs Count";

            dgv.AllowUserToAddRows = false;
        }
        #endregion
    }
}
