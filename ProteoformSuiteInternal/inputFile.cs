using System.IO;

namespace ProteoformSuiteInternal
{
    public class InputFile
    {

        #region Private Fields

        private static int instanceCounter;

        private int instanceId;

        #endregion Private Fields

        #region Public Properties

        public int UniqueId
        {
            get { return this.instanceId; }
            set { this.instanceId = value; }
        }


        //For all files
        public string complete_path { get; set; }
        public string directory { get; set; }
        public string filename { get; set; }
        public string extension { get; set; }
        public Purpose purpose { get; set; }


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
        public bool targeted_td_result { get; set; } = false;

        #endregion Public Properties

        #region Public Constructors

        public InputFile(string complete_path, Purpose purpose)
        {
            this.complete_path = complete_path;
            this.directory = Path.GetDirectoryName(complete_path);
            this.filename = Path.GetFileNameWithoutExtension(complete_path);
            this.extension = Path.GetExtension(complete_path);
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

        #endregion Public Constructors
    }

    public enum Purpose
    {
        Identification,
        Quantification,
        BottomUp,
        TopDown,
        RawFile,
        CalibrationIdentification,
        CalibrationTopDown,
        ProteinDatabase,
        PtmList
    }

    public enum Labeling
    {
        NeuCode,
        Unlabeled
    } 
}
