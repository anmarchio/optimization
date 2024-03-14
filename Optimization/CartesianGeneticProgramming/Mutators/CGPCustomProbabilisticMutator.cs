using System;
using System.Linq;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Mutators;

namespace Optimization.CartesianGeneticProgramming.Mutators
{
    /// <summary>
    /// Use this in conjunction with adaptive mutation only! #ES constructor!
    /// Main Idea: When sigma is low, mutate mainly parameters (local search). When sigma is high, input mutation rates increase (global search).
    /// Parameters are mutated as follows:
    /// - non-categorical parameters are mutated equal to the value drawn from the normal distribution
    /// - categorical parameters are mutated uniformly, mutation rate increases as sigma does
    /// Inputs are mutated as follows:
    /// - A value from the normal distribution is drawn. If that value is less than 5, no input mutation occurs. Otherwise the chance for the input mutation increases with sigma.
    /// </summary>
    [Serializable]
    public class CGPCustomProbabilisticMutator : AdaptiveMutator, ICopyable 
    {

        private CGPConfiguration configuration;
        private CGPDecoder Decoder;

      

        public CGPCustomProbabilisticMutator(IRandom random, CGPConfiguration configuration, CGPDecoder decoder, double startStdDev) : base(random, startStdDev)
        {
            this.configuration = configuration;
            Random = random;
            Decoder = decoder;
        }

