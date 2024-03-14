using System;
using System.Collections.Generic;

namespace Optimization.Fitness.ErrorHandling
{
    /// <summary>
    /// If this is ever thrown (god forbid), then log the exception so its cause can be eradicated.
    /// </summary>
    [Serializable]
    public class UnexpectedException : Exception
    {

        public UnexpectedException(string message) : base(message)
        {

        }


        public Dictionary<int, List<float>> ColumnNodeMap { get; private set; }
        public float Current { get; private set; }
        public Dictionary<float, List<float>> ExecutionTree { get; private set; }
    }
}
