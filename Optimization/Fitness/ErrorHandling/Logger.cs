using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using Extensions;
using Optimization.CartesianGeneticProgramming;
using Optimization.EvolutionStrategy.Encodings;

namespace Optimization.Fitness.ErrorHandling
{

    /// <summary>
    /// Semi handy Logging class. do not mindlessly catch exceptions. rethrow them for debugging and use ConsumeExceptions in order to facilitate long evolution runs over weekends
    /// </summary>
    public class Logger
    {
        public static object mutex = new object();


        private static BlockingCollection<Exception> ExceptionQueue = new BlockingCollection<Exception>();
        private static BackgroundWorker Worker;

        public static uint ExceptionsCount { get; private set; } = 0;

        public static void LogException(Exception e, params string[] additionalInformation)
        {
            if (Worker == null)
            {
                Worker = new BackgroundWorker();
                Worker.DoWork += write_exceptions;
                Worker.WorkerSupportsCancellation = true;
                Worker.RunWorkerAsync();
            }
            ExceptionQueue.Add(e);
            ExceptionsCount++;
        }

        private static void write_exceptions(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            using (var writer = new StreamWriter(Path.Combine(BasePath, DefaultFilename), append: true))
            {

                while (!worker.CancellationPending)
                {
                    Exception ex;
                    if (ExceptionQueue.TryTake(out ex, 100))
                    {
                        writer.WriteLine(DateTime.Now);
                        writer.WriteLine(ex.Message);
                        writer.WriteLine(ex.StackTrace);
                        writer.WriteLine();
                    }
                }
            }
        }


        /// <summary>
        /// If false, Exceptions thrown in places where the Logger is used to Log them (usually all CGP relevant classes), are rethrown for easier debugging or error handling on higher levels
        /// Set this to true if you are confident that you code is doing what it ought to and want to ensure that the program does not crash during long computations with rare but unexpected errors.
        /// </summary>
        public static bool ConsumeExceptions { get; set; } = false;

        public static string DefaultFilename { get; set; } = "ExceptionsLog.txt";
        public static string BasePath { get; set; } = Directory.GetCurrentDirectory();

        public static string ExceptionGrids
        {
            get
            {
                return Path.Combine(BasePath, "ExceptionGrids.txt");
            }
        }


        public static void PrintIterationVector(FloatVector vector, CGPConfiguration parameters, string directory, int iteration)
        {
            // print vector for later use
            using (var writer = new StreamWriter(Path.Combine(directory, "vector " + iteration + ".txt")))
            {
                for (int i = 0; i < vector.Length - 1; i++)
                {
                    writer.Write(vector[i].ToInvariantString() + ",");
                }
                writer.Write(vector[vector.Length - 1].ToInvariantString());
                writer.WriteLine();
            }

            // maybe write parameters for convenience;       
        }

        public static void PrintVector(FloatVector vector, CGPConfiguration parameters, string directory)
        {
            // print vector for later use
            using (var writer = new StreamWriter(Path.Combine(directory, "vector.txt")))
            {
                for (int i = 0; i < vector.Length - 1; i++)
                {
                    writer.Write(vector[i].ToInvariantString() + ",");
                }
                writer.Write(vector[vector.Length - 1].ToInvariantString());
                writer.WriteLine();
            }

            // maybe write parameters for convenience;       
        }

        public static void PrintVector(MultipleFloatVectorEncoding vector, CGPConfiguration parameters, string directory)
        {
            // print vector for later use
            using (var writer = new StreamWriter(Path.Combine(directory, "vector.txt")))
            {
                int i;
                for (i = 0; i < parameters.ColumnCount - 1; i++)
                {
                    for (int j = 0; j < parameters.ColumnOperatorCount[i] * parameters.NodeLength; j++)
                    {
                        writer.Write(vector[i, j] + ",");
                    }
                }
                //print outputs
                for (int l = 0; l < parameters.OutputsCount; l++)
                {
                    writer.Write(vector[i, l]);
                    writer.WriteLine();
                }
            }
        }



        public static void PrintGrid(FloatVector vector, CGPConfiguration parameters, string filename, bool append = true, string tag = null, bool printActiveNodes = false, bool printNodeIndices = false)
        {
            PrintGrid(new Exception("ignore this"), vector, parameters, filename, append, tag, printActiveNodes, printNodeIndices);
        }

