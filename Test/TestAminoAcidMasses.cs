using NUnit.Framework;
using ProteoformSuiteInternal;

namespace Test
{
    [TestFixture]

    public class TestAminoAcidMasses
    {

        [Test]
        public void TestAminoAcidMassesConstructor()
        {
            var MassesList1 = new AminoAcidMasses(true, false, true, false);
            Assert.AreEqual(131.040485, MassesList1.AA_Masses['M']);
            Assert.AreEqual(160.030649, MassesList1.AA_Masses['C']);

            var MassesList2 = new AminoAcidMasses(false, false, true, false);
            Assert.AreEqual(131.040485, MassesList2.AA_Masses['M']);
            Assert.AreEqual(103.009185, MassesList2.AA_Masses['C']);
        }

        [Test]
        public void TestAminoAcidMassesDefaultLysine()
        {
            var MassesList1 = new AminoAcidMasses(true, false, true, false);
            Assert.AreEqual(136.109162, MassesList1.AA_Masses['K']);
            var MassesList2 = new AminoAcidMasses(true, true, false, false);
            Assert.Less(MassesList2.AA_Masses['K'], 136.109162);
            var MassesList3 = new AminoAcidMasses(true, false, false, true);
            Assert.Greater(MassesList3.AA_Masses['K'], 136.109162);
        }
    }
}
