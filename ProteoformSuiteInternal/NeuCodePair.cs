using System;
using System.Collections.Generic;

namespace ProteoformSuiteInternal
{
    [Serializable]
    public class NeuCodePair : Component
    {
        #region Public Properties

        public Component neuCodeLight { get; set; }
        public Component neuCodeHeavy { get; set; }
        public List<int> overlapping_charge_states { get; set; }

        public string id_light { get; set; }
        public string id_heavy { get; set; }
        public double intensity_ratio { get; set; }
        public int lysine_count { get; set; }

        #endregion Public Properties

        #region Public Constructors

        public NeuCodePair(Component neuCodeLight, Component neuCodeHeavy, double mass_difference, List<int> overlapping_charge_states, bool light_is_lower) : base(neuCodeLight)
        {
            this.overlapping_charge_states = overlapping_charge_states;
            this.neuCodeLight = neuCodeLight;
            this.neuCodeHeavy = neuCodeHeavy;
            this.id_light = neuCodeLight.id;
            this.id_heavy = neuCodeHeavy.id;

            int diff_integer = Convert.ToInt32(Math.Round(mass_difference / Lollipop.MONOISOTOPIC_UNIT_MASS - 0.5, 0, MidpointRounding.AwayFromZero));
            double firstCorrection;

            firstCorrection = light_is_lower ?
                neuCodeLight.weighted_monoisotopic_mass + diff_integer * Lollipop.MONOISOTOPIC_UNIT_MASS :
                firstCorrection = neuCodeLight.weighted_monoisotopic_mass - (diff_integer + 1) * Lollipop.MONOISOTOPIC_UNIT_MASS;

            this.lysine_count = Math.Abs(Convert.ToInt32(Math.Round((neuCodeHeavy.weighted_monoisotopic_mass - firstCorrection) / Lollipop.NEUCODE_LYSINE_MASS_SHIFT, 0, MidpointRounding.AwayFromZero)));
            this.intensity_ratio = neuCodeLight.intensity_sum_olcs / neuCodeHeavy.intensity_sum_olcs; //ratio of overlapping charge states
            this.neuCodeCorrection = Math.Round((this.lysine_count * 0.1667 - 0.4), 0, MidpointRounding.AwayFromZero) * Lollipop.MONOISOTOPIC_UNIT_MASS;

            //marking pair as accepted or not when it's created
            set_accepted();
            this.calculate_properties();
        }

        public NeuCodePair(Component neucodeLight, Component neucodeHeavy) : base(neucodeLight) //need this to open and read in tsv files
        {
            this.neuCodeLight = neucodeLight;
            this.id_light = neuCodeLight.id;
            this.neuCodeHeavy = neucodeHeavy;
            this.id_heavy = neucodeHeavy.id;
        }

        public NeuCodePair()
        { }

        #endregion Public Constructors

        #region Public Methods

        public void set_accepted()
        {
            accepted = lysine_count >= SaveState.lollipop.min_lysine_ct && lysine_count <= SaveState.lollipop.max_lysine_ct
                && intensity_ratio >= Convert.ToDouble(SaveState.lollipop.min_intensity_ratio) && intensity_ratio <= Convert.ToDouble(SaveState.lollipop.max_intensity_ratio);
        }

        #endregion Public Methods

    }
}
