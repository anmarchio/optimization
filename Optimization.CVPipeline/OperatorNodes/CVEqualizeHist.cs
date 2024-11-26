using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVEqualizeHist : CVNode
    {
        public CVEqualizeHist() : base()
        {

        }

        public CVEqualizeHist(List<CVNode> children, float[] parameter) : base(children, parameter)
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
            return new List<float>[0]
            {

            };
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
            }
        }

        
        public override UMat Execute(UMat input)
        {
            
            output = new UMat();
            CvInvoke.EqualizeHist(input, output);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            return;
            // do nothing : EqualizeHist doesn't need any parameters
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[0]; // since EqualizeHist doesn't return any parameters
        }
    }
}
