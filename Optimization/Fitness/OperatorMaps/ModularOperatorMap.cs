using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Extensions;
using Optimization.CartesianGeneticProgramming;
using Optimization.CartesianGeneticProgramming.Interfaces;
using Optimization.HalconPipeline;
using Optimization.HalconPipeline.Interfaces;

namespace Optimization.Fitness.OperatorMaps
{
    [Serializable]
    public class ModularOperatorMap : IOperatorMap, IOperatorEncoder
    {
        public ModularOperatorMap(List<IParameterInformant> operators, DependencyTree dependencyTree = null)
        {
            if (dependencyTree != null)
                Dependencies = dependencyTree;
            Initialize(operators);
        }

        public ModularOperatorMap(params IParameterInformant[] operators)
        {
            Initialize(operators.ToList());
        }

        /// <summary>
        /// manually set the inputOperators. Possible usecases: building pipelines that do only morphological work
        /// </summary>
        /// <param name="operators">can be null, in which case all operators will be allowed to use program input (e.g. images)</param>
        /// <param name="inputOperators"></param>
        public ModularOperatorMap(List<IParameterInformant> operators, List<IParameterInformant> inputOperators)
        {
            if (operators != null)
                operators = operators.Union(inputOperators).ToList();
            else
                operators = inputOperators;
            //InitializeInputOperators(inputOperators);
            Initialize(operators);
        }

        /*
        private void InitializeInputOperators(List<IParameterInformant> operators)
        {
            InputOperators = new HashSet<float>();
            foreach (var node in OperatorNodeMap)
            {
                if((node.OperatorType & OperatorType.InputNode) == OperatorType.InputNode) 
                InputOperators.Add(OperatorTypeMap[node.GetType()]);
            }
        }*/

        public List<IParameterInformant> GetUniqueOperators(IEnumerable<IParameterInformant> operators)
        {
            if (operators == null || operators.Count() == 0) throw new ArgumentException("operators list null or empty");
            var uniqueOperators = new List<IParameterInformant>();
            foreach (var op in operators)
                if (!uniqueOperators.Any(x => x.GetType() == op.GetType())) uniqueOperators.Add(op);
            return uniqueOperators;
        }

        private void Initialize(List<IParameterInformant> operators)
        {

            //operators = GetUniqueOperators(operators.Where(x => !(x is IInputNode)));
            operators = GetUniqueOperators(operators);

            OperatorTypeMap = new BiDictionary<float, Type>();
            Operators = operators.Cast<Node>().ToList();
            OperatorNodeMap = new BiDictionary<float, IParameterInformant>();
            OperatorInputCount = new Dictionary<float, int>();
            ParameterBounds = new Dictionary<float, List<float>[]>();
            OperatorIdentifiers = new HashSet<float>();
            PrintingMap = new Dictionary<float, string>();

            for (int i = 0; i < operators.Count; i++)
            {
                OperatorTypeMap.Add(i, operators[i].GetType());
                OperatorInputCount.Add(i, operators[i].CGPInputCount);
                ParameterBounds.Add(i, operators[i].CGPParameterBounds);
                OperatorIdentifiers.Add(i);
                PrintingMap.Add(i, operators[i].GetType().Name);
                OperatorNodeMap.Add(i, operators[i]);
            }

            // in case the constructor for manual specification wasn't used
            /*
            if (InputOperators == null)
            {
                throw new Exception("blub");
                //InitializeInputOperators(operators.Where(x => x.IsInputNode).ToList());
            }*/

            /*
            foreach(var op in operators)
                if (!Dependencies.Nodes.Exists(x => op.IsOrOperatorType(x.OperatorType) || x.IsAndOperatorType(op.OperatorType)))
                {
                    throw new Exception(string.Format("{0} is lacking representation {1} in dependencies",
                        op.GetType().Name, op.OperatorType));
                }
                */
        }

        public List<Node> Operators { get; set; }

