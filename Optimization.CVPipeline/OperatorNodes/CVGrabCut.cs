using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVGrabCut : CVNode
    {
        public CVGrabCut() : base()
        {

        }

        public CVGrabCut(List<CVNode> children, float[] parameters) : base(children, parameters)
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
                return OperatorType.ImageToRegion;
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
                    0 // x-Pixel
                },
                new List<float>
                {
                    0 // x-Pixel
                },
                new List<float>
                {
                    255
                },
                new List<float>
                {
                    255
                }

            };
        }

        public System.Drawing.Rectangle rect
        {
            get;
            set;
        } = new System.Drawing.Rectangle(0, 0, 255, 255);

        //Doenst crop anything, because rectangle is img. Therefore no distinction between foreground and background
        public override UMat Execute(UMat input)
        {
            output = new UMat();
            //Input needs to be a color image
            
            UMat bgdmodel = new UMat();
            bgdmodel.GetInputOutputArray();

            UMat fgdmodel = new UMat();
            fgdmodel.GetInputOutputArray();                       
                        
            //mask is also a user interaction required parameter, by marking the foreground and background pixels mask with vallues 0 and 1 is created
            //usually a rectangle gets drawn by the user, which in this case replaces the mask
            //Make Use of RegionMarker
            CvInvoke.GrabCut(input, output, rect, bgdmodel, fgdmodel, 3, (Emgu.CV.CvEnum.GrabcutInitType) 0);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle((int) parameters[0], (int) parameters[1], (int) parameters[2], (int) parameters[3]);
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { rect.X, rect.Y, rect.Width, rect.Height};
        }
    }
}
