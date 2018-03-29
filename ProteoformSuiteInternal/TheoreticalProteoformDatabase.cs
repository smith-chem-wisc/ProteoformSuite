using Proteomics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UsefulProteomicsDatabases;

namespace ProteoformSuiteInternal
{
    public class TheoreticalProteoformDatabase
    {

        #region Public Fields

        //Protein
        public Dictionary<InputFile, Protein[]> theoretical_proteins = new Dictionary<InputFile, Protein[]>();
        public ProteinWithGoTerms[] expanded_proteins = new ProteinWithGoTerms[0];
        public Dictionary<int, Dictionary<string, List<TheoreticalProteoform>>> theoreticals_by_accession = new Dictionary<int, Dictionary<string, List<TheoreticalProteoform>>>(); //key is decoy database, -100 for theoretical, key is accession, value is theoretical proteoforms w/ that accession

        //Modifications
        public Dictionary<string, List<Modification>> uniprotModifications = new Dictionary<string, List<Modification>>();
        public List<ModificationWithMass> variableModifications = new List<ModificationWithMass>();
        public List<ModificationWithMass> all_mods_with_mass = new List<ModificationWithMass>();
        public Dictionary<ModificationWithMass, UnlocalizedModification> unlocalized_lookup = new Dictionary<ModificationWithMass, UnlocalizedModification>();

        //PtmSets
        public List<PtmSet> all_possible_ptmsets = new List<PtmSet>();
        public Dictionary<double, List<PtmSet>> possible_ptmset_dictionary = new Dictionary<double, List<PtmSet>>();

        //Settings
        public bool limit_triples_and_greater = true;

        public Dictionary<char, double> aaIsotopeMassList;

        #endregion Public Fields

        #region Public Methods

        public void get_theoretical_proteoforms(string current_directory)
        {
            if (!ready_to_make_database(current_directory))
                return;

            //Clear out data from potential previous runs
            foreach (ProteoformCommunity community in Sweet.lollipop.decoy_proteoform_communities.Values)
            {
                community.theoretical_proteoforms = new TheoreticalProteoform[0];
            }

            theoretical_proteins.Clear();

            //Read the UniProt-XML and ptmlist
            var psiModDeserialized = Loaders.LoadPsiMod(Path.Combine(current_directory, "Mods", "PSI-MOD.obo.xml"));
            Dictionary<string, int> formalChargesDictionary = Loaders.GetFormalChargesDictionary(psiModDeserialized);

            List<ModificationWithLocation> all_known_modifications = Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.PtmList).SelectMany(file => PtmListLoader.ReadModsFromFile(file.complete_path, formalChargesDictionary)).OfType<ModificationWithLocation>().ToList();
            uniprotModifications = make_modification_dictionary(all_known_modifications);
            Parallel.ForEach(Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.ProteinDatabase).ToList(), database =>
            {
                lock (theoretical_proteins) theoretical_proteins.Add(database, ProteinDbLoader.LoadProteinXML(database.complete_path, true, DecoyType.None, all_known_modifications, database.ContaminantDB, Sweet.lollipop.mod_types_to_exclude, out Dictionary<string, Modification> um).ToArray());
                lock (all_known_modifications) all_known_modifications.AddRange(ProteinDbLoader.GetPtmListFromProteinXml(database.complete_path).OfType<ModificationWithLocation>().Where(m => !Sweet.lollipop.mod_types_to_exclude.Contains(m.modificationType)));
            });

            foreach (string filename in Directory.GetFiles(Path.Combine(current_directory, "Mods")))
            {
                List<ModificationWithMass> new_mods = !filename.EndsWith("variable.txt") || Sweet.lollipop.methionine_oxidation ?
                    PtmListLoader.ReadModsFromFile(filename, formalChargesDictionary).OfType<ModificationWithMass>().ToList() :
                    new List<ModificationWithMass>(); // Empty variable modifications if not selected
                if (filename.EndsWith("variable.txt"))
                    variableModifications = new_mods;
                all_known_modifications.AddRange(new_mods);
            }

            all_known_modifications = new HashSet<ModificationWithLocation>(all_known_modifications).ToList();
            uniprotModifications = make_modification_dictionary(all_known_modifications);

