using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class TusherAnalysis2
        : TusherAnalysis
    {
        #region Public Methods

        public void compute_proteoform_statistics(List<ExperimentalProteoform> satisfactoryProteoforms, decimal bkgdAverageIntensity, decimal bkgdStDev, Dictionary<string, List<string>> conditionsBioReps, string numerator_condition, string denominator_condition, string induced_condition, decimal sKnot_minFoldChange, bool define_histogram)
        {
            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                eP.quant.TusherValues2.impute_biorep_intensities(eP.biorepTechrepIntensityList, conditionsBioReps, numerator_condition, denominator_condition, induced_condition, bkgdAverageIntensity, bkgdStDev, sKnot_minFoldChange, Sweet.lollipop.useRandomSeed_quant, Sweet.lollipop.seeded);
                eP.quant.determine_statistics();
            }

            if (define_histogram)
                QuantitativeDistributions.defineSelectObservedWithImputedIntensityDistribution(satisfactoryProteoforms.SelectMany(pf => pf.quant.TusherValues2.allIntensities.Values), QuantitativeDistributions.logSelectIntensityWithImputationHistogram);

            normalize_protoeform_intensities(satisfactoryProteoforms);

            foreach (ExperimentalProteoform eP in satisfactoryProteoforms)
            {
                eP.quant.TusherValues2.determine_proteoform_statistics(induced_condition, sKnot_minFoldChange);
            }
        }

        private void normalize_protoeform_intensities(List<ExperimentalProteoform> satisfactoryProteoforms)
        {
            // Make lookup of intensities by condition/biorep for normalization
            Dictionary<Tuple<string, string>, List<double>> conditionBiorep_intensities = new Dictionary<Tuple<string, string>, List<double>>();
            List<BiorepTechrepIntensity> allOriginalBiorepIntensities = satisfactoryProteoforms.SelectMany(pf => pf.quant.TusherValues2.allIntensities.Values).ToList();
            NormalizeIntensities(allOriginalBiorepIntensities, conditionBiorep_intensities);

            // Zero-center the intensities for each proteoform
            foreach (ExperimentalProteoform pf in satisfactoryProteoforms)
            {
                pf.quant.TusherValues2.normalization_subtractand = pf.quant.TusherValues2.allIntensities.Values.Average(b => b.intensity_sum); // row average (for this proteoform)
                foreach (BiorepTechrepIntensity b in pf.quant.TusherValues2.allIntensities.Values)
                {
                    b.intensity_sum = b.intensity_sum - pf.quant.TusherValues2.normalization_subtractand;
                }
            }
        }

        public List<List<TusherStatistic>> compute_balanced_biorep_permutation_relativeDifferences(Dictionary<string, List<string>> conditionsBioReps, List<InputFile> input_files, string induced_condition, List<ExperimentalProteoform> satisfactoryProteoforms, decimal sKnot_minFoldChange)
        {
            BalancedPermutationChecks(conditionsBioReps);

            List<InputFile> files = Sweet.lollipop.get_files(input_files, Purpose.Quantification).ToList();
            List<string> bioreps = files.Select(f => f.biological_replicate).Distinct().ToList();
            string uninduced_condition = conditionsBioReps.Keys.FirstOrDefault(x => x != induced_condition);

            List<Tuple<string, string, string>> all = files.Where(f => f.lt_condition == induced_condition).Select(f => new Tuple<string, string, string>(f.lt_condition, f.biological_replicate, f.technical_replicate))
                .Concat(files.Where(f => f.hv_condition == induced_condition).Select(f => new Tuple<string, string, string>(f.hv_condition, f.biological_replicate, f.technical_replicate)))
                .Concat(files.Where(f => f.lt_condition == uninduced_condition).Select(f => new Tuple<string, string, string>(f.lt_condition, f.biological_replicate, f.technical_replicate)))
                .Concat(files.Where(f => f.hv_condition == uninduced_condition).Select(f => new Tuple<string, string, string>(f.hv_condition, f.biological_replicate, f.technical_replicate)))
                .Distinct().ToList();
            List<Tuple<string, string, string>> allInduced = all.Where(x => x.Item1 == induced_condition).ToList();
            List<Tuple<string, string, string>> allUninduced = all.Where(x => x.Item1 != induced_condition).ToList();
            List<IEnumerable<Tuple<string, string, string>>> permutations = ExtensionMethods.Combinations(all, allInduced.Count).ToList();
            List<IEnumerable<Tuple<string, string, string>>> balanced_permutations_induced = permutations.Where(p =>
                bioreps.All(rep => p.Count(x => x.Item2 == rep) == allInduced.Count(x => x.Item2 == rep)) // the number of each biorep should be the same as the original set for balanced permutations
                ).ToList();

            List<List<TusherStatistic>> permutedRelativeDifferences = new List<List<TusherStatistic>>(); // each internal list is sorted
            foreach (IEnumerable<Tuple<string, string, string>> induced in balanced_permutations_induced)
            {
                List<TusherStatistic> relativeDifferences = new List<TusherStatistic>();
                foreach (ExperimentalProteoform pf in satisfactoryProteoforms)
                {
                    List<IBiorepIntensity> induced_intensities = induced.Where(x => pf.quant.TusherValues2.allIntensities.ContainsKey(x)).Select(x => pf.quant.TusherValues2.allIntensities[x]).ToList<IBiorepIntensity>();
                    List<IBiorepIntensity> uninduced_intensities = pf.quant.TusherValues2.allIntensities.Values.Except(induced_intensities).ToList();
                    relativeDifferences.Add(
                        new TusherStatistic(pf.quant.TusherValues2.getSingleTestStatistic(induced_intensities, uninduced_intensities, pf.quant.TusherValues2.StdDev(induced_intensities, uninduced_intensities), sKnot_minFoldChange),
                        pf.quant.TusherValues2.getSingleFoldChange(induced_intensities, uninduced_intensities),
                        bioreps.Select(b => pf.quant.TusherValues2.getSingleFoldChange(induced_intensities.Where(x => x.biorep == b).ToList(), uninduced_intensities.Where(x => x.biorep == b).ToList())).ToList()
                        ));
                }
                permutedRelativeDifferences.Add(relativeDifferences);
            }
            return permutedRelativeDifferences;
        }

        public void computeSortedRelativeDifferences(List<ExperimentalProteoform> satisfactoryProteoforms, List<List<TusherStatistic>> permutedRelativeDifferences)
        {
            sortedProteoformRelativeDifferences = satisfactoryProteoforms.Select(eP => eP.quant.TusherValues2.tusher_statistic).OrderBy(x => x.relative_difference).ToList();
            sortedPermutedRelativeDifferences = permutedRelativeDifferences.Select(list => list.OrderBy(x => x.relative_difference).ToList()).ToList();
            avgSortedPermutationRelativeDifferences = Enumerable.Range(0, sortedProteoformRelativeDifferences.Count).Select(i => sortedPermutedRelativeDifferences.Average(sorted => sorted[i].relative_difference)).OrderBy(x => x).ToList();
            int ct = 0;
            foreach (ExperimentalProteoform p in satisfactoryProteoforms.OrderBy(eP => eP.quant.TusherValues2.relative_difference).ToList())
            {
                p.quant.TusherValues2.correspondingAvgSortedRelDiff = avgSortedPermutationRelativeDifferences[ct++];
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
                Parallel.ForEach(Sweet.lollipop.satisfactoryProteoforms, eP => { eP.quant.TusherValues2.significant = eP.quant.TusherValues2.roughSignificanceFDR <= Sweet.lollipop.localFdrCutoff; });
            }
            inducedOrRepressedProteins = Sweet.lollipop.getInducedOrRepressedProteins(Sweet.lollipop.satisfactoryProteoforms.Where(pf => pf.quant.TusherValues2.significant), GoAnalysis);
        }

        #endregion Public Methods
    }
}