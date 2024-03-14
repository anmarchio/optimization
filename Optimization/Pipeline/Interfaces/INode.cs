using System.Collections.Generic;

namespace Optimization.Pipeline.Interfaces
{
    public interface INode
    {
        int Height { get; }

        string ToDOTString();

        float NodeID { get; set; }

        bool IsInputNode { get; }
    }
}
