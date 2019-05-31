using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ProteoWPFSuite
{
    public class DisplayTopDownProteoform : DisplayObject
    {
        #region Public Constructors

        public DisplayTopDownProteoform(TopDownProteoform t)
            : base(t)
        {
            this.t = t;
        }

        #endregion Public Constructors

        #region Private Fields

        private readonly TopDownProteoform t;

        #endregion Private Fields

        #region Public Properties

        public string Accession
        {
            get { return t.accession; }
        }

        public string Name
        {
            get { return t.name; }
        }

        public string geneID
        {
            get { return t.geneID; }
        }

        public string Sequence
        {
            get { return t.sequence; }
        }

        public int Begin
        {
            get { return t.topdown_begin; }
        }

        public int End
        {
            get { return t.topdown_end; }
        }

        public string ptm_description
        {
            get { return t.topdown_ptm_description; }
        }

        public bool correct_id
        {
            get { return t.correct_id; }
        }

        public string PS_accession
        {
            get { return t.linked_proteoform_references.First().accession.Split('_')[0]; }
        }

        public string PS_ptm_description
        {
            get { return t.ptm_description; }
        }

        public string uniprot_mods
        {
            get { return t.linked_proteoform_references != null ? t.uniprot_mods : ""; }
        }

        public bool novel_mods
        {
            get { return t.novel_mods; }
        }


        public int theoretical_begin
        {
            get { return t.begin; }
        }

        public int theoretical_end
        {
            get { return t.end; }
        }

        public double modified_mass
        {
            get { return t.modified_mass; }
        }

        public double theoretical_mass
        {
            get { return t.theoretical_mass; }
        }

        public double retentionTime
        {
            get { return t.agg_rt; }
        }

        public double best_c_score
        {
            get { return t.topdown_hits.Max(h => h.score); }
        }

        public int Observations
        {
            get { return t.topdown_hits.Count; }
        }

        public string PFR_accession
        {
            get { return t.pfr_accession; }
        }

        public string family_id
        {
            get { return t.family != null ? t.family.family_id.ToString() : ""; }
        }

        public string manual_id
        {
            get { return t.manual_validation_id; }
        }

        public double mass_error
        {
            get { return t.mass_error; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatTopDownTable(System.Windows.Forms.DataGridView dgv, bool identified_topdown)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;

            foreach (System.Windows.Forms.DataGridViewColumn c in dgv.Columns)
            {
                string h = header(c.Name);
                string n = number_format(c.Name);
                c.HeaderText = h != null ? h : c.HeaderText;
                c.DefaultCellStyle.Format = n != null ? n : c.DefaultCellStyle.Format;
                c.Visible = visible(c.Name, c.Visible, identified_topdown);
            }
        }

        public static DataTable FormatTopDownTable(List<DisplayTopDownProteoform> display, string table_name, bool identified)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayTopDownProteoform).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name), visible(x.Name, true, identified)));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        public static string header(string name)
        {
            if (name == nameof(modified_mass)) { return "Modified Mass"; }
            if (name == nameof(ptm_description)) { return "PTM Description"; }
            if (name == nameof(retentionTime)) { return "Retention Time"; }
            if (name == nameof(theoretical_mass)) { return "Theoretical Mass"; }
            if (name == nameof(correct_id)) { return "Correct ID"; }
            if (name == nameof(PS_accession)) { return "Proteoform Suite Accession"; }
            if (name == nameof(PS_ptm_description)) { return "Proteoform Suite PTM Description"; }
            if (name == nameof(theoretical_begin)) { return "Theoretical Begin"; }
            if (name == nameof(theoretical_end)) { return "Theoretical End"; }
            if (name == nameof(best_c_score)) { return "Best Hit C-Score"; }
            if (name == nameof(manual_id)) { return "Best Hit Info"; }
            if (name == nameof(family_id)) { return "Family ID"; }
            if (name == nameof(mass_error)) { return "Mass Error"; }
            if (name == nameof(PFR_accession)) { return "PFR Accession"; }
            if (name == nameof(uniprot_mods)) return "UniProt-Annotated Modifications";
            if (name == nameof(novel_mods)) return "Potentially Novel Mods";
            return null;
        }

        private static bool visible(string property_name, bool current, bool identified_topdown)
        {
            if (!identified_topdown)
            {
                if (property_name == nameof(PS_accession)) { return false; }
                if (property_name == nameof(correct_id)) { return false; }
                if (property_name == nameof(PS_ptm_description)) { return false; }
                if (property_name == nameof(theoretical_begin)) { return false; }
                if (property_name == nameof(theoretical_end)) { return false; }
                if (property_name == nameof(family_id)) { return false; }
                if (property_name == nameof(mass_error)) { return false; }
                if (property_name == nameof(uniprot_mods)) { return false; }
                if (property_name == nameof(novel_mods)) { return false; }
            }
            return current;
        }

        private static string number_format(string property_name)
        {
            if (property_name == nameof(modified_mass)) { return "0.0000"; }
            if (property_name == nameof(theoretical_mass)) { return "0.0000"; }
            if (property_name == nameof(retentionTime)) { return "0.00"; }
            if (property_name == nameof(mass_error)) { return "0.0000"; }
            return null;
        }

        #endregion Public Methods
    }
}