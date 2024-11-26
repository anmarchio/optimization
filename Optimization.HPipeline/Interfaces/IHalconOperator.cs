using HalconDotNet;
using Optimization.HalconPipeline.Interfaces;

namespace Optimization.HPipeline.Interfaces
{
    public interface IHalconOperator : IOutputDisposable
    {
        HObject Execute();

        HObject Output { get; }

        float NodeID { get; set; }
    }
}
