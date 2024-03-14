using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Analyzers
{

    /// <summary>
    /// Used to gather data for analysis. Not to be confused with Evaluators, which are responsible for assigning fitness to an individual
    /// </summary>
    public abstract class Analyzer : IEvolutionAnalyzer, ICopyable
    {

        public abstract void Analyze(EvolutionStrategy evolutionStrategy);
        public abstract ICopyable Copy();

        public abstract void Save(string directory);
    }
}
