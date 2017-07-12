using ProteoformSuiteInternal;
using System.Windows.Forms;

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
            get { return t.start_index; }
        }

        public int End
        {
            get { return t.stop_index; }
        }

        public string pfr
        {
            get { return t.pfr; }
        }

        public double modified_mass
        {
            get { return t.modified_mass; }
        }

        public string ptm_description
        {
            get { return t.ptm_description; }
        }

        public int bottomUpPSMcount
        {
            get { return t.bottom_up_PSMs;  }
        }

        public double retentionTime
        {
            get { return t.agg_RT; }
        }

        public double theoretical_mass
        {
            get { return t.theoretical_mass;  }
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
            dgv.Columns[nameof(pfr)].HeaderText = "PFR";
            dgv.AllowUserToAddRows = false;
        }

        #endregion

    }
}
