using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVLog : CVNode
    {
        public CVLog() : base()
        {

        }

        public CVLog(params CVNode[] children) : base(children)
        {

        }

        public CVLog(List<CVNode> children, float[] parameters) : base(children, parameters)
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
            output = new UMat();
            CvInvoke.Log(input, output);
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
