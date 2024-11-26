using System.Collections.Generic;
using System.ComponentModel;
using Optimization.Fitness;

namespace Optimization.EvolutionStrategy.Interfaces
{
    public interface IEvolutionStrategy : ICopyableRandom
    {
        List<IIndividual> Evolve();
        void RegisterWorker(BackgroundWorker worker, int start, int end);

        FitnessConfiguration FitnessConfiguration { get; set; }

        ESConfiguration Configuration { get; set; }

        IIndividual Best { get; set; }

        IEvaluator Evaluator { get; set; }
    }
}
