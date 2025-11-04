using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class HistoToThresh : HalconThresholdNode
    {
        public override int CGPInputCount
        {
            get
            {
                return 2;
            }
        }

        public override List<float>[] CGPParameterBounds
        {
            get
            {
                return new List<float>[]
                {
                    new List<float>()
                    {
                        0.5f, 1, 2, 3, 4, 5
                    }
                };
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageAndRegionToRegion | OperatorType.Threshold;
            }
        }

        public float Sigma { get; set; } = 1;

        public override List<string> HalconFunctionCall()
        {

            List<string> lines = new List<string>();

            string imageTypeOutput = "Img_type";
            string typeText = HObjectExtensions.ImageTypeHalconText(OutputVariableName, imageTypeOutput);

            lines.Add(typeText);
            lines.Add($"if (( {imageTypeOutput} # 'byte') and ({imageTypeOutput} # 'uint2') and ({imageTypeOutput} # 'int4') )");

            string convOutput = "Conv_out";
            var convText = HObjectExtensions.ConvertStandardHalconText(OutputVariableName, convOutput);
            lines.AddRange(convText);

            lines.Add($"union1({OutputVariableName}, RegionUnion)");
            lines.Add($"gray_histo(RegionUnion, Image, AbsoluteHisto, RelativeHisto)");

            lines.Add($"if(|AbsoluteHisto| <= 0)");
            lines.Add($"{OutputVariableName} := RegionUnion");
            lines.Add($"endif");

            lines.Add($"histo_to_thresh(AbsoluteHisto, {Sigma}, MinThresh, MaxThresh)");

            lines.Add($"threshold(Image, {OutputVariableName}, MinThresh, MaxThresh)");
            
            return lines;
        }


        /// <summary>
        /// Look at the HALCON documentation of histo_to_thresh. This should treat different image types differently.
        /// We should implement some sort of image type checking/conversion anyway to reduce the chance of GrayValue exceptions.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override HObject Execute(HObject input)
        {
            using (var abs = base.Execute(GatherImage()))
            {
                if (!abs.IsImageType("byte"))
                    using (var conv = abs.ConvertToStandardType())
                        return ExecuteUsing(conv);
                else
                    return ExecuteUsing(abs);
            }
        }


        private HObject ExecuteUsing(HObject input)
        {
            HTuple minThresh, maxThresh;
            HTuple absHisto, relHisto;

            HObject regionUnion = null;
            HOperatorSet.Union1(GatherRegions(), out regionUnion);

            HOperatorSet.GrayHisto(regionUnion, input, out absHisto, out relHisto);

            if (absHisto.Type == HTupleType.EMPTY)
            {
                output = regionUnion;
                return output;
            }

            // introduce image type checking + use absHisto/relHisto accordingly (+ scaling of threshold. see halcon doc)
            HOperatorSet.HistoToThresh(absHisto, Sigma, out minThresh, out maxThresh);


            HOperatorSet.Threshold(input, out output, minThresh, maxThresh);
            return output;
        }

        private HObject GatherImage()
        {
            var image = Children.First(x => x.IsOrOperatorType(OperatorType.ImageToImage)) as HalconOperatorNode;
            if (image == null) throw new Exception("HistoToThresh expects one child of type ImageToImage");
            return image.Output;
        }

        private HObject GatherRegions()
        {
            var regions = Children.First(x => x.IsOrOperatorType(OperatorType.RegionToRegion) ||
                                                x.IsOrOperatorType(OperatorType.ImageToRegion) ||
                                                x.IsOrOperatorType(OperatorType.ImageAndRegionToRegion)) as HalconOperatorNode;
            if (regions == null) throw new Exception("HistoToThresh expects one child of type OperatorType.XToRegion");
            return regions.Output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Sigma = parameters[0];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Sigma };
        }
    }
}
