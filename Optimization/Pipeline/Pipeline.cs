using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Extensions;
using Optimization.CartesianGeneticProgramming;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;
using Optimization.Pipeline.Interfaces;
using Optimization.Serialization.Interfaces;

namespace Optimization.Pipeline
{

    [Serializable]
    public class Pipeline<TInput, TOutput, TNode, TInputNode, TOutputNode, TWeight> : ICopyable, ISupportsSerialization where TNode : Node where TInputNode : TNode, IOutputNode<TInput> where TOutputNode : TNode, IOutputNode<TOutput>
    {
        [XmlIgnore]
        public List<TOutputNode> OutputNodes { get; set; }
        [XmlIgnore]
        public List<TInputNode> InputNodes { get; set; }

        public virtual List<Edge> AdjacencyList { get; set; } // contains edges of type x -> y

        [XmlIgnore]
        public Dictionary<float, TNode> NodeList { get; set; }

        protected Pipeline()
        {
            OutputNodes = new List<TOutputNode>();
            InputNodes = new List<TInputNode>();
        }

        public Pipeline(params TOutputNode[] outputNodes)
        {
            OutputNodes = outputNodes.ToList();
            InputNodes = new List<TInputNode>();
            var leaves = TraverseBreadthBackward().Where(x => !x.HasChildren());

            foreach (var leaf in leaves)
            {
                if (!leaf.IsInputNode) // this is for backwards compatibility with old pipelines and to spare oneself the hassle
                                       // of having to specify input nodes if every input is identical. Will not work with
                                       // multiple inputs where some identification is necessary.
                {
                    var inputNode = Activator.CreateInstance(typeof(TInputNode)) as TInputNode;
                    leaf.AddChild(inputNode);
                    InputNodes.Add(inputNode);
                    inputNode.NodeID = -1;
                    var iInputNode = inputNode as IInputNode<TInput>;
                    if (iInputNode != null) iInputNode.ProgramInputIdentifier = inputNode.NodeID; // behavior with prgraminputidentifier und nodeid must be unified asap
                }
                else if (!InputNodes.Contains(leaf))
                {
                    var inputNode = leaf as TInputNode;
                    if (inputNode == null) throw new Exception(string.Format("Used an InputNode (node.IsInputNode) that is not allowed with pipeline using input node if type: {0}", typeof(TInputNode).Name));
                    InputNodes.Add(inputNode);
                }
            }
            //InputNodes = .Cast<TInputNode>().ToList();
        }

        /*
        public Pipeline(List<TInputNode> inputNodes, List<TOutputNode> outputNodes)
        {
            if (inputNodes.Distinct().Count() != inputNodes.Count()) throw new Exception("all nodes must be unique (input node duplicate)");

            if (outputNodes.Distinct().Count() != outputNodes.Count()) throw new Exception("all nodes must be unique(output node duplicate)");

            InputNodes = inputNodes;
            OutputNodes = outputNodes;

            if (TraverseBreadthForward().ToList().Count != TraverseBreadthBackward().ToList().Count)
                throw new Exception("Forward and backward traversal do not yield the same Nodes. This is usually because the output node was specified incorrectly.");
        }*/

        protected Pipeline(FloatVector vector, CGPConfiguration configuration)
        {
            var decoder = new CGPDecoder(configuration);  // this will cause problems with fipipeline.... move the decoding function to the configuration
            var activeNodes = decoder.ActiveNodes(vector);
            var columnNodeMap = decoder.ComputeColumnNodeMap(vector, activeNodes);
            var executionTree = decoder.ComputeExecutionTree(vector, activeNodes);

            var operatorNodes = new List<TNode>();

            foreach (var node in activeNodes)
            {
                var predecessors = operatorNodes.Where(x => executionTree[node].Contains(x.NodeID)).ToList();

                Type op;
                if (node >= 0)
                    op = configuration.Decode(vector[configuration.OperatorIndex(node)]);
                else
                    op = configuration.Decode(node);
                /*
                dynamic newNode = Activator.CreateInstance(op, predecessors, parameters);*/
                dynamic newNode = Activator.CreateInstance(op) as Node;

                if (node >= 0) // if not an input node, set parameters from cgp values
                {
                    var index = configuration.ParameterIndex(node);
                    var parameters = new float[configuration.ParameterCount];
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        parameters[i] = vector[index + i];
                    }
                    newNode.FromCGPNodeParameters(parameters);
                }

                foreach (var child in predecessors)
                    newNode.AddChild(child);
                newNode.NodeID = node;
                if (newNode.IsInputNode) newNode.ProgramInputIdentifier = node;
                operatorNodes.Add(newNode);
            }

