using NUnit.Framework; 
using System;
using System.Collections.Generic;
using System.Linq;
using ProteoformSuiteInternal;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    [TestFixture]
    class TestAggregateTdHits
    {
        [Test]
        public void MyStupidTest()
        {
            SaveState.lollipop.top_down_hits = new List<TopDownHit>();
        }
    }
}
