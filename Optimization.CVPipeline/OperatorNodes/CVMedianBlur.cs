using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVMedianBlur : CVNode
    {
        public CVMedianBlur() : base()
        {

        }

        public CVMedianBlur(List<CVNode> children, float[] parameters) : base(children, parameters)
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
                    new List<float>
                    {
                        3, 5, 7, 9
                    } //ksize
            };
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
            }
        }

        public System.Int32 ksize { get; set; } = 3;
        public override UMat Execute(UMat input)
        {
            
            output = new UMat();
            CvInvoke.MedianBlur(input, output, ksize);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            ksize = (int)parameters[0];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)ksize};
        }
    }
}
