using ProteoformSuiteInternal;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class DisplayProteoformFamily : DisplayObject
    {

        #region Public Constructors

        public DisplayProteoformFamily(ProteoformFamily f)
            : base(f)
        {
            this.f = f;
        }

        #endregion

        #region Private Fields

        private ProteoformFamily f;

        #endregion

        #region Public Properties

        public int family_id
        {
            get { return f.family_id; }
        }

        public int lysine_count
        {
            get { return f.lysine_count; }
        }

        public int experimental_count
        {
            get { return f.experimental_proteoforms.Count; }
        }

        public string experimentals_list
        {
            get { return f.experimentals_list; }
        }

        public string agg_mass_list
        {
            get { return f.agg_mass_list; }
        }

        public int theoretical_count
        {
            get { return f.theoretical_proteoforms.Count; }
        }

        public int topdown_count
        {
            get { return f.topdown_proteoforms.Count; }
        }

        public string accession_list
        {
            get { return f.accession_list; }
        }

        public string name_list
        {
            get { return f.name_list; }
        }

        public string gene_list
        {
            get { return f.gene_list; }
        }

        public int relation_count
        {
            get { return f.relations.Count; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void format_families_dgv(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.ReadOnly = true;

            //set column header
            dgv.Columns[nameof(family_id)].HeaderText = "Family ID";
            dgv.Columns[nameof(lysine_count)].HeaderText = "Lysine Count";
            dgv.Columns[nameof(experimental_count)].HeaderText = "Experimental Proteoforms";
            dgv.Columns[nameof(theoretical_count)].HeaderText = "Theoretical Proteoforms";
            dgv.Columns[nameof(relation_count)].HeaderText = "Relation Count";
            dgv.Columns[nameof(accession_list)].HeaderText = "Theoretical Accessions";
            dgv.Columns[nameof(name_list)].HeaderText = "Theoretical Names";
            dgv.Columns[nameof(gene_list)].HeaderText = "Gene Names";
            dgv.Columns[nameof(experimentals_list)].HeaderText = "Experimental Accessions";
            dgv.Columns[nameof(agg_mass_list)].HeaderText = "Experimental Aggregated Masses";
            dgv.Columns[nameof(topdown_count)].HeaderText = "TopDown Proteoforms";
        }

        #endregion

    }
}
