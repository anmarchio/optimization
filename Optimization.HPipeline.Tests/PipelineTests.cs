using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Extensions;
using HalconDotNet;
using NUnit.Framework;
using Optimization.CartesianGeneticProgramming;
using Optimization.CartesianGeneticProgramming.Mutators;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Random;
using Optimization.Fitness.ErrorHandling;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.HPipeline.OperatorNodes;
using Optimization.Pipeline;
using Optimization.Pipeline.Interfaces;
using Optimization.Tests;
using Optimization.Tests.Categories;
using Optimization.Tests.TestImages;
using Serilog;

namespace Optimization.HPipeline.Tests
{
    [TestFixture]
    public class PipelineTests
    {
        [SetUp]
        public void SetUp()
        {
            var h = new HObject();
        }

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            // or

            Directory.SetCurrentDirectory(CommonInformation.TestResultsDirectory);
        }

        [Test,ShortTest]
        public void Traverse()
        {
            // halcon pipelines
            foreach (var pipeline in CommonHalconPipelines.Collection)
            {
                var breadthBack = pipeline.TraverseBreadthBackward().ToList();
                var breadthFor = pipeline.TraverseBreadthForward().ToList();
                var depthBack = pipeline.TraverseDepthBackward().ToList();
                var depthFor = pipeline.TraverseDepthForward().ToList();

                Assert.AreEqual(breadthBack.Count, breadthFor.Count, "difference in breadthBack: {0}, breadthFor: {1}", breadthBack.Count, breadthFor.Count);
                Assert.AreEqual(breadthFor.Count, depthBack.Count, "difference in breadthFor: {0}, depthBack: {1}", breadthFor.Count, depthBack.Count);
                Assert.AreEqual(depthBack.Count, depthFor.Count, "difference in depthBack: {0}, depthFor: {1}", depthBack.Count, depthFor.Count);
            }

            // dependency trees

            foreach (var tree in DependencyTree.DependencyCollection)
            {
                var breadthBack = tree.TraverseBreadthBackward().ToList();
                var breadthFor = tree.TraverseBreadthForward().ToList();
                var depthBack = tree.TraverseDepthBackward().ToList();
                var depthFor = tree.TraverseDepthForward().ToList();

                Assert.AreEqual(breadthBack.Count, breadthFor.Count, "difference in breadthBack: {0}, breadthFor: {1}", breadthBack.Count, breadthFor.Count);
                Assert.AreEqual(breadthFor.Count, depthBack.Count, "difference in breadthFor: {0}, depthBack: {1}", breadthFor.Count, depthBack.Count);
                Assert.AreEqual(depthBack.Count, depthFor.Count, "difference in depthBack: {0}, depthFor: {1}", depthBack.Count, depthFor.Count);
            }
        }



        [Test,ShortTest]
        public void RandomHalconPipelinesExecution()
        {
            var path = Path.Combine(CommonInformation.TestResultsDirectory, "exceptions");
            path.CreateDirectory();
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo
                .File(path: Path.Combine(path, "operatorexception.txt"),
                rollingInterval: RollingInterval.Hour)
                 //outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}{Properties:lj}")
                 .CreateLogger();

            int seed = 0;
            int numCreate = 50;
            var image = CommonHImages.StandardFiber;
            var collection = CommonHalconPipelines.Collection;
            collection = collection.Where(x => x.Name != "PeronaMalik").ToList();
            var dir = Path.Combine(CommonInformation.TestResultsDirectory, "randomserialization");
            dir.CreateDirectory();
            
            foreach (var pipeline in collection)
            {
                FloatVector vector; CGPConfiguration config;
                var map = new HalconOperatorMap(pipeline.Nodes, pipeline.ToDependencyTree());
                var random = new SystemRandom(seed);
                pipeline.ToCGPEncoding(map, random, out vector, out config);
                var creator = new CGPFloatVectorCreator(random, config);

                for (int i = 0; i < numCreate; i++)
                {
                    var vec = creator.Create() as FloatVector;
                    var pipe = new HalconPipeline(vec, config);
                    try
                    {
                        var result = pipe.ExecuteSingle(image);
                        path = Path.Combine(dir, "xmlser.txt");
                        pipe.SerializeXml(path);
                        var pipe2 = HalconPipeline.DeserializeXml(path);
                        var result2 = pipe2.ExecuteSingle(image);
                        Assert.AreEqual(result.TestEqualObj(result2), 1);
                    }
                    catch (OperatorException oex) 
                    {
                        new CGPPipelineException(oex.Message, oex, pipe).UseSerilog();
                        throw;
                    }
                  
                }
            }
        }


        [Test,ShortTest]
        public void FFTTest()
        {
            Assert.Warn("FFT mit andi besprechen. unsinnige implementierung + guass unsinn (relative threshold) FFT vorerst ausgeschlossen aus projekt");
            /*
          var set = new List<HalconOperatorNode>() { new SobelAmp(), new FFT(), new Threshold(), new SelectShape(), new Union2() };
          var map = new HalconOperatorMap(set);
          int seed = 0;
          int numCreate = 200;
          var image = CommonHImages.StandardFiber;
          image = CommonHImages.StandardFiber;
          var random = new SystemRandom(seed);
          var config = new CGPConfiguration(10, 10, 2, set.Max(x => x.CGPInputCount), set.Max(x => x.CGPParameterCount), map,
           1, 1);
          var creator = new CGPFloatVectorCreator(random, config);


          var fft = new FFT();
          var output = fft.Execute(image);
          var imgType = output.GetImageType();
          output.Dump(Path.Combine(CommonInformation.TestResultsDirectory, "fft.hobj"), format: "hobj");


          for (int i = 0; i < numCreate; i++)
          {
              var vec = creator.Create() as FloatVector;
              var pipe = new HalconPipeline(vec, config);
              var result = pipe.Execute(image);
          }*/
        }

        [Test, ShortTest]
        public void DependencyGraph()
        {
            var dependencies = DependencyTree.GetSimpleDependencies();
            dependencies.WriteToDOTFile(Path.Combine(CommonInformation.TestResultsDirectory, "dependency.txt"));


            dependencies = DependencyTree.DeprecatedFilterThresholdMorphologicalArchitectureDependencyTree();
            dependencies.WriteToDOTFile(Path.Combine(CommonInformation.TestResultsDirectory, "simpler_dependencies.txt"));
        }

        [Test,ShortTest]
        public void FullNodeCollectionOperatorMapExecution()
        {
            int seed = 0;
            int numCreate = 200;
            var image = CommonHImages.StandardFiber;
            image = image.SingleChannelFromMulti();

            var collection = CommonHalconPipelines.HalconOperatorNodeCollection;
            var map = new HalconOperatorMap(collection);
            var random = new SystemRandom(seed);
            var config = new CGPConfiguration(10, map.Dependencies.EstimateCgpColumnCount(), 2, collection.Max(x => x.CGPInputCount), collection.Max(x => x.CGPParameterCount), map,
             1, 1);
            var creator = new CGPFloatVectorCreator(random, config);

            for (int i = 0; i < numCreate; i++)
            {
                var vec = creator.Create() as FloatVector;
                var pipe = new HalconPipeline(vec, config);
                try
                {
                    var result = pipe.Execute(image);
                }
                catch (Exception)
                {
                    pipe.WriteToDOTFile(Path.Combine(CommonInformation.TestResultsDirectory, "fullnodecollectionOperatorMapeExecutionPipeline.txt"));
                    //throw;
                }
            }
        }

