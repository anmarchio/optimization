using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Optimization.HPipeline.OperatorNodes;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline
{
    /// <summary>
    /// 
    /// </summary>
    public static class CommonHalconPipelines
    {
        /// <summary>
        /// author: leen
        /// This collection contains all currently explored pipelines that have been discovered for specific purposes
        /// </summary>
        #region HalconPipelines
        public static List<HalconPipeline> Collection
        {
        
            get
            {
                var list = new List<HalconPipeline>();
                list.Add(StatusQuo);
                list.Add(StatusQuoOptimizerTest);
                list.Add(StatusQuoFromCF);
                list.Add(StatusQuoPANOX);
                list.Add(FiberOrientationHdev);
                list.Add(FiberOrientationBrightHdev);
                list.Add(CrackDetectionHdev);
                list.Add(NonWovenTwistsHdev);
                list.Add(Threshold);
                list.Add(PeronaMalik);
                list.Add(BramlBA);
                list.Add(ExistCarbonFiber);
                return list;
            }
        }

        /// <summary>
        /// author: braml
        /// This includes all pipelines that can be build by using the three categories of HalconOperatorNodes
        /// and that are exportable into a hdev format, therefore support all current tests
        /// </summary>
        public static List<HalconPipeline> CompleteCollection
        {
            get
            {
                var list = new List<HalconPipeline>();

                var img2ImgOperators = from node in CommonHalconPipelines.HalconOperatorNodeCollection
                                       where (node.OperatorType & OperatorType.ImageToImage) == OperatorType.ImageToImage
                                       select node;

                /*
                 * Removed the LaplaceOfGuassian Node from Collection
                 * To fix the test run and avoid issues
                 */
                // img2ImgOperators = img2ImgOperators.Skip(18);

                var img2RegionOperators = from node in CommonHalconPipelines.HalconOperatorNodeCollection
                                          where (node.OperatorType & OperatorType.ImageToRegion) == OperatorType.ImageToRegion
                                          select node;

                var region2RegionOperators = from node in CommonHalconPipelines.HalconOperatorNodeCollection
                                             where (node.OperatorType & OperatorType.RegionToRegion) == OperatorType.RegionToRegion
                                             select node;
                int i = 0, j = 0, k = 0;
                var inputnode = new HalconInputNode();
                foreach (var imgnode in img2ImgOperators)
                {
                    i++;
                    var node1 = imgnode;
                    node1.AddChild(inputnode);

                    //Maybe add try catch to check wether implementation for export is present
                    try
                    {
                        var checklines = node1.HalconFunctionCall();
                    } catch (NotImplementedException)
                    {
                        continue;
                    }
                    foreach (var imgregnode in img2RegionOperators)
                    {
                        j++;
                        var node2 = imgregnode;
                        node2.AddChild(node1);

                        //Maybe add try catch to check wether implementation for export is present
                        try
                        {
                            var checklines = imgregnode.HalconFunctionCall();
                        }
                        catch (NotImplementedException)
                        {
                            continue;
                        }
                        foreach (var regnode in region2RegionOperators)
                        {
                            k++;
                            var node3 = regnode;
                            node3.AddChild(node2);
                            //Maybe add try catch to check wether implementation for export is present
                            try
                            {
                                var checklines = regnode.HalconFunctionCall();
                            }
                            catch (NotImplementedException)
                            {
                                continue;
                            }

                            var pipeline = new HalconPipeline(node3) { Name = String.Format("Member" + "{0}" + "{1}" + "{2}", i.ToString(), j.ToString(), k.ToString() )};

                            list.Add((HalconPipeline) pipeline.Copy());

                            node1.RemoveAll();
                            node2.RemoveAll();
                            node3.RemoveAll();

                            inputnode = new HalconInputNode();
                        }
                    }
                }
                    
                return list;
            }
        }

        public static Dictionary<string, HalconPipeline> HalconPipelineDictionary
        {
            get
            {
                var dict = Collection.ToDictionary(x => x.Name, x => x);
                return dict;
            }

        }

        public static List<HalconOperatorNode> HalconOperatorNodeCollection
        {
            get
            {
                var typeOf = typeof(HalconOperatorNode);
                var list = new List<HalconOperatorNode>();

                foreach (Type type in Assembly.GetAssembly(typeOf).GetTypes()
                    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeOf)))
                {
                    list.Add(Activator.CreateInstance(type) as HalconOperatorNode);
                }

                if (list.Contains(null)) throw new NullReferenceException("Apparently something went wrong when trying to instantiate the cv nodes");
                return list;
            }
        }

        public static List<HalconOperatorNode> FastHalconOperatorNodes
        {
            get
            {
                var path = Configuration.HalconFastOperatorsPath;
                try
                {
                    using (var reader = new StreamReader(path))
                    {
                        var ser = new XmlSerializer(typeof(List<string>));
                        return Node.DeserializeOperatorList((List<string>) ser.Deserialize(reader)).Cast<HalconOperatorNode>().ToList();
                    }
                }
                catch (FileNotFoundException ex)
                {
                    Serilog.Log.Logger.Error(ex, $"Path: {path} does not exist. Using HalconOperatorNodeCollection instead.");
                    return HalconOperatorNodeCollection;
                }
            }
        }

        public static List<HalconOperatorNode> GetFastHalconOperatorNodes(string pathToFastOperatorXml)
        {
            try
            {
                using (var reader = new StreamReader(pathToFastOperatorXml))
                {
                    var ser = new XmlSerializer(typeof(List<string>));
                    return Node.DeserializeOperatorList((List<string>)ser.Deserialize(reader)).Cast<HalconOperatorNode>().ToList();
                }
            }
            catch (FileNotFoundException ex)
            {
                Serilog.Log.Logger.Error(ex, $"Path: {pathToFastOperatorXml} does not exist. Using HalconOperatorNodeCollection instead.");
                return HalconOperatorNodeCollection;
            }
        }


        public static HalconPipeline StatusQuo
        {
            get
            {
                // status quo pipeline
                var sobelAmp = new SobelAmp(null, SobelAmp.SobelFilterType.y, 3);
                var threshold1 = new Threshold(sobelAmp, 0, 117);
                // threshold2 = new Threshold(sobelAmp, 0, 117);
                //var union2 = new Union2(threshold1, threshold2);
                var connection1 = new Connection(threshold1, Connection.Neighborhood4);
                var connection2 = new Connection(connection1, Connection.Neighborhood4);
                var selectShape1 = new SelectShape(connection2, 6, 99999, SelectShape.SelectShapeFeatureTypes.area);
                var union1 = new Union1(selectShape1);

                var closing = new Closing(union1, StructElementTypes.Circle, 0.8f, 0);

                var closing2 = new Closing(closing, StructElementTypes.Rectangle, 20, 5);
                var connection3 = new Connection(closing2, Connection.Neighborhood4);
                var selectShape = new SelectShape(connection3, 70, 99999, SelectShape.SelectShapeFeatureTypes.area); // root (last operator)
                var pipeline = new HalconPipeline(selectShape) { Name = "StatusQuo" };
                return pipeline;
            }
        }

        public static HalconPipeline ExistCarbonFiber
        {
            get
            {
                // status quo pipeline
                var sobelAmp = new SobelAmp(null, SobelAmp.SobelFilterType.y, 3);
                var threshold = new Threshold(sobelAmp, 12, 128);
                var connection1 = new Connection(threshold, Connection.Neighborhood4);
                var connection2 = new Connection(connection1, Connection.Neighborhood4);
                var selectShape1 = new SelectShape(connection2, 6, 99999, SelectShape.SelectShapeFeatureTypes.area);
                var union1 = new Union1(selectShape1);
                var closing = new Closing(union1, StructElementTypes.Circle, 0.8f, 0);
                var closing2 = new Closing(closing, StructElementTypes.Rectangle, 20, 5);
                var connection3 = new Connection(closing2, Connection.Neighborhood4);
                var selectShape = new SelectShape(connection3, 70, 99999, SelectShape.SelectShapeFeatureTypes.area); // root (last operator)
                var pipeline = new HalconPipeline(selectShape) { Name = "ExistCarbonFiber" };
                return pipeline;
            }
        }

        public static HalconPipeline PeronaMalik
        {
            get
            {
                var anisotropic = new AnisotropicDiffusion();
                var otsu = new AutoThreshold();
                otsu.AddChild(anisotropic);
                var union1 = new Union1(otsu);
                var pipeline = new HalconPipeline(union1);
                pipeline.Name = "PeronaMalik";
                return pipeline;
            }
        }

        public static HalconPipeline StatusQuoOptimizerTest
        {
            get
            {
                var sobelAmp = new SobelAmp(null, SobelAmp.SobelFilterType.y, 3);
                //Changed bacuse of different behaviour
                //var thresholdAccessChannel1 = new ThresholdAccessChannel(sobelAmp, 3, 12, -1);

                var threshold = new Threshold(sobelAmp, 0, (128 - 23));

                var union1 = new Union1(threshold);

                //subject to change, needs a connection beforehand
                // var connection = new Connection();
                var selectShape1 = new SelectShape(union1, 5, 99999, SelectShape.SelectShapeFeatureTypes.anisometry);
                //A union operator following a selectShape operator just eridacates all progress and usefull output
                var pipeline = new HalconPipeline(selectShape1);
                pipeline.Name = "StatusQuoOptimizerTest";
                return pipeline;
            }
        }

        public static HalconPipeline StatusQuoFromCF
        {
            get
            {
                // status quo pipeline
                var sobelAmp = new SobelAmp(null, SobelAmp.SobelFilterType.y, 5);
                //Changed bacuse of different behaviour
                //var thresholdAccessChannel1 = new ThresholdAccessChannel(sobelAmp, 1, 5, -1);
                //var thresholdAccessChannel2 = new ThresholdAccessChannel(sobelAmp, 1, 5, 1);
                var threshold1 = new Threshold(sobelAmp, 0, 127 - 5);
                //var threshold2 = new Threshold(sobelAmp, 0, 127 - 5);
                //var union2 = new Union2(threshold1, threshold2);
                var connection1 = new Connection(threshold1, Connection.Neighborhood4);
                var connection2 = new Connection(connection1, Connection.Neighborhood4);
                var selectShape1 = new SelectShape(connection2, 53, 99999, SelectShape.SelectShapeFeatureTypes.area);
                var union1 = new Union1(selectShape1);

                var closing = new Closing(union1, StructElementTypes.Circle, 10, 2);

                var closing2 = new Closing(closing, StructElementTypes.Rectangle, 13, 7);
                var connection3 = new Connection(closing2, Connection.Neighborhood4);
                var selectShape = new SelectShape(connection3, 76, 99999, SelectShape.SelectShapeFeatureTypes.area); // root (last operator)
                var pipeline = new HalconPipeline(selectShape);
                pipeline.Name = "StatusQuoFromCF";
                return pipeline;
            }
        }

        public static HalconPipeline StatusQuoPANOX
        {
            get
            {
                // status quo pipeline
                var sobelAmp = new SobelAmp(null, SobelAmp.SobelFilterType.y, 5);
                //Changed bacuse of different behaviour
                //var thresholdAccessChannel1 = new ThresholdAccessChannel(sobelAmp, 1, 22, -1);
                //var thresholdAccessChannel2 = new ThresholdAccessChannel(sobelAmp, 1, 12, 1);
                var threshold1 = new Threshold(sobelAmp, 0, 127 - 22);
                //var threshold2 = new Threshold(sobelAmp, 0, 127 + 12);
                //var union2 = new Union2(threshold1, threshold2);
                var connection1 = new Connection(threshold1, Connection.Neighborhood4);
                var connection2 = new Connection(connection1, Connection.Neighborhood4);
                var selectShape1 = new SelectShape(connection2, 48, 99999, SelectShape.SelectShapeFeatureTypes.area);
                var union1 = new Union1(selectShape1);

                var closing = new Closing(union1, StructElementTypes.Circle, 0.8f, 0);

                var closing2 = new Closing(closing, StructElementTypes.Ellipse, 4, 14);
                var connection3 = new Connection(closing2, Connection.Neighborhood8);
                var selectShape = new SelectShape(connection3, 85, 99999, SelectShape.SelectShapeFeatureTypes.area); // root (last operator)
                var pipeline = new HalconPipeline(selectShape);
                pipeline.Name = "StatusQuoPANOX";
                return pipeline;
            }
        }

        public static HalconPipeline FiberOrientationHdev
        {
            get
            {
                var edgesImage = new EdgesImage(EdgesImage.EdgesImageFilterType.canny, 1, EdgesImage.NMS.nms, 20, 40);
                var threshold = new Threshold(edgesImage, 30, 255);
                var connection1 = new Connection(threshold);
                var selectShape1 = new SelectShape(connection1, 80, 999999, SelectShape.SelectShapeFeatureTypes.area);
                var union1 = new Union1(selectShape1);
                var closing1 = new Closing(union1, StructElementTypes.Circle, 5, 0);
                var closing2 = new Closing(closing1, StructElementTypes.Rectangle, 20, 5);
                var connection2 = new Connection(closing2);
                var selectShape2 = new SelectShape(connection2, 300, 999999, SelectShape.SelectShapeFeatureTypes.area);
                var pipeline = new HalconPipeline(selectShape2);
                pipeline.Name = "FiberOrientationHdev";
                return pipeline;
            }
        }

        public static HalconPipeline FiberOrientationBrightHdev
        {
            get
            {
                var edgesImage = new EdgesImage(EdgesImage.EdgesImageFilterType.canny, 1, EdgesImage.NMS.nms, 20, 40);
                var threshold = new Threshold(edgesImage, 190, 255);
                var connection1 = new Connection(threshold);
                var selectShape1 = new SelectShape(connection1, 150, 999999, SelectShape.SelectShapeFeatureTypes.area);
                var union1 = new Union1(selectShape1);
                var closing1 = new Closing(union1, StructElementTypes.Circle, 8, 0);
                var closing2 = new Closing(closing1, StructElementTypes.Rectangle, 20, 5);
                var connection2 = new Connection(closing2);
                var selectShape2 = new SelectShape(connection2, 500, 999999, SelectShape.SelectShapeFeatureTypes.area);
                var pipeline = new HalconPipeline(selectShape2);
                pipeline.Name = "FiberOrientationBrightHdev";
                return pipeline;
            }
        }

        public static HalconPipeline CrackDetectionHdev
        {
            get
            {
                var sobelAmpEquHisto = new SobelAmpEquHistoImage(null, SobelAmp.SobelFilterType.y, 7);
                var threshold1 = new Threshold(sobelAmpEquHisto, 0, 127 -30);
                //var threshold2 = new Threshold(sobelAmpEquHisto, 0, 127 + 30);
                //var union2 = new Union2(threshold1, threshold2);
                var connection1 = new Connection(threshold1);
                var selectShape1 = new SelectShape(connection1, 200, 99999, SelectShape.SelectShapeFeatureTypes.area);
                var union1 = new Union1(selectShape1);
                var closing1 = new Closing(union1, StructElementTypes.Circle, 4, 0);
                var closing2 = new Closing(closing1, StructElementTypes.Rectangle, 20, 5);
                var connection2 = new Connection(closing2);
                var selectShape2 = new SelectShape(connection2, 500, 99999, SelectShape.SelectShapeFeatureTypes.area);
                var pipeline = new HalconPipeline(selectShape2);
                pipeline.Name = "CrackDetectionHdev";
                return pipeline;
            }
        }

        public static HalconPipeline BramlBA
        {
            get
            {
                var sobel2 = new SobelAmp() { FilterType = SobelAmp.SobelFilterType.y, MaskSize = 7 };
                //Changed bacuse of different behaviour
                //var thresholdAccessChannel1 = new ThresholdAccessChannel(sobelAmp, 1, 22, -1);
                //var thresholdAccessChannel2 = new ThresholdAccessChannel(sobelAmp, 1, 12, 1);
                var threshold1 = new Threshold(sobel2, 0, 127 - 12);
                //var threshold2 = new Threshold(sobel2, 0, 127 + 12);
                var union1 = new Union1(new List<HalconOperatorNode>() { threshold1 });
                var connection1 = new Connection(union1);
                var connection2 = new Connection(connection1);
                var selectShape = new SelectShape(connection2, 7, 99999, SelectShape.SelectShapeFeatureTypes.area);
                var union2 = new Union1(selectShape);
                var closing1 = new Closing(union2, StructElementTypes.Circle, 0.8f, 0.8f);
                var closing2 = new Closing(closing1, StructElementTypes.Rectangle, 20, 5);
                var connection3 = new Connection(closing2);
                var selectShape2 = new SelectShape(connection3, 7, 99999, SelectShape.SelectShapeFeatureTypes.area);
                var pipeline = new HalconPipeline(selectShape2);
                pipeline.Name = "BramlBA";
                return pipeline;

            }
        }

        public static HalconPipeline NonWovenTwistsHdev
        {
            get
            {
                
                var threshold = new Threshold() { Min = 0, Max = 73};
                var connection1 = new Connection(threshold);
                var selectShape1 = new SelectShape(connection1, 500, 999999, SelectShape.SelectShapeFeatureTypes.area);
                var union1 = new Union1(selectShape1);
                var closing1 = new Closing(union1, StructElementTypes.Circle, 8, 0);
                var closing2 = new Closing(closing1, StructElementTypes.Rectangle, 20, 5);
                var connection2 = new Connection(closing2);
                var selectShape2 = new SelectShape(connection2, 800, 999999, SelectShape.SelectShapeFeatureTypes.area);
                var pipeline = new HalconPipeline(selectShape2);
                pipeline.Name = "NonWovenTwistsHdev";
                return pipeline;
            }
        }

        public static HalconPipeline Threshold
        {
            get
            {
                
                var threshold = new Threshold() { Min = 45, Max = 255 };
                var pipeline = new HalconPipeline(threshold);
                pipeline.Name = "Threshold";
                return pipeline;
            }
        }

        /*
        public static HalconPipeline NittingFFT
        {
            get
            {
                //var noFilter = new NoFilter(null);
                var cropSmallestRectangle = new CropSmallestRectangle(null, 20, 255);
                var fft = new FFT(cropSmallestRectangle, 10.0f, 2.0f, 0, FftFilterType.Gauss);
                // Size < maxSize && Size > minSize; threshold: 40 - 255
                var areaSizeThreshold = new AreaSizeThreshold(fft, 60, 255, 10000, 20000, 200, 300);
                //var noMorphological = new NoMorphological(areaSizeThreshold);
                var pipeline = new HalconPipeline(areaSizeThreshold);
                return pipeline;
            }
        }

        public static HalconPipeline NittingFFTMorphologic
        {
            get
            {
                var cropSmallestRectangle = new CropSmallestRectangle(null, 20, 255);
                var fft = new FFT(cropSmallestRectangle, 0.1f, 0.2f, 11, FftFilterType.Bandpass);
                var threshold = new Threshold(fft, -500, 50);
                var connection = new Connection(threshold);
                var erosion = new Erosion(connection, StructElementTypes.Circle, 5.0f, 5.0f);
                var connection2 = new Connection(erosion);
                var selectShape = new SelectShape(connection2, 10000, 99999, SelectShapeFeatureTypes.area);
                var areaToRectangle = new AreaToRectangle(selectShape);
                var pipeline = new HalconPipeline(areaToRectangle);
                return pipeline;
            }
        }*/

        /*
        public static HalconPipeline NittingGap
        {
            get
            {
                var gapRatio = new Ratio(null, 45, 15, 15, 0.075f);
                var pipeline = new HalconPipeline(gapRatio);
                return pipeline;
            }
        }*/

        /*
    public static HalconPipeline LooseFilaments
    {
        get
        {
            var noFilter = new NoFilter(null);
            var threshold = new Threshold(noFilter, 20, 255);
            var fillUp = new FillUp(threshold);
            var innerRectangle = new InnerRectangle(fillUp);
            var genRectangle = new GenRectangle(innerRectangle, 35);
            var difference = new Difference(genRectangle);
            var reduceDomain = new ReduceDomain(difference);
            var fastThreshold = new FastThreshold(reduceDomain);
            var feature = new HFeatureSet(fastThreshold, "connect_num", 90);
            var feature = new HFeatureSet(fastThreshold, "column", MinFilamentExp);
            var area = new Area(fastThreshold, 5000);
            var union = new Union2(area);
            var pipeline = new HalconPipeline(union);
            return pipeline;
        }
    }*/

        #endregion


        #region Ad-hoc appending of pipelines

        /// <summary>
        /// Creates a "select-shape" pipeline intended for concatenation 
        /// </summary>
        /// <param name="areaSizeThresholdMin"></param>
        /// <param name="areaSizeThresholdMax"></param>
        /// <returns></returns>
        public static HalconPipeline GetSelectShape(int areaSizeThresholdMin, int areaSizeThresholdMax)
        {
            var selectShape = new SelectShape
            {
                Min = areaSizeThresholdMin,
                Max = areaSizeThresholdMax,
                Features = SelectShape.SelectShapeFeatureTypes.area,
            };
            return new HalconPipeline(selectShape);
        }

        /// <summary>
        /// Creates a "closing" pipeline intended for concatenation
        /// Closing structure element is a circle, A it's radius
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static HalconPipeline GetClosing(float A, float B)
        {
            var closing = new Closing
            {
                StructElementType = StructElementTypes.Circle,
                A = A,
                B = B
            };
            return new HalconPipeline(closing);
        }

        public static HalconPipeline GetOpening(float A, float B)
        {
            var opening = new Opening
            {
                StructElement = StructElementTypes.Circle,
                A = A,
                B = B
            };
            return new HalconPipeline(opening);
        }
        #endregion
    }
}

