using HalconDotNet;

namespace Optimization.HPipeline.Interfaces
{
    public interface IHObjectComparable
    {
        bool OutputEquals(HObject hobject);
    }
}
