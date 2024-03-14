using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions;
using HalconDotNet;
using NUnit.Framework;
using Optimization.CartesianGeneticProgramming;
using Optimization.Data;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Random;
using Optimization.Fitness;
using Optimization.HPipeline.Fitness;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.Tests;
using Optimization.Tests.Categories;

namespace Optimization.HPipeline.Tests.FitnessTests
{
    [TestFixture]
    public class PipelineEvaluatorTest
    {
        private static string ResultsPath = System.IO.Path.Combine(CommonInformation.TestResultsDirectory, "OverallFitnessComputation");
        
        [TestCase,ExtremeLongTest]
        public void OverallFitnessComputation()
        {
            var fitnessFunctions = new FitnessFunction[] { FitnessFunction.MCC };
            var weights = new double[] { 1 };
            var trainDataLoader = new DataLoader<ReferenceImage>(CommonHImages.ReferenceSetHalcon);
            var valDataLoader = new DataLoader<ReferenceImage>(CommonHImages.ReferenceSetHalcon);
            var fitConfig = new HalconFitnessConfiguration(CommonHImages.ReferenceSetHalcon, fitnessFunctions, weights, 4, null, 1000, 0.3f);
            var sq = CommonHalconPipelines.StatusQuo;
            var hmap = new HalconOperatorMap(sq.Nodes, sq.ToDependencyTree());
            CGPConfiguration config;
            FloatVector vector;
            var random = new SystemRandom(0);
            sq.ToCGPEncoding(hmap, random, out vector, out config);

            var sq2 = new HalconPipeline(vector, config);

            ResultsPath.CreateDirectory();
            sq.WriteToDOTFile(System.IO.Path.Combine(ResultsPath, "sq1.txt"));
            sq2.WriteToDOTFile(System.IO.Path.Combine(ResultsPath, "sq2.txt"));

            //foreach (var node in sq.Nodes)
            //    Assert.IsTrue(sq2.Nodes.Exists(x => HalconOperatorNodesTest.AreEqual(x, node)), "{0} from sq does not have an equal in sq2", node.GetType().Name);
            var evaluator = new HalconPipelineCGPEvaluator(config, trainDataLoader, valDataLoader, fitConfig);

            evaluator.Evaluate(vector);
            var fit = evaluator.WeightedFitnessOf(vector);
            Assert.Warn("Fitness used to be higher than {0}, now is {1}", 0.3, fit);
        }

        [TestCase,ShortTest]
        public void VerifyMCC()
        {
            var sq = CommonHalconPipelines.StatusQuo;
            var refSet = CommonHImages.ReferenceSetHalcon;
            var runningFit = 0.0;
            FloatVector vector;
            CGPConfiguration config;
            var hmap = new HalconOperatorMap(sq.Nodes, sq.ToDependencyTree());
            var random = new SystemRandom(0);
            sq.ToCGPEncoding(hmap, random, out vector, out config);
            var sq2 = new HalconPipeline(vector, config);
            var path1 = System.IO.Path.Combine(ResultsPath, "sq1results");
            path1.CreateDirectory();
            var path2 = System.IO.Path.Combine(ResultsPath, "sq2results");
            path2.CreateDirectory();

            sq.WriteOutputs(refSet[0].Image, path1);
            sq2.WriteOutputs(refSet[0].Image, path2);

            for(int i = 0; i < refSet.Count; i++)
            {
                var img = refSet[i];
                var res = sq.ExecuteSingle(img.Image);
                var res2 = sq2.ExecuteSingle(img.Image);

                img.Image.Dump(System.IO.Path.Combine(ResultsPath, "sq1_result"), res);
                img.Image.Dump(System.IO.Path.Combine(ResultsPath, "sq2_result"), res2);

                var fit = img.ComputeFitness(img.UnionReferenceRegions, FitnessFunction.MCC);
                Assert.AreEqual(fit, 1, 1e-4);
                var resFit = img.ComputeFitness(res, FitnessFunction.MCC);
                var resFit2 = img.ComputeFitness(res2, FitnessFunction.MCC);
                Assert.AreEqual(resFit, resFit2, "Fitness values of original SQ ({0}) and En-/Decoded SQ ({1}) not equal", resFit, resFit2);
                runningFit += resFit;
            }
        }


        internal class HalconPipelineEvaluatorDummy : HalconPipelineEvaluator
        {
            public HalconPipelineEvaluatorDummy(DataLoader<ReferenceImage> train,
                DataLoader<ReferenceImage> val,
                HalconFitnessConfiguration fitConfig) : base(train, val, fitConfig)
            {

            }
        }

        internal class HalconPipelineDummy : HalconPipeline
        {
            public HalconPipelineDummy(DataLoader<ReferenceImage> refSet)
            {
                ReferenceSet = refSet;
                OutputDictionary = refSet.Batches().First().ToDictionary(x => x.Input, x => x.Reference);
            }

            public DataLoader<ReferenceImage> ReferenceSet { get; set; }

            public Dictionary<HObject, HObject> OutputDictionary { get; set; }

            /// <summary>
            /// We'll return the reference for each input in order to evaluate objects against themselves
            /// </summary>
            /// <param name="input"></param>
            /// <returns></returns>
            public override HObject ExecuteSingle(HObject input)
            {
                return OutputDictionary[input];
            }
        }

        /// <summary>
        /// We use a dummy pipeline to compare each reference image's reference regions against itself.
        /// Must return 1 over all reference images.
        ///
        /// Seems like a trivially dumb test, but did actually reveal issues with Halcon and reference images ;)
        /// </summary>
        [Test]
        public void Evaluate()
        {
            var refSet = CommonHImages.ReferenceSetHalcon;
            refSet.FitsIntoMemory = true;
            var loader = new DataLoader<ReferenceImage>(refSet);
            Path.Combine(CommonInformation.TestResultsDirectory, "PipelineEvaluatorTest", "Evaluate").CreateDirectory();
            foreach (var img in loader.Batches().First())
            {
                img.Input.Dump(Path.Combine(CommonInformation.TestResultsDirectory, "PipelineEvaluatorTest", "Evaluate", Path.GetFileNameWithoutExtension(img.Filename)), img.Reference);
            }
            foreach (var fitFunc in new FitnessFunction[]
            {
                FitnessFunction.IntersectionOverUnion, FitnessFunction.Recall,
                FitnessFunction.Precision, FitnessFunction.MCC,
                FitnessFunction.Accuracy,
            })
            {
                var fitConfig = new HalconFitnessConfiguration(new FitnessFunction[] {fitFunc}, new double[] {1});
                var evaluator = new HalconPipelineEvaluator(loader, loader, fitConfig);
                var pipeline = new HalconPipelineDummy(loader);
                
                evaluator.Evaluate(pipeline);
                Assert.AreEqual(1, fitConfig.WeightedFitnessOf(pipeline), $"failed at {fitFunc}");
                evaluator.Evaluate(pipeline as IIndividual);
                Assert.AreEqual(1, fitConfig.WeightedFitnessOf(pipeline), $"failed at {fitFunc}");
            }
        }
    }
}