        public DependencyTree Dependencies { get; set; } = DependencyTree.GetSimpleDependencies();
        /*
            17/09/2025
            Benchmark experimentation with loosened dependencies to test against classic CGP-IP
            //public DependencyTree Dependencies { get; set; } = DependencyTree.GetUnlimitedDependencies();
        */

        protected HashSet<float> InputOperators { get; set; } = null;

        protected BiDictionary<float, IParameterInformant> OperatorNodeMap { get; set; }

        protected virtual BiDictionary<float, Type> OperatorTypeMap { get; set; }


        /// <summary>
        /// Artificially inflates the InputBounds in order to allow each operator the same probability of being used as input.
        /// otherwise operators which are unique in their dependency type fill entire columns and are vastly more likely to be used as input.
        /// </summary>
        protected bool InflateInputBounds { get; set; } = true;

        public Dictionary<float, int> OperatorInputCount
        {
            get; protected set;
        }

        public IOperatorMap OperatorMap
        {
            get
            {
                return this;
            }
        }

        public HashSet<float> OperatorIdentifiers
        {
            get; protected set;
        }

        public Dictionary<float, List<float>[]> ParameterBounds
        {
            get; protected set;
        }
        public Dictionary<float, string> PrintingMap
        {
            get; protected set;
        }

        public virtual bool SerializeBinarySupported
        {
            get
            {
                return true;
            }
        }

        public virtual bool SerializeXmlSupported
        {
            get
            {
                return false;
            }
        }

        public List<float> ProgramOutputBounds
        {
            get; set;
        }

        protected Dictionary<float, Type> InputOperatorTypeMap { get; set; } = new Dictionary<float, Type>();

        public bool IsInitialized
        {
            get; set;
        } = false;

        public Type Decode(float op)
        {
            if (op < 0)
            {
                return InputOperatorTypeMap[op];
            }
            else
                return OperatorTypeMap[op];
        }

        public float Encode(Node op)
        {
            return OperatorTypeMap[op.GetType()];
        }

        /*
        public virtual Dictionary<int, List<float>> InitializeOperatorBounds(CGPConfiguration configuration)
        {
            var bounds = new Dictionary<int, List<float>>();
            for (int i = 0; i < configuration.ColumnCount; i++)
            {
                bounds.Add(i, new List<float>());
            }

            int column = 0;
            HashSet<OperatorType> lastUsedSequence = null;
            var sequenceKeys = OperatorSequence.Keys.ToList();
            foreach(var pair in OperatorSequence)
            {
                bool addedSomething = false;
                foreach (var allowedOperatorType in pair.Value)
                {
                    var operatorTypes = OperatorNodes.Where(x => x.IsOperatorType(allowedOperatorType)).Select(x => x.GetType());
                    if (operatorTypes.Count() == 0) continue;
                    else
                    {
                        addedSomething = true;
                        foreach (var t in operatorTypes)
                            bounds[column].Add(OperatorNodeMap[t]);

                    }

                }
                if (addedSomething)
                {
                    lastUsedSequence = pair.Value;
                    column++;
                }
                    
            }

            for (; column < configuration.ColumnCount; column++)
            {
                foreach (var allowedOperatorType in lastUsedSequence)
                {
                    var operatorTypes = OperatorNodes.Where(x => x.IsOperatorType(allowedOperatorType)).Select(x => x.GetType());
                    if (operatorTypes.Count() == 0) continue;
                    else
                    {
                        foreach (var t in operatorTypes)
                            bounds[column].Add(OperatorNodeMap[t]);
                    }

                }
            }

            return bounds;
        }*/



