using NUnit.Framework;
using Optimization.CVPipeline.CVCGP;

namespace Optimization.CVPipeline.Tests.Miscellaneous
{
    public class OperatorMapTests
    {
        public void Serialize()
        {
            Assert.Warn(("Not implemented"));
            var cvMap = new CVOperatorMap(CommonCVPipelines.NodeCollection);
            
            //Optimization.Tests.SerializationTests.ConfigurationTests.AssertEqual(cvMap);
        }
    }
}
