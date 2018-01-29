using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Proteomics;
using ProteoformSuiteInternal;
using UsefulProteomicsDatabases;
using System.IO;

namespace Test
{
    public class ConstructorsForTesting
    {
        //MAKE RELATION
        public static void make_relation(Proteoform p1, Proteoform p2)
        {
            ProteoformRelation pp = new ProteoformRelation(p1, p2, ProteoformComparison.ExperimentalExperimental, 0, TestContext.CurrentContext.TestDirectory);
            DeltaMassPeak ppp = new DeltaMassPeak(pp, new HashSet<ProteoformRelation> { pp });
            pp.peak = ppp;
            pp.Accepted = true;
            ppp.Accepted = true;
            p1.relationships.Add(pp);
            p2.relationships.Add(pp);
        }

        public static ProteoformRelation make_relation(Proteoform p1, Proteoform p2, ProteoformComparison c, double delta_mass)
        {
            ProteoformRelation pp = new ProteoformRelation(p1, p2, c, delta_mass, TestContext.CurrentContext.TestDirectory);
            p1.relationships.Add(pp);
            p2.relationships.Add(pp);
            return pp;
        }
        
        //MAKE THEORETICAL
        public static TheoreticalProteoform make_a_theoretical(string a, ProteinWithGoTerms p, Dictionary<InputFile, Protein[]> dict)
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            PtmSet set = new PtmSet(p.OneBasedPossibleLocalizedModifications.SelectMany(m => m.Value.OfType<ModificationWithMass>().SelectMany(mmm => new List<Ptm> { new Ptm(0, mmm) })).ToList());
            TheoreticalProteoform t = new TheoreticalProteoform(a, "", p.BaseSequence,  new List<ProteinWithGoTerms> { p }, 100, 0, set, true, true, dict);
            t.begin = 1;
            t.end = 4;
            return t;
        }

