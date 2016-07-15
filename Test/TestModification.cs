using System.Collections.Generic;
using NUnit.Framework;
using ProteoformSuiteInternal;
using System.Linq;
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

            listForPosition1.Add(new Modification("description1", "acession1", "featureType1", "position1", new char[] { 'K' }, 1, 1.1));
            listForPosition1.Add(new Modification("description2", "acession2", "featureType2", "position2", new char[] { 'K' }, 2, 2.1));

            List<Modification> listForPosition2 = new List<Modification>();

            listForPosition2.Add(new Modification("description3", "acession3", "featureType3", "position3", new char[] { 'K' }, 3, 3.1));
            listForPosition2.Add(new Modification("description4", "acession4", "featureType4", "position4", new char[] { 'K' }, 4, 4.1));

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
            List<Modification> listForPosition1 = new List<Modification>();

            listForPosition1.Add(new Modification("description1", "acession1", "featureType1", "position1", new char[] { 'K' }, 1.1, 1.1));
            listForPosition1.Add(new Modification("description2", "acession2", "featureType2", "position2", new char[] { 'K' }, 2.01, 2.01));

            List<Modification> listForPosition2 = new List<Modification>();

            listForPosition2.Add(new Modification("description3", "acession3", "featureType3", "position3", new char[] { 'K' }, 3.001, 3.001));
            listForPosition2.Add(new Modification("description4", "acession4", "featureType4", "position4", new char[] { 'K' }, 4.0001, 4.0001));

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
            List<Modification> listForPosition1 = new List<Modification>();
            listForPosition1.Add(new Modification("description1", "acession1", "featureType1", "position1", new char[] { 'K' }, 1, 1));
            List<Modification> listForPosition2 = new List<Modification>();
            listForPosition2.Add(new Modification("description2", "acession2", "featureType2", "position2", new char[] { 'K' }, 1, 1));
            List<Modification> listForPosition3 = new List<Modification>();
            listForPosition3.Add(new Modification("description3", "acession3", "featureType3", "position3", new char[] { 'K' }, 1, 1));
            List<Modification> listForPosition4 = new List<Modification>();
            listForPosition4.Add(new Modification("description4", "acession4", "featureType4", "position4", new char[] { 'K' }, 1, 1));
            List<Modification> listForPosition5 = new List<Modification>();
            listForPosition5.Add(new Modification("description5", "acession5", "featureType5", "position5", new char[] { 'K' }, 100, 100));

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
            List<Modification> listForPosition1 = new List<Modification>();
            listForPosition1.Add(new Modification("description1", "acession1", "featureType1", "position1", new char[] { 'K' }, 1, 1));
            List<Modification> listForPosition2 = new List<Modification>();
            listForPosition2.Add(new Modification("description2", "acession2", "featureType2", "position2", new char[] { 'K' }, 1, 1));
            List<Modification> listForPosition3 = new List<Modification>();
            listForPosition3.Add(new Modification("description3", "acession3", "featureType3", "position3", new char[] { 'K' }, 1, 1));
            List<Modification> listForPosition4 = new List<Modification>();
            listForPosition4.Add(new Modification("description4", "acession4", "featureType4", "position4", new char[] { 'K' }, 1, 1));
            List<Modification> listForPosition5 = new List<Modification>();
            listForPosition5.Add(new Modification("description5", "acession5", "featureType5", "position5", new char[] { 'K' }, 100, 100));
            listForPosition5.Add(new Modification("description6", "acession6", "featureType6", "position6", new char[] { 'K' }, 200, 100));

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
