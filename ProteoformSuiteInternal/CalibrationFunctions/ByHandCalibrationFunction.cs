using System;
using System.Collections.Generic;

namespace ProteoformSuiteInternal
{
    internal class ByHandCalibrationFunction : CalibrationFunction
    {

        public ByHandCalibrationFunction()
        {
        }

        public override double Predict(double[] t)
        {
            return -t[1] / 200000;
        }

        public override void Train(IEnumerable<LabeledDataPoint> trainingList)
        {
        }
    }
}