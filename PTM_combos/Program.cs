using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTM_combos
{


    class Program
    {
        

        static void Main(string[] args)
        {
            Dictionary<int, string[]> positionAndPtms = new Dictionary<int, string[]>();
            positionAndPtms = SomePtmData();// for one protein

            Dictionary<string, double> ptmAndMass = new Dictionary<string, double>();
            ptmAndMass = PTM_Masses();// lookup table for all ptm masses

            Dictionary<int, Dictionary<int, string>> sortedProteinPTMs = new Dictionary<int, Dictionary<int, string>>();

            sortedProteinPTMs = GetAnOrganizedList(positionAndPtms);//(index, dict(pos,ptm))

            foreach (KeyValuePair<int,string[]> kvp in  positionAndPtms)
            {
                foreach (string ptm in kvp.Value)
                {
                    Console.WriteLine("{0}\t{1}\t{2}",kvp.Key,ptm,ptmAndMass[ptm]);
                }
            }

            int maxPTMs = 3;

            int numPtmsNeeded = maxPTMs;

            List<int[]> aPC = AllPossibleCombinations(numPtmsNeeded,sortedProteinPTMs.Keys.ToArray().Max());

            Console.WriteLine("aPC Count: "+ aPC.Count);


            List<int[]> uPC = UniquePositionCombinations(aPC, sortedProteinPTMs);

            Console.WriteLine("uPC Count: "+ uPC.Count);

            foreach (int[] c in uPC)
            {
                Console.WriteLine(String.Join(" ", c));
            }

            List<int[]> uMC = UniqueMassCombinations(uPC, sortedProteinPTMs, ptmAndMass);

            Console.WriteLine("uMC Count: " + uMC.Count);

            foreach (int[] c in uMC)
            {
                Console.WriteLine(String.Join(" ", c));
            }

        }

        static List<string[]> GetUniqueGroups(List<string[]> pG)
        {
            return pG;// this is wrong
        }

        static List<int[]> UniqueMassCombinations(List<int[]> uPC, Dictionary<int, Dictionary<int, string>> sPP, Dictionary<string,double>pAM)
        {
            List<int[]> uMC = new List<int[]>();
            List<double> allPosCat = new List<double>();

            foreach (int[] c in uPC)
            {
                double[] p = new double[c.Length];
                for (int i = 0; i < c.Length; i++)
                {
                    int myKey = sPP[c[i]].Keys.ToArray()[0];
                    Dictionary<int, string> posPTM = sPP[c[i]];
                    string ptm = posPTM[myKey];
                    double mass = pAM[ptm];
                    p[i] = mass;  
                }

                double comboMass = p.Sum();

                //Console.WriteLine();
                if (!(allPosCat.Contains(comboMass)))
                {
                    allPosCat.Add(comboMass);
                    uMC.Add(c);
                }
            }
            return uMC;
        }
            static List<int[]> UniquePositionCombinations(List<int[]> aPC, Dictionary<int, Dictionary<int, string>> sPP)
        {
            List<int[]> uPC = new List<int[]>();
            List<string> allPosCat = new List<string>();

            foreach (int[] c in aPC)
            {
                int[] p = new int[c.Length];
                for (int i = 0; i < c.Length; i++)
                {
                    p[i] = sPP[c[i]].Keys.ToArray()[0];//this adds the amino acid position to the array
                    //Console.Write(p[i] + " ");
                }

                string pCat = String.Join("", p);

                //Console.WriteLine();
                if (!(allPosCat.Contains(pCat)))
                {
                    allPosCat.Add(pCat);
                    uPC.Add(c);
                }
            }

            return uPC;
        }
            static List<int[]>AllPossibleCombinations(int subset, int fullset)
        {
            List<int[]> combos = new List<int[]>();

            for (int i = 1; i <= subset; i++)
            {
                foreach (int[] c in Combinations(i, fullset))
                {
                    combos.Add(c.ToArray());
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

        static Dictionary<int, Dictionary<int, string>> GetAnOrganizedList(Dictionary<int, string[]> pAP)
        {
            Dictionary<int, Dictionary<int, string>> sorted = new Dictionary<int, Dictionary<int, string>>();

            int count = 0;

            var keyList = pAP.Keys.ToList();
            keyList.Sort();

            foreach (var key in keyList)
            {
                Array.Sort(pAP[key]);
                foreach (string ptm in pAP[key])
                {
                    Dictionary<int, string> oneMod = new Dictionary<int, string>();
                    oneMod.Add(key,ptm);
                    sorted.Add(count, oneMod);
                    count++;
                    Console.WriteLine("{0}\t{1}\t{2}",count,key,ptm);
                }
            }
            return sorted;
        }

        static Dictionary<string,double> PTM_Masses()
        {
            Dictionary<string, double> pM = new Dictionary<string, double>();

            pM.Add("A", 1.2);
            pM.Add("B", 2.3);
            pM.Add("C", 3.4);
            pM.Add("D", 4.5);
            pM.Add("E", 5.6);
            pM.Add("F", 6.7);
            pM.Add("G", 7.8);

            return pM;
        }

        static Dictionary<int,string[]> SomePtmData()
        {
            Dictionary<int, string[]> sD = new Dictionary<int, string[]>();

            string[] a = { "A", "B", "C" };
            string[] b = { "D", "B", "C" };
            string[] c = { "A", "E", "C" };
            string[] d = { "A", "B", "F" };
            string[] e = { "E", "F", "G" };
            int one = 17;
            int two = 28;
            int three = 31;
            int four = 46;
            int five = 59;
            sD.Add(one, a);
            sD.Add(two, b);
            sD.Add(three, c);
            sD.Add(four, d);
            sD.Add(five, e);
            return sD;
        }
    }
}
