using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

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
        public ProteoformFamily family { get; set; }

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

        public Proteoform()
        { }

        public List<Proteoform> get_connected_proteoforms()
        {
            return relationships.Where(r => r.accepted).SelectMany(r => r.connected_proteoforms).ToList();
        }
    }

    //Note ExperimentalProteoform is a bit of a misnomer. These are not experimental observations, but rather aggregated experimental
    //observations. Each NeuCodePair is an ExperimentalProteoform, but this class is used after accounting for missed lysines and monoisotopics.
    //However, I think this makes the programming a bit cleaner, since "Experimental-Theoretical" pairs should naturally be between 
    //"ExperimentalProteoform" and "TheoreticalProteoform" objects
    public class ExperimentalProteoform : Proteoform
    {
        // PROPERTIES
        public Component root;
        public List<Component> aggregated_components { get; set; } = new List<Component>();
        public List<Component> lt_verification_components { get; set; } = new List<Component>();
        public List<Component> hv_verification_components { get; set; } = new List<Component>();
        public List<Component> lt_quant_components { get; set; } = new List<Component>();
        public List<Component> hv_quant_components { get; set; } = new List<Component>();
        public List<biorepIntensity> biorepIntensityList { get; set; } = new List<biorepIntensity>(); //(bool light = true, int biorep, double intensity)
        public quantitativeValues quant { get; set; }
        public bool accepted { get; set; } = true;
        public double agg_mass { get; set; } = 0;
        public double agg_intensity { get; set; } = 0;
        public double agg_rt { get; set; } = 0;
        public bool mass_shifted { get; set; } = false; //make sure in ET if shifting multiple peaks, not shifting same E > once. 
        public int observation_count
        {
            get { return aggregated_components.Count; }
        }
        public int light_observation_count
        {
            get { return lt_quant_components.Count; }
        }
        public int heavy_observation_count
        {
            get { return hv_quant_components.Count; }
        }

        // CONTRUCTORS
        public ExperimentalProteoform(ExperimentalProteoform eP)
        {
            copy(eP);
        }
        public ExperimentalProteoform(string accession, Component root, List<Component> candidate_observations, bool is_target) : base(accession)
        {
            this.root = root;
            this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p, this.root)));
            this.calculate_properties();
            this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault();
        }

        public ExperimentalProteoform(string accession, Component root, bool is_target) : base(accession)
        {
            this.root = root;
            this.is_target = is_target;
        }

        public ExperimentalProteoform(string accession, ExperimentalProteoform temp, List<Component> candidate_observations, bool is_target) : base(accession) //this is for first mass of aggregate components. uses a temporary component
        {
            Component root = new Component();
            NeuCodePair ncRoot = new NeuCodePair();
            if (Lollipop.neucode_labeled)
            {
                ((Component)ncRoot).weighted_monoisotopic_mass = temp.agg_mass;
                ((Component)ncRoot).intensity_sum = temp.agg_intensity;
                ncRoot.rt_apex = temp.agg_rt;
                ncRoot.lysine_count = temp.lysine_count;

                this.root = ncRoot;
                this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p, this.root)));
                this.calculate_properties();
                this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault(); //reset root to component with max intensity
            }
            else
            {
                root.weighted_monoisotopic_mass = temp.agg_mass;
                root.intensity_sum = temp.agg_intensity;
                root.rt_apex = temp.agg_rt;

                this.root = root;
                this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p, this.root)));
                this.calculate_properties();
                this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault(); //reset root to component with max intensity
            }
        }
        private void copy(ExperimentalProteoform e)
        {
            this.root = e.root;
            this.agg_intensity = e.agg_intensity;
            this.agg_mass = e.agg_mass;
            this.modified_mass = e.modified_mass;
            this.agg_rt = e.agg_rt;
            this.lysine_count = e.lysine_count;
            this.accepted = e.accepted;
            this.quant = e.quant;
            this.mass_shifted = e.mass_shifted;
            this.is_target = e.is_target;
            this.is_decoy = e.is_decoy;
            this.family = e.family;
            this.aggregated_components = new List<Component>(e.aggregated_components);
            this.lt_quant_components = new List<Component>(e.lt_quant_components);
            this.lt_verification_components = new List<Component>(e.lt_verification_components);
            this.hv_quant_components = new List<Component>(e.hv_quant_components);
            this.hv_verification_components = new List<Component>(e.hv_verification_components);
        }

        // AGGREGATION METHODS
        public void aggregate()
        {
            ExperimentalProteoform temp_pf = new ExperimentalProteoform("tbd", this.root, Lollipop.remaining_components.ToList(), true); //first pass returns temporary proteoform
            ExperimentalProteoform new_pf = new ExperimentalProteoform("tbd", temp_pf, Lollipop.remaining_components.ToList(), true); //second pass uses temporary protoeform from first pass.
            copy(new_pf);
            this.root = temp_pf.root; //maintain the original component root
        }

        public void verify()
        {
            foreach (Component c in Lollipop.remaining_verification_components)
            {
                if (this.includes(c, this, true))
                    lt_verification_components.Add(c);
                if (Lollipop.neucode_labeled && this.includes(c, this, false))
                    hv_verification_components.Add(c);
            }
        }

        //for reading in neucode EE pairs
        public ExperimentalProteoform(string accession, double agg_mass, int lysine_count, double agg_rt) : base(accession)
        {
            this.accession = accession;
            this.agg_mass = agg_mass;
            this.lysine_count = lysine_count;
            this.agg_rt = agg_rt;
        }

        public void aggregate_and_verify()
        {
            aggregate();
            verify();
        }

        public void assign_quantitative_components()
        {
            foreach (Component c in Lollipop.remaining_components)
            {
                if (this.includes(c, this, true)) lt_quant_components.Add(c);
                if (this.includes(c, this, false)) hv_quant_components.Add(c);
            }
            //lt_quant_components.AddRange(Lollipop.remaining_components.Where(r => this.includes(r, this, true)));
            ////ep.getBiorepAndFractionIntensities(false); //split lt components by biorep and fraction
            //hv_quant_components.AddRange(Lollipop.remaining_components.Where(r => this.includes(r, this, false)));
            ////ep.getBiorepAndFractionIntensities(true); //split hv components by biorep and fraction
        }

        public void calculate_properties()
        {
            //if not neucode labeled, the intensity sum of overlapping charge states was calculated with all charge states.
            if (Lollipop.neucode_labeled)
            {
                this.agg_intensity = aggregated_components.Select(p => p.intensity_sum_olcs).Sum();
                this.agg_mass = aggregated_components.Select(p => (p.weighted_monoisotopic_mass - Math.Round(p.weighted_monoisotopic_mass - this.root.weighted_monoisotopic_mass, 0) * Lollipop.MONOISOTOPIC_UNIT_MASS) * p.intensity_sum_olcs / this.agg_intensity).Sum(); //remove the monoisotopic errors before aggregating masses
                this.agg_rt = aggregated_components.Select(p => p.rt_apex * p.intensity_sum_olcs / this.agg_intensity).Sum();
            }
            else
            {
                this.agg_intensity = aggregated_components.Select(p => p.intensity_sum).Sum();
                this.agg_mass = aggregated_components.Select(p => (p.weighted_monoisotopic_mass - Math.Round(p.weighted_monoisotopic_mass - this.root.weighted_monoisotopic_mass, 0) * Lollipop.MONOISOTOPIC_UNIT_MASS) * p.intensity_sum / this.agg_intensity).Sum(); //remove the monoisotopic errors before aggregating masses
                this.agg_rt = aggregated_components.Select(p => p.rt_apex * p.intensity_sum / this.agg_intensity).Sum();

            }
            if (root is NeuCodePair) this.lysine_count = ((NeuCodePair)this.root).lysine_count;
            this.modified_mass = this.agg_mass;
            this.accepted = this.aggregated_components.Count >= Lollipop.min_agg_count; 
        }

        //This aggregates based on lysine count, mass, and retention time all at the same time. Note that in the past we aggregated based on 
        //lysine count first, and then aggregated based on mass and retention time afterwards, which may give a slightly different root for the 
        //experimental proteoform because the precursor aggregation may shuffle the intensity order slightly. We haven't observed any negative
        //impact of this difference as of 160812. -AC
        public bool includes(Component candidate, Component root)
        {
            bool does_include = tolerable_rt(candidate, root.rt_apex) && tolerable_mass(candidate, root.weighted_monoisotopic_mass);
            if (candidate is NeuCodePair) does_include = does_include && tolerable_lysCt((NeuCodePair)candidate, ((NeuCodePair)root).lysine_count);
            return does_include;
        }

        public bool includes(Component candidate)
        {
            bool does_include = tolerable_rt(candidate, this.agg_rt) && tolerable_mass(candidate, this.agg_mass);
            if (candidate is NeuCodePair) does_include = does_include && tolerable_lysCt((NeuCodePair)candidate, this.lysine_count);
            return does_include;
        }

        public bool includes(Component candidate, ExperimentalProteoform root, bool light)
        {
            double corrected_mass = root.agg_mass;
            if (!light)
                corrected_mass = corrected_mass + root.lysine_count * Lollipop.NEUCODE_LYSINE_MASS_SHIFT;

            bool does_include = tolerable_rt(candidate, root.agg_rt) && tolerable_mass(candidate, corrected_mass);
            if (candidate is NeuCodePair) does_include = does_include && tolerable_lysCt((NeuCodePair)candidate, root.lysine_count);
            return does_include;
        }

        private bool tolerable_rt(Component candidate, double rt_apex)
        {
            return candidate.rt_apex >= rt_apex - Convert.ToDouble(Lollipop.retention_time_tolerance) &&
                candidate.rt_apex <= rt_apex + Convert.ToDouble(Lollipop.retention_time_tolerance);
        }

        private bool tolerable_lysCt(NeuCodePair candidate, int lysine_count)
        {
            int max_missed_lysines = Convert.ToInt32(Lollipop.missed_lysines);
            List<int> acceptable_lysineCts = Enumerable.Range(lysine_count - max_missed_lysines, max_missed_lysines * 2 + 1).ToList();
            return acceptable_lysineCts.Contains(candidate.lysine_count);
        }

        private bool tolerable_mass(Component candidate, double corrected_mass)
        {
            int max_missed_monoisotopics = Convert.ToInt32(Lollipop.missed_monos);
            List<int> missed_monoisotopics_range = Enumerable.Range(-max_missed_monoisotopics, max_missed_monoisotopics * 2 + 1).ToList();
            foreach (int missed_mono_count in missed_monoisotopics_range)
            {
                double shift = missed_mono_count * Lollipop.MONOISOTOPIC_UNIT_MASS;
                double shifted_mass = corrected_mass + shift;
                double mass_tolerance = shifted_mass / 1000000 * (double)Lollipop.mass_tolerance;
                double low = shifted_mass - mass_tolerance;
                double high = shifted_mass + mass_tolerance;
                bool tolerable_mass = candidate.weighted_monoisotopic_mass >= low && candidate.weighted_monoisotopic_mass <= high;
                if (tolerable_mass) return true; //Return a true result immediately; acts as an OR between these conditions
            }
            return false;
        }


        // QUANTITATION CLASS AND METHODS
        public void make_biorepIntensityList()
        {
            this.biorepIntensityList.Clear();
            foreach (string condition in Lollipop.ltConditionsBioReps.Keys)
            {
                foreach (int b in this.lt_quant_components.Where(c => c.input_file.lt_condition == condition).Select(c => c.input_file.biological_replicate).Distinct())
                {
                    this.biorepIntensityList.Add(new biorepIntensity(true, false, b, condition, this.lt_quant_components.Where(c => c.input_file.biological_replicate == b).Select(i => i.intensity_sum).ToList().Sum()));
                }
            }
            if (Lollipop.neucode_labeled)
            {
                foreach (string condition in Lollipop.hvConditionsBioReps.Keys)
                {
                    foreach (int b in this.hv_quant_components.Where(c => c.input_file.hv_condition == condition).Select(c => c.input_file.biological_replicate).Distinct())
                    {
                        this.biorepIntensityList.Add(new biorepIntensity(false, false, b, condition, this.hv_quant_components.Where(c => c.input_file.biological_replicate == b).Select(i => i.intensity_sum).ToList().Sum()));
                    }
                }
            }
        }

        public class quantitativeValues
        {
            public string accession { get; set; }
            public List<biorepIntensity> lightBiorepIntensities { get; set; }
            public List<biorepIntensity> heavyBiorepIntensities { get; set; }
            public List<biorepIntensity> lightImputedIntensities { get; set; }
            public List<biorepIntensity> heavyImputedIntensities { get; set; }
            public decimal lightIntensitySum { get; set; } = 0;
            public decimal heavyIntensitySum { get; set; } = 0;
            public decimal intensitySum { get; set; } = 0;
            public decimal logFoldChange { get; set; } = 0;
            public decimal variance { get; set; } = 0;
            public decimal pValue { get; set; } = 0;
            public bool significant { get; set; } = false;

            //Selecting numerator and denominator is not implemented
            public quantitativeValues(ExperimentalProteoform eP, decimal bkgdAverageIntensity, decimal bkgdStDev, string numerator, string denominator)
            {
                //numerator and denominator not used yet b/c of the programming that would require.
                eP.quant = this;
                accession = eP.accession;
                lightBiorepIntensities = eP.biorepIntensityList.Where(b => b.light).ToList();
                lightImputedIntensities = imputedIntensities(true, lightBiorepIntensities, bkgdAverageIntensity, bkgdStDev);
                lightIntensitySum = (decimal)lightBiorepIntensities.Select(i => i.intensity).Sum() + (decimal)lightImputedIntensities.Select(i => i.intensity).Sum();
                List<biorepIntensity> allLights = lightBiorepIntensities.Concat(lightImputedIntensities).ToList();

                List<biorepIntensity> allHeavys = new List<biorepIntensity>();
                if (Lollipop.neucode_labeled)
                {
                    heavyBiorepIntensities = eP.biorepIntensityList.Where(b => !b.light).ToList();
                    heavyImputedIntensities = imputedIntensities(false, heavyBiorepIntensities, bkgdAverageIntensity, bkgdStDev);
                    heavyIntensitySum = (decimal)heavyBiorepIntensities.Select(i => i.intensity).Sum() + (decimal)heavyImputedIntensities.Select(i => i.intensity).Sum();
                    allHeavys = heavyBiorepIntensities.Concat(heavyImputedIntensities).ToList();
                }
                intensitySum = lightIntensitySum + heavyIntensitySum;
                logFoldChange = (decimal)Math.Log((double)lightIntensitySum / (double)heavyIntensitySum, 2); // Will get divide by zero error if not neuCode labeled, right? -AC
                variance = Variance(logFoldChange, allLights, allHeavys);
                pValue = PValue(logFoldChange, allLights, allHeavys);
            }

            private List<biorepIntensity> imputedIntensities(bool light, List<biorepIntensity> observedBioreps, decimal bkgdAverageIntensity, decimal bkgdStDev)
            {
                List<biorepIntensity> imputedBioreps = new List<biorepIntensity>();
                foreach (KeyValuePair<string, List<int>> entry in light ? Lollipop.ltConditionsBioReps : Lollipop.hvConditionsBioReps)//keys are conditions and values are bioreps.
                {
                    foreach (int biorep in entry.Value)
                    {
                        if (!observedBioreps.Where(l => l.light == light).Select(k => k.condition).Contains(entry.Key)) // no bioreps observed from this conditon at all
                            add_biorep_intensity(imputedBioreps, bkgdAverageIntensity, bkgdStDev, biorep, entry.Key, light);
                        else if (!observedBioreps.Where(l => l.condition == entry.Key && l.light == light).Select(b => b.biorep).Contains(biorep)) //this condtion was observed but this biorep was not
                            add_biorep_intensity(imputedBioreps, bkgdAverageIntensity, bkgdStDev, biorep, entry.Key, light);
                    }
                }
                return imputedBioreps;
            }

            private void add_biorep_intensity(List<biorepIntensity> b, decimal bkgdAverageIntensity, decimal bkgdStDev, int biorep, string key, bool light)
            {
                Random rand = new Random(); //reuse this if you are generating many
                double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
                double u2 = rand.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                double logIntensity = (double)bkgdAverageIntensity + (double)bkgdStDev * randStdNormal;
                double intensity = Math.Pow(2, logIntensity);
                b.Add(new biorepIntensity(light, true, biorep, key, intensity));//random normal(mean,stdDev^2)
            }

            private decimal Variance(decimal logFoldChange, List<biorepIntensity> lights, List<biorepIntensity> heavys)
            {
                decimal squaredVariance = 0;
                foreach (int biorep in lights.Select(b => b.biorep).ToList())
                {
                    decimal logRepRatio = (decimal)Math.Log((double)(((decimal)lights.Where(b => b.biorep == biorep).Select(i => i.intensity).ToList().Sum()) / ((decimal)heavys.Where(b => b.biorep == biorep).Select(i => i.intensity).ToList().Sum())), 2);
                    squaredVariance = squaredVariance + (decimal)Math.Pow(((double)logRepRatio - (double)logFoldChange), 2);
                }
                return (decimal)Math.Pow((double)squaredVariance, 0.5);
            }

            private decimal PValue(decimal logFoldChange, List<biorepIntensity> lights, List<biorepIntensity> heavys)
            {
                decimal pValue = 0;
                int maxPermutations = 10000;
                ConcurrentBag<decimal> permutedRatios = new ConcurrentBag<decimal>();

                Parallel.For(0, maxPermutations, i =>
                {
                    List<double> combined = lights.Select(j => j.intensity).Concat(heavys.Select(j => j.intensity)).ToList();
                    combined.Shuffle();
                    double numerator = combined.Take(lights.Count).Sum();
                    double denominator = combined.Skip(lights.Count).Take(heavys.Count).Sum();
                    decimal someRatio = (decimal)Math.Log(numerator / denominator, 2);
                    permutedRatios.Add(someRatio);
                });

                if (logFoldChange > 0)
                    pValue = (decimal)(1M / maxPermutations) + (decimal)permutedRatios.Count(x => x > logFoldChange) / (decimal)permutedRatios.Count(); //adding a slight positive shift so that later logarithms don't produce fault
                else
                    pValue = (decimal)(1M / maxPermutations) + (decimal)permutedRatios.Count(x => x < logFoldChange) / (decimal)permutedRatios.Count(); //adding a slight positive shift so that later logarithms don't produce fault
                return pValue;
            }
        }

        // OTHER METHODS
        public void shift_masses(int shift)
        {
            if (Lollipop.neucode_labeled)
            {
                foreach (Component c in this.aggregated_components)
                {
                    ((NeuCodePair)c).manual_mass_shift += shift * Lollipop.MONOISOTOPIC_UNIT_MASS;
                    ((NeuCodePair)c).neuCodeLight.manual_mass_shift += shift * Lollipop.MONOISOTOPIC_UNIT_MASS;
                    ((NeuCodePair)c).neuCodeHeavy.manual_mass_shift += shift * Lollipop.MONOISOTOPIC_UNIT_MASS;
                }
            }
            else //unlabeled
            {
                foreach (Component c in this.aggregated_components)
                {
                    c.manual_mass_shift += shift * Lollipop.MONOISOTOPIC_UNIT_MASS;
                }
            }
            this.mass_shifted = true; //if shifting multiple peaks @ once, won't shift same E more than once if it's in multiple peaks.
        }
        

        // TESTING METHODS
        public ExperimentalProteoform(string accession) : base(accession)
        {
            this.aggregated_components = new List<Component>() { root };
            this.accession = accession;
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
        public ExperimentalProteoform(string accession, Component root, List<Component> candidate_observations, List<Component> quantitative_observations, bool is_target) : base(accession)
        {
            this.root = root;
            this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p, this.root)));
            this.calculate_properties();
            if (quantitative_observations.Count > 0)
            {
                this.lt_quant_components.AddRange(quantitative_observations.Where(r => this.includes(r, this, true)));
                if (Lollipop.neucode_labeled) this.hv_quant_components.AddRange(quantitative_observations.Where(r => this.includes(r, this, false)));
            }
            this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault();
        }

        
        

        

        

    }

    public class TheoreticalProteoform : Proteoform
    {
        public List<Protein> proteinList { get; set; } = new List<Protein>();
        public string name { get; set; }
        public string description { get; set; }
        public string fragment { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
        public string gene_id { get; set; }
        public double unmodified_mass { get; set; }
        private string sequence { get; set; }
        public List<GoTerm> goTerms { get; set; } = null;
        public PtmSet ptm_set { get; set; } = new PtmSet(new List<Ptm>());
        public List<Ptm> ptm_list { get { return ptm_set.ptm_combination.ToList(); } }
        public double ptm_mass { get { return ptm_set.mass; } }
        public string ptm_descriptions_readin { get; set; } //for reading in neucode data for labelfree 
        public string ptm_descriptions
        {
            get { return ptm_list_string(); }
        }
        public string accession_reduced
        {
            get
            {
                string[] accession_string = this.accession.Split('_');
                return accession_string[0];
            }
        }
        public List<Psm> psm_list { get; set; } = new List<Psm>();
        private int _psm_count_BU;
        public int psm_count_BU { set { _psm_count_BU = value; } get { if (!Lollipop.opened_results_originally) return psm_list.Count; else return _psm_count_BU; } }
        public string of_interest { get; set; } = "";
        public List<TopDownProteoform> TD_proteoforms { get; set; } = new List<TopDownProteoform>();

        public TheoreticalProteoform(string accession, string description, Protein protein, bool is_metCleaved, double unmodified_mass, int lysine_count, List<GoTerm> goTerms, PtmSet ptm_set, double modified_mass, string gene_id, bool is_target) : 
            base(accession, modified_mass, lysine_count, is_target)
        {
            this.proteinList.Add(protein);
            this.accession = accession;
            this.description = description;
            this.name = protein.name;
            this.fragment = protein.fragment;
            this.begin = protein.begin + Convert.ToInt32(is_metCleaved);
            this.end = protein.end;
            this.ptm_set = ptm_set;
            this.unmodified_mass = unmodified_mass;
            this.gene_id = gene_id;
            if (Lollipop.td_results) match_theoreticals();
        }

        public TheoreticalProteoform(string accession, string description, string name, string fragment, int begin, int end, double unmodified_mass, int lysine_count, List<GoTerm> goTerms, PtmSet ptm_set, double modified_mass, string gene_id, bool is_target) :
            base(accession, modified_mass, lysine_count, is_target)
        {
            this.accession = accession;
            this.description = description;
            this.name = name;
            this.fragment = fragment;
            this.begin = begin;
            this.end = end;
            this.ptm_set = ptm_set;
            this.unmodified_mass = unmodified_mass;
            this.gene_id = gene_id;
        }

        private void match_theoreticals()
        {
            List<TopDownProteoform> topdowns = Lollipop.proteoform_community.topdown_proteoforms.Where(td => td.accession == this.accession_reduced).ToList();
            Parallel.ForEach<TopDownProteoform>(topdowns, td => td.topdown_theoreticals.Add(this));
            this.TD_proteoforms.AddRange(topdowns);
        }

        //for reading in neucode pairs
        public TheoreticalProteoform(string accession, double modified_mass, int lysine_count, string ptm_descriptions) : base(accession)
        {
            this.accession = accession;
            this.modified_mass = modified_mass;
            this.lysine_count = lysine_count;
            this.ptm_descriptions_readin = ptm_descriptions;
        }


        //for Tests
        public TheoreticalProteoform(string accession) : base(accession)
        {
            this.accession = accession;
        }

        //for Tests
        public TheoreticalProteoform(string accession, double modified_mass, int lysine_count, bool is_target) : base(accession, modified_mass, lysine_count, is_target)
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
    }

    public class TheoreticalProteoformGroup : TheoreticalProteoform
    {

        public List<string> accessionList { get; set; } // this is the list of accession numbers for all proteoforms that share the same modified mass. the list gets alphabetical order

        public TheoreticalProteoformGroup(string accession, string description, string name, string fragment, int begin, int end, double unmodified_mass, int lysine_count, List<GoTerm> goTerms, PtmSet ptm_set, double modified_mass, string gene_id, bool is_target)
            : base(accession, description, name, fragment, begin, end, unmodified_mass, lysine_count, goTerms, ptm_set, modified_mass, gene_id, is_target)
        { }
        public TheoreticalProteoformGroup(List<TheoreticalProteoform> theoreticals)
            : base(theoreticals[0].accession + "_T" + theoreticals.Count(), String.Join(";", theoreticals.Select(t => t.description)), String.Join(";", theoreticals.Select(t => t.name)), String.Join(";", theoreticals.Select(t => t.fragment)), theoreticals[0].begin, theoreticals[0].end, theoreticals[0].unmodified_mass, theoreticals[0].lysine_count, theoreticals[0].goTerms, theoreticals[0].ptm_set, theoreticals[0].modified_mass, theoreticals[0].gene_id, theoreticals[0].is_target)
        {
            this.accessionList = theoreticals.Select(p => p.accession).ToList();
            this.proteinList = theoreticals.SelectMany(p => p.proteinList).ToList();
        }
    }

    //public class weightedRatioIntensityVariance
    //{
    //    public double ratio { get; set; } = 0;
    //    public double intensity { get; set; } = 0;
    //    public double variance { get; set; } = 0;
    //    public double pValue { get; set; } = 0;
    //}

    public class bftIntensity
    {        
        public bool light { get; set; } = true; // true if unlabelled or neucode light; false if neucode heavy
        public int biorep { get; set; }
        public int fraction { get; set; }
        public int techrep { get; set; }
        public double intensity { get; set; }
        public bftIntensity(bool light, int biorep, int fraction, int techrep, double intensity)
        {
            this.light = light;
            this.biorep = biorep;
            this.fraction = fraction;
            this.techrep = techrep;
            this.intensity = intensity;
        }
    }

    public class biorepIntensity
    {
        public bool light { get; set; } = true; // true if unlabelled or neucode light; false if neucode heavy
        public bool imputed { get; set; } = false;
        public int biorep { get; set; }
        public string condition { get; set; }
        public double intensity { get; set; }

        public biorepIntensity(bool light, bool imputed, int biorep, string condition, double intensity)
        {
            this.light = light;
            this.imputed = imputed;
            this.biorep = biorep;
            this.condition = condition;
            this.intensity = intensity;
        }
    }

    public class TopDownProteoform : Proteoform
    {
        public string uniprot_id { get; set; }
        public string name { get; set; }
        public string sequence { get; set; }
        public int start_index { get; set; }
        public int stop_index { get; set; }
        public List<Ptm> ptm_list { get; set; } = new List<Ptm>();
        public double monoisotopic_mass { get; set; }
        public double theoretical_mass { get; set; }
        public double agg_rt { get; set; }
        public string ptm_descriptions
        {
            get { return ptm_list_string(); }
        }
        public double reported_mass { get; set; }
        private TopDownHit root;
        public List<TheoreticalProteoform> topdown_theoreticals = new List<TheoreticalProteoform>();
        public List<TopDownHit> topdown_hits;
        public int etd_match_count { get { return relationships.Where(r => r.relation_type == ProteoformComparison.etd).ToList().Count; } }
        public int ttd_match_count { get { return relationships.Where(r => r.relation_type == ProteoformComparison.ttd).ToList().Count; } }


        public TopDownProteoform(string accession, TopDownHit root, List<TopDownHit> candidate_hits) : base(accession)
        {
            this.root = root;
            this.name = root.name;
            this.ptm_list = root.ptm_list;
            this.uniprot_id = root.uniprot_id;
            this.sequence = root.sequence;
            this.start_index = root.start_index;
            this.theoretical_mass = root.theoretical_mass;
            this.stop_index = root.stop_index;
            this.topdown_hits = new List<TopDownHit>() { root };
            this.topdown_hits.AddRange(candidate_hits.Where(p => this.includes(p)));
            this.calculate_properties();
        }


        private void calculate_properties()
        {
            this.agg_rt = topdown_hits.Select(h => h.retention_time).Average(); //need to use average (no intensity info)
            this.monoisotopic_mass = topdown_hits.Select(h => (h.corrected_mass - Math.Round(h.corrected_mass - root.corrected_mass, 0) * Lollipop.MONOISOTOPIC_UNIT_MASS)).Average();
            this.modified_mass = this.monoisotopic_mass;
            this.reported_mass = topdown_hits.Select(h => (h.reported_mass - Math.Round(h.reported_mass - root.reported_mass, 0) * Lollipop.MONOISOTOPIC_UNIT_MASS)).Average();
            int count = Lollipop.proteoform_community.topdown_proteoforms.Where(p => p.uniprot_id == this.uniprot_id).ToList().Count + 1;
            this.accession = accession + "_" + count + "_" + Math.Round(this.modified_mass, 2);
        }

        public bool includes(TopDownHit candidate)
        {
            bool does_include = tolerable_rt(candidate) && tolerable_mass(candidate);
            return does_include;
        }

        private bool tolerable_rt(TopDownHit candidate)
        {
            return candidate.retention_time >= this.root.retention_time - Convert.ToDouble(Lollipop.retention_time_tolerance) &&
                candidate.retention_time <= this.root.retention_time + Convert.ToDouble(Lollipop.retention_time_tolerance);
        }

        private bool tolerable_mass(TopDownHit candidate)
        {
            //still look for missed mono's
            int max_missed_monoisotopics = Convert.ToInt32(Lollipop.missed_monos);
            List<int> missed_monoisotopics = Enumerable.Range(-max_missed_monoisotopics, max_missed_monoisotopics * 2 + 1).ToList();
            foreach (int m in missed_monoisotopics)
            {
                double shift = m * Lollipop.MONOISOTOPIC_UNIT_MASS;
                double mass_tolerance = (this.root.corrected_mass + shift) / 1000000 * Convert.ToInt32(Lollipop.mass_tolerance);
                double low = this.root.corrected_mass + shift - mass_tolerance;
                double high = this.root.corrected_mass + shift + mass_tolerance;
                bool tolerable_mass = candidate.corrected_mass >= low && candidate.corrected_mass <= high;
                if (tolerable_mass) return true;
            }
            return false;
        }

        public string ptm_list_string()
        {
            string _modifications_string = "";
            foreach (Ptm ptm in ((TopDownProteoform)this).ptm_list) _modifications_string += (ptm.modification.description + "@" + ptm.position + "; ");
            return _modifications_string;
        }
    }
}


