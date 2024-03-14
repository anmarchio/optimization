using System;
using System.Collections.Generic;
using System.Linq;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;

namespace Optimization.EvolutionStrategy.Selectors
{
    /// <summary>
    /// Randomly selects individuals.
    /// </summary>
    [Serializable]
    public class RandomSelector : ISelector, ICopyableRandom
    {
        protected IRandom Random { get; set; }
        public RandomSelector(IRandom random)
        {
            this.Random = random;
        }


        public virtual IIndividual Select(List<IIndividual> individuals)
        {
            return individuals.ElementAt(Random.Next(individuals.Count)).Copy() as IIndividual;
        }

        public virtual List<IIndividual> Select(List<IIndividual> population, int number)
        {
            var result = new List<IIndividual>();
            while (result.Count < number)
            {
                result.Add(population.ElementAt(Random.Next(population.Count)).Copy() as IIndividual);
            }
            return result;
        }

        public ICopyable Copy()
        {
            throw new NotImplementedException();
        }

        public ICopyable Copy(IRandom rand)
        {
            return new RandomSelector(rand);
        }

        public FitnessConfiguration FitnessConfiguration { get; set; } = null; // not required, only by interface
    }
}
