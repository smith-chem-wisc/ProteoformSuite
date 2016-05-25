using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace PS_0._00
{
    public class Component
    {
        public string file_origin { get; set; }
        public int id { get; set; }
        public double monoisotopic_mass { get; set; }
        public double intensity_sum { get; set; }
        public double delta_mass { get; set; }
        public double relative_abundance { get; set; }
        public double fract_abundance { get; set; }
        public string scan_range { get; set; }
        public string rt_range { get; set; }
        public double rt_apex { get; set; }
        public double weighted_monoisotopic_mass { get; set; }
        public List<ChargeState> charge_states { get; set; } = new List<ChargeState>();
        public int num_charge_states
        {
            get { return this.charge_states.Count; }
        }
        private int num_detected_intervals { get; set; }
        private int num_charge_states_fromFile { get; set; }

        public Component(DataRow component_row)
        {
            this.id = component_row.Field<int>(0);
            this.monoisotopic_mass = component_row.Field<double>(1);
            this.intensity_sum = component_row.Field<double>(2);
            this.num_charge_states_fromFile = component_row.Field<int>(3);
            this.num_detected_intervals = component_row.Field<int>(4);
            this.delta_mass = component_row.Field<double>(5);
            this.relative_abundance = component_row.Field<double>(6);
            this.fract_abundance = component_row.Field<double>(7);
            this.scan_range = component_row.Field<string>(8);
            this.rt_range = component_row.Field<string>(9);
            this.rt_apex = component_row.Field<double>(10);
            this.file_origin = component_row.Field<string>(11);
        }

        public double calculate_sum_intensity()
        {
            this.intensity_sum = this.charge_states.Select(charge_state => charge_state.intensity).Sum();
            return this.intensity_sum;
        }

        public double calculate_sum_intensity(List<int> charges_to_sum)
        {
            return this.charge_states.Where(cs => charges_to_sum.Contains(cs.charge_count)).Select(charge_state => charge_state.intensity).Sum();
        }

        public void calculate_weighted_monoisotopic_mass()
        {
            this.weighted_monoisotopic_mass = this.charge_states.Select(charge_state => charge_state.intensity / this.intensity_sum * charge_state.calculated_mass).Sum();
        }

        public void add_charge_state(DataRow charge_row)
        {
            int charge_state = charge_row.Field<int>(1);
            double intensity = charge_row.Field<double>(2);
            double mz_centroid = charge_row.Field<double>(3);
            double calculated_mass = charge_row.Field<double>(4);
            charge_states.Add(new ChargeState(charge_state, intensity, mz_centroid, calculated_mass));
        }
    }

    public class ChargeState
    {
        public int charge_count { get; set; }
        public double intensity { get; set; }
        public double mz_centroid { get; set; }
        public double calculated_mass { get; set; }

        public ChargeState(int chage_count, double intensity, double mz_centroid, double calculated_mass)
        {
            this.charge_count = charge_count;
            this.intensity = intensity;
            this.mz_centroid = mz_centroid;
            this.calculated_mass = calculated_mass;
        }
    }
}
