using NUnit.Framework;
using ProteoformSuiteInternal;
using Proteomics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    [TestFixture]

    public class TestModification
    {

        [Test]
        public void TestPtmCombos1()
        {
            List<Modification> listForPosition1 = new List<Modification>();

            //description, accession, feature_type, position, targetaas, monoisotpic mass shift, average mass shift
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);

            listForPosition1.Add(new ModificationWithMass("description1", "type", motif, TerminusLocalization.Any, 1));
            listForPosition1.Add(new ModificationWithMass("description2", "type", motif, TerminusLocalization.Any, 2));

            List<Modification> listForPosition2 = new List<Modification>();

            listForPosition2.Add(new ModificationWithMass("description3", "type", motif, TerminusLocalization.Any, 3));
            listForPosition2.Add(new ModificationWithMass("description4", "type", motif, TerminusLocalization.Any, 4));

            Dictionary<int, List<Modification>> ptm_data = new Dictionary<int, List<Modification>>();
            ptm_data.Add(1, listForPosition1);
            ptm_data.Add(2, listForPosition2);

            Dictionary<double, int> fake_ranks = new Dictionary<double, int>
            {
                { 1, 1 },
                { 2, 2 },
                { 3, 3 },
                { 4, 4 },
            };

            // With masses 1, 2, 3, 4
            Assert.AreEqual(1 + 4, PtmCombos.get_combinations(ptm_data, 1, fake_ranks, 1, false).Count());
            // With masses 1, 2, 3, 4, 5, 6
            Assert.AreEqual(1 + 6, PtmCombos.get_combinations(ptm_data, 2, fake_ranks, 1, false).Count());
            // With masses 1, 2, 3, 4, 5, 6
            Assert.AreEqual(1 + 6, PtmCombos.get_combinations(ptm_data, 3, fake_ranks, 1, false).Count());
            // With masses 1, 2, 3, 4, 5, 6
            Assert.AreEqual(1 + 6, PtmCombos.get_combinations(ptm_data, 4, fake_ranks, 1, false).Count());
        }


        [Test]
        public void TestPtmCombos2()
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);

            List<Modification> listForPosition1 = new List<Modification>();

            listForPosition1.Add(new ModificationWithMass("description1", "", motif, TerminusLocalization.Any, 1.1));
            listForPosition1.Add(new ModificationWithMass("description2", "", motif, TerminusLocalization.Any, 2.01));

            List<Modification> listForPosition2 = new List<Modification>();

            listForPosition2.Add(new ModificationWithMass("description3", "", motif, TerminusLocalization.Any, 3.001));
            listForPosition2.Add(new ModificationWithMass("description4", "", motif, TerminusLocalization.Any, 4.0001));

            Dictionary<int, List<Modification>> ptm_data = new Dictionary<int, List<Modification>>();
            ptm_data.Add(1, listForPosition1);
            ptm_data.Add(2, listForPosition2);

            //PtmCombos ptmCombos = new PtmCombos(ptm_data);

            //Assert.AreEqual(4, ptmCombos.all_ptms.Count);
            //Assert.AreEqual(6, ptmCombos.all_ptms.Select(b => b.position).Sum());

            Dictionary<double, int> fake_ranks = new Dictionary<double, int>
            {
                { 1.1, 1 },
                { 2.01, 2 },
                { 3.001, 3 },
                { 4.0001, 4 },
            };

            Assert.AreEqual(1 + 4, PtmCombos.get_combinations(ptm_data, 1, fake_ranks, 1, false).Count());
            Assert.AreEqual(1 + 8, PtmCombos.get_combinations(ptm_data, 2, fake_ranks, 1, false).Count());
            Assert.AreEqual(1 + 8, PtmCombos.get_combinations(ptm_data, 3, fake_ranks, 1, false).Count());
            Assert.AreEqual(1 + 8, PtmCombos.get_combinations(ptm_data, 4, fake_ranks, 1, false).Count());

        }


        [Test]
        public void TestPtmCombos3()
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);

            List<Modification> listForPosition1 = new List<Modification>();
            listForPosition1.Add(new ModificationWithMass("description1", "type", motif, TerminusLocalization.Any, 1, new Dictionary<string, IList<string>>(), new List<string>(), new List<double>()));
            List<Modification> listForPosition2 = new List<Modification>();
            listForPosition2.Add(new ModificationWithMass("description2", "type", motif, TerminusLocalization.Any, 1, new Dictionary<string, IList<string>>(), new List<string>(), new List<double>()));
            List<Modification> listForPosition3 = new List<Modification>();
            listForPosition3.Add(new ModificationWithMass("description3", "type", motif, TerminusLocalization.Any, 1, new Dictionary<string, IList<string>>(), new List<string>(), new List<double>()));
            List<Modification> listForPosition4 = new List<Modification>();
            listForPosition4.Add(new ModificationWithMass("description4", "type", motif, TerminusLocalization.Any, 1, new Dictionary<string, IList<string>>(), new List<string>(), new List<double>()));
            List<Modification> listForPosition5 = new List<Modification>();
            listForPosition5.Add(new ModificationWithMass("description5", "type", motif, TerminusLocalization.Any, 100, new Dictionary<string, IList<string>>(), new List<string>(), new List<double>()));

            Dictionary<int, List<Modification>> ptm_data = new Dictionary<int, List<Modification>>();
            ptm_data.Add(1, listForPosition1);
            ptm_data.Add(2, listForPosition2);
            ptm_data.Add(3, listForPosition3);
            ptm_data.Add(4, listForPosition4);
            ptm_data.Add(5, listForPosition5);

            //PtmCombos ptmCombos = new PtmCombos(ptm_data);

            Dictionary<double, int> fake_ranks = new Dictionary<double, int>
            {
                { 1, 1 },
                { 100, 2 },
                { 3, 3 },
                { 4, 4 },
            };

            Assert.AreEqual(1 + 2, PtmCombos.get_combinations(ptm_data, 1, fake_ranks, 1, false).Count);
            Assert.AreEqual(1 + 4, PtmCombos.get_combinations(ptm_data, 2, fake_ranks, 1, false).Count);
            Assert.AreEqual(1 + 6, PtmCombos.get_combinations(ptm_data, 3, fake_ranks, 1, false).Count);

        }


        [Test]
        public void TestPtmCombos4()
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);

            List<Modification> listForPosition1 = new List<Modification>();
            listForPosition1.Add(new ModificationWithMass("description1", "type", motif, TerminusLocalization.Any, 1, new Dictionary<string, IList<string>>(), new List<string>(), new List<double>()));
            List<Modification> listForPosition2 = new List<Modification>();
            listForPosition2.Add(new ModificationWithMass("description2", "type", motif, TerminusLocalization.Any, 1, new Dictionary<string, IList<string>>(), new List<string>(), new List<double>()));
            List<Modification> listForPosition3 = new List<Modification>();
            listForPosition3.Add(new ModificationWithMass("description3", "type", motif, TerminusLocalization.Any, 1, new Dictionary<string, IList<string>>(), new List<string>(), new List<double>()));
            List<Modification> listForPosition4 = new List<Modification>();
            listForPosition4.Add(new ModificationWithMass("description4", "type", motif, TerminusLocalization.Any, 1, new Dictionary<string, IList<string>>(), new List<string>(), new List<double>()));
            List<Modification> listForPosition5 = new List<Modification>();
            listForPosition5.Add(new ModificationWithMass("description5", "type", motif, TerminusLocalization.Any, 100, new Dictionary<string, IList<string>>(), new List<string>(), new List<double>()));
            List<Modification> listForPosition6 = new List<Modification>();
            listForPosition5.Add(new ModificationWithMass("description6", "type", motif, TerminusLocalization.Any, 200, new Dictionary<string, IList<string>>(), new List<string>(), new List<double>()));

            Dictionary<int, List<Modification>> ptm_data = new Dictionary<int, List<Modification>>();
            ptm_data.Add(1, listForPosition1);
            ptm_data.Add(2, listForPosition2);
            ptm_data.Add(3, listForPosition3);
            ptm_data.Add(4, listForPosition4);
            ptm_data.Add(5, listForPosition5);

            //PtmCombos ptmCombos = new PtmCombos(ptm_data);

            Dictionary<double, int> fake_ranks = new Dictionary<double, int>
            {
                { 1, 1 },
                { 100, 2 },
                { 200, 3 },
                { 4, 4 },
            };

            Assert.AreEqual(1, PtmCombos.get_combinations(ptm_data, 0, fake_ranks, 1, false).Count());
            Assert.AreEqual(0, PtmCombos.get_combinations(ptm_data, 0, fake_ranks, 1, false)[0].mass);
            Assert.AreEqual(0, PtmCombos.get_combinations(ptm_data, 0, fake_ranks, 1, false)[0].ptm_combination.Count());
            Assert.AreEqual(1 + 3, PtmCombos.get_combinations(ptm_data, 1, fake_ranks, 1, false).Count());
            Assert.AreEqual(1 + 3 + 3, PtmCombos.get_combinations(ptm_data, 2, fake_ranks, 1, false).Count());
            Assert.AreEqual(1 + 3 + 3 + 3, PtmCombos.get_combinations(ptm_data, 3, fake_ranks, 1, false).Count());
        }

        [Test]
        public void test_limit_large_combos()
        {
            IDictionary<int, List<Modification>> a1 = new Dictionary<int, List<Modification>>
            {
                { 1, new List<Modification>{ ConstructorsForTesting.get_modWithMass("ox", 16) } },
                { 2, new List<Modification>{ ConstructorsForTesting.get_modWithMass("ox", 16) } },
                { 3, new List<Modification>{ ConstructorsForTesting.get_modWithMass("ox", 16) } },
                { 4, new List<Modification>{ ConstructorsForTesting.get_modWithMass("ac", 42) } },
            };
            List<PtmSet> sets1 = PtmCombos.get_combinations(a1, 3, new Dictionary<double, int> { { 16, 1 }, { 42, 2 } }, 1, true);
            Assert.AreEqual(1, sets1.Count(s => s.ptm_combination.Count == 3));
            Assert.True(sets1.Where(s => s.ptm_combination.Count == 3).First().ptm_combination.All(p => p.modification.id == "ox"));

            IDictionary<int, List<Modification>> a2 = new Dictionary<int, List<Modification>>
            {
                { 1, new List<Modification>{ ConstructorsForTesting.get_modWithMass("ox", 16), ConstructorsForTesting.get_modWithMass("ox", 16) } },
                { 3, new List<Modification>{ ConstructorsForTesting.get_modWithMass("ox", 16) } },
                { 4, new List<Modification>{ ConstructorsForTesting.get_modWithMass("ac", 42) } },
            };
            List<PtmSet> sets2 = PtmCombos.get_combinations(a2, 3, new Dictionary<double, int> { { 16, 1 }, { 42, 2 } }, 1, true);
            Assert.AreEqual(0, sets2.Count(s => s.ptm_combination.Count == 3));
        }
        
        [Test]
        public void test_same_ptmset()
        {
            PtmSet set1 = new PtmSet(new List<Ptm>()
            {
                new Ptm(1, ConstructorsForTesting.get_modWithMass("id1", 5)),
                new Ptm(2, ConstructorsForTesting.get_modWithMass("id2", 6)),
                new Ptm(2, ConstructorsForTesting.get_modWithMass("id4", 6))
            });
            PtmSet set2 = new PtmSet(new List<Ptm>()
            {
                new Ptm(1, ConstructorsForTesting.get_modWithMass("id1", 5)),
                new Ptm(2, ConstructorsForTesting.get_modWithMass("id2", 6)),
                new Ptm(2, ConstructorsForTesting.get_modWithMass("id4", 6))
            });
            Assert.IsTrue(set1.same_ptmset(set2, true));
            Assert.IsTrue(set1.same_ptmset(set2, false));
            set2 = new PtmSet(new List<Ptm>()
            {
                new Ptm(1, ConstructorsForTesting.get_modWithMass("id1", 5)),
                new Ptm(2, ConstructorsForTesting.get_modWithMass("id2", 6)),
                new Ptm(2, ConstructorsForTesting.get_modWithMass("id3", 6))
            });
            Assert.IsFalse(set1.same_ptmset(set2, true));
            Assert.IsFalse(set1.same_ptmset(set2, false));
            Assert.IsFalse(set2.same_ptmset(set1, true));
            Assert.IsFalse(set2.same_ptmset(set1, false));

            set2 = new PtmSet(new List<Ptm>()
            {
                new Ptm(1, ConstructorsForTesting.get_modWithMass("id1", 5)),
                new Ptm(2, ConstructorsForTesting.get_modWithMass("id2", 6)),
            });
            Assert.IsFalse(set1.same_ptmset(set2, true));
            Assert.IsFalse(set1.same_ptmset(set2, false));
            Assert.IsFalse(set2.same_ptmset(set1, true));
            Assert.IsFalse(set2.same_ptmset(set1, false));
        }
    }
}
