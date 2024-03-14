using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class Connection : HalconOperatorNode
    {
        public Connection() : base()
        {
            Neighborhood = Neighborhood4;
            Initialize();
        }

        private Connection(Connection copy) : base(copy)
        {

        }

        public Connection(HalconOperatorNode child, int neighborhood = Neighborhood8) : base(new List<HalconOperatorNode>() { child })
        {
            Neighborhood = neighborhood;
            Initialize();
        }


        public Connection(IList<HalconOperatorNode> children, int neighborhood = Neighborhood8) : base(children)
        {
            Neighborhood = neighborhood;
            Initialize();

        }

        public Connection(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            FromCGPNodeParameters(parameters);
        }

      
        private void Initialize()
        {
        }

        public const int Neighborhood4 = 4;
        public const int Neighborhood8 = 8;

        public int Neighborhood { get; set; }


        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if (cgpParameterBounds == null)
                {
                    var operatorMap = new OperatorMap();
                    cgpParameterBounds = operatorMap.ParameterBounds[6];
                }
                return cgpParameterBounds;
            }
        }
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return DecodingMap.connection;
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
            //HOperatorSet.SetSystem("neighborhood", Neighborhood);  -- this can cause very weird behavior
            HOperatorSet.Connection(input, out output);
            return output;
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Neighborhood};
        }

    
        public override void FromCGPNodeParameters(float[] parameters)
        {
            Neighborhood = (int)parameters[0];
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
            //lines.Add($"set system ('{"neigborhood"}', {Neighborhood.ToString()})"):
            lines.Add($"connection ({Children.First().OutputVariableName}, {OutputVariableName})");
            return lines;
        }
    }
}
