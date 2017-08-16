using System.Windows.Forms;
using ProteoformSuiteInternal;
using System;
using System.Linq;

namespace ProteoformSuiteGUI
{
    public class DisplayTopDownHit : DisplayObject
    {

        #region Public Constructors

        public DisplayTopDownHit(TopDownHit h)
            : base(h)
        {
            this.h = h;
        }

        #endregion

        #region Private Fields

        private TopDownHit h;

        #endregion

        #region Public Properties

        public string input_file_filename
        {
            get { return h.filename; }
        }

        public int Scan
        {
            get { return h.ms2ScanNumber; }
        }

        public double reported_mass
        {
            get { return h.reported_mass; }
        }

        public double theoretical_mass
        {
            get { return h.theoretical_mass; }
        }

        public double retention_time
        {
            get { return h.ms2_retention_time; }
        }

        public string uniprot_id
        {
            get { return h.uniprot_id; }
        }

        public string Sequence
        {
            get { return h.sequence; }
        }

        public int Begin
        {
            get { return h.begin; }
        }

        public int End
        {
            get { return h.end; }
        }

        public string ptm_description
        {
            get
            {
                return h.ptm_list == null ?
                    "Unknown" :
                    h.ptm_list.Count == 0 ?
                        "Unmodified" :
                        String.Join("; ", h.ptm_list.Select(ptm => ptm.position > 0 ? ptm.modification.id + "@" + ptm.position : Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id).ToList());
            }
        }
        
        public string Accession
        {
            get { return h.accession; }
        }

        public string Name
        {
            get { return h.name; }
        }

        public double pvalue
        {
            get { return h.pvalue; }
        }

        public double Score
        {
            get { return h.score; }
        }

        public string PFR
        {
            get { return h.pfr; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatTopdownHitsTable(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;

            //round table values
            dgv.Columns[nameof(reported_mass)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(theoretical_mass)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(retention_time)].DefaultCellStyle.Format = "0.##";
            dgv.Columns[nameof(pvalue)].DefaultCellStyle.Format = "0.####";

            //Headers
            dgv.Columns[nameof(input_file_filename)].HeaderText = "Input Filename";
            dgv.Columns[nameof(reported_mass)].HeaderText = "Reported Mass";
            dgv.Columns[nameof(theoretical_mass)].HeaderText = "Theoreitcal Mass";
            dgv.Columns[nameof(retention_time)].HeaderText = "Retention Time";
            dgv.Columns[nameof(uniprot_id)].HeaderText = "Uniprot ID";
            dgv.Columns[nameof(ptm_description)].HeaderText = "PTM Description";
            dgv.Columns[nameof(pvalue)].HeaderText = "P-Score";

        }

        #endregion

    }
}