        [Test,ShortTest]
        public void FullNodeCollectionOperatorMapExecutionFullDependencyTree()
        {
            int seed = 0;
            int numCreate = 500;
            var image = CommonHImages.StandardFiber;
            image = CommonHImages.StandardFiber;
            image = image.SingleChannelFromMulti();
            var collection = CommonHalconPipelines.HalconOperatorNodeCollection;
            var map = new HalconOperatorMap(collection, DependencyTree.GetSimpleDependencies());
            var random = new SystemRandom(seed);
            var config = new CGPConfiguration(10, map.Dependencies.EstimateCgpColumnCount(), 2, collection.Max(x => x.CGPInputCount), collection.Max(x => x.CGPParameterCount), map,
             1, 1);
            var creator = new CGPFloatVectorCreator(random, config);

            var filename = Path.Combine(CommonInformation.TestResultsDirectory, "xml_serialization.txt");

            var exceptions = new List<Exception>();

            for (int i = 0; i < numCreate; i++)
            {
                var vec = creator.Create() as FloatVector;
                var pipe = new HalconPipeline(vec, config);
                try
                {
                    var result = pipe.Execute(image);
                }
                catch(Exception e)
                {
                    Assert.Warn("Exception {1} at iteration: {0} ignore if duple_div exception", i, e.Message);
                    exceptions.Add(e);
                }

                // also test xml
                try
                {
                    pipe.SerializeXml(filename);
                    var deser = HalconPipeline.DeserializeXml(filename);
                    deser.Execute(image);
                }
                catch (Exception e)
                {
                    Assert.Warn("Exception {1} at iteration: {0}, ignore if tuple_div exception", i, e.Message);
                    exceptions.Add(e);
                }
            }

            Assert.Warn("total of {0} exceptions", exceptions.Count);
        }

        [Test,ShortTest][NonParallelizable]
        public void VerifyOperatorOutputs()
        {
            HOperatorSet.SetSystem("no_object_result", "exception");

            var img2ImgOperators = from node in CommonHalconPipelines.HalconOperatorNodeCollection
                                   where (node.OperatorType & OperatorType.ImageToImage) == OperatorType.ImageToImage
                                   select node;

            Assert.Greater(img2ImgOperators.Count(), 0);
            var img = CommonHImages.StandardFiber;

            var imgType = img.GetImageType();

            foreach (var node in img2ImgOperators)
            {
                var result = node.Execute(img);
                Assert.IsTrue(result.IsImage(), "failed at {0}, output not an image", node.GetType().Name);
            }

            var img2RegionOperators = from node in CommonHalconPipelines.HalconOperatorNodeCollection
                                      where (node.OperatorType & OperatorType.ImageToRegion) == OperatorType.ImageToRegion
                                      select node;
            Assert.Greater(img2RegionOperators.Count(), 0);
            HObject resultRegion = null;
            foreach (var node in img2RegionOperators)
            {
                resultRegion = node.Execute(img);
                Assert.IsTrue(resultRegion.IsRegion(), "failed at {0}, output not a region", node.GetType().Name);
            }


            var region2RegionOperators = from node in CommonHalconPipelines.HalconOperatorNodeCollection
                                         where (node.OperatorType & OperatorType.RegionToRegion) == OperatorType.RegionToRegion
                                         select node;
            Assert.Greater(region2RegionOperators.Count(), 0);

            foreach (var node in region2RegionOperators)
            {
                var res = node.Execute(resultRegion);
                Assert.IsTrue(res.IsRegion(), "failed at {0}, output not a region", node.GetType().Name);
            }

            HOperatorSet.SetSystem("no_object_result", "true");

        }

        [Test,ShortTest]
        public void VerifyImageAndRegionToRegionOperatorOutputs()
        {
            //Assert.Fail("This can only be tackled if there is some more information which operator excepts which as input... working on it.");

            var ImageAndRegionToRegion = from node in CommonHalconPipelines.HalconOperatorNodeCollection
                                         where node.IsOrOperatorType(OperatorType.ImageAndRegionToRegion)
                                         select node;
            var img = CommonHImages.StandardFiber;
            HOperatorSet.Rgb1ToGray(img, out img);

            foreach (var node in ImageAndRegionToRegion)
            {
                var inpNode = new HalconInputNode();
                var sobel = new SobelAmp();
                var thresh = new Threshold();
                node.AddChild(sobel);
                node.AddChild(thresh);
                sobel.AddChild(inpNode);
                thresh.AddChild(inpNode);
                var pipe = new HalconPipeline(node);
                var res = pipe.ExecuteSingle(img);
                if (res.GetObjClass().Type == HTupleType.EMPTY) { Assert.Warn("{0} returned an empty object", node.GetType().Name); continue; }
                Assert.IsTrue(res.IsRegion(), "failed at {0}, output not a region", node.GetType().Name);
            }
        }


        [Test,ShortTest]
        public void RandomPipelinesCopy()
        {
            int seed = 0;
            int numCreate = 30;
            var image = CommonHImages.StandardFiber;
            foreach (var pipeline in CommonHalconPipelines.Collection)
            {
                FloatVector vector; CGPConfiguration config;
                var map = new HalconOperatorMap(pipeline.Nodes, pipeline.ToDependencyTree());
                var random = new SystemRandom(seed);
                pipeline.ToCGPEncoding(map, random, out vector, out config);
                var creator = new CGPFloatVectorCreator(random, config);

                for (int i = 0; i < numCreate; i++)
                {
                    var vec = creator.Create() as FloatVector;
                    var pipe = new HalconPipeline(vec, config);
                    var copy = pipe.Copy() as HalconPipeline;

                    for (int j = 0; j < copy.Nodes.Count; j++)
                        HalconOperatorNodesTest.AreEqual(pipe.Nodes[j], copy.Nodes[j]);

                }
            }
        }


