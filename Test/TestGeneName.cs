using NUnit.Framework;
using System;
using Proteomics;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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
        }
    }
}