        /// <summary>
        /// Configure input bounds to depend on the operator instead of the column and operator bounds to group operators with identical output type to be eligible for the same column
        /// additionally order columns according to dependency tree, s.t. there is always non-empty input bounds for each operator after filtering possible eligible nodes
        /// that are located to the right in the grid.
        /// 
        /// use levels back in terms of edges of the dependency tree
        /// 
        /// remove nodes from dependency tree that represent no available operator from the operatorlist
        /// </summary>
        /// <param name="configuration"></param> 
        /// <returns></returns>
        private Dictionary<int, List<float>> _operatorBounds;
        private List<Tuple<int, DependencyNode>> _columnDependencyPairs;
        private List<Tuple<DependencyNode, List<float>>> _dependencyOperatorIdentifierPairs;
        public virtual Dictionary<int, List<float>> InitializeOperatorBounds(CGPConfiguration configuration)
        {
            // standard initialization
            var requiredOpTypes = OperatorNodeMap.Values();
            // remove nodes from dependency tree that are not represented by an operator; throws exception if the graph becomes disconnected
            foreach (var node in Dependencies.Nodes)
            {
                if (!requiredOpTypes.Any(x => x.IsOrOperatorType(node.OperatorType)))
                {
                    Dependencies.Remove(node);
                }
            }

            var forwards = Dependencies.TraverseBreadthForward();
            var backwards = Dependencies.TraverseBreadthBackward();
            var union = forwards.Union(backwards);
            if (!forwards.All(union.Contains) || !backwards.All(union.Contains))
                throw new Exception(string.Format("Graph seems to have been disconnected: forwards pass #nodes: {0}, backwards pass #nodes: {1}"
                    +" this typically happens if there is no operator present for a dependency node that acts as a link, e.g. image2image <- image2region <- region2region"
                    + "but there is no operator of type image2region.", forwards.Count(), backwards.Count()));
 

            if (configuration.ColumnCount < Dependencies.Nodes.Where(x => x.OperatorType != OperatorType.InputNode).Count())
                throw new Exception(string.Format("column count: {0} and number of nodes in the dependencyTree {1} is incompatible. Each" +
                 "OperatorType is assigned its own column in order to reduce the chance if exceptions due to invalid input to operatornodes", configuration.ColumnCount, Dependencies.Nodes.Count));

            _columnDependencyPairs = new List<Tuple<int, DependencyNode>>();
            _dependencyOperatorIdentifierPairs = new List<Tuple<DependencyNode, List<float>>>();
            _operatorBounds = new Dictionary<int, List<float>>();
            for (int i = 0; i < configuration.ColumnCount; i++) _operatorBounds.Add(i, new List<float>());

            int column = 0;
            var layers = Dependencies.Layers;
            layers = layers.OrderBy(x => x.Key).ToDictionary(x => x.Key, y => y.Value);
            
            //for (; layers.ContainsKey(column);)  // this avoids layer -1 which must only contain InputNodes (or input identifiers)
            //{
            //   var layer = layers[column];
            foreach(var layer in layers)
            {
                if (layer.Key < 0) continue;
                foreach (var node in layer.Value)
                {
                    _columnDependencyPairs.Add(new Tuple<int, DependencyNode>(column, node));
                    foreach (var op in OperatorIdentifiers)
                    {
                        // it appears that the and operator types are missing (edgesimage causes issues)
                        if (OperatorNodeMap[op].IsOrOperatorType(node.OperatorType))
                            _operatorBounds[column].Add(op);
                    }
                    _dependencyOperatorIdentifierPairs.Add(new Tuple<DependencyNode, List<float>>(node, _operatorBounds[column]));
                    column++;
                }
            }

            // for remaining columns repeat potential outputnodes
            if (column < configuration.ColumnCount)
            {
                foreach (var node in Dependencies.OutputNodes.Cycle())
                {
                    _columnDependencyPairs.Add(new Tuple<int, DependencyNode>(column, node));
                    foreach (var op in OperatorIdentifiers)
                    {
                        if (OperatorNodeMap[op].IsOrOperatorType(node.OperatorType))
                            _operatorBounds[column].Add(op);
                    }
                    _dependencyOperatorIdentifierPairs.Add(new Tuple<DependencyNode, List<float>>(node, _operatorBounds[column]));
                    column++;
                    if (column >= _operatorBounds.Count) break;
                }
            }

            var supportedOperatorTypes = Dependencies.Nodes.Select(x => x.OperatorType);
            if (!Operators.Where(z => !(z is IInputNode) && supportedOperatorTypes.Any(t => z.IsOrOperatorType(t)))
                .All(x => _operatorBounds.Values.Any(y => y.Contains(Encode(x)))))
                throw new ArgumentException("Not all operators are are mapped to a column. (Some are not used).");
            
            return _operatorBounds;

            /*
            var dependencyIDNodeIDDict = new Dictionary<float, List<float>>();
            DependencyTree.InitializeNodeIDs();
            foreach (var node in DependencyTree.TraverseDepthForward())
            {
                dependencyIDNodeIDDict.Add(node.NodeID, new List<float>());


            }




            /*
             * should rework input bounds to Dictionary<float, List<List<float>>> and write extension for cycling iteration. Then rewrite mutators to support 
             * different input bounds for each parameter... then we can enforce proper inputs for imageandregion2region type operators (and are flexible enough to handle arbitrary inputs)
             * 
             * we also need to keep information about which node in the dependencytree is associated with which nodeIDs in the grid
             * 
             * 
             * it is as of yet unclear how to handle variable input sizes (e.g. fi operator map... would be nice if this one here could handle that also)
             * 
             * map from input type or input identifier to operatortype? unsure how to do this properly...
             * but multi input should definitely be supported
             * */

            /*
           
            foreach(var opType in DependencyTree.TraverseBreadthForward())
            {
                var operators = OperatorNodeMap.Where(x => x.IsOperatorType(opType.OperatorType));
                if(operators.Count() > 0)
                {
                    var operatorIdentifies = operators.Select(x => OperatorTypeMap[x.GetType()]).ToList();

                    // consistency check
                    operatorBounds.Add(column - 1, operatorIdentifies);
                    foreach (var identifier in operatorIdentifies)
                    {
                        var nodeIDs = configuration.FirstAndLastNodeIDOfColumn(column);
                        dependencyIDNodeIDDict[identifier]
                                .AddRange(Enumerable.Range(nodeIDs.Item1, nodeIDs.Item2).Select(x => (float)x).ToList());
                    }

                    /*
                    if (opType.IsLeaf())
                    {
                        for(int i = 0; i < )
                    }
                    //inputBounds.Add(column, )




                    column++;  
                }
            }

            throw new NotImplementedException();
            */
        }


