using System.Collections.Generic;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Terminators
{
    public class MultipleTerminator : ITerminator
    {
        private List<ITerminator> Terminators { get; set; }
        /// <summary>
        /// Checks if _any_ of the terminators terminates and then terminates.
        /// </summary>
        /// <param name="evolutionStrategy"></param>
        /// <returns></returns>
        public MultipleTerminator(List<ITerminator> terminators)
        {
            Terminators = terminators;
        }
        public bool Terminate(EvolutionStrategy evolutionStrategy)
        {
            foreach(var terminator in Terminators)
            {
                if (terminator.Terminate(evolutionStrategy) == true) return true;
            }
            return false;
        }
    }
}
