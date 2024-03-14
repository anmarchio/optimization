using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions;
using HalconDotNet;
using NUnit.Framework;
using Optimization.HPipeline.Serialization;
using Optimization.Tests;
using Optimization.Tests.Categories;

namespace Optimization.HPipeline.Tests.SerializationTests
{
    [TestFixture]
    public class IndexedHObjectConverterTest
    {   
        [Test,ShortTest]
        public void Inverse()
        {
            var dir = Path.Combine(CommonHImages.ImageFormatConversionDirectory, "Indexed");
            var images = Path.Combine(dir, "images");
            var labels = Path.Combine(dir, "labels");
            var converter = new IndexedHObjectConverter(dir, dir);

            Assert.IsTrue(IndexedHObjectConverter.IsIndexedColorData(dir));

            converter.ConvertIndexedToHObject();

            var convertDir = Path.Combine(CommonInformation.TestResultsDirectory, "Converted");

            converter.DirectoryOut = convertDir;
                
            Assert.IsTrue(IndexedHObjectConverter.IsRegionMarkerData(dir));

            converter.ConvertHObjectToIndexed();

            converter.DirectoryIn = convertDir;
            converter.DirectoryOut = convertDir;

            converter.ConvertIndexedToHObject();

            var originalRegions = Directory.EnumerateFiles(Path.Combine(dir, "regions")).Where(x => x.EndsWith(".hobj"));
            var convRegionsPath = Path.Combine(convertDir, "regions");
            var convRegions = Directory.EnumerateFiles(convRegionsPath);
            Assert.AreEqual(convRegions.Count(), originalRegions.Count());

            foreach (var reg1 in originalRegions)
            {
                HObject region1, region2;
                HOperatorSet.ReadRegion(out region1, reg1);
                HOperatorSet.ReadRegion(out region2, Path.Combine(convRegionsPath, Path.GetFileName(reg1)));
                Assert.AreNotEqual(region1.TestEqualObj(region2), 0);
            }
        }

        [Test,ShortTest]
        public void MultipleInverse()
        {
            var dir = Path.Combine(CommonHImages.ImageFormatConversionDirectory, "IndexedMultilabel");
            var images = Path.Combine(dir, "images");
            var labels = Path.Combine(dir, "labels");
            var convertDir = Path.Combine(CommonInformation.TestResultsDirectory, "ConvertedMultilabel");
            convertDir.CreateDirectory();
            var converter = new IndexedHObjectConverter(dir, convertDir);


            var labelIndexDict = new Dictionary<string, int>
            {
                {"Contaminant", 1 },
                {"FiberCrack", 2 },
                {"LooseFilament", 3 }
            };
            var indexLabelDict = labelIndexDict.Invert();
            converter.LabelToIndex = labelIndexDict;
            converter.IndexToLabel = indexLabelDict;

            Assert.IsTrue(IndexedHObjectConverter.IsRegionMarkerData(dir));
            converter.ConvertHObjectToIndexed();

            converter.DirectoryIn = convertDir;
            Assert.IsTrue(IndexedHObjectConverter.IsIndexedColorData(convertDir));
            converter.ConvertIndexedToHObject();
            Assert.IsTrue(IndexedHObjectConverter.IsRegionMarkerData(dir));

            var originalRegions = Directory.EnumerateFiles(Path.Combine(dir, "regions")).Where(x => x.EndsWith(".hobj"));
            var convRegionsPath = Path.Combine(convertDir, "regions");
            var convRegions = Directory.EnumerateFiles(convRegionsPath);
            Assert.AreEqual(convRegions.Count(), originalRegions.Count());
            foreach (var reg1 in originalRegions)
            {
                HObject region1, region2;
                HOperatorSet.ReadRegion(out region1, reg1);
                HOperatorSet.ReadRegion(out region2, Path.Combine(convRegionsPath, Path.GetFileName(reg1)));
                Assert.AreNotEqual(region1.TestEqualObj(region2), 0);
            }
        }
    }
}
