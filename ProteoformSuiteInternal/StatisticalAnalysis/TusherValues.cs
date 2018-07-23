using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class TusherValues
        : IStatisiticalSignificance
    {

        #region Public Properties        

        public decimal numeratorIntensitySum { get; set; } = 0;
        public decimal denominatorIntensitySum { get; set; } = 0;
        public decimal scatter { get; set; } = 0;
        public bool significant { get; set; } = false;
        public bool significant_relative_difference { get; set; } = false;
        public bool significant_fold_change { get; set; } = false;
        public decimal relative_difference { get; set; }
        public decimal fold_change { get; set; }
        public TusherStatistic tusher_statistic { get; set; }
        public decimal correspondingAvgSortedRelDiff { get; set; }
        public decimal roughSignificanceFDR { get; set; } = 0;
        public double normalization_subtractand { get; set; } = 0;

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Calculates the pooled standard deviation across unlogged intensities for the two conditions for this proteoform.
        /// This is known as the "scatter s(i)" in the Tusher et al. paper.
        /// </summary>
        /// <param name="allInduced"></param>
        /// <param name="allUninduced"></param>
        /// <returns></returns>
        public decimal StdDev(IEnumerable<IBiorepIntensity> allInduced, IEnumerable<IBiorepIntensity> allUninduced)
        {
            int numerator_count = allInduced.Count();
            int denominator_count = allUninduced.Count();
            if ((numerator_count + denominator_count) == 2)
                return 1000000m;

            decimal a = (decimal)((1d / (double)numerator_count + 1d / (double)denominator_count) / ((double)numerator_count + (double)denominator_count - 2d));
            double avgNumerator = allInduced.Average(x => x.intensity_sum);
            double avgDenominator = allUninduced.Average(x => x.intensity_sum);
            decimal numeratorSumSquares = allInduced.Sum(n => (decimal)Math.Pow(n.intensity_sum - avgNumerator, 2d));
            decimal denominatorSumSquares = allUninduced.Sum(d => (decimal)Math.Pow(d.intensity_sum - avgDenominator, 2d));
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
        public decimal getSingleTestStatistic(List<IBiorepIntensity> allInduced, List<IBiorepIntensity> allUninduced, decimal pooledStdDev, decimal sKnot)
        {
            double t = (allInduced.Average(l => l.intensity_sum) - allUninduced.Average(h => h.intensity_sum)) / ((double)(pooledStdDev + sKnot));
            return (decimal)t;
        }

        public decimal getSingleFoldChange(List<IBiorepIntensity> allInduced, List<IBiorepIntensity> allUninduced)
        {
            double i = allInduced.Sum(x => x.intensity_sum + normalization_subtractand);
            double u = allUninduced.Sum(x => x.intensity_sum + normalization_subtractand);
            if (u == 0) return 1;
            return (decimal)i / (decimal)u;
        }

        #endregion Public Methods

    }
}
