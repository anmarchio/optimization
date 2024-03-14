using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Terminators;
using Optimization.Fitness;

namespace Optimization.EvolutionStrategy
{
    /// <summary>
    /// Main class responsible for handling the whole evolutionary process (creation, procreation, selection).
    /// </summary>
    [Serializable]
    public class EvolutionStrategy : IEvolutionStrategy
    {
        protected EvolutionStrategy()
        {

        }

        public EvolutionStrategy(ICreator creator, IMutator mutator, ISelector survivalSelector, ISelector procreationSelector, IEvaluator evaluator, ITerminator terminator, IEvolutionAnalyzer analyzer, ESConfiguration ESconfig, FitnessConfiguration fitnessConfiguration)
        {
            if (ESconfig.Rho > 0) throw new Exception("Rho is not equal 0, yet no recombinator has been set.");

            Creator = creator;
            Mutator = mutator;
            SurvivalSelector = survivalSelector;
            ProcreationSelector = procreationSelector;
            Terminator = terminator;
            Configuration = ESconfig;
            Evaluator = evaluator;
            Analyzer = analyzer;
            FitnessConfiguration = fitnessConfiguration;
        }

        public EvolutionStrategy(ICreator creator, IMutator mutator, ISelector survivalSelector, ISelector procreationSelector, IEvaluator evaluator, IRecombinator recombinator, ITerminator terminator, IEvolutionAnalyzer analyzer, ESConfiguration ESconfig, FitnessConfiguration fitnessConfiguration)
        {
            Creator = creator;
            Mutator = mutator;
            SurvivalSelector = survivalSelector;
            ProcreationSelector = procreationSelector;
            Terminator = terminator;
            Configuration = ESconfig;
            Recombinator = recombinator;
            Evaluator = evaluator;
            Analyzer = analyzer;
            FitnessConfiguration = fitnessConfiguration;
        }

        public FitnessConfiguration FitnessConfiguration { get; set; }
        
        /// <summary>
        /// Don't use this if you use parallelization.
        /// </summary>
        /// <returns></returns>
        public virtual ICopyable Copy()
        {
            var copy = Creator as ICopyable;
            var creator = copy != null ? copy.Copy() as ICreator : Creator;
            copy = Mutator as ICopyable;
            var mutator = copy != null ? copy.Copy() as IMutator : Mutator;
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
            
            return new EvolutionStrategy(creator, mutator,
                survivalSelector, procreationSelector,
                evaluator, recombinator,
                terminator, analyzer, Configuration, FitnessConfiguration);
        }

        private int PercentageOffsetMin { get; set; }
        private int PercentageOffsetMax{ get; set; }
        private BackgroundWorker Worker { get; set; }
        public IEvolutionAnalyzer Analyzer { get; protected set; }

        public ICreator Creator { get; protected set; }
        public IRecombinator Recombinator { get; protected set; }
        public IMutator Mutator { get; protected set; }
        public ISelector SurvivalSelector { get; protected set; }
        public ISelector ProcreationSelector { get; protected set; }
        public ITerminator Terminator { get; protected set; }
        public ESConfiguration Configuration { get; set; }

        public IIndividual Best { get; set; }

        public IEvaluator Evaluator { get; set; }

        public List<IIndividual> Population { get; protected set; }

        public List<IIndividual> Offspring { get; protected set; }

        public void RegisterWorker(BackgroundWorker worker, int percentageOffsetMin, int percentageOffsetMax)
        {
            Worker = worker;
            PercentageOffsetMin = percentageOffsetMin;
            PercentageOffsetMax = percentageOffsetMax;
        }


        public int CurrentGeneration { get; set; } = 0;

        protected void Initialize()
        {
            Offspring = new List<IIndividual>();
            Population = new List<IIndividual>();
            CurrentGeneration = 0;

            while (Population.Count < Configuration.Mu)
                Population.Add(Creator.Create());

        }

        protected virtual void Evaluate(List<IIndividual> individuals)
        {
            Evaluator.Evaluate(individuals);
        }

        protected virtual List<IIndividual> Crossover(List<IIndividual> individuals)
        {
            var parents = new List<IIndividual>[Configuration.Lambda];
            var recombined = new List<IIndividual>();
            for (int i = 0; i < parents.Length; i++)
            {
                parents[i] = ProcreationSelector.Select(individuals, Configuration.Rho);
            }
            foreach (var p in parents)
            {
                var tmp = Recombinator.Cross(p);
                recombined.Add(tmp);
            }
            return recombined;
        }

        protected virtual void Mutate(List<IIndividual> individuals)
        {
            foreach(var indiv in individuals)
                Offspring.Add(Mutator.Mutate(indiv));
        }

        protected virtual void SelectSurvivors(List<IIndividual> individuals)
        {
            Population = SurvivalSelector.Select(individuals, Configuration.Mu);
        }


