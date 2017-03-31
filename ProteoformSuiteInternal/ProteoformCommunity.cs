using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            ConcurrentBag<ProteoformRelation> relations = new ConcurrentBag<ProteoformRelation>(); // Note, this could be faster by adding to a temporary list, but the lock to an object outside the parallel loop is keeping the instanceId and maybe other fields from getting distorted.

            Parallel.ForEach(pfs1, pf1 =>
            {
                lock (pf1)
                    pf1.candidate_relatives = pfs2
                        .Where(pf2 => (!Lollipop.neucode_labeled || pf2.lysine_count == pf1.lysine_count)
                            && (pf1.modified_mass - pf2.modified_mass) >= Lollipop.et_low_mass_difference
                            && (pf1.modified_mass - pf2.modified_mass) <= Lollipop.et_high_mass_difference)
                        .ToList();
            });

            Parallel.ForEach(pfs1, pf1 =>
            {
                double mass_tolerance = pf1.modified_mass / 1000000 * (double)Lollipop.mass_tolerance;
                HashSet<string> pf1_prot_accessions = new HashSet<string>(pf1.candidate_relatives.OfType<TheoreticalProteoform>().Select(t => t.ExpandedProteinList.FirstOrDefault().Accession + "_G" + t.ExpandedProteinList.Count() + (t as TheoreticalProteoformGroup != null ? "_T" + ((TheoreticalProteoformGroup)t).accessionList.Count : "")));
                foreach (string accession in pf1_prot_accessions)
                {
                    List<Proteoform> candidate_pfs2_with_accession = pf1.candidate_relatives.OfType<TheoreticalProteoform>().Where(t => t.ExpandedProteinList.FirstOrDefault().Accession + "_G" + t.ExpandedProteinList.Count() + (t as TheoreticalProteoformGroup != null ? "_T" + ((TheoreticalProteoformGroup)t).accessionList.Count : "") == accession).ToList<Proteoform>();
                    //candidate_pfs2_with_accession.Sort(Comparer<Proteoform>.Create((x, y) => Math.Abs(pf1.modified_mass - x.modified_mass).CompareTo(Math.Abs(pf1.modified_mass - y.modified_mass))));
                    //Proteoform closest_pf2 = candidate_pfs2_with_accession.First();
                    //int best_pf2_ranksum = candidate_pfs2_with_accession
                    //    .OrderBy(pf => pf.ptm_set_rank_sum)
                    //    .FirstOrDefault(pf => Math.Abs(closest_pf2.modified_mass - pf.modified_mass) <= mass_tolerance)
                    //    .ptm_set_rank_sum;
                    Proteoform best_pf2 = candidate_pfs2_with_accession.OrderBy(x => (double)x.ptm_set_rank_sum * Math.Abs(x.modified_mass - pf1.modified_mass)).FirstOrDefault(); // major score: delta rank; tie breaker: mass difference divided by constant, so less than one
                    //Proteoform best_pf2 = candidate_pfs2_with_accession.FirstOrDefault(x => x.ptm_set_rank_sum == best_pf2_ranksum);

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
                lock (pf)
                {
                    pf.candidate_relatives = pfs2.Where(pf2 => allowed_ee_relation(pf, pf2)).ToList<Proteoform>();
                    pf.ptm_set = null;
                    pf.linked_proteoform_references = null;
                    pf.gene_name = null;
                }
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
                && pf1.modified_mass - pf2.modified_mass <= Lollipop.ee_max_mass_difference
                && Math.Abs(pf1.agg_rt - pf2.agg_rt) <= Lollipop.ee_max_RetentionTime_difference;
            //where ProteoformRelation.mass_difference_is_outside_no_mans_land(pf1.modified_mass - pf2.modified_mass)
            //putative counts include no-mans land, currently
        }

        private static List<ProteoformRelation> count_nearby_relations(List<ProteoformRelation> all_ordered_relations)
        {
            List<int> ordered_relation_ids = all_ordered_relations.Select(r => r.instanceId).ToList();
            Parallel.ForEach<ProteoformRelation>(all_ordered_relations, relation => relation.set_nearby_group(all_ordered_relations, ordered_relation_ids));
            return all_ordered_relations;
        }

        public Dictionary<string, List<ProteoformRelation>> relate_ed()
        {
            Dictionary<string, List<ProteoformRelation>> ed_relations = new Dictionary<string, List<ProteoformRelation>>();
            Parallel.ForEach(decoy_proteoforms, decoys =>
            {
                ed_relations[decoys.Key] = relate_et(experimental_proteoforms.Where(e => e.accepted).ToArray(), decoys.Value, ProteoformComparison.ed);
            });
            return ed_relations;
        }

        public Dictionary<string, List<ProteoformRelation>> relate_ef()
        {
            List<ProteoformRelation> all_ef_relations = new List<ProteoformRelation>();
            Dictionary<string, List<ProteoformRelation>> ef_relations = new Dictionary<string, List<ProteoformRelation>>();
            ExperimentalProteoform[] pfs1 = new List<ExperimentalProteoform>(this.experimental_proteoforms.Where(e => e.accepted)).ToArray();
            ExperimentalProteoform[] pfs2 = new List<ExperimentalProteoform>(this.experimental_proteoforms.Where(e => e.accepted)).ToArray();
            foreach (ExperimentalProteoform pf1 in pfs1)
            {
                List<ProteoformRelation> ef_relation_addition = pfs2
                        .Where(pf2 => allowed_ef_relation(pf1, pf2))
                        .Select(pf2 => new ProteoformRelation(pf1, pf2, ProteoformComparison.ef, pf1.modified_mass - pf2.modified_mass))
                        .OrderBy(r => r.delta_mass)
                        .ToList();
                all_ef_relations.AddRange(ef_relation_addition);
            }
            //take 10 random subsets from all allowed ef relations (will use median for fdr calculations of each peak)
            for (int i = 0; i < 10; i++)
            {
                string key = "EF_relations_" + i;
                all_ef_relations.Shuffle();
                ef_relations.Add(key, all_ef_relations.Take(Lollipop.ee_relations.Count).ToList());
            }
            count_nearby_relations(ef_relations.Values.First().OrderBy(r => r.delta_mass).ToList()); //count from first decoy database
            return ef_relations;
        }

        public bool allowed_ef_relation(ExperimentalProteoform pf1, ExperimentalProteoform pf2)
        {
            return pf1.modified_mass >= pf2.modified_mass
            && pf1 != pf2
            && (pf1.modified_mass - pf2.modified_mass <= Lollipop.ee_max_mass_difference)
            && (!Lollipop.neucode_labeled || pf1.lysine_count != pf2.lysine_count)
            && (Lollipop.neucode_labeled || Math.Abs(pf1.agg_rt - pf2.agg_rt) > Lollipop.ee_max_RetentionTime_difference * 2)
            && (!Lollipop.neucode_labeled || Math.Abs(pf1.agg_rt - pf2.agg_rt) < Lollipop.ee_max_RetentionTime_difference);
        }


        //GROUP and ANALYZE RELATIONS
        public static List<PtmSet> all_possible_ptmsets;
        public List<ProteoformRelation> remaining_relations_outside_no_mans = new List<ProteoformRelation>();
        public List<DeltaMassPeak> accept_deltaMass_peaks(List<ProteoformRelation> relations, Dictionary<string, List<ProteoformRelation>> decoy_relations)
        {
            if (all_possible_ptmsets == null)
            {
                //Generate all two-member sets and all three-member sets of the same modification (three-member combinitorics gets out of hand for assignment)
                all_possible_ptmsets = PtmCombos.generate_all_ptmsets(2, Lollipop.all_mods_with_mass);
                all_possible_ptmsets.AddRange(Lollipop.all_mods_with_mass.Select(m => 
                    new PtmSet(new List<Ptm> {
                        new Ptm(-1, m),
                        new Ptm(-1, m),
                        new Ptm(-1, m)
                    })));
            }

            //order by E intensity, then by descending unadjusted_group_count (running sum) before forming peaks, and analyze only relations outside of no-man's-land
            remaining_relations_outside_no_mans = relations.Where(r => r.outside_no_mans_land).OrderByDescending(r => r.nearby_relations_count).ThenByDescending(r => r.agg_intensity_1).ToList(); // Group count is the primary sort
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