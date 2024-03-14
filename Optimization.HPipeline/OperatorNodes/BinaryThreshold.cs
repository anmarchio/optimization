using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class BinaryThreshold : HalconThresholdNode
    {
        public BinaryThreshold() : base() { }

        public BinaryThreshold(List<HalconOperatorNode> children, float[] parameters) : base(children, parameters)
        {
            FromCGPNodeParameters(parameters);
        }
        public BinaryThresholdMethod Method { get; set; }
        public BinaryThresholdLightDark LightDark { get; set; }
        public enum BinaryThresholdMethod
        {
            max_separability, smooth_histo
        }
        public enum BinaryThresholdLightDark
        {
            dark, light
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
                    Enum.GetValues(typeof(BinaryThresholdMethod)).Cast<BinaryThresholdMethod>().ToList().Select(x => (float)x).ToList(),
                    Enum.GetValues(typeof(BinaryThresholdLightDark)).Cast<BinaryThresholdLightDark>().ToList().Select(x => (float)x).ToList(),
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

            lines.Add(string.Format("binary_threshold ({0}, {1}, '{2}', '{3}', {4})", convOutput, OutputVariableName,
                Method.ToString(), LightDark.ToString(), "Usedthreshold"));

            lines.Add($"else");
            lines.Add(string.Format("binary_threshold ({0}, {1}, '{2}', '{3}', {4})", OutputVariableName, OutputVariableName,
                Method.ToString(), LightDark.ToString(), "Usedthreshold"));

            lines.Add($"endif");

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            using (var abs = base.Execute(input))
            {
                HTuple UsedThreshold;
                if (!abs.IsImageType("byte", "uint2", "real"))
                    using (var conv = abs.ConvertToStandardType())
                    {
                        HOperatorSet.BinaryThreshold(conv, out output, Method.ToString(), LightDark.ToString(), out UsedThreshold);
                    }
                else
                    HOperatorSet.BinaryThreshold(abs, out output, Method.ToString(), LightDark.ToString(), out UsedThreshold);

                return output;
            }
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Method = (BinaryThresholdMethod)parameters[0];
            LightDark = (BinaryThresholdLightDark)parameters[1];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)Method, (float)LightDark }; // since binary_threshold doesn't return any parameters
        }
    }
}
