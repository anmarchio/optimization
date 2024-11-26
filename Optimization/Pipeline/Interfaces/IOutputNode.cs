namespace Optimization.HalconPipeline.Interfaces
{
    public interface IOutputNode<TOutput>
    {
        TOutput Output { get; }

        TOutput Execute();

        TOutput Execute(TOutput input);

        void ResetOutput();

    }
}
