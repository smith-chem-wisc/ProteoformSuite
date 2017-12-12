using System;
using System.Collections.Generic;

namespace ProteoformSuiteInternal
{
    public class ChargeState
    {

        #region Public Properties

        public int charge_count { get; set; } //value from deconv 4.0
        public double intensity { get; set; } //value from deconv 4.0
        public double mz_centroid { get; set; } //value from deconv 4.0
        public double calculated_mass { get; set; }  // the value reported by decon 4.0 is incorrect, so we calculate it from m/z and charge (including correction when necessary)
        public double reported_mass { get; set; } //value from decon 4.0
        #endregion Public Properties

        #region Public Constructors

        public ChargeState(List<string> charge_row)
        {
            this.charge_count = Convert.ToInt32(charge_row[0]);
            this.intensity = Convert.ToDouble(charge_row[1]) / this.charge_count; //charge state normalized
            this.mz_centroid = Convert.ToDouble(charge_row[2]);
            this.reported_mass = Convert.ToDouble(charge_row[3]);
            this.calculated_mass = correct_calculated_mass();
        }

        //For testing
        public ChargeState(int charge_count, double intensity, double mz_centroid)
        {
            this.charge_count = charge_count;
            this.intensity = intensity;
            this.mz_centroid = mz_centroid;
            this.calculated_mass = correct_calculated_mass();
        }

        #endregion Public Constructors

        #region Public Methods

        public double correct_calculated_mass() // the correction is a linear shift to m/z
        {
            return (charge_count * mz_centroid - charge_count * 1.00727645D);//Thermo deconvolution 4.0 miscalculates the monoisotopic mass from the reported mz and charge state values.
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
