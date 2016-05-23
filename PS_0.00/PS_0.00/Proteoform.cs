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
        public string accession { get; set; }
        public double modified_mass { get; set; }
        public int lysine_count { get; set; }
        public List<ProteoformRelation> experimental_relationships { get; set; } = new List<ProteoformRelation>();
        public List<ProteoformRelation> theoretical_relationships { get; set; } = new List<ProteoformRelation>();
        public List<ProteoformRelation> decoy_relationships { get; set; } = new List<ProteoformRelation>();

        public Proteoform(string accession, double modified_mass, int lysine_count)
        {
            this.accession = accession;
            this.modified_mass = modified_mass;
            this.lysine_count = lysine_count;
        }

        public Proteoform(string accession)
        {
            this.accession = accession;
        }
    }

    public class ProteoformRelation
    {
        public Proteoform pf1;
        public Proteoform pf2;
        public double delta_mass;
        public bool accepted { get; set; } = false;
        public ProteoformRelation(Proteoform pf1, Proteoform pf2, double delta_mass)
        {
            this.pf1 = pf1;
            this.pf2 = pf2;
            this.delta_mass = delta_mass;
        }

        public void accept()
        {
            this.accepted = true;
        }
    }

    public class ExperimentalProteoform : Proteoform
    {
        private NeuCodePair root;
        public List<NeuCodePair> proteoforms;
        public double agg_mass { get; set; } = 0;
        public double agg_intensity { get; set; } = 0;
        public double agg_rt { get; set; } = 0;
        public int observation_count
        {
            get { return proteoforms.Count; }
        }

        public ExperimentalProteoform(string accession, NeuCodePair root) : base(accession)
        {
            this.root = root;
            proteoforms = new List<NeuCodePair>() { root };
        }

        public void calculate_properties()
        {
            this.agg_intensity = proteoforms.Select(p => p.light_intensity).Sum();
            this.agg_rt = proteoforms.Select(p => p.light_apexRt * p.light_intensity / this.agg_intensity).Sum();
            this.agg_mass = proteoforms.Select(p =>
                (p.light_corrected_mass + Math.Round((this.root.light_corrected_mass - p.light_corrected_mass), 0) * 1.0015) //mass + mass shift
                * p.light_intensity / this.agg_intensity).Sum();
            this.lysine_count = this.root.lysine_count;
            this.modified_mass = this.agg_mass;
        }

        public bool add(NeuCodePair new_pair)
        {
            proteoforms.Add(new_pair);
            if (new_pair.light_intensity > root.light_intensity) this.root = new_pair;
            return true;
        }

        public bool includes(NeuCodePair candidate)
        {
            return tolerable_rt(candidate) && tolerable_lysCt(candidate) && tolerable_mass(candidate);
        }

        private bool tolerable_rt(NeuCodePair candidate)
        {
            return candidate.light_apexRt >= this.root.light_apexRt - Convert.ToDouble(Lollipop.retention_time_tolerance) &&
                candidate.light_apexRt <= this.root.light_apexRt + Convert.ToDouble(Lollipop.retention_time_tolerance);
        }

        private bool tolerable_lysCt(NeuCodePair candidate)
        {
            int max_missed_lysines = Convert.ToInt32(Lollipop.missed_lysines);
            List<int> acceptable_lysineCts = Enumerable.Range(this.root.lysine_count - max_missed_lysines, this.root.lysine_count + max_missed_lysines).ToList();
            return acceptable_lysineCts.Contains(candidate.lysine_count);
        }

        private bool tolerable_mass(NeuCodePair candidate)
        {
            int max_missed_monoisotopics = Convert.ToInt32(Lollipop.missed_monos);
            List<int> missed_monoisotopics = Enumerable.Range(-max_missed_monoisotopics, max_missed_monoisotopics).ToList();
            foreach (int m in missed_monoisotopics)
            {
                double shift = m * 1.0015;
                double mass_tolerance = (this.root.light_corrected_mass + shift) / 1000000 * Convert.ToInt32(Lollipop.mass_tolerance);
                double low = this.root.light_corrected_mass + shift - mass_tolerance;
                double high = this.root.light_corrected_mass + shift + mass_tolerance;
                bool tolerable_mass = candidate.light_corrected_mass >= low && candidate.light_corrected_mass <= high;
                if (tolerable_mass) return true; //Return a true result immediately; acts as an OR between these conditions
            }
            return false;
        }
    }

    public class TheoreticalProteoform : Proteoform
    {
        public string name { get; set; }
        public string fragment { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
        public double unmodified_mass { get; set; }
        public double ptm_mass { get; set; }
        private string sequence { get; set; }
        public bool is_target { get; set; } = true;
        public bool is_decoy { get; } = false;
        public List<string> ptm_list { get; set; } = new List<string>();

        public TheoreticalProteoform(string accession, string name, string fragment, int begin, int end, double unmodified_mass, int lysine_count, List<string> ptm_list, double ptm_mass, double modified_mass) : base(accession, modified_mass, lysine_count)
        {
            this.accession = accession;
            this.name = name;
            this.begin = begin;
            this.end = end;
            this.unmodified_mass = unmodified_mass;
            this.ptm_list = ptm_list;
            this.ptm_mass = ptm_mass;
        }
    
        public static double CalculateProteoformMass(string pForm, Dictionary<char, double> aaIsotopeMassList)
        {
            double proteoformMass = 18.010565; // start with water
            char[] aminoAcids = pForm.ToCharArray();
            List<double> aaMasses = new List<double>();
            Parallel.For(0, pForm.Length, i =>
            {
                if (aaIsotopeMassList.ContainsKey(aminoAcids[i]))
                    aaMasses.Add(aaIsotopeMassList[aminoAcids[i]]);
            });
            return proteoformMass + aaMasses.Sum();
        }

        public string ptm_list_string()
        {
            return string.Join("; ", ptm_list);
        }
    }
}
