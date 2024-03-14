using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]

    public class ExpandGray : HalconOperatorNode
    {
        public ExpandGray() : base()
        {

        }

        public ExpandGray(List<HalconOperatorNode> children, float[] parameters) : base(children, parameters)
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
                        -1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10
                    }, // Iterations
                    new List<float>()
                    {
                        (float)ModeType.image, (float)ModeType.region
                    }, // Mode
                    new List<float>{
                        5, 10, 15, 20, 25, 30, 40, 50
                    } // Threshold
                };
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageAndRegionToRegion;
            }
        }

        public float Threshold { get; set; } = 30;
        public int Iterations { get; set; } = -1;

        public ModeType Mode { get; set; } = ModeType.image;

        /// <summary>
        /// Author: braml
        /// Function to convert FunctionCall to halcon code for export into a hdev file
        /// </summary>
        /// <returns> List of strings that represent code to be executed as .hdev file</returns>
        public override List<string> HalconFunctionCall()
        {
            List<string> lines = new List<string>();

            //Übersetzung von findImage
            lines.Add($"gen_empty_obj({"Tmp"})");

            lines.Add($"if ({Iterations.ToString()} > 0)");
            lines.Add($"expand_gray ({Children.First().OutputVariableName}, {Children.First().OutputVariableName}," +
                $" {"Tmp"}, {OutputVariableName}," +
                $" {Iterations.ToString()}, '{Mode.ToString()}'," +
                $" {Threshold.ToString()})");
            lines.Add($"else");
            lines.Add($"expand_gray ({Children.First().OutputVariableName}, {Children.First().OutputVariableName}," +
                $" {"Tmp"}, {OutputVariableName}," +
                $" {Iterations.ToString()}, '{"maximal"}'," +
                $" {Threshold.ToString()})");
            lines.Add($"endif");

            return lines;
        }

        public enum ModeType
        {
            image, region
        }

        /// <summary>
        /// Workaround for imageandregiontoregion: search for input image
        /// </summary>
        /// <returns></returns>
        public override HObject Execute(HObject input)
        {
            var image = FindImage();
            HObject empty;
            HOperatorSet.GenEmptyObj(out empty);
            if(Iterations > 0)
                HOperatorSet.ExpandGray(input, image, empty, out output, Iterations, Mode.ToString(), Threshold);
            else
                HOperatorSet.ExpandGray(input, image, empty, out output, "maximal", Mode.ToString(), Threshold);

            return output;
        }

        private HObject FindImage()
        {
            var leaves = GetLeafNodes();
            var first = leaves.First();
            return (first as HalconInputNode).Input;                
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Iterations = (int)parameters[0];
            Mode = (ModeType)parameters[1];
            Threshold = parameters[2];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Iterations, (float) Mode, Threshold };
        }
    }
}
