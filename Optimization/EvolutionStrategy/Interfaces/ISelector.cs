using System.Collections.Generic;
using Optimization.Fitness;

namespace Optimization.EvolutionStrategy.Interfaces
{
    public interface ISelector : ICopyable
    {
        IIndividual Select(List<IIndividual> individuals);
        List<IIndividual> Select(List<IIndividual> population, int number);

        FitnessConfiguration FitnessConfiguration { get; set; }
    }
}
