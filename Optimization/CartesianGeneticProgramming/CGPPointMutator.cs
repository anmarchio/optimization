using PRIME.Optimization.EvolutionStrategy.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRIME.Optimization.CartesianGeneticProgramming
{
    [Serializable]

    public class CGPPointMutator : IMutator, ICopyable
    {
        public float MutationRate { get; private set; }
        public IRandom Random { get; private set; }

        private Dictionary<int, List<float>> InputBounds { get; set; }

        private Dictionary<int, List<float>> OperatorBounds { get; set; }
        private Dictionary<float, List<float>[]> ParameterBounds { get; set; }

        public CGPConfiguration Configuration { get; private set; }

        /// <summary>
        /// Simple point mutator class: every gene is altered according to the mutation rate. Gaussian distributed floats are added to the parametergenes.
        /// Inputs and operators are chosen by a uniform distribution from the set appropriate for the given column. (i.e. as usual)
        /// 
        /// </summary>
        /// <param name="random"></param>
        /// <param name="configuration"></param>
        /// <param name="mutationRate"></param>
        public CGPPointMutator(IRandom random, CGPConfiguration configuration, float mutationRate)
        {
            Configuration = configuration;
            MutationRate = mutationRate;
            InputBounds = Configuration.InputBounds;
            OperatorBounds = Configuration.OperatorBounds;
            ParameterBounds = Configuration.ParameterBounds;
            Random = random;
        }
        public IIndividual Mutate(IIndividual individual)
        {
            var floatVector = individual.FloatVector;

            for (int i = 0; i < floatVector.Length; i++)
            {
                // if mutate
                if (Random.NextDouble() <= MutationRate)
                {
                    var column = Configuration.ColumnOf(i / Configuration.NodeLength);
                    if(Configuration.IsInputGene(i))
                    {
                        floatVector[i] = InputBounds[column][Random.Next(InputBounds[column].Count)];
                    }
                    else if(Configuration.IsOperatorGene(i))
                    {
                        floatVector[i] = OperatorBounds[column][Random.Next(OperatorBounds[column].Count)];
                    }
                    else if(Configuration.IsParameterGene(i))
                    {
                        float newValue = floatVector[i] + (float)Random.NextGaussian();
                        floatVector[i] = newValue;
                    }
                    else
                    {
                        floatVector[i] = Math.Max(1, Random.Next(Configuration.ProgramInputCount+1));
                    }
                }
            }

            return floatVector;
        }

        public ICopyable Copy()
        {
            return new CGPPointMutator(Random, Configuration, MutationRate);
        }
    }
}