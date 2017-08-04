using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class Log2FoldChangeValues
        : IStatisiticalSignificance
    {

        #region Fold Change Analysis Properties

        public Dictionary<Tuple<InputFile, string>, BiorepFractionTechrepIntensity> allBftIntensities { get; set; } // each bft corresponds to a file and condition
        public List<double> log2FoldChanges = new List<double>();
        public double average_log2fc { get; set; } = 0;
        public double stdev_log2fc { get; set; } = 0;
        public double tTestStatistic { get; set; } = 0;
        public double pValue_uncorrected { get; set; } = 0;
        public double benjiHoch_value { get; set; } = 0;
        public bool significant { get; set; } = false;

        #endregion Fold Change Analysis Properties

        #region Fold Change Analysis Methods

        public void impute_bft_intensities(List<BiorepFractionTechrepIntensity> bftIntensityList, List<InputFile> allFiles, Dictionary<Tuple<InputFile, string>, double> fileCondition_avgLog2I, Dictionary<Tuple<InputFile, string>, double> fileCondition_stdevLog2I)
        {
            List<BiorepFractionTechrepIntensity> imputed_bft_intensities = imputedIntensities(bftIntensityList, allFiles, fileCondition_avgLog2I, fileCondition_stdevLog2I);
            allBftIntensities = bftIntensityList.Concat(imputed_bft_intensities).ToDictionary(bft => new Tuple<InputFile, string>(bft.input_file, bft.condition), bft => bft);
        }

        public void normalize_bft_intensities(Dictionary<Tuple<string, string>, double> conditionBiorepNormalizationDivisors)
        {
            foreach (BiorepFractionTechrepIntensity bft in allBftIntensities.Values)
            {
                bft.intensity_sum = bft.intensity_sum / conditionBiorepNormalizationDivisors[new Tuple<string, string>(bft.condition, bft.input_file.biological_replicate)];
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
        public static List<BiorepFractionTechrepIntensity> imputedIntensities(List<BiorepFractionTechrepIntensity> observedBftIntensities, List<InputFile> allFiles, Dictionary<Tuple<InputFile, string>, double> fileCondition_avgLog2I, Dictionary<Tuple<InputFile, string>, double> fileCondition_stdevLog2I)
        {
            //avtIntensities are log base 2
            //stdevIntensities are log base 2

            List<BiorepFractionTechrepIntensity> intensities_without_corresponding_values = observedBftIntensities.Where(bft => !observedBftIntensities.Any(ff => bft.condition != ff.condition && bft.input_file.biological_replicate == ff.input_file.biological_replicate && bft.input_file.fraction == ff.input_file.fraction && bft.input_file.technical_replicate == ff.input_file.technical_replicate)).ToList();
            List<BiorepFractionTechrepIntensity> imputed_values = new List<BiorepFractionTechrepIntensity>();
            foreach (BiorepFractionTechrepIntensity bft in intensities_without_corresponding_values)
            {
                Tuple<InputFile, string> x = new Tuple<InputFile, string>(bft.input_file, bft.input_file.lt_condition != bft.condition ? bft.input_file.lt_condition : bft.input_file.hv_condition);
                decimal avglog2i = (decimal)fileCondition_avgLog2I[x];
                decimal stdevlog2i = (decimal)fileCondition_stdevLog2I[x];
                imputed_values.Add(new BiorepFractionTechrepIntensity(x.Item1, x.Item2, true, QuantitativeProteoformValues.imputed_intensity(avglog2i + Sweet.lollipop.backgroundShift * stdevlog2i, stdevlog2i * Sweet.lollipop.backgroundWidth, Sweet.lollipop.useRandomSeed, Sweet.lollipop.seeded)));
            }
            return imputed_values;
        }

        public void calculate_log2FoldChanges(string numerator_condition, string denominator_condition)
        {
            List<BiorepFractionTechrepIntensity> numeratorBfts = allBftIntensities.Values.Where(bft => bft.condition == numerator_condition).ToList();
            log2FoldChanges = numeratorBfts.Select(bft => Math.Log(bft.intensity_sum, 2) - Math.Log(allBftIntensities[new Tuple<InputFile, string>(bft.input_file, denominator_condition)].intensity_sum, 2)).ToList();
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
