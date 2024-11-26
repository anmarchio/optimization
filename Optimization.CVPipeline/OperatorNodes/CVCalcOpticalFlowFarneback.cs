using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVCalcOpticalFlowFarneback : CVNode
    {
        public CVCalcOpticalFlowFarneback() : base()
        {

        }

        public CVCalcOpticalFlowFarneback(List<CVNode> children, float[] parameters) : base(children, parameters)
        {

        }

        public override List<float>[] CGPParameterBounds
        {
            get
            {
                return GetCGPParametersBounds();
            }
        }

        public List<float>[] GetCGPParametersBounds()
        {
            return new List<float>[] 
            {
                new List<float>
                {
                    1, 10, 25, 50 , 100, 127, 191, 255 
                },//windowsize
                new List<float>
                {
                    1, 10, 100, 1000
                }//iterations
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

        public int windowsize
        {
            get;
            set;
        }

        public int iterations
        {
            get;
            set;
        }


        public override UMat Execute(UMat input)
        {
            UMat input2 = new UMat();
            input2 = input.Clone();
            output = new UMat();
            
            CvInvoke.CalcOpticalFlowFarneback(input, input2, output, 0.5, 1, windowsize, iterations, 5, 1.1, 0);

            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            windowsize = (int) parameters[0];
            iterations = (int)parameters[1];
        }

        public override float[] ToCGPNodeParameters()
        {
            throw new NotImplementedException();
        }
    }
}
