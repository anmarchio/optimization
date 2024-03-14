using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVAbsDiff : CVNode
    {
        
        public CVAbsDiff() : base()
        {

        }

        public CVAbsDiff(List<CVNode> children, float[] parameters) : base(children, parameters)
        {

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

        public override int CGPInputCount
        {
            get
            {
                return 2;
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToRegion;
            }
        }

        public override UMat Execute(UMat input)
        {
            if (Children.Count == 2)
            {
                output = new UMat();
                CvInvoke.AbsDiff((Children[0] as CVNode).Output, (Children[1] as CVNode).Output, output);
                // AbsDiff von input und erstem unterschiedlichem Kind nach output
                CvInvoke.Normalize(output, output, 0, 255);
            }
            //AbsDiff von allen nicht wirklich sinnvoll
            /* will not be reached as cgpinputcount == 2 ;>
            else
            {
                output = (Children.First() as CVNode).Output; // entspricht dem input
                for (int i = 1; i < Children.Count; i++)
                {
                    CvInvoke.AbsDiff((Children[i] as CVNode).Output, output, output);
                    CvInvoke.Normalize(output, output, 0, 255);
                }
            }*/ 
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
