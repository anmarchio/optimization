using System;

namespace Optimization.HalconPipeline
{
    [Flags]
    public enum OperatorType : short  // formerly: Filter, Threshold, Morphological
    {
        Default = 0,
        ImageToImage = 1,
        ImageToRegion = 2,
        RegionToRegion = 4,
        ImageAndRegionToRegion = 8,
        EdgeAmpAndRegionToRegion = 256,
        InputImageAndRegionToRegion = 512,
        ImageToXLDContData = 1024,
        Bug = 2048,

        InputNode = 16, 
        EdgeAmplitude = 32,
        Threshold = 64,
        Filter = 128,
    }
}
