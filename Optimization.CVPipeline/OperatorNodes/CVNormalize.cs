using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVNormalize : CVNode
    {
        public CVNormalize() : base()
        {

        }

        public CVNormalize(List<CVNode> children, float[] parameters) : base(children, parameters)
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
                    10, 25, 50, 100, 127, 192, 255
                },//alpha
                new List<float>
                {
                    10, 25, 50, 100, 127, 192, 255
                }//beta
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

        public double alpha
        {
            get;
            set;
        } = 255.0;

        public double beta
        {
            get;
            set;
        } = 255;

        public override UMat Execute(UMat input)
        {
            output = new UMat();
            CvInvoke.Normalize(input, output, alpha, beta);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            alpha = ( double) parameters[0];
            beta =  ( double) parameters[1];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)alpha, (float)beta };
        }
    }
}
