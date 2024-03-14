using System;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Random
{

    /// <summary>
    /// Pseudorandom number generator. https://de.wikipedia.org/wiki/Mersenne-Twister
    /// </summary>
    [Serializable]
    public class MersenneTwister : IRandom
    {
        private const int N = 624;
        private const int M = 397;
        private uint[] A = { 0, 0x9908B0DF };

        private object lockable = new object();

        private int Index { get; set; }
        public uint Seed { get; private set; }

        private uint[] Vector { get; set; }

        public MersenneTwister(int seed)
        {
            Seed = (uint) seed;
            Index = N;
            InitializeVector();
        }
        
        public override string ToString()
        {
            return "MersenneTwister - Seed: " + Seed.ToString();
        }

        private void InitializeVector()
        {
            const uint mult = 1812433253;
            Vector = new uint[N];
            uint seed = Seed;

            for (uint i = 0; i < Vector.Length; i++)
            {
                Vector[i] = seed;
                seed = mult * (seed ^ (seed >> 30)) + (i + 1);
            }
        }

        private void UpdateVector()
        {
            int i = 0;
            for (; i < N - M; i++)
            {
                Vector[i] = Vector[i + (M)] ^ (((Vector[i] & 0x80000000) | (Vector[i + 1] & 0x7FFFFFFF)) >> 1) ^ A[Vector[i + 1] & 1];
            }
            for (; i < N - 1; i++)
            {
                Vector[i] = Vector[i + (M - N)] ^ (((Vector[i] & 0x80000000) | (Vector[i + 1] & 0x7FFFFFFF)) >> 1) ^ A[Vector[i + 1] & 1];
            }
            Vector[N - 1] = Vector[M - 1] ^ (((Vector[N - 1] & 0x80000000) | (Vector[0] & 0x7FFFFFFF)) >> 1) ^ A[Vector[0] & 1];
        }

        private uint Twist()
        {
            uint e;

            
           if (Index >= N)
                {
                lock (lockable) // might parallelize mutation/evaluation in the future
                {
                    UpdateVector();
                    Index = 0;
                }
}
                e = Vector[Index++];
            

            e ^= (e >> 11);             /* Tempering */
            e ^= (e << 7) & 0x9D2C5680;
            e ^= (e << 15) & 0xEFC60000;
            e ^= (e >> 18);

            return e;
        }

        public int Next()
        {
            return Math.Abs((int) Twist());
        }

        public int Next(int maxExclusive)
        {
            if (maxExclusive <= 0) throw new Exception("Interval must not be negative");
            var val = Math.Abs((int)Twist());
            return val % maxExclusive;
        }

        public int Next(int min, int maxExclusive)
        {
            return Next(maxExclusive - min) + min;
        }

        public double NextDouble()
        {
            return Math.Abs(Twist()) * (1.0 / 4294967296.0);
        }

        /// <summary>
        /// Returns a normal (gaussian) distributed value from a distribution specified by mu and sigma.
        /// </summary>
        /// <param name="mu"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public double NextGaussian(double mu = 0, double sigma = 1)
        {
            var u1 = NextDouble();
            var u2 = NextDouble();

            var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            var rand_normal = mu + sigma * rand_std_normal;
            return rand_normal;
        }
    }
}
