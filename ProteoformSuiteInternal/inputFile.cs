using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class inputFile
    {
        public int biorep { get; set; } = 1;
        private int _techrep = 1;
        private string _condition = "no_condition";

        public bool matchingCalibrationFile { get; set; }
        public string path { get; set; }
        public string filename { get; set; }
        public string extension { get; set; }
        public inputFileType inputFileType { get; set; }
        public label lbl { get; set; }
        //public sampleCategory sample { get; set; }

        public int techrep
        {
            get
            {
                return _techrep;
            }
            set
            {
                _techrep = this.techrep;
            }
        }
        public string condition
        {
            get
            {
                return _condition;
            }
            set
            {
                _condition = this.condition;
            }
        }
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

    public class sampleCategory
    {
        private int _biorep = 1;
        private int _techrep = 1;
        private string _condition = "no_condition";

        public int biorep
        {
            get
            {
                return _biorep;
            }
            set
            {
                _biorep = this.biorep;
            }
        }
        public int techrep
        {
            get
            {
                return _techrep;
            }
            set
            {
                _techrep = this.techrep;
            }
        }
        public string condition
        {
            get
            {
                return _condition;
            }
            set
            {
                _condition = this.condition;
            }
        }
    }
}
