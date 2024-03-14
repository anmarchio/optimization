using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Extensions;
using Newtonsoft.Json;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Terminators;

namespace Optimization.EvolutionStrategy.Analyzers
{
    /// <summary>
    /// Logs average population fitness, average offspring fitness and best individual fitness.
    /// </summary>
    [Serializable]

    public class FitnessValueAnalyzer : Analyzer
    {
        public Dictionary<int, double> AveragePopulationFitnessValue;
        public Dictionary<int, double> AverageOffspringFitnessValue;
        public Dictionary<int, double> BestIndividualFitnessValue;

     
        public FitnessValueAnalyzer()
        {
            AverageOffspringFitnessValue = new Dictionary<int, double>();
            AveragePopulationFitnessValue = new Dictionary<int, double>();
            BestIndividualFitnessValue = new Dictionary<int, double>();
        }

        public override void Analyze(EvolutionStrategy evolutionStrategy)
        {
           var generation = evolutionStrategy.CurrentGeneration;
           var offspring = evolutionStrategy.Offspring;
           var population = evolutionStrategy.Population;
           var fitConfig = evolutionStrategy.FitnessConfiguration;

            if (offspring.Count == 0)
            {
                // hack with .First().Value to keep old behavior in tact. this should use the first fitness function provided
                AveragePopulationFitnessValue.Add(-1, population.Sum(x => fitConfig.WeightedFitnessOf(x)) / population.Count);
                BestIndividualFitnessValue.Add(-1, fitConfig.WeightedFitnessOf(evolutionStrategy.Best));
                AverageOffspringFitnessValue.Add(-1, 0);
                return;    
            }

            AveragePopulationFitnessValue.Add(generation, population.Sum(x => fitConfig.WeightedFitnessOf(x)) / population.Count);
            AverageOffspringFitnessValue.Add(generation, offspring.Sum(x => fitConfig.WeightedFitnessOf(x)) / offspring.Count);
            BestIndividualFitnessValue.Add(generation, fitConfig.WeightedFitnessOf(evolutionStrategy.Best));
        }

        public override void Save(string directory)
        {
            using (var writer = new StreamWriter(Path.Combine(directory, "AvgOffspringFit.txt"), false))
            {
                WriteDictionary(AverageOffspringFitnessValue, writer, "AvgOffspring");
            }
            using (var writer = new StreamWriter(Path.Combine(directory, "AvgPopulationFit.txt"), false))
            {
                WriteDictionary(AveragePopulationFitnessValue, writer, "AvgPopulation");
            }
            using (var writer = new StreamWriter(Path.Combine(directory, "BestIndividualFit.txt"), false))
            {
                WriteDictionary(BestIndividualFitnessValue, writer, "BestIndividual");
            }

            SaveJson(directory);
        }

        public void SaveJson(string directory)
        {
            using (var writer = new StreamWriter(Path.Combine(directory, "AvgOffspringFit.json"), false))
            {
                writer.WriteLine(JsonConvert.SerializeObject(AverageOffspringFitnessValue
                    .Select(x => new {Generation = x.Key, AverageOffspringFitness = x.Value}), Formatting.Indented));
            }
            using (var writer = new StreamWriter(Path.Combine(directory, "AvgPopulationFit.json"), false))
            {
                writer.WriteLine(JsonConvert.SerializeObject(AveragePopulationFitnessValue
                    .Select(x => new {Generation = x.Key, AveragePopulationFitness = x.Value}), Formatting.Indented));
            }
            using (var writer = new StreamWriter(Path.Combine(directory, "BestIndividualFit.json"), false))
            {
                writer.WriteLine(JsonConvert.SerializeObject(BestIndividualFitnessValue.
                    Select(x => new {Generation = x.Key, BestIndividualFitness = x.Value}), Formatting.Indented));
            }
        }

        public static void WriteDictionary(Dictionary<int, double> dictionary, StreamWriter writer, string header)
        {
            writer.WriteLine("Generation," + header);
            foreach (var key in dictionary.Keys)
            {
                writer.WriteLine(key.ToString() + "," + dictionary[key].ToInvariantString());
            }
        }

        public override ICopyable Copy()
        {
            return new FitnessValueAnalyzer();
        }
    }
}