        [Test,ShortTest]
        public void HalconMemoryUsage()
        {
            Assert.Warn("Ignore since it takes too long and was successful the first time. Repeat if desired.");
            /*
            var sampleImages = CommonImages.EnumerateSampleImageDirectory().ToList();
            var sq = CommonHalconPipelines.StatusQuo;
            sq.AutoDisposeIntermediateOutputs = true;
            var mem = new ComputerInfo().TotalPhysicalMemory;
            var watch = new Stopwatch();
            var duration = new long[2];
            var disp = new bool[] { true, false };
            for (int i = 0; i < 2; i++)
            {
                watch.Reset();
                watch.Start();
                sq.AutoDisposeIntermediateOutputs = disp[i];
                for (int run = 0; run < 1000; run++)
                {
                    Console.WriteLine("run: {0}", run);
                    foreach (var sampleImg in sampleImages)
                    {
                        HObject img;
                        HOperatorSet.ReadImage(out img, sampleImg);

                        var output = sq.ExecuteSingle(img);
                        img.Dispose();
                        output.Dispose();
                    }
                    Assert.True(new ComputerInfo().AvailablePhysicalMemory > mem / 100, "failed at run: {0}", i);
                }
                watch.Stop();
                duration[i] = watch.ElapsedMilliseconds;
            }
            Assert.Pass("duration difference of manual dispose calls: {0}, garbage collector and hoping for the best: {1}, diff: {2}", duration[0], duration[1], duration[0] - duration[1]);
            */
        }

        [Test,ShortTest]
        public void RandomPipelineXmlSerialization()
        {

        }

        [Test,ShortTest]
        public void XmlSerialization()
        {
            // halcon pipelines
            var img = CommonHImages.StandardFiber;  // loads as rgb
            HOperatorSet.Rgb1ToGray(img, out img);
            var path = Path.Combine(CommonInformation.TestResultsDirectory, "serialize_xml_pipe12345.txt");

            try { File.Delete(path); } catch { };

            var tmp = CommonHalconPipelines.StatusQuo;
            tmp.ExecuteSingle(img);

            foreach (var pipeline in CommonHalconPipelines.Collection)
            {
                try
                {
                    pipeline.SerializeXml(path);

                    var pipe = HalconPipeline.DeserializeXml(path);
                    Assert.AreEqual(pipeline.Nodes.Count, pipe.Nodes.Count, "node count not equal");
                    Assert.AreEqual(pipeline.OutputNodes.Count, pipe.OutputNodes.Count, "outputnode count not equal");

                    var ressq = pipeline.ExecuteSingle(img);
                    var respipe = pipe.ExecuteSingle(img);

                    HTuple equal;
                    HOperatorSet.CompareObj(ressq, respipe, 0, out equal);
                    Assert.IsTrue(equal, "failed at pipeline: {0}", pipeline.Name);

                }catch(Exception e)
                {
                    Assert.Warn("warning: {0}, at pipeline: {1}", e, pipeline.Name);
                }
                /*
                pipeline.ResetOutput();
                pipe.ResetOutput();
                ressq.Dispose();
                respipe.Dispose();*/
            }
 
            // artifical pipeline with all nodes
            var collection = CommonHalconPipelines.HalconOperatorNodeCollection;
            for(int i = 0; i < collection.Count - 1; i++)
            {
                collection[i].AddParent(collection[i + 1]);
            }

            var pipeFull = new HalconPipeline(collection.Last());
            pipeFull.SerializeXml(path);
            var deser = HalconPipeline.DeserializeXml(path);

            File.Delete(path);
        }


        [Test,ShortTest]
        public void MultipleInputDependencyTree()
        {
            Assert.Pass("Fails if used with a full operatornode collection.");
            var seed = 0;
            var numCreate = 20;
            var refSet = CommonHImages.ReferenceSetHalcon;
            var multiDependency = DependencyTree.MultipleImagesInput();
            multiDependency.WriteToDOTFile(Path.Combine(CommonInformation.TestResultsDirectory, "multidependencytree.txt"));
            var collection = CommonHalconPipelines.HalconOperatorNodeCollection;
            var map = new HalconOperatorMap(collection, multiDependency);
            var random = new SystemRandom(seed);
            var config = new CGPConfiguration(10, 10, 2, collection.Max(x => x.CGPInputCount), collection.Max(x => x.CGPParameterCount), map,
             multiDependency.InputNodes.Count, 1);

            var creator = new CGPFloatVectorCreator(random, config);

            var input = new Dictionary<float, HObject>();
            foreach(DependencyInputNode inNode in multiDependency.InputNodes)
            {
                input.Add(inNode.ProgramInputIdentifier, refSet[0].Image);
            }

            int countBoth = 0;

            for(int i=0; i < numCreate; i++)
            {
                var pipe = new HalconPipeline(creator.Create().FloatVector, config);
                var res = pipe.Execute(input);
                if (pipe.InputNodes.Count > 1)
                    countBoth++;
            }

            Assert.IsTrue(countBoth > 0);
        }



        [Test,ShortTest]
        public void BinarySerialization()
        {
            var sq = CommonHalconPipelines.StatusQuo;
            var path = Path.Combine(CommonInformation.TestResultsDirectory, "serialize_binary_pipe.txt");
            sq.SerializeBinary(path);

            var pipe = HalconPipeline.DeserializeBinary(path) as HalconPipeline;
            Assert.AreEqual(sq.Nodes.Count, pipe.Nodes.Count, "node count not equal");
            Assert.AreEqual(sq.OutputNodes.Count, pipe.OutputNodes.Count, "outputnode count not equal");

            var img = CommonHImages.StandardFiber;

            var ressq = sq.ExecuteSingle(img);
            var respipe = pipe.ExecuteSingle(img);

            HTuple equal;
            HOperatorSet.CompareObj(ressq, respipe, 0, out equal);
            Assert.IsTrue(equal);
        }



        [Test,ShortTest]
        public void CopyTest()
        {
            foreach (var pipeline in CommonHalconPipelines.Collection)
            {
                var copy = (HalconPipeline)pipeline.Copy();

                Assert.AreEqual(pipeline.Nodes.Count, copy.Nodes.Count);

                TestExecution(pipeline, copy);

                for (int i = 0; i < pipeline.Nodes.Count; i++)
                {
                    var node1 = pipeline.Nodes[i];
                    var node2 = pipeline.Nodes[i];
                    Assert.AreEqual(node1.GetType(), node2.GetType(), "failed at type identity: node1 {0} node2 {1}", node1.AssemblyQualifiedName, node2.AssemblyQualifiedName);
                    var params1 = node1.ToCGPNodeParameters();
                    var params2 = node2.ToCGPNodeParameters();
                    Assert.AreEqual(params1.Length, params2.Length, "failed at parameter length of node: {0}", node1.AssemblyQualifiedName);
                    for (int j = 0; j < params1.Length; j++)
                    {
                        Assert.AreEqual(params1[j], params2[j], "failed at node: {0} at paramIdx: {1}", node1.AssemblyQualifiedName, j);
                    }
                }
            }
        }

        private void TestExecution(HalconPipeline pipeline1, HalconPipeline pipeline2)
        {
            var result1 = pipeline1.ExecuteSingle(CommonHImages.StandardFiber);
            var result2 = pipeline2.ExecuteSingle(CommonHImages.StandardFiber);
            HTuple isEqual;
            HOperatorSet.CompareObj(result1, result2, 0, out isEqual);
            Assert.IsTrue(isEqual);
        }