            foreach (var node in operatorNodes)
            {
                var pred = executionTree[node.NodeID];
                var add = operatorNodes.Where(x => pred.Contains(x.NodeID));
                foreach (var a in add)
                {
                    node.AddChild(a);
                }
            }

            InputNodes = operatorNodes.Where(x => !x.HasChildren()).Cast<TInputNode>().ToList();
            OutputNodes = operatorNodes.Where(x => !x.HasParents()).Cast<TOutputNode>().ToList();

            if (InputNodes.Count == 0 || OutputNodes.Count == 0)
            {
                throw new Exception("Something went horribly wrong. There are no input nodes or no output nodes (ie no nodes without children/parents)");
            }

        }
        public string Name { get; set; } = "DefaultName";


        #region execution functions

        /// <summary>
        /// uses identical input for all input nodes
        /// </summary>
        /// <param name="input">whatever the input is</param>
        /// <returns>dictionary mapping output nodes to their output</returns>
        public virtual Dictionary<Node, TOutput> Execute(TInput input)
        {
            foreach (var inputNode in InputNodes) inputNode.Execute(input);
            var ret = new Dictionary<Node, TOutput>();
            for (int i = 0; i < OutputNodes.Count; i++)
            {
                ret.Add(OutputNodes[i] as TNode, OutputNodes[i].Execute());
            }

            return ret;
        }

        public virtual TOutput ExecuteSingle(TInput input)
        {
            if (OutputNodes.Count > 1) throw new Exception("This pipeline has more than one output node. Do not use ExecuteSingle in that case.");
            foreach (var inputNode in InputNodes) inputNode.Execute(input);
            try
            {
                return OutputNodes.First().Execute();
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception("Invalid Operation Exception: if the sequence is empty, this may be due to the Execute(InputType input) function not properly " +
                    "initializing the output variable. Check if you override the output variable by e.g. declaring it in Execute.", ex);
            }
        }
        #endregion



        protected virtual void DeserializeXml()
        {
            foreach (var node in Nodes)
            {
                foreach (var child in node.Children)
                {
                    child.Parents.Add(node);
                }
            }

            NodeList = new Dictionary<float, TNode>();
            foreach (var node in Nodes)
                NodeList.Add(node.NodeID, node);

            foreach (var edge in AdjacencyList)
            {
                NodeList[edge.Head].AddChild(NodeList[edge.Tail]);
            }

            InputNodes = Nodes.Where(x => x.IsLeaf()).Cast<TInputNode>().ToList();
            OutputNodes = Nodes.Where(x => !x.HasParents()).Cast<TOutputNode>().ToList();
        }


        protected virtual void SerializeXml()
        {
            if (!AllNodesAreUniquelyIdentified()) InitializeNodeIDs();
            AdjacencyList = new List<Edge>();
            foreach (var node in Nodes)
            {
                foreach (var child in node.Children)
                {
                    AdjacencyList.Add(new Edge(node.NodeID, child.NodeID));
                }
            }
        }


        #region traversal functions

