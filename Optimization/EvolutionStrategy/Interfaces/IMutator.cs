namespace Optimization.EvolutionStrategy.Interfaces
{
    public interface IMutator
    {
        IIndividual Mutate(IIndividual individual /*, double stdDev = 2*/);
    }
}
