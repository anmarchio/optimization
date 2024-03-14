using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Random;
using Optimization.Tests.Categories;

namespace Optimization.Tests.RandomTests
{
    [TestFixture]
    public class MersenneTwisterTest
    {
        [Test,ShortTest]
        public void DoTheTwist()
        {
            var twister = new MersenneTwister(0);

            var values = new double[100000];

            for(int i = 0; i < values.Length; i++)
            {
                values[i] = twister.NextGaussian();
            }

            Assert.AreEqual(values.Average(), 0, 1, "avg: "+values.Average());

        }

        private long RunRandom(IRandom random, int iterations)
        {
            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < iterations; i++)
            {
                random.Next();
            }
            watch.Stop();
            return watch.ElapsedMilliseconds;
        }

        [Test,ExtremeLongTest]
        public void RandomNumberGeneratorSpeedComparison()
        {
            Assert.Pass("Run this only if necessary. Worked, but takes too long to execute.");
            int seed = 0;
            int numNumbers = int.MaxValue / 8;
            var systemRandom = new SystemRandom(seed);
            var mathNetRandom = new MathNetRandom(seed);
            var customMersenne = new MersenneTwister(seed);

            var systemTime = RunRandom(systemRandom, numNumbers);
            var mathNetTime = RunRandom(mathNetRandom, numNumbers);
            var customMersenneTime = RunRandom(customMersenne, numNumbers);

            Assert.Pass("System: {0}, mathNet: {1}, custom: {2}:", systemTime, mathNetTime, customMersenneTime);

        }
    }
}
