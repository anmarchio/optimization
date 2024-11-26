using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class GrayErosion : HalconOperatorNode
    {
        public GrayErosion() : base() { }

        public GrayErosion(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            FromCGPNodeParameters(parameters);
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
                    StructureElement.GetParameterBoundsA(),
                    StructureElement.GetParameterBoundsB(),
                    StructureElement.GetParameterBoundsGrayValues()
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

        public override HObject Execute(HObject input)
        {
            var imageType = input.GetImageType();
            using (var str = StructureElement.GenerateGray(StructElement, imageType, A, B, GrayValueMax))
            {
                HOperatorSet.GrayErosion(input, str, out output);
                return output;
            }
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            A = parameters[0];
            B = parameters[1];
            GrayValueMax = parameters[2];

        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { A, B, GrayValueMax };
        }

        public StructElementTypes StructElement { get; set; } = StructElementTypes.Circle;

        public float A { get; set; } = 5;
        public float B { get; set; } = 5;
        public float GrayValueMax { get; set; } = 255;

        /// <summary>
        /// Author: braml
        /// Function to convert FunctionCall to halcon code for export into a hdev file
        /// </summary>
        /// <returns> List of strings that represent code to be executed as .hdev file</returns>
        public override List<string> HalconFunctionCall()
        {
            List<string> lines = new List<string>();

            string imageTypeOutput = "Img_type";
            string typeText = HObjectExtensions.ImageTypeHalconText(Children.First().OutputVariableName, imageTypeOutput);
            var structName = StructElement.ToString() + NodeID.ToString();
            var structString = StructureElement.GenerateGrayHalconString(StructElement, structName, imageTypeOutput, A, B, GrayValueMax);
            var grayerosion = string.Format("gray_erosion({0}, {1}, {2})", Children.First().OutputVariableName,
                structName, OutputVariableName);

            lines.Add(typeText);
            lines.Add(structString);
            lines.Add(grayerosion);

            return lines;
        }
    }
}
