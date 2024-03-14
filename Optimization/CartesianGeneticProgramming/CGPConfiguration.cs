using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using Optimization.CartesianGeneticProgramming.Interfaces;
using Optimization.EvolutionStrategy;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Pipeline;
using Optimization.Pipeline.Interfaces;

namespace Optimization.CartesianGeneticProgramming
{
    /// <summary>
    /// Specifies all necessary information for decoding, individual creation, mutation as well as offering many helper functions for computing vector related indices.
    /// </summary>
    [Serializable]
    public class CGPConfiguration : IConfiguration, IOperatorEncoder
    {

        public CGPConfiguration()
        {
        }

        /// <summary>
        /// Convenient class for determining index values and allelic constraints.
        /// </summary>
        /// <param name="rowCount">the number of rows</param>
        /// <param name="columnCount">the number of columns</param>
        /// <param name="levelsBack">the effect of this parameter is actually implementation dependent, see OperatorMap</param>
        /// <param name="inputCount">the MAXIMUM number of inputs for each node</param>
        /// <param name="parameterCount">the MAXIMUM number of parameters of each node</param>
        /// <param name="operatorMap">this is usually used during mutation and creation in order to determine valid values for each FloatVector entry</param>
        /// <param name="programInputCount">the number of program inputs</param>
        /// <param name="programOutputCount">the number of program outputs</param>
        public CGPConfiguration(int rowCount, int columnCount, int levelsBack, int inputCount, int parameterCount, IOperatorMap operatorMap, int programInputCount, int programOutputCount)
        {
            if (levelsBack < 1 || programOutputCount < 1) throw new Exception("Levels-Back and program outputs must at least be 1.");
            if (rowCount < 0 || columnCount < 0 || inputCount < 0 || parameterCount < 0) throw new Exception("Parameters must not be negative.");
            if (operatorMap == null) throw new ArgumentNullException("OperatorMap must not be null.");
            
            RowCount = rowCount;
            ColumnCount = columnCount;
            LevelsBack = levelsBack;
            InputCount = inputCount;
            ParameterCount = parameterCount;
            OperatorMap = operatorMap;
            ProgramInputCount = programInputCount;
            OutputsCount = programOutputCount;

            InitializeProgramInputIdentifies();
            OperatorBounds = OperatorMap.InitializeOperatorBounds(this);
            OperatorMap.Initialize(this);
        }

        public bool IsParameterGene(float idx)
        {
            var i = ((int)idx) % NodeLength;
            return (i > InputCount && i < NodeLength - 1);
        }

        public bool IsNumberOfInputsParameterGene(float idx)
        {
            var i = ((int)idx) % NodeLength;
            return i == NodeLength - 1;
        }

        public bool IsOperatorGene(float idx)
        {
            var i = ((int)idx) % NodeLength;
            return i == InputCount;
        }

        public bool IsInputGene(float idx)
        {
            var i = ((int)idx) % NodeLength;
            return i < InputCount;
        }

        private void InitializeProgramInputIdentifies()
        {
            var programInputIdentifies = new List<float>();
            for (float i = -1; i >= -1 * ProgramInputCount; i--) programInputIdentifies.Add(i);
            ProgramInputIdentifiers = programInputIdentifies;
        }

        /// <summary>
        /// Convenient class for determining index values and allelic constraints. This is the crossover constructor. Crossover behavior should probably be moved to EvolutionStrategy.
        /// This is mostly legacy code.
        /// </summary>
        /// <param name="rowCount">the number of rows</param>
        /// <param name="columnCount">the number of columns</param>
        /// <param name="levelsBack">the effect of this parameter is actually implementation dependent, see OperatorMap</param>
        /// <param name="inputCount">the MAXIMUM number of inputs for each node</param>
        /// <param name="parameterCount">the MAXIMUM number of parameters of each node</param>
        /// <param name="crossoverColumns"> determines which columns to use for crossover specific behavior depends on the actual recombinator</param>
        /// <param name="operatorMap">this is usually used during mutation and creation in order to determine valid values for each FloatVector entry</param>
        /// <param name="programInputCount">the number of program inputs</param>
        /// <param name="programOutputCount">the number of program outputs</param>
        public CGPConfiguration(int rowsCount, int columnsCount, int levelsBack, int inputsCount, int parametersCount, int[] crossoverColumns, IOperatorMap operatorMap, int programInputsCount, int programOutputsCount)
        {
            if (levelsBack < 1 || programOutputsCount < 1) throw new Exception("Levels-Back and program outputs must at least be 1.");
            if (rowsCount < 0 || columnsCount < 0 || inputsCount < 0 || parametersCount < 0) throw new Exception("Parameters must not be negative.");
            if (operatorMap == null || crossoverColumns == null) throw new ArgumentNullException("OperatorMap must not be null.");

            RowCount = rowsCount;
            ColumnCount = columnsCount;
            LevelsBack = levelsBack;
            InputCount = inputsCount;
            ParameterCount = parametersCount;
            OperatorMap = operatorMap;
            ProgramInputCount = programInputsCount;
            CrossoverColumns = crossoverColumns;
            OutputsCount = programOutputsCount;

            InitializeProgramInputIdentifies();

            OperatorBounds = OperatorMap.InitializeOperatorBounds(this);
            OperatorMap.Initialize(this);
        }

