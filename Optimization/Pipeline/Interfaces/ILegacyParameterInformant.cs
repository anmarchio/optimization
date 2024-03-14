using System;

namespace Optimization.Pipeline.Interfaces
{
    public interface ILegacyParameterInformant<TIn, TParam, TOut> : IParameterInformant
    {
        Func<TIn, TParam, TOut> EvaluationFunction { get; }
    }
}
