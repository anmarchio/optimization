using System;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Random
{
    [Serializable]
    public class SystemRandom : IRandom
    {
        private System.Random random;

        public int Seed { get; private set; }

        public SystemRandom(int seed)
        {
            Seed = seed;
            random = new System.Random(seed);
        }

        public int Next()
        {
            return random.Next();
        }

        public override string ToString()
        {
            return "SystemRandom - Seed: " + Seed.ToString();
        }

        public double NextDouble()
        {
            return random.NextDouble();
        }

        public int Next(int maxExclusive)
        {
            return random.Next(maxExclusive);
        }

        public int Next(int min, int maxExclusive)
        {
            return random.Next(min, maxExclusive);
        }

        public double NextGaussian(double mu = 0, double sigma = 1)
        {
            return random.NextGaussian(mu, sigma);
        }
    }
}
