using System.Collections.Generic;
using System.IO;
using Extensions;
using Optimization.CVPipeline.OperatorNodes;

namespace Optimization.CVPipeline
{
    public class CommonCVPipelines
    {

        public static List<CVPipeline> CVPipelineCollection
        {
            get
            {
                return new List<CVPipeline>()
                {
                    GrayImgMorph_Reg, GausImg_Reg/*, GrayImg_Complex, Bin_Reg*/, ColImg_GrayImg
                };
            }
        }

        //Main-test as seen in Halcon-Pipeline, except in Halcon SobelY is used
        /*
         * WARNING: THE FOLLOWING CODE IS COMPLETELY MISPLACED. IT OUGHT TO HAVE BEEN PUT INTO CVPIPELINE.TESTS
         */
        public static CVPipeline GrayImgMorph_Reg
        {
            get
            {
                //Create Gray_Img
                CVDecolor gray_img = new CVDecolor();
                // For transformation from gray_scale to binary image use Adaptive threshold

                CVSobelX sobel1 = new CVSobelX(1, 0, 3); //xOrder locked
                                                         /*has no children, therefore output cant be initialized from CVNode/CGPNode
                                                         * need for fixed InputNodes maybe, that dont need children.output as input
                                                         */
                sobel1.AddChild(gray_img);


                /* in parameters eig noch 4 für thresholdtype to zero inverse
                 * Better Binary threshold for binary image*/
                CVBinaryThreshold thresh1 = new CVBinaryThreshold(128, sobel1);
                /*thresh1.FromCGPNodeParameters(new float[] { 128});*/

                CVOpen open = new CVOpen(new List<CVNode> { thresh1 }, new float[] { 3, 5, 5 });

                var close = new CVClose(new List<CVNode> { open }, new float[] { 3, 5, 5 });

                //var concomp = new CVConnectedComponents();
                //concomp.AddChild(thresh1/*close*/);

                CVPipeline pipeline = new CVPipeline(close);

                pipeline.Name = "CVGrayImgMorph_Reg";

               // var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CommonCVPipelines");
                //dir.CreateDirectory();

               // pipeline.WriteToDOTFile(Path.Combine(dir, "cvgrayimgmoprh_reg.txt"));

                return pipeline;
            }
        }


        public static CVPipeline GausImg_Reg
        {
            get
            {
                var gray_img = new CVDecolor();

                var gaus1 = new CVGaussianBlur();
                gaus1.AddChild(gray_img);

                var sobel1 = new CVSobelX(1, 0, 3); //xOrder locked
                sobel1.AddChild(gaus1);
                //No Idea if Binary, Adaptive or just CVThreshold
                // 128 + 15 = 143 for upper bound
                var thresh1 = new CVBinaryThreshold(143, sobel1);                              
                                
                CVOpen open = new CVOpen(new List<CVNode> { thresh1 }, new float[] { 3, 5, 5 });

                CVClose close = new CVClose(new List<CVNode> { open }, new float[] { 3, 5, 5 });


                //var concomp = new CVConnectedComponents();
                //concomp.AddChild(close);

                var pipeline = new CVPipeline(close);

                pipeline.Name = "CVGausImg_Reg";

                //var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CommonCVPipelines");
                //dir.CreateDirectory();

                //pipeline.WriteToDOTFile(Path.Combine(dir, "cvgausimg_reg.txt"));
                return pipeline;
            }
        }

