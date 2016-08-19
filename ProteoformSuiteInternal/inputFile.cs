using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class InputFile
    {
        public bool matchingCalibrationFile { get; set; } = false;
        public Purpose matched_for_calibration { get; set; } // For calibration file listings

        // For quantitation files
        public int biological_replicate { get; set; } = 1;
        public int technical_replicate { get; set; } = 1;
        public string condition { get; set; } = "no_condition";

        public string path { get; set; }
        public string filename { get; set; }
        public string extension { get; set; }
        public Purpose purpose { get; set; }
        public Labeling label { get; set; }

        public InputFile(string path, string filename, string extension, Labeling label, Purpose purpose)
        {
            this.path = path;
            this.filename = filename;
            this.extension = extension;
            this.label = label;
            this.purpose = purpose;
        }
        public InputFile()
        { }
    }

    public enum Purpose
    {
        Identification,
        Quantification,
        Calibration,
        BottomUp,
        TopDown
    }

    public enum Labeling
    {
        NeuCode,
        Unlabeled
    }
}