        #region Parameters

        public int[] CrossoverColumns
        {
            get; set;
        }


        private int length = 0;
        /// <summary>
        /// The length of the encoding vector. Calculated by: NodeLength * ColumnCount * RowCount + OutputsCount; Also works with varying length columns (e.g. MultipleFloatVectorEncoding)
        /// </summary>
        public int Length
        {
            get
            {
                if (usingMultipleFloatVectorEncoding)
                {
                    if (length == 0) length = (ColumnDistributionOfOperatorTypes[0] * ColumnOperatorCount[0] + ColumnDistributionOfOperatorTypes[1] * ColumnOperatorCount[ColumnDistributionOfOperatorTypes[0]] +
                         ColumnDistributionOfOperatorTypes[2] * ColumnOperatorCount[ColumnDistributionOfOperatorTypes[0] + ColumnDistributionOfOperatorTypes[1]]) * NodeLength + OutputsCount;
                    return length;
                }
                else
                {
                    if (length == 0) length = NodeLength * ColumnCount * RowCount + OutputsCount;
                    return length;
                }
            }
        }

        internal void Print(string configDirectory)
        {
            using (var writer = new StreamWriter(configDirectory + "CGPConfiguration.txt"))
            {
                writer.WriteLine(this.ToString());
            }
        }

        public int RowCount { get; set; }

        /// <summary>
        /// Returns the column of the specified node number.  MultipleFloatVector compatible 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int ColumnOf(int node)
        {
            if (usingMultipleFloatVectorEncoding)
            {
                int column, nodenumberincolumn;
                ColumnOfAndNodeNumberInVector(node, usingMultipleFloatVectorEncoding, out column, out nodenumberincolumn);
                return column;
            }
            else
            {
                return (node) / RowCount;
            }
        }

        /// <summary>
        /// Returns the column of the specified node number.  MultipleFloatVector compatible 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public int ColumnOf(float node)
        {
            if (usingMultipleFloatVectorEncoding)
            {
                int column, nodenumberincolumn;
                ColumnOfAndNodeNumberInVector(node, usingMultipleFloatVectorEncoding, out column, out nodenumberincolumn);
                return column;
            }
            else
            {
                return ((int)node) / RowCount;
            }
        }

        /// <summary>
        /// Number of columns of the grid. Gets initialized by the constructor.
        /// </summary>
        public int ColumnCount { get; set; }

        /// <summary>
        /// The size of the grid embedded in a vector. Vector.Length - program inputs - program outputs.
        /// </summary>
        public int GridSize
        {
            get
            {
                if (usingMultipleFloatVectorEncoding)
                {
                    return (ColumnDistributionOfOperatorTypes[0] * ColumnOperatorCount[0] + ColumnDistributionOfOperatorTypes[1] * ColumnOperatorCount[ColumnDistributionOfOperatorTypes[0]] +
                         ColumnDistributionOfOperatorTypes[2] * ColumnOperatorCount[ColumnDistributionOfOperatorTypes[0] + ColumnDistributionOfOperatorTypes[1]]);
                }
                else
                {
                    return ColumnCount * RowCount;
                }
            }
        }

        /// <summary>
        /// Returns the length of one node in the Cartesian grid. Length changes depending on the mutation using the normal distribution with non-categorical values or not.
        /// </summary>
        public int NodeLength
        {
            get
            {
                if (useNormalDistributedMutationStepSizeForNonCategoricalValues == false)
                {
                    return InputCount + ParameterCount + 1;
                }
                else
                {
                    return InputCount + ParameterCount * 2 + 1;
                }
            }
        }

