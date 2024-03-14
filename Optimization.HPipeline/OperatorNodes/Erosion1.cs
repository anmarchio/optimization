using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class Erosion1 : HalconOperatorNode
    {
        public Erosion1() : base() { }

        public Erosion1(IList<HalconOperatorNode> children, float[] parameters) : base(children)
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

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.RegionToRegion;
            }
        }

        public StructElementTypes StructElement { get; set; }

        public float A { get; set; } = 5;
        public float B { get; set; } = 5;

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
            var erosion = string.Format("erosion1 ({0}, {1}, {2}, {3})", Children.First().OutputVariableName,
                structName, OutputVariableName, Iterations.ToString());

            lines.Add(structString);
            lines.Add(erosion);

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            using (var str = StructureElement.Generate(StructElement, A, B))
            {
                HOperatorSet.Erosion1(input, str, out output, Iterations);
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
