using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class TusherValues1
        : TusherValues
    {

        #region Tusher Analysis Properties

        public List<BiorepIntensity> numeratorOriginalIntensities { get; set; } = new List<BiorepIntensity>();
        public List<BiorepIntensity> denominatorOriginalIntensities { get; set; } = new List<BiorepIntensity>();
        public List<BiorepIntensity> numeratorImputedIntensities { get; set; } = new List<BiorepIntensity>();
        public List<BiorepIntensity> denominatorImputedIntensities { get; set; } = new List<BiorepIntensity>();
        public Dictionary<Tuple<string, string>, BiorepIntensity> allIntensities { get; set; } = new Dictionary<Tuple<string, string>, BiorepIntensity>();

        #endregion Tusher Analysis Properties

        #region Public Methods

        public void impute_biorep_intensities(List<BiorepIntensity> biorepIntensityList, Dictionary<string, List<string>> conditionBioReps, string numerator_condition, string denominator_condition, string induced_condition, decimal bkgdAverageIntensity, decimal bkgdStDev, decimal sKnot, bool useRandomSeed, Random seeded)
        {
            //bkgdAverageIntensity is log base 2
            //bkgdStDev is log base 2

            significant = false;
            numeratorOriginalIntensities = biorepIntensityList.Where(b => b.condition == numerator_condition).Select(x => new BiorepIntensity(x.imputed, x.biorep, x.condition, x.intensity_sum)).ToList(); // normalized, so create new objects
            numeratorImputedIntensities = imputedIntensities(numeratorOriginalIntensities, bkgdAverageIntensity, bkgdStDev, numerator_condition, conditionBioReps[numerator_condition], useRandomSeed, seeded);
            numeratorIntensitySum = (decimal)numeratorOriginalIntensities.Sum(i => i.intensity_sum) + (decimal)numeratorImputedIntensities.Sum(i => i.intensity_sum);
            List<BiorepIntensity> allNumeratorIntensities = numeratorOriginalIntensities.Concat(numeratorImputedIntensities).ToList();

            denominatorOriginalIntensities = biorepIntensityList.Where(b => b.condition == denominator_condition).Select(x => new BiorepIntensity(x.imputed, x.biorep, x.condition, x.intensity_sum)).ToList(); // normalized, so create new objects
            denominatorImputedIntensities = imputedIntensities(denominatorOriginalIntensities, bkgdAverageIntensity, bkgdStDev, denominator_condition, conditionBioReps[denominator_condition], useRandomSeed, seeded);
            denominatorIntensitySum = (decimal)denominatorOriginalIntensities.Sum(i => i.intensity_sum) + (decimal)denominatorImputedIntensities.Sum(i => i.intensity_sum);
            List<BiorepIntensity> allDenominatorIntensities = denominatorOriginalIntensities.Concat(denominatorImputedIntensities).ToList();

            allIntensities = allNumeratorIntensities.Concat(allDenominatorIntensities).ToDictionary(x => new Tuple<string, string>(x.condition, x.biorep), x => x);
        }

        public void determine_proteoform_statistics(string induced_condition, decimal sKnot)
        {
            List<BiorepIntensity> allNumeratorIntensities = numeratorOriginalIntensities.Concat(numeratorImputedIntensities).ToList();
            List<BiorepIntensity> allDenominatorIntensities = denominatorOriginalIntensities.Concat(denominatorImputedIntensities).ToList();

            // We are using linear intensities, like in Tusher et al. (2001).
            // This is a non-parametric test, and so it makes no assumptions about the incoming probability distribution, unlike a simple t-test.
            // Therefore, the right-skewed intensity distributions is okay for this test.
            scatter = StdDev(allNumeratorIntensities, allDenominatorIntensities);
            List<IBiorepIntensity> induced = allIntensities.Where(kv => kv.Key.Item1 == induced_condition).Select(kv => kv.Value).ToList<IBiorepIntensity>();
            List<IBiorepIntensity> uninduced = allIntensities.Where(kv => kv.Key.Item1 != induced_condition).Select(kv => kv.Value).ToList<IBiorepIntensity>();
            relative_difference = getSingleTestStatistic(induced, uninduced, scatter, sKnot);
            fold_change = getSingleFoldChange(induced, uninduced);
            List<decimal> biorep_foldchanges = allNumeratorIntensities.Select(x => x.biorep).Distinct().Select(biorep => getSingleFoldChange(allIntensities.Where(kv => kv.Key.Item1 == induced_condition && kv.Key.Item2 == biorep).Select(kv => kv.Value).ToList<IBiorepIntensity>(), allIntensities.Where(kv => kv.Key.Item1 != induced_condition && kv.Key.Item2 == biorep).Select(kv => kv.Value).ToList<IBiorepIntensity>())).ToList();
            tusher_statistic = new TusherStatistic(relative_difference, fold_change, biorep_foldchanges);
        }

        /// <summary>
        /// Returns imputed intensities for a certain condition for biological replicates this proteoform was not observed in.
        /// </summary>
        /// <param name="observedBioreps"></param>
        /// <param name="bkgdAverageIntensity"></param>
        /// <param name="bkgdStDev"></param>
        /// <param name="condition"></param>
        /// <param name="bioreps"></param>
        /// <returns></returns>
        public static List<BiorepIntensity> imputedIntensities(IEnumerable<BiorepIntensity> observedBioreps, decimal bkgdAverageIntensity, decimal bkgdStDev, string condition, List<string> bioreps, bool useRandomSeed, Random seeded)
        {
            //bkgdAverageIntensity is log base 2
            //bkgdStDev is log base 2

            return (
                from biorep in bioreps
                where !observedBioreps.Any(k => k.condition == condition && k.biorep == biorep)
                select new BiorepIntensity(true, biorep, condition, QuantitativeProteoformValues.imputed_intensity(bkgdAverageIntensity, bkgdStDev, useRandomSeed, seeded)))
                .ToList();
        }

        #endregion Public Methods

    }
}
