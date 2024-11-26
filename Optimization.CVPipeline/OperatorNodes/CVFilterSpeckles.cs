using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVFilterSpeckles : CVNode
    {
        public CVFilterSpeckles() : base()
        {

        }

        public CVFilterSpeckles(List<CVNode> children, float[] parameters) : base(children, parameters)
        {

        }

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
        public List<float>[] GetCGPParameterBounds()
        {
            return new List<float>[]
            {
                    new List<float>()
                    {
                        5, 10, 15, 20, 25, 45, 55, 70, 80
                    },
                    new List<float>()
                    {
                        5, 10, 20, 30, 40
                    },
                    new List<float>()
                    {
                        20, 30, 40, 50, 60, 70
                    }
            };
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
            }
        }

        public float NewValue { get; set; }
        public int MaxSpeckleSize { get; set; }
        public float MaxDiff { get; set; }

        public override UMat Execute(UMat input)
        {
            CvInvoke.FilterSpeckles(input, NewValue, MaxSpeckleSize, MaxDiff);
            return input;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            NewValue = parameters[0];
            MaxSpeckleSize = (int)parameters[1];
            MaxDiff = parameters[2];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { NewValue, MaxSpeckleSize, MaxDiff };
        }
    }
}
