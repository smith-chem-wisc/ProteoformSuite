using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class QuantitativeProteoformValues
    {

        #region Public Properties

        public ExperimentalProteoform proteoform { get; set; }
        public string accession { get { return proteoform.accession; } }
        public List<BiorepIntensity> numeratorOriginalBiorepIntensities { get; set; }
        public List<BiorepIntensity> denominatorOriginalBiorepIntensities { get; set; }
        public List<BiorepIntensity> numeratorImputedIntensities { get; set; }
        public List<BiorepIntensity> denominatorImputedIntensities { get; set; }
        public Dictionary<Tuple<string,int>, BiorepIntensity> allIntensities { get; set; }
        public decimal numeratorIntensitySum { get; set; } = 0;
        public decimal denominatorIntensitySum { get; set; } = 0;
        public decimal intensitySum { get; set; } = 0;
        public decimal logFoldChange { get; set; } = 0;
        public decimal scatter { get; set; } = 0;
        public decimal pValue { get; set; } = 0;
        public bool significant { get; set; } = false;
        public decimal relative_difference { get; set; }
        public decimal correspondingAvgSortedRelDiff { get; set; }
        public decimal roughSignificanceFDR { get; set; } = 0;

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

        public void determine_biorep_intensities_and_test_statistics(List<BiorepIntensity> biorepIntensityList, Dictionary<string, List<int>> conditionBioReps, string numerator_condition, string denominator_condition, string induced_condition, decimal bkgdAverageIntensity, decimal bkgdStDev, decimal sKnot)
        {
            //bkgdAverageIntensity is log base 2
            //bkgdStDev is log base 2

            significant = false;
            numeratorOriginalBiorepIntensities = biorepIntensityList.Where(b => b.condition == numerator_condition).ToList();
            numeratorImputedIntensities = imputedIntensities(numeratorOriginalBiorepIntensities, bkgdAverageIntensity, bkgdStDev, numerator_condition, conditionBioReps[numerator_condition]);
            numeratorIntensitySum = (decimal)numeratorOriginalBiorepIntensities.Sum(i => i.intensity) + (decimal)numeratorImputedIntensities.Sum(i => i.intensity);
            List<BiorepIntensity> allNumeratorIntensities = numeratorOriginalBiorepIntensities.Concat(numeratorImputedIntensities).ToList();

            List<BiorepIntensity> allDenominatorIntensities = new List<BiorepIntensity>();
            denominatorOriginalBiorepIntensities = biorepIntensityList.Where(b => b.condition == denominator_condition).ToList();
            denominatorImputedIntensities = imputedIntensities(denominatorOriginalBiorepIntensities, bkgdAverageIntensity, bkgdStDev, denominator_condition, conditionBioReps[denominator_condition]);
            denominatorIntensitySum = (decimal)denominatorOriginalBiorepIntensities.Sum(i => i.intensity) + (decimal)denominatorImputedIntensities.Sum(i => i.intensity);
            allDenominatorIntensities = denominatorOriginalBiorepIntensities.Concat(denominatorImputedIntensities).ToList();

            allIntensities = allNumeratorIntensities.Concat(allDenominatorIntensities).ToDictionary(x => new Tuple<string, int>(x.condition, x.biorep), x => x);

            intensitySum = numeratorIntensitySum + denominatorIntensitySum;
            logFoldChange = (decimal)Math.Log((double)numeratorIntensitySum / (double)denominatorIntensitySum, 2);
            pValue = PValue(logFoldChange, allNumeratorIntensities, allDenominatorIntensities);

            // We are using linear intensities, like in Tusher et al. (2001).
            // This is a non-parametric test, and so it makes no assumptions about the incoming probability distribution, unlike a simple t-test.
            // Therefore, the right-skewed intensity distributions is okay for this test.
            scatter = StdDev(allNumeratorIntensities, allDenominatorIntensities);
            List<BiorepIntensity> induced = allIntensities.Where(kv => kv.Key.Item1 == induced_condition).Select(kv => kv.Value).ToList();
            List<BiorepIntensity> uninduced = allIntensities.Where(kv => kv.Key.Item1 != induced_condition).Select(kv => kv.Value).ToList();
            relative_difference = getSingleTestStatistic(induced, uninduced, scatter, sKnot);
        }

        #endregion Public Methods

        #region Imputation Methods

        /// <summary>
        /// Returns imputed intensities for a certain condition for biological replicates this proteoform was not observed in.
        /// </summary>
        /// <param name="observedBioreps"></param>
        /// <param name="bkgdAverageIntensity"></param>
        /// <param name="bkgdStDev"></param>
        /// <param name="condition"></param>
        /// <param name="bioreps"></param>
        /// <returns></returns>
        public static List<BiorepIntensity> imputedIntensities(List<BiorepIntensity> observedBioreps, decimal bkgdAverageIntensity, decimal bkgdStDev, string condition, List<int> bioreps)
        {
            //bkgdAverageIntensity is log base 2
            //bkgdStDev is log base 2

            return (
                from biorep in bioreps
                where !observedBioreps.Any(k => k.condition == condition && k.biorep == biorep)
                select add_biorep_intensity(bkgdAverageIntensity, bkgdStDev, biorep, condition))
                .ToList();
        }

        /// <summary>
        /// Computes an intensity from a background distribution of intensity I and standard deviation s.
        /// The standard normal distribution of mean 0 and variance 1 is computed from two random numbers distributed uniformly on (0, 1).
        /// Then, a number from that distribution, x, is used to calculate the imputed intensity: I + s * x
        /// </summary>
        /// <param name="bkgdAverageIntensity"></param>
        /// <param name="bkgdStDev"></param>
        /// <param name="biorep"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static BiorepIntensity add_biorep_intensity(decimal bkgdAverageIntensity, decimal bkgdStDev, int biorep, string key)
        {
            //bkgdAverageIntensity is coming in as a log 2 number
            //bkgdStDev is coming in as a log 2 number

            double u1 = ExtensionMethods.RandomNumber(); // these are uniform(0,1) random doubles
            double u2 = ExtensionMethods.RandomNumber();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); // random normal(0,1) -- normal(mean,variance)
            double intensity = Math.Pow(2, (double)bkgdAverageIntensity + (double)bkgdStDev * randStdNormal); // std dev is calculated for log intensities, so convert to linear after adding I + s * x
            return new BiorepIntensity(true, biorep, key, intensity); // random normal(mean,stdDev^2)
        }

        #endregion Imputation Methods

        #region Relative Difference Methods

        /// <summary>
        /// Calculates the pooled standard deviation across unlogged intensities for the two conditions for this proteoform.
        /// This is known as the "scatter s(i)" in the Tusher et al. paper.
        /// </summary>
        /// <param name="allInduced"></param>
        /// <param name="allUninduced"></param>
        /// <returns></returns>
        public decimal StdDev(IEnumerable<BiorepIntensity> allInduced, IEnumerable<BiorepIntensity> allUninduced)
        {
            int numerator_count = allInduced.Count();
            int denominator_count = allUninduced.Count();
            if ((numerator_count + denominator_count) == 2)
                return 1000000m;

            decimal a = (decimal)((1d / (double)numerator_count + 1d / (double)denominator_count) / ((double)numerator_count + (double)denominator_count - 2d));
            double avgNumerator = allInduced.Average(x => x.intensity);
            double avgDenominator = allUninduced.Average(x => x.intensity);
            decimal numeratorSumSquares = allInduced.Sum(n => (decimal)Math.Pow(n.intensity - avgNumerator, 2d));
            decimal denominatorSumSquares = allUninduced.Sum(d => (decimal)Math.Pow(d.intensity - avgDenominator, 2d));
            decimal stdev = (decimal)Math.Sqrt((double)((numeratorSumSquares + denominatorSumSquares) * a));
            return stdev;
        }

        /// <summary>
        /// This is the relative difference from Tusher, et al. (2001) using unlogged intensity values
        /// d(i) = { Average(measurement x from state I) - Average(measurement x from state U) } / { (pooled std dev from I and U) - s_knot }
        /// </summary>
        /// <param name="allInduced"></param>
        /// <param name="allUninduced"></param>
        /// <param name="pooledStdDev"></param>
        /// <param name="sKnot">
        /// A constant intended to "minimize the coefficient of variation"
        /// </param>
        /// <returns></returns>
        public decimal getSingleTestStatistic(List<BiorepIntensity> allInduced, List<BiorepIntensity> allUninduced, decimal pooledStdDev, decimal sKnot)
        {
            double t = (allInduced.Average(l => l.intensity) - allUninduced.Average(h => h.intensity)) / ((double)(pooledStdDev + sKnot));
            return (decimal)t;
        }

        #endregion Relative Difference Methods

        #region Fold Change Method

        public decimal PValue(decimal logFoldChange, List<BiorepIntensity> allNumerators, List<BiorepIntensity> allDenominators)
        {
            if (allNumerators.Count != allDenominators.Count)
                throw new ArgumentException("Error: Imputation has gone awry. Each biorep should have the same number of biorep intensities for NeuCode light and heavy at this point.");

            int maxRandomizations = 10000;
            ConcurrentBag<decimal> randomizedRatios = new ConcurrentBag<decimal>();

            Parallel.For(0, maxRandomizations, i =>
            {
                List<double> combined = allNumerators.Select(j => j.intensity).Concat(allDenominators.Select(j => j.intensity)).ToList();
                combined.Shuffle();
                double numerator = combined.Take(allNumerators.Count).Sum();
                double denominator = combined.Skip(allNumerators.Count).Take(allDenominators.Count).Sum();
                decimal someRatio = (decimal)Math.Log(numerator / denominator, 2);
                randomizedRatios.Add(someRatio);
            });

            decimal pValue = logFoldChange > 0 ?
                (decimal)(1M / maxRandomizations) + (decimal)randomizedRatios.Count(x => x > logFoldChange) / (decimal)randomizedRatios.Count : //adding a slight positive shift so that later logarithms don't produce fault
                (decimal)(1M / maxRandomizations) + (decimal)randomizedRatios.Count(x => x < logFoldChange) / (decimal)randomizedRatios.Count; //adding a slight positive shift so that later logarithms don't produce fault

            return pValue;
        }

        #endregion Fold Change Method

        #region Additional Relative Difference Method (Not from Tusher, et al. paper)

        /// <summary>
        /// Calculates a rough q-value for this proteoform by dividing the estimated number of pfs that pass given the balanced permutations by the real pfs that pass.
        /// The threshold for passing is drawn at the test statistic for this proteoform.
        /// To calculate the "estimated number of pfs that pass given balanced permutations," I multiply the proportion of passing permuations by the number of pfs.
        /// 
        /// This calculation is an extension of the relative difference method presented in the Tusher et al. paper.
        /// </summary>
        /// <param name="testStatistic"></param>
        /// <param name="permutedTestStatistics"></param>
        /// <param name="satisfactoryProteoformsCount"></param>
        /// <param name="sortedProteoformTestStatistics"></param>
        /// <returns></returns>
        public static decimal computeExperimentalProteoformFDR(decimal testStatistic, List<decimal> permutedTestStatistics, int satisfactoryProteoformsCount, List<decimal> sortedProteoformTestStatistics)
        {
            decimal minimumPositivePassingTestStatistic = Math.Abs(testStatistic);
            decimal minimumNegativePassingTestStatistic = -minimumPositivePassingTestStatistic;

            int totalFalsePermutedPassingValues = permutedTestStatistics.Count(v => v <= minimumNegativePassingTestStatistic && v <= 0 || minimumPositivePassingTestStatistic <= v && v >= 0);
            decimal averagePermutedPassing = (decimal)totalFalsePermutedPassingValues / (decimal)permutedTestStatistics.Count * (decimal)satisfactoryProteoformsCount;

            int totalRealPassing = sortedProteoformTestStatistics.Count(stat => stat <= minimumNegativePassingTestStatistic && stat <= 0 || minimumPositivePassingTestStatistic <= stat && stat >= 0);

            decimal fdr = averagePermutedPassing / (decimal)totalRealPassing; // real passing will always be above zero because this proteoform always passes
            return fdr;
        }

        #endregion Additional Relative Difference Method (Not from Tusher, et al. paper)

    }
}
