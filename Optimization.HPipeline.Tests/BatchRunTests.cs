using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using Extensions;
using System;
using System.Runtime.InteropServices;
using NUnit.Framework;
using Optimization.CartesianGeneticProgramming;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Random;
using Optimization.HPipeline.Fitness;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.Tests;
using Optimization.Tests.Categories;
using Optimization.Tests.TestImages;

namespace Optimization.HPipeline.Tests
{
    [TestFixture]
    public class BatchRunTests
    {
        private string ReferenceSetDirectory = Path.Combine(CommonHImages.ImageFormatConversionDirectory, "Indexed");
        private ReferenceSet HalconReferenceSet { get { return new ReferenceSet(ReferenceSetDirectory) {ImageResize = CommonImages.Size}; } }

        /// <summary>
        /// We want to check if for multiple batch runs using the same seed
        /// all best individuals from the same evolution strategy are identical
        /// w.r.t. their final fitness value and their FloatVector representation
        /// </summary>
        [Test, ExtremeLongTest]
        public void CheckIfSeedingWorksProperly()
        {
            int maxNumTests = 5;
            int seed = 0, generations = 5, iterations = 5;
            var random = new SystemRandom(seed);
            // halcon evolution strategies
            CGPConfiguration config; FloatVector vector;
            var pipe = CommonHalconPipelines.StatusQuo;
            var map = new HalconOperatorMap(pipe.Nodes, pipe.ToDependencyTree());
            CommonHalconPipelines.StatusQuo.ToCGPEncoding(map, random, out vector, out config);
            // we store for each generation each best individual of each testRun... they all should have the same fitness value
            // in fact, they should all be identical (so we can compare their float vectors)
            var best = Enumerable.Range(0, iterations)
                .ToDictionary(x => x, 
                    x => new List<IIndividual>());
            var refSet = HalconReferenceSet;
            var dir = Path.Combine(CommonInformation.TestResultsDirectory);
            
            for (int numTest = 0; numTest < maxNumTests; numTest++)
            {
                var es = CommonHalconEvolutionStrategies.BuildStandardCGPEvolutionStrategy(config, refSet, refSet, generations,
                    iterations: iterations, saveDirectory: dir, seed: seed);
                es.Run();
                for(int i = 0; i < iterations; i++)
                    best[i].Add(es.BestIndividuals.ElementAt(i).Item2);
            }

            for (int i = 0; i < iterations; i++)
            {
                foreach (var fitFunc in best.First().Value.First().Fitness.Keys)
                {
                    Assert.IsTrue(best[i].All(x => x.Fitness[fitFunc] == best[i].First().Fitness[fitFunc]));
                }

                for (int j = 0; j < config.Length; j++)
                {
                    for(int k = 0; k < maxNumTests; k++)
                        Assert.AreEqual(best[i][k].FloatVector[j], best[i][0].FloatVector[j]);
                }
            }

            best = Enumerable.Range(0, iterations)
                .ToDictionary(x => x,
                    x => new List<IIndividual>());
            for (int numTest = 0; numTest < maxNumTests; numTest++)
            {
                var es = CommonHalconEvolutionStrategies.SelfAdaptiveEvolutionStrategy(config, refSet, refSet, generations, iterations, dir, seed);
                es.Run();
                for (int i = 0; i < iterations; i++)
                    best[i].Add(es.BestIndividuals.ElementAt(i).Item2);
            }

            for (int i = 0; i < iterations; i++)
            {
                foreach (var fitFunc in best.First().Value.First().Fitness.Keys)
                {
                    Assert.IsTrue(best[i].All(x => x.Fitness[fitFunc] == best[i].First().Fitness[fitFunc]));
                }

                for (int j = 0; j < config.Length; j++)
                {
                    for (int k = 0; k < maxNumTests; k++)
                        Assert.AreEqual(best[i][k].FloatVector[j], best[i][0].FloatVector[j]);
                }
            }
            Directory.Delete(dir, recursive:true);

        }

