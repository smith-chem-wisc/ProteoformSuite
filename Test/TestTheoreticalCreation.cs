using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using Proteomics;
using System.Linq;

namespace Test
{
    [TestFixture]
    public class TestTheoreticalCreation
    {
        [Test]
        public void test_get_modification_dictionary()
        {

        }

        [Test]
        public void test_protein_grouping_by_sequence()
        {
            DatabaseReference d1 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            DatabaseReference d2 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            DatabaseReference d3 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            GoTerm g1 = new GoTerm(d1);
            GoTerm g2 = new GoTerm(d1);
            GoTerm g3 = new GoTerm(d1);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { d1 }, new List<GoTerm> { g1 });
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { d2 }, new List<GoTerm> { g2 });
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("MCSSSSSSSSSS", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { d3 }, new List<GoTerm> { g3 });
            ProteinSequenceGroup psg = new ProteinSequenceGroup(new List<ProteinWithGoTerms> { p1, p2, p3 }.OrderByDescending(p => p.IsContaminant ? 1 : 0));
            Assert.AreEqual(3, psg.GoTerms.Count());
            Assert.AreEqual(3, psg.GeneNames.Count());
            Assert.AreEqual("T1_3G", psg.Accession);
            Assert.False(psg.IsContaminant);
            Assert.AreEqual("MSSSSSSSSSSS", psg.BaseSequence);
        }

        [Test]
        public void test_protein_grouping_by_sequence_contaminant()
        {
            DatabaseReference d1 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            DatabaseReference d2 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            DatabaseReference d3 = new DatabaseReference("GO", ":", new List<Tuple<string, string>> { new Tuple<string, string>("term", "P:") });
            GoTerm g1 = new GoTerm(d1);
            GoTerm g2 = new GoTerm(d1);
            GoTerm g3 = new GoTerm(d1);
            ProteinWithGoTerms p1 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T1", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { d1 }, new List<GoTerm> { g1 });
            ProteinWithGoTerms p2 = new ProteinWithGoTerms("MSSSSSSSSSSS", "T2", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, false, new List<DatabaseReference> { d2 }, new List<GoTerm> { g2 });
            ProteinWithGoTerms p3 = new ProteinWithGoTerms("MCSSSSSSSSSS", "T3", new List<Tuple<string, string>> { new Tuple<string, string>("", "") }, new Dictionary<int, List<Modification>>(), new int?[] { 0 }, new int?[] { 0 }, new string[] { "" }, "T2", "T3", true, true, new List<DatabaseReference> { d3 }, new List<GoTerm> { g3 });
            ProteinSequenceGroup psg = new ProteinSequenceGroup(new List<ProteinWithGoTerms> { p1, p2, p3 }.OrderByDescending(p => p.IsContaminant ? 1 : 0));
            Assert.AreEqual(3, psg.GoTerms.Count());
            Assert.AreEqual(3, psg.GeneNames.Count());
            Assert.AreEqual("T3_3G", psg.Accession);
            Assert.True(psg.IsContaminant);
            Assert.AreEqual("MCSSSSSSSSSS", psg.BaseSequence);
        }
    }
}
