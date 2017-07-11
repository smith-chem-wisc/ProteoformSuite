using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class ProteoformCommunity
    {

        #region Public Fields

        public ExperimentalProteoform[] experimental_proteoforms = new ExperimentalProteoform[0];
        public TheoreticalProteoform[] theoretical_proteoforms = new TheoreticalProteoform[0];
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
                 select new ProteoformRelation(pf1, pf2, relation_type, pf1.modified_mass - pf2.modified_mass, Environment.CurrentDirectory)).ToList();

            return count_nearby_relations(relations);  //putative counts include no-mans land
        }

        public bool allowed_relation(Proteoform pf1, Proteoform pf2, ProteoformComparison relation_type)
        {
            switch (relation_type)
            {
                case (ProteoformComparison.ExperimentalTheoretical):
                case (ProteoformComparison.ExperimentalDecoy):
                    return (!Sweet.lollipop.neucode_labeled || pf2.lysine_count == pf1.lysine_count)
                        && (pf1.modified_mass - pf2.modified_mass) >= Sweet.lollipop.et_low_mass_difference
                        && (pf1.modified_mass - pf2.modified_mass) <= Sweet.lollipop.et_high_mass_difference
                        && (pf2.ptm_set.ptm_combination.Count < 3 || pf2.ptm_set.ptm_combination.Select(ptm => ptm.modification.monoisotopicMass).All(x => x == pf2.ptm_set.ptm_combination.First().modification.monoisotopicMass));

                case (ProteoformComparison.ExperimentalExperimental):
                    return pf1.modified_mass >= pf2.modified_mass
                        && pf1 != pf2
                        && (!Sweet.lollipop.neucode_labeled || pf1.lysine_count == pf2.lysine_count)
                        && pf1.modified_mass - pf2.modified_mass <= Sweet.lollipop.ee_max_mass_difference
                        && Math.Abs(((ExperimentalProteoform)pf1).agg_rt - ((ExperimentalProteoform)pf2).agg_rt) <= Sweet.lollipop.ee_max_RetentionTime_difference;

                case (ProteoformComparison.ExperimentalFalse):
                    return pf1.modified_mass >= pf2.modified_mass
                        && pf1 != pf2
                        && (pf1.modified_mass - pf2.modified_mass <= Sweet.lollipop.ee_max_mass_difference)
                        && (!Sweet.lollipop.neucode_labeled || Math.Abs(pf1.lysine_count - pf2.lysine_count) > Sweet.lollipop.missed_lysines)
                        && (Sweet.lollipop.neucode_labeled || Math.Abs(((ExperimentalProteoform)pf1).agg_rt - ((ExperimentalProteoform)pf2).agg_rt) > Sweet.lollipop.ee_max_RetentionTime_difference * 2)
                        && (!Sweet.lollipop.neucode_labeled || Math.Abs(((ExperimentalProteoform)pf1).agg_rt - ((ExperimentalProteoform)pf2).agg_rt) < Sweet.lollipop.ee_max_RetentionTime_difference);

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

        public List<ProteoformRelation> relate_ef(ExperimentalProteoform[] pfs1, ExperimentalProteoform[] pfs2)
        {
            List<ProteoformRelation> all_ef_relations = relate(pfs1, pfs2, ProteoformComparison.ExperimentalFalse, true, Environment.CurrentDirectory, true);
            ProteoformRelation[] to_shuffle = new List<ProteoformRelation>(all_ef_relations).ToArray();
            to_shuffle.Shuffle();
            return to_shuffle.Take(Sweet.lollipop.ee_relations.Count).ToList();
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

            if (peaks.Count > 0 && peaks.First().RelationType == ProteoformComparison.ExperimentalTheoretical)
                Sweet.lollipop.et_peaks.AddRange(peaks);
            else
                Sweet.lollipop.ee_peaks.AddRange(peaks);

            Sweet.update_peaks_from_presets(); // accept or unaccept peaks noted in presets

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
            Stack<Proteoform> remaining = new Stack<Proteoform>(this.experimental_proteoforms.Where(e => e.accepted).ToArray());
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
