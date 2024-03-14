using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVPyrMeanShiftFiltering : CVNode
    {
        public CVPyrMeanShiftFiltering() : base()
        {

        }

        public CVPyrMeanShiftFiltering(List<CVNode> children, float[] parameters) : base(children, parameters)
        {

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
                    3, 5, 8, 9, 16
                }, //Spatial window radius sp
                new List<float>
                {
                    3, 5, 8, 9, 16
                }//color window radius sr
            };
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

        public double sp
        {
            get;
            set;
        } = 3.0;

        public double sr
        {
            get;
            set;
        } = 3.0;

        public override UMat Execute(UMat input)
        {
            output = new UMat();

            CvInvoke.PyrMeanShiftFiltering(input, output, sp, sr, 1, new Emgu.CV.Structure.MCvTermCriteria(5, 1));

            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            sp = (double) parameters[0];
            sr = (double) parameters[1];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float) sp, (float) sr };
        }
    }
}
