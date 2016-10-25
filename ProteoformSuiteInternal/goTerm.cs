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
            List<int> countOfGoTermInSubset = new List<int>();

            foreach (KeyValuePair<GoTerm, int> pair in goMasterSet)
            {
                for (int i = 0; i < pair.Value; i++)
                {
                    completeDatabaseGoTerms.Add(pair.Key);
                }
            }

            List<List<int>> indices = new List<List<int>>();

            for (int i = 0; i < maxPermutations; i++)
            {
                indices.Add(GenerateRandom(allSampleGoTerms, 0, completeDatabaseGoTerms.Count()));
            }

            object sync = new object();

            GoTerm[] cDGT = completeDatabaseGoTerms.ToArray();

            Parallel.ForEach(indices, set => 
            {
                List<GoTerm> subset = new List<GoTerm>();
                foreach (int index in set)
                {
                    lock (sync)
                    {
                        subset.Add(cDGT[index]);
                    };              
                }

                int termCount = 0;
                termCount = (from t in subset
                             where t.id == _goTerm.id
                             select t).ToList().Count();
                lock (sync)
                {
                    countOfGoTermInSubset.Add(termCount);
                };
            });
            int someCount = completeDatabaseGoTerms.Count();

            return (double)(countOfGoTermInSubset.Count(i => i >= k)) /(countOfGoTermInSubset.Count());
        }

        static Random random = new Random();

        public static List<int> GenerateRandom(int count, int min, int max)
        {
            if (max <= min || count < 0 ||
                    // max - min > 0 required to avoid overflow
                    (count > max - min && max - min > 0))
            {
                // need to use 64-bit to support big ranges (negative min, positive max)
                throw new ArgumentOutOfRangeException("Range " + min + " to " + max +
                        " (" + ((Int64)max - (Int64)min) + " values), or count " + count + " is illegal");
            }

            // generate count random values.
            HashSet<int> candidates = new HashSet<int>();

            // start count values before max, and end at max
            for (int top = max - count; top < max; top++)
            {
                // May strike a duplicate.
                // Need to add +1 to make inclusive generator
                // +1 is safe even for MaxVal max value because top < max
                if (!candidates.Add(random.Next(min, top + 1)))
                {
                    // collision, add inclusive max.
                    // which could not possibly have been added before.
                    candidates.Add(top);
                }
            }

            // load them in to a list, to sort
            List<int> result = candidates.ToList();

            // shuffle the results because HashSet has messed
            // with the order, and the algorithm does not produce
            // random-ordered results (e.g. max-1 will never be the first value)
            for (int i = result.Count - 1; i > 0; i--)
            {
                int k = random.Next(i + 1);
                int tmp = result[k];
                result[k] = result[i];
                result[i] = tmp;
            }
            return result;
        }

        public static List<int> GenerateRandom(int count)
        {
            return GenerateRandom(count, 0, Int32.MaxValue);
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
