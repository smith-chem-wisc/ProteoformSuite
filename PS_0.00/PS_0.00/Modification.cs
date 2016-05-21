using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Adapted from the class by the same name from Morpheus (http://cwenger.github.io/Morpheus) by Craig Wenger
namespace PS_0._00
{
    public class Modification
    {
        // unused but available public string pA, cF, lC, tR, kW, dR;

        public string Description { get; set; } //ID
        public string Accession { get; set; } //AC
        public string FeatureType { get; set; } //FT
        public string Position { get; set; } //PP
        public char[] TargetAAs { get; set; } //TG
        public double MonoisotopicMassShift { get; set; } //MM
        public double AverageMassShift { get; set; } //MA

        public Modification(string description, string accession, string featureType, 
            string position, char[] targetAAs, double monoisotopicMassShift, double averageMassShift)
        {
            this.Description = description;
            this.Accession = accession;
            this.FeatureType = featureType;
            this.Position = position;
            this.TargetAAs = targetAAs;
            this.MonoisotopicMassShift = monoisotopicMassShift;
            this.AverageMassShift = averageMassShift;
        }

        public override string ToString()
        {
            return "Description=" + this.Description + " Accession=" + this.Accession + 
                " FeatureType=" + this.FeatureType + " MonisotopicMass=" + this.MonoisotopicMassShift;
        }
    }

    public class OneUniquePtmGroup
    {
        private List<string> _unique_ptm_combination;
        private double _mass;

        public OneUniquePtmGroup(double mass, List<string> unique_ptm_combination)
        {
            _mass = mass;
            _unique_ptm_combination = unique_ptm_combination;
        }

        public OneUniquePtmGroup()
        {

        }

        public List<string> unique_ptm_combinations
        {
            get { return _unique_ptm_combination; }
            set { _unique_ptm_combination = value; }
        }

        public double mass
        {
            get { return _mass; }
            set { _mass = value; }
        }
    }

    class AllUniquePtmGroups
    {
        private List<OneUniquePtmGroup> _all_unique_ptm_groups;
        public List<OneUniquePtmGroup> unique_ptm_groups
        {
            get { return _all_unique_ptm_groups; }
            set { _all_unique_ptm_groups = value; }
        }
    }

    class PtmCombos
    {

        public List<OneUniquePtmGroup> combos(int numPtmsNeeded, Dictionary<string, Modification> uniprotModificationTable, Dictionary<int, List<string>> somePtmData)
        {

            Dictionary<int, Dictionary<int, string>> sortedProteinPTMs = new Dictionary<int, Dictionary<int, string>>();

            sortedProteinPTMs = GetAnOrganizedList(somePtmData);//(index, dict(pos,ptm))

            List<int[]> aPC = AllPossibleCombinations(numPtmsNeeded, sortedProteinPTMs.Keys.ToArray().Max() + 1);

            List<int[]> uPC = UniquePositionCombinations(aPC, sortedProteinPTMs);

            List<int[]> uMC = UniqueMassCombinations(uPC, sortedProteinPTMs, uniprotModificationTable);

            List<OneUniquePtmGroup> aupg = new List<OneUniquePtmGroup>();

            aupg = GetGroups(uMC, sortedProteinPTMs, uniprotModificationTable);

            return aupg;
        }

        static List<OneUniquePtmGroup> GetGroups(List<int[]> uMC, Dictionary<int, Dictionary<int, string>> sPP, Dictionary<string, Modification> uniprotModificationTable)
        {
            //Console.Writeline("  CALL GetGroups:");
            List<OneUniquePtmGroup> oupg = new List<OneUniquePtmGroup>();


            //Console.Writeline("   CALL GetGroups  int[] in uMC count = "+uMC.Count);
            foreach (int[] c in uMC)
            {
                //Console.Writeline("    GetGroups uMC int[]: "+String.Join("; ",c));

                double[] p = new double[c.Length];
                List<string> ptms_in_one_combo = new List<string>();
                for (int i = 0; i < c.Length; i++)
                {
                    int myKey = sPP[c[i]].Keys.ToArray()[0];
                    Dictionary<int, string> posPTM = sPP[c[i]];
                    string ptm = posPTM[myKey];
                    ptms_in_one_combo.Add(ptm);
                    double mass = uniprotModificationTable[ptm].MonoisotopicMassShift;
                    p[i] = mass;
                }
                OneUniquePtmGroup one = new OneUniquePtmGroup();
                one.mass = p.Sum();
                one.unique_ptm_combinations = ptms_in_one_combo;
                //Console.Writeline("\t\tGetGroups--ptms_in_one_combo: " + String.Join("; ", ptms_in_one_combo));
                oupg.Add(one);
            }

            //Console.Writeline("   CALL GetGroups  oupg count = " + oupg.Count);

            foreach (OneUniquePtmGroup won in oupg)
            {
                foreach (string item in won.unique_ptm_combinations)
                {
                    //Console.Writeline("\t\tGetGroups--ptm combos to be returned: " + item);
                }

            }

            return oupg;
        }
        static List<int[]> UniqueMassCombinations(List<int[]> uPC, Dictionary<int, Dictionary<int, string>> sPP, Dictionary<string, Modification> uniprotModificationTable)
        {
            //Console.Writeline("** Call UniqueMassCombinations");
            List<int[]> uMC = new List<int[]>();
            List<double> allPosCat = new List<double>();

            foreach (int[] c in uPC)
            {
                //Console.Writeline("\tUniqueMassCombinations\tint[] c" + String.Join(" ",c));
                double[] each_ptm_mass_array = new double[c.Length];
                for (int i = 0; i < c.Length; i++)
                {
                    Dictionary<int, string> posPTM = new Dictionary<int, string>();

                    posPTM = sPP[c[i]];

                    int one_position = -1;
                    string one_ptm = null;

                    foreach (KeyValuePair<int, string> kvp in posPTM)
                    {
                        one_position = kvp.Key;
                        one_ptm = kvp.Value;
                    }

                    double one_ptm_mass = uniprotModificationTable[one_ptm].MonoisotopicMassShift;
                    each_ptm_mass_array[i] = one_ptm_mass;
                }

                double comboMass = each_ptm_mass_array.Sum();//sum of all ptm masses in one combination

                bool found = false;

                foreach (double set_mass in allPosCat)
                {
                    ////Console.Writeline("set_mass: {0} combo_mass: {1}",set_mass,comboMass);
                    if (comboMass >= (set_mass - 0.00001) && comboMass <= (set_mass + 0.00001))
                    {
                        ////Console.Writeline("Inside ---> set_mass: {0} combo_mass: {1}", set_mass, comboMass);
                        found = true;
                        break;
                    }
                }

                if (!(found))
                {
                    ////Console.Writeline("------combo_mass: {0}", comboMass);
                    allPosCat.Add(comboMass);
                    uMC.Add(c);
                }
            }
            return uMC;
        }
        static List<int[]> UniquePositionCombinations(List<int[]> aPC, Dictionary<int, Dictionary<int, string>> sPP)
        {
            //Console.Writeline("## Call UniquePositionsCombinations");

            List<int[]> uPC = new List<int[]>();
            List<string> allPosCat = new List<string>();

            foreach (int[] c in aPC)
            {
                //Console.Writeline("\t\t##UniquePositionCombinations\tint[] c" + String.Join(" ", c));

                int[] p = new int[c.Length];
                for (int i = 0; i < c.Length; i++)
                {
                    p[i] = sPP[c[i]].Keys.ToArray()[0];//this adds the amino acid position to the array
                }

                string pCat = String.Join(";", p);

                if (!(allPosCat.Contains(pCat)))
                {
                    allPosCat.Add(pCat);
                    uPC.Add(c);
                }
            }

            return uPC;
        }
        static List<int[]> AllPossibleCombinations(int subset, int fullset)
        {
            subset = Math.Min(subset, fullset);
            //Console.Writeline("$$$APC CALLED subset: {0}\tfullset: {1}", subset, fullset);

            List<int[]> combos = new List<int[]>();

            for (int i = 1; i <= subset; i++)
            {
                //Console.Writeline("   $$$APC subset loop");
                foreach (int[] c in Combinations(i, fullset))
                {
                    combos.Add(c.ToArray());
                    //Console.Writeline("\t\t\t$$$ APC int []" + String.Join(", ", c));
                }
            }
            return combos;
        }

        static IEnumerable<int[]> Combinations(int subset, int fullset)
        {
            int[] result = new int[subset];
            Stack<int> stack = new Stack<int>();
            stack.Push(0);

            while (stack.Count > 0)
            {
                int index = stack.Count - 1;
                int value = stack.Pop();

                while (value < fullset)
                {
                    result[index++] = value++;
                    stack.Push(value);
                    if (index == subset)
                    {
                        yield return result;
                        break;
                    }
                }
            }
        }

        static Dictionary<int, Dictionary<int, string>> GetAnOrganizedList(Dictionary<int, List<string>> pAP)
        {
            //Console.WriteLine("GAOL CALLED");

            Dictionary<int, Dictionary<int, string>> sorted = new Dictionary<int, Dictionary<int, string>>();

            int count = 0;

            var keyList = pAP.Keys.ToList();
            keyList.Sort();

            foreach (var key in keyList)
            {
                pAP[key].Sort();
                foreach (string ptm in pAP[key])
                {
                    Dictionary<int, string> oneMod = new Dictionary<int, string>();
                    oneMod.Add(key, ptm);
                    sorted.Add(count, oneMod);
                    //Console.WriteLine("key: {0}\tptm: {1}\tcount: {2}", key, ptm, count);
                    count++;
                }
            }
            return sorted;
        }

    }
}
