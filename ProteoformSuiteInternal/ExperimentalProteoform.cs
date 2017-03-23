using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
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
        public List<BiorepIntensity> biorepIntensityList { get; set; } = new List<BiorepIntensity>(); //(bool light = true, int biorep, double intensity)
        public quantitativeValues quant { get; set; }
        public bool accepted { get; set; } = true;
        public double agg_mass { get; set; } = 0;
        public double agg_intensity { get; set; } = 0;
        public double agg_rt { get; set; } = 0;
        public bool mass_shifted { get; set; } = false; //make sure in ET if shifting multiple peaks, not shifting same E > once. 
        public int observation_count { get { return aggregated_components.Count; } }
        public int light_observation_count { get { return lt_quant_components.Count; } }
        public int heavy_observation_count {  get { return hv_quant_components.Count; } }


        // CONTRUCTORS
        public ExperimentalProteoform(string accession, Component root, List<Component> candidate_observations, bool is_target) : base(accession)
        {
            quant = new quantitativeValues(this);
            this.root = root;
            this.aggregated_components.AddRange(candidate_observations.Where(p => this.includes(p, this.root)));
            this.calculate_properties();
            this.root = this.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault();
        }

        public ExperimentalProteoform(string accession, Component root, bool is_target) : base(accession)
        {
            quant = new quantitativeValues(this);
            this.root = root;
            this.is_target = is_target;
        }

        public ExperimentalProteoform(string accession, ExperimentalProteoform temp, List<Component> candidate_observations, bool is_target, bool neucode_labeled) : base(accession) //this is for first mass of aggregate components. uses a temporary component
        {
            Component root = new Component();
            NeuCodePair ncRoot = new NeuCodePair();
            if (neucode_labeled)
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


        // COPYING CONSTRUCTOR
        public ExperimentalProteoform(ExperimentalProteoform eP)
            : base(eP.accession, eP.modified_mass, eP.lysine_count, eP.is_target)
        {
            copy_aggregate(eP);
            quant = new quantitativeValues(this);
        }

        private void copy_aggregate(ExperimentalProteoform e)
        {
            this.root = e.root;
            //doesn't copy quant on purpose
            this.agg_intensity = e.agg_intensity;
            this.agg_mass = e.agg_mass;
            this.modified_mass = e.modified_mass;
            this.agg_rt = e.agg_rt;
            this.lysine_count = e.lysine_count;
            this.accepted = e.accepted;
            this.mass_shifted = e.mass_shifted;
            this.is_target = e.is_target;
            this.is_decoy = e.is_decoy;
            this.family = e.family;
            this.aggregated_components = new List<Component>(e.aggregated_components);
            this.lt_quant_components = new List<Component>(e.lt_quant_components);
            this.lt_verification_components = new List<Component>(e.lt_verification_components);
            this.hv_quant_components = new List<Component>(e.hv_quant_components);
            this.hv_verification_components = new List<Component>(e.hv_verification_components);
            this.biorepIntensityList = new List<BiorepIntensity>(e.biorepIntensityList);
        }


        // AGGREGATION METHODS
        public void aggregate()
        {
            ExperimentalProteoform temp_pf = new ExperimentalProteoform("tbd", this.root, new List<Component>(Lollipop.remaining_components), true); //first pass returns temporary proteoform
            ExperimentalProteoform new_pf = new ExperimentalProteoform("tbd", temp_pf, new List<Component>(Lollipop.remaining_components), true, Lollipop.neucode_labeled); //second pass uses temporary protoeform from first pass.
            copy_aggregate(new_pf); //doesn't copy quant on purpose
            this.root = temp_pf.root; //maintain the original component root
        }

        public void verify()
        {
            foreach (Component c in Lollipop.remaining_verification_components)
            {
                if (this.includes_neucode_component(c, this, true))
                    lt_verification_components.Add(c);
                if (Lollipop.neucode_labeled && this.includes_neucode_component(c, this, false))
                    hv_verification_components.Add(c);
            }
        }

        public void assign_quantitative_components()
        {
            foreach (Component c in Lollipop.remaining_quantification_components)
            {
                if (this.includes_neucode_component(c, this, true)) lt_quant_components.Add(c);
                if (this.includes_neucode_component(c, this, false)) hv_quant_components.Add(c);
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
            this.accepted = this.aggregated_components.Count() >= Lollipop.min_agg_count;
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

        public bool includes_neucode_component(Component candidate, ExperimentalProteoform root, bool light)
        {
            double corrected_mass = light ? root.agg_mass :
                root.agg_mass + root.lysine_count * Lollipop.NEUCODE_LYSINE_MASS_SHIFT;
            bool does_include = tolerable_rt(candidate, root.agg_rt) && tolerable_mass(candidate, corrected_mass);
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
        public List<BiorepIntensity> make_biorepIntensityList<T>(List<T> lt_quant_components, List<T> hv_quant_components, IEnumerable<string> ltConditionStrings, IEnumerable<string> hvConditionStrings)
            where T : IBiorepable
        {
            List<BiorepIntensity> biorepIntensityList = new List<BiorepIntensity>();
            //foreach (string condition in Lollipop.ltConditionsBioReps.Keys)
            foreach (string condition in ltConditionStrings)
            {
                foreach (int b in lt_quant_components.Where(c => c.input_file.lt_condition == condition).Select(c => c.input_file.biological_replicate).Distinct())
                {
                    biorepIntensityList.Add(new BiorepIntensity(true, false, b, condition, lt_quant_components.Where(c => c.input_file.biological_replicate == b).Sum(i => i.intensity_sum)));
                }
            }
            if (Lollipop.neucode_labeled)
            {
                //foreach (string condition in Lollipop.hvConditionsBioReps.Keys)
                foreach (string condition in hvConditionStrings)
                {
                    foreach (int b in hv_quant_components.Where(c => c.input_file.hv_condition == condition).Select(c => c.input_file.biological_replicate).Distinct())
                    {
                        biorepIntensityList.Add(new BiorepIntensity(false, false, b, condition, hv_quant_components.Where(c => c.input_file.biological_replicate == b).Sum(i => i.intensity_sum)));
                    }
                }
            }
            this.biorepIntensityList = biorepIntensityList;
            return biorepIntensityList;
        }

        public class quantitativeValues
        {
            public string accession { get { return proteoform.accession; } }
            public List<BiorepIntensity> lightBiorepIntensities { get; set; }
            public List<BiorepIntensity> heavyBiorepIntensities { get; set; }
            public List<BiorepIntensity> lightImputedIntensities { get; set; }
            public List<BiorepIntensity> heavyImputedIntensities { get; set; }
            public decimal lightIntensitySum { get; set; } = 0;
            public decimal heavyIntensitySum { get; set; } = 0;
            public decimal intensitySum { get; set; } = 0;
            public decimal logFoldChange { get; set; } = 0;
            public decimal variance { get; set; } = 0;
            public decimal pValue { get; set; } = 0;
            public bool significant { get; set; } = false;
            public decimal testStatistic { get; set; }
            public List<decimal> permutedTestStatistics { get; set; }
            public decimal FDR { get; set; } = 0;
            public ExperimentalProteoform proteoform { get; set; }

            //Selecting numerator and denominator is not implemented
            public quantitativeValues(ExperimentalProteoform eP)
            {
                eP.quant = this;
                proteoform = eP;
            }

            public void determine_biorep_intensities_and_test_statistics(bool neucode_labeled, List<BiorepIntensity> biorepIntensityList, decimal bkgdAverageIntensity, decimal bkgdStDev, string numerator, string denominator, decimal sKnot)
            {
                //bkgdAverageIntensity is log base 2
                //bkgdStDev is log base 2

                //numerator and denominator not used yet b/c of the programming that would require.
                significant = false;
                lightBiorepIntensities = biorepIntensityList.Where(b => b.light).ToList();
                lightImputedIntensities = imputedIntensities(true, lightBiorepIntensities, bkgdAverageIntensity, bkgdStDev, Lollipop.ltConditionsBioReps);
                lightIntensitySum = (decimal)lightBiorepIntensities.Sum(i => i.intensity) + (decimal)lightImputedIntensities.Sum(i => i.intensity);
                List<BiorepIntensity> allLights = lightBiorepIntensities.Concat(lightImputedIntensities).ToList();

                List<BiorepIntensity> allHeavys = new List<BiorepIntensity>();
                if (neucode_labeled)
                {
                    heavyBiorepIntensities = biorepIntensityList.Where(b => !b.light).ToList();
                    heavyImputedIntensities = imputedIntensities(false, heavyBiorepIntensities, bkgdAverageIntensity, bkgdStDev, Lollipop.hvConditionsBioReps);
                    heavyIntensitySum = (decimal)heavyBiorepIntensities.Sum(i => i.intensity) + (decimal)heavyImputedIntensities.Sum(i => i.intensity);
                    allHeavys = heavyBiorepIntensities.Concat(heavyImputedIntensities).ToList();
                }

                intensitySum = lightIntensitySum + heavyIntensitySum;
                logFoldChange = (decimal)Math.Log((double)lightIntensitySum / (double)heavyIntensitySum, 2); // Will get divide by zero error if not neuCode labeled, right? -AC
                variance = Variance(logFoldChange, allLights, allHeavys);
                pValue = PValue(logFoldChange, allLights, allHeavys);
                decimal proteinLevelStdDev = getProteinLevelStdDev(allLights, allHeavys); //this is log2 bases
                testStatistic = getSingleTestStatistic(allLights, allHeavys, proteinLevelStdDev, sKnot);
                permutedTestStatistics = getPermutedTestStatistics(allLights, allHeavys, proteinLevelStdDev, sKnot);
            }

            public static decimal computeExperimentalProteoformFDR(decimal testStatistic, List<List<decimal>> permutedTestStatistics, int satisfactoryProteoformsCount, List<decimal> sortedProteoformTestStatistics)
            {
                decimal minimumPositivePassingTestStatistic = Math.Abs(testStatistic);
                decimal minimumNegativePassingTestStatistic = -minimumPositivePassingTestStatistic;

                int totalFalsePermutedPositiveValues = 0;
                int totalFalsePermutedNegativeValues = 0;

                foreach (List<decimal> pts in permutedTestStatistics)
                {
                    totalFalsePermutedPositiveValues += pts.Count(p => p >= minimumPositivePassingTestStatistic);
                    totalFalsePermutedNegativeValues += pts.Count(p => p <= minimumNegativePassingTestStatistic);
                }

                decimal avergePermuted = (decimal)(totalFalsePermutedPositiveValues + totalFalsePermutedNegativeValues) / (decimal)satisfactoryProteoformsCount;
                return avergePermuted / ((decimal)(sortedProteoformTestStatistics.Count(s => s >= minimumPositivePassingTestStatistic) + sortedProteoformTestStatistics.Count(s => s <= minimumNegativePassingTestStatistic)));
            }

            public static List<BiorepIntensity> imputedIntensities(bool light, List<BiorepIntensity> observedBioreps, decimal bkgdAverageIntensity, decimal bkgdStDev, Dictionary<string, List<int>> observedConditionsBioreps)
            {
                //bkgdAverageIntensity is log base 2
                //bkgdStDev is log base 2

                List<BiorepIntensity> imputedBioreps = new List<BiorepIntensity>();
                foreach (KeyValuePair<string, List<int>> entry in observedConditionsBioreps)//keys are conditions and values are bioreps.
                {
                    foreach (int biorep in entry.Value)
                    {
                        if (!observedBioreps.Where(l => l.light == light).Select(k => k.condition).Contains(entry.Key)) // no bioreps observed from this conditon at all
                            imputedBioreps.Add(add_biorep_intensity(bkgdAverageIntensity, bkgdStDev, biorep, entry.Key, light));
                        else if (!observedBioreps.Where(l => l.condition == entry.Key && l.light == light).Select(b => b.biorep).Contains(biorep)) //this condtion was observed but this biorep was not
                            imputedBioreps.Add(add_biorep_intensity(bkgdAverageIntensity, bkgdStDev, biorep, entry.Key, light));
                    }
                }
                return imputedBioreps;
            }

            public static BiorepIntensity add_biorep_intensity(decimal bkgdAverageIntensity, decimal bkgdStDev, int biorep, string key, bool light)
            {
                //bkgdAverageIntensity is coming in as a log 2 number
                //bkgdStDev is coming in as a log 2 number

                double u1 = ExtensionMethods.RandomNumber(); //these are uniform(0,1) random doubles
                double u2 = ExtensionMethods.RandomNumber();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                double intensity = Math.Pow(2, (double)bkgdAverageIntensity) + (Math.Pow(2, (double)bkgdStDev) * randStdNormal);
                return (new BiorepIntensity(light, true, biorep, key, intensity));//random normal(mean,stdDev^2)
            }

            public decimal Variance(decimal logFoldChange, List<BiorepIntensity> allLights, List<BiorepIntensity> allHeavies)
            {
                decimal squaredVariance = 0;
                foreach (int biorep in allLights.Select(b => b.biorep).ToList())
                {
                    List<BiorepIntensity> lights_in_biorep = allLights.Where(b => b.biorep == biorep).ToList();
                    List<BiorepIntensity> heavies_in_biorep = allHeavies.Where(b => b.biorep == biorep).ToList();

                    if (lights_in_biorep.Count != heavies_in_biorep.Count)
                        throw new ArgumentException("Error: Imputation has gone awry. Each biorep should have the same number of biorep intensities for NeuCode light and heavy at this point.");

                    decimal logRepRatio = (decimal)Math.Log(
                            (double)(((decimal)lights_in_biorep.Sum(i => i.intensity)) /
                            ((decimal)heavies_in_biorep.Sum(i => i.intensity)))
                            , 2);
                    squaredVariance += (decimal)Math.Pow(((double)logRepRatio - (double)logFoldChange), 2);
                }
                return (decimal)Math.Pow((double)squaredVariance, 0.5);
            }

            public decimal getProteinLevelStdDev(List<BiorepIntensity> allLights, List<BiorepIntensity> allHeavys)
            {
                if ((allLights.Count + allHeavys.Count) == 2)
                    return 1000000m;

                decimal a = (decimal)((1d / (double)allLights.Count + 1d / (double)allHeavys.Count) / ((double)allLights.Count + (double)allHeavys.Count - 2d));
                double log2LightAvg = Math.Log(allLights.Average(l => l.intensity), 2);
                double log2HeavyAvg = Math.Log(allHeavys.Average(l => l.intensity), 2);
                decimal lightSumSquares = allLights.Sum(l => (decimal)Math.Pow(Math.Log(l.intensity, 2) - log2LightAvg, 2d));
                decimal heavySumSquares = allHeavys.Sum(h => (decimal)Math.Pow(Math.Log(h.intensity, 2) - log2HeavyAvg, 2d));
                decimal stdev = (decimal)Math.Sqrt((double)((lightSumSquares + heavySumSquares) * a));
                return stdev;
            }

            public decimal getSingleTestStatistic(List<BiorepIntensity> allLights, List<BiorepIntensity> allHeavys, decimal proteinLevelStdDev, decimal sKnot)
            {
                double t = (Math.Log(allLights.Average(l => l.intensity), 2) - Math.Log(allHeavys.Average(h => h.intensity), 2)) / ((double)(proteinLevelStdDev + sKnot));
                return (decimal)t;
            }

            public List<decimal> getPermutedTestStatistics(List<BiorepIntensity> allLights, List<BiorepIntensity> allHeavys, decimal protproteinLevelStdDevein, decimal sKnot)
            {
                List<decimal> pst = new List<decimal>();
                int ltCount = allLights.Count;
                int hvCount = allHeavys.Count;
                List<int> arr = Enumerable.Range(0, ltCount + hvCount).ToList();
                var result = ExtensionMethods.Combinations(arr, ltCount);

                List<BiorepIntensity> allBiorepIntensities = new List<BiorepIntensity>(allLights.Concat(allHeavys));

                int last = ltCount;
                if (ltCount != hvCount) // This shouldn't happen because imputation forces these lists to be the same length
                {
                    last += hvCount;
                    throw new ArgumentException("Error: Imputation has gone awry. Each biorep should have the same number of biorep intensities for NeuCode light and heavy at this point.");
                }

                for (int i = 0; i < last; i++)
                {
                    List<BiorepIntensity> lightlist = new List<BiorepIntensity>();
                    List<BiorepIntensity> heavylist = new List<BiorepIntensity>();
                    foreach (int index in result.ElementAt(i))
                        lightlist.Add(allBiorepIntensities[index]);
                    heavylist = allBiorepIntensities.Except(lightlist).ToList();
                    pst.Add(getSingleTestStatistic(lightlist, heavylist, protproteinLevelStdDevein, sKnot)); //adding the test statistic for each combo
                }
                return pst;
            }

            public decimal PValue(decimal logFoldChange, List<BiorepIntensity> allLights, List<BiorepIntensity> allHeavies)
            {
                if (allLights.Count != allHeavies.Count)
                    throw new ArgumentException("Error: Imputation has gone awry. Each biorep should have the same number of biorep intensities for NeuCode light and heavy at this point.");

                int maxPermutations = 10000;
                ConcurrentBag<decimal> permutedRatios = new ConcurrentBag<decimal>();

                Parallel.For(0, maxPermutations, i =>
                {
                    List<double> combined = allLights.Select(j => j.intensity).Concat(allHeavies.Select(j => j.intensity)).ToList();
                    combined.Shuffle();
                    double numerator = combined.Take(allLights.Count).Sum();
                    double denominator = combined.Skip(allLights.Count).Take(allHeavies.Count).Sum();
                    decimal someRatio = (decimal)Math.Log(numerator / denominator, 2);
                    permutedRatios.Add(someRatio);
                });

                decimal pValue = logFoldChange > 0 ?
                    (decimal)(1M / maxPermutations) + (decimal)permutedRatios.Count(x => x > logFoldChange) / (decimal)permutedRatios.Count : //adding a slight positive shift so that later logarithms don't produce fault
                    (decimal)(1M / maxPermutations) + (decimal)permutedRatios.Count(x => x < logFoldChange) / (decimal)permutedRatios.Count; //adding a slight positive shift so that later logarithms don't produce fault

                return pValue;
            }
        }

        // OTHER METHODS
        public void shift_masses(int shift, bool neucode_labeled)
        {
            if (neucode_labeled)
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
    }
}
