using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using CommandLine;
using Emgu.CV.CvEnum;
using Extensions;
using HalconDotNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Optimization.CartesianGeneticProgramming;
using Optimization.CVPipeline;
using Optimization.CVPipeline.CVCGP;
using Optimization.Data;
using Optimization.EvolutionStrategy;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Random;
using Optimization.Fitness;
using Optimization.Fitness.OperatorMaps;
using Optimization.HPipeline;
using Optimization.HPipeline.Fitness;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.HPipeline.Serialization;
using Optimization.Pipeline;
using Optimization.Pipeline.Interfaces;
using Serilog;
using Serilog.Core;

namespace Optimization.Commandline
{
    public class Program
    {
        [Verb("batch", HelpText = "Starts a batch run of evolution strategies.")]
        public class BatchOptions
        {
            #region BatchRun configuration
            [Option("verbose", Required = false,
                HelpText = "Print more detailed output.")]
            public bool Verbose { get; set; }

            [Option("backend", Required = true,
                HelpText = "Computer vision backend to be used for the optimization. Currently supported: [halcon, emgucv (OpenCV)]")]
            public Backend Backend { get; set; }

            [Option("runs", Required = true,
                HelpText = "The number of batch runs.")]
            public int Runs { get; set; }

            [Option("val-data-dir", Required = false,
                HelpText = "The validation data directory.")]
            public string ValidationDataDirectory { get; set; }

            [Option("train-data-dir", Required = true,
                HelpText = "The training data directory.")]
            public string TrainingDataDirectory { get; set; }

            [Option("results-dir", Required = false,
                HelpText = "The output directory. Defaults to working directory/results<timestamp>")]
            public string ResultDirectory { get; set; }

            [Option("evo-type", Required = false,
                HelpText = "The evolution strategy type. Currently supports standard and selfadaptive", Default = EvolutionStrategyType.standard)]
            public EvolutionStrategyType EvolutionStrategyType { get; set; }

            [Option("seed", Required = false,
                HelpText = "Random seed to be used. Helps replicating results.")]
            public int? Seed { get; set; }

            [Option("batch-size", Required = false,
                HelpText = "The number of images considered 'a batch'. If indvidual items in your dataset are large, try using a low value.")]
            public int? BatchSize { get; set; }

            [Option("queue-size", Required = false, Hidden = true,
                HelpText = "The number of batches that are loaded into the queue.")]
            public int? QueueSize { get; set; }

            [Option("exec-thresh", Required = false, Default = -1,
                HelpText = "Execution time threshold for each pipeline for a single image in milliseconds. Set -1 to ignore entirely.")]
            public int? ExecutionTimeThreshold { get; set; }

            [Option("fits-mem", Required = false, Default = false,
                HelpText = "Set this to true if the datasets easily fit in memory." +
                         " This will prevent overhead from loading images for each individual and releasing memory immediately afterwards.")]
            public bool FitsInMemory { get; set; }

            [Option("fit-funcs",  Required = false,
                HelpText = "The fitness functions you would like to optimize against. For a list of options use fit-funcs verb.")]
            public IEnumerable<FitnessFunction> FitnessFunctions { get; set; }

            [Option("fit-func", Required = false, Default = Fitness.FitnessFunction.MCC,
                HelpText = "The fitness function you would like to optimize against. Will be overwritten by fit-funcs. For a list of options use fit-funcs verb.")]
            public FitnessFunction FitnessFunction { get; set; }

            [Option("weights", Required = false,
                HelpText = "The weights in the order as they should be applied to fit-funcs")]
            public IEnumerable<double> Weights { get; set; }

            [Option("bounding-box", Required = false, Default = false, Hidden = true,
                HelpText = "If your data consists of masks for semantic segmentation, this automatically computes the bounding box on both the input data as well as the individual pipeline's results.")]
            public bool BoundingBox { get; set; }

            [Option("search-space", Required = false, Default = SearchSpaceType.Full,
                HelpText = "Switch the search space. Options are: Full, Parameters. In ParametersOnly, if you load an existing pipeline," +
                           " only it's operator's parameters are mutated, not the structure of the pipeline")]
            public SearchSpaceType SearchSpace { get; set; }

            [Option("status-quo", Required = false,
                HelpText = "Use in conjunction with search space: Parameters. This is the pipeline which's parameters will be optimized.")]
            public string StatusQuoXml { get; set; }
            #endregion

            #region evolution strategy configuration
            [Option("generations", Required = true,
                HelpText = "The number of generations for each batch run.")]
            public int Generations { get; set; }

            #endregion

            #region parallelization configuration
            [Option(longName: "worker-count", Required = false,
                HelpText = "The number of workers running evolutionstrategies in parallel. Defaults to 1." +
                "If the number of cores allows it, it is advised to use a higher worker-count and adjust the parallelization.",
                Default = 1)]
            public int WorkerCount { get; set; }

