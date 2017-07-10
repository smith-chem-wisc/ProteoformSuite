using System;
using ProteoformSuiteInternal;

namespace ProteoformSuiteGUI
{
    public class PeakAcceptabilityChangedEventArgs : EventArgs
    {
        public bool IsPeakAcceptable { get; }
        public DeltaMassPeak Peak { get; }

        public PeakAcceptabilityChangedEventArgs(bool IsPeakAcceptable, DeltaMassPeak Peak)
        {
            this.IsPeakAcceptable = IsPeakAcceptable; //True if peak is acceptable
            this.Peak = Peak;
        }
    }
}
