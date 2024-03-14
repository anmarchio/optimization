using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace Optimization.Pipeline
{
    public class OperatorException : Exception
    {
        /// <summary>
        /// This exceptions adds all information contained in Node.GetRepresentation() for all Childnodes of node (including node) to the 
        /// exceptions Data dictionary.
        /// 
        /// Use this to provide additional debugging information by e.g. iteration through the data dictionary and logging the child nodes.
        /// </summary>
        /// <param name="operatorInformation">should include assemblyqualifiedname and parameters</param>
        /// <param name="innerException"></param>
        public OperatorException(Node node, Exception innerException) : base($"Exception caused at: {node.GetRepresentation()}, {innerException.Message}", innerException)
        {
            Node = node;
        }

        public Node Node { get; private set; }

        /// <summary>
        /// Gets a representation string of all nodes in the subtree of Node
        /// in order to print their parameters to the logging file.
        /// Remember that all nodes in the subtree are executed before Node,
        /// so they might be relevant for debugging the exception thrown at Node
        /// </summary>
        /// <returns>Dictionary for json serialization containing all child nodes</returns>
        public Dictionary<string, List<string>> GetSubtree()
        {
            var descendants = Node.GetDescendants();
            var heights = new Dictionary<string, List<string>>();
            foreach (var x in descendants)
                heights.Add($"Height {x.Key}", x.Value.Select(y => y.GetRepresentation()).ToList());

            return heights;
        }

        public void UseSerilog()
        {
            foreach (var x in GetSubtree())
                Data.Add(x.Key, x.Value);
            Log.Error("{Exception}{NewLine}{Properties}", this,Environment.NewLine, GetSubtree(), Node.GetType());
        }
    }
}