        #region tests for parralelization

        /// <summary>
        /// Tests wether the BatchRun returns the same Individuals independent of parallelization degree 
        /// </summary>
        [Test]
        public void CheckIfDegreeProducesSameResults()
        {
            int maxNumTests = 5;

            int seed = 42;
            var random = new SystemRandom(seed);

            var paralleldegree = 2;

            // halcon evolution strategies
            CGPConfiguration config;
            FloatVector vector;
            var pipe = CommonHalconPipelines.StatusQuo;
            var map = new HalconOperatorMap(pipe.Nodes, pipe.ToDependencyTree());
            CommonHalconPipelines.StatusQuo.ToCGPEncoding(map, random, out vector, out config);

            // Dictionary 
            var best = new List<IIndividual>();
            // key: seed, value: best individuals returned by batch run
            var resultDict = new Dictionary<int, List<IIndividual>>();
            var refSet = HalconReferenceSet;
            var dir = Path.Combine(CommonInformation.TestResultsDirectory);

            //Correct Number of exponetial increasing degree
            for (int numTest = 0; numTest < maxNumTests; numTest++)
            {
                paralleldegree = (int) Math.Pow(2, numTest);

                //Create Evolutionstrategy
                var es = CommonHalconEvolutionStrategies.BuildStandardCGPEvolutionStrategy(config, refSet,
                    refSet, generations: 5, iterations: 5, saveDirectory: dir, seed: seed,
                    parallelDegree: paralleldegree);

                //Create BatchRun
                var indiv = es.Run();
                foreach (var pair in es.BestIndividuals)
                {
                    if (!resultDict.ContainsKey(pair.Item1))
                        resultDict.Add(pair.Item1, new List<IIndividual>());
                    resultDict[pair.Item1].Add(pair.Item2);
                }

                best.Add(indiv);
            }

            //Assertions for List of Individuals, check if members are the same
            //Maybe it is neccerssary to map Individuals to their corresponding seed for a easy lookup check, requires modification of IIndividual for additional property
            foreach (var indiv in resultDict.Values)
            {
                for (int i = 0; i < indiv.First().FloatVector.Length; i++)
                    Assert.IsTrue(indiv.Select(x => x.FloatVector[i]).Distinct().Count() == 1);
            }


            best.Clear();
            resultDict.Clear();

            for (int numTest = 0; numTest < maxNumTests; numTest++)
            {
                paralleldegree = (int) Math.Pow(2, numTest);

                //Create Evolutionstrategy
                var es = CommonHalconEvolutionStrategies.SelfAdaptiveEvolutionStrategy(config, refSet, refSet,
                    generations: 5, iterations: 5, saveDirectory: dir, seed: seed, parallelDegree: paralleldegree);

                //Create BatchRun
                var indiv = es.Run();
                foreach (var pair in es.BestIndividuals)
                {
                    if (!resultDict.ContainsKey(pair.Item1))
                        resultDict.Add(pair.Item1, new List<IIndividual>());
                    resultDict[pair.Item1].Add(pair.Item2);
                }

                best.Add(indiv);
            }

            //Assertions for List of Individuals, check if members are the same
            //Maybe it is neccerssary to map Individuals to their corresponding seed for a easy lookup check, requires modification of IIndividual for additional property
            foreach (var indiv in resultDict.Values)
            {
                for (int i = 0; i < indiv.First().FloatVector.Length; i++)
                    Assert.IsTrue(indiv.Select(x => x.FloatVector[i]).Distinct().Count() == 1);
            }

            best.Clear();
            resultDict.Clear();

            Directory.Delete(dir, recursive: true);
        }

