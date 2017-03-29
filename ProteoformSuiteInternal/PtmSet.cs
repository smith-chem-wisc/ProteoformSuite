using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class PtmSet
    {
        public double mass { get; private set; }
        public List<Ptm> ptm_combination { get; private set; }

        public PtmSet(List<Ptm> unique_ptm_combination)
        {
            ptm_combination = unique_ptm_combination;
            mass = ptm_combination.Sum(ptm => ptm.modification.monoisotopicMass);
        }
    }
 }
