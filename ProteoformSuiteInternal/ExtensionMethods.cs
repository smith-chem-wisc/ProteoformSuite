using System;
using System.Collections.Generic;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public static class ExtensionMethods
    {
        //Function to get random number
        private static readonly Random random = new Random();
        private static readonly object syncLock = new object();
        public static double RandomNumber()
        {
            lock (syncLock)
            { // synchronize
                return random.NextDouble();
            }
        }

        public static IEnumerable<IEnumerable<T>> Combinations<T>(this IEnumerable<T> elements, int k)//given an array of elements, it returns all combination sub arrays of length k
        {
            return k == 0 ? 
                new[] { new T[0] } :
                    elements.SelectMany((e, i) => elements.Skip(i + 1).Combinations(k - 1).Select(c => (new[] { e }).Concat(c)));
        }

        //public static List<T> ToNonNullList<T>(this IEnumerable<T> obj)
        //{
        //    return obj == null ? new List<T>() : obj.ToList();
        //}

        public static IEnumerable<object> filter(IEnumerable<object> some_list, string s)
        {
            return some_list.Where(f =>
                    f.GetType().GetProperties().Where(p => new Type[] { typeof(int), typeof(double), typeof(string), typeof(decimal), typeof(bool) }.Contains(p.PropertyType)).Any(i => i.GetValue(f).ToString().Contains(s)) ||
                    f.GetType().GetFields().Where(i => !i.IsLiteral && new Type[] { typeof(int), typeof(double), typeof(string), typeof(decimal), typeof(bool) }.Contains(i.FieldType)).Any(i => i.GetValue(f).ToString().Contains(s))
                );
        }

        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property) //this method can be used to make a distinct list of objects by one of the object properties
        {
            return items.GroupBy(property).Select(x => x.First());
        }

        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n);
                n--;
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        public static T[] Slice<T>(this T[] source, int index, int length)  // returns a copy of a hunk of an object array. 
        {
            T[] slice = new T[length];
            Array.Copy(source, index, slice, 0, length);
            return slice;
        }

        public static class EnumUntil
        {
            public static IEnumerable<T> GetValues<T>()
            {
                return Enum.GetValues(typeof(T)).Cast<T>();
            }
        }

        //https://msdn.microsoft.com/en-us/magazine/mt620016.aspx
        public static double Gauss(double z)
        {
            // input = z-value (-inf to +inf)
            // output = p under Standard Normal curve from -inf to z
            // e.g., if z = 0.0, function returns 0.5000
            // ACM Algorithm #209
            double y; // 209 scratch variable
            double p; // result. called 'z' in 209
            double w; // 209 scratch variable
            if (z == 0.0)
                p = 0.0;
            else
            {
                y = Math.Abs(z) / 2;
                if (y >= 3.0)
                {
                    p = 1.0;
                }
                else if (y < 1.0)
                {
                    w = y * y;
                    p = ((((((((0.000124818987 * w
                      - 0.001075204047) * w + 0.005198775019) * w
                      - 0.019198292004) * w + 0.059054035642) * w
                      - 0.151968751364) * w + 0.319152932694) * w
                      - 0.531923007300) * w + 0.797884560593) * y * 2.0;
                }
                else
                {
                    y = y - 2.0;
                    p = (((((((((((((-0.000045255659 * y
                      + 0.000152529290) * y - 0.000019538132) * y
                      - 0.000676904986) * y + 0.001390604284) * y
                      - 0.000794620820) * y - 0.002034254874) * y
                      + 0.006549791214) * y - 0.010557625006) * y
                      + 0.011630447319) * y - 0.009279453341) * y
                      + 0.005353579108) * y - 0.002141268741) * y
                      + 0.000535310849) * y + 0.999936657524;
                }
            }
            if (z > 0.0)
                return (p + 1.0) / 2;
            else
                return (1.0 - p) / 2;
        }

        //https://msdn.microsoft.com/en-us/magazine/mt620016.aspx
        public static double Student2T(double t, double df)
        {
            // for large integer df or double df
            // adapted from ACM algorithm 395
            // returns 2-tail p-value
            double n = df; // to sync with ACM parameter name
            double a, b, y;
            t = t * t;
            y = t / n;
            b = y + 1.0;
            if (y > 1.0E-6) y = Math.Log(b);
            a = n - 0.5;
            b = 48.0 * a * a;
            y = a * y;
            y = (((((-0.4 * y - 3.3) * y - 24.0) * y - 85.5) /
              (0.8 * y * y + 100.0 + b) + y + 3.0) / b + 1.0) *
              Math.Sqrt(y);
            return 2.0 * Gauss(-y); // ACM algorithm 209
        }
    }
}
