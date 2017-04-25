using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    [Serializable]
    public class QuantitativeProteoformValues
    {

        #region Private Fields

        [NonSerialized]
        private ExperimentalProteoform _proteoform;

        #endregion Private Fields

        #region Public Properties

        public ExperimentalProteoform proteoform
        {
            get
            {
                return _proteoform;
            }

            set
            {
                _proteoform = value;
                accession = value.accession;
            }
        }
        public string accession { get; set; }
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

        #endregion Public Properties

        #region Public Constructor

        //Selecting numerator and denominator is not implemented
        public QuantitativeProteoformValues(ExperimentalProteoform eP)
        {
            eP.quant = this;
            proteoform = eP;
        }

        #endregion Public Constructor

        #region Public Methods

        public void determine_biorep_intensities_and_test_statistics(bool neucode_labeled, List<BiorepIntensity> biorepIntensityList, decimal bkgdAverageIntensity, decimal bkgdStDev, string numerator, string denominator, decimal sKnot)
        {
            //bkgdAverageIntensity is log base 2
            //bkgdStDev is log base 2

            //numerator and denominator not used yet b/c of the programming that would require.
            significant = false;
            lightBiorepIntensities = biorepIntensityList.Where(b => b.light).ToList();
            lightImputedIntensities = imputedIntensities(true, lightBiorepIntensities, bkgdAverageIntensity, bkgdStDev, SaveState.lollipop.ltConditionsBioReps);
            lightIntensitySum = (decimal)lightBiorepIntensities.Sum(i => i.intensity) + (decimal)lightImputedIntensities.Sum(i => i.intensity);
            List<BiorepIntensity> allLights = lightBiorepIntensities.Concat(lightImputedIntensities).ToList();

            List<BiorepIntensity> allHeavys = new List<BiorepIntensity>();
            if (neucode_labeled)
            {
                heavyBiorepIntensities = biorepIntensityList.Where(b => !b.light).ToList();
                heavyImputedIntensities = imputedIntensities(false, heavyBiorepIntensities, bkgdAverageIntensity, bkgdStDev, SaveState.lollipop.hvConditionsBioReps);
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

        #endregion Public Methods

    }
}
