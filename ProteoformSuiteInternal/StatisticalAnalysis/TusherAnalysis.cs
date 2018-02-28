using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public abstract class TusherAnalysis
        : IGoAnalysis
    {
        #region Public Properties

        public List<TusherStatistic> sortedProteoformRelativeDifferences { get; set; } = new List<TusherStatistic>(); // real relative differences (and fold changes) for each selected proteoform; sorted
        public List<List<TusherStatistic>> permutedRelativeDifferences { get; set; } = new List<List<TusherStatistic>>(); // relative differences (and fold changes) for each proteoform for each balanced permutation
        public List<List<TusherStatistic>> sortedPermutedRelativeDifferences { get; set; } = new List<List<TusherStatistic>>(); // sorted relative differences for each balanced permutation
        public List<decimal> avgSortedPermutationRelativeDifferences { get; set; } = new List<decimal>(); // average relative difference across sorted values for each balanced permutation
        public List<TusherStatistic> flattenedPermutedRelativeDifferences { get; set; } = new List<TusherStatistic>(); // all relative differences (and fold changes) from permutations
        public decimal minimumPassingNegativeTestStatistic { get; set; } // the first NEGATIVE relative difference from a selected proteoform that exceeded the offset BELOW the expected relative differences (avg sorted permuted); everything equal to or BELOW this value is considered significant
        public decimal minimumPassingPositiveTestStatisitic { get; set; } // the first POSITIVE relative difference from a selected proteoform that exceeded the offset ABOVE the expected relative differences (avg sorted permuted); everything equal to or ABOVE this value is considered significant
        public double relativeDifferenceFDR { get; set; } // average # of permuted relative differences that pass minimumPassingNegativeTestStatistic & minimumPassingPositiveTestStatisitic, divided by the number of selected proteoforms that passed
        public List<ProteinWithGoTerms> inducedOrRepressedProteins { get; set; } = new List<ProteinWithGoTerms>(); // This is the list of proteins from proteoforms that underwent significant induction or repression
        public GoAnalysis GoAnalysis { get; set; } = new GoAnalysis();
        public QuantitativeDistributions QuantitativeDistributions { get; set; }
        public Dictionary<Tuple<string, string>, double> conditionBiorep_sums { get; set; } = new Dictionary<Tuple<string, string>, double>();

        #endregion Public Properties

        #region Public Constructor

        public TusherAnalysis()
        {
            this.QuantitativeDistributions = new QuantitativeDistributions(this);
        }

        #endregion Public Constructor

        #region Public Method

        public abstract void reestablishSignficance(IGoAnalysis analysis);

        public void NormalizeIntensities(IEnumerable<IBiorepIntensity> intensities, Dictionary<Tuple<string, string>, List<double>> conditionBiorep_intensities)
        {
            // Make lookup of intensities by condition/biorep for normalization
            foreach (IBiorepIntensity bi in intensities)
            {
                Tuple<string, string> key2 = new Tuple<string, string>(bi.condition, bi.biorep);
                bool yes = conditionBiorep_intensities.TryGetValue(key2, out List<double> intensities2);
                if (yes)
                {
                    intensities2.Add(bi.intensity_sum);
                }
                else
                {
                    conditionBiorep_intensities.Add(key2, new List<double> { bi.intensity_sum });
                }
            }

            // Mixing bias normalization
            conditionBiorep_sums = conditionBiorep_intensities.ToDictionary(kv => kv.Key, kv => kv.Value.Sum());
            foreach (IBiorepIntensity bi in intensities)
            {
                double norm_divisor = conditionBiorep_sums[new Tuple<string, string>(bi.condition, bi.biorep)] /
                    (Sweet.lollipop.neucode_labeled ? conditionBiorep_sums.Where(kv => kv.Key.Item2 == bi.biorep).Average(kv => kv.Value) : conditionBiorep_sums.Average(kv => kv.Value));
                bi.intensity_sum = bi.intensity_sum / norm_divisor;
            }
        }

        public void BalancedPermutationChecks(Dictionary<string, List<string>> conditionsBioReps)
        {
            if (!conditionsBioReps.All(x => x.Value.OrderBy(y => y).SequenceEqual(conditionsBioReps.First().Value.OrderBy(z => z))))
            {
                throw new ArgumentException("Error: Permutation analysis doesn't currently handle unbalanced experimental designs.");
            }
            if (conditionsBioReps.Count > 2)
            {
                throw new ArgumentException("Error: Permutation analysis doesn't currently handle experimental designs with more than 2 conditions.");
            }
        }

        /// <summary>
        /// Calculates the FDR and establishes significance for proteoforms with a method published by Tusher, et al. (2001)
        ///
        /// First, thresholds are established on either side of the line where the test statistic is equal to the average permuted test statistic of the same rank.
        /// Every proteoform test statistic that passes that threshold is considered significant.
        ///
        /// The average number of proteoforms that pass by chance is used to calculate the FDR.
        /// This calculated as the proportion of all permuted tests that pass the thresholds, times the number of proteoforms quantified.
        /// The denominator is the number of proteoforms that passed.
        /// Thus, the FDR is (# pfs passing by chance / # pfs passing).
        /// </summary>
        /// <param name="sortedAvgPermutationTestStatistics">
        /// The averages of test statistics calculated from permuted intensities for each proteoform. These are sorted independently of the real proteoform test statistics.
        /// </param>
        /// <param name="sortedProteoformTestStatistics">
        /// The test statistics calculated for each proteoform. These are sorted independently of the avg permuted test statistics.
        /// </param>
        /// <param name="satisfactoryProteoforms">
        /// All proteoforms selected for quantification.
        /// </param>
        /// <param name="permutedTestStatistics">
        /// All test statistics calculated from permuted proteoform intensities.
        /// </param>
        /// <param name="significanceTestStatOffset">
        /// Offset from the line where the proteoform test statistsic is equal to the average permuted test statistic: d(i) = dE(i). This is used as a threshold for significance for positive or negative test statistics.
        /// </param>
        /// <returns></returns>
        public double computeRelativeDifferenceFDR(List<decimal> sortedAvgPermutationTestStatistics, List<TusherStatistic> sortedProteoformTestStatistics, List<ExperimentalProteoform> satisfactoryProteoforms, List<TusherStatistic> permutedTestStatistics, decimal significanceTestStatOffset)
        {
            minimumPassingNegativeTestStatistic = Decimal.MinValue;
            minimumPassingPositiveTestStatisitic = Decimal.MaxValue;

            for (int i = 0; i < satisfactoryProteoforms.Count; i++)
            {
                decimal lower_threshold = sortedAvgPermutationTestStatistics[i] - significanceTestStatOffset;
                decimal higher_threshold = sortedAvgPermutationTestStatistics[i] + significanceTestStatOffset;
                if (sortedProteoformTestStatistics[i].relative_difference < lower_threshold && sortedProteoformTestStatistics[i].relative_difference <= 0)
                {
                    minimumPassingNegativeTestStatistic = sortedProteoformTestStatistics[i].relative_difference; // last one below
                }
                if (sortedProteoformTestStatistics[i].relative_difference > higher_threshold && sortedProteoformTestStatistics[i].relative_difference >= 0)
                {
                    minimumPassingPositiveTestStatisitic = sortedProteoformTestStatistics[i].relative_difference; //first one above
                    break;
                }
            }

            IEnumerable<TusherStatistic> permutedPassingProteoforms = permutedTestStatistics.Where(v => v.is_passing_permutation(minimumPassingNegativeTestStatistic, minimumPassingPositiveTestStatisitic, Sweet.lollipop.fold_change_conjunction, Sweet.lollipop.useFoldChangeCutoff, Sweet.lollipop.foldChangeCutoff, Sweet.lollipop.useAveragePermutationFoldChange, Sweet.lollipop.useBiorepPermutationFoldChange, Sweet.lollipop.minBiorepsWithFoldChange, out bool is_passing_relative_difference, out bool is_passing_fold_change));
            double avgPermutedPassingProteoforms = (double)permutedPassingProteoforms.Count() / (double)permutedTestStatistics.Count * (double)satisfactoryProteoforms.Count;

            int totalPassingProteoforms = 0;
            foreach (ExperimentalProteoform pf in satisfactoryProteoforms)
            {
                TusherValues tusherValues = this as TusherAnalysis != null ? pf.quant.TusherValues1 as TusherValues : pf.quant.TusherValues2;
                tusherValues.significant = tusherValues.tusher_statistic.is_passing_real(minimumPassingNegativeTestStatistic, minimumPassingPositiveTestStatisitic, Sweet.lollipop.fold_change_conjunction, Sweet.lollipop.useFoldChangeCutoff, Sweet.lollipop.foldChangeCutoff, Sweet.lollipop.useAveragePermutationFoldChange, Sweet.lollipop.useBiorepPermutationFoldChange, Sweet.lollipop.minBiorepsWithFoldChange, out bool is_passing_relative_difference, out bool is_passing_fold_change);
                tusherValues.significant_relative_difference = is_passing_relative_difference;
                tusherValues.significant_fold_change = is_passing_fold_change;
                totalPassingProteoforms += Convert.ToInt32(tusherValues.significant);
            }

            if (totalPassingProteoforms == 0)
            {
                return Double.NaN;
            }

            double fdr = avgPermutedPassingProteoforms / (double)totalPassingProteoforms;
            return fdr;
        }

        public void computeIndividualExperimentalProteoformFDRs(List<ExperimentalProteoform> satisfactoryProteoforms, List<TusherStatistic> permutedTestStatistics, List<TusherStatistic> sortedProteoformTestStatistics)
        {
            Parallel.ForEach(satisfactoryProteoforms, eP =>
            {
                TusherValues tusherValues = this as TusherAnalysis != null ? eP.quant.TusherValues1 as TusherValues : eP.quant.TusherValues2;
                tusherValues.roughSignificanceFDR = QuantitativeProteoformValues.computeExperimentalProteoformFDR(tusherValues.relative_difference, permutedTestStatistics, satisfactoryProteoforms.Count, sortedProteoformTestStatistics);
            });
        }

        #endregion Public Method
    }
}