        /// <summary>
        /// Number of outputs of the grid. Gets initialized by the constructor.
        /// </summary>
        public int OutputsCount { get; set; }
        public int ParameterCount { get; set; }
        public int InputCount { get; set; }
        public int ProgramInputCount { get; set; }


        /// <summary>
        /// Index of a node in the vector. MultipleFloatVector compatible (Outdated: Use this, if program inputs are also counted as nodes.)
        /// </summary>
        public int NodeIndex(int node)
        {
            if (usingMultipleFloatVectorEncoding)
            {
                int column, nodenumberincolumn;
                ColumnOfAndNodeNumberInVector(node, usingMultipleFloatVectorEncoding, out column, out nodenumberincolumn);
                return nodenumberincolumn * NodeLength;
            }
            else
            {
                return node * NodeLength;
            }
        }

        /// <summary>
        /// Index of a node in the vector. MultipleFloatVector compatible (Outdated: Use this, if program inputs are also counted as nodes.)
        /// </summary>
        public int NodeIndex(float node)  // warning: exceptions caused here are usually the consequence of ill-configured/inconsistent configurations
        {
            if (usingMultipleFloatVectorEncoding)
            {
                int column, nodenumberincolumn;
                ColumnOfAndNodeNumberInVector(node, usingMultipleFloatVectorEncoding, out column, out nodenumberincolumn);
                return nodenumberincolumn * NodeLength;
            }
            else
            {
                return (int)node * NodeLength;
            }
        }

        /// <summary>
        /// Index of a node's parameter genes in the vector. MultipleFloatVector compatible (Outdated: Use this, if program inputs are also counted as nodes.)
        /// </summary>
        public int ParameterIndex(int node) // warning: exceptions caused here are usually the consequence of ill-configured/inconsistent configurations
        {
            if (usingMultipleFloatVectorEncoding)
            {
                int column, nodenumberincolumn;
                ColumnOfAndNodeNumberInVector(node, usingMultipleFloatVectorEncoding, out column, out nodenumberincolumn);
                return nodenumberincolumn * NodeLength + 1 + InputCount;
            }
            else
            {
                return node * NodeLength + 1 + InputCount;
            }
        }

        /// <summary>
        /// Index of a node's parameter genes in the vector. MultipleFloatVector compatible (Outdated: Use this, if program inputs are also counted as nodes.)
        /// </summary>
        public int ParameterIndex(float node) // warning: exceptions caused here are usually the consequence of ill-configured/inconsistent configurations
        {
            if (usingMultipleFloatVectorEncoding)
            {
                int column, nodenumberincolumn;
                ColumnOfAndNodeNumberInVector(node, usingMultipleFloatVectorEncoding, out column, out nodenumberincolumn);
                return nodenumberincolumn * NodeLength + 1 + InputCount;
            }
            else
            {
                return (int)node * NodeLength + 1 + InputCount;
            }
        }

        /// <summary>
        /// Index of a node's function gene in the vector. MultipleFloatVector compatible (Outdated: Use this, if program inputs are also counted as nodes.)
        /// </summary>
        public int OperatorIndex(int node) // warning: exceptions caused here are usually the consequence of ill-configured/inconsistent configurations
        {
            if (usingMultipleFloatVectorEncoding)
            {
                int column, nodenumberincolumn;
                ColumnOfAndNodeNumberInVector(node, usingMultipleFloatVectorEncoding, out column, out nodenumberincolumn);
                return nodenumberincolumn * NodeLength + InputCount;
            }
            else
            {
                return node * NodeLength + InputCount;
            }
        }

        /// <summary>
        /// Index of a node's function gene in the vector. MultipleFloatVector compatible (Outdated: Use this, if program inputs are also counted as nodes.)
        /// </summary>
        public int OperatorIndex(float node) // warning: exceptions caused here are usually the consequence of ill-configured/inconsistent configurations
        {
            if (usingMultipleFloatVectorEncoding)
            {
                int column, nodenumberincolumn;
                ColumnOfAndNodeNumberInVector(node, usingMultipleFloatVectorEncoding, out column, out nodenumberincolumn);
                return nodenumberincolumn * NodeLength + InputCount;
            }
            else
            {
                return (int)node * NodeLength + InputCount;
            }
        }

