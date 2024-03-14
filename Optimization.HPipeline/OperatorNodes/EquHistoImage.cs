using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class EquHistoImage : HalconOperatorNode
    {
        public EquHistoImage(List<HalconOperatorNode> children, float[] parameters) : base(children, parameters)
        {
            FromCGPNodeParameters(parameters);
        }
        public EquHistoImage() : base()
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
                return new List<float>[0];  // since euq_histo_image doesn't return any parameters
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
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

            lines.Add(String.Format("equ_histo_image ({0}, {1})", Children.First().OutputVariableName, OutputVariableName));

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.EquHistoImage(input, out output);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            // do nothing : euq_histo_image doesn't return any parameters
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[0]; // since euq_histo_image doesn't return any parameters
        }
    }
}
