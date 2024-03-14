using System;
using System.Linq;
using Optimization.EvolutionStrategy;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Mutators;

namespace Optimization.CartesianGeneticProgramming.Mutators
{
    /// <summary>
    /// CGPMutator used for normal CGP mutations. Optionally using accumulate (mutate until an active gene is mutated) or single (immediately mutate an active gene).
    /// </summary>
    /// <remarks>
    /// When this mutator is used in combination with the mode "single", it is equivalent to using the CGPMutator.
    /// </remarks>
    [Serializable]
    public class CGPStandardMutator : Mutator
    {

        /// <summary>
        /// SingleActive refers to mutating nodes _until_ an active one is hit
        /// </summary>
        /// <param name="random"></param>
        /// <param name="configuration"></param>
        /// <param name="decoder"></param>
        /// <param name="singleActive">if true, SingleActive from Goldman Paper is used. Else just one active node is mutated (ain't it confusing?)</param>
        public CGPStandardMutator(IRandom random, CGPConfiguration configuration, CGPDecoder decoder, bool singleActive) : base(random)
        {
            SingleActive = singleActive;
            Configuration = configuration;
            Decoder = decoder;
        }

        private bool SingleActive
        {
            get; set;
        }
        private CGPConfiguration Configuration
        {
            get; set;
        }

        private CGPDecoder Decoder { get; set; }

     

        /// <summary>
        /// If "singleActive" is false, then only one active node is mutated. Otherwise a random nodes are mutated until an active one was mutated.
        /// <paramref name="vector"/> 
        /// </summary>
        /// <param name="random">A random number generator.</param>
        /// <param name="vector">The integer vector to manipulate.</param>#
        /// <returns>The manipulated integer vector.</returns>
        public void Apply(IRandom random, FloatVector vector, CGPConfiguration gridInfo, bool singleActive)
        {
            // workaround for dysfunctional CGPIntegerVectorCreator integration into algorithm (dunno why it does not terminate properly when used)
            if (vector.Length != gridInfo.Length) throw new Exception("°_°");

            var activeNodes = Decoder.ActiveNodes(vector, excludeProgramInputs:true);
            var parameterBounds = gridInfo.ParameterBounds;
            var operatorSet = gridInfo.OperatorBounds;
            var nodeWasActive = false;
            int maxNode = gridInfo.GridSize;
            
            // mutate until one active node was mutated - TO DO: GET SOURCE
            while (!nodeWasActive)
            {
                float node = -1;
                // determine which node to mutate ("node" refers to one of the nodes of the actual grid, ignoring the program input nodes)
                if (singleActive)
                    node = random.Next(maxNode);
                else
                    node = activeNodes.ElementAt(random.Next(activeNodes.Count));
                int nodeOffset = gridInfo.NodeIndex(node); // therefore: add programinputs.count to nodeoffset in grid in order to determine offset in vector

                // compute which parts of the node to mutate


                var func = vector[gridInfo.OperatorIndex(node)];

                // mutate random gene of node in case of SearchSpaceType.Full
                // mutate only parameter gene in case of SearchSpaceType.Parameters
                int offset = 0, parameterCount = parameterBounds[func].Count();
                switch (Configuration.SearchSpace)
                {
                    case SearchSpaceType.Full:
                        offset = random.Next(gridInfo.NodeLength);
                        break;
                    case SearchSpaceType.ParametersOnly:
                        offset = random.Next(gridInfo.InputCount + 1, gridInfo.NodeLength + 1);
                        break;
                }

                int column = gridInfo.ColumnOf(node);

                if (offset < gridInfo.InputCountOfOperator(func)) // input genes
                {
                    // compute which input connection to mutate
                    vector[nodeOffset + offset] = random.SelectRandom(gridInfo.InputBounds[column][func][offset]);
                }
                else if (offset == gridInfo.InputCount) // function gene
                {
                    offset = nodeOffset + gridInfo.InputCount;
                    // determine the column of the current node                
                    // get new random operator value from the list of valid operators (may actually be the operator itself)
                    vector[offset] = operatorSet[column].ElementAt(random.Next(0, gridInfo.OperatorBounds[column].Count));

                    // to do: retrieve and set valid parameters
                    var f = vector[offset];
                    for (int i = 1; i <= parameterBounds[f].Length; i++)
                    {
                        vector[offset + i] = random.SelectRandom(gridInfo.ParameterBounds[f][i - 1]);
                    }
                    var nodeIdx = gridInfo.NodeIndex(node);
                    for(int i = 0; i < gridInfo.InputBounds[column][func].Length; i++)
                    {
                        vector[nodeIdx + i] = random.SelectRandom(gridInfo.InputBounds[column][func][i]);
                    }
                }
                else
                {
                    if (parameterBounds[func].Count() > 0) // parameter genes
                    {
                        offset = random.Next(0, parameterBounds[func].Count());
                        vector[gridInfo.ParameterIndex(node) + offset] = random.SelectRandom(gridInfo.ParameterBounds[func][offset]);
                    }
                }
                // }
                if (activeNodes.Contains(node)) nodeWasActive = true;
            }
        }

