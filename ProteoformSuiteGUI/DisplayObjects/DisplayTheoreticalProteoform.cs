using ProteoformSuiteInternal;
using System.Windows.Forms;
using System.Linq;
using System;
using System.Data;


namespace ProteoformSuiteGUI
{
    public class DisplayTheoreticalProteoform : DisplayObject
    {

        #region Public Constructors

        public DisplayTheoreticalProteoform(TheoreticalProteoform t)
            : base(t)
        {
            this.t = t;
        }

        #endregion

        #region Private Fields

        private TheoreticalProteoform t;

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

        public string Description
        {
            get { return t.description; }
        }

        public string gene_name
        {
            get
            {
                return t.gene_name != null ?
                    t.gene_name.get_prefered_name(Lollipop.preferred_gene_label) :
                    "";
            }
        }

        public string Fragment
        {
            get { return t.fragment; }
        }

        public int Begin
        {
            get { return t.begin; }
        }

        public int End
        {
            get { return t.end; }
        }

        public string Sequence
        {
            get { return t.sequence; }
        }

        public double modified_mass
        {
            get { return t.modified_mass; }
        }

        public double unmodified_mass
        {
            get { return t.unmodified_mass; }
        }

        public double ptm_mass
        {
            get { return t.ptm_mass; }
        }

        public bool Contaminant
        {
            get { return t.contaminant; }
        }

        public double lysine_count
        {
            get { return t.lysine_count; }
        }

        public string ptm_description
        {
            get { return t.ptm_description; }
        }

        public string goTerm_IDs
        {
            get { return t.goTerm_IDs; }
        }

        public int bottomUpPSMcount
        {
            get { return t.psm_list.Count;  }
        }
        
        public string groupedAccessions
        {
            get { return String.Join(", ", t.ExpandedProteinList.SelectMany(p => p.AccessionList).Select(a => a.Split('_')[0]).Distinct()); }
        }

        public bool topdown_theoretical
        {
            get { return t.topdown_theoretical;  }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatTheoreticalProteoformTable(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.ReadOnly = true;

            //round table values
            dgv.Columns[nameof(unmodified_mass)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(ptm_mass)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(modified_mass)].DefaultCellStyle.Format = "0.####";

            //set column header
            dgv.Columns[nameof(unmodified_mass)].HeaderText = "Unmodified Mass";
            dgv.Columns[nameof(ptm_mass)].HeaderText = "PTM Mass";
            dgv.Columns[nameof(ptm_description)].HeaderText = "PTM Description";
            dgv.Columns[nameof(modified_mass)].HeaderText = "Modified Mass";
            dgv.Columns[nameof(lysine_count)].HeaderText = "Lysine Count";
            dgv.Columns[nameof(goTerm_IDs)].HeaderText = "GO Term IDs";
            dgv.Columns[nameof(gene_name)].HeaderText = "Gene Name";
            dgv.Columns[nameof(bottomUpPSMcount)].HeaderText = "Bottom-Up PSM Count";
            dgv.Columns[nameof(groupedAccessions)].HeaderText = "Grouped Theoretical Accessions";
            dgv.Columns[nameof(topdown_theoretical)].HeaderText = "Top-Down Theoretical";

            dgv.AllowUserToAddRows = false;
        }
        
        #endregion

    }
}
