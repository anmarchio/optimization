using System.Collections.Generic;

namespace Optimization.EvolutionStrategy.Interfaces
{
    public interface IRecombinator
    {
        IIndividual Cross(List<IIndividual> parents);
    }
}
