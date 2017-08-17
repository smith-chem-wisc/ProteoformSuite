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

        public string PFR
        {
            get { return t.pfr; }
        }

        public double modified_mass
        {
            get { return t.modified_mass; }
        }

        public string ptm_description
        {
            get { return t.topdown_ptm_description; }
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

        public double theoretical_mass
        {
            get { return t.theoretical_mass;  }
        }

        public int Observations
        {
            get { return t.topdown_hits.Count; }
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
        }
        #endregion
    }
}
