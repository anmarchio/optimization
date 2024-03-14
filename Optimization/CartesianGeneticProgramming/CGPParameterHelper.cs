using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PRIME.Optimization.CartesianGeneticProgramming
{ 
    /// <summary>
     /// This class is to provide a projection from all operators in OperatorMap.cs to a subset thereof.
     /// </summary>
    public class CGPParameterHelper
    {
        /*

        public static Dictionary<int, Function[]> GetOperatorSet(CGPConfiguration param)
        {
            return ComputeOperatorSet(OperatorMap.FilterMap, OperatorMap.ThresholdMap, OperatorMap.MorphOperatorMap, param);
        }


        /// <summary>
        /// map column index on array containing all valid operator-int-codes.
        /// </summary>
        private static Dictionary<int, Function[]> ComputeOperatorSet(Dictionary<Function, Func<HObject[], HTuple[], HObject[]>> filters, Dictionary<Function, Func<HObject[], HTuple[], HObject[]>> thresholds, Dictionary<Function, Func<HObject[], HTuple[], HObject[]>> morphOperators, CGPConfiguration param)
        {
            var mat = new Dictionary<int, Function[]>();
            mat.Add(0, filters.Select(x => x.Key).ToArray());
            mat.Add(1, thresholds.Select(x => x.Key).ToArray());

            for (int i = 2; i < param.ColumnsCount; i++)
            {
                mat.Add(i, morphOperators.Select(x => x.Key).ToArray());

            }
                return mat;
        }

        public static Dictionary<Function, Tuple<int, int>[]> GetParameterBounds(CGPConfiguration param)
        {
            return OperatorMap.ParameterBoundsMap;
        }
        

        // map from column index to (min, max) value of input nodes (compute from levels back)
        /// <summary>
        /// Computes the appropriate input bounds given the CGPConfiguration. The key corresponds to the column, the result to the allowed [min, max) value for that column. The last entry specifies 
        /// allowed values for program outputs. The first entry specifies the allowed input nodes for the first column.
        /// </summary>
        /// <returns> A dictionary containing (column, [min,max)-Value) pairs</returns>
        public static Dictionary<int, Tuple<int, int>> GetInputBounds(CGPConfiguration param)
        {
            return ComputeInputBounds(param);
        }

       
        private static Dictionary<int, Tuple<int, int>> ComputeInputBounds(CGPConfiguration param)
        {
            int gridColumnsCount = param.ColumnsCount, programInputsCount = param.ProgramInputsCount, gridRowsCount = param.RowsCount, levelsBack = param.LevelsBack;
            var mat = new Dictionary<int, Tuple<int, int>>();

            mat.Add(0, new Tuple<int, int>(0, param.ProgramInputsCount - 1)); // filters may only take programinputs as inputs

            int thresholdRowEnd = programInputsCount + gridRowsCount - 1;
            // threshold may only take filters as inputs  (min: first filter (programinputscount), max: last filter (programinputscount + first column length, i.e. row count))
            mat.Add(1, new Tuple<int, int>(param.ProgramInputsCount, param.ProgramInputsCount + param.RowsCount - 1));
            
            // compute for other columns depending on levels-back parameter)
            
            for (int i = 2; i <= param.ColumnsCount; i++)
            {
                int upperBound = (programInputsCount + (i * gridRowsCount) - 1);
                int lowerBound = upperBound - (levelsBack * gridRowsCount);

                lowerBound = Math.Max(param.ProgramInputsCount + param.RowsCount, lowerBound);

                mat.Add(i, new Tuple<int, int>(lowerBound, upperBound));
            }
            return mat;
        }*/
    }
}
