using NUnit.Framework;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.IO;
using Proteomics;
using System.Linq;


namespace Test
{
    [TestFixture]
    class TestCalibration
    {
        [Test]
        public void get_topdown_hit_sequence()
        {
            Sweet.lollipop = new Lollipop();
            Sweet.lollipop.carbamidomethylation = false;
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "uniprot_yeast_test_12entries.xml") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.enter_input_files(new string[] { Path.Combine(TestContext.CurrentContext.TestDirectory, "ptmlist.txt") }, Lollipop.acceptable_extensions[2], Lollipop.file_types[2], Sweet.lollipop.input_files, false);
            Sweet.lollipop.theoretical_database.get_theoretical_proteoforms(TestContext.CurrentContext.TestDirectory);

            //make theo database so have ptm's and sequences... 
            TopDownHit hit = new TopDownHit();
            hit.sequence = "ASDACSDASD";
            hit.ptm_list = new List<Ptm>();
            ModificationWithMass mod = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.linksToOtherDbs.ContainsKey("RESID")).Where(m => m.linksToOtherDbs["RESID"].Contains("AA0502")).FirstOrDefault();
            hit.ptm_list.Add(new Ptm(hit.sequence.Length, mod));
            ModificationWithMass mod2 = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.linksToOtherDbs.ContainsKey("RESID")).Where(m => m.linksToOtherDbs["RESID"].Contains("AA0170")).FirstOrDefault();
            hit.ptm_list.Add(new Ptm(3, mod2));
            ModificationWithMass mod3 = Sweet.lollipop.theoretical_database.uniprotModifications.Values.SelectMany(m => m).OfType<ModificationWithMass>().Where(m => m.linksToOtherDbs.ContainsKey("RESID")).Where(m => m.linksToOtherDbs["RESID"].Contains("AA0433")).FirstOrDefault();
            hit.ptm_list.Add(new Ptm(1, mod3));

            string sequencewithchemicalformula = hit.GetSequenceWithChemicalFormula();
            Assert.AreEqual("[C2H4]AS[C5H12NO5P]DACSDAS[C6H9NO3]D", sequencewithchemicalformula);

            //should add carbamidomethylation to C... 
            Sweet.lollipop.carbamidomethylation = true;
            sequencewithchemicalformula = hit.GetSequenceWithChemicalFormula();
            Assert.AreEqual("[C2H4]AS[C5H12NO5P]DA[H3C2N1O1]CSDAS[C6H9NO3]D", sequencewithchemicalformula);

            //should return null if N term formula wrong or doesn't match mass 
            ModificationWithMassAndCf badNtermMod = new ModificationWithMassAndCf("badNtermMod", null, null, ModificationSites.NTerminus, Chemistry.ChemicalFormula.ParseFormula("H") , -1000, null, null, null, null);
            hit = new TopDownHit();
            hit.sequence = "ASDACSDASD";
            hit.ptm_list = new List<Ptm>() { new Ptm(1, badNtermMod) };
            Assert.IsNull(hit.GetSequenceWithChemicalFormula());

            //should return null if mod chem formula wrong or doesn't match mass 
            ModificationWithMassAndCf badMod = new ModificationWithMassAndCf("badMod", null, null, ModificationSites.Any, Chemistry.ChemicalFormula.ParseFormula("H"), -1000, null, null, null, null);
            hit = new TopDownHit();
            hit.sequence = "ASDACSDASD";
            hit.ptm_list = new List<Ptm>() { new Ptm(1, badMod) };
            Assert.IsNull(hit.GetSequenceWithChemicalFormula());

            //should return null if N term formula wrong or doesn't match mass 
            ModificationWithMassAndCf badCtermMod = new ModificationWithMassAndCf("badCtermMod", null, null, ModificationSites.TerminusC, Chemistry.ChemicalFormula.ParseFormula("H"), -1000, null, null, null, null);
            hit = new TopDownHit();
            hit.sequence = "ASDACSDASD";
            hit.ptm_list = new List<Ptm>() { new Ptm(1, badCtermMod) };
            Assert.IsNull(hit.GetSequenceWithChemicalFormula());

        }
    }
}
