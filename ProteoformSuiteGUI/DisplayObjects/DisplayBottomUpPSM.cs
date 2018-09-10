//using ProteoformSuiteInternal;
//using System.Windows.Forms;

//namespace ProteoformSuiteGUI
//{
//    public class DisplayBottomUpPSM : DisplayObject
//    {
//        #region Public Constructors

//        public DisplayBottomUpPSM(BottomUpPSM t)
//            : base(t)
//        {
//            this.t = t;
//        }

//        #endregion

//        #region Private Fields

//        private BottomUpPSM t;

//        #endregion

//        #region Public Properties

//        public string Accession
//        {
//            get { return t.protein_accession; }
//        }

//        public string Description
//        {
//            get { return t.protein_description; }
//        }

//        public string Sequence
//        {
//            get { return t.sequence_with_modifications; }
//        }

//        public string Begin
//        {
//            get { return t.start_residue; }
//        }

//        public string End
//        {
//            get { return t.stop_residue; }
//        }

//        public double precursor_mass_error
//        {
//            get { return t.precursor_mass_error; }
//        }

//        public string ptm_description
//        {
//            get { return t.ptm_descriptions; }
//        }

//        #endregion Public Properties

//        #region Public Methods

//        public static void FormatTopDownProteoformTable(DataGridView dgv)
//        {
//            if (dgv.Columns.Count <= 0) return;

//            dgv.ReadOnly = true;

//            //round table values
//            dgv.Columns[nameof(precursor_mass_error)].DefaultCellStyle.Format = "0.####";

//            //set column header
//            dgv.Columns[nameof(ptm_description)].HeaderText = "PTM Description";
//            dgv.Columns[nameof(precursor_mass_error)].HeaderText = "Precursor Mass Error";

//            dgv.AllowUserToAddRows = false;
//        }

//        #endregion

//    }
//}