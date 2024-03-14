using System;
using System.Linq;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Mutators;

namespace Optimization.CartesianGeneticProgramming.Mutators
{
    /// <summary>
    /// Manipulates each dimension in the float vector with uniform probabilistic mutation or via the usage of normal distributed values which are rounded to the next feasible value.
    /// </summary>
    /// <remarks>
    /// stdDev attribute of EvolutionStrategy controls the mutation rate for adaptive mutation -- use/implement CGPCustomProbabilisticMutator for adaptive mutation instead!
    ///</remarks>
    [Serializable]
    public class CGPProbabilisticMutator : AdaptiveMutator, ICopyable
    {

        private CGPConfiguration configuration;
        private CGPDecoder Decoder;

        /// <summary>
        /// standard deviation of a normal distribution
        /// </summary>
        /// <remarks>
        /// some values: aka % of area that lies above the value 2, with sigma = infinity this rate would be 0.5
        /// 1 = 0.0228
        /// 1.2 = 0.0478
        /// 1.4 = 0.0766
        /// 1.6 = 0.1056
        /// 1.8 = 0.1333
        /// 2 = 0.1587
        /// 3 = 0.2525
        /// 4 = 0.3085
        /// 5 = 0.3446
        /// </remarks>
        double stdDev = -1;

     

        public override double StandardDeviation
        {
            get { return stdDev; }
            set { stdDev = value; }
        }

