using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class ProteoformFamily
    {
        public string accession_list
        {
            get { return string.Join("; ", theoretical_proteoforms.Select(p => p.accession)); }
        }
        public int lysine_count { get; set; } = -1;
        public List<ExperimentalProteoform> experimental_proteoforms { get; set; }
        public int experimental_count { get; set; }
        public List<TheoreticalProteoform> theoretical_proteoforms { get; set; }
        public int theoretical_count { get; set; }
        public int relation_count { get; set; }
        public HashSet<ProteoformRelation> relations
        {
            get { return new HashSet<ProteoformRelation>(proteoforms.SelectMany(p => p.relationships.Where(r => r.peak.peak_accepted))); }
        }
        public List<Proteoform> _proteoforms;
        public List<Proteoform> proteoforms
        {
            get { return this._proteoforms; }
            set
            {
                _proteoforms = value;

                HashSet<int> lysine_counts = new HashSet<int>(value.Select(p => p.lysine_count));
                if (lysine_counts.Count == 1) this.lysine_count = lysine_counts.FirstOrDefault();
                else this.lysine_count = -1;

                this.experimental_proteoforms = value.Where(p => p is ExperimentalProteoform).Select(p => (ExperimentalProteoform)p).ToList();
                this.experimental_count = experimental_proteoforms.Count();
                this.theoretical_proteoforms = value.Where(p => p is TheoreticalProteoform).Select(p => (TheoreticalProteoform)p).ToList();
                this.theoretical_count = theoretical_proteoforms.Count();
                this.relation_count = new HashSet<MassDifference>(value.SelectMany(p => p.relationships.Where(r => r.peak.peak_accepted))).Count();
            }
        }

        public ProteoformFamily(List<Proteoform> proteoforms)
        {
            this.proteoforms = proteoforms;
        }
    }
}
