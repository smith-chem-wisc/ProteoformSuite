using ProteoformSuiteInternal;
using System.Windows.Forms;

namespace ProteoformSuiteGUI
{
    public class DisplayChargeState : DisplayObject
    {

        #region Public Constructors

        public DisplayChargeState(ChargeState c)
            : base(c)
        {
            this.c = c;
        }

        #endregion

        #region Private Fields

        private ChargeState c;

        #endregion

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

        public double signal_to_noise
        {
            get { return c.signal_to_noise; }
        }
        
        public int isotopic_peaks_left_averagine
        {
            get { return c.isotopic_peaks_left_averagine; }
        }

        public int isotopic_peaks_right_averagine
        {
            get { return c.isotopic_peaks_right_averagine; }
        }

        #endregion Public Properties

        #region Public Methods

        public static void FormatChargeStateTable(DataGridView dgv, bool quantitative)
        {
            if (dgv.Columns.Count <= 0) return;

            //round table values
            dgv.Columns[nameof(intensity)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(mz_centroid)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(calculated_mass)].DefaultCellStyle.Format = "0.####";
            dgv.Columns[nameof(signal_to_noise)].DefaultCellStyle.Format = "0.####";

            //set column header
            dgv.Columns[nameof(intensity)].HeaderText = "Intensity";
            dgv.Columns[nameof(mz_centroid)].HeaderText = "Centroid m/z";
            dgv.Columns[nameof(calculated_mass)].HeaderText = "Calculated Mass";
            dgv.Columns[nameof(charge_count)].HeaderText = "Charge Count";
            dgv.Columns[nameof(signal_to_noise)].HeaderText = "Signal to Noise";
            dgv.Columns[nameof(isotopic_peaks_left_averagine)].HeaderText = "Number Isotopic Peaks Left of Averagine Peak";
            dgv.Columns[nameof(isotopic_peaks_right_averagine)].HeaderText = "Number of Isotopic Peaks Right of Averagine Peak";

            dgv.AllowUserToAddRows = false;
        }

        #endregion

    }
}
