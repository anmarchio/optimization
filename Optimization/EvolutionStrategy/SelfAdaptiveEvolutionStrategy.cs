using System.Drawing.Imaging;
using System.Linq;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;

namespace Optimization.EvolutionStrategy
{
    public class SelfAdaptiveEvolutionStrategy : EvolutionStrategy
    {
        protected SelfAdaptiveEvolutionStrategy() : base()
        {

        }

        public SelfAdaptiveEvolutionStrategy(ICreator creator, IAdaptiveMutator mutator, ISelector survivalSelector,
            ISelector procreationSelector, IEvaluator evaluator, IRecombinator recombinator,
            ITerminator terminator, IEvolutionAnalyzer analyzer, ESConfiguration ESconfig, FitnessConfiguration fitnessConfiguration,
            bool maximization = true) : base(creator, mutator,
                survivalSelector, procreationSelector, evaluator, recombinator, terminator, analyzer, ESconfig, fitnessConfiguration)
        {
        }

        public SelfAdaptiveEvolutionStrategy(ICreator creator, IAdaptiveMutator mutator, ISelector survivalSelector,
    ISelector procreationSelector, IEvaluator evaluator, ITerminator terminator, IEvolutionAnalyzer analyzer, ESConfiguration ESconfig,
    FitnessConfiguration fitnessConfiguration,
    bool maximization = true) : base(creator, mutator,
        survivalSelector, procreationSelector, evaluator, terminator, analyzer, ESconfig, fitnessConfiguration)
        {
        }

        public int GenerationsWithoutImprovement { get; set; } = 0;

        protected override void GenerationPostProcessing()
        {
            // move to inheriting class
            CountGenerationsWithoutImprovement();
            (Mutator as IAdaptiveMutator).Adapt(GenerationsWithoutImprovement);

            base.GenerationPostProcessing();    
        }

        protected void CountGenerationsWithoutImprovement()
        {
            GenerationsWithoutImprovement++;
            var pop = Population.OrderBy(x => FitnessConfiguration.WeightedFitnessOf(x));

            if (FitnessConfiguration.Maximization)
            {
                var maxFitness = FitnessConfiguration.WeightedFitnessOf(pop.First());
                if (maxFitness > FitnessConfiguration.WeightedFitnessOf(Best))
                {
                    GenerationsWithoutImprovement = 0;
                    Best = Population.First();
                }
            }
            else
            {
                var minFitness = FitnessConfiguration.WeightedFitnessOf(pop.Last());
                if (minFitness < FitnessConfiguration.WeightedFitnessOf(Best))
                {
                    GenerationsWithoutImprovement = 0;
                    Best = Population.Last();
                }
            }
        }

        public override ICopyable Copy()
        {
            var copy = Creator as ICopyable;
            var creator = copy != null ? copy.Copy() as ICreator : Creator;
            copy = Mutator as ICopyable;
            var mutator = copy != null ? copy.Copy() as IAdaptiveMutator : Mutator as IAdaptiveMutator;
            copy = SurvivalSelector as ICopyable;
            var survivalSelector = copy != null ? copy.Copy() as ISelector : SurvivalSelector;
            copy = ProcreationSelector as ICopyable;
            var procreationSelector = copy != null ? copy.Copy() as ISelector : ProcreationSelector;
            copy = Evaluator as ICopyable;
            var evaluator = copy != null ? copy.Copy() as IEvaluator : Evaluator;
            copy = Recombinator as ICopyable;
            var recombinator = copy != null ? copy.Copy() as IRecombinator : Recombinator;
            copy = Terminator as ICopyable;
            var terminator = copy != null ? copy.Copy() as ITerminator : Terminator;
            copy = Analyzer as ICopyable;
            var analyzer = copy != null ? copy.Copy() as IEvolutionAnalyzer : Analyzer;

            return new SelfAdaptiveEvolutionStrategy(creator, mutator,
                survivalSelector, procreationSelector,
                evaluator, recombinator,
                terminator, analyzer, Configuration, FitnessConfiguration);
        }

