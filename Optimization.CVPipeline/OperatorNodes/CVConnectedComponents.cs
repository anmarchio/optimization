using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVConnectedComponents : CVNode
    {
        public CVConnectedComponents() : base()
        {

        }

        public CVConnectedComponents(List<CVNode> children, float[] parameters) : base(children, parameters)
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
                return OperatorType.RegionToRegion; //removed ImageToRegion because usage only after moprhing operations
            }
        }

        public override List<float>[] CGPParameterBounds
        {
            get
            {
                return GetCGPParameterBounds();
            }
        }

        public List<float> [] GetCGPParameterBounds()
        {
            return new List<float>[0];
        }

        public override UMat Execute(UMat input)
        {
            UMat input2 = new UMat();
            input.ConvertTo(input2, Emgu.CV.CvEnum.DepthType.Cv8S);

            output = new UMat();
            CvInvoke.ConnectedComponents(input2, output);
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
