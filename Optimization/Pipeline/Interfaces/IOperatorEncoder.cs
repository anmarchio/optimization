using System;
using Optimization.CartesianGeneticProgramming.Interfaces;

namespace Optimization.HalconPipeline.Interfaces
{
    public interface IOperatorEncoder
    {
        IOperatorMap OperatorMap { get; }

        float Encode(Node op);

        Type Decode(float op);

        DependencyTree Dependencies { get;}
    }
}
