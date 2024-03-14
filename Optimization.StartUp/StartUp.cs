using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using Extensions;
using HalconDotNet;
using Optimization.HPipeline.Serialization;
using Optimization.Serialization;
using Serilog;

namespace Optimization.StartUp
{
    class StartUp
    {
        [STAThread]
        static void Main(string[] args)
        {
            var path = "exceptions";
            path.CreateDirectory();
            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File(Path.Combine(path, "log.txt"), rollingInterval: RollingInterval.Hour).CreateLogger();

            HOperatorSet.SetSystem("temporary_mem_cache", "false");

            var database = DataBaseScanner.Database.SQLite;
            if (!Configuration.UseSQLite) database = DataBaseScanner.Database.Postgres;

            try
            {
                Console.WriteLine("Starting Optimization, waiting for batch runs...");
                MonitorDataBase(database);
            }
            catch(Exception ex)
            {
                Log.Fatal(ex, "Program died");
                Console.WriteLine("Program died, see exceptions/ for infos.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
            Log.CloseAndFlush();
        }

        private static void MonitorDataBase(DataBaseScanner.Database database)
        {
            var scanner = new DataBaseScanner(database);
            var t = new Thread(scanner.ScanContinuously);
            t.Start();


            while (true)
            {
                var line = Console.ReadLine();
                if (line.Equals("exit"))
                {
                    scanner.Stop();
                    t.Join();
                    return;
                }

                Thread.Sleep(100);
            }
        }

        private static void ReportStatus(object sender, ProgressChangedEventArgs e)
        {
            Console.WriteLine(e.ProgressPercentage);
        }

        private static void Work(object sender, DoWorkEventArgs e)
        {
            throw new NotImplementedException();
        }
        
        public static void ZMQInterProcessCommunication(string[] args)
        {
            foreach (var s in args)
                Console.WriteLine(s);

            SocketConnector connector = null;

            var endpoint = args.Where(x => x.Contains("-endpoint=")).FirstOrDefault();

            if (!string.IsNullOrEmpty(endpoint))
            {
                var split = endpoint.Split('=');
                if (!split[1].Equals("default"))
                {
                    connector = new SocketConnector(split[1]);
                }
                else
                {
                    connector = new SocketConnector();
                }
            }
            else
            {
                connector = new SocketConnector();
            }

            Console.WriteLine("Using endpoint: " + connector.Endpoint);

            while (true)
            {
                var line = Console.ReadLine();
                if (line.Equals("exit"))
                {
                    connector.Disconnect();
                    break;
                }
            }
        }
    }
}