        public Dictionary<int, Dictionary<float, List<float>[]>> ComputeInputBounds(CGPConfiguration configuration)
        {
            // first column manages pointers to input only (<-- depr? does not seem to be relevant for this code)
            // seems to only initialize all the required lists
            // maps from [column][operator] to parameter options (List<float>[])
            var inputBounds = new Dictionary<int, Dictionary<float, List<float>[]>>();
            for (int i = 0; i < configuration.ColumnCount; i++)
            {
                inputBounds[i] = new Dictionary<float, List<float>[]>();
                foreach (var op in _operatorBounds[i])
                {
                    inputBounds[i][op] = new List<float>[OperatorInputCount[op]];
                    for (int j = 0; j < inputBounds[i][op].Length; j++) inputBounds[i][op][j] = new List<float>();
                }
            }

            // compute for all operators the nodeIDs (gridcellIds) that that operator can be placed in
            var operatorNodeIdMap = new Dictionary<float, List<float>>();
            foreach (var op in OperatorIdentifiers)
            {
                operatorNodeIdMap.Add(op, new List<float>());
                var columns = _operatorBounds.Where(x => x.Value.Contains(op)).Select(x => x.Key);
                foreach (var column in columns)
                {
                    var firstLast = configuration.FirstAndLastNodeIDOfColumn(column);
                    operatorNodeIdMap[op] = operatorNodeIdMap[op].Union(Enumerable.Range(firstLast.Item1, firstLast.Item2 - firstLast.Item1).Select(x => (float)x)).ToList();
                }
            }

            // now that we know which column may hold which operators, we can simply compute the union of all eligible input nodes
            foreach (var pair in _columnDependencyPairs)
            {
                var column = pair.Item1;
                var depNode = pair.Item2;

                if (depNode.AndDependencies != null) // and-dependencies have to be handled in such a way that the first input gene can only point to nodes of one type and the second input gene to nodes of another type
                {
                    for (int i = 0; i < depNode.AndDependencies.Length; i++)
                    {
                        var ops = _operatorBounds[column];
                        foreach (var op in ops)
                        {

                            for (int j = 0; j < OperatorInputCount[op]; j++) // depNode.AndDependencies.Length -- this may fail if depenency.length != operator input count... rewrite to make more robust
                            {
                                var dependency = depNode.AndDependencies[j];
                                foreach (var onDependingNode in dependency)
                                {
                                    var eligibleOperators = OperatorNodeMap.Values().Where(x => x.IsOrOperatorType(onDependingNode.OperatorType));
                                    foreach (var eligibleOperator in eligibleOperators)
                                    {
                                        var eligibleOperatorIdentifier = OperatorNodeMap[eligibleOperator];
                                        inputBounds[column][op][j] = inputBounds[column][op][j].Union(operatorNodeIdMap[eligibleOperatorIdentifier]).ToList();
                                    }

                                }
                            }

                        }
                    }
                }
                else // if there are no and-dependencies, then just use the same input bounds for each input gene... refactor: compute once, let all point to same list
                {
                    var ops = _operatorBounds[column];
                    foreach (var op in ops)
                    {
                        for (int i = 0; i < OperatorInputCount[op]; i++)
                        {
                            var dependency = depNode.Children;
                            foreach (var onDependingNode in dependency)
                            {
                                var eligibleOperators = OperatorNodeMap.Values().Where(x => x.IsOrOperatorType(onDependingNode.OperatorType));
                                foreach (var eligibleOperator in eligibleOperators)
                                {
                                    var eligibleOperatorIdentifier = OperatorNodeMap[eligibleOperator];
                                    inputBounds[column][op][i] = inputBounds[column][op][i].Union(operatorNodeIdMap[eligibleOperatorIdentifier]).ToList();
                                }

                            }
                        }
                    }
                }
            }

            // clip for each column, such that no input is to the right of the operator in the grid
            // use levels back in terms of edges in the dependency tree (assuming comatible output?)

            foreach (var col in inputBounds.Keys)
            {
                //if (col == 0) continue;
                foreach (var op in inputBounds[col].Keys)
                {
                    for (int i = 0; i < inputBounds[col][op].Length; i++)
                    {
                        int largest = -1;
                        if(col != 0) largest = configuration.FirstAndLastNodeIDOfColumn(col - 1).Item2; // item2 represents the first node in the next column
                        inputBounds[col][op][i] = (from node in inputBounds[col][op][i]
                                                   where node < largest // proper input node references are set below
                                                   select node).ToList();
                    }
                }
            }

           //Dependencies.WriteToDOTFile("test.txt");

            // the input nodes of the dependency tree are also used to determine the "columns" that point to program inputs
            foreach (var node in Dependencies.InputNodes)
            {
                var inputNode = node as DependencyInputNode;
                if (inputNode == null) throw new Exception("The dependency tree appears to have input nodes that are not proper DependencyInputNodes");

                var ops = _dependencyOperatorIdentifierPairs.Where(x => x.Item1.Children.Contains(node)).Select(x => x.Item2); // get all nodes whose children are at least one input node
                foreach (var opList in ops)
                {
                    foreach (var op in opList)
                        for (int col = 0; col < configuration.ColumnCount; col++)
                        {
                            if (!inputBounds[col].ContainsKey(op)) continue;
                            for (int inputIdx = 0; inputIdx < inputBounds[col][op].Length; inputIdx++)
                                if (inputBounds[col].ContainsKey(op))
                                    inputBounds[col][op][inputIdx].Add(inputNode.ProgramInputIdentifier); // the nodeID corresponds to the programinputidentifier
                            // temporary until restructured to using input placeholders in dependency tree
                            // and merging opmap with configuration.... this travesty has continued long enough
                        }
                }

            }

            ProgramOutputBounds = new List<float>();
            foreach (var node in Dependencies.OutputNodes)
            {
                var ops = _dependencyOperatorIdentifierPairs.Where(x => x.Item1.Equals(node)).Select(x => x.Item2);
                foreach (var op in ops) foreach (var o in op) ProgramOutputBounds = ProgramOutputBounds.Union(operatorNodeIdMap[o]).ToList();
            }

            return inputBounds;
        }

