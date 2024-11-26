using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVBilateralFilter : CVNode
    {
        public CVBilateralFilter() : base()
        {

        }

        public CVBilateralFilter(List<CVNode> children, float[] parameter) : base(children, parameter)
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

                    Enumerable.Range(0, 50).Select(x => (float)x * 5).ToList(), // d in {0, 5 ..., 245} 

                    new List<float>
                    {
                        1, 2, 3, 10, 20, 50, 100
                    }, // sigmaColor
                    new List<float>
                    {
                        1, 2, 3, 10, 20, 50, 100
                    } // sigmaSpace
            };
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
            }
        }
        
        public System.Int32 d { get; set; } = 10;

        public System.Double sigmaColor { get; set; } = 3.0;

        public System.Double sigmaSpace { get; set; } = 3.0;

        public override UMat Execute(UMat input)
        {
            
            output = new UMat();
            CvInvoke.BilateralFilter(input, output, d, sigmaColor, sigmaSpace, Emgu.CV.CvEnum.BorderType.Default);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            d = (System.Int32)parameters[0];
            sigmaColor = parameters[1];
            sigmaSpace = parameters[2];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)d, (float)sigmaColor, (float)sigmaSpace};
        }
    }
}
