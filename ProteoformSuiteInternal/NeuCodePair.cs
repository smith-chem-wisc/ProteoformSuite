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
        public string scan_range { get; set; }

        public Component neuCodeLight { get; set; }
        public Component neuCodeHeavy { get; set; }
        public double intensity_ratio { get; set; }
        public double intensity_sum { get; set; } //intensity sum for overlapping charge states -> determined when grouped into neucode pairs.

        public List<ChargeState> charge_states { get; set; } // light charge states that had charge counts also observed for the heavy component
        public int lysine_count { get; set; }

        public double weighted_monoisotopic_mass { get; set; }

        public double rt_apex { get; set; }
        public bool accepted { get; set; }

        #endregion Public Properties

        #region Public Constructors

        public NeuCodePair(Component neuCodeLight, double light_intensity_sum_olcs, Component neuCodeHeavy, double heavy_intensity_sum_olcs, double mass_difference, HashSet<int> overlapping_charge_states, bool light_is_lower)
        {
            this.scan_range = neuCodeLight.scan_range;
            this.rt_apex = neuCodeLight.rt_apex;
            this.input_file = neuCodeLight.input_file;
            this.intensity_sum = light_intensity_sum_olcs;
            this.charge_states = neuCodeLight.charge_states.Where(x => overlapping_charge_states.Contains(x.charge_count)).ToList();

            this.neuCodeLight = neuCodeLight;
            this.neuCodeHeavy = neuCodeHeavy;

            int diff_integer = Convert.ToInt32(Math.Round(mass_difference / Lollipop.MONOISOTOPIC_UNIT_MASS - 0.5, 0, MidpointRounding.AwayFromZero));
            double firstCorrection = light_is_lower ?
                neuCodeLight.weighted_monoisotopic_mass + diff_integer * Lollipop.MONOISOTOPIC_UNIT_MASS :
                neuCodeLight.weighted_monoisotopic_mass - (diff_integer + 1) * Lollipop.MONOISOTOPIC_UNIT_MASS;

            this.lysine_count = Math.Abs(Convert.ToInt32(Math.Round((neuCodeHeavy.weighted_monoisotopic_mass - firstCorrection) / Lollipop.NEUCODE_LYSINE_MASS_SHIFT, 0, MidpointRounding.AwayFromZero)));
            this.intensity_ratio = light_intensity_sum_olcs / heavy_intensity_sum_olcs; //ratio of overlapping charge states
            double neuCodeCorrection = Math.Round((this.lysine_count * 0.1667 - 0.4), 0, MidpointRounding.AwayFromZero) * Lollipop.MONOISOTOPIC_UNIT_MASS;
            this.weighted_monoisotopic_mass = neuCodeLight.weighted_monoisotopic_mass + neuCodeCorrection;

            //marking pair as accepted or not when it's created
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
            accepted = lysine_count >= Sweet.lollipop.min_lysine_ct && lysine_count <= Sweet.lollipop.max_lysine_ct
                && intensity_ratio >= Convert.ToDouble(Sweet.lollipop.min_intensity_ratio) && intensity_ratio <= Convert.ToDouble(Sweet.lollipop.max_intensity_ratio);
        }

        #endregion Public Methods
    }
}