        /// <summary>
        /// Traverse pipeline via breadth first search from input to output
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TNode> TraverseBreadthForward()
        {
            var queue = new Queue<Node>();
            var visited = new List<Node>();
            foreach (var inputNode in InputNodes) queue.Enqueue(inputNode);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                visited.Add(current);
                foreach (var parent in current.Parents) if (!visited.Contains(parent) && !queue.Contains(parent)) queue.Enqueue(parent);
                yield return current as TNode;
            }
        }

        /// <summary>
        /// Traverse pipeline via breadth first search from output to input
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TNode> TraverseBreadthBackward()
        {
            var queue = new Queue<Node>();
            var visited = new HashSet<Node>();
            foreach (var outputNode in OutputNodes) queue.Enqueue(outputNode);
            foreach (var outputNode in OutputNodes) visited.Add(outputNode);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                foreach (var child in current.Children)
                    if (!visited.Contains(child))
                    {
                        queue.Enqueue(child);
                        visited.Add(child);
                    }
                yield return current as TNode;
            }
        }

        public IEnumerable<TNode> TraverseDepthForward()
        {
            var stack = new Stack<Node>();
            var visited = new List<Node>();
            foreach (var inputNode in InputNodes.Where(x => x.HasChildren())) stack.Push(inputNode);
            foreach (var inputNode in InputNodes.Where(x => !x.HasChildren())) stack.Push(inputNode);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                visited.Add(current);
                foreach (var parent in current.Parents) if (!visited.Contains(parent) && !stack.Contains(parent)) stack.Push(parent);
                yield return current as TNode;
            }
        }

        /// <summary>
        /// Traverse the pipeline in depth first order, starting from the output nodes
        /// </summary>
        /// <param name="node">if != null, start from node instead of output nodes</param>
        /// <returns></returns>
        public IEnumerable<TNode> TraverseDepthBackward(TNode node = null)
        {
            var stack = new Stack<Node>();
            var visited = new List<Node>();
            if (node == null)
                foreach (var outputNode in OutputNodes) stack.Push(outputNode);
            else
                stack.Push(node);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                visited.Add(current);
                foreach (var child in current.Children) if (!visited.Contains(child) && !stack.Contains(child)) stack.Push(child);
                yield return current as TNode;
            }
        }
        private List<TNode> nodes = null;
        public List<TNode> Nodes
        {
            get
            {
                if (nodes == null || OutputNodes.Count > 0)
                {
                    nodes = TraverseBreadthBackward().ToList();
                }
                return nodes;
            }

            set
            {
                nodes = value;
            }
        }

        protected Dictionary<int, List<TNode>> layers = null;
        /// <summary>
        /// All nodes ordered by maximum distance to leaf nodes. Ignores InputNodes
        /// </summary>
        [XmlIgnore]

        public virtual Dictionary<int, List<TNode>> Layers
        {
            get
            {
                if (layers == null)
                {
                    layers = new Dictionary<int, List<TNode>>();
                    foreach (var node in TraverseBreadthBackward())
                    {
                        if (node.IsInputNode) continue;
                        if (!layers.ContainsKey(node.Height)) layers.Add(node.Height, new List<TNode>());
                        layers[node.Height].Add(node);
                    }
                }

                return layers;
            }
            set
            {
                layers = value;
            }
        }

        private int width = -1;
        public int Width
        {
            get
            {
                if (width == -1)
                {
                    width = Layers.Max(x => x.Value.Count);
                }
                return width;
            }
        }

        private int height = -1;
        public int Height
        {
            get
            {
                if (height == -1)
                {
                    height = OutputNodes.Max(x => x.Height);
                }

                return height;
            }
        }

        public virtual bool SerializeBinarySupported
        {
            get
            {
                return true;  // default is true, as binary does not require custom logic to be implemented by subclass, only the [Serializable] attribute at each node
            }
        }

        public virtual bool SerializeXmlSupported
        {
            get
            {
                return false; // default is false, as xml requires custom logic to be implemented by subclass
            }
        }

        #region IIndividual
        public virtual MultipleFloatVectorEncoding MultipleFloatVectorEncoding
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual FloatVector FloatVector
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual BooleanVector BooleanVector
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        [XmlIgnore]
        public Dictionary<FitnessFunction, double?> Fitness { get; set; }


        #endregion

        #endregion

        public void WriteToDOTFile(string filename)
        {
            try
            {
                Path.GetDirectoryName(filename).CreateDirectory();
            }
            catch (ArgumentException) { } // in case that filename is a relative path in the same directory

            using (var writer = new StreamWriter(filename))
            {
                writer.Write(ToDOTString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeHeader">if true, adds "digraph Pipeline { \n" + "rankdir = \"RL\";" at the top and "}" at bottom </param>
        /// <returns> the DOT string representation of a graph. For very large graphs might be infeasible</returns>
        public virtual string ToDOTString(bool includeHeader = true)
        {
            if (!AllNodesAreUniquelyIdentified()) InitializeNodeIDs();
            string ret = "";
            if (includeHeader)
                ret += "digraph Pipeline { \n" + "rankdir = \"RL\";";

            foreach (var node in TraverseBreadthBackward())
            {
                ret += node.LabelToDot();
            }
            foreach (var node in TraverseBreadthBackward())
            {
                ret += node.EdgesToDOT();
            }
            if (includeHeader)
                ret += "}";
            return ret;
        }

        public bool AllNodesAreUniquelyIdentified()
        {
            return Nodes.Distinct(new Node.IDEqualityComparer<TNode>()).Count() == Nodes.Count;
        }

        public void InitializeNodeIDs()
        {
            int i = 0;
            foreach (var node in TraverseBreadthBackward())
            {
                node.NodeID = i++;
            }
        }


        /// <summary>
        /// tries to remove given node. Warning: this may result in the graph representing the pipeline being split in multiple components, in which case an exception is thrown
        /// </summary>
        /// <param name="node"></param>
        public virtual void Remove(TNode node)
        {
            node.RemoveAll();
            var inputNode = node as TInputNode;
            if (inputNode != null && InputNodes.Contains(inputNode))
                InputNodes.Remove(inputNode);
            var outputNode = node as TOutputNode;
            if (outputNode != null && OutputNodes.Contains(outputNode))
                OutputNodes.Remove(outputNode);

            if (OutputNodes.Count == 0 || InputNodes.Count == 0) throw new Exception("By removing Node " + node.ToString() + " the last input or output node were removed.");

            if (NodeList != null && NodeList.ContainsKey(node.NodeID))
            {
                AdjacencyList.RemoveAll(x => x.Head.Equals(node) || x.Tail.Equals(node));
            }
            //Update();
            //if (TraverseBreadthBackward().Count() != TraverseBreadthForward().Count()) throw new Exception("The graph became disconnected by removing node: " + node.OperatorType);
        }


        /// <summary>
        /// "replace" and "with" are the roots of the trees that are to be replaced/ the replacement respectively
        /// runs through the pipeline in a greedy fashion and replaces the first subtree it finds whose nodes match
        /// the nodes in "replace" as defined by comparer by "with". parent and child structures outside of the subtree
        /// are preserved
        /// </summary>
        /// <param name="replace">tree representing the blueprint for the replacement</param>
        /// <param name="with">the replacement; as it might be used multiple times, it is copied internally</param>
        /// <param name="comparer">defines the equality criterion</param>
        public void Replace(TNode replace, TNode with, EqualityComparer<TNode> comparer)
        {
            var replaceLayers = replace.GetDescendants();
            var withLayers = with.GetDescendants();
            var maxIt = Nodes.Count;

            for (int it = 0; it < maxIt; it++)
            {

                // iterate through nodes
                var iterator = TraverseDepthBackward();
                Dictionary<int, List<TNode>> subtree = null;
                foreach (var node in iterator)
                {
                    subtree = SubtreeEquals(replaceLayers, node, comparer);
                    if (subtree != null)
                    {
                        ReplaceSubtree(subtree, withLayers);
                        break;
                    }
                }
                // if no node was replaced in an entire sweep, return
                if (subtree == null)
                {

                    //Update();

                    return;
                }
            }


        }

        protected virtual void UpdateProperties(Dictionary<int, List<TNode>> subtree, Dictionary<int, List<Node>> withLayers)
        {

        }

        /*
        private void Update()
        {
            Nodes = TraverseBreadthBackward().ToList();
            //InputNodes = nodes.Where(x => !x.HasChildren()).Cast<TInputNode>().ToList();
        }*/

        protected void ReplaceSubtree(Dictionary<int, List<TNode>> treeToBeReplaced, Dictionary<int, List<Node>> replacingTree)
        {
            // copy replacing tree 
            var copy = Copy(replacingTree);

            // handle in/output nodes

            if (OutputNodes.Contains(treeToBeReplaced[0][0]))
            {
                OutputNodes.Add((TOutputNode)replacingTree[0][0]);
                OutputNodes.Remove((TOutputNode)treeToBeReplaced[0][0]);
            }

            // disconnect all intermediate connections
            for (int i = 0, j = 1; j < treeToBeReplaced.Keys.Count; i++, j++)
            {
                foreach (var node in treeToBeReplaced[i])
                    foreach (var child in treeToBeReplaced[j])
                        node.RemoveChild(child);
            }

            // connect all incoming nodes (into layer 0)

            foreach (var root in treeToBeReplaced[0])
            {
                for (int i = root.Parents.Count - 1; i >= 0; i--)
                {
                    var parent = root.Parents[i];
                    if (treeToBeReplaced.Keys.Count == 1 || !root.HasChildren()) // should check if there is a path from root to any leaf
                        parent.RemoveChild(root);
                    foreach (var newChild in copy[0])
                        parent.AddChild(newChild);
                }
            }
            // connect all outgoing nodes (from last layer)

            foreach (var leaf in treeToBeReplaced[treeToBeReplaced.Keys.Last()])
            {
                for (int i = leaf.Children.Count - 1; i >= 0; i--)
                {
                    var child = leaf.Children[i];
                    if (treeToBeReplaced.Keys.Count == 1 || !leaf.HasParents())  // should probably check if there is a path from root to this leaf
                        child.RemoveParent(leaf);
                    foreach (var newParent in copy[copy.Keys.Last()])
                        child.AddParent(newParent);
                }
            }

            UpdateProperties(treeToBeReplaced, copy);

        }
        private static float MaxID = -1;
        protected Dictionary<int, List<Node>> Copy(Dictionary<int, List<Node>> layers)
        {
            if (layers.SelectMany(x => x.Value).Select(x => x).Distinct().Count() != layers.SelectMany(x => x.Value).Select(x => x).Count())
                throw new Exception("Not all nodes are uniquely named");
            var copyLayers = new Dictionary<int, List<Node>>();
            // copy
            foreach (var layer in layers.Keys)
            {
                copyLayers.Add(layer, new List<Node>());
                foreach (var n in layers[layer])
                {
                    copyLayers[layer].Add(n.Copy() as Node);
                }
            }
            // connect
            for (int layer = 0; layer < layers.Keys.Count - 1; layer++)
            {
                foreach (var node in layers[layer])
                {
                    var copyNode = copyLayers[layer].Find(x => x.NodeID == node.NodeID);
                    foreach (var child in node.Children)
                    {
                        var childCopy = copyLayers[layer + 1].Find(x => x.NodeID == child.NodeID);
                        copyNode.AddChild(childCopy);
                    }
                }
            }

            if (MaxID == -1)
                MaxID = Nodes.Max(x => x.NodeID) + 1;

            foreach (var layer in copyLayers)
            {
                foreach (var node in layer.Value) node.NodeID = MaxID++;
            }

            return copyLayers;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="replaceLayers"></param>
        /// <param name="node2"></param>
        /// <param name="comparer"></param>
        /// <returns>null if not equal, the equal subtree if one can be found</returns>
        protected Dictionary<int, List<TNode>> SubtreeEquals(Dictionary<int, List<Node>> replaceLayers, TNode node, EqualityComparer<TNode> comparer)
        {
            if (!comparer.Equals((TNode)replaceLayers[0][0], node)) return null; // if roots don't match
            var nodelayers = node.GetDescendants(replaceLayers.Keys.Count - 1);  // node2layers will be at most the same as replaceLayers
            if (nodelayers.Keys.Count != replaceLayers.Keys.Count) return null; // hence return false if not equal
            var ret = new Dictionary<int, List<TNode>>();

            foreach (var layer in replaceLayers)
            {
                ret.Add(layer.Key, new List<TNode>());
                foreach (var n in layer.Value)
                {
                    var find = nodelayers[layer.Key].FirstOrDefault(x => comparer.Equals((TNode)x, (TNode)n));
                    if (find == null) return null;
                    ret[layer.Key].Add((TNode)find);
                }
            }
            return ret;
        }


        public virtual ICopyable Copy()
        {
            if (!AllNodesAreUniquelyIdentified()) InitializeNodeIDs();
            var copyNodes = new List<TNode>();

            foreach (var node in Nodes)
            {
                copyNodes.Add((TNode)node.Copy());
            }

            foreach (var node in TraverseBreadthBackward())
            {
                var copy = copyNodes.Find(x => x.NodeID == node.NodeID);
                foreach (var child in node.Children)
                {
                    copy.AddChild(copyNodes.Find(x => x.NodeID == child.NodeID));
                }
            }

            //var inputNodes = copyNodes.Where(x => !x.HasChildren()).Cast<TInputNode>();
            var outputNodes = copyNodes.Where(x => !x.HasParents()).Cast<TOutputNode>();

            return new Pipeline<TInput, TOutput, TNode, TInputNode, TOutputNode, TWeight>(outputNodes.ToArray());
        }


        public void SerializeBinary(string filename)
        {
            using (var fs = File.Create(filename))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(fs, this);
            }
        }

        public static Pipeline<TInput, TOutput, TNode, TInputNode, TOutputNode, TWeight> DeserializeBinary(string filename)
        {
            using (var fs = File.OpenRead(filename))
            {
                var bf = new BinaryFormatter();
                var pipe = bf.Deserialize(fs) as Pipeline<TInput, TOutput, TNode, TInputNode, TOutputNode, TWeight>;
                return pipe;
            }
        }

        public virtual void SerializeXml(string filename)
        {
            SerializeXml();
            using (var writer = new StreamWriter(filename))
            {
                var xml = new XmlSerializer(GetType());
                xml.Serialize(writer, this);
            }
        }

        public virtual Dictionary<Node, object> Execute(object input)
        {
            var dict = new Dictionary<Node, object>();
            var tmp = Execute((TInput)input);
            foreach (var node in dict.Keys)
                dict.Add(node, dict[node]);
            return dict;
        }

        public virtual object ExecuteSingle(object input)
        {
            return ExecuteSingle((TInput)input);
        }

        /// <summary>
        /// Concatenates two pipeline objects such that this pipeline is followed by appendPipeline
        ///
        /// both this and the appendPipeline are copied to avoid any weird interference with the original objects
        /// </summary>
        /// <param name="appendPipeline"></param>
        /// <returns></returns>
        public Pipeline<TInput, TOutput, TNode, TInputNode, TOutputNode, TWeight> Concatenate(
            Pipeline<TInput, TOutput, TNode, TInputNode, TOutputNode, TWeight> appendPipeline)
        {
            if (OutputNodes.Count != 1 || appendPipeline.InputNodes.Count != 1)
                throw new NotImplementedException(
                    "Currently only supports pipelines with exactly 1 output and exactly 1 input node.");
            appendPipeline = (Pipeline < TInput, TOutput, TNode, TInputNode, TOutputNode, TWeight >) appendPipeline.Copy();
            var outputNodes = ((Pipeline<TInput, TOutput, TNode, TInputNode, TOutputNode, TWeight>) this.Copy())
                .OutputNodes;
            outputNodes.First().AddParent(appendPipeline.InputNodes.First().Parents.First());
            return new Pipeline<TInput, TOutput, TNode, TInputNode, TOutputNode, TWeight>(appendPipeline.OutputNodes.ToArray());
        }

        public ICopyable Copy(IRandom rand)
        {
            throw new NotImplementedException();
        }

        public class Edge
        {
            public Edge()
            {

            }
            public Edge(float head, float tail)
            {
                Head = head;
                Tail = tail;
            }
            public Edge(float head, float tail, TWeight weight)
            {
                Head = head; Tail = tail; Weight = weight;
            }
            public float Head { get; set; }
            public float Tail { get; set; }

            public TWeight Weight { get; set; }
        }

    }
}
