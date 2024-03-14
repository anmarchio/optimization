using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVBoxFilter : CVNode
    {
        public CVBoxFilter() : base()
        {

        }

        public CVBoxFilter(List<CVNode> children, float[] parameter) : base (children, parameter)
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
                        3, 5, 7
                    }, // width
                    new List<float>
                    {
                        3, 5, 7
                    } // height
            };
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
            }
        }

        public System.Drawing.Size KernelSize {
            get;
            set;
        } = new System.Drawing.Size(3,3);

        public System.Drawing.Point Anchor
        {
            get;
            set;
        } = new System.Drawing.Point(-1,-1);

        //Normalize alters alpha, by which the values are multiplicated
        //otherwise works the same as blur or rather blur works the same if normalize is true
        public override UMat Execute(UMat input)
        {
            
            output = new UMat();
            CvInvoke.BoxFilter(input, output, input.Depth, KernelSize, Anchor, true, Emgu.CV.CvEnum.BorderType.Default);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            KernelSize = new System.Drawing.Size((int)parameters[0], (int)parameters[1]);
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { KernelSize.Width, KernelSize.Height };
        }
    }
}
