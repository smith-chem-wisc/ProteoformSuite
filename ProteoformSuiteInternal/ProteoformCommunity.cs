using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace ProteoformSuiteInternal
{
    public class ProteoformCommunity
    {

        #region Public Fields

        public ExperimentalProteoform[] experimental_proteoforms = new ExperimentalProteoform[0];
        public TheoreticalProteoform[] theoretical_proteoforms = new TheoreticalProteoform[0];
        public TopDownProteoform[] topdown_proteoforms = new TopDownProteoform[0];
        public List<ProteoformFamily> families = new List<ProteoformFamily>();

        #endregion Public Fields

        #region Public Properties

        public bool has_e_proteoforms
        {
            get { return experimental_proteoforms.Length > 0; }
        }

        public bool has_e_and_t_proteoforms
        {
            get { return experimental_proteoforms.Length > 0 && theoretical_proteoforms.Length > 0; }
        }

        #endregion

        #region BUILDING RELATIONSHIPS

        public List<ProteoformRelation> relate(ExperimentalProteoform[] pfs1, Proteoform[] pfs2, ProteoformComparison relation_type, bool accepted_only)
        {
            if (accepted_only)
                pfs1 = pfs1.Where(pf1 => pf1.accepted).ToArray();

            if (accepted_only && (relation_type == ProteoformComparison.ExperimentalExperimental || relation_type == ProteoformComparison.ExperimentalFalse))
                pfs2 = pfs2.OfType<ExperimentalProteoform>().Where(pf2 => pf2.accepted).ToArray();

            Parallel.ForEach(pfs1, pf1 =>
            {
                lock (pf1)
                {
                    pf1.candidate_relatives = pfs2.Where(pf2 => allowed_relation(pf1, pf2, relation_type)).ToList();

                    if (relation_type == ProteoformComparison.ExperimentalExperimental)
                    {
                        pf1.ptm_set = null;
                        pf1.linked_proteoform_references = null;
                        pf1.gene_name = null;
                    }

                    if (relation_type == ProteoformComparison.ExperimentalTheoretical || relation_type == ProteoformComparison.ExperimentalDecoy)
                    {
                        ProteoformRelation best_relation = pf1.candidate_relatives
                            .Select(pf2 => new ProteoformRelation(pf1, pf2, relation_type, pf1.modified_mass - pf2.modified_mass))
                            .Where(r => r.candidate_ptmset != null) // don't consider unassignable relations for ET
                            .OrderBy(r => r.candidate_ptmset.ptm_rank_sum + Math.Abs(Math.Abs(r.candidate_ptmset.mass) - Math.Abs(r.DeltaMass)) * 10E-6) // get the best explanation for the experimental observation
                            .FirstOrDefault();

                        pf1.candidate_relatives = best_relation != null ?
                            new List<Proteoform> { best_relation.connected_proteoforms[1] } :
                            new List<Proteoform>();
                    }
                }
            });

            List<ProteoformRelation> relations =
                (from pf1 in pfs1
                 from pf2 in pf1.candidate_relatives
                 select new ProteoformRelation(pf1, pf2, relation_type, pf1.modified_mass - pf2.modified_mass)).ToList();

            return count_nearby_relations(relations);  //putative counts include no-mans land
        }

        public bool allowed_relation(Proteoform pf1, Proteoform pf2, ProteoformComparison relation_type)
        {
            switch (relation_type)
            {
                case (ProteoformComparison.ExperimentalTheoretical):
                case (ProteoformComparison.ExperimentalDecoy):
                    return (!SaveState.lollipop.neucode_labeled || pf2.lysine_count == pf1.lysine_count)
                        && (pf1.modified_mass - pf2.modified_mass) >= SaveState.lollipop.et_low_mass_difference
                        && (pf1.modified_mass - pf2.modified_mass) <= SaveState.lollipop.et_high_mass_difference
                        && (pf2.ptm_set.ptm_combination.Count < 3 || pf2.ptm_set.ptm_combination.Select(ptm => ptm.modification.monoisotopicMass).All(x => x == pf2.ptm_set.ptm_combination.First().modification.monoisotopicMass));

                case (ProteoformComparison.ExperimentalExperimental):
                    return pf1.modified_mass >= pf2.modified_mass
                        && pf1 != pf2
                        && (!SaveState.lollipop.neucode_labeled || pf1.lysine_count == pf2.lysine_count)
                        && pf1.modified_mass - pf2.modified_mass <= SaveState.lollipop.ee_max_mass_difference
                        && Math.Abs(((ExperimentalProteoform)pf1).agg_rt - ((ExperimentalProteoform)pf2).agg_rt) <= SaveState.lollipop.ee_max_RetentionTime_difference;

                case (ProteoformComparison.ExperimentalFalse):
                    return pf1.modified_mass >= pf2.modified_mass
                        && pf1 != pf2
                        && (pf1.modified_mass - pf2.modified_mass <= SaveState.lollipop.ee_max_mass_difference)
                        && (!SaveState.lollipop.neucode_labeled || pf1.lysine_count != pf2.lysine_count)
                        && (SaveState.lollipop.neucode_labeled || Math.Abs(((ExperimentalProteoform)pf1).agg_rt - ((ExperimentalProteoform)pf2).agg_rt) > SaveState.lollipop.ee_max_RetentionTime_difference * 2)
                        && (!SaveState.lollipop.neucode_labeled || Math.Abs(((ExperimentalProteoform)pf1).agg_rt - ((ExperimentalProteoform)pf2).agg_rt) < SaveState.lollipop.ee_max_RetentionTime_difference);
                default:
                    return false;
            }
        }

        public static List<ProteoformRelation> count_nearby_relations(List<ProteoformRelation> all_relations)
        {
            List<ProteoformRelation> all_ordered_relations = all_relations.OrderBy(x => x.DeltaMass).ToList();
            List<int> ordered_relation_ids = all_ordered_relations.Select(r => r.InstanceId).ToList();
            Parallel.ForEach(all_ordered_relations, relation => relation.set_nearby_group(all_ordered_relations, ordered_relation_ids));
            return all_ordered_relations;
        }

        //public Dictionary<string, List<ProteoformRelation>> relate_ed()
        //{
        //    Dictionary<string, List<ProteoformRelation>> ed_relations = new Dictionary<string, List<ProteoformRelation>>();
        //    foreach (var decoys in decoy_proteoforms)
        //    {
        //        ed_relations.Add(decoys.Key, relate(experimental_proteoforms, decoys.Value, ProteoformComparison.ExperimentalDecoy, true));
        //    }
        //    return ed_relations;
        //}

        public List<ProteoformRelation> relate_ef(ExperimentalProteoform[] pfs1, ExperimentalProteoform[] pfs2)
        {
            List<ProteoformRelation> all_ef_relations = relate(pfs1, pfs2, ProteoformComparison.ExperimentalFalse, true);
            ProteoformRelation[] to_shuffle = new List<ProteoformRelation>(all_ef_relations).ToArray();
            to_shuffle.Shuffle();
            return to_shuffle.Take(SaveState.lollipop.ee_relations.Count).ToList();
        }

        public List<ProteoformRelation> relate_td(List<ExperimentalProteoform> experimentals, List<TheoreticalProteoform> theoreticals, List<TopDownProteoform> topdowns)
        {
            List<ProteoformRelation> td_relations = new List<ProteoformRelation>();

            int max_missed_monoisotopics = Convert.ToInt32(SaveState.lollipop.missed_monos);
            List<int> missed_monoisotopics_range = Enumerable.Range(-max_missed_monoisotopics, max_missed_monoisotopics * 2 + 1).ToList();
            foreach (TopDownProteoform topdown in topdowns)
            {
                List<ProteoformRelation> all_td_relations = new List<ProteoformRelation>();
                double mass = topdown.monoisotopic_mass;

                foreach (int m in missed_monoisotopics_range)
                {
                        double shift = m * Lollipop.MONOISOTOPIC_UNIT_MASS;
                        double mass_tol = (mass + shift) / 1000000 * Convert.ToInt32(SaveState.lollipop.mass_tolerance);
                        double low = mass + shift - mass_tol;
                        double high = mass + shift + mass_tol;
                        List<ExperimentalProteoform> matching_e = experimentals.Where(ep => ep.modified_mass >= low && ep.modified_mass <= high
                        && (Math.Abs(ep.agg_rt - topdown.agg_RT) <= Convert.ToDouble(SaveState.lollipop.retention_time_tolerance))).ToList();
                        foreach (ExperimentalProteoform e in matching_e)
                        {
                            all_td_relations.Add(new ProteoformRelation(topdown, e, ProteoformComparison.ExperimentalTopDown, (e.modified_mass - mass)));
                        }
                }
                //add the best td relation
                foreach (ProteoformRelation relation in all_td_relations)
                {
                    if (SaveState.lollipop.theoretical_database.possible_ptmset_dictionary.TryGetValue(Math.Round(relation.DeltaMass, 1), out List<PtmSet> candidate_sets))
                    {
                        double mass_tolerance = topdown.modified_mass / 1000000 * (double)SaveState.lollipop.mass_tolerance;
                        relation.candidate_ptmset = topdown.generate_possible_added_ptmsets(candidate_sets.Where(s => Math.Abs(s.mass - relation.DeltaMass) < 0.05).ToList(), relation.DeltaMass, mass_tolerance, SaveState.lollipop.theoretical_database.all_mods_with_mass, topdown, topdown.sequence, SaveState.lollipop.mod_rank_first_quartile)
                        .OrderBy(x => (double)x.ptm_rank_sum + Math.Abs(Math.Abs(x.mass) - Math.Abs(relation.DeltaMass)) * 10E-6) // major score: delta rank; tie breaker: deltaM, where it's always less than 1
                        .FirstOrDefault();
                    }
                }

                ProteoformRelation best_relation = all_td_relations.Where(r => r.candidate_ptmset != null).OrderBy(r => r.candidate_ptmset.ptm_rank_sum).FirstOrDefault();
                if (best_relation != null)
                {
                    best_relation.connected_proteoforms[0].relationships.Add(best_relation);
                    best_relation.connected_proteoforms[1].relationships.Add(best_relation);
                    ((ExperimentalProteoform)best_relation.connected_proteoforms[1]).accepted = true;
                    best_relation.Accepted = true;
                    td_relations.Add(best_relation);
                }


                //match each td proteoform group to the closest theoretical w/ same accession and modifications. (if no match always make relationship with unmodified)
                //if accession the same, or uniprot ID the same, or same sequence (take into account cleaved methionine)
                List<TheoreticalProteoform> candidate_theoreticals = theoreticals.Where(t => t.name.Split(';').Contains(topdown.uniprot_id)).ToList();

                List<ProteoformRelation> possible_ttd_relations = candidate_theoreticals.Select(t => new ProteoformRelation(t, topdown, ProteoformComparison.TheoreticalTopDown, topdown.theoretical_mass - t.modified_mass)).ToList();
                ProteoformRelation best_ttd_relation;
                foreach (ProteoformRelation relation in possible_ttd_relations)
                {
                    if (SaveState.lollipop.theoretical_database.possible_ptmset_dictionary.TryGetValue(Math.Round(relation.DeltaMass, 1), out List<PtmSet> candidate_sets))
                    {
                        double mass_tolerance = topdown.theoretical_mass / 1000000 * (double)SaveState.lollipop.mass_tolerance;
                        relation.candidate_ptmset = topdown.generate_possible_added_ptmsets(candidate_sets.Where(s => Math.Abs(s.mass - relation.DeltaMass) < 0.05).ToList(), relation.DeltaMass, mass_tolerance, SaveState.lollipop.theoretical_database.all_mods_with_mass, relation.connected_proteoforms[0], ((TheoreticalProteoform)relation.connected_proteoforms[0]).sequence, SaveState.lollipop.mod_rank_first_quartile)
                        .OrderBy(x => (double)x.ptm_rank_sum + Math.Abs(Math.Abs(x.mass) - Math.Abs(relation.DeltaMass)) * 10E-6) // major score: delta rank; tie breaker: deltaM, where it's always less than 1
                        .FirstOrDefault();
                    }
                }
                best_ttd_relation = possible_ttd_relations.Where(r => r.candidate_ptmset != null).OrderBy(r => r.candidate_ptmset.ptm_rank_sum).FirstOrDefault();
                if (best_ttd_relation == null)
                {
                    best_ttd_relation = possible_ttd_relations.Where(r => r.connected_proteoforms[0].ptm_set.ptm_combination.Count == 0 && (((TheoreticalProteoform)r.connected_proteoforms[0]).fragment == "full" || ((TheoreticalProteoform)r.connected_proteoforms[0]).fragment == "full-met-cleaved")).FirstOrDefault();
                }
                if (best_ttd_relation == null)
                {
                    best_ttd_relation = possible_ttd_relations.Where(r => r.connected_proteoforms[0].ptm_set.ptm_combination.Count == 0).FirstOrDefault();
                }
                if (best_ttd_relation != null)
                {
                    best_ttd_relation.connected_proteoforms[0].relationships.Add(best_ttd_relation);
                    best_ttd_relation.connected_proteoforms[1].relationships.Add(best_ttd_relation);
                    best_ttd_relation.Accepted = true;
                    td_relations.Add(best_ttd_relation);
                }
                else
                {   //need to remove ETD relations of td that couldn't be matched with a theoretical --> no gene name! 
                    //shows Warning message in TopDown GUI if no TTD relations for the accession. 
                    td_relations.RemoveAll(p => p.connected_proteoforms.Contains(topdown));
                }
            }

            return td_relations;
        }

        #endregion BUILDING RELATIONSHIPS

        #region GROUP and ANALYZE RELATIONS

        public List<ProteoformRelation> remaining_relations_outside_no_mans = new List<ProteoformRelation>();
        public List<DeltaMassPeak> accept_deltaMass_peaks(List<ProteoformRelation> relations, Dictionary<string, List<ProteoformRelation>> decoy_relations)
        {
            //order by E intensity, then by descending unadjusted_group_count (running sum) before forming peaks, and analyze only relations outside of no-man's-land
            remaining_relations_outside_no_mans = relations.Where(r => r.outside_no_mans_land).OrderByDescending(r => r.nearby_relations_count).ThenByDescending(r => ((ExperimentalProteoform)r.connected_proteoforms[0]).agg_intensity).ToList(); // Group count is the primary sort
            List<DeltaMassPeak> peaks = new List<DeltaMassPeak>();

            ProteoformRelation root = remaining_relations_outside_no_mans.FirstOrDefault();
            List<ProteoformRelation> running = new List<ProteoformRelation>();
            List<Thread> active = new List<Thread>();
            while (remaining_relations_outside_no_mans.FirstOrDefault() != null || active.Count > 0)
            {
                while (root != null && active.Count < 1) // Use Environment.ProcessorCount instead of 1 to enable parallization
                {
                    if (root.RelationType != ProteoformComparison.ExperimentalExperimental && root.RelationType != ProteoformComparison.ExperimentalTheoretical)
                        throw new ArgumentException("Only EE and ET peaks can be accepted");

                    Thread t = new Thread(new ThreadStart(root.generate_peak));
                    t.Start();
                    running.Add(root);
                    active.Add(t);
                    root = find_next_root(remaining_relations_outside_no_mans, running);
                }

                foreach (Thread t in active)
                {
                    t.Join();
                }

                foreach (DeltaMassPeak peak in running.Select(r => r.peak))
                {
                    peaks.Add(peak);
                    Parallel.ForEach(peak.grouped_relations, relation =>
                    {
                        lock (relation)
                        {
                            relation.peak = peak;
                            relation.Accepted = peak.Accepted;
                        }
                    });
                }

                List<ProteoformRelation> mass_differences_in_peaks = running.SelectMany(r => r.peak.grouped_relations).ToList();
                remaining_relations_outside_no_mans = remaining_relations_outside_no_mans.Except(mass_differences_in_peaks).ToList();

                running.Clear();
                active.Clear();
                root = find_next_root(remaining_relations_outside_no_mans, running);
            }
            if (peaks.Count > 0 && peaks.First().RelationType == ProteoformComparison.ExperimentalTheoretical) SaveState.lollipop.et_peaks.AddRange(peaks); else SaveState.lollipop.ee_peaks.AddRange(peaks);

            //Nearby relations are no longer needed after counting them
            Parallel.ForEach(decoy_relations.SelectMany(kv => kv.Value).Concat(relations).ToList(), r =>
            {
                lock (r) r.nearby_relations.Clear();
            });

            return peaks;
        }

        public static ProteoformRelation find_next_root(IEnumerable<ProteoformRelation> ordered, IEnumerable<ProteoformRelation> running)
        {
            return ordered.FirstOrDefault(r =>
                running.All(s =>
                    r.DeltaMass < s.DeltaMass - 20 || r.DeltaMass > s.DeltaMass + 20));
        }

        public List<DeltaMassPeak> accept_deltaMass_peaks(List<ProteoformRelation> relations, List<ProteoformRelation> false_relations)
        {
            return accept_deltaMass_peaks(relations, new Dictionary<string, List<ProteoformRelation>> { { "", false_relations } });
        }

        #endregion  GROUP and ANALYZE RELATIONS

        #region CONSTRUCTING FAMILIES

        public static bool gene_centric_families = false;
        public static string preferred_gene_label;
        public List<ProteoformFamily> construct_families()
        {
            clean_up_td_relations();
            List<Proteoform> proteoforms = new List<Proteoform>();
            proteoforms.AddRange(this.experimental_proteoforms.Where(e => e.accepted).ToList());
            proteoforms.AddRange(topdown_proteoforms.Where(t => !t.targeted).ToList()); //want to include families with no E proteoforms, only topdown proteoforms. For now, only non-targeted topdown proteoforms
            Stack<Proteoform> remaining = new Stack<Proteoform>(proteoforms);
            List<ProteoformFamily> running_families = new List<ProteoformFamily>();
            List<Proteoform> running = new List<Proteoform>();
            List<Thread> active = new List<Thread>();
            while (remaining.Count > 0 || active.Count > 0)
            {
                while (remaining.Count > 0 && active.Count < Environment.ProcessorCount)
                {
                    Proteoform root = remaining.Pop();
                    ProteoformFamily fam = new ProteoformFamily(root);
                    Thread t = new Thread(new ThreadStart(fam.construct_family));
                    t.Start();
                    running_families.Add(fam);
                    running.Add(root);
                    active.Add(t);
                }

                foreach (Thread t in active)
                {
                    t.Join();
                }

                List<Proteoform> cumulative_proteoforms = new List<Proteoform>();
                foreach (ProteoformFamily family in running_families.ToList())
                {
                    if (cumulative_proteoforms.Contains(family.proteoforms.First()))
                    {
                        running_families.Remove(family); // check for duplicates due to arbitrary seed selection
                    }
                    else
                    {
                        cumulative_proteoforms.AddRange(family.proteoforms);
                        Parallel.ForEach(family.proteoforms, p => { lock (p) p.family = family; });
                    }
                }

                this.families.AddRange(running_families);
                remaining = new Stack<Proteoform>(remaining.Except(cumulative_proteoforms));

                running_families.Clear();
                running.Clear();
                active.Clear();
            }
            if (gene_centric_families) families = combine_gene_families(families).ToList();
            Parallel.ForEach(families, f => f.identify_experimentals());
            return families;
        }

        //if E in relation w/ T and TD of diff accesions, TD takes priority because has retention time evidence as well 
        public void clean_up_td_relations()
        {
            foreach (ExperimentalProteoform e in this.experimental_proteoforms.Where(e => e.accepted && e.relationships.Where(r =>
                r.Accepted && r.RelationType == ProteoformComparison.ExperimentalTheoretical).Count() >= 1 && e.relationships.Where(r => r.RelationType == ProteoformComparison.ExperimentalTopDown).Count() == 1))
            {
                string accession = e.relationships.Where(r => r.RelationType == ProteoformComparison.ExperimentalTopDown).First().connected_proteoforms[0].accession.Split('_')[0];
                foreach (ProteoformRelation relation in e.relationships.Where(r => r.RelationType == ProteoformComparison.ExperimentalTheoretical && r.connected_proteoforms[1].accession.Split('_')[0] != accession))
                {
                    relation.Accepted = false;
                }
            }
        }

        public IEnumerable<ProteoformFamily> combine_gene_families(IEnumerable<ProteoformFamily> families)
        {
            Stack<ProteoformFamily> remaining = new Stack<ProteoformFamily>(families);
            List<ProteoformFamily> running = new List<ProteoformFamily>();
            List<Proteoform> cumulative_proteoforms = new List<Proteoform>();
            List<Thread> active = new List<Thread>();
            while (remaining.Count > 0 || active.Count > 0)
            {
                while (remaining.Count > 0 && active.Count < Environment.ProcessorCount)
                {
                    ProteoformFamily fam = remaining.Pop();
                    Thread t = new Thread(() => fam.merge_families(this.families));
                    t.Start();
                    running.Add(fam);
                    active.Add(t);
                }

                foreach (Thread t in active)
                {
                    t.Join();
                }

                remaining = new Stack<ProteoformFamily>(remaining.Except(running));
                foreach (ProteoformFamily family in running)
                {
                    if (!cumulative_proteoforms.Contains(family.proteoforms.First()))
                    {
                        cumulative_proteoforms.AddRange(family.proteoforms);
                        Parallel.ForEach(family.proteoforms, p => { lock (p) p.family = family; });
                        yield return family;
                    }
                }

                running.Clear();
                active.Clear();
            }
        }

        #endregion CONSTRUCTING FAMILIES

        #region CLEAR FAMILIES

        public void clear_families()
        {
            families.Clear();
            foreach (Proteoform p in experimental_proteoforms)
            {
                p.family = null;
                p.ptm_set = new PtmSet(new List<Ptm>());
                p.linked_proteoform_references = null;
                p.gene_name = null;
            }
            foreach (Proteoform p in theoretical_proteoforms) p.family = null;
        }
        #endregion CLEAR FAMILIES
    }



}
