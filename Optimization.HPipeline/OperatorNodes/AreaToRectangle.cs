using System;
using System.Collections.Generic;
using HalconDotNet;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class AreaToRectangle: HalconOperatorNode
    {
        public AreaToRectangle() { }
        public AreaToRectangle(HalconOperatorNode child) : base(child)
        {
            Initialize();
        }
        
        public AreaToRectangle(IList<HalconOperatorNode> children) : base(children)
        {
            Initialize();
        }

        public AreaToRectangle(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            Initialize();
        }

    
        private void Initialize()
        {
            cgpParameterBounds = new List<float>[0];
        }

        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if(cgpParameterBounds == null)
                {
                    cgpParameterBounds = new List<float>[0];
                }
                return cgpParameterBounds;
            }
        }
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return areatorectangle;
            }
        }

        private HObject[] areatorectangle(HObject[] arg1, HTuple[] arg2)
        {
            HObject[] objects = new HObject[arg1.Length];
            CGPOperatorSet.AreaToRectangle(arg1[0], out objects[0]);
            return objects;
        }

        public override void DisposeOutput()
        {
            if (output == null) return;
            output.Dispose();
            output = null;
        }

        public override HObject Execute(HObject input)
        {
            CGPOperatorSet.AreaToRectangle(input, out output);
            return output;
        }



        public override float[] ToCGPNodeParameters()
        {
            return new float[] { };
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            
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

        public override List<string> HalconFunctionCall()
        {
            throw new NotImplementedException();
        }
    }
}