using System;
using System.Collections.Generic;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.CartesianGeneticProgramming
{
    public static class Extensions
    {
        public static T SelectRandom<T>(this IRandom random, List<T> list)
        {
            if (list.Count == 0) throw new Exception("Tried to select a random element from an empty list.");
            return list[random.Next(list.Count)];
        }
    }
}
