using System;
using System.IO;

namespace Extensions
{
    public static class ExceptionExtensions
    {
        private static object mutex = new object();
        const string logfile = "logfile.txt";

        public static void Log(this Exception exception)
        {
            lock(mutex)
            {
                using (var writer = new StreamWriter(logfile))
                {
                    writer.WriteLine();
                    writer.WriteLine(DateTime.Now);
                    writer.WriteLine(exception.Message);
                    writer.WriteLine(exception.StackTrace);
                    writer.WriteLine();
                }
            }
        }
    }
}
