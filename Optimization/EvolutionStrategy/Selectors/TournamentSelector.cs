using System;
using System.Collections.Generic;
using System.Linq;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;

namespace Optimization.EvolutionStrategy.Selectors
{
    /// <summary>
    /// Selection strategy randomly selecting a bunch of individuals and then selecting the highest fitness one out of them.
    /// </summary>
    [Serializable]
    public class TournamentSelector : RandomSelector
    {
        protected int TournamentSize;
        protected double Probablity = 0.70;

        public TournamentSelector(IRandom random, FitnessConfiguration fitnessConfiguration, int tournamentsize = 3, double probability = 0.70) : base(random)
        {
            Random = random;
            TournamentSize = tournamentsize;

            if (Probablity > 1 || Probablity < 0)
            {
                Probablity = 0.5;
            }
            else
            {
                Probablity = probability;
            }

            FitnessConfiguration = fitnessConfiguration;
        }

        public override List<IIndividual> Select(List<IIndividual> individuals, int number)
        {
            var selected = new List<IIndividual>();
            var tournament = new List<IIndividual>();

            // it does not really make sense to select with tournamentselection when the number of individuals is too low -> just random picks
            //if (TournamentSize > individuals.Count())
            //{
            //    throw new Exception("Only use Tournament Selection if your ESConfiguration has Mu > Size of the tournament!");
            //}
            int i;

            //select number-amount of individuals according to tournament selection
            for (int iterations = 0; iterations < number; iterations++)
            {
                //select tournamentsize random individuals
                for (i = 0; i < TournamentSize; i++)
                {
                    tournament.Add(individuals.ElementAt(Random.Next(0, individuals.Count)));
                }

                if (FitnessConfiguration.Maximization)
                {
                    tournament.OrderBy(x => x.Fitness);
                }
                else
                {
                    tournament.OrderByDescending(x => x.Fitness);
                }
                //choose the best individual from tournament with probability p
                //choose the second best individual with probability p * (1 - p)
                //choose the third best individual with probability p*((1 - p) ^ 2)
                for (i = 0; i < TournamentSize; i++)
                {
                    if (Random.NextDouble() < Probablity * Math.Pow(1 - Probablity, i))
                    {
                        selected.Add(tournament.ElementAt(i).Copy() as IIndividual);
                        break;          //only add one out of the tournament
                    }
                }

                if (selected.Count() < iterations + 1)  // the probability of p + (p*(1-p)) + ... != 1 for non infinity
                {
                    selected.Add(tournament.ElementAt(i - 1).Copy() as IIndividual);      //add the fittest of the individuals
                }
            }

            /*  while (selected.Count() < number)
              {
                  if (maximization)
                  {
                      selected.Add(individuals.OrderBy(x => x.Fitness).First());
                  }
                  else
                  {
                      selected.Add(individuals.OrderByDescending(x => x.Fitness).First());
                  }
              }
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