        /// <summary>
        /// Method executing the evolutionary steps (Initilization, Procreation, Evaluation, Selection).
        /// </summary>
        /// <returns></returns>
        /*
        public virtual IIndividual EvolveOld()
        {
            Initialize();

            // evaluate
            foreach (var individual in Population)
            {
                individual.Fitness = Evaluator.Evaluate(individual);
            }

            Best = SelectBest(Population);

          
            if (Analyzer != null) Analyzer.Analyze(this);

            while (!Terminator.Terminate(this))
            {
                if (Maximization)
                {
                    BestParent = Population.Where(x => x.Fitness == Population.Max(y => y.Fitness)).First();    
                }
                else
                {
                    BestParent = Population.Where(x => x.Fitness == Population.Min(y => y.Fitness)).First();        
                }

                // crossover
                if (Configuration.Rho > 1)
                {
                    if (stdDev == -1)       //no adaptive mutation aka use the stdDev value of the underlying mutation function (or the probability passed to the mutation function in the constructor)
                    {
                        var parents = new List<IIndividual>[Configuration.Lambda];
                        for (int i = 0; i < parents.Length; i++)
                        {
                            parents[i] = ProcreationSelector.Select(Population, Configuration.Rho);
                        }
                        foreach (var p in parents)
                        {
                            var tmp = Recombinator.Cross(p);
                            Offspring.Add(Mutator.Mutate(tmp));
                        }
                    }
                    else //adaptive mutation
                    {
                        var parents = new List<IIndividual>[Configuration.Lambda];
                        for (int i = 0; i < parents.Length; i++)
                        {
                            parents[i] = ProcreationSelector.Select(Population, Configuration.Rho);
                        }
                        foreach (var p in parents)
                        {
                            var tmp = Recombinator.Cross(p);
                            Offspring.Add(Mutator.Mutate(tmp, stdDev));
                        }
                    }

                }
                else // no crossover            
                {
                    var parents = ProcreationSelector.Select(Population, Configuration.Lambda);
                    if (stdDev == -1)       //no adaptive mutation aka use the stdDev value of the underlying mutation function (or the probability passed to the mutation function in the constructor)
                    {
                        foreach (var indiv in parents)
                        {
                            Offspring.Add(Mutator.Mutate(indiv));
                        }
                    }
                    else //adaptive mutation
                    {
                        foreach (var indiv in parents)
                        {
                            Offspring.Add(Mutator.Mutate(indiv, stdDev));
                        }
                    }

                }

                #region Multithreading experiment
                //NOTE: Test runs of 3 minutes with multithreading yielded: 178, 109, 176, 111, 142, 153 generations respectively
                //NOTE: Test runs of 3 minutes without multithreading yielded: 126, 172, 94 generations respectively
                // --> Not quite sure if it adds something, therefore using sequencial execution because of predictability and its known to work.

                //the following "section" was used for debugging to compare the order of the fitness values of the individuals on sequencial/parallel execution
                //var referenceFitnessList = new List<double>();
                //foreach (var individual in Offspring)
                //{
                //    individual.Fitness = Evaluator.Evaluate(individual);
                //    referenceFitnessList.Add(individual.Fitness);
                //}

                //multithreading attempt -- works but not tested in depth
                /*   var threadList = new List<Thread>();
                   var resultList = new List<double>();
                   foreach (var individual in Offspring)
                   {
                       Thread t2 = new Thread(() => { resultList.Add(Evaluator.Evaluate(individual)); });
                       threadList.Add(t2);
                   }

                   for (int i = 0; i < Offspring.Count(); i++)
                   {
                       threadList.ElementAt(i).Start();
                       threadList.ElementAt(i).Join();
                   }

                   for (int i = 0; i < resultList.Count(); i++)
                   {
                       Offspring.ElementAt(i).Fitness = resultList.ElementAt(i);
                   }
               
                #endregion

                // evaluate
                foreach (var individual in Offspring)
                {
                    individual.Fitness = Evaluator.Evaluate(individual);
                }

                if (Maximization)
                {
                    BestOffspring = Offspring.Where(x => x.Fitness == Offspring.Max(y => y.Fitness)).First();      
                }
                else
                {
                    BestOffspring = Offspring.Where(x => x.Fitness == Offspring.Min(y => y.Fitness)).First();      
                }

                // survivor selection
                if (Configuration.PlusSelection)
                {
                    Population = SurvivalSelector.Select(Offspring.Union(Population).ToList(), Configuration.Mu, Maximization);
                }
                else
                {
                    Population = SurvivalSelector.Select(Offspring, Configuration.Mu, Maximization);
                }

                // prioritize Offspring over Parents if their fitness is equal to specifically utilize neutral genetic drift
                if (Population.Count() == 1 && BestParent.Fitness == BestOffspring.Fitness)
                {
                    Population.Clear();
                    Population.Add(BestOffspring);
                }

                if (Analyzer != null) Analyzer.Analyze(this);

                Offspring.Clear();
                CurrentGeneration++;
                generations_without_fitness_improvements++;

                if (Maximization)
                {
                    var maxFitness = Population.Max(x => x.Fitness);
                    if (maxFitness > Best.Fitness)
                    {
                        generations_without_fitness_improvements = 0;
                        Best = Population.Last(x => x.Fitness == maxFitness);
                    }
                }
                else
                {
                    var minFitness = Population.Min(x => x.Fitness);
                    if (minFitness < Best.Fitness)
                    {
                        generations_without_fitness_improvements = 0;
                        Best = Population.Last(x => x.Fitness == minFitness);
                    }
                }
                if (stdDev != -1)   //adaptive mutation expected, is handed to the probabilistic mutator and there identified by the different values for sigma, maybe outsource into new mutator (CGPAdaptiveMutator)
                {
                    mutateSigma();
                }
                // optional: let backgroundworker report progress
                if (Worker != null && Worker.WorkerReportsProgress)
                {
                    ReportProgress();
                }

                if(Worker != null && Worker.CancellationPending)
                {
                    return Best;
                }

            }
            return Best;
        }*/
    }
}
