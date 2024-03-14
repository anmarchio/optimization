using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.EvolutionStrategy.Random;

namespace Optimization.Data
{
    public class DataLoader<DType> : ICopyable
    {

        /// <summary>
        /// DataLoader class that loads batches of a given dataset, optionally shuffles the dataset
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="random"> the IRandom instance used to create the shuffled index</param>
        /// <param name="shuffle"></param>
        /// <param name="truncate">if dataset.Count / batchSize != 0 and truncate is true, then the last
        /// few elements of count less than batchSize will not be returned </param>
        public DataLoader(DataSet<DType> dataset, IRandom random = null, bool truncate = false)
        {
            var batchSize = dataset.Count;
            Initialize(dataset, batchSize, random, truncate);
        }

        /// <summary>
        /// DataLoader class that loads batches of a given dataset, optionally shuffles the dataset
        /// </summary>
        /// <param name="dataset"></param>
        /// <param name="random"> the IRandom instance used to create the shuffled index</param>
        /// <param name="batchSize"></param>
        /// <param name="shuffle"></param>
        /// <param name="truncate">if dataset.Count / batchSize != 0 and truncate is true, then the last
        /// few elements of count less than batchSize will not be returned </param>
        public DataLoader(DataSet<DType> dataset, int batchSize, IRandom random = null, bool truncate = false)
        {
            Initialize(dataset, Math.Min(batchSize, dataset.Count), random, truncate);
        }

        private void Initialize(DataSet<DType> dataset, int batchSize, IRandom random = null, bool truncate = false, int? workerCount = null)
        {
            if (random != null)
            {
                Random = random;
                Shuffle = true;
            }
            DataSet = dataset;
            WorkerCount = batchSize > 3 ? 4 : 1;
            BatchSize = batchSize;
        }

        public int QueueSize { get; set; } = 8;

        public bool Shuffle { get; private set; }

        public bool Truncate { get; private set; }
        public int BatchSize { get; private set; }

        public int NumBatches
        {
            get
            {
                if (!Truncate)
                {
                    return (int)Math.Ceiling((decimal)DataSetSize / BatchSize);
                }
                else
                {
                    return (int)Math.Floor((decimal)DataSetSize / BatchSize);
                }
            }
        }

        private int WorkerCount { get; set; }

        public int DataSetSize
        {
            get
            {
                return DataSet.Count;
            }
        }

        private IRandom Random { get; set; }

        public DataSet<DType> DataSet { get; private set; }

        private List<DType> FullSet { get; set; } = null;

        public bool ReturnsFullSet
        {
            get
            {
                return DataSet.FitsIntoMemory;
            }
        }


        public IEnumerable<List<DType>> Batches()
        {
            if (DataSet.FitsIntoMemory)
            {
                if (FullSet == null)
                    FullSet = GetFullSet();
                yield return FullSet;
            }
            else
            {

                BlockingCollection<List<DType>> queue;
                using (queue = new BlockingCollection<List<DType>>(QueueSize))
                {

                    int[] indexPermutation = null;
                    if (Shuffle)
                        indexPermutation = Random.IndexPermutation(DataSet.Count);
                    else
                        indexPermutation = Enumerable.Range(0, DataSet.Count).ToArray();

                    var args = new PrefetchArgs(DataSet, indexPermutation, queue, BatchSize, Truncate);

                    var worker = new Thread(new ParameterizedThreadStart(PreloadBatchAsync));
                    try
                    {                 
                        worker.Start(args);

                        for (int i = 0; i < NumBatches; i++)
                        {
                            List<DType> batch = null;
                            while (!queue.TryTake(out batch, TimeSpan.FromMinutes(10))) ;
                            yield return batch;
                        }
                    }
                    finally
                    {
                        args.CancellationPending = true;
                        if(worker != null)
                            worker.Join();
                    }

                    foreach (var batch in queue)
                        foreach (var item in batch)
                            if (item is IDisposable) ((IDisposable)item).Dispose();
                }
            }
        }

        public List<DType> GetFullSet()
        {
            var list = new List<DType>();
            for (int i = 0; i < DataSet.Count; i++)
                list.Add(DataSet[i]);
            return list;
        }

        private static void PreloadBatchAsync(object sender)
        {
            try
            {
                var args = sender as PrefetchArgs;
                int numBatches = args.DataSet.Count / args.BatchSize;


                for (int i = 0; i < numBatches; i++)
                {
                    int startIdx = i * args.BatchSize;
                    var batch = new List<DType>();
                    for (int j = 0; j < args.BatchSize; j++)
                    {
                        batch.Add(args.DataSet[args.IndexPermutation[startIdx + j]]);
                    }

                    
                    while (!args.CancellationPending && !args.Queue.TryAdd(batch)) ;
                    if (args.CancellationPending)
                    {
                        args.Queue.CompleteAdding();
                        return;
                    }
                }


                // handle few remaining elements
                if (args.DataSet.Count % args.BatchSize != 0 && !args.Truncate)
                {
                    var batch = new List<DType>();
                    for (int i = numBatches * args.BatchSize; i < args.DataSet.Count; i++)
                    {
                        batch.Add(args.DataSet[args.IndexPermutation[i]]);
                    }

                    while (!args.CancellationPending && !args.Queue.TryAdd(batch)) ;
                }

                args.Queue.CompleteAdding();
            }
            catch (Exception ex)
            {
                Serilog.Log.Logger.Debug(ex, "in batch worker thread");
            }
        }

        internal class PrefetchArgs
        {
            internal PrefetchArgs(DataSet<DType> dataSet, int[] indexPermutation, BlockingCollection<List<DType>> queue, int batchSize, bool truncate)
            {
                DataSet = dataSet;
                IndexPermutation = indexPermutation;
                Queue = queue;
                BatchSize = batchSize;
                Truncate = truncate;
                CancellationPending = false;
            }

            public int BatchSize { get; private set; }
            public DataSet<DType> DataSet { get; private set; }
            
            public int[] IndexPermutation { get; private set; }
            public bool Truncate { get; private set; }
            public bool CancellationPending { get; internal set; }
            public BlockingCollection<List<DType>> Queue { get; private set; }
        }


        public ICopyable Copy()
        {
            return new DataLoader<DType>(DataSet, Random, Truncate);
        }

        public ICopyable Copy(IRandom rand)
        {
            return new DataLoader<DType>(DataSet, Random, Truncate);
        }
    }
}
