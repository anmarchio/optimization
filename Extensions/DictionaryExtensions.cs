using System.Collections.Generic;

namespace Extensions
{
    public static class DictionaryExtensions
    {
        
        /// <summary>
        /// only works if each value is unique
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <returns></returns>
        public static Dictionary<TValue, TKey> Invert<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        {
            var inverted = new Dictionary<TValue, TKey>();

            foreach(var pair in dict)
            {
                inverted.Add(pair.Value, pair.Key);
            }
            return inverted;
        }
    }
}
