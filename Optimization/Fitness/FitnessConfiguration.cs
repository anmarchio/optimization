using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms.Layout;
using System.Xml.Serialization;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness.ErrorHandling;

namespace Optimization.Fitness
{
    /// <summary>
    /// Responsible for various threshold types (region count, execution time, pixel percentage) as well as the fitness functions, their weights and the reference set.
    /// </summary>
    public class FitnessConfiguration : IConfiguration
    {
        public FitnessConfiguration()
        {
            FitnessFunctions = new FitnessFunction[] { FitnessFunction.MCC };
            Weights = new double[] {1};
            Initialize(null, null, null, true);
        }

        public FitnessConfiguration(FitnessFunction fitnessFunction, int? regionCountThreshold, int? executionTimeThreshold, double? pixelPercentageThreshold, bool maximization = true)
        {
            FitnessFunctions = new FitnessFunction[] { fitnessFunction };
            Weights = new double[] {1};
            Initialize(regionCountThreshold, executionTimeThreshold, pixelPercentageThreshold, maximization);
        }

        protected void Initialize(int? regionCountThreshold, int? executionTimeThreshold, double? pixelPercentageThreshold, bool maximization)
        {
            if(FitnessFunctions.Length != Weights.Length) throw new ArgumentException("Weights and FitnessFunctions must be of same length.");
            RegionCountThreshold = regionCountThreshold;
            ExecutionTimeThreshold = executionTimeThreshold;
            PixelPercentageThreshold = pixelPercentageThreshold;
            WeightOf = Enumerable.Range(0, FitnessFunctions.Length)
                .ToDictionary(x => FitnessFunctions[x], x => Weights[x]);
            Maximization = maximization;
        }

        public FitnessConfiguration(FitnessFunction[] fitnessFunctions, double[] weights, int? regionCountThreshold, int? executionTimeThreshold, double? pixelPercentageThreshold, bool maximization = true)
        {
            FitnessFunctions = fitnessFunctions;
            Weights = weights;
            Initialize(regionCountThreshold, executionTimeThreshold, pixelPercentageThreshold, maximization);
        }


        public bool Maximization { get; set; }


        public int RegionScoreWeight = 0;
        public int ArtifactScoreWeight = 0;
        
        public int FitnessScoreWeight = 1;

        private int individualWeightTypesSum = -1;
        public int IndividualWeightTypesSum
        {
            get
            {
                if (individualWeightTypesSum == -1)
                {
                    individualWeightTypesSum = RegionScoreWeight + ArtifactScoreWeight + FitnessScoreWeight;
                }
                return individualWeightTypesSum;
            }
        }

        public double[] Weights { get; set; }

        public double WeightSum
        {
            get
            {         
                    if (Weights != null) return Weights.Sum();
                    return 1;  
            }
        }

        public double WeightedFitnessOf(IIndividual individual)
        {
            return individual.Fitness.Sum(x => x.Value * WeightOf[x.Key]) / WeightSum ?? (Maximization ? double.MinValue : double.MaxValue);
        }

        public FitnessFunction[] FitnessFunctions { get; set; }

        [XmlIgnore]
        public RegionScore[] RegionScores { get; set; }

        [XmlIgnore]
        public ArtifactScore[] ArtifactScores { get; set; }

       
        //public FitnessFunctionParameters FitnessFunctionParameters { get; internal set; }
        public ExcessRegionHandling ExcessRegionHandling { get; set; } = ExcessRegionHandling.None;

        [XmlIgnore]
        public double FScoreBetaSquare { get; protected set; }

        public int? RegionCountThreshold { get; set; }
        public bool UseRegionCountThreshold { get { return RegionCountThreshold != null; } }
        public int? ExecutionTimeThreshold { get; set; }
        public bool UseExecutionTimeThreshold { get { return ExecutionTimeThreshold != null; } }
        public bool UseExecutionTimeFitnessPenalty { get; set; }
        public double ExecutionTimeFunctionScaleFactor { get; set; }
        public double? PixelPercentageThreshold { get; set; }
        public bool UsePixelPercentageThreshold { get { return PixelPercentageThreshold != null; } }


        public ConfigurationType ConfigurationType
        {
            get
            {
                return ConfigurationType.Fitness;
            }
        }

        public bool SerializeBinarySupported
        {
            get
            {
                return false;
            }
        }

        public bool SerializeXmlSupported
        {
            get
            {
                return true;
            }
        }

        public string Filename { get; set; } = "/Default/";
        [XmlIgnore]
        public Dictionary<FitnessFunction, double> WeightOf { get; private set; }

        internal void Print(string configDirectory)
        {
            using (var writer = new StreamWriter(configDirectory + "\\" + "FitnessConfiguration.txt"))
            {
                writer.WriteLine("RegionCountThreshold: " + RegionCountThreshold);
                writer.WriteLine("ExecutionTimeThreshold " + ExecutionTimeThreshold);
                writer.WriteLine("PixelPercentageThreshold" + PixelPercentageThreshold);

                int i = 0;
                foreach (var func in FitnessFunctions)
                {
                    writer.WriteLine(func.ToString() + Weights[i]);
                    i++;
                }

                writer.WriteLine("FScoreBetaSquare " + FScoreBetaSquare);
            }
        }

        public void SerializeXml(string filename)
        {
            using (var writer = new StreamWriter(filename))
            {
                var xml = new XmlSerializer(GetType());
                xml.Serialize(writer, this);
            }
        }

        public void SerializeBinary(string filename)
        {
            throw new NotImplementedException();
        }
    }
}