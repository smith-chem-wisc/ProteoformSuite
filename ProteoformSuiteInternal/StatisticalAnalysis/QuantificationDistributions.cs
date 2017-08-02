using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class QuantitativeDistributions
    {

        #region Public Fields

        public TusherAnalysis analysis;

        // Histograms
        public SortedDictionary<decimal, int> logIntensityHistogram = new SortedDictionary<decimal, int>(); // all intensities
        public SortedDictionary<decimal, int> logSelectIntensityHistogram = new SortedDictionary<decimal, int>(); // selected intensities
        public SortedDictionary<decimal, int> logSelectIntensityWithImputationHistogram = new SortedDictionary<decimal, int>(); // selected intensities & imputed intensities

        // Gaussian fit to histograms
        public decimal allObservedAverageIntensity; //log base 2
        public decimal allObservedStDev;
        public decimal allObservedGaussianArea;
        public decimal allObservedGaussianHeight;

        public decimal selectAverageIntensity; //log base 2
        public decimal selectStDev;
        public decimal selectGaussianArea;
        public decimal selectGaussianHeight;

        public decimal selectWithImputationAverageIntensity;
        public decimal selectWithImputationStDev;
        public decimal selectWithImputationGaussianArea;
        public decimal selectWithImputationGaussianHeight;

        public decimal bkgdAverageIntensity; //log base 2
        public decimal bkgdStDev;
        public decimal bkgdGaussianHeight;

        #endregion Public Fields

        #region Public Constructor

        public QuantitativeDistributions(TusherAnalysis analysis)
        {
            this.analysis = analysis;
        }

        #endregion Public Constructor

        #region Public Methods

        public void defineAllObservedIntensityDistribution(IEnumerable<ExperimentalProteoform> experimental_proteoforms, SortedDictionary<decimal, int> logIntensityHistogram) // the distribution of all observed experimental proteoform biorep intensities
        {
            IEnumerable<decimal> allIntensities = analysis as TusherAnalysis1 != null ?
                define_intensity_distribution(experimental_proteoforms.SelectMany(pf => pf.biorepIntensityList), logIntensityHistogram).Where(i => i > 1) : //these are log2 values
                define_intensity_distribution(experimental_proteoforms.SelectMany(pf => pf.biorepTechrepIntensityList), logIntensityHistogram).Where(i => i > 1); //these are log2 values
            allObservedAverageIntensity = allIntensities.Average();
            allObservedStDev = (decimal)Math.Sqrt(allIntensities.Average(v => Math.Pow((double)(v - allObservedAverageIntensity), 2))); //population stdev calculation, rather than sample
            allObservedGaussianArea = get_gaussian_area(logIntensityHistogram);
            allObservedGaussianHeight = allObservedGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)allObservedStDev, 2));
        }

        public void defineSelectObservedIntensityDistribution(IEnumerable<ExperimentalProteoform> satisfactory_proteoforms, SortedDictionary<decimal, int> logSelectIntensityHistogram)
        {
            IEnumerable<decimal> allRoundedIntensities = analysis as TusherAnalysis1 != null ?
                define_intensity_distribution(satisfactory_proteoforms.SelectMany(pf => pf.biorepIntensityList), logSelectIntensityHistogram).Where(i => i > 1) : //these are log2 values
                define_intensity_distribution(satisfactory_proteoforms.SelectMany(pf => pf.biorepTechrepIntensityList), logSelectIntensityHistogram).Where(i => i > 1); //these are log2 values
            selectAverageIntensity = allRoundedIntensities.Average();
            selectStDev = (decimal)Math.Sqrt(allRoundedIntensities.Average(v => Math.Pow((double)(v - selectAverageIntensity), 2))); //population stdev calculation, rather than sample
            selectGaussianArea = get_gaussian_area(logSelectIntensityHistogram);
            selectGaussianHeight = selectGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)selectStDev, 2));
        }

        public void defineSelectObservedWithImputedIntensityDistribution(IEnumerable<ExperimentalProteoform> satisfactory_proteoforms, SortedDictionary<decimal, int> logSelectIntensityHistogram)
        {
            IEnumerable<decimal> allRoundedIntensities = analysis as TusherAnalysis1 != null ?
                define_intensity_distribution(satisfactory_proteoforms.SelectMany(pf => pf.quant.TusherValues1.allIntensities.Values).ToList(), logSelectIntensityWithImputationHistogram) :
                define_intensity_distribution(satisfactory_proteoforms.SelectMany(pf => pf.quant.TusherValues2.allIntensities.Values).ToList(), logSelectIntensityWithImputationHistogram);
            selectWithImputationAverageIntensity = allRoundedIntensities.Average();
            selectWithImputationStDev = (decimal)Math.Sqrt(allRoundedIntensities.Average(v => Math.Pow((double)(v - selectAverageIntensity), 2))); //population stdev calculation, rather than sample
            selectWithImputationGaussianArea = get_gaussian_area(logSelectIntensityHistogram);
            selectWithImputationGaussianHeight = selectGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)selectStDev, 2));
        }

        public void defineBackgroundIntensityDistribution(Dictionary<string, List<string>> quantBioFracCombos, List<ExperimentalProteoform> satisfactoryProteoforms, int condition_count, decimal backgroundShift, decimal backgroundWidth)
        {
            bkgdAverageIntensity = selectAverageIntensity + backgroundShift * selectStDev;
            bkgdStDev = selectStDev * backgroundWidth;

            int numMeasurableIntensities = quantBioFracCombos.Keys.Count * condition_count * satisfactoryProteoforms.Count; // all bioreps, all light conditions + all heavy conditions, all satisfactory proteoforms
            int numMeasuredIntensities = satisfactoryProteoforms.Sum(eP => eP.biorepIntensityList.Count); //biorep intensities are created to be unique to the light/heavy + condition + biorep
            int numMissingIntensities = numMeasurableIntensities - numMeasuredIntensities; //this could be negative if there were tons more quantitative intensities

            decimal bkgdGaussianArea = selectGaussianArea / (decimal)numMeasuredIntensities * (decimal)numMissingIntensities;
            bkgdGaussianHeight = bkgdGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)bkgdStDev, 2));
        }

        public List<decimal> define_intensity_distribution(IEnumerable<IBiorepIntensity> intensities, SortedDictionary<decimal, int> histogram)
        {
            histogram.Clear();

            List<decimal> rounded_intensities = (
                from i in intensities
                select Math.Round((decimal)Math.Log(i.intensity_sum, 2), 1))
                .ToList();

            foreach (decimal roundedIntensity in rounded_intensities)
            {
                if (histogram.ContainsKey(roundedIntensity))
                    histogram[roundedIntensity]++;
                else
                    histogram.Add(roundedIntensity, 1);
            }

            return rounded_intensities;
        }

        public decimal get_gaussian_area(SortedDictionary<decimal, int> histogram)
        {
            decimal gaussian_area = 0;
            bool first = true;
            decimal x1 = 0;
            decimal y1 = 0;
            foreach (KeyValuePair<decimal, int> entry in histogram)
            {
                if (first)
                {
                    x1 = entry.Key;
                    y1 = (decimal)entry.Value;
                    first = false;
                }
                else
                {
                    gaussian_area += (entry.Key - x1) * (y1 + ((decimal)entry.Value - y1) / 2);
                    x1 = entry.Key;
                    y1 = (decimal)entry.Value;
                }
            }
            return gaussian_area;
        }

        #endregion Public Methods

    }
}
