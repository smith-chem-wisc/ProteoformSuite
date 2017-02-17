using NUnit.Framework;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.Linq;
using Proteomics;
using System;

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

            listForPosition1.Add(new ModificationWithMass("description1", new Tuple<string, string>("", ""), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));
            listForPosition1.Add(new ModificationWithMass("description2", new Tuple<string, string>("", ""), motif, ModificationSites.K, 2, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));

            List<Modification> listForPosition2 = new List<Modification>();

            listForPosition2.Add(new ModificationWithMass("description3", new Tuple<string, string>("", ""), motif, ModificationSites.K, 3, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));
            listForPosition2.Add(new ModificationWithMass("description4", new Tuple<string, string>("", ""), motif, ModificationSites.K, 4, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));

            Dictionary<int, List<Modification>> ptm_data = new Dictionary<int, List<Modification>>();
            ptm_data.Add(1, listForPosition1);
            ptm_data.Add(2, listForPosition2);

            PtmCombos ptmCombos = new PtmCombos(ptm_data);

            Assert.AreEqual(4, ptmCombos.all_ptms.Count);
            Assert.AreEqual(6, ptmCombos.all_ptms.Select(b => b.position).Sum());

            // With masses 1, 2, 3, 4
            Assert.AreEqual(1 + 4, ptmCombos.get_combinations(1).Count());
            // With masses 1, 2, 3, 4, 5, 6
            Assert.AreEqual(1 + 6, ptmCombos.get_combinations(2).Count());
            // With masses 1, 2, 3, 4, 5, 6
            Assert.AreEqual(1 + 6, ptmCombos.get_combinations(3).Count());
            // With masses 1, 2, 3, 4, 5, 6
            Assert.AreEqual(1 + 6, ptmCombos.get_combinations(4).Count());

        }


        [Test]
        public void TestPtmCombos2()
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);

            List<Modification> listForPosition1 = new List<Modification>();

            listForPosition1.Add(new ModificationWithMass("description1", new Tuple<string, string>("", ""), motif, ModificationSites.K, 1.1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));
            listForPosition1.Add(new ModificationWithMass("description2", new Tuple<string, string>("", ""), motif, ModificationSites.K, 2.01, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));

            List<Modification> listForPosition2 = new List<Modification>();

            listForPosition2.Add(new ModificationWithMass("description3", new Tuple<string, string>("", ""), motif, ModificationSites.K, 3.001, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));
            listForPosition2.Add(new ModificationWithMass("description4", new Tuple<string, string>("", ""), motif, ModificationSites.K, 4.0001, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));

            Dictionary<int, List<Modification>> ptm_data = new Dictionary<int, List<Modification>>();
            ptm_data.Add(1, listForPosition1);
            ptm_data.Add(2, listForPosition2);

            PtmCombos ptmCombos = new PtmCombos(ptm_data);

            Assert.AreEqual(4, ptmCombos.all_ptms.Count);
            Assert.AreEqual(6, ptmCombos.all_ptms.Select(b => b.position).Sum());

            Assert.AreEqual(1 + 4, ptmCombos.get_combinations(1).Count());
            Assert.AreEqual(1 + 8, ptmCombos.get_combinations(2).Count());
            Assert.AreEqual(1 + 8, ptmCombos.get_combinations(3).Count());
            Assert.AreEqual(1 + 8, ptmCombos.get_combinations(4).Count());

        }


        [Test]
        public void TestPtmCombos3()
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);

            List<Modification> listForPosition1 = new List<Modification>();
            listForPosition1.Add(new ModificationWithMass("description1", new Tuple<string, string>("", ""), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));
            List<Modification> listForPosition2 = new List<Modification>();
            listForPosition2.Add(new ModificationWithMass("description2", new Tuple<string, string>("", ""), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));
            List<Modification> listForPosition3 = new List<Modification>();
            listForPosition3.Add(new ModificationWithMass("description3", new Tuple<string, string>("", ""), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));
            List<Modification> listForPosition4 = new List<Modification>();
            listForPosition4.Add(new ModificationWithMass("description4", new Tuple<string, string>("", ""), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));
            List<Modification> listForPosition5 = new List<Modification>();
            listForPosition5.Add(new ModificationWithMass("description5", new Tuple<string, string>("", ""), motif, ModificationSites.K, 100, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));

            Dictionary<int, List<Modification>> ptm_data = new Dictionary<int, List<Modification>>();
            ptm_data.Add(1, listForPosition1);
            ptm_data.Add(2, listForPosition2);
            ptm_data.Add(3, listForPosition3);
            ptm_data.Add(4, listForPosition4);
            ptm_data.Add(5, listForPosition5);

            PtmCombos ptmCombos = new PtmCombos(ptm_data);

            Assert.AreEqual(1 + 2, ptmCombos.get_combinations(1).Count());
            Assert.AreEqual(1 + 4, ptmCombos.get_combinations(2).Count());
            Assert.AreEqual(1 + 6, ptmCombos.get_combinations(3).Count());

        }


        [Test]
        public void TestPtmCombos4()
        {
            ModificationMotif motif;
            ModificationMotif.TryGetMotif("K", out motif);

            List<Modification> listForPosition1 = new List<Modification>();
            listForPosition1.Add(new ModificationWithMass("description1", new Tuple<string, string>("", ""), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));
            List<Modification> listForPosition2 = new List<Modification>();
            listForPosition2.Add(new ModificationWithMass("description2", new Tuple<string, string>("", ""), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));
            List<Modification> listForPosition3 = new List<Modification>();
            listForPosition3.Add(new ModificationWithMass("description3", new Tuple<string, string>("", ""), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));
            List<Modification> listForPosition4 = new List<Modification>();
            listForPosition4.Add(new ModificationWithMass("description4", new Tuple<string, string>("", ""), motif, ModificationSites.K, 1, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));
            List<Modification> listForPosition5 = new List<Modification>();
            listForPosition5.Add(new ModificationWithMass("description5", new Tuple<string, string>("", ""), motif, ModificationSites.K, 100, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));
            listForPosition5.Add(new ModificationWithMass("description6", new Tuple<string, string>("", ""), motif, ModificationSites.K, 200, new Dictionary<string, IList<string>>(), -1, new List<double>(), new List<double>(), ""));

            Dictionary<int, List<Modification>> ptm_data = new Dictionary<int, List<Modification>>();
            ptm_data.Add(1, listForPosition1);
            ptm_data.Add(2, listForPosition2);
            ptm_data.Add(3, listForPosition3);
            ptm_data.Add(4, listForPosition4);
            ptm_data.Add(5, listForPosition5);

            PtmCombos ptmCombos = new PtmCombos(ptm_data);

            Assert.AreEqual(1, ptmCombos.get_combinations(0).Count());
            Assert.AreEqual(0, ptmCombos.get_combinations(0)[0].mass);
            Assert.AreEqual(0, ptmCombos.get_combinations(0)[0].ptm_combination.Count());
            Assert.AreEqual(1 + 3, ptmCombos.get_combinations(1).Count());
            Assert.AreEqual(1 + 3 + 3, ptmCombos.get_combinations(2).Count());
            Assert.AreEqual(1 + 3 + 3 + 3, ptmCombos.get_combinations(3).Count());
        }
    }
}
