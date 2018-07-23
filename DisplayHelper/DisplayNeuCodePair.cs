using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace ProteoWPFSuite
{
    public class DisplayNeuCodePair : DisplayObject
    {
        #region Public Constructors

        public DisplayNeuCodePair(NeuCodePair c)
            : base(c)
        {
            this.c = c;
        }

        #endregion Public Constructors

        #region Private Fields

        private readonly NeuCodePair c;

        #endregion Private Fields

        #region Public Properties

        public string id_light
        {
            get { return c.neuCodeLight.id; }
        }

        public string id_heavy
        {
            get { return c.neuCodeHeavy.id; }
        }

        public string overlapping_charge_states
        {
            get { return String.Join(",", c.charge_states.Select(x => x.charge_count.ToString())); }
        }

        public double intensity_ratio
        {
            get { return c.intensity_ratio; }
        }

        public double intensity_sum
        {
            get { return c.intensity_sum; }
        }

        public int lysine_count
        {
            get { return c.lysine_count; }
        }

        public double mass
        {
            get { return c.weighted_monoisotopic_mass; }
        }

        public double mass_light
        {
            get { return c.neuCodeLight.weighted_monoisotopic_mass; }
        }

        public double mass_heavy
        {
            get { return c.neuCodeHeavy.weighted_monoisotopic_mass; }
        }

        public double rt_apex
        {
            get { return c.rt_apex; }
        }

        public double rt_apex_heavy
        {
            get { return c.neuCodeHeavy.rt_apex; }
        }

        public string scan_range
        {
            get { return c.min_scan + "-" + c.max_scan; }
        }

        public string input_file_uniqueId
        {
            get { return c.input_file.UniqueId.ToString(); }
        }

        public string input_file_filename
        {
            get { return c.input_file.filename; }
        }

        public string input_file_purpose
        {
            get { return c.input_file.purpose.ToString(); }
        }

        public bool Accepted
        {
            get { return c.accepted; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatNeuCodeTable(System.Windows.Forms.DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0)
            {
                return;
            }

            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;

            foreach (System.Windows.Forms.DataGridViewColumn col in dgv.Columns)
            {
                string h = header(col.Name);
                string n = number_format(col.Name);
                col.HeaderText = h != null ? h : col.HeaderText;
                col.DefaultCellStyle.Format = n != null ? n : col.DefaultCellStyle.Format;
                col.Visible = visible(col.Name, col.Visible);
            }
        }

        public static DataTable FormatNeuCodeTable(List<DisplayNeuCodePair> display, string table_name)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayNeuCodePair).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name), visible(x.Name, true)));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        #endregion Public Methods

        #region Private Methods

        private static string header(string property_name)
        {
            if (property_name == nameof(id_light)) { return "Light NeuCode Component ID"; }
            if (property_name == nameof(id_heavy)) { return "Heavy NeuCode Component ID"; }
            if (property_name == nameof(intensity_ratio)) { return "Intensity Ratio"; }
            if (property_name == nameof(intensity_sum)) { return "Intensity Sum Overlapping Charge States"; }
            if (property_name == nameof(lysine_count)) { return "Lysine Count"; }
            if (property_name == nameof(input_file_filename)) { return "Input Filename"; }
            if (property_name == nameof(input_file_purpose)) { return "Input File Purpose"; }
            if (property_name == nameof(input_file_uniqueId)) { return "Input File Unique ID"; }
            if (property_name == nameof(scan_range)) { return "Scan Range"; }
            if (property_name == nameof(mass)) { return "Corrected NeuCode Light Weighted Monoisotopic Mass"; }
            if (property_name == nameof(mass_light)) { return "Light Weighted Monoisotopic Mass"; }
            if (property_name == nameof(mass_heavy)) { return "Heavy Weighted Monoisotopic Mass"; }
            if (property_name == nameof(rt_apex)) { return "Light Apex RT"; }
            if (property_name == nameof(rt_apex_heavy)) { return "Heavy Apex RT"; }
            return null;
        }

        private static bool visible(string property_name, bool current)
        {
            return current;
        }

        private static string number_format(string property_name)
        {
            if (property_name == nameof(intensity_ratio)) { return "0.####"; }
            if (property_name == nameof(intensity_sum)) { return "0.####"; }
            return null;
        }

        #endregion Private Methods
    }
}
