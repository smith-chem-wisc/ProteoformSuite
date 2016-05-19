using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace PS_0._00
{
    public class Component
    {
        public string file_origin { get; set; }
        public int id { get; set; }
        public double monoisotopic_mass { get; set; }
        public double intensity_sum { get; set; }
        public List<ChargeState> charge_states { get; set; }
        public double delta_mass { get; set; }
        public double relative_abundance { get; set; }
        public double fract_abundance { get; set; }
        public int scan_start { get; set; }
        public int scan_end { get; set; }
        public double rt_start { get; set; }
        public double rt_end { get; set; }
        public double rt_apex { get; set; }
        public double weighted_monoisotopic_mass { get; set; }

        public Component(int id, double monoisotopic_mass, double sum_intensity, int num_charge_states,
                        int num_detected_intervals, double delta_mass, double relative_abundance, double fract_abundance, string scan_range,
                        string rt_range, double rt_apex, string filename)
        {
            this.id = id;
            this.monoisotopic_mass = monoisotopic_mass;
            this.intensity_sum = sum_intensity;
            this.delta_mass = delta_mass;
            this.relative_abundance = relative_abundance;
            this.fract_abundance = fract_abundance;
            this.file_origin = filename;
            this.charge_states = new List<ChargeState>();

            this.calculate_sum_intensity();
            this.calculate_weighted_monoisotopic_mass();
        }

        public int get_num_charge_states()
        {
            return this.charge_states.Count;
        }

        public void calculate_sum_intensity()
        {
            this.intensity_sum = 0;
            foreach (ChargeState cs in this.charge_states)
            {
                this.intensity_sum += cs.intensity;
            }
        }

        public void calculate_weighted_monoisotopic_mass()
        {
            this.weighted_monoisotopic_mass = 0;
            foreach (ChargeState cs in this.charge_states)
            {
                this.weighted_monoisotopic_mass += cs.intensity / this.intensity_sum * cs.calculated_mass;
            }
        }

        public void add_charge_state(int charge_state, double intensity, double mz_centroid, double calculated_mass)
        {
            charge_states.Add(new ChargeState(charge_state, intensity, mz_centroid, calculated_mass));
        }

        public bool is_neucode_pair(Component c, float minIntensity, float maxIntensity, float minLysineCount, float maxLysineCount)
        {
            return false;
        }

        //public DataRow get_component_datarow()
        //{

        //}

        public DataTable get_chargestate_datatable()
        {
            return new DataTable();
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

    public class Proteoform 
    {

        public DataTable getNeuCodeLightTable()
        {
            return new DataTable();
        } 
    }
}
