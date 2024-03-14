using Serilog;
using System;
using System.IO;
using System.Windows.Forms;
using Extensions;
using Newtonsoft.Json;

namespace PRIME.RegionMarker
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            var path = "exceptions";
            path.CreateDirectory();

            Log.Logger = new LoggerConfiguration().MinimumLevel.Debug().WriteTo.File(Path.Combine(path, "log.txt"), rollingInterval: RollingInterval.Hour).CreateLogger();
            try
            {
                string[] labels = null;

                try
                {
                    labels = ReadLabels();
                }
                catch (FileNotFoundException)
                {
                    using (var writer = new StreamWriter("labels.json"))
                    {
                        writer.WriteLine(JsonConvert.SerializeObject(new string[]
                        {
                            "FiberCrack", "LooseFilament", "Fuzzball", "Loop", "Artifact", "Contaminant", "Other", "Foreground",
                            "Background"
                        }));
                    }

                    labels = ReadLabels();
                }
                var marker = new RegionMarker(labels);
                Application.Run(marker);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "fatal");
            }

            Log.CloseAndFlush();
        }

        private static string[] ReadLabels()
        {
            using (var reader = new StreamReader("labels.json"))
            {
                var json = JsonConvert.DeserializeObject<string[]>(reader.ReadToEnd());
                return json;
            }
        }
    }
}
