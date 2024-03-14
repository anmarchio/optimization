using System;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Terminators
{
    /// <summary>
    /// Terminates the evolutionary process after a specific amount of generations is reached.
    /// </summary>
    [Serializable]
    public class GenerationCountTerminator : ITerminator
    {
        public int GenerationsMaximum { get; private set; }

        public GenerationCountTerminator(int maxGenerations)
        {
            GenerationsMaximum = maxGenerations;
        }

        public bool Terminate(EvolutionStrategy evolutionStrategy)
        {
            if (evolutionStrategy.CurrentGeneration < GenerationsMaximum) return false;
            return true;
        }
    }
}
