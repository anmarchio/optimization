using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVMorphologyClose : CVNode
    {
        public CVMorphologyClose() : base()
        {

        }

        public CVMorphologyClose(List<CVNode> children, float[] parameters) : base(children, parameters)
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
                    new List<float>()
                    {
                        1, 3, 5, 8, 10, 15
                    }, // iterations
                    //Enum.GetValues(typeof(Emgu.CV.CvEnum.ElementShape)).Cast<Emgu.CV.CvEnum.ElementShape>().ToList().Select(x => (float)x).ToList(), // elementtype
                    new List<float>()
                    {
                        3, 5, 9, 13, 20, 30, 40
                    }, // height                   
                    new List<float>()
                    {
                        3, 5, 9, 13, 20, 30, 40
                    } // width
                    
            };
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.RegionToRegion;
            }
        }

        private System.Drawing.Point Anchor { get; set; } = new System.Drawing.Point(-1, -1);

        public int Iterations { get; set; } = 3;
        public Emgu.CV.CvEnum.ElementShape StructElementType { get; set; } = Emgu.CV.CvEnum.ElementShape.Rectangle;

        public int ElementWidth { get; set; }
        public int ElementHeight { get; set; }


        private IInputArray StructElement
        {
            get
            {
                return CvInvoke.GetStructuringElement(StructElementType, new System.Drawing.Size(ElementWidth, ElementHeight), Anchor);
            }
        }


        public override UMat Execute(UMat input)
        {
            output = new UMat();
            CvInvoke.MorphologyEx(input, output, Emgu.CV.CvEnum.MorphOp.Close, StructElement, Anchor, Iterations, Emgu.CV.CvEnum.BorderType.Default, new Emgu.CV.Structure.MCvScalar(0));
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Iterations = (int)parameters[0];
            ElementWidth = (int)parameters[1];
            ElementHeight = (int)parameters[2];           
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Iterations, ElementWidth, ElementHeight };
        }
    }
}
