using Optimization.Data;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.HPipeline.Fitness;

namespace Optimization.HPipeline
{
    public class HalconPipelineEvaluator : HalconPipelineCGPEvaluator
    {
        public HalconPipelineEvaluator(DataLoader<ReferenceImage> trainData, DataLoader<ReferenceImage> valData, HalconFitnessConfiguration fitnessConfiguration) : base(trainData, valData, fitnessConfiguration)
        {
            IndividualsEvaluated = 0;
        }

        public override ICopyable Copy()
        {
            return new HalconPipelineEvaluator(TrainDataLoader, ValidationDataLoader, (HalconFitnessConfiguration)FitnessConfiguration);
        }
    }
}
