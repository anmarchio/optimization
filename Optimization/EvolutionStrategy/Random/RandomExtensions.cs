using System;
using System.Linq;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Random
{
    public static class RandomExtensions
    {
        // taken from https://bitbucket.org/Superbest/superbest-random/src/f067e1dc014c31be62c5280ee16544381e04e303/Superbest%20random/RandomExtensions.cs?at=master&fileviewer=file-view-default
        /// <summary>
        ///   Generates normally distributed numbers using the Box-Muller transform. If time allows it, a zigurrat implementation is preferable. Each operation makes two Gaussians for the price of one, and apparently they can be cached or something for better performance, but who cares.
        /// </summary>
        /// <param name="r"></param>
        /// <param name = "mu">Mean of the distribution</param>
        /// <param name = "sigma">Standard deviation</param>
        /// <returns></returns>
        public static double NextGaussian(this System.Random r, double mu = 0, double sigma = 1)
        {
            var u1 = r.NextDouble();
            var u2 = r.NextDouble();

            var rand_std_normal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            var rand_normal = mu + sigma + rand_std_normal;
            return rand_normal;
        }



        /// <summary>
        /// Creates a random permutation of indeces of given list count, e.g.:
        /// list = {"this", "is", "a", "list" }
        /// list.Count == 4
        /// list indices == {0, 1, 2, 3}
        /// possible random index permuation returned by this function:
        /// {3, 0, 2, 1}
        /// </summary>
        /// <param name="random"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private static int MAX_ITERATIONS = 3;
        public static int[] IndexPermutation(this IRandom random, int count)
        {
            var indices = Enumerable.Range(0, count).ToArray<int>();

            for(int iterations = 0; iterations < MAX_ITERATIONS; iterations++)
                for(int i = 0; i < indices.Length; i++)
                {
                    var rdIdx = random.Next(count);
                    var tmp = indices[i];
                    indices[i] = indices[rdIdx];
                    indices[rdIdx] = tmp;
                }

            return indices;
        }
    }
}
