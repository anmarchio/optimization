using System.Collections.Generic;

namespace Optimization.EvolutionStrategy.Interfaces
{
    public interface IValidator
    {

        /// <summary>
        /// Validates the best Invididual at the end of the evolution run.
        /// </summary>
        /// <param name="best"></param>
        void Validate(List<IIndividual> best);
    }
}
