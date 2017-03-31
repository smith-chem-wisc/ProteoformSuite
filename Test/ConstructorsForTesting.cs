using System;
using System.Collections.Generic;
using System.Linq;
using Proteomics;
using ProteoformSuiteInternal;

namespace Test
{
    public class ConstructorsForTesting
    {
        //MAKE RELATION
        public static void make_relation(Proteoform p1, Proteoform p2)
        {
            ProteoformRelation pp = new ProteoformRelation(p1, p2, ProteoformComparison.ee, 0);
            DeltaMassPeak ppp = new DeltaMassPeak(pp, new List<ProteoformRelation> { pp });
            pp.peak = ppp;
            pp.accepted = true;
            ppp.peak_accepted = true;
            p1.relationships.Add(pp);
            p2.relationships.Add(pp);
        }

        public static void make_relation(Proteoform p1, Proteoform p2, ProteoformComparison c, double delta_mass)
        {
            ProteoformRelation pp = new ProteoformRelation(p1, p2, c, delta_mass);
            p1.relationships.Add(pp);
            p2.relationships.Add(pp);
        }


        //MAKE THEORETICAL
        public static TheoreticalProteoform make_a_theoretical(string a, ProteinWithGoTerms p, Dictionary<InputFile, Protein[]> dict)
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            PtmSet set = new PtmSet(p.OneBasedPossibleLocalizedModifications.SelectMany(m => m.Value.OfType<ModificationWithMass>().SelectMany(mmm => new List<Ptm> { new Ptm(0, mmm) })).ToList());
            return new TheoreticalProteoform(a, "", new List<ProteinWithGoTerms> { p }, false, 100, 0, set, true, true, dict);
        }

        public static TheoreticalProteoform make_a_theoretical()
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            string mod_title = "oxidation";
            ModificationWithMass m = new ModificationWithMass(mod_title, new Tuple<string, string>("", mod_title), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), "");
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>> { { 1, new List<Modification> { m } } }, new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") }) }, new List<GoTerm> { new GoTerm(new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") })) });
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(0, m) });
            return new TheoreticalProteoform("T1", "T1_1", new List<ProteinWithGoTerms> { p1 }, false, 100, 0, set, true, false, new Dictionary<InputFile, Protein[]>());
        }

        public static TheoreticalProteoform make_a_theoretical(string a, string d, double mass, ProteinWithGoTerms p, Dictionary<InputFile, Protein[]> dict)
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            string mod_title = "oxidation";
            ModificationWithMass m = new ModificationWithMass(mod_title, new Tuple<string, string>("", mod_title), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), "");
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("ordered locus", "GENE") }, new Dictionary<int, List<Modification>> { { 1, new List<Modification> { m } } }, new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") }) }, new List<GoTerm> { new GoTerm(new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") })) });
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(0, m) });
            return new TheoreticalProteoform(a, d, new List<ProteinWithGoTerms> { p }, false, mass, 0, set, true, false, dict);
        }

        public static TheoreticalProteoform make_a_theoretical(string a, string d, double mass, Dictionary<InputFile, Protein[]> dict)
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);
            string mod_title = "oxidation";
            ModificationWithMass m = new ModificationWithMass(mod_title, new Tuple<string, string>("", mod_title), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), "");
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("ordered locus", "GENE") }, new Dictionary<int, List<Modification>> { { 1, new List<Modification> { m } } }, new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") }) }, new List<GoTerm> { new GoTerm(new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") })) });
            PtmSet set = new PtmSet(new List<Ptm> { new Ptm(0, m) });
            return new TheoreticalProteoform(a, d, new List<ProteinWithGoTerms> { p1 }, false, mass, 0, set, true, false, dict);
        }

        public static TheoreticalProteoform make_a_theoretical(string a, double mass, int lysine_count)
        {
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>> { { 0, new List<Modification> { new Modification("unmodified") } } }, new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") }) }, new List<GoTerm> { new GoTerm(new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") })) });
            PtmSet set = new PtmSet(new List<Ptm>());
            return new TheoreticalProteoform(a, "", new List<ProteinWithGoTerms> { p1 }, false, mass, lysine_count, set, true, false, new Dictionary<InputFile, Protein[]>());
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
            e.lysine_count = lysine_count;
            e.accepted = true;
            return e;
        }

        public static ExperimentalProteoform ExperimentalProteoform(string accession, Component root, List<Component> candidate_observations, List<Component> quantitative_observations, bool is_target)
        {
            ExperimentalProteoform e = new ExperimentalProteoform(accession, root, is_target);
            e.aggregated_components.AddRange(candidate_observations.Where(p => e.includes(p, e.root)));
            e.calculate_properties();
            if (quantitative_observations.Count > 0)
            {
                e.lt_quant_components.AddRange(quantitative_observations.Where(r => e.includes_neucode_component(r, e, true)));
                if (Lollipop.neucode_labeled) e.hv_quant_components.AddRange(quantitative_observations.Where(r => e.includes_neucode_component(r, e, false)));
            }
            e.root = e.aggregated_components.OrderByDescending(a => a.intensity_sum).FirstOrDefault();
            return e;
        }


        //INPUT FILE
        public static InputFile InputFile(string complete_path, Labeling label, Purpose purpose, string lt_con, string hv_con, int biorep, int fraction, int techrep) // for neucode files. here both conditions are present in one file
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
            ModificationWithMass m = new ModificationWithMass(id, new Tuple<string, string>("", ""), motif, ModificationSites.K, mass, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), "");
            return m;
        }
    }
}
