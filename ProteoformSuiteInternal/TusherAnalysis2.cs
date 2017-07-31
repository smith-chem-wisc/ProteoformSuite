using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public static class TusherAnalysis2
    {

        #region Public Methods

        public static List<QuantitativeProteoformValues> compute_proteoform_statistics(List<ExperimentalProteoform> satisfactoryProteoforms, decimal bkgdAverageIntensity, decimal bkgdStDev, Dictionary<string, List<string>> conditionsBioReps, string numerator_condition, string denominator_condition, string induced_condition, decimal sKnot_minFoldChange, bool define_histogram)
        {
            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                eP.quant.TusherValues2.impute_biorep_intensities(eP.biotechIntensityList, conditionsBioReps, numerator_condition, denominator_condition, induced_condition, bkgdAverageIntensity, bkgdStDev, sKnot_minFoldChange);
            }

            if (define_histogram)
                Sweet.lollipop.distributions.defineSelectObservedWithImputedIntensityDistribution(satisfactoryProteoforms, Sweet.lollipop.distributions.logSelectIntensityWithImputationHistogram);

            normalize_protoeform_intensities(satisfactoryProteoforms);

            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                eP.quant.TusherValues2.determine_proteoform_statistics(induced_condition, sKnot_minFoldChange);
                eP.quant.determine_statistics();
            }

            return satisfactoryProteoforms.Where(eP => eP.accepted == true).Select(e => e.quant).ToList();
        }

        private static void normalize_protoeform_intensities(List<ExperimentalProteoform> satisfactoryProteoforms)
        {
            // Make lookup of intensities by condition/biorep for normalization
            Dictionary<Tuple<string, string>, List<double>> conditionBiorep_intensities = new Dictionary<Tuple<string, string>, List<double>>();
            List<BiorepTechrepIntensity> allOriginalBiorepIntensities = satisfactoryProteoforms.SelectMany(pf => pf.quant.TusherValues2.allIntensities.Values).ToList();
            foreach (BiorepTechrepIntensity bi in allOriginalBiorepIntensities)
            {
                Tuple<string, string> key2 = new Tuple<string, string>(bi.condition, bi.biorep);
                bool yes = conditionBiorep_intensities.TryGetValue(key2, out List<double> intensities2);
                if (yes) intensities2.Add(bi.intensity_sum);
                else conditionBiorep_intensities.Add(key2, new List<double> { bi.intensity_sum });
            }

            // Mixing bias normalization
            Dictionary<Tuple<string, string>, double> conditionBiorep_sums = conditionBiorep_intensities.ToDictionary(kv => kv.Key, kv => kv.Value.Sum());
            foreach (BiorepTechrepIntensity bi in allOriginalBiorepIntensities)
            {
                double norm_divisor = conditionBiorep_sums[new Tuple<string, string>(bi.condition, bi.biorep)] / conditionBiorep_sums.Where(kv => kv.Key.Item2 == bi.biorep).Average(kv => kv.Value);
                bi.intensity_sum = bi.intensity_sum / norm_divisor;
            }

            // Zero-center the intensities for each proteoform
            foreach (ExperimentalProteoform pf in satisfactoryProteoforms)
            {
                double avg_biorepintensity = pf.quant.TusherValues2.allIntensities.Values.Average(b => b.intensity_sum); // row average (for this proteoform)
                foreach (BiorepTechrepIntensity b in pf.quant.TusherValues2.allIntensities.Values)
                {
                    b.intensity_sum = b.intensity_sum - avg_biorepintensity;
                }
            }
        }

        public static List<List<decimal>> compute_balanced_biorep_permutation_relativeDifferences(Dictionary<string, List<string>> conditionsBioReps, List<InputFile> input_files, string induced_condition, List<ExperimentalProteoform> satisfactoryProteoforms, decimal sKnot_minFoldChange)
        {
            if (!conditionsBioReps.All(x => x.Value.OrderBy(y => y).SequenceEqual(conditionsBioReps.First().Value.OrderBy(z => z))))
                throw new ArgumentException("Error: Permutation analysis doesn't currently handle unbalanced experimental designs.");
            if (conditionsBioReps.Count > 2)
                throw new ArgumentException("Error: Permutation analysis doesn't currently handle experimental designs with more than 2 conditions.");

            List<InputFile> files = Sweet.lollipop.get_files(input_files, Purpose.Quantification).ToList();
            List<string> bioreps = files.Select(f => f.biological_replicate).Distinct().ToList();

            List<Tuple<string, string, string>> all = files.Select(f => new Tuple<string, string, string>(f.lt_condition, f.biological_replicate, f.technical_replicate))
                .Concat(files.Select(f => new Tuple<string, string, string>(f.hv_condition, f.biological_replicate, f.technical_replicate))).ToList();
            List<Tuple<string, string, string>> allInduced = all.Where(x => x.Item1 == induced_condition).ToList();
            List<Tuple<string, string, string>> allUninduced = all.Where(x => x.Item1 != induced_condition).ToList();
            List<IEnumerable<Tuple<string, string, string>>> permutations = ExtensionMethods.Combinations(all, allInduced.Count).ToList();
            List<IEnumerable<Tuple<string, string, string>>> balanced_permutations_induced = permutations.Where(p =>
                !p.SequenceEqual(allInduced) // not the original set
                && bioreps.All(rep => p.Count(x => x.Item2 == rep) == allInduced.Count(x => x.Item2 == rep))) // the number of each biorep should be the same as the original set
                .ToList();

            List<List<decimal>> permutedRelativeDifferences = new List<List<decimal>>(); // each internal list is sorted
            foreach (IEnumerable<Tuple<string, string, string>> induced in balanced_permutations_induced)
            {
                List<decimal> relativeDifferences = new List<decimal>();
                foreach (ExperimentalProteoform pf in satisfactoryProteoforms)
                {
                    List<BiorepTechrepIntensity> induced_intensities = induced.Select(x => pf.quant.TusherValues2.allIntensities[x]).ToList();
                    List<BiorepTechrepIntensity> uninduced_intensities = pf.quant.TusherValues2.allIntensities.Values.Except(induced_intensities).ToList();
                    relativeDifferences.Add(pf.quant.TusherValues2.getSingleTestStatistic(induced_intensities, uninduced_intensities, pf.quant.TusherValues2.StdDev(induced_intensities, uninduced_intensities), sKnot_minFoldChange));
                }
                permutedRelativeDifferences.Add(relativeDifferences);
            }
            return permutedRelativeDifferences;
        }

        public static void computeSortedRelativeDifferences(List<ExperimentalProteoform> satisfactoryProteoforms, List<List<decimal>> permutedRelativeDifferences)
        {
            Sweet.lollipop.sortedProteoformRelativeDifferences = satisfactoryProteoforms.Select(eP => eP.quant.TusherValues2.relative_difference).OrderBy(reldiff => reldiff).ToList();
            Sweet.lollipop.sortedPermutedRelativeDifferences = permutedRelativeDifferences.Select(list => list.OrderBy(reldiff => reldiff).ToList()).ToList();
            Sweet.lollipop.avgSortedPermutationRelativeDifferences = Enumerable.Range(0, Sweet.lollipop.sortedProteoformRelativeDifferences.Count).Select(i => Sweet.lollipop.sortedPermutedRelativeDifferences.Average(sorted => sorted[i])).OrderBy(x => x).ToList();
            int ct = 0;
            foreach (ExperimentalProteoform p in satisfactoryProteoforms.OrderBy(eP => eP.quant.TusherValues2.relative_difference).ToList())
            {
                p.quant.TusherValues2.correspondingAvgSortedRelDiff = Sweet.lollipop.avgSortedPermutationRelativeDifferences[ct++];
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
        public static double computeRelativeDifferenceFDR(List<decimal> sortedAvgPermutationTestStatistics, List<decimal> sortedProteoformTestStatistics, List<ExperimentalProteoform> satisfactoryProteoforms, List<decimal> permutedTestStatistics, decimal significanceTestStatOffset)
        {
            Sweet.lollipop.minimumPassingNegativeTestStatistic = Decimal.MinValue;
            Sweet.lollipop.minimumPassingPositiveTestStatisitic = Decimal.MaxValue;

            for (int i = 0; i < satisfactoryProteoforms.Count; i++)
            {
                decimal lower_threshold = sortedAvgPermutationTestStatistics[i] - significanceTestStatOffset;
                decimal higher_threshold = sortedAvgPermutationTestStatistics[i] + significanceTestStatOffset;
                if (sortedProteoformTestStatistics[i] < lower_threshold && sortedProteoformTestStatistics[i] <= 0)
                    Sweet.lollipop.minimumPassingNegativeTestStatistic = sortedProteoformTestStatistics[i]; // last one below
                if (sortedProteoformTestStatistics[i] > higher_threshold && sortedProteoformTestStatistics[i] >= 0)
                {
                    Sweet.lollipop.minimumPassingPositiveTestStatisitic = sortedProteoformTestStatistics[i]; //first one above
                    break;
                }
            }

            double avgFalsePermutedPassingProteoforms = (double)permutedTestStatistics.Count(v => v <= Sweet.lollipop.minimumPassingNegativeTestStatistic && v <= 0 || Sweet.lollipop.minimumPassingPositiveTestStatisitic <= v && v >= 0) / (double)permutedTestStatistics.Count * (double)satisfactoryProteoforms.Count;

            int totalPassingProteoforms = 0;
            foreach (ExperimentalProteoform pf in satisfactoryProteoforms)
            {
                decimal test_statistic = pf.quant.TusherValues2.relative_difference;
                pf.quant.TusherValues2.significant = test_statistic <= Sweet.lollipop.minimumPassingNegativeTestStatistic && test_statistic <= 0 || Sweet.lollipop.minimumPassingPositiveTestStatisitic <= test_statistic && test_statistic >= 0;
                totalPassingProteoforms += Convert.ToInt32(pf.quant.TusherValues2.significant);
            }

            if (totalPassingProteoforms == 0)
                return Double.NaN;

            double fdr = (double)avgFalsePermutedPassingProteoforms / (double)totalPassingProteoforms;
            return fdr;
        }

        public static void computeIndividualExperimentalProteoformFDRs(List<ExperimentalProteoform> satisfactoryProteoforms, List<decimal> permutedTestStatistics, List<decimal> sortedProteoformTestStatistics)
        {
            Parallel.ForEach(satisfactoryProteoforms, eP =>
            {
                eP.quant.TusherValues2.roughSignificanceFDR = QuantitativeProteoformValues.computeExperimentalProteoformFDR(eP.quant.TusherValues2.relative_difference, permutedTestStatistics, satisfactoryProteoforms.Count, sortedProteoformTestStatistics);
            });
        }

        public static void reestablishSignficance()
        {
            if (!Sweet.lollipop.useLocalFdrCutoff)
                Sweet.lollipop.relativeDifferenceFDR = computeRelativeDifferenceFDR(Sweet.lollipop.avgSortedPermutationRelativeDifferences, Sweet.lollipop.sortedProteoformRelativeDifferences, Sweet.lollipop.satisfactoryProteoforms, Sweet.lollipop.flattenedPermutedRelativeDifferences, Sweet.lollipop.offsetTestStatistics);
            else
                Parallel.ForEach(Sweet.lollipop.satisfactoryProteoforms, eP => { eP.quant.TusherValues2.significant = eP.quant.TusherValues2.roughSignificanceFDR <= Sweet.lollipop.localFdrCutoff; });
            Sweet.lollipop.inducedOrRepressedProteins = Sweet.lollipop.getInducedOrRepressedProteins(Sweet.lollipop.satisfactoryProteoforms, Sweet.lollipop.minProteoformFoldChange, Sweet.lollipop.maxGoTermFDR, Sweet.lollipop.minProteoformIntensity);
            Sweet.lollipop.GO_analysis();
        }

        #endregion Public Methods

    }
}
