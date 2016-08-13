using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class InputFile
    {
        public int biological_replicate { get; set; } = 1;
        public int technical_replicate { get; set; } = 1;
        public string condition { get; set; } = "no_condition";

        public bool matchingCalibrationFile { get; set; }
        public string path { get; set; }
        public string filename { get; set; }
        public string extension { get; set; }
        public inputFileType inputFileType { get; set; }
        public label lbl { get; set; }
    }

    public enum inputFileType
    {
        id,
        quant,
        calibration
    }

    public enum label
    {
        neuCode,
        unlabeled
    }
}
