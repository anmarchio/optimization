using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVForwardFourier : CVNode
    {
        public CVForwardFourier() : base()
        {

        }

        public CVForwardFourier(params CVNode[] children) : base(children)
        {

        }

        public CVForwardFourier(List<CVNode> children, float[] parameters) : base(children, parameters)
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
            return new List<float>[0];
        } //No parameters to alter


        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
            }
        }



        public override UMat Execute(UMat input)
        {
            /*UMat input2 = new UMat();
                input.ConvertTo(input2, Emgu.CV.CvEnum.DepthType.);*/
                
            output = new UMat();
            //output.ConvertTo(output, (Emgu.CV.CvEnum.DepthType) input.Depth);
            
            CvInvoke.Dft(input, output, Emgu.CV.CvEnum.DxtType.Forward, 0);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            return;
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { };
        }
    }
}
