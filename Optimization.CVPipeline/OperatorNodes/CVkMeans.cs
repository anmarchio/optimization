using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVkMeans : CVNode
    {
        public CVkMeans() : base()
        {

        }

        public CVkMeans(List<CVNode> children, float[] parameters) : base(children, parameters)
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

        //Number of clusters to split the set by
        public List<float>[] GetCGPParameterBounds()
        {
            return new List<float>[]
            {
                     new List<float>
                    {
                        1, 2, 3, 4, 5, 6, 7
                    },
                    //k
                     new List<float>
                    {
                        100, 15, 200, 250, 300, 500, 1000
                    }
                    //termcrit
            };
        }

        /*For termcrit variable
        Specifies maximum number of iterations and/or accuracy
        public override int[] CGPParametersBounds
        {
            get
            {
                return new int[]
                {
                    100, 200, 300, 500, 1000
                };
            }
        }
        */

        public int k
        {
            get;
            set;
        } = 2;

        public Emgu.CV.Structure.MCvTermCriteria termcrit
        {
            get;
            set;
        } = new Emgu.CV.Structure.MCvTermCriteria(100);

        public override UMat Execute(UMat input)
        {
            output = new UMat();
            var input2 = new UMat();
            input.ConvertTo(input2, Emgu.CV.CvEnum.DepthType.Cv32F);

            CvInvoke.Kmeans(input2, k, output, termcrit, 2, 0);
            return output;
            
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            k = (int)parameters[0];
            termcrit = new Emgu.CV.Structure.MCvTermCriteria((int)parameters[1]);
        } 
                
        public override float[] ToCGPNodeParameters()
        {
            return  new float[] { k, (float) termcrit.MaxIter };
        }
    }
}
