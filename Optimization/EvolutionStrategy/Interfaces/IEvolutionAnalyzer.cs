namespace Optimization.EvolutionStrategy.Interfaces
{
    public interface IEvolutionAnalyzer
    {
        void Analyze(EvolutionStrategy evolutionStrategy);

        void Save(string directory);
    }
}
