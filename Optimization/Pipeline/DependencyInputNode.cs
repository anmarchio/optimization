using System;

namespace Optimization.HalconPipeline
{
    [Serializable]
    public class DependencyInputNode : DependencyNode
    {
        public DependencyInputNode(float programInputIdentified) : base(OperatorType.InputNode)
        {
            ProgramInputIdentifier = programInputIdentified;
        }

        public float ProgramInputIdentifier { get; set; }
    }
}
