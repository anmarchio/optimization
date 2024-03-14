using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVResize : CVNode
    {
        public CVResize() : base()
        {

        }

        public CVResize(List<CVNode> children, float[] parameters) : base(children, parameters)
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
                        (float) 0.25, (float) 0.5, (float) 0.75,  2, 4, 8
                    }, // faktor
            };
        }

        public System.Drawing.SizeF dSize
        {
            get;
            set;
        } = new System.Drawing.SizeF(2 , 2);

        public float Factor { get; set; }

        public override UMat Execute(UMat input)
        {
            output = new UMat();

            //System.Drawing.Size dSize = new System.Drawing.Size();
            //FromCGPNodeParameters(input.Width, input.Height, parameters);
            dSize = new System.Drawing.SizeF(Factor * input.Rows, Factor * input.Cols);
            CvInvoke.Resize(input, output, dSize.ToSize());
            return output;
        }

        /*
         * public override void FromCGPNodeParameters(int width, int height, float[] parameters)
        {
            System.Drawing.Size dSize = new System.Drawing.Size(width * (int) parameters[0], height * (int) parameters[0]);
        }
        */

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Factor = parameters[0];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { dSize.Width, dSize.Height };
        }
    }
}
