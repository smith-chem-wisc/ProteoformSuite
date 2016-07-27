using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
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
            if (pf != null) this.experimental_proteoforms.Add(pf);
        }
        public void add(TheoreticalProteoform pf)
        {
            if (pf != null) this.theoretical_proteoforms.Add(pf);
        }
        public void add(TheoreticalProteoform pf, string decoy_database)
        {
            if (pf != null) this.decoy_proteoforms[decoy_database].Add(pf);
        }

        //BUILDING RELATIONSHIPS
        public List<ProteoformRelation> relate_et(Proteoform[] pfs1, Proteoform[] pfs2, ProteoformComparison relation_type)
        {
            pfs1 = pfs1.Where(p => p != null).ToArray(); //this should be the set of experimental values
            pfs2 = pfs2.Where(p => p != null).ToArray(); //this should be the set of theoretical values

            if (Lollipop.neucode_labeled)
            {
                List<ProteoformRelation> relations = new List<ProteoformRelation>(
                from pf1 in pfs1
                from pf2 in pfs2
                where pf1.lysine_count == pf2.lysine_count &&
                    pf1.modified_mass - pf2.modified_mass >= Lollipop.et_low_mass_difference && //use if this step is rate-limiting, otherwise, just process them all
                    pf1.modified_mass - pf2.modified_mass <= Lollipop.et_high_mass_difference //use if this step is rate-limiting, otherwise, just process them all
                select new ProteoformRelation(pf1, pf2, relation_type, pf1.modified_mass - pf2.modified_mass)
            );
                count_nearby_relations(relations.Where(p => p.outside_no_mans_land).ToList()); //only make peaks out of relations outside no-mans-land
                return relations;
            }
            else
            {
                List<ProteoformRelation> relations = new List<ProteoformRelation>(
                from pf1 in pfs1
                from pf2 in pfs2
                where pf1.modified_mass - pf2.modified_mass >= Lollipop.et_low_mass_difference && //use if this step is rate-limiting, otherwise, just process them all
                    pf1.modified_mass - pf2.modified_mass <= Lollipop.et_high_mass_difference//use if this step is rate-limiting, otherwise, just process them all
                select new ProteoformRelation(pf1, pf2, relation_type, pf1.modified_mass - pf2.modified_mass)
            );
                count_nearby_relations(relations.Where(p => p.outside_no_mans_land).ToList());  //only make peaks out of relations outside no-mans-land
                return relations;
            }
        }

        public List<ProteoformRelation> relate_ee(Proteoform[] pfs1, Proteoform[] pfs2, ProteoformComparison relation_type)
        {
            pfs1 = pfs1.Where(p => p != null).ToArray(); //this should be the set of experimental values
            pfs2 = pfs2.Where(p => p != null).ToArray(); //this should be the set of experimental values

            if (Lollipop.neucode_labeled)
            {
                List<ProteoformRelation> relations = new List<ProteoformRelation>(
                from pf1 in pfs1
                from pf2 in pfs2
                where pf1.modified_mass > pf2.modified_mass
                where pf1.lysine_count == pf2.lysine_count
                where pf1.modified_mass - pf2.modified_mass <= Lollipop.ee_max_mass_difference //use if this step is rate-limiting, otherwise, just process them all
                select new ProteoformRelation(pf1, pf2, relation_type, pf1.modified_mass - pf2.modified_mass)
            );
                count_nearby_relations(relations.Where(p => p.outside_no_mans_land).ToList());  //only make peaks out of relations outside no-mans-land
                return relations;
            }
            else
            {
                List<ProteoformRelation> relations = new List<ProteoformRelation>(
                from pf1 in pfs1
                from pf2 in pfs2
                where pf1.modified_mass > pf2.modified_mass
                where pf1.modified_mass - pf2.modified_mass <= Lollipop.ee_max_mass_difference //use if this step is rate-limiting, otherwise, just process them all
                select new ProteoformRelation(pf1, pf2, relation_type, pf1.modified_mass - pf2.modified_mass)
            );
                count_nearby_relations(relations.Where(p => p.outside_no_mans_land).ToList());
                return relations;
            }
        }

        private static void count_nearby_relations(List<ProteoformRelation> relations)
        {
            //PARALELL PROBLEM
            //Parallel.ForEach<ProteoformRelation>(relations, relation => relation.set_nearby_group(relations));
            foreach(ProteoformRelation relation in relations)
            {
                relation.set_nearby_group(relations);
            }
        }

        //public List<ProteoformRelation> relate_ee()
        //{
        //    return relate(this.experimental_proteoforms.ToArray(), this.experimental_proteoforms.ToArray(), ProteoformComparison.ee);
        //}
        //public List<ProteoformRelation> relate_et()
        //{
        //    return relate(this.experimental_proteoforms.ToArray(), this.theoretical_proteoforms.ToArray(), ProteoformComparison.et);
        //}

        public Dictionary<string, List<ProteoformRelation>> relate_ed()
        {
            Dictionary<string, List<ProteoformRelation>> ed_relations = new Dictionary<string, List<ProteoformRelation>>();
            Parallel.ForEach<KeyValuePair<string, List<TheoreticalProteoform>>>(this.decoy_proteoforms, decoys =>
            {
                ed_relations[decoys.Key] = relate_et(experimental_proteoforms.ToArray(), decoys.Value.ToArray(), ProteoformComparison.ed);
            });
            return ed_relations;
        }
        public List<ProteoformRelation> relate_unequal_ee_lysine_counts()
        {
            List<ProteoformRelation> ef_relations = new List<ProteoformRelation>();
            Proteoform[] pfs1 = this.experimental_proteoforms.ToArray();
            Proteoform[] pfs2 = this.experimental_proteoforms.ToArray();
            foreach (ExperimentalProteoform pf1 in pfs1)
            {
                int num_equal_lysines = pfs2.Where(p => p.lysine_count == pf1.lysine_count).Count();
                new Random().Shuffle(pfs2);
                List<ProteoformRelation> ef_relation_addition = new List<ProteoformRelation>(
                    from pf2 in pfs2
                        .Where(p => p.lysine_count != pf1.lysine_count && Math.Abs(pf1.modified_mass - p.modified_mass) <= Lollipop.ee_max_mass_difference)
                        .Take(pfs2.Where(p => p.lysine_count == pf1.lysine_count).Count()) // take only the number that would be chosen with equal lysine counts from a randomized set
                    select new ProteoformRelation(pf1, pf2, ProteoformComparison.ef, pf1.modified_mass - pf2.modified_mass)
                );
                count_nearby_relations(ef_relation_addition.Where(p => p.outside_no_mans_land).ToList());
                ef_relations.AddRange(ef_relation_addition);
            }
            return ef_relations;
        }

        //GROUP and ANALYZE RELATIONS
        public List<DeltaMassPeak> accept_deltaMass_peaks(List<ProteoformRelation> relations, Dictionary<string, List<ProteoformRelation>> decoy_relations)
        {
            List<ProteoformRelation> grouped_relations = new List<ProteoformRelation>();
            List<ProteoformRelation> relations_outside_no_mans = relations.Where(r => r.outside_no_mans_land).ToList();
            //analyze relations outside of no-man's-land
            //List<ProteoformRelation> remaining_relations = exclusive_relation_group(relations_in_no_mans, );
            List<DeltaMassPeak> peaks = new List<DeltaMassPeak>();
            while (relations_outside_no_mans.Count > 0)
            {
                ProteoformRelation top_relation = relations_outside_no_mans[0];
                List<ProteoformRelation> mass_differences_in_peak = top_relation.accept_exclusive(grouped_relations);
                if (top_relation.relation_type == ProteoformComparison.ee || top_relation.relation_type == ProteoformComparison.et)
                {
                    peaks.Add(new DeltaMassPeak(top_relation));
                    relations_in_peaks.AddRange(mass_differences_in_peak);
                }
                grouped_relations.AddRange(mass_differences_in_peak);
                relations_outside_no_mans = exclusive_relation_group(relations_outside_no_mans, grouped_relations);
            }
            //PARALLEL PROBLEM
           // Parallel.ForEach<DeltaMassPeak>(peaks, relation_group => relation_group.calculate_fdr(decoy_relations));
            foreach (DeltaMassPeak relation_group in peaks)
            {
                relation_group.calculate_fdr(decoy_relations);
            }
            this.delta_mass_peaks.AddRange(peaks);
            return peaks;
        }
        public List<DeltaMassPeak> accept_deltaMass_peaks(List<ProteoformRelation> relations, List<ProteoformRelation> false_relations)
        {
            return accept_deltaMass_peaks(relations, new Dictionary<string, List<ProteoformRelation>> { { "", false_relations } });
        }

        private List<ProteoformRelation> exclusive_relation_group(List<ProteoformRelation> relations, List<ProteoformRelation> grouped_relations)
        {
            return relations.Except(grouped_relations).OrderByDescending(r => r.unadjusted_group_count).ToList();
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
