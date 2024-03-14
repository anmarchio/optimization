using System;
using System.Linq;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Mutators;

namespace Optimization.CartesianGeneticProgramming.Mutators
{
    /// <summary>
    /// Manipulates each dimension in the vector with the mutation strength given
    /// in the sigma parameter vector and rounds the result to the next feasible value.
    /// </summary>
    [Serializable]
    public class CGPMutator : Mutator
    {
        private const int InputMutation = 0, OperatorMutation = 1, ParameterMutation = 2;

        private CGPConfiguration configuration;
        private int[] mutationProbability;
        private CGPDecoder Decoder;

     
        public CGPMutator(IRandom random, CGPConfiguration configuration, CGPDecoder decoder, int[] mutationProbability) : base(random)
        {
            this.configuration = configuration;
            this.mutationProbability = mutationProbability;
            this.Random = random;
            Decoder = decoder;
        }


        /// <summary>
        /// Randomly mutates exactly one active node (CGPStandardMutator mutates until an active nodes is mutated)!
        /// <paramref name="vector"/> and rounds the result to the next feasible value.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the sigma vector is null or of length 0.</exception>
        /// <param name="random">A random number generator.</param>
        /// <param name="vector">The integer vector to manipulate.</param>
        /// <param name="gridInfo">The CGPConfiguration used.</param>
        /// <param name="operatorset">The operator set used in the current CGP</param>
        /// <param name="parameterBounds"> the corresponding parameter bounds for the operators in operatorset</param>
        /// <param name="probability"> the probabilty of a node's input, operator or parameters being mutated</param>
        /// <returns>The manipulated integer vector.</returns>
        public void Apply(IRandom random, FloatVector vector, CGPConfiguration gridInfo, int[] probability)
        {
            if (vector.Length != gridInfo.Length) throw new Exception("An individual's length must match the CGPConfiguration.Length °_°");

            var activeNodes = Decoder.ActiveNodes(vector, excludeProgramInputs:true);
            var maxNode = gridInfo.GridSize;

            // determine which node to mutate ("node" refers to one of the nodes of the actual grid, ignoring the program input nodes)
            var node = activeNodes.ElementAt(random.Next(0, activeNodes.Count));
            var nodeOffset = gridInfo.NodeIndex(node); //node * gridInfo.NodeLength + gridInfo.ProgramInputsCount; // therefore: add programinputs.count to nodeoffset in grid in order to determine offset in vector

            // compute which parts of the node to mutate

            var func = vector[gridInfo.OperatorIndex(node)];
            //if (func != Function.threshold && func != Function.sobelAmp)
            //{
            var column = (int)node / (gridInfo.RowCount);
            double rd = random.Next(0, 100);
            int offset;
            if (rd < probability[InputMutation])
            {
                // compute which input connection to mutate
                offset = random.Next(0, gridInfo.InputCountOfOperator(vector[gridInfo.OperatorIndex(node)]));
                vector[nodeOffset + offset] = random.SelectRandom(gridInfo.InputBounds[column][func][offset]);
            }

            rd = random.Next(0, 100);
            if (rd < probability[OperatorMutation])
            {
                offset = nodeOffset + gridInfo.InputCount;
                // determine the column of the current node                
                // get new random operator value from the list of valid operators (may actually be the operator itself)
                vector[offset] = gridInfo.OperatorBounds[column].ElementAt(random.Next(0, gridInfo.OperatorBounds[column].Count));

                // to do: retrieve and set valid parameters
                var f = vector[offset];
                for (int i = 1; i <= gridInfo.ParameterBounds[f].Length; i++)
                {
                    vector[offset + i] = gridInfo.ParameterBounds[func][i - 1].ElementAt(random.Next(0, gridInfo.ParameterBounds[func][i - 1].Count()));
                }
            }

            rd = random.Next(0, 100);
            if (rd < probability[ParameterMutation])
            {

                if (gridInfo.ParameterBounds[func].Count() > 0)
                {
                    offset = random.Next(gridInfo.ParameterBounds[func].Count());
                    vector[gridInfo.ParameterIndex(node) + offset] = gridInfo.ParameterBounds[func][offset].ElementAt(random.Next(0, gridInfo.ParameterBounds[func][offset].Count()));
                }
            }
        }

        /// <summary>
        /// Retrieves the bounds and forwards the call to the static Apply method.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        /// <param name="vector">The vector of integer values that is manipulated.</param>
        protected void Manipulate(IRandom random, FloatVector vector)
        {
            Apply(random, vector, configuration, mutationProbability);
        }

        public override IIndividual Mutate(IIndividual individual)
        {
            var copy = individual.Copy() as IIndividual;
            Apply(Random, copy.FloatVector, configuration, mutationProbability);
            return copy;
        }

        /// <summary>
        /// Creates a copy
        /// </summary>
        /// <returns></returns>
        public override ICopyable Copy()
        {
            return new CGPMutator(Random, configuration, Decoder, mutationProbability);
        }

        /// <summary>
        /// Creates a Copy with a newly created Random object, allow recreation with specific random value
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        public override ICopyable Copy(IRandom rand)
        {
            return new CGPMutator(rand, configuration, Decoder,mutationProbability);
        }
    }
}


