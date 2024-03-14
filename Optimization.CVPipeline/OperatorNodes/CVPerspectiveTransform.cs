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
    class CVPerspectiveTransform : CVNode
    {
        public CVPerspectiveTransform()
        {

        }

        public CVPerspectiveTransform(List<CVNode> children, float[] parameters) : base(children, parameters)
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
                return OperatorType.ImageToImage | OperatorType.InputNode;
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
                        3, 4
                    }, // width
                    new List<float>
                    {
                        3, 4
                    } // height
            };
        }

        //UMat ist doch ne Matrix
        public UMat mat
        {
            get;
            set;
        } = new UMat();

        public override UMat Execute(UMat input)
        {
            
            output = new UMat();
            CvInvoke.PerspectiveTransform(input, output, mat);
            return output;
        }

        /*public override void FromCGPNodeParameters(float[] parameters)
        {
            mat = new System.Drawing.Size((int)parameters[0], (int)parameters[1]);
        }*/

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { };
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            return;
        }
    }
}
