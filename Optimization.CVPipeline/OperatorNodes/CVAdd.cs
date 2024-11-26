using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVAdd : CVNode
    {
        public CVAdd() : base()
        {

        }

        public CVAdd(List<CVNode> children, float[] parameters) : base(children, parameters)
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

        //Just adds itself...
        public override UMat Execute(UMat input)
        {
            /*
            //UMat input2 = input.Clone();

            CVNode help = Children[1] as CVNode;

            //CVNone help = new CVNone(new List<CVNode> { Children.First() as CVNode }, new float[0]);
            //das macht alles nichts input = input2

            UMat input2 = help.Output; //Hier ist noch möglich Groesseanzupassen

            //input = Children[0];
            CvInvoke.Add(input, input2, output);
            */
            //if( output == null)

                if (Children.Count == 2)
                {
                    output = new UMat();
                    CvInvoke.Add((Children[0] as CVNode).Output, (Children[1] as CVNode).Output, output);
                    // Add von input und erstem unterschiedlichem Kind nach output
                    CvInvoke.Normalize(output, output, 0, 255);
                }
                else{
                    output = (Children.First() as CVNode).Output; // entspricht dem input
                    for (int i = 1; i < Children.Count; i++)
                    {
                        CvInvoke.Add((Children[i] as CVNode).Output, output, output);
                        CvInvoke.Normalize(output, output, 0, 255);
                    }                    
                }

            //CvInvoke.Add(input, output, output);
            //CvInvoke.Normalize(output, output, 0, 255);

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
