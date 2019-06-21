using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
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

        #endregion Public Constructors

        #region Private Fields

        private readonly TopDownProteoform t;

        #endregion Private Fields

        #region Public Properties

        public string Accession
        {
            get
            {
                return t.accession + (t.ambiguous_topdown_hits.Count > 0
                           ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(p => p.accession))
                           : "");
            }
        }

        public string Name
        {
            get
            {
                return t.name + (t.ambiguous_topdown_hits.Count > 0
                           ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(p => p.name))
                           : "");
            }
        }

        public string Sequence
        {
            get
            {
                return t.sequence + (t.ambiguous_topdown_hits.Count > 0
                           ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(p => p.sequence))
                           : "");
            }
        }

        public string Begin
        {
            get
            {
                return t.topdown_begin + (t.ambiguous_topdown_hits.Count > 0
                           ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(p => p.begin))
                           : "");
            }
        }

        public string End
        {
            get
            {
                return t.topdown_end + (t.ambiguous_topdown_hits.Count > 0
                           ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(p => p.end))
                           : "");
            }
        }

        public string ptm_description
        {
            get
            {
                return t.topdown_ptm_description + (t.ambiguous_topdown_hits.Count > 0
                     ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(p => p.ptm_description))
                     : "");
            }
        }

        public string bu_PSMs_count
        {
            get
            {
                return Proteoform.get_possible_PSMs(t.accession.Split('_')[0], t.topdown_ptm_set, t.topdown_begin, t.topdown_end).Count.ToString()
                       + (t.ambiguous_topdown_hits.Count > 0
                           ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(i => Proteoform.get_possible_PSMs(i.accession.Split('_')[0], new PtmSet(i.ptm_list), i.begin, i.end).Count.ToString()))
                    :"");
            }
        }

        public string bu_PSMs
        {
            get
            {
                return Proteoform.get_possible_PSMs(t.accession.Split('_')[0], t.topdown_ptm_set, t.topdown_begin,
                           t.topdown_end).Count(p => p.ptm_list.Count > 0) == 0
                    ? "N/A"
                    : String.Join(", ",
                          Proteoform.get_possible_PSMs(t.accession.Split('_')[0], t.topdown_ptm_set, t.topdown_begin,
                              t.topdown_end).Where(p => p.ptm_list.Count > 0).Select(p => p.ptm_description).Distinct())
                      + (t.ambiguous_topdown_hits.Count > 0
                          ? " | " + String.Join(" | ",
                                t.ambiguous_topdown_hits.Select(i =>
                                    Proteoform.get_possible_PSMs(i.accession.Split('_')[0], new PtmSet(i.ptm_list),
                                        i.begin, i.end).Count(p => p.ptm_list.Count > 0) == 0
                                        ? "N/A"
                                        : String.Join(", ",
                                            Proteoform.get_possible_PSMs(i.accession.Split('_')[0],
                                                    new PtmSet(i.ptm_list), i.begin, i.end)
                                                .Where(p => p.ptm_list.Count > 0)
                                                .Select(p => p.ptm_description).Distinct())))
                          : "");
            }
        }

        public double modified_mass
        {
            get { return t.modified_mass; }
        }

        public string theoretical_mass
        {
            get
            {
                return t.theoretical_mass + (t.ambiguous_topdown_hits.Count > 0
                                 ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(p => p.theoretical_mass))
                                 : "");
            }
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
            get
            {
                return t.pfr_accession + (t.ambiguous_topdown_hits.Count > 0
                           ? " | " + String.Join(" | ", t.ambiguous_topdown_hits.Select(p => p.pfr_accession))
                           : "");
            }
        }

        public string family_id
        {
            get { return t.family != null ? t.family.family_id.ToString() : ""; }
        }

        public string manual_id
        {
            get { return t.manual_validation_id; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatTopDownTable(DataGridView dgv, bool identified_topdown)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;

            foreach (DataGridViewColumn c in dgv.Columns)
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
            if (name == nameof(best_c_score)) { return "Best Hit C-Score"; }
            if (name == nameof(manual_id)) { return "Best Hit Info"; }
            if (name == nameof(family_id)) { return "Family ID"; }
            if (name == nameof(PFR_accession)) { return "PFR Accession"; }
            if (name == nameof(bu_PSMs)) return "Modified Bottom-Up PSMs";
            if (name == nameof(bu_PSMs_count)) return "Bottom-Up PSMs Count";
            return null;
        }

        private static bool visible(string property_name, bool current, bool identified_topdown)
        {
            if (!identified_topdown)
            {
                if (property_name == nameof(family_id)) { return false; }
            }
            return current;
        }

        private static string number_format(string property_name)
        {
            if (property_name == nameof(modified_mass)) { return "0.0000"; }
            if (property_name == nameof(theoretical_mass)) { return "0.0000"; }
            if (property_name == nameof(retentionTime)) { return "0.00"; }
            return null;
        }

        #endregion Public Methods
    }
}