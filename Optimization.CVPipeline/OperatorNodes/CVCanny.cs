using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVCanny : CVNode
    {
        public CVCanny() : base()
        {

        }

        public CVCanny(List<CVNode> children, float[] parameter) : base(children, parameter)
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
                    // Threshold1 in {0, 5 ..., 245} 
                    Enumerable.Range(0, 50).Select(x => (float)x * 5).ToList(),
                      
                    // Threshold2 in {0, 5 ..., 245} 
                    Enumerable.Range(0, 50).Select(x => (float)x * 5).ToList(),

                    //apertureSize
                    new List<float>
                    {
                        3, 5, 7
                    }
            };
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToRegion;
            }
        }

        public System.Double threshold1 { get; set; } = 10.0;
        public System.Double threshold2 { get; set; } = 10.0;

        public int apertureSize { get; set; } = 3; 

        public override UMat Execute(UMat input)
        {
            
            output = new UMat();
            CvInvoke.Canny(input, output, threshold1, threshold2, apertureSize);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            threshold1 = (System.Double)parameters[0];
            threshold2 = (System.Double)parameters[1];
            apertureSize = (int)parameters[2];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)threshold1, (float)threshold2, (float)apertureSize};
        }
    }
}
