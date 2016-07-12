using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProteoformSuiteInternal
{
    public class Protein
    {
        public string accession { get; set; }
        public string name { get; set; }
        public string fragment { get; set; }
        public int begin { get; set; }
        public int end { get; set; }
        public string sequence { get; set; }
        public Dictionary<int, List<Modification>> ptms_by_position { get; set; }

        public Protein(string accession, string name, string fragment, int begin, int end, string sequence, Dictionary<int, List<Modification>> positionsAndPtms)
        {
            this.accession = accession;
            this.name = name;
            this.fragment = fragment;
            this.begin = begin;
            this.end = end;
            this.sequence = sequence;
            this.ptms_by_position = positionsAndPtms;
        }
        
        public override string ToString()
        {
            return this.accession + "_" + this.name + "_" + this.fragment;
        }
    }

    public class ProteinSequenceGroup : Protein
    {

        public List<string> accessionList { get; set; } // this is the list of accession numbers for all proteins that share the same sequence. the list gets alphabetical order

        public ProteinSequenceGroup(string accession, string name, string fragment, int begin, int end, string sequence, Dictionary<int, List<Modification>> positionsAndPtms) 
            : base(accession, name, fragment, begin, end, sequence, positionsAndPtms)
        { }
        public ProteinSequenceGroup(List<Protein> proteins) 
            : base (proteins[0].accession + "_G" + proteins.Count(), proteins[0].name, proteins[0].fragment, proteins[0].begin, proteins[0].end, proteins[0].sequence, proteins[0].ptms_by_position)
        {
            this.accessionList = proteins.Select(p => p.accession).ToList();
            HashSet<int> all_positions = new HashSet<int>(proteins.SelectMany(p => p.ptms_by_position.Keys).ToList());
            Dictionary<int, List<Modification>> ptms_by_position = new Dictionary<int, List<Modification>>();
            foreach (int position in all_positions)
                ptms_by_position.Add(position, proteins.Where(p => p.ptms_by_position.ContainsKey(position)).SelectMany(p => p.ptms_by_position[position]).ToList());
        }
    }
}
