using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ProteoformSuiteInternal;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class ProteoformCommunity
    {
        //Please do not list {get;set} for new fields, so they are properly recorded in save all AC161103
        public ExperimentalProteoform[] experimental_proteoforms = new ExperimentalProteoform[0];
        public TheoreticalProteoform[] theoretical_proteoforms = new TheoreticalProteoform[0];
        public List<TopDownProteoform> topdown_proteoforms = new List<TopDownProteoform>();

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
            object sync = new object();

            Parallel.ForEach(pfs1, pf1 => 
            {
                lock (sync)
                {
                    IEnumerable<Proteoform> candidate_pfs2 = pfs2.
                        Where(pf2 => (!Lollipop.neucode_labeled || pf2.lysine_count == pf1.lysine_count)
                            && (pf1.modified_mass - pf2.modified_mass) >= Lollipop.et_low_mass_difference
                            && (pf1.modified_mass - pf2.modified_mass) <= Lollipop.et_high_mass_difference);

                    foreach (string accession in new HashSet<string>(candidate_pfs2.Select(p => p.accession)))
                    {
                        List<Proteoform> candidate_pfs2_with_accession = candidate_pfs2.Where(x => x.accession == accession).ToList();
                        candidate_pfs2_with_accession.Sort(Comparer<Proteoform>.Create((x, y) => Math.Abs(pf1.modified_mass - x.modified_mass).CompareTo(Math.Abs(pf1.modified_mass - y.modified_mass))));
                        Proteoform best_pf2 = candidate_pfs2_with_accession.First();
                        ProteoformRelation pr = new ProteoformRelation(pf1, best_pf2, relation_type, pf1.modified_mass - best_pf2.modified_mass);
                        List<ProteoformRelation> neucode_detected = new List<ProteoformRelation>(); 
                        if (Lollipop.limit_NC_et_pairs) neucode_detected = Lollipop.neucode_et_pairs.Where(r => ((TheoreticalProteoform)r.connected_proteoforms[1]).accession_reduced == ((TheoreticalProteoform)best_pf2).accession_reduced
                          && ((TheoreticalProteoform)r.connected_proteoforms[1]).ptm_descriptions_readin == ((TheoreticalProteoform)best_pf2).ptm_descriptions
                          && Math.Abs(pr.delta_mass - r.delta_mass) <= Lollipop.NC_et_mass_tol).ToList();
                       if (!Lollipop.limit_NC_et_pairs|| neucode_detected.Count > 0) relations.Add(pr);
                    }
                }
            });

            count_nearby_relations(relations.ToList());
            return relations.ToList();
        }

        
        public List<ProteoformRelation> relate_ee(ExperimentalProteoform[] pfs1, ExperimentalProteoform[] pfs2, ProteoformComparison relation_type)
        {
            List<ProteoformRelation> relations_in_NC = new List<ProteoformRelation>();
            List<ProteoformRelation> relations = new List<ProteoformRelation>(
                from pf1 in pfs1
                from pf2 in pfs2
                where allowed_ee_relation(pf1, pf2)
                select new ProteoformRelation(pf1, pf2, relation_type, pf1.modified_mass - pf2.modified_mass)
            );
            if (!Lollipop.limit_NC_ee_pairs) { count_nearby_relations(relations); return relations; }
            
            //for label-free, if read-in neucode results
            else
            {
                foreach (ProteoformRelation pf in relations)
                {
                    double heavy_mass = ((ExperimentalProteoform)pf.connected_proteoforms[0]).agg_mass;
                    double light_mass = ((ExperimentalProteoform)pf.connected_proteoforms[1]).agg_mass;
                   List<ProteoformRelation> in_neucode = Lollipop.neucode_ee_pairs.Where(r =>
                   Math.Abs(((ExperimentalProteoform)r.connected_proteoforms[0]).agg_mass - heavy_mass) <= Lollipop.NC_ee_mass_tol
                   && Math.Abs(((ExperimentalProteoform)r.connected_proteoforms[1]).agg_mass - light_mass) <= Lollipop.NC_ee_mass_tol).ToList();
                    if (in_neucode.Count > 0) relations_in_NC.Add(pf);
                }
                count_nearby_relations(relations_in_NC); 
                return relations_in_NC;
            }
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
                ed_relations[decoys.Key] = relate_et(experimental_proteoforms.Where(p => p.accepted).ToArray(), decoys.Value, ProteoformComparison.ed);
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

        public List<ProteoformRelation> relate_td(List<ExperimentalProteoform> experimentals, List<TheoreticalProteoform> theoreticals, List<TopDownProteoform> topdowns)
        {

            List<ProteoformRelation> etd_full_relations = new List<ProteoformRelation>();
            List<ProteoformRelation> td_relations = new List<ProteoformRelation>();

            int max_missed_monoisotopics = Convert.ToInt32(Lollipop.missed_monos);
            List<int> missed_monoisotopics_range = Enumerable.Range(-max_missed_monoisotopics, max_missed_monoisotopics * 2 + 1).ToList();
            foreach (TopDownProteoform td_proteoform in topdowns)
            {
                foreach (int m in missed_monoisotopics_range)
                {
                    double shift = m * Lollipop.MONOISOTOPIC_UNIT_MASS;
                    double mass_tol = (td_proteoform.modified_mass + shift) / 1000000 * Convert.ToInt32(Lollipop.mass_tolerance);
                    double low = td_proteoform.modified_mass + shift - mass_tol;
                    double high = td_proteoform.modified_mass + shift + mass_tol;
                    List<ExperimentalProteoform> matching_e = experimentals.Where(ep => ep.modified_mass >= low && ep.modified_mass <= high
                    && Math.Abs(ep.agg_rt - td_proteoform.agg_rt) < Convert.ToDouble(Lollipop.retention_time_tolerance)).ToList();
                    foreach (ExperimentalProteoform e in matching_e)
                    {
                        ProteoformRelation td_relation = new ProteoformRelation( td_proteoform, e, ProteoformComparison.etd, (e.modified_mass - td_proteoform.modified_mass));
                        etd_full_relations.Add(td_relation);
                    }
                }

                //match each td proteoform to the closest theoretical w/ same accession and number of modifications. (if no match always make relationship with unmodified)
                if (theoreticals.Count > 0)
                {
                    TheoreticalProteoform theo = null;
                    try
                    {
                        theo = theoreticals.Where(t => t.accession_reduced == td_proteoform.accession.Split('_')[0] && t.ptm_list.Count 
                            == td_proteoform.ptm_list.Count).OrderBy(t => Math.Abs(t.modified_mass - td_proteoform.theoretical_mass)).First();

                    }
                    catch {

                        if (td_proteoform.ptm_list.Count == 0) continue; //if can't find match to unmodified topdown, nothing to do
                        //if modified topdown, compare with unmodified theoretical
                        else
                        {
                            try
                            {
                                theo = theoreticals.Where(t => t.accession_reduced == td_proteoform.accession.Split('_')[0] && t.ptm_list.Count == 0).OrderBy(
                          t => Math.Abs(t.modified_mass - td_proteoform.theoretical_mass)).First();
                            }
                            catch { continue; }
                        }
                    }

                    ProteoformRelation t_td_relation = new ProteoformRelation(td_proteoform, theo, ProteoformComparison.ttd, (td_proteoform.theoretical_mass - theo.modified_mass));
                    t_td_relation.accepted = true;
                    t_td_relation.connected_proteoforms[0].relationships.Add(t_td_relation);
                    t_td_relation.connected_proteoforms[1].relationships.Add(t_td_relation);
                    td_relations.Add(t_td_relation);
                }
            }

            //map each experimental to only one td proteoform of the same accession #
            foreach (ExperimentalProteoform e in etd_full_relations.Where(r => r.relation_type == ProteoformComparison.etd).Select(r => r.connected_proteoforms[1]).Distinct())
            {
                List<ProteoformRelation> relations = etd_full_relations.Where(r => r.connected_proteoforms[1] == e).ToList();
                foreach (string accession in new HashSet<string>(relations.Select(r => r.connected_proteoforms[0].accession.Split('_')[0])).Distinct())
                {
                    List<ProteoformRelation> relations_with_accession = relations.Where(x => x.connected_proteoforms[0].accession.Split('_')[0] == accession).ToList();
                    ProteoformRelation best = relations_with_accession.OrderBy(x => Math.Abs(x.delta_mass - Math.Round(x.delta_mass, 0))).First();
                    best.accepted = true;
                    best.connected_proteoforms[0].relationships.Add(best);
                    best.connected_proteoforms[1].relationships.Add(best);
                    td_relations.Add(best);
                }
            }

            return td_relations;
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
            List<Proteoform> remaining = new List<Proteoform>(this.experimental_proteoforms.Where(e => e.accepted).ToList());
            remaining.AddRange(this.topdown_proteoforms);
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
