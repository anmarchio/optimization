using System;
using System.Collections.Generic;
using System.Linq;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;

namespace Optimization.EvolutionStrategy.Selectors
{
    /// <summary>
    /// Best / elitist / elitism selection strategy, i.e. the individuals with the highest fitness value(s) are selected
    /// </summary>
    [Serializable]
    public class BestSelector : ISelector
    {
     
        public BestSelector(FitnessConfiguration fitnessConfiguration)
        {
            FitnessConfiguration = fitnessConfiguration;

        }

        public ICopyable Copy()
        {
            return this;
        }

        public ICopyable Copy(IRandom rand)
        {
            return this;
        }

        public FitnessConfiguration FitnessConfiguration { get; set; }

        public List<IIndividual> Select(List<IIndividual> individuals, int number)
        {
            var selected = new List<IIndividual>();
            if(FitnessConfiguration.Maximization)
                individuals = individuals.OrderByDescending(x => FitnessConfiguration.WeightedFitnessOf(x)).ToList();
            else
                individuals = individuals.OrderBy(x => FitnessConfiguration.WeightedFitnessOf(x)).ToList();

            //used to make sure that everything works fine if the number of individuals to be selected is greater than the size of the population from which to select from
            var countIndividuals = individuals.Count;                   
            
            for(int i = 0; i < number; i++)
            {
                selected.Add(individuals[i % countIndividuals].Copy() as IIndividual);
            }
            return selected;
        }

        public IIndividual Select(List<IIndividual> individuals)
        {
            return Select(individuals, 1).First();
        }
    }
}
