using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class HighpassImage : HalconOperatorNode
    {
        public HighpassImage() : base() { }

        public HighpassImage(IList<HalconOperatorNode> ignoreThis, float[] parameters) : base()
        {
            FromCGPNodeParameters(parameters);
        }

        public int MaskWidth { get; set; } = 5;
        public int MaskHeight { get; set; } = 5;

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
                    cgpParameterBounds[0] = new List<float>();
                    for(int i = 3; i <= 501; i = i + 2) cgpParameterBounds[0].Add(i);
                    cgpParameterBounds[1] = new List<float>();
                    for (int i = 3; i <= 501; i = i + 2) cgpParameterBounds[1].Add(i);
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

            lines.Add($"highpass_image ({Children.First().OutputVariableName}, {OutputVariableName}, {MaskWidth.ToString()}, {MaskHeight.ToString()})");

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.HighpassImage(input, out output, MaskWidth, MaskHeight);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            MaskWidth = (int)parameters[0];
            MaskHeight = (int)parameters[1];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { MaskWidth, MaskHeight };
        }
    }
}