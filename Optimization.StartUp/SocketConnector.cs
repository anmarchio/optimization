using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Optimization.HPipeline;
using ZeroMQ;

namespace Optimization.StartUp
{
    public class SocketConnector
    {
        private Thread thread;

        public  SocketConnector(string endpoint = null)
        {
            if (!string.IsNullOrEmpty(endpoint)) Endpoint = endpoint;
            IncomingQueue = new Queue<string>();
            OutgoingQueue = new Queue<string>();
            Connect();
        }

        public string Endpoint { get; set; } = "tcp://127.0.0.1:5555";


        public void Connect()
        {
            Running = true;
            if (thread != null) throw new Exception("Connection should already be established.");
            thread = new Thread(Poll);
            thread.Start();

            Worker = new BackgroundWorker();
            Worker.DoWork += Work;
            Worker.ProgressChanged += UpdateProgress;
            Worker.RunWorkerAsync();
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;
        }

        private BackgroundWorker Worker { get; set; }

        private bool Running { get; set; } = true;

        private void Poll()
        {

            using (var context = new ZContext())
            using (var responder = new ZSocket(context, ZSocketType.REP))
            {
                // Bind
                responder.Bind(Endpoint);

                while (Running)
                {
                    Thread.Sleep(1000);
                             
                    var response = ReceiveFrame(responder);
                    if (response != null) SendFrame(responder, response);

                    if (OutgoingQueue.Count > 0)
                        SendFrame(responder, OutgoingQueue.Dequeue());

                    // Receive
                    /*
                    using (ZFrame request = responder.ReceiveFrame(ZSocketFlags.DontWait, out error))
                    {
                        if (request == null) { Thread.Sleep(1000); continue; }
                        var req = request.ReadString();
                        Console.WriteLine("Received {0}", req);

                        // Do some work
                       
                        Worker.RunWorkerAsync();
                        var response = Response(req);

                        // Send
                        responder.Send(new ZFrame(response));

                    }*/
                }
            }
        }

        private Queue<string> IncomingQueue { get; set; }

        private Queue<string> OutgoingQueue { get; set; }

        private string ReceiveFrame(ZSocket socket)
        {
            ZError error;
            using (ZFrame request = socket.ReceiveFrame(ZSocketFlags.DontWait, out error))
            {
                if (request == null) return null;
                var req = request.ReadString();

                Console.WriteLine("Received {0}", req);

                if (req.Contains("progress"))
                    return "progress=" + BackgroundWorkerProgress;

                if(IsJob(req))
                    IncomingQueue.Enqueue(req);
            }
            return null;
        }

        private bool IsJob(string req)
        {
            if (req.Contains("ALL")) return true;
            var split = req.Split(';');
            if (CommonHalconPipelines.HalconPipelineDictionary.Keys.Contains(split[0])) return true;
            return false;
        }

        private void SendFrame(ZSocket socket, string response)
        {
            socket.Send(new ZFrame(response));
        }

        public string BackgroundWorkerProgress { get; set; }

        private void UpdateProgress(object sender, ProgressChangedEventArgs e)
        {
            BackgroundWorkerProgress = e.ProgressPercentage.ToString();
        }


        /// <summary>
        /// In case other types of responses are desired, simply inherit from SocketConnector and overwrite this Response method.
        /// Also, I recommend overwriting the Options property.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected virtual string Response(string request)
        {
            var split = request.Split(';');
            string runId = split.Length > 3 ? "id=" + split[3] : null;

            switch (split[0])
            {
                case "options": return Options;
                case "update": return "progress=" + BackgroundWorkerProgress;
            }

            // currently: only perform one batch run at time... scaling this takes up too much time now and is currently unnecessary

            if(Worker != null && Worker.IsBusy)
            {
                return "busy";
            }
            else
            {
                if (Worker == null)
                    Worker = new BackgroundWorker();
                else
                {
                }
            }

            if(split[0].Equals("ALL"))
            {
                throw new NotImplementedException();
                //var result = Program.HalconBatchFromPipelineName("ALL", split[1], split.Length > 2 ? split[2] : null); return runId != null ? runId : "batch completed\n" + result;
            }
            if (CommonHalconPipelines.HalconPipelineDictionary.Keys.Contains(split[0]))
            {
                throw new NotImplementedException();
                //var result = Program.HalconBatchFromPipelineName(split[0], split[1], split.Length > 2 ? split[2] : null); return runId != null ? runId : "batch completed\n" + result;
            }
            return "illegal request: for valid options, write 'options'";         
        }


        private string StartBatchRunFromString(string s, BackgroundWorker worker)
        {
            var split = s.Split(';');
            if (split[0].Equals("ALL"))
            {
                throw new NotImplementedException();
                //var result = Program.HalconBatchFromPipelineName("ALL", split[1], split.Length > 2 ? split[2] : null, worker);
                //return result;
            }
            if (CommonHalconPipelines.HalconPipelineDictionary.Keys.Contains(split[0]))
            {
                throw new NotImplementedException();
                //var result = Program.HalconBatchFromPipelineName(split[0], split[1], split.Length > 2 ? split[2] : null, worker);
                //return result; 
            }

            return "illegal request";
        }

        protected virtual void Work(object sender, DoWorkEventArgs e)
        {
            /*
             * ; return runId != null ? runId : "batch completed\n" + result;
             * */
            var worker = sender as BackgroundWorker;
            while(!worker.CancellationPending)
            {
                while(IncomingQueue.Count == 0 && !worker.CancellationPending) { Thread.Sleep(1000); } // wait for job
                var job = IncomingQueue.Dequeue();
                var result = StartBatchRunFromString(job, worker);

                OutgoingQueue.Enqueue(result);
            }
        }

        public virtual string Options
        {
            get
            {
                var options = "Halcon Options: \n";
                foreach (var key in CommonHalconPipelines.HalconPipelineDictionary.Keys)
                    options += key + "\n";
                return options;
            }
        }


        public void Disconnect()
        {
            Running = false;
            if(thread != null)
                thread.Join();
            if (Worker != null)
                Worker.CancelAsync();
        }

    }
}
