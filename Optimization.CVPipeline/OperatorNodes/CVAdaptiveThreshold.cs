using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVAdaptiveThreshold : CVNode
    {
        public CVAdaptiveThreshold() : base()
        {

        }

        public CVAdaptiveThreshold(List<CVNode> children, float[] parameters) : base(children, parameters)
        {

        }

        public AdaptiveThresholdType AdaptiveType { get; set; } = AdaptiveThresholdType.GaussianC;
        public int BlockSize { get; set; } = 3;

        public override int CGPInputCount
        {
            get
            {
                return 1;
            }
        }

        public override List<float>[] CGPParameterBounds
        {
            get
            {
                return GetCGPParameterBounds();
            }
        }

        public  List<float>[] GetCGPParameterBounds()
        {
            return new List<float>[2]
            {
                    new List<float>()
                    {
                        (float) AdaptiveThresholdType.GaussianC, (float) AdaptiveThresholdType.MeanC
                    },
                    new List<float>()
                    {
                        3, 5, 7
                    }
            };
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToRegion;
            }
        }


        public override UMat Execute(UMat input)
        {
            output = new UMat();
            input.ConvertTo(output, DepthType.Cv8U);
                            
            CvInvoke.AdaptiveThreshold(output, output, 1, AdaptiveType, ThresholdType.Binary, BlockSize, 0);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            AdaptiveType = (AdaptiveThresholdType)parameters[0];
            BlockSize = (int)parameters[1];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)AdaptiveType, BlockSize };
        }
    }
}