// UNUSED METHODS

//public ExperimentalProteoform(string accession, ExperimentalProteoform temp, List<Component> candidate_observations, List<Component> quantitative_observations, bool is_target) : base(accession) //this is for first mass of aggregate components. uses a temporary component
//        {
//    Component root = new Component();
//    NeuCodePair ncRoot = new NeuCodePair();
//    if (Lollipop.neucode_labeled)
//    {
//        ((Component)ncRoot).attemptToSetWeightedMonoisotopic_mass(temp.agg_mass);
//        ((Component)ncRoot).attemptToSetIntensity(temp.agg_intensity);
//        ncRoot.rt_apex = temp.agg_rt;
//        ncRoot.lysine_count = temp.lysine_count;

//        this.root = ncRoot;
//        this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p, this.root)));
//        this.calculate_properties();
//        if (quantitative_observations.Count > 0)
//        {
//            this.lt_quant_components.AddRange(quantitative_observations.Where(r => this.includes(r, this, true)));
//            this.light_observation_count = this.lt_quant_components.Count;
//            if (Lollipop.neucode_labeled)
//            {
//                this.hv_quant_components.AddRange(quantitative_observations.Where(r => this.includes(r, this, false)));
//                this.heavy_observation_count = this.hv_quant_components.Count;
//            }

