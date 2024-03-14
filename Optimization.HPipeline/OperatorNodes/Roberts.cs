using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class Roberts : HalconOperatorNode
    {
        public Roberts() : base() { }

        public Roberts(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            FromCGPNodeParameters(parameters);
        }
        public RobertsFilterType FilterType { get; set; }
        public enum RobertsFilterType
        {
            gradient_max, gradient_sum, roberts_max
        }
        public override int CGPInputCount
        {
            get
            {
                return 1;
            }
        }

        public override List<float>[] CGPParameterBounds
        {
            get
            {
                return new List<float>[] {
                Enum.GetValues(typeof(RobertsFilterType)).Cast<RobertsFilterType>().ToList().Select(x => (float)x).ToList()
                };
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

            lines.Add($"roberts ({Children.First().OutputVariableName}, {OutputVariableName}, '{FilterType.ToString()}')");

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.Roberts(input, out output, FilterType.ToString());
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            FilterType = (RobertsFilterType)parameters[0];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)FilterType };
        }
    }
}
