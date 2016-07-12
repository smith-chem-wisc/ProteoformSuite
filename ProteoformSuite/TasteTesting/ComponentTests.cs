using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TasteTesting
{
    [TestClass]
    public class ComponentTests
    {
        [TestMethod]
        public void FindNeuCodePairs_withEmptyComponentList()
        {
            List<ProteoformSuite.Component> c_list = new List<ProteoformSuite.Component>(); //arrange
            ProteoformSuite.Lollipop.find_neucode_pairs(c_list); //act
            Assert.AreEqual(0, ProteoformSuite.Lollipop.raw_neucode_pairs.Count); //assert
        }
    }
}