        /// <summary>
        /// Tetsts wether BatchRuns have unique seeds and therefore produce different unique, different results
        /// </summary>
        [Test]
        public void CheckIfSeedsProduceUniqueResults()
        {
            int maxNumTests = 5;

            int seed = 42;
            var random = new SystemRandom(seed);

            var paralleldegree = 2;

            // halcon evolution strategies
            CGPConfiguration config; FloatVector vector;
            var pipe = CommonHalconPipelines.StatusQuo;
            var map = new HalconOperatorMap(pipe.Nodes, pipe.ToDependencyTree());
            CommonHalconPipelines.StatusQuo.ToCGPEncoding(map, random, out vector, out config);

            // Dictionary 
            var best = new List<IIndividual>();
            // key: seed, value: best individuals returned by batch run
            var resultDict = new Dictionary<int, List<IIndividual>>();

            var refSet = HalconReferenceSet;
            var dir = Path.Combine(CommonInformation.TestResultsDirectory);

            //Correct Number of exponetial increasing degree
            for (int numTest = 0; numTest < maxNumTests; numTest++)
            {
                paralleldegree = (int)Math.Pow(2, numTest);

                //Create Evolutionstrategy
                var es = CommonHalconEvolutionStrategies.BuildStandardCGPEvolutionStrategy(config, refSet, refSet, generations: 5, iterations: 5, saveDirectory: dir, seed: seed, parallelDegree: paralleldegree);

                //Create BatchRun
                var indiv = es.Run();
                foreach (var pair in es.BestIndividuals)
                {
                    if (!resultDict.ContainsKey(pair.Item1))
                        resultDict.Add(pair.Item1, new List<IIndividual>());
                    resultDict[pair.Item1].Add(pair.Item2);
                }
                best.Add(indiv);
            }
            //Assertion for unique List elements, then list size should equal the expected produced individuals
            //Maybe it is neccerssary to map Individuals to their corresponding seed for a easy lookup check, requires modification of IIndividual for additional property
            List<IIndividual> uniqueseeds = new List<IIndividual>();

            for (int i = 0; i < maxNumTests; i++)
            {
                foreach (var indiv in resultDict.Values)
                {
                    uniqueseeds.Add(indiv[i]);
                }
                Assert.True(uniqueseeds.Select(x => x.FloatVector).Distinct(new FloatVector.FloatVectorEqualityComparer())
                                .Count() == resultDict.Keys.Distinct().Count(), "failed at BuildParallelStandardCGPEvolutionStrategy");
                uniqueseeds.Clear();
            }


            best.Clear();
            resultDict.Clear();

            for (int numTest = 0; numTest < maxNumTests; numTest++)
            {
                paralleldegree = (int)Math.Pow(2, numTest);

                //Create Evolutionstrategy
                var es = CommonHalconEvolutionStrategies.SelfAdaptiveEvolutionStrategy(config, refSet, refSet, generations: 5,
                    iterations: 5, saveDirectory: dir, seed: seed, parallelDegree: paralleldegree);

                //Create BatchRun
                var indiv = es.Run();
                foreach (var pair in es.BestIndividuals)
                {
                    if (!resultDict.ContainsKey(pair.Item1))
                        resultDict.Add(pair.Item1, new List<IIndividual>());
                    resultDict[pair.Item1].Add(pair.Item2);
                }
                best.Add(indiv);
            }
            //Assertion for unique List elements, then list size should equal the expected produced individuals
            //Maybe it is neccerssary to map Individuals to their corresponding seed for a easy lookup check, requires modification of IIndividual for additional property
            for (int i = 0; i < maxNumTests; i++)
            {
                foreach (var indiv in resultDict.Values)
                {
                    uniqueseeds.Add(indiv[i]);
                }
                Assert.True(uniqueseeds.Select(x => x.FloatVector).Distinct(new FloatVector.FloatVectorEqualityComparer())
                                .Count() == resultDict.Keys.Distinct().Count(), "failed at BuildParallelStandardCGPEvolutionStrategy");
                uniqueseeds.Clear();
            }

            best.Clear();
            resultDict.Clear();

            Directory.Delete(dir, recursive: true);
        }
        
        #endregion
    }
}
