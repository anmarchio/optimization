using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class CoherenceEnhancingDiff : HalconOperatorNode
    {
        public CoherenceEnhancingDiff() : base() { }

        public CoherenceEnhancingDiff(List<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            FromCGPNodeParameters(parameters);
        }

        public float Sigma { get; set; } = 0;
        public int Rho { get; set; } = 0;
        public float Theta { get; set; } = 0.1f;
        public int Iterations { get; set; } = 5;

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
                    cgpParameterBounds = new List<float>[4];
                    cgpParameterBounds[0] = new List<float>();
                    for (float i = 0.0f; i <= 1.0f; i = i + 0.1f) cgpParameterBounds[0].Add(i); // Sigma
                    cgpParameterBounds[1] = new List<float>();
                    for (int i = 0; i <= 30; i++) cgpParameterBounds[1].Add(i); // Rho
                    cgpParameterBounds[2] = new List<float>();
                    for (float i = 0.1f; i <= 0.5f; i = i + 0.1f) cgpParameterBounds[2].Add(i); // Theta
                    cgpParameterBounds[3] = new List<float>();
                    for (int i = 1; i <= 500; i++) cgpParameterBounds[3].Add(i); // Iterations                    
                }
                return cgpParameterBounds;
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

            lines.Add($"coherence_enhancing_diff ({Children.First().OutputVariableName}, {OutputVariableName}," +
                $" {Sigma.ToString()}, {Rho.ToString()}, {Theta.ToInvariantString()}, {Iterations.ToString()})");

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.CoherenceEnhancingDiff(input, out output, Sigma, Rho, Theta, Iterations);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Sigma = (float)parameters[0];
            Rho = (int)parameters[1];
            Theta = (float)parameters[2];
            Iterations = (int)parameters[3];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Sigma, Rho, Theta, Iterations };
        }
    }
}
