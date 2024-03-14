using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.CartesianGeneticProgramming;
using Optimization.Fitness.Interfaces;
using Optimization.Fitness.OperatorMaps;
using Optimization.Pipeline.Interfaces;

namespace Optimization.HPipeline.Fitness.OperatorMaps
{

    public class LegacyHalconOperatorMap : ModularLegacyOperatorMap<HObject[], HTuple[], HObject[]>
    {
        public LegacyHalconOperatorMap(List<HalconOperatorNode> operators)
            : base(operators.Union(new List<HalconOperatorNode>() { new HalconInputNode() })
                  .Cast<ILegacyParameterInformant<HObject[], HTuple[], HObject[]>>().ToList())
        {
        }

        public override void Initialize(CGPConfiguration configuration)
        {
            base.Initialize(configuration);
            foreach (var inputId in configuration.ProgramInputIdentifiers)
                InputOperatorTypeMap.Add(inputId, typeof(HalconInputNode));
        }
    }


    [Serializable]
    public class ModularLegacyOperatorMap<TIn, TParam, TOut> : ModularOperatorMap, IDecodingMap<TIn, TParam, TOut>
    {
        public ModularLegacyOperatorMap(List<ILegacyParameterInformant<TIn, TParam, TOut>> operators) : base(operators.Cast<IParameterInformant>().ToList())
        {
            Initialize(operators);
        }

        public ModularLegacyOperatorMap(params ILegacyParameterInformant<TIn, TParam, TOut>[] operators) : base(operators.Cast<IParameterInformant>().ToList())
        {
            Initialize(operators.ToList());
        }


        protected void Initialize(List<ILegacyParameterInformant<TIn, TParam, TOut>> operators)
        {
            operators = GetUniqueOperators(operators.Cast<IParameterInformant>().ToList())
                .Cast<ILegacyParameterInformant<TIn, TParam, TOut>>().ToList();  // this is all really ugly, but since the legacy maps should not be used in the future, it does not matter
            
            map = new Dictionary<float, Func<TIn, TParam, TOut>>(); 
            for (int i = 0; i < OperatorIdentifiers.Count; i++)
            {
                if (operators[i].IsInputNode) continue;        
                map.Add(i, operators[i].EvaluationFunction);
            }
        }
        
        private Dictionary<float, Func<TIn, TParam, TOut>> map;

        public Func<TIn, TParam, TOut> this[float i]
        {
            get
            {
                return map[i];
            }
        }


    }
}
