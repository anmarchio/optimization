using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.Pipeline;
using Optimization.Pipeline.Interfaces;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class Union2 : HalconOperatorNode
    {
        private Union2(Union2 copy) : base(copy)
        {

        }
        public Union2() : base()
        {
            Initialize();
        }

        public Union2(params HalconOperatorNode[] children) : base(children)
        {
            Initialize();
        }

        public Union2(IList<HalconOperatorNode> children, float[] parameters) : base(children)
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
                    cgpParameterBounds = operatorMap.ParameterBounds[2];
                }
                return cgpParameterBounds;
            }
        }
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return DecodingMap.union2;
            }
        }

        public override void DisposeOutput()
        {
            if (output == null) return;
            output.Dispose();
            output = null;
        }

        public override HObject Execute()
        {
            HObject concatenation = null;
            try
            {
                var input = Children.First() as IOutputNode<HObject>;
                concatenation = input.Output.Clone();
                for (int i = 1; i < Children.Count; i++)
                {
                    input = Children[i] as IOutputNode<HObject>;
                    concatenation = concatenation.ConcatObj(input.Output);
                }
                return Execute(concatenation);
            }catch(Exception ex)
            {
                throw new OperatorException(this, ex);
            }
            finally
            {
                if (concatenation != null) concatenation.Dispose();
            }
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

        /// <summary>
        /// Authors: mara
        /// Function to convert FunctionCall to halcon code for export into a hdev file
        /// </summary>
        /// <returns></returns>
        public override List<string> HalconFunctionCall()
        {
            List<string> lines = new List<string>();
            lines.Add(string.Format("gen_empty_obj ({0})", OutputVariableName));                
            for (int i = 0; i < Children.Count; i++)
                lines.Add(string.Format("concat_obj ({0}, {1}, {2})", OutputVariableName, Children[i].OutputVariableName, OutputVariableName));
            lines.Add(string.Format("union1 ({0}, {1})", OutputVariableName, OutputVariableName));
            return lines;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Initialize();
        }
    }
}
