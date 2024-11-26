using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class MedianWeighted : HalconOperatorNode
    {
        public MedianWeightedMaskType MaskType { get; set; } = MedianWeightedMaskType.gauss;
        public int MaskSize { get; set; } = 3;  // according to documentation, the only supported mask size
        public enum MedianWeightedMaskType
        {
            gauss, inner
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
                    cgpParameterBounds = new List<float>[1];
                    cgpParameterBounds[0] = Enum.GetValues(typeof(MedianWeightedMaskType)).Cast<MedianWeightedMaskType>().ToList().Select(x => (float)x).ToList();
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

            lines.Add($"median_weighted ({Children.First().OutputVariableName}, {OutputVariableName}, '{MaskType.ToString()}', {MaskSize.ToString()})");

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.MedianWeighted(input, out output, MaskType.ToString(), MaskSize);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            MaskType = (MedianWeightedMaskType)parameters[0];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)MaskType };
        }
    }
}
