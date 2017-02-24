using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace ProteoformSuiteInternal
{
    public class ProteoformFamily
    {
        public int family_id { get; set; }
        public string name_list
        {
            get { return string.Join("; ", theoretical_proteoforms.Select(p => p.name)); }
        }
        public string accession_list
        {
            get { return string.Join("; ", theoretical_proteoforms.Select(p => p.accession)); }
        }
        public string experimentals_list
        {
            get { return string.Join("; ", experimental_proteoforms.Select(p => p.accession)); }
        }
        public string agg_mass_list
        {
            get { return string.Join("; ", experimental_proteoforms.Select(p => Math.Round(p.agg_mass, Lollipop.deltaM_edge_display_rounding))); }
        }
        public int lysine_count { get; set; } = -1;
        public List<ExperimentalProteoform> experimental_proteoforms { get; set; }
        public int experimental_count
        {
            get { return this.experimental_proteoforms.Count; }
        }
        public List<TheoreticalProteoform> theoretical_proteoforms { get; set; }
        public int theoretical_count
        {
            get { return this.theoretical_proteoforms.Count; }
        }
        public int relation_count
        {
            get { return this.relations.Count; }
        }
        public HashSet<ProteoformRelation> relations { get; set; }
        public HashSet<Proteoform> _proteoforms;
        public HashSet<Proteoform> proteoforms
        {
            get { return this._proteoforms; }
            set
            {
                _proteoforms = value;
                HashSet<int> lysine_counts = new HashSet<int>(value.Select(p => p.lysine_count));
                if (lysine_counts.Count == 1) this.lysine_count = lysine_counts.FirstOrDefault();
                this.experimental_proteoforms = value.Where(p => p is ExperimentalProteoform).Select(p => (ExperimentalProteoform)p).ToList();
                this.theoretical_proteoforms = value.Where(p => p is TheoreticalProteoform).Select(p => (TheoreticalProteoform)p).ToList();
                this.relations = new HashSet<ProteoformRelation>(value.SelectMany(p => p.relationships.Where(r => r.peak.peak_accepted)), new RelationComparer());
            }
        }

        public ProteoformFamily(IEnumerable<Proteoform> proteoforms, int family_id)
        {
            this.proteoforms = new HashSet<Proteoform>(proteoforms, new ProteoformComparer());
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

    public class ProteoformComparer: IEqualityComparer<Proteoform>
    {
        public bool Equals(Proteoform p1, Proteoform p2)
        {
            return p1.accession == p2.accession;
        }
        public int GetHashCode(Proteoform p)
        {
            return p.accession.ToCharArray().Sum(c => Convert.ToInt32(c));
        }
    }
}