        /*
        public static CVPipeline AndAnotherPipeline
        {
            get
            {
                var gray_img = new CVDecolor();
                //var gaus1 = new CVGaussianBlur();
                var sobel1 = new CVSobelX(1, 0, 3); //yOrder locked
                sobel1.AddChild(gray_img);
                //No Idea if Binary, Adaptive or just CVThreshold
                // 128 + 15 = 143 for upper bound
                var thresh1 = new CVBinaryThreshold(143, sobel1);

                //var gaus2 = new CVGaussianBlur();
                var sobel2 = new CVSobel(0, 1, 3); //yOrder locked
                //sobel2.AddChild(gaus2);
                // 128 - 15 = 113 for lower bound
                var thresh2 = new CVBinaryThreshold(113, sobel2);

                var union = new CVUnion(thresh1, thresh2);


                CVOpen open = new CVOpen(new List<CVNode> { thresh1 }, new float[] { 3, 5, 5 });

                CVClose close = new CVClose(new List<CVNode> { open }, new float[] { 3, 5, 5 });

                var concomp = new CVConnectedComponents();
                concomp.AddChild(close);

                var pipeline = new CVPipeline( concomp );

                pipeline.Name = "CVAndAnotherPipeline";

                var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CommonCVPipelines");
                dir.CreateDirectory();

                pipeline.WriteToDOTFile(Path.Combine(dir, "cvandanotherpipelien.txt"));
                return pipeline;                
            }
        } 
        */

        public static CVPipeline Bin_Reg
        {
            get
            {
                var gray_img = new CVDecolor();

                var sobel = new CVSobel(1, 1, 3);
                //Sobel in x & y direction
                sobel.AddChild(gray_img);

                var thres = new CVBinaryThreshold(128, sobel);

                var concomp = new CVConnectedComponents();
                concomp.AddChild(thres);

                var pipeline = new CVPipeline(concomp);
                pipeline.Name = "CVBin_RegPipeline";

                //var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CommonCVPipelines");
                //dir.CreateDirectory();

                //pipeline.WriteToDOTFile(Path.Combine(dir, "cvbin_reg.txt"));
                return pipeline;
            }
        }

        public static CVPipeline ColImg_Bin
        {
            get
            {
                var col_img = new CVNone();
                var gray_img = new CVDecolor();
                gray_img.AddChild(col_img);

                var bin_thres = new CVBinaryThreshold(128, gray_img);

                /*var concomp = new CVConnectedComponents();
                concomp.AddChild(bin_thres);*/

                var pipeline = new CVPipeline(bin_thres);
                pipeline.Name = "CVColImg_BinPipeline2";

                //var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CommonCVPipelines");
               // dir.CreateDirectory();

                //pipeline.WriteToDOTFile(Path.Combine(dir, "cvcolimg_bin.txt"));
                return pipeline;
            }
        }

        public static CVPipeline ColImg_GrayImg
        {
            get
            {
                var none = new CVNone();

                var gray_img = new CVDecolor();
                gray_img.AddChild(none);

                var pipeline = new CVPipeline(gray_img);
                pipeline.Name = "CVColImg_GrayImg";

               // var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CommonCVPipelines");
               // dir.CreateDirectory();

               // pipeline.WriteToDOTFile(Path.Combine(dir, "cvcolimg_grayimg.txt"));
                return pipeline;
            }
        }

        public static CVPipeline Complex_GrayImg
        {
            get
            {
                var gray_img = new CVDecolor();

                var sobel = new CVSobel(1, 1, 3);
                sobel.AddChild(gray_img);

                var pipeline = new CVPipeline(sobel);
                pipeline.Name = "CVComplex_GrayImg";

               // var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CommonCVPipelines");
               // dir.CreateDirectory();

               // pipeline.WriteToDOTFile(Path.Combine(dir, "cvcomplex_grayimg.txt"));
                return pipeline;
            }
        }
        
        //Can be expanded to binary image to complex
        public static CVPipeline GrayImg_Complex
        {
            get
            {
                var gray_img = new CVDecolor();

                //FFT
                var fft = new CVForwardFourier();
                fft.AddChild(gray_img);

                //Frequency Filter
                //Use various Low-Pass Filter: Bur, BoxFilter, BilateralFilter, Median or Gaussian blur(with CvgetGausKernel)
                var filter = new CVMedianBlur();
                filter.AddChild(fft);


                //IFT
                var ift = new CVInverseFourier();
                ift.AddChild(filter);

                var pipeline = new CVPipeline(ift);
                pipeline.Name = "CVGrayImg_Complex";

               // var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CommonCVPipelines");
               // dir.CreateDirectory();

              //  pipeline.WriteToDOTFile(Path.Combine(dir, "cvgrayimg_complex.txt"));
                return pipeline;
            }
        }

