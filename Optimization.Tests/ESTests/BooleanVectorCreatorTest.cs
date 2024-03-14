using NUnit.Framework;
using Optimization.EvolutionStrategy.Creators;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Random;
using Optimization.Tests.Categories;

namespace Optimization.Tests.ESTests
{   /// <summary>
    /// This class tests the creation of individuals that are encoded as boolean vectors.
    /// </summary>
    [TestFixture] // Indicates to NUnit that this class contains tests.
    public class BooleanVectorCreatorTest
    {
        /// <summary>
        /// Test the random instantiation of an individual.
        /// </summary>
        [Test,ShortTest] // Indicate to NUnit that this method is a test.
        public void Create()
        {
            var random = new SystemRandom(0); // IMPORTANT: Always set a specific seed, so that the test always returns the same result.
            int maxTrueCount = 10;
            var creator = new BooleanVectorCreator(random, 50, maxTrueCount);
            var vector = creator.Create().BooleanVector;

            for (int i = 0; i < 100; i++)
            {
                // Use assert statements to assert something about the results. Consider providing a helpful message in case the test fails.
                Assert.IsTrue(IsValid(vector, maxTrueCount), "iteration: " + i + " tc: " + vector.TrueCount);
                vector = creator.Create().BooleanVector;
            }
        }

        private bool IsValid(BooleanVector vector, int maxTrueCount)
        {
            return vector.TrueCount <= maxTrueCount;
        }
    }
}