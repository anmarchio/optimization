using System;
using System.Collections.Generic;
using Optimization.CartesianGeneticProgramming;
using Optimization.Fitness;

namespace Optimization.EvolutionStrategy.Interfaces
{

    /// <summary>
    /// Responsible for all the nasty fitness evaluating.
    /// </summary>
    public interface IEvaluator : IValidator
    {
        void Evaluate(List<IIndividual> individuals);

        void Evaluate(IIndividual individual);

        int IndividualsEvaluated { get; set; }

        FitnessConfiguration FitnessConfiguration { get; set; }

        double WeightedFitnessOf(IIndividual individual);
    }
}