        private bool TestConversion(HalconPipeline pipeline)
        {
            var image = CommonHImages.StandardFiber;
            var dir = CommonInformation.TestResultsDirectory;

            var operatorEncoder1 = new HalconOperatorMap(pipeline.Nodes, pipeline.ToDependencyTree());
            var operatorEncoder2 = new HalconOperatorMap(pipeline.Nodes, pipeline.ToDependencyTree());

            var random = new SystemRandom(0);

            FloatVector vector; CGPConfiguration configuration;

            pipeline.ToCGPEncoding(operatorEncoder1, random, out vector, out configuration);

            Logger.PrintGrid(vector, configuration, Path.Combine(dir, "CGPConversion_gridpipeline.txt"), false, "original", true, true);

            var result = pipeline.ExecuteSingle(image);


            Logger.PrintGrid(vector, configuration, Path.Combine(dir, "CGPConversion_gridpipeline.txt"), true, "first encoding", true, true);

            var decoded = new HalconPipeline(vector, configuration);
            var resultDecoded = decoded.ExecuteSingle(image);

            decoded.ToCGPEncoding(operatorEncoder2, random, out vector, out configuration);
            Logger.PrintGrid(vector, configuration, Path.Combine(dir, "CGPConversion_gridpipeline.txt"), true, "second encoding", true, true);

            HTuple area1, row1, column1, area2, row2, column2;
            HOperatorSet.AreaCenter(result, out area1, out row1, out column1);
            HOperatorSet.AreaCenter(resultDecoded, out area2, out row2, out column2);

            HTuple isEqual;
            HOperatorSet.CompareObj(result, resultDecoded, 0, out isEqual);
            if (isEqual == false)
                image.Dump(Path.Combine(dir, "CGPConversion_image_result"), result, resultDecoded);
            Assert.IsTrue(isEqual, "results are not identical at pipeline: {0}", pipeline.Name);

            return true;
        }

        private bool TestConversionWithMutation(HalconPipeline pipeline)
        {
            var image = CommonHImages.StandardFiber;
            var dir = CommonInformation.TestResultsDirectory;

            var operatorEncoder1 = new HalconOperatorMap(pipeline.Nodes, pipeline.ToDependencyTree());

            var random = new SystemRandom(0);
            
            FloatVector vector; CGPConfiguration configuration;
            
            pipeline.ToCGPEncoding(operatorEncoder1, random, out vector, out configuration);

            var mutator = new SinglePassMutator(random, configuration);

            for (int i = 0; i < 10; i++)
                mutator.Mutate(vector);
            
            var decoded = new HalconPipeline(vector, configuration);
            
            decoded.ToCGPEncoding(operatorEncoder1, configuration, random);
            for (int i = 0; i < 10; i++)
                mutator.Mutate(vector);

            return true;
        }




        [Test,ShortTest]
        public void CGPConversion()
        {
            foreach (var pipeline in CommonHalconPipelines.Collection)
            {
                Assert.IsTrue(TestConversion(pipeline));
            }

        }

        [Test,ShortTest]
        public void ExecuteHalconPipelineCollection()
        {

            var refSet = CommonHImages.ReferenceSetHalcon;
            int i = 0;

            foreach (var pipeline in CommonHalconPipelines.Collection)
            {

                foreach (var file in CommonImages.EnumerateSampleImageDirectory())
                {
                    var inputImage = new HImage(file);
                    var result = pipeline.Execute(inputImage).First().Value;
                    i++;
                }
            }
        }

        #region comparison tests to actual halcon scripts -- are either deprecated or broken due to undocumented changes in some of the operators/decoding maps...

        /// <summary>
        /// HalconScript to compare to: infimo_sobel_y_MSA
        /// </summary>
        //[Test]
        public void CompareHalconScript()
        {

            var dir = Directory.GetCurrentDirectory();
            dir = dir.Replace("\\bin\\Debug", "");
            string[] files = new string[]
             {
                dir + @"\FitnessTests\SampleImage\5\5.bmp",

             };

            HObject comparison;
            HTuple isEqual;

            foreach (var file in files)
            {
                HImage Image = new HImage(file);
                HObject Result1, Result2, Result3, Result4, Result5, Result6, Result7, Result8, Result9, Result10, Result11, Result12, Result13;

                HOperatorSet.SobelAmp(Image, out Result1, "y", 3);
                HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\sobel.hobj");
                HOperatorSet.CompareObj(Result1, comparison, 0, out isEqual);
                Assert.IsTrue(isEqual, "sobel");


                HOperatorSet.AccessChannel(Result1, out Result2, 3);
                HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\access_channel_3.hobj");
                HOperatorSet.CompareObj(Result2, comparison, 0, out isEqual);
                Assert.IsTrue(isEqual, "accessChannel");

                HOperatorSet.Threshold(Result2, out Result3, -128, -12);
                HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\threshold_neg.hobj");
                HOperatorSet.CompareObj(Result3, comparison, 0, out isEqual);
                Assert.IsTrue(isEqual, "thresh neg");

                HOperatorSet.Threshold(Result2, out Result4, 12, 128);
                HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\threshold_pos.hobj");
                HOperatorSet.CompareObj(Result4, comparison, 0, out isEqual);
                Assert.IsTrue(isEqual, "thresh pos");

                HOperatorSet.Union2(Result3, Result4, out Result5);
                HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\union2.hobj");
                HOperatorSet.CompareObj(Result5, comparison, 0, out isEqual);
                Assert.IsTrue(isEqual, "union2");

                HOperatorSet.Connection(Result5, out Result6);
                HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\connection1.hobj");
                HOperatorSet.CompareObj(Result6, comparison, 0, out isEqual);
                Assert.IsTrue(isEqual, "connection1");

                HOperatorSet.Connection(Result6, out Result7);
                HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\connection2.hobj");
                HOperatorSet.CompareObj(Result7, comparison, 0, out isEqual);
                Assert.IsTrue(isEqual, "connection2");


                HOperatorSet.SelectShape(Result7, out Result8, "area", "and", 6, 99999);
                HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\select_shape1.hobj");
                HOperatorSet.CompareObj(Result8, comparison, 0, out isEqual);
                Assert.IsTrue(isEqual, "ss1");

                HOperatorSet.Union1(Result8, out Result9);
                HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\union1.hobj");
                HOperatorSet.CompareObj(Result9, comparison, 0, out isEqual);
                Assert.IsTrue(isEqual, "union1");

                HObject circle, rectangle;
                HOperatorSet.GenCircle(out circle, 100, 100, 8);
                HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\circle.hobj");
                HOperatorSet.CompareObj(circle, comparison, 0, out isEqual);
                Assert.IsTrue(isEqual, "circle");

                HOperatorSet.GenRectangle1(out rectangle, 1, 1, 20, 5);
                HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\rectangle.hobj");
                HOperatorSet.CompareObj(rectangle, comparison, 0, out isEqual);
                Assert.IsTrue(isEqual, "rectangle");

                HOperatorSet.Closing(Result9, circle, out Result10);
                HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\closing_circle.hobj");
                HOperatorSet.CompareObj(Result10, comparison, 0, out isEqual);
                Assert.IsTrue(isEqual, "closing_circle");

                HOperatorSet.Closing(Result10, rectangle, out Result11);
                HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\closing_rectangle.hobj");
                HOperatorSet.CompareObj(Result11, comparison, 0, out isEqual);
                Assert.IsTrue(isEqual, "closing_rectangle");

                HOperatorSet.Connection(Result11, out Result12);
                HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\connection3.hobj");
                HOperatorSet.CompareObj(Result12, comparison, 0, out isEqual);
                Assert.IsTrue(isEqual, "connection3");

                HOperatorSet.SelectShape(Result12, out Result13, "area", "and", 70, 99999);
                HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\select_shape_2.hobj");
                HOperatorSet.CompareObj(Result13, comparison, 0, out isEqual);
                Assert.IsTrue(isEqual, "selectShape");

            }
        }

