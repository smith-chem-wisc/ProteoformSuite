using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class TusherAnalysis1
        : TusherAnalysis
    {
        #region Public Methods

        public void compute_proteoform_statistics(List<ExperimentalProteoform> satisfactoryProteoforms, decimal bkgdAverageIntensity, decimal bkgdStDev, Dictionary<string, List<string>> conditionsBioReps, string numerator_condition, string denominator_condition, string induced_condition, decimal sKnot_minFoldChange, bool define_histogram)
        {
            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                eP.quant.TusherValues1.impute_biorep_intensities(eP.biorepIntensityList, conditionsBioReps, numerator_condition, denominator_condition, induced_condition, bkgdAverageIntensity, bkgdStDev, sKnot_minFoldChange, Sweet.lollipop.useRandomSeed_quant, Sweet.lollipop.seeded);
            }

            if (define_histogram)
            {
                QuantitativeDistributions.defineSelectObservedWithImputedIntensityDistribution(satisfactoryProteoforms.SelectMany(pf => pf.biorepIntensityList), QuantitativeDistributions.logSelectIntensityWithImputationHistogram);
            }

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
            NormalizeIntensities(allOriginalBiorepIntensities, conditionBiorep_intensities);

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
            BalancedPermutationChecks(conditionsBioReps);

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

        public override void reestablishSignficance(IGoAnalysis analysis)
        {
            if (!Sweet.lollipop.useLocalFdrCutoff)
            {
                relativeDifferenceFDR = computeRelativeDifferenceFDR(avgSortedPermutationRelativeDifferences, sortedProteoformRelativeDifferences, Sweet.lollipop.satisfactoryProteoforms, flattenedPermutedRelativeDifferences, Sweet.lollipop.offsetTestStatistics);
            }
            else
            {
                Parallel.ForEach(Sweet.lollipop.satisfactoryProteoforms, eP => { eP.quant.TusherValues1.significant = eP.quant.TusherValues1.roughSignificanceFDR <= Sweet.lollipop.localFdrCutoff; });
            }
            inducedOrRepressedProteins = Sweet.lollipop.getInducedOrRepressedProteins(Sweet.lollipop.satisfactoryProteoforms.Where(pf => pf.quant.TusherValues1.significant), GoAnalysis);
            analysis.GoAnalysis.GO_analysis(inducedOrRepressedProteins);
        }

        #endregion Public Methods
    }
}