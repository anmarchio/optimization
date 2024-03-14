using System.Collections.Generic;
using System.Linq;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Pipeline.Interfaces;

namespace Optimization.Pipeline
{
    /// <summary>
    /// 
    /// </summary>
    public class Optimizer<TPipeline, TNode> where TPipeline : IPipeline<TNode> where TNode : IParameterInformant
    {

        /// <summary>
        /// Use this constructor if you want to perform a full search. Warning: may take years.
        /// </summary>
        /// <param name="pipeline">Pipeline to be optimized</param>
        /// <param name="analyzer">Analyzer to measure fitness</param>
        public Optimizer(IPipeline<TNode> pipeline, IEvaluator analyzer)
        {
            Initialize(pipeline, analyzer, pipeline.Nodes.ToList());
        }

        public Optimizer(IPipeline<TNode> pipeline, IEvaluator analyzer, List<TNode> nodeSubset)
        {
            Initialize(pipeline, analyzer, nodeSubset);
        }

        private void Initialize(IPipeline<TNode> pipeline, IEvaluator evaluator, List<TNode> nodes)
        {
            if (!pipeline.AllNodesAreUniquelyIdentified()) pipeline.InitializeNodeIDs();

            Pipeline = pipeline;
            Evaluator = evaluator;
            Nodes = nodes.Cast<IParameterInformant>().ToList();
            Parameterization = new Dictionary<float, float[]>();
            Enumerator = new Dictionary<float, IEnumerator<float[]>>();

            foreach (var node in Nodes) Parameterization.Add(node.NodeID, node.ToCGPNodeParameters());

            var enu = Nodes.Select(x => x.EnumerateParameters().ToList().Count).Select(x => (long)x).ToArray();

            NecessaryEvaluations = enu.Any() && enu[0] > 0 ? enu[0] : 1;
            for (int i = 1; i < enu.Length; i++) NecessaryEvaluations *= enu[i] > 0 ? enu[i] : 1;
        }

        public IPipeline<TNode> Pipeline { get; private set; }
        public IEvaluator Evaluator { get; set; }

        /// <summary>
        /// Maps from Nodename to Parameterization
        /// </summary>
        public Dictionary<float, float[]> Parameterization;

        private Dictionary<float, IEnumerator<float[]>> Enumerator;

        private List<IParameterInformant> Nodes { get; set; }

        public long NecessaryEvaluations { get; private set; }

        /// <summary>
        /// Tests every possible combination of halcon node parameters. May take a long time.
        /// </summary>
        public double Optimize()
        {
            double max = 0, tmp = 0;
            foreach (var node in Nodes) Enumerator.Add(node.NodeID, node.EnumerateParameters().GetEnumerator());
            foreach (var node in Nodes)
            {
                Enumerator[node.NodeID].MoveNext();
                node.FromCGPNodeParameters(Enumerator[node.NodeID].Current);
            }

            Evaluator.Evaluate(Pipeline);
            max = Evaluator.WeightedFitnessOf(Pipeline);

            var stack = new Stack<IParameterInformant>(Nodes.Where(x => x.CGPParameterCount > 0));

            while (stack.Count > 0)
            {

                var current = stack.Pop();
                if (!Enumerator[current.NodeID].MoveNext())
                {
                    Enumerator[current.NodeID] = current.EnumerateParameters().GetEnumerator();
                    Enumerator[current.NodeID].MoveNext();
                    current.FromCGPNodeParameters(Enumerator[current.NodeID].Current);
                    continue;
                }

                var param = Enumerator[current.NodeID].Current;
                current.FromCGPNodeParameters(param);

                Evaluator.Evaluate(Pipeline);
                tmp = Evaluator.WeightedFitnessOf(Pipeline);

                if (tmp > max)
                {
                    max = tmp;
                    foreach (var node in Pipeline.Nodes) Parameterization[node.NodeID] = node.ToCGPNodeParameters();
                }

                for (int i = Nodes.IndexOf(current); i < Nodes.Count; i++)
                {
                    if (Nodes[i].CGPParameterCount > 0)
                        stack.Push(Nodes[i]);
                }
            }

            foreach(var node in Nodes)
            {
                node.FromCGPNodeParameters(Parameterization[node.NodeID]);
            }


            return max;
        }
    }
}
