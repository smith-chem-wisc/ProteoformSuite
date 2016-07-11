using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace ProteoformSuite
{
    public class Proteoform
    {
        public string accession { get; set; }
        public double modified_mass { get; set; }
        public int lysine_count { get; set; } = -1
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
        private Component root;
        public List<Component> aggregated_components;
        public double agg_mass { get; set; } = 0;
        public double agg_intensity { get; set; } = 0;
        public double agg_rt { get; set; } = 0;
        public int observation_count
        {
            get { return aggregated_components.Count; }
        }

        public ExperimentalProteoform(string accession, Component root, List<Component> candidate_observations, bool is_target) : base(accession, is_target)
        {
            this.root = root;
            this.aggregated_components = new List<Component>() { root };
            this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p)));
            this.calculate_properties();
        }

        private void calculate_properties()
        {
            this.agg_intensity = aggregated_components.Select(p => p.intensity_sum).Sum();
            this.agg_rt = aggregated_components.Select(p => p.rt_apex * p.intensity_sum / this.agg_intensity).Sum();
            this.agg_mass = aggregated_components.Select(p =>
                (p.corrected_mass + Math.Round((this.root.corrected_mass - p.corrected_mass), 0) * 1.0015) //mass + mass shift
                * p.intensity_sum / this.agg_intensity).Sum();
            if (root is NeuCodePair) this.lysine_count = ((NeuCodePair)this.root).lysine_count;
            this.modified_mass = this.agg_mass;
        }

        public bool includes(Component candidate)
        {
            bool does_include = tolerable_rt(candidate) && tolerable_mass(candidate);
            if (candidate is NeuCodePair) does_include = does_include && tolerable_lysCt((NeuCodePair)candidate);
            return does_include;
        }

        private bool tolerable_rt(Component candidate)
        {
            return candidate.rt_apex >= this.root.rt_apex - Convert.ToDouble(Lollipop.retention_time_tolerance) &&
                candidate.rt_apex <= this.root.rt_apex + Convert.ToDouble(Lollipop.retention_time_tolerance);
        }

        private bool tolerable_lysCt(NeuCodePair candidate)
        {
            int max_missed_lysines = Convert.ToInt32(Lollipop.missed_lysines);
            List<int> acceptable_lysineCts = Enumerable.Range(((NeuCodePair)this.root).lysine_count - max_missed_lysines, max_missed_lysines * 2 + 1).ToList();
            return acceptable_lysineCts.Contains(candidate.lysine_count);
        }

        private bool tolerable_mass(Component candidate)
        {
            int max_missed_monoisotopics = Convert.ToInt32(Lollipop.missed_monos);
            List<int> missed_monoisotopics = Enumerable.Range(-max_missed_monoisotopics, max_missed_monoisotopics * 2 + 1).ToList();
            foreach (int m in missed_monoisotopics)
            {
                double shift = m * 1.0015;
                double mass_tolerance = (this.root.corrected_mass + shift) / 1000000 * Convert.ToInt32(Lollipop.mass_tolerance);
                double low = this.root.corrected_mass + shift - mass_tolerance;
                double high = this.root.corrected_mass + shift + mass_tolerance;
                bool tolerable_mass = candidate.corrected_mass >= low && candidate.corrected_mass <= high;
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
