using System;
using System.Collections.Generic;
using HalconDotNet;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.HalconPipeline;
using System.Linq;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]

    public class CropRectangle : HalconOperatorNode
    {
        public CropRectangle() : base() { }

        private CropSmallestRectangle smallestRectangle;

        public CropRectangle(CropSmallestRectangle smallestRectangle)
        {
            this.smallestRectangle = smallestRectangle;
        }


        public CropRectangle(HalconOperatorNode child, int maskWidth, int maskHeight, float minRatio) : base()
        {

            Initialize(maskWidth, maskHeight, minRatio);
        }

        public CropRectangle(IList<HalconOperatorNode> children, int maskWidth, int maskHeight, float minRatio) : base()
        {
            Initialize(maskWidth, maskHeight, minRatio);
        }

        public CropRectangle(IList<HalconOperatorNode> children, float[] parameters) : base()
        {
            Initialize((int)parameters[0], (int)parameters[1], (float)parameters[2]);
        }



        private void Initialize(int maskWidth, int maskHeight, float minRatio)
        {
            MaskWidth = maskWidth;
            MaskHeight = maskHeight;
            MinRatio = minRatio;

        }


        public double MinRatio { get; set; } = 0.5;
        public float MaskHeight { get; set; } = 3;
        public float MaskWidth { get; set; } = 3;

        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if (cgpParameterBounds == null)
                {
                    cgpParameterBounds = new List<float>[3];
                    cgpParameterBounds[0] = new List<float>()
            {
                3, 5, 7, 9, 13, 15, 17, 19, 21, 23, 27, 29
            };
                    cgpParameterBounds[1] = new List<float>
            {
                3, 5, 7, 9, 13, 15, 17, 19, 21, 23, 27, 29
            };
                    cgpParameterBounds[2] = new List<float>();
                    for (int i = 2; i < 23; i++) cgpParameterBounds[2].Add((float)i * 0.005f);// used as offset to Low
                }
                return cgpParameterBounds;
            }
        }
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return croprectangle;
            }
        }

        private HObject[] croprectangle(HObject[] arg1, HTuple[] arg2)
        {
            HObject[] o = new HObject[arg1.Length];
            CGPOperatorSet.RelativeThreshold(arg1[0], out o[0], arg2[0], arg2[1], arg2[2]);
            return o;
        }

        public override HObject Execute(HObject input)
        {
            CGPOperatorSet.RelativeThreshold(input, out output, MinRatio, MaskWidth, MaskHeight);
            return output;
        }

        public override void DisposeOutput()
        {
            if (output == null) return;
            output.Dispose();
            output = null;
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { MaskWidth, MaskHeight, (float)MinRatio };
        }


        public override void FromCGPNodeParameters(float[] parameters)
        {
            MaskWidth = parameters[0];
            MaskHeight = parameters[1];
            MinRatio = parameters[2];
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

        public override List<string> HalconFunctionCall()
        {
            return HObjectExtensions.RelativeThresholdHalconText(Children.First().OutputVariableName, OutputVariableName);
        }
    }
}