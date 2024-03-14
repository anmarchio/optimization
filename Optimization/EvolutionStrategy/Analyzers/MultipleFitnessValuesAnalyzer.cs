using System;
using System.Collections.Generic;
using System.IO;
using Extensions;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;

namespace Optimization.EvolutionStrategy.Analyzers
{
    /// <summary>
    /// Use this if you use multiple fitness functions -- may have deprecated behavior. Have not used it in a while.
    ///
    /// actually depends on weird implementation of multiplefitnessvalues in IIndividual. This should be reworked in multiple ways.
    /// In particular: we should move the analyzer functionality to something that follows a event based pattern. E.g., analyzers subscribe
    /// to the evaluator and the evaluator broadcasts whenever an individual is evaluated and the corresponding fitness values.
    /// The fact that individuals store their fitness values has very weird side effects, because sometimes some class changes them and others
    /// don't quite notice. In retrospect, pretty poor decision making on my part. Mi scusi.
    /// </summary>
    [Serializable]

    public class MultipleFitnessValuesAnalyzer : Analyzer
    {
        private Dictionary<int, double[][]> MultipleFitnessValues { get; set; }
        private FitnessConfiguration FitnessConfiguration { get; set; }

        public MultipleFitnessValuesAnalyzer(FitnessConfiguration fitConfig)
        {
            FitnessConfiguration = fitConfig;
            MultipleFitnessValues = new Dictionary<int, double[][]>();
        }
        
        public override void Analyze(EvolutionStrategy evolutionStrategy)
        {
            throw new NotImplementedException();

            /*

            Changes made to the way we represent fitness values broke this code.

            We should move this to just store the individual dictionaries that provide fitness information
            and then serialize it using json
             
            var generation = evolutionStrategy.CurrentGeneration;
            var offspring = evolutionStrategy.Offspring;

            double[][] fitnessValues;
            if (offspring.Count == 0)
            {
                var population = evolutionStrategy.Population;
                fitnessValues = new double[population.Count][];
                for (int i = 0; i < population.Count; i++)
                {
                    fitnessValues[i] = population[i].MultipleFitnessValues;
                }
                MultipleFitnessValues.Add(-1, fitnessValues);
                return;
            }
            fitnessValues = new double[offspring.Count][];
            for (int i = 0; i < offspring.Count; i++)
            {
                fitnessValues[i] = offspring[i].MultipleFitnessValues;
            }
            MultipleFitnessValues.Add(generation, fitnessValues);
            */
        }

        public override void Save(string directory)
        {
            using (var writer = new StreamWriter(Path.Combine(directory, "MultFitValuesAnalyzer.txt"), true))
            {
                writer.WriteLine();
                //int maxLength = FitnessConfiguration.FitnessFunctions.Max(x => x.ToString().Length);
                // maxLength = maxLength > "Generation |".Length ? maxLength : "Generation |".Length;
                //int maxLength = 18;
                string headRow = "Generation,"; // + new string(' ', maxLength - "Generation".Length) + "|";
                var fitnessFunctions = FitnessConfiguration.FitnessFunctions;
                for (int i = 0; i < fitnessFunctions.Length - 1; i++)
                {
                    headRow += fitnessFunctions[i].ToString() + ",";// + new string(' ', maxLength - fitnessFunctions[i].ToString().Length) + "|";
                }
                headRow += fitnessFunctions[fitnessFunctions.Length - 1].ToString(); //+ new string(' ', maxLength - fitnessFunctions[fitnessFunctions.Length - 1].ToString().Length);
                writer.WriteLine(headRow);

                foreach (var generation in MultipleFitnessValues.Keys)
                {                
                    int fitnessCount = 1; double value;
                    for (int i = 0; i < MultipleFitnessValues[generation].Length; i++)
                    {
                        string row = generation.ToString() + ","; // + new string(' ', maxLength - generation.ToString().Length) + "|";
                        if (MultipleFitnessValues[generation][i] == null) continue;
                        fitnessCount = MultipleFitnessValues[generation][i].Length;
                        for (int j = 0; j < fitnessCount - 1; j++)
                        {
                            value = MultipleFitnessValues[generation][i][j];
                            //length = maxLength - value.ToInvariantString().Length;
                            //length = length > 0 ? length : 0;
                            row += value.ToInvariantString() + ","; // + new string(' ', length) + "|";
                        }
                        value = MultipleFitnessValues[generation][i][fitnessCount - 1];
                        //length = maxLength - value.ToString().Length;
                        //length = length > 0 ? length : 0;
                        row += value.ToInvariantString(); //+ new string(' ', length);
                        writer.WriteLine(row);
                    }

                }
                writer.WriteLine();
            }
        }

        public override ICopyable Copy()
        {
            return new MultipleFitnessValuesAnalyzer(FitnessConfiguration);
        }
    }
}