        public static TheoreticalProteoform make_a_theoretical()
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            string mod_title = "oxidation";
            ModificationWithMass m = new ModificationWithMass(mod_title, "modtype", motif, TerminusLocalization.Any, 1);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>> { { 1, new List<Modification> { m } } }, new List<ProteolysisProduct> { new ProteolysisProduct(1, 12, "") }, "T2", "T3", true, false, new List<DatabaseReference> { new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") }) }, new List<GoTerm> { new GoTerm(new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") })) });
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(0, m) });
            return new TheoreticalProteoform("T1", "T1_1", p1.BaseSequence, new List<ProteinWithGoTerms> { p1 }, 100, 0, set, true, false, new Dictionary<InputFile, Protein[]>());
        }

        public static TheoreticalProteoform make_a_theoretical(string a, string d, double mass, ProteinWithGoTerms p, Dictionary<InputFile, Protein[]> dict)
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            string mod_title = "oxidation";
            ModificationWithMass m = new ModificationWithMass(mod_title, "modtype", motif, TerminusLocalization.Any, 1);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("ordered locus", "GENE") }, new Dictionary<int, List<Modification>> { { 1, new List<Modification> { m } } }, new List<ProteolysisProduct> { new ProteolysisProduct(1, 12, "") }, "T2", "T3", true, false, new List<DatabaseReference> { new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") }) }, new List<GoTerm> { new GoTerm(new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") })) });
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(0, m) });
            return new TheoreticalProteoform(a, d, p1.BaseSequence,  new List<ProteinWithGoTerms> { p }, mass, 0, set, true, false, dict);
        }

        public static TheoreticalProteoform make_a_theoretical(string a, string d, double mass, Dictionary<InputFile, Protein[]> dict)
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            string mod_title = "oxidation";
            ModificationWithMass m = new ModificationWithMass(mod_title, "modtype", motif, TerminusLocalization.Any, 1);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("ordered locus", "GENE") }, new Dictionary<int, List<Modification>> { { 1, new List<Modification> { m } } }, new List<ProteolysisProduct> { new ProteolysisProduct(1, 12, "") }, "T2", "T3", true, false, new List<DatabaseReference> { new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") }) }, new List<GoTerm> { new GoTerm(new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") })) });
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(0, m) });
            return new TheoreticalProteoform(a, d, p1.BaseSequence, new List<ProteinWithGoTerms> { p1 }, mass, 0, set, true, false, dict);
        }

        public static TheoreticalProteoform make_a_theoretical(string a, double mass, int lysine_count)
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            ModificationWithMass unmodification = new ModificationWithMass("Unmodified", null, motif, TerminusLocalization.Any, 0, null, null, null, null);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>> { { 0, new List<Modification> { unmodification } } }, new List<ProteolysisProduct> { new ProteolysisProduct(1, 12, "") }, "T2", "T3", true, false, new List<DatabaseReference> { new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") }) }, new List<GoTerm> { new GoTerm(new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") })) });
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm() });
            return new TheoreticalProteoform(a, "", p1.BaseSequence, new List<ProteinWithGoTerms> { p1 }, mass, lysine_count, set, true, false, new Dictionary<InputFile, Protein[]>());
        }


        //EXPERIMENTAL PROTEOFORMS
        public static ExperimentalProteoform ExperimentalProteoform(string accession)
        {
            return new ExperimentalProteoform(accession, new Component(), true);
        }

        public static ExperimentalProteoform ExperimentalProteoform(string accession, double modified_mass, int lysine_count, bool is_target)
        {
            ExperimentalProteoform e = new ExperimentalProteoform(accession, new Component(), is_target);
            e.modified_mass = modified_mass;
            e.agg_mass = modified_mass;
            e.lysine_count = lysine_count;
            e.accepted = true;
            return e;
        }

        public static ExperimentalProteoform ExperimentalProteoform(string accession, IAggregatable root, List<IAggregatable> candidate_observations, List<Component> quantitative_observations, bool is_target)
        {
            ExperimentalProteoform e = new ExperimentalProteoform(accession, root, is_target);
            e.aggregated.AddRange(candidate_observations.Where(p => e.includes(p, e.root)));
            e.calculate_properties();
            if (quantitative_observations.Count > 0)
            {
                e.lt_quant_components.AddRange(quantitative_observations.Where(r => e.includes_neucode_component(r, e, true)));
                if (Sweet.lollipop.neucode_labeled)
                    e.hv_quant_components.AddRange(quantitative_observations.Where(r => e.includes_neucode_component(r, e, false)));
            }
            e.root = e.aggregated.OrderByDescending(a => a.intensity_sum).FirstOrDefault();
            return e;
        }

        //TOPDOWN pROTEOFORM
        public static TopDownProteoform TopDownProteoform(string accession, double modified_mass, double retention_time)
        {
            TopDownHit h = new TopDownHit();
            h.reported_mass = modified_mass;
            h.theoretical_mass = modified_mass;
            h.ms2_retention_time = retention_time;
            h.sequence = "MSSSSSSSSSS";
            h.begin = 10;
            h.end = 20;
            TopDownProteoform td = new TopDownProteoform(accession, new List<TopDownHit>() { h } );
            (td as ExperimentalProteoform).topdown_id = true;
            td.accepted = true;
            return td;
        }


        //INPUT FILE
        public static InputFile InputFile(string complete_path, Labeling label, Purpose purpose, string lt_con, string hv_con, string biorep, string fraction, string techrep) // for neucode files. here both conditions are present in one file
        {
            InputFile f = new InputFile(complete_path, label, purpose);
            f.lt_condition = lt_con;
            f.hv_condition = hv_con;
            f.biological_replicate = biorep;
            f.fraction = fraction;
            f.technical_replicate = techrep;
            return f;
        }


        //MODIFICATION
        public static ModificationWithMass get_modWithMass(string id, double mass)
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            ModificationWithMass m = new ModificationWithMass(id, "modtype", motif, TerminusLocalization.Any, mass);
            return m;
        }

        public static Dictionary<string, List<Modification>> read_mods()
        {
            Loaders.LoadElements(Path.Combine(TestContext.CurrentContext.TestDirectory, "elements.dat"));
            List<ModificationWithLocation> all_modifications = Sweet.lollipop.get_files(Sweet.lollipop.input_files, Purpose.PtmList).SelectMany(file => PtmListLoader.ReadModsFromFile(file.complete_path)).OfType<ModificationWithLocation>().ToList();
            return Sweet.lollipop.theoretical_database.make_modification_dictionary(all_modifications);
        }
    }
}
