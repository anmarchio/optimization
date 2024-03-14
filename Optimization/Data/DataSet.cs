using Extensions;
using Optimization.Data.Interfaces;

namespace Optimization.Data
{
    public abstract class DataSet<DType> : ISizeInformant
    {
        public abstract DType this[int i] { get; }

        public abstract int Count { get; }

        public abstract bool FitsIntoMemory { get; set; }

        public ImageSize ImageResize { get; set; }
    }
}
