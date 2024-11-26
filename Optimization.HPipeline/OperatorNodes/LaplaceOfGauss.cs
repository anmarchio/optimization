using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class LaplaceOfGauss : HalconOperatorNode
    {
        public LaplaceOfGauss() : base() { }

        public LaplaceOfGauss(IList<HalconOperatorNode> ignoreThis, float[] parameters) : base()
        {
            FromCGPNodeParameters(parameters);
        }
        public float Sigma { get; set; } = 5.0f;
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
                    cgpParameterBounds = new List<float>[1];
                    cgpParameterBounds[0] = new List<float>();
                    for (float i = 0.7f; i <= 25.0f; i = i + 0.01f) cgpParameterBounds[0].Add(i);
                }
                return cgpParameterBounds;
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                /*
                 * Remove this node from collection
                 */
                //return OperatorType.ImageToImage;
                return OperatorType.Bug;
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

            lines.Add($"laplace_of_gauss ({Children.First().OutputVariableName}, {OutputVariableName}, {((float) Sigma).ToInvariantString()})");

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.LaplaceOfGauss(input, out output, Sigma);
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
    }
}
