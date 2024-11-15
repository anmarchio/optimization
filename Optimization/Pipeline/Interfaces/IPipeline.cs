using System.Collections.Generic;
using System.Dynamic;
using System.Windows.Forms;
using Optimization.CartesianGeneticProgramming;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Serialization.Interfaces;

namespace Optimization.Pipeline.Interfaces
{
    public interface IPipeline : IIndividual, ISupportsSerialization
    {
        void ResetOutput();

        bool AllNodesAreUniquelyIdentified();

        void InitializeNodeIDs();

        string ToDOTString(bool includeHeader = true);

        string Name { get; set; }
        
    }

    public interface IPipeline<TNode, TInput, TOutput> : IPipeline<TNode> where TNode : INode
    {
        Dictionary<Node, TOutput> Execute(TInput input);

        Dictionary<Node, TOutput> Execute(Dictionary<float, TInput> inputs);

        TOutput ExecuteSingle(TInput input);

        TOutput ExecuteSingle(Dictionary<float, TInput> inputs);
    }


    public interface IPipeline<TNode> : IPipeline where TNode : INode
    {
        List<TNode> Nodes { get; }
    }
}
