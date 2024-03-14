using System;
using System.Collections.Generic;
using System.Linq;
using Optimization.CartesianGeneticProgramming;
using Optimization.CartesianGeneticProgramming.Interfaces;
using Optimization.Pipeline;
using Optimization.Pipeline.Interfaces;

namespace Optimization.HPipeline.Fitness.OperatorMaps
{
    public class OperatorMap : IOperatorMap
    {
        public OperatorMap()
        {
            ParameterBounds = InitializeParameterBounds();
        }

        /// <summary>
        /// Specifies the valid operators (list of floats) for each column (int)
        /// </summary>
        public Dictionary<int, List<float>> InitializeOperatorBounds(CGPConfiguration configuration)
        {
            var bounds = new Dictionary<int, List<float>>();
            bounds.Add(0, new List<float>() { 0 });
            bounds.Add(1, new List<float>() { 1 });

            for(int i = 2; i < configuration.ColumnCount; i++)
            {
                bounds.Add(i, new List<float>() { 2, 3, 4, 5, 6 });
            }
            return bounds;
        }

        /// <summary>
        /// Specifies the max number of inputs for each operator (to avoid building unecessarily wide pipelines)
        /// </summary>
        public Dictionary<float, int> OperatorInputCount = new Dictionary<float, int>()
        {
            {0, 1}, {1, 1}, {2, 2}, {3,1 }, {4,1 }, {5,1 }, {6,1 }
        };


        /// <summary>
        /// Set set of all operators
        /// </summary>
        public HashSet<float> Operators = new HashSet<float>()
        {
            0, 1, 2, 3, 4, 5, 6
            /*
             *            
             *            
            // filters
            {0, sobelAmp },

            // thresholds
            {1, thresholdAccessChannel },

            // morphological and set operators
            {2, union2 },
            {3, union1 },
            {4, closing },
            {5, selectShape },
            {6, connection }
             * 
             * */
        };

        /// <summary>
        /// List of valid parameters for each operator
        /// </summary>
        public Dictionary<float, List<float>[]> ParameterBounds
        {
            get; private set;        
        }


        public Dictionary<float, string> PrintingMap = new Dictionary<float, string>()
        {
            {0, "sobelAmp" },

            // thresholds
            {1, "thresholdAccessChannel" },

            // morphological and set operators
            {2, "union2" },
            {3, "union1" },
            {4, "closing" },
            {5, "selectShape" },
            {6, "connection" }
        };

        private Dictionary<float, List<float>[]> InitializeParameterBounds()
        {
            var dictionary = new Dictionary<float, List<float>[]>();

            // sobelAmp

            List<float>[] parameters = new List<float>[2];
            parameters[0] = new List<float>();
            parameters[1] = new List<float>();
            
            for(float i = 0; i < DecodingMap.filterType.Count; i++) parameters[0].Add(i);           //Parameter FilterType
            for(float i = 1; i < 4; i++) parameters[1].Add(i * 2 + 1);                              //Parameter size
            dictionary.Add(0, parameters);

            //thresholdaccesschannel

            parameters = new List<float>[3];
            parameters[0] = new List<float>(); parameters[1] = new List<float>(); parameters[2] = new List<float>();
            for (float i = 1; i < 51; i++) parameters[0].Add(i);        //threshold
            //for (float i = 0; i < 2; i++) parameters[1].Add(i);
            parameters[1].Add(-1); parameters[1].Add(-1);               // sign
            for (float i = 1; i < 4; i++) parameters[2].Add(i);         //channel, adding these possibilities without knowing if the image even has more than one channel is definately risky

            dictionary.Add(1, parameters);

            // union2

            parameters = new List<float>[0];
            dictionary.Add(2, parameters);

            // union1

            dictionary.Add(3, parameters);

            // closing
            parameters = new List<float>[3];
            parameters[0] = new List<float>(); parameters[1] = new List<float>(); parameters[2] = new List<float>();

            for (float i = 0; i < DecodingMap.structElement.Count; i++) parameters[0].Add(i);
            for(float i = 1; i < 21; i++)
            {
                parameters[1].Add(i); parameters[2].Add(i);
            }
            dictionary.Add(4, parameters);

            // selectshape
            parameters = new List<float>[2];
            parameters[0] = new List<float>(); parameters[1] = new List<float>();
            for (float i = 0; i < DecodingMap.features.Count; i++) parameters[0].Add(i);
            for (float i = 20; i < 101; i++) parameters[1].Add(i);

            dictionary.Add(5, parameters);

            // connection
            parameters = new List<float>[1];
            parameters[0] = new List<float>();
            parameters[0].Add(4); parameters[0].Add(8);

            dictionary.Add(6, parameters);

            return dictionary;
        }

        public Dictionary<int, Dictionary<float, List<float>[]>> ComputeInputBounds(CGPConfiguration param)
        {
            int gridColumnsCount = param.ColumnCount, programInputsCount = param.ProgramInputCount, gridRowsCount = param.RowCount, levelsBack = param.LevelsBack;
            var mat = new Dictionary<int, List<float>>();

            mat.Add(0, param.ProgramInputIdentifiers);             // filters may only take programinputs as inputs


            // threshold may only take filters as inputs  (min: first filter (programinputscount), max: last filter (programinputscount + first column length, i.e. row count))
            mat.Add(1, new List<float>());


            var enumer = Enumerable.Range(0, param.RowCount);
            foreach( var e in enumer) { mat[1].Add(e); }

            // compute for other columns depending on levels-back parameter)

            for (int i = 2; i <= param.ColumnCount; i++)
            {
                int upperBound = ((i * gridRowsCount));
                int lowerBound = upperBound - (levelsBack * gridRowsCount);

                lowerBound = Math.Max(param.RowCount, lowerBound);

                mat.Add(i, new List<float>());
                enumer = Enumerable.Range(lowerBound, upperBound - lowerBound);
                foreach (var e in enumer) { mat[i].Add(e); }            
            }
            //return mat;
            return null;
        }

        public void Initialize(CGPConfiguration configuration)
        {          
        }

        public void SerializeXml(string filename)
        {
            throw new NotImplementedException();
        }

        public void SerializeBinary(string filename)
        {
            throw new NotImplementedException();
        }

        public float Encode(Node op)
        {
            throw new NotImplementedException();
        }

        public Type Decode(float op)
        {
            throw new NotImplementedException();
        }

        HashSet<float> IOperatorMap.OperatorIdentifiers
        {
            get { throw new NotImplementedException(); }
        }

        Dictionary<float, int> IOperatorMap.OperatorInputCount
        {
            get { throw new NotImplementedException(); }
        }

        Dictionary<float, string> IOperatorMap.PrintingMap
        {
            get { throw new NotImplementedException(); }
        }

        public bool SerializeBinarySupported
        {
            get
            {
                return false;
            }
        }

        public bool SerializeXmlSupported
        {
            get
            {
                return false;
            }
        }

        IOperatorMap IOperatorEncoder.OperatorMap
        {
            get
            {
                return this;
            }
        }

        public List<float> ProgramOutputBounds
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool IsInitialized
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public DependencyTree Dependencies
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
