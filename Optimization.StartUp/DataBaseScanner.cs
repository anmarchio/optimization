using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Extensions;
using Optimization.EvolutionStrategy;
using Optimization.Serialization;
using Optimization.Serialization.Interfaces;
using Serilog;

namespace Optimization.StartUp
{
    [Obsolete("Should move away from communication via database." +
              " We should use the commandline tool to execute a batch job," +
              " then read all analyzer .json results and send them to the django application.")]
    public class DataBaseScanner
    {
        public enum Database
        {
            SQLite, Postgres
        }

        public DataBaseScanner(Database db = Database.SQLite)
        {
            if (db == Database.SQLite)
                connector = new SqliteConnector(Configuration.SQLiteDBPath);
            else if (db == Database.Postgres)
                connector = new PostgresConnector(Configuration.Server,
                    Configuration.UserID, Configuration.Port, Configuration.Password, Configuration.DataBaseName);
            BatchRunQueue = new Queue<Tuple<string, BatchRun>>();
            Worker = new BackgroundWorker();
            Worker.DoWork += ExecuteBatch;
            Worker.WorkerSupportsCancellation = true;
            Worker.WorkerReportsProgress = true;
            Worker.ProgressChanged += ReportProgress;
            Worker.RunWorkerAsync();

            /*
            Worker2 = new BackgroundWorker();
            Worker2.DoWork += ExecuteBatch;
            Worker2.WorkerSupportsCancellation = true;
            Worker2.WorkerReportsProgress = true;
            Worker2.ProgressChanged += ReportProgress;
            Worker2.RunWorkerAsync();

            */

            //Room for more Worker-"Threads" to execute on the Queue

        }

        private void ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine(e.ProgressPercentage + "/100%");
            connector.updateQuery(SQLQueryStrings.UpdateBatchRunProgress(e.ProgressPercentage.ToString(), CurrentBatchRun.Item1));
        }

        private Tuple<string, BatchRun> CurrentBatchRun { get; set; }

