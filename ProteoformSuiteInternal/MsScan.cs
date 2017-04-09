using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class MsScan
    {
        public int scan_number { get; set; }
        public string filename { get; set; }
        public double retention_time { get; set; }
        public double injection_time { get; set; }
        public double TIC { get; set; }
        public int ms_order { get; set; }
        public double[] noises { get; set; }
        public double[] peak_x { get; set; }
        public double[] peak_y { get; set; }
        public double lock_mass_shift { get; set; } = 0.0;
        public double precursor_mz { get; set; }

        public MsScan(int ms_order, int scan_number, string filename, double retention_time, double injection_time, double TIC)
        {
            this.ms_order = ms_order;
            this.scan_number = scan_number;
            this.filename = filename;
            this.retention_time = retention_time;
            this.injection_time = injection_time;
            this.TIC = TIC;
            this.peak_x = peak_x;
            this.peak_y = peak_y;
            this.noises = noises;
        }
    }
}
