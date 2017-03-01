using System;
using System.Collections.Generic;
using System.Linq;
using Proteomics;
using System.Threading.Tasks;
using System.Threading;

namespace ProteoformSuiteInternal
{
    //public class Modification
    //{
    //    //Adapted from the class by the same name from Morpheus (http://cwenger.github.io/Morpheus) by Craig Wenger
    //    // unused but available public string pA, cF, lC, tR, kW, dR;
    //    public string description { get; set; } = "unmodified"; //ID
    //    public string accession { get; set; } = ""; //AC
    //    public string feature_type { get; set; } = ""; //FT
    //    public string position { get; set; } = ""; //PP
    //    public char[] target_aas { get; set; } = new char[0]; //TG
    //    public double monoisotopic_mass_shift { get; set; } = 0; //MM
    //    public double average_mass_shift { get; set; } = 0; //MA
    //    public Modification() // constructs an "un-Modification"
    //    { }
    //    public Modification(string description, string accession, string featureType,
    //        string position, char[] targetAAs, double monoisotopicMassShift, double averageMassShift)
    //    {
    //        this.description = description;
    //        this.accession = accession;
    //        this.feature_type = featureType;
    //        this.position = position;
    //        this.target_aas = targetAAs;
    //        this.monoisotopic_mass_shift = monoisotopicMassShift;
    //        this.average_mass_shift = averageMassShift;
    //    }

    //    public Modification(string description)
    //    {
    //        this.description = description;
    //    }

    //    public override string ToString()
    //    {
    //        return "Description=" + this.description + " Accession=" + this.accession +
    //            " FeatureType=" + this.feature_type + " MonisotopicMass=" + this.monoisotopic_mass_shift;
    //    }
    //}

    public class Ptm
    {
        public int position = -1;
        public ModificationWithMass modification = new ModificationWithMass("Unmodified", new Tuple<string, string>("N/A", "Unmodified"), null, ModificationSites.Any, 0, null, -1, null, null, null);
        public Ptm() // initializes an "un-Modification"
        { }
        public Ptm(int position, ModificationWithMass modification)
        {
            this.position = position;
            this.modification = modification;
        }
    }

    public class PtmSet
    {
        public double mass;
        private List<Ptm> _ptm_combination;
        public List<Ptm> ptm_combination
        {
            get { return this._ptm_combination; }
            set
            {
                this._ptm_combination = value;
                this.mass = value.Sum(ptm => ptm.modification.monoisotopicMass);
            }
        }

        public PtmSet(List<Ptm> unique_ptm_combination)
        {
            this.ptm_combination = unique_ptm_combination;
        }
    }

    public class PtmCombos
    {
        public List<Ptm> all_ptms;
        public PtmCombos(IDictionary<int, List<Modification>> ptm_data)
        {
            this.all_ptms = (
                from position in ptm_data.Keys
                from modification in ptm_data[position].OfType<ModificationWithMass>()
                select new Ptm(position, modification))
            .OrderBy(p => p.position).ToList();
        }

        //Gets unique-mass (given a tolerance) ptm combinations from all the possible ones without positional redundancy
        //given a maximum number of ptms allowed on a theoretical protein
        //Includes an empty combination with no PTMS
        public List<PtmSet> get_combinations(int num_ptms_needed)
        {
            List<PtmSet> combinations = all_unique_positional_combinations(num_ptms_needed);
            List<PtmSet> unique_mass_combinations = new List<PtmSet> { new PtmSet(new List<Ptm>()) }; //initialize with an "unmodified" ptmset

            //I tried speeding this up by removing all similar masses instead; it doesn't speed it up... AC170223
            foreach (PtmSet combination in combinations)
            {
                double lower_mass = combination.mass - Lollipop.ptmset_mass_tolerance;
                double upper_mass = combination.mass + Lollipop.ptmset_mass_tolerance;
                if (unique_mass_combinations.All(c => !(c.mass >= lower_mass && c.mass <= upper_mass)))
                    unique_mass_combinations.Add(combination);
            }

            return unique_mass_combinations;
        }

        //For each length up to the maximum number of ptms (or the max number of modifications in this list),
        //generate all the combinations, with the shortest combinations first, and with the modifications at the first positions first
        private List<PtmSet> all_unique_positional_combinations(int max_length)
        {
            max_length = Math.Min(max_length, this.all_ptms.Count);
            List<PtmSet> combos = new List<PtmSet>(
                from i in Enumerable.Range(1, max_length)
                from combination in unique_positional_combinations(i)
                //where combination.ptm_combination.Count == combination.ptm_combination.DistinctBy(c => c.position).Count() //where the length of the positions list is the same as the number of unique positions
                select combination
            );
            return combos;
        }

        //For each length up to the maximum number of ptms (or the max number of modifications in this list),
        //generate all the combinations, with the shortest combinations first, and with the modifications at the first positions first
        //private List<PtmSet> all_possible_combinations(int max_length)
        //{
        //    max_length = Math.Min(max_length, this.all_ptms.Count);
        //    List<PtmSet> combos = new List<PtmSet>(
        //        from i in Enumerable.Range(1, max_length)
        //        from combination in combinations(i)
        //        select combination
        //    );
        //    return combos;
        //}

        //Generates all combinations of a certain length with unique positions
        private IEnumerable<PtmSet> unique_positional_combinations(int combination_length)
        {
            Ptm[] result = new Ptm[combination_length];
            int prev_position = -2;
            Stack<int> stack = new Stack<int>();
            stack.Push(0);

            while (stack.Count > 0)
            {
                int result_index = stack.Count - 1;
                int mod_index = stack.Pop();
                Ptm value = this.all_ptms[mod_index];
                while (mod_index < this.all_ptms.Count)
                {
                    value = this.all_ptms[mod_index];
                    result[result_index] = value;
                    mod_index++;
                    if (prev_position == value.position) continue;
                    prev_position = value.position;
                    result_index++;
                    if (mod_index < this.all_ptms.Count)
                        stack.Push(mod_index);
                    if (result_index == combination_length)
                    {
                        Ptm[] destinationArray = new Ptm[combination_length];
                        Array.Copy(result, destinationArray, combination_length);
                        yield return new PtmSet(destinationArray.ToList());
                        prev_position = -2;
                        break;
                    }
                }
            }
        }

        ////Generates all the combinations of a certain length
        //private IEnumerable<PtmSet> combinations(int combination_length)
        //{
        //    Ptm[] result = new Ptm[combination_length];
        //    Stack<int> stack = new Stack<int>();
        //    stack.Push(0);

        //    while (stack.Count > 0)
        //    {
        //        int result_index = stack.Count - 1;
        //        int mod_index = stack.Pop();
        //        Ptm value = this.all_ptms[mod_index];
        //        while (mod_index < this.all_ptms.Count)
        //        {
        //            result[result_index] = this.all_ptms[mod_index];
        //            result_index++;
        //            mod_index++;
        //            if (mod_index < this.all_ptms.Count)
        //                stack.Push(mod_index);
        //            if (result_index == combination_length)
        //            {
        //                Ptm[] destinationArray = new Ptm[combination_length];
        //                Array.Copy(result, destinationArray, combination_length);
        //                yield return new PtmSet(destinationArray.ToList());
        //                break;
        //            }
        //        }
        //    }
        //}
    }
}