//        }
//        this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault(); //reset root to component with max intensity
//    }
//    else
//    {
//        root.attemptToSetWeightedMonoisotopic_mass(temp.agg_mass);
//        root.attemptToSetIntensity(temp.agg_intensity);
//        root.rt_apex = temp.agg_rt;

//        this.root = root;
//        this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p, this.root)));
//        this.calculate_properties();
//        if (quantitative_observations.Count > 0)
//        {
//            this.lt_quant_components.AddRange(quantitative_observations.Where(r => this.includes(r, this, true)));
//            this.light_observation_count = this.lt_quant_components.Count;
//        }
//        this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault(); //reset root to component with max intensity
//    }
//}

//private static int allBioreps()
//{
//    return Lollipop.input_files.Where(q => q.purpose == Purpose.Quantification).Select(b => b.biological_replicate).Distinct().ToList().Count();
//}

//private static int allFractions()
//{
//    List<int> bioreps =  Lollipop.input_files.Where(q => q.purpose == Purpose.Quantification).Select(b => b.biological_replicate).Distinct().ToList();
//    int allFractionsCount = 0;
//    foreach (int br in bioreps)
//    {
//        allFractionsCount += Lollipop.input_files.Where(q => q.purpose == Purpose.Quantification).Where(rep => rep.biological_replicate == br).Select(f => f.fraction).Distinct().ToList().Count();
//    }
//    return allFractionsCount;
//}

