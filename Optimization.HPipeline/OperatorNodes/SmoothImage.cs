using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class SmoothImage : HalconOperatorNode
    {
        public SmoothImage() : base() { }

        public SmoothImage(IList<HalconOperatorNode> ignoreThis, float[] parameters) : base()
        {
            FromCGPNodeParameters(parameters);
        }

        public SmoothImageFilter Filter { get; set; }
        public float Alpha { get; set; } = 1;

        public enum SmoothImageFilter
        {
            deriche1, deriche2, gauss, shen
        }
        public override int CGPInputCount
        {
            get
            {
                return 1;
            }
        }

        private List<float>[] cgpParameterBounds;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if (cgpParameterBounds == null)
                {
                    cgpParameterBounds = new List<float>[2];
                    cgpParameterBounds[0] = Enum.GetValues(typeof(SmoothImageFilter)).Cast<SmoothImageFilter>().ToList().Select(x => (float)x).ToList();
                    cgpParameterBounds[1] = new List<float>();
                    for (float i = 0.01f; i <= 50.0f; i = i + 0.01f) cgpParameterBounds[1].Add(i); // alpha
                }
                return cgpParameterBounds;
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
            }
        }

        /// <summary>
        /// Author: braml
        /// Function to convert FunctionCall to halcon code for export into a hdev file
        /// </summary>
        /// <returns> List of strings that represent code to be executed as .hdev file</returns>
        public override List<string> HalconFunctionCall()
        {
            List<string> lines = new List<string>();

            lines.Add($"smooth_image ({Children.First().OutputVariableName}, {OutputVariableName}, '{Filter.ToString()}', {Alpha.ToString()})");

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.SmoothImage(input, out output, Filter.ToString(), Alpha);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Filter = (SmoothImageFilter)parameters[0];
            Alpha = parameters[1];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float) Filter, Alpha };
        }
    }
}
