using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVConnectedComponentsWithStats : CVNode
    {
        public CVConnectedComponentsWithStats() : base()
        {

        }

        public CVConnectedComponentsWithStats(List<CVNode> children, float[] parameters) : base(children, parameters)
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
                return OperatorType.RegionToRegion;
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
            return new List<float>[0];
        }

        public override UMat Execute(UMat input)
        {
            UMat input2 = new UMat();
            input.ConvertTo(input2, Emgu.CV.CvEnum.DepthType.Cv8S);

            var labels = new UMat();
            var stats = new UMat();
            var centroids = new UMat();

            CvInvoke.ConnectedComponentsWithStats(input2, labels, stats, centroids);
            return stats;
            //return new List<UMat>() { labels, stats, centroids };
            
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            return;
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[0];
        }
    }
}
