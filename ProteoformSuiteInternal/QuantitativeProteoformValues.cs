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
        public List<BiorepIntensity> numeratorBiorepIntensities { get; set; }
        public List<BiorepIntensity> denominatorBiorepIntensities { get; set; }
        public List<BiorepIntensity> numeratorImputedIntensities { get; set; }
        public List<BiorepIntensity> denominatorImputedIntensities { get; set; }
        public decimal numeratorIntensitySum { get; set; } = 0;
        public decimal denominatorIntensitySum { get; set; } = 0;
        public decimal intensitySum { get; set; } = 0;
        public decimal logFoldChange { get; set; } = 0;
        public decimal variance { get; set; } = 0;
        public decimal scatter { get; set; } = 0;
        public decimal pValue { get; set; } = 0;
        public bool significant { get; set; } = false;
        public decimal testStatistic { get; set; }
        public List<decimal> permutedTestStatistics { get; set; } // Balanced permutation test statistics
        public decimal averagePermutedTestStatistic { get; set; } // Average for balanced permutations
        public decimal correspondingAveragePermutedTestStatistic { get; set; } // Corresponding value from Tusher plot
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

            significant = false;
            numeratorBiorepIntensities = biorepIntensityList.Where(b => b.condition == SaveState.lollipop.numerator_condition).ToList();
            numeratorImputedIntensities = imputedIntensities(numeratorBiorepIntensities, bkgdAverageIntensity, bkgdStDev, SaveState.lollipop.numerator_condition, SaveState.lollipop.conditionsBioReps[SaveState.lollipop.numerator_condition]);
            numeratorIntensitySum = (decimal)numeratorBiorepIntensities.Sum(i => i.intensity) + (decimal)numeratorImputedIntensities.Sum(i => i.intensity);
            List<BiorepIntensity> allNumeratorIntensities = numeratorBiorepIntensities.Concat(numeratorImputedIntensities).ToList();

            List<BiorepIntensity> allDenominatorIntensities = new List<BiorepIntensity>();
            denominatorBiorepIntensities = biorepIntensityList.Where(b => b.condition == SaveState.lollipop.denominator_condition).ToList();
            denominatorImputedIntensities = imputedIntensities(denominatorBiorepIntensities, bkgdAverageIntensity, bkgdStDev, SaveState.lollipop.denominator_condition, SaveState.lollipop.conditionsBioReps[SaveState.lollipop.denominator_condition]);
            denominatorIntensitySum = (decimal)denominatorBiorepIntensities.Sum(i => i.intensity) + (decimal)denominatorImputedIntensities.Sum(i => i.intensity);
            allDenominatorIntensities = denominatorBiorepIntensities.Concat(denominatorImputedIntensities).ToList();

            intensitySum = numeratorIntensitySum + denominatorIntensitySum;
            logFoldChange = (decimal)Math.Log((double)numeratorIntensitySum / (double)denominatorIntensitySum, 2);
            variance = Variance(logFoldChange, allNumeratorIntensities, allDenominatorIntensities);
            pValue = PValue(logFoldChange, allNumeratorIntensities, allDenominatorIntensities);
            scatter = StdDev(allNumeratorIntensities, allDenominatorIntensities); //this is log2 bases
            testStatistic = getSingleTestStatistic(allNumeratorIntensities, allDenominatorIntensities, scatter, sKnot);
            permutedTestStatistics = getBalancedPermutedTestStatistics(allNumeratorIntensities, allDenominatorIntensities, scatter, sKnot);
            averagePermutedTestStatistic = permutedTestStatistics.Average();
        }

        /// <summary>
        /// Calculates a q-value for this proteoform by dividing the estimated number of pfs that pass given the balanced permutations by the real pfs that pass.
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

            int totalFalsePermutedPassingValues = permutedTestStatistics.Count(v => v <= minimumNegativePassingTestStatistic || minimumPositivePassingTestStatistic <= v);
            decimal averagePermutedPassing = (decimal)totalFalsePermutedPassingValues / (decimal)permutedTestStatistics.Count * (decimal)satisfactoryProteoformsCount;

            int totalRealPassing = sortedProteoformTestStatistics.Count(stat => stat <= minimumNegativePassingTestStatistic || minimumPositivePassingTestStatistic <= stat);

            decimal fdr = averagePermutedPassing / (decimal)totalRealPassing; // real passing will always be above zero because this proteoform always passes
            return fdr;
        }

        public static List<BiorepIntensity> imputedIntensities(List<BiorepIntensity> observedBioreps, decimal bkgdAverageIntensity, decimal bkgdStDev, string condition, List<int> bioreps)
        {
            //bkgdAverageIntensity is log base 2
            //bkgdStDev is log base 2

            return (
                from biorep in bioreps
                where !observedBioreps.Select(k => k.condition).Contains(condition) || // no bioreps observed from this conditon at all 
                    !observedBioreps.Where(l => l.condition == condition).Select(b => b.biorep).Contains(biorep) // OR this condtion was observed but this biorep was not
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

            double u1 = ExtensionMethods.RandomNumber(); //these are uniform(0,1) random doubles
            double u2 = ExtensionMethods.RandomNumber();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double intensity = Math.Pow(2, (double)bkgdAverageIntensity) + (Math.Pow(2, (double)bkgdStDev) * randStdNormal);
            return new BiorepIntensity(true, biorep, key, intensity);//random normal(mean,stdDev^2)
        }

        public decimal Variance(decimal logFoldChange, List<BiorepIntensity> allNumerators, List<BiorepIntensity> allDenominators)
        {
            decimal squaredVariance = 0;
            foreach (int biorep in allNumerators.Select(b => b.biorep).ToList())
            {
                List<BiorepIntensity> numerators_in_biorep = allNumerators.Where(b => b.biorep == biorep).ToList();
                List<BiorepIntensity> denominators_in_biorep = allDenominators.Where(b => b.biorep == biorep).ToList();

                if (numerators_in_biorep.Count != denominators_in_biorep.Count)
                    throw new ArgumentException("Error: Imputation has gone awry. Each biorep should have the same number of biorep intensities for the numerator and denominator at this point.");

                decimal logRepRatio = (decimal)Math.Log(
                        (double)(((decimal)numerators_in_biorep.Sum(i => i.intensity)) /
                        ((decimal)denominators_in_biorep.Sum(i => i.intensity)))
                        , 2);

                squaredVariance += (decimal)Math.Pow(((double)logRepRatio - (double)logFoldChange), 2);
            }
            return (decimal)Math.Pow((double)squaredVariance, 0.5);
        }

        /// <summary>
        /// Calculates the pooled standard deviation across the two conditions for this proteoform.
        /// This is known as the "scatter s(i)" in the Tusher et al. paper.
        /// </summary>
        /// <param name="allNumerators"></param>
        /// <param name="allDenominators"></param>
        /// <returns></returns>
        public decimal StdDev(List<BiorepIntensity> allNumerators, List<BiorepIntensity> allDenominators)
        {
            if ((allNumerators.Count + allDenominators.Count) == 2)
                return 1000000m;

            decimal a = (decimal)((1d / (double)allNumerators.Count + 1d / (double)allDenominators.Count) / ((double)allNumerators.Count + (double)allDenominators.Count - 2d));
            double log2NumeratorAvg = Math.Log(allNumerators.Average(l => l.intensity), 2);
            double log2DenominatorAvg = Math.Log(allDenominators.Average(l => l.intensity), 2);
            decimal numeratorSumSquares = allNumerators.Sum(l => (decimal)Math.Pow(Math.Log(l.intensity, 2) - log2NumeratorAvg, 2d));
            decimal denominatorSumSquares = allDenominators.Sum(h => (decimal)Math.Pow(Math.Log(h.intensity, 2) - log2DenominatorAvg, 2d));
            decimal stdev = (decimal)Math.Sqrt((double)((numeratorSumSquares + denominatorSumSquares) * a));
            return stdev;
        }

        /// <summary>
        /// This is the relative difference from Tusher, et al. (2001)
        /// d(i) = { Average(measurement x from state I) - Average(measurement x from state U) } / { (pooled std dev from I and U) - s_knot }
        /// </summary>
        /// <param name="allNumerators"></param>
        /// <param name="allDenominators"></param>
        /// <param name="proteinLevelStdDev"></param>
        /// <param name="sKnot">
        /// A constant intended to "minimize the coefficient of variation"
        /// </param>
        /// <returns></returns>
        public decimal getSingleTestStatistic(List<BiorepIntensity> allNumerators, List<BiorepIntensity> allDenominators, decimal proteinLevelStdDev, decimal sKnot)
        {
            double t = (Math.Log(allNumerators.Average(l => l.intensity), 2) - Math.Log(allDenominators.Average(h => h.intensity), 2)) / ((double)(proteinLevelStdDev + sKnot));
            return (decimal)t;
        }

        /// <summary>
        /// Gets the test statistics for all balanced permutations of the intensity sums for this proteoform. 
        /// A balanced permutation has n/2 (integer division) values from the original set.
        /// Because positional permutations yield the same test statistic, only unique sets of permuted values are considered.
        /// 
        /// Example 1 (triples):
        /// Normal: 1, 2, 3 | Stress: 4, 5, 6
        /// Nine balanced permutations for normal (technically these are "nearly balanced")
        /// *4*, 2, 3
        /// *5*, 2, 3
        /// *6*, 2, 3
        /// 1, *4*, 3
        /// 1, *5*, 3
        /// 1, *6*, 3
        /// 1, 2, *4*
        /// 1, 2, *5*
        /// 1, 2, *6*
        /// 
        /// Example 2 (quadruples, like in Tusher et al.):
        /// Normal: 1, 2, 3, 4 | Stress: 5, 6, 7, 8
        /// Thirty-six balanced permutations for normal
        /// *5*, *6*, 3, 4 ;; 1, *5*, *6*, 4 ;; 1, 2, *5*, *6* ;; *5*, 2, *6*, 4 ;; *5*, 2, 3 *6* ;; 1, *5*, 3, *6*
        /// *5*, *7*, 3, 4 ;; 1, *5*, *7*, 4 ;; 1, 2, *5*, *7* ;; *5*, 2, *7*, 4 ;; *5*, 2, 3 *7* ;; 1, *5*, 3, *7*
        /// *5*, *8*, 3, 4 ;; 1, *5*, *8*, 4 ;; 1, 2, *5*, *8* ;; *5*, 2, *8*, 4 ;; *5*, 2, 3 *8* ;; 1, *5*, 3, *8*
        /// *6*, *7*, 3, 4 ;; 1, *6*, *7*, 4 ;; 1, 2, *6*, *7* ;; *6*, 2, *7*, 4 ;; *6*, 2, 3 *7* ;; 1, *6*, 3, *7*
        /// *6*, *8*, 3, 4 ;; 1, *6*, *8*, 4 ;; 1, 2, *6*, *8* ;; *6*, 2, *8*, 4 ;; *6*, 2, 3 *8* ;; 1, *6*, 3, *8*
        /// *7*, *8*, 3, 4 ;; 1, *7*, *8*, 4 ;; 1, 2, *7*, *8* ;; *7*, 2, *8*, 4 ;; *7*, 2, 3 *8* ;; 1, *7*, 3, *8*
        /// </summary>
        /// <param name="allNumerators"></param>
        /// <param name="allDenominators"></param>
        /// <param name="protproteinLevelStdDevein"></param>
        /// <param name="sKnot"></param>
        /// <returns></returns>
        public List<decimal> getBalancedPermutedTestStatistics(List<BiorepIntensity> allNumerators, List<BiorepIntensity> allDenominators, decimal protproteinLevelStdDevein, decimal sKnot)
        {
            if (allNumerators.Count != allDenominators.Count) // This shouldn't happen because imputation forces these lists to be the same length
                throw new ArgumentException("Error: Imputation has gone awry. Each biorep should have the same number of biorep intensities for the numerator and denominator at this point.");

            List<BiorepIntensity> allBiorepIntensities = new List<BiorepIntensity>(allNumerators.Concat(allDenominators));
            List<int> arr = Enumerable.Range(0, allBiorepIntensities.Count).ToList();
            List<HashSet<int>> permutations = new List<HashSet<int>>(ExtensionMethods.Combinations(arr, allNumerators.Count).Select(list => new HashSet<int>(list))); // using hash sets gets rid of positional permuations, even though it seems Combinations doesn't produce those
            List<HashSet<int>> balanced_permutations = permutations.Where(p => p.Count(i => i >= allNumerators.Count) == allNumerators.Count / 2).ToList(); // integer division is intended with (allNumerators.Count / 2) to check the number of denominator entries
            List<decimal> balanced_permuted_test_statistics = new List<decimal>();
            foreach (var permuation in balanced_permutations)
            {
                List<BiorepIntensity> numerators = permuation.Select(i => allBiorepIntensities[i]).ToList();
                List<BiorepIntensity> denominators = allBiorepIntensities.Except(numerators).ToList();
                balanced_permuted_test_statistics.Add(getSingleTestStatistic(numerators, denominators, protproteinLevelStdDevein, sKnot));
            }
            return balanced_permuted_test_statistics;
        }

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

        #endregion Public Methods

    }
}
