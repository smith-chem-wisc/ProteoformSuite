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
            dgv.AllowUserToAddRows = false;
        }

        #endregion

    }
}
