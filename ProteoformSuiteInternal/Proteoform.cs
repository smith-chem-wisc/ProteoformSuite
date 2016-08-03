using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class Proteoform
    {
        public string accession { get; set; }
        public double modified_mass { get; set; }
        public int lysine_count { get; set; } = -1;
        public bool is_target { get; set; } = true;
        public bool is_decoy { get; set; } = false;
        public List<ProteoformRelation> relationships { get; set; } = new List<ProteoformRelation>();

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
        public Proteoform(string accession)
        {
            this.accession = accession;
        }

        public List<Proteoform> get_connected_proteoforms()
        {
            return relationships.Where(r => r.peak.peak_accepted).SelectMany(r => r.connected_proteoforms).ToList();
            //return relationships.SelectMany(r => r.connected_proteoforms).ToList();
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
        private int _observation_count;
        public int observation_count
        {
            set { _observation_count = value; }
            get { if (!Lollipop.updated_agg) { return _observation_count; }
                else { return aggregated_components.Count; } }
        }

        public ExperimentalProteoform(string accession, Component root, List<Component> candidate_observations, bool is_target) : base(accession)
        {
            this.root = root;
            this.aggregated_components = new List<Component>() { root };
            this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p)));
            this.calculate_properties();
        }

        public ExperimentalProteoform(string accession, double modified_mass, int lysine_count, bool is_target) : base(accession)
        {
            this.aggregated_components = new List<Component>() { root };
            this.accession = accession;
            this.modified_mass = modified_mass;
            this.lysine_count = lysine_count;
            if (!is_target)
            {
                this.is_target = false;
                this.is_decoy = true;
            }
        }

        //for Tests
        public ExperimentalProteoform(string accession) : base(accession)
        {
            this.aggregated_components = new List<Component>() { root };
            this.accession = accession;
        }

        private void calculate_properties()
        {
            if (Lollipop.neucode_labeled)
            {   //if neucode labeled, use intensity sum for overlapping charge states --> neucode pair. 
                this.agg_intensity = aggregated_components.Select(p => p.intensity_sum_olcs).Sum();
                this.agg_rt = aggregated_components.Select(p => p.rt_apex * p.intensity_sum_olcs / this.agg_intensity).Sum();
                this.agg_mass = aggregated_components.Select(p =>
                    (p.corrected_mass + Math.Round((this.root.corrected_mass - p.corrected_mass), 0) * 1.0015) //mass + mass shift
                    * p.intensity_sum_olcs / this.agg_intensity).Sum();
            }
            else
            {
                this.agg_intensity = aggregated_components.Select(p => p.intensity_sum).Sum();
                this.agg_rt = aggregated_components.Select(p => p.rt_apex * p.intensity_sum / this.agg_intensity).Sum();
                this.agg_mass = aggregated_components.Select(p =>
                    (p.corrected_mass + Math.Round((this.root.corrected_mass - p.corrected_mass), 0) * 1.0015) //mass + mass shift
                    * p.intensity_sum / this.agg_intensity).Sum();
            }
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

        public string as_tsv_row()
        {
            return String.Join("\t", new List<string> { this.accession.ToString(), this.modified_mass.ToString(), this.lysine_count.ToString(), this.is_target.ToString(), this.is_decoy.ToString(),
                this.agg_mass.ToString(), this.agg_intensity.ToString(), this.agg_rt.ToString(), this.observation_count.ToString() });
        }

        public static string get_tsv_header()
        {
            return String.Join("\t", new List<string> { "accession", "modified_mass", "lysine_count", "is_target", "is_decoy",
                "agg_mass", "agg_intensity", "agg_rt", "observation_count" });
        }
    }

    public class TheoreticalProteoform : Proteoform
    {
        public string name { get; set; }
        public string description { get; set; }
        public string fragment { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
        public double unmodified_mass { get; set; }
        public double ptm_mass { get; set; }
        private string sequence { get; set; }
        public List<Ptm> ptm_list { get; set; } = new List<Ptm>();
        public string ptm_descriptions
        {
            get { return ptm_list_string(); }
        }


        public TheoreticalProteoform(string accession, string description, string name, string fragment, int begin, int end, double unmodified_mass, int lysine_count, List<Ptm> ptm_list, double ptm_mass, double modified_mass, bool is_target) : base(accession, modified_mass, lysine_count, is_target)
        {
            this.accession = accession;
            this.description = description;
            this.name = name;
            this.fragment = fragment;
            this.begin = begin;
            this.end = end;
            this.unmodified_mass = unmodified_mass;
            this.ptm_list = ptm_list;
            this.ptm_mass = ptm_mass;
        }

        //for Tests
        public TheoreticalProteoform(string accession): base(accession)
        {
            this.accession = accession;
        }
        //for Tests
        public TheoreticalProteoform(string accession, double modified_mass, int lysine_count, bool is_target) : base (accession,  modified_mass,  lysine_count,  is_target)
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
            if (ptm_list.Count == 0)
                return "unmodified";
            else
                return string.Join("; ", ptm_list.Select(ptm => ptm.modification.description));
        }

        //use this if reading in theoretical/decoy databases. 
        public void set_ptm_list(string descriptions)
        {
            if (Lollipop.opened_results && !Lollipop.updated_theoretical)
            {
                string[] ptm_descriptions = descriptions.Split(';');
                foreach (string description in ptm_descriptions)
                {
                    description.Trim();
                    description.TrimEnd(';');
                    Modification mod = new Modification(description);
                    Ptm ptm = new Ptm(0, mod);
                    this.ptm_list.Add(ptm);
                }
            }
        }

        public string as_tsv_row(string decoy_database)
        {
            if (is_target)
            return String.Join("\t", new List<string> { this.accession.ToString(), this.modified_mass.ToString(), this.lysine_count.ToString(), this.is_target.ToString(), this.is_decoy.ToString(),
                this.description.ToString(), this.name.ToString(), this.fragment.ToString(), this.begin.ToString(), this.end.ToString(), this.unmodified_mass.ToString(), ptm_descriptions.ToString(), this. ptm_mass.ToString() });
            else
                return String.Join("\t", new List<string> { this.accession.ToString(), this.modified_mass.ToString(), this.lysine_count.ToString(), this.is_target.ToString(), this.is_decoy.ToString(),
                this.description.ToString(), this.name.ToString(), this.fragment.ToString(), this.begin.ToString(), this.end.ToString(), this.unmodified_mass.ToString(), ptm_descriptions.ToString(), this. ptm_mass.ToString(), decoy_database.ToString()});
        }

        public static string get_tsv_header(bool is_target)
        {
            if (is_target)
                return String.Join("\t", new List<string> { "accession", "modified_mass", "lysine_count", "is_target", "is_decoy",
                "description", "name", "fragment", "begin", "end", "unmodified_mass", "ptm_list", "ptm_mass" });
            else
                return  String.Join("\t", new List<string> { "accession", "modified_mass", "lysine_count", "is_target", "is_decoy",
                "description", "name", "fragment", "begin", "end", "unmodified_mass", "ptm_list", "ptm_mass", "decoy_database" });
        }
    }
}
