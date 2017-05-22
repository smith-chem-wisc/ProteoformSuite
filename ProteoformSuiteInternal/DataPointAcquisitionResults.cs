using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    internal class DataPointAquisitionResults
    {

        #region Public Fields

        public int numMs1MassChargeCombinationsConsidered;
        public int numMs1MassChargeCombinationsThatAreIgnoredBecauseOfTooManyPeaks;

        #endregion Public Fields

        #region Public Properties

        public List<LabeledMs1DataPoint> Ms1List { get; set; }
        #endregion Public Properties
    }
}
