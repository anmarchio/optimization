# Wiki

* [Commandline](commandline.md)
* [Programming-Guidelines](programming-guidelines.md)
* [REST communication between Optimization and Webapp](rest.md)
* [Testing-Guidelines](testing.md)
* [Troubleshooting](troubleshooting.md)
* [gitlab runner](gitlab-runner.md)
* [Home](home.md)

## Testing Guidelines

This page contains a few notes on how to test this very heuristic and random-based application.

First of all, we use [NUnit](https://github.com/nunit/nunit) for automating our tests. If you are not familiar with NUnit, feel free to read up on it on their official repository and wiki.

### Running Tests

You can run tests in VisualStudio by first going to:

Test -> Test Settings -> Default Processor Architecture -> X64 (at least with my Halcon distribution I need to set this, else I'll get a BadImageFormatException. Feel free to ignore this is tests work for you right out of the box)

After building the solution, you should be able to see tests in the Test Explorer window (Test -> Windows -> Test Explorer) where you can run individual tests.

Additionally, whenever you push your code to GitLab, all tests will be run automatically and you'll get an E-mail notification if something fails.

### Structure of Tests

The structure is as follows:

As you can see, each sub-project of the optimization project has its own testing project. Always add your tests to the test project corresponding to the project that you wrote your new code in. If working on the Extensions project, add your tests to Optimization.Tests

```
optimization
|__Optimization
|__Optimization.Tests
|__Optimization.Commandline
|__Optimization.Commandline.Tests
|__Optimization.CVPipeline
|__Optimization.CVPipeline.Tests
|__Optimization.HPipeline
|__Optimization.HPipeline.Tests
|  |__DataTests
|  |__FitnessTests
|  |  |__DecodingMapTest.cs
|  |  | ...
|  |__SerializationTests
|  |__BatchRunTests.cs
|  |__ ...
| ...
```

All test projects offer a few simple basic directories, that you should work with when reading/writing image data. Also make sure to clean up after your test (i.e. remove images that might have been written to disk).

The most important typically reside in:

* Optimization.Tests: CommonInformation.cs
* Optimization.Tests.TestImages: CommonImages.cs
* Optimization.HPipeline.Tests: CommonHImages.cs
* Optimization.CVPipeline.Tests: CommonCVImages.cs

In particular the various sample images might be of use. 

**Important**: when writing test code that writes something to disk, use the CommonInformation.TestResultsDirectory as target directory. This prevents NUnit from trying to write in protected places (e.g. C://).

Most test classes should be placed at a location in the namespace corresponding to the class that they test. Also use the same name with "Tests" appended, such as: HPipeline.cs -> HPipelineTests.cs

### Categories

NUnit allows to decorate methods and classes with attributes. Because the application is very heuristic in nature, some tests consist of randomly executing as many pipelines and operators as possible. This consumes a lot of time. Hence, these tests should be flagged with LongRunning attribute, enabling us to execute them only occasionally, in particular when changes were made that could affect the outcome of the test and before merging.

#### TBD: When to use which category

Currently we offer 5 different categories of tests:

* ShortTest: <5 seconds
* LongTest: >= 5 seconds
* ExtremeLongTest: too long
* EmguCvTest: Only apply to Optimiztion.CommandLine.Test, indicates that EmguCv is involved
* HalconTest: Only apply to Optimiztion.CommandLine.Test, indicates that Halcon is involved

If you want to test locally and want to exclude some kinds of tests, you can use the NUnit Console Runner...

TBD

### Seeds

The second imortant thing is to use seeding to make tests as deterministic as possible. It's not very useful if a test fails due to some random configuration if we cannot reproduce the error and fix it afterwards.

Take following test, for example:

``` C#
/// <summary>
/// This class tests the creation of individuals that are encoded as boolean vectors.
/// </summary>
[TestFixture] // Indicates to NUnit that this class contains tests.
public class BooleanVectorCreatorTest
{
        /// <summary>
        /// Test the random instantiation of an individual.
        /// </summary>
        [Test] // Indicate to NUnit that this method is a test.
        public void Create()
        {
            var random = new SystemRandom(0); // IMPORTANT: Always set a specific seed, so that the test always returns the same result.
            int maxTrueCount = 10;
            var creator = new BooleanVectorCreator(random, 50, maxTrueCount);
            var vector = creator.Create().BooleanVector;

            for(int i = 0; i < 100; i++)
            {
                // Use assert statements to assert something about the results. Consider providing a helpful message in case the test fails.
                Assert.IsTrue(IsValid(vector, maxTrueCount), "iteration: "+ i + " tc: " + vector.TrueCount);
                vector = creator.Create().BooleanVector;
            }
        }
}   
```

### Types of Tests

There are various different types of tests one can write. Feel free to search the internet for introductions. Whenever possible, you should write unit tests to test individual classes and methods if they behave as expected. In particular, border cases should be treated. If you wrote something that changes the behaviour of a larger port ion of the code, e.g. added a new Terminator class for evolution strategies, then you should also write a test that runs an evolution strategy while using your terminator class. Just check the various testing projects inside the solutions for inspiration.