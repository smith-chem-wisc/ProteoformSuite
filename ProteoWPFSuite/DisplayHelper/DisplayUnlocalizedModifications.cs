using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ProteoWPFSuite
{
    public class DisplayUnlocalizedModification : DisplayObject
    {
        #region Public Constructors

        public DisplayUnlocalizedModification(UnlocalizedModification m)
            : base(m)
        {
            this.m = m;
        }

        #endregion Public Constructors

        #region Private Fields

        private readonly UnlocalizedModification m;

        #endregion Private Fields

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

        public string Type
        {
            get { return m.original_modification.modificationType; }
            set { m.id = value; }
        }

        public int PtmCount
        {
            get { return m.ptm_count; }
            set { m.ptm_count = value; }
        }

        public bool require_proteoform_without_mod
        {
            get { return m.require_proteoform_without_mod; }
            set { m.require_proteoform_without_mod = value; }
        }

        public int PtmRank
        {
            get { return m.ptm_rank; }
            set { m.ptm_rank = value; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatUnlocalizedModificationTable(System.Windows.Forms.DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;

            //EDITABILITY
            dgv.Columns[nameof(OriginalID)].ReadOnly = true;
            dgv.Columns[nameof(Mass)].ReadOnly = true;

            foreach (System.Windows.Forms.DataGridViewColumn c in dgv.Columns)
            {
                string h = header(c.Name);
                string n = number_format(c.Name);
                c.HeaderText = h != null ? h : c.HeaderText;
                c.DefaultCellStyle.Format = n != null ? n : c.DefaultCellStyle.Format;
                c.Visible = visible(c.Name, c.Visible);
            }
        }

        public static DataTable FormatUnlocalizedModificationTable(List<DisplayUnlocalizedModification> display, string table_name)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayUnlocalizedModification).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name), visible(x.Name, true)));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        #endregion Public Methods

        #region Private Methods

        private static string header(string property_name)
        {
            if (property_name == nameof(OriginalID)) { return "Original ID"; }
            if (property_name == nameof(ID)) { return "New ID"; }
            if (property_name == nameof(PtmCount)) { return "Num. PTMs Represented"; }
            if (property_name == nameof(require_proteoform_without_mod)) { return "Require Proteoform Without This Modification"; }
            if (property_name == nameof(PtmRank)) { return "Frequency-Based Rank of PTM Mass"; }
            return null;
        }

        private static bool visible(string property_name, bool current)
        {
            return current;
        }

        private static string number_format(string property_name)
        {
            return null;
        }

        #endregion Private Methods
    }
}