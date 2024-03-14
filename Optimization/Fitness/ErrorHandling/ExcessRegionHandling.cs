namespace Optimization.Fitness.ErrorHandling
{
    /// <summary>
    /// Specify the action when a RegionException (i.e. too many ROIs are produced) is thrown.
    /// </summary>
    public enum ExcessRegionHandling
    {
        ThrowException, Union1, ContinuousSelectShape, None
    }
}