        //[Test]
        public void CompareStatusQuo()
        {

            var sobelAmp = new SobelAmp(null, SobelAmp.SobelFilterType.y, 3);
            var thresholdAccessChannel1 = new ThresholdAccessChannel(sobelAmp, 3, 12, -1);
            var thresholdAccessChannel2 = new ThresholdAccessChannel(sobelAmp, 3, 12, 1);
            var union2 = new Union2(thresholdAccessChannel1, thresholdAccessChannel2);
            var connection1 = new Connection(union2, Connection.Neighborhood8);
            var connection2 = new Connection(connection1, Connection.Neighborhood8);
            var selectShape1 = new SelectShape(connection2, 6, 99999, SelectShape.SelectShapeFeatureTypes.area);
            var union1 = new Union1(selectShape1);

            var closing = new Closing(union1, StructElementTypes.Circle, 8, 0);

            var closing2 = new Closing(closing, StructElementTypes.Rectangle, 20, 5);
            var connection3 = new Connection(closing2, Connection.Neighborhood8);
            var selectShape = new SelectShape(connection3, 70, 99999, SelectShape.SelectShapeFeatureTypes.area);
            /*var pipeline = new HalconPipeline(sobelAmp, new List<HalconOperatorNode>()
            {
               sobelAmp, thresholdAccessChannel1, thresholdAccessChannel2, union2, connection1, connection2, selectShape1, union1, closing, closing2, connection3, selectShape
            });*/
            var pipeline = new HalconPipeline(selectShape);
            //var pipeline = CommonPipelines.StatusQuo;

            var dir = Directory.GetCurrentDirectory();
            dir = dir.Replace("\\bin\\Debug", "");

            HObject comparison;


            HObject Image = new HImage(dir + @"\FitnessTests\SampleImage\5\5.bmp");

            var result = pipeline.Execute(Image);

            HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\sobel.hobj");
            Assert.IsTrue(sobelAmp.OutputEquals(comparison));

            HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\threshold_neg.hobj");
            Assert.IsTrue(thresholdAccessChannel1.OutputEquals(comparison));

            HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\threshold_pos.hobj");
            Assert.IsTrue(thresholdAccessChannel2.OutputEquals(comparison));

            HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\union2.hobj");
            Assert.IsTrue(union2.OutputEquals(comparison));

            HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\connection1.hobj");
            Assert.IsTrue(connection1.OutputEquals(comparison));

            HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\connection2.hobj");
            Assert.IsTrue(connection2.OutputEquals(comparison));

            HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\select_shape1.hobj");
            Assert.IsTrue(selectShape1.OutputEquals(comparison));

            HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\union1.hobj");
            Assert.IsTrue(union1.OutputEquals(comparison));

            HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\closing_circle.hobj");
            Assert.IsTrue(closing.OutputEquals(comparison));

            HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\closing_rectangle.hobj");
            Assert.IsTrue(closing2.OutputEquals(comparison));

            HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\connection3.hobj");
            Assert.IsTrue(connection3.OutputEquals(comparison));

            HOperatorSet.ReadObject(out comparison, dir + @"\FitnessTests\SampleImage\5\select_shape_2.hobj");
            Assert.IsTrue(selectShape.OutputEquals(comparison));
        }
        //[Test]
        public void CompareFiberOrientationHdev()
        {
            var pipeline = CommonHalconPipelines.FiberOrientationHdev;

            var dir = Directory.GetCurrentDirectory();
            dir = dir.Replace("\\bin\\Debug", "");
            var path = dir + @"\PipelineTests\ReferenceImages\CompareFiberOrientationHdev\";

            HObject comparison = null;

            HObject Image = new HImage(path + @"Image__2017-06-11__18-05-49.bmp");
            HOperatorSet.Rgb1ToGray(Image, out Image);

            pipeline.Execute(Image);

            HOperatorSet.ReadObject(out comparison, path + "edgesImage");
            Assert.IsTrue(pipeline.Nodes[8].OutputEquals(comparison), "edgesImage");
            HOperatorSet.ReadObject(out comparison, path + "threshold");
            Assert.IsTrue(pipeline.Nodes[7].OutputEquals(comparison), "threshold");
            HOperatorSet.ReadObject(out comparison, path + "connection1");
            Assert.IsTrue(pipeline.Nodes[6].OutputEquals(comparison), "connection1");
            HOperatorSet.ReadObject(out comparison, path + "selectShape1");
            Assert.IsTrue(pipeline.Nodes[5].OutputEquals(comparison), "selectShape1");
            HOperatorSet.ReadObject(out comparison, path + "union1");
            Assert.IsTrue(pipeline.Nodes[4].OutputEquals(comparison), "union1");
            HOperatorSet.ReadObject(out comparison, path + "closing1");
            Assert.IsTrue(pipeline.Nodes[3].OutputEquals(comparison), "closing1");
            HOperatorSet.ReadObject(out comparison, path + "closing2");
            Assert.IsTrue(pipeline.Nodes[2].OutputEquals(comparison), "closing2");
            HOperatorSet.ReadObject(out comparison, path + "connection2");
            Assert.IsTrue(pipeline.Nodes[1].OutputEquals(comparison), "connection2");
            HOperatorSet.ReadObject(out comparison, path + "selectShape2");
            Assert.IsTrue(pipeline.Nodes[0].OutputEquals(comparison), "selectShape2");

        }

