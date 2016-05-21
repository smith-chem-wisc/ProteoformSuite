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
        public double delta_mass { get; set; }
        public double relative_abundance { get; set; }
        public double fract_abundance { get; set; }
        public string scan_range { get; set; }
        public string rt_range { get; set; }
        public double rt_apex { get; set; }
        public double weighted_monoisotopic_mass { get; set; }
        private List<ChargeState> charge_states { get; set; } = new List<ChargeState>();
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

        public bool is_neucode_pair(Component c, float minIntensity, float maxIntensity, float minLysineCount, float maxLysineCount)
        {
            return false;
        }

        public List<ChargeState> get_chargestates()
        {
            return charge_states;
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

    public class NeuCodePair 
    {
        Component neuCodeLight;
        Component neuCodeHeavy;
        public string file_origin
        {
            get { return neuCodeLight.file_origin; }
        }
        public int light_id
        {
            get { return neuCodeLight.id; }
        }
        public double light_weighted_monoisotopic_mass
        {
            get { return neuCodeLight.weighted_monoisotopic_mass; }
        }
        public double light_corrected_mass { get; set; }
        public double light_intensity { get; set; }
        public double light_apexRt
        {
            get { return neuCodeLight.rt_apex; }
        }
        public int heavy_id
        {
            get { return neuCodeHeavy.id; }
        }
        public double heavy_weighted_monoisotopic_mass
        {
            get { return neuCodeHeavy.weighted_monoisotopic_mass; }
        }
        public double heavy_intensity { get; set; }
        public List<int> overlapping_charge_states { get; set; }
        
        public double intensity_ratio { get; set; }
        public int lysine_count { get; set; }
        public bool accepted { get; set; }

        public NeuCodePair(Component lower_rawNeuCode, Component higher_rawNeuCode)
        {
            double mass_difference = higher_rawNeuCode.weighted_monoisotopic_mass - lower_rawNeuCode.weighted_monoisotopic_mass; //changed from decimal; it doesn't seem like that should make a difference
            int diff_integer = Convert.ToInt32(Math.Round(mass_difference / 1.0015 - 0.5, 0, MidpointRounding.AwayFromZero));
            List<int> lower_charges = lower_rawNeuCode.get_chargestates().Select(charge_state => charge_state.charge_count).ToList<int>();
            List<int> higher_charges = higher_rawNeuCode.get_chargestates().Select(charge_states => charge_states.charge_count).ToList<int>();
            this.overlapping_charge_states = lower_charges.Intersect(higher_charges).ToList();
            double lower_intensity = lower_rawNeuCode.calculate_sum_intensity(this.overlapping_charge_states);
            double higher_intensity = lower_rawNeuCode.calculate_sum_intensity(this.overlapping_charge_states);

            if (lower_intensity <= 0 || higher_intensity <= 0)
            {
                this.accepted = false;
                return;
            }
            else
            {
                this.accepted = true;
                if (lower_intensity > higher_intensity) //lower mass is neucode light
                {
                    neuCodeLight = lower_rawNeuCode;
                    this.light_intensity = lower_intensity;
                    neuCodeHeavy = higher_rawNeuCode;
                    this.heavy_intensity = higher_intensity;
                }
                else //higher mass is neucode light
                {
                    neuCodeLight = higher_rawNeuCode;
                    this.light_intensity = higher_intensity;
                    neuCodeHeavy = lower_rawNeuCode;
                    this.heavy_intensity = lower_intensity;
                }

                double firstCorrection = neuCodeLight.weighted_monoisotopic_mass + diff_integer * 1.0015;
                this.lysine_count = Math.Abs(Convert.ToInt32(Math.Round((neuCodeHeavy.weighted_monoisotopic_mass - firstCorrection) / 0.036015372, 0, MidpointRounding.AwayFromZero)));
                this.intensity_ratio = this.light_intensity / this.heavy_intensity;
                this.light_corrected_mass = neuCodeLight.weighted_monoisotopic_mass + Math.Round((this.lysine_count * 0.1667 - 0.4), 0, MidpointRounding.AwayFromZero) * 1.0015;
            }
        }
    }

    public class Proteoform
    {

    }

    public class ExperimentalProteoform : Proteoform
    {
        public List<NeuCodePair> proteoforms;
        public double agg_mass { get; set; } = 0;
        public double agg_intensity { get; set; } = 0;
        public double agg_rt { get; set; } = 0;
        public int lysine_count { get; set; }
        public int observation_count
        {
            get { return proteoforms.Count; }
        }

        public ExperimentalProteoform(List<NeuCodePair> pf_to_aggregate)
        {
            proteoforms = pf_to_aggregate;
            NeuCodePair root = pf_to_aggregate[0];
            this.lysine_count = root.lysine_count;
            this.agg_intensity = pf_to_aggregate.Select(p => p.light_intensity).Sum();
            this.agg_rt = pf_to_aggregate.Select(p => p.light_apexRt * p.light_intensity / this.agg_intensity).Sum();
            this.agg_mass = pf_to_aggregate.Select(p => 
                (p.light_corrected_mass + Math.Round((root.light_corrected_mass - p.light_corrected_mass), 0) * 1.0015) //mass + mass shift
                * p.light_intensity / this.agg_intensity).Sum();
        }
    }

    public class TheoreticalProteoform : Proteoform
    {
        public string accession { get; set; }
        public string name { get; set; }
        public string fragment { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
        public double unmodified_mass { get; set; }
        public double ptm_mass { get; set; }
        public double modified_mass { get; set; }
        public int lysine_count { get; set; }
        private string sequence { get; set; }
        public List<OneUniquePtmGroup> ptm_list { get; set; } = new List<OneUniquePtmGroup>();

        public TheoreticalProteoform(string accession, string name, string fragment, int begin, int end, double unmodified_mass, int lysine_count, List<OneUniquePtmGroup> ptm_list, double ptm_mass, double modified_mass)
        {
            this.accession = accession;
            this.name = name;
            this.begin = begin;
            this.end = end;
            this.unmodified_mass = unmodified_mass;
            this.modified_mass = modified_mass;
            this.lysine_count = lysine_count;
            this.ptm_list = ptm_list;
            this.ptm_mass = ptm_mass;
        }

        public string ptm_list_string()
        {
            return string.Join("; ", ptm_list);
        }
    }
}
