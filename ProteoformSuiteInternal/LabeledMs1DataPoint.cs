using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    //CALIBRATION
    public class LabeledMs1DataPoint : IHasInputsAndOutputs
    {
        public readonly double mz;
        public readonly double retentionTime;
        public readonly double totalIonCurrent;
        public readonly double injectionTime;
        public readonly TopDownHit identification;
        public double Label { get; private set; }
        public double[] Inputs { get; private set; }

        public LabeledMs1DataPoint(double mz, double retentionTime, double totalIonCurrent, double? injectionTime, double label, TopDownHit identification)
        {
            this.mz = mz;
            this.retentionTime = retentionTime;
            this.totalIonCurrent = totalIonCurrent;
            this.injectionTime = injectionTime ?? double.NaN;
            this.Label = label;
            this.identification = identification;
            Inputs = new double[] { mz, retentionTime, totalIonCurrent, this.injectionTime };
        }
    }
}