//Quantitative Class and Methods
//public class qVals
//{
//    public double ratio { get; set; }
//    public double intensity { get; set; }
//    public double fraction { get; set; }
//    public List<Component> light { get; set; }
//    public List<Component> heavy { get; set; }
//}

//public weightedRatioIntensityVariance weightedRatioAndWeightedVariance(List<InputFile> inputFileList) //the inputFileList is a list of "quantitative" input files
//{
//    List<qVals> quantitativeValues = new List<qVals>();
//    weightedRatioIntensityVariance wRIV = new weightedRatioIntensityVariance();

//    double squaredVariance = 0;

//    inputFileList.ForEach(inFile =>
//    {
//        qVals q = new qVals();
//        q.light = (from s in lt_quant_components where s.input_file == inFile select s).ToList();
//        q.heavy = (from s in hv_quant_components where s.input_file == inFile select s).ToList();
//        double numerator = (from s in lt_quant_components where s.input_file == inFile select s.intensity_sum).Sum();
//        double denominator = (from s in hv_quant_components where s.input_file == inFile select s.intensity_sum).Sum();
//        if (numerator == 0)
//            numerator = numerator + 1000;//adding 1000 to deal with missing values
//        if (denominator == 0)
//            denominator = denominator + 1000;//adding 1000 to deal with missing values
//        q.ratio = Math.Log(numerator / denominator, 2);
//        q.intensity = numerator + denominator;

