namespace Optimization.EvolutionStrategy.Interfaces
{
    public interface IAdaptiveMutator : IMutator
    {
        double StandardDeviation { get; set; }

        double MutationProbability { get; set; }

        void Adapt(int generationsWithoutImprovement);
    }
}
