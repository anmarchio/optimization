using System;
using System.Collections.Generic;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.EvolutionStrategy.Analyzers
{
    /// <summary>
    /// Throw all your other Analyzers in here
    /// </summary>
    [Serializable]

    public class MultiAnalyzer : Analyzer
    {
        public List<Analyzer> Analyzers { get; private set; }

        public MultiAnalyzer(List<Analyzer> analyzers)
        {
            Analyzers = analyzers;
        }

    
        public override void Analyze(EvolutionStrategy evolutionStrategy)
        {
            foreach(var analyzer in Analyzers)
            {
                analyzer.Analyze(evolutionStrategy);
            }
        }

        public override ICopyable Copy()
        {
            var list = new List<Analyzer>();
            foreach(var analyzer in Analyzers)
            {
                var copy = analyzer as ICopyable;
                list.Add(copy.Copy() as Analyzer);
            }
            return new MultiAnalyzer(list);
        }

        public override void Save(string directory)
        {
           foreach(var analyzer in Analyzers)
            {
                analyzer.Save(directory);
            }
        }
    }
}
