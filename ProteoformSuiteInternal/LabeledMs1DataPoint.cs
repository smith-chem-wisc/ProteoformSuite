namespace ProteoformSuiteInternal
{
    //CALIBRATION
    public class LabeledMs1DataPoint
    {
        public readonly double mz;
        public readonly double retentionTime;
        public double logTotalIonCurrent;
        public double logInjectionTime;
        public readonly TopDownHit identification;
        public double massError { get; private set; }
        public double[] Inputs { get; private set; }

        public LabeledMs1DataPoint(double mz, double retentionTime, double logTotalIonCurrent, double logInjectionTime, double massError, TopDownHit identification)
        {
            this.mz = mz;
            this.retentionTime = retentionTime;
            this.logTotalIonCurrent = logTotalIonCurrent;
            this.logInjectionTime = logInjectionTime;
            this.massError = massError;
            this.identification = identification;
            Inputs = new double[] { mz, retentionTime };
        }
    }
}