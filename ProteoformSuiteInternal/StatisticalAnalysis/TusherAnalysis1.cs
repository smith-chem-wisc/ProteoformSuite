using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class TusherAnalysis1
        : TusherAnalysis
    {

        #region Public Fields

        public Dictionary<Tuple<string, string>, double> conditionBiorep_sums = new Dictionary<Tuple<string, string>, double>();

        #endregion Public Fields

        #region Public Methods

        public void compute_proteoform_statistics(List<ExperimentalProteoform> satisfactoryProteoforms, decimal bkgdAverageIntensity, decimal bkgdStDev, Dictionary<string, List<string>> conditionsBioReps, string numerator_condition, string denominator_condition, string induced_condition, decimal sKnot_minFoldChange, bool define_histogram)
        {
            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                eP.quant.TusherValues1.impute_biorep_intensities(eP.biorepIntensityList, conditionsBioReps, numerator_condition, denominator_condition, induced_condition, bkgdAverageIntensity, bkgdStDev, sKnot_minFoldChange, Sweet.lollipop.useRandomSeed_quant, Sweet.lollipop.seeded);
            }

            if (define_histogram)
                QuantitativeDistributions.defineSelectObservedWithImputedIntensityDistribution(satisfactoryProteoforms.SelectMany(pf => pf.biorepIntensityList), QuantitativeDistributions.logSelectIntensityWithImputationHistogram);

            normalize_protoeform_intensities(satisfactoryProteoforms);

            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                eP.quant.TusherValues1.determine_proteoform_statistics(induced_condition, sKnot_minFoldChange);
                eP.quant.determine_statistics();
            }
        }

        private void normalize_protoeform_intensities(List<ExperimentalProteoform> satisfactoryProteoforms)
        {
            // Make lookup of intensities by condition/biorep for normalization
            Dictionary<Tuple<string, string>, List<double>> conditionBiorep_intensities = new Dictionary<Tuple<string, string>, List<double>>();
            List<BiorepIntensity> allOriginalBiorepIntensities = satisfactoryProteoforms.SelectMany(pf => pf.quant.TusherValues1.allIntensities.Values).ToList();
            foreach (BiorepIntensity bi in allOriginalBiorepIntensities)
            {
                Tuple<string, string> key2 = new Tuple<string, string>(bi.condition, bi.biorep);
                bool yes = conditionBiorep_intensities.TryGetValue(key2, out List<double> intensities2);
                if (yes) intensities2.Add(bi.intensity_sum);
                else conditionBiorep_intensities.Add(key2, new List<double> { bi.intensity_sum });
            }

            // Mixing bias normalization
            conditionBiorep_sums = conditionBiorep_intensities.ToDictionary(kv => kv.Key, kv => kv.Value.Sum());
            foreach (BiorepIntensity bi in allOriginalBiorepIntensities)
            {
                double norm_divisor = conditionBiorep_sums[new Tuple<string, string>(bi.condition, bi.biorep)] / 
                    (Sweet.lollipop.neucode_labeled ? conditionBiorep_sums.Where(kv => kv.Key.Item2 == bi.biorep).Average(kv => kv.Value) : conditionBiorep_sums.Average(kv => kv.Value));
                bi.intensity_sum = bi.intensity_sum / norm_divisor;
            }

            // Zero-center the intensities for each proteoform
            foreach (ExperimentalProteoform pf in satisfactoryProteoforms)
            {
                pf.quant.TusherValues1.normalization_subtractand = pf.quant.TusherValues1.allIntensities.Values.Average(b => b.intensity_sum); // row average (for this proteoform)
                foreach (BiorepIntensity b in pf.quant.TusherValues1.allIntensities.Values)
                {
                    b.intensity_sum = b.intensity_sum - pf.quant.TusherValues1.normalization_subtractand;
                }
            }
        }


        /// <summary>
        /// Gets the relative differences of permuted bioreps.
        /// In the Tusher et al. (2001) paper, they made all 36 balanced permutations of the 2 cell lines. Here, we have one control that is orthogonal to the treatment: bioreps.
        /// Therefore, we can make 3 balanced permutations (see Example 3).
        /// </summary>
        /// <param name="conditionsBioReps"></param>
        /// <returns></returns>
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
        /// 
        /// Example 3 (duples):
        /// Normal: n_1, n_2, n_3 | Stress: s_1, s_2, s_3
        /// Duple 1: n_1, s_1 ;; Duple 2: n_2, s_2 ;; Duple 3: n_3, s_3
        /// Three balanced permutations for "normal." Balanced when 2 from the original set.
        /// *s_1*, n_2, n_3
        /// n_1, *s_2*, n_3
        /// n_1, n_2, *s_3*
        /// 
        /// Example 4 (duples):
        /// Normal: n_1, n_2, n_3, n_4 | Stress: s_1, s_2, s_3, s_4
        /// Duple 1: n_1, s_1 ;; Duple 2: n_2, s_2 ;; Duple 3: n_3, s_3 ;; Duple 4: n_4, s_4
        /// Six balanced permutations for "normal." Balanced when 2 from the original set.
        /// *s_1*, *s_2*, n_3, n_4
        /// *s_1*, s_2, *s_3*, n_4
        /// *s_1*, s_2, n_3, *s_4*
        /// n_1, *s_2*, *s_3*, n_4
        /// n_1, *s_2*, n_3, *s_4*
        /// n_1, n_2, *s_3*, *s_4*
        /// 
        public List<List<TusherStatistic>> compute_balanced_biorep_permutation_relativeDifferences(Dictionary<string, List<string>> conditionsBioReps, string induced_condition, List<ExperimentalProteoform> satisfactoryProteoforms, decimal sKnot_minFoldChange)
        {
            if (!conditionsBioReps.All(x => x.Value.OrderBy(y => y).SequenceEqual(conditionsBioReps.First().Value.OrderBy(z => z))))
                throw new ArgumentException("Error: Permutation analysis doesn't currently handle unbalanced experimental designs.");
            if (conditionsBioReps.Count > 2)
                throw new ArgumentException("Error: Permutation analysis doesn't currently handle experimental  designs with more than 2 conditions.");

            List<string> bioreps = conditionsBioReps.SelectMany(kv => kv.Value).Distinct().ToList();
            List<Tuple<string, string>> allInduced = conditionsBioReps[induced_condition].Select(v => new Tuple<string, string>(induced_condition, v)).ToList();
            List<Tuple<string, string>> allUninduced = conditionsBioReps.Where(kv => kv.Key != induced_condition).SelectMany(kv => kv.Value.Select(v => new Tuple<string, string>(kv.Key, v))).ToList();
            List<Tuple<string, string>> all = allInduced.Concat(allUninduced).ToList();
            List<IEnumerable<Tuple<string, string>>> permutations = ExtensionMethods.Combinations(all, allInduced.Count).ToList();
            List<IEnumerable<Tuple<string, string>>> balanced_permutations_induced = permutations.Where(p =>
                bioreps.All(rep => p.Count(x => x.Item2 == rep) == allInduced.Count(x => x.Item2 == rep)) // all bioreps are represented for balanced permutations
                ).ToList();

            List<List<TusherStatistic>> permutedRelativeDifferences = new List<List<TusherStatistic>>(); // each internal list is sorted
            foreach (IEnumerable<Tuple<string, string>> induced in balanced_permutations_induced)
            {
                List<TusherStatistic> relativeDifferences = new List<TusherStatistic>();
                foreach (ExperimentalProteoform pf in satisfactoryProteoforms)
                {
                    List<IBiorepIntensity> induced_intensities = induced.Select(x => pf.quant.TusherValues1.allIntensities[x]).ToList<IBiorepIntensity>();
                    List<IBiorepIntensity> uninduced_intensities = pf.quant.TusherValues1.allIntensities.Values.Except(induced_intensities).ToList();
                    relativeDifferences.Add(new TusherStatistic(
                        pf.quant.TusherValues1.getSingleTestStatistic(induced_intensities, uninduced_intensities, pf.quant.TusherValues1.StdDev(induced_intensities, uninduced_intensities), sKnot_minFoldChange), 
                        pf.quant.TusherValues1.getSingleFoldChange(induced_intensities, uninduced_intensities),
                        bioreps.Select(b => pf.quant.TusherValues1.getSingleFoldChange(induced_intensities.Where(x => x.biorep == b).ToList(), uninduced_intensities.Where(x => x.biorep == b).ToList())).ToList()
                        ));
                }
                permutedRelativeDifferences.Add(relativeDifferences);
            }
            return permutedRelativeDifferences;
        }

        public void computeSortedRelativeDifferences(List<ExperimentalProteoform> satisfactoryProteoforms, List<List<TusherStatistic>> permutedRelativeDifferences)
        {
            sortedProteoformRelativeDifferences = satisfactoryProteoforms.Select(eP => eP.quant.TusherValues1.tusher_statistic).OrderBy(x => x.relative_difference).ToList();
            sortedPermutedRelativeDifferences = permutedRelativeDifferences.Select(list => list.OrderBy(x => x.relative_difference).ToList()).ToList();
            avgSortedPermutationRelativeDifferences = Enumerable.Range(0, sortedProteoformRelativeDifferences.Count).Select(i => sortedPermutedRelativeDifferences.Average(sorted => sorted[i].relative_difference)).OrderBy(x => x).ToList();
            int ct = 0;
            foreach (ExperimentalProteoform p in satisfactoryProteoforms.OrderBy(eP => eP.quant.TusherValues1.relative_difference).ToList())
            {
                p.quant.TusherValues1.correspondingAvgSortedRelDiff = avgSortedPermutationRelativeDifferences[ct++];
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
                    minimumPassingNegativeTestStatistic = sortedProteoformTestStatistics[i].relative_difference; // last one below
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
                pf.quant.TusherValues1.significant = pf.quant.TusherValues1.tusher_statistic.is_passing_real(minimumPassingNegativeTestStatistic, minimumPassingPositiveTestStatisitic, Sweet.lollipop.fold_change_conjunction, Sweet.lollipop.useFoldChangeCutoff, Sweet.lollipop.foldChangeCutoff, Sweet.lollipop.useAveragePermutationFoldChange, Sweet.lollipop.useBiorepPermutationFoldChange, Sweet.lollipop.minBiorepsWithFoldChange, out bool is_passing_relative_difference, out bool is_passing_fold_change);
                pf.quant.TusherValues1.significant_relative_difference = is_passing_relative_difference;
                pf.quant.TusherValues1.significant_fold_change = is_passing_fold_change;
                totalPassingProteoforms += Convert.ToInt32(pf.quant.TusherValues1.significant);
            }

            if (totalPassingProteoforms == 0)
                return Double.NaN;

            double fdr = (double)avgPermutedPassingProteoforms / (double)totalPassingProteoforms;
            return fdr;
        }

        public void computeIndividualExperimentalProteoformFDRs(List<ExperimentalProteoform> satisfactoryProteoforms, List<TusherStatistic> permutedTestStatistics, List<TusherStatistic> sortedProteoformTestStatistics)
        {
            Parallel.ForEach(satisfactoryProteoforms, eP =>
            {
                eP.quant.TusherValues1.roughSignificanceFDR = QuantitativeProteoformValues.computeExperimentalProteoformFDR(eP.quant.TusherValues1.relative_difference, permutedTestStatistics, satisfactoryProteoforms.Count, sortedProteoformTestStatistics);
            });
        }

        public override void reestablishSignficance(IGoAnalysis analysis)
        {
            if (!Sweet.lollipop.useLocalFdrCutoff)
                relativeDifferenceFDR = computeRelativeDifferenceFDR(avgSortedPermutationRelativeDifferences, sortedProteoformRelativeDifferences, Sweet.lollipop.satisfactoryProteoforms, flattenedPermutedRelativeDifferences, Sweet.lollipop.offsetTestStatistics);
            else
            Parallel.ForEach(Sweet.lollipop.satisfactoryProteoforms, eP => { eP.quant.TusherValues1.significant = eP.quant.TusherValues1.roughSignificanceFDR <= Sweet.lollipop.localFdrCutoff; });
            inducedOrRepressedProteins = Sweet.lollipop.getInducedOrRepressedProteins(Sweet.lollipop.satisfactoryProteoforms.Where(pf => pf.quant.TusherValues1.significant), GoAnalysis);
            analysis.GoAnalysis.GO_analysis(inducedOrRepressedProteins);
        }

        #endregion Public Methods

    }
}
