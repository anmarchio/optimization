using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.CartesianGeneticProgramming;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.Fitness.Interfaces;

namespace Optimization.HPipeline.Fitness
{

    /// <summary>
    /// I'm baffled that this is still in use. could probably be gradually replaced by pipeline objects made up of HalconOperatorNodes
    /// </summary>
    public class PipelineBuilder
    {

        public PipelineBuilder(CGPDecoder decoder, CGPConfiguration configuration, IDecodingMap<HObject[], HTuple[], HObject[]> decodingMap)
        {
            Decoder = decoder;
            Configuration = configuration;
            DecodingMap = decodingMap;
        }

        private CGPDecoder Decoder;
        private CGPConfiguration Configuration;
        private IDecodingMap<HObject[], HTuple[], HObject[]> DecodingMap;

        public Dictionary<float, Func<HObject[], HTuple[], HObject[]>> Decode(FloatVector vector, out Dictionary<float, HTuple[]> parameterArray, out List<float> outputNodes)
        {
            // retrieve active nodes

            var activeNodes = Decoder.ActiveNodes(vector).Where(x => x >= 0).ToList();
            //activeNodes.OrderByDescending(x => x);
            var actionArray = new Dictionary<float, Func<HObject[], HTuple[], HObject[]>>();
            parameterArray = new Dictionary<float, HTuple[]>();
            outputNodes = new List<float>();

            for (uint i = 0; i < Configuration.OutputsCount; i++)
            {
                outputNodes.Add((vector[(uint)vector.Length - 1 - i]));
            }

            //Logger.PrintGrid(vector, parameters, "decoding.txt", true, null, true, true);

            //for (int i = 0; i < activeNodes.Count; i++)
            foreach (var node in activeNodes)
            {
                // fill action array
                //var node = activeNodes[i];
                var nodeIndex = Configuration.NodeIndex(node);
                var func = vector[Configuration.OperatorIndex(node)];
                actionArray.Add(node, DecodingMap[func]);
                // fill parameterArray
                parameterArray.Add(node, new HTuple[Configuration.ParameterCount]);
                int parameterIndex = Configuration.ParameterIndex(node);
                for (int j = 0; j < Configuration.ParameterCount; j++)
                {
                    parameterArray[node][j] = vector[parameterIndex];
                    parameterIndex++;
                }              
            }
            return actionArray;
        }

        /// <summary>
        /// Output actionArray contains the active nodes along with their execution function.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="parameterArray"></param>
        /// <param name="outputNodes"></param>
        /// <returns></returns>
        public Dictionary<float, Func<HObject[], HTuple[], HObject[]>> Decode(MultipleFloatVectorEncoding vector, out Dictionary<float, HTuple[]> parameterArray, out List<float> outputNodes)
        {
            // retrieve active nodes

            var activeNodes = Decoder.ActiveNodes(vector);
            //activeNodes.OrderByDescending(x => x);
            var actionArray = new Dictionary<float, Func<HObject[], HTuple[], HObject[]>>();
            parameterArray = new Dictionary<float, HTuple[]>();
            outputNodes = new List<float>();

            for (int i = 0; i < Configuration.OutputsCount; i++)
            {
                outputNodes.Add((vector[Configuration.ColumnCount - 1, i]));
            }

            //Logger.PrintGrid(vector, parameters, "decoding.txt", true, null, true, true);

            //for (int i = 0; i < activeNodes.Count; i++)
            foreach (var node in activeNodes)
            {
                // fill action array
                //var node = activeNodes[i];
                var column = Configuration.ColumnOf(node);
                var nodeIndex = Configuration.NodeIndex(node);

                var func = vector[column, Configuration.OperatorIndex(node)];
                actionArray.Add(node, DecodingMap[func]);
                
                // fill parameterArray
                parameterArray.Add(node, new HTuple[Configuration.ParameterCount]);
                int parameterIndex = Configuration.ParameterIndex(node);
                for (int j = 0; j < Configuration.ParameterCount; j++)
                {
                    parameterArray[node][j] = vector[column, parameterIndex];
                    parameterIndex++;
                }
            }
            return actionArray;
        }
    }
}