        /// <summary>
        /// Returns whether a node should be mutated or not based on the random value of the normal distribution being greater than the upperBound.
        /// </summary>
        /// <remarks>
        /// Recommended value(s) of stdDev: [1..4]
        /// Recommended value(s) of upperBound: 2
        /// This results in a mutation probabilities between 2.2% (with stdDev=1, upperBound=2) and 31% (with stdDev=4, upperBound=2).
        /// http://onlinestatbook.com/2/calculators/normal_dist.html
        /// </remarks>
        /// <param name="random">Random normal distribution number generator</param>
        /// <param name="stdDev">Standard deviation of the normal distribution</param>
        /// <param name="upperBound">All generated values greater than this upperBound result in a mutation.</param>
        /// <returns>True = Mutation; False = No Mutation </returns>
        private bool MutateParameter(IRandom random, double stdDev = 1, double upperBound = 2)
        {
            if (random.NextGaussian(sigma: stdDev) > upperBound)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns whether a node should be mutated or not based on the random value of the normal distribution being greater than the upperBound.
        /// </summary>
        /// <remarks>
        /// Recommended value(s) of StandardDeviation: [1..4]
        /// Recommended value(s) of upperBound: 2
        /// This results in a mutation probabilities between 2.2% (with StandardDeviation=1, upperBound=2) and 31% (with StandardDeviation=4, upperBound=2).
        /// http://onlinestatbook.com/2/calculators/normal_dist.html
        /// </remarks>
        /// <param name="random">Random normal distribution number generator</param>
        /// <param name="StandardDeviation">Standard deviation of the normal distribution</param>
        /// <param name="upperBound">All generated values greater than this upperBound result in a mutation.</param>
        /// <returns>True = Mutation; False = No Mutation </returns>
        private bool MutateInput(IRandom random)
        {
            var rnd = random.NextGaussian(sigma: StandardDeviation);
            //if (rnd < 5)
            //{
            //    return false;
            //}
            //else
            //{
                if (random.Next(0, 1) <= 0.02 * Math.Pow(1.48, rnd - 5))
                {
                    return true;
                }
                return false;
            //}
        }


        /// <summary>
        /// Returns a normal distributed value with mu and sigma. Sigma gets scaled depending on the number of elements in the series.
        /// sigmanew = numberofelements/25 * sigma, sigma element (1..5)
        /// Returns a normal distributed position value (int) out of an ordered sequence.
        /// </summary>
        /// <param name="random"></param>
        /// <param name="numberofelements"></param>
        /// <param name="mu"></param>
        /// <param name="StandardDeviation"></param>
        /// <returns></returns>
        /// <remarks>
        /// numberofelements = 250 -> newsigma element (10..50)
        /// numberofelements = 50 -> newsigma element (2..10)
        /// numberofelements = 5 -> newsigma element (1/5..1)
        /// </remarks>
        private int getNormalDistributedValue(IRandom random, int numberofelements, double mu, double StandardDeviation = 1)
        {
            var value = Convert.ToInt32(random.NextGaussian(mu, numberofelements / 25 * StandardDeviation));
            if (value < 0)
            {
                value = 0;                      //return first index of the sequence
            }
            else if (value >= numberofelements)
            {
                value = numberofelements - 1;   //return last index of the sequence
            }
            return value;
        }

        /// <summary>
        /// Mutates each Input and Parameter within the provided FloatVector with a probability specified by a Gaussian distribution.
        /// <paramref name="vector"/> and rounds the result to the next feasible value.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the sigma vector is null or of length 0.</exception>
        /// <param name="mersenneTwister">A random number generator for normal distributed Gaussian values.</param>
        /// <param name="vector">The integer vector to manipulate.</param>
        /// <param name="gridInfo">The CGPConfiguration used.</param>
        /// <returns>The manipulated float vector.</returns>
        public void Apply(IRandom random, FloatVector vector, CGPConfiguration gridInfo, double StandardDeviation)
        {
            if (vector.Length != gridInfo.Length) throw new Exception("An individual's length must match the CGPConfiguration.Length °_°");

            //iterate through all nodes

            var nodes = gridInfo.NodesCount;

            for (int node = 0; node < nodes; node++)
            {
                var func = vector[gridInfo.OperatorIndex(node)];
                int column = gridInfo.ColumnOf(node);

                int offset;

                //input mutation
                for (offset = 0; offset < gridInfo.InputCountOfOperator(func); offset++)
                {
                    if (MutateInput(random) == true)
                    {
                        vector[gridInfo.NodeIndex(node) + offset] = random.SelectRandom(gridInfo.InputBounds[column][func][offset]);
                    }
                }

                //parameter mutation
                if (gridInfo.useNormalDistributedMutationStepSizeForNonCategoricalValues)
                {
                    var FuncParameterCount = gridInfo.ParameterBounds[func].Count();

                    //mutate non-categorical values relative to the value of the number drawn from the normal distribution, categorical values uniformly random
                    for (offset = 0; offset < FuncParameterCount / 2; offset++)
                    {
                        if (gridInfo.ParameterBounds[func][offset + FuncParameterCount / 2].ElementAt(0) == 0) //value is non-categorical
                        {
                            var count = gridInfo.ParameterBounds[func][offset].Count();
                            var currentValue = vector[gridInfo.ParameterIndex(node) + offset];
                            var currentPosition = gridInfo.ParameterBounds[func][offset].FindIndex(x => x == currentValue);
                            vector[gridInfo.ParameterIndex(node) + offset] = gridInfo.ParameterBounds[func][offset].ElementAt(getNormalDistributedValue(random, count, currentPosition, StandardDeviation));
                        }
                        else //value is categorical
                        {
                            if (MutateParameter(random, StandardDeviation, 2) == true)
                            {
                                vector[gridInfo.ParameterIndex(node) + offset] = gridInfo.ParameterBounds[func][offset].ElementAt(random.Next(0, gridInfo.ParameterBounds[func][offset].Count()));
                            }
                        }
                    }
                }
                else
                {
                    for (offset = 0; offset < gridInfo.ParameterBounds[func].Count(); offset++)
                    {
                        if (MutateParameter(random, StandardDeviation, 2) == true)
                        {
                            vector[gridInfo.ParameterIndex(node) + offset] = gridInfo.ParameterBounds[func][offset].ElementAt(random.Next(0, gridInfo.ParameterBounds[func][offset].Count()));
                        }
                    }
                }
            }

            //output mutation
            var lastColumn = gridInfo.ColumnCount - 1;
            for (var i = gridInfo.Length - gridInfo.OutputsCount; i < gridInfo.Length; i++)
            {
                if (MutateParameter(random, StandardDeviation, 2) == true)
                {
                    vector[i] = random.SelectRandom(gridInfo.ProgramOutputBounds);// gridInfo.InputBounds[lastColumn].ElementAt(random.Next(gridInfo.InputBounds[lastColumn].Count));
                }
            }

        }


        /// <summary>
        /// Mutates each Input and Parameter within the provided FloatVector with a probability specified by a Gaussian distribution.
        /// <paramref name="vector"/> and rounds the result to the next feasible value.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the sigma vector is null or of length 0.</exception>
        /// <param name="mersenneTwister">A random number generator for normal distributed Gaussian values.</param>
        /// <param name="vector">The integer vector to manipulate.</param>
        /// <param name="gridInfo">The CGPConfiguration used.</param>
        /// <returns>The manipulated float vector.</returns>
        public void Apply(IRandom random, MultipleFloatVectorEncoding vector, CGPConfiguration gridInfo, double StandardDeviation)
        {
            if (vector.Length() != gridInfo.Length) throw new Exception("An individual's length must match the CGPConfiguration.Length °_°");

            //iterate through all nodes
            var operators = gridInfo.Operators;
            //gridInfo.Operators calls OperatorMap.Operators -> notimplementedException

            var nodes = gridInfo.NodesCount;

            for (int node = 0; node < nodes; node++)
            {
                int column, nodenumberincolumn;
                gridInfo.ColumnOfAndNodeNumberInVector(node, true, out column, out nodenumberincolumn);
                var func = vector[column, gridInfo.OperatorIndex(node)];

                int offset;

                //input mutation
                for (offset = 0; offset < gridInfo.InputCountOfOperator(func); offset++)
                {
                    if (MutateInput(random) == true)
                    {
                        vector[column, gridInfo.NodeIndex(node) + offset] = random.SelectRandom(gridInfo.InputBounds[column][func][offset]);
                    }
                }

                //parameter mutation
                if (gridInfo.useNormalDistributedMutationStepSizeForNonCategoricalValues)
                {
                    var FuncParameterCount = gridInfo.ParameterBounds[func].Count();

                    //mutate non-categorical values relative to the value of the number drawn from the normal distribution, categorical values uniformly random
                    for (offset = 0; offset < FuncParameterCount / 2; offset++)
                    {
                        if (gridInfo.ParameterBounds[func][offset + FuncParameterCount / 2].ElementAt(0) == 0) //value is non-categorical
                        {
                            var count = gridInfo.ParameterBounds[func][offset].Count();
                            var currentValue = vector[column, gridInfo.ParameterIndex(node) + offset];
                            var currentPosition = gridInfo.ParameterBounds[func][offset].FindIndex(x => x == currentValue);
                            vector[column, gridInfo.ParameterIndex(node) + offset] = gridInfo.ParameterBounds[func][offset].ElementAt(getNormalDistributedValue(random, count, currentPosition, StandardDeviation));
                        }
                        else //value is categorical
                        {
                            if (MutateParameter(random, StandardDeviation, 2) == true)
                            {
                                vector[column, gridInfo.ParameterIndex(node) + offset] = gridInfo.ParameterBounds[func][offset].ElementAt(random.Next(0, gridInfo.ParameterBounds[func][offset].Count()));
                            }
                        }
                    }
                }
                else
                {
                    for (offset = 0; offset < gridInfo.ParameterBounds[func].Count(); offset++)
                    {
                        if (MutateParameter(random, StandardDeviation, 2) == true)
                        {
                            vector[column, gridInfo.ParameterIndex(node) + offset] = gridInfo.ParameterBounds[func][offset].ElementAt(random.Next(0, gridInfo.ParameterBounds[func][offset].Count()));
                        }
                    }
                }
            }
            // output mutation
            var lastColumn = gridInfo.ColumnCount - 1;
            for (var i = 0; i < gridInfo.OutputsCount; i++)
            {
                if (MutateParameter(random, StandardDeviation, 2) == true)
                {
                    vector[lastColumn, i] = random.SelectRandom(gridInfo.ProgramOutputBounds);
                }
            }
        }

        public override IIndividual Mutate(IIndividual individual)
        {

            if (StandardDeviation == -1 || StandardDeviation == 95)
            {
                throw new Exception("No mutation probability set in EvolutionStrategy for adaptive mutation for use in CGPCustomProbabilisticMutator!!! Pass a sigma in the ES constructor!");
            }

            var copy = individual.Copy() as IIndividual;
            if (individual.GetType() == typeof(MultipleFloatVectorEncoding))
            {
                Apply(Random, copy.MultipleFloatVectorEncoding, configuration, StandardDeviation);
            }
            else if (individual.GetType() == typeof(FloatVector))
            {
                Apply(Random, copy.FloatVector, configuration, StandardDeviation);
            }
            return copy;
        }

        public override ICopyable Copy()
        {
            return new CGPCustomProbabilisticMutator(Random, configuration, Decoder, StandardDeviation);
        }

        public override ICopyable Copy(IRandom rand)
        {
            throw new NotImplementedException();
        }
    }
}