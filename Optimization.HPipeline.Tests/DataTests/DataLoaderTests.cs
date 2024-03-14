using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Extensions;
using NUnit.Framework;
using Optimization.CartesianGeneticProgramming;
using Optimization.Data;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Random;
using Optimization.Fitness;
using Optimization.HPipeline.Fitness;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.Tests;
using Optimization.Tests.Categories;

namespace Optimization.HPipeline.Tests.DataTests
{
    [TestFixture]
    public class DataLoaderTests
    {

        private class TestDataSet : DataSet<int>
        {
            private int[] Data { get; set; }
            public TestDataSet(int numElements)
            {
                Data = Enumerable.Range(0, numElements).ToArray();
            }

            public override int this[int i]
            {
                get
                {
                    return Data[i];
                }
            }

            public override int Count
            {
                get
                {
                    return Data.Length;
                }
            }

            public override bool FitsIntoMemory { get; set; } = true;
        }

        private class DoesNotFitInMemoryRefSet : ReferenceSet
        {
            public DoesNotFitInMemoryRefSet(string folder) : base(folder)
            {

            }

            public override bool FitsIntoMemory { get; set; } = false;

        }


        [Test,ShortTest]
        public void LoadReferenceSet()
        {
            // load using old directory structure

            var refSet = CommonHImages.ReferenceSetHalcon;
            var refSetColor = CommonHImages.ReferenceSetHalconColor;
            var imagePaths = Directory.EnumerateFiles(CommonHImages.SampleImageDirectory).Select(x => Path.GetFileName(x));
            var labelPaths = new Dictionary<string, IEnumerable<string>>();
            foreach (var imagePath in imagePaths)
            {
                var imgFilename = Path.GetFileName(imagePath);
                var oldDirectoryName = imgFilename;
                var region = Directory.EnumerateFiles(Path.Combine(CommonHImages.SampleImageDirectory, "regions", oldDirectoryName)).Select(x => Path.GetFileName(x));
                labelPaths.Add(imagePath, region.Select(x => Path.Combine("regions", oldDirectoryName, x)));
            }
            var refSetList = new FileListReferenceSet(CommonHImages.SampleImageDirectory,
                Directory.EnumerateFiles(CommonHImages.SampleImageDirectory).Select(x => Path.GetFileName(x)),
                labelPaths);
            Assert.AreEqual(refSet.Count, refSetList.Count);
        }


        private void CheckImages(ReferenceSet refSet)
        {
            for(int i = 0; i < refSet.Count; i++)
            {
                var image = refSet[i];
                Assert.NotNull(image.Image, "Input image seems to be null");
                Assert.NotNull(image.Input, "Input objects seem to be null");
                Assert.True(image.Image.IsInitialized(), "Halcon iconic object (image) is not initialized");
                Assert.True(image.Input.IsInitialized(), "Halcon iconic object (hobj)  is not initialized");
            }
        }

        /// <summary>
        /// Checks if images get disposed prematurely.
        /// </summary>
        [Test]
        public void TestMemory()
        {
            var refSet = new DoesNotFitInMemoryRefSet(CommonHImages.SampleImageDirectory);

            CheckImages(refSet);

            var dumpDir = Path.Combine(CommonInformation.TestResultsDirectory, "TestMemoryEvolution");
            dumpDir.CreateDirectory();

            var opMap = new HalconOperatorMap(CommonHalconPipelines.GetFastHalconOperatorNodes(Path.Combine(CommonInformation.Directory,
                "..", "..", "..", "Optimization.Commandline", "Operators", "Halcon", "fast.xml")));
            var config = opMap.ToCGPConfiguration(rows: 10, programInputCount: 1, programOutputCount: 1);

            var batch = CommonHalconEvolutionStrategies.SelfAdaptiveEvolutionStrategy(config, refSet, refSet, 1, 2, dumpDir);
            batch.Run();

            batch = CommonHalconEvolutionStrategies.BuildStandardCGPEvolutionStrategy(config, refSet, refSet, 1, iterations: 2, saveDirectory: dumpDir);
            batch.Run();

            CheckImages(refSet);

            Directory.Delete(dumpDir, recursive: true);
        }


        /// <summary>
        /// Testing if shuffling items still returns all expected elements
        /// </summary>
        [Test,ShortTest]
        public void Shuffle()
        {
            var random = new MersenneTwister(DateTime.Now.Millisecond);
            var testData = new TestDataSet(random.Next(5000));
            var dataLoader = new DataLoader<int>(testData, 7, random);

            int elementsCount = 0;
            foreach (var batch in dataLoader.Batches())
            {
                foreach (var number in batch)
                {
                    Assert.NotNull(number);
                    elementsCount++;
                }
            }
            Assert.AreEqual(elementsCount, testData.Count);
        }


        [Test,ShortTest]
        public void Batches()
        {
            Assert.Pass("Only run this if memory leaks in DataLoader are suspected");
            var dir = "/evias/eval_data/Batteriebleche/Dunkelfeld_convert";
            //var dir = @"C:\Users\leen\Pictures\carbon_convert\train";
            //var dir = @"C:\Users\leen\Pictures\Dunkelfeld_convert";
            var il = dir.GetImagesList();
            var refSet = new FileListReferenceSet(dir, il, il.GetLabelDictionary());
            var loader = new DataLoader<ReferenceImage>(refSet, batchSize: 1) { QueueSize = 1 };

            try
            {
                for(int i = 0; i < 50; i++)
                {
                    foreach(var batch in loader.Batches())
                    {
                        // do nothing
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                Assert.Warn("Directory {dir} not found. This test is aimed at the gitlab-runner.");
            }
        }
    }
}
