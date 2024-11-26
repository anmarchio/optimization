using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class Dilation1 : HalconOperatorNode
    {
        public Dilation1() : base() { }

        public Dilation1(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            FromCGPNodeParameters(parameters);
        }

        public int Iterations { get; set; } = 10;
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
                    Enumerable.Range(1, 50).Select(x => (float)x).ToList(),
                    StructureElement.GetParameterBounds(),
                    StructureElement.GetParameterBoundsA(),
                    StructureElement.GetParameterBoundsB()
                };
                
            }
        }

        public StructElementTypes StructElement { get; set; }

        public float A { get; set; } = 5;
        public float B { get; set; } = 5;

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.RegionToRegion;
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

            var structName = StructElement.ToString() + NodeID.ToString();
            var structString = StructureElement.GenerateHalconString(StructElement, structName, A, B);
            var dilation = string.Format("dilation1 ({0}, {1}, {2}, {3})", Children.First().OutputVariableName,
                structName, OutputVariableName, Iterations.ToString());

            lines.Add(structString);
            lines.Add(dilation);

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            using (var SE = StructureElement.Generate(StructElement, A, B))
            {
                HOperatorSet.Dilation1(input, SE, out output, Iterations);
                return output;
            }
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Iterations = (int)parameters[0];
            StructElement = (StructElementTypes)parameters[1];
            A = parameters[2];
            B = parameters[3];

        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Iterations, (float)StructElement, A, B };
        }
    }
}
