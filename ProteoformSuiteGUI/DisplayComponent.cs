using System;
using ProteoformSuiteInternal;
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
            get { return c.num_charge_states; }
        }

        public double intensity_sum
        {
            get { return c.intensity_sum; }
        }

        public double intensity_sum_olcs
        {
            get { return c.intensity_sum_olcs; }
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

        public double delta_mass
        {
            get { return c.delta_mass; }
        }

        public int num_detected_intervals
        {
            get { return c.num_detected_intervals; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatComponentsTable(DataGridView dgv, bool quantitative)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;

            //round table values
            dgv.Columns[nameof(reported_monoisotopic_mass)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(delta_mass)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(weighted_monoisotopic_mass)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(rt_apex)].DefaultCellStyle.Format = "0.##";
            dgv.Columns[nameof(relative_abundance)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(fract_abundance)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(intensity_sum)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(intensity_reported)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(intensity_sum_olcs)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(manual_mass_shift)].DefaultCellStyle.Format = "0.####";

            //Headers
            dgv.Columns[nameof(weighted_monoisotopic_mass)].HeaderText = "Weighted Monoisotopic Mass";
            dgv.Columns[nameof(rt_apex)].HeaderText = "Apex RT";
            dgv.Columns[nameof(intensity_sum)].HeaderText = "Intensity Sum";
            dgv.Columns[nameof(intensity_sum_olcs)].HeaderText = "Intensity Sum Overlapping Charge States";
            dgv.Columns[nameof(input_file_filename)].HeaderText = "Input Filename";
            dgv.Columns[nameof(input_file_purpose)].HeaderText = "Input File Purpose";
            dgv.Columns[nameof(input_file_uniqueId)].HeaderText = "Input File Unique ID";
            dgv.Columns[nameof(component_id)].HeaderText = "Component ID";
            dgv.Columns[nameof(scan_range)].HeaderText = "Scan Range";
            dgv.Columns[nameof(rt_range)].HeaderText = "RT Range";
            dgv.Columns[nameof(num_charge_states)].HeaderText = "No. Charge States";
            dgv.Columns[nameof(manual_mass_shift)].HeaderText = "Manual Mass Shift";
            dgv.Columns[nameof(reported_monoisotopic_mass)].HeaderText = "Monoisotopic Mass (from Thermo Decon.)";
            dgv.Columns[nameof(intensity_reported)].HeaderText = "Intensity (from Thermo Decon.)";
            dgv.Columns[nameof(num_detected_intervals)].HeaderText = "No. Detected Intervals (from Thermo Decon.)";
            dgv.Columns[nameof(delta_mass)].HeaderText = "Delta Mass (from Thermo Decon.)";
            dgv.Columns[nameof(relative_abundance)].HeaderText = "Relative Abundance (from Thermo Decon.)";
            dgv.Columns[nameof(fract_abundance)].HeaderText = "Fractional Abundance (from Thermo Decon.)";

            //Visibility
            dgv.Columns[nameof(intensity_sum_olcs)].Visible = Lollipop.neucode_labeled;
            dgv.Columns[nameof(manual_mass_shift)].Visible = false;
        }

        #endregion
    }
}
