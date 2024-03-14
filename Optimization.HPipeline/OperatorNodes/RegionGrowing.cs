using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]

    public class RegionGrowing : HalconOperatorNode
    {
        public RegionGrowing() : base()
        {

        }
        public RegionGrowing(List<HalconOperatorNode> children, float[] parameter) : base(children, parameter)
        {

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
                return new List<float>[]
                {
                    new List<float>()
                    {
                        1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21
                    }, // RasterHeight
                    new List<float>()
                    {
                        1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21
                    }, // RasterWidth
                    new List<float>()
                    {
                        1, 2, 3, 4, 5, 6, 7, 8, 9, 19, 12, 13, 18, 25
                    }, // Tolerance
                    new List<float>()
                    {
                        1, 5, 10, 20, 50, 100, 200, 500, 1000
                    } // MinRegionSize
                };
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToRegion;
            }
        }

        public float RasterHeight { get; set; } = 3;

        public float RasterWidth { get; set; } = 3;
        public float Tolerance { get; set; } = 6;
        public float MinRegionSize { get; set; } = 100;

        /// <summary>
        /// Author: braml
        /// Function to convert FunctionCall to halcon code for export into a hdev file
        /// </summary>
        /// <returns> List of strings that represent code to be executed as .hdev file</returns>
        public override List<string> HalconFunctionCall()
        {
            List<string> lines = new List<string>();

            lines.Add($"regiongrowing ({Children.First().OutputVariableName}, {OutputVariableName}, {RasterHeight.ToString()}, " +
                $"{RasterWidth.ToString()}, {Tolerance.ToString()}, {MinRegionSize.ToString()})");

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.Regiongrowing(input, out output,  RasterHeight, RasterWidth, Tolerance, MinRegionSize);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            RasterHeight = parameters[0];
            RasterWidth = parameters[1];
            Tolerance = parameters[2];
            MinRegionSize = parameters[3];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { RasterHeight, RasterWidth, Tolerance, MinRegionSize };
        }
    }
}
