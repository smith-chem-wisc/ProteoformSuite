using NUnit.Framework;
using ProteoformSuiteInternal;
using System.Collections.Generic;
using System.Linq;

namespace Test
{
    [TestFixture]
    class TestExtensions
    {
        [Test]
        public void test_distinct_by()
        {
            Proteoform a = new Proteoform("a");
            Proteoform b = new Proteoform("b");
            Proteoform c = new Proteoform("c");
            Proteoform c1 = new Proteoform("c");
            List<Proteoform> z = new List<Proteoform> { a, b, c, c1 };
            z.DistinctBy(p => p.accession);
            Assert.AreEqual(4, z.Count);
            z = z.DistinctBy(p => p.accession).ToList();
            Assert.AreEqual(3, z.Count);
        }

        [Test]
        public void test_slice()
        {
            int[] a = Enumerable.Range(0, 10).ToArray();
            int[] b = a.Slice(3, 4);
            Assert.AreEqual(new int[] { 3, 4, 5, 6 }, b);
        }

        enum test_enum
        {
            one,
            two
        }

        [Test]
        public void test_enumuntil()
        {
            Assert.AreEqual(2, ExtensionMethods.EnumUntil.GetValues<test_enum>().Count());
        }

        [Test]
        public void test_filter()
        {
            List<object> asdf = new List<object>();
            asdf.Add(ConstructorsForTesting.get_modWithMass("asdf", 0));
            asdf.Add(ConstructorsForTesting.get_modWithMass("asd", 0));
            asdf.Add(ConstructorsForTesting.get_modWithMass("asdf", 0));
            asdf.Add(ConstructorsForTesting.InputFile(@"fake.txt", Labeling.NeuCode, Purpose.CalibrationIdentification, "", "", "1", "1", "1"));
            asdf.Add(ConstructorsForTesting.InputFile(@"ake.txt", Labeling.NeuCode, Purpose.CalibrationIdentification, "", "", "1", "1", "1"));
            Assert.AreEqual(5, asdf.Count);
            Assert.AreEqual(3, ExtensionMethods.filter(asdf, "f").Count());
        }
    }
}