            [Option("use-halcon-parallelization", Required =false,
                HelpText ="Turn Halcon's parallelization on operator lvl on or off, Defaults to true, so halcon parallelization is used." +
                "Please enter false to turn the parallelization completly off.",
                Default = true)]
            public bool Parallelization { get; set; }
            #endregion

            #region CGP Configuration
            [Option(longName: "cgp-rows", Default = 10, Hidden = true, Required = false,
                HelpText = "The number of rows of the CGP grid.")]
            public int CgpRows { get; set; }

            [Option(longName: "cgp-cols", Default = 10, Hidden = true, Required = false,
                HelpText = "The number of columns of the CGP grid.")]
            public int CgpColumns { get; set; }

            [Option(longName: "cgp-node-input-count", Hidden = true, Required = false,
                HelpText = "The maximum number of inputs of all nodes.")]
            public int? CgpNodeInputCount { get; set; }

            [Option(longName: "cgp-levels-back", Default = 1, Hidden = true,
                HelpText = "The maximum levels back (in terms of columns) any given operator may take its input from.")]
            public int CgpLevelsBack { get; set; }

            [Option(longName: "cgp-parameter-count", Hidden = true,
                HelpText = "The maximum number of parameters of all nodes.")]
            public int? CgpParameterCount { get; set; }

            [Option(longName: "cgp-program-input-count", Hidden = true, Required = false, Default = 1,
                HelpText = "The total number of program inputs (i.e. pipeline inputs).")]
            public int CgpProgramInputCount { get; set; }

            [Option(longName: "cgp-program-output", Hidden = true, Required = false, Default = 1,
                HelpText = "The total number of program outputs (i.e. pipeline inputs).")]
            public int CgpProgramOutputCount { get; set; }

            #endregion

            #region config-file options

            [Option("operators", Required = false, HelpText = "Path to a .xml file containing the allowed operators.")]
            public string Operators { get; set; }

            #endregion

        }

        [Verb("operators",
            HelpText = "Prints lists of operators that can be used to restrict the allowed operators in operator maps used by batch runs.")]
        public class OperatorOptions
        {
            [Option("out-dir", Required = false, HelpText = "The directory to print the operator lists in. Defaults to current working directory.")]
            public string OutDirectory { get; set; }

            [Option("backend", Required = true,
                HelpText = "Computer vision backend to be used for the optimization." +
                           " Prints a list of all available operators. This can then be altered and loaded when starting a batch run." +
                           " Currently supported: [halcon, emgucv (OpenCV)]")]
            public Backend Backend { get; set; }

            [Option('p', "pipeline", Required = false, HelpText = "Name of a pipeline whose operators you want to use.")]
            public string Pipeline { get; set; }


            [Option("filename", Required = false, Default = "operators.xml", HelpText = "The filename. If absolute path, overrides out-dir option.")]
            public string Filename { get; set; }
        }

        [Verb("convert",
        HelpText = "Convert input images formats from indexed images to .hobjs and vice versa. Supports old and new region marker format, standard parent/images, parent/labels format.")]
        public class ConvertOptions
        {
            [Option("in-dir", HelpText = "Directory of input data.")]
            public string InDirectory { get; set; }

            [Option("out-dir", Required = true, HelpText = "Directory to output converted files.")]
            public string OutDirectory { get; set; }

            [Option("out-format", Required = true, HelpText = "Output format. Currently supported: hobj, indexed")]
            public FormatOption OutFormat { get; set; }
        }

        [Verb("evaluate",
            HelpText = "Evaluate a pipeline on a given dataset.")]
        public class EvaluateOptions
        {
            [Option("pipeline-path", Required = true,
                HelpText = "Path to the pipeline.xml file.")]
            public string PipelinePath { get; set; }

            [Option("data-dir",
                HelpText = @"Directory containing input data in standard format: data-dir\images and data-dir\labels")]
            public string DataDirectory { get; set; }

            [Option(longName: "out-dir", HelpText = @"Directory to store evaluation results in. Defaults to evaluation\<datetime>", Required = false)]
            public string ResultDirectory { get; set; }

            [Option("backend", Required = false, Default = Commandline.Backend.halcon, Hidden = true)]
            public Backend Backend { get; set; }

            [Option(longName: "print-confusion", Required = false, Default = true)]
            public bool LogConfusionMatrix { get; set; }

            [Option("rgb", Required = false,
                HelpText = "RGB Values in form: r1 g1 b1 r2 g2 b2 r3 g3 b3" +
                           "are used for actual, reference, intersection colors in this order.")]
            public IEnumerable<int> RGBValues { get; set; }

            [Option("margin-width", Required = false, Default = 10,
                HelpText = "the width of the margin used to print results into the image")]
            public int MarginWidth { get; set; }

