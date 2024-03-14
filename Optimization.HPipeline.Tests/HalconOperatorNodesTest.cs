using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using NUnit.Framework;
using Optimization.Tests;
using Optimization.Tests.Categories;

namespace Optimization.HPipeline.Tests
{

    [TestFixture]
    public class HalconOperatorNodesTest
    {

        private void TestXmlSerialization(HalconOperatorNode node)
        {
            var path = Path.Combine(CommonInformation.TestResultsDirectory, "node_xml_test.txt");
            var xml = new XmlSerializer(node.GetType());
            using (var writer = new StreamWriter(path))
            {
                xml.Serialize(writer, node);
            }
            using (var reader = new StreamReader(path))
            {
                var tmp = xml.Deserialize(reader) as HalconOperatorNode;


                AreEqual(node, tmp);
            }
        }


        [Test,ShortTest]
        public void SerializeHalconOperatorNodeNamespace()
        {
            foreach(var node in CommonHalconPipelines.HalconOperatorNodeCollection)
            { 
                TestXmlSerialization(node);
            }
        }

        [Test, LongTest]
        public void PrintOperatorNodeAsHalconCode()
        {
            var dummyInput = new HalconInputNode() { ProgramInputIdentifier = -1, NodeID = -1 };
            foreach(var node in CommonHalconPipelines.GetFastHalconOperatorNodes(Path.Combine(CommonInformation.Directory,
                "..", "..", "..", "Optimization.Commandline", "Operators", "Halcon", "fast.xml")))
            {
                try
                {
                    node.AddChild(dummyInput);
                    var varName = node.OutputVariableName;
                    var codeCall = node.HalconFunctionCall();
                }
                catch (Exception)
                {
                    Assert.Fail("Not all OperatorNodes implement the necessary OutputVariableName and HalconFunctionCall: {0}", node.GetType().Name);
                }
            }
        }

        public static bool AreEqual(HalconOperatorNode node1, HalconOperatorNode node2)
        {
            if (!node1.GetType().Equals(node2.GetType())) return false;

            Assert.NotNull(node1.CGPParameterBounds);
            Assert.NotNull(node2.CGPParameterBounds);          
            var properties = node1.GetType().GetProperties().ToList().Where(x => x.CanRead && x.CanWrite);


            foreach (var prop in properties)
            {
                var val1 = prop.GetValue(node1);
                var val2 = prop.GetValue(node2);
                if (val1 == null && val2 == null)
                    continue;
                if (val1 == null ^ val2 == null)
                    return false;
                    //Assert.Fail("val1 xor val2 is null for property: {0} in node: {1}", prop.Name, node1.GetType().Name);
                if (val1.GetType().IsPrimitive)
                {
                    if(!val1.Equals(val2))
                        return false;
                    //Assert.AreEqual(val1, val2, "failed at property: {0} of node: {1}:", prop.Name, node1.GetType().Name);
                }
            }
            return true;
        }

        [Test,ShortTest]
        public void CopyHalconOperatorNode()
        {
            var types = CommonHalconPipelines.HalconOperatorNodeCollection.Select(x => x.GetType());

            foreach (var type in types)
            {
                var node = Activator.CreateInstance(type) as HalconOperatorNode;
                var copy = node.Copy() as HalconOperatorNode;
                AreEqual(node, copy);
            }
        }

        [Test, LongTest]
        public void EnumerateParameters()
        {
            var types = CommonHalconPipelines.HalconOperatorNodeCollection.Select(x => x.GetType());

            foreach (var type in types)
            {
                var node = Activator.CreateInstance(type) as HalconOperatorNode;
                var p = node.EnumerateParameters();
                var count = p.Count() < 5 ? p.Count() : 5;
                foreach(var param in p.Take(count))
                {
                    node.FromCGPNodeParameters(param);
                    var copy = node.Copy() as HalconOperatorNode;
                    AreEqual(copy, node);
                    TestXmlSerialization(copy);
                }
            }
        }
    }
}
