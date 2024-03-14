using System.Xml.Serialization;
using Optimization.Fitness;

namespace Optimization.HPipeline.Fitness
{
    public class HalconFitnessConfiguration : FitnessConfiguration
    {

        public HalconFitnessConfiguration()
        {
            FitnessFunctions = new FitnessFunction[] { FitnessFunction.MCC };
            Weights = new double[] {1};
            Initialize(null, null, null, true);
        }

        public HalconFitnessConfiguration(FitnessFunction[] fitnessFunctions, double[] weights, bool maximization = true)
        {
            FitnessFunctions = fitnessFunctions;
            Weights = weights;
            Initialize(null, null, null, maximization);
        }

        #region legacy constructors... separated reference data from the rest of the configuration
        public HalconFitnessConfiguration(ReferenceSet refSet, FitnessFunction[] fitnessFunctions, double[] weights, bool maximization = true)
        {
            FitnessFunctions = fitnessFunctions;
            Weights = weights;
            ReferenceSet = refSet;
            ValidationSet = refSet;
            Initialize(null, null, null, maximization); // to init WeightsOf dict
        }

        public HalconFitnessConfiguration(ReferenceSet refSet, FitnessFunction[] fitnessFunctions, double[] weights, double fScoreBetaSquare, bool maximization = true)
        {
            FitnessFunctions = fitnessFunctions;
            Weights = weights;
            ReferenceSet = refSet;
            ValidationSet = refSet;
            FScoreBetaSquare = fScoreBetaSquare;
            Initialize(null, null, null, maximization); // to init WeightsOf dict
        }

        public HalconFitnessConfiguration(ReferenceSet refSet, FitnessFunction[] fitnessFunctions, double[] weights, int? regionCountThreshold, int? executionTimeThreshold, double? pixelPercentageThreshold, bool maximization = true)
        {
            FitnessFunctions = fitnessFunctions;
            Weights = weights;
            ReferenceSet = refSet;
            ValidationSet = refSet;

            Initialize(regionCountThreshold, executionTimeThreshold, pixelPercentageThreshold, maximization);
        }

        public HalconFitnessConfiguration(ReferenceSet refSet, FitnessFunction[] fitnessFunctions, double[] weights, double fScoreBetaSquare, int? regionCountThreshold, int? executionTimeThreshold, double? pixelPercentageThreshold, bool maximization = true)
        {
            FitnessFunctions = fitnessFunctions;
            Weights = weights;
            ReferenceSet = refSet;
            ValidationSet = refSet;
            FScoreBetaSquare = fScoreBetaSquare;

            Initialize(regionCountThreshold, executionTimeThreshold, pixelPercentageThreshold, maximization);
        }

        #endregion


        [XmlIgnore]
        public ReferenceSet ReferenceSet { get; set; }

        [XmlIgnore]
        public ReferenceSet ValidationSet { get; set; }


        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            var tmp = obj as HalconFitnessConfiguration;

            if (tmp.FitnessFunctions.Length != FitnessFunctions.Length) return false;
            for (int i = 0; i < FitnessFunctions.Length; i++)
            {
                if (tmp.FitnessFunctions[i] != FitnessFunctions[i]) return false;
            }

            if (ReferenceSet == null ^ tmp.ReferenceSet == null) return false;
            if (ReferenceSet != null && tmp.ReferenceSet != null && ReferenceSet.DirectorySize != tmp.ReferenceSet.DirectorySize) return false;
            if (ValidationSet == null ^ ValidationSet == null) return false;
            if (ValidationSet != null && tmp.ValidationSet != null && ValidationSet.DirectorySize != tmp.ValidationSet.DirectorySize) return false;
            if (RegionCountThreshold != tmp.RegionCountThreshold) return false;
            if (PixelPercentageThreshold != tmp.PixelPercentageThreshold) return false;
            if (ExecutionTimeThreshold != tmp.ExecutionTimeThreshold) return false;

            return true;
        }


        public override int GetHashCode()
        {
            return (int)ConfigurationType;
        }
    }
}
