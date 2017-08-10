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
        public int community_number; //-100 for target, decoy database number for decoys
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

        public List<ProteoformRelation> relate(ExperimentalProteoform[] pfs1, Proteoform[] pfs2, ProteoformComparison relation_type, bool accepted_only, string current_directory, bool limit_et_relations)
        {
            if (accepted_only)
                pfs1 = pfs1.Where(pf1 => pf1.accepted).ToArray();

            if (accepted_only && (relation_type == ProteoformComparison.ExperimentalExperimental || relation_type == ProteoformComparison.ExperimentalFalse))
                pfs2 = pfs2.OfType<ExperimentalProteoform>().Where(pf2 => pf2.accepted).ToArray();

            Dictionary<int, List<Proteoform>> pfs2_lysine_lookup = new Dictionary<int, List<Proteoform>>();
            if (Sweet.lollipop.neucode_labeled)
            {
                foreach (Proteoform pf2 in pfs2)
                {
                    if (!pfs2_lysine_lookup.TryGetValue(pf2.lysine_count, out List<Proteoform> same_lysine_ct)) pfs2_lysine_lookup.Add(pf2.lysine_count, new List<Proteoform> { pf2 });
                    else same_lysine_ct.Add(pf2);
                }
            }

            Parallel.ForEach(pfs1, pf1 =>
            {
                lock (pf1)
                {
                    if (Sweet.lollipop.neucode_labeled && (relation_type == ProteoformComparison.ExperimentalTheoretical || relation_type == ProteoformComparison.ExperimentalDecoy || relation_type == ProteoformComparison.ExperimentalExperimental))
                    {
                        pfs2_lysine_lookup.TryGetValue(pf1.lysine_count, out List<Proteoform> pfs2_same_lysine_count);
                        pf1.candidate_relatives = pfs2_same_lysine_count != null ? pfs2_same_lysine_count.Where(pf2 => allowed_relation(pf1, pf2, relation_type)).ToList() : new List<Proteoform>();
                    }

                    else if (Sweet.lollipop.neucode_labeled && relation_type == ProteoformComparison.ExperimentalFalse)
                    {
                        List<Proteoform> pfs2_lysines_outside_tolerance = pfs2_lysine_lookup.Where(kv => Math.Abs(pf1.lysine_count - kv.Key) > Sweet.lollipop.maximum_missed_lysines).SelectMany(kv => kv.Value).ToList();
                        pf1.candidate_relatives = pfs2_lysines_outside_tolerance.Where(pf2 => allowed_relation(pf1, pf2, relation_type)).ToList();
                    }

                    else if (!Sweet.lollipop.neucode_labeled)
                    {
                        pf1.candidate_relatives = pfs2.Where(pf2 => allowed_relation(pf1, pf2, relation_type)).ToList();
                    }

                    if (relation_type == ProteoformComparison.ExperimentalExperimental)
                    {
                        pf1.ptm_set = null;
                        pf1.linked_proteoform_references = null;
                        pf1.gene_name = null;
                    }

                    if (limit_et_relations && (relation_type == ProteoformComparison.ExperimentalTheoretical || relation_type == ProteoformComparison.ExperimentalDecoy))
                    {
                        ProteoformRelation best_relation = pf1.candidate_relatives
                            .Select(pf2 => new ProteoformRelation(pf1, pf2, relation_type, pf1.modified_mass - pf2.modified_mass, current_directory))
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
                 select new ProteoformRelation(pf1, pf2, relation_type, pf1.modified_mass - pf2.modified_mass, current_directory)).ToList();

            return count_nearby_relations(relations);  //putative counts include no-mans land
        }

        public bool allowed_relation(Proteoform pf1, Proteoform pf2_with_allowed_lysines, ProteoformComparison relation_type)
        {
            if (relation_type == ProteoformComparison.ExperimentalTheoretical || relation_type == ProteoformComparison.ExperimentalDecoy)
                return 
                    (pf1.modified_mass - pf2_with_allowed_lysines.modified_mass) >= Sweet.lollipop.et_low_mass_difference
                    && (pf1.modified_mass - pf2_with_allowed_lysines.modified_mass) <= Sweet.lollipop.et_high_mass_difference
                    && !(pf2_with_allowed_lysines as TheoreticalProteoform).topdown_theoretical
                    && (pf2_with_allowed_lysines.ptm_set.ptm_combination.Count < 3 || pf2_with_allowed_lysines.ptm_set.ptm_combination.Select(ptm => ptm.modification.monoisotopicMass).All(x => x == pf2_with_allowed_lysines.ptm_set.ptm_combination.First().modification.monoisotopicMass));

            else if (relation_type == ProteoformComparison.ExperimentalExperimental)
                return 
                    pf1 != pf2_with_allowed_lysines
                    && pf1.modified_mass >= pf2_with_allowed_lysines.modified_mass
                    && pf1 != pf2_with_allowed_lysines
                    && pf1.modified_mass - pf2_with_allowed_lysines.modified_mass <= Sweet.lollipop.ee_max_mass_difference
                    && Math.Abs((pf1 as ExperimentalProteoform).agg_rt - (pf2_with_allowed_lysines as ExperimentalProteoform).agg_rt) <= Sweet.lollipop.ee_max_RetentionTime_difference;

            else if (relation_type == ProteoformComparison.ExperimentalFalse) 
                return 
                    pf1.modified_mass >= pf2_with_allowed_lysines.modified_mass
                    && pf1 != pf2_with_allowed_lysines
                    && (pf1.modified_mass - pf2_with_allowed_lysines.modified_mass <= Sweet.lollipop.ee_max_mass_difference)
                    && (Sweet.lollipop.neucode_labeled || Math.Abs((pf1 as ExperimentalProteoform).agg_rt - (pf2_with_allowed_lysines as ExperimentalProteoform).agg_rt) > Sweet.lollipop.ee_max_RetentionTime_difference * 2)
                    && (!Sweet.lollipop.neucode_labeled || Math.Abs((pf1 as ExperimentalProteoform).agg_rt - (pf2_with_allowed_lysines as ExperimentalProteoform).agg_rt) < Sweet.lollipop.ee_max_RetentionTime_difference);

            else
                return false;
        }

        public static List<ProteoformRelation> count_nearby_relations(List<ProteoformRelation> all_relations)
        {
            List<ProteoformRelation> all_ordered_relations = all_relations.OrderBy(x => x.DeltaMass).ToList();
            List<int> ordered_relation_ids = all_ordered_relations.Select(r => r.InstanceId).ToList();
            Parallel.ForEach(all_ordered_relations, relation => relation.set_nearby_group(all_ordered_relations, ordered_relation_ids));
            return all_ordered_relations;
        }

        public List<ProteoformRelation> relate_ef(ExperimentalProteoform[] pfs1, ExperimentalProteoform[] pfs2)
        {
            List<ProteoformRelation> all_ef_relations = relate(pfs1, pfs2, ProteoformComparison.ExperimentalFalse, true, Environment.CurrentDirectory, true);
            ProteoformRelation[] to_shuffle = new List<ProteoformRelation>(all_ef_relations).ToArray();
            to_shuffle.Shuffle();
            return to_shuffle.Take(Sweet.lollipop.ee_relations.Count).ToList();
        }

        public List<ProteoformRelation> relate_td()
        {
             List<ProteoformRelation> td_relations = new List<ProteoformRelation>();
            int max_missed_monoisotopics = Convert.ToInt32(Sweet.lollipop.maximum_missed_monos);
            List<int> missed_monoisotopics_range = Enumerable.Range(-max_missed_monoisotopics, max_missed_monoisotopics * 2 + 1).ToList();
            foreach (TopDownProteoform topdown in topdown_proteoforms)
            {
                //match each td proteoform group to the closest theoretical w/ best explanation.... otherwise make new theoretical proteoform
                List<TheoreticalProteoform> candidate_theoreticals;
                lock (Sweet.lollipop.theoretical_database.theoreticals_by_accession)
                {
                    Sweet.lollipop.theoretical_database.theoreticals_by_accession[community_number].TryGetValue(topdown.accession.Split('_')[0], out candidate_theoreticals);
                }
                if (candidate_theoreticals != null)
                {
                    List<ProteoformRelation> possible_ttd_relations = candidate_theoreticals.Select(t => new ProteoformRelation(topdown, t, ProteoformComparison.TopdownTheoretical, topdown.theoretical_mass - t.modified_mass, Environment.CurrentDirectory)).ToList();
                    ProteoformRelation best_ttd_relation;
                    foreach (ProteoformRelation relation in possible_ttd_relations)
                    {
                        if (Sweet.lollipop.theoretical_database.possible_ptmset_dictionary.TryGetValue(Math.Round(relation.DeltaMass, 1), out List<PtmSet> candidate_sets))
                        {
                            double mass_tolerance = topdown.theoretical_mass / 1000000 * (double)Sweet.lollipop.mass_tolerance;
                            relation.candidate_ptmset = topdown.generate_possible_added_ptmsets(candidate_sets.Where(s => Math.Abs(s.mass - relation.DeltaMass) < 0.05).ToList(), relation.DeltaMass, mass_tolerance, Sweet.lollipop.theoretical_database.all_mods_with_mass, relation.connected_proteoforms[1] as TheoreticalProteoform, Sweet.lollipop.mod_rank_first_quartile)
                            .OrderBy(x => (double)x.ptm_rank_sum + Math.Abs(Math.Abs(x.mass) - Math.Abs(relation.DeltaMass)) * 10E-6) // major score: delta rank; tie breaker: deltaM, where it's always less than 1
                            .FirstOrDefault();
                        }
                    }
                    best_ttd_relation = possible_ttd_relations.Where(r => r.candidate_ptmset != null).OrderBy(r => r.candidate_ptmset.ptm_rank_sum).FirstOrDefault();

                    if (best_ttd_relation == null)
                    {
                        TheoreticalProteoform t = candidate_theoreticals.Where(r => r.ptm_set.ptm_combination.Count == 0 && r.fragment == "full" || r.fragment == "full-met-cleaved").FirstOrDefault();
                        if (t == null) t = candidate_theoreticals.Where(r => r.ptm_set.ptm_combination.Count == 0).FirstOrDefault();
                        if (t != null)
                        {
                            string[] a = topdown.accession.Split('_');
                            int count = candidate_theoreticals.Where(c => c.topdown_theoretical).Count(c => c.accession.Split('_')[1] == a[4]) + 1;
                            TheoreticalProteoform new_t = new TheoreticalProteoform(a[0] + "_" + a[4] + "_" + count + "TD", t.description, t.ExpandedProteinList, (topdown.theoretical_mass - topdown.ptm_set.mass), topdown.lysine_count, topdown.ptm_set, t.is_target, false, null);
                            new_t.contaminant = false;
                            new_t.psm_list = t.psm_list;
                            new_t.sequence = topdown.sequence;
                            new_t.begin = topdown.begin;
                            new_t.end = topdown.end;
                            new_t.topdown_theoretical = true;
                            ProteoformRelation relation = new ProteoformRelation(topdown, new_t, ProteoformComparison.TopdownTheoretical, topdown.theoretical_mass - new_t.modified_mass, Environment.CurrentDirectory);
                            if (Sweet.lollipop.theoretical_database.possible_ptmset_dictionary.TryGetValue(Math.Round(relation.DeltaMass, 1), out List<PtmSet> candidate_sets))
                            {
                                double mass_tolerance = topdown.theoretical_mass / 1000000 * (double)Sweet.lollipop.mass_tolerance;
                                relation.candidate_ptmset = topdown.generate_possible_added_ptmsets(candidate_sets.Where(s => Math.Abs(s.mass - relation.DeltaMass) < 0.05).ToList(), relation.DeltaMass, mass_tolerance, Sweet.lollipop.theoretical_database.all_mods_with_mass, relation.connected_proteoforms[1] as TheoreticalProteoform, Sweet.lollipop.mod_rank_first_quartile)
                                .OrderBy(x => (double)x.ptm_rank_sum + Math.Abs(Math.Abs(x.mass) - Math.Abs(relation.DeltaMass)) * 10E-6) // major score: delta rank; tie breaker: deltaM, where it's always less than 1
                                .FirstOrDefault();
                                if (relation.candidate_ptmset != null)
                                {
                                    theoretical_proteoforms = theoretical_proteoforms.Concat(new List<TheoreticalProteoform> { new_t }).ToArray();
                                    Sweet.lollipop.theoretical_database.theoreticals_by_accession[community_number][new_t.accession.Split('_')[0]].Add(new_t);
                                    best_ttd_relation = relation;
                                }
                            }
                        }
                    }
                    if (best_ttd_relation != null)
                    {
                        best_ttd_relation.connected_proteoforms[0].relationships.Add(best_ttd_relation);
                        best_ttd_relation.connected_proteoforms[1].relationships.Add(best_ttd_relation);
                        best_ttd_relation.Accepted = true;
                        td_relations.Add(best_ttd_relation);
                    }
                    else
                    //shows Warning message in TopDown GUI if no TTD relations for the topdown proteoform. 
                    {
                        continue;
                    }
                }
                else continue;

                double mass = topdown.modified_mass;
                List<ProteoformRelation> all_td_relations = new List<ProteoformRelation>();
                foreach (int m in missed_monoisotopics_range)
                {
                    double shift = m * Lollipop.MONOISOTOPIC_UNIT_MASS;
                    double mass_tol = (mass + shift) / 1000000 * Convert.ToInt32(Sweet.lollipop.mass_tolerance);
                    double low = mass + shift - mass_tol;
                    double high = mass + shift + mass_tol;
                    List<ExperimentalProteoform> matching_e = experimental_proteoforms.Where(ep => ep.accepted && ep.modified_mass >= low && ep.modified_mass <= high
                        && Math.Abs(ep.agg_rt - topdown.agg_RT) <= Convert.ToDouble(Sweet.lollipop.retention_time_tolerance)).ToList();
                    foreach (ExperimentalProteoform e in matching_e)
                    {
                        all_td_relations.Add(new ProteoformRelation(topdown, e, ProteoformComparison.TopdownExperimental, (e.modified_mass - mass), Environment.CurrentDirectory));
                    }
                }
                //add the best td relation
                foreach (ProteoformRelation relation in all_td_relations)
                {
                    if (Sweet.lollipop.theoretical_database.possible_ptmset_dictionary.TryGetValue(Math.Round(relation.DeltaMass, 1), out List<PtmSet> candidate_sets))
                    {
                        //only candidate relations of unmodified or missed monoisotopic(s)
                        double mass_tolerance = topdown.modified_mass / 1000000 * (double)Sweet.lollipop.mass_tolerance;
                        relation.candidate_ptmset = topdown.generate_possible_added_ptmsets(candidate_sets.Where(s => s.mass == 0 || s.ptm_combination.Count(p => p.modification.modificationType != "Deconvolution Error") == 0).ToList(), relation.DeltaMass, mass_tolerance, Sweet.lollipop.theoretical_database.all_mods_with_mass, topdown.relationships.First().connected_proteoforms[1] as TheoreticalProteoform, Sweet.lollipop.mod_rank_first_quartile)
                        .OrderBy(x => (double)x.ptm_rank_sum + Math.Abs(Math.Abs(x.mass) - Math.Abs(relation.DeltaMass)) * 10E-6) // major score: delta rank; tie breaker: deltaM, where it's always less than 1
                        .FirstOrDefault();
                    }
                }

                ProteoformRelation best_relation = all_td_relations.Where(r => r.candidate_ptmset != null).OrderBy(r => r.candidate_ptmset.ptm_rank_sum + Math.Abs(Math.Abs(r.candidate_ptmset.mass) - Math.Abs(r.DeltaMass)) * 10E-6).FirstOrDefault();

                if (best_relation != null)
                {
                    best_relation.connected_proteoforms[0].relationships.Add(best_relation);
                    best_relation.connected_proteoforms[1].relationships.Add(best_relation);
                    best_relation.Accepted = true;
                    td_relations.Add(best_relation);
                    //if experimental is connected to a topdown with fewer missed monoisotopics, go with this one
                    List<ProteoformRelation> relationsToRemove = new List<ProteoformRelation>();
                    foreach (ProteoformRelation relation in best_relation.connected_proteoforms[1].relationships.Where(r => r.RelationType == ProteoformComparison.TopdownExperimental))
                    {
                        if (relation.candidate_ptmset.ptm_combination.Count(m => m.modification.modificationType == "Deconvolution Error") < best_relation.candidate_ptmset.ptm_combination.Count(m => m.modification.modificationType == "Deconvolution Error"))
                        {
                            relationsToRemove.Add(best_relation);
                            break;
                        }
                        if (relation.candidate_ptmset.ptm_combination.Count(m => m.modification.modificationType == "Deconvolution Error") > best_relation.candidate_ptmset.ptm_combination.Count(m => m.modification.modificationType == "Deconvolution Error"))
                        {
                            relationsToRemove.Add(relation);
                        }
                    }
                    foreach(ProteoformRelation relation in relationsToRemove)
                    {
                        td_relations.Remove(relation);
                        relation.connected_proteoforms[0].relationships.Remove(relation);
                        relation.connected_proteoforms[1].relationships.Remove(relation);
                    }
                }
            }
            return td_relations;
        }

        #endregion BUILDING RELATIONSHIPS

        #region GROUP and ANALYZE RELATIONS

        public HashSet<ProteoformRelation> remaining_relations_outside_no_mans = new HashSet<ProteoformRelation>();
        public List<DeltaMassPeak> accept_deltaMass_peaks(List<ProteoformRelation> relations, Dictionary<string, List<ProteoformRelation>> decoy_relations)
        {
            //order by E intensity, then by descending unadjusted_group_count (running sum) before forming peaks, and analyze only relations outside of no-man's-land
            remaining_relations_outside_no_mans = new HashSet<ProteoformRelation>(relations.Where(r => r.outside_no_mans_land).OrderByDescending(r => r.nearby_relations_count).ThenByDescending(r => ((ExperimentalProteoform)r.connected_proteoforms[0]).agg_intensity)); // Group count is the primary sort
            List<DeltaMassPeak> peaks = new List<DeltaMassPeak>();

            ProteoformRelation root = remaining_relations_outside_no_mans.FirstOrDefault();
            List<ProteoformRelation> running = new List<ProteoformRelation>();
            List<Thread> active = new List<Thread>();
            while (remaining_relations_outside_no_mans.FirstOrDefault() != null || active.Count > 0)
            {
                while (root != null && active.Count < Environment.ProcessorCount) // Use Environment.ProcessorCount instead of 1 to enable parallization
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
                foreach (ProteoformRelation r in mass_differences_in_peaks)
                {
                    remaining_relations_outside_no_mans.Remove(r);
                }

                running.Clear();
                active.Clear();
                root = find_next_root(remaining_relations_outside_no_mans, running);
            }

            if (peaks.Count > 0 && peaks.First().RelationType == ProteoformComparison.ExperimentalTheoretical)
                Sweet.lollipop.et_peaks.AddRange(peaks);
            else
                Sweet.lollipop.ee_peaks.AddRange(peaks);

            Sweet.update_peaks_from_presets(); // accept or unaccept peaks noted in presets
            Sweet.mass_shifts_from_presets(); //shift peaks

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
        public static bool include_td_nodes = true;
        public static string preferred_gene_label;
        public List<ProteoformFamily> construct_families()
        {
            ProteoformFamily.reset_family_counter();
            Parallel.ForEach(Sweet.lollipop.td_relations, t =>
            {
                t.Accepted = include_td_nodes;
            });

            List<Proteoform> proteoforms = new List<Proteoform>();
            proteoforms.AddRange(this.experimental_proteoforms.Where(e => e.accepted).ToList());
            if (include_td_nodes) proteoforms.AddRange(topdown_proteoforms); //want to include families with no E proteoforms, only topdown proteoforms. For now, only non-targeted topdown proteoforms
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

        public IEnumerable<ProteoformFamily> combine_gene_families(IEnumerable<ProteoformFamily> families)
        {
            Stack<ProteoformFamily> remaining = new Stack<ProteoformFamily>(families);
            List<ProteoformFamily> running = new List<ProteoformFamily>();
            HashSet<Proteoform> cumulative_proteoforms = new HashSet<Proteoform>();
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

                foreach (ProteoformFamily family in running)
                {
                    if (!family.proteoforms.Any(p => cumulative_proteoforms.Contains(p)))
                    {
                        foreach (Proteoform p in family.proteoforms)
                        {
                            cumulative_proteoforms.Add(p);
                        }
                        Parallel.ForEach(family.proteoforms, p => { lock (p) p.family = family; });
                        yield return family;
                    }
                }
                remaining = new Stack<ProteoformFamily>(remaining.Except(remaining.Where(f => f.proteoforms.Any(p => cumulative_proteoforms.Contains(p)))));

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
            foreach (Proteoform p in topdown_proteoforms)
            {
                p.family = null;
                p.linked_proteoform_references = null;
                p.gene_name = null;
            }
            foreach (Proteoform p in theoretical_proteoforms) p.family = null;
        }
        #endregion CLEAR FAMILIES

    }
}
