using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS_0._00
{
    class Protein
    {
        public string Accession { get; set; }
        public string Name { get; set; }
        public string Fragment { get; set; }
        public int Begin { get; set; }
        public int End { get; set; }
        public string Sequence { get; set; }
        public Dictionary<int, List<string>> PositionsAndPtms { get; set; }

        public Protein(string accession, string name, string fragment, int begin, int end, string sequence, Dictionary<int, List<string>> positionsAndPtms)
        {
            this.Accession = accession;
            this.Name = name;
            this.Fragment = fragment;
            this.Begin = begin;
            this.End = end;
            this.Sequence = sequence;
            this.PositionsAndPtms = positionsAndPtms;
        }
        
        public override string ToString()
        {
            return this.Accession + "_" + this.Name + "_" + this.Fragment;
        }
    }
}
