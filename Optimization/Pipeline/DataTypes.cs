using System;

namespace Optimization.HalconPipeline
{
    [Flags]
    public enum DataTypes : short
    {
        Image = 2,
        ROI = 4,
        EdgeImage = 8,  // specifically for one operator... well...

        // for pi evolution
        Trigonometric = 16,
        Real = 32,        
    }
}
