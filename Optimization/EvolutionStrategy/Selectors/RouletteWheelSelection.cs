using System;
using System.Collections.Generic;
using System.Linq;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;

namespace Optimization.EvolutionStrategy.Selectors
{
    /// <summary>
    /// Selection strategy selecting individuals proportional to their fitness, i.e. high fitness - high chance, low fitness - low chance
    /// </summary>
    [Serializable]
    public class RouletteWheelSelection : RandomSelector
    {
    
        public RouletteWheelSelection(IRandom random, FitnessConfiguration fitnessConfiguration) : base(random)
        {
            Random = random;
            FitnessConfiguration = fitnessConfiguration;
        }

        public override List<IIndividual> Select(List<IIndividual> individuals, int number)
        {
            //if(maximization == false)
            //{
            //    throw new Exception("Roulette Wheel Selection as a fitness proportional selection method can not be used for minimization.");
            //}
            var selected = new List<IIndividual>();

            if (FitnessConfiguration.Maximization)
            {
                var fitnessSum = individuals.Sum(x => FitnessConfiguration.WeightedFitnessOf(x));

                if (fitnessSum < 0)     //otherwise Random.Next would throw an exception
                {
                    for (int i = 0; i < number; i++)
                    {
                        selected.Add(individuals.ElementAt(Random.Next(0, individuals.Count() - 1)));
                    }
                }
                else
                {
                    for (int iterations = 0; iterations < number; iterations++)
                    {

                        var randomNumber = Random.Next(0, (int)fitnessSum);

                        int i = 0;
                        double partialSum = 0;

                        while (randomNumber > partialSum)
                        {
                            partialSum += FitnessConfiguration.WeightedFitnessOf(individuals.ElementAt(i));
                            i++;
                        }
                        selected.Add(individuals.ElementAt(i).Copy() as IIndividual);
                    }
                }
            }

            //not sure if this approach to minimization works, calculate new Fitness 1-originalFitness
            else
            {
                var adaptedFitnessList = new List<Tuple<IIndividual, double>>();
                Tuple<IIndividual, double> adaptedFitnessIndividual;
                for (int i = 0; i < individuals.Count; i++)
                {
                    var newFitness = 1 - FitnessConfiguration.WeightedFitnessOf(individuals.ElementAt(i));
                    adaptedFitnessIndividual = new Tuple<IIndividual, double>(individuals.ElementAt(i), newFitness);
                    adaptedFitnessList.Add(adaptedFitnessIndividual);
                }

                var fitnessSum = adaptedFitnessList.Sum(x => x.Item2);

                for (int iterations = 0; iterations < number; iterations++)
                {
                    var randomNumber = Random.Next(0, (int)fitnessSum);

                    int i = 0;
                    double partialSum = 0;

                    while (randomNumber > partialSum)
                    {
                        partialSum += adaptedFitnessList.ElementAt(i).Item2;
                        i++;
                    }
                    selected.Add(adaptedFitnessList.ElementAt(i).Item1.Copy() as IIndividual);
                }
            }

            /*
// Find the sum of fitnesses. The function fitness(i) should 
//return the fitness value   for member i**

float sumFitness = 0.0f;
for (int i=0; i < nmembers; i++)
    sumFitness += fitness(i);

// Get a floating point number in the interval 0.0 ... sumFitness**
float randomNumber = (float(rand() % 10000) / 9999.0f) * sumFitness;

// Translate this number to the corresponding member**
int memberID=0;
float partialSum=0.0f;

while (randomNumber > partialSum)
{
   partialSum += fitness(memberID);
   memberID++;
} 

We have just found the member of the population using the roulette algorithm * *
It is stored in the "memberID" variable**
Repeat this procedure as many times to find random members of the population**
*/
            return selected;
        }

        public override IIndividual Select(List<IIndividual> individuals)
        {
            var selection = Select(individuals, 1);
            return selection.First().Copy() as IIndividual;
        }

    }
}

