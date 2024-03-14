
using NUnit.Framework;
using PRIME.Optimization.CartesianGeneticProgramming;
using PRIME.Optimization.PiEvolution;
using PRIME.Optimization.Visualization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRIME.Optimization.Tests.VisualizationTests
{
    [TestFixture]
    public class InformationDataPairTest
    {
      //  [Test]
        public void Serialize()
        {
            var data = new DataSeries(new double[] { 2, 4, 5, 1, 2 });
            var info = new CGPConfiguration(4, 5, 3, 2, 1, new FiOperatorMap(), 7, 8);
            var pair = new InformationDataPair();
            pair.AddData(data, "testEntry");
            pair.AddInformation(info, "testEntry");

            pair.Serialize("test.txt");

            var test = InformationDataPair.Deserialize("test.txt");
        }
    }
}
