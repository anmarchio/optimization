using System;
using System.Collections.Generic;
using System.Linq;
using Optimization.CartesianGeneticProgramming;
using Optimization.Fitness.OperatorMaps;
using Optimization.Pipeline;
using Optimization.Pipeline.Interfaces;

namespace Optimization.HPipeline.Fitness.OperatorMaps
{
    [Serializable]
    public class HalconOperatorMap : ModularOperatorMap
    {
        public HalconOperatorMap(List<HalconOperatorNode> operators, DependencyTree dependencyTree=null)
            : base(operators.Union(new List<HalconOperatorNode>() { new HalconInputNode()}).Cast<IParameterInformant>().ToList(), dependencyTree)
        {
        }

        public HalconOperatorMap(params HalconOperatorNode[] operators) : base(operators.Union(new List<HalconOperatorNode>() { new HalconInputNode() }).ToArray())
        {
        }

        public override void Initialize(CGPConfiguration configuration)
        {
            base.Initialize(configuration);
            foreach (var inputId in configuration.ProgramInputIdentifiers)
                InputOperatorTypeMap.Add(inputId, typeof(HalconInputNode));
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var tmp = obj as HalconOperatorMap;
            if (tmp == null) return false;
            if (OperatorIdentifiers.Count != tmp.OperatorIdentifiers.Count) return false;
            foreach(var op in OperatorIdentifiers)
            {
                if (tmp.Decode(op).Name != Decode(op).Name) return false;
            }
            return true;
        }

        public CGPConfiguration ToCGPConfiguration(int rows, int programInputCount, int programOutputCount)
        {
            var cgp = new CGPConfiguration(rows, Dependencies.Nodes.Count, 1, OperatorInputCount.Max(x => x.Value), ParameterBounds.Max(x => x.Value.Length), this, programInputCount, programOutputCount);
            return cgp;
        }

        public override int GetHashCode()
        {
            return OperatorIdentifiers.Count;
        }
    }
}
