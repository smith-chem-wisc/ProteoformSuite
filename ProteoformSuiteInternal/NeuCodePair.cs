using System;
using System.Collections.Generic;

namespace ProteoformSuiteInternal
{
    public class NeuCodePair : Component
    {
        Component neuCodeLight;
        Component neuCodeHeavy;
        public List<int> overlapping_charge_states { get; set; }

        public int id_light { get; set; }
        public int id_heavy { get; set; }
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

            int diff_integer = Convert.ToInt32(Math.Round(mass_difference / 1.0015 - 0.5, 0, MidpointRounding.AwayFromZero)); 
            double firstCorrection;

            if (light_is_lower) { firstCorrection = neuCodeLight.weighted_monoisotopic_mass + diff_integer * 1.0015; }
            else  { firstCorrection = neuCodeLight.weighted_monoisotopic_mass - (diff_integer + 1) * 1.0015; }

            this.lysine_count = Math.Abs(Convert.ToInt32(Math.Round((neuCodeHeavy.weighted_monoisotopic_mass - firstCorrection) / 0.036015372, 0, MidpointRounding.AwayFromZero)));
            this.intensity_ratio = neuCodeLight.intensity_sum_olcs / neuCodeHeavy.intensity_sum_olcs; //ratio of overlapping charge states

            //marking pair as accepted or not when it's created
            if (this.lysine_count > Lollipop.min_lysine_ct && this.lysine_count < Lollipop.max_lysine_ct
                && this.intensity_ratio > Convert.ToDouble(Lollipop.min_intensity_ratio) && this.intensity_ratio < Convert.ToDouble(Lollipop.max_intensity_ratio))
                 { this.accepted = true; }
            else { this.accepted = false; }

            this.corrected_mass = this.weighted_monoisotopic_mass + Math.Round((this.lysine_count * 0.1667 - 0.4), 0, MidpointRounding.AwayFromZero) * 1.0015;
        }

        new public string as_tsv_row()
        {
            return String.Join("\t", new List<string> { this.id.ToString(), this.intensity_sum.ToString(), this.weighted_monoisotopic_mass.ToString(), this.corrected_mass.ToString(), this.rt_apex.ToString(),
                this.neuCodeHeavy.id.ToString(), this.neuCodeHeavy.intensity_sum.ToString(), this.neuCodeHeavy.weighted_monoisotopic_mass.ToString(), this.intensity_ratio.ToString(), this.lysine_count.ToString(),
                this.file_origin.ToString() });
        }

        new public static string get_tsv_header()
        {
            return String.Join("\t", new List<string> { "light_id", "light_intensity", "light_weighted_monoisotopic_mass", "light_corrected_mass", "light_apexRt",
                "heavy_id", "heavy_intensity", "heavy_weighted_monoisotopic_mass", "intensity_ratio", "lysine_count", "file_origin" });
        }
    }
}
