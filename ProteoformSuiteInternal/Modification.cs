using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public class Modification
    {
        //Adapted from the class by the same name from Morpheus (http://cwenger.github.io/Morpheus) by Craig Wenger
        // unused but available public string pA, cF, lC, tR, kW, dR;
        public string description { get; set; } = "unmodified"; //ID
        public string accession { get; set; } = ""; //AC
        public string feature_type { get; set; } = ""; //FT
        public string position { get; set; } = ""; //PP
        public char[] target_aas { get; set; } = new char[0]; //TG
        public double monoisotopic_mass_shift { get; set; } = 0; //MM
        public double average_mass_shift { get; set; } = 0; //MA
        public Modification() // constructs an "un-Modification"
        { }
        public Modification(string description, string accession, string featureType,
            string position, char[] targetAAs, double monoisotopicMassShift, double averageMassShift)
        {
            this.description = description;
            this.accession = accession;
            this.feature_type = featureType;
            this.position = position;
            this.target_aas = targetAAs;
            this.monoisotopic_mass_shift = monoisotopicMassShift;
            this.average_mass_shift = averageMassShift;
        }

        public override string ToString()
        {
            return "Description=" + this.description + " Accession=" + this.accession +
                " FeatureType=" + this.feature_type + " MonisotopicMass=" + this.monoisotopic_mass_shift;
        }
    }

    public class Ptm
    {
        public int position = -1;
        public Modification modification = new Modification();
        public Ptm() // initializes an "un-Modification"
        { }
        public Ptm(int position, Modification modification)
        {
            this.position = position;
            this.modification = modification;
        }
    }

    public class PtmSet
    {
        public double mass;
        private IEnumerable<Ptm> _ptm_combination;
        public IEnumerable<Ptm> ptm_combination
        {
            get { return this._ptm_combination; }
            set
            {
                this._ptm_combination = value;
                this.mass = value.Select(ptm => ptm.modification.monoisotopic_mass_shift).Sum();
            }
        }

        public PtmSet(IEnumerable<Ptm> unique_ptm_combination)
        {
            this.ptm_combination = unique_ptm_combination;
        }
    }

    public class PtmCombos
    {
        public List<Ptm> all_ptms;
        public PtmCombos(Dictionary<int, List<Modification>> ptm_data)
        {
            this.all_ptms = new List<Ptm>(
                from position in ptm_data.Keys
                from modification in ptm_data[position]
                select new Ptm(position, modification)
            );
        }

        //Gets unique-mass (given a tolerance) ptm combinations from all the possible ones without positional redundancy
        //given a maximum number of ptms allowed on a theoretical protein
        //Includes an empty combination with no PTMS
        public List<PtmSet> get_combinations(int num_ptms_needed)
        {
            List<PtmSet> combinations = all_possible_combinations(num_ptms_needed);
            List<PtmSet> unique_positional_combinations = combinations.Where(set => set.ptm_combination.Select(ptm => ptm.position).Count() //where the length of the positions list
                == new HashSet<int>(set.ptm_combination.Select(ptm => ptm.position)).Count).ToList(); //is the same as the number of unique positions
            List<PtmSet> unique_mass_combinations = new List<PtmSet>();
            foreach (PtmSet combination in unique_positional_combinations)
            {
                double lower_mass = combination.mass - Lollipop.ptmset_mass_tolerance;
                double upper_mass = combination.mass + Lollipop.ptmset_mass_tolerance;
                if (unique_mass_combinations.Where(c => c.mass >= lower_mass && c.mass <= upper_mass).Count() == 0)
                    unique_mass_combinations.Add(combination);
            }
            unique_mass_combinations.Add(new PtmSet(new List<Ptm>()));
            return unique_mass_combinations;
        }

        //For each length up to the maximum number of ptms (or the max number of modifications in this list),
        //generate all the combinations, with the shortest combinations first, and with the modifications at the first positions first
        private List<PtmSet> all_possible_combinations(int max_length)
        {
            max_length = Math.Min(max_length, this.all_ptms.Count);
            List<PtmSet> combos = new List<PtmSet>(
                from i in Enumerable.Range(1, max_length)
                from combination in combinations(i)
                select combination
            );
            return combos;
        }

        //Generates all the combinations of a certain length
        private IEnumerable<PtmSet> combinations(int combination_length)
        {
            Ptm[] result = new Ptm[combination_length];
            Stack<int> stack = new Stack<int>();
            stack.Push(0);

            while (stack.Count > 0)
            {
                int result_index = stack.Count - 1;
                int mod_index = stack.Pop();
                Ptm value = this.all_ptms[mod_index];
                while (mod_index < this.all_ptms.Count)
                {
                    result[result_index] = this.all_ptms[mod_index];
                    result_index++;
                    mod_index++;
                    if (mod_index < this.all_ptms.Count)
                        stack.Push(mod_index);
                    if (result_index == combination_length)
                    {
                        Ptm[] destinationArray = new Ptm[combination_length];
                        Array.Copy(result, destinationArray, combination_length);
                        yield return new PtmSet(destinationArray);
                        break;
                    }
                }
            }
        }
    }
}
