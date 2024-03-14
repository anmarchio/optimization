using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVInvert : CVNode
    {
        public CVInvert() : base()
        {

        }

        public CVInvert(params CVNode[] children) : base(children)
        {

        }

        public CVInvert(List<CVNode> children, float[] parameters) : base(children, parameters)
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
            return new List<float>[1] {
                    new List<float>
                    {
                        0, 1, 2, 3, 4, 16
                    } // DecompMethodEnum
                };
        } //No parameters to alter


        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
            }
        }

        public Emgu.CV.CvEnum.DecompMethod DecompMethodEnum { get; set; } = Emgu.CV.CvEnum.DecompMethod.Normal;

        public override UMat Execute(UMat input)
        {
            output = new UMat();
            CvInvoke.Invert(input, output, DecompMethodEnum);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            DecompMethodEnum = (Emgu.CV.CvEnum.DecompMethod)parameters[0];
            return;
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] {(float)DecompMethodEnum};
        }
    }
}
