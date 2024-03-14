using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class LocalThreshold : HalconThresholdNode
    {
        public LocalThreshold() : base() { }

        public LocalThreshold(List<HalconOperatorNode> children, float[] parameters) : base(children, parameters)
        {
            FromCGPNodeParameters(parameters);
        }

        public LocalThresholdMethod Method { get; set; } = LocalThresholdMethod.adapted_std_deviation;
        public LocalThresholdLightDark LightDark { get; set; } = LocalThresholdLightDark.light;
        public enum LocalThresholdMethod
        {
            adapted_std_deviation
        }
        public enum LocalThresholdLightDark
        {
            dark, light
        }

        #region generic parameters -- excluding range
        public int MaskSize { get; set; } = 15;
        public float Scale { get; set; } = 0.2f;
        #endregion

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
                return
                    new List<float>[]
               {
                   Enum.GetValues(typeof(LocalThresholdMethod)).Cast<LocalThresholdMethod>().ToList().Select(x => (float)x).ToList(),
                   Enum.GetValues(typeof(LocalThresholdLightDark)).Cast<LocalThresholdLightDark>().ToList().Select(x => (float)x).ToList(),
                   new List<float>() { 15, 21, 31 }, // masksize
                   new List<float>() { 0.2f, 0.3f, 0.5f }, // scale
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

            //Abs export; after this call the image is located in OutputVariableName
            var absText = base.HalconFunctionCall();
            //Attach at the rigth position, at start
            lines.AddRange(absText);

            string imageTypeOutput = "Img_type";
            string typeText = HObjectExtensions.ImageTypeHalconText(OutputVariableName, imageTypeOutput);
            string channelCountOutput = "Channel_count";
            string channelText = HObjectExtensions.SingleChannelHalconText(OutputVariableName, channelCountOutput);
            //Using Export functions for IsImageType, IsSingleChannel, ConvertToStandardType, SingleChannelFromMulti, all declared inHObjectExtension
            lines.Add(typeText);
            lines.Add(channelText);
            //strict condition
            lines.Add($"if ( ({imageTypeOutput} # 'byte') and ({imageTypeOutput} # 'uint2') and ({imageTypeOutput} # 'direction')" +
                $" and ({imageTypeOutput} # 'cyclic' ) and ({imageTypeOutput} # 'real')" +
                $" and ({channelCountOutput} == 1) )");

            string convOutput = "Conv_out";
            var convText = HObjectExtensions.ConvertStandardHalconText(OutputVariableName, convOutput);
            lines.AddRange(convText);

            string singleOutput = "Single_out";
            var singleText = HObjectExtensions.SingleChanelFromMultiHalconText(convOutput, singleOutput);
            lines.AddRange(singleText);

            //Actual Call
            lines.Add($"local_threshold ({singleOutput}, {OutputVariableName}, '{Method.ToString()}'," +
                $" '{LightDark.ToString()}', ['mask_size', 'scale'], [{MaskSize.ToString()}, {Scale.ToInvariantString()}] )");

            //less strict condition
            lines.Add($"elseif ( ({imageTypeOutput} # 'byte') and ({imageTypeOutput} # 'uint2') and ({imageTypeOutput} # 'direction')" +
                $" and ({imageTypeOutput} # 'cyclic' ) and ({imageTypeOutput} # 'real'))");

            //Alternative Call
            lines.Add($"local_threshold ({convOutput}, {OutputVariableName}, '{Method.ToString()}'," +
                $" '{LightDark.ToString()}',['mask_size', 'scale'], [{MaskSize.ToString()}, {Scale.ToInvariantString()}] )");

            //condition for other already fitting image types
            lines.Add($"elseif (({channelCountOutput} # 1))");

            //Alternative Call
            string singleOutputAlt = "Single_out_alt";
            var singleTextAlt = HObjectExtensions.SingleChanelFromMultiHalconText(OutputVariableName, singleOutputAlt);
            lines.AddRange(singleTextAlt);
            lines.Add($"local_threshold({singleOutputAlt}, {OutputVariableName}, '{Method.ToString()}'," +
                $" '{LightDark.ToString()}', ['mask_size', 'scale'], [{MaskSize.ToString()}, {Scale.ToInvariantString()}] )");

            //No Condition holds, so perfectly suited already
            lines.Add($"else");
            lines.Add($"local_threshold ({OutputVariableName}, {OutputVariableName}, '{Method.ToString()}'," +
                $" '{LightDark.ToString()}', ['mask_size', 'scale'], [{MaskSize.ToString()}, {Scale.ToInvariantString()}] )");

            lines.Add($"endif");

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            using (var abs = base.Execute(input))
            {
                if (!abs.IsImageType("byte", "uint2") && !abs.IsSingleChannel())
                {
                    using (var conv = abs.ConvertToStandardType())
                    {
                        using (var single = conv.SingleChannelFromMulti())
                        {
                            return ExecuteUsing(single);
                        }
                    }
                }
                else if (!abs.IsImageType("byte", "uint2"))
                {
                    using (var conv = abs.ConvertToStandardType())
                    {
                        return ExecuteUsing(conv);
                    }
                }
                else if (!abs.IsSingleChannel())
                {
                    using (var single = abs.SingleChannelFromMulti())
                    {
                        return ExecuteUsing(single);
                    }
                }
                else
                {
                    return ExecuteUsing(abs);
                }
            }
        }

        private HObject ExecuteUsing(HObject input)
        {
            HOperatorSet.LocalThreshold(input, out output, Method.ToString(), LightDark.ToString(),
               new string[] { "mask_size", "scale" }, new double[] { MaskSize, Scale });
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Method = (LocalThresholdMethod)parameters[0];
            LightDark = (LocalThresholdLightDark)parameters[1];
            MaskSize = (int)parameters[2];
            Scale = parameters[3];

        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)Method, (float)LightDark, MaskSize, Scale };
        }
    }
}
