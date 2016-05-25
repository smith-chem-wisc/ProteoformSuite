using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS_0._00
{
    public class ProteoformCommunity
    {
        public List<ExperimentalProteoform> experimental_proteoforms { get; set; } = new List<ExperimentalProteoform>();
        public List<TheoreticalProteoform> theoretical_proteoforms { get; set; } = new List<TheoreticalProteoform>();
        public Dictionary<string, List<TheoreticalProteoform>> decoy_proteoforms = new Dictionary<string, List<TheoreticalProteoform>>();
        public List<DeltaMassPeak> relations_within_peak = new List<DeltaMassPeak>();

        //INITIALIZATION of PROTEOFORMS
        //public void add(string accession, NeuCodePair candidate)
        //{
        //    foreach(ExperimentalProteoform pf in experimental_proteoforms) //Need to keep order of proteoforms coming in to keep ordering of max intensity; parallelized beforehand
        //    {
        //        if (pf.includes(candidate)) pf.add(candidate); return;
        //    }
        //    ExperimentalProteoform new_pf = new ExperimentalProteoform(accession, candidate, true);
        //    //Lollipop.experimental_proteoforms.Add(new_pf);
        //    this.experimental_proteoforms.Add(new_pf);
        //}

        public void add(ExperimentalProteoform pf)
        {
            this.experimental_proteoforms.Add(pf);
        }

        public void add(TheoreticalProteoform pf)
        {
            this.theoretical_proteoforms.Add(pf);
        }

        public void add(TheoreticalProteoform pf, string decoy_database)
        {
            this.decoy_proteoforms[decoy_database].Add(pf);
        }

        public void calculate_aggregated_experimental_masses()
        {
            Parallel.ForEach<ExperimentalProteoform>(this.experimental_proteoforms, pf => { pf.calculate_properties(); });
        }

        public void Clear()
        {
            this.experimental_proteoforms.Clear();
            this.theoretical_proteoforms.Clear();
            this.decoy_proteoforms.Clear();
        }

        //BUILDING RELATIONSHIPS
        private List<ProteoformRelation> relate(Proteoform[] pfs1, Proteoform[] pfs2, ProteoformComparison relation_type)
        {
            List<ProteoformRelation> relations = new List<ProteoformRelation>(
                from pf1 in pfs1
                from pf2 in pfs2
                where pf1.lysine_count == pf2.lysine_count
                where Math.Abs(pf1.modified_mass - pf2.modified_mass) <= Lollipop.max_mass_difference
                select new ProteoformRelation(pf1, pf2, relation_type, pf1.modified_mass - pf2.modified_mass)
            );
            count_nearby_relations(relations);
            return relations;
        }

        private void count_nearby_relations(List<ProteoformRelation> relations)
        {
            Parallel.ForEach<ProteoformRelation>(relations, relation => relation.set_nearby_group(relations));
        }

        public List<ProteoformRelation> relate_ee()
        {
            return relate(this.experimental_proteoforms.ToArray(), this.experimental_proteoforms.ToArray(), ProteoformComparison.ee);
        }
        public List<ProteoformRelation> relate_et()
        {
            return relate(this.experimental_proteoforms.ToArray(), this.theoretical_proteoforms.ToArray(), ProteoformComparison.et);
        }
        public Dictionary<string, List<ProteoformRelation>> relate_ed()
        {
            Dictionary<string, List<ProteoformRelation>> ed_relations = new Dictionary<string, List<ProteoformRelation>>();
            Parallel.ForEach<KeyValuePair<string, List<TheoreticalProteoform>>>(this.decoy_proteoforms, decoys =>
            {
                ed_relations[decoys.Key] = relate(experimental_proteoforms.ToArray(), decoys.Value.ToArray(), ProteoformComparison.ed);
            });
            return ed_relations;
        }
        public List<ProteoformRelation> relate_unequal_ee_lysine_counts()
        {
            Proteoform[] pfs1 = this.experimental_proteoforms.ToArray();
            Proteoform[] pfs2 = this.experimental_proteoforms.ToArray();
            List<ProteoformRelation> ef_relations = new List<ProteoformRelation>(
                from pf1 in pfs1
                from pf2 in pfs2
                where pf1.lysine_count != pf2.lysine_count
                where Math.Abs(pf1.modified_mass - pf2.modified_mass) <= Lollipop.max_mass_difference
                select new ProteoformRelation(pf1, pf2, ProteoformComparison.ef, pf1.modified_mass - pf2.modified_mass)
                );
            count_nearby_relations(ef_relations);
            return ef_relations;
        }

        //GROUP and ANALYZE RELATIONS
        public void accept_deltaMass_peak(List<ProteoformRelation> relations, Dictionary<string, List<ProteoformRelation>> decoy_relations)
        {
            List<ProteoformRelation> grouped_relations = new List<ProteoformRelation>();
            List<ProteoformRelation> exclusive_relation_group = relations.Except(grouped_relations).OrderByDescending(r => r.group_count)
                .Where(r => Math.Abs(r.group_adjusted_deltaM) >= Lollipop.no_mans_land_upperBound && Math.Abs(r.group_adjusted_deltaM) <= Lollipop.no_mans_land_lowerBound).ToList();
            List<DeltaMassPeak> relations_within_peak = new List<DeltaMassPeak>();
            foreach (ProteoformRelation relation in exclusive_relation_group)
            {
                relation.accept_exclusive_group(grouped_relations);
                if (relation.relation_type == ProteoformComparison.ee || relation.relation_type == ProteoformComparison.et) relations_within_peak.Add(new DeltaMassPeak(relation));
                grouped_relations.AddRange(relation.mass_difference_group);
            }
            Parallel.ForEach<DeltaMassPeak>(relations_within_peak, relation_group => relation_group.calculate_fdr(decoy_relations));
            this.relations_within_peak.AddRange(relations_within_peak);
        }
        public void accept_deltaMass_peak(List<ProteoformRelation> relations, List<ProteoformRelation> false_relations)
        {
            accept_deltaMass_peak(relations, new Dictionary<string, List<ProteoformRelation>> { { "", false_relations } });
        }

        //CONSTRUCTING FAMILIES
        public void construct_families()
        {

        }

        private void determine_adjacencies()
        {

        }
    }
}
