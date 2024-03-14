namespace Optimization.EvolutionStrategy.Interfaces
{
    public interface ICreator : ICopyableRandom
    {
        IIndividual Create();
    }
}
