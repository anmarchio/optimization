using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVFilter2D : CVNode
    {
        public CVFilter2D() : base()
        {

        }

        public CVFilter2D(List<CVNode> children, float[] parameters) : base(children, parameters)
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

        public Matrix<float> kernel
        {
            get;
            set;
        } = new Matrix<float>(
            new float[,] {
                {1.0f, 2.0f, 1.0f},
                {2.0f, 4.0f, 2.0f},
                {1.0f, 2.0f, 1.0f}
            }
        );

        public System.Drawing.Point Anchor
        {
            get;
            set;
        } = new System.Drawing.Point(-1, -1);

        public override UMat Execute(UMat input)
        {
            output = new UMat();
            CvInvoke.Filter2D(input, output, kernel, Anchor);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            kernel = new Matrix<float>(
            new float[,] {
                {parameters[0], 2*parameters[0], parameters[0]},
                {2*parameters[0], 4*parameters[0], 2*parameters[0]},
                {parameters[0], 2*parameters[0], parameters[0]}
                }   
            );
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { kernel.Width, kernel.Height };
        }
    }
}
