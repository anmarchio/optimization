using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVSqrt : CVNode
    {
        public CVSqrt() : base()
        {

        }

        public CVSqrt(params CVNode[] children) : base(children)
        {

        }

        public CVSqrt(List<CVNode> children, float[] parameters) : base(children, parameters)
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
            return new List<float>[0];
        } //No parameters to alter


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
            CvInvoke.Sqrt(input, output);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            return;
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { };
        }
    }
}