        /// <summary>
        /// Calculates the number of nodes used in the carthesian grid.  MultipleFloatVector compatible 
        /// </summary>
        public int NodesCount
        {
            get
            {
                if (usingMultipleFloatVectorEncoding)
                {
                    return ColumnDistributionOfOperatorTypes[0] * ColumnOperatorCount[0] +
                        ColumnDistributionOfOperatorTypes[1] * ColumnOperatorCount[ColumnDistributionOfOperatorTypes[0]] +
                        ColumnDistributionOfOperatorTypes[2] * ColumnOperatorCount[ColumnOperatorCount.Count - 1];
                }
                else
                {
                    return ColumnCount * RowCount;
                }

            }
        }

        /// <summary>
        /// Returns the first (topmost) and last (bottom-most) nodeID of the nodes contained in one column. [first, last), i.e. is nodes 0, 1, 2 are contained in column 0,
        /// this function will return (0, 3)
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public Tuple<int, int> FirstAndLastNodeIDOfColumn(int column)
        {
            if (column < 0) throw new Exception("There are no columns with negative indices. Something most have gone wrong computing the column.");
            return new Tuple<int, int>(RowCount * column, RowCount * (column+1));
        }


        public int LevelsBack
        {
            get; set;
        }

        [XmlIgnore]
        public IOperatorMap OperatorMap { get; set; }

        /// <summary>
        /// Lists for each operator (key) the valid values of the parameters for each parameter of the operator.
        /// </summary>
        [XmlIgnore]
        public Dictionary<float, List<float>[]> ParameterBounds
        {
            get
            {
                return OperatorMap.ParameterBounds;
            }
        }
        [XmlIgnore]
        public List<float> ProgramInputIdentifiers
        {
            get;
            private set;
        }
        [XmlIgnore]
        public HashSet<float> Operators
        {
            get
            {
                return OperatorMap.OperatorIdentifiers;
            }
        }

        [XmlIgnore]
        private Dictionary<int, List<float>> operatorBounds;

        /// <summary>
        /// Valid list of operators for each column.
        /// </summary>
        [XmlIgnore]
        public Dictionary<int, List<float>> OperatorBounds
        {
            get
            {
                return operatorBounds;
            }
            private set
            {
                operatorBounds = value;
            }
        }
        [XmlIgnore]
        private Dictionary<int, Dictionary<float, List<float>[]>> inputBounds;

        /// <summary>
        /// Lists for each column (int key) the set of node names that may be used as input.
        /// </summary>
        [XmlIgnore]
        public Dictionary<int, Dictionary<float, List<float>[]>> InputBounds
        {
            get
            {
                if (inputBounds == null)
                    inputBounds = OperatorMap.ComputeInputBounds(this);
                return inputBounds;
            }
        }

        public List<float> ProgramOutputBounds
        {
            get
            {
                return OperatorMap.ProgramOutputBounds;
            }
        }

        public int InputCountOfOperator(float ofOperator)
        {
            return OperatorMap.OperatorInputCount[ofOperator];
        }

        #endregion

        public override string ToString()
        {
            return "RowCount: " + RowCount + " ColumnCount: " + ColumnCount + " LevelsBack: " + LevelsBack + " InputCount: " + InputCount + " ParameterCount: " + ParameterCount + CossoverColumnsToString() + OperatorMap.ToString() + " ProgramInputCount: " + ProgramInputCount + " ProgramOutputCount" + ProgramInputCount;
        }

        private string CossoverColumnsToString()
        {
            var s = " CrossoverColumns: ";
            if (CrossoverColumns == null) return s + "None ";
            for (int i = 0; i < CrossoverColumns.Length - 1; i++)
            {
                s += CrossoverColumns[i] + ", ";
            }
            s += CrossoverColumns[CrossoverColumns.Length - 1];
            return s;
        }


        public ConfigurationType ConfigurationType
        {
            get
            {
                return ConfigurationType.CGP;
            }
        }

        public SearchSpaceType SearchSpace { get; set; } = SearchSpaceType.Full;

        #region leichtra additions ;)


