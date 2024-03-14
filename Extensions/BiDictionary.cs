using System;
using System.Collections.Generic;

namespace Extensions
{
    /// <summary>
    /// https://stackoverflow.com/questions/255341/getting-key-of-value-of-a-generic-dictionary#255630
    /// </summary>
    /// <typeparam name="TFirst"></typeparam>
    /// <typeparam name="TSecond"></typeparam>
    [Serializable]
    public class BiDictionary<TFirst, TSecond>
    {
        IDictionary<TFirst, TSecond> firstToSecond = new Dictionary<TFirst, TSecond>();
        IDictionary<TSecond, TFirst> secondToFirst = new Dictionary<TSecond, TFirst>();

        public void Add(TFirst first, TSecond second)
        {
            if (firstToSecond.ContainsKey(first) ||
                secondToFirst.ContainsKey(second))
            {
                throw new ArgumentException("Duplicate first or second");
            }
            firstToSecond.Add(first, second);
            secondToFirst.Add(second, first);
        }

        public void Remove(TFirst first, TSecond second)
        {
            if(!firstToSecond.ContainsKey(first) || !secondToFirst.ContainsKey(second))
            {
                throw new ArgumentException("first or second entry not contained in dictionaries");
            }
            firstToSecond.Remove(first);
            secondToFirst.Remove(second);
        }

        public bool TryGetByFirst(TFirst first, out TSecond second)
        {
            return firstToSecond.TryGetValue(first, out second);
        }

        public bool TryGetBySecond(TSecond second, out TFirst first)
        {
            return secondToFirst.TryGetValue(second, out first);
        }

        public TFirst this[TSecond second]
        {
            get
            {
                TFirst tmp;
                if (TryGetBySecond(second, out tmp)) return tmp;
                throw new KeyNotFoundException(string.Format("OperatorNodeMap does not contain key: {0}", tmp));
            }         
        }

        public TSecond this[TFirst first]
        {
            get
            {
                TSecond tmp;
                if (TryGetByFirst(first, out tmp)) return tmp;
                throw new KeyNotFoundException(string.Format("OperatorNodeMap does not contain key: {0}", tmp));
            }
        }

        public IEnumerable<TFirst> Keys()
        {
            foreach (var first in firstToSecond.Keys) yield return first;
        }

        public IEnumerable<TSecond> Values()
        {
            foreach (var second in secondToFirst.Keys) yield return second;
        }
    }
}
