using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Optimization.CartesianGeneticProgramming;
using Optimization.CartesianGeneticProgramming.Mutators;
using Optimization.CVPipeline.CVCGP;
using Optimization.EvolutionStrategy;
using Optimization.EvolutionStrategy.Analyzers;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Random;
using Optimization.EvolutionStrategy.Selectors;
using Optimization.EvolutionStrategy.Terminators;
using Optimization.Fitness;
using Optimization.Pipeline.Interfaces;

namespace Optimization.CVPipeline
{
    public class CommonCVEvolutionStrategies
    {
        public static BatchRun BuildStandardCVEvolutionStrategy(CVDataLoader train, CVDataLoader val, List<CVNode> nodes, int generations, int seed, int iterations, string saveDirectory, BackgroundWorker worker = null, bool logImages = true)
        {
            var modularOperatorMap = new CVOperatorMap(nodes/*, DependencyTree.ImagesOnly()*/);
            CGPConfiguration CGPconfig;
            CGPconfig = new CGPConfiguration(3, modularOperatorMap.Dependencies.Nodes.Count, 1, nodes.Max(x => x.CGPInputCount), nodes.Max(x => x.CGPParameterCount), modularOperatorMap, 1, 1);

            return BuildStandardCVEvolutionStrategy(train, val, CGPconfig, generations, seed, iterations, saveDirectory, worker, logImages);
        }

        public static BatchRun BuildStandardCVEvolutionStrategy(CVDataLoader train, CVDataLoader val, CGPConfiguration config, int generations, int seed, int iterations, string saveDirectory, BackgroundWorker worker = null, bool logImages = true)
        {
            var random = new MersenneTwister(seed);
            var map = config.OperatorMap as CVOperatorMap;
            if (map == null) throw new ArgumentException("expected CVOperatorMap in CGPCOnfiguration.OperatorMap");
            //FloatVector vector;
            //pipeline.ToCGPEncoding(modularOperatorMap, out vector, out CGPconfig);
            var decoder = new CGPDecoder(config);
            var mutator = new CGPStandardMutator(random, config, decoder, true);
            var procrSelector = new RandomSelector(random);
            var terminator = new GenerationCountTerminator(generations);
            var ESconfig = new ESConfiguration(mu: 1, lambda: 4, rho: 0, plus: true);
            var fitnessConfiguration = new FitnessConfiguration();
            var surivalSelector = new BestSelector(fitnessConfiguration);

            var evaluator = new CVCGPEvaluator(train, val, decoder, config, fitnessConfiguration);

            var analyzers = new List<Analyzer>();
            analyzers.Add(new ConsoleStatusUpdateAnalyzer());
            //analyzers.Add(new MultipleFitnessValuesAnalyzer(fitConfig));
            analyzers.Add(new FitnessValueAnalyzer());

            var analyzer = new MultiAnalyzer(analyzers);
            var creator = new CGPFloatVectorCreator(random, config);

            var ES = new EvolutionStrategy.EvolutionStrategy(creator, mutator, surivalSelector, procrSelector, evaluator, terminator, analyzer, ESconfig, fitnessConfiguration);
            var batch = new BatchRun(ES, new List<IConfiguration> { config, ESconfig }, saveDirectory, iterations);
            if(logImages)
                batch.RegisterLoggingAction(LogImages);
            batch.RegisterLoggingAction(BatchRun.LogAnalyzers);
            batch.RegisterLoggingAction(LogDot);
            batch.RegisterLoggingAction(BatchRun.LogLegend);
            return batch;
        }


        private static void LogDot(BatchRun.LoggingObject obj)
        {
            var batch = obj.BatchRun;
            var gridDir = batch.GridDirectory;

            var best = obj.BatchRun.BestIndividuals.ToList()[obj.Iteration].Item2;
            var config = obj.BatchRun.Configurations.Where(x => x.ConfigurationType == ConfigurationType.CGP).First() as CGPConfiguration;
            var map = config.OperatorMap as IOperatorEncoder;
            if (map == null) return;
            if (config == null) return;
            var pipe = new CVPipeline(best.FloatVector, config);
            if (pipe.SerializeXmlSupported)
                pipe.SerializeXml(Path.Combine(gridDir, obj.Iteration.ToString(), "pipeline.xml"));
            
            pipe.WriteToDOTFile(Path.Combine(gridDir, obj.Iteration.ToString(), "pipeline.txt"));
        }


        private static void LogImages(BatchRun.LoggingObject logging)
        {
            var batch = logging.BatchRun;
            var best = batch.BestIndividuals.ToList()[logging.Iteration].Item2;
            var imgDir = logging.BatchRun.ImagesDirectory;

            var evaluator = batch.EvolutionStrategy.Evaluator as CVCGPEvaluator;
            if (evaluator == null) return;
            var data = evaluator.TrainDataLoader;
            //var pipeline = new CVPipeline(evaluator.Configuration.OperatorMap as ModularLegacyOperatorMap<IInputArray[], float[], IInputArray>, evaluator.Decoder, best.FloatVector, evaluator.Configuration);
            var pipeline = new CVPipeline(best.FloatVector, evaluator.Configuration);

            foreach (var b in data.Batches())
            {
                foreach(var item in b)
                {
                    var output = pipeline.ExecuteSingle(item.Image);
                    item.Image.WriteImage(Path.Combine(imgDir, logging.Iteration.ToString(), item.Name), item.ReferenceImage, output);
                }
            }
            
        }
    }
}
