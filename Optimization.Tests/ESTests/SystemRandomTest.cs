using System;
using System.Linq;
using NUnit.Framework;
using Optimization.EvolutionStrategy.Random;
using Optimization.Tests.Categories;

namespace Optimization.Tests.ESTests
{
    [TestFixture]
    public class SystemRandomTest
    {
        [Test,ShortTest]
        public void Gaussian()
        {
            var values = new double[100000];
            var random = new MersenneTwister((int)(DateTime.Now.Ticks));
            var values2 = new double[100000];

            for (int i = 0; i < values.Length; i++)
            {
                values2[i] = random.NextDouble();
                values[i] = random.NextGaussian();
            }

            var count = values.Where(x => x < 0).Count();
            var count2 = values2.Where(x => x > 0.5).Count();

            Assert.AreEqual(count, count2, count / 100);
        }
    }
}
