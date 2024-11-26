using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class SigmaImage : HalconOperatorNode
    {
        public SigmaImage() : base() { }

        public SigmaImage(IList<HalconOperatorNode> ignoreThis, float[] parameters) : base()
        {
            FromCGPNodeParameters(parameters);
        }

        public int MaskHeight { get; set; } = 3;
        public int MaskWidth { get; set; } = 3;
        public int Sigma { get; set; } = 20;

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
                    cgpParameterBounds = new List<float>[3];
                    cgpParameterBounds[0] = new List<float>();
                    for (int i = 3; i <= 20; i = i + 2) cgpParameterBounds[0].Add(i); //mask height, used to go up to 101
                    cgpParameterBounds[1] = new List<float>();
                    for (int i = 3; i <= 20; i = i + 2) cgpParameterBounds[1].Add(i); // mask width, used to go up to 101
                    cgpParameterBounds[2] = new List<float>();
                    for (int i = 0; i <= 255; i++) cgpParameterBounds[2].Add(i); // sigma
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

            lines.Add($"sigma_image ({Children.First().OutputVariableName}, {OutputVariableName}, {MaskHeight.ToString()}, {MaskWidth.ToString()}, " +
                $"{Sigma.ToString()})");

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.SigmaImage(input, out output, MaskHeight, MaskWidth, Sigma);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            MaskHeight = (int)parameters[0];
            MaskWidth = (int)parameters[1];
            Sigma = (int)parameters[2];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { MaskHeight, MaskWidth, Sigma };
        }
    }
}
