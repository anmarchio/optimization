using System.Collections.Generic;
using System.IO;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Analyzers
{
    public class MutationProbabilityAnalyzer : Analyzer
    {
        public Dictionary<int, double> MutationProbabilities { get; set; } = new Dictionary<int, double>();

        private bool FirstCall { get; set; } = true;

        public override void Analyze(EvolutionStrategy evolutionStrategy)
        {
            var mutator = evolutionStrategy.Mutator as IAdaptiveMutator;
            var generation = evolutionStrategy.CurrentGeneration;
            if(mutator != null)
            {
                if (FirstCall)
                {
                    MutationProbabilities.Add(-1, mutator.MutationProbability);
                    FirstCall = false;
                }
                else
                    MutationProbabilities.Add(generation, mutator.MutationProbability);
            }
        }

        public override ICopyable Copy()
        {
            return new MutationProbabilityAnalyzer();
        }

        public override void Save(string directory)
        {
            using (var writer = new StreamWriter(Path.Combine(directory, "MutationProbabilities.txt"), false))
            {
                FitnessValueAnalyzer.WriteDictionary(MutationProbabilities, writer, "MutationProbability");
            }
        }
    }
}
