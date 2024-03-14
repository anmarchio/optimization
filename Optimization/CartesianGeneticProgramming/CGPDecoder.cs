using System;
using System.Collections.Generic;
using System.Linq;
using Optimization.CartesianGeneticProgramming.Interfaces;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.CartesianGeneticProgramming
{
    /// <summary>
    /// Used to decode individuals (vectors) into executable pipelines for fitness evaluation.
    /// </summary>
    [Serializable]
    public class CGPDecoder : ICGPDecoder
    {

        public CGPDecoder(CGPConfiguration configuration) 
        {
            Configuration = configuration;
        }

        public CGPConfiguration Configuration { get; private set; }

        /// <summary>
        /// Returns all active nodes, EXCLUDING program inputs (i.e. "nodes" with value lower than zero) as active
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public virtual List<float> ActiveNodes(FloatVector vector, bool excludeProgramInputs=false)
        {
            List<float> activeNodes;
            var queue = new Queue<float>();
            var nodeLength = Configuration.NodeLength;
            // retrieve output nodes: all entries following the last node of the grid are considered output nodes with their only value being the node they get the output from
            var length = vector.Length - Configuration.GridSize * nodeLength;
            float[] outputNodes = new float[length];
            Array.Copy(vector.ToArray(), Configuration.GridSize * nodeLength, outputNodes, 0, length);
            foreach (var o in outputNodes)
            {
                if (excludeProgramInputs && o < 0) continue;
                queue.Enqueue(o);
            }

            activeNodes = new List<float>(queue);

            // for each output node iterate through cartesian grid and determine all active nodes 
            while (queue.Count > 0)
            {
                var nextNode = queue.Dequeue();
                
                int iterateThroughInputsCount = Configuration.InputCountOfOperator(vector[Configuration.OperatorIndex(nextNode)]);               
                for (int i = 0; i < iterateThroughInputsCount; i++)
                {
                    var addNode = vector[Configuration.NodeIndex(nextNode) + i];
                    if (excludeProgramInputs && addNode < 0) continue;
                    if (!activeNodes.Contains(addNode)) //&& addNode >= 0)
                    {
                        activeNodes.Add(addNode);
                        if(addNode >= 0)
                            queue.Enqueue(addNode);
                    }
                }
            }
            return activeNodes;
        }

        /// <summary>
        /// Returns all active nodes, EXCLUDING program inputs (i.e. "nodes" with value lower than zero) as active
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public virtual List<float> ActiveNodes(MultipleFloatVectorEncoding vector)
        {
            List<float> activeNodes;
            var queue = new Queue<float>();
            var nodeLength = Configuration.NodeLength;
            // retrieve output nodes: all entries in the last column of the grid are considered output nodes with their only value being the node they get the output from
            //var length = vector.Length(Configuration.ColumnCount - 1);        //does not work when self-adaptive mutation is used since the sigma value is stored after the outputs
            var length = Configuration.OutputsCount;

            float[] outputnodes = new float[length];
            for (int k = 0; k < length; k++)
            {
                queue.Enqueue(vector[Configuration.ColumnCount - 1, k]);
            }

            activeNodes = new List<float>(queue);

            // for each output node iterate through cartesian grid and determine all active nodes 
            while (queue.Count > 0)
            {
                var nextNode = queue.Dequeue();
                int column = Configuration.ColumnOf(nextNode);

                int iterateThroughInputsCount = Configuration.InputCountOfOperator(vector[column, Configuration.OperatorIndex(nextNode)]);
                for (int i = 0; i < iterateThroughInputsCount; i++)
                {
                    var addNode = vector[column, Configuration.NodeIndex(nextNode) + i];
                    if (!activeNodes.Contains(addNode) && addNode >= 0)
                    {
                        activeNodes.Add(addNode);
                        queue.Enqueue(addNode);
                    }
                }
            }
            return activeNodes;
        }

       
        /// <summary>
        /// EXCLUDING program inputs
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="activeNodes"></param>
        /// <returns>Keys: nodeNames, Values: List of preceeding nodes in the execution order</returns>
        public virtual Dictionary<float, List<float>> ComputeExecutionTree(IIndividual vector, List<float> activeNodes = null, bool excludeProgramInputs=false)
        {
            if (vector.GetType() == typeof(MultipleFloatVectorEncoding)) return ComputeExecutionTree(vector.MultipleFloatVectorEncoding, activeNodes);

            if (activeNodes == null) activeNodes = ActiveNodes(vector.FloatVector);
            var executionTree = new Dictionary<float, List<float>>();

            foreach(var node in activeNodes)
            {
                if (excludeProgramInputs && node < 0) continue;
                if (node < 0)
                {
                    executionTree.Add(node, new List<float>());   
                    continue;
                }
                var nodeIndex = Configuration.NodeIndex(node);
                if (!executionTree.ContainsKey(node)) executionTree.Add(node, new List<float>());
                var inputsCount = Configuration.InputCountOfOperator(vector.FloatVector[Configuration.OperatorIndex(node)]);
                for (int j = 0; j < inputsCount; j++)
                {
                    if (activeNodes.Contains(vector.FloatVector[nodeIndex + j])) //&& vector[nodeIndex + j] >= 0)
                        executionTree[node].Add((vector.FloatVector[nodeIndex + j]));
                }
            }
            return executionTree;
        }

        /// <summary>
        /// EXCLUDING program inputs; ExecutionTree contains the inputs for the active nodes (by nodenumber)
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="activeNodes"></param>
        /// <returns>Keys: nodeNames, Values: List of preceeding nodes in the execution order</returns>
        public virtual Dictionary<float, List<float>> ComputeExecutionTree(MultipleFloatVectorEncoding vector, List<float> activeNodes = null)
        {
            if (activeNodes == null) activeNodes = ActiveNodes(vector);
            var executionTree = new Dictionary<float, List<float>>();

            foreach (var node in activeNodes)
            {
                if (node < 0)
                {
                    continue;
                }
                var nodeIndex = Configuration.NodeIndex(node);
                var column = Configuration.ColumnOf(node);
                if (!executionTree.ContainsKey(node))
                {
                    executionTree.Add(node, new List<float>());
                }
                var inputsCount = Configuration.InputCountOfOperator(vector[column, Configuration.OperatorIndex(node)]);
                for (int j = 0; j < inputsCount; j++)
                {
                    if (activeNodes.Contains(vector[column, nodeIndex + j])) //&& vector[nodeIndex + j] >= 0)
                        executionTree[node].Add((vector[column, nodeIndex + j]));
                }
            }
            return executionTree;
        }


        /// <summary>
        /// key -1 refers to program inputs; ColumnNodeMap contains which nodes (nodenumbers) are active in the active columns
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="activeNodes"></param>
        /// <returns>Key: column index, values: active nodes (names)</returns>
        public Dictionary<int, List<float>> ComputeColumnNodeMap(IIndividual vector, List<float> activeNodes = null, bool excludeProgramInputs=false)
        {
            if (activeNodes == null)
            {
                if(vector.GetType() == typeof(MultipleFloatVectorEncoding))
                {
                    activeNodes = ActiveNodes(vector.MultipleFloatVectorEncoding);
                }
                else if (vector.GetType() == typeof(FloatVector))
                {
                    activeNodes = ActiveNodes(vector.FloatVector, excludeProgramInputs);
                }
            }

            var columnNodeMap = new Dictionary<int, List<float>>();

            foreach (var node in activeNodes)
            {
                if(node < 0)
                {
                    if (!columnNodeMap.ContainsKey(-1)) columnNodeMap.Add(-1, new List<float>());
                    if (!columnNodeMap[-1].Contains(node)) columnNodeMap[-1].Add(node);
                    continue;
                }
                var column = Configuration.ColumnOf(node);
                if (!columnNodeMap.ContainsKey(column)) columnNodeMap.Add(column, new List<float>());
                if (!columnNodeMap[column].Contains(node)) columnNodeMap[column].Add(node);
            }

            columnNodeMap = columnNodeMap.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            return columnNodeMap;
        }


        public List<float> GetOutputNodes(FloatVector vector)
        {
            var list = new List<float>();
            for(int i = vector.Length - Configuration.OutputsCount; i < vector.Length; i++)
            {
                list.Add(vector[i]);
            }
            return list;
        }
    }
}
