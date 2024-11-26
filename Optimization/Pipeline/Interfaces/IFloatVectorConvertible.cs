using Optimization.CartesianGeneticProgramming;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.HalconPipeline.Interfaces
{
    public interface IFloatVectorConvertible
    {
        FloatVector ToCGPEncoding(IOperatorEncoder encoder, CGPConfiguration config, IRandom random);

        void ToCGPEncoding(IOperatorEncoder operatorEncoder, IRandom random, out FloatVector vector, out CGPConfiguration configuration, int forceLevelsBack = 1);
    }
}
