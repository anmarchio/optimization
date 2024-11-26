using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class AutoThreshold : HalconThresholdNode
    {

        public AutoThreshold(List<HalconOperatorNode> children, float[] parameters) : base(children, parameters)
        {

        }
        public AutoThreshold() : base()
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
                        0, 0.5f, 1, 2, 3, 4, 5 // Sigma
                    }
                };
            }
        }

        public float Sigma { get; set; } = 0;

        public override OperatorType OperatorType
        {
            get
            {
                return base.OperatorType | OperatorType.ImageToRegion;
            }
        }

        public override HObject Execute(HObject input)
        {
            using (var abs = base.Execute(input))
            {
                var imgType = abs.GetImageType();
                if (!(abs.Equals("byte") || abs.Equals("uint2") || abs.Equals("real")))
                    using (var conv = input.ConvertToStandardType())
                    {
                        HOperatorSet.AutoThreshold(conv, out output, Sigma);
                    }
                else
                    HOperatorSet.AutoThreshold(abs, out output, Sigma);
            }
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Sigma = parameters[0];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Sigma };
        }

        /// <summary>
        /// Author: braml
        /// Function to convert FunctionCall to halcon code for export into a hdev file
        /// </summary>
        /// <returns> List of strings that represent code to be executed as .hdev file</returns>
        public override List<string> HalconFunctionCall()
        {
            List<string> lines = new List<string>();

            var absText = base.HalconFunctionCall();
            lines.AddRange(absText);

            string imageTypeOutput = "Img_type";
            string typeText = HObjectExtensions.ImageTypeHalconText(OutputVariableName, imageTypeOutput);

            lines.Add(typeText);
            lines.Add($"if (( {imageTypeOutput} # 'byte') and ({imageTypeOutput} # 'uint2') and ({imageTypeOutput} # 'real') )");

            string convOutput = "Conv_out";
            var convText = HObjectExtensions.ConvertStandardHalconText(OutputVariableName, convOutput);
            lines.AddRange(convText);

            lines.Add(string.Format("auto_threshold ({0}, {1}, {2})", convOutput, OutputVariableName, Sigma.ToInvariantString()));

            lines.Add($"else");

            lines.Add(string.Format("auto_threshold ({0}, {1}, {2})", OutputVariableName, OutputVariableName, Sigma.ToInvariantString()));

            lines.Add($"endif");

            return lines;
        }
    }
}
