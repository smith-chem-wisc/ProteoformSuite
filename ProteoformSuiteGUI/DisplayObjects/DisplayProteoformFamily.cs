using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
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

        #endregion Public Constructors

        #region Private Fields

        private readonly ProteoformFamily f;

        #endregion Private Fields

        #region Public Properties

        public int family_id
        {
            get { return f.family_id; }
        }

        public int experimental_count
        {
            get { return f.experimental_proteoforms.Count(p => !p.topdown_id); }
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
            get { return f.experimental_proteoforms.Count(e => e.topdown_id); }
        }

        public int gene_count
        {
            get { return f.gene_names.Select(p => p.get_prefered_name(Lollipop.preferred_gene_label)).Where(n => n != null).Distinct().Count(); }
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

        public static void FormatFamiliesTable(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.ReadOnly = true;

            foreach (DataGridViewColumn c in dgv.Columns)
            {
                string h = header(c.Name);
                c.HeaderText = h != null ? h : c.HeaderText;
            }
        }

        public static DataTable FormatFamiliesTable(List<DisplayProteoformFamily> display, string table_name)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayProteoformFamily).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name), true));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        #endregion Public Methods

        #region Private Methods

        private static string header(string property_name)
        {
            if (property_name == nameof(family_id)) { return "Family ID"; }
            if (property_name == nameof(experimental_count)) { return "Experimental Proteoforms"; }
            if (property_name == nameof(theoretical_count)) { return "Theoretical Proteoforms"; }
            if (property_name == nameof(relation_count)) { return "Relation Count"; }
            if (property_name == nameof(accession_list)) { return "Theoretical Accessions"; }
            if (property_name == nameof(name_list)) { return "Theoretical Names"; }
            if (property_name == nameof(gene_list)) { return "Gene Names"; }
            if (property_name == nameof(experimentals_list)) { return "Experimental Accessions"; }
            if (property_name == nameof(agg_mass_list)) { return "Experimental Aggregated Masses"; }
            if (property_name == nameof(topdown_count)) { return "Top-Down Proteoforms"; }
            if (property_name == nameof(gene_count)) { return "Gene Count"; }
            return null;
        }

        #endregion Private Methods
    }
}