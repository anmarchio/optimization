using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVLaplacian : CVNode
    {
        public CVLaplacian() : base()
        {

        }

        public CVLaplacian(List<CVNode> children, float[] parameters) : base(children, parameters)
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
                        1, 3, 5, 7
                    }, // KernelSize
                    new List<float>
                    {
                        1, 2, 3
                    }, // Scale
                    new List<float>
                    {
                        0, 1, 2, 3
                    } //Delta
            };
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
            }
        }

        public System.Int32 KernelSize { get; set; } = 1;
        public System.Double Scale { get; set; } = 1;
        public System.Double Delta { get; set; } = 0;

        public override UMat Execute(UMat input)
        {
           
            output = new UMat();
            CvInvoke.Laplacian(input, output, input.GetInputArray().GetDepth(), KernelSize, Scale, Delta, Emgu.CV.CvEnum.BorderType.Default);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            KernelSize = (System.Int32)parameters[0];
            Scale = (System.Double)parameters[1];
            Delta = (System.Double)parameters[2];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { KernelSize, (float)Scale, (float)Delta };
        }
    }
}
