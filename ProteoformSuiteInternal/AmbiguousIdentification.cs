using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{

    public class AmbiguousIdentification
    {
        public int begin;
        public int end;
        public PtmSet set = new PtmSet(new List<Ptm>());
        public ProteoformRelation relation;
        public TheoreticalProteoform theoretical_base;

        public AmbiguousIdentification(int begin, int end, PtmSet set, ProteoformRelation relation, TheoreticalProteoform theoretical_base)
        {
            this.begin = begin;
            this.end = end;
            this.relation = relation;
            this.theoretical_base = theoretical_base;
            this.set = set;
        }

    }
}
