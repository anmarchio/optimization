using System;
using System.Collections.Generic;
using System.Linq;

namespace Extensions
{
    public static class ListExtensions
    {

        /// <summary>
        /// returns a list containing all subsets of size allowedSubsetSize if allowedSubsetSize != -1, else returns all
        /// possible subsets (including the empty set)
        /// 
        /// adapted from: http://codeding.com/?article=12
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<List<T>> Subsets<T>(this List<T> list)
        {
            var retLists = new List<List<T>>();
            var totalSubsetCount = (int)Math.Pow(2, list.Count);

            for (int i = 0; i < totalSubsetCount; i++)
            {
                var newList = new List<T>();
                //if (allowedSubsetSize != -1 && OneBitCount(i) != allowedSubsetSize) continue;
                for(int bitIndex = 0; bitIndex < list.Count; bitIndex ++)
                {
                    if(GetBit(i, bitIndex) == 1)
                    {
                        newList.Add(list[bitIndex]);
                    }
                }
                retLists.Add(newList);
            }

            return retLists;
        }

        public static IEnumerable<IEnumerable<T>> GetPermutations<T>(this IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }

        private static int OneBitCount(int position)
        {
            var val = (int) Math.Pow(2, position);
            return Convert.ToString(val, 2).ToString().Count(x => x.Equals("1"));
        }

        private static int GetBit(int value, int position)
        {
            int bit = value & (int)Math.Pow(2, position);
            return (bit > 0 ? 1 : 0);
        }


        public static IEnumerable<T> Union<T>(this IEnumerable<IEnumerable<T>> lists)
        {
            var ret = lists.ElementAt(0);
            for (int i = 1; i < lists.Count(); i++)
                ret = ret.Union(lists.ElementAt(i));
            return ret;
        }

        /// <summary>
        /// use this enumerator only with using() so it gets disposed properly and doesn't linger on (as there is effectively a non-terminating while loop in there)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<T> Cycle<T>(this List<T> list)
        {
            for(int i = 0; true; i = (i + 1) % list.Count)
            {
                yield return list[i];
            }
            
        } 


        public static IEnumerable<A> Slice<A>(this IEnumerable<A> source, int from, int to)
        {
            return source.Take(to).Skip(from);
        }

        public static bool EntriesAreEqual<T>(this IEnumerable<T> listA, IEnumerable<T> listB, IEqualityComparer<T> comparer)
        {
            if (listA.Count() != listB.Count()) return false;

            foreach(var entry in listA)
            {
                if (!listB.Contains(entry, comparer)) return false;
            }

            return true;
        }

        public static bool EntriesAreEqual<T>(this IEnumerable<T> listA, IEnumerable<T> listB)
        {
            var comparer = EqualityComparer<T>.Default;
            return listA.EntriesAreEqual(listB, comparer);

        }


    }
}
