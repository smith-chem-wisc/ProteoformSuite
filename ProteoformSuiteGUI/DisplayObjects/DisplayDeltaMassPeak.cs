using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class DisplayDeltaMassPeak : DisplayObject
    {
        #region Public Constructor

        public DisplayDeltaMassPeak(DeltaMassPeak peak)
            : base(peak)
        { }

        #endregion Public Constructor

        #region Public Properties

        public double DeltaMass
        {
            get { return (display_object as DeltaMassPeak).DeltaMass; }
        }

        public int PeakRelationGroupCount
        {
            get { return (display_object as DeltaMassPeak).peak_relation_group_count; }
        }

        public bool Accepted
        {
            get { return (display_object as DeltaMassPeak).Accepted; }
            set { Sweet.lollipop.change_peak_acceptance(display_object as DeltaMassPeak, value, true); }
        }

        public double DecoyRelationCount
        {
            get { return (display_object as DeltaMassPeak).decoy_relation_count; }
        }

        public double PeakGroupFDR
        {
            get { return (display_object as DeltaMassPeak).peak_group_fdr; }
        }

        public string possiblePeakAssignments_string
        {
            get { return (display_object as DeltaMassPeak).possiblePeakAssignments_string; }
        }

        public string MassShifter
        {
            get
            {
                return (display_object as DeltaMassPeak).mass_shifter;
            }
            set
            {
                (display_object as DeltaMassPeak).mass_shifter = value;
                Sweet.shift_peak_action(display_object as DeltaMassPeak);
            }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatPeakListGridView(DataGridView dgv, bool mask_mass_shifter)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;

            //making all columns invisible first - faster
            foreach (DataGridViewColumn column in dgv.Columns) { column.Visible = false; }
            if (!mask_mass_shifter)
            {
                dgv.Columns[nameof(MassShifter)].ReadOnly = false; //user can say how much they want to change monoisotopic by for each
            }

            foreach (DataGridViewColumn c in dgv.Columns)
            {
                string h = header(c.Name, mask_mass_shifter);
                string n = number_format(c.Name);
                c.HeaderText = h != null ? h : c.HeaderText;
                c.DefaultCellStyle.Format = n != null ? n : c.DefaultCellStyle.Format;
                c.Visible = visible(c.Name, c.Visible, mask_mass_shifter);
            }
        }

        public static DataTable FormatPeakListGridView(List<DisplayDeltaMassPeak> display, string table_name, bool mask_mass_shifter)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayDeltaMassPeak).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name, mask_mass_shifter), visible(x.Name, true, mask_mass_shifter)));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        #endregion Public Methods

        #region Private Methods

        private static bool visible(string property_name, bool current, bool mask_mass_shifter)
        {
            if (property_name == nameof(PeakRelationGroupCount)) { return true; }
            if (property_name == nameof(DeltaMass)) { return true; }
            if (property_name == nameof(PeakGroupFDR)) { return true; }
            if (property_name == nameof(Accepted)) { return true; }
            if (property_name == nameof(possiblePeakAssignments_string)) { return true; }
            if (!mask_mass_shifter && property_name == nameof(MassShifter)) { return true; }
            return current;
        }

        private static string header(string property_name, bool mask_mass_shifter)
        {
            if (property_name == nameof(PeakRelationGroupCount)) { return "Peak Center Count"; }
            if (property_name == nameof(DecoyRelationCount)) { return "Decoy Count under Peak"; }
            if (property_name == nameof(DeltaMass)) { return "Peak Center Delta Mass"; }
            if (property_name == nameof(PeakGroupFDR)) { return "Peak FDR"; }
            if (property_name == nameof(Accepted)) { return "Peak Accepted"; }
            if (property_name == nameof(possiblePeakAssignments_string)) { return "Peak Assignment Possibilites"; }
            if (!mask_mass_shifter && property_name == nameof(MassShifter)) { return "Mass Shifter"; }
            return null;
        }

        private static string number_format(string property_name)
        {
            if (property_name == nameof(DeltaMass)) { return "0.0000"; }
            if (property_name == nameof(PeakGroupFDR)) { return "0.00"; }
            return null;
        }

        #endregion Private Methods
    }
}