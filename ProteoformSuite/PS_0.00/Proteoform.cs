using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace ProteoformSuite
{
    public class NeuCodePair 
    {
        Component neuCodeLight;
        Component neuCodeHeavy;
        public string file_origin { get { return neuCodeLight.file_origin; } }
        public int light_id { get { return neuCodeLight.id; } }
        public double light_weighted_monoisotopic_mass { get { return neuCodeLight.weighted_monoisotopic_mass; } }
        public double light_corrected_mass { get; set; }
        public double light_intensity { get; set; }
        public double light_apexRt { get { return neuCodeLight.rt_apex; } }
        public int heavy_id { get { return neuCodeHeavy.id; } }
        public double heavy_weighted_monoisotopic_mass { get { return neuCodeHeavy.weighted_monoisotopic_mass; } }
        public double heavy_intensity { get; set; }
        public List<int> overlapping_charge_states { get; set; }
        
        public double intensity_ratio { get; set; }
        public int lysine_count { get; set; }
        public bool accepted { get; set; } = false;

        public NeuCodePair(Component lower_rawNeuCode, Component higher_rawNeuCode)
        {
            double mass_difference = higher_rawNeuCode.weighted_monoisotopic_mass - lower_rawNeuCode.weighted_monoisotopic_mass; //changed from decimal; it doesn't seem like that should make a difference
            List<int> lower_charges = lower_rawNeuCode.charge_states.Select(charge_state => charge_state.charge_count).ToList<int>();
            List<int> higher_charges = higher_rawNeuCode.charge_states.Select(charge_states => charge_states.charge_count).ToList<int>();
            this.overlapping_charge_states = lower_charges.Intersect(higher_charges).ToList();
            double lower_intensity = lower_rawNeuCode.calculate_sum_intensity(this.overlapping_charge_states);
            double higher_intensity = lower_rawNeuCode.calculate_sum_intensity(this.overlapping_charge_states);

            if (lower_intensity > 0 || higher_intensity > 0)
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

                int diff_integer = Convert.ToInt32(Math.Round(mass_difference / 1.0015 - 0.5, 0, MidpointRounding.AwayFromZero));
                double firstCorrection = neuCodeLight.weighted_monoisotopic_mass + diff_integer * 1.0015;
                this.lysine_count = Math.Abs(Convert.ToInt32(Math.Round((neuCodeHeavy.weighted_monoisotopic_mass - firstCorrection) / 0.036015372, 0, MidpointRounding.AwayFromZero)));
                this.intensity_ratio = this.light_intensity / this.heavy_intensity;
                this.light_corrected_mass = neuCodeLight.weighted_monoisotopic_mass + Math.Round((this.lysine_count * 0.1667 - 0.4), 0, MidpointRounding.AwayFromZero) * 1.0015;
            }
        }

        public string as_csv_row()
        {
            return String.Join(",", new List<string> { this.light_id.ToString(), this.light_intensity.ToString(), this.light_weighted_monoisotopic_mass.ToString(), this.light_corrected_mass.ToString(), this.light_apexRt.ToString(),
                this.heavy_id.ToString(), this.heavy_intensity.ToString(), this.heavy_weighted_monoisotopic_mass.ToString(), this.intensity_ratio.ToString(), this.lysine_count.ToString(),
                this.file_origin.ToString() });
        }

        public static string get_csv_header()
        {
            return String.Join(",", new List<string> { "light_id", "light_intensity", "light_weighted_monoisotopic_mass", "light_corrected_mass", "light_apexRt",
                "heavy_id", "heavy_intensity", "heavy_weighted_monoisotopic_mass", "intensity_ratio", "lysine_count", "file_origin" });
        }
    }

    public class Proteoform
    {
        public string accession { get; set; }
        public double modified_mass { get; set; }
        public int lysine_count { get; set; }
        public bool is_target { get; set; } = true;
        public bool is_decoy { get; } = false;
        public List<MassDifference> relationships { get; set; } = new List<MassDifference>();

        public Proteoform(string accession, double modified_mass, int lysine_count, bool is_target)
        {
            this.accession = accession;
            this.modified_mass = modified_mass;
            this.lysine_count = lysine_count;
            if (!is_target)
            {
                this.is_target = false;
                this.is_decoy = true;
            }
        }
        public Proteoform(string accession, bool is_target)
        {
            this.accession = accession;
        }

        public List<Proteoform> get_connected_proteoforms()
        {
            return relationships.SelectMany(r => r.connected_proteoforms).ToList();
        }
    }
    
    //Note ExperimentalProteoform is a bit of a misnomer. These are not experimental observations, but rather aggregated experimental
    //observations. Each NeuCodePair is an ExperimentalProteoform, but this class is used after accounting for missed lysines and monoisotopics.
    //However, I think this makes the programming a bit cleaner, since "Experimental-Theoretical" pairs should naturally be between 
    //"ExperimentalProteoform" and "TheoreticalProteoform" objects
    public class ExperimentalProteoform : Proteoform 
    {
        private NeuCodePair root;
        public List<NeuCodePair> aggregated_neucode_pairs;
        public double agg_mass { get; set; } = 0;
        public double agg_intensity { get; set; } = 0;
        public double agg_rt { get; set; } = 0;
        public int observation_count
        {
            get { return aggregated_neucode_pairs.Count; }
        }

        public ExperimentalProteoform(string accession, NeuCodePair root, List<NeuCodePair> candidate_observations, bool is_target) : base(accession, is_target)
        {
            this.root = root;
            this.aggregated_neucode_pairs = new List<NeuCodePair>() { root };
            this.aggregated_neucode_pairs.AddRange(candidate_observations.Where(p => this.includes(p)));
            this.calculate_properties();
        }

        private void calculate_properties()
        {
            this.agg_intensity = aggregated_neucode_pairs.Select(p => p.light_intensity).Sum();
            this.agg_rt = aggregated_neucode_pairs.Select(p => p.light_apexRt * p.light_intensity / this.agg_intensity).Sum();
            this.agg_mass = aggregated_neucode_pairs.Select(p =>
                (p.light_corrected_mass + Math.Round((this.root.light_corrected_mass - p.light_corrected_mass), 0) * 1.0015) //mass + mass shift
                * p.light_intensity / this.agg_intensity).Sum();
            this.lysine_count = this.root.lysine_count;
            this.modified_mass = this.agg_mass;
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
            List<int> acceptable_lysineCts = Enumerable.Range(this.root.lysine_count - max_missed_lysines, max_missed_lysines * 2 + 1).ToList();
            return acceptable_lysineCts.Contains(candidate.lysine_count);
        }

        private bool tolerable_mass(NeuCodePair candidate)
        {
            int max_missed_monoisotopics = Convert.ToInt32(Lollipop.missed_monos);
            List<int> missed_monoisotopics = Enumerable.Range(-max_missed_monoisotopics, max_missed_monoisotopics * 2 + 1).ToList();
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

        public string as_csv_row()
        {
            return String.Join(",", new List<string> { this.accession.ToString(), this.modified_mass.ToString(), this.lysine_count.ToString(), this.is_target.ToString(), this.is_decoy.ToString(),
                this.agg_mass.ToString(), this.agg_intensity.ToString(), this.agg_rt.ToString(), this.observation_count.ToString() });
        }

        public static string get_csv_header()
        {
            return String.Join(",", new List<string> { "accession", "modified_mass", "lysine_count", "is_target", "is_decoy",
                "agg_mass", "agg_intensity", "agg_rt", "observation_count" });
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
        public List<Ptm> ptm_list { get; set; } = new List<Ptm>();

        public TheoreticalProteoform(string accession, string name, string fragment, int begin, int end, double unmodified_mass, int lysine_count, List<Ptm> ptm_list, double ptm_mass, double modified_mass, bool is_target) : base(accession, modified_mass, lysine_count, is_target)
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
            for (int i = 0; i < pForm.Length; i++)
            {
                if (aaIsotopeMassList.ContainsKey(aminoAcids[i])) aaMasses.Add(aaIsotopeMassList[aminoAcids[i]]);
            }
            return proteoformMass + aaMasses.Sum();
        }

        public string ptm_list_string()
        {
            return string.Join("; ", ptm_list.Select(ptm => ptm.modification.description));
        }
    }
}
