using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;

namespace Test
{
    [TestFixture]
    class TestGeneName
    {
        [Test]
        public void test_construct_genename()
        {
            Tuple<string, string> gene_name = new Tuple<string, string>("primary", "ABC");
            Tuple<string, string> ordered_name = new Tuple<string, string>("ordered locus", "ABCD");
            Tuple<string, string> ordered_name2 = new Tuple<string, string>("ordered locus", "ABCD2");
            Tuple<string, string> gene_name2 = new Tuple<string, string>("primary", "ABCDE");
            List<Tuple<string, string>> gene_names = new List<Tuple<string, string>> { gene_name, ordered_name, ordered_name2, gene_name2 };

            GeneName g = new GeneName(gene_names);
            Assert.AreEqual(gene_name.Item2, g.primary);
            Assert.AreEqual(ordered_name.Item2, g.ordered_locus);

            //Getting preferred name
            Assert.AreEqual(gene_name.Item2, g.get_prefered_name(Lollipop.gene_name_labels[0]));
            Assert.AreEqual(ordered_name.Item2, g.get_prefered_name(Lollipop.gene_name_labels[1]));
            Assert.AreEqual(gene_name.Item2, g.get_prefered_name(""));
        }

        [Test]
        public void test_construct_genename_from_others()
        {
            Tuple<string, string> gene_name = new Tuple<string, string>("primary", "ABC");
            GeneName g = new GeneName(new List<Tuple<string, string>> { gene_name });
            Tuple<string, string> ordered_name = new Tuple<string, string>("ordered locus", "ABCD");
            GeneName h = new GeneName(new List<Tuple<string, string>> { ordered_name });
            Tuple<string, string> ordered_name2 = new Tuple<string, string>("ordered locus", "ABCD2");
            GeneName i = new GeneName(new List<Tuple<string, string>> { ordered_name2 });
            Tuple<string, string> gene_name2 = new Tuple<string, string>("primary", "ABCDE");
            GeneName j = new GeneName(new List<Tuple<string, string>> { gene_name2 });
            List<GeneName> gene_names = new List<GeneName> { g,h,i,j };

            GeneName k = new GeneName(gene_names);
            Assert.AreEqual(gene_name.Item2, k.primary);
            Assert.AreEqual(ordered_name.Item2, k.ordered_locus);
        }
    }
}
