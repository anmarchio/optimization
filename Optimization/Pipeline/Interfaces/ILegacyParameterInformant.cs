using System;

namespace Optimization.HalconPipeline.Interfaces
{
    public interface ILegacyParameterInformant<TIn, TParam, TOut> : IParameterInformant
    {
        Func<TIn, TParam, TOut> EvaluationFunction { get; }
    }
}
