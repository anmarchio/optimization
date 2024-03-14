using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRIME.Optimization.Fitness;
using HalconDotNet;
using System.IO;

namespace OptimizationTests.FitnessTests
{
    [TestClass]
    public class PipelineConfusionMatrixTest : PipelineConfusionMatrix
    {
        public PipelineConfusionMatrixTest()
        {
        }

        [TestMethod]
        public new void F2Score()
        {
            HTuple square1, square2;
            square1 = new HTuple(new int[] { 0, 1, 0, 1 });
            square2 = new HTuple(new int[] { 0, 0, 1, 1 });
            // check if MCC is 1 for equal objects
            HObject o1, o2;
            HOperatorSet.GenRegionPoints(out o1, square1, square2);
            HOperatorSet.GenRegionPoints(out o2, square1, square2);

            var matrix = new PipelineConfusionMatrix(o1, o1, 16);
            Assert.AreEqual(1, matrix.F2Score());

            var obj = new HObject();

            var dir = Directory.GetCurrentDirectory();
            dir = dir.Replace("\\bin\\Debug", "");

            string[] s = new string[]
            {
                dir + @"\FitnessTests\SampleImage\1.bmp",
                dir + @"\FitnessTests\SampleImage\3.bmp",
                dir + @"\FitnessTests\SampleImage\4.bmp",
                dir + @"\FitnessTests\SampleImage\57.bmp",
                dir + @"\FitnessTests\SampleImage\65.bmp",
                dir + @"\FitnessTests\SampleImage\72.bmp"
            };

            var refSet = new ReferenceSet(s);

            for (int j = 0; j < refSet.Count; j++)
            {
                var refImage = refSet[j];
                refImage.Dump();
                var regionCount = refImage.RegionCount;
                for (int i = 0; i < regionCount; i++)
                {
                    var region = refImage[i];
                    matrix = new PipelineConfusionMatrix(region, region, refImage.SmallestOuterRectangleSize(i));
                    Assert.AreEqual(1, matrix.F2Score());
                }
            }

            for (int j = 0; j < refSet.Count; j++)
            {
                var refImage = refSet[j];
                refImage.Dump();
                var regionCount = refImage.RegionCount;
                for (int i = 0; i < regionCount; i++)
                {
                    var region = refImage[i];
                    matrix = new PipelineConfusionMatrix(o1, region, refImage.SmallestOuterRectangleSize(i));
                    Assert.AreEqual(0, matrix.F2Score());
                }
            }
        }
    }
}
