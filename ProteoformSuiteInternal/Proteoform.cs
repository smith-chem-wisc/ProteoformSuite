using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class Proteoform
    {
        public string accession { get; set; }
        public double modified_mass { get; set; }
        public int lysine_count { get; set; } = -1;
        public bool is_target { get; set; } = true;
        public bool is_decoy { get; set; } = false;
        public List<Proteoform> candidate_relatives { get; set; }
        public List<ProteoformRelation> relationships { get; set; } = new List<ProteoformRelation>();
        public ProteoformFamily family { get; set; }
        public PtmSet ptm_set { get; set; } = new PtmSet(new List<Ptm>());
        public string theoretical_reference_accession { get; set; }
        public string theoretical_reference_fragment { get; set; }
        public Proteoform theoretical_reference { get; set; }
        public GeneName gene_name { get; set; }

        public Proteoform(string accession, double modified_mass, int lysine_count, bool is_target)
        {
            this.accession = accession;
            this.modified_mass = modified_mass;
            this.lysine_count = lysine_count;
            this.is_target = is_target;
            this.is_decoy = !is_target;
        }

        public Proteoform(string accession)
        {
            this.accession = accession;
        }

        public List<Proteoform> get_connected_proteoforms()
        {
            return relationships.Where(r => r.accepted).SelectMany(r => r.connected_proteoforms).ToList();
        }

        public List<ExperimentalProteoform> identify_connected_experimentals()
        {
            List<ExperimentalProteoform> identified = new List<ExperimentalProteoform>();
            foreach (ProteoformRelation r in relationships.Where(r => r.accepted).Distinct().ToList())
            {
                ExperimentalProteoform e = r.connected_proteoforms.OfType<ExperimentalProteoform>().FirstOrDefault(p => p != this);
                if (e == null) continue; // Looking at an ET pair, expecting an EE pair

                double mass_tolerance = this.modified_mass / 1000000 * (double)Lollipop.mass_tolerance;
                int sign = Math.Sign(e.modified_mass - modified_mass);
                double deltaM = Math.Sign(r.peak_center_deltaM) < 0 ? r.peak_center_deltaM : sign * r.peak_center_deltaM; // give EE relations the correct sign, but don't switch negative ET relation deltaM's
                ModificationWithMass best_addition = null;
                ModificationWithMass best_loss = null;
                foreach (ModificationWithMass m in Lollipop.uniprotModificationTable.SelectMany(kv => kv.Value).OfType<ModificationWithMass>().ToList())
                {
                    if (deltaM >= m.monoisotopicMass - mass_tolerance && deltaM <= m.monoisotopicMass + mass_tolerance 
                        && (best_addition == null || Math.Abs(deltaM - m.monoisotopicMass) < Math.Abs(deltaM - best_addition.monoisotopicMass))
                        || best_addition != null && Math.Abs(deltaM - m.monoisotopicMass) == Math.Abs(deltaM - best_addition.monoisotopicMass) && m.modificationType == "Unlocalized")
                    {
                        best_addition = m;
                    }

                    if (this.ptm_set.ptm_combination.Select(ptm => ptm.modification).Contains(m) 
                        && deltaM >= -m.monoisotopicMass - mass_tolerance && deltaM <= -m.monoisotopicMass + mass_tolerance
                        && (best_loss == null || Math.Abs(deltaM - (-m.monoisotopicMass)) < Math.Abs(deltaM - (-best_loss.monoisotopicMass))))
                    {
                        best_loss = m;
                    }
                }

                // If they're the same and someone hasn't labeled 0 difference with a "ModificationWithMass", then label it null
                if (best_addition == null && best_loss == null && Math.Abs(r.peak_center_deltaM) <= mass_tolerance)
                {
                    lock (r) lock (e) assign_pf_identity(e, this, ptm_set, r, null);
                    identified.Add(e);
                }

                if (best_addition == null && best_loss == null)
                    continue;

                PtmSet with_mod_change = best_loss != null ?
                    new PtmSet(new List<Ptm>(this.ptm_set.ptm_combination.Except(this.ptm_set.ptm_combination.Where(ptm => ptm.modification.Equals(best_loss))))) :
                    new PtmSet(new List<Ptm>(this.ptm_set.ptm_combination).Concat(new Ptm[] { new Ptm(-1, best_addition) }).ToList());
                lock (r) lock (e) assign_pf_identity(e, this, with_mod_change, r, best_loss != null ? best_loss : best_addition);
                identified.Add(e);
            }
            return identified;
        }

        private void assign_pf_identity(ExperimentalProteoform e, Proteoform theoretical_reference, PtmSet set, ProteoformRelation r, ModificationWithMass m)
        {
            if (r.represented_modification == null)
            {
                r.represented_modification = m;
            }
            if (e.theoretical_reference == null)
            {
                e.theoretical_reference_accession = this.theoretical_reference_accession;
                e.theoretical_reference_fragment = this.theoretical_reference_fragment;
                e.theoretical_reference = this;
                e.ptm_set = set;
            }
            if (e.gene_name == null)
                e.gene_name = this.gene_name;
            else
                e.gene_name.gene_names.Concat(this.gene_name.gene_names);
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