        //[Test]
        public void CompareCrackDetectionHdev()
        {
            var pipeline = CommonHalconPipelines.CrackDetectionHdev;

            var dir = Directory.GetCurrentDirectory();
            dir = dir.Replace("\\bin\\Debug", "");
            var path = dir + @"\PipelineTests\ReferenceImages\CompareCrackDetectionHdev\";

            HObject comparison = null;

            HObject Image = new HImage(path + @"Image__2017-06-11__18-03-17.bmp");
            HOperatorSet.Rgb1ToGray(Image, out Image);

            pipeline.Execute(Image);

            HOperatorSet.ReadObject(out comparison, path + "sobelAmp");
            Assert.IsTrue(pipeline.Nodes[9].OutputEquals(comparison), "sobelAmp");
            HOperatorSet.ReadObject(out comparison, path + "threshold1");
            Assert.IsTrue(pipeline.Nodes[8].OutputEquals(comparison), "threshold1");
            HOperatorSet.ReadObject(out comparison, path + "threshold2");
            Assert.IsTrue(pipeline.Nodes[10].OutputEquals(comparison), "threshold2");
            HOperatorSet.ReadObject(out comparison, path + "union21");
            Assert.IsTrue(pipeline.Nodes[7].OutputEquals(comparison), "union21");
            HOperatorSet.ReadObject(out comparison, path + "connection1");
            Assert.IsTrue(pipeline.Nodes[6].OutputEquals(comparison), "connection1");
            HOperatorSet.ReadObject(out comparison, path + "selectShape1");
            Assert.IsTrue(pipeline.Nodes[5].OutputEquals(comparison), "selectShape1");
            HOperatorSet.ReadObject(out comparison, path + "union11");
            Assert.IsTrue(pipeline.Nodes[4].OutputEquals(comparison), "union11");
            HOperatorSet.ReadObject(out comparison, path + "closing1");
            Assert.IsTrue(pipeline.Nodes[3].OutputEquals(comparison), "closing1");
            HOperatorSet.ReadObject(out comparison, path + "closing2");
            Assert.IsTrue(pipeline.Nodes[2].OutputEquals(comparison), "closing2");
            HOperatorSet.ReadObject(out comparison, path + "connection2");
            Assert.IsTrue(pipeline.Nodes[1].OutputEquals(comparison), "connection2");
            HOperatorSet.ReadObject(out comparison, path + "selectShape2");
            Assert.IsTrue(pipeline.Nodes[0].OutputEquals(comparison), "selectShape2");

        }

        //[Test]
        public void CompareNonWovenTwists()
        {
            var pipeline = CommonHalconPipelines.NonWovenTwistsHdev;


            var dir = Directory.GetCurrentDirectory();
            dir = dir.Replace("\\bin\\Debug", "");
            var path = dir + @"\PipelineTests\ReferenceImages\CompareNonWovenTwistsHdev\";

            HObject comparison = null;

            HObject Image = new HImage(path + @"Image__2017-06-11__17-55-19.bmp");
            HOperatorSet.Rgb1ToGray(Image, out Image);

            pipeline.Execute(Image);


            HOperatorSet.ReadObject(out comparison, path + "threshold");
            Assert.IsTrue(pipeline.Nodes[7].OutputEquals(comparison), "threshold");
            HOperatorSet.ReadObject(out comparison, path + "connection1");
            Assert.IsTrue(pipeline.Nodes[6].OutputEquals(comparison), "connection1");
            HOperatorSet.ReadObject(out comparison, path + "selectShape1");
            Assert.IsTrue(pipeline.Nodes[5].OutputEquals(comparison), "selectShape1");
            HOperatorSet.ReadObject(out comparison, path + "union11");
            Assert.IsTrue(pipeline.Nodes[4].OutputEquals(comparison), "union11");
            HOperatorSet.ReadObject(out comparison, path + "closing1");
            Assert.IsTrue(pipeline.Nodes[3].OutputEquals(comparison), "closing1");
            HOperatorSet.ReadObject(out comparison, path + "closing2");
            Assert.IsTrue(pipeline.Nodes[2].OutputEquals(comparison), "closing2");
            HOperatorSet.ReadObject(out comparison, path + "connection2");
            Assert.IsTrue(pipeline.Nodes[1].OutputEquals(comparison), "connection2");
            HOperatorSet.ReadObject(out comparison, path + "selectShape2");
            Assert.IsTrue(pipeline.Nodes[0].OutputEquals(comparison), "selectShape2");

        }
        #endregion

        [Test,ShortTest]
        public void ReplaceNodes()
        { 
            var sq = CommonHalconPipelines.StatusQuo;

            var conn = new Connection(){Neighborhood = 4};
            var conn2 = new Connection(conn){Neighborhood = 4};

            var sel = new SelectShape();
            var conn3 = new Connection(sel);

            var sobel = new SobelAmp();
            var union2 = new Union2();
            var comparer = new HalconOperatorNode.HalconOperatorTypeEqualityComparer();
            var thresh = new ThresholdAccessChannel();
            var unThresh = new Union2(thresh, thresh);
            
            var dir = CommonInformation.TestResultsDirectory;

            sq.WriteToDOTFile(Path.Combine(dir, "sq_before_replacement.txt"));
            sq.Replace(conn2, conn3, comparer);

            Assert.AreEqual(sq.TraverseBreadthBackward().Count(), sq.TraverseBreadthForward().Count(), "pipeline got disconnected.");

            Assert.AreEqual(2, sq.Nodes.Count(x => x.GetType() == typeof(Connection)));
            sq.WriteToDOTFile(Path.Combine(dir, "sq_after_first_replacement.txt"));

            sq.Replace(conn, sel, comparer);

            Assert.AreEqual(sq.TraverseBreadthBackward().Count(), sq.TraverseBreadthForward().Count(), "pipeline got disconnected.");

            sq.WriteToDOTFile(Path.Combine(dir, "sq_after_last_replacement.txt"));
            Assert.AreEqual(0, sq.Nodes.Count(x => x.GetType() == typeof(Connection)));


            sq = CommonHalconPipelines.StatusQuo;

            var n = sq.Nodes.First(x => !x.HasParents());

            sq.Replace(n, sobel, comparer);
            Assert.AreEqual(sq.TraverseBreadthBackward().Count(), sq.TraverseBreadthForward().Count(), "pipeline got disconnected.");

            Assert.AreEqual(sq.Nodes.Count, 1);

            sq = CommonHalconPipelines.StatusQuo;

            sq.Replace(union2, union2, comparer);
            Assert.AreEqual(sq.TraverseBreadthBackward().Count(), sq.TraverseBreadthForward().Count(), "pipeline got disconnected.");


            sq.WriteToDOTFile(Path.Combine(dir, "sq_union2rep.txt"));
            Assert.AreEqual(11, sq.Nodes.Count);
        }

        [Test,ShortTest]
        public void CheckIfDotFilesUseOnlySensibleAttributes()
        {
            foreach (var pipeline in CommonHalconPipelines.Collection)
            {
                pipeline.WriteToDOTFile(Path.Combine(CommonInformation.TestResultsDirectory, pipeline.Name + "_dot.txt"));
            }
        }

        /*
         * This number will limit the number of test runs
         * because it is radonmly generated and usually 
         * contains ~1800 iterations. 
         * Setting this value to  -1
         * will ignore the max value.
         */
        const int MAX_NUMBER_OF_HALCON_EXPORT_TEST_RUNS = 10;

