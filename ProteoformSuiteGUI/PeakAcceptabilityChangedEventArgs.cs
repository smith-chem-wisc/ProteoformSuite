using System;
using ProteoformSuiteInternal;

namespace ProteoformSuiteGUI
{
    public class PeakAcceptabilityChangedEventArgs : EventArgs
    {
        private bool _isPeakAcceptable;
        public bool IsPeakAcceptable
        {
            get
            {
                return this._isPeakAcceptable;
            }
        }

        private DeltaMassPeak _Peak;
        public DeltaMassPeak Peak
        {
            get
            {
                return this._Peak;
            }
        }

        public PeakAcceptabilityChangedEventArgs(bool IsPeakAcceptable, DeltaMassPeak Peak)
        {
            this._isPeakAcceptable = IsPeakAcceptable; //True if peak is acceptable
            this._Peak = Peak;
        }
    }
}
