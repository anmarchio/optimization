using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Selectors;
using Optimization.Fitness;

namespace Optimization.EvolutionStrategy
{
    public class RandomSearch : EvolutionStrategy
    {
        private class RandomSearchMutator : IMutator
        {
            public RandomSearchMutator(ICreator creator)
            {
                Creator = creator;
            }
            private ICreator Creator { get; set; }
            public IIndividual Mutate(IIndividual individual)
            {
                return Creator.Create();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="creator"></param>
        /// <param name="evaluator"></param>
        /// <param name="terminator"></param>
        /// <param name="analyzer"></param>
        /// <param name="fitnessConfiguration"></param>
        public RandomSearch(ICreator creator, IEvaluator evaluator, ITerminator terminator, IEvolutionAnalyzer analyzer,
            FitnessConfiguration fitnessConfiguration) : base()
        {
            var selector = new BestSelector(fitnessConfiguration);
            Creator = creator;
            Mutator = new RandomSearchMutator(creator);
            ProcreationSelector = selector;
            SurvivalSelector = selector;
            Evaluator = evaluator;
            Terminator = terminator;
            Analyzer = analyzer;
            Configuration = new ESConfiguration(1, 1, true);
            FitnessConfiguration = fitnessConfiguration;
        }

        public IIndividual Run()
        {
            return Evolve();
        }
    }
}
