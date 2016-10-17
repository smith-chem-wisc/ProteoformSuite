using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class ProteoformFamily
    {
        public int family_id { get; set; }
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
            get { return new HashSet<ProteoformRelation>(proteoforms.SelectMany(p => p.relationships.Where(r => r.peak.peak_accepted)), new RelationComparer()); }
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

        public ProteoformFamily(List<Proteoform> proteoforms, int family_id)
        {
            this.proteoforms = proteoforms;
            this.family_id = family_id;
        }
    }

    public class RelationComparer : IEqualityComparer<ProteoformRelation>
    {
        public bool Equals(ProteoformRelation r1, ProteoformRelation r2)
        {
            return
                r1.connected_proteoforms[0] == r2.connected_proteoforms[1] && r1.connected_proteoforms[1] == r2.connected_proteoforms[0] ||
                r1.connected_proteoforms[0] == r2.connected_proteoforms[0] && r1.connected_proteoforms[1] == r2.connected_proteoforms[1];
        }
        public int GetHashCode(ProteoformRelation r)
        {
            return r.nearby_relations_count;
        }
    }
}
