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
        public double monoisotopic_mass { get; set; }
        public double theoretical_mass { get; set; }
        public string accession { get; set; }
        public string name { get; set; }

        public TopDownHit(string accession, string uniprot_id, string name, string sequence, int start_index, int stop_index, List<Ptm> modifications, double monoisotopic_mass, double theoretical_mass, int scan, double retention_time, string filename, double score)
        {
            this.accession = accession;
            this.uniprot_id = uniprot_id;
            this.name = name;
            this.sequence = sequence;
            this.start_index = start_index;
            this.stop_index = stop_index;
            this.ptm_list = modifications;
            this.monoisotopic_mass = monoisotopic_mass;
            this.theoretical_mass = theoretical_mass;
            this.scan = scan;
            this.retention_time = retention_time;
            this.filename = filename;
            this.score = score;
        }
    }
}
