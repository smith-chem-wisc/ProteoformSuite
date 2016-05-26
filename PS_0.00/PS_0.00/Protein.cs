using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PS_0._00
{
    public class Protein
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

    public class ProteinSequenceGroups : Protein
    {

        public List<string> AccessionList { get; set; } // this is the list of accession numbers for all proteins that share the same sequence. the list gets alphabetical order


        public ProteinSequenceGroups(string accession, string name, string fragment, int begin, int end, string sequence, Dictionary<int, List<string>> positionsAndPtms) : base(accession, name, fragment, begin, end, sequence, positionsAndPtms)
        {
            this.AccessionList = AccessionList;
            this.Accession = accession; // the accession gets ammended with the number of entries in the list
            this.Name = name;
            this.Fragment = fragment;
            this.Begin = begin;
            this.End = end;
            this.Sequence = sequence;
            this.PositionsAndPtms = positionsAndPtms;
        }

        public Dictionary<int, List<string>> consolodatePositionsAndPtms(Dictionary<int, List<string>> conPAP, Dictionary<int, List<string>> pap)  // takes the ptms from all accession with the same sequence and creates one aggregated list
        {
            foreach (KeyValuePair<int, List<string>> entry in pap)
            {
                if (conPAP.ContainsKey(entry.Key))
                {
                    List<string> somePTMs = conPAP[entry.Key];
                    somePTMs.Concat(entry.Value);
                    somePTMs.Distinct();
                    somePTMs.Sort();
                    conPAP[entry.Key] = somePTMs;
                }
                else
                {
                    conPAP.Add(entry.Key, entry.Value);
                }
            }

            return conPAP;
        }

        public static List<string> uniqueProteinSequences(IEnumerable<Protein> proteins) // this finds all unique proteins sequences in the protein[]
        {
            return new List<string>(new HashSet<string>(proteins.Select(p => p.Sequence)).ToList());
        }

        public static ProteinSequenceGroups[] consolidateProteins(IEnumerable<Protein> proteins) // this creates the proteinsequencegorup[] which adds the accesion list field and changes the accesion name.
        {
            List<string> sequences = ProteinSequenceGroups.uniqueProteinSequences(proteins);
            ProteinSequenceGroups[] psgs = new ProteinSequenceGroups[sequences.Count];
            int counter = 0;
            foreach (string sequence in sequences)
            {
                List<string> accessionList = new List<string>();
                //ProteinSequenceGroups psg = new ProteinSequenceGroups();
                Dictionary<int, List<string>> conPAP = new Dictionary<int, List<string>>();

                string accession = "";
                string name = "";
                string fragment = "";
                int begin = -1;
                int end = -1;

                ProteinSequenceGroups psg = new ProteinSequenceGroups(accession, name, fragment, begin, end, "", conPAP);

                foreach (Protein protein in proteins)
                {
                    if (protein.Sequence == sequence)
                    {
                        accessionList.Add(protein.Accession);
                        conPAP = psg.consolodatePositionsAndPtms(conPAP, protein.PositionsAndPtms);
                        if (accessionList.Count() == 1 || string.Compare(protein.Accession, accession) < 1)
                        {
                            accession = protein.Accession;
                            name = protein.Name;
                            fragment = protein.Fragment;
                            begin = protein.Begin;
                            end = protein.End;
                        }
                    }
                }

                accessionList.Sort();

                psg.AccessionList = accessionList;
                psg.Accession = accessionList[0] + "_G" + accessionList.Count();
                psg.Sequence = sequence;
                psg.PositionsAndPtms = conPAP;

                psg.Name = name;
                psg.Fragment = fragment;
                psg.Begin = begin;
                psg.End = end;

                psgs[counter] = psg;

                counter++;
            }

            return psgs;
        }

        public static void printProteinGroupArray(ProteinSequenceGroups[] p) // doesn't really work, but it could. 
        {
            for (int i = 0; i < p.Length; i++)
            {

                string allAccessions = string.Join(", ", p[i].AccessionList);

                string proteinGroupFields = allAccessions + "\t" + p[i].Accession + "\t" + p[i].Name + "\t" + p[i].Fragment + "\t" + p[i].Begin + "\t" + p[i].End + "\t" + p[i].Sequence;
                Console.WriteLine(proteinGroupFields);
                foreach (KeyValuePair<int, List<string>> entry in p[i].PositionsAndPtms)
                {
                    Console.WriteLine("\tposition: " + entry.Key + " ptms: " + string.Join(", ", entry.Value));
                }

            }
            
        }

    }
}
