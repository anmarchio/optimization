using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using HalconDotNet;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class SobelAmpEquHistoImage : HalconOperatorNode
    {

        public SobelAmpEquHistoImage() : base()
        {
            Initialize(null);
        }

        public SobelAmpEquHistoImage(HObject inputImage, SobelAmp.SobelFilterType filterType, int maskSize) : base()
        {
            Initialize(inputImage);

            FilterType = filterType;
            MaskSize = maskSize;
        }

        public SobelAmpEquHistoImage(IList<HalconOperatorNode> ignoreThis, float[] parameters) : base()
        {
            FromCGPNodeParameters(parameters);
        }


        private void Initialize(HObject inputImage)
        {
        }

        public SobelAmp.SobelFilterType FilterType { get; set; }

        public int MaskSize { get; set; } = 3;
      
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
                return sobelAmpEquHistoImage;
            }
        }

        private static Dictionary<double, string> filterType = new Dictionary<double, string>()
        {
            {0, "y"},
            {1, "y_binomial"},
            {2, "x"},
            {3, "x_binomial"},       
        };

        private HObject[] sobelAmpEquHistoImage(HObject[] arg1, HTuple[] arg2)
        {
            HObject[] o = new HObject[arg1.Length];
            HOperatorSet.EquHistoImage(arg1[0], out o[0]);
            HOperatorSet.SobelAmp(o[0], out o[0], filterType[arg2[0]], (arg2[1]));
            return o;
        }

        public override void DisposeOutput()
        {
            if (output == null) return;
            output.Dispose();
            output = null;
        }

        public override HObject Execute(HObject input)
        {
            HObject tmp = null;
            try
            {
                HOperatorSet.EquHistoImage(input, out tmp);
                HOperatorSet.SobelAmp(tmp, out output, FilterType.ToString(), MaskSize);
                return output;
            }
            finally
            {
                if(!tmp.IsNullOrEmpty()) tmp.Dispose();
            }
        }

 
        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)FilterType, MaskSize };
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            FilterType = (SobelAmp.SobelFilterType)parameters[0];
            MaskSize = (int)parameters[1];
            Initialize(null);
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
                /*
                 * Remove this node from collection
                 */
                //return OperatorType.ImageToImage;
                return OperatorType.Bug;
            }
        }

        /// <summary>
        /// Author: braml
        /// Function to convert FunctionCall to halcon code for export into a hdev file
        /// </summary>
        /// <returns> List of strings that represent code to be executed as .hdev file</returns>
        public override List<string> HalconFunctionCall()
        {
            List<string> lines = new List<string>();

            lines.Add($"equ_histo_image ({Children.First().OutputVariableName}, {OutputVariableName})");
            lines.Add($"sobel_amp ({OutputVariableName}, {OutputVariableName}, '{FilterType.ToString()}', {MaskSize.ToInvariantString()})");

            return lines;
        }
    }
}
