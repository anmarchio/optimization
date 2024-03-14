using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Emgu.CV;
using Extensions;
using NUnit.Framework;
using Optimization.CartesianGeneticProgramming;
using Optimization.CVPipeline.CVCGP;
using Optimization.CVPipeline.OperatorNodes;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Random;
using Optimization.Fitness.ErrorHandling;
using Optimization.Pipeline;
using Optimization.Tests;

namespace Optimization.CVPipeline.Tests
{
    [TestFixture]
    public class CVPipelineTests
    {

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            // or
            Directory.SetCurrentDirectory(CommonInformation.TestResultsDirectory);
        }

        public static bool AreEqual(CVNode node1, CVNode node2)
        {

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
                    Assert.Fail("val1 xor val2 is null for property: {0} in node: {1}", prop.Name, node1.GetType().Name);
                if (val1.GetType().IsPrimitive || val1.GetType().IsEnum)
                {
                    Assert.AreEqual(val1, val2, "failed at property: {0} of node: {1}:", prop.Name, node1.GetType().Name);
                }
            }
            return true;
        }

        private void TestXmlSerialization(CVNode node)
        {
            var path = Path.Combine(CommonInformation.TestResultsDirectory, "node_xml_test.txt");
            var xml = new XmlSerializer(node.GetType());
            using (var writer = new StreamWriter(path))
            {
                xml.Serialize(writer, node);
            }
            using (var reader = new StreamReader(path))
            {
                var tmp = xml.Deserialize(reader) as CVNode;
                AreEqual(node, tmp);
            }
        }

        [Test]
        public void SerializeCVNodeNamespace()
        {
            foreach (var node in CommonCVPipelines.NodeCollection)
            {
                TestXmlSerialization(node);
            }
        }

        [Test]
        public void CVGrayImgToSobelToMorphToReg()
        {
            var pipeline = CommonCVPipelines.GrayImgMorph_Reg;
            var img = CommonCVImages.StandardFiberCV;//.ToGray();

            var result = pipeline.ExecuteSingle(img);

            var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CVSimplePipelineOutput");
            dir.CreateDirectory();
            pipeline.WriteOutputs(img, dir);
        }

        [Test]
        public void CVGausImgToSobelToMorphToReg()
        {
            var pipeline = CommonCVPipelines.GausImg_Reg;
            var img = CommonCVImages.StandardFiberCV;

            var result = pipeline.ExecuteSingle(img);

            var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CvSimplePipeline2Output");
            dir.CreateDirectory();
            pipeline.WriteOutputs(img, dir);
        }

        
        //[Test]
        public void CVGrayImgToComplexImg()
        {
            var pipeline = CommonCVPipelines.GrayImg_Complex;
            var img = CommonCVImages.StandardFiberCV;

            var result = pipeline.ExecuteSingle(img);

            var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CvSimplePipeline3Output");
            dir.CreateDirectory();
            pipeline.WriteOutputs(img, dir);
        }
        

        [Test]
        public void CVColImgToGrayImg()
        {
            var pipeline = CommonCVPipelines.ColImg_GrayImg;
            var img = CommonCVImages.StandardFiberCV;

            var result = pipeline.ExecuteSingle(img);

            var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CvSimplePipeline4Output");
            dir.CreateDirectory();
            pipeline.WriteOutputs(img, dir);
        }

        [Test]
        public void CVColImgToBinImg()
        {
            var pipeline = CommonCVPipelines.ColImg_Bin;
            var img = CommonCVImages.StandardFiberCV;

            var result = pipeline.ExecuteSingle(img);

            var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CvSimplePipeline5Output");
            dir.CreateDirectory();
            pipeline.WriteOutputs(img, dir);
        }

        [Test]
        public void CVBinImgToReg()
        {
            var pipeline = CommonCVPipelines.Bin_Reg;
            var img = CommonCVImages.StandardFiberCV;

            var result = pipeline.ExecuteSingle(img);

            var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CvSimplePipeline6Output");
            dir.CreateDirectory();
            pipeline.WriteOutputs(img, dir);
        }

        public void Traverse()
        {

            // cv pipelines
            foreach (var pipeline in CommonCVPipelines.CVPipelineCollection)
            {
                var breadthBack = pipeline.TraverseBreadthBackward().ToList();
                var breadthFor = pipeline.TraverseBreadthForward().ToList();
                var depthBack = pipeline.TraverseDepthBackward().ToList();
                var depthFor = pipeline.TraverseDepthForward().ToList();

                Assert.AreEqual(breadthBack.Count, breadthFor.Count);
                Assert.AreEqual(breadthFor.Count, depthBack.Count);
                Assert.AreEqual(depthBack.Count, depthFor.Count);


                var layers = pipeline.Layers;
                int maxNodesPerLayer = 2;
                if (!pipeline.Nodes.Exists(x => x.GetType() == typeof(CVUnion))) maxNodesPerLayer = 1;
                Assert.AreEqual(layers.Max(x => x.Value.Count), maxNodesPerLayer, "failed at cv pipelines");
                Assert.AreEqual(layers.Count, pipeline.Height + 1, "failed at cv pipelines");
            }
        }

        [Test]
        public void CVOperatorMap()
        {
            var pipe = CommonCVPipelines.GrayImgMorph_Reg.Nodes;
            var map = new CVOperatorMap(pipe/*,DependencyTree.GetBasicDependencies()*/);
            var config = new CGPConfiguration(2, 4, 1, pipe.Max(x => x.CGPInputCount), pipe.Max(x => x.CGPParameterCount), map, 1, 1);
            var random = new SystemRandom(0);
            var creator = new CGPFloatVectorCreator(random, config);
            var tmp1 = creator.Create().FloatVector;
            var pipe1 = new CVPipeline(tmp1, config);
        }


        [Test]
        public void XmlSerialization()
        {
            // cv pipelines
            var img2 = CommonCVImages.StandardFiberCV;
            //var img2_single = new UMat();
            //CvInvoke.ExtractChannel(img2, img2_single, 0);
            var path = Path.Combine(CommonInformation.TestResultsDirectory, "serialize_xml_pipe.txt");

            foreach (var pipeline in CommonCVPipelines.CVPipelineCollection)
            {
                pipeline.SerializeXml(path);

                var pipe = CVPipeline.DeserializeXml(path);
                Assert.AreEqual(pipeline.Nodes.Count, pipe.Nodes.Count, "node count not equal");
                Assert.AreEqual(pipeline.OutputNodes.Count, pipe.OutputNodes.Count, "outputnode count not equal");

                var ressq = pipeline.ExecuteSingle(img2.Clone());//Formerly img2_single
                var respipe = pipe.ExecuteSingle(img2.Clone());//formerly img2_single

                IOutputArray dst = new UMat();
                var imageSize = 0;
                var dim1 = ressq.GetInputArray().GetSize();
                var dim2 = respipe.GetInputArray().GetSize();

                Assert.AreEqual(dim1.Height, dim2.Height);
                Assert.AreEqual(dim1.Width, dim2.Width);

                imageSize = dim1.Height * dim1.Width;

                CvInvoke.Compare(ressq, respipe, dst, Emgu.CV.CvEnum.CmpType.Equal);
                Assert.AreEqual(dst.GetInputArray().GetUMat().Positives(), imageSize, "failed at pipeline: " + pipeline.Nodes.ToList());

                pipeline.ResetOutput();
                pipe.ResetOutput();
                ressq.GetInputArray().Dispose();
                respipe.GetInputArray().Dispose();
            }


            // artifical pipeline with all nodes
            var collection = CommonCVPipelines.NodeCollection;
            for (int i = 0; i < collection.Count - 1; i++)
            {
                collection[i].AddParent(collection[i + 1]);
            }

            var pipeFull = new CVPipeline(collection.Last());
            pipeFull.SerializeXml(path);
            var deser = CVPipeline.DeserializeXml(path);
        }
        [Test]
        public void RandomPipelineXmlSerialization()
        {
            int seed = 0;
            int numCreate = 10;
            bool color = false;
            foreach (var image2 in new List<UMat> { CommonCVImages.StandardFiberCV.ToGray(), CommonCVImages.StandardFiberCV })
            {
                var collection = CommonCVPipelines.NodeCollection;
                var map = new CVOperatorMap(collection/*, DependencyTree.ImagesOnly()*/);
                var random = new SystemRandom(seed);
                var config = new CGPConfiguration(10, 10, 1, collection.Max(x => x.CGPInputCount), collection.Max(x => x.CGPParameterCount),
                    map, 1, 1);
                var creator = new CGPFloatVectorCreator(random, config);

                var dir = Path.Combine(CommonInformation.TestResultsDirectory, "FullCVNodeCollectionCGP");
                dir.CreateDirectory();

                var serPath = Path.Combine(dir, "randompipelinexmlserialization.txt");

                for (int i = 0; i < numCreate; i++)
                {
                    var vec = creator.Create() as FloatVector;
                    var pipe = new CVPipeline(vec, config);
                    try
                    {
                        var result = pipe.ExecuteSingle(image2);
                        pipe.SerializeXml(serPath);
                        var deser = CVPipeline.DeserializeXml(serPath);
                        var result2 = deser.ExecuteSingle(image2);
                        var dst = new UMat();
                        CvInvoke.AbsDiff(result, result2, dst);  // check if images are identical
                        Assert.AreEqual(dst.Binary().Positives(), 0);

                    }
                    catch(AssertionException)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        pipe.WriteToDOTFile(Path.Combine(dir, i.ToString() + ".txt"));
                        Assert.Warn("Exception at pipeline: {0}: {1}, color: {2}", i, ex.Message, color);
                    }
                }
                color = true;
            }

        }


        [Test]
        public void CGPConversion()
        {
            foreach (var pipeline in CommonCVPipelines.CVPipelineCollection)
            {
                Assert.IsTrue(TestConversion(pipeline));
            }
        }

        private bool TestConversion(CVPipeline pipeline)
        {
            var image = CommonCVImages.StandardFiberCV.ToGray();
            var random = new SystemRandom(0);
            var operatorEncoder1 = new CVOperatorMap(pipeline.Nodes, pipeline.ToDependencyTree());  // this is necessary due to successive calls to Initialize method cause errors
            var operatorEncoder2 = new CVOperatorMap(pipeline.Nodes, pipeline.ToDependencyTree());  // and ToCGPEncoding calls Initialize. This can be avoided by finally merging configuration and operatormap

            FloatVector vector; CGPConfiguration configuration;

            pipeline.ToCGPEncoding(operatorEncoder1, random, out vector, out configuration);

            var result = pipeline.ExecuteSingle(image);

            var dir = CommonInformation.TestResultsDirectory;

            Logger.PrintGrid(vector, configuration, Path.Combine(dir, "CGPConversion_gridpipeline.txt"), false, "first encoding", true, true);

            var decoded = new CVPipeline(vector, configuration);
            image = CommonCVImages.StandardFiberCV.ToGray();
            var resultDecoded = decoded.ExecuteSingle(image);

            decoded.ToCGPEncoding(operatorEncoder2, random, out vector, out configuration);
            Logger.PrintGrid(vector, configuration, Path.Combine(dir, "CGPConversion_gridpipeline.txt"), true, "second encoding", true, true);

            Assert.IsTrue(resultDecoded.Difference(result).Positives() == 0, "results are not identical at pipeline: {0}", pipeline.ToString());

            return true;
        }

        [Test]
        public void CheckIfDotFilesUseOnlySensibleAttributes()
        {

            foreach (var pipeline in CommonCVPipelines.CVPipelineCollection)
            {
                pipeline.WriteToDOTFile(Path.Combine(CommonInformation.TestResultsDirectory, pipeline.Name + "_dot.txt"));
            }
        }

        [Test]
        public void RandomPipelineExecution()
        {
            int seed = 0;
            int numCreate = 10;
            var image2 = CommonCVImages.StandardFiberCV;//.ToGray();

            foreach (var pipeline in CommonCVPipelines.CVPipelineCollection)
            {
                FloatVector vector; CGPConfiguration config;
                //DependencyTree tree = new DependencyTree();
                CVOperatorMap map;
                if (pipeline.Name.Equals("CVColImg_GrayImg"))
                    map = new CVOperatorMap(pipeline.Nodes, pipeline.ToDependencyTree());
                else
                {
                    map = new CVOperatorMap(pipeline.Nodes, pipeline.ToDependencyTree());
                }
                var random = new SystemRandom(seed);
                pipeline.ToCGPEncoding(map, random, out vector, out config);
                var creator = new CGPFloatVectorCreator(random, config);

                for (int i = 0; i < numCreate; i++)
                {
                    var vec = creator.Create() as FloatVector;
                    var pipe = new CVPipeline(vec, config);
                    var result = pipe.Execute(image2);
                }
            }
        }

        [Test]
        public void FullCVNodeCollectionCGP()
        {
            int seed = 0;
            int numCreate = 10;
            bool color = false;
            foreach(var image2 in new List<UMat> { CommonCVImages.StandardFiberCV.ToGray(), CommonCVImages.StandardFiberCV })
            {
                var collection = CommonCVPipelines.NodeCollection;
                var map = new CVOperatorMap(collection/*, DependencyTree.ImagesOnly()*/);
                var random = new SystemRandom(seed);
                var config = new CGPConfiguration(10, 10, 1, collection.Max(x => x.CGPInputCount), collection.Max(x => x.CGPParameterCount),
                    map, 1, 1);
                var creator = new CGPFloatVectorCreator(random, config);

                var dir = Path.Combine(CommonInformation.TestResultsDirectory, "FullCVNodeCollectionCGP");
                dir.CreateDirectory();

                for (int i = 0; i < numCreate; i++)
                {
                    var vec = creator.Create() as FloatVector;
                    var pipe = new CVPipeline(vec, config);
                    try
                    {
                        var result = pipe.Execute(image2);
                    }
                    catch (Exception ex)
                    {
                        pipe.WriteToDOTFile(Path.Combine(dir, i.ToString() + ".txt"));
                        Assert.Warn("Exception at pipeline: {0}: {1}, color: {2}", i, ex.Message, color);
                    }
                }

                color = true;
            }
            //(for nodes requiring color), example CVGrabCut
            //DependencyTree tree = new DependencyTree();

        }
    }
}
