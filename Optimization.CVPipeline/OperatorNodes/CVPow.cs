using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVPow : CVNode
    {
        public CVPow() : base()
        {

        }

        public CVPow(List<CVNode> children, float[] parameters) : base(children, parameters)
        {

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
                    1,2, 4, 8, 10,  16, 32
                }
            };
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
                return OperatorType.ImageToImage;
            }
        }

        public double power
        {
            get;
            set;
        } = 2.0;

        public override UMat Execute(UMat input)
        {
            output = new UMat();
            CvInvoke.Pow(input, power, output);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            power = (double) parameters[0];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)power };
        }
    }
}
