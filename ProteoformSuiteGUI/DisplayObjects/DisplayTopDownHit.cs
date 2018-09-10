using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

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

        #endregion Public Constructors

        #region Private Fields

        private readonly TopDownHit h;

        #endregion Private Fields

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
                        string.Join("; ", h.ptm_list.Select(ptm => ptm.position > 0 ? ptm.modification.OriginalId + "@" + ptm.position : UnlocalizedModification.LookUpId(ptm.modification)).ToList());
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

        public double pscore
        {
            get { return h.pscore; }
        }

        public double Score
        {
            get { return h.score; }
        }

        public string PFR_accession
        {
            get { return h.pfr_accession; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatTopDownHitsTable(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;

            foreach (DataGridViewColumn c in dgv.Columns)
            {
                string h = header(c.Name);
                string n = number_format(c.Name);
                c.HeaderText = h != null ? h : c.HeaderText;
                c.DefaultCellStyle.Format = n != null ? n : c.DefaultCellStyle.Format;
                c.Visible = visible(c.Name, c.Visible);
            }
        }

        public static DataTable FormatTopDownHitsTable(List<DisplayComponent> display, string table_name)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayComponent).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name), visible(x.Name, true)));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        #endregion Public Methods

        #region Private Methods

        private static bool visible(string property_name, bool current)
        {
            return current;
        }

        private static string header(string property_name)
        {
            if (property_name == nameof(input_file_filename)) { return "Input Filename"; }
            if (property_name == nameof(reported_mass)) { return "Reported Mass"; }
            if (property_name == nameof(theoretical_mass)) { return "Theoretical Mass"; }
            if (property_name == nameof(uniprot_id)) { return "Uniprot ID"; }
            if (property_name == nameof(retention_time)) { return "Retention Time"; }
            if (property_name == nameof(pscore)) { return "P-Score"; }
            if (property_name == nameof(ptm_description)) { return "PTM Description"; }
            if (property_name == nameof(PFR_accession)) { return "PFR Accession"; }
            return null;
        }

        private static string number_format(string property_name)
        {
            if (property_name == nameof(reported_mass)) { return "0.0000"; }
            if (property_name == nameof(theoretical_mass)) { return "0.0000"; }
            if (property_name == nameof(retention_time)) { return "0.00"; }
            return null;
        }

        #endregion Private Methods
    }
}