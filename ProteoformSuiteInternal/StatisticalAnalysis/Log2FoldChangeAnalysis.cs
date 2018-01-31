using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class Log2FoldChangeAnalysis
        : IGoAnalysis
    {

        #region Public Properties

        public Dictionary<Tuple<string, string>, double> conditionBiorep_avgLog2I { get; set; } = new Dictionary<Tuple<string, string>, double>(); // used to impute bft-intensities
        public Dictionary<Tuple<string, string>, double> conditionBiorep_stdevLog2I { get; set; } = new Dictionary<Tuple<string, string>, double>(); // used to impute bft-intensities
        public Dictionary<Tuple<string, string>, double> conditionBiorepIntensitySums { get; set; } = new Dictionary<Tuple<string, string>, double>(); // used to normalize columns
        public double benjiHoch_fdr { get; set; } = 0.05;
        public double minFoldChange { get; set; } = 2.0;
        public List<ProteinWithGoTerms> inducedOrRepressedProteins { get; set; } = new List<ProteinWithGoTerms>(); // This is the list of proteins from proteoforms that underwent significant induction or repression
        public GoAnalysis GoAnalysis { get; set; } = new GoAnalysis();
        public QuantitativeDistributions QuantitativeDistributions { get; set; }

        #endregion Public Properties

        #region Public Method

        public Log2FoldChangeAnalysis()
        {
            this.QuantitativeDistributions = new QuantitativeDistributions(this);
        }

        public void compute_proteoform_statistics(List<ExperimentalProteoform> satisfactoryProteoforms, Dictionary<string, List<string>> conditionsBioReps, string numerator_condition, string denominator_condition, string induced_condition, decimal sKnot_minFoldChange, bool define_histogram)
        {
            //Not all proteoforms were observed in each fraction, so we have to be careful about finding the intensities
            Dictionary<Tuple<string, string>, List<double>> conditionBiorep_intensities = new Dictionary<Tuple<string, string>, List<double>>();
            Dictionary<Tuple<string, string>, List<double>> conditionbiorep_square_standard_differences = new Dictionary<Tuple<string, string>, List<double>>();

            Parallel.ForEach(Sweet.lollipop.satisfactoryProteoforms, pf =>
            {
                pf.quant.Log2FoldChangeValues.numeratorOriginalIntensities = pf.biorepIntensityList.Where(b => b.condition == numerator_condition).Select(x => new BiorepIntensity(x.imputed, x.biorep, x.condition, x.intensity_sum)).ToList();
                pf.quant.Log2FoldChangeValues.denominatorOriginalIntensities = pf.biorepIntensityList.Where(b => b.condition == denominator_condition).Select(x => new BiorepIntensity(x.imputed, x.biorep, x.condition, x.intensity_sum)).ToList();
            });

            // Calculate normalization factors (sum condition-biorep intensity / average intensity from that biorep)
            normalize_bft_intensities(Sweet.lollipop.satisfactoryProteoforms);

            //do after normalizing because impute after normalizing
            QuantitativeDistributions.defineAllObservedIntensityDistribution(Sweet.lollipop.target_proteoform_community.experimental_proteoforms.SelectMany(pf => pf.quant.Log2FoldChangeValues.numeratorOriginalIntensities.Concat(pf.quant.Log2FoldChangeValues.denominatorOriginalIntensities)), QuantitativeDistributions.logIntensityHistogram);
            QuantitativeDistributions.defineSelectObservedIntensityDistribution(Sweet.lollipop.satisfactoryProteoforms.SelectMany(pf => pf.quant.Log2FoldChangeValues.numeratorOriginalIntensities.Concat(pf.quant.Log2FoldChangeValues.denominatorOriginalIntensities)), QuantitativeDistributions.logSelectIntensityHistogram);
            QuantitativeDistributions.defineBackgroundIntensityDistribution(Sweet.lollipop.quantBioFracCombos, Sweet.lollipop.satisfactoryProteoforms, Sweet.lollipop.condition_count, Sweet.lollipop.backgroundShift, Sweet.lollipop.backgroundWidth);

            // Use those values to impute missing ones
            foreach (var pf in Sweet.lollipop.satisfactoryProteoforms)
            {
                pf.quant.Log2FoldChangeValues.impute_biorep_intensities(pf.quant.Log2FoldChangeValues.numeratorOriginalIntensities.Concat(pf.quant.Log2FoldChangeValues.denominatorOriginalIntensities).ToList(), conditionsBioReps, numerator_condition, denominator_condition, induced_condition, QuantitativeDistributions.bkgdAverageIntensity, QuantitativeDistributions.bkgdStDev, sKnot_minFoldChange, Sweet.lollipop.useRandomSeed_quant, Sweet.lollipop.seeded);
            }
            QuantitativeDistributions.defineSelectObservedWithImputedIntensityDistribution(Sweet.lollipop.satisfactoryProteoforms.SelectMany(pf => pf.quant.Log2FoldChangeValues.allIntensities.Values), QuantitativeDistributions.logSelectIntensityWithImputationHistogram);

            // Calculate log2 fold changes
            Parallel.ForEach(Sweet.lollipop.satisfactoryProteoforms, pf =>
            {
                pf.quant.Log2FoldChangeValues.calcluate_statistics(Sweet.lollipop.numerator_condition, Sweet.lollipop.denominator_condition);
                int degrees_of_freedom = Sweet.lollipop.conditionsBioReps.Values.SelectMany(x => x).Count() - 2; //degrees of freedom for unpaired 2 samples t test = n1 + n2 - 2
                pf.quant.Log2FoldChangeValues.pValue_uncorrected =  ExtensionMethods.Student2T(pf.quant.Log2FoldChangeValues.tTestStatistic, degrees_of_freedom); // using a two-tailed test. Null hypothesis is that our value x is equal to the mean. Alternative hypothesis is in either direction.

                if (pf.quant.Log2FoldChangeValues.pValue_uncorrected <= 0)
                    pf.quant.Log2FoldChangeValues.pValue_uncorrected = Double.Epsilon;
                if (pf.quant.Log2FoldChangeValues.pValue_uncorrected > 1)
                    pf.quant.Log2FoldChangeValues.pValue_uncorrected = 1;
            });

            // Benjimini-Hochberg correction
            establish_benjiHoch_significance();
        }

        public void establish_benjiHoch_significance()
        {
            double benjiHoch_criticalValue = 0;
            int rank = 1;
            foreach (ExperimentalProteoform pf in Sweet.lollipop.satisfactoryProteoforms.OrderBy(x => x.quant.Log2FoldChangeValues.pValue_uncorrected))
            {
                pf.quant.Log2FoldChangeValues.benjiHoch_value = (double)rank++ / Sweet.lollipop.satisfactoryProteoforms.Count * benjiHoch_fdr;
                if (pf.quant.Log2FoldChangeValues.pValue_uncorrected < pf.quant.Log2FoldChangeValues.benjiHoch_value)
                    benjiHoch_criticalValue = pf.quant.Log2FoldChangeValues.benjiHoch_value;
            }

            foreach (ExperimentalProteoform pf in Sweet.lollipop.satisfactoryProteoforms)
            {
                pf.quant.Log2FoldChangeValues.significant = Math.Abs(pf.quant.Log2FoldChangeValues.logfold2change) >= minFoldChange && pf.quant.Log2FoldChangeValues.benjiHoch_value <= benjiHoch_criticalValue; // every test at or below the critical value is significant, even if the p-value is greater than its benjiHoch value
            }

            inducedOrRepressedProteins = Sweet.lollipop.getInducedOrRepressedProteins(Sweet.lollipop.satisfactoryProteoforms.Where(pf => pf.quant.Log2FoldChangeValues.significant), GoAnalysis);
            GoAnalysis.GO_analysis(inducedOrRepressedProteins);
        }


        public void normalize_bft_intensities(List<ExperimentalProteoform> satisfactoryProteoforms)
        {
            // Calculate avgLog2Intensity and stdevLog2Intensity for each file-condition --> for normalize
            Dictionary<Tuple<string, string>, List<double>> conditionBiorep_intensities = new Dictionary<Tuple<string, string>, List<double>>();
            List<BiorepIntensity> allOriginalIntensities = satisfactoryProteoforms.SelectMany(pf => pf.quant.Log2FoldChangeValues.numeratorOriginalIntensities.Concat(pf.quant.Log2FoldChangeValues.denominatorOriginalIntensities)).ToList();
            foreach (BiorepIntensity bft in allOriginalIntensities)
            {
                Tuple<string, string> key2 = new Tuple<string, string>(bft.condition, bft.biorep);
                bool yes = conditionBiorep_intensities.TryGetValue(key2, out List<double> intensities2);
                if (yes) intensities2.Add(bft.intensity_sum);
                else conditionBiorep_intensities.Add(key2, new List<double> { bft.intensity_sum });
            }
            conditionBiorepIntensitySums = conditionBiorep_intensities.ToDictionary(kv => kv.Key, kv => kv.Value.Sum()); // this is the linear intensity sum
            foreach (BiorepIntensity bft in allOriginalIntensities)
            {
                double norm_divisor = conditionBiorepIntensitySums[new Tuple<string, string>(bft.condition, bft.biorep)] /
                    (Sweet.lollipop.neucode_labeled ? conditionBiorepIntensitySums.Where(kv => kv.Key.Item2 == bft.biorep).Average(kv => kv.Value) : conditionBiorepIntensitySums.Average(kv => kv.Value));
                bft.intensity_sum = bft.intensity_sum / norm_divisor;
            }
            #endregion Public Method
        }
    }
}
