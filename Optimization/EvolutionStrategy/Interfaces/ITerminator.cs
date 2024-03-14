namespace Optimization.EvolutionStrategy.Interfaces
{

    /// <summary>
    /// Can access most evolutionstrategy information in order to determine termination.
    /// </summary>
    public interface ITerminator
    {
        bool Terminate(EvolutionStrategy evolutionStrategy);
    }
}
