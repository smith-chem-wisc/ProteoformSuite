using System;
using System.Collections.Generic;

namespace ProteoformSuiteInternal
{
    [Serializable]
    public class ChargeState
    {
        #region Public Properties

        public int charge_count { get; set; } //value from deconv 4.0
        public double intensity { get; set; } //value from deconv 4.0
        public double mz_centroid { get; set; } //value from deconv 4.0
        public double calculated_mass { get; set; }  // the value reported by decon 4.0 is incorrect, so we calculate it from m/z and charge (including correction when necessary)

        #endregion Public Properties

        #region Public Constructors

        public ChargeState(List<string> charge_row, double mz_correction) //the correction used is determined from measurement of lock-mass compound. It is read in at the same time the data is read in. We do not keep track of the correction because it adds confusion when charge states are combined.
        {
            this.charge_count = Convert.ToInt32(charge_row[0]);
            this.intensity = Convert.ToDouble(charge_row[1]);
            this.mz_centroid = correct_calculated_mz(Convert.ToDouble(charge_row[2]), mz_correction); //no point to keeping the uncorrected mz if there is a correction.
            this.calculated_mass = correct_calculated_mass();
        }

        //For testing
        public ChargeState(int charge_count, double intensity, double mz_centroid, double mz_correction)
        {
            this.charge_count = charge_count;
            this.intensity = intensity;
            this.mz_centroid = correct_calculated_mz(mz_centroid, mz_correction);
            this.calculated_mass = correct_calculated_mass();
        }

        #endregion Public Constructors

        #region Public Methods

        public double correct_calculated_mz(double mz, double mz_correction) // the correction is a linear shift to m/z
        {
            return (mz + mz_correction);//Thermo deconvolution 4.0 miscalculates the monoisotopic mass from the reported mz and charge state values.
        }

        public double correct_calculated_mass() // the correction is a linear shift to m/z
        {
            return (this.charge_count * this.mz_centroid - this.charge_count * 1.00727645D);//Thermo deconvolution 4.0 miscalculates the monoisotopic mass from the reported mz and charge state values.
        }

        public ChargeState mergeTheseChargeStates(ChargeState csToMerge)
        {
            if (csToMerge != null)
            {
                double totalIntensity = this.intensity + csToMerge.intensity;
                this.calculated_mass = (this.intensity * this.calculated_mass + csToMerge.intensity * csToMerge.calculated_mass) / totalIntensity;
                this.mz_centroid = (this.calculated_mass + 1.00727645D * this.charge_count) / this.charge_count;
                this.intensity = totalIntensity;
            }
            return this;
        }

        public override string ToString()
        {
            return String.Join("\t", new List<string> { charge_count.ToString(), intensity.ToString() });
        }
        #endregion Public Methods
    }
}