            [Option("fit-funcs", Required = true,
                HelpText = "The fitness functions (as list). Provide weights if you want to apply non-equal weighting.")]
            public IEnumerable<FitnessFunction> FitnessFunctions { get; set; }

            [Option("type", Required = false, Default = "margin",
                HelpText = "margin or fill for regions")]
            public string Type { get; set; }
        }

        [Verb("fit-func", HelpText = "Prints all available fitness functions")]
        public class FitFuncOptions
        {
        }

        [Verb("brute-force", HelpText = "uses brute force optimization to optimize e.g. individual threshold." +
                                        "You can specify a list of which nodes are to be optimized by id." +
                                        "Note that this brute forces all values and may be infeasible to compute." +
                                        "Use with caution.")]
        public class BruteForceOptions
        {
            [Option("pipeline-path", Required = true,
                HelpText = "Path to the pipeline.xml file.")]
            public string PipelinePath { get; set; }

            [Option("data-dir", Required = true,
                HelpText = @"Directory containing input data in standard format: data-dir\images and data-dir\labels")]
            public string DataDirectory { get; set; }

            [Option(longName: "out-dir", HelpText = @"Directory to store evaluation results in. Defaults to evaluation\<datetime>", Required = false)]
            public string ResultDirectory { get; set; }

            [Option("filename", HelpText = "Filename of optimized pipeline.", Default = "optimized.xml")]
            public string Filename { get; set; }

            [Option("backend", Required = false, Default = Commandline.Backend.halcon, Hidden = true)]
            public Backend Backend { get; set; }

            [Option("node-ids", Required = true, Default = null,
                HelpText = "The ids of nodes that are to be optimized.")]
            public IEnumerable<int> NodeIds { get; set; }

            [Option("weights", Required = false,
                HelpText = "Weights for the fitness functions. Provide as float.")]
            public IEnumerable<double> Weights { get; set; }

            [Option("fit-funcs", Required = true,
                HelpText = "The fitness functions (as list). Provide weights if you want to apply non-equal weighting.")]
            public IEnumerable<FitnessFunction> FitnessFunctions { get; set; }

