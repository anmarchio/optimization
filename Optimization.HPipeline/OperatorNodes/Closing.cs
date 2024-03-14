using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using HalconDotNet;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class Closing : HalconOperatorNode
    {
        private Closing(Closing copy) : base(copy)
        {

        }

        public Closing() :base()
        {
            A = 5; B = 5; StructElementType = StructElementTypes.Rectangle; 
        }

        public Closing(HalconOperatorNode child, StructElementTypes structType, float a, float b) : base(new List<HalconOperatorNode>() { child })
        {
            A = a; B = b;
            StructElementType = structType;
        }

        public Closing(IList<HalconOperatorNode> children, StructElementTypes structType, float a, float b) : base(children)
        {
            A = a; B = b;
            StructElementType = structType;

        }

        public Closing(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            FromCGPNodeParameters(parameters);
        }


        public object Execute(object input)
        {
            var i = input as HObject;
            return Execute(input);
        }

        public override HObject Execute(HObject input)
        {
            using (var str = StructElement)
            {
                HOperatorSet.Closing(input, str, out output);
                return output;
            }
        }


        public float A { get; set; } = 5;

        public float B { get; set; } = 5;

        public float C { get; set; } = 0;

        public StructElementTypes StructElementType { get; set; }

        public override void DisposeOutput()
        {
            if (output == null) return;
            output.Dispose();
            output = null;
        }


        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)StructElementType, A, B, C };
        }

     
        public override void FromCGPNodeParameters(float[] parameters)
        {
            StructElementType = (StructElementTypes)parameters[0];
            A = parameters[1];
            B = parameters[2];
            C = parameters[3];
        }

    
        [XmlIgnore]
        public HObject StructElement
        {
            get
            {
                return StructureElement.Generate(StructElementType, A, B, C);
            }
        }


        public override List<float>[] CGPParameterBounds
        {
            get
            {
                return new List<float>[]
                {
                    StructureElement.GetParameterBounds(),
                    StructureElement.GetParameterBoundsA(),
                    StructureElement.GetParameterBoundsB(),
                    StructureElement.GetParameterBoundsC()
                };
            }
        }
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return DecodingMap.closing;
            }
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
                return OperatorType.RegionToRegion;
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

            var structName = StructElementType.ToString() + NodeID.ToString();
            var structString = StructureElement.GenerateHalconString(StructElementType, structName, A, B);
            var closingString = string.Format("closing ({0}, {1}, {2})", Children.First().OutputVariableName, structName, OutputVariableName);

            lines.Add(structString);
            lines.Add(closingString);

            return lines;        }
    }
}