        protected virtual void CGPSelection()
        {
            var parent = SelectBest(Population);
            var offspring = SelectBest(Offspring);
            // prioritize Offspring over Parents if their fitness is equal to specifically utilize neutral genetic drift
            if (Population.Count() == 1 && parent.Fitness == offspring.Fitness)
            {
                Population.Clear();
                Population.Add(offspring);
            }
        }

        protected virtual void GenerationStep()
        {
            Best = SelectBest(Population);

            List<IIndividual> parents = null;
            if (Configuration.Rho > 0) // crossover
                parents = Crossover(Population);
            else
                parents = ProcreationSelector.Select(Population, Configuration.Lambda).ToList();

            Mutate(parents);


            Evaluate(Offspring);

            if (Configuration.PlusSelection)
                SelectSurvivors(Offspring.Union(Population).ToList());
            else
                SelectSurvivors(Offspring);
        }


        /// <summary>
        /// Move this to inheriting class
        /// </summary>
        protected virtual void GenerationPostProcessing()
        {
            CGPSelection();
            if (Analyzer != null) Analyzer.Analyze(this);
            Offspring.Clear();
            CurrentGeneration++;
        }

        public virtual IIndividual Evolve()
        {
            Initialize();

            Evaluate(Population);

            Best = SelectBest(Population);

            if (Analyzer != null) Analyzer.Analyze(this);

            while (!Terminator.Terminate(this))
            {
                GenerationStep();

                GenerationPostProcessing();
                // optional: let backgroundworker report progress
                if (Worker != null && Worker.WorkerReportsProgress)
                {
                    ReportProgress();
                    if(Worker.CancellationPending)
                        return Best;
                }
            }
            return Best;
        }

        protected IIndividual SelectBest(IEnumerable<IIndividual> individuals)
        {
            IIndividual best = null;
            individuals = individuals.OrderBy(x => FitnessConfiguration.WeightedFitnessOf(x)).ToList();

            if (FitnessConfiguration.Maximization)
            {
                best = individuals.Last();
            }
            else
            {
                best = individuals.First();
            }

            return best;
        }
   

        private void ReportProgress()
        {
            int offsetInterval = PercentageOffsetMax - PercentageOffsetMin;
            // actually requires more work if different terminators are to be supported
            if (Terminator is GenerationCountTerminator)
            {
                var maxGenerations = (Terminator as GenerationCountTerminator).GenerationsMaximum;
                Worker.ReportProgress(PercentageOffsetMin + (int)((float)CurrentGeneration / maxGenerations * offsetInterval));
            }
        }

        
        public override string ToString()
        {
            string ret = "EvolutionStrategy: \n"
                    + "Creator : " + Creator.ToString() + "\n"
                    + "Mutator : " + Mutator.ToString() + "\n"
                    + "SurvivalSelector : " + SurvivalSelector.ToString() + "\n"
                    + "ProcreationSelector : " + ProcreationSelector.ToString() + "\n"
                    + "Terminator : " + Terminator.ToString() + "\n"
                    + "Configuration : " + Configuration.ToString() + "\n"
                    + "Recombinator : " + (Recombinator == null ? "" : Recombinator.ToString()) + "\n"
                    + "Evaluator : " + Evaluator.ToString() + "\n"
                    + "Analyzer : " + Analyzer.ToString() + "\n"
                    + "Maximization : " + FitnessConfiguration.Maximization.ToString() + "\n";
            return ret;
        }

        /// <summary>
        /// Creates a Copy of a EvolutionSTrategy with respect to a new random object, for recreation
        /// </summary>
        /// <param name="rand"></param>
        /// <returns></returns>
        public ICopyable Copy(IRandom rand)
        {

            var random = rand;

            var copyRand = Creator as ICopyableRandom;
            var creator = copyRand != null ? copyRand.Copy(random) as ICreator : Creator;
            copyRand = Mutator as ICopyableRandom;
            var mutator = copyRand != null ? copyRand.Copy(random) as IMutator : Mutator;
            copyRand = SurvivalSelector as ICopyableRandom;
            var survivalSelector = copyRand != null ? copyRand.Copy(random) as ISelector : SurvivalSelector;
            copyRand = ProcreationSelector as ICopyableRandom;
            var procreationSelector = copyRand != null ? copyRand.Copy(random) as ISelector : ProcreationSelector;

            var copy = Evaluator as ICopyable;
            var evaluator = copy != null ? copy.Copy() as IEvaluator : Evaluator;
            copy = Recombinator as ICopyable;
            var recombinator = copy != null ? copy.Copy() as IRecombinator : Recombinator;
            copy = Terminator as ICopyable;
            var terminator = copy != null ? copy.Copy() as ITerminator : Terminator;
            copy = Analyzer as ICopyable;
            var analyzer = copy != null ? copy.Copy() as IEvolutionAnalyzer : Analyzer;

            return new EvolutionStrategy(creator, mutator,
                survivalSelector, procreationSelector,
                evaluator, recombinator,
                terminator, analyzer, Configuration, FitnessConfiguration);
        }

    }
}