        /// <summary>
        /// Only use with MultipleFloatVectorEncoding!! Constructor specifying the normal parameters with the addition of columnDistributionOfOperatorTypes which sets how many columns of each operator type 
        /// (image-image, image-region, region-region) exist. This is the constructor for varying length columns (therefore no rowCount).
        /// </summary>
        /// <param name="columnCount">the number of columns</param>
        /// <param name="columnDistributionOfOperatorTypes">integer array containing the number of columns foreach type of operators (e.g. 3[image-image], 1[image-region], 2[region-region])</param>
        /// <param name="levelsBack">the effect of this parameter is actually implementation dependent, see OperatorMap</param>
        /// <param name="inputCount">the MAXIMUM number of inputs for each node</param>
        /// <param name="parameterCount">the MAXIMUM number of parameters of each node</param>
        /// <param name="operatorMap">this is usually used during mutation and creation in order to determine valid values for each FloatVector entry</param>
        /// <param name="programInputCount">the number of program inputs</param>
        /// <param name="programOutputCount">the number of program outputs</param>
        public CGPConfiguration(int columnCount, List<int> columnDistributionOfOperatorTypes, int levelsBack, int inputCount, int parameterCount, IOperatorMap operatorMap, int programInputCount, int programOutputCount, string directory, bool useSelfAdaptiveMutation = false)
        {
            if (levelsBack < 1 || programOutputCount < 1) throw new Exception("Levels-Back and program outputs must at least be 1.");
            if (columnCount < 0 || inputCount < 0 || parameterCount < 0) throw new Exception("Parameters must not be negative.");
            if (operatorMap == null) throw new ArgumentNullException("OperatorMap must not be null.");
            if (columnDistributionOfOperatorTypes.Sum() + 1 != columnCount) throw new Exception("ColumnCount and the sum of columnDistributionOfOperatorTypes+1 must be equal. +1 because of the Outputs being in the last column");
            if (columnDistributionOfOperatorTypes.Count() != 3) throw new Exception("columnDistributionOfOperatorTypes must consist of 3 integer values.");
            //if (operatorMap.GetType() != typeof(ExtendedOperatorMap)) throw new Exception("This constructor only accepts the ExtendedOperatorMap!");

            ColumnCount = columnCount;
            ColumnDistributionOfOperatorTypes = columnDistributionOfOperatorTypes;
            LevelsBack = levelsBack;
            InputCount = inputCount;

            ParameterCount = parameterCount;
            OperatorMap = operatorMap;
            ProgramInputCount = programInputCount;
            OutputsCount = programOutputCount;
            usingMultipleFloatVectorEncoding = true;
            Directory = directory;
            this.useSelfAdaptiveMutation = useSelfAdaptiveMutation;

            InitializeProgramInputIdentifies();
            OperatorBounds = OperatorMap.InitializeOperatorBounds(this);
            OperatorMap.Initialize(this);
        }

        /// <summary>
        /// Only use with MultipleFloatVectorEncoding!! This is the crossover constructor. Crossover behavior should probably be moved to EvolutionStrategy.
        /// </summary>
        /// <param name="rowCount">the number of rows</param>
        /// <param name="columnCount">the number of columns</param>
        /// <param name="levelsBack">the effect of this parameter is actually implementation dependent, see OperatorMap</param>
        /// <param name="inputCount">the MAXIMUM number of inputs for each node</param>
        /// <param name="parameterCount">the MAXIMUM number of parameters of each node</param>
        /// <param name="crossoverColumns"> determines which columns to use for crossover specific behavior depends on the actual recombinator</param>
        /// <param name="operatorMap">this is usually used during mutation and creation in order to determine valid values for each FloatVector entry</param>
        /// <param name="programInputCount">the number of program inputs</param>
        /// <param name="programOutputCount">the number of program outputs</param>
        public CGPConfiguration(int columnsCount, List<int> columnDistributionOfOperatorTypes, int levelsBack, int inputsCount, int parametersCount, int[] crossoverColumns, IOperatorMap operatorMap, int programInputsCount, int programOutputsCount, string directory, bool useSelfAdaptiveMutation = false)
        {
            if (levelsBack < 1 || programOutputsCount < 1) throw new Exception("Levels-Back and program outputs must at least be 1.");
            if (columnsCount < 0 || inputsCount < 0 || parametersCount < 0) throw new Exception("Parameters must not be negative.");
            if (operatorMap == null || crossoverColumns == null) throw new ArgumentNullException("OperatorMap must not be null.");
            if (columnDistributionOfOperatorTypes.Sum() + 1 != columnsCount) throw new Exception("ColumnCount and the sum of columnDistributionOfOperatorTypes+1 must be equal. +1 because of the Outputs being in the last column");
            if (columnDistributionOfOperatorTypes.Count() != 3) throw new Exception("columnDistributionOfOperatorTypes must consist of 3 integer values.");
            //if (operatorMap.GetType() != typeof(ExtendedOperatorMap)) throw new Exception("This constructor only accepts the ExtendedOperatorMap!");

            ColumnCount = columnsCount;
            ColumnDistributionOfOperatorTypes = columnDistributionOfOperatorTypes;
            LevelsBack = levelsBack;
            InputCount = inputsCount;
            ParameterCount = parametersCount;
            OperatorMap = operatorMap;
            ProgramInputCount = programInputsCount;
            CrossoverColumns = crossoverColumns;
            OutputsCount = programOutputsCount;
            usingMultipleFloatVectorEncoding = true;
            Directory = directory;
            this.useSelfAdaptiveMutation = useSelfAdaptiveMutation;

            InitializeProgramInputIdentifies();

            OperatorBounds = OperatorMap.InitializeOperatorBounds(this);
            OperatorMap.Initialize(this);
        }

