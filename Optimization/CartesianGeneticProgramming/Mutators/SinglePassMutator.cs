using System;
using Optimization.EvolutionStrategy;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Mutators;

namespace Optimization.CartesianGeneticProgramming.Mutators
{
    public class SinglePassMutator : Mutator, IAdaptiveMutator
    {
        public SinglePassMutator(IRandom random, CGPConfiguration configuration) : base(random)
        {
            Configuration = configuration;
            AdaptationFunction = (int x) => { return 0.3 + Math.Min(x * 0.1, 0.6); };
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="random"></param>
        /// <param name="configuration"></param>
        /// <param name="calculateProbability">function that accepts a number of generations without improvement (int) and returns an appropriate mutation probability [0, 1]</param>
        public SinglePassMutator(IRandom random, CGPConfiguration configuration, Func<int, double> calculateProbability) : base(random)
        {
            Configuration = configuration;
            AdaptationFunction = calculateProbability;
        }

        public Func<int, double> AdaptationFunction { get; set; }

        public double StandardDeviation
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public double MutationProbability
        {
            get; set;
        } = 0.3;

        public CGPConfiguration Configuration { get; set; }

        public void Adapt(int generationsWithoutImprovement)
        {
            MutationProbability = AdaptationFunction.Invoke(generationsWithoutImprovement);

            if (MutationProbability < 0 || MutationProbability > 1) throw new ArgumentException(MutationProbability + " is not a valid probability [0,1]");
        }

        /// <summary>
        /// Not yet implemented
        /// </summary>
        /// <returns></returns>
        public override ICopyable Copy()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a copy with a specific random object for recreation of behaviour
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        public override ICopyable Copy(IRandom rand)
        {
            return new SinglePassMutator(rand, Configuration);
        }

        public override IIndividual Mutate(IIndividual individual)
        {
            var vec = individual.FloatVector.Copy() as FloatVector;

            for(int node = 0; node < Configuration.NodesCount;  node++)
            {
                int nodeIdx = Configuration.NodeIndex(node);
                int operatorIdx = Configuration.OperatorIndex(node);
                int column = Configuration.ColumnOf(node);
                
                // Only mutate operator and input in case that SearchSpaceType is Full
                // mutate operator
                bool mutateOperator = Random.NextDouble() < MutationProbability;
                if (mutateOperator && Configuration.SearchSpace == SearchSpaceType.Full)
                    vec[operatorIdx] =
                        Random.SelectRandom(Configuration.OperatorBounds[Configuration.ColumnOf(node)]);

                float op = vec[operatorIdx];

                // mutate input
                // if we mutated the operator we need to mutate the inputs as well to ensure that we get valid inputs for the mutated operator
                for (int i = 0; i < Configuration.InputCountOfOperator(op); i++)
                    if ((mutateOperator || Random.NextDouble() < MutationProbability) && Configuration.SearchSpace == SearchSpaceType.Full)
                        vec[nodeIdx + i] = Random.SelectRandom(Configuration.InputBounds[column][op][i]);

                
                // mutate or adjust parameters (in case the operator was changed)
                // not checking anything here corresponds to SearchSpaceType.Parameters; in case we add new search space types this has to be rewritten
                int parameterIdx = Configuration.ParameterIndex(node);
                for(int i = 0; i < Configuration.ParameterBounds[op].Length; i++)
                    if (mutateOperator || Random.NextDouble() < MutationProbability)
                        vec[parameterIdx + i] = Random.SelectRandom(Configuration.ParameterBounds[op][i]);

            }

            // mutate outputs
            for (int i = 0; i < Configuration.OutputsCount; i++)
                if (Random.NextDouble() < MutationProbability)
                    vec[vec.Length - i - 1] = Random.SelectRandom(Configuration.ProgramOutputBounds);//Random.SelectRandom(Configuration.InputBounds[Configuration.ColumnCount - 1]);

            return vec;
        }
    }
}
