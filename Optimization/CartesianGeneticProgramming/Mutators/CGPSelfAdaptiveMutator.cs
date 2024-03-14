using System;
using System.Linq;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Mutators;

namespace Optimization.CartesianGeneticProgramming.Mutators
{
    /// <summary>
    /// Manipulates each dimension in the float vector(s) with the mutation probability of normal distributed values which are rounded to the next feasible value.
    /// </summary>
    [Serializable]
    class CGPSelfAdaptiveMutator : AdaptiveMutator, ICopyable
    {
        private CGPConfiguration configuration;
        private CGPDecoder Decoder;

        /// <summary>
        /// standard deviation of the current vector; sigma value for self adaptation
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
       // double stdDev = -1;

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
        /// 5 = 0.3446</remarks>
        /// <param name="random"></param>
        /// <param name="configuration"></param>
        /// <param name="decoder"></param>
        /// <param name="stdDev">Starting value of the standard deviation for self-adataptive mutation; default value relatively high because of search space exploration at the beginning of the evolution</param>
        public CGPSelfAdaptiveMutator(IRandom random, CGPConfiguration configuration, CGPDecoder decoder, double startStdDev) : base(random, startStdDev)
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
        private bool ToMutate(IRandom random, double stdDev = 1, double upperBound = 2)
        {
            if (random.NextGaussian(sigma: stdDev) > upperBound)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a normal distributed position value (int) for the selection out of a ordered sequence.
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

            if(value < 0)
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
        /// Function returning a normal distributed value between 0 and 1
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        private double generateGaussianValueBetween0and1(IRandom random)
        {
            var x = random.NextGaussian(0, 1);          //almost always values between -3 and 3 are returned
            
            if (x > 3)       //~0.27% of numbers are > 3 or < 3
            {
                x = 3;
            }
            if (x < -3)
            {
                x = -3;
            }
            x = x / 6;      //x between -0.5 and 0.5
            return x + 0.5;     //x between 0 and 1
        }

        /// <summary>
        /// Function returning the relative position of the current sigma value in relation to the lower and upper bounds of possible values for sigma.
        /// </summary>
        /// <param name="probability"></param>
        /// <param name="sigmaMin"></param>
        /// <param name="sigmaMax"></param>
        /// <returns></returns>
        private double scaleProbabilityToSigma(double probability, double sigmaMin = 1, double sigmaMax = 5)
        {
            var sigma = sigmaMin + (sigmaMax - sigmaMin) * probability;
            if(sigma < sigmaMin)
            {
                return sigmaMin + 0.0000001;
            }
            else if(sigma > sigmaMax)
            {
                return sigmaMax - 0.0000001;
            }
            else
            {
                return sigma;
            }
        }

        /// <summary>
        /// Function calculating a sigma value from the relative position with upper and lower bounds specified.
        /// </summary>
        /// <param name="sigma"></param>
        /// <param name="sigmaMin"></param>
        /// <param name="sigmaMax"></param>
        /// <returns></returns>
        private double scaleSigmaToProbability(double sigma, double sigmaMin = 1, double sigmaMax = 5)
        {
            var ratio = (sigma - sigmaMin) / (sigmaMax - sigmaMin);
            if(ratio >= 1)
            {
                return 0.999999;
            }
            else if(ratio <= 0)
            {
                return 0.000001;
            }
            else
            {
                return ratio;
            }
        }

        /// <summary>
        /// Update formula for self adaptation with exp value y = 0.22 from paper "Intelligent Mutation Rate Control in Canonical Genetic Algorithms"
        /// </summary>
        /// <param name="originalSigma"></param>
        /// <param name="sigmaMin"></param>
        /// <param name="sigmaMax"></param>
        /// <returns></returns>
        private double selfAdaptSigma(IRandom random, double originalSigma, double sigmaMin = 1, double sigmaMax = 5)
        {
            var probability = scaleSigmaToProbability(originalSigma, sigmaMin, sigmaMax);
            var gaussianValue = generateGaussianValueBetween0and1(random);
            var newProbability = Math.Pow(((1 + (1 - probability) / probability) * Math.Exp(-0.22 * gaussianValue)), -1);
            if (newProbability > 1)     //this should not be possible since the paper says that the formula returns a value between ]0, 1[ for input values between ]0, 1[
            {
                newProbability = 1;
            }
            var newSigma = scaleProbabilityToSigma(newProbability, sigmaMin, sigmaMax);
            return newSigma;
        }

        /// <summary>
        /// Mutates each Input and Parameter within the provided FloatVector with a uniform probability or by usage of the gaussian distribution.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the sigma vector is null or of length 0.</exception>
        /// <paramref name="vector"/> 
        /// <param name="mersenneTwister">A random number generator for normal distributed Gaussian values.</param>
        /// <param name="vector">The integer vector to manipulate.</param>
        /// <param name="gridInfo">The CGPConfiguration used.</param>
        /// <returns>The manipulated float vector.</returns>
        public void Apply(IRandom random, FloatVector vector, CGPConfiguration gridInfo)
        {
            throw new NotImplementedException("Self-adaptive mutation is currently only available for MultipleFloatVectorEncoding!");
            //implementation for self-adaptive mutation can follow the one for MultipleFloatVectorEncoding below
            //note that the location of the sigma parameter in the genotype has to be specified for FloatVector first!
            /*
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
                        vector[gridInfo.NodeIndex(node) + offset] = gridInfo.InputBounds[column].ElementAt(random.Next(gridInfo.InputBounds[column].Count));
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
                    vector[i] = gridInfo.InputBounds[lastColumn].ElementAt(random.Next(gridInfo.InputBounds[lastColumn].Count));
                }
            }
            */
        }


        /// <summary>
        /// Mutates each Input and Parameter within the provided vector with a uniform probability or by usage of the gaussian distribution.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the sigma vector is null or of length 0.</exception>
        /// <paramref name="vector"/> 
        /// <param name="mersenneTwister">A random number generator for normal distributed Gaussian values.</param>
        /// <param name="vector">The integer vector to manipulate.</param>
        /// <param name="gridInfo">The CGPConfiguration used.</param>
        /// <returns>The manipulated float vector.</returns>
        public void Apply(IRandom random, MultipleFloatVectorEncoding vector, CGPConfiguration gridInfo)
        {
            if (gridInfo.useSelfAdaptiveMutation == true)
            {
                if (vector.Length() != gridInfo.Length + 1) throw new Exception("An individual's length must match the CGPConfiguration.Length °_°");
                vector[gridInfo.ColumnCount - 1, gridInfo.OutputsCount] = (float)selfAdaptSigma(random, vector[gridInfo.ColumnCount - 1, gridInfo.OutputsCount]);
                StandardDeviation = vector[gridInfo.ColumnCount - 1, gridInfo.OutputsCount];
            }
            else
            {
                if (vector.Length() != gridInfo.Length) throw new Exception("An individual's length must match the CGPConfiguration.Length °_°");
            }

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
                    if (ToMutate(random, StandardDeviation, 2) == true)
                    {
                        vector[column, gridInfo.NodeIndex(node) + offset] = random.SelectRandom(gridInfo.InputBounds[column][func][offset]);//gridInfo.InputBounds[column].ElementAt(random.Next(gridInfo.InputBounds[column].Count));
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
                            vector[column, gridInfo.ParameterIndex(node) + offset] = gridInfo.ParameterBounds[func][offset].ElementAt(getNormalDistributedValue(random, count, currentPosition, StandardDeviation));
                        }
                        else //value is categorical
                        {
                            if (ToMutate(random, StandardDeviation, 2) == true)
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
                        if (ToMutate(random, StandardDeviation, 2) == true)
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
                if (ToMutate(random, StandardDeviation, 2) == true)
                {
                    vector[lastColumn, i] = random.SelectRandom(gridInfo.ProgramOutputBounds);//gridInfo.InputBounds[lastColumn].ElementAt(random.Next(gridInfo.InputBounds[lastColumn].Count));
                }
            }
        }

        /// <summary>
        /// Function forwarding the call to the appropriate method. Self-adaptive mutation does not require any standard deviation to be passed to it (since it is encoded within the genotype)!
        /// </summary>
        /// <remarks>
        /// The starting value of self-adaptive mutation is set in the creator to a default value instead.
        /// </remarks>
        /// <param name="individual"></param>
        /// <param name="stdDev"></param>
        /// <returns></returns>
        public override IIndividual Mutate(IIndividual individual)
        {
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
            return new CGPSelfAdaptiveMutator(Random, configuration, Decoder, StandardDeviation);
        }

        public override ICopyable Copy(IRandom rand)
        {
            return new CGPSelfAdaptiveMutator(rand, configuration, Decoder, StandardDeviation);
        }
    }
}


