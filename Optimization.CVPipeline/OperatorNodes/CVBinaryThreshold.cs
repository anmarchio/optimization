using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVBinaryThreshold : CVNode
    {
        public CVBinaryThreshold() : base()
        {

        }
        public CVBinaryThreshold(float threshold, params CVNode[] children) : base(children)
        {
            Threshold = threshold;
        }

 
     
        public override UMat Execute(UMat input)
        {
            //if (output == null)
                output = new UMat();
            CvInvoke.Threshold(input, output, Threshold, 1, Emgu.CV.CvEnum.ThresholdType.Binary);
            return output;
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Threshold };
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Threshold = parameters[0];
        }

        public float Threshold { get; set; } = 10;

        public override List<float>[] CGPParameterBounds
        {
            get
            {
                return GetCGPParameterBounds();
            }
        }

        public List<float>[] GetCGPParameterBounds()
        {
            var l = new List<float>[1]
            {
                    Enumerable.Range(0, 50).Select(x => (float)x * 5).ToList(), // threshold in {0, 5 ..., 245} 
            };
            return l;
        }


        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToRegion;
            }
        }

        public override int CGPInputCount
        {
            get
            {
                return 1;
            }
        }
    }
}
