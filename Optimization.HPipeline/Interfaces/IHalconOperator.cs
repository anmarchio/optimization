using HalconDotNet;
using Optimization.Pipeline.Interfaces;

namespace Optimization.HPipeline.Interfaces
{
    public interface IHalconOperator : IOutputDisposable
    {
        HObject Execute();

        HObject Output { get; }

        float NodeID { get; set; }
    }
}
