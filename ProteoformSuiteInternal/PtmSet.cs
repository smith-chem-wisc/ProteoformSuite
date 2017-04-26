using System;
using System.Collections.Generic;
using System.Linq;

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
            ptm_rank_sum = ptm_combination.Sum(ptm => mod_ranks[ptm.modification.monoisotopicMass])
                + additional_ptm_penalization_factor * (ptm_combination.Count - 1) // penalize additional PTMs
                - ptm_combination.Count(ptm => SaveState.lollipop.theoretical_database.variableModifications.Contains(ptm.modification)); // favor variable modifications over regular
        }

        #endregion Public Method

    }
}