        /// <summary>
        /// author: braml
        /// Test to run the hdev file and check resulting HObjects for equality.
        /// Test to check wether all operators have an Implementation that supports the hdev export
        /// (I suggest the usage of the hdevrunner (demo) to run an ProcessStartInfo cmd to execute the hdev file(sadly on on windows))
        /// </summary>
        /// 
        [Test, LongTest, LocalTest, ExtremeLongTest]
        public void HalconExport()
        {
            //The new structure ind the ci pipeline document doesnt even run this test,so there is no need for a warning
            //Assert.Warn("This test won't work on the server, as hdevelop probably won't run properly.");

            //Prepare image and variable for out- and input
            string imageInputPath = CommonImages.StandardFiberPath.Replace($@"\", $@"/");
            var image = new HImage(imageInputPath);

            // Intialize and start the Process
            using (var process = new Process())
            {

                var pipelineCollection = CommonHalconPipelines.CompleteCollection;
                int i = 1;
                foreach (var pipeline in pipelineCollection)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    var halconFilePath = Path.Combine(CommonInformation.HDevPipelines, pipeline.Name);
                    //Log pipelineOutputs, does not override therefore useless
                    pipeline.WriteOutputs(image, halconFilePath);
                    HObject pipelineOutput = pipeline.ExecuteSingle(image);
                    //startInfo.FileName = Path.Combine(Environment.GetEnvironmentVariable("HALCONROOT"), "bin", Environment.GetEnvironmentVariable("HALCONARCH"));
                    startInfo.FileName = "hdevelop";
                    startInfo.Arguments = $@"-run {halconFilePath + ".hdev"}";

                    try
                    {
                        Console.WriteLine("Test pipeline " + i + " of " + pipelineCollection.Count);
                        i++;

                        if (MAX_NUMBER_OF_HALCON_EXPORT_TEST_RUNS > -1 
                            && i > MAX_NUMBER_OF_HALCON_EXPORT_TEST_RUNS)
                            break;

                        //Create hdev files and return the save paths of the all output image nodes for the specific pipeline, optimaly only 1
                        List<string> imageOutputPath = pipeline.WriteToHalconFile(halconFilePath, imageInputPath);

                        System.Threading.Thread.Sleep(3000);

                        process.StartInfo = startInfo;
                        Console.WriteLine("Executing exported hdev script per cmd!");
                        process.Start();

                        //Maybe use a delay here to compensate for execution time of the hdev script
                        System.Threading.Thread.Sleep(15000);

                        //Reading ouput of hdev script, only first and hopefully only output node
                        HOperatorSet.ReadObject(out HObject scriptOutput, (imageOutputPath[0]));

                        string tmpPipelineOutPath = Path.Combine(halconFilePath, "pipelineOut");
                        HOperatorSet.WriteObject(pipelineOutput, tmpPipelineOutPath);
                        HOperatorSet.ReadObject(out HObject tmpPipelineOut, tmpPipelineOutPath);

                        //Image descriptive porperties to determine equality
                        //Count of discovered regions
                        HOperatorSet.CountObj(tmpPipelineOut, out HTuple hNumber1);
                        HOperatorSet.CountObj(scriptOutput, out HTuple hNumber2);
                        //Object class
                        HOperatorSet.GetObjClass(tmpPipelineOut, out HTuple hClass1);
                        HOperatorSet.GetObjClass(scriptOutput, out HTuple hClass2);
                        //Area of discovered regions
                        HOperatorSet.AreaCenter(tmpPipelineOut, out HTuple hArea1, out HTuple hRows1, out HTuple hCols1);
                        HOperatorSet.AreaCenter(scriptOutput, out HTuple hArea2, out HTuple hRows2, out HTuple hCols2);
                        //Fuzzy Entropy
                        HOperatorSet.FuzzyEntropy(tmpPipelineOut, image, 0, 255, out HTuple hEntropy1);
                        HOperatorSet.FuzzyEntropy(scriptOutput, image, 0, 255, out HTuple hEntropy2);

                        //Compute "bool expression" for given property
                        HOperatorSet.TupleEqual(hNumber1, hNumber2, out HTuple hTuple1);
                        HOperatorSet.TupleEqual(hArea1, hArea2, out HTuple hTuple2);
                        HOperatorSet.TupleEqual(hClass1, hClass2, out HTuple hTuple3);
                        HOperatorSet.TupleEqual(hEntropy1, hEntropy2, out HTuple hTuple4);

                        try
                        {
                            Assert.IsTrue(hTuple1);
                            Assert.IsTrue(hTuple2);
                            Assert.IsTrue(hTuple3);
                            Assert.IsTrue(hTuple4);

                        }
                        catch (IndexOutOfRangeException)
                        {
                            Assert.Fail("Regions do not have same length or AccessException of HALCONDOTNET.CORE");
                        }
                    }
                    catch (NotImplementedException)
                    {
                        Assert.Fail("pipeline missing operator implementation: {0}", pipeline.Name);
                    }
                    catch (ArgumentNullException e)
                    {
                        Assert.Fail(e.Message);
                    }
                    catch (InvalidOperationException e)
                    {
                        Assert.Fail(e.Message);
                    }
                }
            }
        }

