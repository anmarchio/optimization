using Optimization.Serialization.Interfaces;

namespace Optimization.EvolutionStrategy.Interfaces
{

    /// <summary>
    /// Implement this interface if you want to automate your experiments
    /// </summary>
    public interface IConfiguration : ISupportsSerialization
    {
        ConfigurationType ConfigurationType { get; }
    }

    /// <summary>
    /// Add your new Configuration type here
    /// </summary>
    public enum ConfigurationType
    {
        Fitness, EvolutionStrategy, CGP, PiSet
    }
}
