using System;
using System.Diagnostics;
using Extensions;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Analyzers
{
    /// <summary>
    /// Well. Spams the console. Alternatively spams the Output window while debugging.
    /// </summary>
    [Serializable]


    public class ConsoleStatusUpdateAnalyzer : Analyzer
    {
        public ConsoleStatusUpdateAnalyzer()
        {

        }

        public ConsoleStatusUpdateAnalyzer(int interval)
        {
            Interval = interval;
        }

        public int? Interval { get; private set; }
       

        public override void Analyze(EvolutionStrategy evolutionStrategy)
        {
            var eval = evolutionStrategy.Evaluator as IEvaluator;
            var line = $"Generation: {evolutionStrategy.CurrentGeneration} " +
                       $"#eval: {evolutionStrategy.Evaluator.IndividualsEvaluated} " +
                       $"best: {evolutionStrategy.FitnessConfiguration.WeightedFitnessOf(evolutionStrategy.Best).ToInvariantString()}";

            if (Interval == null)
            {
                Console.WriteLine(line);
                Trace.WriteLine(line);
            }
            else if(evolutionStrategy.CurrentGeneration % Interval == 0)
            {
                Console.WriteLine(line);
                Trace.WriteLine(line);
            }
        }

        public override ICopyable Copy()
        {
            if (Interval == null)
                return new ConsoleStatusUpdateAnalyzer();
            else
                return new ConsoleStatusUpdateAnalyzer((int)Interval);
        }

        public override void Save(string filename)
        {
            // ignore - - this analyzer has nothing to write
        }
    }
}
