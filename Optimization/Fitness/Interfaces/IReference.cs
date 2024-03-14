namespace Optimization.Fitness.Interfaces
{
    public interface IReference<TInput, TReference> : IImage
    {
        double ComputeFitness(object actual, FitnessFunction function);

        TInput Input { get; }
        TReference Reference { get; }

        double PercentageOfPixels(TReference actual);
    }
}
