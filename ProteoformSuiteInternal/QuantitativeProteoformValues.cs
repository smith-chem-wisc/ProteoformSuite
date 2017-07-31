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
        public decimal logFoldChange { get; set; } = 0; // rough value
        public decimal intensitySum { get; set; } = 0; // rough value
        public decimal pValue_randomization { get; set; } = 0; // rough value

        #endregion Public Properties

        #region Public Fields

        public TusherValues1 TusherValues1 = new TusherValues1();
        public TusherValues2 TusherValues2 = new TusherValues2();
        public Log2FoldChangeValues Log2FoldChangeValues = new Log2FoldChangeValues();

        #endregion Public Fields

        #region Public Constructor

        //Selecting numerator and denominator is not implemented
        public QuantitativeProteoformValues(ExperimentalProteoform eP)
        {
            eP.quant = this;
            proteoform = eP;
        }

        #endregion Public Constructor

        #region Public Methods

        public void determine_statistics()
        {
            intensitySum = TusherValues1.numeratorIntensitySum + TusherValues1.denominatorIntensitySum;
            logFoldChange = (decimal)Math.Log((double)TusherValues1.numeratorIntensitySum / (double)TusherValues1.denominatorIntensitySum, 2);
            pValue_randomization = Randomization_PValue(logFoldChange, TusherValues1.numeratorOriginalBiorepIntensities.Concat(TusherValues1.numeratorImputedIntensities).ToList(), TusherValues1.denominatorOriginalBiorepIntensities.Concat(TusherValues1.denominatorImputedIntensities).ToList());
        }

        /// <summary>
        /// Computes an intensity from a background distribution of intensity I and standard deviation s.
        /// The standard normal distribution of mean 0 and variance 1 is computed from two random numbers distributed uniformly on (0, 1).
        /// Then, a number from that distribution, x, is used to calculate the imputed intensity: I + s * x
        /// </summary>
        /// <param name="avgLog2Intensity"></param>
        /// <param name="stdevLog2Intensity"></param>
        /// <returns></returns>
        public static double imputed_intensity(decimal avgLog2Intensity, decimal stdevLog2Intensity)
        {
            //bkgdAverageIntensity is coming in as a log 2 number
            //bkgdStDev is coming in as a log 2 number

            double u1 = ExtensionMethods.RandomNumber(); // these are uniform(0,1) random doubles
            double u2 = ExtensionMethods.RandomNumber();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); // random normal(0,1) -- normal(mean,variance)
            double intensity = Math.Pow(2, (double)avgLog2Intensity + (double)stdevLog2Intensity * randStdNormal); // std dev is calculated for log intensities, so convert to linear after adding I + s * x
            return intensity;
        }

        /// <summary>
        /// Calculates a rough q-value for this proteoform by dividing the estimated number of pfs that pass given the balanced permutations by the real pfs that pass.
        /// The threshold for passing is drawn at the test statistic for this proteoform.
        /// To calculate the "estimated number of pfs that pass given balanced permutations," I multiply the proportion of passing permuations by the number of pfs.
        /// 
        /// This calculation is an estimation of the relative difference method presented in the Tusher et al. paper, but it is not part of that calculation.
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

        public decimal Randomization_PValue(decimal logFoldChange, List<BiorepIntensity> allNumerators, List<BiorepIntensity> allDenominators)
        {
            if (allNumerators.Count != allDenominators.Count)
                throw new ArgumentException("Error: Imputation has gone awry. Each biorep should have the same number of biorep intensities for NeuCode light and heavy at this point.");

            int maxRandomizations = 10000;
            ConcurrentBag<decimal> randomizedRatios = new ConcurrentBag<decimal>();

            Parallel.For(0, maxRandomizations, i =>
            {
                List<double> combined = allNumerators.Select(j => j.intensity_sum).Concat(allDenominators.Select(j => j.intensity_sum)).ToList();
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
