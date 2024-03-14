using NUnit.Framework;
using Optimization.HPipeline.Fitness.OperatorMaps;

namespace Optimization.HPipeline.Tests.FitnessTests
{
    [TestFixture]
    public class DecodingMapTest : DecodingMap
    {
        /*
         * This ought to removed, as it is replaced by EnumerateParameters pipeline tests
         * 
        [Test]
        public void SobelAmpParameters()
        {
            var operatorMap = new OperatorMap();
            var sobelBounds = operatorMap.ParameterBounds[0];
            var tuples = new HTuple[3];
            var hobjects = new HObject[MaxHObjects];

            hobjects[0] = CommonImages.StandardFiber;

            for (int i = 0; i < sobelBounds[0].Count; i++)
            {
                for (int j = 0; j < sobelBounds[1].Count; j++)
                {
                    tuples[0] = sobelBounds[0][i];
                    tuples[1] = sobelBounds[1][j];
                    sobelAmp(hobjects, tuples);
                }
            }
        }
        [Test]
        public void ThresholdAccessChannelParameters()
        {
            Assert.Pass("This Test takes a long time. Run only if absolutely necessary.");
            var operatorMap = new OperatorMap();
            var thresholdBounds = operatorMap.ParameterBounds[1];
            var tuples = new HTuple[3];
            var hobjects = new HObject[MaxHObjects];

            hobjects[0] = CommonImages.StandardFiber;

            HOperatorSet.SobelAmp(hobjects[0], out hobjects[0], "y", 3);

            var backup = hobjects[0].Clone();

            for (int i = 0; i < thresholdBounds[0].Count; i++)
            {
                for (int j = 0; j < thresholdBounds[1].Count; j++)
                {
                    for (int k = 0; k < thresholdBounds[2].Count; k++)
                    {
                        hobjects[0] = backup.Clone();
                        tuples[0] = thresholdBounds[0][i];
                        tuples[1] = thresholdBounds[1][j];
                        tuples[2] = thresholdBounds[2][k];
                        thresholdAccessChannel(hobjects, tuples);

                    }
                }
            }
        }

        [Test]
        public void SobelAmpAndThresholdAccessChannel()
        {
            Assert.Pass("This Test takes a long time. Only run if necessary.");

            var operatorMap = new OperatorMap();
            var thresholdBounds = operatorMap.ParameterBounds[1];
            var tuples = new HTuple[3]; var sobelTuples = new HTuple[3];
            var hobjects = new HObject[3][];

            hobjects[0] = new HObject[MaxHObjects];
            hobjects[1] = new HObject[MaxHObjects];
            hobjects[2] = new HObject[MaxHObjects];


            var sobelBounds = operatorMap.ParameterBounds[0];

            hobjects[0][0] = CommonImages.StandardFiber;

            var backup = hobjects[0][0].Clone();

            var errParam = new List<Tuple<HTuple[], Exception>>();

            for (int i = 0; i < thresholdBounds[0].Count; i++)
            {
                for (int j = 0; j < thresholdBounds[1].Count; j++)
                {
                    for (int k = 0; k < thresholdBounds[2].Count; k++)
                    {

                        for (int l = 0; l < sobelBounds[0].Count; l++)
                        {
                            for (int m = 0; m < sobelBounds[1].Count; m++)
                            {
                                sobelTuples[0] = sobelBounds[0][l];
                                sobelTuples[1] = sobelBounds[1][m];

                                hobjects[0][0] = backup;
                                hobjects[1] = sobelAmp(hobjects[0], sobelTuples);

                                tuples[0] = thresholdBounds[0][i];
                                tuples[1] = thresholdBounds[1][j];
                                tuples[2] = thresholdBounds[2][k];

                                try
                                {
                                   hobjects[2] = thresholdAccessChannel(hobjects[1], tuples);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine("th param: " + tuples[0].ToString() + " " + tuples[1].ToString() + " " + tuples[2].ToString());

                                    Console.WriteLine("sobelparam: " + sobelTuples[0].ToString() + " " + sobelTuples[1].ToString());
                                    var newTuple = new HTuple[5];
                                    newTuple[0] = sobelTuples[0]; newTuple[1] = sobelTuples[1];
                                    newTuple[2] = tuples[0]; newTuple[3] = tuples[1]; newTuple[4] = tuples[2];
                                    errParam.Add(new Tuple<HTuple[], Exception>(newTuple, e));

                                    Dispose(hobjects);

                                    //if(hobjects[0] != null) hobjects[0].Dispose();
                                    //if (hobjects[1] != null) hobjects[1].Dispose();
                                }
                                Dispose(hobjects);


                                //if (hobjects[0] != null) hobjects[0].Dispose();
                                // if (hobjects[1] != null) hobjects[1].Dispose();
                            }
                        }
                    }
                }
            }


            using (var writer = new StreamWriter(Path.Combine(CommonInformation.TestResultsDirectory, "exceptions.txt")))
            {
                foreach (var e in errParam)
                {
                    writer.WriteLine(e.Item2.Message + HTupleToString(e.Item1));
                }
            }

            Assert.AreEqual(0, errParam.Count);
        }

        private void Dispose(HObject[][] objects)
        {
            for(int i = 1; i < objects.Length;i ++)
            {
                for(int j = 0; j < objects[i].Length; j++)
                {
                    if (objects[i][j] != null) objects[i][j].Dispose();
                }
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private string HTupleToString(HTuple[] tuple)
        {
            string s = "";
            for (int i = 0; i < tuple.Length; i++)
            {
                s += tuple[i] + " ";
            }
            return s;
        }


        [Test]
        public void ClosingParameters()
        {
            var operatorMap = new OperatorMap();
            var closingParameterBounds = operatorMap.ParameterBounds[4];
            var tuples = new HTuple[3];
            var hobjects = new HObject[MaxHObjects];
            HOperatorSet.GenRandomRegion(out hobjects[0], 100, 500);

            for (int i = 0; i < closingParameterBounds[0].Count; i++)
            {
                for (int j = 0; j < closingParameterBounds[1].Count; j++)
                {
                    for (int k = 0; k < closingParameterBounds[2].Count; k++)
                    {
                        tuples[0] = closingParameterBounds[0][i];
                        tuples[1] = closingParameterBounds[1][j];
                        tuples[2] = closingParameterBounds[2][k];
                        closing(hobjects, tuples);
                    }
                }
            }

        }
        [Test]
        public void SelectShapeParameters()
        {
            var operatorMap = new OperatorMap();
            var selectShapeBounds = operatorMap.ParameterBounds[5];
            var tuples = new HTuple[3];
            var hobjects = new HObject[MaxHObjects];
            HOperatorSet.GenRandomRegion(out hobjects[0], 100, 500);


            for (int i = 0; i < selectShapeBounds[0].Count; i++)
            {
                for (int j = 0; j < selectShapeBounds[1].Count; j++)
                {

                    tuples[0] = selectShapeBounds[0][i];
                    tuples[1] = selectShapeBounds[1][j];
                    selectShape(hobjects, tuples);

                }
            }
        }
        [Test]
        public void ConnectionParameters()
        {
            var operatorMap = new OperatorMap();
            var connectionBounds = operatorMap.ParameterBounds[6];
            var tuples = new HTuple[3];
            var hobjects = new HObject[MaxHObjects];
            HOperatorSet.GenRandomRegion(out hobjects[0], 100, 500);

            for (int i = 0; i < connectionBounds[0].Count; i++)
            {
                tuples[0] = connectionBounds[0][i];
                connection(hobjects, tuples);
            }
        }*/
    }
}