        /// <summary>
        /// /// some values: aka % of area that lies above the value 2, with sigma = infinity this rate would be 0.5
        /// </summary>
        /// <remarks>
        /// 1 = 0.0228;
        /// 1.2 = 0.0478;
        /// 1.4 = 0.0766;
        /// 1.6 = 0.1056;
        /// 1.8 = 0.1333;
        /// 2 = 0.1587;
        /// 3 = 0.2525;
        /// 4 = 0.3085;
        /// 5 = 0.3446
        /// </remarks>
        /// <param name="random"></param>
        /// <param name="configuration"></param>
        /// <param name="decoder"></param>
        /// <param name="stdDev"></param>
        public CGPProbabilisticMutator(IRandom random, CGPConfiguration configuration, CGPDecoder decoder, double stdDev = 5) : base(random, stdDev)
        {
            this.configuration = configuration;
            this.Random = random;
            Decoder = decoder;
            this.stdDev = stdDev;
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
        private bool ToMutate(IRandom random, double stdDev = 1, double upperBound = 2)
        {
            if (random.NextGaussian(sigma: stdDev) > upperBound)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a normal distributed position value (int) for the given selection out of an ordered sequence.
        /// Returns a normal distributed value with mu and sigma. Sigma gets scaled depending on the number of elements in the series.
        /// </summary>
        /// <param name="random"></param>
        /// <param name="numberofelements"></param>
        /// <param name="mu">current position</param>
        /// <param name="stdDev"></param>
        /// <returns></returns>
        /// <remarks>
        /// numberofelements = 250 -> newsigma element (10..50)
        /// numberofelements = 50 -> newsigma element (2..10)
        /// numberofelements = 5 -> newsigma element (1/5..1)
        /// </remarks>
        private int getNormalDistributedValue(IRandom random, int numberofelements, double mu, double stdDev = 1)
        {
            var factor = 0.0;
            for (int i = 0; i < numberofelements; i++)
            {
                factor += Math.Pow(i - mu, 2);
            }
            var scaleFactor = scaleSigmaToProbability(stdDev) + 0.4;
            var newsigma = scaleFactor * Math.Sqrt(factor / numberofelements);

            var value = Convert.ToInt32(random.NextGaussian(mu, newsigma));

            if (value < 0)
            {
                value = 0;              //return first index of the sequence
            }
            else if (value >= numberofelements)
            {
                value = numberofelements - 1;   //return last index of the sequence
            }

            return value;
        }

        /// <summary>
        /// Returns a probability value between 0 and 1 depending on the height of the sigma value in a ratio to a lower and upper bound value.
        /// </summary>
        /// <param name="sigma"></param>
        /// <param name="sigmaMin"></param>
        /// <param name="sigmaMax"></param>
        /// <returns></returns>
        private double scaleSigmaToProbability(double sigma, double sigmaMin = 1, double sigmaMax = 5)
        {
            return (sigma - sigmaMin) / (sigmaMax - sigmaMin);
        }

        /// <summary>
        /// Mutates each input and parameter gene within the provided FloatVector with a uniform probability or one specified by a Gaussian distribution.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the sigma vector is null or of length 0.</exception>
        /// <param name="random">A random number generator.</param>
        /// <param name="vector">The vector/genotype to manipulate.</param>
        /// <param name="gridInfo">The CGPConfiguration used.</param>
        /// <returns>The manipulated float vector.</returns>
        public void Apply(IRandom random, FloatVector vector, CGPConfiguration gridInfo)
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
                    if (ToMutate(random, stdDev, 2) == true)
                    {
                        vector[gridInfo.NodeIndex(node) + offset] = random.SelectRandom(gridInfo.InputBounds[column][func][offset]);//gridInfo.InputBounds[column].ElementAt(random.Next(gridInfo.InputBounds[column].Count));
                    }
                }

                //parameter mutation
                if (gridInfo.useNormalDistributedMutationStepSizeForNonCategoricalValues)
                {
                    var FuncParameterCount = gridInfo.ParameterBounds[func].Count();

                    //mutate non-categorical values relative to the value of the number drawn from the normal distribution, categorical values uniformly random
                    for (offset = 0; offset < gridInfo.ParameterBounds[func].Count() / 2; offset++)
                    {
                        if (gridInfo.ParameterBounds[func][offset + FuncParameterCount / 2].ElementAt(0) == 0) //value is non-categorical
                        {
                            var count = gridInfo.ParameterBounds[func][offset].Count();
                            var currentValue = vector[gridInfo.ParameterIndex(node) + offset];
                            var currentPosition = gridInfo.ParameterBounds[func][offset].FindIndex(x => x == currentValue);
                            vector[gridInfo.ParameterIndex(node) + offset] = gridInfo.ParameterBounds[func][offset].ElementAt(getNormalDistributedValue(random, count, currentPosition, stdDev));
                        }
                        else //value is categorical
                        {
                            if (ToMutate(random, stdDev, 2) == true)
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
                        if (ToMutate(random, stdDev, 2) == true)
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
                if (ToMutate(random, stdDev, 2) == true)
                {
                    vector[i] = random.SelectRandom(gridInfo.ProgramOutputBounds);//gridInfo.InputBounds[lastColumn].ElementAt(random.Next(gridInfo.InputBounds[lastColumn].Count));
                }
            }

        }


        /// <summary>
        /// Mutates each input and parameter gene within the provided vector with a uniform probability or by a Gaussian distribution when 
        /// mutation distinguishing between categorical and non-categorical parameters is desired.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the sigma vector is null or of length 0.</exception>
        /// <param name="mersenneTwister">A random number generator.</param>
        /// <param name="vector">The vector/genotype to manipulate.</param>
        /// <param name="gridInfo">The CGPConfiguration used.</param>
        /// <returns>The manipulated float vector.</returns>
        public void Apply(IRandom random, MultipleFloatVectorEncoding vector, CGPConfiguration gridInfo)
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
                    if (ToMutate(random, stdDev, 2) == true)
                    {
                        vector[column, gridInfo.NodeIndex(node) + offset] = random.SelectRandom(gridInfo.InputBounds[column][func][offset]); // gridInfo.InputBounds[column].ElementAt(random.Next(gridInfo.InputBounds[column].Count));
                    }
                }

                //parameter mutation
                if (gridInfo.useNormalDistributedMutationStepSizeForNonCategoricalValues)
                {
                    var FuncParameterCount = gridInfo.ParameterBounds[func].Count();

                    //mutate non-categorical values relative to the value of the number drawn from the normal distribution, categorical values uniformly random
                    for (offset = 0; offset < gridInfo.ParameterBounds[func].Count() / 2; offset++)
                    {
                        if (gridInfo.ParameterBounds[func][offset + FuncParameterCount / 2].ElementAt(0) == 0) //value is non-categorical
                        {
                            var count = gridInfo.ParameterBounds[func][offset].Count();
                            var currentValue = vector[column, gridInfo.ParameterIndex(node) + offset];
                            var currentPosition = gridInfo.ParameterBounds[func][offset].FindIndex(x => x == currentValue);
                            vector[column, gridInfo.ParameterIndex(node) + offset] = gridInfo.ParameterBounds[func][offset].ElementAt(getNormalDistributedValue(random, count, currentPosition, stdDev));
                        }
                        else //value is categorical
                        {
                            if (ToMutate(random, stdDev, 2) == true)
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
                        if (ToMutate(random, stdDev, 2) == true)
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
                if (ToMutate(random, stdDev, 2) == true)
                {
                    vector[lastColumn, i] = random.SelectRandom(gridInfo.ProgramOutputBounds);//gridInfo.InputBounds[lastColumn].ElementAt(random.Next(gridInfo.InputBounds[lastColumn].Count));
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
            Apply(random, vector, configuration);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Use CGPCustomProbabilisticMutator for adaptive mutation in the future! Just not nice checking specific values to enable some settings.
        /// </remarks>
        /// <param name="individual">Individual to be mutated, abstracted from their encoding type</param>
        /// <param name="stdDev">Standard deviation for the mutation</param>
        /// <returns>A mutated copy of the provided individual</returns>
        public override IIndividual Mutate(IIndividual individual)
        {
            if (stdDev == -1 && this.stdDev == -1)
            {
                throw new Exception("No mutation probability set in ProbabilisticMutator!!! [Neither in the constructor nor via the EvolutionStrategies adaptive sigma]");
            }

            var copy = individual.Copy() as IIndividual;
            if (individual.GetType() == typeof(MultipleFloatVectorEncoding))
            {
                Apply(Random, copy.MultipleFloatVectorEncoding, configuration);
            }
            else if (individual.GetType() == typeof(FloatVector))
            {
                Apply(Random, copy.FloatVector, configuration);
            }
            return copy;
        }

        public override ICopyable Copy()
        {
            return new CGPProbabilisticMutator(Random, configuration, Decoder, stdDev);
        }

        public override ICopyable Copy(IRandom rand)
        {
            return new CGPProbabilisticMutator(rand, configuration, Decoder, stdDev);
        }
    }
}




