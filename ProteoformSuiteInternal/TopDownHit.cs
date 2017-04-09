using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class TopDownHit
    {
        public MsScan ms_scan { get; set; }
        public int scan { get; set; }
        public double retention_time { get; set; }
        public string filename { get; set; }
        public string uniprot_id { get; set; }
        public string sequence { get; set; }
        public int start_index { get; set; }
        public int stop_index { get; set; }
        public List<Ptm> ptm_list { get; set; } = new List<Ptm>();
        public double theoretical_mass { get; set; }
        public string accession { get; set; }
        public string name { get; set; }

        public double reported_mass { get; set; } //reported in TD results file
        public double corrected_mass { get; set; } //calibrated mass

        public int charge { get; set; }
        public double mass_error { get; set; }
        public double mz { get; set; }
        public double intensity { get; set; } //precursor ion intensity
        public bool targeted { get; set; }
        public InputFile file { get; set; }

        public TopDownHit(InputFile file, string accession, string uniprot_id, string name, string sequence, int start_index, int stop_index, List<Ptm> modifications, double reported_mass, double theoretical_mass, int scan, double retention_time, string filename, bool targeted)
        {
            this.file = file;
            this.accession = accession;
            this.uniprot_id = uniprot_id;
            this.name = name;
            this.sequence = sequence;
            this.start_index = start_index;
            this.stop_index = stop_index;
            this.ptm_list = modifications;
            this.reported_mass = reported_mass;
            this.corrected_mass = reported_mass;
            this.theoretical_mass = theoretical_mass;
            this.scan = scan;
            this.retention_time = retention_time;
            this.filename = filename;
            this.targeted = targeted;
        }

        public bool same_ptm_hits(TopDownHit root)
        {
            if (this.ptm_list.Count == root.ptm_list.Count)
            {
                foreach (Ptm mod in this.ptm_list)
                {
                    if (root.ptm_list.Where(p => p.modification == mod.modification && p.position == mod.position).Count() == 1) continue;
                    else return false;
                }
                return true;
            }
            else return false;
        }

        public double get_mass_error(double theoretical, double observed)
        {
            return (observed - theoretical_mass) - Math.Round(observed - theoretical, 0);
        }
    }

    //CALIBRATION
    public class TrainingPoint 
    {
        public DataPoint datapoint;
        public double label;

        public TrainingPoint(DataPoint t, double label)
        {
            datapoint = t;
            this.label = label;
        }
    }

    public class DataPoint
    {
        public double mz;
        public double rt;
        public int msnOrder;
        public int SelectedIonGuessChargeStateGuess;
        public double IsolationMZ;
        public double relativeMZ;
        public string filename;

        public DataPoint(double mz, double rt, int msnOrder, string filename, int SelectedIonGuessChargeStateGuess = 0, double IsolationMZ = 0, double relativeMZ = 0)
        {
            this.mz = mz;
            this.rt = rt;
            this.msnOrder = msnOrder;
            this.SelectedIonGuessChargeStateGuess = SelectedIonGuessChargeStateGuess;
            this.IsolationMZ = IsolationMZ;
            this.relativeMZ = relativeMZ;
            this.filename = filename;
        }
    }

    public class LabeledDataPoint
    {
        public double[] inputs;
        public double output;

        public LabeledDataPoint(double[] v1, double v2)
        {
            inputs = v1;
            output = v2;
        }
    }



}
