using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MathNet.Numerics;
using Optimization.CartesianGeneticProgramming;
using Optimization.Data;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Evaluators;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;
using Optimization.Fitness.Interfaces;
using Optimization.Pipeline.Interfaces;
using Serilog;

namespace Optimization.Pipeline
{
    public class PipelineEvaluator<TInputData, TOutput, TPipeline, TNode, TPipeInput, TPipeOutput> : Evaluator<TInputData> where TInputData : IReference<TPipeInput, TPipeOutput>, IDisposable where TPipeline : IPipeline<TNode, TPipeInput, TPipeOutput> where TNode : Node
    {
        /// <summary>
        /// Standard pipeline evaluator that either converts a floatvector representation using the specified cgpconfiguration into a pipeline
        /// or evaluates a pipeline directly.
        ///
        /// Uses fitConfig to compute fitness values
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="trainDataLoader"></param>
        /// <param name="valDataLoader"></param>
        /// <param name="fitConfig"></param>
        public PipelineEvaluator(CGPConfiguration configuration, DataLoader<TInputData> trainDataLoader, DataLoader<TInputData> valDataLoader, FitnessConfiguration fitConfig)
            : base(trainDataLoader, valDataLoader, fitConfig)
        {
            if (FitnessConfiguration.UseExecutionTimeThreshold) Watch = new Stopwatch();
            Configuration = configuration;
            DisposeAfterEachBatch = !trainDataLoader.ReturnsFullSet;
        }

        protected PipelineEvaluator(DataLoader<TInputData> trainDataLoader, DataLoader<TInputData> valDataLoader,
            FitnessConfiguration fitConfig) : base(trainDataLoader, valDataLoader, fitConfig)
        {
        }

        public CGPConfiguration Configuration { get; private set; }

        public override ICopyable Copy()
        {
            return new PipelineEvaluator<TInputData, TOutput, TPipeline, TNode, TPipeInput, TPipeOutput>(Configuration, TrainDataLoader, ValidationDataLoader, FitnessConfiguration);
        }

        public bool DisposeAfterEachBatch { get; set; }

        private Stopwatch Watch { get; set; }

        protected override void EvaluateItem(IIndividual individual, TInputData item, params object[] additional)
        {
            var pipe = (TPipeline)additional[0];
            var fitnessFunctionValues = FitnessConfiguration.FitnessFunctions.ToDictionary(x => x, x => (double?)0.0);

            Watch?.Start();
            try
            {
                var result = pipe.ExecuteSingle(item.Input);


                if (Watch != null)
                {
                    Watch.Stop();
                    if (Watch.ElapsedMilliseconds > FitnessConfiguration.ExecutionTimeThreshold)
                        foreach (var fitFunc in FitnessConfiguration.FitnessFunctions)
                            fitnessFunctionValues[fitFunc] = null;
                }

                var pixelPercentage = item.PercentageOfPixels(result);
                if (pixelPercentage > 1 || pixelPercentage < 0) throw new ArithmeticException($"illegal pixel percentage {pixelPercentage}");
                if (FitnessConfiguration.UsePixelPercentageThreshold &&
                    FitnessConfiguration.PixelPercentageThreshold < pixelPercentage)
                    foreach (var fitFunc in FitnessConfiguration.FitnessFunctions)
                        fitnessFunctionValues[fitFunc] = null;

                // if we already know that the individual is invalid, we can spare us the trouble of computing the fitness
                if (fitnessFunctionValues.All(x => x.Value != null))
                    foreach (var fitnessFunc in FitnessConfiguration.FitnessFunctions)
                        fitnessFunctionValues[fitnessFunc] = item.ComputeFitness(result, fitnessFunc);

                OnIndividualEvaluationCompleted(new IndividualEvaluationEventArgs
                {
                    FitnessValues = fitnessFunctionValues,
                    Individual = individual,
                    Item = item,
                });

                foreach (var fitnessFunction in FitnessConfiguration.FitnessFunctions)
                    individual.Fitness[fitnessFunction] += fitnessFunctionValues[fitnessFunction];
            }
            catch (Exception ex) when (ex is OperatorException
            || ex is ArithmeticException)
            {
                if (ex is OperatorException)
                {
                    var oex = (OperatorException)ex;
                    var pex = new CGPPipelineException($"{pipe.Name} caused an exception.", oex, pipe);
                    pex.UseSerilog();
                }
                // Hiding ArithmeticException
            }
            finally
            {
                pipe?.ResetOutput();
                Watch?.Reset();
            }
        }

