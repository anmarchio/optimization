using System.IO;
using NUnit.Framework;

namespace Optimization.CVPipeline.Tests.Miscellaneous
{
    [TestFixture]
    public class DataLoaderTest
    {
        private class CVReferenceHuge : CVReferenceSet
        {
            public CVReferenceHuge(string directory) : base(directory, Emgu.CV.CvEnum.ImreadModes.Grayscale)
            {
            }
        }

        /// <summary>
        /// Ideally we'd populate the database with dummy data to ease the process of testing whether the data is read properly from the db.
        /// Manually starting batch runs all the time is rather tedious.
        /// </summary>
        [TearDown]
        public void DataBaseEntries()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        [SetUp]
        public void PopulateDataBase()
        {

        }


        [Test]
        public void HugeDataSetEvolution()
        {
            Assert.Pass("Waste of everybody's time.");
            var refSet = new CVReferenceSet(@"C:\Users\leen\Pictures\compare_ann_cgp_5000\train", Emgu.CV.CvEnum.ImreadModes.Grayscale);
            var pipe = CommonCVPipelines.NodeCollection;
            var train = new CVDataLoader(refSet, batchSize: 12);
            train.GetFullSet();

            //var batch = CommonCVEvolutionStrategies.BuildStandardCVEvolutionStrategy(train, train, pipe, 10, 0, 1, Path.Combine(CommonInformation.TestResultsDirectory, "CVBatch"), null, false);
            //batch.Run();
        }


        [Test]
        public void LoadReferenceSet()
        {
            // load using old directory structure
            var refSetColor = CommonCVImages.ColorReferenceSetCV;
            var refSetGray = CommonCVImages.ReferenceSetCV;

            // load using list of individual paths
            var refSetColorList = new CVReferenceSet(CommonCVImages.IndexedImagesPath,
                Directory.EnumerateFiles(Path.Combine(CommonCVImages.IndexedImagesPath, "images")),
                Directory.EnumerateFiles(Path.Combine(CommonCVImages.IndexedImagesPath, "labels")),
                imgType: Emgu.CV.CvEnum.ImreadModes.AnyColor);

            var refSetGrayList = new CVReferenceSet(CommonCVImages.IndexedImagesPath,
                Directory.EnumerateFiles(Path.Combine(CommonCVImages.IndexedImagesPath, "images")),
                Directory.EnumerateFiles(Path.Combine(CommonCVImages.IndexedImagesPath, "labels")),
                imgType: Emgu.CV.CvEnum.ImreadModes.Grayscale);

            Assert.Greater(refSetColor.Count, 0);
            Assert.Greater(refSetColorList.Count, 0);

            Assert.AreEqual(refSetColor.Count, refSetColorList.Count);
            Assert.AreEqual(refSetGray.Count, refSetGrayList.Count);
        }

    }
}
