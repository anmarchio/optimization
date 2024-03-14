using System;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Mutators
{
    public abstract class AdaptiveMutator : Mutator, IAdaptiveMutator
    {
        public AdaptiveMutator(IRandom random, double startStdDev) : base(random)
        {
            StandardDeviation = startStdDev;
        }

        public double MutationProbability
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

        /// <summary> 
        /// default value: -1 = no adaptive mutation ||
        /// some values:
        /// 1 = 0.0228;
        /// 1.2 = 0.0478;
        /// 1.4 = 0.0766;
        /// 1.6 = 0.1056;
        /// 1.8 = 0.1333;
        /// 2 = 0.1587;
        /// 3 = 0.2525;
        /// 4 = 0.3085
        /// </summary>
        public virtual double StandardDeviation
        {
            get; set;
        } = -1;

        public virtual void Adapt(int generationsWithoutImprovements)
        {
            mutateSigma(generationsWithoutImprovements);
        }



        /// <summary>
        /// Mutate sigma/stdDev as a adaptation strategy. Results in a sawtooth trend.
        /// TODO NOTE: require a minimum fitness increase, else extremely local search (is resetted with minimal fitness increases)
        /// </summary>
        /// <returns></returns>
        private void mutateSigma(int generations_without_fitness_improvements)
        {
            if (StandardDeviation > 5)              //reduce sigma to avoid bad results from too heavy input-mutation, maybe?!
            {
                StandardDeviation = 2.5;
            }
            if (generations_without_fitness_improvements > 0)        //increase sigma by a stepsize to increase mutation rate when fitness has not increased
            {
                StandardDeviation += 0.1;
            }
            else                       //reset sigma if a better fitness value was achieved
            {
                StandardDeviation = 1;
            }
        }
    }
}
