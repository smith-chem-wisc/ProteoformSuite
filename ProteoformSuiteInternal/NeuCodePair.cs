using Chemistry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class NeuCodePair
        : IFileIntensity, IAggregatable
    {
        #region Public Properties

        public InputFile input_file { get; set; }
        public int min_scan { get; set; }
        public int max_scan { get; set; }
        public Component neuCodeLight { get; set; }
        public Component neuCodeHeavy { get; set; }
        public double intensity_ratio { get; set; }
        public double intensity_sum { get; set; } //intensity sum for overlapping charge states -> determined when grouped into neucode pairs.
        public List<ChargeState> charge_states { get; set; } // light charge states that had charge counts also observed for the heavy component
        public List<int> overlappingChargeStates { get; set; }
        public int lysine_count { get; set; }
        public int cysteine_count { get; set; }
        public double exact_cysteine_count { get; set; }
        public List<Component> mislabeled_components { get; set; }
        public double weighted_monoisotopic_mass { get; set; }

        public double rt_apex { get; set; }
        public bool accepted { get; set; }
        public bool mislabeled { get; set; }

        #endregion Public Properties

        #region Public Constructors

        public NeuCodePair(Component neuCodeLight, double light_intensity_sum_olcs, Component neuCodeHeavy, double heavy_intensity_sum_olcs, double mass_difference, 
            HashSet<int> overlapping_charge_states, bool light_is_lower, bool neuCodeLightIsMoreAbundant)
        {
            this.min_scan = neuCodeLight.min_scan;
            this.max_scan = neuCodeLight.max_scan;
            this.rt_apex = neuCodeLight.rt_apex;
            this.input_file = neuCodeLight.input_file;
            this.intensity_sum = light_intensity_sum_olcs;
            this.charge_states = neuCodeLight.charge_states.Where(x => overlapping_charge_states.Contains(x.charge_count)).ToList();
            overlappingChargeStates = new List<int>();
            foreach (ChargeState c in charge_states)
                overlappingChargeStates.Add(c.charge_count);
            mislabeled_components = new List<Component>();

            this.neuCodeLight = neuCodeLight;
            this.neuCodeHeavy = neuCodeHeavy;

            int diff_integer = Convert.ToInt32(Math.Round(mass_difference / Lollipop.MONOISOTOPIC_UNIT_MASS - 0.5, 0, MidpointRounding.AwayFromZero));
            double firstCorrection = light_is_lower ?
                neuCodeLight.weighted_monoisotopic_mass + diff_integer * Lollipop.MONOISOTOPIC_UNIT_MASS :
                neuCodeLight.weighted_monoisotopic_mass - (diff_integer + 1) * Lollipop.MONOISOTOPIC_UNIT_MASS;

            if(Sweet.lollipop.neucode_labeled)
            {
                this.lysine_count = Math.Abs(Convert.ToInt32(Math.Round((neuCodeHeavy.weighted_monoisotopic_mass - firstCorrection) / Lollipop.NEUCODE_LYSINE_MASS_SHIFT, 0, MidpointRounding.AwayFromZero)));

                /* Well, the reason for the correction is b/c determining an MI mass makes use of the averagine model.   When dealing with isotopically heavy pfms, however, 
                 * part of the mass is not due to the usual number of C,H,N,O,S atoms but is simply extra neutron mass.    
                 * So, then averagine is more likely to get the MI mass wrong by -1 or -2 or -3 units.  And the more heavy lysines you have the more likely it will miss the MI by more units.  
                 * Hence we do this correction that adds 0, 1, 2, or 3 to correct for the -1, -2, -3 that it will “likely” miss by.
                 */

                double neuCodeCorrection = Math.Round((this.lysine_count * 0.1667 - 0.4), 0, MidpointRounding.AwayFromZero) * Lollipop.MONOISOTOPIC_UNIT_MASS;
                this.weighted_monoisotopic_mass = neuCodeLight.weighted_monoisotopic_mass + neuCodeCorrection;
            }
            //Some empirical corrections for deconvolution issues will need to be made here.. @JGP
            else if(Sweet.lollipop.cystag_labeled)
            {
                this.exact_cysteine_count = Math.Abs(neuCodeHeavy.weighted_monoisotopic_mass - firstCorrection) / Lollipop.CYSTAG_MASS_SHIFT;
                this.cysteine_count = Math.Abs(Convert.ToInt32(Math.Round((neuCodeHeavy.weighted_monoisotopic_mass - firstCorrection) / Lollipop.CYSTAG_MASS_SHIFT, 0, MidpointRounding.AwayFromZero)));

                double neuCodeCorrection = Math.Round((this.cysteine_count * 0.2 - 0.4), 0, MidpointRounding.AwayFromZero) * Lollipop.MONOISOTOPIC_UNIT_MASS;
                this.weighted_monoisotopic_mass = neuCodeLight.weighted_monoisotopic_mass + neuCodeCorrection;
                double adjustedMono = weighted_monoisotopic_mass - ((weighted_monoisotopic_mass / 1000000) * 5);
                this.weighted_monoisotopic_mass = adjustedMono;
            }
            this.intensity_ratio = neuCodeLightIsMoreAbundant ? light_intensity_sum_olcs / heavy_intensity_sum_olcs : heavy_intensity_sum_olcs/light_intensity_sum_olcs; //ratio of overlapping charge states
            

            //marking pair as accepted or not when it's created
            set_accepted();
        }

        //Used for reading NeuCode Pairs in directly.
        public NeuCodePair(InputFile inputFile, Component neuCodeLight, Component neuCodeHeavy, double weightedMonoisotopic, int cysteineCount, int lysineCount, double intensityRatio, double apexRt, int minScan, int maxScan, double sumIntensityOlcs,
            int lightId, int heavyId, string overlappingChargeStates)
        {
            this.input_file = inputFile;
            this.neuCodeLight = neuCodeLight;
            this.neuCodeHeavy = neuCodeHeavy;
            

            if (cysteineCount != -1)
                this.cysteine_count = cysteineCount;
            if(lysineCount != -1)
                this.lysine_count = lysineCount;

            //Performing manual calibration here -JGP
            double ppmCalibration = -5;
            double massCalibration = (ppmCalibration / 1000000) * neuCodeLight.weighted_monoisotopic_mass;
            double neuCodeCorrection = Math.Round((this.cysteine_count * 0.1667 - 0.4), 0, MidpointRounding.AwayFromZero) * Lollipop.MONOISOTOPIC_UNIT_MASS;

            this.weighted_monoisotopic_mass = neuCodeLight.weighted_monoisotopic_mass + neuCodeCorrection + massCalibration;

            this.intensity_ratio = intensityRatio;
            this.rt_apex = apexRt;
            this.min_scan = minScan;
            this.max_scan = maxScan;
            this.intensity_sum = sumIntensityOlcs;
            charge_states = new List<ChargeState>();

            this.overlappingChargeStates = new List<int>();
            string[] chargeStates = overlappingChargeStates.Split(',');
            foreach (string charge in chargeStates)
            {
                this.overlappingChargeStates.Add(Convert.ToInt32(charge));
                ChargeState c = new ChargeState(Convert.ToInt32(charge), 100, this.weighted_monoisotopic_mass.ToMz(Convert.ToInt32(charge)));
                charge_states.Add(c);
            }
            mislabeled_components = new List<Component>();
            mislabeled = false;
            set_accepted();
        }

        public NeuCodePair()
        { }

        #endregion Public Constructors

        #region Public Methods

        public static double calculate_sum_intensity_olcs(List<ChargeState> light_or_heavy_charge_states, HashSet<int> charges_to_sum)
        {
            return light_or_heavy_charge_states.Where(cs => charges_to_sum.Contains(cs.charge_count)).Sum(charge_state => charge_state.intensity);
        }

        public void set_accepted()
        {
            accepted = (((lysine_count >= Sweet.lollipop.min_lysine_ct && lysine_count <= Sweet.lollipop.max_lysine_ct) && Sweet.lollipop.neucode_labeled) || 
                ((cysteine_count >= Sweet.lollipop.min_cysteine_ct && cysteine_count <= Sweet.lollipop.max_cysteine_ct) && Sweet.lollipop.cystag_labeled)
                && intensity_ratio >= Convert.ToDouble(Sweet.lollipop.min_intensity_ratio) && intensity_ratio <= Convert.ToDouble(Sweet.lollipop.max_intensity_ratio));
        }

        #endregion Public Methods
    }
}