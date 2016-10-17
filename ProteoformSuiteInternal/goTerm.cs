using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class GoTerm
    {
        public string id { get; set; }
        public string description { get; set; }
        public aspect aspect { get; set; }
    }

    public enum aspect
    {
        molecularFunction,
        cellularComponent,
        biologicalProcess
    }

    public class GoTermNumber
    {
        public GoTerm goTerm { get; set; }
        public string id {get; set;}
        public string description { get; set; }
        public string aspect { get; set; }        
        public int k { get; set; } // number in set of enriched/depleted proteins
        public int f { get; set; } // number in database
        public double pValue { get; set; }
        public double logfold { get; set; } // log base2 fold change
        public string proteinInCategoryFromSample { get; set; }

        public GoTermNumber(GoTerm _goTerm, List<Protein> proteinsInSample, Dictionary<GoTerm, int> goMasterSet)
        {
            goTerm = _goTerm;
            id = _goTerm.id;
            description = _goTerm.description;
            aspect = _goTerm.aspect.ToString();
            
            proteinInCategoryFromSample = String.Join("; ", (from p in proteinsInSample from t in p.goTerms where t.id == _goTerm.id select p).ToList().Select(o=>o.accession).ToList());
            k = sampleGoTermCount(_goTerm, proteinsInSample);            
            f = goMasterSet[goMasterSet.Keys.Where(k => k.id == _goTerm.id).First()];
            logfold = getGoTermLogFold(k, f, proteinsInSample, goMasterSet); 
            pValue = goTerm_pValue(_goTerm, proteinsInSample, goMasterSet, k, f);
        }

        private double getGoTermLogFold(int k, int f, List<Protein> proteinsInSample, Dictionary<GoTerm, int> goMasterSet)
        {
            int allSampleGoTerms = (from p in proteinsInSample
                                    from g in p.goTerms
                                    select g).ToList().Count();
               
            double numerator = (double)k / allSampleGoTerms;

            int allDataBaseGoTerms = 0;
            foreach (KeyValuePair<GoTerm,int> pair in goMasterSet)
            {
                allDataBaseGoTerms = allDataBaseGoTerms + pair.Value;
            }
            double denominator = (double)f / allDataBaseGoTerms;
            if (denominator > 0)
                return Math.Log(numerator / denominator, 2);
            else
                return 0d;
        }

        private double goTerm_pValue(GoTerm _goTerm, List<Protein> proteinsInSample, Dictionary<GoTerm, int> goMasterSet, int k, int f)
        {
            int allSampleGoTerms = 0;

            foreach (Protein p in proteinsInSample)
            {
                allSampleGoTerms = allSampleGoTerms + p.goTerms.Count();
            }

            int maxPermutations = 500;
            List<GoTerm> completeDatabaseGoTerms = new List<GoTerm>();
            ConcurrentBag<int> countOfGoTermInSubset = new ConcurrentBag<int>();

            foreach (KeyValuePair<GoTerm, int> pair in goMasterSet)
            {
                for (int i = 0; i < pair.Value; i++)
                {
                    completeDatabaseGoTerms.Add(pair.Key);
                }
            }

            IList<GoTerm> copyList = new List<GoTerm>();
            copyList = completeDatabaseGoTerms;
            List<GoTerm> subset = new List<GoTerm>();

            for (int i = 0; i < maxPermutations; i++)
            {
                int termCount = 0;              
                copyList.Shuffle();
                subset = copyList.Take(allSampleGoTerms).ToList();
                termCount = (from t in subset
                             where t.id == _goTerm.id
                             select t).ToList().Count();
                countOfGoTermInSubset.Add(termCount);
            }
            return (double)(countOfGoTermInSubset.Count(i => i >= k)) /(countOfGoTermInSubset.Count());
        }

        private int sampleGoTermCount(GoTerm _goTerm, List<Protein> proteinsInSample)
        {
            int termCount = 0;
            termCount = (from p in proteinsInSample
                         from t in p.goTerms
                         where t.id == _goTerm.id
                         select p).ToList().Count();

            return termCount;
        }
    }
}
