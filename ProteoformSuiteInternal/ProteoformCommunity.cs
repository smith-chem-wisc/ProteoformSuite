using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ProteoformSuiteInternal;
using System.Threading.Tasks;
using System.Threading;

namespace ProteoformSuiteInternal
{
    public class ProteoformCommunity
    {
        //Please do not list {get;set} for new fields, so they are properly recorded in save all AC161103
        public ExperimentalProteoform[] experimental_proteoforms = new ExperimentalProteoform[0];
        public TheoreticalProteoform[] theoretical_proteoforms = new TheoreticalProteoform[0];
        public bool has_e_proteoforms
        {
            get { return experimental_proteoforms.Length > 0; }
        }
        public bool has_e_and_t_proteoforms
        {
            get { return experimental_proteoforms.Length > 0 && theoretical_proteoforms.Length > 0; }
        }
        public Dictionary<string, TheoreticalProteoform[]> decoy_proteoforms = new Dictionary<string, TheoreticalProteoform[]>();
        public List<ProteoformRelation> relations_in_peaks = new List<ProteoformRelation>();
        public List<DeltaMassPeak> delta_mass_peaks = new List<DeltaMassPeak>();
        public List<ProteoformFamily> families = new List<ProteoformFamily>();
        //public static double maximum_delta_mass_peak_fdr = 25;

        //BUILDING RELATIONSHIPS
        public List<ProteoformRelation> relate_et(Proteoform[] pfs1, Proteoform[] pfs2, ProteoformComparison relation_type)
        {
            ConcurrentBag<ProteoformRelation> relations = new ConcurrentBag<ProteoformRelation>();

            foreach (Proteoform pf1 in pfs1) // thread-unsafe portion, accessing pfs2
            {
                pf1.candidate_relatives = pfs2
                    .Where(pf2 => (!Lollipop.neucode_labeled || pf2.lysine_count == pf1.lysine_count)
                        && (pf1.modified_mass - pf2.modified_mass) >= Lollipop.et_low_mass_difference
                        && (pf1.modified_mass - pf2.modified_mass) <= Lollipop.et_high_mass_difference).ToList();
            }

            Parallel.ForEach(pfs1, pf1 => 
            {
                lock (pf1)
                    foreach (string accession in new HashSet<string>(pf1.candidate_relatives.Select(p => p.accession)))
                    {
                        List<Proteoform> candidate_pfs2_with_accession = pf1.candidate_relatives.Where(x => x.accession == accession).ToList();
                        candidate_pfs2_with_accession.Sort(Comparer<Proteoform>.Create((x, y) => Math.Abs(pf1.modified_mass - x.modified_mass).CompareTo(Math.Abs(pf1.modified_mass - y.modified_mass))));
                        Proteoform best_pf2 = candidate_pfs2_with_accession.First();
                        lock (best_pf2) lock (relations)
                                relations.Add(new ProteoformRelation(pf1, best_pf2, relation_type, pf1.modified_mass - best_pf2.modified_mass));
                    }
            });

            count_nearby_relations(relations.ToList());
            return relations.ToList();
        }

        
        public List<ProteoformRelation> relate_ee(ExperimentalProteoform[] pfs1, ExperimentalProteoform[] pfs2, ProteoformComparison relation_type)
        {
            List<ProteoformRelation> relations = new List<ProteoformRelation>(
                from pf1 in pfs1
                from pf2 in pfs2
                where allowed_ee_relation(pf1, pf2)
                select new ProteoformRelation(pf1, pf2, relation_type, pf1.modified_mass - pf2.modified_mass)
            );
            count_nearby_relations(relations);  //putative counts include no-mans land
            return relations;
        }

        public bool allowed_ee_relation(ExperimentalProteoform pf1, ExperimentalProteoform pf2)
        {
            return pf1.modified_mass >= pf2.modified_mass
                && pf1 != pf2
                && (!Lollipop.neucode_labeled || pf1.lysine_count == pf2.lysine_count)
                && pf1.modified_mass - pf2.modified_mass <= Lollipop.ee_max_mass_difference
                && Math.Abs(pf1.agg_rt - pf2.agg_rt) <= Lollipop.ee_max_RetentionTime_difference;
                //where ProteoformRelation.mass_difference_is_outside_no_mans_land(pf1.modified_mass - pf2.modified_mass)
                //putative counts include no-mans land, currently
        }

