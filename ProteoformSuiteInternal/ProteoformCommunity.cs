using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace ProteoformSuiteInternal
{
    public class ProteoformCommunity
    {
        //Please do not list {get;set} for new fields, so they are properly recorded in save all AC161103
        public ExperimentalProteoform[] experimental_proteoforms = new ExperimentalProteoform[0];
        public TheoreticalProteoform[] theoretical_proteoforms = new TheoreticalProteoform[0];
        public TopDownProteoform[] topdown_proteoforms = new TopDownProteoform[0];

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
            ConcurrentBag<ProteoformRelation> relations = new ConcurrentBag<ProteoformRelation>(); // Note, this could be faster by adding to a temporary list, but the lock to an object outside the parallel loop is keeping the instanceId and maybe other fields from getting distorted.

            Parallel.ForEach(pfs1, pf1 =>
            {
                lock (pf1)
                    pf1.candidate_relatives = pfs2
                        .Where(pf2 => (!Lollipop.neucode_labeled || pf2.lysine_count == pf1.lysine_count)
                            && allowed_mass_difference(pf1.modified_mass, pf2.modified_mass, ProteoformComparison.et))
                        .ToList();
            });

            Parallel.ForEach(pfs1, pf1 => 
            {
                HashSet<string> pf1_prot_accessions = new HashSet<string>(pf1.candidate_relatives.OfType<TheoreticalProteoform>().Select(t => t.proteinList.FirstOrDefault().Accession + "_G" + t.proteinList.Count() + (t as TheoreticalProteoformGroup != null ? "_T" + ((TheoreticalProteoformGroup)t).accessionList.Count : "")));
                foreach (string accession in pf1_prot_accessions)
                {
                    List<Proteoform> candidate_pfs2_with_accession = pf1.candidate_relatives.OfType<TheoreticalProteoform>().Where(t => t.proteinList.FirstOrDefault().Accession + "_G" + t.proteinList.Count() + (t as TheoreticalProteoformGroup != null ? "_T" + ((TheoreticalProteoformGroup)t).accessionList.Count : "") == accession).ToList<Proteoform>();
                    candidate_pfs2_with_accession.Sort(Comparer<Proteoform>.Create((x, y) => Math.Abs(pf1.modified_mass - x.modified_mass).CompareTo(Math.Abs(pf1.modified_mass - y.modified_mass))));
                    Proteoform best_pf2 = candidate_pfs2_with_accession.First();
                    lock (best_pf2) lock (relations)
                        relations.Add(new ProteoformRelation(pf1, best_pf2, relation_type, pf1.modified_mass - best_pf2.modified_mass));
                }
            });

            return count_nearby_relations(relations.OrderBy(r => r.delta_mass).ToList());
        }


        public List<ProteoformRelation> relate_ee(ExperimentalProteoform[] pfs1, ExperimentalProteoform[] pfs2, ProteoformComparison relation_type)
        {
            Parallel.ForEach(new HashSet<ExperimentalProteoform>(pfs1.Concat(pfs2)), pf =>
            {
                lock (pf) pf.candidate_relatives = pfs2.Where(pf2 => allowed_ee_relation(pf, pf2)).ToList<Proteoform>();
            });

            List<ProteoformRelation> relations =
                (from pf1 in pfs1
                 from pf2 in pf1.candidate_relatives
                 select new ProteoformRelation(pf1, pf2, relation_type, pf1.modified_mass - pf2.modified_mass))
                 .OrderBy(r => r.delta_mass).ToList();

            return count_nearby_relations(relations);  //putative counts include no-mans land
        }

        public bool allowed_ee_relation(ExperimentalProteoform pf1, ExperimentalProteoform pf2)
        {
            return pf1.modified_mass >= pf2.modified_mass
                && pf1 != pf2
                && (!Lollipop.neucode_labeled || pf1.lysine_count == pf2.lysine_count)
                && allowed_mass_difference(pf1.modified_mass, pf2.modified_mass, ProteoformComparison.ee)
                && Math.Abs(pf1.agg_rt - pf2.agg_rt) < Lollipop.ee_max_RetentionTime_difference;

            //where ProteoformRelation.mass_difference_is_outside_no_mans_land(pf1.modified_mass - pf2.modified_mass)
            //putative counts include no-mans land, currently
        }

        public bool allowed_ef_relation(ExperimentalProteoform pf1, ExperimentalProteoform pf2)
        {
            return pf1.modified_mass >= pf2.modified_mass
            && pf1 != pf2
            && (!Lollipop.neucode_labeled || pf1.lysine_count != pf2.lysine_count)
            && (Lollipop.neucode_labeled || Math.Abs(pf1.agg_rt - pf2.agg_rt) > Lollipop.ee_max_RetentionTime_difference * 2)
            && allowed_mass_difference(pf1.modified_mass, pf2.modified_mass, ProteoformComparison.ef)
            && (!Lollipop.neucode_labeled || Math.Abs(pf1.agg_rt - pf2.agg_rt) < Lollipop.ee_max_RetentionTime_difference);
        }

        public bool allowed_mass_difference(double pf1_mass, double pf2_mass, ProteoformComparison comparison)
        {
            //foreach (double mass in comparison == ProteoformComparison.et || comparison == ProteoformComparison.ed ? Lollipop.notch_masses_et : Lollipop.notch_masses_ee
            if (comparison == ProteoformComparison.et || comparison == ProteoformComparison.ed)
            {
                if (Lollipop.notch_search_et)
                {
                    foreach (double mass in Lollipop.notch_masses_et)
                    {
                        if (pf1_mass - pf2_mass <= mass + Lollipop.peak_width_base_et
                        && pf1_mass - pf2_mass >= mass - Lollipop.peak_width_base_et) return true;
                    }
                    return false;
                }
                else
                    return (pf1_mass - pf2_mass <= Lollipop.et_high_mass_difference && pf1_mass - pf2_mass >= Lollipop.et_low_mass_difference);
            }
            else
            {
                if (Lollipop.notch_search_ee)
                {
                    foreach (double mass in Lollipop.notch_masses_ee)
                    {
                        if (pf1_mass - pf2_mass <= mass + Lollipop.peak_width_base_ee
                        && pf1_mass - pf2_mass >= mass - Lollipop.peak_width_base_ee) return true;
                    }
                    return false;
                }
                else
                    return (pf1_mass - pf2_mass <= Lollipop.ee_max_mass_difference);
            }
        }

        private static List<ProteoformRelation> count_nearby_relations(List<ProteoformRelation> all_ordered_relations)
        {
            List<int> ordered_relation_ids = all_ordered_relations.Select(r => r.instanceId).ToList();
            Parallel.ForEach<ProteoformRelation>(all_ordered_relations, relation => relation.set_nearby_group(all_ordered_relations, ordered_relation_ids));
            return all_ordered_relations;
        }

        public Dictionary<string, List<ProteoformRelation>> relate_ed()
        {
            //TODO: simply fy with ?: operator
            Dictionary<string, List<ProteoformRelation>> ed_relations = new Dictionary<string, List<ProteoformRelation>>();
            if (!Lollipop.limit_TD_BU_theoreticals)
            {
                Parallel.ForEach(decoy_proteoforms, decoys =>
                {
                    ed_relations[decoys.Key] = relate_et(experimental_proteoforms.Where(p => p.accepted).ToArray(), decoys.Value, ProteoformComparison.ed);
                });
            }
            else
            {
                Parallel.ForEach(decoy_proteoforms, decoys =>
                {
                    ed_relations[decoys.Key] = relate_et(experimental_proteoforms.Where(p => p.accepted).ToArray(), decoys.Value.Where(t => t.psm_count_BU > 0).ToArray(), ProteoformComparison.ed);
                });
            }
            return ed_relations;
        }

        public List<ProteoformRelation> relate_ef()
        {
            List<ProteoformRelation> ef_relations = new List<ProteoformRelation>();
            ExperimentalProteoform[] pfs1 = new List<ExperimentalProteoform>(this.experimental_proteoforms.Where(p => p.accepted)).ToArray();
            ExperimentalProteoform[] pfs2 = new List<ExperimentalProteoform>(this.experimental_proteoforms.Where(p => p.accepted)).ToArray();

            foreach (ExperimentalProteoform pf1 in pfs1)
            {
                List<ProteoformRelation> ef_relation_addition = pfs2
                        .Where(pf2 => allowed_ef_relation(pf1, pf2))
                        .Select(pf2 => new ProteoformRelation(pf1, pf2, ProteoformComparison.ef, pf1.modified_mass - pf2.modified_mass))
                        .ToList();
                ef_relations.AddRange(ef_relation_addition);
            }
            //take subset equal to # EE relations
            new Random().Shuffle(ef_relations.ToArray());
            ef_relations = ef_relations.Take(Lollipop.ee_relations.Count).ToList();
            ef_relations = ef_relations.OrderBy(r => r.delta_mass).ToList();
            count_nearby_relations(ef_relations);
            return ef_relations;
        }

        public List<ProteoformRelation> relate_td(List<ExperimentalProteoform> experimentals, List<TheoreticalProteoform> theoreticals, List<TopDownProteoform> topdowns)
        {
            List<ProteoformRelation> td_relations = new List<ProteoformRelation>();

            int max_missed_monoisotopics = Convert.ToInt32(Lollipop.missed_monos);
            List<int> missed_monoisotopics_range = Enumerable.Range(-max_missed_monoisotopics, max_missed_monoisotopics * 2 + 1).ToList();
            foreach (TopDownProteoform topdown in topdowns)
            {
                double mass = topdown.monoisotopic_mass;
                for (int i = 0; i < 2; i++) //look once w/ topdown observed mass. if no relation found, check w/ theoretical mass
                {
                    foreach (int m in missed_monoisotopics_range)
                    {
                        double shift = m * Lollipop.MONOISOTOPIC_UNIT_MASS;
                        double mass_tol = (mass + shift) / 1000000 * Convert.ToInt32(Lollipop.mass_tolerance);
                        double low = mass + shift - mass_tol;
                        double high = mass + shift + mass_tol;
                        List<ExperimentalProteoform> matching_e = experimentals.Where(ep => ep.modified_mass >= low && ep.modified_mass <= high
                        && (Math.Abs(ep.agg_rt - topdown.agg_rt) <= Convert.ToDouble(Lollipop.retention_time_tolerance))).ToList();
                        foreach (ExperimentalProteoform e in matching_e)
                        {
                            e.accepted = true;
                            ProteoformRelation td_relation = new ProteoformRelation(topdown, e, ProteoformComparison.etd, (e.modified_mass - mass));
                            td_relation.accepted = true;
                            td_relation.connected_proteoforms[0].relationships.Add(td_relation);
                            td_relation.connected_proteoforms[1].relationships.Add(td_relation);
                            td_relations.Add(td_relation);
                        }
                    }
                    if (topdown.relationships.Count == 0) mass = topdown.theoretical_mass;
                    else i = 3; //don't recheck
                }

                //match each td proteoform group to the closest theoretical w/ same accession and modifications. (if no match always make relationship with unmodified)
                if (theoreticals.Count > 0)
                {
                    TheoreticalProteoform theo = theoreticals.Where(t => t.accession.Split('_')[0] == topdown.accession.Split('_')[0] && topdown.same_ptms(t)).FirstOrDefault();
                    if (theo == null)
                    { 
                        if (topdown.ptm_list.Count == 0) continue; //if can't find match to unmodified topdown, nothing to do (not in database)
                                                                   //if modified topdown, compare with unmodified theoretical
                        else
                        {
                            try
                            {
                                theo = theoreticals.Where(t => t.accession.Split('_')[0] == topdown.accession.Split('_')[0] && t.ptm_set.ptm_combination.Count == 0).OrderBy(
                                t => Math.Abs(t.modified_mass - topdown.theoretical_mass)).First();
                            }
                            catch { continue; }
                        }
                    }
                    ProteoformRelation t_td_relation = new ProteoformRelation(topdown, theo, ProteoformComparison.ttd, (topdown.theoretical_mass - theo.modified_mass));
                    t_td_relation.accepted = true;
                    t_td_relation.connected_proteoforms[0].relationships.Add(t_td_relation);
                    t_td_relation.connected_proteoforms[1].relationships.Add(t_td_relation);
                    td_relations.Add(t_td_relation);
                }
            }
            return td_relations;
        }

        private bool matching_RT(List<double> rt1s, List<double> rt2s, double tolerance)
        {
            foreach(double rt1 in rt1s)
            {
                foreach(double rt2 in rt2s)
                {
                    if (Math.Abs(rt1 - rt2) <= tolerance) return true;
                }
            }
            return false;        
        }

        public List<ProteoformRelation> relate_targeted_td(List<ExperimentalProteoform> experimentals, List<TopDownProteoform> topdown_proteoforms)
        {
            List<ProteoformRelation> td_relations = new List<ProteoformRelation>();
            int max_missed_monoisotopics = Convert.ToInt32(Lollipop.missed_monos);
            List<int> missed_monoisotopics_range = Enumerable.Range(-max_missed_monoisotopics, max_missed_monoisotopics * 2 + 1).ToList();
            foreach (TopDownProteoform td_proteoform in topdown_proteoforms)
            {
                foreach (int m in missed_monoisotopics_range)
                {
                    double shift = m * Lollipop.MONOISOTOPIC_UNIT_MASS;
                    double mass_tol = (td_proteoform.modified_mass + shift) / 1000000 * Convert.ToInt32(Lollipop.mass_tolerance);
                    double low = td_proteoform.modified_mass + shift - mass_tol;
                    double high = td_proteoform.modified_mass + shift + mass_tol;
                    List<ExperimentalProteoform> matching_e = experimentals.Where(ep => ep.modified_mass >= low && ep.modified_mass <= high).ToList();
                    foreach (ExperimentalProteoform e in matching_e)
                    {
                        ProteoformRelation td_relation = new ProteoformRelation(td_proteoform, e, ProteoformComparison.ettd, (e.modified_mass - td_proteoform.modified_mass));
                        td_relation.accepted = true;
                        td_relation.connected_proteoforms[0].relationships.Add(td_relation);
                        td_relation.connected_proteoforms[1].relationships.Add(td_relation);
                        td_relations.Add(td_relation);
                    }
                }
            }
            return td_relations;
        }

        //GROUP and ANALYZE RELATIONS
        public List<ProteoformRelation> remaining_relations_outside_no_mans = new List<ProteoformRelation>();
        public List<DeltaMassPeak> accept_deltaMass_peaks(List<ProteoformRelation> relations, Dictionary<string, List<ProteoformRelation>> decoy_relations)
        {
            //order by E intensity, then by descending unadjusted_group_count (running sum) before forming peaks, and analyze only relations outside of no-man's-land
            this.remaining_relations_outside_no_mans = relations.Where(r => r.outside_no_mans_land).OrderByDescending(r => r.nearby_relations_count).ThenByDescending(r => r.agg_intensity_1).ToList(); // Group count is the primary sort
            List<DeltaMassPeak> peaks = new List<DeltaMassPeak>();

            ProteoformRelation root = remaining_relations_outside_no_mans.FirstOrDefault();
            List<ProteoformRelation> running = new List<ProteoformRelation>();
            List<Thread> active = new List<Thread>();
            while (remaining_relations_outside_no_mans.FirstOrDefault() != null || active.Count > 0)
            {
                while (root != null && active.Count < Environment.ProcessorCount)
                {
                    if (root.relation_type != ProteoformComparison.ee && root.relation_type != ProteoformComparison.et)
                        throw new ArgumentException("Only EE and ET peaks can be accepted");

                    Thread t = new Thread(new ThreadStart(root.generate_peak));
                    t.Start();
                    running.Add(root);
                    active.Add(t);
                    root = find_next_root(this.remaining_relations_outside_no_mans, running);
                }

                foreach (Thread t in active)
                {
                    t.Join();
                }

                foreach (DeltaMassPeak peak in running.Select(r => r.peak))
                {
                    peaks.Add(peak);
                    Parallel.ForEach<ProteoformRelation>(peak.grouped_relations, relation =>
                    {
                        lock (relation)
                        {
                            relation.peak = peak;
                            relation.accepted = peak.peak_accepted;
                        }
                    });
                }

                List<ProteoformRelation> mass_differences_in_peaks = running.SelectMany(r => r.peak.grouped_relations).ToList();
                relations_in_peaks.AddRange(mass_differences_in_peaks);
                this.remaining_relations_outside_no_mans = this.remaining_relations_outside_no_mans.Except(mass_differences_in_peaks).ToList();
                
                running.Clear();
                active.Clear();
                root = find_next_root(this.remaining_relations_outside_no_mans, running);
            }
            delta_mass_peaks.AddRange(peaks);
            return peaks;
        }

        public static ProteoformRelation find_next_root(IEnumerable<ProteoformRelation> ordered, IEnumerable<ProteoformRelation> running)
        {
            return ordered.FirstOrDefault(r =>
                running.All(s =>
                    r.delta_mass < s.delta_mass - 10 || r.delta_mass > s.delta_mass + 10));
        }

        public List<DeltaMassPeak> accept_deltaMass_peaks(List<ProteoformRelation> relations, List<ProteoformRelation> false_relations)
        {
            return accept_deltaMass_peaks(relations, new Dictionary<string, List<ProteoformRelation>> { { "", false_relations } });
        }


        //CONSTRUCTING FAMILIES
        public static bool gene_centric_families = false;
        public static string preferred_gene_label;
        public List<ProteoformFamily> construct_families()
        {
            clean_up_td_relations();
            List<Proteoform> proteoforms = new List<Proteoform>();
            proteoforms.AddRange(this.experimental_proteoforms.Where(e => e.accepted).ToList());
            proteoforms.AddRange(topdown_proteoforms);
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
                r.accepted && r.relation_type == ProteoformComparison.et).Count() >= 1 && e.relationships.Where(r => r.relation_type == ProteoformComparison.etd).Count() == 1))
            {
                string accession = e.relationships.Where(r => r.relation_type == ProteoformComparison.etd).First().accession_1.Split('_')[0];
                foreach (ProteoformRelation relation in e.relationships.Where(r => r.relation_type == ProteoformComparison.et && r.accession_2.Split('_')[0] != accession))
                {
                    relation.accepted = false;
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
                    Thread t = new Thread(new ThreadStart(fam.merge_families));
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
    }
}