            [Option("fits-mem", Default = true,
                HelpText = "Set to false if dataset does not fit into memory")]
            public bool FitsMemory { get; set; }
        }

        
        public static int Main(string[] args)
        {
            int status = -1;
            try
            {
                var path = Path.Combine("exceptions");
                path.CreateDirectory();

                var commandHash = string.Join(" ", args).GetStringSha256Hash();
                if (!string.IsNullOrEmpty(commandHash))
                {
                    var logfile = Path.Combine(path, commandHash.Substring(0, 8) + "-.txt");
                    Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo
                        .File(logfile, rollingInterval: RollingInterval.Day).CreateLogger();
                    Log.Information(string.Join(" ", args));
                    Log.Information(System.Reflection.Assembly.GetExecutingAssembly().ToString());
                }

                status = Parser.Default.ParseArguments<BatchOptions,
                        OperatorOptions,
                        ConvertOptions,
                        EvaluateOptions,
                        FitFuncOptions,
                        BruteForceOptions>(args)
                    .MapResult(
                        (BatchOptions opts) => RunBatch(opts, args),
                        (OperatorOptions opts) => RunOperators(opts),
                        (ConvertOptions opts) => RunConvert(opts),
                        (EvaluateOptions opts) => RunEvaluate(opts),
                        (FitFuncOptions opts) => RunFitFuncOptions(),
                        (BruteForceOptions opts) => RunBruteForceOptions(opts),
                        errs => 1);

            }
            catch (Exception ex)
            {
                Console.WriteLine(@"Program died, see exceptions/ for infos.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                Log.Fatal(ex, "Program died");
            }
            finally
            {
                Log.CloseAndFlush();
            }

            #if DEBUG
                Console.WriteLine("Press enter to close...");
                Console.ReadLine();
            #endif

            return status;
        }

        /// <summary>
        /// The idea is to use the brute force optimizer to try every possible parameter combination
        /// of all nodes passed as arguments. May take too long to compute, so the user has to handle this with care
        ///
        /// First we load the pipeline, the data and set the fitness functions and weights. Then we brute force all
        /// combinations of parameters of the given nodes and evaluate them on the data. Finally, the best result
        /// is written to an .xml
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static int RunBruteForceOptions(BruteForceOptions o)
        {
            if (!o.Weights.Any())
                o.Weights = Enumerable.Repeat(1.0d, o.FitnessFunctions.Count());
            else if(o.Weights.Count() != o.FitnessFunctions.Count())
            {
                Console.WriteLine("Provide no weights at all or exactly as many weights as fitness functions.");
                return -1;
            }

            if (string.IsNullOrEmpty(o.ResultDirectory)) o.ResultDirectory = "brute-force";
            o.ResultDirectory.CreateDirectory();

            switch (o.Backend)
            {
                case Backend.halcon:
                    var data = new FileListReferenceSet(o.DataDirectory,
                        o.DataDirectory.GetImagesList(),
                        o.DataDirectory.GetImagesList().GetLabelDictionary())
                    {
                        FitsIntoMemory = o.FitsMemory
                    };

                    var pipeline = HalconPipeline.DeserializeXml(o.PipelinePath);
                    if (pipeline.Nodes.Select(x => x.NodeID).Distinct().Count() != pipeline.Nodes.Count)
                        throw new ArgumentException("All nodes must be uniquely identified (NodeID).");

                    var evaluator = new HalconPipelineEvaluator(new DataLoader<ReferenceImage>(data),
                        new DataLoader<ReferenceImage>(data),
                        new HalconFitnessConfiguration(o.FitnessFunctions.ToArray(), o.Weights.ToArray()));

                    var optimizer = new Optimizer<HalconPipeline, HalconOperatorNode>(pipeline, evaluator,
                        pipeline.Nodes.Where(x => o.NodeIds.Select(y => (float)y).Contains(x.NodeID)).ToList());
                    Console.WriteLine($"optimizing: {optimizer.NecessaryEvaluations} combinations. May take an infeasible amount of time.");
                    Console.WriteLine($"best fitness: {optimizer.Optimize()}");

                    foreach (var pair in optimizer.Parameterization)
                    {
                        pipeline.Nodes.FirstOrDefault(x => x.NodeID == pair.Key).FromCGPNodeParameters(pair.Value);
                    }
                    pipeline.SerializeXml(Path.Combine(o.ResultDirectory, o.Filename));
                    break;

                case Backend.emgucv:
                    throw new NotImplementedException();
            }

            return 0;
        }

        private static int RunFitFuncOptions()
        {
            foreach (var fit in Enum.GetNames(typeof(Fitness.FitnessFunction)))
            {
                Console.WriteLine(fit);
            }
            return 0;
        }


        /// <summary>
        /// Initializes and executes a batch run of evolution strategies for a given train and val dataset.
        ///
        /// First we set the logger to log in the results directory, so that we have access to the logs even if we execute this
        /// on a docker image that we want to remove immediately after the batch run is finished.
        ///
        /// Then we initialize the required components for a batch run, such as the cgp configuration and the operator map.
        ///
        /// Finally we execute the batch run and then validate on the validation data.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private static int RunBatch(BatchOptions o, string[] args)
        {
            BatchRun batchRun = null;
            ModularOperatorMap map = null;

            if (o.ResultDirectory == null)
                o.ResultDirectory = Path.Combine(Directory.GetCurrentDirectory(), "results",
                        DateTime.Now.ToString("yyyyMMddHHss"));

            // quick hack: create additional logger in result directory
            var path = Path.Combine(o.ResultDirectory, "exceptions");
            path.CreateDirectory();

            // quick hack 2: create file containing source directory path

            using (var writer = new StreamWriter(Path.Combine(o.ResultDirectory, "source.json")))
            {
                //writer.WriteLine("trainingDataDirectory: " + o.TrainingDataDirectory);
                //writer.WriteLine("validationDataDirectory: " + o.ValidationDataDirectory);
                Dictionary<string, string> jsonDictSrc = new Dictionary<string, string>();
                jsonDictSrc.Add("trainingDataDirectory", o.TrainingDataDirectory);
                jsonDictSrc.Add("validationDataDirectory", o.ValidationDataDirectory);
                List<Dictionary<string, string>> jsonDictSrcArr = new List<Dictionary<string, string>>
                {
                    jsonDictSrc
                };
                var json = JsonConvert.SerializeObject(jsonDictSrcArr, Formatting.Indented);
                writer.WriteLine(json);
            }

            // we want to log each combination of verbs and options in a unique logging file
            var commandHash = string.Join(" ", args).GetStringSha256Hash();
            if (!string.IsNullOrEmpty(commandHash)) // prevent crash if no verb was specified
            {
                var logfile = Path.Combine(path, commandHash.Substring(0, 8) + "-.txt");
                Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo
                    .File(logfile, rollingInterval: RollingInterval.Day).CreateLogger();
                Log.Information(string.Join(" ", args));
                Log.Information(System.Reflection.Assembly.GetExecutingAssembly().ToString());
            }

            if (o.ExecutionTimeThreshold <= 0) o.ExecutionTimeThreshold = null;

            o.ResultDirectory.CreateDirectory();
            Console.WriteLine("Writing results to: " + o.ResultDirectory);

            // in case you want to optimize an existing pipeline
            // (optionally in a Full or ParametersOnly search space)
            IIndividual statusQuo = null;
            if (!string.IsNullOrEmpty(o.StatusQuoXml))
            {
                switch (o.Backend)
                {
                    case Backend.emgucv:
                        statusQuo = CVPipeline.CVPipeline.DeserializeXml(o.StatusQuoXml);
                        break;
                    case Backend.halcon:
                        statusQuo = HalconPipeline.DeserializeXml(o.StatusQuoXml);
                        break;
                }
            }

            map = InitializeOperatorMap(o, statusQuo);

            CGPConfiguration cgpConfig = InitializeCgpConfiguration(o, map, statusQuo);

            if (o.Seed == null)
            {
                string msg = "No seed was specified, seeding randomly.";
                Log.Logger.Warning(msg);
                Console.WriteLine(msg);
                o.Seed = new Random().Next();
            }

            Log.Logger.Information($"Seed: {o.Seed.ToString()}");

            if (o.ValidationDataDirectory == null)
                o.ValidationDataDirectory = o.TrainingDataDirectory;

            InitializeBackend((int)o.Seed, o.Backend, o.Parallelization);

            // prepare batch run
            batchRun = InitializeBatchRun(o, cgpConfig);
            
            try
            {
                using (var writer = new StreamWriter(Path.Combine(o.ResultDirectory, "seed.txt")))
                {
                    writer.WriteLine(o.Seed);
                }
            }
            catch (IOException) { }

            batchRun.Run();

            try
            {
                batchRun.Validate();
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "while validating");
            }

            return 0;
        }


