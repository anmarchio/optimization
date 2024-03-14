using System.Linq;
using Optimization.EvolutionStrategy.Creators;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.CartesianGeneticProgramming
{
    /// <summary>
    /// Creates a FloatVector encoded genotype of all the nodes of the Carthesian grid (depending on the CGPConfiguration); MultipleFloatVector consists of column amount of float vectors (one for each column)
    /// instead of one vector for the whole genotype as with FloatVector encoding. Each suitable operator is present in each column.
    /// </summary>
    public class CGPMultipleFloatVectorCreator : Creator
    {
        private CGPConfiguration parameters;
        public MultipleFloatVectorEncoding StandardSolutionMultiple = null;

        public CGPMultipleFloatVectorCreator(IRandom random, CGPConfiguration configuration) : base(random, configuration.Length)
        {
            this.parameters = configuration;
        }

        public CGPMultipleFloatVectorCreator(IRandom random, CGPConfiguration configuration, MultipleFloatVectorEncoding standardSolution) : base(random, configuration.Length)
        {
            this.parameters = configuration;
            StandardSolutionMultiple = standardSolution;
        }

      
        public static MultipleFloatVectorEncoding Apply(IRandom random, CGPConfiguration parameters)
        {
            var inputBounds = parameters.InputBounds;
            var operatorSet = parameters.OperatorBounds;
            var parameterBounds = parameters.ParameterBounds;

            var vector = new float[parameters.ColumnCount][];

            for (var node = 0; node < parameters.NodesCount;)
            {
                var column = parameters.ColumnOf(node);

                var operators = operatorSet[column];
                if (vector[column] == null)
                {
                    vector[column] = new float[operators.Count() * parameters.NodeLength];
                }

                foreach (var op in operators)
                {
                    // initialize operators: retrieves appropriate operatorset for each column and places each one in the grid
                    vector[column][parameters.OperatorIndex(node)] = op;

                    // initialize inputs: similar to the above
                    var nodeIndex = parameters.NodeIndex(node); //node * parameters.NodeLength + parameters.ProgramInputsCount;
                    for (var j = 0; j < parameters.InputCountOfOperator(op); j++)
                    {
                        var inBounds = inputBounds[column];
                        vector[column][nodeIndex + j] = random.SelectRandom(inBounds[op][j]);// inBounds.ElementAt(random.Next(inBounds.Count));
                    }

                    // initialize parameters
                    var parameterIndex = parameters.ParameterIndex(node);
                    var paramBounds = parameterBounds[op];
                    for (var j = 0; j < paramBounds.Length; j++)
                    {
                        vector[column][parameterIndex + j] = paramBounds[j].ElementAt(random.Next(paramBounds[j].Count));
                    }
                    node++;
                }
            }

            // initialize outputs
            var lastColumn = parameters.ColumnCount - 1;
            if (parameters.useSelfAdaptiveMutation == true)  //store the self-adapting sigma after the output(s) -- be careful with CGPDecoder class since it could mistakenly interpret sigma as a program output
            {
                int i = 0;
                vector[lastColumn] = new float[parameters.OutputsCount + 1];
                for (i = 0; i < parameters.OutputsCount; i++)
                {
                    vector[lastColumn][i] = random.SelectRandom(parameters.ProgramOutputBounds);//inputBounds[lastColumn].ElementAt(random.Next(inputBounds[lastColumn].Count));
                }
                vector[lastColumn][i] = (float)2.5;      //sigma = 2.5 as default
            }
            else
            {
                vector[lastColumn] = new float[parameters.OutputsCount];
                for (var i = 0; i < parameters.OutputsCount; i++)
                {
                    vector[lastColumn][i] = random.SelectRandom(parameters.ProgramOutputBounds);//inputBounds[lastColumn].ElementAt(random.Next(inputBounds[lastColumn].Count));
                }
            }

            //Logger.PrintGrid(vector, parameters, "grids.txt", true, "before", true);

            return new MultipleFloatVectorEncoding(vector); ;
        }

        public override IIndividual Create()
        {
            if (StandardSolutionMultiple != null) return StandardSolutionMultiple;
            return Apply(Random, parameters);
        }

        public override ICopyable Copy()
        {
            return new CGPMultipleFloatVectorCreator(Random, parameters);
        }

        public override ICopyable Copy(IRandom rand)
        {
            throw new System.NotImplementedException();
        }
    }
}

