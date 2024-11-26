using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class Union1 : HalconOperatorNode
    {
        private Union1(Union1 copy) : base(copy)
        {

        }
        public Union1() : base()
        {
            Initialize();
        }
        public Union1(HalconOperatorNode child) : base(new List<HalconOperatorNode>() { child })
        {
            Initialize();

        }

        public Union1(IList<HalconOperatorNode> children) : base(children)
        {
            Initialize();

        }

        public Union1(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            FromCGPNodeParameters(parameters);
        }


        private void Initialize()
        {
        }

        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if (cgpParameterBounds == null)
                {
                    var operatorMap = new OperatorMap();
                    cgpParameterBounds = operatorMap.ParameterBounds[3];
                }
                return cgpParameterBounds;
            }
        }

        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return DecodingMap.union1;
            }

        }

        public override void DisposeOutput()
        {
            if (output == null) return;
            output.Dispose();
            output = null;
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.Union1(input, out output);
            return output;
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[0];
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Initialize();
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
            lines.Add(string.Format("union1 ({0}, {1})", Children.First().OutputVariableName, OutputVariableName));
            return lines;
        }
    }
}