        private void ExecuteBatch(object sender, DoWorkEventArgs e)
        {
            var w = sender as BackgroundWorker;

            while(!w.CancellationPending)
            {
                if(BatchRunQueue.Count > 0)
                {
                    CurrentBatchRun = BatchRunQueue.Dequeue();
                    var ID = CurrentBatchRun.Item1;
                    try
                    {                
                        Console.WriteLine("Starting Batch Run with ID:" + ID);
                        var batch = CurrentBatchRun.Item2;
                        batch.Run();

                        // log validation results in database -- WARNING: again picking only first fitness value
                        // would have to rewrite this, but we don't really want to maintain the communication via database
                        var trainFit = batch.BestIndividuals.Select(x => x.Item2.Fitness.First().Value).ToArray();
                        batch.Validate();
                        int i = 0;
                        foreach(var best in batch.BestIndividuals)
                        {
                            WriteResult(i, ID, trainFit[i] ?? 0, best.Item2.Fitness.First().Value ?? 0);
                            i++;
                        }
                        // racing condition?
                        if (w.CancellationPending && !batch.Finished)
                            SetBatchRunState("cancelled", ID);
                        else
                            SetBatchRunState("finished", ID);

                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, string.Format("during batch run execution: id: {0}", ID));
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        SetBatchRunState("error", ID);
                    }
                }
                Thread.Sleep(1000);
            }
        }

        /*
        private void Validate(Tuple<string, BatchRun> batch)
        {
            var id = batch.Item1;
            var run = batch.Item2;
            var eval = run.EvolutionStrategy.Evaluator as IValidator;
            
            int i = 0;
            foreach(var best in run.BestIndividuals)
            {
                var trainFitness = best.Fitness;
                var valFitness =  eval.Validate(best);
                WriteResult(i, id, trainFitness, valFitness);
                i++;  
            }    
        }*/

        private void WriteResult(int iteration, string batchRunId, double trainFitness, double valFitness)
        {
            if (double.IsNaN(trainFitness))
                trainFitness = -1;
            if (double.IsNaN(valFitness))
                valFitness = -1;
            connector.updateQuery(SQLQueryStrings.WriteBatchRunResult(iteration.ToString(), batchRunId, trainFitness.ToInvariantString(),
                valFitness.ToInvariantString()));
        }

        IConnector connector;

        private BackgroundWorker Worker { get; set; }

        private BackgroundWorker Worker2 { get; set; }

        private bool Running { get; set; } = true;

        private bool Finished = false;

        public void ScanContinuously()
        {

            while(Running)
            {
                Thread.Sleep(1000);

                if (CurrentBatchRun == null || CurrentBatchRun.Item2.Finished == true)
                {
                    try
                    {
                        var next = GetNextBatchRun();
                        if (next == null)
                        {
                            continue;
                        }
                        BatchRunQueue.Enqueue(next);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine(ex.StackTrace);
                        Console.WriteLine();
                        Log.Error(ex, "error during database scanning");
                    }
                }             
            }

            Finished = true;
        }

        public Queue<Tuple<string, BatchRun>> BatchRunQueue { get; set; }

        public void Stop()
        {
            Running = false;
            Worker.CancelAsync();
            while(Worker.IsBusy && !Finished) { Thread.Sleep(10); }
            connector.Close();
        }

        /// <summary>
        /// should retrieve the most recent batch run
        /// </summary>
        /// <returns></returns>
        private Tuple<string, BatchRun> GetNextBatchRun()
        {

            var tableBatch = connector.selectQuery(SQLQueryStrings.UnfinishedBatchRuns);
            if (tableBatch.Rows.Count < 1) return null;
            var batchID = tableBatch.Rows[0][tableBatch.Columns.IndexOfColumnName("id")].ToString();
            try
            {
                var confID = tableBatch.Rows[0][tableBatch.Columns.IndexOfColumnName("cgp_configuration_id")].ToString();
                var tableCGP = connector.selectQuery(SQLQueryStrings.CGPConfiguration(confID));
                var config = FromTableConverter.CGPConfiguration(tableCGP.Columns, tableCGP.Rows[0]);

                /*
                var trainbatchDataID = tableBatch.Rows[0][tableBatch.Columns.IndexOfColumnName("train_batch_data_id")].ToString();
                var traindataTable = connector.selectQuery(SQLQueryStrings.BatchData(trainbatchDataID));
                var traindataDir = traindataTable.Rows[0][traindataTable.Columns.IndexOfColumnName("name")].ToString();*/

                /*
                var varbatchDataID = tableBatch.Rows[0][tableBatch.Columns.IndexOfColumnName("val_batch_data_id")].ToString();
                var valdataTable = connector.selectQuery(SQLQueryStrings.BatchData(trainbatchDataID));
                var valdataDir = traindataTable.Rows[0][traindataTable.Columns.IndexOfColumnName("name")].ToString();*/

                var batch = FromTableConverter.BatchRun(tableBatch.Columns, tableBatch.Rows[0], config, connector);

                //var valRefSet = FromTableConverter.ReferenceSet(tableBatch.Columns, tableBatch.Rows[0], "val");
                //throw new NotImplementedException("forgot to pass validationset properly to evaluator");

                SetBatchRunState("busy", batchID);

                return new Tuple<string, BatchRun>(batchID, batch);
            }catch(Exception ex)
            {
                Log.Error(ex, string.Format("during batch run creation: id: {0}", batchID));
                SetBatchRunState("error", batchID);
            }
            return null;
        }

        private void SetBatchRunState(string state, string id)
        {
            /*
            var tableBatch = connector.selectQuery(SQLQueryStrings.BatchRun(id));
            tableBatch.Rows[0][tableBatch.Columns.IndexOfColumnName("state")] = state;
            */
            connector.updateQuery(SQLQueryStrings.UpdateBatchRunState(state, id));

        }
    }
}
