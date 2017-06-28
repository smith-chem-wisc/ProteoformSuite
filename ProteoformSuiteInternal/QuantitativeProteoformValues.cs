using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class QuantitativeProteoformValues
    {

        #region Public Properties

        public ExperimentalProteoform proteoform { get; set; }
        public string accession { get { return proteoform.accession; } }
        public List<BiorepIntensity> numeratorBiorepIntensities { get; set; }
        public List<BiorepIntensity> denominatorBiorepIntensities { get; set; }
        public List<BiorepIntensity> numeratorImputedIntensities { get; set; }
        public List<BiorepIntensity> denominatorImputedIntensities { get; set; }
        public decimal numeratorIntensitySum { get; set; } = 0;
        public decimal denominatorIntensitySum { get; set; } = 0;
        public decimal intensitySum { get; set; } = 0;
        public decimal logFoldChange { get; set; } = 0;
        public decimal variance { get; set; } = 0;
        public decimal scatter { get; set; } = 0;
        public decimal pValue { get; set; } = 0;
        public bool significant { get; set; } = false;
        public decimal testStatistic { get; set; }
        public List<decimal> permutedTestStatistics { get; set; }
        public decimal averagePermutedTestStatistic { get; set; }
        public decimal correspondingAveragePermutedTestStatistic { get; set; }
        public decimal FDR { get; set; } = 0;

        #endregion Public Properties

        #region Public Constructor

        //Selecting numerator and denominator is not implemented
        public QuantitativeProteoformValues(ExperimentalProteoform eP)
        {
            eP.quant = this;
            proteoform = eP;
        }

        #endregion Public Constructor

        #region Public Methods

        public void determine_biorep_intensities_and_test_statistics(bool neucode_labeled, List<BiorepIntensity> biorepIntensityList, decimal bkgdAverageIntensity, decimal bkgdStDev, string numerator, string denominator, decimal sKnot)
        {
            //bkgdAverageIntensity is log base 2
            //bkgdStDev is log base 2

            significant = false;
            numeratorBiorepIntensities = biorepIntensityList.Where(b => b.condition == SaveState.lollipop.numerator_condition).ToList();
            numeratorImputedIntensities = imputedIntensities(numeratorBiorepIntensities, bkgdAverageIntensity, bkgdStDev, SaveState.lollipop.numerator_condition, SaveState.lollipop.conditionsBioReps[SaveState.lollipop.numerator_condition]);
            numeratorIntensitySum = (decimal)numeratorBiorepIntensities.Sum(i => i.intensity) + (decimal)numeratorImputedIntensities.Sum(i => i.intensity);
            List<BiorepIntensity> allNumeratorIntensities = numeratorBiorepIntensities.Concat(numeratorImputedIntensities).ToList();

            List<BiorepIntensity> allDenominatorIntensities = new List<BiorepIntensity>();
            denominatorBiorepIntensities = biorepIntensityList.Where(b => b.condition == SaveState.lollipop.denominator_condition).ToList();
            denominatorImputedIntensities = imputedIntensities(denominatorBiorepIntensities, bkgdAverageIntensity, bkgdStDev, SaveState.lollipop.denominator_condition, SaveState.lollipop.conditionsBioReps[SaveState.lollipop.denominator_condition]);
            denominatorIntensitySum = (decimal)denominatorBiorepIntensities.Sum(i => i.intensity) + (decimal)denominatorImputedIntensities.Sum(i => i.intensity);
            allDenominatorIntensities = denominatorBiorepIntensities.Concat(denominatorImputedIntensities).ToList();

            intensitySum = numeratorIntensitySum + denominatorIntensitySum;
            logFoldChange = (decimal)Math.Log((double)numeratorIntensitySum / (double)denominatorIntensitySum, 2);
            variance = Variance(logFoldChange, allNumeratorIntensities, allDenominatorIntensities);
            pValue = PValue(logFoldChange, allNumeratorIntensities, allDenominatorIntensities);
            scatter = getProteinLevelStdDev(allNumeratorIntensities, allDenominatorIntensities); //this is log2 bases
            testStatistic = getSingleTestStatistic(allNumeratorIntensities, allDenominatorIntensities, scatter, sKnot);
            permutedTestStatistics = getPermutedTestStatistics(allNumeratorIntensities, allDenominatorIntensities, scatter, sKnot);
            averagePermutedTestStatistic = permutedTestStatistics.Average();
        }

        public static decimal computeExperimentalProteoformFDR(decimal testStatistic, List<decimal> permutedTestStatistics, int satisfactoryProteoformsCount, List<decimal> sortedProteoformTestStatistics)
        {
            decimal minimumPositivePassingTestStatistic = Math.Abs(testStatistic);
            decimal minimumNegativePassingTestStatistic = -minimumPositivePassingTestStatistic;

            List<decimal> orderedps = permutedTestStatistics.OrderBy(x => x).ToList();
            List<decimal> orderedps2 = permutedTestStatistics.OrderByDescending(x => x).ToList();

            int totalFalsePermutedPassingValues = permutedTestStatistics.Count(v => v <= minimumNegativePassingTestStatistic || minimumPositivePassingTestStatistic <= v);
            decimal averagePermutedPassing = (decimal)totalFalsePermutedPassingValues / (decimal)permutedTestStatistics.Count * (decimal)satisfactoryProteoformsCount;

            int totalRealPassing = sortedProteoformTestStatistics.Count(stat => stat <= minimumNegativePassingTestStatistic || minimumPositivePassingTestStatistic <= stat);

            decimal fdr = averagePermutedPassing / (decimal)totalRealPassing;
            return fdr;
        }

        public static List<BiorepIntensity> imputedIntensities(List<BiorepIntensity> observedBioreps, decimal bkgdAverageIntensity, decimal bkgdStDev, string condition, List<int> bioreps)
        {
            //bkgdAverageIntensity is log base 2
            //bkgdStDev is log base 2

            List<BiorepIntensity> imputedBioreps = new List<BiorepIntensity>();
            foreach (int biorep in bioreps)
            {
                // no bioreps observed from this conditon at all OR this condtion was observed but this biorep was not
                if (!observedBioreps.Select(k => k.condition).Contains(condition) || 
                    !observedBioreps.Where(l => l.condition == condition).Select(b => b.biorep).Contains(biorep))
                        imputedBioreps.Add(add_biorep_intensity(bkgdAverageIntensity, bkgdStDev, biorep, condition));
            }
            return imputedBioreps;
        }

        public static BiorepIntensity add_biorep_intensity(decimal bkgdAverageIntensity, decimal bkgdStDev, int biorep, string key)
        {
            //bkgdAverageIntensity is coming in as a log 2 number
            //bkgdStDev is coming in as a log 2 number

            double u1 = ExtensionMethods.RandomNumber(); //these are uniform(0,1) random doubles
            double u2 = ExtensionMethods.RandomNumber();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double intensity = Math.Pow(2, (double)bkgdAverageIntensity) + (Math.Pow(2, (double)bkgdStDev) * randStdNormal);
            return new BiorepIntensity(true, biorep, key, intensity);//random normal(mean,stdDev^2)
        }

        public decimal Variance(decimal logFoldChange, List<BiorepIntensity> allNumerators, List<BiorepIntensity> allDenominators)
        {
            decimal squaredVariance = 0;
            foreach (int biorep in allNumerators.Select(b => b.biorep).ToList())
            {
                List<BiorepIntensity> numerators_in_biorep = allNumerators.Where(b => b.biorep == biorep).ToList();
                List<BiorepIntensity> denominators_in_biorep = allDenominators.Where(b => b.biorep == biorep).ToList();

                if (numerators_in_biorep.Count != denominators_in_biorep.Count)
                    throw new ArgumentException("Error: Imputation has gone awry. Each biorep should have the same number of biorep intensities for the numerator and denominator at this point.");

                decimal logRepRatio = (decimal)Math.Log(
                        (double)(((decimal)numerators_in_biorep.Sum(i => i.intensity)) /
                        ((decimal)denominators_in_biorep.Sum(i => i.intensity)))
                        , 2);

                squaredVariance += (decimal)Math.Pow(((double)logRepRatio - (double)logFoldChange), 2);
            }
            return (decimal)Math.Pow((double)squaredVariance, 0.5);
        }

        public decimal getProteinLevelStdDev(List<BiorepIntensity> allNumerators, List<BiorepIntensity> allDenominators)
        {
            if ((allNumerators.Count + allDenominators.Count) == 2)
                return 1000000m;

            decimal a = (decimal)((1d / (double)allNumerators.Count + 1d / (double)allDenominators.Count) / ((double)allNumerators.Count + (double)allDenominators.Count - 2d));
            double log2NumeratorAvg = Math.Log(allNumerators.Average(l => l.intensity), 2);
            double log2DenominatorAvg = Math.Log(allDenominators.Average(l => l.intensity), 2);
            decimal numeratorSumSquares = allNumerators.Sum(l => (decimal)Math.Pow(Math.Log(l.intensity, 2) - log2NumeratorAvg, 2d));
            decimal denominatorSumSquares = allDenominators.Sum(h => (decimal)Math.Pow(Math.Log(h.intensity, 2) - log2DenominatorAvg, 2d));
            decimal stdev = (decimal)Math.Sqrt((double)((numeratorSumSquares + denominatorSumSquares) * a));
            return stdev;
        }

        public decimal getSingleTestStatistic(List<BiorepIntensity> allNumerators, List<BiorepIntensity> allDenominators, decimal proteinLevelStdDev, decimal sKnot)
        {
            double t = (Math.Log(allNumerators.Average(l => l.intensity), 2) - Math.Log(allDenominators.Average(h => h.intensity), 2)) / ((double)(proteinLevelStdDev + sKnot));
            return (decimal)t;
        }

        public List<decimal> getPermutedTestStatistics(List<BiorepIntensity> allNumerators, List<BiorepIntensity> allDenominators, decimal protproteinLevelStdDevein, decimal sKnot)
        {
            List<decimal> pst = new List<decimal>();
            List<int> arr = Enumerable.Range(0, allNumerators.Count + allDenominators.Count).ToList();
            var result = ExtensionMethods.Combinations(arr, allNumerators.Count);

            List<BiorepIntensity> allBiorepIntensities = new List<BiorepIntensity>(allNumerators.Concat(allDenominators));

            int last = allNumerators.Count;
            if (allNumerators.Count != allDenominators.Count) // This shouldn't happen because imputation forces these lists to be the same length
            {
                last += allDenominators.Count;
                throw new ArgumentException("Error: Imputation has gone awry. Each biorep should have the same number of biorep intensities for the numerator and denominator at this point.");
            }

            for (int i = 1; i < last + 1; i++)
            {
                List<BiorepIntensity> numeratorlist = new List<BiorepIntensity>();
                List<BiorepIntensity> denominatorlist = new List<BiorepIntensity>();
                foreach (int index in result.ElementAt(i))
                {
                    numeratorlist.Add(allBiorepIntensities[index]);
                }
                denominatorlist = allBiorepIntensities.Except(numeratorlist).ToList();
                pst.Add(getSingleTestStatistic(numeratorlist, denominatorlist, protproteinLevelStdDevein, sKnot)); //adding the test statistic for each combo
            }
            return pst;
        }

        public decimal PValue(decimal logFoldChange, List<BiorepIntensity> allNumerators, List<BiorepIntensity> allDenominators)
        {
            if (allNumerators.Count != allDenominators.Count)
                throw new ArgumentException("Error: Imputation has gone awry. Each biorep should have the same number of biorep intensities for NeuCode light and heavy at this point.");

            int maxPermutations = 10000;
            ConcurrentBag<decimal> permutedRatios = new ConcurrentBag<decimal>();

            Parallel.For(0, maxPermutations, i =>
            {
                List<double> combined = allNumerators.Select(j => j.intensity).Concat(allDenominators.Select(j => j.intensity)).ToList();
                combined.Shuffle();
                double numerator = combined.Take(allNumerators.Count).Sum();
                double denominator = combined.Skip(allNumerators.Count).Take(allDenominators.Count).Sum();
                decimal someRatio = (decimal)Math.Log(numerator / denominator, 2);
                permutedRatios.Add(someRatio);
            });

            decimal pValue = logFoldChange > 0 ?
                (decimal)(1M / maxPermutations) + (decimal)permutedRatios.Count(x => x > logFoldChange) / (decimal)permutedRatios.Count : //adding a slight positive shift so that later logarithms don't produce fault
                (decimal)(1M / maxPermutations) + (decimal)permutedRatios.Count(x => x < logFoldChange) / (decimal)permutedRatios.Count; //adding a slight positive shift so that later logarithms don't produce fault

            return pValue;
        }

        #endregion Public Methods

    }
}