        public static CVPipeline GrayImg_Magn
        {
            get
            {
                var gray_img = new CVDecolor();

                var sobel = new CVSobel(1, 1, 3);
                sobel.AddChild(gray_img);

                var pipeline = new CVPipeline(sobel);
                pipeline.Name = "CVGrayImg_Magn";

              //  var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CommonCVPipelines");
              //  dir.CreateDirectory();

             //   pipeline.WriteToDOTFile(Path.Combine(dir, "cvgrayimg_magn.txt"));
                return pipeline;
            }
        }

        public static CVPipeline GrayImg_Orient
        {
            get
            {
                var gray_img = new CVDecolor();

                var sobel = new CVSobel(1, 1, 3);
                sobel.AddChild(gray_img);

                var fft = new CVForwardFourier();
                fft.AddChild(sobel);

                var pipeline = new CVPipeline(fft);
                pipeline.Name = "CVGrayImg_Orient";

              //  var dir = Path.Combine(CommonInformation.TestResultsDirectory, "CommonCVPipelines");
              //  dir.CreateDirectory();

              //  pipeline.WriteToDOTFile(Path.Combine(dir, "cvgrayimg_orient.txt"));
                return pipeline;
            }
        }

        public static List<CVNode> NodeCollection
        {
            get
            /*
            {
            string nspace = "PRIME.CVPipeline.CVOperatorNodes";
            var types = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(t => t.GetTypes())
                            .Where(t => t.IsClass && t.Namespace == nspace && !t.Name.Contains("<>c"));

            var list = new List<CVNode>();
            foreach (var t in types)
            {
                list.Add(Activator.CreateInstance(t) as CVNode);
            }
            if (list.Contains(null)) throw new NullReferenceException("Apparently something went wrong when trying to instantiate the cv nodes");
            return list;
            }*/
            {
                return new List<CVNode>
                {
                new CVAdaptiveThreshold(),
                //new CVBackgroundSubtractorMOG(),
                new CVBilateralFilter(),
                new CVBinaryThreshold(),
                new CVBlur(),
                new CVBoxFilter(),
                new CVCalcOpticalFlowFarneback(),
                new CVCanny(),
                new CVClose(),
                //new CVConnectedComponents(),
                //new CVConnectedComponentsWithStats(),
                new CVCornerHarris(),
                new CVDct(),
                new CVDecolor(),
                new CVDilate(),
                new CVEdgePreservingFilter(),
                new CVEqualizeHist(),
                new CVErode(),
                new CVFastNlMeansDenoisingColored(),
                new CVFilter2D(),
                new CVFilterSpeckles(),
                new CVForwardFourier(),
                new CVGaussianBlur(),
                //new CVGrabCut(),
                //new CVHoughCircles(),
                //new CVHoughLines(),
                new CVInverseFourier(),
                new CVInvert(),
                //new CVkMeans(),
                new CVLaplacian(),
                new CVMedianBlur(),
                new CVMorphologyBlackHat(),
                new CVMorphologyClose(),
                new CVMorphologyDilate(),
                new CVMorphologyErode(),
                new CVMorphologyGradient(),
                new CVMorphologyOpen(),
                new CVMorphologyTopHat(),
                new CVNone(),
                new CVNormalize(),
                new CVOpen(),
                new CVPyrDown(),
                new CVPyrUp(),
                new CVPyrMeanShiftFiltering(),
                new CVResize(),
                new CVResizeForFrame(),
                new CVSobel(),
                new CVSobelX(),
                new CVSobelY(),
                new CVSwap(),
                new CVThreshold(),
                new CVUnion(),
                new CVWatershed()
                };
            }
        }
    }
}
