using Optimization.EvolutionStrategy.Analyzers;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Selectors;
using Optimization.EvolutionStrategy.Terminators;
using Optimization.Fitness;
using System.Collections.Generic;

namespace Optimization.EvolutionStrategy
{
    public class LocalSearch : EvolutionStrategy
    {

        /// <summary>
        /// The neighborhood is implicitly defined my mutator.
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="mutator"></param>
        /// <param name="evaluator"></param>
        /// <param name="analyzer"></param>
        /// <param name="iterations"></param>
        /// <param name="neighborhoodSize"></param>
        /// <param name="maximization"></param>
        /// <param name="fitnessConfiguration"></param>
        public LocalSearch(ICreator creator, IMutator mutator, IEvaluator evaluator, Analyzer analyzer, int iterations, int neighborhoodSize,
            FitnessConfiguration fitnessConfiguration, bool maximization=true) : base()
        {
            Analyzer = analyzer;
            Mutator = mutator;
            Evaluator = evaluator;
            Terminator = new GenerationCountTerminator(iterations);
            Configuration = new ESConfiguration(1, neighborhoodSize, true);
            Creator = creator;
            SurvivalSelector = new BestSelector(fitnessConfiguration);
            ProcreationSelector = SurvivalSelector;
            FitnessConfiguration = fitnessConfiguration;
        }

        public List<IIndividual> Run()
        {
            return Evolve();
        }
    }
}