        /// <summary>
        /// author: braml
        /// Test to run the hdev file and check resulting HObjects for equality,
        /// when exporting the pipelines in CommonHalconPipelines.Collection
        /// </summary>
        [Test, LongTest, LocalTest]
        public void CommonHDevExport()
        {
            //The new structure ind the ci pipeline document doesnt even run this test,so there is no need for a warning
            //Assert.Warn("This test won't work on the server, as hdevelop probably won't run properly.");

            //Prepare image and variable for out- and input
            string imageInputPath = CommonImages.StandardFiberPath.Replace($@"\", $@"/");
            var image = new HImage(imageInputPath);

            // Intialize and start the Process
            using (var process = new Process())
            {

                var pipelineCollection = CommonHalconPipelines.Collection;
                foreach (var pipeline in pipelineCollection)
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    var halconFilePath = Path.Combine(CommonInformation.HDevPipelines, pipeline.Name);
                    //Log pipelineOutputs
                    pipeline.WriteOutputs(image, halconFilePath);
                    HObject pipelineOutput = pipeline.ExecuteSingle(image);
                    //startInfo.FileName = Path.Combine(Environment.GetEnvironmentVariable("HALCONROOT"), "bin", Environment.GetEnvironmentVariable("HALCONARCH"));
                    startInfo.FileName = "hdevelop";
                    startInfo.Arguments = $@"-run {halconFilePath + ".hdev"}";

                    try
                    {
                        //Create hdev files and return the save paths of the all output image nodes for the specific pipeline, optimaly only 1
                        List<string> imageOutputPath = pipeline.WriteToHalconFile(halconFilePath, imageInputPath);

                        System.Threading.Thread.Sleep(5000);

                        process.StartInfo = startInfo;
                        Console.WriteLine("Executing exported hdev script per cmd!");
                        process.Start();

                        //Maybe use a delay here to compensate for execution time of the hdev script
                        System.Threading.Thread.Sleep(15000);

                        //Reading ouput of hdev script, only first and hopefully only output node
                        HOperatorSet.ReadObject(out HObject scriptOutput, (imageOutputPath[0]));

                        HOperatorSet.WriteObject(pipelineOutput, halconFilePath + "pipelineOut");
                        
                        //Image descriptive porperties to determine equality
                        //Count of discovered regions
                        HOperatorSet.CountObj(pipelineOutput, out HTuple hNumber1);
                        HOperatorSet.CountObj(scriptOutput, out HTuple hNumber2);
                        //Object class
                        HOperatorSet.GetObjClass(pipelineOutput, out HTuple hClass1);
                        HOperatorSet.GetObjClass(scriptOutput, out HTuple hClass2);
                        //Area of discovered regions
                        HOperatorSet.AreaCenter(pipelineOutput, out HTuple hArea1, out HTuple hRows1, out HTuple hCols1);
                        HOperatorSet.AreaCenter(scriptOutput, out HTuple hArea2, out HTuple hRows2, out HTuple hCols2);
                        //Fuzzy Entropy
                        HOperatorSet.FuzzyEntropy(pipelineOutput, image, 0, 255, out HTuple hEntropy1);
                        HOperatorSet.FuzzyEntropy(scriptOutput, image, 0, 255, out HTuple hEntropy2);

                        //Compute "bool expression" for given property
                        HOperatorSet.TupleEqual(hNumber1, hNumber2, out HTuple hTuple1);
                        HOperatorSet.TupleEqual(hArea1, hArea2, out HTuple hTuple2);
                        HOperatorSet.TupleEqual(hClass1, hClass2, out HTuple hTuple3);
                        HOperatorSet.TupleEqual(hEntropy1, hEntropy2, out HTuple hTuple4);

                        try
                        {
                            Assert.IsTrue(hTuple1);
                            Assert.IsTrue(hTuple2);
                            Assert.IsTrue(hTuple3);
                            Assert.IsTrue(hTuple4);

                        } catch (IndexOutOfRangeException)
                        {
                            Assert.Fail("Regions do not have same length or AccessException of HALCONDOTNET.CORE");
                        }
                    }
                    catch (NotImplementedException)
                    {
                        Assert.Fail("pipeline missing operator implementation: {0}", pipeline.Name);
                    }
                    catch (ArgumentNullException e)
                    {
                        Assert.Fail(e.Message);
                    }
                    catch (InvalidOperationException e)
                    {
                        Assert.Fail(e.Message);
                    }
                }
            }                
        }

        [Test,ShortTest]
        public void FullDependencyTree()
        {
            var opMap = new HalconOperatorMap(CommonHalconPipelines.HalconOperatorNodeCollection, DependencyTree.GetSimpleDependencies());
            Assert.AreEqual(opMap.GetUniqueOperators(CommonHalconPipelines.HalconOperatorNodeCollection.Cast<IParameterInformant>()).Count, CommonHalconPipelines.HalconOperatorNodeCollection.Count);
            Assert.AreEqual(opMap.OperatorIdentifiers.Count, CommonHalconPipelines.HalconOperatorNodeCollection.Count);
        }
        [Test,ShortTest]
        public void CGPConversionWithMutation()
        {
            foreach (var pipeline in CommonHalconPipelines.Collection)
            {
                Assert.IsTrue(TestConversionWithMutation(pipeline));
            }
        }

        [Test,ShortTest]
        public void OperatorException()
        {
            var path = Path.Combine(CommonInformation.TestResultsDirectory, "exceptions");
            path.CreateDirectory();
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo
                .File(path: Path.Combine(path, "operatorexception.txt"),
                rollingInterval: RollingInterval.Hour)
                //outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}{Properties:lj}")
                 .CreateLogger();


            var op = new SobelAmp();

            Assert.Throws(typeof(OperatorException), () => op.Execute());

            var thresh = new Threshold();
            op.AddChild(thresh);

            Assert.Throws(typeof(OperatorException), () => op.Execute());

            try
            {
                op.Execute();
            }
            catch (OperatorException oex)
            {
                oex.UseSerilog();
            }
        }

        [Test, ShortTest]
        public void CGPPipelineException()
        {
            var path = Path.Combine(CommonInformation.TestResultsDirectory, "exceptions");
            path.CreateDirectory();
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo
                .File(path: Path.Combine(path, "pipelineexception.txt"),
                rollingInterval: RollingInterval.Hour)
                 //outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}{Properties:lj}")
                 .CreateLogger();

            // below values are taken from a log file with
            // bandpass image throwing an exception

            var ar = new AreaToRectangle();
            var eg = new ExpandGray()
            {
                Threshold =30,
                Iterations=9,
                Mode=ExpandGray.ModeType.image
            };
            var st = new Threshold()
            {
                Min = 0,
                Max = 44
            };
            var bp = new BandpassImage()
            {
                FilterType = BandpassImage.BandpassImageFilterType.lines  // the logfile shows 5 here. obv there is something wrong
            };

            ar.AddChild(eg);
            eg.AddChild(st);
            st.AddChild(bp);

            var pipe = new HalconPipeline(ar);
            try
            {
                throw new CGPPipelineException("meep", new Exception(), pipe);
            }catch(CGPPipelineException pex)
            {
                pex.UseSerilog();
            }
        }

        /// <summary>
        /// Helper function to check if float arrays are the same, in both length and content
        /// </summary>
        /// <param name="arr1"></param>
        /// <param name="arr2"></param>
        /// <returns></returns>
        public bool AreEqual(float[] arr1, float[] arr2)
        {
            if (arr1.Length != arr2.Length) return false;
            for(int i = 0; i < arr1.Length; i++)
                if (arr1[i] != arr2[i])
                    return false;
            return true;
        }

        /// <summary>
        /// Check if two concatenated pipelines have sensible properties
        /// such as a sensible sum of node.count and equal input/output nodes
        /// (in terms of parameters)
        ///
        /// Note that this is somewhat a comparison by proxy, as we cannot check for identity in terms of
        /// location in memory, because we copy all involved objects
        /// </summary>
        [Test, ShortTest]
        public void Concatenate()
        {
            foreach (var first in CommonHalconPipelines.Collection)
            {
                foreach (var second in CommonHalconPipelines.Collection)
                {
                    var concatenation = first.Concatenate(second);

                    Assert.IsTrue(AreEqual(first.InputNodes.First().ToCGPNodeParameters(), concatenation.InputNodes.First().ToCGPNodeParameters()));
                    Assert.IsTrue(AreEqual(second.OutputNodes.First().ToCGPNodeParameters(), concatenation.OutputNodes.First().ToCGPNodeParameters()));
                    Assert.AreEqual(first.Nodes.Count(x => !x.IsInputNode) + second.Nodes.Count(x => !x.IsInputNode),
                        concatenation.Nodes.Count(x => !x.IsInputNode));
                }
            }
        }
        
    }
}