            all_mods_with_mass = uniprotModifications.SelectMany(kv => kv.Value).OfType<ModificationWithMass>().Concat(variableModifications).ToList();
            Sweet.lollipop.modification_ranks = rank_mods(theoretical_proteins, variableModifications, all_mods_with_mass);

            unlocalized_lookup = make_unlocalized_lookup(all_mods_with_mass.Concat(new List<ModificationWithMass> { new Ptm().modification }));
            load_unlocalized_names(Path.Combine(Environment.CurrentDirectory, "Mods", "stored_mods.modnames"));

            //this is for ptmsets --> used in RELATIONS
            all_possible_ptmsets = PtmCombos.generate_all_ptmsets(2, all_mods_with_mass, Sweet.lollipop.modification_ranks, Sweet.lollipop.mod_rank_first_quartile / 2).ToList();
            for (int i = 2; i < Math.Max(2, Sweet.lollipop.max_ptms) + 1; i++) // the method above doesn't make 2 or more of a kind, so we make it here
            {
                all_possible_ptmsets.AddRange(all_mods_with_mass.Select(m => new PtmSet(Enumerable.Repeat(new Ptm(-1, m), i).ToList(), Sweet.lollipop.modification_ranks, Sweet.lollipop.mod_rank_first_quartile / 2)));
            }

