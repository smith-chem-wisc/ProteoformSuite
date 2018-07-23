using System.Collections.Generic;
using System.Linq;
using System;

namespace ProteoformSuiteInternal
{
    public class PtmSet
    {

        #region Public Properties

        public double mass { get; private set; }

        public List<Ptm> ptm_combination { get; private set; }

        public int ptm_rank_sum { get; set; }

        #endregion Public Properties

        #region Public Constructors

        public PtmSet(List<Ptm> unique_ptm_combination)
        {
            ptm_combination = unique_ptm_combination;
            mass = ptm_combination.Select(ptm => ptm.modification).Sum(m => m.monoisotopicMass);
        }

        public PtmSet(List<Ptm> unique_ptm_combination, Dictionary<double, int> mod_ranks, int additional_ptm_penalization_factor)
            : this(unique_ptm_combination)
        {
            compute_ptm_rank_sum(mod_ranks, additional_ptm_penalization_factor);
        }

        #endregion Public Constructors

        #region Public Method

        public void compute_ptm_rank_sum(Dictionary<double, int> mod_ranks, int additional_ptm_penalization_factor)
        {
            ptm_rank_sum = ptm_combination.Sum(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification u) ? u.ptm_rank : mod_ranks.TryGetValue(ptm.modification.monoisotopicMass, out int rank) ? rank : Sweet.lollipop.mod_rank_sum_threshold)
                + additional_ptm_penalization_factor * (ptm_combination.Count - 1) // penalize additional PTMs
                - ptm_combination.Count(ptm => Sweet.lollipop.theoretical_database.variableModifications.Contains(ptm.modification)); // favor variable modifications over regular
        }

        public bool same_ptmset(PtmSet that, bool unlocalized)
        {
            if (unlocalized) //methyl,methyl,methyl = methyl; methyl; methyl, etc
            {
                string this_ptms = String.Join(", ", ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id).OrderBy(m => m));
                string that_ptms = String.Join(", ", that.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id).OrderBy(m => m));
                return this_ptms == that_ptms;
            }
            else
            {
                List<string> this_ptms = this.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id).ToList();
                List<string> that_ptms = that.ptm_combination.Select(ptm => Sweet.lollipop.theoretical_database.unlocalized_lookup.TryGetValue(ptm.modification, out UnlocalizedModification x) ? x.id : ptm.modification.id).ToList();
                if (this_ptms.Count != that_ptms.Count) return false;
                foreach (string m in this_ptms.Distinct())
                {
                    if (that_ptms.Count(s => s == m) != this_ptms.Count(s => s == m))
                    {
                        return false;
                    }
                }
                foreach (string m in that_ptms.Distinct())
                {
                    if (that_ptms.Count(s => s == m) != this_ptms.Count(s => s == m))
                    {
                        return false;
                    }
                }
                return true;
            }
        }


        #endregion Public Method

    }
}