        private static void InitializeBackend(int seed, Backend backend, bool parallel)
        {
            if (backend == Backend.halcon)
            {
                // this reduces speed, but prevents excessive (and ever-growing) memory consumption
                HOperatorSet.SetSystem("temporary_mem_cache", "false");
                HOperatorSet.SetSystem("seed_rand", seed);

                //removes parallelization
                if (!parallel)
                    HOperatorSet.SetSystem("parallelize_operators", "false");               
             }
        }

        /// <summary>
        /// Print all operators of a given backend or pipeline.
        /// This can then be used by the "batch" verb to limit the search to operators from that list.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static int RunOperators(OperatorOptions o)
        {

            if (o.OutDirectory == null)
                o.OutDirectory = Directory.GetCurrentDirectory();

            var list = new List<string>();

            switch (o.Backend)
            {
                case Backend.emgucv:
                    if (o.Pipeline == null)
                        list.AddRange(CommonCVPipelines.NodeCollection.Select(x => x.AssemblyQualifiedName));
                    else
                    {
                        try
                        {
                            var pipe = CommonCVPipelines.CVPipelineCollection.First(x =>
                                x.Name.ToLower() == o.Pipeline.ToLower());
                            list.AddRange(pipe.Nodes.Select(x => x.AssemblyQualifiedName).Distinct());
                        }
                        catch
                        {
                            Console.WriteLine($"No predefined Pipeline with Name {o.Pipeline} exists.");
                            return -1;
                        }
                    }
                    break;
                case Backend.halcon:
                    if (o.Pipeline == null)
                        list.AddRange(CommonHalconPipelines.HalconOperatorNodeCollection.Select(x => x.AssemblyQualifiedName));
                    else
                    {
                        try
                        {
                            var pipe = CommonHalconPipelines.Collection.First(x =>
                                x.Name.ToLower() == o.Pipeline.ToLower());
                            list.AddRange(pipe.Nodes.Select(x => x.AssemblyQualifiedName).Distinct());
                        }
                        catch
                        {
                            Console.WriteLine($"No predefined Pipeline with Name {o.Pipeline} exists.");
                            return -1;
                        }
                    }
                    break;
            }

            string path = null;
            if (Path.IsPathRooted(o.Filename))
                path = o.Filename;
            else
                path = Path.Combine(o.OutDirectory, o.Filename);

            using (var writer = new StreamWriter(path))
            {
                var serializer = new XmlSerializer(typeof(List<string>));
                serializer.Serialize(writer, list);
            }

            return 0;
        }

        /// <summary>
        /// Convert images from regionmarker format to indexed images or vice versa.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static int RunConvert(ConvertOptions o)
        {
            if (string.IsNullOrEmpty(o.InDirectory))
                o.InDirectory = Directory.GetCurrentDirectory();
            o.OutDirectory.CreateDirectory();
            IndexedHObjectConverter converter = new IndexedHObjectConverter(o.InDirectory, o.OutDirectory); ;
            switch (o.OutFormat)
            {
                case FormatOption.hobj:
                    converter.ConvertIndexedToHObject();
                    break;
                case FormatOption.indexed:
                    converter.ConvertHObjectToIndexed();
                    break;
            }

            return 0;
        }

