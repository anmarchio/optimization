using System;
using System.Collections.Generic;
using System.Linq;
using Optimization.Data;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;
using Serilog;

namespace Optimization.EvolutionStrategy.Evaluators
{
    public abstract class Evaluator<TData> : IEvaluator, ICopyable, IValidityTester
    {
        protected Evaluator(DataLoader<TData> trainDataLoader, DataLoader<TData> valDataLoader, FitnessConfiguration fitnessConfiguration)
        {
            TrainDataLoader = trainDataLoader;
            ValidationDataLoader = valDataLoader;
            FitnessConfiguration = fitnessConfiguration;
        }

        public int IndividualsEvaluated { get; set; } = 0;

        public DataLoader<TData> TrainDataLoader { get; set; }
        public DataLoader<TData> ValidationDataLoader { get; set; }
        public FitnessConfiguration FitnessConfiguration { get; set; }
        public double WeightedFitnessOf(IIndividual individual)
        {
            return FitnessConfiguration.WeightedFitnessOf(individual);
        }

        public event EventHandler<EvaluationEventArgs> LoaderEvaluationCompleted;

        public event EventHandler<IndividualEvaluationEventArgs> IndividualEvaluationCompleted;

        protected virtual void OnIndividualEvaluationCompleted(IndividualEvaluationEventArgs e)
        {
            IndividualEvaluationCompleted?.Invoke(this, e);
        }

        protected virtual void OnLoaderEvaluationCompleted(EvaluationEventArgs e)
        {
            LoaderEvaluationCompleted?.Invoke(this, e);
        }

        public class EvaluationEventArgs : EventArgs
        {
            public virtual List<object> GetObjectForJson()
            {
                return EvaluatedIndividuals.Select(x => new
                {
                    IndividualId = x.GetId(),
                    Fitness = x.Fitness.ToDictionary(y => y.Key, y => y.Value)
                }).Cast<object>().ToList();
            }

            public List<IIndividual> EvaluatedIndividuals { get; set; }
            public DataLoader<TData> DataLoader { get; set; }
        }

        public class IndividualEvaluationEventArgs : EventArgs
        {
            public IIndividual Individual { get; set; }
            public TData Item { get; set; }
            public Dictionary<FitnessFunction, double?> FitnessValues { get; set; }

            public virtual object GetObjectForJson()
            {
                return new
                {
                    FitnessValues = FitnessValues.ToDictionary(x => x.Key, x => x.Value),
                    IndividualId = Individual.GetId(),
                    Item = Item.ToString()
                };
            }
        }

        public abstract ICopyable Copy();


        public virtual void Evaluate(List<IIndividual> individuals)
        {
            EvaluateLoader(individuals, TrainDataLoader);
            IndividualsEvaluated += individuals.Count;
        }

        public virtual void Evaluate(IIndividual individual)
        {
            IndividualsEvaluated += 1;
            EvaluateLoader(new List<IIndividual>(){ individual}, TrainDataLoader);
        }

        protected virtual void EvaluateLoader(List<IIndividual> individuals, DataLoader<TData> loader)
        {
            foreach (var individual in individuals)
            {
                foreach (var batch in loader.Batches())
                {
                    EvaluateBatch(individual, batch);
                }
            }

            // average fitness values over the dataset size
            foreach(var individual in individuals)
                foreach(var fitFunc in FitnessConfiguration.FitnessFunctions)
                    individual.Fitness[fitFunc] /= loader.DataSetSize;

            OnLoaderEvaluationCompleted(new EvaluationEventArgs() {DataLoader = loader, EvaluatedIndividuals = individuals});
        }


        protected virtual void EvaluateBatch(IIndividual individual, List<TData> batch, params object[] additional)
        {
            if (individual.Fitness.Any(x => x.Value == null)) return;
            foreach (var item in batch)
            {
                try
                {
                    EvaluateItem(individual, item, additional);
                }
                catch (Exception ex)
                {           
                    Log.Error(ex, "During Evaluation");
                    return;
                }
            }
        }

        /// <summary>
        /// You need to assign each individual fitness value for each fitness function to individual.Fitness
        ///
        /// Simply add individual.Fitness[fitnessFunction] += item.ComputeFitness(...)
        ///
        /// EvaluateLoader(...) averages over all items and clears the individual.Fitness dictionary before evaluation.
        /// </summary>
        /// <param name="individual"></param>
        /// <param name="item"></param>
        /// <param name="additional"></param>
        /// <returns></returns>
        protected abstract void EvaluateItem(IIndividual individual, TData item, params object[] additional);

        public void Validate(List<IIndividual> best)
        {
            EvaluateLoader(best, ValidationDataLoader);
        }

        public bool IsValid(IIndividual individual)
        {
            EvaluateLoader(new List<IIndividual>() {individual}, TrainDataLoader);
            return individual.Fitness.All(x => x.Value != null);
        }

        protected void ResetFitness(IIndividual individual)
        {
            individual.Fitness = FitnessConfiguration.FitnessFunctions.ToDictionary(x => x, x => (double?) 0.0);
        }

        public ICopyable Copy(IRandom rand)
        {
            throw new NotImplementedException();
        }
    }
}
