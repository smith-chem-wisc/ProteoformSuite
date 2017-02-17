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

        public string complete_path { get; set; }
        public string directory { get; set; }
        public string filename { get; set; }
        public string extension { get; set; }
        public Purpose purpose { get; set; } //ID, Quant, Calib, Bottom-Up or Top-Down


        //For identification files
        public ComponentReader reader = new ComponentReader();
        public bool matchingCalibrationFile { get; set; } = false;
        public Labeling label { get; set; }

        // For quantitation files
        public int biological_replicate { get; set; } = 1;
        public int fraction { get; set; } = 1;
        public int technical_replicate { get; set; } = 1;
        public string lt_condition { get; set; } = "lt_condition";
        public string hv_condition { get; set; } = "hv_condition";

        //For database files
        public bool ContaminantDB { get; set; } = false;

        //For top-down files
        public TDProgram td_program { get; set; } = TDProgram.NRTDP;

        public InputFile(string complete_path, string directory, string filename, string extension, Labeling label, Purpose purpose)
        {
            this.complete_path = complete_path;
            this.directory = directory;
            this.filename = filename;
            this.extension = extension;
            this.label = label;
            this.purpose = purpose;
            this.instanceId = ++instanceCounter;
        }

        public InputFile(string complete_path, string directory, string filename, string extension, Purpose purpose)
        {
            this.complete_path = complete_path;
            this.directory = directory;
            this.filename = filename;
            this.extension = extension;
            this.purpose = purpose;
            this.instanceId = ++instanceCounter;
        }

        public InputFile(string completePath, Labeling label, Purpose purpose)
        {
            this.complete_path = completePath;
            this.directory = Path.GetDirectoryName(completePath);
            this.filename = Path.GetFileNameWithoutExtension(completePath);
            this.extension = Path.GetExtension(completePath);
            this.label = label;
            this.purpose = purpose;
            this.instanceId = ++instanceCounter;
        }

        public InputFile(string completePath, Labeling label, Purpose purpose, int biorep)
        {
            this.complete_path = completePath;
            this.directory = Path.GetDirectoryName(completePath);
            this.filename = Path.GetFileNameWithoutExtension(completePath);
            this.extension = Path.GetExtension(completePath);
            this.label = label;
            this.purpose = purpose;
            this.biological_replicate = biorep;
            this.instanceId = ++instanceCounter;
        }

        public InputFile(int uniqueID, string completePath, Labeling label, Purpose purpose)
        {
            this.complete_path = completePath;
            this.instanceId = uniqueID;
            this.directory = Path.GetDirectoryName(completePath);
            this.filename = Path.GetFileNameWithoutExtension(completePath);
            this.extension = Path.GetExtension(completePath);
            this.label = label;
            this.purpose = purpose;           
        }

        public InputFile(int uniqueID, bool matchingFile, int bioRep, int fraction, int techRep, string ltCond, string hvCond, string completePath, Labeling label, Purpose purpose)
        {
            this.complete_path = completePath;
            this.instanceId = uniqueID;
            this.matchingCalibrationFile = matchingFile;
            this.biological_replicate = bioRep;
            this.fraction = fraction;
            this.technical_replicate = techRep;
            this.lt_condition = ltCond;
            this.hv_condition = hvCond;
            this.directory = Path.GetDirectoryName(completePath);
            this.filename = Path.GetFileNameWithoutExtension(completePath);
            this.extension = Path.GetExtension(completePath);
            this.label = label;
            this.purpose = purpose;
        }

        public InputFile()
        { }
    }

    //for TD 
    public enum TDProgram
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
        ProteinDatabase,
        PtmList
    }

    public enum Labeling
    {
        NeuCode,
        Unlabeled
    }
}
