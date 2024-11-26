using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class FastThreshold : HalconThresholdNode
    {
        public FastThreshold() : base() { }

        public FastThreshold(List<HalconOperatorNode> children, float[] parameters) : base(children, parameters)
        {
            FromCGPNodeParameters(parameters);
        }

        public int MinGray { get; set; } = 10;
        public int MaxGrayOffset { get; set; } = 200;
        public int MinSize { get; set; } = 2;

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
                        cgpParameterBounds = new List<float>[3];
                        cgpParameterBounds[0] = new List<float>();
                        for (int i = 0; i <= 254; i++) cgpParameterBounds[0].Add(i); // min gray
                        cgpParameterBounds[1] = new List<float>();
                        for (int i = 0; i <= 255; i++) cgpParameterBounds[1].Add(i); // max gray offset
                        cgpParameterBounds[2] = new List<float>();
                        for (int i = 2; i <= 200; i++) cgpParameterBounds[2].Add(i); // min size
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
        /// Workaround for imageandregiontoregion: search for input image
        /// </summary>
        /// <returns></returns>
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
                $" and ({imageTypeOutput} # 'cyclic' ) and ({imageTypeOutput} # 'real') " +
                $" and ({channelCountOutput} == 1) )");

            string convOutput = "Conv_out";
            var convText = HObjectExtensions.ConvertStandardHalconText(OutputVariableName, convOutput);
            lines.AddRange(convText);

            string singleOutput = "Single_out";
            var singleText = HObjectExtensions.SingleChanelFromMultiHalconText(convOutput, singleOutput);
            lines.AddRange(singleText);

            //Actual Call
            lines.Add($"fast_threshold ({singleOutput}, {OutputVariableName}, {MinGray.ToString()}," +
                $" {Math.Max(MinGray + MaxGrayOffset, 255).ToString()}, {MinSize.ToString()})");

            //less strict condition
            lines.Add($"elseif ( ({imageTypeOutput} # 'byte') and ({imageTypeOutput} # 'uint2') and ({imageTypeOutput} # 'direction')" +
                $" and ({imageTypeOutput} # 'cyclic' ) and ({imageTypeOutput} # 'real') )");

            //Alternative Call
            lines.Add($"fast_threshold ({convOutput}, {OutputVariableName}, {MinGray.ToString()}," +
                $" {Math.Max(MinGray + MaxGrayOffset, 255).ToString()}, {MinSize.ToString()})");

            //condition for other already fitting image types
            lines.Add($"elseif (({channelCountOutput} # 1))");

            //Alternative Call
            string singleOutputAlt = "Single_out_alt";
            var singleTextAlt = HObjectExtensions.SingleChanelFromMultiHalconText(OutputVariableName, singleOutputAlt);
            lines.AddRange(singleTextAlt);
            lines.Add($"fast_threshold({singleOutputAlt}, {OutputVariableName}, { MinGray.ToString()}," +
                $" {Math.Max(MinGray + MaxGrayOffset, 255).ToString()}, {MinSize.ToString()})");

            //No Condition holds, so perfectly suited already
            lines.Add($"else");
            lines.Add($"fast_threshold ({OutputVariableName}, {OutputVariableName}, {MinGray.ToString()}," +
                $" {Math.Max(MinGray + MaxGrayOffset, 255).ToString()}, {MinSize.ToString()})");

            lines.Add($"endif");

            return lines;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override HObject Execute(HObject input)
        {
            using (var abs = base.Execute(input))
            {
                if (!abs.IsImageType("byte", "uint2", "direction", "cyclic", "real") && !abs.IsSingleChannel())
                {
                    using (var conv = abs.ConvertToStandardType())
                    {
                        using (var single = conv.SingleChannelFromMulti())
                        {
                            HOperatorSet.FastThreshold(single, out output, MinGray, Math.Max(MinGray + MaxGrayOffset, 255), MinSize);
                        }
                    }
                }
                else if (!abs.IsImageType("byte", "uint2", "direction", "cyclic", "real"))
                    using (var conv = abs.ConvertToStandardType())
                    {
                        HOperatorSet.FastThreshold(conv, out output, MinGray, Math.Max(MinGray + MaxGrayOffset, 255), MinSize);
                    }
                else if (!abs.IsSingleChannel())
                    using (var single = abs.SingleChannelFromMulti())
                    {
                        HOperatorSet.FastThreshold(single, out output, MinGray, Math.Max(MinGray + MaxGrayOffset, 255), MinSize);
                    }
                else
                    HOperatorSet.FastThreshold(abs, out output, MinGray, Math.Max(MinGray + MaxGrayOffset, 255), MinSize);

                return output;
            }
        }
        public override void FromCGPNodeParameters(float[] parameters)
        {
            MinGray = (int)parameters[0];
            MaxGrayOffset = (int)parameters[1];
            MinSize = (int)parameters[2];
        }
        public override float[] ToCGPNodeParameters()
        {
            return new float[] { MinGray, MaxGrayOffset, MinSize };
        }

    }
}
