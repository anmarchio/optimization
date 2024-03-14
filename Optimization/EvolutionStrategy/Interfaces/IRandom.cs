namespace Optimization.EvolutionStrategy.Interfaces
{
    public interface IRandom
    {
        int Next();
        int Next(int min, int maxExclusive);
        int Next(int maxExclusive);

        /// <summary>
        /// Returns a value in [0,1)
        /// </summary>
        /// <returns></returns>
        double NextDouble();

        double NextGaussian(double mu = 0, double sigma = 1);
    }
}