        public static void PrintPiGrid(Exception e, FloatVector vector, CGPConfiguration parameters, string directory, bool append = true, string tag = null, bool printActiveNodes = false, bool printNodeIndices = false)
        {
            List<float> activeNodes = null;
            var decoder = new CGPDecoder(parameters);
            if (printActiveNodes)
                activeNodes = decoder.ActiveNodes(vector);
            string[,] grid = new string[parameters.RowCount, parameters.ColumnCount];

            var printingMap = parameters.OperatorMap.PrintingMap;


            // print program grid (containing nodes)
            for (var node = 0; node < parameters.NodesCount; node++)
            {
                var column = node / parameters.RowCount;
                var row = node % parameters.RowCount;
                var nodeIndex = parameters.NodeIndex(node);
                string nodeString = "";
                if (activeNodes != null && activeNodes.Contains(node))
                {
                    nodeString += "{{";
                }
                nodeString += node.ToString() + ": ";

                if (printNodeIndices)
                    nodeString += parameters.NodeIndex(node) + ": ";

                nodeString += "IN: ";

                var function = vector[parameters.OperatorIndex(node)];
                if (function == (float)0.057)
                {
                    Console.WriteLine(function);
                }
                for (var i = 0; i < parameters.InputCountOfOperator(function); i++)
                {
                    nodeString += vector[nodeIndex + i].ToString(CultureInfo.InvariantCulture) + " ";
                }
                // decode node function
                var op = vector[nodeIndex + parameters.InputCount];

                nodeString += printingMap[op];
                nodeString += "(";
                for (var j = 0; j < parameters.ParameterCount - 1; j++)
                {
                    nodeString += vector[parameters.ParameterIndex(node) + j].ToString(CultureInfo.InvariantCulture) + ",";
                }
                nodeString += vector[parameters.ParameterIndex(node) + parameters.ParameterCount - 1].ToString(CultureInfo.InvariantCulture);
                nodeString += ")";

                if (activeNodes != null && activeNodes.Contains(node))
                {
                    nodeString += "}}";
                }
                grid[row, column] = nodeString;
                // write nodestring Inputs + Function + Parameters
            }

            lock (mutex)
            {

                using (var writer = new StreamWriter(Path.Combine(directory, "grid.txt"), append))
                {
                    var max = 0;
                    foreach (var v in grid)
                    {
                        if (v.Length > max) max = v.Length;
                    }

                    if (tag != null) writer.WriteLine(tag);
                    writer.WriteLine("HashCode: " + vector.GetHashCode());
                    writer.WriteLine("Time: " + DateTime.Now);
                    // print exception details

                    writer.WriteLine(e.GetType().ToString() + ", " + e.Message + ", " + e.StackTrace);

                    if (e is UnexpectedException)
                    {
                        var unexpected = (UnexpectedException)e;
                        writer.WriteLine("current:" + unexpected.Current.ToString() + ": ");
                        foreach (var key in unexpected.ColumnNodeMap.Keys)
                        {
                            writer.WriteLine("Column: " + key);
                            foreach (var node in unexpected.ColumnNodeMap[key])
                            {
                                var exe = "in: ";
                                foreach (var input in unexpected.ExecutionTree[node])
                                {
                                    exe += input + " ";
                                }
                            }
                        }
                    }

                    // print program inputs
                    writer.Write("Inputs: ");
                    for (int i = 0; i < parameters.ProgramInputCount; i++)
                    {
                        writer.Write(parameters.ProgramInputIdentifiers[i] + " ");
                    }
                    writer.WriteLine();

                    // print nodes
                    for (int i = 0; i < parameters.RowCount; i++)
                    {
                        string s = "";
                        for (int j = 0; j < parameters.ColumnCount; j++)
                        {
                            s += grid[i, j] + new string(' ', (int)max - grid[i, j].Length) + " | ";
                        }
                        writer.WriteLine(s);


                    }

                    // print program outputs
                    writer.Write("Outputs: ");
                    for (var i = 0; i < parameters.OutputsCount; i++)
                    {
                        writer.Write(vector[vector.Length - parameters.OutputsCount + i] + " ");
                    }
                    writer.WriteLine();

                    // print active nodes
                    if (activeNodes != null)
                    {
                        writer.Write("active Nodes: ");
                        for (int i = 0; i < activeNodes.Count; i++)
                        {
                            writer.Write(activeNodes[i] + " ");
                        }
                        writer.WriteLine();
                    }
                    writer.WriteLine(new string('_', (int)((max + 3) * parameters.ColumnCount)));
                }
            }
        }

