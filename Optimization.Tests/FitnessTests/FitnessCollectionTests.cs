using NUnit.Framework;
using Optimization.Fitness;
using Optimization.Tests.Categories;

namespace Optimization.Tests.FitnessTests
{
    [TestFixture]
    public class FitnessCollectionTests
    {
        [Test,ShortTest]
        public void PrecisionRecall()
        {
            // Check border cases
            Assert.AreEqual(Collection.Precision(tp: 0, fp: 0), 0);
            Assert.AreEqual(Collection.Recall(tp: 0, fn: 0), 0);
            Assert.AreEqual(Collection.Precision(10, 0), 1);
            Assert.AreEqual(Collection.Recall(0, 0), 0);
            Assert.AreEqual(Collection.Precision(5, 5), 0.5);
            Assert.AreEqual(Collection.Recall(5, 5), 0.5);
        }


        [Test,ShortTest]
        public void Accuracy()
        {
            // Check border cases
            Assert.AreEqual(Collection.Accuracy(0, 0, 0, 0), 0);
            Assert.AreEqual(Collection.Accuracy(5, 5, 5, 5), 0.5);
            Assert.AreEqual(Collection.Accuracy(5, 0, 0, 0), 1);
            Assert.AreEqual(Collection.Accuracy(0, 5, 0, 0), 1);
            Assert.AreEqual(Collection.Accuracy(0, 0, 5, 0), 0);
            Assert.AreEqual(Collection.Accuracy(0, 0, 0, 5), 0);
        }

        [Test,ShortTest]
        public void MCC()
        {
            // From Wikipedia
            Assert.AreEqual(Collection.MCC(90, 1, 4, 5), 0.1352420307, delta: 0.00001);
        }

        [Test,ShortTest]
        public void IntersectionOverUnion()
        {
            Assert.AreEqual(Collection.IntersectionOverUnion(5, 10), 0.5);
            Assert.AreEqual(Collection.IntersectionOverUnion(1000, 2000), Collection.IntersectionOverUnion(1000d, 2000d));
        }
    }
}
