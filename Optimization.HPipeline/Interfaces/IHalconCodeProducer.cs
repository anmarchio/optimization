using Optimization.HalconPipeline.Interfaces;
using System.Collections.Generic;

namespace Optimization.HPipeline.Interfaces
{
    public interface IHalconCodeProducer : ICodeProducer
    {
        List<string> HalconFunctionCall();
    }
}
