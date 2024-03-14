using System.Collections.Generic;
using Optimization.Pipeline.Interfaces;
using Optimization.Serialization.Interfaces;

namespace Optimization.CartesianGeneticProgramming.Interfaces
{
    /// <summary>
    /// Defines the necessary dictionaries for creation of an operator map (a Cartesian grid) like the operators, input bounds and parameter bounds.
    /// </summary>
    public interface IOperatorMap : ISupportsSerialization, IOperatorEncoder
    {
        /// <summary>
        /// A set of all Operator genes (must be unique)
        /// </summary>
        HashSet<float> OperatorIdentifiers { get; }

        /// <summary>
        /// The valid list of operators for each column
        /// </summary>
        Dictionary<int, List<float>> InitializeOperatorBounds(CGPConfiguration configuration);

        /*
        /// <summary>
        /// Contains all input bounds, i.e. a mapping from a column index [0, n) to valid inputs. To be computed from levels-back
        /// </summary>
        public static Dictionary<int, Tuple<float, float>> InputBounds;*/

        Dictionary<float, List<float>[]> ParameterBounds { get; }

        /// <summary>
        /// Specifies which operator (delegate) can have how many inputs; to be used when decoding as to avoid innecessarily bloated trees.
        /// </summary>
        Dictionary<float, int> OperatorInputCount { get; }


        /// <summary>
        /// Lists for each column (int key) the set of node names that may be used as input.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Dictionary<int, Dictionary<float, List<float>[]>> ComputeInputBounds(CGPConfiguration parameters);


        /// <summary>
        /// This is necessary in order to initialize CGPConfiguration depend Properties, as a CGPConfiguration demands a IOperatorMap be passed to its constructor.
        /// </summary>
        /// <param name="parameters"></param>
        void Initialize(CGPConfiguration parameters);


        /// <summary>
        /// This is used by Logging functions to make the printed grid more readible.
        /// </summary>
        Dictionary<float, string> PrintingMap { get; }


        List<float> ProgramOutputBounds { get; }



        bool IsInitialized { get; set; }
    }
}