        /* this constuctor is not needed, because the initialization of the useNormalDistributedMutationStepSizeForNonCategoricalValues attribute is done by the ExtendedOperatorMap
        /// <summary>
        /// Only use with MultipleFloatVectorEncoding!! Constuctor for mutation step sizes relative to the value generated by a normal distribution. Constructor specifying the normal parameters with the addition of columnDistributionOfOperatorTypes which sets how many columns of each operator type 
        /// (image-image, image-region, region-region) exist. This is the constructor for varying length columns (therefore no rowCount).
        /// </summary>
        /// <param name="columnCount">the number of columns</param>
        /// <param name="columnDistributionOfOperatorTypes">integer array containing the number of columns foreach type of operators (e.g. 3[image-image], 1[image-region], 2[region-region])</param>
        /// <param name="levelsBack">the effect of this parameter is actually implementation dependent, see OperatorMap</param>
        /// <param name="inputCount">the MAXIMUM number of inputs for each node</param>
        /// <param name="parameterCount">the MAXIMUM number of parameters of each node</param>
        /// <param name="operatorMap">this is usually used during mutation and creation in order to determine valid values for each FloatVector entry</param>
        /// <param name="programInputCount">the number of program inputs</param>
        /// <param name="programOutputCount">the number of program outputs</param>
        public CGPConfiguration(int columnCount, List<int> columnDistributionOfOperatorTypes, int levelsBack, int inputCount, int parameterCount, IOperatorMap operatorMap, int programInputCount, int programOutputCount, string directory, bool useNormalDistributionMutationStepsForNonCategoricalValues)
        {
            if (levelsBack < 1 || programOutputCount < 1) throw new Exception("Levels-Back and program outputs must at least be 1.");
            if (columnCount < 0 || inputCount < 0 || parameterCount < 0) throw new Exception("Parameters must not be negative.");
            if (operatorMap == null) throw new ArgumentNullException("OperatorMap must not be null.");
            if (columnDistributionOfOperatorTypes.Sum() + 1 != columnCount) throw new Exception("ColumnCount and the sum of columnDistributionOfOperatorTypes+1 must be equal. +1 because of the Outputs being in the last column");
            if (columnDistributionOfOperatorTypes.Count() != 3) throw new Exception("columnDistributionOfOperatorTypes must consist of 3 integer values.");
            if (operatorMap.GetType() != typeof(ExtendedOperatorMap)) throw new Exception("This constructor only accepts the ExtendedOperatorMap!");
            if (useNormalDistributionMutationStepsForNonCategoricalValues == false) throw new Exception("Please don't use this constuctor if you don't need it for the relative mutation step sizes of the non-categorical parameter values!");

            ColumnCount = columnCount;
            ColumnDistributionOfOperatorTypes = columnDistributionOfOperatorTypes;
            LevelsBack = levelsBack;
            InputCount = inputCount;


            ParameterCount = parameterCount;
            OperatorMap = operatorMap;
            ProgramInputCount = programInputCount;
            OutputsCount = programOutputCount;
            usingMultipleFloatVectorEncoding = true;
            Directory = directory;
            useNormalDistributedMutationStepSizeForNonCategoricalValues = useNormalDistributionMutationStepsForNonCategoricalValues;
            InitializeProgramInputIdentifies();
            OperatorBounds = OperatorMap.InitializeOperatorBounds(this);
            OperatorMap.Initialize(this);
        }
    */

