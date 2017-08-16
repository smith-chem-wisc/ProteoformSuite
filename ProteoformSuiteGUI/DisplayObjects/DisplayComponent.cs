﻿using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class DisplayComponent : DisplayObject
    {

        #region Public Constructors

        public DisplayComponent(Component c)
            : base(c)
        {
            this.c = c;
        }

        #endregion

        #region Private Fields

        private Component c;

        #endregion

        #region Public Properties

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

        public string scan_range
        {
            get { return c.scan_range; }
        }

        public string component_id
        {
            get { return c.id; }
        }

        public bool Accepted
        {
            get { return c.accepted; }
        }

        public double weighted_monoisotopic_mass
        {
            get { return c.weighted_monoisotopic_mass; }
        }

        public int num_charge_states
        {
            get { return c.charge_states.Count; }
        }

        public double intensity_sum
        {
            get { return c.intensity_sum; }
        }

        public string rt_range
        {
            get { return c.rt_range; }
        }

        public double rt_apex
        {
            get { return c.rt_apex; }
        }

        public double manual_mass_shift
        {
            get { return c.manual_mass_shift; }
        }


        // Pulled from the Thermo Deconvolution Results. Not used elsewhere.
        public double reported_monoisotopic_mass
        {
            get { return c.reported_monoisotopic_mass; }
        }

        public double intensity_reported
        {
            get { return c.intensity_reported; }
        }

        public double relative_abundance
        {
            get { return c.relative_abundance; }
        }

        public double fract_abundance
        {
            get { return c.fract_abundance; }
        }

        public double reported_delta_mass
        {
            get { return c.reported_delta_mass; }
        }

        public int num_detected_intervals
        {
            get { return c.num_detected_intervals; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatComponentsTable(DataGridView dgv)
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

        public static DataTable FormatComponentsTable(List<DisplayComponent> display, string table_name)
        {
            IEnumerable<Tuple<PropertyInfo, string, bool>> property_stuff = typeof(DisplayComponent).GetProperties().Select(x => new Tuple<PropertyInfo, string, bool>(x, header(x.Name), visible(x.Name, true)));
            return DisplayUtility.FormatTable(display.OfType<DisplayObject>().ToList(), property_stuff, table_name);
        }

        #endregion

        #region Private Methods

        private static bool visible(string property_name, bool current)
        {
            if (property_name == nameof(manual_mass_shift)) return false;
            return current;
        }

        private static string header(string property_name)
        {
            if (property_name == nameof(weighted_monoisotopic_mass)) return "Weighted Monoisotopic Mass";
            if (property_name == nameof(rt_apex)) return "Apex RT";
            if (property_name == nameof(intensity_sum)) return "Intensity Sum";
            if (property_name == nameof(input_file_filename)) return "Input Filename";
            if (property_name == nameof(input_file_purpose)) return "Input File Purpose";
            if (property_name == nameof(input_file_uniqueId)) return "Input File Unique ID";
            if (property_name == nameof(component_id)) return "Component ID";
            if (property_name == nameof(scan_range)) return "Scan Range";
            if (property_name == nameof(rt_range)) return "RT Range";
            if (property_name == nameof(num_charge_states)) return "No. Charge States";
            if (property_name == nameof(manual_mass_shift)) return "Manual Mass Shift";
            if (property_name == nameof(reported_monoisotopic_mass)) return "Monoisotopic Mass (from Thermo Decon.)";
            if (property_name == nameof(intensity_reported)) return "Intensity (from Thermo Decon.)";
            if (property_name == nameof(num_detected_intervals)) return "No. Detected Intervals (from Thermo Decon.)";
            if (property_name == nameof(reported_delta_mass)) return "Reported Delta Mass (from Thermo Decon.)";
            if (property_name == nameof(relative_abundance)) return "Relative Abundance (from Thermo Decon.)";
            if (property_name == nameof(fract_abundance)) return "Fractional Abundance (from Thermo Decon.)";
            return null;
        }

        private static string number_format(string property_name)
        {
            if (property_name == nameof(reported_monoisotopic_mass)) return "0.0000";
            if (property_name == nameof(reported_delta_mass)) return "0.0000";
            if (property_name == nameof(weighted_monoisotopic_mass)) return "0.0000";
            if (property_name == nameof(rt_apex)) return "0.00";
            if (property_name == nameof(relative_abundance)) return "0.0000";
            if (property_name == nameof(fract_abundance)) return "0.0000";
            if (property_name == nameof(intensity_sum)) return "0.0000";
            if (property_name == nameof(intensity_reported)) return "0.0000";
            if (property_name == nameof(manual_mass_shift)) return "0.0000";
            return null;
        }

        #endregion Private Methods

    }
}
