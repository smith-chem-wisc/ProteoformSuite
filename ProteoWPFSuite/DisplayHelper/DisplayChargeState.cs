using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ProteoWPFSuite
{
    public class DisplayChargeState : DisplayObject
    {
        #region Public Constructors

        public DisplayChargeState(ChargeState c)
            : base(c)
        {
            this.c = c;
        }

        #endregion Public Constructors

        #region Private Fields

        private readonly ChargeState c;

        #endregion Private Fields

        #region Public Properties

        public double calculated_mass
        {
            get { return c.calculated_mass; }
        }

        // Pulled from the Thermo Deconvolution Results. Not used elsewhere.
        public double mz_centroid
        {
            get { return c.mz_centroid; }
        }

        public double intensity
        {
            get { return c.intensity; }
        }

        public int charge_count
        {
            get { return c.charge_count; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatChargeStateTable(System.Windows.Forms.DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0)
            {
                return;
            }

            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;

            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                System.Windows.Forms.DataGridViewColumn c = dgv.Columns[i];
                string h = header(c.Name);
                string n = number_format(c.Name);
                c.HeaderText = h != null ? h : c.HeaderText;
                c.DefaultCellStyle.Format = n != null ? n : c.DefaultCellStyle.Format;
                c.Visible = visible(c.Name, c.Visible);
            }
        }

        public static DataTable FormatChargeStateTable(List<DisplayChargeState> display, string table_name)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayExperimentalProteoform).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name), visible(x.Name, true)));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        #endregion Public Methods

        #region Private Methods

        private static string header(string property_name)
        {
            if (property_name == nameof(intensity)) { return "Intensity"; }
            if (property_name == nameof(mz_centroid)) { return "Centroid m/z"; }
            if (property_name == nameof(calculated_mass)) { return "Calculated Mass"; }
            if (property_name == nameof(charge_count)) { return "Charge Count"; }
            return null;
        }

        private static bool visible(string property_name, bool current)
        {
            return current;
        }

        private static string number_format(string property_name)
        {
            if (property_name == nameof(intensity)) { return "0.0000"; }
            if (property_name == nameof(mz_centroid)) { return "0.0000"; }
            if (property_name == nameof(calculated_mass)) { return "0.0000"; }
            return null;
        }

        #endregion Private Methods
    }
}