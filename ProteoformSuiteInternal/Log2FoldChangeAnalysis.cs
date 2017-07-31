using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public static class Log2FoldChangeAnalysis
    {

        #region Public Method

        public static void calculate_log2fc_statistics()
        {
            //Not all proteoforms were observed in each fraction, so we have to be careful about finding the intensities
            Dictionary<Tuple<string, string>, List<double>> conditionBiorep_intensities = new Dictionary<Tuple<string, string>, List<double>>();
            Dictionary<Tuple<InputFile, string>, List<double>> fileCondition_intensities = new Dictionary<Tuple<InputFile, string>, List<double>>();
            Dictionary<Tuple<InputFile, string>, List<double>> fileCondition_square_standard_differences = new Dictionary<Tuple<InputFile, string>, List<double>>();

            // Calculate avgLog2Intensity and stdevLog2Intensity for each file-condition
            List<BiorepFractionTechrepIntensity> allOriginalBfts = Sweet.lollipop.satisfactoryProteoforms.SelectMany(pf => pf.bftIntensityList).ToList();
            foreach (BiorepFractionTechrepIntensity bft in allOriginalBfts)
            {
                Tuple<InputFile, string> key1 = new Tuple<InputFile, string>(bft.input_file, bft.condition);
                bool yep = fileCondition_intensities.TryGetValue(key1, out List<double> intensities1);
                if (yep) intensities1.Add(bft.intensity_sum);
                else fileCondition_intensities.Add(key1, new List<double> { bft.intensity_sum });

                Tuple<string, string> key2 = new Tuple<string, string>(bft.condition, bft.input_file.biological_replicate);
                bool yes = conditionBiorep_intensities.TryGetValue(key2, out List<double> intensities2);
                if (yes) intensities2.Add(bft.intensity_sum);
                else conditionBiorep_intensities.Add(key2, new List<double> { bft.intensity_sum });
            }

            Sweet.lollipop.fileCondition_avgLog2I = fileCondition_intensities.ToDictionary(kv => kv.Key, kv => kv.Value.Average(x => Math.Log(x, 2)));
            Sweet.lollipop.conditionBiorepIntensitySums = conditionBiorep_intensities.ToDictionary(kv => kv.Key, kv => kv.Value.Sum()); // this is the linear intensity sum

            foreach (BiorepFractionTechrepIntensity bft in allOriginalBfts)
            {
                Tuple<InputFile, string> x = new Tuple<InputFile, string>(bft.input_file, bft.condition);
                double value = Math.Pow(Math.Log(bft.intensity_sum, 2) - Sweet.lollipop.fileCondition_avgLog2I[x], 2);
                bool yep = fileCondition_square_standard_differences.TryGetValue(x, out List<double> std_diffs);
                if (yep) std_diffs.Add(value);
                else fileCondition_square_standard_differences.Add(x, new List<double> { value });
            }

            Sweet.lollipop.fileCondition_stdevLog2I = fileCondition_square_standard_differences.ToDictionary(kv => kv.Key, kv => Math.Sqrt(kv.Value.Sum() / (kv.Value.Count - 1)));

            // Use those values to impute missing ones
            Parallel.ForEach(Sweet.lollipop.satisfactoryProteoforms, pf =>
                pf.quant.Log2FoldChangeValues.impute_bft_intensities(pf.bftIntensityList, Sweet.lollipop.input_files, Sweet.lollipop.fileCondition_avgLog2I, Sweet.lollipop.fileCondition_stdevLog2I));

            // Calculate normalization factors (sum condition-biorep intensity / average intensity from that biorep)
            Sweet.lollipop.conditionBiorepNormalizationDivisors = Sweet.lollipop.conditionBiorepIntensitySums.ToDictionary(kv => kv.Key, kv => kv.Value / Sweet.lollipop.conditionBiorepIntensitySums.Where(cbi => cbi.Key.Item2 == kv.Key.Item2).Average(cbi => cbi.Value));
            Parallel.ForEach(Sweet.lollipop.satisfactoryProteoforms, pf =>
                pf.quant.Log2FoldChangeValues.normalize_bft_intensities(Sweet.lollipop.conditionBiorepNormalizationDivisors));

            // Calculate log2 fold changes
            Parallel.ForEach(Sweet.lollipop.satisfactoryProteoforms, pf =>
                pf.quant.Log2FoldChangeValues.calculate_log2FoldChanges(Sweet.lollipop.numerator_condition, Sweet.lollipop.denominator_condition));
            List<double> all_log2fc = Sweet.lollipop.satisfactoryProteoforms.SelectMany(pf => pf.quant.Log2FoldChangeValues.log2FoldChanges).ToList();
            Sweet.lollipop.log2FC_population_average = all_log2fc.Average(); // caution: average of averages is usally incorrect
            Sweet.lollipop.log2FC_population_stdev = Math.Sqrt(all_log2fc.Sum(x => Math.Pow(x - Sweet.lollipop.log2FC_population_average, 2)) / all_log2fc.Count); // caution: average of averages is usally incorrect

            // Calculate p-values
            foreach (ExperimentalProteoform pf in Sweet.lollipop.satisfactoryProteoforms)
            {
                pf.quant.Log2FoldChangeValues.tTestStatistic = (pf.quant.Log2FoldChangeValues.average_log2fc - Sweet.lollipop.log2FC_population_average) / (pf.quant.Log2FoldChangeValues.stdev_log2fc / Math.Sqrt(pf.quant.Log2FoldChangeValues.log2FoldChanges.Count));
                pf.quant.Log2FoldChangeValues.pValue_uncorrected = ExtensionMethods.Student2T(pf.quant.Log2FoldChangeValues.tTestStatistic, Sweet.lollipop.conditionsBioReps.Values.SelectMany(x => x).Distinct().Count() - 1); // using a two-tailed test. Null hypothesis is that our value x is equal to the mean. Alternative hypothesis is in either direction.

                if (pf.quant.Log2FoldChangeValues.pValue_uncorrected <= 0)
                    pf.quant.Log2FoldChangeValues.pValue_uncorrected = Double.Epsilon;
                if (pf.quant.Log2FoldChangeValues.pValue_uncorrected > 1)
                    pf.quant.Log2FoldChangeValues.pValue_uncorrected = 1;
            }

            // Benjimini-Hochberg correction
            establish_benjiHoch_significance();
        }

        public static void establish_benjiHoch_significance()
        {
            double benjiHoch_criticalValue = 0;
            int rank = 1;
            foreach (ExperimentalProteoform pf in Sweet.lollipop.satisfactoryProteoforms.OrderBy(x => x.quant.Log2FoldChangeValues.pValue_uncorrected))
            {
                pf.quant.Log2FoldChangeValues.benjiHoch_value = (double)rank++ / Sweet.lollipop.satisfactoryProteoforms.Count * Sweet.lollipop.benjiHoch_fdr;
                if (pf.quant.Log2FoldChangeValues.pValue_uncorrected < pf.quant.Log2FoldChangeValues.benjiHoch_value)
                    benjiHoch_criticalValue = pf.quant.Log2FoldChangeValues.benjiHoch_value;
            }

            foreach (ExperimentalProteoform pf in Sweet.lollipop.satisfactoryProteoforms)
            {
                pf.quant.Log2FoldChangeValues.significant = pf.quant.Log2FoldChangeValues.benjiHoch_value <= benjiHoch_criticalValue; // every test at or below the critical value is significant, even if the p-value is greater than its benjiHoch value
            }
        }

        #endregion Public Method

    }
}
