using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class TopDownHit
    {
        public int scan { get; set; }
        public double retention_time { get; set; }
        public string filename { get; set; }
        public double score { get; set; }
        public string uniprot_id { get; set; }
        public string sequence { get; set; }
        public int start_index { get; set; }
        public int stop_index { get; set; }
        public List<Ptm> ptm_list { get; set; } = new List<Ptm>();
        public double theoretical_mass { get; set; }
        public string accession { get; set; }
        public string name { get; set; }
        public Result_Set result_set { get; set; }

        public double reported_mass { get; set; } //reported in TD results file
        public double corrected_mass { get; set; } //calibrated mass

        public TopDownHit(string accession, string uniprot_id, string name, string sequence, int start_index, int stop_index, List<Ptm> modifications, double reported_mass, double theoretical_mass, int scan, double retention_time, string filename, double score, Result_Set result_set)
        {
            this.accession = accession;
            this.uniprot_id = uniprot_id;
            this.name = name;
            this.sequence = sequence;
            this.start_index = start_index;
            this.stop_index = stop_index;
            this.ptm_list = modifications;
            this.reported_mass = reported_mass;
            this.corrected_mass = corrected_mass; 
            this.theoretical_mass = theoretical_mass;
            this.scan = scan;
            this.retention_time = retention_time;
            this.filename = filename;
            this.score = score;
            this.result_set = result_set;
        }
    }

    public enum Result_Set
    {
       tight_absolute_mass, 
       find_unexpected_mods,
       biomarker
    }

}