        /*
        public Dictionary<int, List<float>> ComputeInputBounds(CGPConfiguration configuration)
        {
            // input bounds depend on: levels back, the operatorsequence and the input nodes

            var bounds = new Dictionary<int, List<float>>();
            for (int i = 0; i < configuration.ColumnCount + 1; i++)
            {
                bounds.Add(i, new List<float>());
            }

            int column = 0;

            bounds[column] = configuration.ProgramInputIdentifiers;
            column++;


            int lastChangedColumn = 0;
            var sequenceKeys = OperatorSequence.Keys.ToList();
            HashSet<OperatorType> previousSequence = null;
            foreach (var pair in OperatorSequence)
            {
                if (!OperatorNodes.Any(x => pair.Value.Contains(x.OperatorType))) continue;
               
                // check if sequence entry is repeated
                if (previousSequence != null && !pair.Value.EntriesAreEqual(previousSequence))
                { // else: use previous column as allowed inputs
                    lastChangedColumn = column;
                    var range = configuration.FirstAndLastNodeIDOfColumn(column - 1);
                    bounds[column].AddRange(Enumerable.Range(range.Item1, configuration.RowCount).Select(x => (float)x));
                }
                else
                { // then perform standard levelsback test
                    previousSequence = pair.Value;
                    var l = Math.Min(configuration.LevelsBack, column - lastChangedColumn);
                    var rangeStart = configuration.FirstAndLastNodeIDOfColumn(column - l).Item1;
                    var rangeEnd = configuration.FirstAndLastNodeIDOfColumn(column - 1).Item2;
                    bounds[column].AddRange(Enumerable.Range(rangeStart, rangeEnd - rangeStart).Select(x => (float)x));
                }

                foreach (var allowedOperatorType in pair.Value)
                {
                    if ((allowedOperatorType & OperatorType.InputNode) == OperatorType.InputNode)
                    {
                        bounds[column] = bounds[column].Union(configuration.ProgramInputIdentifiers).ToList();
                    }
                }
                column++;
            }
        

            for (; column < configuration.ColumnCount + 1; column++)
            {

                var l = Math.Min(configuration.LevelsBack, column - lastChangedColumn);
                var rangeStart = configuration.FirstAndLastNodeIDOfColumn(column - l).Item1;
                var rangeEnd = configuration.FirstAndLastNodeIDOfColumn(column - 1).Item2;
                bounds[column].AddRange(Enumerable.Range(rangeStart, rangeEnd - rangeStart).Select(x => (float)x));

                foreach (var allowedOperatorType in OperatorSequence.Last().Value)
                {
                    if ((allowedOperatorType & OperatorType.InputNode) == OperatorType.InputNode)
                    {
                        bounds[column] = bounds[column].Union(configuration.ProgramInputIdentifiers).ToList();
                    }
                }
            }


            return bounds;


            
            for (int i = 0; i < operators.Count; i++)
            {
                OperatorTypeMap.Add(i, operators[i].GetType());
                OperatorInputCount.Add(i, operators[i].CGPInputCount);
                ParameterBounds.Add(i, operators[i].CGPParameterBounds);
                OperatorIdentifiers.Add(i);
                PrintingMap.Add(i, operators[i].GetType().Name);
                OperatorNodeMap.Add(i, operators[i]);
            }
        }*/

        public virtual void Initialize(CGPConfiguration configuration)
        {
            IsInitialized = true;
        }

        public virtual void SerializeXml(string filename)
        {
            throw new NotSupportedException();
        }

        public virtual void SerializeBinary(string filename)
        {
            using (var fs = File.Create(filename))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(fs, this);
            }
        }
    }
}
