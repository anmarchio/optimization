using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.CartesianGeneticProgramming;
using Optimization.Data;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;
using Optimization.Pipeline;

namespace Optimization.HPipeline.Fitness
{
    public class HalconPipelineCGPEvaluator : PipelineEvaluator<ReferenceImage, HObject, HalconPipeline, HalconOperatorNode, HObject, HObject>
    {
        public HalconPipelineCGPEvaluator(CGPConfiguration configuration, DataLoader<ReferenceImage> trainDataLoader, DataLoader<ReferenceImage> valDataLoader, HalconFitnessConfiguration fitConfig)
            : base(configuration, trainDataLoader, valDataLoader, fitConfig)
        {
        }

        protected HalconPipelineCGPEvaluator(DataLoader<ReferenceImage> trainDataLoader,
            DataLoader<ReferenceImage> valDataLoader,
            HalconFitnessConfiguration fitConfig) : base(trainDataLoader, valDataLoader, fitConfig)
        {

        }

        /// <summary>
        /// concatenates the two pipelines and evaluates them against the trainDataLoader
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="appendPipeline"></param>
        /// <returns>weighted sum of fitness according to fitConfig</returns>
        public HalconPipeline EvaluateConcatenation(HalconPipeline pipeline,
            HalconPipeline appendPipeline)
        {
            var concatenation = new HalconPipeline(pipeline.Concatenate(appendPipeline).OutputNodes.ToArray());
            Evaluate(concatenation);
            return concatenation;
        }

        /// <summary>
        /// Tries appending all pipelines in appendPipelines to pipeline and checks which yields the best results
        /// returns a new pipeline using the concatination of both pipelines
        ///
        /// caution: only works for pipelines with single output and input nodes
        /// </summary>
        /// <param name="pipeline"></param>
        /// <param name="appendPipelines"></param>
        /// <returns></returns>
        public HalconPipeline EvaluateConcatenation(HalconPipeline pipeline, List<HalconPipeline> appendPipelines)
        {
            var concatPipelines = new List<HalconPipeline>();

           foreach(var appendPipeline in appendPipelines)
                 concatPipelines.Add(EvaluateConcatenation(pipeline, appendPipeline));
            

            if (FitnessConfiguration.Maximization)
                return concatPipelines.Select(x => Tuple.Create(x, FitnessConfiguration.WeightedFitnessOf(x)))
                    .OrderByDescending(y => y.Item2).First().Item1;
            
            return concatPipelines.Select(x => Tuple.Create(x, FitnessConfiguration.WeightedFitnessOf(x)))
                    .OrderBy(y => y.Item2).First().Item1;
            
        }

        public override ICopyable Copy()
        {
            return new HalconPipelineCGPEvaluator(Configuration, TrainDataLoader, ValidationDataLoader, (HalconFitnessConfiguration)FitnessConfiguration);
        }
    }
}
