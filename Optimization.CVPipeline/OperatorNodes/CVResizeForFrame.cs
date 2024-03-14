using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVResizeForFrame : CVNode
    {
        public CVResizeForFrame() : base()
        {

        }

        public CVResizeForFrame(List<CVNode> children, float[] parameters) : base(children, parameters)
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
                        (float) 0.1, (float) 0.2, (float) 0.25, (float) 0.33, (float) 0.5, (float) 0.75
                    }, // faktor
            };
        }

        public System.Drawing.SizeF FrameSize/*(int width, int height, int faktor)*/
        {
            get;
            set;
            //return System.Drawing.Size(width * faktor, height * faktor);
        } = new System.Drawing.SizeF(2, 2);

        public float Factor { get; set; } = 1;
                
        public override UMat Execute(UMat input)
        {
            output = new UMat();          
            FrameSize = new System.Drawing.SizeF(Factor * input.Rows, Factor * output.Cols);
            CvInvoke.ResizeForFrame(input, output, FrameSize.ToSize(), Emgu.CV.CvEnum.Inter.Linear, false);
            return output;
        }

        /*public override void FromCGPNodeParameters(int width, int height, float[] parameters)
        {
            FrameSize = new System.Drawing.Size(width * (int)parameters[0], height * (int)parameters[0]);
        }*/

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Factor = parameters[0];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { FrameSize.Width, FrameSize.Height };
        }
    }
}