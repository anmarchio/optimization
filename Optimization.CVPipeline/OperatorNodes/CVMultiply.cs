using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVMultiply : CVNode
    {
        public CVMultiply() : base()
        {

        }

        public CVMultiply(List<CVNode> children, float[] parameters) : base(children, parameters)
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
            if (Children.Count == 2)
            {
                output = new UMat();
                CvInvoke.Multiply((Children[0] as CVNode).Output, (Children[1] as CVNode).Output, output);
                // Divide von input und erstem unterschiedlichem Kind nach output
                CvInvoke.Normalize(output, output, 0, 255);
            }
            else
            {
                output = (Children.First() as CVNode).Output; // entspricht dem input
                for (int i = 1; i < Children.Count; i++)
                {
                    CvInvoke.Multiply((Children[i] as CVNode).Output, output, output);
                    CvInvoke.Normalize(output, output, 0, 255);
                }
            }
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
