namespace Optimization.EvolutionStrategy.Interfaces
{

    /// <summary>
    /// Implement this interfaces if each run of a Evolution Strategy needs a freshly instantiated copy of an object (e.g. Analyzers, Evaluators)
    /// </summary>
    public interface ICopyable
    {
        ICopyable Copy();
        
    }

    public interface ICopyableRandom : ICopyable
    {
        ICopyable Copy(IRandom rand);
    }
}
