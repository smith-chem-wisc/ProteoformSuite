using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProteoformSuiteInternal;

namespace TasteTesting
{
    [TestClass]
    public class ComponentTests
    {
        [TestMethod]
        public void FindNeuCodePairs_withEmptyComponentList()
        {
            List<ProteoformSuiteInternal.Component> c_list = new List<ProteoformSuiteInternal.Component>(); //arrange
            ProteoformSuiteInternal.Lollipop.find_neucode_pairs(c_list); //act
            Assert.AreEqual(0, ProteoformSuiteInternal.Lollipop.raw_neucode_pairs.Count); //assert
        }
    }
}
