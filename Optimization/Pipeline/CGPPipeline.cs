using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Optimization.CartesianGeneticProgramming;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Random;
using Optimization.Fitness;
using Optimization.Pipeline.Interfaces;

namespace Optimization.Pipeline
{
    [Serializable]
    public class CGPPipeline<TInput, TOutput, TNode, TInputNode, TOutputNode, TWeight > : Pipeline<TInput, TOutput, TNode, TInputNode, TOutputNode, TWeight>, IPipeline<TNode, TInput, TOutput>, IFloatVectorConvertible where TNode : CGPNode<TOutput>, IParameterInformant, IOutputDisposable where TInputNode : IOutputNode<TInput>, TNode where TOutputNode : TNode, IOutputNode<TOutput>
    {
        public CGPPipeline() : base()
        {

        }

        public CGPPipeline(FloatVector vector, CGPConfiguration config) : base(vector, config)
        {

        }

        public CGPPipeline(params TOutputNode[] outputNodes) : base(outputNodes)
        {

        }

        /*
        public CGPPipeline(List<TInputNode> inputNodes, List<TOutputNode> outputNodes) : base(inputNodes, outputNodes)
        {

        }*/

        /// <summary>
        /// Does NOT free input nodes. This is intended, as they contain references to images that might be used elsewhere.
        /// If you know for sure that you do not require the input images anymore, free them manually.
        /// </summary>
        public void ResetOutput()
        {
            foreach (var node in TraverseBreadthBackward())
            {
                if (node.IsInputNode) continue;
                var outputNode = node as IOutputNode<TOutput>;
                if (outputNode != null) outputNode.ResetOutput();
            }
        }

        public bool AutoDisposeIntermediateOutputs { get; set; } = false;

        /*
        public override Dictionary<Node, TOutput> Execute(TInput input)
        {
            ResetOutput();
            var dict = base.Execute(input);
            if (AutoDisposeIntermediateOutputs) DisposeIntermediateOutputs();
            return dict;
        }*/

        public override Dictionary<Node, TOutput> Execute(TInput input)
        {
            ResetOutput();
            var dict = base.Execute(input);
            if (AutoDisposeIntermediateOutputs) DisposeIntermediateOutputs();
            return dict;
        }

        public override object ExecuteSingle(object input)
        {
            ResetOutput();
            var ret = base.ExecuteSingle(input);
            if (AutoDisposeIntermediateOutputs) DisposeIntermediateOutputs();
            return ret;
        }

        public override TOutput ExecuteSingle(TInput input)
        {
            ResetOutput();
            var ret =  base.ExecuteSingle(input);
            if (AutoDisposeIntermediateOutputs) DisposeIntermediateOutputs();
            return ret;
        }

