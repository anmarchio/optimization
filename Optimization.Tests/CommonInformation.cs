using System.IO;
using Extensions;
using NUnit.Framework;

namespace Optimization.Tests
{
    public static class CommonInformation
    {

        public static string Directory
        {
            get
            {
                return TestContext.CurrentContext.TestDirectory;
            }
        }

        public static string OptimizationTestsDirectory
        {
            get
            {
                var dir = System.IO.Directory.GetParent(Directory);
                return Path.Combine(dir.Parent.Parent.FullName, "Optimization.Tests", "Debug", "bin");
            }
        }

        public static string TestResultsDirectory
        {
            get
            {
                var path = Path.Combine(Directory, "TestResults");
                path.CreateDirectory();
                return path;
            }
        }

        public static string HDevPipelines
        {
            get
            {
                var path = Path.Combine(TestResultsDirectory, "HDevPipelines");
                path.CreateDirectory();
                return path;
            }
        }
    }
}
