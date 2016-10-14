using System;

namespace ProteoformSuiteInternal
{
    static class RandomExtensions
    {

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
}
