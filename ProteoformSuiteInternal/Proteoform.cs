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
        {

        }

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
        private Component root;
        public List<Component> aggregated_components { get; set; } = new List<Component>();
        public List<Component> lt_quant_components { get; set; } = new List<Component>();
        public List<Component> hv_quant_components { get; set; } = new List<Component>();
        public bool accepted { get; set; } = true;
        public double agg_mass { get; set; } = 0;
        public double agg_intensity { get; set; } = 0;
        public double agg_rt { get; set; } = 0;
        public int observation_count
        {
            get { return aggregated_components.Count; }
        }
        public bool mass_shifted { get; set; } = false; //make sure in ET if shifting multiple peaks, not shifting same E > once. 
        public int light_observation_count// this should get deleted at some point. just for testing.
        {
            get { return lt_quant_components.Count; }
            set { }
        }
        public int heavy_observation_count// this should get deleted at some point. just for testing.
        {
            get { return lt_quant_components.Count; }
            set { }
        }
        public double aggregated_observation_range // this should get deleted at some point. just for testing.
        {
            get
            {
                if (aggregated_components.Count > 0)
                {
                    List<Component> lights = new List<Component>();
                    if (Lollipop.neucode_labeled)
                    {
                        foreach (Component l in aggregated_components)
                            lights.Add(((NeuCodePair)l).neuCodeLight);
                    }
                    else
                    {
                        foreach (Component l in aggregated_components)
                            lights.Add(l);
                    }
                    List<double> masses = lights.Select(lt => lt.weighted_monoisotopic_mass).ToList();
                    return (masses.Max() - masses.Min()) / 1000000d * masses.Average();
                }
                else
                    return 0d;
            }
            set { }
        }

        public ExperimentalProteoform(ExperimentalProteoform eP)
        {
            this.agg_intensity = eP.agg_intensity;
            this.agg_mass = eP.agg_mass;
            this.agg_rt = eP.agg_rt;
        }


        public ExperimentalProteoform(string accession, Component root, List<Component> candidate_observations, List<Component> quantitative_observations, bool is_target) : base(accession)
        {
            this.root = root;
            this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p, this.root)));
            this.calculate_properties();
            if (quantitative_observations.Count > 0)
            {
                this.lt_quant_components.AddRange(quantitative_observations.Where(r => this.includes(r, this, true)));
                this.light_observation_count = this.lt_quant_components.Count;
                if (Lollipop.neucode_labeled)
                {
                    this.hv_quant_components.AddRange(quantitative_observations.Where(r => this.includes(r, this, false)));
                    this.heavy_observation_count = this.hv_quant_components.Count;
                }
            }
            this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault();
        }

        public ExperimentalProteoform(string accession, ExperimentalProteoform temp, List<Component> candidate_observations, List<Component> quantitative_observations, bool is_target) : base(accession) //this is for first mass of aggregate components. uses a temporary component
        {
            Component root = new Component();
            NeuCodePair ncRoot = new NeuCodePair();
            if (Lollipop.neucode_labeled)
            {
                ((Component)ncRoot).attemptToSetWeightedMonoisotopic_mass(temp.agg_mass);
                ((Component)ncRoot).attemptToSetIntensity(temp.agg_intensity);
                ncRoot.rt_apex = temp.agg_rt;
                ncRoot.lysine_count = temp.lysine_count;

                this.root = ncRoot;
                this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p, this.root)));
                this.calculate_properties();
                if (quantitative_observations.Count > 0)
                {
                    this.lt_quant_components.AddRange(quantitative_observations.Where(r => this.includes(r, this, true)));
                    this.light_observation_count = this.lt_quant_components.Count;
                    if (Lollipop.neucode_labeled)
                    {
                        this.hv_quant_components.AddRange(quantitative_observations.Where(r => this.includes(r, this, false)));
                        this.heavy_observation_count = this.hv_quant_components.Count;
                    }

                }
                this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault(); //reset root to component with max intensity
            }
            else
            {
                root.attemptToSetWeightedMonoisotopic_mass(temp.agg_mass);
                root.attemptToSetIntensity(temp.agg_intensity);
                root.rt_apex = temp.agg_rt;

                this.root = root;
                this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p, this.root)));
                this.calculate_properties();
                if (quantitative_observations.Count > 0)
                {
                    this.lt_quant_components.AddRange(quantitative_observations.Where(r => this.includes(r, this, true)));
                    this.light_observation_count = this.lt_quant_components.Count;
                    if (Lollipop.neucode_labeled)
                    {
                        this.hv_quant_components.AddRange(quantitative_observations.Where(r => this.includes(r, this, false)));
                        this.heavy_observation_count = this.hv_quant_components.Count;
                    }

                }
                this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault(); //reset root to component with max intensity
            }
        }

        public ExperimentalProteoform(string accession, Component root, List<Component> candidate_observations, bool is_target) : base(accession)
        {
            this.root = root;
            this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p, this.root)));
            this.calculate_properties();
            this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault();
        }

        public ExperimentalProteoform(string accession, ExperimentalProteoform temp, List<Component> candidate_observations, bool is_target) : base(accession) //this is for first mass of aggregate components. uses a temporary component
        {
            Component root = new Component();
            NeuCodePair ncRoot = new NeuCodePair();
            if (Lollipop.neucode_labeled)
            {
                ((Component)ncRoot).attemptToSetWeightedMonoisotopic_mass(temp.agg_mass);
                ((Component)ncRoot).attemptToSetIntensity(temp.agg_intensity);
                ncRoot.rt_apex = temp.agg_rt;
                ncRoot.lysine_count = temp.lysine_count;

                this.root = ncRoot;
                this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p, this.root)));
                this.calculate_properties();
                this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault(); //reset root to component with max intensity
            }
            else
            {
                root.attemptToSetWeightedMonoisotopic_mass(temp.agg_mass);
                root.attemptToSetIntensity(temp.agg_intensity);
                root.rt_apex = temp.agg_rt;

                this.root = root;
                this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p, this.root)));
                this.calculate_properties();
                this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault(); //reset root to component with max intensity
            }
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

        //for reading in neucode EE pairs
        public ExperimentalProteoform(string accession, double agg_mass, int lysine_count, double agg_rt) : base(accession)
        {
            this.accession = accession;
            this.agg_mass = agg_mass;
            this.lysine_count = lysine_count;
            this.agg_rt = agg_rt;
        }


        //for Tests
        public ExperimentalProteoform(string accession) : base(accession)
        {
            this.aggregated_components = new List<Component>() { root };
            this.accession = accession;
        }

        private void calculate_properties()
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
            this.accepted = this.aggregated_components.Count >= Lollipop.min_agg_count 
                && this.aggregated_components.Select(c => c.input_file.biological_replicate).Distinct().ToList().Count >= Lollipop.min_num_bioreps;
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

        //Quantitative Class and Methods
        public class qVals
        {
            public double ratio { get; set; }
            public double intensity { get; set; }
            public double fraction { get; set; }
            public List<Component> light { get; set; }
            public List<Component> heavy { get; set; }
        }

        public weightedRatioIntensityVariance weightedRatioAndWeightedVariance(List<InputFile> inputFileList) //the inputFileList is a list of "quantitative" input files
        {
            List<qVals> quantitativeValues = new List<qVals>();
            weightedRatioIntensityVariance wRIV = new weightedRatioIntensityVariance();

            double squaredVariance = 0;

            inputFileList.ForEach(inFile =>
            {
                qVals q = new qVals();
                q.light = (from s in lt_quant_components where s.input_file == inFile select s).ToList();
                q.heavy = (from s in hv_quant_components where s.input_file == inFile select s).ToList();
                double numerator = (from s in lt_quant_components where s.input_file == inFile select s.intensity_sum).Sum();
                double denominator = (from s in hv_quant_components where s.input_file == inFile select s.intensity_sum).Sum();
                if (numerator == 0)
                    numerator = numerator + 1000;//adding 1000 to deal with missing values
                if (denominator == 0)
                    denominator = denominator + 1000;//adding 1000 to deal with missing values
                q.ratio = Math.Log(numerator / denominator, 2);
                q.intensity = numerator + denominator;

                if ((q.light.Count() + q.heavy.Count()) > 0)
                    quantitativeValues.Add(q);
            });

            wRIV.intensity = quantitativeValues.Sum(s => s.intensity);

            if (wRIV.intensity > 0)
            {
                quantitativeValues.ForEach(q =>
                {
                    wRIV.ratio = wRIV.ratio + q.ratio * q.intensity / wRIV.intensity;
                    q.fraction = (double)q.intensity / wRIV.intensity;
                });
                quantitativeValues.ForEach(q =>
                {
                    squaredVariance = squaredVariance + q.fraction * Math.Pow((q.ratio - wRIV.ratio), 2);
                });
                wRIV.pValue = pValueFromPermutation(quantitativeValues, wRIV.intensity, wRIV.ratio);
            }

            if (squaredVariance > 0)
                wRIV.variance = Math.Pow(squaredVariance, 0.5);

            return wRIV;
        }

        private double pValueFromPermutation(List<qVals> quantitativeValues, double totalIntensity, double realRatio)
        {
            double pValue = 0;
            int maxPermutations = 1000;
            ConcurrentBag<double> permutedRatios = new ConcurrentBag<double>();

            Parallel.For(0, maxPermutations, i =>
            {
                double someRatio = 0;
                quantitativeValues.ForEach(q =>
                {
                    IList<Component> combined = new List<Component>();
                    combined = q.light.Concat(q.heavy).ToList();
                    combined.Shuffle();
                    double numerator = (from s in combined.Take(q.light.Count()) select s.intensity_sum).Sum();
                    double denominator = (from s in combined.Skip(q.light.Count()).Take(q.heavy.Count()) select s.intensity_sum).Sum();

                    if (numerator == 0) numerator = numerator + 1000; //adding 1000 to deal with missing values
                    if (denominator == 0) denominator = denominator + 1000; //adding 1000 to deal with missing values
                    someRatio = someRatio + Math.Log(numerator / denominator, 2) * q.intensity / totalIntensity;
                });
                permutedRatios.Add(someRatio);
            });

            if (realRatio > 0)
                pValue = (double)permutedRatios.Count(x => x > realRatio) / permutedRatios.Count();
            else
                pValue = (double)permutedRatios.Count(x => x < realRatio) / permutedRatios.Count();
            return pValue;
        }
    }

    public class TheoreticalProteoform : Proteoform
    {
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
            match_theoreticals();
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
        }
    }

    public class weightedRatioIntensityVariance
    {
        public double ratio { get; set; } = 0;
        public double intensity { get; set; } = 0;
        public double variance { get; set; } = 0;
        public double pValue { get; set; } = 0;
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

