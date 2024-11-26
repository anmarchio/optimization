using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline
{
    [Serializable]
    public abstract class HalconThresholdNode : HalconOperatorNode
    {
        public HalconThresholdNode() : base()
        {

        }

        public HalconThresholdNode(List<HalconOperatorNode> children, float[] parameters) : base(children, parameters)
        {
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.Threshold | OperatorType.ImageToRegion;
            }
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.AbsImage(input, out output);
            return output;
        }

        /// <summary>
        /// author:mara
        /// Helper Function for upper level halcon code export
        /// </summary>
        /// <param name="imageName"></param>
        /// <param name="outVariable"></param>
        /// <param name="standardType"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        public override List<string> HalconFunctionCall()
        {
            List<string> lines = new List<string>();
            lines.Add(string.Format("abs_image ({0}, {1})", Children.First().OutputVariableName, OutputVariableName));
            return lines;
        }
    }
}
