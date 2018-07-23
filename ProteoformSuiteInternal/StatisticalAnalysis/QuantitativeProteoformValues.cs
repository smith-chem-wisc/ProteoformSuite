using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class QuantitativeProteoformValues
    {

        #region Public Properties

        public ExperimentalProteoform proteoform { get; set; }
        public string accession { get { return proteoform.accession; } }
        public decimal tusherlogFoldChange { get; set; } = 0; // rough value
        public decimal intensitySum { get; set; } = 0; // rough value

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
            tusherlogFoldChange = (decimal)Math.Log((double)TusherValues1.numeratorIntensitySum / (double)TusherValues1.denominatorIntensitySum, 2);
        }

        /// <summary>
        /// Computes an intensity from a background distribution of intensity I and standard deviation s.
        /// The standard normal distribution of mean 0 and variance 1 is computed from two random numbers distributed uniformly on (0, 1).
        /// Then, a number from that distribution, x, is used to calculate the imputed intensity: I + s * x
        /// </summary>
        /// <param name="avgLog2Intensity"></param>
        /// <param name="stdevLog2Intensity"></param>
        /// <returns></returns>
        public static double imputed_intensity(decimal avgLog2Intensity, decimal stdevLog2Intensity, bool useRandomSeed, Random seeded)
        {
            //bkgdAverageIntensity is coming in as a log 2 number
            //bkgdStDev is coming in as a log 2 number

            double u1 = useRandomSeed && seeded != null ? seeded.NextDouble() : ExtensionMethods.RandomNumber(); // these are uniform(0,1) random doubles
            double u2 = useRandomSeed && seeded != null ? seeded.NextDouble() : ExtensionMethods.RandomNumber();
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
        public static decimal computeExperimentalProteoformFDR(decimal testStatistic, List<TusherStatistic> permutedTestStatistics, int satisfactoryProteoformsCount, List<TusherStatistic> sortedProteoformTestStatistics)
        {
            decimal minimumPositivePassingTestStatistic = Math.Abs(testStatistic);
            decimal minimumNegativePassingTestStatistic = -minimumPositivePassingTestStatistic;

            int totalFalsePermutedPassingValues = permutedTestStatistics.Count(v => v.is_passing_permutation(minimumNegativePassingTestStatistic, minimumPositivePassingTestStatistic, Sweet.lollipop.fold_change_conjunction, Sweet.lollipop.useFoldChangeCutoff, Sweet.lollipop.foldChangeCutoff, Sweet.lollipop.useAveragePermutationFoldChange, Sweet.lollipop.useBiorepPermutationFoldChange, Sweet.lollipop.minBiorepsWithFoldChange, out bool a, out bool b));
            decimal averagePermutedPassing = (decimal)totalFalsePermutedPassingValues / (decimal)permutedTestStatistics.Count * (decimal)satisfactoryProteoformsCount;

            int totalRealPassing = sortedProteoformTestStatistics.Count(stat => stat.is_passing_real(minimumNegativePassingTestStatistic, minimumPositivePassingTestStatistic, Sweet.lollipop.fold_change_conjunction, Sweet.lollipop.useFoldChangeCutoff, Sweet.lollipop.foldChangeCutoff, Sweet.lollipop.useAveragePermutationFoldChange, Sweet.lollipop.useBiorepPermutationFoldChange, Sweet.lollipop.minBiorepsWithFoldChange, out bool a, out bool b));

            decimal fdr = averagePermutedPassing / (decimal)totalRealPassing; // real passing will always be above zero because this proteoform always passes
            return fdr;
        }

        #endregion Public Methods

    }
}
