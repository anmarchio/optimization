using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Extensions;
using HalconDotNet;
using Newtonsoft.Json;
using Optimization.CartesianGeneticProgramming;
using Optimization.EvolutionStrategy;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;
using Optimization.Fitness.ErrorHandling;
using Optimization.HPipeline.Fitness;
using Optimization.HalconPipeline;
using Optimization.HalconPipeline.Interfaces;
using Collection = Optimization.HPipeline.Fitness.Collection;

namespace Optimization.HPipeline
{
    public class HalconBatchRun : BatchRun
    {

        public HalconBatchRun(IEvolutionStrategy evolutionStrategy, CGPConfiguration CGPConfig, ESConfiguration ESConfig, FitnessConfiguration FitConfig, int iterations, string saveDirectory, int seed = 0)
            : base(evolutionStrategy, new List<IConfiguration>() {CGPConfig, ESConfig, FitConfig }, saveDirectory, iterations)
        {
            Initialize(evolutionStrategy, new List<IConfiguration>() { CGPConfig, ESConfig, FitConfig }, saveDirectory, iterations, seed);

            ImagesDirectory = Path.Combine(SaveDirectory, "Images");

            Configurations.Add(CGPConfig);
            Configurations.Add(ESConfig);
            Configurations.Add(FitConfig);

            for (int i = 0; i < iterations; i++)
            {
                var p = Path.Combine(ImagesDirectory, i.ToString());
                if (!Directory.Exists(p))
                    Directory.CreateDirectory(p);
            }

            LoggingActions.Add(LogHalconBestPipelinesByIteration);
            LoggingActions.Add(LogHalconPipelineEvolution);
            LoggingActions.Add(LogAnalyzers);
            LoggingActions.Add(LogTime);
        }


        private static void SerializePipelines(LoggingObject obj)
        {
            var batch = obj.BatchRun;
            var gridDir = batch.GridDirectory;

            var best = obj.EvolutionStrategy.Best;
            var config = obj.BatchRun.Configurations.Where(x => x.ConfigurationType == ConfigurationType.CGP).First() as CGPConfiguration;
            var map = config.OperatorMap as IOperatorEncoder;
            if (map == null) return;
            if (config == null) return;
            var pipe = new HalconPipeline(best.FloatVector, config);
            var path = Path.Combine(gridDir, obj.Iteration.ToString(), "pipeline.xml");
            if (pipe.SerializeXmlSupported)
            {
                pipe.SerializeXml(path);
            }
        }

        /// <summary>
        /// loggable object exposes the batch runs EvolutionStrategy as loggable.EvolutionStrategy
        /// and the current iteration as loggable.Iteration
        /// </summary>
        /// <param name="loggable"></param>
        public static void LogHalconPipelineEvolution(LoggingObject loggable)
        {
            var ES = loggable.EvolutionStrategy;
            var best = ES.Best;
            var batch = loggable.BatchRun as BatchRun;
            var CGPConfig = batch.Configurations.First(x => x.ConfigurationType == ConfigurationType.CGP) as CGPConfiguration;
            var FitConfig = batch.Configurations.First(x => x.ConfigurationType == ConfigurationType.Fitness) as HalconFitnessConfiguration;
            var refSet = FitConfig.ValidationSet ?? FitConfig.ReferenceSet;

            var plog = Path.Combine(batch.GridDirectory, loggable.Iteration.ToString());

            Logger.PrintGrid(best.FloatVector, CGPConfig, plog, false, null, true, false);
            Logger.PrintVector(best.FloatVector, CGPConfig, plog);
            //Logger.PrintIterationVector(best.FloatVector, CGPConfig, plog, generationNumber);

            var opEncoder = CGPConfig.OperatorMap as IOperatorEncoder;
            if (opEncoder != null)
            {
                var pipeline = new HalconPipeline(best.FloatVector, CGPConfig);
                var dict = new Dictionary<string, Dictionary<string, object>>();

                using (var writer = new StreamWriter(Path.Combine(batch.ImagesDirectory,
                    loggable.Iteration.ToString(), "ConfusionMatrix.txt"), append: true))
                {
                    for (int i = 0; i < refSet.Count; i++)
                    {
                        using (var image = refSet[i])
                        {
                            LogConfusionMatrix(pipeline, image, writer);
                            dict.Add(Path.GetFileName(image.Filename), ConfusionMatrixToDictionary(pipeline, image, FitConfig.FitnessFunctions));
                            using (var output = pipeline.ExecuteSingle(image.Image))
                            {
                                using (var hImage = new HImage(image.Image))
                                {
                                    hImage.Dump(
                                        Path.Combine(batch.ImagesDirectory, loggable.Iteration.ToString(),
                                            Path.GetFileName(image.Filename)),
                                        reference: image.ReferenceRegions, actual: output);
                                }
                            }
                        }
                    }
                }
                // write ConfusionMatrix.json for all images in the images directory of each batch run iteration
                using (var writer = new StreamWriter(Path.Combine(batch.GridDirectory, batch.ImagesDirectory,
                    loggable.Iteration.ToString(), "ConfusionMatrix.json")))
                {
                    writer.WriteLine(JsonConvert.SerializeObject(dict, Formatting.Indented));
                }

                try
                {
                    pipeline.WriteToDOTFile(Path.Combine(plog, "pipeline.txt"));

                }
                catch (OperatorException ex)
                {
                    Console.WriteLine(ex.Message, ex.StackTrace);
                    Serilog.Log.Logger.Error(ex, "{Exception}");
                }
            }
        }

