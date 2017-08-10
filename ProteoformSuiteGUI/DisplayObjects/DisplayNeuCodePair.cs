using ProteoformSuiteInternal;
using System;
using System.Linq;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class DisplayNeuCodePair
    {

        #region Public Constructors

        public DisplayNeuCodePair(NeuCodePair c)
        {
            this.c = c;
        }

        #endregion

        #region Private Fields

        private NeuCodePair c;

        #endregion

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
            get { return String.Join(",", c.overlapping_charge_states.Select(x => x.ToString())); }
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
            get { return c.scan_range; }
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

        public static void FormatNeuCodeTable(DataGridView dgv)
        {
            if (dgv.Columns.Count <= 0) return;

            dgv.AllowUserToAddRows = false;
            dgv.ReadOnly = true;

            //round table values
            dgv.Columns[nameof(intensity_ratio)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(intensity_sum)].DefaultCellStyle.Format = "0.####";

            //Headers
            dgv.Columns[nameof(id_light)].HeaderText = "Light NeuCode Component ID";
            dgv.Columns[nameof(id_heavy)].HeaderText = "Heavy NeuCode Component ID";
            dgv.Columns[nameof(intensity_ratio)].HeaderText = "Intensity Ratio";
            dgv.Columns[nameof(intensity_sum)].HeaderText = "Intensity Sum Overlapping Charge States";
            dgv.Columns[nameof(lysine_count)].HeaderText = "Lysine Count";
            dgv.Columns[nameof(input_file_filename)].HeaderText = "Input Filename";
            dgv.Columns[nameof(input_file_purpose)].HeaderText = "Input File Purpose";
            dgv.Columns[nameof(input_file_uniqueId)].HeaderText = "Input File Unique ID";
            dgv.Columns[nameof(scan_range)].HeaderText = "Scan Range";
            dgv.Columns[nameof(mass)].HeaderText = "Corrected NeuCode Light Weighted Monoisotopic Mass";
            dgv.Columns[nameof(mass_light)].HeaderText = "Light Weighted Monoisotopic Mass";
            dgv.Columns[nameof(mass_heavy)].HeaderText = "Heavy Weighted Monoisotopic Mass";
            dgv.Columns[nameof(rt_apex)].HeaderText = "Light Apex RT";
            dgv.Columns[nameof(rt_apex_heavy)].HeaderText = "Heavy Apex RT";
        }

        #endregion

    }
}
