using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class ThresholdAccessChannel : HalconThresholdNode
    {
        public ThresholdAccessChannel() : base()
        {
        }

        public ThresholdAccessChannel(HalconOperatorNode child, int channel, int threshold, int sign) : base()
        {
            Channel = channel;
            Threshold = threshold;
            Sign = sign;
            AddChild(child);

        }

        public ThresholdAccessChannel(List<HalconOperatorNode> children, int channel, int threshold, int sign) : base()
        {
            Channel = channel;
            Threshold = threshold;
            Sign = sign;
            foreach (var child in children) AddChild(child);
        }

        public ThresholdAccessChannel(List<HalconOperatorNode> children, float[] parameters) : base(children, parameters)
        {
        }


        public int Channel
        {
            get; set;
        } = 1;
        public int Threshold
        {
            get; set;
        } = 0;
        public int Sign
        {
            get; set;
        } = 1;

        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if (cgpParameterBounds == null)
                {
                    var operatorMap = new OperatorMap();
                    cgpParameterBounds = operatorMap.ParameterBounds[1];
                }
                return cgpParameterBounds;
            }
        }
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return threshFuncMirrorExecute;
            }
        }


        /// <summary>
        /// this is for backwards compatibility simply to keep the old fitness evaluator running with somewhat newer operatormaps... 
        /// this should not be mimicked. do NEVER get inspiration from DecodingMap. It should be removed alltogether in the near future
        /// as should the old fitness evaluator
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="tuple"></param>
        /// <returns></returns>
        private HObject[] threshFuncMirrorExecute(HObject[] obj, HTuple[] tuple)
        {
            var input = obj[0];
            var o = new HObject[DecodingMap.MaxHObjects];
            HTuple numChannels;
            HOperatorSet.CountChannels(input, out numChannels);
            if (numChannels == 3)
                HOperatorSet.AccessChannel(input, out obj[1], Channel);
            else
                output = input;

            HTuple grayVal;
            HOperatorSet.GetGrayval(input, 0, 0, out grayVal);
            var min = grayVal.TupleMin();

            if (min < 0)
            {
                if (Sign == -1.0)
                    HOperatorSet.Threshold(output, out o[0], -128.0, Sign * Threshold);
                else
                    HOperatorSet.Threshold(output, out o[0], Threshold, 128.0);
            }
            else
            {
                HOperatorSet.Threshold(output, out o[0], Threshold, 255);
            }
            return o;
        }

        public override void DisposeOutput()
        {
            if (output == null) return;
            output.Dispose();
            output = null;
        }

        public override HObject Execute(HObject input)
        {
            using (var abs = base.Execute(input))
            {
                HTuple numChannels;
                HObject tmp = null;
                try
                {

                    HOperatorSet.CountChannels(abs, out numChannels);
                    if (numChannels == 3)
                    {
                        HOperatorSet.AccessChannel(abs, out tmp, Channel);
                        HOperatorSet.Threshold(tmp, out output, Threshold, 255);
                        tmp.Dispose();
                    }
                    else
                        HOperatorSet.Threshold(abs, out output, Threshold, 255);

                    //HTuple grayVal;
                    //HOperatorSet.GetGrayval(input, 0, 0, out grayVal);
                    //var min = grayVal.TupleMin();

                    return output;
                }
                finally
                {
                    if (tmp != null) tmp.Dispose();
                }
            }
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Threshold, Sign, Channel };
        }


        public override void FromCGPNodeParameters(float[] parameters)
        {
            Channel = (int)parameters[2];
            Threshold = (int)parameters[0];
            Sign = (int)parameters[1];
        }

        public override int CGPInputCount
        {
            get
            {
                return 1;
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
        /// Authors: mara
        /// Function to convert FunctionCall to halcon code for export into a hdev file
        /// </summary>
        /// <returns></returns>
        public override List<string> HalconFunctionCall()
        {

            List<string> lines = new List<string>();

            //Abs export; after this call the image is located in OutputVariableName
            var absText = base.HalconFunctionCall();
            //Attach at the rigth position, at start
            lines.AddRange(absText);

            string checkName = "Num_channels";
            lines.Add(string.Format("count_channels ({0}, {1})", OutputVariableName, checkName));
            lines.Add(string.Format("if({0} == 3)", checkName));

            string accessOutput = "Access_out";
            lines.Add(string.Format("access_channel ({0}, {1}, {2})", OutputVariableName, accessOutput, Channel));
            lines.Add(string.Format("threshold ({0}, {1}, {2}, {3})", accessOutput, OutputVariableName, Threshold, 255));
                       
            lines.Add($"else");

            lines.Add($"threshold ({OutputVariableName}, {OutputVariableName}, {Threshold}, {255})");

            lines.Add($"endif");

            return lines;
        }
    }
}