        // ==================>
        // ========> TEST THIS
        // ==================>
        public static void LogHalconBestPipelinesByIteration(LoggingObject loggable)
        {
            Console.WriteLine("LogHalconBestPipelinesByIteration ...");
            var ES = loggable.EvolutionStrategy;
            if(ES.Configuration.LogGenBestIndividuals)
            { 
                // Convert ConcurrentQueue to an array for easier iteration.
                var batchRunsIndividuals = Enumerable.Range(0, loggable.BatchRun.GenBestIndividuals.Count)
                    .ToDictionary(x => x, x => loggable.BatchRun.GenBestIndividuals.ElementAt(x).Item2);

                var batch = loggable.BatchRun as BatchRun;
                var CGPConfig = batch.Configurations.First(x => x.ConfigurationType == ConfigurationType.CGP) as CGPConfiguration;
                var FitConfig = batch.Configurations.First(x => x.ConfigurationType == ConfigurationType.Fitness) as HalconFitnessConfiguration;
                var refSet = FitConfig.ValidationSet ?? FitConfig.ReferenceSet;

                Console.WriteLine($"Writing individuals: {batchRunsIndividuals.Count()} ...");

                for (int i = 0; i < batchRunsIndividuals.Count(); i++)
                {
                    // Access the individual elements of the current item.                      
                    var listOfBest = batchRunsIndividuals[i]; // Tuple<int, List<IIndividual>>
                    try
                    {
                        for (int j = 0; j < listOfBest.Count(); j++)
                        {
                            if (CGPConfig == null)
                            {
                                Console.WriteLine($"ERROR: CGP Configuration not found for Gen {j} in Batchrun {i}!");
                                continue;
                            }

                            var plog = Path.Combine(batch.GridDirectory, loggable.Iteration.ToString());

                            // Create the pipeline if OperatorEncoder is present
                            var opEncoder = CGPConfig.OperatorMap as IOperatorEncoder;
                            if (opEncoder != null)
                            {
                                var pipeline = new HalconPipeline(listOfBest[j].FloatVector, CGPConfig);

                                var filename = Path.Combine(plog, "pipelines",
                                    $"pipeline_{j + 1}_of_{listOfBest.Count()}_in_Run_{i}.txt");

                                // Ensure the directory exists
                                Directory.CreateDirectory(Path.GetDirectoryName(filename) ?? string.Empty);

                                Console.WriteLine($"Run {i}, Gen{j}: Writing dot pipeline for Gen {j} Run {loggable.Iteration.ToString()} to {filename}");
                                pipeline.WriteToDOTFile(filename);
                            }
                        }
                    }
                    catch (OperatorException ex)
                    {
                        // Log and handle exceptions related to operators
                        Console.WriteLine($"OperatorException: {ex.Message}");
                        Console.WriteLine(ex.StackTrace);
                        Serilog.Log.Logger.Error(ex, "Error while processing generation");
                    }
                    catch (Exception ex)
                    {
                        // Log and handle any generic exceptions
                        Console.WriteLine($"Exception: {ex.Message}");
                        Console.WriteLine(ex.StackTrace);
                        Serilog.Log.Logger.Error(ex, "Unexpected error occurred");
                    }
                }
            }
        }

        public static void LogConfusionMatrix(HalconPipeline pipeline, ReferenceImage image, StreamWriter writer, IEnumerable<FitnessFunction> fitnessFunctions = null)
        {
            try
            {
                if (fitnessFunctions == null)
                    fitnessFunctions = new[] {FitnessFunction.IntersectionOverUnion, FitnessFunction.MCC};
                using (HObject output = pipeline.ExecuteSingle(image.Image))
                {

                    int truePositives, trueNegatives, falsePositives, falseNegatives;
                    Collection.ConfusionMatrix(output, image.Reference, image.Height, image.Width, out truePositives,
                        out trueNegatives, out falsePositives, out falseNegatives);

                    writer.WriteLine($"Image: {image.Filename}");
                    foreach (var fitFunc in fitnessFunctions)
                    {
                        writer.WriteLine($"{fitFunc}: {image.ComputeFitness(output, fitFunc)}");
                    }
                    writer.WriteLine($"True positives: {truePositives}");
                    writer.WriteLine($"True negatives: {trueNegatives}");
                    writer.WriteLine($"False positives: {falsePositives}");
                    writer.WriteLine($"False negatives: {falseNegatives}");
                    writer.WriteLine(
                        $"Reference image height: {image.Height}, width: {image.Width}, total: {image.Height * image.Width}");
                    writer.WriteLine();
                }
            }
            catch (OperatorException ex)
            {
                Console.WriteLine(ex.Message, ex.StackTrace);
                Serilog.Log.Logger.Error(ex, "{Exception}");
            }
        }

