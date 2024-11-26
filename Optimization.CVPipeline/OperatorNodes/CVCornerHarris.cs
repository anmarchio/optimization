using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVCornerHarris : CVNode
    {
        public CVCornerHarris() : base()
        {

        }

        public CVCornerHarris(List<CVNode> children, float[] parameter) : base(children, parameter)
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
                        2,3,5,7,9
                    }//possible width and height
            };
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
            }
        }

        public System.Drawing.Size block_size
        {
            get;
            set;
        } = new System.Drawing.Size(2, 2);

        public override UMat Execute(UMat input)
        {
            
            output = new UMat();
            CvInvoke.CornerHarris(input, output, (int) block_size.Height);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            block_size = new System.Drawing.Size((int)parameters[0], (int) parameters[0]);
            //throw new NotImplementedException();
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { block_size.Width, block_size.Height };
        }


    }
}