//        if ((q.light.Count() + q.heavy.Count()) > 0)
//            quantitativeValues.Add(q);
//    });

//    wRIV.intensity = quantitativeValues.Sum(s => s.intensity);

//    if (wRIV.intensity > 0)
//    {
//        quantitativeValues.ForEach(q => {
//            wRIV.ratio = wRIV.ratio + q.ratio * q.intensity / wRIV.intensity;
//            q.fraction = (double)q.intensity / wRIV.intensity;
//        });
//        quantitativeValues.ForEach(q => {
//            squaredVariance = squaredVariance + q.fraction * Math.Pow((q.ratio - wRIV.ratio), 2);
//        });
//        wRIV.pValue = pValueFromPermutation(quantitativeValues, wRIV.intensity, wRIV.ratio);
//    }

//    if (squaredVariance > 0)
//        wRIV.variance = Math.Pow(squaredVariance, 0.5);

//    return wRIV;
//}

//private double pValueFromPermutation(List<qVals> quantitativeValues, double totalIntensity, double realRatio)
//{
//    double pValue = 0;
//    int maxPermutations = 5000;
//    ConcurrentBag<double> permutedRatios = new ConcurrentBag<double>();

//    Parallel.For(0, maxPermutations, i =>
//    {
//        double someRatio = 0;
//        quantitativeValues.ForEach(q =>
//        {
//            IList<Component> combined = new List<Component>();
//            combined = q.light.Concat(q.heavy).ToList();
//            combined.Shuffle();
//            double numerator = (from s in combined.Take(q.light.Count()) select s.intensity_sum).Sum();
//            double denominator = (from s in combined.Skip(q.light.Count()).Take(q.heavy.Count()) select s.intensity_sum).Sum();

