using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Extensions
{
    public class MatrixComparer<T> : IEqualityComparer<T[,]>
    {
        public bool Equals(T[,] x, T[,] y)
        {
            //if (x.PrintAsMatrix().Equals(y.PrintAsMatrix())) return true;
            //return false;
            if (x.Length != y.Length) return false;
            if (x.Rank != y.Rank) return false;
            for (int i = 0; i < x.GetLength(0); i++)
                for (int j = 0; j < x.GetLength(1); j++)
                    if (!x[i, j].Equals(y[i, j])) return false;
            return true;
        }

        public int GetHashCode(T[,] obj)
        {
            return obj.GetHashCode();
        }
    }

    public static class SystemMatrixExtensions
    {
        public static T[] Populate<T>(this T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++) arr[i] = value;
            return arr;
        }
        public static IEnumerable<string> EnumerateRows<T>(this T[,] matrix)
        {
            int maxLength = matrix.MaxLength();
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                var s = "";
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    s += new string(' ', maxLength - matrix[i, j].ToString().Length) + (matrix[i, j].ToString() + "\t");
                }
                yield return s;
            }
        }


        /// <summary>
        /// http://www.shodor.org/UNChem/math/stats/
        /// </summary>
        /// <param name="actual"></param>
        /// <param name="expected"></param>
        /// <returns>error in percent for each individual data point</returns>
        public static double[] PercentError(this double[] actual, double[] expected)
        {
            if (actual.Length != expected.Length) throw new Exception("length of arrays must be equal");
            var error = new double[actual.Length];

            for(int i = 0; i < actual.Length; i++)
            {
                error[i] = (actual[i] - expected[i]) / expected[i] * 100;
            }

            return error;
        }

        public static double[,] PercentError(this double[,] actual, double[,] expected)
        {
            if (actual.Length != expected.Length) throw new Exception("length of arrays must be equal");
            var error = new double[actual.GetLength(0), actual.GetLength(1)];

            for (int i = 0; i < actual.GetLength(0); i++)
            {
                for(int j = 0; j < actual.GetLength(1); j++)
                    error[i,j] = (actual[i,j] - expected[i,j]) / expected[i,j] * 100;
            }

            return error;
        }

        public static T[,] ToRowVector<T>(this T[] arr)
        {
            var ret = new T[1, arr.Length];
            for (int i = 0; i < arr.Length; i++) ret[0, i] = arr[i];
            return ret;
        }

        public static T[] Interval<T>(this T[] arr, int from, int toExclusive)
        {
            var ret = new T[toExclusive - from];
            for (int i = from, j = 0; i < toExclusive; i++, j++) ret[j] = arr[i];
            return ret;
        }


        public static IEnumerable<string> EnumerateRows<T>(this T[,] matrix, string[] columnHeads = null, string[] rowInformation = null, char delimiter = '\t')
        {
            int maxLength = matrix.MaxLength();
            int rowLength = 0;
            if (columnHeads != null)
                maxLength = maxLength > columnHeads.Max(x => x.Length) ? maxLength : columnHeads.Max(x => x.Length);
            if (rowInformation != null)
                rowLength = rowInformation.Max(x => x.Length) + 4;


            if (columnHeads != null)
            {
                var s = "";
                if (rowInformation != null) s += new string(' ', rowLength) + delimiter;
                for (int j = 0; j < columnHeads.Length; j++)
                {
                    s += new string(' ', maxLength - columnHeads[j].Length) + (columnHeads[j] + delimiter);
                }

                 yield return s;
            }

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                var s = "";
                if (rowInformation != null)
                    s += new string(' ', rowLength - rowInformation[i].ToString().Length - 4) + (rowInformation[i].ToString() + new string(' ', 4) + delimiter);

                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    s += new string(' ', maxLength - matrix[i, j].ToString().Length) + (matrix[i, j].ToString() + delimiter);
                }
                yield return s;
            }       
        }

        public static IEnumerable<string> EnumerateRows(this double[,] matrix, string[] columnHeads = null, string[] rowInformation = null, char delimiter = '\t')
        {
            int maxLength = matrix.MaxLength();
            int rowLength = 0;
            if (columnHeads != null)
                maxLength = maxLength > columnHeads.Max(x => x.Length) ? maxLength : columnHeads.Max(x => x.Length);
            if (rowInformation != null)
                rowLength = rowInformation.Max(x => x.Length) + 4;


            if (columnHeads != null)
            {
                var s = "";
                if (rowInformation != null) s += new string(' ', rowLength) + delimiter;
                for (int j = 0; j < columnHeads.Length; j++)
                {
                    s += new string(' ', maxLength - columnHeads[j].Length) + (columnHeads[j] + delimiter);
                }

                yield return s;
            }

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                var s = "";
                if (rowInformation != null)
                    s += new string(' ', rowLength - rowInformation[i].ToString().Length - 4) + (rowInformation[i].ToString() + new string(' ', 4) + delimiter);

                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    s += new string(' ', maxLength - matrix[i, j].ToString().Length) + (matrix[i, j].ToInvariantString() + delimiter);
                }
                yield return s;
            }
        }

        public static IEnumerable<T[]> EnumerateColumns<T>(this T[,] matrix)
        {
            for(int i = 0; i < matrix.GetLength(1); i++)
               yield return matrix.GetColumn(i);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="percentage">(0,1) the percentage used for the learning set</param>
        /// <param name="">the learning set</param>
        /// <param name="">the evaluation set</param>
        public static void Split<T>(this T[,] matrix, float percentage, out T[,] learning, out T[,] evaluation)
        {
            if (percentage < 0 || percentage > 1) throw new Exception("invalid percentage");
            int endIdx = (int) (percentage * matrix.GetLength(0));
            if (endIdx <= 0) throw new Exception("learning set would be empty");

            learning = new T[endIdx, matrix.GetLength(1)];

            for(int i = 0; i < endIdx; i++)
                for (int j = 0; j < matrix.GetLength(1); j++)
                    learning[i, j] = matrix[i, j];
            

            evaluation = new T[matrix.GetLength(0) - endIdx, matrix.GetLength(1)];

            for (int i = endIdx, k = 0; i < matrix.GetLength(0); i++, k++)
                for (int j = 0; j < matrix.GetLength(1); j++)
                    evaluation[k, j] = matrix[i, j];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="percentage">the percentage used for the learning set</param>
        /// <param name="">the learning set</param>
        /// <param name="">the evaluation set</param>
        public static void Split<T>(this T[] array, float percentage, out T[] learning, out T[] evaluation)
        {
            if (percentage <= 0 || percentage >= 1) throw new Exception("invalid percentage");
            int endIdx = (int)(percentage * array.Length);
            if (endIdx <= 0) throw new Exception("learning set would be empty");

            learning = new T[endIdx];

            for (int i = 0; i < endIdx; i++)
                learning[i] = array[i];


            evaluation = new T[array.Length - endIdx];

            for (int i = endIdx, j = 0; i < array.Length; i++, j++)
                evaluation[j] = array[i];
        }

        public static string PrintAsMatrix<T>(this T[,] matrix)
        {
            string s = "";
            int maxLength = matrix.MaxLength();
            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    s += new string(' ', maxLength - matrix[i, j].ToString().Length) + (matrix[i, j].ToString() + "\t");
                }
                s += '\n';
            }
            return s;
        }


        public static string ToString<T>(this T[] array, char delimiter = ',', bool equalLength = false)
        {
            var maxLength = array.Max(x => x.ToString().Length);
            var ret = "";

            for(int i = 0; i < array.Length - 1; i++)
            {
                if (equalLength)
                    ret += new string(' ', maxLength - array[i].ToString().Length) + (array[i].ToString() + delimiter);
                else
                    ret += array[i].ToString() + delimiter;
            }

            if (equalLength)
                ret += new string(' ', maxLength - array.Last().ToString().Length) + (array.Last().ToString());
            else
                ret += array.Last().ToString();

            return ret;
        }

        public static string ToInvariantString(this double[] array, char delimiter = ',', bool equalLength = false)
        {
            var maxLength = array.Max(x => x.ToInvariantString().Length);
            var ret = "";

            for (int i = 0; i < array.Length - 1; i++)
            {
                if (equalLength)
                    ret += new string(' ', maxLength - array[i].ToInvariantString().Length) + (array[i].ToInvariantString() + delimiter);
                else
                    ret += array[i].ToInvariantString() + delimiter;
            }

            if (equalLength)
                ret += new string(' ', maxLength - array.Last().ToInvariantString().Length) + (array.Last().ToInvariantString());
            else
                ret += array.Last().ToInvariantString();

            return ret;
        }

        public static string ToInvariantString(this float[] array, char delimiter = ',', bool equalLength = false)
        {
            var maxLength = array.Max(x => x.ToInvariantString().Length);
            var ret = "";

            for (int i = 0; i < array.Length - 1; i++)
            {
                if (equalLength)
                    ret += new string(' ', maxLength - array[i].ToInvariantString().Length) + (array[i].ToInvariantString() + delimiter);
                else
                    ret += array[i].ToInvariantString() + delimiter;
            }

            if (equalLength)
                ret += new string(' ', maxLength - array.Last().ToInvariantString().Length) + (array.Last().ToInvariantString());
            else
                ret += array.Last().ToInvariantString();

            return ret;
        }

        public static double EuclidianDistance(this double[] vector1, double[] vector2)
        {
            double squaresum = 0;
            for (int i = 0; i < vector1.Length; i++) squaresum += Math.Pow(vector1[i] - vector2[i], 2);
            return Math.Sqrt(squaresum);
        }

        public static double SquareEuclidianDistance(this double[] vector1, double[] vector2)
        {
            double squaresum = 0;
            for (int i = 0; i < vector1.Length; i++) squaresum += Math.Pow(vector1[i] - vector2[i], 2);
            return squaresum;
        }

        public static T[,] ToColumnVector<T>(this T[] array)
        {
            var mat = new T[array.Length, 1];
            for (int i = 0; i < array.Length; i++) mat[i, 0] = array[i];
            return mat;
        }

        public static string PrintAsMatrix<T>(this T[,] matrix, string[] columnHeads = null, string[] rowInformation = null)
        {
            string s = "";
            int maxLength = matrix.MaxLength();
            int rowLength = 0;
            if (columnHeads != null)
                maxLength = maxLength > columnHeads.Max(x => x.Length) ? maxLength : columnHeads.Max(x => x.Length);
            if (rowInformation != null)
                rowLength = rowInformation.Max(x => x.Length) + 4;


            if (columnHeads != null)
            {
                if (rowInformation != null) s += new string(' ', rowLength) + "\t";
                for (int j = 0; j < columnHeads.Length; j++)
                {
                    s += new string(' ', maxLength - columnHeads[j].Length) + (columnHeads[j] + "\t");
                }

                s += "\n";
            }

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                if (rowInformation != null)
                    s += new string(' ', rowLength - rowInformation[i].ToString().Length - 4) + (rowInformation[i].ToString() + new string(' ', 4) + "\t");

                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    s += new string(' ', maxLength - matrix[i, j].ToString().Length) + (matrix[i, j].ToString() + "\t");
                }
                s += "\n";
            }

            return s;
        }

        public static int MaxLength<T>(this T[,] matrix)
        {
            int maxLength = 0;

            foreach (var value in matrix)
            {
                if (value == null) continue;
                if (value.ToString().Length > maxLength) maxLength = value.ToString().Length;
            }
            return maxLength;
        }

        public static T[,] DropColumns<T>(this T[,] input, int[] columns)
        {
            T[,] result = new T[input.GetLength(0), input.GetLength(1) - columns.Length];

            for (int i = 0, j = 0; i < input.GetLength(0); i++)
            {

                for (int k = 0, u = 0; k < input.GetLength(1); k++)
                {
                    if (columns.Contains(k)) continue;
                    result[j, u] = input[i, k];
                    u++;
                }
                j++;
            }
            return result;
        }

        public static T[,] AddColumn<T>(this T[,] input, T[] newColumn)
        {
            if (newColumn.Length != input.GetLength(0)) throw new Exception();
            var ret = new T[input.GetLength(0), input.GetLength(1) + 1];
            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    ret[i, j] = input[i, j];
                }
                ret[i, input.GetLength(1)] = newColumn[i];
            }
            return ret;
        }

        public static T[,] AddColumn<T>(this T[] input, T[] newColumn)
        {
            if (newColumn.Length != input.GetLength(0)) throw new Exception();
            var ret = new T[input.Length, 2];
            for (int i = 0; i < input.Length; i++)
            {
                ret[i, 0] = input[i];
                ret[i, 1] = newColumn[i];
            }
            return ret;
        }

        public static T[,] AddRow<T>(this T[,] input, T[] newRow)
        {
            if (newRow.Length != input.GetLength(1)) throw new Exception();
            var ret = new T[input.GetLength(0) + 1, input.GetLength(1)];
            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    ret[i, j] = input[i, j];
                }
            }
            for (int i = 0; i < newRow.Length; i++) ret[ret.GetLength(0) - 1, i] = newRow[i];
            return ret;
        }

        public static T[,] InsertRow<T>(this T[,] input, T[] newRow, int index)
        {
            if (newRow.Length != input.GetLength(1)) throw new Exception();
            if (index >= input.GetLength(0)) throw new Exception();

            var ret = new T[input.GetLength(0) + 1, input.GetLength(1)];
            for (int i = 0; i < index; i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    ret[i, j] = input[i, j];
                }
            }

            for (int j = 0; j < input.GetLength(1); j++) ret[index, j] = newRow[j];

            for (int i = index + 1; i < input.GetLength(0) + 1; i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    ret[i, j] = input[i, j];
                }
            }
            return ret;
        }

        public static T[,] ConcatRows<T>(this T[,] input, T[,] newRows)
        {
            if (newRows.GetLength(1) != input.GetLength(1)) throw new Exception();
            var ret = new T[input.GetLength(0) + newRows.GetLength(0), input.GetLength(1)];
            for (int i = 0; i < input.GetLength(0); i++)
            {
                for (int j = 0; j < input.GetLength(1); j++)
                {
                    ret[i, j] = input[i, j];
                }
            }
            for (int i = input.GetLength(0) - 1, j = 0; i < ret.GetLength(0); i++, j++)
            {
                for (int k = 0; k < input.GetLength(1); k++)
                {
                    ret[i, k] = newRows[j, k];
                }
            }
            return ret;
        }

        public static T[] DropColumns<T>(this T[] input, int[] columns)
        {
            var arr = new T[input.Length - columns.Length];

            for (int i = 0, j = 0; i < input.Length; i++)
            {
                if (columns.Contains(i)) continue;
                arr[j] = input[i];
                j++;
            }
            return arr;
        }

        public static T[] GetRow<T>(this T[,] matrix, int rowIndex)
        {
            var ret = new T[matrix.GetLength(1)];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = matrix[rowIndex, i];
            }

            return ret;
        }


        public static T[] Append<T>(this T[] array1, T[] array2)
        {
            var ret = new T[array1.Length + array2.Length];
            for(int i = 0; i < array1.Length; i++)
            {
                ret[i] = array1[i];
            }
            for(int i = 0, j = array1.Length; i < array2.Length; i++, j++)
            {
                ret[j] = array2[i];
            }
            return ret;
        }

        public static object[] ToObject<T>(this T[] array) 
        {
            var o = new object[array.Length];
            for (int i = 0; i < o.Length; i++) o[i] = array[i];
            return o;
        }

        public static T[] GetColumn<T>(this T[,] matrix, int columnIndex)
        {
            if (columnIndex >= matrix.GetLength(1)) throw new IndexOutOfRangeException("colidx: " + columnIndex + " matrix columns:" + matrix.GetLength(1));
            var ret = new T[matrix.GetLength(0)];
            for (int i = 0; i < ret.Length; i++)
            {
                ret[i] = matrix[i, columnIndex];
            }

            return ret;
        }

        public static T[,] ToMatrix<T>(this T[][] array)
        {
            var mat = new T[array.Length, array[0].Length];
            for (int i = 0; i < mat.GetLength(0); i++)
                for (int j = 0; j < mat.GetLength(1); j++)
                    mat[i, j] = array[i][j];
            return mat;
        }

        public static T[][] ToArray<T>(this T[,] mat)
        {
            var array = new T[mat.GetLength(0)][];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new T[mat.GetLength(1)];
                for (int j = 0; j < array[i].Length; j++)
                {
                    array[i][j] = mat[i, j];
                }
            }

            return array;
        }


        public static double[,] ApplyFunction(this double[,] matrix, Func<double, double> func)
        {
            var mat = new double[matrix.GetLength(0), matrix.GetLength(1)];

            for (int i = 0; i < matrix.GetLength(0); i++)
            {
                for (int j = 0; j < matrix.GetLength(1); j++)
                {
                    mat[i, j] = func.Invoke(matrix[i, j]);
                }
            }

            return mat;
        }

        /// <summary>
        /// Reads lines until the matrix is full; i.e. number of lines in file must be greater/equal number of rows in matrix
        /// </summary>
        /// <param name="data">matrix must be initialized</param>
        /// <param name="filename">absolute or relative path to data; must be in PrintAsMatrix() format</param>
        public static void Fill(this double[,] data, string filename)
        {
            using (var reader = new StreamReader(filename))
            {
                for (int i = 0; i < data.GetLength(0); i++)
                {
                    var splitLine = reader.ReadLine().Split('\t');
                    for (int j = 0; j < data.GetLength(1); j++)
                    {
                        data[i, j] = double.Parse(splitLine[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Reads lines until the matrix is full; i.e. number of lines in file must be greater/equal number of rows in matrix
        /// </summary>
        /// <param name="data">matrix must be initialized</param>
        /// <param name="filename">absolute or relative path to data; must be in PrintAsMatrix() format</param>
        public static void Fill(this float[,] data, string filename)
        {
            using (var reader = new StreamReader(filename))
            {

                for (int i = 0; i < data.GetLength(0); i++)
                {
                    var splitLine = reader.ReadLine().Split('\t');
                    for (int j = 0; j < data.GetLength(1); j++)
                    {
                        data[i, j] = float.Parse(splitLine[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Reads lines until the matrix is full; i.e. number of lines in file must be greater/equal number of rows in matrix ignoring columnHeads and rowInformation
        /// </summary>
        /// <param name="data">matrix must be initialized</param>
        /// <param name="filename">absolute or relative path to data; must be in PrintAsMatrix() format</param>
        public static void Fill(this double[,] data, string filename, out string[] columnHeads, out string[] rowInformation)
        {
            var lines = File.ReadAllLines(filename);

            columnHeads = new string[data.GetLength(1)];
            rowInformation = new string[data.GetLength(0)];

            var split = lines[0].Split('\t');
            for (int i = 1; i < split.Length - 1; i++)
            {
                columnHeads[i - 1] = split[i];
            }

            for (int i = 1; i < data.GetLength(0) + 1; i++)
            {
                split = lines[i].Split('\t');

                rowInformation[i - 1] = split[0];
                for (int j = 1; j < data.GetLength(1) + 1; j++)
                {
                    data[i - 1, j - 1] = double.Parse(split[j]);
                }
            }

        }

        public static T[,] ConcatenateColumns<T>(this T[,] arr1, T[,] arr2)
        {
            var arr = new T[arr1.GetLength(0), arr1.GetLength(1) + arr2.GetLength(1)];

            for(int i = 0; i < arr.GetLength(0); i++ )
            {
                for(int j = 0; j < arr1.GetLength(1); j++)
                {
                    arr[i, j] = arr1[i, j];
                }
                for(int j = arr1.GetLength(1), k = 0; j < arr.GetLength(1) ; j++, k++)
                {
                    arr[i, j] = arr2[i, k];
                }
            }

            return arr;
        }


        /// <summary>
        /// Reads lines until the matrix is full; i.e. number of lines in file must be greater/equal number of rows in matrix ignoring columnHeads and rowInformation
        /// </summary>
        /// <param name="data">matrix must be initialized</param>
        /// <param name="filename">absolute or relative path to data; must be in PrintAsMatrix() format</param>
        public static void Fill(this float[,] data, string filename, out string[] columnHeads, out string[] rowInformation)
        {
            var lines = File.ReadAllLines(filename);

            columnHeads = new string[data.GetLength(1)];
            rowInformation = new string[data.GetLength(0)];

            var split = lines[0].Split('\t');
            for (int i = 1; i < split.Length - 1; i++)
            {
                columnHeads[i - 1] = split[i];
            }

            for (int i = 1; i < data.GetLength(0) + 1; i++)
            {
                split = lines[i].Split('\t');

                rowInformation[i - 1] = split[0];
                for (int j = 1; j < data.GetLength(1) + 1; j++)
                {
                    data[i - 1, j - 1] = float.Parse(split[j]);
                }
            }

        }
    }
}
