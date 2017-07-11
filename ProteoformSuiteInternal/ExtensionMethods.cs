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
            return k == 0 ? new[] { new T[0] } :
              elements.SelectMany((e, i) =>
                elements.Skip(i + 1).Combinations(k - 1).Select(c => (new[] { e }).Concat(c)));
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

        public static void Shuffle<T>(this IList<T> list) //this randomly shuffles a list. useful for permutations
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                //int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                int k = SafeRandom.GetNext(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
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
    }
}
