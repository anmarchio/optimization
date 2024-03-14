using System.IO;
using System.Linq;
using System.Xml.Serialization;
using NUnit.Framework;
using Optimization.CartesianGeneticProgramming;
using Optimization.CVPipeline.OperatorNodes;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;
using Optimization.Tests;

namespace Optimization.CVPipeline.Tests
{
    [TestFixture]
    public class CVCGPEvaluatorTest
    {
        private static string LargeDataSetPath = @"C:\Users\leen\Pictures\compare_ann_cgp_5000\train";

        [Test]
        public void Evaluate()
        {
            var pipe = CommonCVPipelines.GrayImgMorph_Reg;
            var set = CommonCVImages.ReferenceSetCV;
            var train = new CVDataLoader(set);

            var batch = CommonCVEvolutionStrategies.BuildStandardCVEvolutionStrategy(train, train, pipe.Nodes, 10, 0, 1, Path.Combine(CommonInformation.TestResultsDirectory, "CVBatch"), null);
            var best = batch.Run().FloatVector;
            var config = batch.Configurations.Where(x => x.ConfigurationType == ConfigurationType.CGP).First() as CGPConfiguration;

            //var map = config.OperatorMap as Optimization.Fitness.OperatorMaps.ModularLegacyOperatorMap<IInputArray[], float[], IInputArray>;
            //var p1 = new CVPipeline(map, new CGPDecoder(config), best, config);
            var p1 = new CVPipeline(best, config);
            p1.WriteToDOTFile(Path.Combine(CommonInformation.TestResultsDirectory, "cvpipe.txt"));

        }

        [Test]
        public void CVInpaint()
        {
            var set = CommonCVImages.ColorReferenceSetCV;
            var p1 = CommonCVPipelines.GrayImgMorph_Reg;
            var numImg = set.Count;
            for (int i = 0; i < numImg; i++)
            {
                var img = set[i];
                var result = p1.ExecuteSingle(img.Image);
                result.WriteImage(Path.Combine(CommonInformation.TestResultsDirectory, "pre_inpaint_result_write_cv_image_" + i.ToString() + ".jpg"));
                img.ReferenceImage.WriteImage(Path.Combine(CommonInformation.TestResultsDirectory, "reference_write_cv_image_" + i.ToString() + ".jpg"));
                var paint = img.Image.Inpaint(img.ReferenceImage.ToGray(), result);
                paint.WriteImage(Path.Combine(CommonInformation.TestResultsDirectory, "write_cv_image_" + i.ToString() + ".jpg"));
            }
        }

        [Test]
        public void CVBackgroundSubtractorMOG()
        {
            Assert.Pass("Run only if necessary");

            var sub = new CVBackgroundSubtractorMOG();
            //sub.Input = ReferenceSet[0].Image;

            var result = sub.Execute(CommonCVImages.ReferenceSetCV[0].Image);
            //sub.Input.WriteImage((Path.Combine(CommonInformation.TestResultsDirectory, "backgroundSubtrMOG.jpg")), ReferenceSet[0].ReferenceImage, result);

            var set = CommonCVImages.ReferenceSetCV;

            if (Directory.Exists(LargeDataSetPath))
            {
                set = new CVReferenceSet(LargeDataSetPath, Emgu.CV.CvEnum.ImreadModes.Grayscale);
            }


            var train = new CVDataLoader(set);
            CVBackgroundSubtractorMOG best = sub.Copy() as CVBackgroundSubtractorMOG;
            double bestFit = 0;
            foreach (var p in sub.EnumerateParameters())
            {
                double fit = 0;
                var bseg = new CVBackgroundSubtractorMOG();
                bseg.FromCGPNodeParameters(p);
                foreach (var batch in train.Batches())
                {
                    foreach (var item in batch)
                    {
                        //bseg.Input = item.Image;
                        var res = bseg.Execute(item.Image);
                        fit += item.ReferenceImage.ComputeFitness(res, FitnessFunction.MCC);
                    }
                }
                fit /= train.DataSetSize;
                if (fit > bestFit)
                {
                    bestFit = fit;
                    best = bseg.Copy() as CVBackgroundSubtractorMOG;
                }
            }
            Assert.Pass("best fit: {0}", bestFit);

            using (var writer = new StreamWriter(Path.Combine(CommonInformation.TestResultsDirectory, "best_MOG.txt")))
            {
                var xml = new XmlSerializer(typeof(CVBackgroundSubtractorMOG));
                xml.Serialize(writer, best);
            }
        }

