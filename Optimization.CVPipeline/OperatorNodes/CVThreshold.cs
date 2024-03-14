using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVThreshold : CVNode
    {
        public CVThreshold() : base()
        {

        }
        public CVThreshold(float threshold, float maxValue, params CVNode[] children) : base(children)
        {
            MaxValue = maxValue;
            Threshold = threshold;
        }

        public CVThreshold(List<CVNode> children, float[] parameters) : base(children, parameters)
        {

        }


        public override UMat Execute(UMat input)
        {
            output = new UMat();
            if (ThresholdType == Emgu.CV.CvEnum.ThresholdType.Otsu)
                CvInvoke.Threshold(src: input, dst: output, threshold: 0, maxValue: 255, thresholdType: ThresholdType | Emgu.CV.CvEnum.ThresholdType.Binary);
            else
                CvInvoke.Threshold(src: input, dst: output, threshold: Threshold, maxValue: MaxValue, thresholdType: ThresholdType);
            return output;
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Threshold, MaxValue, (float)ThresholdType };
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Threshold = parameters[0];
            MaxValue = parameters[0] + parameters[1];
            ThresholdType = (Emgu.CV.CvEnum.ThresholdType)parameters[2];
        }

        public float MaxValue { get; set; } = 30;
        public float Threshold { get; set; } = 10;

        public Emgu.CV.CvEnum.ThresholdType ThresholdType { get; set; } = Emgu.CV.CvEnum.ThresholdType.Binary;

        public override List<float>[] CGPParameterBounds
        {
            get
            {
                return GetCGPParameterBounds();
            }
        }

        public List<float>[] GetCGPParameterBounds()
        {
            var l = new List<float>[3]
            {
                    Enumerable.Range(0, 50).Select(x => (float)x * 5).ToList(), // threshold in {0, 5 ..., 245} 
                    new List<float>() // maxValue = some random max value, unsure how to use this properly. eg in binary threshold it assignes: x > thresh ? maxValue : 0
                    {
                        255
                    },
                    Enum.GetValues(typeof(Emgu.CV.CvEnum.ThresholdType)).Cast<Emgu.CV.CvEnum.ThresholdType>().ToList().Select(x => (float)x).ToList(),
            };
            return l;
        }


        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToRegion;
            }
        }

        public override int CGPInputCount
        {
            get
            {
                return 1;
            }
        }

    }
}
