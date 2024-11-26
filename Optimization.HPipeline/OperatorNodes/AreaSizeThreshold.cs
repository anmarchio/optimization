using System;
using System.Collections.Generic;
using HalconDotNet;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class AreaSizeThreshold : HalconThresholdNode
    {
        public AreaSizeThreshold() { }

        /// <summary>
        /// Splits the image into regions and thresholds them according to size
        /// </summary>
        /// <param name="child">Input</param>
        /// <param name="minGray">Min Gray Value</param>
        /// <param name="maxGray">Max Gray Value</param>
        /// <param name="minSize">Min Size</param>
        /// <param name="maxSize">Max Size</param>
        /// <param name="windowWidth">Window width</param>
        /// <param name="windowHeight">Window height</param>
        /*
        public AreaSizeThreshold(HalconOperatorNode child, int minGray, int maxGray, int minSize, int maxSize, int windowWidth, int windowHeight) : base(child)
        {

            Initialize(minGray, maxGray, minSize, maxSize, windowWidth, windowHeight);
        }

        public AreaSizeThreshold(IList<HalconOperatorNode> children, int minGray, int maxGray, int minSize, int maxSize, int windowWidth, int windowHeight) : base(children)
        {
            Initialize(minGray, maxGray, minSize, maxSize, windowWidth, windowHeight);
        }

        public AreaSizeThreshold(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            Initialize((int)parameters[0], (int)parameters[1], (int)parameters[2], (int)parameters[3], (int)parameters[4], (int)parameters[5]);
        }*/

     
        private void Initialize(int minGray, int maxGray, int minSize, int maxSize, int windowWidth, int windowHeight)
        {
            MinGray = minGray;
            MaxGray = maxGray;
            MinSize = minSize;
            MaxSize = maxSize;
            WindowWidth = windowHeight;
            WindowHeight = windowWidth;
            
        }

        public float MinGray { get; set; } = 15;
        public float MaxGray { get; set; } = 230;
        public float MinSize { get; set; } = 9000;
        public float MaxSize { get; set; } = 18000;
        public float WindowWidth { get; set; } = 160;
        public float WindowHeight { get; set; } = 160;


        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if(cgpParameterBounds == null)
                {
                    cgpParameterBounds = new List<float>[6];
                    cgpParameterBounds[0] = new List<float>();
                    cgpParameterBounds[1] = new List<float>();
                    cgpParameterBounds[2] = new List<float>();
                    cgpParameterBounds[3] = new List<float>();
                    cgpParameterBounds[4] = new List<float>();
                    cgpParameterBounds[5] = new List<float>();
                    for (int i = 15; i < 30; i++) cgpParameterBounds[0].Add(i);
                    for (int i = 230; i < 256; i++) cgpParameterBounds[1].Add(i);
                    for (int i = 9000; i < 11000; i = i + 1000) cgpParameterBounds[2].Add(i);
                    for (int i = 18000; i < 22000; i = i + 1000) cgpParameterBounds[3].Add(i);
                    for (int i = 160; i < 321; i = i + 10) cgpParameterBounds[4].Add(i);
                    for (int i = 160; i < 321; i = i + 10) cgpParameterBounds[5].Add(i);
                }
                return cgpParameterBounds;
            }
        }
        
        public override float[] ToCGPNodeParameters()
        {
            return new float[] { MinGray, MaxGray, MinSize, MaxSize, WindowWidth, WindowHeight };
        }
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return areasizethreshold;
            }
        }

        private HObject[] areasizethreshold(HObject[] arg1, HTuple[] arg2)
        {
            HObject[] o = new HObject[arg1.Length];
            CGPOperatorSet.AreaSizeThreshold(arg1[0], out o[0], arg2[0], arg2[1], arg2[2], arg2[3], arg2[4], arg2[5]);
            return o;
        }

        public override HObject Execute(HObject input)
        {
            using (var abs = base.Execute(input))
            {
                CGPOperatorSet.AreaSizeThreshold(abs, out output, MinGray, MaxGray, MinSize, MaxSize, WindowWidth, WindowHeight);
                return output;
            }
        }

        public override void DisposeOutput()
        {
            if (output == null) return;
            output.Dispose();
            output = null;
        }

      
        public override void FromCGPNodeParameters(float[] parameters)
        {
            MinGray = parameters[0];
            MaxGray = parameters[1];
            MinSize = parameters[2];
            MaxSize = parameters[3];
            WindowWidth = parameters[4];
            WindowHeight = parameters[5];
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

        public override List<string> HalconFunctionCall()
        {
            throw new NotImplementedException();
        }
    }
}