        [Test]
        public void Intersection()
        {
            var img0 = CommonCVImages.ReferenceSetCV[0].ReferenceImage;

            var positives = img0.Positives();
            var intersection = img0.Intersect(img0);
            var intersectionPositives = intersection.Positives();

            CommonCVImages.ReferenceSetCV[0].Image.WriteImage(Path.Combine(CommonInformation.TestResultsDirectory, "intersection_0_0.jpg"), intersection);

            Assert.Greater(positives, 0);
            Assert.AreEqual(positives, intersectionPositives);

            intersection = img0.Intersect(CommonCVImages.ReferenceSetCV[1].ReferenceImage);

            CommonCVImages.ReferenceSetCV[0].Image.WriteImage(Path.Combine(CommonInformation.TestResultsDirectory, "intersection_0_1.jpg"), intersection);


        }

        [Test]
        public void Difference()
        {
            var img0 = CommonCVImages.ReferenceSetCV[0].ReferenceImage;

            var positives = img0.Positives();
            var difference = img0.Difference(img0);

            CommonCVImages.ReferenceSetCV[0].Image.WriteImage(Path.Combine(CommonInformation.TestResultsDirectory, "difference_0_0.jpg"),
                difference);
            Assert.AreEqual(difference.Positives(), 0);


            var img1 = img0.Invert();
            difference = img0.Difference(img1);
            Assert.AreEqual(difference.Positives(), img0.Positives()); // because intersection should be empty

            CommonCVImages.ReferenceSetCV[0].Image.WriteImage(Path.Combine(CommonInformation.TestResultsDirectory, "difference_0_0_inv.jpg"),
                difference);

            difference = img0.Difference(CommonCVImages.ReferenceSetCV[1].ReferenceImage);
            var intersection = img0.Intersect(CommonCVImages.ReferenceSetCV[1].ReferenceImage);

            CommonCVImages.ReferenceSetCV[0].Image.WriteImage(Path.Combine(CommonInformation.TestResultsDirectory, "difference_0_1.jpg"),
                difference, intersection);
        }

        [Test]
        public void Union()
        {
            var img0 = CommonCVImages.ReferenceSetCV[0].ReferenceImage;
            var union = img0.Union(img0);
            Assert.AreEqual(img0.Positives(), img0.Positives());
            CommonCVImages.ReferenceSetCV[0].Image.WriteImage(Path.Combine(CommonInformation.TestResultsDirectory, "union_0_0.jpg"), union);
        }


        [Test]
        public void Positives()
        {
            var actual = CommonCVImages.ReferenceSetCV[0].ReferenceImage;
            var reference = CommonCVImages.ReferenceSetCV[2].ReferenceImage; // remember: img2 is empty (so "no positives")
            var tp = reference.TruePositives(actual);
            Assert.AreEqual(tp, reference.Positives());
            tp = reference.TruePositives(actual);
            Assert.AreEqual(0, tp);
            var fn = reference.FalseNegatives(actual);
            Assert.AreEqual(tp + fn, reference.Positives());
            var fp = reference.FalsePositives(actual);
            Assert.AreEqual(tp + fp, actual.Positives());
        }

        [Test]
        public void Negatives()
        {
            var reference = CommonCVImages.ReferenceSetCV[0].ReferenceImage;
            var actual = CommonCVImages.ReferenceSetCV[1].ReferenceImage;
            var size = reference.GetInputArray().GetSize();

            Assert.AreEqual(reference.Invert().Positives(), size.Height * size.Width - reference.Positives());
            Assert.AreEqual(reference.Invert().Negatives(), size.Height * size.Width - reference.Negatives());

            var tn = reference.TrueNegatives(actual);
            var fp = reference.FalsePositives(actual);
            Assert.AreEqual(tn + fp, reference.Negatives());
            var fn = reference.FalseNegatives(actual);
            Assert.AreEqual(tn + fn, actual.Negatives());

            Assert.AreEqual(reference.Positives() + reference.Negatives(), size.Height * size.Width);
            Assert.AreEqual(actual.Positives() + actual.Negatives(), size.Height * size.Width);
        }
    }
}
