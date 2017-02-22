using Accord.Math;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Proteomics;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class GoTermNumber
    {
        public GoTerm goTerm { get; set; }
        public string id { get; set; }
        public string description { get; set; }
        public Aspect aspect { get; set; }


        public int q { get; set; }//count of proteins in selected subset with the particular Go term
        public int k { get; set; }//count of proteins in selected subset
        public int m { get; set; }//count of proteins in background with the particular Go term
        public int t { get; set; }//count of proteins in background
        public decimal log_odds_ratio { get; set; } = 0; // fold change
        public double p_value { get; set; } = 1;//p-value calculated using hypergeometric test.
        public decimal by { get; set; } = 1;//benjamini yekutieli calculated after all p-values are calculated

        public GoTermNumber() //for testing
        { }

        public GoTermNumber(GoTerm g)
        {
            this.goTerm = g;
            this.id = g.id;
            this.description = g.description;
            this.aspect = g.aspect;
            this.q = Lollipop.inducedOrRepressedProteins.SelectMany(p=>p.GoTerms.Where(t=>t.id==g.id)).ToList().Count();
            //this.q = Lollipop.inducedOrRepressedProteins.Count(p => p.goTerms.Contains(g));
            this.k = Lollipop.inducedOrRepressedProteins.Count();
            this.m = Lollipop.GO_ProteinBackgroundSet.SelectMany(p => p.GoTerms.Where(t => t.id == g.id)).ToList().Count();
            //this.m = Lollipop.goMasterSet[g];
            this.t = Lollipop.GO_ProteinBackgroundSet.Count();
            if(q != 0 && k != 0 && m != 0 && t != 0)
                this.log_odds_ratio = (decimal)(Math.Log((double)q/(double)k, 2)- Math.Log((double)m / (double)t, 2));
            this.p_value = GoTerm_pValue(q, k, m, t);
        }

        public GoTermNumber(GoTerm g, int q, int k, int m, int t)
        {
            this.goTerm = g;
            this.id = g.id;
            this.description = g.description;
            this.aspect = g.aspect;
            this.q = q;
            //this.q = Lollipop.inducedOrRepressedProteins.Count(p => p.goTerms.Contains(g));
            this.k = k;
            this.m = m;
            this.t = t;
            if (q != 0 && k != 0 && m != 0 && t != 0)
                this.log_odds_ratio = (decimal)(Math.Log((double)q / (double)k, 2) - Math.Log((double)m / (double)t, 2));
            this.p_value = GoTerm_pValue(q, k, m, t);
        }


        public double GoTerm_pValue(int q, int k, int m, int t) //this is the hypergeometric probability as used by GOEAST algorithm
        {
            double p = 0;
            for (int i = q; i <= m; i++)
            {
                BigInteger top = binomialCoefficient(m, i) * binomialCoefficient(t - m, k - i);
                BigInteger bottom = binomialCoefficient(t, k);
                p += (double)top /(double)bottom;
            }
            return Math.Min(p,1d);
        }

        public BigInteger binomialCoefficient(int n, int k)
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

        public void benjaminiYekutieli()//multiple testing correction similar to benjamini hochberg but for interdependant data
        {
            // FDR = pValue * summation * totalNumberOfTests / (rank of the pValue is a sorted ascending list)
            // summation = sum from i=1 to i=totalNumberOfTests of 1/i 
            // for this work, the pValue is the hypergeometric probability

            double sum = 0;
            int nbp = Lollipop.goTermNumbers.Count;//These are only for "interesting proteins", which is the set of proteins induced or repressed beyond a specified fold change, intensity and below FDR. There is a test for each different go term. The number of tests equals the number of different go terms
            List<double> pvals = Lollipop.goTermNumbers.Select(g => g.p_value).ToList();
            pvals.Sort();
            double rank = (double)pvals.IndexOf(this.p_value) + 1d; // add one because index starts at zero. this gives us the rank of the pvalue in the sorted list.
            for (int i = 1; i <= nbp; i++)
            {
                sum += 1d / (double)i;
            }

            this.by = (decimal)Math.Min(this.p_value * (double)nbp * sum / rank, 1d); // range of FDRs if from 0 to 1
        }
    }
}
