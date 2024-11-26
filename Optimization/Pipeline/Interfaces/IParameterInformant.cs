using System.Collections.Generic;

namespace Optimization.HalconPipeline.Interfaces
{
    public interface IParameterInformant : INode
    {

        /// <summary>
        /// Enumerates all parameter combinations
        /// </summary>
        /// <returns>Iterator for all parameter combinations</returns>
        IEnumerable<float[]> EnumerateParameters();
     

        /// <summary>
        /// Must return the number of parameters encoded in cgp (i.e. number of parameter genes)
        /// </summary>
        int CGPParameterCount { get; }


        /// <summary>
        /// Return all paramters as an array of parameter genes. If an operator has no parameters use new float[0].
        /// </summary>
        /// <returns></returns>
        float[] ToCGPNodeParameters();

        void FromCGPNodeParameters(float[] parameters);

        List<float>[] CGPParameterBounds { get; }

        OperatorType OperatorType { get; }
        int CGPInputCount { get; }

        bool IsOrOperatorType(OperatorType t);

    }
}
