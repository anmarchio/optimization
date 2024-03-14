using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions;
using NUnit.Framework;
using Optimization.Commandline.Tests.Categories;
using Optimization.EvolutionStrategy;
using Optimization.HPipeline;
using Optimization.Tests;
using Optimization.Tests.Categories;
using Optimization.Tests.TestImages;

namespace Optimization.Commandline.Tests
{
    [TestFixture]
    public class ProgramTest
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            CommonInformation.TestResultsDirectory.CreateDirectory();
            Directory.SetCurrentDirectory(CommonInformation.TestResultsDirectory);
        }

        /// <summary>
        /// Use this to avoid redundant command definitions.
        /// </summary>
        /// <returns>List of example commands starting batch runs using the CLI.</returns>
        private List<string> GetCommandList(string outDir)
        {
            outDir.CreateDirectory();
            var inDir = Path.Combine(CommonImages.ImageFormatConversionDirectory, "Indexed");
            var operatorDir = Path.Combine(CommonInformation.TestResultsDirectory, "..", "..", "..", "..", "Optimization.Commandline", "Operators");

            var commands = new List<string>()
            {
                string.Format($@"batch --backend=halcon --runs=2 --train-data-dir={inDir} --val-data-dir={inDir} --generations=1 --results-dir={outDir}"),
                string.Format($@"batch --backend=halcon --runs=2 --train-data-dir={inDir} --val-data-dir={inDir} --generations=1 --results-dir={outDir} --seed=37"),
                @"operators --backend=halcon",
                string.Format($@"batch --backend=halcon --runs=2 --train-data-dir={inDir} --val-data-dir={inDir} --generations=1 --results-dir={outDir} --seed=37 --operators=operators.xml"),
                string.Format($@"batch --backend=halcon --runs=2 --train-data-dir={inDir} --val-data-dir={inDir} --generations=1 --results-dir={outDir} --seed=37 --operators=operators.xml --fit-func=IntersectionOverUnion"),
                string.Format($@"batch --backend=halcon --runs=2 --train-data-dir={inDir} --val-data-dir={inDir} --generations=1 --results-dir={outDir} --seed=37 --operators={Path.Combine(operatorDir, "Halcon", "laplace.xml")} --fit-func=IntersectionOverUnion"),
                string.Format($@"batch --backend=halcon --runs=2 --train-data-dir={inDir} --val-data-dir={inDir} --generations=1 --results-dir={outDir} --seed=37 --operators={Path.Combine(operatorDir, "Halcon", "batteriebleche.xml")} --fit-func=IntersectionOverUnion"),
                $@"batch --backend=halcon --runs=2 --train-data-dir={inDir} --val-data-dir={inDir} --generations=1 --results-dir={outDir} --seed=37 --operators={Path.Combine(operatorDir, "Halcon", "batteriebleche.xml")} --fit-func=IntersectionOverUnion --worker-count=2",
                $@"batch --backend=halcon --runs=2 --train-data-dir={inDir} --val-data-dir={inDir} --generations=1 --results-dir={outDir} --seed=37 --operators={Path.Combine(operatorDir, "Halcon", "batteriebleche.xml")} --fit-func=IntersectionOverUnion --worker-count=6",
            };

            return commands;
        }

        /// <summary>
        /// Compares confusion matrices produced by the "batch" verb and the "evaluate" verb to ensure that they are identical.
        ///
        /// Added worker-count argument
        /// </summary>
        [Test,ShortTest]
        public void ConfusionMatrices()
        {
            var resultsConfusionFilename = "ResultsConfusionIndexed";
            var evalConfusionFilename = "EvalConfusionIndexed";

            var outDir = Path.Combine(CommonInformation.TestResultsDirectory, resultsConfusionFilename);
            try { Directory.Delete(outDir, recursive: true); } catch { }
            outDir.CreateDirectory();
            var evalOutDir = Path.Combine(CommonInformation.TestResultsDirectory, evalConfusionFilename);
            try { Directory.Delete(evalOutDir, recursive: true); } catch { }
            evalOutDir.CreateDirectory();



            try
            {
                foreach (var evotype in new List<string>() {"standard", "selfadaptive"})
                {
                    var dirList = new List<string>()
                    {
                        Path.Combine(CommonImages.ImageFormatConversionDirectory, "Indexed")
                    };

                    foreach (int workerCount in new List<int>() {1, 2, 4})
                    {
                        foreach (var inDir in dirList)
                        {

                            var batch =
                                $@"batch --backend=halcon --runs=5 --train-data-dir={inDir} --val-data-dir={inDir} --generations=5 --results-dir={outDir}" +
                                $@" --seed=0 --operators={Path.Combine(Directory.GetParent(outDir).Parent.Parent.Parent.Parent.FullName,
                                    "Optimization.Commandline", "Operators", "Halcon", "fast.xml")}" +
                                " --fit-funcs=IntersectionOverUnion MCC" +
                                " --fits-mem=True" +
                                $" --evo-type={evotype}" +
                                $" --worker-count={workerCount}";

                            Assert.AreEqual(0, Program.Main(batch.Split(' ')), evotype);

                            foreach (var p in Directory.EnumerateDirectories(Path.Combine(resultsConfusionFilename, "Grid")))
                            {
                                var eval =
                                    $@"evaluate --backend=halcon --data-dir={inDir} --out-dir={Path.Combine(evalOutDir, p)} --fit-funcs=IntersectionOverUnion MCC";
                                var gridPath = Path.Combine(CommonInformation.TestResultsDirectory, p);
                                var imagePath = Path.Combine(CommonInformation.TestResultsDirectory, p)
                                    .Replace("Grid", "Images");
                                var pipePath = Path.Combine(gridPath, "pipeline.xml");
                                var eval_pipe = eval + " --pipeline-path=" + pipePath;
                                Assert.AreEqual(0, Program.Main(eval_pipe.Split(' ')), $"{p}, {evotype}");

                                using (var reader1 = new StreamReader(Path.Combine(imagePath, "ConfusionMatrix.txt")))
                                {
                                    var confPath = Path.Combine(evalOutDir, p, "ConfusionMatrix.txt");
                                    using (var reader2 = new StreamReader(confPath))
                                    {
                                        Assert.AreEqual(reader1.ReadToEnd(), reader2.ReadToEnd(), $"{p}, {evotype}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                try
                {
                    Directory.Delete(outDir, recursive: true);
                } catch { }

                try
                {
                    Directory.Delete(evalOutDir, recursive: true);
                }
                catch { }
            }
        }

        /// <summary>
        /// Executes the "evaluate" verb with a few sample pipelines.
        /// </summary>
        [Test,ShortTest]
        public void RunEvaluate()
        {
            var evalOutDir = Path.Combine(CommonInformation.TestResultsDirectory, "evalConfusionIndexed");
            try { Directory.Delete(evalOutDir, recursive: true); } catch { }
            evalOutDir.CreateDirectory();

            var dirList = new List<string>()
            {
                Path.Combine(CommonImages.TestImagesDirectory, "CompareCrackDetectionHdev"),
                Path.Combine(CommonImages.TestImagesDirectory, "CompareFiberOrientationHdev"),
                Path.Combine(CommonImages.TestImagesDirectory, "CompareNonWovenTwistsHdev"),
            };

            var pipeList = new List<HalconPipeline>
            {
                CommonHalconPipelines.CrackDetectionHdev,
                CommonHalconPipelines.FiberOrientationHdev,
                CommonHalconPipelines.NonWovenTwistsHdev
            };

            Assert.AreEqual(dirList.Count, pipeList.Count);

            for (int i = 0; i < dirList.Count; i++)
            {
                var pipe = pipeList[i];
                var inDir = dirList[i];
                pipe.SerializeXml("pipe.xml");

                var eval = $@"evaluate --backend=halcon --fit-funcs=MCC --data-dir={inDir} --out-dir={evalOutDir} --pipeline-path=pipe.xml";
                Assert.AreEqual(0, Program.Main(eval.Split(' ')));

            }
        }


        /// <summary>
        /// Just run a few more or less random halcon related commands to see if anything breaks.
        /// </summary>
        [Test,Halcon]
        public void HalconOperators()
        {
            var outDir = Path.Combine(CommonInformation.TestResultsDirectory, "results");
            var commands = GetCommandList(outDir);
            for (int i = 0; i < commands.Count; i++)
            {
                var command = commands[i];
                Assert.AreEqual(0, Program.Main(command.Split((' '))), command);
            }
        }

        [Test, EmguCv]
        public void EmguCvOperators()
        {
            var outDir = Path.Combine(CommonInformation.TestResultsDirectory, "results");
            outDir.CreateDirectory();
            var inDir = Path.Combine(CommonImages.ImageFormatConversionDirectory, "Indexed");

            var commands = new List<string>()
            {
                string.Format(@"batch --backend=emgucv --runs=5 --train-data-dir={0} --val-data-dir={1} --generations=5 --results-dir={2}", inDir, inDir, outDir),
                string.Format(@"batch --backend=emgucv --runs=5 --train-data-dir={0} --val-data-dir={1} --generations=5 --results-dir={2} --seed=37", inDir, inDir, outDir)
            };

            try
            {
                for(int i = 0; i < commands.Count; i++)
                {
                    var command = commands[i];
                    Assert.AreEqual(0, Program.Main(command.Split((' '))), command);
                }
            }
            finally
            {
                Directory.Delete(outDir, recursive: true);
            }
        }

        /// <summary>
        /// Test if using an empty "Verb" for just printing options works
        ///
        /// Ideally we would check if the command __actually__ prints all the fitness function options, but this seems like an unreasonable amount of work.
        /// </summary>
        [Test,ShortTest]
        public void PrintFitFuncOptions()
        {
            Assert.AreEqual(0, Program.Main(new string[] {"fit-func"}));
		}
		
        /// <summary>
        /// Check if running the CLT without any verb correctly prints all available verbs
        /// 1 corresponds to a "parsing" error, due to no verb being passed as argument
        /// </summary>
        [Test,ShortTest]
        public void RunEmptyVerb()
        {
            Assert.AreEqual(1, Program.Main(new [] {""}));
        }


        /// <summary>
        /// Calling CLI with statusquo pipelines and searchspace options
        /// </summary>
        [Test,ShortTest]
        public void SearchSpaces()
        {
            var outDir = Path.Combine(CommonInformation.TestResultsDirectory, "results");
            outDir.CreateDirectory();
            var inDir = Path.Combine(CommonImages.ImageFormatConversionDirectory, "Indexed");

            try
            {
                foreach (var pipeline in CommonHalconPipelines.Collection.Where(x => x.Name != "PeronaMalik"))
                {
                    var pipePath = Path.Combine(outDir, pipeline.Name + ".xml");
                    pipeline.SerializeXml(pipePath);
                    Assert.AreEqual(0,
                        Program.Main($"operators --backend=halcon --pipeline={pipeline.Name} --out-dir={outDir}"
                            .Split(' ')));
                    Assert.AreEqual(0,
                        Program.Main(($"batch" +
                        $" --backend=halcon" +
                        $" --runs=2" +
                        $" --train-data-dir={inDir}" +
                        $" --val-data-dir={inDir}" +
                        $" --generations=10" +
                        $" --results-dir={outDir}" +
                        $" --seed=37" +
                        $" --search-space={SearchSpaceType.ParametersOnly.ToString()}" +
                        $" --status-quo={pipePath}" +
                        $" --fits-mem").Split(' ')),
                        $"{pipeline.Name} failed");
                }
            }
            finally
            {
                Directory.Delete(outDir, recursive: true);
            }
        }


        /// <summary>
        /// Plausibility check for bruce-force verb in CLI
        /// </summary>
        [Test,LongTest]
        public void BruteForce()
        {
            var outDir = Path.Combine(CommonInformation.TestResultsDirectory, "results");
            outDir.CreateDirectory();
            var inDir = Path.Combine(CommonImages.ImageFormatConversionDirectory, "Indexed");

            try
            {
                foreach (var pipeline in CommonHalconPipelines.Collection.Where(x => x.Name != "PeronaMalik"))
                {
                    var pipePath = Path.Combine(outDir, pipeline.Name + ".xml");
                    pipeline.SerializeXml(pipePath);
                    Assert.AreEqual(0,
                        Program.Main(($"brute-force" +
                                      $" --backend=halcon" +
                                      $" --data-dir={inDir}" +
                                      $" --out-dir={outDir}" +
                                      $" --pipeline-path={pipePath}" +
                                      $" --node-ids=-1" +  // not really optimizing anything, as this takes way too long. Should be input nodes without parameters.
                                      $" --fit-funcs=IntersectionOverUnion").Split(' ')),
                        $"{pipeline.Name} failed optimizing");
                }
            }
            finally
            {
                Directory.Delete(outDir, recursive: true);
            }
        }

        /// <summary>
        /// Checks if validation.json and overview.json are identical if identical training and validation data is used (which it should be).
        /// </summary>
        [Test, ShortTest]
        public void Overview()
        {
            var outDir = Path.Combine(CommonInformation.TestResultsDirectory, "results");
            List<string> commands = GetCommandList(outDir);

            for (int i = 0; i < commands.Count; i++)
            {
                var command = commands[i];
                Assert.AreEqual(0, Program.Main(command.Split((' '))), command);

                using (var valReader = new StreamReader(Path.Combine(outDir, "validation.json")))
                {
                    using (var overviewReader = new StreamReader(Path.Combine(outDir, "overview.json")))
                    {

                        Assert.AreEqual(valReader.ReadToEnd(),
                            overviewReader.ReadToEnd(),
                            "Using identical train and val data, the overview for training and validation are expected to be identical.");
                    }
                }
            }
            
        }
    }
}
