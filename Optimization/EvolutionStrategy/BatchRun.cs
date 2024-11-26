using Extensions;
using MathNet.Numerics.LinearAlgebra.Solvers;
using Newtonsoft.Json;
using Optimization.CartesianGeneticProgramming;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Random;
using Optimization.HalconPipeline.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Optimization.EvolutionStrategy
{
    public class BatchRun
    {
        private EvolutionStrategy eS;
        private List<IConfiguration> list;
        private int iterations;
        private int paralleldegree;

        //No LoggingObjects or LoggingActions

        /// <summary>
        /// Constructor for most generic BatchRun; prepares Parallel execution
        /// </summary>
        /// <param name="evolutionStrategy"></param>
        /// <param name="CGPConfig"></param>
        /// <param name="saveDirectory"></param>
        /// <param name="iterations"></param>
        /// <param name="seed"></param>
        /// <param name="paralleldegree"></param>
        public BatchRun(IEvolutionStrategy evolutionStrategy, CGPConfiguration CGPConfig, string saveDirectory, int strategyCount, int seed = 0, int collectionCount = 1, int paralleldegree = 1)
        {
            var configs = new List<IConfiguration>() { CGPConfig, evolutionStrategy.Configuration };
            //Startup Prozess
            Initialize(evolutionStrategy, configs, saveDirectory, strategyCount, seed, collectionCount, paralleldegree);
        }

        /// <summary>
        /// Constructor for given List of Configurations
        /// </summary>
        /// <param name="evolutionStrategy"></param>
        /// <param name="configurations"></param>
        /// <param name="saveDirectory"></param>
        /// <param name="strategyCount"></param>
        /// <param name="collectionCount"></param>
        /// <param name="seed"></param>
        /// <param name="paralleldegree"></param>
        public BatchRun(IEvolutionStrategy evolutionStrategy, List<IConfiguration> configurations, string saveDirectory, int strategyCount, int seed = 0, int collectionCount = 1, int paralleldegree = 1)
        {
            Initialize(evolutionStrategy, configurations, saveDirectory, strategyCount, seed, collectionCount, paralleldegree);
        }
        
        //Basic properties
        public IEvolutionStrategy EvolutionStrategy { get; protected set; }
        public IIndividual BestIndividual { get; set; }

        public string SaveDirectory { get; protected set; }
        public string AnalyzerDirectory { get; protected set; }
        public string ImagesDirectory { get; protected set; }
        public string ConfigDirectory { get; protected set; }
        public string GridDirectory { get; protected set; }
        public int StrategyCount { get; protected set; }
        public int Seed { get; protected set; }
        public int ParallelisationDegree { get; protected set; }
        /// <summary>
        /// Flag for consumers to stop removing queue elements/strategies. If true then stop.
        /// </summary>
        public bool EarlyStopping { get; set; }

        public bool Finished { get; set; }
        public int CollectionCount { get; set; }
        
        //Complex properties
        public List<IConfiguration> Configurations { get; protected set; }
        public List<int> SeedList { get; set; }
        public List<Thread> WorkingThreads { get; set; }
        public Thread ProducerThread { get; set; }

        //Logging properties
        public List<Action<LoggingObject>> LoggingActions { get; protected set; }

        /// <summary>
        /// Producer-Consumer properties
        /// Tuple<(int) Run, (int) Generation, (IIndividual) BestIndividual>
        /// </summary>
        public ConcurrentQueue<Tuple<int, IIndividual>> BestIndividuals { get; protected set; }


        /// <summary>
        /// Stores BestIndividual per Generation for logging and analysis
        /// WARNING: Do not use for more than 1 batchrun => danger of memory overflow!
        /// List<Tuple<(int) Run, (int) Generation, (IIndividual) BestIndividual>
        /// </summary>
        public List<Tuple<int, List<IIndividual>>> GenBestIndividuals { get; protected set; }

        /// <summary>
        /// Seed, Evolutionstrategy, "Index"
        /// </summary>
        public BlockingCollection<Tuple<int, IEvolutionStrategy, int>> evolutionStrategies { get; protected set; }
        public class LoggingObject
        {
            /// <summary>
            /// BatchRun object (similar to sender) that invokes the logging action
            /// </summary>
            public BatchRun BatchRun { get; set; }
            /// <summary>
            /// The evolution strategy corresponding to the Iteration
            /// </summary>
            public IEvolutionStrategy EvolutionStrategy { get; set; }
            /// <summary>
            /// Iteration or index of evolution strategy
            /// </summary>
            public int Iteration { get; set; }
        }

        /// <summary>
        /// Set all attributes; create threads and a producer for evolution strategies
        /// </summary>
        /// <param name="evolutionStrategy"></param>
        /// <param name="configurations"></param>
        /// <param name="saveDirectory"></param>
        /// <param name="iterations"></param>
        /// <param name="seed"></param>
        /// <param name="paralleldegree"></param>
        protected void Initialize(IEvolutionStrategy evolutionStrategy, List<IConfiguration> configurations, string saveDirectory, int strategyCount, int seed, int collectionCount = 1, int paralleldegree = 1)
        {
            this.LoggingActions = new List<Action<LoggingObject>>();

            this.EvolutionStrategy = evolutionStrategy;
            this.BestIndividual = null;

            this.SaveDirectory = saveDirectory;
            this.SaveDirectory.CreateDirectory();

            this.AnalyzerDirectory = Path.Combine(saveDirectory, "Analyzer");
            this.ConfigDirectory = Path.Combine(saveDirectory, "Config");
            this.GridDirectory = Path.Combine(saveDirectory, "Grid");
            this.ImagesDirectory = Path.Combine(saveDirectory, "Images");

            this.ConfigDirectory.CreateDirectory();

            for (int i = 0; i < strategyCount; i++)
            {
                var pa = Path.Combine(this.AnalyzerDirectory, i.ToString());
                var pg = Path.Combine(this.GridDirectory, i.ToString());
                var pi = Path.Combine(this.ImagesDirectory, i.ToString());
                pa.CreateDirectory();
                pg.CreateDirectory();
                pi.CreateDirectory();
            }

            this.StrategyCount = strategyCount;
            this.Seed = seed;
            this.ParallelisationDegree = paralleldegree;
            this.EarlyStopping = false;
            this.CollectionCount = collectionCount;

            this.Configurations = configurations;
            this.SeedList = produceSeeds(seed);

            this.BestIndividuals = new ConcurrentQueue<Tuple<int, IIndividual>>();

            
            if (evolutionStrategy.Configuration.LogGenBestIndividuals)
                this.GenBestIndividuals = new List<Tuple<int, List<IIndividual>>>();

            this.evolutionStrategies = new BlockingCollection<Tuple<int, IEvolutionStrategy, int>>(collectionCount);
        }

        /// <summary>
        /// Executes the evolution of the es and writes the bestIndivdual to List of bestIndividuals
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="individuals"></param>
        /// <param name="sender"></param>
        private static void ThreadFunc(Object sender)
        {
            //Einfuegen einer korrekten Abbruchbedingung (return) wenn Lsite leer und Complete Adding gestetz bzw cancellationPending auf false ist
            IIndividual best = null;
            List<IIndividual> bestCollectionPerGen = null;

            var run = (BatchRun)sender;

            //If Queue is not empty and the producer is not finished
            while ((!run.evolutionStrategies.IsAddingCompleted || run.evolutionStrategies.Count > 0 ) &&
                   !run.EarlyStopping)
            {
                if (!run.evolutionStrategies.TryTake(out Tuple<int, IEvolutionStrategy, int> esTmpTuple))
                    continue;
                int seed = esTmpTuple.Item1;
                var evolutionstrategytemp = esTmpTuple.Item2;

                try
                {
                    Console.WriteLine("Evolving");
                    bestCollectionPerGen = evolutionstrategytemp.Evolve();
                    best = bestCollectionPerGen.Last();
                    run.BestIndividuals.Enqueue(Tuple.Create(seed, best));

                    if (evolutionstrategytemp.Configuration.LogGenBestIndividuals)
                        run.GenBestIndividuals.Add(Tuple.Create(seed, bestCollectionPerGen));
                    

                    run.Log(new LoggingObject { BatchRun = run,
                        EvolutionStrategy = evolutionstrategytemp
                        , Iteration = esTmpTuple.Item3});
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}, {Environment.NewLine} {ex.StackTrace}, {Environment.NewLine} Iteration: {esTmpTuple.Item3}");
                    Serilog.Log.Error(ex, "{Exception}{Properties}", esTmpTuple.Item3);
                }

            }

        }

        /// <summary>
        /// 
        /// Produces replicatable copies and stores them in a thread safe collection
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="c"></param>
        /// <param name="seeds"></param>
        /// <param name="sender"></param>
        private static void ProduceFunc(Object sender)
        {
            var run = (BatchRun)sender;
            for (int i = 0; i < run.StrategyCount; i++)
            {
                var seed = run.SeedList[i];
                //Create new Random for Copy function
                // var random = new SystemRandom(seed);
                var random = new SystemRandom(seed == -1 ? DateTime.Now.Millisecond : seed);

                //var evolutionstrategycpy = strategy.Copy(seed) as IEvolutionStrategy;
                var evolutionstrategycpy = run.EvolutionStrategy.Copy(random) as IEvolutionStrategy;
                
                while(!run.evolutionStrategies.TryAdd(Tuple.Create(seed, evolutionstrategycpy, i)))
                {
                }
            }
            run.evolutionStrategies.CompleteAdding();
        }

        /// <summary>
        /// Executes all working/consuming and producing threads for parallel execution
        /// </summary>
        /// <returns></returns>
        public IIndividual Run()
        {

            LogStart();
            LogConfigurations();

            this.WorkingThreads = new List<Thread>();
            for (int i = 0; i < ParallelisationDegree; i++)
            {
                //Alternativ
                // Thread t = new Thread(new ThreadStart(this.ThreadFunc(this)))
                Thread t = new Thread(() => ThreadFunc(this));
                this.WorkingThreads.Insert(i, t);
            }

            //Alterantiv
            //this.ProducerThread = new Thread(new ThreadStart(this.ProduceFunc(this)));
            this.ProducerThread = new Thread(() => ProduceFunc(this));


            //Begin filling up collection of evolution strategies
            Console.WriteLine("Initialising Producer Thread");
            this.ProducerThread.Start();

            for (int j = 0; j < this.WorkingThreads.Count; j++)
            {
                this.WorkingThreads[j].Start();
            }

            this.ProducerThread.Join();

            for (int j = 0; j < this.WorkingThreads.Count; j++)
            {
                this.WorkingThreads[j].Join();
            }

            //Now Garbage Collector will be able to Finalize Threads and close them

            LogOverview();

            Finished = true;

            return GetBestIndividual();
                       
            }


        /// <summary>
        /// Creates a reproducible List of seeds for correct initialization of evolution straty copies
        /// </summary>
        /// <param name="seed"></param>
        /// <returns></returns>
        private List<int> produceSeeds(int seed)
        {
            var randomInts = new List<int>();
            var rand = new System.Random(seed);

            for (int i = 0; i < this.StrategyCount; i++)
            {
                randomInts.Add(rand.Next());
            }

            return randomInts;
        }

        public void RegisterLoggingAction(Action<LoggingObject> action)
        {
            LoggingActions.Add(action);
        }
        private void LogStart()
        {
            using (var writer = new StreamWriter(Path.Combine(SaveDirectory, "date.txt")))
            {
                writer.WriteLine("Start: " + DateTime.Now);
            }
        }

        private void LogOverview()
        {
            var indexedIndividuals = Enumerable.Range(0, BestIndividuals.Count)
                .ToDictionary(x => x, x => BestIndividuals.ElementAt(x).Item2);


            using (var writer = new StreamWriter(Path.Combine(SaveDirectory, "overview.txt")))
                for (int i = 0; i < BestIndividuals.ToList().Count; i++)
                {
                    writer.WriteLine("Iteration: " + i + " Best Fitness: " +
                                     EvolutionStrategy.FitnessConfiguration.WeightedFitnessOf(indexedIndividuals[i]));
                }

            /*
            using (var writer = new StreamWriter(Path.Combine(SaveDirectory, myVectorFile...)))
                for (int i = 0; i < BestIndividuals.ToList().Count; i++)
                {
                    !!! writer Best Individual Vector to file !!!
                }
            */
            using (var writer = new StreamWriter(Path.Combine(SaveDirectory, "overview.json")))
            {
                writer.WriteLine(BestIndividualsToJson(indexedIndividuals));
            }
        }

        private void Log(LoggingObject loggable)
        {
            for (int i = 0; i < LoggingActions.Count; i++)
            {
                LoggingActions[i].Invoke(loggable);
            }
        }

        public static void LogAnalyzers(LoggingObject loggable)
        {
            var ES = loggable.EvolutionStrategy as EvolutionStrategy;
            var batch = loggable.BatchRun;
            ES.Analyzer.Save(Path.Combine(batch.AnalyzerDirectory, loggable.Iteration.ToString()));
        }

        public void LogConfigurations()
        {
            foreach (var config in Configurations)
            {
                var log = config;
                if (log == null) continue;
                var path = Path.Combine(ConfigDirectory, config.ConfigurationType + ".txt");

                if (config.SerializeXmlSupported)
                {
                    config.SerializeXml(path);
                }
            }
        }

        public static void LogTime(LoggingObject loggable)
        {
            var batch = loggable.BatchRun as BatchRun;
            using (var writer = new StreamWriter(Path.Combine(batch.SaveDirectory, "date.txt"), true))
            {
                writer.WriteLine("Iteration: " + loggable.Iteration + " End: " + DateTime.Now.ToString());
            }
        }

        public static void LogProgressToOutput(LoggingObject loggable)
        {
            var batch = loggable.BatchRun;
            Trace.WriteLine("BatchIteration: " + loggable.Iteration + "\\" + batch.StrategyCount);
            Trace.WriteLine("Best: " + loggable.EvolutionStrategy.Best.Fitness);
        }

        public static void LogLegend(LoggingObject loggable)
        {
            var batch = loggable.BatchRun;
            var dir = Path.Combine(batch.ImagesDirectory, loggable.Iteration.ToString());
            dir.CreateDirectory();
            using (var writer = new StreamWriter(Path.Combine(dir, "legend.txt")))
            {
                writer.WriteLine("Red: actual (ist)");
                writer.WriteLine("Green: reference (soll)");
                writer.WriteLine("Yellow: intersection of actual and reference, i.e. true positives");
            }
        }

        public void Validate()
        {
            var validator = EvolutionStrategy.Evaluator as IValidator;
            if (validator == null)
                throw new Exception(
                    "Evaluator does not implement IValidator interface, thus cannot be used to evaluate");

            validator.Validate(BestIndividuals.Select(x => x.Item2).ToList());

            using (var writer = new StreamWriter(Path.Combine(SaveDirectory, "validation.json")))
            {
                // convert to "index: IIndividual" dictionary
                writer.WriteLine(BestIndividualsToJson(Enumerable.Range(0, BestIndividuals.Count)
                    .ToDictionary(x => x, x => BestIndividuals.ElementAt(x).Item2))); 
            }
        }

        private static string BestIndividualsToJson(Dictionary<int, IIndividual> best)
        {
            
            var bestList = best.Select(x => new {x.Value.Fitness, Iteration = x.Key, IndividualId = x.Value.GetId()})
                .OrderBy(x => x.Iteration)
                .ToList();

            return JsonConvert.SerializeObject(bestList,
                Formatting.Indented);
        }

        public IIndividual GetBestIndividual()
        {
            var orderedIndividuals = BestIndividuals.Select(x => x.Item2)
                .OrderBy(x => EvolutionStrategy.FitnessConfiguration.WeightedFitnessOf(x));

            if (EvolutionStrategy.FitnessConfiguration.Maximization)
            {
                return orderedIndividuals.First();
            }
            else
            {
                return orderedIndividuals.Last();
            }
        }
    }
}
