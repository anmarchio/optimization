using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class ZeroCrossing : HalconOperatorNode
    {
        public ZeroCrossing(List<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            FromCGPNodeParameters(parameters);
        }
        public ZeroCrossing() : base()
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
                return new List<float>[0];  // since ZeroCrossing doesn't return any parameters
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToRegion;
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

            string imageTypeOutput = "Img_type";
            string typeText = HObjectExtensions.ImageTypeHalconText(Children.First().OutputVariableName, imageTypeOutput);

            lines.Add(typeText);

            lines.Add($"if ( ({imageTypeOutput} # 'int1') and ({imageTypeOutput} # 'int2') and ({imageTypeOutput} # 'int4')" +
                $" and ({imageTypeOutput} # 'real'))");

            string convOutput = "Conv_out";
            var convText = HObjectExtensions.ConvertStandardHalconText(Children.First().OutputVariableName, convOutput, "int2");
            lines.AddRange(convText);

            lines.Add($"zero_crossing ({convOutput}, {OutputVariableName})");

            lines.Add($"else");
            lines.Add($"zero_crossing ({Children.First().OutputVariableName}, {OutputVariableName}) ");

            lines.Add($"endif");

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            if(!input.IsImageType("int1", "int2", "int4", "real"))
            {
                using(var conv = input.ConvertToStandardType("int2"))
                {
                    HOperatorSet.ZeroCrossing(conv, out output);
                }
            }
            else
                HOperatorSet.ZeroCrossing(input, out output);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            // do nothing : zero_crossing doesn't return any parameters
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[0]; // since zero_crossing doesn't return any parameters
        }

    }
}