        public void Apply(IRandom random, MultipleFloatVectorEncoding vector, CGPConfiguration gridInfo, bool singleActive)
        {
            // workaround for dysfunctional CGPIntegerVectorCreator integration into algorithm (dunno why it does not terminate properly when used)
            if (vector.Length() != gridInfo.Length) throw new Exception("°_°");

            var activeNodes = Decoder.ActiveNodes(vector);
            var parameterBounds = gridInfo.ParameterBounds;
            var operatorSet = gridInfo.OperatorBounds;
            var nodeWasActive = false;
            int maxNode = gridInfo.GridSize;

            // mutate until one active node was mutated - TO DO: GET SOURCE
            while (!nodeWasActive)
            {
                float node = -1;
                // determine which node to mutate ("node" refers to one of the nodes of the actual grid, ignoring the program input nodes)
                if (singleActive)
                    node = random.Next(maxNode);
                else
                    node = activeNodes.ElementAt(random.Next(activeNodes.Count));
                int nodeOffset = gridInfo.NodeIndex(node); // therefore: add programinputs.count to nodeoffset in grid in order to determine offset in vector
                var column = gridInfo.ColumnOf(node);

                // compute which parts of the node to mutate

                var func = vector[column, gridInfo.OperatorIndex(node)];

                // mutate random gene of node

                int offset = 0, parameterCount = parameterBounds[func].Count();
                offset = random.Next(gridInfo.NodeLength);

                if (offset < gridInfo.InputCount) // input genes
                {
                    // compute which input connection to mutate
                    vector[column, nodeOffset + offset] = random.SelectRandom(gridInfo.InputBounds[column][func][offset]); //gridInfo.InputBounds[column].ElementAt(random.Next(gridInfo.InputBounds[column].Count));
                }
                else
                {
                    if (parameterBounds[func].Count() > 0) // parameter genes
                    {
                        offset = random.Next(0, parameterBounds[func].Count());
                        vector[column, gridInfo.ParameterIndex(node) + offset] = gridInfo.ParameterBounds[func][offset].ElementAt(random.Next(0, gridInfo.ParameterBounds[func][offset].Count()));
                    }
                }
                if (activeNodes.Contains(node)) nodeWasActive = true;
            }
        }


        /// <summary>
        /// Retrieves the bounds and forwards the call to the static Apply method.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        /// <param name="vector">The vector of integer values that is manipulated.</param>

        public override IIndividual Mutate(IIndividual individual)
        {
            var copy = individual.Copy() as IIndividual;
            if (individual.GetType() == typeof(MultipleFloatVectorEncoding))
            {
                Apply(Random, copy.MultipleFloatVectorEncoding, Configuration, SingleActive);
            }
            else if (individual.GetType() == typeof(FloatVector))
            {
                Apply(Random, copy.FloatVector, Configuration, SingleActive);
            }
            return copy as IIndividual;
        }

        /// <summary>
        /// Creates a simple copy
        /// </summary>
        /// <returns></returns>
        public override ICopyable Copy()
        {
            return new CGPStandardMutator(Random, Configuration, Decoder, SingleActive);
        }

        /// <summary>
        /// Creates a copy with a specific random value, allows for recreation with a certain random object
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        public override ICopyable Copy(IRandom rand)
        {
            return new CGPStandardMutator(rand, Configuration, Decoder, SingleActive);
        }
    }
}
