using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Proteomics;

namespace ProteoformSuiteInternal
{
    public class PtmCombos
    {
        //Gets unique-mass (given a tolerance) ptm combinations from all the possible ones without positional redundancy
        //given a maximum number of ptms allowed on a theoretical protein
        //Includes an empty combination with no PTMS
        public static List<PtmSet> get_combinations(IDictionary<int, List<Modification>> ptm_data, int num_ptms_needed)
        {
            List<Ptm> all_ptms = (
                from position in ptm_data.Keys
                from modification in ptm_data[position].OfType<ModificationWithMass>()
                select new Ptm(position, modification))
            .OrderBy(p => p.position).ToList();

            List<PtmSet> combinations = all_unique_positional_combinations(all_ptms, num_ptms_needed);
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

        public static List<PtmSet> generate_all_ptmsets(int max_num_ptms, List<ModificationWithMass> mods)
        {
            List<PtmSet> sets = new List<PtmSet>();
            List<Ptm> unlocalized_ptms = mods.Select(m => new Ptm(-1, m)).Concat(new Ptm[] { new Ptm() }).ToList();
            Parallel.For(1, max_num_ptms + 1, ptm_set_length =>
            {
                lock (sets) sets.AddRange(combinations(unlocalized_ptms, ptm_set_length).ToList());
            });
            return sets;
        }


        // LOCALIZED MODIFICATION COMBINATIONS
        //For each length up to the maximum number of ptms (or the max number of modifications in this list),
        //generate all the combinations, with the shortest combinations first, and with the modifications at the first positions first
        private static List<PtmSet> all_unique_positional_combinations(List<Ptm> all_ptms, int max_length)
        {
            max_length = Math.Min(max_length, all_ptms.Count);
            List<PtmSet> combos = new List<PtmSet>(
                from i in Enumerable.Range(1, max_length)
                from combination in unique_positional_combinations(all_ptms, i)
                //where combination.ptm_combination.Count == combination.ptm_combination.DistinctBy(c => c.position).Count() //where the length of the positions list is the same as the number of unique positions
                select combination
            );
            return combos;
        }

        //Generates all combinations of a certain length with unique positions
        private static IEnumerable<PtmSet> unique_positional_combinations(List<Ptm> all_ptms, int combination_length)
        {
            Ptm[] result = new Ptm[combination_length];
            int prev_position = -2;
            Stack<int> stack = new Stack<int>();
            stack.Push(0);

            while (stack.Count > 0)
            {
                int result_index = stack.Count - 1;
                int mod_index = stack.Pop();
                Ptm value = all_ptms[mod_index];
                while (mod_index < all_ptms.Count)
                {
                    value = all_ptms[mod_index];
                    result[result_index] = value;
                    mod_index++;
                    if (prev_position == value.position) continue;
                    prev_position = value.position;
                    result_index++;
                    if (mod_index < all_ptms.Count)
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


        // UNLOCALIZED MODIFICATION COMBINATIONS
        private static List<PtmSet> all_possible_combinations(List<Ptm> all_ptms, int max_length)
        {
            max_length = Math.Min(max_length, all_ptms.Count);
            List<PtmSet> combos = new List<PtmSet>(
                from i in Enumerable.Range(1, max_length)
                from combination in combinations(all_ptms, i)
                select combination
            );
            return combos;
        }

        //Generates all the combinations of a certain length
        private static IEnumerable<PtmSet> combinations(List<Ptm> all_ptms, int combination_length)
        {
            Ptm[] result = new Ptm[combination_length];
            Stack<int> stack = new Stack<int>();
            stack.Push(0);

            while (stack.Count > 0)
            {
                int result_index = stack.Count - 1;
                int mod_index = stack.Pop();
                Ptm value = all_ptms[mod_index];
                while (mod_index < all_ptms.Count)
                {
                    result[result_index] = all_ptms[mod_index];
                    result_index++;
                    mod_index++;
                    if (mod_index < all_ptms.Count)
                        stack.Push(mod_index);
                    if (result_index == combination_length)
                    {
                        Ptm[] destinationArray = new Ptm[combination_length];
                        Array.Copy(result, destinationArray, combination_length);
                        yield return new PtmSet(destinationArray.ToList());
                        break;
                    }
                }
            }
        }
    }
}
