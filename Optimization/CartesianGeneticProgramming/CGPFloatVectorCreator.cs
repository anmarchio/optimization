using System;
using Optimization.EvolutionStrategy.Creators;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.CartesianGeneticProgramming
{   /// <summary>
    /// Creates randomly initialized CGP-vectors (depending on the CGPConfiguration) [the operators themselves are also randomly initialized]; may also be used to return the status quo solution embedded in a 10x11 grid (set CGPConfiguration accordingly)
    /// or to load a previously evolved vector from a text file.
    /// </summary>
    [Serializable]
    public class CGPFloatVectorCreator : Creator, ICopyable
    {

        private CGPConfiguration parameters;
        public CGPFloatVectorCreator(IRandom random, CGPConfiguration configuration) : base(random, configuration.Length)
        {
            this.parameters = configuration;
        }

        public CGPFloatVectorCreator(IRandom random, CGPConfiguration configuration, FloatVector standardSolution) : base(random, configuration.Length)
        {
            this.parameters = configuration;
            StandardSolution = standardSolution;
        }


        public FloatVector StandardSolution { get; set; }

     
        public static FloatVector Apply(IRandom random, CGPConfiguration parameters)
        {
            var inputBounds = parameters.InputBounds;
            var operatorSet = parameters.OperatorBounds;
            var parameterBounds = parameters.ParameterBounds;

            var vector = new FloatVector(parameters.Length);

            // initialize each node
            for (var node = 0; node < parameters.NodesCount; node++)
            {
                var column = parameters.ColumnOf(node);
                var nodeIndex = parameters.NodeIndex(node); //node * parameters.NodeLength + parameters.InputsCount;			//leo's variant -.- kinda the stuff i wanted to avoid
                //var nodeIndex = parameters.NodeIndex(node); //node * parameters.NodeLength + parameters.ProgramInputsCount;		//ralf's variant
                
                // initialize operators: retrieves appropriate operatorset for each column and selects a random one
                var op = random.SelectRandom(operatorSet[column]);
                vector[parameters.OperatorIndex(node)] = op;

                // initialize inputs: similar to the above
                for (var j = 0; j < parameters.InputCountOfOperator(op); j++)
                {
                    var inBounds = inputBounds[column];
                    vector[nodeIndex + j] = random.SelectRandom(inBounds[op][j]);
                }

                // initialize parameters
                var parameterIndex = parameters.ParameterIndex(node);
                var paramBounds = parameterBounds[op];
                for (var j = 0; j < paramBounds.Length; j++)
                {
                    vector[parameterIndex + j] = random.SelectRandom(paramBounds[j]);
                }
            }

            // initialize outputs
            for (var i = parameters.Length - parameters.OutputsCount; i < parameters.Length; i++)
            {
                vector[i] = random.SelectRandom(parameters.ProgramOutputBounds);
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
            return new CGPFloatVectorCreator(Random, parameters, StandardSolution);
        }

        public override ICopyable Copy(IRandom rand)
        {
            return new CGPFloatVectorCreator(rand, parameters);
        }
    }
}
