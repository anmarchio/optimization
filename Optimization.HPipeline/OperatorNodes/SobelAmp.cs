using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class SobelAmp : HalconOperatorNode
    {

        public SobelAmp() : base()
        {
            Initialize(null);
        }

        public SobelAmp(HObject inputImage, SobelFilterType filterType, int maskSize) : base()
        {
            Initialize(inputImage);

            FilterType = filterType;
            MaskSize = maskSize;
        }

        public SobelAmp(IList<HalconOperatorNode> ignoreThis, float[] parameters) : base()
        {
            FromCGPNodeParameters(parameters);
        }

        private void Initialize(HObject inputImage)
        {
            MaskSize = 3;
        }

        public SobelFilterType FilterType { get; set; }

        public enum SobelFilterType
        {
            y, y_binomial, x, x_binomial
        }

        public int MaskSize { get; set; }

        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if (cgpParameterBounds == null)
                {
                    var operatorMap = new OperatorMap();
                    cgpParameterBounds = operatorMap.ParameterBounds[0];
                }
                return cgpParameterBounds;
            }
        }
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return DecodingMap.sobelAmp;
            }
        }

        public override void DisposeOutput()
        {
            if (output == null) return;
            output.Dispose();
            output = null;
        }

        public override HObject Execute(HObject input)
        {
            if (FilterType == SobelFilterType.x_binomial || FilterType == SobelFilterType.y_binomial)
            {
                if (!input.IsImageType("byte", "uint2", "real"))
                    using (var conv = input.ConvertToStandardType())
                    {
                        HOperatorSet.SobelAmp(conv, out output, FilterType.ToString(), MaskSize);
                        return output;
                    }
            }
            else if (!input.IsImageType("byte", "int2", "uint2", "real"))
            {
                using (var conv = input.ConvertToStandardType())
                {
                    HOperatorSet.SobelAmp(conv, out output, FilterType.ToString(), MaskSize);
                    return output;
                }
            }
            
            HOperatorSet.SobelAmp(input, out output, FilterType.ToString(), MaskSize);          
            return output;
        }


        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)FilterType, MaskSize };
        }


        public override void FromCGPNodeParameters(float[] parameters)
        {
            FilterType = (SobelFilterType)parameters[0];
            MaskSize = (int)parameters[1];
        }

        public override int CGPInputCount
        {
            get
            {
                return 1;
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage | OperatorType.EdgeAmplitude;
            }
        }

        /// <summary>
        /// Authors: mara
        /// Function to convert FunctionCall to halcon code for export into a hdev file
        /// </summary>
        /// <returns></returns>
        public override List<string> HalconFunctionCall()
        {
            List<string> lines = new List<string>();

            string convOutput = "Conv_out";
            var convText = HObjectExtensions.ConvertStandardHalconText(Children.First().OutputVariableName, convOutput);
            lines.AddRange(convText);

            string imageTypeOutput = "Img_type";
            string typeText = HObjectExtensions.ImageTypeHalconText(Children.First().OutputVariableName, imageTypeOutput);

            lines.Add(typeText);

            lines.Add($"if (('{FilterType.ToString()}' == '{SobelFilterType.x_binomial.ToString()}')" +
                    $" or ('{FilterType.ToString()}' == '{SobelFilterType.y_binomial.ToString()}') )");

            lines.Add($"if (( ({imageTypeOutput} # 'byte') and ({imageTypeOutput} # 'uint2') and ({imageTypeOutput} # 'real') ))");

            string FilterTypeString = FilterType.ToString();
            if (FilterType == SobelFilterType.x_binomial)
            {
                FilterTypeString = "x";
            }
            else if (FilterType == SobelFilterType.y_binomial)
            {
                FilterTypeString = "y";
            }
            lines.Add(string.Format("sobel_amp ({0}, {1}, '{2}', {3})", convOutput, OutputVariableName, FilterTypeString, MaskSize));

            // lines.Add($"endif");

            lines.Add($"elseif (( ({imageTypeOutput} # 'byte') and ({imageTypeOutput} # 'uint2')" +
                    $" and ({imageTypeOutput} # 'real') and ({imageTypeOutput} # 'int2') ))");

            lines.Add(string.Format("sobel_amp ({0}, {1}, '{2}', {3})", convOutput, OutputVariableName, FilterTypeString, MaskSize));

            lines.Add($"else");

            lines.Add(string.Format("sobel_amp ({0}, {1}, '{2}', {3})", Children.First().OutputVariableName, OutputVariableName, FilterTypeString, MaskSize));

            lines.Add($"endif");

            string resizeConvOutput = "Conv_out";
            string resizeImageTypeOutput = "Img_type";
            List<string> resizeText = HObjectExtensions.OutputResizeimageHalconText(OutputVariableName, OutputVariableName, resizeConvOutput, resizeImageTypeOutput);
            lines.AddRange(resizeText);

            return lines;
        }
    }
}
