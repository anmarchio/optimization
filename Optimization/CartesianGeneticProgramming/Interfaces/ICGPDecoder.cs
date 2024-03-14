using System.Collections.Generic;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.CartesianGeneticProgramming.Interfaces
{
    public interface ICGPDecoder
    {
        List<float> ActiveNodes(FloatVector vector, bool excludeProgramInputs=false);

        Dictionary<float, List<float>> ComputeExecutionTree(IIndividual vector, List<float> activeNodes = null, bool excludeProgramInputs = false);

        Dictionary<int, List<float>> ComputeColumnNodeMap(IIndividual vector, List<float> activeNodes = null, bool excludeProgramInputs = false);
    }
}
