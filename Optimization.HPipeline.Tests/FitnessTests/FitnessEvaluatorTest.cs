using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using NUnit.Framework;
using Optimization.Data;
using Optimization.HPipeline.Fitness;
using Optimization.Tests.Categories;
using Collection = Optimization.HPipeline.Fitness.Collection;

namespace Optimization.HPipeline.Tests.FitnessTests
{
    [TestFixture]
    public class FitnessEvaluatorTest
    {
        [Test,ShortTest]
        public void Precision()
        {
            // check if precision is 1 for equal objects
            HObject o1, o2;
            HOperatorSet.GenRandomRegion(out o1, 128, 128);
            o2 = new HObject(o1);
            var precision = Collection.Precision(o2, o1);
            Assert.AreEqual(1, precision);

            // check if precision is zero for hobjects with an empty intersection

            HTuple square1, square2;
            square1 = new HTuple(new int[] { 0, 1, 2, 3, 4, 5 });
            square2 = new HTuple(new int[] { 6, 7, 8, 9, 10, 11 });
            HOperatorSet.GenRegionPoints(out o1, square1, square1);
            HOperatorSet.GenRegionPoints(out o2, square2, square2);

            precision = Collection.Precision(o2, o1);

            Assert.AreEqual(0, precision);

        }
        [Test,ShortTest]
        public void Recall()
        {

            // check if recall is 1 for equal objects
            HObject o1, o2;
            HOperatorSet.GenRandomRegion(out o1, 128, 128);
            o2 = new HObject(o1);
            var recall = Collection.Recall(o2, o1);
            Assert.AreEqual(1, recall);

            // check if recall is zero for hobjects with an empty intersection

            HTuple square1, square2;
            square1 = new HTuple(new int[] { 0, 1, 2, 3, 4, 5 });
            square2 = new HTuple(new int[] { 6, 7, 8, 9, 10, 11 });
            HOperatorSet.GenRegionPoints(out o1, square1, square1);
            HOperatorSet.GenRegionPoints(out o2, square2, square2);

            recall = Collection.Recall(o2, o1);

            Assert.AreEqual(0, recall);
        }

        [Test,ShortTest]
        public void MCC()
        {
            /*
            ReferenceSet = new ReferenceSetMock();
            var set = ReferenceSet as ReferenceSetMock;
            var image = new ReferenceImageMock();
            image.SetPixelCount(16);
            set.AddReferenceImage(image);
            */

         
            HTuple square1, square2;
            square1 = new HTuple(new int[] { 0, 1, 0, 1});
            square2 = new HTuple(new int[] { 0, 0, 1, 1});
            // check if MCC is 1 for equal objects
            HObject o1, o2;
            HOperatorSet.GenRegionPoints(out o1, square1, square2);
            HOperatorSet.GenRegionPoints(out o2, square1, square2);

            o2 = new HObject(o1);
            var MCC = Collection.MCC(o2, o1, height: 4, width: 4);

            // check if recall is -1 for hobjects with an empty intersection

            HOperatorSet.GenRegionPoints(out o1, square1, square2);
            square1 = new HTuple(new int[] { 2, 3, 2, 3 });
            square2 = new HTuple(new int[] { 2, 2, 3, 3 });
            HOperatorSet.GenRegionPoints(out o2, square1, square2);

            double tp = 0, fp = 4, tn = 8, fn = 4;
            var tmp = Optimization.Fitness.Collection.MCC(tp, tn, fp, fn);

            MCC = Collection.MCC(o2, o1, height:4, width:4);

            Assert.AreEqual(-0.3333333333333333, MCC);


            square1 = new HTuple(new int[] { 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3 });
            square2 = new HTuple(new int[] { 0, 1, 2, 0, 1, 2, 0, 1, 2, 0, 1, 2 });
            HOperatorSet.GenRegionPoints(out o1, square1, square2);

            square1 = new HTuple(new int[] { 0, 1, 2, 3 });
            square2 = new HTuple(new int[] { 3, 3, 3, 3 });
            HOperatorSet.GenRegionPoints(out o2, square1, square2);
            Assert.AreEqual(Collection.MCC(o2, o1, height:4, width:4), -1);
        }

        /// <summary>
        /// Check if IntersectionOverUnion returns sensible values.
        /// </summary>
        [Test,ShortTest]
        public void IoU()
        {
            HTuple square1, square2;
            square1 = new HTuple(new int[] { 0, 1, 0, 1 });
            square2 = new HTuple(new int[] { 0, 0, 1, 1 });
            // check if iou is 1 for equal objects
            HObject o1, o2;
            HOperatorSet.GenRegionPoints(out o1, square1, square2);
            HOperatorSet.GenRegionPoints(out o2, square1, square2);

            Assert.AreEqual(1, Collection.IntersectionOverUnion(o1, o1));
            Assert.AreEqual(1, Collection.IntersectionOverUnion(o2, o2));
            
            // check if iou is 0 for hobjects with an empty intersection
            HOperatorSet.GenRegionPoints(out o1, square1, square2);
            square1 = new HTuple(new int[] { 2, 3, 2, 3 });
            square2 = new HTuple(new int[] { 2, 2, 3, 3 });
            HOperatorSet.GenRegionPoints(out o2, square1, square2);

            Assert.AreEqual(0, Collection.IntersectionOverUnion(o2, o1));
            
            // larger objects has 4 pixels, smaller 2 and is contained in the larger
            square1 = new HTuple(new int[] { 2, 3 });
            square2 = new HTuple(new int[] { 3, 3 });
            HOperatorSet.GenRegionPoints(out o1, square1, square2);
            Assert.AreEqual(0.5, Collection.IntersectionOverUnion(o2, o1));
        }


        /// <summary>
        /// We want to know if using the concatenate functionality from pipelines
        /// allows us to evaluate them against a dataset and return the best possible concatenation
        ///
        /// Below we check if using EvaluateConcatenation throws errors and, if not, 
        /// </summary>
        [Test, ShortTest]
        public void EvaluateConcatenation()
        {
            var refSet = CommonHImages.ReferenceSetHalcon;
            var loader = new DataLoader<ReferenceImage>(refSet);
            var fitConfig = new HalconFitnessConfiguration();
            // config only needed if we convert from floatvector to pipeline
            var evaluator = new HalconPipelineCGPEvaluator(configuration: null,
                trainDataLoader: loader,
                valDataLoader: loader,
                fitConfig: fitConfig);

            var appendPipelines = new List<HalconPipeline>
            {
                CommonHalconPipelines.GetClosing(5, 5),
                CommonHalconPipelines.GetOpening(5, 5),
                CommonHalconPipelines.GetSelectShape(5, 99999)
            };

            // if a pipeline was appended properly,
            // we'd expect that the resulting pipeline has nodecount of the original pipeline
            // + the nodecount of the appended pipeline (without any InputNodes)
            var nodeCounts = appendPipelines.Select(x => x.Nodes.Count(y => !y.IsInputNode));

            foreach (var pipeline in CommonHalconPipelines.Collection)
            {
                var pipe = evaluator.EvaluateConcatenation(pipeline, appendPipelines);
                Assert.IsTrue(nodeCounts.Select(x => x + pipeline.Nodes.Count(y => !y.IsInputNode))
                    .Contains(pipe.Nodes.Count(x => !x.IsInputNode)));
            }

        }
  
    }
}

