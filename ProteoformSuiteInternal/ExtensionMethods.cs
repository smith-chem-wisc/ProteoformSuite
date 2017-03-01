using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
    }

    //public static class ThreadSafeRandom
    //{
    //    [ThreadStatic]
    //    private static Random Local;

    //    public static Random ThisThreadsRandom
    //    {
    //        get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
    //    }
    //}

    public class SafeRandom : Random
    {
        private const int PoolSize = 2048;

        private static readonly Lazy<RandomNumberGenerator> Rng =
            new Lazy<RandomNumberGenerator>(() => new RNGCryptoServiceProvider());

        private static readonly Lazy<object> PositionLock =
            new Lazy<object>(() => new object());

        private static readonly Lazy<byte[]> Pool =
            new Lazy<byte[]>(() => GeneratePool(new byte[PoolSize]));

        private static int bufferPosition;

        //public static int GetNext()
        //{
        //    while (true)
        //    {
        //        var result = (int)(GetRandomUInt32() & int.MaxValue);

        //        if (result != int.MaxValue)
        //        {
        //            return result;
        //        }
        //    }
        //}

        public static int GetNext(int maxValue)
        {
            if (maxValue < 1)
            {
                throw new ArgumentException(
                    "Must be greater than zero.",
                    "maxValue");
            }
            return GetNext(0, maxValue);
        }

        public static int GetNext(int minValue, int maxValue)
        {
            const long Max = 1 + (long)uint.MaxValue;

            if (minValue >= maxValue)
            {
                throw new ArgumentException(
                    "minValue is greater than or equal to maxValue");
            }

            long diff = maxValue - minValue;
            var limit = Max - (Max % diff);

            while (true)
            {
                var rand = GetRandomUInt32();
                if (rand < limit)
                {
                    return (int)(minValue + (rand % diff));
                }
            }
        }

        //public static void GetNextBytes(byte[] buffer)
        //{
        //    if (buffer == null)
        //    {
        //        throw new ArgumentNullException("buffer");
        //    }

        //    if (buffer.Length < PoolSize)
        //    {
        //        lock (PositionLock.Value)
        //        {
        //            if ((PoolSize - bufferPosition) < buffer.Length)
        //            {
        //                GeneratePool(Pool.Value);
        //            }

        //            Buffer.BlockCopy(
        //                Pool.Value,
        //                bufferPosition,
        //                buffer,
        //                0,
        //                buffer.Length);
        //            bufferPosition += buffer.Length;
        //        }
        //    }
        //    else
        //    {
        //        Rng.Value.GetBytes(buffer);
        //    }
        //}

        //public static double GetNextDouble()
        //{
        //    return GetRandomUInt32() / (1.0 + uint.MaxValue);
        //}

        //public override int Next()
        //{
        //    return GetNext();
        //}

        //public override int Next(int maxValue)
        //{
        //    return GetNext(0, maxValue);
        //}

        //public override int Next(int minValue, int maxValue)
        //{
        //    return GetNext(minValue, maxValue);
        //}

        //public override void NextBytes(byte[] buffer)
        //{
        //    GetNextBytes(buffer);
        //}

        //public override double NextDouble()
        //{
        //    return GetNextDouble();
        //}

        private static byte[] GeneratePool(byte[] buffer)
        {
            bufferPosition = 0;
            Rng.Value.GetBytes(buffer);
            return buffer;
        }

        private static uint GetRandomUInt32()
        {
            uint result;
            lock (PositionLock.Value)
            {
                if ((PoolSize - bufferPosition) < sizeof(uint))
                {
                    GeneratePool(Pool.Value);
                }

                result = BitConverter.ToUInt32(
                    Pool.Value,
                    bufferPosition);
                bufferPosition += sizeof(uint);
            }

            return result;
        }
    }
}
