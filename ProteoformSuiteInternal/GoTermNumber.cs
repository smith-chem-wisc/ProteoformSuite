using Accord.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace ProteoformSuiteInternal
{
    public class GoTermNumber : GoTerm
    {
        public GoTerm goTerm { get; set; }

        public int q_significantProteinsWithThisGoTerm { get; private set; }
        public int k_significantProteins { get; private set; }
        public int m_backgroundProteinsWithThisGoTerm { get; private set; }
        public int t_backgroundProteins { get; private set; }
        public decimal log_odds_ratio { get; set; } = 0; // fold change
        public double p_value { get; set; } = 1; //p-value calculated using hypergeometric test.
        public double by { get; set; } = 1; //benjamini yekutieli calculated after all p-values are calculated

        /// <summary>
        /// q is the count of proteins in selected subset with the particular Go term; 
        /// k is the count of proteins in selected subset;
        /// m is the count of proteins in background with the particular Go term;
        /// t is the count of proteins in background
        /// </summary>
        /// <param name="g"></param>
        /// <param name="q_significantProteinsWithThisGoTerm"></param>
        /// <param name="k_significantProteins"></param>
        /// <param name="m_backgroundProteinsWithThisGoTerm"></param>
        /// <param name="t_backgroundProteins"></param>
        public GoTermNumber(GoTerm g, int q_significantProteinsWithThisGoTerm, int k_significantProteins, int m_backgroundProteinsWithThisGoTerm, int t_backgroundProteins) 
            : base(g.Id, g.Description, g.Aspect)
        {
            this.q_significantProteinsWithThisGoTerm = q_significantProteinsWithThisGoTerm;
            this.k_significantProteins = k_significantProteins;
            this.m_backgroundProteinsWithThisGoTerm = m_backgroundProteinsWithThisGoTerm;
            this.t_backgroundProteins = t_backgroundProteins;

            if (q_significantProteinsWithThisGoTerm > 0 && k_significantProteins > 0 && m_backgroundProteinsWithThisGoTerm > 0 && t_backgroundProteins > 0)
            {
                this.log_odds_ratio = (decimal)(Math.Log((double)q_significantProteinsWithThisGoTerm / (double)k_significantProteins, 2) - Math.Log((double)m_backgroundProteinsWithThisGoTerm / (double)t_backgroundProteins, 2));
                this.p_value = GoTerm_pValue(q_significantProteinsWithThisGoTerm, k_significantProteins, m_backgroundProteinsWithThisGoTerm, t_backgroundProteins);
            }
        }

        /// <summary>
        /// Hypergeometric probability as used by GOEAST algorithm
        /// </summary>
        /// <param name="q"></param>
        /// <param name="k"></param>
        /// <param name="m"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double GoTerm_pValue(int q, int k, int m, int t)
        {
            double p = 0;
            for (int i = q; i <= m; i++)
            {
                BigInteger top = binomialCoefficient(m, i) * binomialCoefficient(t - m, k - i);
                BigInteger bottom = binomialCoefficient(t, k);
                p += (double)BigRational.Divide(top, bottom);
            }
            return Math.Min(1, p);
        }

        public static BigInteger binomialCoefficient(int n, int k)
        {

            // This function gets the total number of unique combinations based upon N and K.
            // N is the total number of items.
            // K is the size of the group.
            // Total number of unique combinations = N! / ( K! (N - K)! ).
            // This function is less efficient, but is more likely to not overflow when N and K are large.
            // Taken from:  http://blog.plover.com/math/choose.html
            //
            BigInteger r = 1;
            long d;
            if (k > n) return 0;
            for (d = 1; d <= k; d++)
            {
                r *= n--;
                r /= d;
            }
            return r;
        }

        /// <summary>
        /// Multiple testing correction similar to benjamini hochberg but for interdependant data.
        /// Assumes p-values are sorted.
        /// </summary>
        /// <param name="nbp"></param>
        /// <param name="pvals"></param>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public static double benjaminiYekutieli(int nbp, List<double> pvals, double pValue)
        {
            // FDR = pValue * summation * totalNumberOfTests / (rank of the pValue is a sorted ascending list)
            // summation = sum from i=1 to i=totalNumberOfTests of 1/i 
            // for this work, the pValue is the hypergeometric probability

            //int nbp = Lollipop.goTermNumbers.Count;//These are only for "interesting proteins", which is the set of proteins induced or repressed beyond a specified fold change, intensity and below FDR. There is a test for each different go term. The number of tests equals the number of different go terms
            //List<double> pvals = Lollipop.goTermNumbers.Select(g => g.p_value).ToList();

            double rank = pvals.IndexOf(pValue) + 1d; // add one because index starts at zero. this gives us the rank of the pvalue in the sorted list.
            double sum = Enumerable.Range(1, nbp).Sum(x => 1d / x);
            double by = pValue * nbp * sum / rank;
            return Math.Min(1, by); // range of FDRs if from 0 to 1
        }
    }
}
