using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PRIME.Optimization.Pipeline;
using Emgu.CV;

namespace PRIME.CVPipeline.CVOperatorNodes
{
    [Serializable]
    public class CVEigen : CVNode
    {
        public CVEigen() : base()
        {

        }

        public CVEigen(List<CVNode> children, float[] parameters) : base(children, parameters)
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
                return OperatorType.ImageToRegion | OperatorType.InputNode;
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
            //input needs Resizing to be symmetric
            UMat input2 = new UMat();
            

            output = new UMat();
            CvInvoke.Eigen(input2, output);
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
