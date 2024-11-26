using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class Opening : HalconOperatorNode
    {
        public Opening() : base() { }
        public Opening(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            FromCGPNodeParameters(parameters);
        }
        public override int CGPInputCount
        {
            get
            {
                return 2;
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.RegionToRegion;
            }
        }

        public override HObject Execute(HObject input)
        {
            using (var str = StructureElement.Generate(StructElement, A, B))
            {
                HOperatorSet.Opening(input, str, out output);
                return output;
            }
        }
        public StructElementTypes StructElement { get; set; }

        public float A { get; set; } = 5;
        public float B { get; set; } = 5;

        public override void FromCGPNodeParameters(float[] parameters)
        {
            StructElement = (StructElementTypes)parameters[0];
            A = parameters[1];
            B = parameters[2];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)StructElement, A, B };
        }


        public override List<float>[] CGPParameterBounds
        {
            get
            {
                return new List<float>[] {
                    StructureElement.GetParameterBounds(),
                    StructureElement.GetParameterBoundsA(),
                    StructureElement.GetParameterBoundsB()
                };

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
            var opening = string.Format("opening({0}, {1}, {2})", Children.First().OutputVariableName,
                structName, OutputVariableName);

            lines.Add(structString);
            lines.Add(opening);

            return lines;
        }
    }
}
