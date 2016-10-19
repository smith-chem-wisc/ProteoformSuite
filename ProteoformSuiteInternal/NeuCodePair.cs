using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace ProteoformSuiteInternal
{
    public class NeuCodePair : Component
    {
        public Component neuCodeLight { get; set; }
        public Component neuCodeHeavy { get; set; }
        public List<int> overlapping_charge_states { get; set; }

        public string id_light { get; set; }
        public string id_heavy { get; set; }
        public double intensity_ratio { get; set; }
        public int lysine_count { get; set; }
        //public bool accepted { get; set; } //moved this to Component
       

        public NeuCodePair(Component neuCodeLight, Component neuCodeHeavy, double mass_difference, List<int> overlapping_charge_states, bool light_is_lower) : base(neuCodeLight)
        {
            this.overlapping_charge_states = overlapping_charge_states;
            this.neuCodeLight = neuCodeLight;
            this.neuCodeHeavy = neuCodeHeavy;
            this.id_light = neuCodeLight.id;
            this.id_heavy = neuCodeHeavy.id;

            int diff_integer = Convert.ToInt32(Math.Round(mass_difference / Lollipop.MONOISOTOPIC_UNIT_MASS - 0.5, 0, MidpointRounding.AwayFromZero)); 
            double firstCorrection;

            if (light_is_lower)
                firstCorrection = neuCodeLight.corrected_mass + diff_integer * Lollipop.MONOISOTOPIC_UNIT_MASS; 
            else
                firstCorrection = neuCodeLight.corrected_mass - (diff_integer + 1) * Lollipop.MONOISOTOPIC_UNIT_MASS; 

            this.lysine_count = Math.Abs(Convert.ToInt32(Math.Round((neuCodeHeavy.corrected_mass - firstCorrection) / Lollipop.NEUCODE_LYSINE_MASS_SHIFT, 0, MidpointRounding.AwayFromZero)));
            this.intensity_ratio = neuCodeLight.intensity_sum_olcs / neuCodeHeavy.intensity_sum_olcs; //ratio of overlapping charge states

            //marking pair as accepted or not when it's created
            set_accepted();

            this.corrected_mass = this.corrected_mass + Math.Round((this.lysine_count * 0.1667 - 0.4), 0, MidpointRounding.AwayFromZero) * Lollipop.MONOISOTOPIC_UNIT_MASS;
        }

        public void set_accepted()
        {
            if (this.lysine_count >= Lollipop.min_lysine_ct 
             && this.lysine_count <= Lollipop.max_lysine_ct
             && this.intensity_ratio >= Convert.ToDouble(Lollipop.min_intensity_ratio) 
             && this.intensity_ratio <= Convert.ToDouble(Lollipop.max_intensity_ratio))
                this.accepted = true; 
            else
                this.accepted = false;
        }

        public NeuCodePair(Component neucodeLight, Component neucodeHeavy) : base(neucodeLight) //need this to open and read in tsv files
        {
            this.neuCodeLight = neucodeLight;
            this.id_light = neuCodeLight.id;
            this.neuCodeHeavy = neucodeHeavy;
            this.id_heavy = neucodeHeavy.id;
        }
    }
}
