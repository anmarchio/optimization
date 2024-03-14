using System;
using System.IO;
using Extensions;

namespace Optimization.Serialization
{
    public static class CSVWriter
    {
        public static void Write(double[,] data, string[] columnHeads, string path, bool append = false)
        {
            if (columnHeads != null && data.GetLength(1) != columnHeads.Length) throw new Exception("must specify a name for each column");

            using (var writer = new StreamWriter(path, append))
            {
                if (columnHeads != null)
                {
                    for (int i = 0; i < columnHeads.Length - 1; i++) writer.Write(columnHeads[i] + ",");
                    writer.Write(columnHeads[columnHeads.Length - 1] + writer.NewLine);
                }

                for(int i = 0; i < data.GetLength(0); i++)
                {

                    for(int j = 0; j < data.GetLength(1) - 1; j++)
                    {
                        writer.Write(data[i, j].ToInvariantString() + ",");
                    }
                    writer.Write(data[i, data.GetLength(1) - 1].ToInvariantString() + writer.NewLine);
                }
            }
        }

        public static void WriteWithRowLabels(double[,] data, string[] rowLabels, string path, bool append = false)
        {
            if (rowLabels != null && data.GetLength(0) != rowLabels.Length) throw new Exception("must specify a name for each column");

            using (var writer = new StreamWriter(path, append))
            {
                for (int i = 0; i < data.GetLength(0); i++)
                {
                    if (rowLabels != null)
                    {
                        writer.Write(rowLabels[i] + ",");
                    }
                    for (int j = 0; j < data.GetLength(1) - 1; j++)
                    {
                        writer.Write(data[i, j].ToInvariantString() + ",");
                    }
                    writer.Write(data[i, data.GetLength(1) - 1].ToInvariantString() + writer.NewLine);
                }
            }
        }

        public static void Write(float[,] data, string[] columnHeads, string path, bool append = false)
        {
            if (columnHeads != null && data.GetLength(1) != columnHeads.Length) throw new Exception("must specify a name for each column");

            using (var writer = new StreamWriter(path, append))
            {
                if (columnHeads != null)
                {
                    for (int i = 0; i < columnHeads.Length - 1; i++) writer.Write(columnHeads[i] + ",");
                    writer.Write(columnHeads[columnHeads.Length - 1] + writer.NewLine);
                }
                for (int i = 0; i < data.GetLength(0); i++)
                {

                    for (int j = 0; j < data.GetLength(1) - 1; j++)
                    {
                            writer.Write(data[i, j].ToInvariantString() + ",");
                    }
                    writer.Write(data[i, data.GetLength(1) - 1].ToInvariantString() + writer.NewLine);
                }
            }

        }
    }
}