        public static void PrintGrid(Exception e, FloatVector vector, CGPConfiguration parameters, string filename, bool append = true, string tag = null, bool printActiveNodes = false, bool printNodeIndices = false)
        {
            List<float> activeNodes = null;
            var decoder = new CGPDecoder(parameters);
            if (printActiveNodes)
                activeNodes = decoder.ActiveNodes(vector).Where(x => x >= 0).ToList();
            string[,] grid = new string[parameters.RowCount, parameters.ColumnCount];

            var printingMap = parameters.OperatorMap.PrintingMap;


            // print program grid (containing nodes)
            for (var node = 0; node < parameters.NodesCount; node++)
            {
                var column = node / parameters.RowCount;
                var row = node % parameters.RowCount;
                var nodeIndex = parameters.NodeIndex(node);
                string nodeString = "";
                if (activeNodes != null && activeNodes.Contains(node))
                {
                    nodeString += "{{";
                }
                nodeString += node.ToString() + ": ";

                if (printNodeIndices)
                    nodeString += parameters.NodeIndex(node) + ": ";

                nodeString += "IN: ";

                var function = vector[parameters.OperatorIndex(node)];

                for (var i = 0; i < parameters.InputCountOfOperator(function); i++)
                {
                    nodeString += vector[nodeIndex + i].ToString(CultureInfo.InvariantCulture) + " ";
                }
                // decode node function
                var op = vector[nodeIndex + parameters.InputCount];

                nodeString += printingMap[op];
                nodeString += "(";
                for (var j = 0; j < parameters.ParameterCount - 1; j++)
                {
                    nodeString += vector[parameters.ParameterIndex(node) + j].ToString(CultureInfo.InvariantCulture) + ",";
                }
                nodeString += vector[parameters.ParameterIndex(node) + parameters.ParameterCount - 1].ToString(CultureInfo.InvariantCulture);
                nodeString += ")";

                if (activeNodes != null && activeNodes.Contains(node))
                {
                    nodeString += "}}";
                }
                grid[row, column] = nodeString;
                // write nodestring Inputs + Function + Parameters
            }

            lock (mutex)
            {
                if (!filename.Contains(".txt"))
                    filename = Path.Combine(BasePath, filename, "grid.txt");
                using (var writer = new StreamWriter(filename, append))
                {
                    var max = 0;
                    foreach (var v in grid)
                    {
                        if (v.Length > max) max = v.Length;
                    }

                    if (tag != null) writer.WriteLine(tag);
                    writer.WriteLine("HashCode: " + vector.GetHashCode());
                    writer.WriteLine("Time: " + DateTime.Now);
                    // print exception details

                    writer.WriteLine(e.GetType().ToString() + ", " + e.Message + ", " + e.StackTrace);

                    if (e is UnexpectedException)
                    {
                        var unexpected = (UnexpectedException)e;
                        writer.WriteLine("current:" + unexpected.Current.ToString() + ": ");

                        foreach (var key in unexpected.ColumnNodeMap.Keys)
                        {
                            writer.WriteLine("Column: " + key);
                            foreach (var node in unexpected.ColumnNodeMap[key])
                            {
                                var exe = "in: ";
                                foreach (var input in unexpected.ExecutionTree[node])
                                {
                                    exe += input + " ";
                                }
                            }
                        }
                    }

                    // print program inputs
                    writer.Write("Inputs: ");
                    for (int i = 0; i < parameters.ProgramInputCount; i++)
                    {
                        writer.Write(parameters.ProgramInputIdentifiers[i] + " ");
                    }
                    writer.WriteLine();

                    // print nodes
                    for (int i = 0; i < parameters.RowCount; i++)
                    {
                        string s = "";
                        for (int j = 0; j < parameters.ColumnCount; j++)
                        {
                            s += grid[i, j] + new string(' ', (int)max - grid[i, j].Length) + " | ";
                        }
                        writer.WriteLine(s);


                    }

                    // print program outputs
                    writer.Write("Outputs: ");
                    for (var i = 0; i < parameters.OutputsCount; i++)
                    {
                        writer.Write(vector[vector.Length - parameters.OutputsCount + i] + " ");
                    }
                    writer.WriteLine();

                    // print active nodes
                    if (activeNodes != null)
                    {
                        writer.Write("active Nodes: ");
                        for (int i = 0; i < activeNodes.Count; i++)
                        {
                            writer.Write(activeNodes[i] + " ");
                        }
                        writer.WriteLine();
                    }
                    writer.WriteLine(new string('_', (int)((max + 3) * parameters.ColumnCount)));
                }
            }
        }


        public static void PrintGrid(MultipleFloatVectorEncoding vector, CGPConfiguration parameters, string filename, bool append = true, string tag = null, bool printActiveNodes = false, bool printNodeIndices = false)
        {
            PrintGrid(new Exception("ignore this"), vector, parameters, filename, append, tag, printActiveNodes, printNodeIndices);
        }