//            if (numerator == 0) numerator = numerator + 1000; //adding 1000 to deal with missing values
//            if (denominator == 0) denominator = denominator + 1000; //adding 1000 to deal with missing values
//            someRatio = someRatio + Math.Log(numerator / denominator, 2) * q.intensity / totalIntensity;
//        });
//        permutedRatios.Add(someRatio);
//    });

//    if (realRatio > 0)
//        pValue = (double)permutedRatios.Count(x => x > realRatio) / permutedRatios.Count();
//    else
//        pValue = (double)permutedRatios.Count(x => x < realRatio) / permutedRatios.Count();
//    return pValue;
//}

//public double bftAggIntensityValue(int b, int f, int t, bool light)
//{
//    List<bftIntensity> subList = new List<ProteoformSuiteInternal.bftIntensity>();

//    if (light)
//    {
//        if (b != -1)
//            subList = this.bftIntensityList.Where(bft => bft.biorep == b).ToList();
//        if (subList.Count != 0 )
//            if (f != -1)
//                subList = subList.Where(bft => bft.fraction == f).ToList();
//        if (subList.Count != 0)
//            if (t != -1)
//                subList = subList.Where(bft => bft.techrep == t).ToList();
//        if (subList.Count != 0)
//            subList = subList.Where(bft => bft.light == true).ToList();
//    }            
//    else
//    {
//        if (b != -1)
//            subList = this.bftIntensityList.Where(bft => bft.biorep == b).ToList();
//        if (subList.Count != 0)
//            if (f != -1)
//                subList = subList.Where(bft => bft.fraction == f).ToList();
//        if (subList.Count != 0)
//            if (t != -1)
//                subList = subList.Where(bft => bft.techrep == t).ToList();
//        if (subList.Count != 0)
//            subList = subList.Where(bft => bft.light == false).ToList();
//    }