        /// <summary>
        /// Prepares a confusion matrix as dictionary for json serialization.
        /// </summary>
        /// <param name="pipeline">pipeline to execute on image</param>
        /// <param name="image"></param>
        /// <param name="fitnessFunctions">fitness values besides tp, fp, tn, fn to compute</param>
        /// <returns>Dictionary of type {"value name": value, ...}
        /// fit for json serialization</returns>
        public static Dictionary<string, object> ConfusionMatrixToDictionary(HalconPipeline pipeline, ReferenceImage image,
            IEnumerable<FitnessFunction> fitnessFunctions = null)
        {
            if (fitnessFunctions == null)
                fitnessFunctions = new [] {FitnessFunction.IntersectionOverUnion, FitnessFunction.MCC};

            using (HObject output = pipeline.ExecuteSingle(image.Image))
            {
                int truePositives, trueNegatives, falsePositives, falseNegatives;
                Collection.ConfusionMatrix(output, image.Reference, image.Height, image.Width, out truePositives,
                    out trueNegatives, out falsePositives, out falseNegatives);

                var dict = new Dictionary<string, object>();

                dict.Add("true positives", truePositives);
                dict.Add("true negatives", trueNegatives);
                dict.Add("false positives", falsePositives);
                dict.Add("false negatives", falseNegatives);

                foreach (var fitFunc in fitnessFunctions)
                {
                    dict.Add(fitFunc.ToString(), image.ComputeFitness(output, fitFunc));
                }

                dict.Add("height", image.Height);
                dict.Add("width", image.Width);
                dict.Add("size total", image.Height * image.Width);

                return dict;
            }
        }

        public static List<Action<LoggingObject>> HalconLoggingActions()
        {
            return new List<Action<LoggingObject>>()
            {
                LogHalconBestPipelinesByIteration,
                LogHalconPipelineEvolution,
                LogAnalyzers,
                LogTime,
                SerializePipelines,
                LogLegend,
                LogAppendHalconPipeline
            };
        }

        /// <summary>
        /// Tries appending pipelines to the best results of each evolution strategy
        /// and logs the results
        /// </summary>
        /// <param name="obj"></param>
        private static void LogAppendHalconPipeline(LoggingObject obj)
        {
            var evaluator = obj.EvolutionStrategy.Evaluator as HalconPipelineCGPEvaluator;
            if (evaluator == null) return;

            var cgpConfig =
                (CGPConfiguration) obj.BatchRun.Configurations.FirstOrDefault(x =>
                    x.ConfigurationType == ConfigurationType.CGP);
            var fitConfig =
                (FitnessConfiguration) obj.BatchRun.Configurations.FirstOrDefault(x =>
                    x.ConfigurationType == ConfigurationType.Fitness) as FitnessConfiguration;
            if (fitConfig == null) return;
            var best = new HalconPipeline(obj.EvolutionStrategy.Best.FloatVector, cgpConfig);

            evaluator.Evaluate(best);

            var refSet = evaluator.TrainDataLoader.DataSet as ReferenceSet;
            if (refSet == null) return;

            Tuple<int, int> minMax = refSet.GetMinAndMaxAreaSize();

            var appendPipelines = new List<HalconPipeline>()
            {
                CommonHalconPipelines.GetOpening(5, 5),
                CommonHalconPipelines.GetClosing(5, 5),
                CommonHalconPipelines.GetSelectShape(minMax.Item1, minMax.Item2)
            };

            var concatenation = evaluator.EvaluateConcatenation(best, appendPipelines);

            if (fitConfig.WeightedFitnessOf(concatenation) > fitConfig.WeightedFitnessOf(best))
                best = concatenation;

            // log best xml, confusion matrix
            var dict = new Dictionary<string, Dictionary<string, object>>();
            for (int i = 0; i < refSet.Count; i++)
            {
                using (var image = refSet[i])
                {
                    dict.Add(Path.GetFileName(image.Filename), ConfusionMatrixToDictionary(best, image, fitConfig.FitnessFunctions));
                }
            }

            // write ConfusionMatrix.json for all images in the images directory of each batch run iteration
            using (var writer = new StreamWriter(Path.Combine(obj.BatchRun.GridDirectory, obj.BatchRun.ImagesDirectory,
                obj.Iteration.ToString(), "AppendPipelineConfusionMatrix.json")))
            {
                writer.WriteLine(JsonConvert.SerializeObject(dict, Formatting.Indented));
            }

            var plog = Path.Combine(obj.BatchRun.GridDirectory, obj.Iteration.ToString());
            best.SerializeXml(Path.Combine(plog, "append_pipeline.xml"));
            best.WriteToDOTFile(Path.Combine(plog, "append_pipeline.txt"));
        }
    }
}