        public string Directory;
        private bool usingMultipleFloatVectorEncoding = false;

        /// <summary>
        /// Mutation using the normal distribution. Genotype stores the information for each parameter if it is a categorical one or not. Initialization of this attribute is done by the ExtendedOperatorMap!
        /// </summary>
        public bool useNormalDistributedMutationStepSizeForNonCategoricalValues = false;

        /// <summary>
        /// Required for self-adaptive mutation in order for CGP to know the gene (for the standard deviation) has to be set.
        /// </summary>
        public bool useSelfAdaptiveMutation = false;

        /// <summary>
        /// Number of columns of the different operator types.
        /// First: number of image-image columns;
        /// Second: number of image-region columns;
        /// Third: number of region-region columns;
        /// </summary>
        [XmlIgnore]
        public List<int> ColumnDistributionOfOperatorTypes
        {
            get; set;
        }

        /// <summary>
        /// Calculates the column and the nodenumber of the node in its column. Use the nodenumber to calculate the nodeindex, operatorindex or parameterindex.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="isMultipleVectorEncoded"></param>
        /// <param name="column"></param>
        /// <param name="nodenumberincolumn"></param>
        public void ColumnOfAndNodeNumberInVector(float node, bool isMultipleVectorEncoded, out int column, out int nodenumberincolumn)
        {
            if (isMultipleVectorEncoded)
            {
                column = 0;
                nodenumberincolumn = (int)node;

                for (int i = 0; i < ColumnCount - 1; i++)
                {
                    if (nodenumberincolumn - ColumnOperatorCount[i] >= 0)
                    {
                        column++;
                        nodenumberincolumn -= ColumnOperatorCount[i];
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else //one vector genotype
            {
                column = ColumnOf(node);
                nodenumberincolumn = (int)node;
            }
        }

        public void SerializeXml(string filename)
        {
            throw new NotImplementedException();
        }

        public void SerializeBinary(string filename)
        {
            using (var fs = File.Create(filename))
            {
                var bf = new BinaryFormatter();
                bf.Serialize(fs, this);
            }
        }

        private List<int> columnOperatorCount;

        /// <summary>
        /// List containing the number of operators for each column.
        /// </summary>

        [XmlIgnore]
        public List<int> ColumnOperatorCount
        {
            get
            {
                if (columnOperatorCount == null)
                {
                    columnOperatorCount = new List<int>();
                    for (int i = 0; i < OperatorBounds.Count(); i++)
                    {
                        columnOperatorCount.Add(OperatorBounds[i].Count());
                    }
                }
                return columnOperatorCount;
            }
            set { }
        }


        #endregion

        public bool SerializeBinarySupported
        {
            get
            {
                if (OperatorMap == null) return false;
                return OperatorMap.SerializeBinarySupported;
            }
        }

        public bool SerializeXmlSupported
        {
            get
            {
                if (OperatorMap == null) return false;
                return OperatorMap.SerializeXmlSupported;
            }
        }

        public DependencyTree Dependencies
        {
            get { return OperatorMap.Dependencies; }
        }

        public IIndividual StatusQuo { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            var tmp = obj as CGPConfiguration;
            if (tmp == null) return false;
            if (RowCount != tmp.RowCount) return false;
            if (ColumnCount != tmp.ColumnCount) return false;
            if (!OperatorMap.Equals(tmp.OperatorMap)) return false;
            if (LevelsBack != tmp.LevelsBack) return false;
            if (InputCount != tmp.InputCount) return false;
            if (OutputsCount != tmp.OutputsCount) return false;
            if (ParameterCount != tmp.ParameterCount) return false;
            if (ProgramInputCount != tmp.ProgramInputCount) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return ((RowCount * LevelsBack * InputCount * OutputsCount * ParameterCount) + ColumnCount) % 992313;
        }

        public float Encode(Node op)
        {
            return OperatorMap.Encode(op);
        }

        public Type Decode(float op)
        {
            return OperatorMap.Decode(op);
        }
    }
}