//    if (subList.Count != 0)
//        return subList.Select(i => i.intensity).Sum();
//    else
//        return 0;
//}

//public double biorepAggIntensityValue(int b, bool light)
//{
//    List<biorepIntensity> subList = new List<biorepIntensity>();

//    if (light)
//    {
//        subList = this.biorepIntensityList.Where(proteoform => proteoform.biorep == b).ToList();
//        if (subList.Count != 0)
//            subList = subList.Where(proteoform => proteoform.light == true).ToList();
//    }
//    else
//    {
//        subList = this.biorepIntensityList.Where(proteoform => proteoform.biorep == b).ToList();
//        if (subList.Count != 0)
//            subList = subList.Where(proteoform => proteoform.light == false).ToList();
//    }

//    if (subList.Count != 0)
//        return subList.Select(i => i.intensity).Sum();
//    else
//        return 0;
//}

//public void make_bftList()
//{
//    this.bftIntensityList.Clear();
//    foreach (int b in this.lt_quant_components.Select(c=>c.input_file.biological_replicate).Distinct())
//    {
//        foreach (int f in this.lt_quant_components.Where(c => c.input_file.biological_replicate==b).Select(f=>f.input_file.fraction).Distinct())
//        {
//            foreach (int t in this.lt_quant_components.Where(c => c.input_file.biological_replicate == b && c.input_file.fraction==f).Select(t => t.input_file.technical_replicate).Distinct())
//            {
//                this.bftIntensityList.Add(new bftIntensity(true, b, f, t, this.lt_quant_components.Where(c => c.input_file.biological_replicate == b && c.input_file.fraction == f && c.input_file.technical_replicate == t).Select(i => i.intensity_sum).ToList().Sum()));
//            }
//        }