        public class EvaluatorCgpEvaluationEventArgs : EvaluationEventArgs
        {
            public List<IPipeline<TNode, TPipeInput, TOutput>> Pipelines { get; set; }

            public override List<object> GetObjectForJson()
            {
                var zip = Enumerable.Range(0, Pipelines.Count)
                    .Select(i => (Pipeline: Pipelines[i], Individual: EvaluatedIndividuals[i]));

                return zip.Select(x => new
                {
                    IndividualId = x.Individual.GetId(),
                    Fitness = x.Individual.Fitness.ToDictionary(y => y.Key, y => y.Value),
                    Pipeline = x.Pipeline.Nodes.Select(y => new
                    {
                        y.NodeID,
                        Children = y.Children.Select(z => z.NodeID),
                        y.GetType().Name,
                        Parameters = y.PropertiesToList()
                    })
                }).Cast<object>().ToList();

            }
        }

        protected override void EvaluateLoader(List<IIndividual> individuals, DataLoader<TInputData> loader)
        {
            var pipelines = new List<IPipeline<TNode, TPipeInput, TOutput>>();

            foreach (var individual in individuals)
            {
                if (individual is IPipeline)
                    pipelines.Add((IPipeline<TNode, TPipeInput, TOutput>)individual);
                else if (individual is FloatVector)
                    pipelines.Add((IPipeline<TNode,TPipeInput,TOutput>)Activator.CreateInstance(typeof(TPipeline), individual.FloatVector, Configuration));
                else
                    throw new NotImplementedException(
                        "Currently only IPipeline objects and FloatVectors are supported");
            }

            EvaluateLoader(individuals, pipelines, loader);
        }

        protected void EvaluateLoader(List<IIndividual> individuals, List<IPipeline<TNode, TPipeInput, TOutput>> pipelines, DataLoader<TInputData> loader)
        {

            foreach (var individual in individuals) ResetFitness(individual);

            foreach (var batch in loader.Batches())
            {
                for (int i = 0; i < individuals.Count; i++)
                {
                    var individual = individuals.ElementAt(i);
                    var pipeline = pipelines.ElementAt(i);
                    base.EvaluateBatch(individual, batch, pipeline);
                }

                if (DisposeAfterEachBatch)
                {
                    foreach (var item in batch)
                        item.Dispose();

                    // hopefully, this frees all memory used in HOperators where the original value was overwritten
                    // (.e.g when resizing an image we overwrite the input variable, which,
                    // according to the documentation, causes the original value still to linger in memory
                    // until the garbage collector decides to make a visit, which he won't, because pointers
                    // are too tiny.)
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
            }

            // average over number of items
            foreach (var individual in individuals)
            {
                foreach (var fitFunc in FitnessConfiguration.FitnessFunctions)
                    individual.Fitness[fitFunc] /= loader.DataSetSize;
            }

            OnLoaderEvaluationCompleted(new EvaluatorCgpEvaluationEventArgs
            {
                DataLoader = loader,
                EvaluatedIndividuals = individuals,
                Pipelines = pipelines
            });
        }

        public void Evaluate(IPipeline<TNode, TPipeInput, TOutput> pipeline)
        {
            EvaluateLoader(new List<IIndividual> { pipeline }, new List<IPipeline<TNode, TPipeInput, TOutput>>() { pipeline }, TrainDataLoader);
        }

    }
}
