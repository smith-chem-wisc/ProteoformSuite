using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class Log2FoldChangeValues
        : IStatisiticalSignificance
    {

        #region Fold Change Analysis Properties

        public Dictionary<Tuple<string, string, string, string>, BiorepFractionTechrepIntensity> allBftIntensities { get; set; } = new Dictionary<Tuple<string, string, string, string>, BiorepFractionTechrepIntensity>(); // each bft corresponds to a file and condition
        public List<double> log2FoldChanges = new List<double>();
        public double average_log2fc { get; set; } = 0;
        public double stdev_log2fc { get; set; } = 0;
        public double tTestStatistic { get; set; } = 0;
        public double pValue_uncorrected { get; set; } = 0;
        public double benjiHoch_value { get; set; } = 0;
        public bool significant { get; set; } = false;

        #endregion Fold Change Analysis Properties

        #region Fold Change Analysis Methods

        public void impute_bft_intensities(List<BiorepFractionTechrepIntensity> bftIntensityList, List<InputFile> allFiles, Dictionary<Tuple<string, string, string, string>, double> fileCondition_avgLog2I, Dictionary<Tuple<string, string, string, string>, double> fileCondition_stdevLog2I)
        {
            List<BiorepFractionTechrepIntensity> imputed_bft_intensities = imputedIntensities(bftIntensityList, allFiles, fileCondition_avgLog2I, fileCondition_stdevLog2I);
            allBftIntensities = bftIntensityList.Concat(imputed_bft_intensities).ToDictionary(bft => new Tuple<string, string, string, string>(bft.condition, bft.biorep, bft.fraction, bft.techrep), bft => bft);
        }

        public void normalize_bft_intensities(Dictionary<Tuple<string, string>, double> conditionBiorepNormalizationDivisors)
        {
            foreach (BiorepFractionTechrepIntensity bft in allBftIntensities.Values)
            {
                bft.intensity_sum = bft.intensity_sum / conditionBiorepNormalizationDivisors[new Tuple<string, string>(bft.condition, bft.biorep)];
            }
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
        public static List<BiorepFractionTechrepIntensity> imputedIntensities(List<BiorepFractionTechrepIntensity> observedBftIntensities, List<InputFile> allFiles, Dictionary<Tuple<string, string, string, string>, double> fileCondition_avgLog2I, Dictionary<Tuple<string, string, string, string>, double> fileCondition_stdevLog2I)
        {
            //avtIntensities are log base 2
            //stdevIntensities are log base 2

            List<BiorepFractionTechrepIntensity> intensities_without_corresponding_values = observedBftIntensities.Where(bft => !observedBftIntensities.Any(ff => bft.condition != ff.condition && bft.biorep == ff.biorep && bft.fraction == ff.fraction && bft.techrep == ff.techrep)).ToList();
            List<BiorepFractionTechrepIntensity> imputed_values = new List<BiorepFractionTechrepIntensity>();
            foreach (BiorepFractionTechrepIntensity bft in intensities_without_corresponding_values)
            {
                string condition = Sweet.lollipop.ltConditionsBioReps.Concat(Sweet.lollipop.hvConditionsBioReps).Select(c => c.Key).Distinct().Where(c => c != bft.condition).First();
                Tuple<string, string, string, string> x = new Tuple<string, string, string, string>(condition, bft.biorep, bft.fraction, bft.techrep);
                decimal avglog2i = (decimal)fileCondition_avgLog2I[x];
                decimal stdevlog2i = (decimal)fileCondition_stdevLog2I[x];
                imputed_values.Add(new BiorepFractionTechrepIntensity(x.Item1, x.Item2, x.Item3, x.Item4, true, QuantitativeProteoformValues.imputed_intensity(avglog2i + Sweet.lollipop.backgroundShift * stdevlog2i, stdevlog2i * Sweet.lollipop.backgroundWidth, Sweet.lollipop.useRandomSeed_quant, Sweet.lollipop.seeded)));
            }
            return imputed_values;
        }

        public void calculate_log2FoldChanges(string numerator_condition, string denominator_condition)
        {
            List<BiorepFractionTechrepIntensity> numeratorBfts = allBftIntensities.Values.Where(bft => bft.condition == numerator_condition).ToList();
           if (Sweet.lollipop.neucode_labeled)
            {
                log2FoldChanges = numeratorBfts.Select(bft => Math.Log(bft.intensity_sum, 2) - Math.Log(allBftIntensities[new Tuple<string, string, string, string>(denominator_condition, bft.biorep, bft.fraction, bft.techrep)].intensity_sum, 2)).ToList();
            }
            else
            {
                List<BiorepFractionTechrepIntensity> denominatorBfts = allBftIntensities.Values.Where(bft => bft.condition == denominator_condition).ToList();
                foreach (var br in numeratorBfts.Select(b => b.biorep).Distinct())
                {
                    log2FoldChanges.Add(Math.Log(numeratorBfts.Where(bft => bft.biorep == br).Sum(bft => bft.intensity_sum), 2) - Math.Log(denominatorBfts.Where(bft => bft.biorep == br).Sum(bft => bft.intensity_sum), 2));
                }
            }
            average_log2fc = log2FoldChanges.Average();
            stdev_log2fc = Math.Sqrt(log2FoldChanges.Sum(fc => Math.Pow(fc - average_log2fc, 2)) / (log2FoldChanges.Count - 1));
        }

        public void calculate_log2TestStatistics(double log2FC_population_average)
        {
            tTestStatistic = (average_log2fc - log2FC_population_average) / (stdev_log2fc / Math.Sqrt(log2FoldChanges.Count));
        }

        #endregion Fold Change Analysis Methods

    }
}