//    }
//    if (Lollipop.neucode_labeled)
//    {
//        foreach (int b in this.hv_quant_components.Select(c => c.input_file.biological_replicate).Distinct())
//        {
//            foreach (int f in this.hv_quant_components.Where(c => c.input_file.biological_replicate == b).Select(f => f.input_file.fraction).Distinct())
//            {
//                foreach (int t in this.hv_quant_components.Where(c => c.input_file.biological_replicate == b && c.input_file.fraction == f).Select(t => t.input_file.technical_replicate).Distinct())
//                {
//                    this.bftIntensityList.Add(new bftIntensity(false, b, f, t, this.hv_quant_components.Where(c => c.input_file.biological_replicate == b && c.input_file.fraction == f && c.input_file.technical_replicate == t).Select(i => i.intensity_sum).ToList().Sum()));
//                }
//            }

//        }
//    }
//}

//public double aggregated_observation_range // this should get deleted at some point. just for testing.
//{
//    get
//    {
//        if (aggregated_components.Count > 0)
//        {
//            List<Component> lights = new List<Component>();
//            if (Lollipop.neucode_labeled)
//            {
//                foreach (Component l in aggregated_components)                       
//                    lights.Add(((NeuCodePair)l).neuCodeLight);                       
//            }
//            else
//            {
//                foreach (Component l in aggregated_components)                     
//                    lights.Add(l);                      
//            }
//            List<double> masses = lights.Select(lt => lt.weighted_monoisotopic_mass).ToList();
//            return (masses.Max() - masses.Min()) / 1000000d * masses.Average();
//        }
//        else
//            return 0d;
//    }
//    set { }
//}
