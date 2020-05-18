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
        public PtmSet ptm_set = new PtmSet(new List<Ptm>());
        public ProteoformRelation relation;
        public TheoreticalProteoform theoretical_base;
        public List<Proteoform> linked_proteoform_references;
        public List<SpectrumMatch> bottom_up_PSMs = new List<SpectrumMatch>();
        public AmbiguousIdentification(int begin, int end, PtmSet ptm_set, ProteoformRelation relation, TheoreticalProteoform theoretical_base, List<Proteoform> linked_proteoform_references)
        {
            this.begin = begin;
            this.end = end;
            this.relation = relation;
            this.theoretical_base = theoretical_base;
            this.ptm_set = ptm_set;
            this.linked_proteoform_references = linked_proteoform_references;
        }

    }
}
