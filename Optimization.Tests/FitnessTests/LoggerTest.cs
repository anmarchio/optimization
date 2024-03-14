using System;
using System.IO;
using NUnit.Framework;
using Optimization.Fitness.ErrorHandling;
using Optimization.Tests.Categories;

namespace Optimization.Tests.FitnessTests
{
    [TestFixture]
    public class LoggerTest
    {
        [Test,ShortTest]
        public void LogExceptions()
        {
            Logger.BasePath = CommonInformation.Directory;
            Logger.LogException(new Exception("test exception"));
            File.Exists(Path.Combine(Logger.BasePath, Logger.DefaultFilename));
        }
    }
}
