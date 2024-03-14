namespace Optimization.Pipeline.Interfaces
{
    public interface IInputNode<TInput> : IOutputNode<TInput>, IInputNode
    {
        TInput Input { get; set; }
    }

    public interface IInputNode : INode
    {
        float ProgramInputIdentifier { get; set; }
    }

}
