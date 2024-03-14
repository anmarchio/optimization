using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using NUnit.Framework;
using Optimization.CartesianGeneticProgramming;
using Optimization.Fitness;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.Serialization.Interfaces;
using Optimization.Tests;
using Optimization.Tests.Categories;

namespace Optimization.HPipeline.Tests.SerializationTests
{
    /// <summary>
    /// Goal of this test class is to test whether serializing/deserializing an object using its supported serialization types (binary, xml) yields an
    /// identical object as well as asserting that at least one serialization type is supported
    /// 
    /// tests are contingent on there being a sensible implementation of the Equals() method.
    /// </summary>
    [TestFixture]
    public class ConfigurationTests
    {

        private static ISupportsSerialization XmlSerialize(ISupportsSerialization conf)
        {
            var path = Path.Combine(CommonInformation.TestResultsDirectory, "conf_ser.xml");
            if (conf.SerializeXmlSupported)
            {
                conf.SerializeXml(path);

                using (var reader = new StreamReader(path))
                {
                    var xml = new XmlSerializer(conf.GetType());
                    var convXml = xml.Deserialize(reader) as ISupportsSerialization;
                    return convXml;
                }

            }
            else
            {
                return null;
            }
        }

        private static ISupportsSerialization BinarySerialize(ISupportsSerialization conf)
        {
            var path = Path.Combine(CommonInformation.TestResultsDirectory, "conv_bin");
            if (conf.SerializeBinarySupported)
            {
                conf.SerializeBinary(path);

                using (var reader = File.OpenRead(path))
                {
                    var xml = new BinaryFormatter();
                    var convXml = xml.Deserialize(reader) as ISupportsSerialization;
                    return convXml;
                }

            }
            else
            {
                return null;
            }
        }

        public static void AssertEqual(ISupportsSerialization serializable)
        {
            if (!serializable.SerializeBinarySupported && !serializable.SerializeXmlSupported) Assert.Fail("At least one serialization type must be supported.");

            var des = XmlSerialize(serializable);
            if (des != null)
            {
                Assert.AreEqual(serializable, des);
            }

            des = BinarySerialize(serializable);
            if (des != null)
            {
                Assert.AreEqual(serializable, des);
            }
        }


        [Test,ShortTest]
        public void SerializeESConfiguration()
        {
            var esConf = new EvolutionStrategy.ESConfiguration();

            AssertEqual(esConf);
        }

        [Test,ShortTest]
        public void SerializeCGPConfiguration()
        {
            var cgpConf = new CGPConfiguration(10, 10, 1, 3, 2, new HalconOperatorMap(CommonHalconPipelines.HalconOperatorNodeCollection), 3, 2); // operatormap is currently not being serialized. this is a serious downfall

            AssertEqual(cgpConf);

        }

        [Test,ShortTest]
        public void SerializeOperatorMaps()
        {
            Assert.Warn("This is currently not supported. Will be implemented after operator maps and configurations have been merged.");

            var halcon = new HalconOperatorMap(CommonHalconPipelines.HalconOperatorNodeCollection);

            //AssertEqual(halcon);
        }

        [Test,ShortTest]
        public void SerializeFitnessConfiguration()
        {
            Assert.Warn("This is pointless at the moment, the equality check compares reference, not actual content.");
            var fit = new FitnessConfiguration(FitnessFunction.MCC, null, null, null);
            //AssertEqual(fit);
        }
        
        [Test,ShortTest]
        public void SerializePiSet()
        {
            Assert.Pass("not implemented -- unsure if current version will stay for long.");
        }

    }
}