        public void DisposeIntermediateOutputs()
        {
            foreach (var node in Nodes)
            {
                if (OutputNodes.Contains(node)) continue;
                var op = node as IOutputDisposable;
                if (op == null) continue;
                op.DisposeOutput();
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Maps this HalconPipeline to a FloatVector encoding for CGP optimization. Creates a random FloatVector and then embedds the pipelines encoding in it.
        /// </summary>
        /// <param name="operatorEncoder">usually the operatormap used should implement this interface</param>
        /// <param name="configuration">if a fixed cgp configuration is desired.</param>
        /// <returns></returns>
        public FloatVector ToCGPEncoding(IOperatorEncoder operatorEncoder, CGPConfiguration configuration, IRandom random)
        {
            FloatVector vector;
            ToCGPEncoding(operatorEncoder, out vector, configuration, random);
            return vector;
        }

        public int EstimateCgpColumnCount(DependencyTree tree)
        {
            var layers = Layers;
            layers = layers.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            var dependencyTypes = tree.Nodes.Select(x => x.OperatorType);
            var pipeTypes = Nodes.Select(x => x.OperatorType);

            if (dependencyTypes.Intersect(pipeTypes).Count() > dependencyTypes.Count())
                throw new Exception();
            /*
            int count = 0;
            foreach (var layer in layers)
            {
                count += layer.Value.Select(x => x.OperatorType).Distinct().Count();
            }

            return count;*/

            // output nodes (of the dependency tree) are repeated; we need to consider that possible only every xth column can actually be used by this pipeline
            // what we actually need to do is to traverse the dependency tree and the pipeline and
            // check how far into the pipeline we get until we hit the cycle part (see dependencytree.cylce)
            // and then multiply this with the remaining nodes of the pipeline
            // or some such thing...

            //var minRequired = tree.Layers.Sum(x => x.Value.Count); // this is the minimal number of columns that is used by default as a consequence
            // of the dependency tree. Each node in the dependency tree is mapped to an individual column
            
            // then we also need to consider that most pipelines have certain repetition
            
            return (int) (tree.Nodes.Where(x => !(x is IInputNode)).Count());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="inputNodes">Maps from input nodes in the pipeline to specific dependencytree input identifiers,
        /// that can be used to tell the pipeline if there are more than one inputs and which operators are allowed to use which
        ///
        /// defaults to dependencyInputNode(-1) for default cgp single image input</param>
        /// <returns></returns>
        public DependencyTree ToDependencyTree(Dictionary<Node, DependencyInputNode> inputNodes = null)
        {
            if (inputNodes == null)
            {
                inputNodes = new Dictionary<Node, DependencyInputNode>();
                var inNode = new DependencyInputNode(-1);
                foreach(var node in InputNodes)
                    inputNodes.Add(node, inNode);
            }


            if(!AllNodesAreUniquelyIdentified()) InitializeNodeIDs();
            var nodes = new List<DependencyNode>();

            foreach (var p in Nodes.Where(x => !(x is IInputNode)))
            {
                nodes.Add(new DependencyNode(p.OperatorType) {NodeID = p.NodeID});
            }

            foreach (var p in Nodes)
            {
                foreach (var c in p.Children)
                {
                    if (c is IInputNode) continue;
                    var treeChildren = nodes.Find(x => x.NodeID == c.NodeID);
                    nodes.Find(x => x.NodeID == p.NodeID).AddChild(treeChildren);
                }
            }

            foreach (var p in InputNodes)
            {
                foreach(var parent in p.Parents)
                    nodes.Find(x => x.NodeID == parent.NodeID).AddChild(inputNodes[p]);
            }
            
            var outputNodeIds = OutputNodes.Select(y => y.NodeID);
            var tree = new DependencyTree(nodes.Where(x => outputNodeIds.Contains(x.NodeID)).ToArray());
            return tree;
        }

        private void ToCGPEncoding(IOperatorEncoder operatorEncoder, out FloatVector vector, CGPConfiguration configuration, IRandom random)
        {
            var creator = new CGPFloatVectorCreator(random, configuration);
            vector = creator.Create().FloatVector;

            var layers = Layers;

            layers = layers.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

            var supportedOperators = operatorEncoder.Dependencies.Nodes.Select(x => x.OperatorType);

            if(!Nodes.All(x => supportedOperators.Any(y => x.IsOrOperatorType(y))))
                throw new ArgumentException();
            // we need to know which operator is allowed in which column (which the cgp config can tell us)

           
            var nodeColumnMapping = new Dictionary<INode, int>(); // holds which node _was_ mapped to which column
            // this information should help with determining which is the leftmost column that parent nodes may be mapped to

            foreach (var layer in layers.Keys)
            {
                for (int i = 0; i < layers[layer].Count; i++)
                {
                    var node = layers[layer][i];
                    var encoded = operatorEncoder.Encode(node);
                    // get lowest allowed column

                    var columnCandidates = configuration.OperatorBounds.Where(x => x.Value.Contains(encoded))
                        .Select(x => x.Key);
                    if (node.Children != null && node.Children.Any(x => nodeColumnMapping.ContainsKey(x)))
                    {
                        var childColumns = new List<int>();
                        foreach(var child in node.Children)
                            if (nodeColumnMapping.ContainsKey(child))
                                childColumns.Add(nodeColumnMapping[child]);
                        columnCandidates = columnCandidates.Where(x => x > childColumns.Max());
                    }
                    if(columnCandidates.Count() == 0)
                        throw new IndexOutOfRangeException("ColumnCandidates is empty. This is usually caused by " +
                                                           "a too low column count. This should not happen once the implementation of" +
                                                           "DependencyTree.EstimateCgpColumnCount returns the appropriate value.");
                    int leftmost = columnCandidates.Min();
                    nodeColumnMapping[node] = leftmost;

                    // if all nodes in this layer map to the same column, there will be no collision if we add i
                    var nodeIdx = configuration.FirstAndLastNodeIDOfColumn(leftmost).Item1 + i;

                    node.NodeID = nodeIdx;
                    // set nodeOperator
                    vector[configuration.OperatorIndex(node.NodeID)] = operatorEncoder.Encode(node);

                    // set input nodenames
                    for (int p = 0; p < layers[layer][i].Children.Count; p++)
                    {
                        var child = layers[layer][i].Children[p];
                        var id = child.NodeID;
                        if (child is IInputNode)
                            id = (child as IInputNode).ProgramInputIdentifier;
                        vector[configuration.NodeIndex(node.NodeID) + p] = id;
                    }

                    // set parameters
                    var parameters = layers[layer][i].ToCGPNodeParameters();
                    for (int j = 0; j < parameters.Length; j++)
                    {
                        vector[configuration.ParameterIndex(node.NodeID) + j] = parameters[j];
                    }
                    for (int j = parameters.Length; j < configuration.ParameterCount; j++)
                    {
                        vector[configuration.ParameterIndex(node.NodeID) + j] = 0;
                    }
                }
            }

            for (int i = vector.Length - configuration.OutputsCount, j = 0; i < vector.Length; i++, j++)
                vector[i] = OutputNodes[j].NodeID;



            /*

            foreach (var column in layers.Keys)
            {
                for (int i = 0; i < layers[column].Count; i++)
                {
                    var nodeName = column * configuration.RowCount + i;
                    layers[column][i].NodeID = nodeName;
                    // set nodeOperator
                    var index = configuration.OperatorIndex(nodeName);
                    vector[index] = operatorEncoder.Encode(layers[column][i]);

                    // set input nodenames
                    index = configuration.NodeIndex(nodeName);
                    for (int p = 0; p < layers[column][i].Children.Count; p++)
                    {
                        vector[index + p] = layers[column][i].Children[p].NodeID;
                    }

                    // set parameters
                    var parameters = layers[column][i].ToCGPNodeParameters();
                    index = configuration.ParameterIndex(nodeName);
                    for (int j = 0; j < parameters.Length; j++)
                    {
                        vector[index + j] = parameters[j];
                    }
                    for (int j = parameters.Length; j < configuration.ParameterCount; j++)
                    {
                        vector[index + j] = 0;
                    }
                }

            }

            for (int i = vector.Length - configuration.OutputsCount, j = 0; i < vector.Length; i++, j++)
                vector[i] = OutputNodes[j].NodeID;
                */
        }

        /// <summary>
        /// The maximum number of parameters of any HalconOperatorNode used in this pipeline. Necessary for CGP - Pipeline mapping
        /// </summary>
        public int MaxParameterCount { get { return Nodes.Max(x => x.CGPParameterCount); } }
        public int MaxChildren { get { return Nodes.Max(x => x.Children.Count); } }


        /// <summary>
        /// Since the behavior deviates for different types of CGP with regards to the Configuration, this can be overridden. E.g. FiPipeline asumes +1 parameter gene to manage the number of input nodes
        /// </summary>
        /// <param name="operatorEncoder"></param>
        /// <param name="vector"></param>
        /// <param name="configuration"></param>
        /// <param name="forceLevelsBack"></param>
        public virtual void ToCGPEncoding(IOperatorEncoder operatorEncoder, IRandom random, out FloatVector vector, out CGPConfiguration configuration, int forceLevelsBack = 1)
        {
            configuration = new CGPConfiguration(Width, EstimateCgpColumnCount(operatorEncoder.Dependencies), forceLevelsBack, MaxChildren, MaxParameterCount, operatorEncoder.OperatorMap, InputNodes.Count, OutputNodes.Count);
            // used to be columnCount = Height (now Nodes.COunt) +1
            ToCGPEncoding(operatorEncoder, out vector, configuration, random);
        }

        public Dictionary<Node, TOutput> Execute(Dictionary<float, TInput> inputs)
        {
            foreach (var programInputIdentifier in inputs.Keys)
            {
                var node = InputNodes.Find(x => x.NodeID == programInputIdentifier);
                if (node == null) continue;
                node.Execute(inputs[programInputIdentifier]);
            }
            var results = new Dictionary<Node, TOutput>();
            foreach(var outputNode in OutputNodes)
            {
                results.Add(outputNode, outputNode.Execute());
            }
            return results;
        }


        public TOutput ExecuteSingle(Dictionary<float, TInput> inputs)
        {
            if (OutputNodes.Count > 1) throw new Exception("do not use execute single if there are more than one output node");
            foreach (var programInputIdentifier in inputs.Keys)
            {
                var node = InputNodes.Find(x => x.NodeID == programInputIdentifier);
                if (node == null) continue;
                node.Execute(inputs[programInputIdentifier]);
            }
            return OutputNodes.First().Execute();
        }

        private static ObjectIDGenerator objectIdGenerator = null;

        private static ObjectIDGenerator ObjectIDGenerator
        {
            get
            {
                if (objectIdGenerator == null)
                    objectIdGenerator = new ObjectIDGenerator();
                return objectIdGenerator;
            }
        }

        public long GetId()
        {
            bool firstTime;
            return ObjectIDGenerator.GetId(this, out firstTime);
        }
    }
}
