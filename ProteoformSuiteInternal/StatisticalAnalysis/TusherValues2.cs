using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class TusherValues2
        : TusherValues
    {

        #region Tusher Analysis Properties

        public List<BiorepTechrepIntensity> numeratorOriginalIntensities { get; set; } = new List<BiorepTechrepIntensity>();
        public List<BiorepTechrepIntensity> denominatorOriginalIntensities { get; set; } = new List<BiorepTechrepIntensity>();
        public List<BiorepTechrepIntensity> numeratorImputedIntensities { get; set; } = new List<BiorepTechrepIntensity>();
        public List<BiorepTechrepIntensity> denominatorImputedIntensities { get; set; } = new List<BiorepTechrepIntensity>();
        public Dictionary<Tuple<string, string, string>, BiorepTechrepIntensity> allIntensities { get; set; } = new Dictionary<Tuple<string, string, string>, BiorepTechrepIntensity>();

        #endregion Tusher Analysis Properties

        #region Public Methods

        public void impute_biorep_intensities(List<BiorepTechrepIntensity> intensities, Dictionary<string, List<string>> conditionBioReps, string numerator_condition, string denominator_condition, string induced_condition, decimal bkgdAverageIntensity, decimal bkgdStDev, decimal sKnot, bool useRandomSeed, Random seeded)
        {
            //bkgdAverageIntensity is log base 2
            //bkgdStDev is log base 2

            significant = false;
            numeratorOriginalIntensities = intensities.Where(b => b.condition == numerator_condition).Select(x => new BiorepTechrepIntensity(x.imputed, x.biorep, x.condition, x.techrep, x.intensity_sum)).ToList(); // normalized, so create new objects
            numeratorImputedIntensities = imputedIntensities(numeratorOriginalIntensities, Sweet.lollipop.get_files(Sweet.lollipop.input_files,  Purpose.Quantification), bkgdAverageIntensity, bkgdStDev, numerator_condition, conditionBioReps[numerator_condition], Sweet.lollipop.useRandomSeed_quant, seeded);
            numeratorIntensitySum = (decimal)numeratorOriginalIntensities.Sum(i => i.intensity_sum) + (decimal)numeratorImputedIntensities.Sum(i => i.intensity_sum);
            List<BiorepTechrepIntensity> allNumeratorIntensities = numeratorOriginalIntensities.Concat(numeratorImputedIntensities).ToList();

            denominatorOriginalIntensities = intensities.Where(b => b.condition == denominator_condition).Select(x => new BiorepTechrepIntensity(x.imputed, x.biorep, x.condition, x.techrep, x.intensity_sum)).ToList(); // normalized, so create new objects
            denominatorImputedIntensities = imputedIntensities(denominatorOriginalIntensities, Sweet.lollipop.get_files(Sweet.lollipop.input_files,  Purpose.Quantification), bkgdAverageIntensity, bkgdStDev, denominator_condition, conditionBioReps[denominator_condition], Sweet.lollipop.useRandomSeed_quant, seeded);
            denominatorIntensitySum = (decimal)denominatorOriginalIntensities.Sum(i => i.intensity_sum) + (decimal)denominatorImputedIntensities.Sum(i => i.intensity_sum);
            List<BiorepTechrepIntensity> allDenominatorIntensities = denominatorOriginalIntensities.Concat(denominatorImputedIntensities).ToList();

            allIntensities = allNumeratorIntensities.Concat(allDenominatorIntensities).ToDictionary(x => new Tuple<string, string, string>(x.condition, x.biorep, x.techrep), x => x);
        }

        public void determine_proteoform_statistics(string induced_condition, decimal sKnot)
        {
            List<BiorepTechrepIntensity> allNumeratorIntensities = numeratorOriginalIntensities.Concat(numeratorImputedIntensities).ToList();
            List<BiorepTechrepIntensity> allDenominatorIntensities = denominatorOriginalIntensities.Concat(denominatorImputedIntensities).ToList();

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
        public static List<BiorepTechrepIntensity> imputedIntensities(List<BiorepTechrepIntensity> observedBioreps, IEnumerable<InputFile> files, decimal bkgdAverageIntensity, decimal bkgdStDev, string condition, List<string> bioreps, bool useRandomSeed, Random seeded)
        {
            //bkgdAverageIntensity is log base 2
            //bkgdStDev is log base 2

            List<Tuple<string, string>> bt = files.Select(x => new Tuple<string, string>(x.biological_replicate, x.technical_replicate)).Distinct().ToList();

            return (
                from x in bt
                where !observedBioreps.Any(k => k.condition == condition && k.biorep == x.Item1 && k.techrep == x.Item2)
                select new BiorepTechrepIntensity(true, x.Item1, condition, x.Item2, QuantitativeProteoformValues.imputed_intensity(bkgdAverageIntensity, bkgdStDev, useRandomSeed, seeded)))
                .ToList();
        }

        #endregion Public Methods

    }
}
