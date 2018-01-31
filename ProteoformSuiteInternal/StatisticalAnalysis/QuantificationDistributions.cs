using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class QuantitativeDistributions
    {

        #region Public Fields

        public TusherAnalysis analysis;
        public Log2FoldChangeAnalysis log2foldAnalysis;
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

        public QuantitativeDistributions(Log2FoldChangeAnalysis analysis)
        {
            this.log2foldAnalysis = analysis;
        }

        #endregion Public Constructor

        #region Public Methods

        public void defineAllObservedIntensityDistribution(IEnumerable<IBiorepIntensity> biorep_intensities, SortedDictionary<decimal, int> logIntensityHistogram) // the distribution of all observed experimental proteoform biorep intensities
        {
            IEnumerable<decimal> rounded_log_intensities = define_rounded_intensity_distribution(biorep_intensities, logIntensityHistogram);

            IEnumerable<decimal> log_intensities = biorep_intensities.Where(i => i.intensity_sum > 1).Select(x => (decimal)Math.Log(x.intensity_sum, 2));
            allObservedAverageIntensity = log_intensities.Average(); //these are log2 values
            allObservedStDev = (decimal)Math.Sqrt(log_intensities.Average(v => Math.Pow((double)((decimal)v - allObservedAverageIntensity), 2))); //population stdev calculation, rather than sample
            allObservedGaussianArea = get_gaussian_area(logIntensityHistogram);
            allObservedGaussianHeight = allObservedGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)allObservedStDev, 2));
        }

        public void defineSelectObservedIntensityDistribution(IEnumerable<IBiorepIntensity> biorep_intensities, SortedDictionary<decimal, int> logSelectIntensityHistogram)
        {
            define_rounded_intensity_distribution(biorep_intensities, logSelectIntensityHistogram);

            IEnumerable<decimal> log_intensities = biorep_intensities.Where(i => i.intensity_sum > 1).Select(x => (decimal)Math.Log(x.intensity_sum, 2));
            selectAverageIntensity = log_intensities.Average();
            selectStDev = (decimal)Math.Sqrt(log_intensities.Average(v => Math.Pow((double)(v - selectAverageIntensity), 2))); //population stdev calculation, rather than sample
            selectGaussianArea = get_gaussian_area(logSelectIntensityHistogram);
            selectGaussianHeight = selectGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)selectStDev, 2));
        }

        public void defineSelectObservedWithImputedIntensityDistribution(IEnumerable<IBiorepIntensity> biorep_intensities, SortedDictionary<decimal, int> logSelectIntensityHistogram)
        {
            IEnumerable<decimal> rounded_log_intensities = define_rounded_intensity_distribution(biorep_intensities, logSelectIntensityWithImputationHistogram);
            selectWithImputationAverageIntensity = rounded_log_intensities.Average();
            selectWithImputationStDev = (decimal)Math.Sqrt(rounded_log_intensities.Average(v => Math.Pow((double)(v - selectAverageIntensity), 2))); //population stdev calculation, rather than sample
            selectWithImputationGaussianArea = get_gaussian_area(logSelectIntensityHistogram);
            selectWithImputationGaussianHeight = selectGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)selectStDev, 2));
        }

        public void defineBackgroundIntensityDistribution(Dictionary<string, List<string>> quantBioFracCombos, List<ExperimentalProteoform> satisfactoryProteoforms, int condition_count, decimal backgroundShift, decimal backgroundWidth)
        {
            bkgdAverageIntensity = selectAverageIntensity + backgroundShift * selectStDev;
            bkgdStDev = selectStDev * backgroundWidth;

            int numMeasurableIntensities = quantBioFracCombos.Keys.Count * condition_count * satisfactoryProteoforms.Count; // all bioreps, all light conditions + all heavy conditions, all satisfactory proteoforms
            int numMeasuredIntensities = satisfactoryProteoforms.Sum(eP => eP.biorepIntensityList.Count); //biorep intensities are created to be unique to the light/heavy + condition + biorep
            int numMissingIntensities = numMeasurableIntensities - numMeasuredIntensities; 

            decimal bkgdGaussianArea = selectGaussianArea / (decimal)numMeasuredIntensities * (decimal)numMissingIntensities;
            bkgdGaussianHeight = bkgdGaussianArea / (decimal)Math.Sqrt(2 * Math.PI * Math.Pow((double)bkgdStDev, 2));
        }

        public List<decimal> define_rounded_intensity_distribution(IEnumerable<IBiorepIntensity> intensities, SortedDictionary<decimal, int> histogram)
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
