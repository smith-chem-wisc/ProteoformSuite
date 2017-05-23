using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class DisplayUnlocalizedModification : DisplayObject
    {

        #region Public Constructors

        public DisplayUnlocalizedModification(UnlocalizedModification m)
            : base(m)
        {
            this.m = m;
        }

        #endregion

        #region Private Fields

        private UnlocalizedModification m;

        #endregion

        #region Public Properties

        public string OriginalID
        {
            get { return m.original_modification.id; }
        }

        public double Mass
        {
            get { return m.mass; }
        }

        public string ID
        {
            get { return m.id; }
            set { m.id = value; }
        }

        public int PtmCount
        {
            get { return m.ptm_count; }
            set { m.ptm_count = value; }
        }

        public int PtmRank
        {
            get { return m.ptm_rank; }
            set { m.ptm_rank = value; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatUnlocalizedModificationTable(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;

            //HEADERS
            dgv.Columns[nameof(OriginalID)].HeaderText = "Original ID";
            dgv.Columns[nameof(ID)].HeaderText = "New ID";
            dgv.Columns[nameof(PtmCount)].HeaderText = "Num. PTMs Represented";
            dgv.Columns[nameof(PtmRank)].HeaderText = "Frequency-Based Rank of PTM Mass";

            //EDITABILITY
            dgv.Columns[nameof(OriginalID)].ReadOnly = true;
            dgv.Columns[nameof(Mass)].ReadOnly = true;
        }

        #endregion Public Methods

    }
}
