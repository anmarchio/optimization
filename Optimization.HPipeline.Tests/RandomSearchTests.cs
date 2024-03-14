using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Optimization.CartesianGeneticProgramming;
using Optimization.Data;
using Optimization.EvolutionStrategy;
using Optimization.EvolutionStrategy.Analyzers;
using Optimization.EvolutionStrategy.Random;
using Optimization.EvolutionStrategy.Terminators;
using Optimization.Fitness;
using Optimization.HPipeline.Fitness;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.Tests;
using Optimization.Tests.Categories;

namespace Optimization.HPipeline.Tests
{
    [TestFixture]
    public class RandomSearchTests
    {

        [Test,ExtremeLongTest]
        public void PerformRandomSearch()
        {
            int seed = 100;
            var random = new SystemRandom(seed);
            var nodes = CommonHalconPipelines.StatusQuo.Nodes;
            Optimization.Fitness.ErrorHandling.Logger.BasePath = CommonInformation.TestResultsDirectory;

            var modularOperatorMap = new LegacyHalconOperatorMap(nodes); // nodes ist eine Liste von HalconOperatorNodes
            var cgpConfig = new CGPConfiguration(10, 10, 10, 2, nodes.Max(x => x.CGPParameterCount),
                modularOperatorMap, 1, 1);
            var decoder = new CGPDecoder(cgpConfig);
            var builder = new PipelineBuilder(decoder, cgpConfig, modularOperatorMap);

            var referenceSet = CommonHImages.ReferenceSetHalcon;
            var fitnessFunctions = new FitnessFunction[] { FitnessFunction.MCC };
            var weights = new double[] { 1 };
            var fitConfig = new HalconFitnessConfiguration(referenceSet, fitnessFunctions, weights, 4, null, 1000, 0.3f);
            //var evaluator = new FitnessEvaluator(fitConfig, cgpConfig, decoder, builder);
            var trainDataLoader = new DataLoader<ReferenceImage>(referenceSet);
            var evaluator = new HalconPipelineCGPEvaluator(cgpConfig, trainDataLoader, trainDataLoader, fitConfig);
            var creator = new CGPFloatVectorCreator(random, cgpConfig);
            var terminator = new GenerationCountTerminator(maxGenerations: 50);

            var analyzers = new List<Analyzer>();
            analyzers.Add(new ConsoleStatusUpdateAnalyzer());
            analyzers.Add(new FitnessValueAnalyzer());
            var analyzer = new MultiAnalyzer(analyzers);

            var randomSearch = new RandomSearch(creator, evaluator, terminator, analyzer, fitConfig);
            randomSearch.Run();
        }
    }
}
