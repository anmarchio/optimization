using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Optimization.CartesianGeneticProgramming;
using Optimization.CartesianGeneticProgramming.Mutators;
using Optimization.Data;
using Optimization.EvolutionStrategy;
using Optimization.EvolutionStrategy.Analyzers;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Random;
using Optimization.EvolutionStrategy.Selectors;
using Optimization.EvolutionStrategy.Terminators;
using Optimization.Fitness;
using Optimization.HPipeline.Fitness;

namespace Optimization.HPipeline
{
    public static class CommonHalconEvolutionStrategies
    {
        /// <summary>
        /// Builds a generic batch run for a standard evolution strategy for CGP
        /// </summary>
        /// <param name="CGPconfig"></param>
        /// <param name="refSet"></param>
        /// <param name="valSet"></param>
        /// <param name="generations">How many generations to run each evolution strategy</param>
        /// <param name="iterations">number of evolution strategies (iterations of the batch run)</param>
        /// <param name="saveDirectory"></param>
        /// <param name="seed"></param>
        /// <param name="worker"></param>
        /// <param name="fitnessFunction">If fitnessFunctions is null, then this one is used, defaults to MCC</param>
        /// <param name="batchSize">how many reference images are contained in each batch. Is only relevant for memory, as every individual gets evaluated on each batch in each iteration</param>
        /// <param name="queueSize">How many batches can be pre-fetched</param>
        /// <param name="executionTimeThreshold"></param>
        /// <param name="fitnessFunctions"></param>
        /// <param name="weights">if null, all get equal weight 1</param>
        /// <returns></returns>
        public static BatchRun BuildStandardCGPEvolutionStrategy(CGPConfiguration CGPconfig, ReferenceSet refSet, ReferenceSet valSet,
            int generations, string saveDirectory, int iterations, int seed = 0, int parallelDegree = 1, BackgroundWorker worker = null,
            FitnessFunction fitnessFunction = FitnessFunction.MCC,
            int? batchSize=null, int? queueSize = null, int? executionTimeThreshold = 1000, FitnessFunction[] fitnessFunctions = null, double[] weights = null)
        {
            var random = new SystemRandom(seed);
            var decoder = new CGPDecoder(CGPconfig);
            //var mutator = new CGPStandardMutator(random, CGPconfig, decoder, true);
            var mutator = new SinglePassMutator(random, CGPconfig);
            var procrSelector = new RandomSelector(random);
            var terminator = new GenerationCountTerminator(generations);
            var ESconfig = new ESConfiguration(1, 4, 0, true);
            
            if (fitnessFunctions == null || !fitnessFunctions.Any())
            {
                fitnessFunctions = new[] { fitnessFunction };
                weights = new double[] {1};
            }
            else if (weights == null || weights.Length == 0)
            {
                weights = Enumerable.Repeat(1d, fitnessFunctions.Length).ToArray();
            }

            var trainDataLoader = new DataLoader<ReferenceImage>(refSet, batchSize: batchSize ?? refSet.Count) { QueueSize = queueSize ?? 8 };
            var valDataLoader = new DataLoader<ReferenceImage>(valSet, batchSize: batchSize?? valSet.Count) { QueueSize = queueSize ?? 8 };
            var fitConfig = new HalconFitnessConfiguration(refSet, fitnessFunctions.ToArray(), weights.ToArray(),
                4, regionCountThreshold: null, executionTimeThreshold: executionTimeThreshold, pixelPercentageThreshold: 0.7f);
            var evaluator = new HalconPipelineCGPEvaluator(CGPconfig, trainDataLoader, valDataLoader, fitConfig);
            var surivalSelector = new BestSelector(fitConfig);

            var analyzer = new MultiAnalyzer(
                new List<Analyzer>
            {
                new ConsoleStatusUpdateAnalyzer(),
                new FitnessValueAnalyzer(),
                new EvaluatorAnalyzer<ReferenceImage>()
            });

            ICreator creator;
            if(CGPconfig.StatusQuo != null)
                creator = new CGPFloatVectorCreator(random, CGPconfig, standardSolution: CGPconfig.StatusQuo.FloatVector);
            else
                creator = new CGPValidFloatVectorCreator(random, CGPconfig, evaluator, 100);

            var ES = new EvolutionStrategy.EvolutionStrategy(creator, mutator, surivalSelector, procrSelector, evaluator, terminator, analyzer, ESconfig, fitConfig);
            var batch = new BatchRun(ES, new List<IConfiguration> { CGPconfig, ESconfig, fitConfig }, saveDirectory, iterations, seed: seed, paralleldegree: parallelDegree);
            foreach (var action in HalconBatchRun.HalconLoggingActions())
                batch.RegisterLoggingAction(action);
            return batch;
        }

