using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVSubtractConst : CVNode
    {
        public CVSubtractConst() : base()
        {

        }

        public CVSubtractConst(List<CVNode> children, float[] parameters) : base(children, parameters)
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
            var l = new List<float>[1]
            {
                    Enumerable.Range(0, 50).Select(x => (float)x * 5).ToList(), // Constant in {0, 5 ..., 245} 
            };
            return l;
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
            }
        }

        public int Constant { get; set; } = 5;

        public override UMat Execute(UMat input)
        {
            MCvScalar cv_value = new MCvScalar(Constant);
            output = input.Clone();
            output.SetTo(cv_value);

            CvInvoke.Subtract(input, output, output);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Constant = (int)parameters[0];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Constant };
        }
    }
}
