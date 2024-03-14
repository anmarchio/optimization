using System;
using System.Collections.Generic;
using System.Linq;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Interfaces;

namespace Optimization.CartesianGeneticProgramming
{
    /// <summary>
    /// Crossover operator for CGP that keeps valid column structures in tact. Specify n > 1 parents and at least one column in the CGPConfiguration.CrossoverColumns parameter.
    /// The entries in CrossoverColumns are used to determine which parent's columns are used for crossover. E.g. CrossoverColumns = [1,3] and n = 3, then the first
    /// parents 0th column and 1st column are used, the second parents 2nd and 3rd column and the third parents remaining columns.
    /// </summary>
    [Serializable]

    public class CGPRecombinator : IRecombinator, ICopyable
    {

        private CGPConfiguration configuration;
        private IRandom random;

        public CGPRecombinator(IRandom random, CGPConfiguration configuration)
        {
            this.configuration = configuration;
            this.random = random;
        }

        /// <summary>
        /// Applies crossover to the FloatVector encoded parents.
        /// </summary>
        /// <param name="random">A random number generator</param>
        /// <param name="parents">List of the parents that undergo crossover</param>
        /// <param name="parameters">CGPConfiguration used</param>
        /// <returns></returns>
        public static FloatVector Apply(IRandom random, List<FloatVector> parents, CGPConfiguration parameters)
        {
            if (parents.Count() == 0) throw new Exception("set of parents is empty");

            // debugging        
            //foreach (var p in parents)         
            //   Logger.PrintGrid(p, parameters, "crossover-apply.txt", true, "parent", true, true);

            var result = new FloatVector(parents[0]);

            // copies i-th parent up until column CrossoverColumns[0,i], then cuts off and uses the next parent.
            // I.e. entries in CrossoverColumns specify after which column to use the next parent.
            int j = parameters.ProgramInputCount;
            for (int i = 0; i < parameters.CrossoverColumns.Length; i++)
            {
                // copy from j to lastNode in specified column from parentvector i mod parents.Length
                var firstNodeInNextColumn = parameters.NodeIndex((parameters.CrossoverColumns[i]) * parameters.RowCount);
                for (; j < firstNodeInNextColumn; j++)
                    result[j] = parents[i % parents.Count][j];
            }

            // Logger.PrintGrid(result, parameters, "crossover-apply.txt", true, "child", true, true);

            return result;
        }

        /// <summary>
        /// Applies crossover to parents with MultipleFloatVectorEncoding.
        /// </summary>
        /// <param name="random">A random number generator</param>
        /// <param name="parents">List of the parents that undergo crossover</param>
        /// <param name="parameters">CGPConfiguration used</param>
        /// <returns></returns>
        public static MultipleFloatVectorEncoding Apply(IRandom random, List<MultipleFloatVectorEncoding> parents, CGPConfiguration parameters)
        {
            if (parents.Count() == 0) throw new Exception("set of parents is empty");

            // debugging        
            //foreach (var p in parents)         
            //   Logger.PrintGrid(p, parameters, "crossover-apply.txt", true, "parent", true, true);

            var result = new MultipleFloatVectorEncoding(parents[parameters.CrossoverColumns.Length]);     //copy the last parents genotype then replace the first columns with the ones from the other parents

            //debugging, without personally checking each entry of the vectors for the correct copying procedure, functionality approved by using isEqual function
            //var equal = parents[0].isEqual(parents[1]);
            //var equal2 = parents[0].isEqual(parents[2]);
            //var equal4 = parents[1].isEqual(parents[2]);

            // copy the columns from the other parent(s) specified by CrossoverColumns
            int currentCopyColumnPosition = 0;
            for (int i = 0; i < parameters.CrossoverColumns.Length; i++)
            {
                while(!(currentCopyColumnPosition > parameters.CrossoverColumns[i]))
                {
                    if (currentCopyColumnPosition >= parameters.ColumnCount)
                    {
                        break;
                    }
                    var loopEnd = result.Length(currentCopyColumnPosition);
                    for (int k = 0; k < loopEnd; k++)  //as long as k is less than the number of entries in the current column
                    {
                        result[currentCopyColumnPosition, k] = parents.ElementAt(i)[currentCopyColumnPosition, k];
                    }
                    currentCopyColumnPosition++;
                }                
            }
            //var equal3 = result.isEqual(parents[0]) || result.isEqual(parents[1]) || result.isEqual(parents[2]);
            return result;
        }

        public IIndividual Cross(List<IIndividual> parents)
        {
            if (parents.ElementAt(0).GetType() == typeof(MultipleFloatVectorEncoding))
            {
                var list = parents.Select(x => x.MultipleFloatVectorEncoding).ToList();
                return Apply(random, list, configuration);
            }
            else if (parents.ElementAt(0).GetType() == typeof(FloatVector))
            {
                var list = parents.Select(x => x.FloatVector).ToList();
                return Apply(random, list, configuration);
            }
            else // encoding not yet implemented
            {
                throw new Exception("CGPRecombinator does not support the used Encoding!");
            }
        }

        public ICopyable Copy()
        {
            return new CGPRecombinator(random, configuration);
        }

        public ICopyable Copy(IRandom rand)
        {
            throw new NotImplementedException();
        }
    }
}