        /// <summary>
        /// Self adaptive version of BuildStandardCGPEvolutionStrategy. Mutation probability is increased if no fit individuals are found.
        /// </summary>
        /// <param name="CGPconfig"></param>
        /// <param name="refSet"></param>
        /// <param name="valSet"></param>
        /// <param name="generations"></param>
        /// <param name="iterations"></param>
        /// <param name="saveDirectory"></param>
        /// <param name="seed"></param>
        /// <param name="worker"></param>
        /// <param name="fitnessFunction"></param>
        /// <param name="batchSize"></param>
        /// <param name="queueSize"></param>
        /// <param name="executionTimeThreshold"></param>
        /// <param name="fitnessFunctions"></param>
        /// <param name="weights"></param>
        /// <returns></returns>
        public static BatchRun SelfAdaptiveEvolutionStrategy(CGPConfiguration CGPconfig, ReferenceSet refSet, ReferenceSet valSet,
            int generations, int iterations, string saveDirectory, int seed = 0, int parallelDegree = 1,
            FitnessFunction fitnessFunction = FitnessFunction.MCC, int?
                batchSize = null, int? queueSize = null, int? executionTimeThreshold = 1000,
            FitnessFunction[] fitnessFunctions = null, double[] weights = null)
        {
            var random = new SystemRandom(seed);
            var mutator = new SinglePassMutator(random, CGPconfig);
            var procrSelector = new RandomSelector(random);
            var terminator = new GenerationCountTerminator(generations);
            var ESconfig = new ESConfiguration(1, 4, 0, true);

            var trainDataLoader = new DataLoader<ReferenceImage>(refSet, batchSize: batchSize ?? refSet.Count) { QueueSize = queueSize ?? 8};
            var valDataLoader = new DataLoader<ReferenceImage>(valSet, batchSize: batchSize ?? valSet.Count) { QueueSize = queueSize ?? 8 };

            if (fitnessFunctions == null || !fitnessFunctions.Any())
            {
                fitnessFunctions = new[] { fitnessFunction };
                weights = new double[] { 1 };
            }
            else if (weights == null || weights.Length == 0)
            {
                weights = Enumerable.Repeat(1d, fitnessFunctions.Length).ToArray();
            }

            var fitConfig = new HalconFitnessConfiguration(refSet,
                fitnessFunctions.ToArray(),
                weights.ToArray(), 
                regionCountThreshold: null,
                pixelPercentageThreshold: 0.7f,
                executionTimeThreshold: executionTimeThreshold)
            {
                ReferenceSet = refSet,
                ValidationSet = valSet
            };

            var surivalSelector = new BestSelector(fitConfig);

            var evaluator = new HalconPipelineCGPEvaluator(CGPconfig, trainDataLoader, valDataLoader, fitConfig);

            var analyzer = new MultiAnalyzer(
                new List<Analyzer>
            {
                new ConsoleStatusUpdateAnalyzer(),
                new FitnessValueAnalyzer(),
                new MutationProbabilityAnalyzer(),
                new EvaluatorAnalyzer<ReferenceImage>()
            });

            ICreator creator;
            if(CGPconfig.StatusQuo != null)
                creator = new CGPFloatVectorCreator(random, CGPconfig, CGPconfig.StatusQuo.FloatVector);
            else
                creator = new CGPValidFloatVectorCreator(random, CGPconfig, evaluator, 100);

            var ES = new SelfAdaptiveEvolutionStrategy(creator, mutator, surivalSelector, procrSelector, evaluator, terminator, analyzer, ESconfig, fitConfig);
            var batch = new BatchRun(ES, new List<IConfiguration> { CGPconfig, ESconfig, fitConfig }, saveDirectory, iterations, seed: seed, paralleldegree: parallelDegree);
            foreach (var action in HalconBatchRun.HalconLoggingActions()) batch.RegisterLoggingAction(action);
            return batch;
        }
    }
}
