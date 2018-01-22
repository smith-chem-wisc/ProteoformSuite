using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics;

namespace ProteoformSuiteInternal
{
    public class Log2FoldChangeValues
        : IStatisiticalSignificance
    {

        #region Fold Change Analysis Properties

        public List<BiorepIntensity> numeratorOriginalIntensities { get; set; } = new List<BiorepIntensity>();
        public List<BiorepIntensity> denominatorOriginalIntensities { get; set; } = new List<BiorepIntensity>();
        public List<BiorepIntensity> numeratorImputedIntensities { get; set; } = new List<BiorepIntensity>();
        public List<BiorepIntensity> denominatorImputedIntensities { get; set; } = new List<BiorepIntensity>();
        public Dictionary<Tuple<string, string>, BiorepIntensity> allIntensities { get; set; } = new Dictionary<Tuple<string, string>, BiorepIntensity>();
        public decimal numeratorIntensitySum { get; set; } = 0;
        public decimal denominatorIntensitySum { get; set; } = 0;

        public double tTestStatistic { get; set; } = 0;
        public double logfold2change { get; set; } = 0;
        public double pValue_uncorrected { get; set; } = 0;
        public double benjiHoch_value { get; set; } = 0;
        public bool significant { get; set; } = false;

        #endregion Fold Change Analysis Properties

        #region Fold Change Analysis Methods

        public void impute_biorep_intensities(List<BiorepIntensity> biorepIntensityList, Dictionary<string, List<string>> conditionBioReps, string numerator_condition, string denominator_condition, string induced_condition, decimal bkgdAverageIntensity, decimal bkgdStDev, decimal sKnot, bool useRandomSeed, Random seeded)
        {
            significant = false;

            numeratorImputedIntensities = imputedIntensities(numeratorOriginalIntensities, bkgdAverageIntensity, bkgdStDev, numerator_condition, conditionBioReps[numerator_condition], useRandomSeed, seeded);
            List<BiorepIntensity> allNumeratorIntensities = numeratorOriginalIntensities.Concat(numeratorImputedIntensities).ToList();
            numeratorIntensitySum = (decimal)numeratorOriginalIntensities.Sum(i => i.intensity_sum) + (decimal)numeratorImputedIntensities.Sum(i => i.intensity_sum);

            denominatorImputedIntensities = imputedIntensities(denominatorOriginalIntensities, bkgdAverageIntensity, bkgdStDev, denominator_condition, conditionBioReps[denominator_condition], useRandomSeed, seeded);
            List<BiorepIntensity> allDenominatorIntensities = denominatorOriginalIntensities.Concat(denominatorImputedIntensities).ToList();
            denominatorIntensitySum = (decimal)denominatorOriginalIntensities.Sum(i => i.intensity_sum) + (decimal)denominatorImputedIntensities.Sum(i => i.intensity_sum);

            allIntensities = allNumeratorIntensities.Concat(allDenominatorIntensities).ToDictionary(x => new Tuple<string, string>(x.condition, x.biorep), x => x);
        }

        /// <summary>
        /// Returns imputed intensities for a certain condition for biological replicates this proteoform was not observed in.
        /// </summary>
        /// <param name="observedBftIntensities"></param>
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


        public void calcluate_statistics(string numerator_condition, string denominator_condition)
        {
            List<BiorepIntensity> numeratorBfts = allIntensities.Values.Where(bft => bft.condition == numerator_condition).ToList();
            List<BiorepIntensity> denominatorBfts = allIntensities.Values.Where(bft => bft.condition == denominator_condition).ToList();
            List<double> numerator_log2_values = numeratorBfts.Select(bft => bft.biorep).Distinct().Select(n => numeratorBfts.Where(bft => bft.biorep == n).Sum(bft => bft.intensity_sum)).Select(v => Math.Log(v, 2)).ToList();
            List<double> denomenator_log2_values = denominatorBfts.Select(bft => bft.biorep).Distinct().Select(n => denominatorBfts.Where(bft => bft.biorep == n).Sum(bft => bft.intensity_sum)).Select(v => Math.Log(v, 2)).ToList();
            logfold2change = numerator_log2_values.Average() - denomenator_log2_values.Average();

            double stdev_numerator = MathNet.Numerics.Statistics.Statistics.StandardDeviation(numerator_log2_values);
            double stdev_denominator = MathNet.Numerics.Statistics.Statistics.StandardDeviation(denomenator_log2_values);
            double sp2 = ((numerator_log2_values.Count - 1)*Math.Pow(stdev_numerator, 2) + (denomenator_log2_values.Count - 1)*Math.Pow(stdev_denominator, 2))
                            / (numerator_log2_values.Count + denomenator_log2_values.Count - 2);
            tTestStatistic = logfold2change / (Math.Sqrt(sp2) * Math.Sqrt(((double)1/numerator_log2_values.Count) + ((double)1)/denomenator_log2_values.Count));
        }

        #endregion Fold Change Analysis Methods

    }
}
