using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVMultiplyConst : CVNode
    {
        public CVMultiplyConst() : base()
        {

        }

        public CVMultiplyConst(List<CVNode> children, float[] parameters) : base(children, parameters)
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
                         2, 3, 4, 5
                    } //Constant
            };
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
            }
        }

        public int Constant { get; set; } = 2;

        public override UMat Execute(UMat input)
        {
            MCvScalar cv_value = new MCvScalar(Constant);
            output = input.Clone();
            output.SetTo(cv_value);

            CvInvoke.Multiply(input, output, output);
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
