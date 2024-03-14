using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Optimization.CartesianGeneticProgramming;
using Optimization.CartesianGeneticProgramming.Mutators;
using Optimization.Data;
using Optimization.EvolutionStrategy;
using Optimization.EvolutionStrategy.Analyzers;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Random;
using Optimization.EvolutionStrategy.Selectors;
using Optimization.EvolutionStrategy.Terminators;
using Optimization.Fitness;
using Optimization.Fitness.ErrorHandling;
using Optimization.HPipeline.Fitness;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.Pipeline;
using Optimization.Tests;
using Optimization.Tests.Categories;
using Optimization.Tests.TestImages;

namespace Optimization.HPipeline.Tests
{
    [TestFixture]
    public class EvolutionStrategyTest
    {

        private static ReferenceSet refSet = new ReferenceSet(Path.Combine(CommonHImages.ImageFormatConversionDirectory, "Indexed")) {ImageResize = CommonImages.Size };
        private static int generations = 10;
        private static int batchsize = 1;

        [Test, ExtremeLongTest]
        public void SelfAdaptive()
        {
            var refSet = new ReferenceSet(Path.Combine(CommonHImages.ImageFormatConversionDirectory, "Indexed")){ImageResize = CommonImages.Size };
            var config = new CGPConfiguration(10, 10, 1, 2, 4, new HalconOperatorMap(CommonHalconPipelines.StatusQuo.Nodes), 1, 1);
            var batch = CommonHalconEvolutionStrategies.SelfAdaptiveEvolutionStrategy(config, refSet, refSet, 5, 5, Path.Combine(CommonInformation.TestResultsDirectory, "SelfAdaptiveTest"));
            var best = batch.Run();
            Assert.Pass("best fitness: {0}", best.Fitness.Sum(x => x.Value ?? 0) / best.Fitness.Count);
        }

        [Test, ExtremeLongTest]
        public void RunSelfAdaptiveWithFullHalconOperatorSet()
        {
            Logger.ConsumeExceptions = true;
            Logger.BasePath = CommonInformation.Directory;
            var collection = CommonHalconPipelines.HalconOperatorNodeCollection;
            var map = new HalconOperatorMap(collection);
            var cols= map.Dependencies.EstimateCgpColumnCount();
            var refSet = new ReferenceSet(Path.Combine(CommonHImages.ImageFormatConversionDirectory, "Indexed")) {ImageResize = CommonImages.Size};
            var config = new CGPConfiguration(10, cols, 1, collection.Max(x => x.CGPInputCount), collection.Max(x => x.CGPParameterCount), map, 1, 1);
            var batch = CommonHalconEvolutionStrategies.SelfAdaptiveEvolutionStrategy(config, refSet, refSet, 5, 5, Path.Combine(CommonInformation.TestResultsDirectory, "SelfAdaptiveTest"));
            var best = batch.Run();
            Assert.Pass("best fitness: {0}", best.Fitness.Sum(x => x.Value ?? 0) / best.Fitness.Count);
        }

        [Test,ExtremeLongTest]
        public void RandomSearchAnisotropic()
        {
            var random = new SystemRandom(0);
            var pipe = CommonHalconPipelines.PeronaMalik;

            CGPConfiguration configuration = null; FloatVector vector = null;
            pipe.ToCGPEncoding(new HalconOperatorMap(pipe.Nodes), random, out vector, out configuration);
            var creator = new CGPFloatVectorCreator(random, configuration);
            
            var fitConfig = new HalconFitnessConfiguration();
            fitConfig.ReferenceSet = refSet;
            var data = new DataLoader<ReferenceImage>(refSet);
            var evaluator = new HalconPipelineCGPEvaluator(configuration, data, data, fitConfig);
            var terminator = new GenerationCountTerminator(2);

            var rd = new RandomSearch(creator, evaluator, terminator, new FitnessValueAnalyzer(), fitConfig);
            var best = rd.Run();
            Assert.Pass("best fitness: {0}", best.Fitness.Sum(x => x.Value ?? 0) / best.Fitness.Count);

        }
        [Test,ExtremeLongTest]
        public void StandardEvolutionStrategy()
        {
            var refSet = new ReferenceSet(Path.Combine(CommonHImages.ImageFormatConversionDirectory, "Indexed")) {ImageResize = CommonImages.Size};
            var config = new CGPConfiguration(10, 10, 1, 2, 4, new LegacyHalconOperatorMap(CommonHalconPipelines.StatusQuo.Nodes), 1, 1);
            //var batch = CommonEvolutionStrategies.BuildLegacyStandardCGPEvolutionStrategy(config, refSet, refSet, 150, 2, Path.Combine(CommonInformation.TestResultsDirectory, "StandardEvolutionTest"), seed:0);
            // all the legacy code does not support many of the newly implemented features that try to reduce unnecessary exceptions, thus throwing them all the fucking timr
            // it is time so say goodbye to the old BA code...
            var batch = CommonHalconEvolutionStrategies.BuildStandardCGPEvolutionStrategy(config, refSet, refSet,
                5, iterations: 5, saveDirectory: Path.Combine(CommonInformation.TestResultsDirectory, "StandardEvolutionTest"), seed: 0);

            var best = batch.Run();
            Assert.Pass("best fitness: {0}", best.Fitness.Sum(x => x.Value ?? 0) / best.Fitness.Count);
        }

    }
}
