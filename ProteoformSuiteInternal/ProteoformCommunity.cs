using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using FlashLFQ;
using IO.Thermo;
using MassSpectrometry;



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
                    && (pf1.modified_mass - pf2_with_allowed_lysines.modified_mass) <= Sweet.lollipop.et_high_mass_difference;

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

        #endregion  GROUP and ANALYZE RELATIONS

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
            Parallel.ForEach(families, f => f.identify_experimentals());
            if (community_number < 0 && experimental_proteoforms.Any(e => !e.topdown_id && e.linked_proteoform_references != null)) quantify_experimentals();
            return families;
        }

        private void quantify_experimentals()
        {
            if (!Sweet.lollipop.input_files.Any(f => f.purpose == Purpose.RawFile)) return;
            foreach (string condition in Sweet.lollipop.input_files.Select(f => f.lt_condition).Distinct())
            {
                for (int tr = 1; tr <= 2; tr++)
                {
                    FlashLFQEngine engine = new FlashLFQEngine();

                    HashSet<InputFile> files_to_quantitate = new HashSet<InputFile>();
                    Dictionary<string, List<ExperimentalProteoform>> quantified_experimentals = new Dictionary<string, List<ExperimentalProteoform>>();

                    foreach (ExperimentalProteoform e in experimental_proteoforms.Where(p => !p.topdown_id && p.linked_proteoform_references != null))
                    {
                        TheoreticalProteoform t = e.linked_proteoform_references.First() as TheoreticalProteoform;
                        string base_sequence = e.begin == 1 && t.begin == 2 ? "M" + t.sequence.Substring(0, e.end - 1) :
                                t.sequence.Substring(e.begin - t.begin, e.end - e.begin + 1);

                        string full_sequence = e.GetSequenceWithChemicalFormula(base_sequence);
                        if (full_sequence != null)
                        {
                            string first_description = t.description.Split(';')[0];
                            string[] elements = first_description.Split('_');
                            string bond_description = elements.Where(s => s.Contains("Disulfide")).FirstOrDefault();
                            int disulfide_bonds = bond_description == null ? 0 : Convert.ToInt32(bond_description.Split(':')[1]);
                            for (int i = 0; i < disulfide_bonds; i++)
                            {
                                full_sequence += "[H-2]";
                            }
                            string fragment_description = elements.Where(s => s.Contains("Fragments")).FirstOrDefault();
                            int fragments = fragment_description == null ? 0 : Convert.ToInt32(fragment_description.Split(':')[1].Count(s => s == 't'));
                            for (int i = 0; i < fragments - 1; i++)
                            {
                                full_sequence += "[H2O]";
                            }

                            double theoretical_mass = new Proteomics.Peptide(full_sequence).MonoisotopicMass;

                            foreach (InputFile component_file in e.aggregated.Select(a => a.input_file).Distinct())
                            {
                                if (component_file.technical_replicate != tr.ToString() || component_file.lt_condition != condition) continue;
                                foreach (InputFile file in Sweet.lollipop.input_files.Where(f => f.purpose == Purpose.RawFile && f.lt_condition == component_file.lt_condition && f.biological_replicate == component_file.biological_replicate &&
                                f.fraction == component_file.fraction && f.technical_replicate == component_file.technical_replicate))
                                {
                                    foreach (Component c in e.aggregated.Where(c => c.input_file == component_file))
                                    {
                                        double rt = Convert.ToDouble(c.rt_range.Split('-')[0]);
                                        while (rt <= Convert.ToDouble(c.rt_range.Split('-')[0]))
                                        {
                                            foreach (var cs in c.charge_states)
                                            {
                                                engine.AddIdentification(Path.GetFileNameWithoutExtension(file.complete_path), base_sequence, full_sequence, theoretical_mass, rt, cs.charge_count, new List<string>() { e.linked_proteoform_references.First().accession.Split('_')[0] });
                                            }
                                            rt += .01;
                                        }
                                    }
                                    files_to_quantitate.Add(file);
                                }
                                if (quantified_experimentals.ContainsKey(full_sequence)) quantified_experimentals[full_sequence].Add(e);
                                else quantified_experimentals.Add(full_sequence, new List<ExperimentalProteoform>() { e });
                            }
                        }
                    }
                    if (files_to_quantitate.Count == 0) return;
                    engine.ReadPeriodicTable(Path.Combine(Environment.CurrentDirectory, "elements.dat"));
                    engine.PassFilePaths(files_to_quantitate.Select(f => f.complete_path).ToArray());
                    engine.ParseArgs(new string[]
                    {
                    "--ppm " + Sweet.lollipop.mass_tolerance,
                    "--sil true",
                    "--pau false",
                    "--mbr false",
                    "--chg true",
                    "--int false",
                    "--rmm false"
                    });
                    engine.ConstructIndexTemplateFromIdentifications();
                    Parallel.ForEach(files_to_quantitate, f =>
                    {
                        IMsDataFile<IMsDataScan<IMzSpectrum<IMzPeak>>> myMsDataFile = ThermoStaticData.LoadAllStaticData(f.complete_path);
                        engine.Quantify(myMsDataFile, f.complete_path);
                    });

                    var summedPeaksByPeptide = engine.SumFeatures(engine.allFeaturesByFile.SelectMany(p => p).ToList(), false);
                    foreach (var i in summedPeaksByPeptide)
                    {
                        foreach (var e in quantified_experimentals[i.Sequence])
                        {
                            foreach (var c in e.aggregated.Where(a => a.input_file.technical_replicate == tr.ToString() && a.input_file.lt_condition == condition))
                            {
                                (c as Component).flash_flq_intensity = i.intensitiesByFile.Sum();
                            }
                        }
                    }
                }
            }
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
            foreach (Proteoform p in theoretical_proteoforms) p.family = null;
        }
        #endregion CLEAR FAMILIES

    }
}
