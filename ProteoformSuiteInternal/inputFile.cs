using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class InputFile
    {
        private static int instanceCounter;
        //private readonly int instanceId;
        private int instanceId;

        public int UniqueId
        {
            get { return this.instanceId; }
            set { this.instanceId = value; }
        }
        public ComponentReader reader = new ComponentReader();

        public bool matchingCalibrationFile { get; set; } = false;

        // For quantitation files
        public int biological_replicate { get; set; } = 1;
        public int fraction { get; set; } = 1;
        public int technical_replicate { get; set; } = 1;
        public string lt_condition { get; set; } = "lt_condition";
        public string hv_condition { get; set; } = "hv_condition";

        public double totalIntensity { get; set; } = 0;

        public string path { get; set; }
        public string filename { get; set; }
        public string extension { get; set; }
        public Purpose purpose { get; set; } //ID, Quant, Calib, Bottom-Up or Top-Down
        public Labeling label { get; set; }
        public TDSoftware td_software { get; set; } = TDSoftware.NRTDP;
        public bool targeted_td_result { get; set; } = false;

        public InputFile(string path, string filename, string extension, Labeling label, Purpose purpose)
        {
            this.path = path;
            this.filename = filename;
            this.extension = extension;
            this.label = label;
            this.purpose = purpose;
            this.instanceId = ++instanceCounter;
        }

        public InputFile(string completePath, Labeling label, Purpose purpose)
        {
            this.path = Path.GetDirectoryName(completePath);
            this.filename = Path.GetFileNameWithoutExtension(completePath);
            this.extension = Path.GetExtension(completePath);
            this.label = label;
            this.purpose = purpose;
            this.instanceId = ++instanceCounter;
        }

        public InputFile(string completePath, Labeling label, Purpose purpose, int biorep)
        {
            this.path = Path.GetDirectoryName(completePath);
            this.filename = Path.GetFileNameWithoutExtension(completePath);
            this.extension = Path.GetExtension(completePath);
            this.label = label;
            this.purpose = purpose;
            this.biological_replicate = biorep;
            this.instanceId = ++instanceCounter;
        }

        public InputFile(int uniqueID, string completePath, Labeling label, Purpose purpose)
        {
            this.instanceId = uniqueID;
            this.path = Path.GetDirectoryName(completePath);
            this.filename = Path.GetFileNameWithoutExtension(completePath);
            this.extension = Path.GetExtension(completePath);
            this.label = label;
            this.purpose = purpose;           
        }

        public InputFile(int uniqueID, bool matchingFile, int bioRep, int fraction, int techRep, string ltCond, string hvCond, string completePath, Labeling label, Purpose purpose)
        {
            this.instanceId = uniqueID;
            this.matchingCalibrationFile = matchingFile;
            this.biological_replicate = bioRep;
            this.fraction = fraction;
            this.technical_replicate = techRep;
            this.lt_condition = ltCond;
            this.hv_condition = hvCond;
            this.path = Path.GetDirectoryName(completePath);
            this.filename = Path.GetFileNameWithoutExtension(completePath);
            this.extension = Path.GetExtension(completePath);
            this.label = label;
            this.purpose = purpose;
        }

        public InputFile()
        { }
    }

    //for TD 
    public enum TDSoftware
    {
        ProSight,
        NRTDP //NU software
    }

    public enum Purpose
    {
        Identification,
        Quantification,
        Calibration,
        BottomUp,
        TopDown,
        RawFile
    }

    public enum Labeling
    {
        NeuCode,
        Unlabeled
    }
}
