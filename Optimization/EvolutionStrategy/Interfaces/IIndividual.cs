using System.Collections.Generic;
using System.Configuration;
using System;
using System.Linq;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.Fitness;
using Optimization.CartesianGeneticProgramming;

namespace Optimization.EvolutionStrategy.Interfaces
{

    /// <summary>
    /// Add your Encoding here if you add an encoding type
    /// </summary>
    public interface IIndividual : ICopyable
    {
        MultipleFloatVectorEncoding MultipleFloatVectorEncoding { get; }

        FloatVector FloatVector { get;}

        BooleanVector BooleanVector { get; }

        /// <summary>
        /// Holds the values of the most recent evaluation of this individual
        /// If you implement your own Evaluator, take care to set the values
        /// properly, such that the values contained in this dictionary
        /// correspond to whatever your evaluator is supposed to return as
        /// evaluation result. For example, the average over an entire dataset
        /// is commonly used.
        ///
        /// When accessing these Fitness values, be careful. The values in here
        /// might correspond to results from .Validate() and .Evaluate()
        /// The latter using training data, the former using validation data.
        ///
        /// Most importantly, if you are interested in the overall fitness
        /// of this individual, one way to compute it using the weights specified
        /// by some user is to use the FitnessConfiguration corresponding to
        /// the evolution strategy that produced this individual by calling
        /// fitnessConfigration.WeightedFitnessOf(individual)
        /// This will apply the proper weights for each FitnessFunction
        ///
        /// Additionally, all evaluators expose a wrapper for this WeightedFitnessOf
        /// method, as they are required to know the FitnessConfiguration.
        /// </summary>
        Dictionary<FitnessFunction, double?> Fitness { get; set; }

        long GetId();
    }
}
