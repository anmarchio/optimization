using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class VarThreshold : HalconThresholdNode
    {
        public VarThreshold() : base() { }

        public VarThreshold(List<HalconOperatorNode> children, float[] parameters) : base(children, parameters)
        {
            FromCGPNodeParameters(parameters);
        }
        public int MaskWidth { get; set; } = 3;
        public int MaskHeight { get; set; } = 3;
        public float StdDevScale { get; set; } = 0;
        public short AbsThreshold { get; set; } = 64;
        public LightDarkType LightDark { get; set; } = LightDarkType.light;
        public enum LightDarkType
        {
            dark, equal, light, not_equal
        }
       
        public override int CGPInputCount
        {
            get
            {
                return 1;
            }
        }

        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if (cgpParameterBounds == null)
                {
                    cgpParameterBounds = new List<float>[5];
                    for (int i = 0; i < cgpParameterBounds.Length - 1; i++) cgpParameterBounds[i] = new List<float>();

                    for(int i = 3; i < 31; i = i + 2) { cgpParameterBounds[0].Add(i); } // mask width
                    for (int i = 3; i < 31; i = i + 2) { cgpParameterBounds[1].Add(i); } // mask height
                    for (float i = -1.0f; i <= 1.0f; i = i + 0.1f) { cgpParameterBounds[2].Add(i); } // stddevscale
                    for (short i = 0; i <= 128; i++) { cgpParameterBounds[3].Add(i); }
                    cgpParameterBounds[4] = Enum.GetValues(typeof(LightDarkType)).Cast<LightDarkType>().ToList().Select(x => (float)x).ToList();
                }
                return cgpParameterBounds;
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

            //Abs export; after this call the image is located in OutputVariableName
            var absText = base.HalconFunctionCall();
            //Attach at the rigth position, at start
            lines.AddRange(absText);

            string imageTypeOutput = "Img_type";
            string typeText = HObjectExtensions.ImageTypeHalconText(OutputVariableName, imageTypeOutput);

            lines.Add(typeText);

            lines.Add($"if ( ({imageTypeOutput} # 'byte') and ({imageTypeOutput} # 'int2') and ({imageTypeOutput} # 'int4')"+
                $" and ({imageTypeOutput} # 'uint2') and ({imageTypeOutput} # 'real'))");

            string convOutput = "Conv_out";
            var convText = HObjectExtensions.ConvertStandardHalconText(OutputVariableName, convOutput);
            lines.AddRange(convText);

            lines.Add($"var_threshold ({convOutput}, {OutputVariableName}, {MaskWidth.ToString()}, {MaskHeight.ToString()}," +
                $" {StdDevScale.ToString()}, {AbsThreshold.ToString()}, '{LightDark.ToString()}')");

            lines.Add($"else");
            lines.Add($"var_threshold ({OutputVariableName}, {OutputVariableName}, {MaskWidth.ToString()}, {MaskHeight.ToString()}," +
                $" {StdDevScale.ToString()}, {AbsThreshold.ToString()}, '{LightDark.ToString()}') ");

            lines.Add($"endif");

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            using (var abs = base.Execute(input))
            {
                if (!abs.IsImageType("byte", "int2", "int4", "uint2", "real"))
                    using (var conv = input.ConvertToStandardType())
                    {
                        HOperatorSet.VarThreshold(conv, out output, MaskWidth, MaskHeight, StdDevScale, AbsThreshold, LightDark.ToString());
                    }
                else
                    HOperatorSet.VarThreshold(abs, out output, MaskWidth, MaskHeight, StdDevScale, AbsThreshold, LightDark.ToString());
                return output;
            }
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            MaskWidth = (int)parameters[0];
            MaskHeight = (int)parameters[1];
            StdDevScale = parameters[2];
            AbsThreshold = (short)parameters[3];
            LightDark = (LightDarkType)parameters[4];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { MaskWidth, MaskHeight, StdDevScale, AbsThreshold, (float)LightDark };
        }
    }
}