        private static void count_nearby_relations(List<ProteoformRelation> all_relations)
        {
            //PARALLEL PROBLEM
            //Parallel.ForEach<ProteoformRelation>(relations, relation => relation.set_nearby_group(relations));
            foreach (ProteoformRelation relation in all_relations) relation.set_nearby_group(all_relations);
        }

        public Dictionary<string, List<ProteoformRelation>> relate_ed()
        {
            Dictionary<string, List<ProteoformRelation>> ed_relations = new Dictionary<string, List<ProteoformRelation>>();
            Parallel.ForEach(decoy_proteoforms, decoys =>
            {
                ed_relations[decoys.Key] = relate_et(experimental_proteoforms, decoys.Value, ProteoformComparison.ed);
            });
            return ed_relations;
        }
        public List<ProteoformRelation> relate_unequal_ee_lysine_counts()
        {
            List<ProteoformRelation> ef_relations = new List<ProteoformRelation>();
            ExperimentalProteoform[] pfs1 = new List<ExperimentalProteoform>(this.experimental_proteoforms).ToArray();
            ExperimentalProteoform[] pfs2 = new List<ExperimentalProteoform>(this.experimental_proteoforms).ToArray();
            foreach (ExperimentalProteoform pf1 in pfs1)
            {
                int num_equal_lysines = pfs2.Where(pf2 => allowed_ee_relation(pf1, pf2)).Count(); //number that would be chosen with equal lysine counts from a randomized set
                new Random().Shuffle(pfs2);
                List<ProteoformRelation> ef_relation_addition = pfs2
                        .Where(p => p.lysine_count != pf1.lysine_count && Math.Abs(pf1.modified_mass - p.modified_mass) <= Lollipop.ee_max_mass_difference)
                        .Take(num_equal_lysines)
                        .Select(pf2 => new ProteoformRelation(pf1, pf2, ProteoformComparison.ef, pf1.modified_mass - pf2.modified_mass)).ToList();
                count_nearby_relations(ef_relation_addition);
                ef_relations.AddRange(ef_relation_addition);
            }
            return ef_relations;
        }

        //GROUP and ANALYZE RELATIONS
        public List<DeltaMassPeak> accept_deltaMass_peaks(List<ProteoformRelation> relations, Dictionary<string, List<ProteoformRelation>> decoy_relations)
        {
            //order by E intensity, then by descending unadjusted_group_count (running sum) before forming peaks, and analyze only relations outside of no-man's-land
            List<ProteoformRelation> grouped_relations = new List<ProteoformRelation>();
            List<ProteoformRelation> remaining_relations_outside_no_mans = relations.OrderByDescending(r => r.nearby_relations_count).
                ThenByDescending(r => r.agg_intensity_1).Where(r => r.outside_no_mans_land).ToList(); // Group count is the primary sort
            List<DeltaMassPeak> peaks = new List<DeltaMassPeak>();
            while (remaining_relations_outside_no_mans.Count > 0)
            {
                ProteoformRelation top_relation = remaining_relations_outside_no_mans[0];
                if (top_relation.relation_type != ProteoformComparison.ee && top_relation.relation_type != ProteoformComparison.et)
                    throw new Exception("Only EE and ET peaks can be accepted");

                DeltaMassPeak new_peak = new DeltaMassPeak(top_relation, remaining_relations_outside_no_mans);
                if (Lollipop.decoy_databases > 0) new_peak.calculate_fdr(decoy_relations);
                peaks.Add(new_peak);

                List<ProteoformRelation> mass_differences_in_peak = new_peak.grouped_relations;
                relations_in_peaks.AddRange(mass_differences_in_peak);
                grouped_relations.AddRange(mass_differences_in_peak);
                remaining_relations_outside_no_mans = exclusive_relation_group(remaining_relations_outside_no_mans, grouped_relations);
            }

            this.delta_mass_peaks.AddRange(peaks);
            return peaks;
        }