        public static void PrintGrid(Exception e, MultipleFloatVectorEncoding vector, CGPConfiguration parameters, string directory, bool append = true, string tag = null, bool printActiveNodes = false, bool printNodeIndices = false)
        {
            List<float> activeNodes = null;
            var decoder = new CGPDecoder(parameters);
            if (printActiveNodes)
                activeNodes = decoder.ActiveNodes(vector);
            string[,] grid = new string[parameters.ColumnOperatorCount.Max() /* parameters.RowCount */, parameters.ColumnCount - 1];               // what to do with rowcount and this encoding? set rowcount to max(AllRows)?

            var printingMap = parameters.OperatorMap.PrintingMap;

            // print program grid (containing nodes)
            for (var node = 0; node < parameters.NodesCount; node++)
            {
                // var column = node / parameters.RowCount;    //dont know why its calculated that way if there's a function for it
                //var row = node % parameters.RowCount;
                int row, column;
                parameters.ColumnOfAndNodeNumberInVector(node, true, out column, out row);
                var nodeIndex = parameters.NodeIndex(node);
                string nodeString = "";
                if (activeNodes != null && activeNodes.Contains(node))
                {
                    nodeString += "{{";
                }
                nodeString += node.ToString() + ": ";

                if (printNodeIndices)
                    nodeString += parameters.NodeIndex(node) + ": ";

                nodeString += "IN: ";

                var function = vector[column, parameters.OperatorIndex(node)];

                for (var i = 0; i < parameters.InputCountOfOperator(function); i++)
                {
                    nodeString += vector[column, nodeIndex + i].ToString(CultureInfo.InvariantCulture) + " ";
                }
                // decode node function
                var op = vector[column, nodeIndex + parameters.InputCount];

                nodeString += printingMap[op];
                nodeString += "(";
                for (var j = 0; j < parameters.ParameterCount - 1; j++)
                {
                    nodeString += vector[column, parameters.ParameterIndex(node) + j].ToString(CultureInfo.InvariantCulture) + ",";
                }
                nodeString += vector[column, parameters.ParameterIndex(node) + parameters.ParameterCount - 1].ToString(CultureInfo.InvariantCulture);
                nodeString += ")";

                if (activeNodes != null && activeNodes.Contains(node))
                {
                    nodeString += "}}";
                }
                grid[row, column] = nodeString;
                // write nodestring Inputs + Function + Parameters
            }

            lock (mutex)
            {

                using (var writer = new StreamWriter(Path.Combine(directory, "grid.txt"), append))
                {
                    var max = 0;
                    foreach (var v in grid)
                    {
                        if (v != null)
                        {
                            if (v.Length > max) max = v.Length;
                        }
                    }

                    if (tag != null) writer.WriteLine(tag);
                    writer.WriteLine("HashCode: " + vector.GetHashCode());
                    writer.WriteLine("Time: " + DateTime.Now);
                    // print exception details

                    writer.WriteLine(e.GetType().ToString() + ", " + e.Message + ", " + e.StackTrace);

                    if (e is UnexpectedException)
                    {
                        var unexpected = (UnexpectedException)e;
                        writer.WriteLine("current:" + unexpected.Current.ToString() + ": ");
                        foreach (var key in unexpected.ColumnNodeMap.Keys)
                        {
                            writer.WriteLine("Column: " + key);
                            foreach (var node in unexpected.ColumnNodeMap[key])
                            {
                                var exe = "in: ";
                                foreach (var input in unexpected.ExecutionTree[node])
                                {
                                    exe += input + " ";
                                }
                            }
                        }
                    }

                    // print program inputs
                    writer.Write("Inputs: ");
                    for (int i = 0; i < parameters.ProgramInputCount; i++)
                    {
                        writer.Write(i + " ");
                    }
                    writer.WriteLine();

                    // print nodes
                    for (int i = 0; i < parameters.ColumnOperatorCount.Max(); i++)
                    {
                        string s = "";
                        for (int j = 0; j < parameters.ColumnCount - 1; j++)                //Last column contains the output nodes
                        {
                            if (grid[i, j] != null)     //not every entry in the grid has to be occupied
                            {
                                s += grid[i, j] + new string(' ', (int)max - grid[i, j].Length) + " | ";
                            }
                        }
                        writer.WriteLine(s);
                    }

                    // print program outputs
                    writer.Write("Outputs: ");
                    for (var i = 0; i < parameters.OutputsCount; i++)
                    {
                        writer.Write(vector[parameters.ColumnCount - 1, i] + " ");
                    }
                    writer.WriteLine();

                    // print active nodes
                    if (activeNodes != null)
                    {
                        writer.Write("active Nodes: ");
                        for (int i = 0; i < activeNodes.Count; i++)
                        {
                            writer.Write(activeNodes[i] + " ");
                        }
                        writer.WriteLine();
                    }
                    writer.WriteLine(new string('_', (int)((max + 3) * parameters.ColumnCount)));
                }
            }
        }

    }

    // write program outputs
}