            //Generate lookup table for ptm sets based on rounded mass of eligible PTMs -- used in forming ET relations
            possible_ptmset_dictionary = make_ptmset_dictionary();
            make_theoretical_proteoforms();
        }

        //separate --> if topdown proteoforms, need to redo this and add topdown proteoforms
        public void make_theoretical_proteoforms()
        {
            theoreticals_by_accession.Clear();
            expanded_proteins = expand_protein_entries(theoretical_proteins.Values.SelectMany(p => p).ToArray());
            populate_aa_mass_dictionary();
            add_topdown_sequences();
            if (Sweet.lollipop.combine_identical_sequences) expanded_proteins = group_proteins_by_sequence(expanded_proteins);
            expanded_proteins = expanded_proteins.OrderBy(x => x.OneBasedPossibleLocalizedModifications.Count).ThenBy(x => x.BaseSequence).ToArray(); // Take on harder problems first to use parallelization more effectively
            process_entries(expanded_proteins, variableModifications);
            process_decoys(Sweet.lollipop.target_proteoform_community.theoretical_proteoforms);

            Parallel.ForEach(new ProteoformCommunity[] { Sweet.lollipop.target_proteoform_community }.Concat(Sweet.lollipop.decoy_proteoform_communities.Values), community =>
            {
                if (Sweet.lollipop.combine_theoretical_proteoforms_byMass) community.theoretical_proteoforms = group_proteoforms_by_mass(community.theoretical_proteoforms);
                add_theoreticals_to_accession_dictionary(community.theoretical_proteoforms, community.community_number);
            });
        }


        private void add_theoreticals_to_accession_dictionary(TheoreticalProteoform[] theoreticals, int community_number)
        {
            lock (theoreticals_by_accession) theoreticals_by_accession.Add(community_number, new Dictionary<string, List<TheoreticalProteoform>>());
            foreach (TheoreticalProteoform t in theoreticals)
            {
                foreach (string t_accession in t.ExpandedProteinList.SelectMany(p => p.AccessionList.Select(a => a.Split('_')[0])).Distinct())
                {
                    if (t_accession == null) continue;
                    lock (theoreticals_by_accession)
                    {
                        if (theoreticals_by_accession[community_number].ContainsKey(t_accession)) theoreticals_by_accession[community_number][t_accession].Add(t);
                        else theoreticals_by_accession[community_number].Add(t_accession, new List<TheoreticalProteoform>() { t });
                    }
                }
            }
        }

        //Generate lookup table for ptm sets based on rounded mass of eligible PTMs -- used in forming ET relations
        public Dictionary<double, List<PtmSet>> make_ptmset_dictionary()
        {
            Dictionary<double, List<PtmSet>> possible_ptmsets = new Dictionary<double, List<PtmSet>>();
            foreach (PtmSet set in all_possible_ptmsets.Where(s => s.ptm_combination.Count == 1 || !s.ptm_combination.Select(ptm => ptm.modification).Any(m => m.monoisotopicMass == 0)).ToList())
            {
                for (int i = 0; i < 10; i++)
                {
                    double midpoint = Math.Round(Math.Round(set.mass, 1) - 0.5 + i * 0.1, 1);
                    if (possible_ptmsets.TryGetValue(midpoint, out List<PtmSet> a)) a.Add(set);
                    else possible_ptmsets.Add(midpoint, new List<PtmSet> { set });
                }
            }
            return possible_ptmsets;
        }

        public Dictionary<string, List<Modification>> make_modification_dictionary(IEnumerable<ModificationWithLocation> all_modifications)
        {
            Dictionary<string, List<Modification>> mod_dict = new Dictionary<string, List<Modification>>();
            foreach (var nice in all_modifications)
            {
                if (mod_dict.TryGetValue(nice.id, out List<Modification> val)) val.Add(nice);
                else mod_dict.Add(nice.id, new List<Modification> { nice });
            }
            return mod_dict;
        }

        public Dictionary<double, int> rank_mods(Dictionary<InputFile, Protein[]> theoretical_proteins, IEnumerable<ModificationWithMass> variable_modifications, IEnumerable<ModificationWithMass> all_mods_with_mass)
        {
            Dictionary<double, int> mod_counts = new Dictionary<double, int>();

            foreach (ModificationWithMass m in theoretical_proteins.SelectMany(kv => kv.Value).SelectMany(p => p.OneBasedPossibleLocalizedModifications).SelectMany(kv => kv.Value).OfType<ModificationWithMass>().ToList())
            {
                if (!mod_counts.TryGetValue(m.monoisotopicMass, out int b)) mod_counts.Add(m.monoisotopicMass, 1);
                else mod_counts[m.monoisotopicMass]++;
            }
            List<KeyValuePair<double, int>> ordered_mod_counts = mod_counts.OrderByDescending(kv => kv.Value).ToList();

            int rank = 3;
            int last_count = 0;
            Dictionary<double, int> mod_ranks = new Dictionary<double, int>();
            foreach (KeyValuePair<double, int> mod_count in ordered_mod_counts)
            {
                mod_ranks.Add(mod_count.Key, rank);
                if (mod_count.Value < last_count) rank++;
                last_count = mod_count.Value;
            }

            //Give unmodified the best rank
            if (!mod_ranks.TryGetValue(0, out int a)) mod_ranks.Add(0, 0);
            else mod_ranks[0] = 0;

            //Give variable mods a good score
            foreach (ModificationWithMass m in variable_modifications)
            {
                if (!mod_ranks.TryGetValue(m.monoisotopicMass, out int b)) mod_ranks.Add(m.monoisotopicMass, 2);
                else mod_ranks[m.monoisotopicMass] = 2;
            }

            List<int> ranks = mod_ranks.Values.OrderBy(x => x).ToList();
            Sweet.lollipop.mod_rank_first_quartile = ranks[ranks.Count / 4];
            Sweet.lollipop.mod_rank_second_quartile = ranks[2 * ranks.Count / 4];
            Sweet.lollipop.mod_rank_third_quartile = ranks[3 * ranks.Count / 4];
            Sweet.lollipop.mod_rank_sum_threshold = ranks.Max();

            //Give the remaining mods the threshold value
            foreach (ModificationWithMass m in all_mods_with_mass)
            {
                if (!mod_ranks.TryGetValue(m.monoisotopicMass, out int lkj))
                    mod_ranks.Add(m.monoisotopicMass, Sweet.lollipop.mod_rank_sum_threshold);
            }

            return mod_ranks;
        }

        public static ProteinWithGoTerms[] expand_protein_entries(Protein[] proteins)
        {
            List<ProteinWithGoTerms> expanded_prots = new List<ProteinWithGoTerms>();
            foreach (Protein p in proteins)
            {
                List<ProteinWithGoTerms> new_prots = new List<ProteinWithGoTerms>();

                //Add full length product
                int begin = 1;
                int end = p.BaseSequence.Length;
                List<GoTerm> goTerms = p.DatabaseReferences.Where(reference => reference.Type == "GO").Select(reference => new GoTerm(reference)).ToList();
                int startPosAfterCleavage = Convert.ToInt32(Sweet.lollipop.methionine_cleavage && p.BaseSequence.StartsWith("M"));
                new_prots.Add(new ProteinWithGoTerms(
                    p.BaseSequence.Substring(begin + startPosAfterCleavage - 1, end - (begin + startPosAfterCleavage) + 1),
                    p.Accession + "_" + (begin + startPosAfterCleavage).ToString() + "full" + end.ToString(),
                    p.GeneNames.ToList(),
                    p.OneBasedPossibleLocalizedModifications,
                    new List<ProteolysisProduct> { new ProteolysisProduct(begin + startPosAfterCleavage, end, Sweet.lollipop.methionine_cleavage && p.BaseSequence.StartsWith("M") ? "full-met-cleaved" : "full") },
                    p.Name, p.FullName, p.IsDecoy, p.IsContaminant, p.DatabaseReferences, goTerms));

                //Add fragments
                List<ProteolysisProduct> products = p.ProteolysisProducts.ToList();
                foreach (ProteolysisProduct product in p.ProteolysisProducts)
                {
                    string feature_type = product.Type.Replace(' ', '-');
                    if (!(feature_type == "peptide" || feature_type == "propeptide" || feature_type == "chain" || feature_type == "signal-peptide")
                            || !product.OneBasedBeginPosition.HasValue
                            || !product.OneBasedEndPosition.HasValue)
                        continue;
                    int feature_begin = (int)product.OneBasedBeginPosition;
                    int feature_end = (int)product.OneBasedEndPosition;
                    if (feature_begin < 1 || feature_end < 1)
                        continue;
                    bool feature_is_just_met_cleavage = Sweet.lollipop.methionine_cleavage && feature_begin == begin + 1 && feature_end == end;
                    string subsequence = p.BaseSequence.Substring(feature_begin - 1, feature_end - feature_begin + 1);
                    Dictionary<int, List<Modification>> segmented_ptms = p.OneBasedPossibleLocalizedModifications.Where(kv => kv.Key >= feature_begin && kv.Key <= feature_end).ToDictionary(kv => kv.Key, kv => kv.Value);
                    if (!feature_is_just_met_cleavage && subsequence.Length != p.BaseSequence.Length && subsequence.Length >= Sweet.lollipop.min_peptide_length)
                        new_prots.Add(new ProteinWithGoTerms(
                            subsequence,
                            p.Accession + "_" + feature_begin.ToString() + "frag" + feature_end.ToString(),
                            p.GeneNames.ToList(),
                            segmented_ptms,
                            new List<ProteolysisProduct> { new ProteolysisProduct(feature_begin, feature_end, feature_type) },
                            p.Name, p.FullName, p.IsDecoy, p.IsContaminant, p.DatabaseReferences, goTerms));
                }
                expanded_prots.AddRange(new_prots);
            }
            return expanded_prots.ToArray();
        }


        public ProteinSequenceGroup[] group_proteins_by_sequence(IEnumerable<ProteinWithGoTerms> proteins)
        {
            Dictionary<string, List<ProteinWithGoTerms>> sequence_groupings = new Dictionary<string, List<ProteinWithGoTerms>>();
            foreach (ProteinWithGoTerms p in proteins)
            {
                if (sequence_groupings.ContainsKey(p.BaseSequence)) sequence_groupings[p.BaseSequence].Add(p);
                else sequence_groupings.Add(p.BaseSequence, new List<ProteinWithGoTerms> { p });
            }
            return sequence_groupings.Select(kv => new ProteinSequenceGroup(kv.Value.OrderByDescending(p => p.IsContaminant ? 1 : 0))).ToArray();
        }

        public TheoreticalProteoformGroup[] group_proteoforms_by_mass(IEnumerable<TheoreticalProteoform> theoreticals)
        {
            Dictionary<double, List<TheoreticalProteoform>> mass_groupings = new Dictionary<double, List<TheoreticalProteoform>>();
            foreach (TheoreticalProteoform t in theoreticals)
            {
                if (mass_groupings.ContainsKey(Math.Round(t.modified_mass, 4))) mass_groupings[Math.Round(t.modified_mass, 4)].Add(t);
                else mass_groupings.Add(Math.Round(t.modified_mass, 4), new List<TheoreticalProteoform> { t });
            }
            return mass_groupings.Select(kv => new TheoreticalProteoformGroup(kv.Value.OrderByDescending(t => t.contaminant ? 1 : 0).ThenBy(t => t.topdown_theoretical ? 1 : 0))).ToArray();
        }

        /// <summary>
        /// Requires at least one ProteinDatabase input file and one input file listing modifications.
        /// </summary>
        /// <returns></returns>
        public bool ready_to_make_database(string current_directory)
        {
            Loaders.LoadElements(Path.Combine(current_directory, "elements.dat"));
            List<InputFile> proteinDbs = Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.ProteinDatabase).ToList();
            return proteinDbs.Count > 0
                && (proteinDbs.Any(file => ProteinDbLoader.GetPtmListFromProteinXml(file.complete_path).Count > 0)
                || Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.PtmList).Count() > 0);
        }

        public void EnterTheoreticalProteformFamily(string seq, ProteinWithGoTerms prot, IDictionary<int, List<Modification>> modifications, string accession, List<TheoreticalProteoform> theoretical_proteoforms, int decoy_number, IEnumerable<ModificationWithMass> variableModifications)
        {
            List<TheoreticalProteoform> new_theoreticals = new List<TheoreticalProteoform>();
            //Calculate the properties of this sequence
            double unmodified_mass = TheoreticalProteoform.CalculateProteoformMass(seq, aaIsotopeMassList);
            int lysine_count = seq.Split('K').Length - 1;
            bool check_contaminants = theoretical_proteins.Any(item => item.Key.ContaminantDB);

            //Figure out the possible ptm sets
            Dictionary<int, List<Modification>> possibleLocalizedMods = modifications.ToDictionary(kv => kv.Key, kv => new List<Modification>(kv.Value));
            foreach (ModificationWithMass m in variableModifications)
            {
                for (int i = 1; i <= prot.BaseSequence.Length; i++)
                {
                    if (prot.BaseSequence[i - 1].ToString() == m.motif.ToString())
                    {
                        if (!possibleLocalizedMods.TryGetValue(i, out List<Modification> a)) possibleLocalizedMods.Add(i, new List<Modification> { m });
                        else a.Add(m);
                    }
                }
            }
            //if topdown protein (added new sequence) only add PTMs of the protein
            //these are the PTMs of the top-down proteoform sequence used to make the protein
            //other top-down proteoforms with this sesquence and different PTM types will be added when add_topdown_theoreticals.
            List<PtmSet> unique_ptm_groups = prot.topdown_protein? new List<PtmSet>() { new PtmSet(prot.OneBasedPossibleLocalizedModifications.SelectMany(m => m.Value.Select(p => new Ptm(m.Key, (ModificationWithMass)p))).ToList(), Sweet.lollipop.modification_ranks, Sweet.lollipop.mod_rank_first_quartile / 2) }
            : PtmCombos.get_combinations(possibleLocalizedMods, Sweet.lollipop.max_ptms, Sweet.lollipop.modification_ranks, Sweet.lollipop.mod_rank_first_quartile / 2, limit_triples_and_greater);

            //Enumerate the ptm combinations with _P# to distinguish from the counts in ProteinSequenceGroups (_#G) and TheoreticalPfGps (_#T)
            int ptm_set_counter = 1;
            foreach (PtmSet ptm_set in unique_ptm_groups)
            {
                TheoreticalProteoform t =
                    new TheoreticalProteoform(
                        accession + "_P" + ptm_set_counter.ToString(),
                        prot.FullDescription + "_P" + ptm_set_counter.ToString() + (decoy_number < 0 ? "" : "_DECOY_" + decoy_number.ToString()),
                        seq,
                        new ProteinWithGoTerms[] { prot },
                        unmodified_mass,
                        lysine_count,
                        ptm_set,
                        decoy_number < 0,
                        check_contaminants,
                        theoretical_proteins);
                t.topdown_theoretical = prot.topdown_protein;
                new_theoreticals.Add(t);
                ptm_set_counter++;
            }
            add_topdown_theoreticals(prot, seq, accession, unmodified_mass, decoy_number, lysine_count, new_theoreticals, ptm_set_counter, Sweet.lollipop.modification_ranks, Sweet.lollipop.mod_rank_first_quartile / 2);
            lock (theoretical_proteoforms) theoretical_proteoforms.AddRange(new_theoreticals);
        }

        //if protein sequence doesn't exist, need to add...
        public void add_topdown_sequences()
        {
            List<ProteinWithGoTerms> new_proteins = new List<ProteinWithGoTerms>();
            foreach (TopDownProteoform topdown in Sweet.lollipop.topdown_proteoforms.OrderBy(t => t.modified_mass))
            {
                List<ProteinWithGoTerms> candidate_theoreticals = expanded_proteins.Where(p => p.AccessionList.Select(a => a.Split('_')[0].Split('-')[0]).Contains(topdown.accession.Split('_')[0].Split('-')[0])).ToList();
                if (candidate_theoreticals.Count > 0)
                {
                    topdown.gene_name = new GeneName(candidate_theoreticals.First().GeneNames);
                    if (!candidate_theoreticals.Any(p => p.BaseSequence == topdown.sequence) && !new_proteins.Any(p => p.AccessionList.Select(a => a.Split('_')[0]).Contains(topdown.accession.Split('_')[0].Split('-')[0]) && p.BaseSequence == topdown.sequence))
                    {
                        int old_proteins_with_same_begin_end_diff_sequence = candidate_theoreticals.Count(t => t.ProteolysisProducts.First().OneBasedBeginPosition == topdown.topdown_begin && t.ProteolysisProducts.First().OneBasedEndPosition == topdown.topdown_end && t.BaseSequence != topdown.sequence);
                        int new_proteins_with_same_being_end_diff_sequence = new_proteins.Count(t => t.AccessionList.Select(a => a.Split('_')[0].Split('-')[0]).Contains(topdown.accession.Split('_')[0]) && t.ProteolysisProducts.First().OneBasedBeginPosition == topdown.topdown_begin && t.ProteolysisProducts.First().OneBasedEndPosition == topdown.topdown_end && t.BaseSequence != topdown.sequence);
                        int count = old_proteins_with_same_begin_end_diff_sequence + new_proteins_with_same_being_end_diff_sequence;
                        ProteinWithGoTerms p = new ProteinWithGoTerms(topdown.sequence, topdown.accession.Split('_')[0].Split('-')[0] + "_" + topdown.topdown_begin + "frag" + topdown.topdown_end + (count > 0 ? "_" + count : ""), candidate_theoreticals.First().GeneNames.ToList(), new Dictionary<int, List<Modification>>(), new List<ProteolysisProduct>() { new ProteolysisProduct(topdown.topdown_begin, topdown.topdown_end, "full") }, candidate_theoreticals.First().Name, candidate_theoreticals.First().FullName, false, false, candidate_theoreticals.First().DatabaseReferences, candidate_theoreticals.First().GoTerms);
                        p.topdown_protein = true;
                        //add the ptm's of the top-down proteoform
                        foreach (Ptm ptm in topdown.topdown_ptm_set.ptm_combination.Where(m => m.position >= p.ProteolysisProducts.First().OneBasedBeginPosition && m.position <= p.ProteolysisProducts.First().OneBasedEndPosition))
                        {
                            if (p.OneBasedPossibleLocalizedModifications.ContainsKey(ptm.position))
                            {
                                p.OneBasedPossibleLocalizedModifications[ptm.position].Add(ptm.modification);
                            }
                            else p.OneBasedPossibleLocalizedModifications.Add(ptm.position, new List<Modification>() { ptm.modification });
                        }
                        new_proteins.Add(p);
                    }
                }
                else topdown.accepted = false;
            }
            expanded_proteins = expanded_proteins.Concat(new_proteins).ToArray();
        }

        public void add_topdown_theoreticals(ProteinWithGoTerms prot, string seq, string accession, double unmodified_mass, int decoy_number, int lysine_count, List<TheoreticalProteoform> new_theoreticals, int ptm_set_counter, Dictionary<double, int> mod_ranks, int added_ptm_penalty)
        {
            foreach (TopDownProteoform topdown in Sweet.lollipop.topdown_proteoforms.Where(p => prot.AccessionList.Select(a => a.Split('_')[0]).Contains(p.accession.Split('_')[0].Split('-')[0])
                && p.topdown_begin == prot.ProteolysisProducts.First().OneBasedBeginPosition && p.topdown_end == prot.ProteolysisProducts.First().OneBasedEndPosition).OrderBy(t => t.accession).ThenByDescending(t => t.sequence.Length)) //order by gene name then descending sequence length --> order matters for creating theoreticals.
            {
                if (!new_theoreticals.Any(t => t.ptm_set.same_ptmset(topdown.topdown_ptm_set, true)))
                {
                    List<ProteinWithGoTerms> proteins = new List<ProteinWithGoTerms>() { prot };
                    if (new_theoreticals.Count > 0) proteins.AddRange(new_theoreticals.First().ExpandedProteinList);
                    //match each td proteoform group to the closest theoretical w/ best explanation.... otherwise make new theoretical proteoform
                    PtmSet ptm_set = new PtmSet(topdown.topdown_ptm_set.ptm_combination, mod_ranks, added_ptm_penalty);
                    TheoreticalProteoform t =
                    new TheoreticalProteoform(
                        accession + "_P" + ptm_set_counter.ToString(),
                        prot.FullDescription + "_P" + ptm_set_counter.ToString() + (decoy_number < 0 ? "" : "_DECOY_" + decoy_number.ToString()),
                        seq,
                        proteins.Distinct().ToArray(),
                        unmodified_mass,
                        lysine_count,
                        ptm_set,
                        decoy_number < 0,
                        false,
                        theoretical_proteins);
                    t.topdown_theoretical = true;
                    new_theoreticals.Add(t);
                    ptm_set_counter++;
                }
            }
        }

        public void populate_aa_mass_dictionary()
        {
            aaIsotopeMassList = new AminoAcidMasses(Sweet.lollipop.carbamidomethylation, Sweet.lollipop.natural_lysine_isotope_abundance, Sweet.lollipop.neucode_light_lysine, Sweet.lollipop.neucode_heavy_lysine).AA_Masses;
        }

        #endregion Public Methods

        #region Unlocalized Mods Public Methods

        public Dictionary<ModificationWithMass, UnlocalizedModification> make_unlocalized_lookup(IEnumerable<ModificationWithMass> all_modifications)
        {
            Dictionary<ModificationWithMass, UnlocalizedModification> mod_dict = new Dictionary<ModificationWithMass, UnlocalizedModification>();
            foreach (var nice in all_modifications)
            {
                if (!mod_dict.TryGetValue(nice, out UnlocalizedModification val))
                    mod_dict.Add(nice, new UnlocalizedModification(nice));
            }
            return mod_dict;
        }

        public void save_unlocalized_names(string filepath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filepath)))
                return;

            using (StreamWriter writer = new StreamWriter(filepath))
            {
                foreach (var unloc in unlocalized_lookup)
                {
                    writer.WriteLine(unloc.Key.id + "\t" + unloc.Value.id + "\t" + unloc.Value.ptm_count.ToString() + "\t" + unloc.Value.require_proteoform_without_mod.ToString());
                }
            }
        }

        public void load_unlocalized_names(string filepath)
        {
            if (!File.Exists(filepath))
                return;

            Dictionary<string, string[]> mod_info = new Dictionary<string, string[]>();
            using (StreamReader reader = new StreamReader(filepath))
            {
                while (true)
                {
                    string a = reader.ReadLine();
                    if (a == null)
                        break;
                    string[] line = a.Split('\t');
                    if (!mod_info.TryGetValue(line[0], out string[] info))
                        mod_info.Add(line[0], line);
                }
            }

            foreach (var mod_unlocalized in unlocalized_lookup)
            {
                if (mod_info.TryGetValue(mod_unlocalized.Key.id, out string[] new_info))
                {
                    mod_unlocalized.Value.id = new_info[1];
                    mod_unlocalized.Value.ptm_count = Convert.ToInt32(new_info[2]);
                    mod_unlocalized.Value.require_proteoform_without_mod = Convert.ToBoolean(new_info[3]);
                }
            }
        }

        public void amend_unlocalized_names(string filepath)
        {
            if (!File.Exists(filepath))
                return;

            Dictionary<string, string[]> mod_info = new Dictionary<string, string[]>();
            using (StreamReader reader = new StreamReader(filepath))
            {
                while (true)
                {
                    string a = reader.ReadLine();
                    if (a == null)
                        break;
                    string[] line = a.Split('\t');
                    if (!mod_info.TryGetValue(line[0], out string[] info))
                        mod_info.Add(line[0], line);
                }
            }

            foreach (var mod_unlocalized in unlocalized_lookup)
            {
                string[] new_info = new string[] { mod_unlocalized.Key.id, mod_unlocalized.Value.id, mod_unlocalized.Value.ptm_count.ToString(), mod_unlocalized.Value.require_proteoform_without_mod.ToString() };
                if (mod_info.TryGetValue(mod_unlocalized.Key.id, out string[] x))
                    mod_info[mod_unlocalized.Key.id] = new_info;
                else
                    mod_info.Add(mod_unlocalized.Key.id, new_info);
            }

            using (StreamWriter writer = new StreamWriter(filepath))
            {
                foreach (var unloc in mod_info.Values.OrderBy(x => x[0]))
                {
                    writer.WriteLine(String.Join("\t", unloc));
                }
            }
        }

        #endregion Unlocalized Mods Public Methods

        #region Private Methods

        private void process_entries(IEnumerable<ProteinWithGoTerms> expanded_proteins, IEnumerable<ModificationWithMass> variableModifications)
        {
            List<TheoreticalProteoform> theoretical_proteoforms = new List<TheoreticalProteoform>();
            Parallel.ForEach(expanded_proteins, p => EnterTheoreticalProteformFamily(p.BaseSequence, p, p.OneBasedPossibleLocalizedModifications, p.Accession, theoretical_proteoforms, -100, variableModifications));
            Sweet.lollipop.target_proteoform_community.theoretical_proteoforms = theoretical_proteoforms.ToArray();
            Sweet.lollipop.target_proteoform_community.community_number = -100;
        }

        private void process_decoys(TheoreticalProteoform[] entries)
        {
            Sweet.lollipop.decoy_proteoform_communities.Clear();
            Parallel.For(0, Sweet.lollipop.decoy_databases, decoyNumber =>
            {
                List<TheoreticalProteoform> decoy_proteoforms = new List<TheoreticalProteoform>();
                StringBuilder sb = new StringBuilder(5000000); // this set-aside is autoincremented to larger values when necessary.
                foreach (TheoreticalProteoform proteoform in entries)
                {
                    sb.Append(proteoform.sequence);
                }
                string giantProtein = sb.ToString();
                TheoreticalProteoform[] shuffled_proteoforms = new TheoreticalProteoform[entries.Length];
                Array.Copy(entries, shuffled_proteoforms, entries.Length);
                Random decoy_rng = Sweet.lollipop.useRandomSeed_decoys ? new Random(decoyNumber + Sweet.lollipop.randomSeed_decoys) : new Random(); // each decoy database needs to have a new random number generator
                decoy_rng.Shuffle(shuffled_proteoforms); //randomize order of protein array
                int prevLength = 0;
                foreach (var p in shuffled_proteoforms)
                {
                    string hunk = giantProtein.Substring(prevLength, p.sequence.Length);
                    prevLength += p.sequence.Length;
                    TheoreticalProteoform t = new TheoreticalProteoform(p.accession + "_DECOY_" + decoyNumber,
                        p.description, hunk, p.ExpandedProteinList, TheoreticalProteoform.CalculateProteoformMass(hunk, aaIsotopeMassList), hunk.Count(s => s == 'K'),
                        p.ptm_set, false, p.contaminant, theoretical_proteins);
                    decoy_proteoforms.Add(t);
                }

                lock (Sweet.lollipop.decoy_proteoform_communities)
                {
                    Sweet.lollipop.decoy_proteoform_communities.Add(Sweet.lollipop.decoy_community_name_prefix + decoyNumber, new ProteoformCommunity());
                    Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + decoyNumber].theoretical_proteoforms = decoy_proteoforms.ToArray();
                    Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + decoyNumber].experimental_proteoforms =
                    Sweet.lollipop.target_proteoform_community.experimental_proteoforms.Select(e => new ExperimentalProteoform(e)).ToArray();
                    Sweet.lollipop.decoy_proteoform_communities[Sweet.lollipop.decoy_community_name_prefix + decoyNumber].community_number = decoyNumber;
                }
            });
        }

        #endregion Private Methods

    }
}
