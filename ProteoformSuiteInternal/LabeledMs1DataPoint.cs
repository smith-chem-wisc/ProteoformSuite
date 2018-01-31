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
        public readonly TopDownHit identification;
        public double Label { get; private set; }
        public double[] Inputs { get; private set; }

        public LabeledMs1DataPoint(double mz, double retentionTime, double label, TopDownHit identification)
        {
            this.mz = mz;
            this.retentionTime = retentionTime;
            this.Label = label;
            this.identification = identification;
            Inputs = new double[] { mz, retentionTime };
        }
    }
}
