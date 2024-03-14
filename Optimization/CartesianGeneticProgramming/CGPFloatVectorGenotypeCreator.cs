using System;
using System.Linq;
using Optimization.EvolutionStrategy.Creators;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.CartesianGeneticProgramming
{
    /// <summary>
    /// Creates a FloatVector encoded genotype of all the nodes of the Carthesian grid (depending on the CGPConfiguration), i.e. every suitable operator is present in each column; 
    /// may also be used to return the status quo solution embedded in a 10x11 grid (set CGPConfiguration accordingly)
    /// or to load a previously evolved vector from a text file.
    /// </summary>
    [Serializable]
    public class CGPFloatVectorGenotypeCreator : Creator
    {
        private CGPConfiguration parameters;
        public FloatVector StandardSolution = null;

        public CGPFloatVectorGenotypeCreator(IRandom random, CGPConfiguration configuration) : base(random, configuration.Length)
        {
            this.parameters = configuration;
        }

        public CGPFloatVectorGenotypeCreator(IRandom random, CGPConfiguration configuration, FloatVector standardSolution) : base(random, configuration.Length)
        {
            this.parameters = configuration;
            StandardSolution = standardSolution;
        }

       
        /// <summary>
        /// Creates a new FloatVector. Attention: still dependant on equally filled columns
        /// </summary>
        /// <param name="random"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static FloatVector Apply(IRandom random, CGPConfiguration parameters)
        {
            var inputBounds = parameters.InputBounds;
            var operatorSet = parameters.OperatorBounds;
            var parameterBounds = parameters.ParameterBounds;

            var vector = new FloatVector(parameters.Length);

            // initialize each node
            //alternative: iterate over columns and increment node 
            for (var node = 0; node < parameters.NodesCount; node++)
            {
                var column = parameters.ColumnOf(node);
                var operatorsincolumn = operatorSet[column].Count;

                // initialize operators along with their inputs and parameters: retrieves appropriate operatorset for each column
                for (int l = 0; l < operatorsincolumn; l++)
                {
                    var nodeIndex = parameters.NodeIndex(node); //node * parameters.NodeLength;

                    var op = operatorSet[column].ElementAt(l);
                    vector[parameters.OperatorIndex(node)] = op;

                    // initialize inputs with random valid values
                    for (var j = 0; j < parameters.InputCountOfOperator(op); j++)
                    {
                        var inBounds = inputBounds[column];
                        vector[nodeIndex + j] = random.SelectRandom(inBounds[op][j]);//inBounds.ElementAt(random.Next(inBounds.Count));
                    }

                    // initialize parameters with random valid values
                    var parameterIndex = parameters.ParameterIndex(node);
                    var paramBounds = parameterBounds[op];
                    for (var j = 0; j < paramBounds.Length; j++)
                    {
                        vector[parameterIndex + j] = paramBounds[j].ElementAt(random.Next(paramBounds[j].Count));
                    }

                    //necessary to not have a double increment of node when the inner loop terminates
                    if (l != operatorsincolumn - 1)
                    {
                        node++;
                    }
                }
            }

            var lastColumn = parameters.ColumnCount;
            // initialize outputs
            for (var i = parameters.Length - parameters.OutputsCount; i < parameters.Length; i++)
            {
                vector[i] = random.SelectRandom(parameters.ProgramOutputBounds);// inputBounds[lastColumn].ElementAt(random.Next(inputBounds[lastColumn].Count));
            }

            //Logger.PrintGrid(vector, parameters, "grids.txt", true, "before", true);

            return vector;
        }

        public override IIndividual Create()
        {
            if (StandardSolution != null) return StandardSolution;
            return Apply(Random, parameters);
        }

        public override ICopyable Copy()
        {
            return new CGPFloatVectorGenotypeCreator(Random, parameters);
        }

        public override ICopyable Copy(IRandom rand)
        {
            throw new NotImplementedException();
        }
    }
}
