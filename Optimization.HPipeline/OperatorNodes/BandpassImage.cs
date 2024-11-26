using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class BandpassImage : HalconOperatorNode
    {
        public BandpassImage() : base() { }

        public BandpassImage(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            FromCGPNodeParameters(parameters);
        }

        public BandpassImageFilterType FilterType { get; set; } = BandpassImageFilterType.lines;


        /// <summary>
        /// according to documentation this only allows 'lines' as value
        /// </summary>
        public enum BandpassImageFilterType
        {
            //gradient_max, gradient_sum, roberts_max
            lines
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
                return new List<float>[]{
                    new List<float>(){0}
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

            lines.Add($"bandpass_image ({Children.First().OutputVariableName}, {OutputVariableName}," +
                $" '{BandpassImageFilterType.lines.ToString()}')");

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.BandpassImage(input, out output, FilterType.ToString());
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            FilterType = (BandpassImageFilterType)parameters[0];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] {(float)FilterType}; // since bandpass_image doesn't return any parameters
        }
    }
}