        /// <summary>
        /// Load an .xml pipeline and data and evaluate the pipeline on that data.
        /// Optionally stores a confusion matrix.
        /// Colors used for images can be configured.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private static int RunEvaluate(EvaluateOptions o)
        {
            if (string.IsNullOrEmpty(o.ResultDirectory))
                o.ResultDirectory = Path.Combine(Directory.GetCurrentDirectory(), "evaluation",
                    DateTime.Now.ToString("yyyyMMddHHss"));
            o.ResultDirectory.CreateDirectory();

            Dictionary<string, Dictionary<string, object>> jsonDict = new Dictionary<string, Dictionary<string, object>>();

            switch (o.Backend)
            {
                case Backend.halcon:
                    var pipe = HalconPipeline.DeserializeXml(o.PipelinePath);
                    HTuple actualColor = null, referenceColor = null, intersectionColor = null;

                    ParseColorsHalcon(o, out actualColor, out referenceColor, out intersectionColor);

                    FileListReferenceSet imgSet = null;
                    try
                    {
                        imgSet = new FileListReferenceSet(o.DataDirectory,
                            o.DataDirectory.GetImagesList(),
                            o.DataDirectory.GetImagesList().GetLabelDictionary());
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        Log.Logger.Debug(ex, "{Exception}");
                        Console.WriteLine($"Tried loading as FileListReferenceSet: {ex.Message}");
                        Console.WriteLine("Will try loading images individually next");
                    }

                    if (imgSet != null)
                    {
                        for (int i = 0; i < imgSet.Count; i++)
                        {
                            using (var image = imgSet[i])
                            {
                                using (HObject output = pipe.ExecuteSingle(image.Input))
                                {

                                    image.Input.Dump(Path.Combine(o.ResultDirectory,
                                        Path.GetFileNameWithoutExtension(image.Filename)),
                                        image.Reference,
                                        output,
                                        type: o.Type,
                                        actualColor: actualColor,
                                        referenceColor: referenceColor,
                                        intersectionColor: intersectionColor);


                                    if (o.LogConfusionMatrix)
                                    {
                                        using (var writer =
                                            new StreamWriter(Path.Combine(o.ResultDirectory, "ConfusionMatrix.txt"),
                                                append: true))
                                        {
                                            HalconBatchRun.LogConfusionMatrix(pipe, image, writer);
                                        }

                                        jsonDict.Add(image.Filename, HalconBatchRun.ConfusionMatrixToDictionary(pipe, image, o.FitnessFunctions));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var img in Directory.EnumerateFiles(o.DataDirectory).Where(x => x.IsImageFilePath()))
                        {
                            Console.WriteLine($"Processing img: {img}");
                            using (var hImage = new HImage(img))
                            {
                                var output = pipe.ExecuteSingle(hImage);
                                hImage.Dump(Path.Combine(o.ResultDirectory,
                                        Path.GetFileNameWithoutExtension(img)),
                                    output,
                                    type: o.Type,
                                    actualColor: actualColor,
                                    referenceColor: referenceColor,
                                    intersectionColor: intersectionColor);
                            }
                        }
                    }

                    break;

                case Backend.emgucv:
                    throw new NotImplementedException();
            }

            if (o.LogConfusionMatrix)
            {
                using (var writer = new StreamWriter(Path.Combine(o.ResultDirectory, "ConfusionMatrix.json")))
                {
                    var json = JsonConvert.SerializeObject(jsonDict, Formatting.Indented);
                    writer.WriteLine(json);
                }

                foreach (var fitFunc in o.FitnessFunctions)
                {
                    List<double> val =new List<double>();
                    foreach (var pair in jsonDict)
                        val.Add((double) pair.Value[fitFunc.ToString()]);
                    
                    Console.WriteLine($"{fitFunc}: {val.Sum() / val.Count}");
                }
            }

            return 0;
        }
        
        /// <summary>
        /// Colors for ROIs in result images can be specified via CLI as triples of ints, i.e.
        /// 0 0 255 for blue, up to a maximum of 3 colors for the 3 types of ROI (actual, reference, intersection).
        /// For example, if we want actual to be red, reference to be green and the intersection to be blue, we pass:
        /// 255 0 0 0 255 0 0 0 255 as the rgb options for the "evaluate" verb.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="actualColor"></param>
        /// <param name="referenceColor"></param>
        /// <param name="intersectionColor"></param>
        private static void ParseColorsHalcon(EvaluateOptions o, out HTuple actualColor, out HTuple referenceColor, out HTuple intersectionColor)
        {
            actualColor = null;
            referenceColor = null;
            intersectionColor = null;

            if (o.RGBValues.Any())
            {
                if (o.RGBValues.Count() % 3 != 0) throw new ArgumentException($"RGB values must be specified as triples" +
                                                                              $" and in multiples of three (i.e. 0, 3, 6, 9)," +
                                                                              $" got: {o.RGBValues.Count()}");
                if (o.RGBValues.Count() >= 3)
                    actualColor = new HTuple(o.RGBValues.ElementAt(0),
                        o.RGBValues.ElementAt(1),
                        o.RGBValues.ElementAt(2));
                if (o.RGBValues.Count() >= 6)
                    referenceColor = new HTuple(o.RGBValues.ElementAt(0),
                        o.RGBValues.ElementAt(1),
                        o.RGBValues.ElementAt(2));
                if (o.RGBValues.Count() >= 9)
                    intersectionColor = new HTuple(o.RGBValues.ElementAt(0),
                        o.RGBValues.ElementAt(1),
                        o.RGBValues.ElementAt(2));
            }
        }


        private static CGPConfiguration InitializeCgpConfiguration(BatchOptions o, ModularOperatorMap map, IIndividual statusQuo)
        {
            // initialize cgp configuration
            o.CgpColumns = Math.Max(map.Dependencies.Height, o.CgpColumns);
            o.CgpRows = Math.Max(map.Dependencies.Width, o.CgpRows);
            if (o.CgpNodeInputCount == null)
            {
                o.CgpNodeInputCount = map.OperatorInputCount.Values.Max();
            }
            if (o.CgpParameterCount == null)
            {
                o.CgpParameterCount = map.ParameterBounds.Values.Max(x => x.Length);
            }

            CGPConfiguration cgpConfig = null;
            // if no statusquo individual was passed as option, we can initialize the cgpconfig as usual
            if (statusQuo == null)
            {
                cgpConfig = new CGPConfiguration(columnCount: (int)o.CgpColumns,
                    rowCount: (int)o.CgpRows, inputCount: (int)o.CgpNodeInputCount,
                    levelsBack: (int)o.CgpLevelsBack, operatorMap: map,
                    parameterCount: (int)o.CgpParameterCount,
                    programInputCount: (int)o.CgpProgramInputCount,
                    programOutputCount: (int)o.CgpProgramOutputCount);
            }
            else
            {
                FloatVector vec = null;
                /* individuals need to be mapped to float vectors, which in turn need to fit a given cgpconfig;
                 * this is why we initialize a fitting cgpconfig from an individual, in case a status quo individual was
                 * passed as option. This is somewhat complicatied and messy, as it depends on the interplay of operatormaps,
                 * cgpconfig and dependencytrees.
                 */
                var tmpRnd = new SystemRandom(seed: (int)o.Seed);
                switch (o.Backend)
                {
                    case Backend.halcon:
                        ((HalconPipeline)statusQuo).ToCGPEncoding(map, tmpRnd,
                            out vec, out cgpConfig);
                        break;
                    case Backend.emgucv:
                        ((CVPipeline.CVPipeline)statusQuo).ToCGPEncoding(map, tmpRnd,
                            out vec, out cgpConfig);
                        break;
                }
                cgpConfig.StatusQuo = vec;
            }

            cgpConfig.SearchSpace = o.SearchSpace;

            return cgpConfig;
        }

        private static BatchRun InitializeBatchRun(BatchOptions o, CGPConfiguration cgpConfig)
        {
            BatchRun batchRun = null;
            switch (o.Backend)
            {
                case Backend.halcon:
                    ReferenceSet halconTrainSet, halconValSet;
                    var trainImages = o.TrainingDataDirectory.GetImagesList();
                    var valImages = o.ValidationDataDirectory.GetImagesList();

                    halconTrainSet = new FileListReferenceSet(
                        o.TrainingDataDirectory,
                        trainImages,
                        trainImages.GetLabelDictionary());

                    halconValSet = new FileListReferenceSet(
                        o.ValidationDataDirectory,
                        valImages,
                        valImages.GetLabelDictionary());

                    halconTrainSet.FitsIntoMemory = o.FitsInMemory;
                    halconValSet.FitsIntoMemory = o.FitsInMemory;
                    
                    switch (o.EvolutionStrategyType)
                    {
                        case EvolutionStrategyType.standard:
                            batchRun = CommonHalconEvolutionStrategies.BuildStandardCGPEvolutionStrategy(
                                CGPconfig: cgpConfig,
                                refSet: halconTrainSet,
                                valSet: halconValSet,
                                generations: o.Generations,
                                iterations: o.Runs,
                                saveDirectory: o.ResultDirectory,
                                seed: (int)o.Seed,
                                batchSize: o.BatchSize,
                                queueSize: o.QueueSize,
                                executionTimeThreshold: o.ExecutionTimeThreshold,
                                fitnessFunction: o.FitnessFunction,
                                fitnessFunctions: o.FitnessFunctions.ToArray(),
                                weights: o.Weights.ToArray(),
                                parallelDegree: (int) o.WorkerCount);
                                
                            break;
                        case EvolutionStrategyType.selfadaptive:
                            batchRun = CommonHalconEvolutionStrategies.SelfAdaptiveEvolutionStrategy(
                                CGPconfig: cgpConfig,
                                refSet: halconTrainSet,
                                valSet: halconValSet,
                                generations: o.Generations,
                                iterations: o.Runs,
                                saveDirectory: o.ResultDirectory,
                                seed: (int)o.Seed,
                                batchSize: o.BatchSize,
                                queueSize: o.QueueSize,
                                executionTimeThreshold: o.ExecutionTimeThreshold,
                                fitnessFunction: o.FitnessFunction,
                                fitnessFunctions: o.FitnessFunctions.ToArray(),
                                weights: o.Weights.ToArray(),
                                parallelDegree: (int) o.WorkerCount);
                            break;
                    }
                    break;

                case Backend.emgucv:
                    CVReferenceSet emguTrainSet, emguValSet;

                    emguTrainSet = new CVReferenceSet(
                        basePath: o.TrainingDataDirectory,
                        imagepaths: Directory.EnumerateFiles(Path.Combine(o.TrainingDataDirectory, "images")),
                        labelPaths: Directory.EnumerateFiles(Path.Combine(o.TrainingDataDirectory, "labels")),
                        imgType: ImreadModes.Grayscale);

                    emguValSet = new CVReferenceSet(
                        basePath: o.TrainingDataDirectory,
                        imagepaths: Directory.EnumerateFiles(Path.Combine(o.ValidationDataDirectory, "images")),
                        labelPaths: Directory.EnumerateFiles(Path.Combine(o.ValidationDataDirectory, "labels")),
                        imgType: ImreadModes.Grayscale);

                    emguTrainSet.FitsIntoMemory = o.FitsInMemory;
                    emguValSet.FitsIntoMemory = o.FitsInMemory;

                    batchRun = CommonCVEvolutionStrategies.BuildStandardCVEvolutionStrategy(
                        train: new CVDataLoader(emguTrainSet),
                        val: new CVDataLoader(emguValSet),
                        config: cgpConfig,
                        generations: o.Generations,
                        seed: (int)o.Seed,
                        iterations: o.Runs,
                        saveDirectory: o.ResultDirectory);

                    break;

                default:
                    throw new ArgumentException($"Unsupported backend: {o.Backend}");
            }

            return batchRun;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="statusQuo"></param>
        /// <returns></returns>
        private static ModularOperatorMap InitializeOperatorMap(BatchOptions o, IIndividual statusQuo)
        {
            ModularOperatorMap map = null;
            List<INode> nodes = null;
            DependencyTree tree = null;

            if (o.Operators != null) // if --operators= was passed
            {
                using (var reader = new StreamReader(o.Operators))
                {
                    var serializer = new XmlSerializer(typeof(List<string>));
                    var list = serializer.Deserialize(reader) as List<string>;
                    nodes = new List<INode>();
                    switch (o.Backend)
                    {

                        case Backend.emgucv:
                            foreach (var n in list)
                                nodes.Add((CVNode)Activator.CreateInstance(Type.GetType(n)));
                            break;
                        case Backend.halcon:
                            foreach (var n in list)
                                nodes.Add((HalconOperatorNode)Activator.CreateInstance(Type.GetType(n)));
                            break;
                    }
                }
            }
            else if (statusQuo != null) // for SearchSpaceType.ParametersOnly and --status-quo options
            {
                switch (o.Backend)
                {
                    case Backend.halcon:
                        var hpipe = (HalconPipeline)statusQuo;
                        tree = hpipe.ToDependencyTree();
                        nodes = hpipe.Nodes.Cast<INode>().ToList();
                        break;
                    case Backend.emgucv:
                        var cvpipe = (CVPipeline.CVPipeline)statusQuo;
                        tree = cvpipe.ToDependencyTree();
                        nodes = cvpipe.Nodes.Cast<INode>().ToList();
                        break;
                }
            }
            else
            {
                switch (o.Backend)
                {
                    case Backend.emgucv:
                        nodes = CommonCVPipelines.NodeCollection.Cast<INode>().ToList();
                        break;
                    case Backend.halcon:
                        nodes = CommonHalconPipelines.HalconOperatorNodeCollection.Cast<INode>().ToList();
                        break;
                }
            }

            // initialize operator map, optionally filter allowed operators
            switch (o.Backend)
            {
                case Backend.halcon:
                    map = new HalconOperatorMap(
                        nodes.Cast<HalconOperatorNode>().ToList(),
                        dependencyTree: tree);
                    break;
                case Backend.emgucv:
                    map = new CVOperatorMap(
                        nodes.Cast<CVNode>(),
                        tree);
                    break;
            }

            return map;
        }
    }
}
