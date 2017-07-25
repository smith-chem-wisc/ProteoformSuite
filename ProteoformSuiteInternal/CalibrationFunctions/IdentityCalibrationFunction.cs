using System.Collections.Generic;
using System.Text;

namespace ProteoformSuiteInternal
{
    public class IdentityCalibrationFunction : CalibrationFunction
    {
        #region Internal Methods

        internal override double Predict(double[] t)
        {
            return 0;
        }

        internal override void Train<LabeledDataPoint>(IEnumerable<LabeledDataPoint> trainingList)
        {
        }

        #endregion Internal Methods

    }
}