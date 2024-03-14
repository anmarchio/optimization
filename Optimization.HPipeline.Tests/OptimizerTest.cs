using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Optimization.CartesianGeneticProgramming;
using Optimization.Data;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Random;
using Optimization.HPipeline.Fitness;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.HPipeline.OperatorNodes;
using Optimization.Pipeline;
using Optimization.Tests;
using Optimization.Tests.Categories;
using Optimization.Tests.TestImages;

namespace Optimization.HPipeline.Tests
{
    [TestFixture]
    public class OptimizerTest
    {


        public Optimizer<HalconPipeline, HalconOperatorNode> GetOptimizer(HalconPipeline pipeline, string refDirectory, IRandom random) {

            var refSet = new ReferenceSet(refDirectory) {ImageResize = CommonImages.Size};
            var loader = new DataLoader<ReferenceImage>(refSet);
            FloatVector vec; CGPConfiguration conf;
            var map = new HalconOperatorMap(pipeline.Nodes);
            pipeline.ToCGPEncoding(map, random, out vec, out conf);
            var eval = new HalconPipelineCGPEvaluator(conf, loader, loader, new HalconFitnessConfiguration());
            var optimizer = new Optimizer<HalconPipeline, HalconOperatorNode>(pipeline, eval);
            return optimizer;
        }


        [Test,ExtremeLongTest]
        public void OptimizeStatusQuo()
        {
            Assert.Pass("Takes a very long time. Run only if necessary.");

            var pipeline = CommonHalconPipelines.StatusQuo;
            var refSet = new ReferenceSet(CommonHImages.SampleImageDirectory);
            var optimizer = new Optimizer<HalconPipeline, HalconOperatorNode>(pipeline,
                new HalconPipelineEvaluator(new DataLoader<ReferenceImage>(refSet),
                    new DataLoader<ReferenceImage>(refSet),
                    new HalconFitnessConfiguration()), new List<HalconOperatorNode>() {pipeline.Nodes[0], pipeline.Nodes[4] });
            
            var iterations = optimizer.NecessaryEvaluations;
            var val = optimizer.Optimize();

            var best = optimizer.Pipeline;

            best.SerializeXml(Path.Combine(CommonInformation.TestResultsDirectory, "best_status_quo.txt"));

            Assert.AreEqual(optimizer.NecessaryEvaluations, optimizer.Evaluator.IndividualsEvaluated);
        }

        [Test,ExtremeLongTest]
        public void SimpleThreshold()
        {
            Assert.Pass("This also takes ages. Does not currently test anything that is ever used -- let only run if necessary.");
            var input = new HalconInputNode();
            var threshold = new Threshold(input, 5, 255);

            var random = new SystemRandom(0);

            var optimizer = GetOptimizer(new HalconPipeline(threshold), CommonHImages.SampleImageDirectory, random);
            var best1 = optimizer.Optimize();


            optimizer = GetOptimizer(new HalconPipeline(threshold), Path.Combine(CommonHImages.ImageFormatConversionDirectory, "Indexed"), random);
            var best2 = optimizer.Optimize();

            Assert.Pass("Best fitness on bachelor thesis sample by using _only_ a simple threshold: {0} on artificial sample: {1}", best1, best2);

        }

        [Test,ExtremeLongTest]
        public void PeronaMalik()
        {
            Assert.Pass("Takes too long");
            var pipeline = CommonHalconPipelines.PeronaMalik;
            var map = new HalconOperatorMap(pipeline.Nodes);
            FloatVector vec; CGPConfiguration conf;
            var random = new SystemRandom(0);
            pipeline.ToCGPEncoding(map, random, out vec, out conf);
            var refSet = new ReferenceSet(Path.Combine(CommonHImages.ImageFormatConversionDirectory, "Converted"));
            var loader = new DataLoader<ReferenceImage>(refSet);
            var eval = new HalconPipelineCGPEvaluator(conf, loader, loader, new HalconFitnessConfiguration());
            var optimizer = new Optimizer<HalconPipeline, HalconOperatorNode>(pipeline, eval);

            var permutations = optimizer.NecessaryEvaluations;


            var fit = optimizer.Optimize();


            var best = optimizer.Pipeline;

            best.SerializeXml(Path.Combine(CommonInformation.TestResultsDirectory, "best_perona_malik.txt"));

            Assert.Pass("Achieved fitness: {0}", fit);
        }
    }
}
