using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVHoughCircles : CVNode
    {
        public CVHoughCircles() : base()
        {

        }

        public CVHoughCircles(List<CVNode> children, float[] parameters) : base(children, parameters)
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

        public HoughType CV_HOUGH_GRADIENT { get; set; } = (Emgu.CV.CvEnum.HoughType) 3;

        public double mindist
        {
            get;
            set;
        } = 50.0;

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
                    10, 20, 25, 50
                }
            };
        }

        public override UMat Execute(UMat input)
        {
            output = new UMat();
            
            //50pixels distance between the circles centers
            CvInvoke.HoughCircles(input, output, (Emgu.CV.CvEnum.HoughType) 3, 1, mindist);

            //Drawing the circles not yet implemented
            
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            mindist = parameters[0];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float) mindist };
        }
    }
}
