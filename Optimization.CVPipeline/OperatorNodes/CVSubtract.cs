using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVSubtract : CVNode
    {
        public CVSubtract() : base()
        {

        }

        public CVSubtract(List<CVNode> children, float[] parameters) : base(children, parameters)
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

            if (Children.Count == 2)
            {
                output = new UMat();
                CvInvoke.Subtract((Children[0] as CVNode).Output, (Children[1] as CVNode).Output, output);
                // Subtract von input und erstem unterschiedlichem Kind nach output
                CvInvoke.Normalize(output, output, 0, 255);
            }
            else
            {
                output = (Children.First() as CVNode).Output; // entspricht dem input
                for (int i = 1; i < Children.Count; i++)
                {
                    CvInvoke.Subtract((Children[i] as CVNode).Output, output, output);
                    CvInvoke.Normalize(output, output, 0, 255);
                }
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
