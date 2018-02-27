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

        public int community_number; //-100 for target, decoy database number for decoys
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

        #endregion Public Properties

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
                    if (!pfs2_lysine_lookup.TryGetValue(pf2.lysine_count, out List<Proteoform> same_lysine_ct)) { pfs2_lysine_lookup.Add(pf2.lysine_count, new List<Proteoform> { pf2 }); }
                    else { same_lysine_ct.Add(pf2); }
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
                        if (pf1 as TopDownProteoform == null) pf1.gene_name = null;
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
            {
                return
                    (pf1.modified_mass - pf2_with_allowed_lysines.modified_mass) >= Sweet.lollipop.et_low_mass_difference
                    && (pf1.modified_mass - pf2_with_allowed_lysines.modified_mass) <= Sweet.lollipop.et_high_mass_difference;
            }
            else if (relation_type == ProteoformComparison.ExperimentalExperimental)
            {
                return
                    pf1 != pf2_with_allowed_lysines
                    && pf1.modified_mass >= pf2_with_allowed_lysines.modified_mass
                    && pf1 != pf2_with_allowed_lysines
                    && pf1.modified_mass - pf2_with_allowed_lysines.modified_mass <= Sweet.lollipop.ee_max_mass_difference
                    && Math.Abs((pf1 as ExperimentalProteoform).agg_rt - (pf2_with_allowed_lysines as ExperimentalProteoform).agg_rt) <= Sweet.lollipop.ee_max_RetentionTime_difference;
            }
            else if (relation_type == ProteoformComparison.ExperimentalFalse)
            {
                return
                    pf1.modified_mass >= pf2_with_allowed_lysines.modified_mass
                    && pf1 != pf2_with_allowed_lysines
                    && (pf1.modified_mass - pf2_with_allowed_lysines.modified_mass <= Sweet.lollipop.ee_max_mass_difference)
                    && (Sweet.lollipop.neucode_labeled || Math.Abs((pf1 as ExperimentalProteoform).agg_rt - (pf2_with_allowed_lysines as ExperimentalProteoform).agg_rt) > Sweet.lollipop.ee_max_RetentionTime_difference * 2)
                    && (!Sweet.lollipop.neucode_labeled || Math.Abs((pf1 as ExperimentalProteoform).agg_rt - (pf2_with_allowed_lysines as ExperimentalProteoform).agg_rt) < Sweet.lollipop.ee_max_RetentionTime_difference);
            }
            else
            {
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
            Random random = Sweet.lollipop.useRandomSeed_decoys ? new Random(community_number + Sweet.lollipop.randomSeed_decoys) : new Random(); //new random generator for each round of
            var shuffled = all_ef_relations.OrderBy(item => random.Next()).ToList();
            return shuffled.Take(Sweet.lollipop.ee_relations.Count).ToList();
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
                    {
                        throw new ArgumentException("Only EE and ET peaks can be accepted");
                    }

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
            {
                Sweet.lollipop.et_peaks.AddRange(peaks);
                Sweet.update_peaks_from_presets(ProteoformComparison.ExperimentalTheoretical); // accept or unaccept peaks noted in presets
                Sweet.mass_shifts_from_presets(); //shift peaks
            }
            else
            {
                Sweet.lollipop.ee_peaks.AddRange(peaks);
                Sweet.update_peaks_from_presets(ProteoformComparison.ExperimentalExperimental); // accept or unaccept peaks noted in presets
            }

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

        #endregion GROUP and ANALYZE RELATIONS

        #region CONSTRUCTING FAMILIES

        public List<ProteoformFamily> construct_families()
        {
            ProteoformFamily.reset_family_counter();
            Parallel.ForEach(experimental_proteoforms, e => e.ambiguous = false); //need to reset all as falsely ambigous
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
            if (Lollipop.gene_centric_families) families = combine_gene_families(families).ToList();
            Sweet.lollipop.theoretical_database.aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, Sweet.lollipop.natural_lysine_isotope_abundance, Sweet.lollipop.neucode_light_lysine, Sweet.lollipop.neucode_heavy_lysine).AA_Masses;
            Parallel.ForEach(families, f => f.identify_experimentals());
            //read in BU results if available, map to proteoforms.
            //Sweet.lollipop.BottomUpPSMList.Clear();
            //BottomUpReader.bottom_up_PTMs_not_in_dictionary.Clear();
            //foreach (InputFile file in Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.BottomUp))
            //{
            //    Sweet.lollipop.BottomUpPSMList.AddRange(BottomUpReader.ReadBUFile(file.complete_path, theoreticals_by_accession.Values.ToList()));
            //}
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
                if (p as TopDownProteoform == null) { p.gene_name = null; }
            }
            foreach (Proteoform p in theoretical_proteoforms) p.family = null;
        }

        #endregion CLEAR FAMILIES
    }
}