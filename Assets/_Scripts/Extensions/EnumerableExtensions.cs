using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace _Scripts.Extensions
{
    public static class EnumerableExtensions
    {
        public static T GetRandom<T>(this List<T> collection)
        {
            return !collection.Any() ? default : collection[Random.Range(0, collection.Count)];
        }

        public static T GetRandom<T>(this T[] collection)
        {
            return !collection.Any() ? default : collection[Random.Range(0, collection.Length)];
        }
    }
}