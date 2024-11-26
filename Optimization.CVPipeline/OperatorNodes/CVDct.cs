using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVDct : CVNode
    {
        public CVDct() : base()
        {

        }

        public CVDct(List<CVNode> children, float[] parameters) : base(children, parameters)
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
            return new List<float>[]
            {
                new List<float>
                {
                    0, 1, 4 
                }
            };
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

        public int flag
        {
            get;
            set;
        } = 0;

        public override UMat Execute(UMat input)
        {
            output = new UMat();
            if (flag == 0)
                CvInvoke.Dct(input, output, Emgu.CV.CvEnum.DctType.Forward);
            else if (flag > 0 && flag == 4)
                CvInvoke.Dct(input, output, Emgu.CV.CvEnum.DctType.Rows);
            else
                CvInvoke.Dct(input, output, Emgu.CV.CvEnum.DctType.Inverse);

            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            flag = (int) parameters[0];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float) flag } ;
        }
    }
}
