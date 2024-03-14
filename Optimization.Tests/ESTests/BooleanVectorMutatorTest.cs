using System;
using NUnit.Framework;
using Optimization.EvolutionStrategy.Creators;
using Optimization.EvolutionStrategy.Mutators;
using Optimization.EvolutionStrategy.Random;
using Optimization.Tests.Categories;

namespace Optimization.Tests.ESTests
{
    [TestFixture]
    public class BooleanVectorMutatorTest
    {
        [Test,ShortTest]
        public void Mutate()
        {
            var random = new SystemRandom(DateTime.Now.Day);
            int maxTrueCount = 10;
            var creator = new BooleanVectorCreator(random, 50, maxTrueCount);
            var vector = creator.Create().BooleanVector;
            var mutator = new BooleanVectorMutator(random, maxTrueCount);

            for (int i = 0; i < 100; i++)
            {
                vector = mutator.Mutate(vector).BooleanVector;
                Assert.IsTrue(vector.TrueCount <= maxTrueCount, "iteration: " + i + " tc: " + vector.TrueCount);
            }
        }
    }
}
