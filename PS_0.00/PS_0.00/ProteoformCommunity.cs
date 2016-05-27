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
        public List<ProteoformRelation> relations_in_peaks = new List<ProteoformRelation>();
        public List<DeltaMassPeak> delta_mass_peaks = new List<DeltaMassPeak>();
        public List<ProteoformFamily> families = new List<ProteoformFamily>();

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
            Parallel.ForEach<ExperimentalProteoform>(this.experimental_proteoforms, pf => pf.calculate_properties());
        }

        //BUILDING RELATIONSHIPS
        private List<ProteoformRelation> relate(Proteoform[] pfs1, Proteoform[] pfs2, ProteoformComparison relation_type)
        {
            List<ProteoformRelation> relations = new List<ProteoformRelation>(
                from pf1 in pfs1
                from pf2 in pfs2
                where pf1.lysine_count == pf2.lysine_count
                where Math.Abs(pf1.modified_mass - pf2.modified_mass) <= Lollipop.max_mass_difference //use if this step is rate-limiting, otherwise, just process them all
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
            List<ProteoformRelation> ef_relations = new List<ProteoformRelation>();
            Proteoform[] pfs1 = this.experimental_proteoforms.ToArray();
            Proteoform[] pfs2 = this.experimental_proteoforms.ToArray();
            Parallel.ForEach<Proteoform>(pfs1, pf1 =>
            {
                int num_equal_lysines = pfs2.Where(p => p.lysine_count == pf1.lysine_count).Count();
                new Random().Shuffle(pfs2);
                List<ProteoformRelation> ef_relation_addition = new List<ProteoformRelation>(
                    from pf2 in pfs2
                        .Where(p => p.lysine_count != pf1.lysine_count && Math.Abs(pf1.modified_mass - p.modified_mass) <= Lollipop.max_mass_difference)
                        .Take(pfs2.Where(p => p.lysine_count == pf1.lysine_count).Count()) // take only the number that would be chosen with equal lysine counts from a randomized set
                    select new ProteoformRelation(pf1, pf2, ProteoformComparison.ef, pf1.modified_mass - pf2.modified_mass)
                );
                count_nearby_relations(ef_relation_addition);
                ef_relations.AddRange(ef_relation_addition);
            });
            return ef_relations;
        }

        //GROUP and ANALYZE RELATIONS
        public List<DeltaMassPeak> accept_deltaMass_peaks(List<ProteoformRelation> relations, Dictionary<string, List<ProteoformRelation>> decoy_relations)
        {
            List<ProteoformRelation> grouped_relations = new List<ProteoformRelation>();
            List<ProteoformRelation> remaining_relations = exclusive_relation_group(relations, grouped_relations);
            List<DeltaMassPeak> peaks = new List<DeltaMassPeak>();
            while (remaining_relations.Count > 0)
            {
                ProteoformRelation top_relation = remaining_relations[0];
                List<ProteoformRelation> mass_differences_in_peak = top_relation.accept_exclusive(grouped_relations);
                if (top_relation.relation_type == ProteoformComparison.ee || top_relation.relation_type == ProteoformComparison.et)
                {
                    peaks.Add(new DeltaMassPeak(top_relation));
                    relations_in_peaks.AddRange(mass_differences_in_peak);
                }
                grouped_relations.AddRange(mass_differences_in_peak);
                remaining_relations = exclusive_relation_group(relations, grouped_relations);
            }
            Parallel.ForEach<DeltaMassPeak>(peaks, relation_group => relation_group.calculate_fdr(decoy_relations));
            this.delta_mass_peaks.AddRange(peaks);
            return peaks;
        }
        public List<DeltaMassPeak> accept_deltaMass_peaks(List<ProteoformRelation> relations, List<ProteoformRelation> false_relations)
        {
            return accept_deltaMass_peaks(relations, new Dictionary<string, List<ProteoformRelation>> { { "", false_relations } });
        }

        private List<ProteoformRelation> exclusive_relation_group(List<ProteoformRelation> relations, List<ProteoformRelation> grouped_relations)
        {
            return relations.Except(grouped_relations).OrderByDescending(r => r.group_count)
                .Where(r => Math.Abs(r.group_adjusted_deltaM) >= Lollipop.no_mans_land_upperBound &&
                            Math.Abs(r.group_adjusted_deltaM) <= Lollipop.no_mans_land_lowerBound).ToList();
        }

        //CONSTRUCTING FAMILIES
        //public void construct_families()
        //{
        //    List<Proteoform> inducted = new List<Proteoform>();
        //    List<Proteoform> remaining = new List<Proteoform>(this.experimental_proteoforms);
        //    while (remaining.Count > 0)
        //    {
        //        ProteoformFamily new_family = new ProteoformFamily(construct_family(new List<Proteoform> { remaining[0] }));
        //        this.families.Add(new_family);
        //        inducted.AddRange(new_family.proteoforms);
        //        remaining = remaining.Except(inducted).ToList();
        //    }
        //}

        //public List<Proteoform> construct_family(List<Proteoform> seed)
        //{
        //    List<Proteoform> expanded_seed = seed.SelectMany(p => p.get_connected_proteoforms()).ToList();
        //    if (expanded_seed.Except(seed).Count() == 0) return seed;
        //    else return construct_family(expanded_seed);
        //}

        //MISC
        public void Clear()
        {
            this.experimental_proteoforms.Clear();
            this.theoretical_proteoforms.Clear();
            this.decoy_proteoforms.Clear();
        }
    }
}
