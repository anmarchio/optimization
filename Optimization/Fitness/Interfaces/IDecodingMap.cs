using System;

namespace Optimization.Fitness.Interfaces
{
    public interface IDecodingMap<TIn, TParam, TOut>
    {
        Func<TIn, TParam, TOut> this[float i]
        {
            get;
        }
    }
}