        public static ProteoformRelation find_next_root(List<ProteoformRelation> ordered, List<ProteoformRelation> running)
        {
            return ordered.FirstOrDefault(r =>
                running.All(s =>
                    r.delta_mass < s.delta_mass - 4 || r.delta_mass > s.delta_mass + 4));

            //if (top_relation.relation_type != ProteoformComparison.ee && top_relation.relation_type != ProteoformComparison.et)
            //    throw new Exception("Only EE and ET peaks can be accepted");
        }

        public List<DeltaMassPeak> accept_deltaMass_peaks(List<ProteoformRelation> relations, List<ProteoformRelation> false_relations)
        {
            return accept_deltaMass_peaks(relations, new Dictionary<string, List<ProteoformRelation>> { { "", false_relations } });
        }

        private List<ProteoformRelation> exclusive_relation_group(List<ProteoformRelation> relations, List<ProteoformRelation> grouped_relations)
        {
            return relations.Except(grouped_relations).OrderByDescending(r => r.nearby_relations_count).ThenByDescending(r => r.agg_intensity_1).ToList();
        }

        //CONSTRUCTING FAMILIES
        public void construct_families()
        {
            List<Proteoform> inducted = new List<Proteoform>();
            List<Proteoform> remaining = new List<Proteoform>(this.experimental_proteoforms);
            int family_id = 1;
            while (remaining.Count > 0)
            {
                ProteoformFamily new_family = new ProteoformFamily(construct_family(new List<Proteoform> { remaining[0] }), family_id);
                this.families.Add(new_family);
                inducted.AddRange(new_family.proteoforms);
                remaining = remaining.Except(inducted).ToList();
                foreach (Proteoform member in new_family.proteoforms) member.family = new_family;
                family_id++;
            }
        }

        public List<Proteoform> construct_family(List<Proteoform> seed)
        {
            List<Proteoform> seed_expansion = seed.SelectMany(p => p.get_connected_proteoforms().Except(seed)).ToList();
            if (seed_expansion.Except(seed).Count() == 0) return seed;
            seed.AddRange(seed_expansion);
            return construct_family(seed);
        }
    }
}

// THREADING DELTAMASSPEAK FINDING (draft, not validated)

//public List<ProteoformRelation> grouped_relations = new List<ProteoformRelation>();
//public List<ProteoformRelation> remaining_relations_outside_no_mans = new List<ProteoformRelation>();
//public List<DeltaMassPeak> accept_deltaMass_peaks(List<ProteoformRelation> relations, Dictionary<string, List<ProteoformRelation>> decoy_relations)
//{
//    //order by E intensity, then by descending unadjusted_group_count (running sum) before forming peaks, and analyze only relations outside of no-man's-land
//    List<ProteoformRelation> remaining_relations_outside_no_mans = relations.OrderByDescending(r => r.nearby_relations_count).ThenByDescending(r => r.agg_intensity_1).Where(r => r.outside_no_mans_land).ToList(); // Group count is the primary sort
//    List<DeltaMassPeak> peaks = new List<DeltaMassPeak>();

//    ProteoformRelation root = remaining_relations_outside_no_mans[0];
//    List<ProteoformRelation> running = new List<ProteoformRelation>();
//    List<Thread> active = new List<Thread>();
//    while (remaining_relations_outside_no_mans.Count > 0 || active.Count > 0)
//    {
//        while (root != null && active.Count < Environment.ProcessorCount)
//        {
//            Thread t = new Thread(new ThreadStart(root.generate_peak));
//            t.Start();
//            running.Add(root);
//            active.Add(t);
//            root = find_next_root(remaining_relations_outside_no_mans, running);
//        }

//        foreach (Thread t in active)
//        {
//            t.Join();
//        }

//        foreach (ProteoformRelation r in running)
//        {
//            peaks.Add(r.peak);
//            List<ProteoformRelation> mass_differences_in_peak = r.peak.grouped_relations;
//            relations_in_peaks.AddRange(mass_differences_in_peak);
//            grouped_relations.AddRange(mass_differences_in_peak);
//            remaining_relations_outside_no_mans = exclusive_relation_group(remaining_relations_outside_no_mans, grouped_relations);
//        }

//        running.Clear();
//        active.Clear();
//        root = find_next_root(remaining_relations_outside_no_mans, running);
//    }
//    this.delta_mass_peaks.AddRange(peaks);
//    return peaks;
//}