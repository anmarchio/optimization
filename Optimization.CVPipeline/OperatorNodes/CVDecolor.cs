using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVDecolor : CVNode
    {
        public CVDecolor() : base()
        {

        }

        public CVDecolor(List<CVNode> children, float[] parameters) : base(children, parameters)
        {

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
        }

        public override UMat Execute(UMat input)
        {
            if (input.NumberOfChannels < 3) output = input.Clone();
            else
            {
                output = new UMat();
                UMat gray_colorboost = new UMat();
                //gray_colorboost.SetTo(new int[] { 0 });
                CvInvoke.Decolor(input, output, gray_colorboost);
            }

            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            return;
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[0];
        }
    }
}
