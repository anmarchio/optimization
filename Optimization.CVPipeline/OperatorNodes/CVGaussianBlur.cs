using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes

{
    [Serializable]
    public class CVGaussianBlur : CVNode
    {
        public CVGaussianBlur() : base()
        {

        }

        public CVGaussianBlur(List<CVNode> children, float[] parameter) : base(children, parameter)
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
                        3, 5, 7
                    }, // width
                    new List<float>
                    {
                        3, 5, 7
                    }, // height
                    new List<float>
                    {
                        0, 1, 2
                    }, //SigmaX
                    new List<float>
                    {
                        0, 1, 2
                    } //SigmaY
            };
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
            }
        }

        public System.Drawing.Size KernelSize { get; set; } = new System.Drawing.Size(3, 3);
        public System.Double SigmaX {get; set;} = 0.0;
        public System.Double SigmaY {get; set;} = 0.0;

        public override UMat Execute(UMat input)
        {
            
            output = new UMat();
            CvInvoke.GaussianBlur(input, output, KernelSize, SigmaX, SigmaY, Emgu.CV.CvEnum.BorderType.Default);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            KernelSize = new System.Drawing.Size((int)parameters[0], (int)parameters[1]);
            SigmaX = (System.Double)parameters[2];
            SigmaY = (System.Double)parameters[3];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { KernelSize.Width, KernelSize.Height, (float)SigmaX, (float)SigmaY };
        }
    }
}
