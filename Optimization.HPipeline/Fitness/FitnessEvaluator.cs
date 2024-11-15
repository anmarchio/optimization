using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HalconDotNet;
using Optimization.CartesianGeneticProgramming;
using Optimization.Data;
using Optimization.EvolutionStrategy.Encodings;
using Optimization.EvolutionStrategy.Evaluators;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;
using Optimization.Fitness.ErrorHandling;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Serilog;

namespace Optimization.HPipeline.Fitness
{
    /// <summary>
    /// Implements a number of fitness functions (MCC, FScore, Recall, Precision, ...), a pipeline-execution function and a wrapper method to be called from heuristiclabs programmable problem definition.
    /// ReferenceSet, cartesiandecoder must be set before this can be used.
    /// </summary>
    [Obsolete("BA Legacy Code -- DO NOT USE")]
    public class FitnessEvaluator : Evaluator<ReferenceImage>, IValidityTester //, ICopyable, IValidator
    {
        #region Properties

        public class PointD  // used for ROC
        {
            public PointD(double X, double Y)
            {
                this.X = X;
                this.Y = Y;
            }
            public double X;
            public double Y;
        }

        public static int dumpedImageCount = 0;
        public static double max = double.MinValue;
        private const int maxImageCount = 10;
        //public ReferenceSet ReferenceSet { get; set; }

        public static int timeThresholdExceptionCount = 0;
        public static int pixelPercentageExceptionCount = 0;
        public static int regionExceptionCount = 0;

        public static string Filename { get; set; }

        // dictionary for easier fitness function calls in Evaluate method
        protected Dictionary<FitnessFunction, Func<HObject[], ReferenceImage, double>> FitnessFunctions;

        #endregion

        public FitnessEvaluator(HalconFitnessConfiguration fitConfig, CGPConfiguration CGPConfig, CGPDecoder decoder, PipelineBuilder builder) : base(new DataLoader<ReferenceImage>(fitConfig.ReferenceSet), new DataLoader<ReferenceImage>(fitConfig.ValidationSet), fitConfig)
        {
            FitnessConfiguration = fitConfig;
            //ReferenceSet = FitnessConfiguration.ReferenceSet;
            Decoder = decoder;
            Builder = builder;
            CGPConfiguration = CGPConfig;

            if (fitConfig.ExecutionTimeThreshold != null)
            {
                UseExecutionTimeThreshold = true;
                ExecutionTimeThreshold = (int)fitConfig.ExecutionTimeThreshold;
            }
            if (fitConfig.PixelPercentageThreshold != null)
            {
                UsePercentageOfPixelsThreshold = true;
                PercentageOfPixelsThreshold = (double)fitConfig.PixelPercentageThreshold;
            }
            if (fitConfig.RegionCountThreshold != null)
            {
                UseRegionsCountThreshold = true;
                RegionsCountThreshold = (int)fitConfig.RegionCountThreshold;
            }

            Executor = new PipelineExecutor(fitConfig.ExecutionTimeThreshold, fitConfig.RegionCountThreshold, fitConfig.PixelPercentageThreshold);

            Initialize();
        }

        private void Initialize()
        {
            FitnessFunctions = new Dictionary<FitnessFunction, Func<HObject[], ReferenceImage, double>>()
            {
                {FitnessFunction.MCC, ComputeMCC },
                {FitnessFunction.FBetaScore, ComputeFScore },
                {FitnessFunction.Recall, ComputeRecall },
                {FitnessFunction.Sensitivity, ComputeRecall },
                //{FitnessFunction.RegionScore, ComputeRegionScore },
                {FitnessFunction.RegionScoreLb, ComputeRegionScoreLN },
                {FitnessFunction.J, ComputeJ },
                {FitnessFunction.MCC_savezones, ComputeMMC_with_savezones },
                {FitnessFunction.Specificity, ComputeSpecificity },
                {FitnessFunction.Precision, ComputePrecision },
            };
        }


        protected override void EvaluateItem(IIndividual individual, ReferenceImage item, params object[] additional)
        {
            throw new NotImplementedException("Not implemented in favor off existing code. Should not be called anyway."); // ignored in favor of easier use of existing code
        }


        protected void EvaluateLoader(IIndividual individual, DataLoader<ReferenceImage> loader)
        {
            throw new NotImplementedException();

            /*
            ResetFitness(individual);
            foreach (var batch in loader.Batches())
            {         
                var fit = Evaluate(individual, batch, CGPConfiguration, FitnessConfiguration.FitnessFunctions, FitnessConfiguration.ExcessRegionHandling);             
                var weightSum = FitnessConfiguration.Weights.Sum();
                for (int i = 0; i < fit.Length; i++)
                {             
                    if (FitnessConfiguration.FitnessFunctions.Length > 1)
                        individual.MultipleFitnessValues[i] += fit[i] / loader.NumBatches;
                    individual.Fitness += fit[i] * FitnessConfiguration.Weights[i];                
                }            
                individual.Fitness /= weightSum;
                individual.Fitness /= loader.NumBatches;
            }

            IndividualsEvaluated++;

            return individual.Fitness;*/
        }

        /*
        public double Evaluate(IIndividual individual)
        {
            var fitness = Evaluate(individual, CGPConfiguration, FitnessConfiguration.FitnessFunctions, FitnessConfiguration.ExcessRegionHandling);
            // var vector = individual.FloatVector;
            //  var fitness = Evaluate(vector, CGPConfiguration, FitnessConfiguration.FitnessFunctions, FitnessConfiguration.ExcessRegionHandling);
            double result = 0;
            evaluatedIndividuals++;
            for (int i = 0; i < FitnessConfiguration.FitnessFunctions.Length; i++)
            {
                result += fitness[i] * FitnessConfiguration.Weights[i];
            }
            var indivFitness = result / FitnessConfiguration.Weights.Sum();
            individual.Fitness = indivFitness;
            individual.MultipleFitnessValues = fitness;
            return indivFitness;
        }*/

        public CGPDecoder Decoder { get; set; }
        public PipelineBuilder Builder { get; set; }
        protected CGPConfiguration CGPConfiguration { get; set; }

        protected PipelineExecutor Executor { get; set; }

        private DecodingMap DecodingMap { get; set; }
        public object UseExcessRegionHandling { get; private set; }
        public long ExecutionTimeThreshold { get; private set; }
        public int RegionsCountThreshold { get; private set; }
        public bool UseRegionsCountThreshold { get; private set; }
        public bool UsePercentageOfPixelsThreshold { get; private set; }
        public double PercentageOfPixelsThreshold { get; private set; }
        public bool UseExecutionTimeThreshold { get; private set; }

  
        /// <summary>
        /// Call this from HeuristicLab's programmable problem definition. Make sure this static class is initialized properly beforehand.
        /// Method for calculation of fitness values of individuals; Execution is forwarded to the PipelineExecutor
        /// </summary>
        public double[] Evaluate(IIndividual individual, List<ReferenceImage> batch, CGPConfiguration parameters, FitnessFunction[] functions, ExcessRegionHandling handling)
        {
            Dictionary<float, HTuple[]> parameterArray;
            Dictionary<float, List<float>> executionTree;
            Dictionary<int, List<float>> columnNodeMap;
            Dictionary<float, Func<HObject[], HTuple[], HObject[]>> actionArray;
            HObject[] resultObjects = new HObject[batch.Count];
            List<PointD> aucPoints = new List<PointD>();
            var regionScore = Enumerable.Repeat<double>(0, batch.Count).ToArray();
            List<float> outputNodes;
            var watch = new Stopwatch();
            HTuple[] regionsCount = new HTuple[batch.Count];
            for (int i = 0; i < regionsCount.Length; i++)
            {
                regionsCount[i] = new HTuple(0);
            }
            bool isFloatVector = false, isMultipleFloatVectorEncoding = false;


            if (individual.GetType() == typeof(MultipleFloatVectorEncoding))
            {
                actionArray = Builder.Decode((MultipleFloatVectorEncoding)individual, out parameterArray, out outputNodes);     //contains the active nodes along with their execution function
                executionTree = Decoder.ComputeExecutionTree((MultipleFloatVectorEncoding)individual);      //contains the inputs for the active nodes (by nodenumber)
                columnNodeMap = Decoder.ComputeColumnNodeMap(individual);       //contains which nodes (nodenumbers) are active in the active columns
                isMultipleFloatVectorEncoding = true;
            }
            else if (individual.GetType() == typeof(FloatVector))
            {
                actionArray = Builder.Decode((FloatVector)individual, out parameterArray, out outputNodes);
                executionTree = Decoder.ComputeExecutionTree((FloatVector)individual, excludeProgramInputs:true);
                columnNodeMap = Decoder.ComputeColumnNodeMap(individual, excludeProgramInputs:true);
                isFloatVector = true;
            }
            else //not supported Encoding
            {
                parameterArray = null;
                executionTree = null;
                outputNodes = null;
                columnNodeMap = null;
                throw new Exception("The specified encoding type is not supported!");
            }


            var results = new double[functions.Length];
            var fitnessValues = new double[functions.Length][];
            HObject[] objects = new HObject[3];
            HTuple[] tuples = new HTuple[3];

            // debugging
            HObject[][] objectArr = new HObject[parameterArray.Count][];
            HObject[] outputs = new HObject[parameterArray.Count];

            try
            {

                for (int imageIdx = 0; imageIdx < batch.Count; imageIdx++)
                {
                    // input image or regions
                    objects[0] = batch[imageIdx].Image.Clone();

                    watch.Start();
                    outputs = Executor.IterateColumns2(objects, parameterArray, actionArray, executionTree, outputNodes, batch[imageIdx].PixelCount, ExcessRegionHandling.ThrowException, columnNodeMap);
                    watch.Stop(); // hammertime

                    if (outputs == null)
                    {
                        Dispose(objects);
                        goto END;
                    }

                    if (FitnessConfiguration.ExecutionTimeThreshold != null && watch.ElapsedMilliseconds > FitnessConfiguration.ExecutionTimeThreshold)
                    {
                        Dispose(objects);
                        goto END;
                    }

                    // compute region count
                    HOperatorSet.CountObj(outputs[0], out regionsCount[imageIdx]);

                    // compute fitness
                    for (int f = 0; f < functions.Length; f++)
                    {

                        if (fitnessValues[f] == null) fitnessValues[f] = new double[batch.Count];
                        fitnessValues[f][imageIdx] = FitnessFunctions[functions[f]].Invoke(new HObject[] { outputs[0], batch[imageIdx].UnionReferenceRegions }, batch[imageIdx]);
                       
                    }
                    resultObjects[imageIdx] = outputs[0];
                }

            }
            catch (Exception e)
            {
                Log.Error("Fitness Evaluation Exception", e);
                if (!e.Message.Contains("HALCON error #6001"))   // union2 memory exceptions still occur, yet they do not break the program anymore... yiay
                {
                    if (isFloatVector)
                    {
                        Logger.PrintGrid(e, individual.FloatVector, parameters, Logger.ExceptionGrids, true, "", true);
                    }
                    else if (isMultipleFloatVectorEncoding)
                    {
                        Logger.PrintGrid(e, individual.MultipleFloatVectorEncoding, parameters, Logger.ExceptionGrids, true, "", true);
                    }
                    Logger.LogException(e);
                }
                Dispose(objects);
                //   if (!Logger.ConsumeExceptions) throw e;

                /*
                #region restart routine if an exception occured -- alternatively simply continue evolution if an exception occured; Exception gets logged and program is restarted
                // Get file path of current process 
                var filePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                var halconKillerRoutine_path = Directory.GetParent(filePath).Parent.Parent.Parent.FullName + "\\HalconKiller\\bin\\Debug\\HalconKiller.exe";

                System.Threading.Thread.Sleep(10000);
                // Start program
                System.Diagnostics.Process.Start(filePath);
                System.Diagnostics.Process.Start(halconKillerRoutine_path);

                Environment.Exit(0);
                #endregion*/

                if (!Logger.ConsumeExceptions) throw;
                goto END;
            }

            /*
            if (functions.Contains(FitnessFunction.AUC))
            {
                int idx = 0;
                for (int i = 0; i < functions.Length; i++)
                {
                    if (functions[i] == FitnessFunction.AUC) idx = i;
                }
                results[idx] = ComputeAUC(aucPoints);
            }*/


            // compute average fitness per objective functions and image
            var executionTimeFitnessPenalty = CalculateExecutionTimeFitnessPenalty(watch.ElapsedMilliseconds / 1000);
            if (FitnessConfiguration.Weights == null || FitnessConfiguration.Weights.Length != batch.Count)
            {
                if (FitnessConfiguration.UseExecutionTimeFitnessPenalty)
                {
                    for (int i = 0; i < fitnessValues.Length; i++)
                    {
                        results[i] = executionTimeFitnessPenalty * fitnessValues[i].Sum() / fitnessValues[i].Length;
                    }
                }
                else
                {
                    for (int i = 0; i < fitnessValues.Length; i++)
                    {
                        results[i] = fitnessValues[i].Sum() / fitnessValues[i].Length;
                    }
                }
            }
            else
            {
                if (FitnessConfiguration.UseExecutionTimeFitnessPenalty)
                {
                    for (int i = 0; i < fitnessValues.Length; i++)
                    {
                        double fsum = 0;
                        for (int j = 0; j < fitnessValues[i].Length; j++)
                        {
                            fsum += FitnessConfiguration.Weights[j] * fitnessValues[i][j];
                        }
                        results[i] = executionTimeFitnessPenalty * fsum / FitnessConfiguration.Weights.Sum();
                    }
                }
                else
                {
                    for (int i = 0; i < fitnessValues.Length; i++)
                    {
                        double fsum = 0;
                        for (int j = 0; j < fitnessValues[i].Length; j++)
                        {
                                fsum += FitnessConfiguration.Weights[j] * fitnessValues[i][j];
                        }
                        results[i] = fsum / FitnessConfiguration.Weights.Sum();
                    }
                }
            }

            // free halcon memory
            for (int i = 0; i < resultObjects.Length; i++)
            {
                if (resultObjects[i] != null) resultObjects[i].Dispose();
            }

        END:
            actionArray.Clear();
            actionArray = null;


            //GC.Collect();
            //GC.WaitForPendingFinalizers();
            return results;
        }


        /// <summary>
        /// Calculates a penalty factor to reduce fitness for increasing execution time needed. Values returned: [0.00000001, 1].
        /// Note that the reduction is just minimal as it should award faster executing pipelines a tiny bit higher fitness to become preferable.
        /// </summary>
        /// <param name="executionTimeInSeconds">Values: [0.."infinity"]</param>
        /// <returns></returns>
        public double CalculateExecutionTimeFitnessPenalty(double executionTimeInSeconds)
        {
            if (executionTimeInSeconds >= FitnessConfiguration.ExecutionTimeFunctionScaleFactor)    //ensure a positive scale factor to make sure that the sign of the fitness number does not change (i.e. a negative fitness value gets positive)
            {
                return 0.00000001;
            }
            else
            {
                return 1 / Math.Log(FitnessConfiguration.ExecutionTimeFunctionScaleFactor) * Math.Log(-executionTimeInSeconds + FitnessConfiguration.ExecutionTimeFunctionScaleFactor);
            }
        }
        
        public bool IsValid(FloatVector individual, DataLoader<ReferenceImage> loader)
        {
            var parameters = CGPConfiguration;
            Dictionary<float, HTuple[]> parameterArray;
            Dictionary<float, List<float>> executionTree;
            Dictionary<int, List<float>> columnNodeMap;
            HObject[] resultObjects = new HObject[loader.DataSetSize];
            List<PointD> aucPoints = new List<PointD>();
            List<float> outputNodes;
            var watch = new Stopwatch();
  
            var actionArray = Builder.Decode(individual, out parameterArray, out outputNodes);
            executionTree = Decoder.ComputeExecutionTree(individual);
            columnNodeMap = Decoder.ComputeColumnNodeMap(individual);

            HObject[] objects = new HObject[3];
            HTuple[] tuples = new HTuple[3];

            // debugging
            HObject[][] objectArr = new HObject[parameterArray.Count][];
            HObject[] outputs = new HObject[parameterArray.Count];

            try
            {
                foreach (var batch in loader.Batches())
                {
                    foreach (var image in batch)
                    {
                        // input image or regions
                        objects[0] = image.Image.Clone();

                        watch.Start();
                        outputs = Executor.IterateColumns2(objects, parameterArray, actionArray, executionTree, outputNodes, image.PixelCount, ExcessRegionHandling.ThrowException, columnNodeMap);
                        watch.Stop();

                        if (UseExecutionTimeThreshold && watch.ElapsedMilliseconds > ExecutionTimeThreshold)
                        {
                            Dispose(objects);
                            Dispose(outputs);
                            return false;
                        }

                        if (outputs == null)
                        {
                            Dispose(objects);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dispose(objects);
                Dispose(outputs);
                Log.Error(ex, "In oldschool fitness evaluator. God knows why this is still used");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Pipeline Execution of the best individual for the purpose of saving the output of the best pipeline on the input image.
        /// Since this individual has previously completed the fitness evaluation and is valid, the fitness evaluation and some other stuff is not necessary compared to the "Evaluate" method.
        /// </summary>
        /// <param name="individual"></param>
        /// <param name="parameters"></param>
        /// <param name="Image">Image on which the pipeline is to be executed.</param>
        /// <param name="handling"></param>
        /// <returns>Regions as the result of the pipeline execution</returns>
        public HObject ExecutePipeline(IIndividual individual, CGPConfiguration parameters, HObject Image, int imagePixelCount, ExcessRegionHandling handling)
        {
            Dictionary<float, HTuple[]> parameterArray;
            Dictionary<float, List<float>> executionTree;
            Dictionary<int, List<float>> columnNodeMap;
            Dictionary<float, Func<HObject[], HTuple[], HObject[]>> actionArray;
            List<float> outputNodes;

            bool isFloatVector = false, isMultipleFloatVectorEncoding = false;


            if (individual.GetType() == typeof(MultipleFloatVectorEncoding))
            {
                actionArray = Builder.Decode(individual.MultipleFloatVectorEncoding, out parameterArray, out outputNodes);     //contains the active node along with their execution function
                executionTree = Decoder.ComputeExecutionTree((MultipleFloatVectorEncoding)individual);      //contains the inputs for the active nodes (by nodenumber)
                columnNodeMap = Decoder.ComputeColumnNodeMap(individual);       //contains which nodes (nodenumbers) are active in the active columns
                isMultipleFloatVectorEncoding = true;
            }
            else if (individual.GetType() == typeof(FloatVector))
            {
                actionArray = Builder.Decode(individual.FloatVector, out parameterArray, out outputNodes);
                executionTree = Decoder.ComputeExecutionTree((FloatVector)individual);
                columnNodeMap = Decoder.ComputeColumnNodeMap(individual);
                isFloatVector = true;
            }
            else //not supported Encoding
            {
                parameterArray = null;
                executionTree = null;
                outputNodes = null;
                columnNodeMap = null;
                throw new Exception("The specified encoding type is not supported!");
            }

            HObject[] image = new HObject[1];
            image[0] = Image.Clone();

            // debugging
            HObject[] outputs = new HObject[parameterArray.Count];

            try
            {
                outputs = Executor.IterateColumns2(image, parameterArray, actionArray, executionTree, outputNodes, imagePixelCount, ExcessRegionHandling.ThrowException, columnNodeMap);

                if (outputs == null)    //should not be possible since this individual has previously been executed successfully
                {
                    Dispose(image);
                    goto END;
                }
            }
            catch (Exception e) //should not be possible since this individual has previously been executed successfully
            {
                //Logger.LogException(new object[] { objects, tuples, debugging, parameters, parameterArray }, e, "FitnessEvaluation.txt");
                if (!e.Message.Contains("HALCON error #6001"))   // union2 memory exceptions still occur, yet they do not break the program anymore... yiay
                {
                    if (isFloatVector)
                    {
                        Logger.PrintGrid(e, individual.FloatVector, parameters, Logger.ExceptionGrids, true, "", true);
                    }
                    else if (isMultipleFloatVectorEncoding)
                    {
                        Logger.PrintGrid(e, individual.MultipleFloatVectorEncoding, parameters, Logger.ExceptionGrids, true, "", true);
                    }
                }
                Dispose(image);
                if (!Logger.ConsumeExceptions) throw e;
                goto END;
            }

        END:
            actionArray.Clear();
            actionArray = null;

            if (image != null)
            {
                Dispose(image);
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();

            return outputs[0];
        }



        #region Pipeline execution
        /*
        private HObject[] IterateColumns2(HObject[] objects, Dictionary<float, HTuple[]> parameterArray, Dictionary<float, Func<HObject[], HTuple[], HObject[]>> actionArray, Dictionary<float, List<float>> executionTree, List<float> outputNodes, int imageIndex, ExcessRegionHandling type, Dictionary<int, List<float>> columnNodeMap)
        {
            var outputs = new Dictionary<float, HObject[]>();
            float current = -1;
            var watch = new Stopwatch();

            try
            {
                foreach (var column in columnNodeMap.Keys)
                {
                    foreach (var node in columnNodeMap[column])
                    {
                        current = node;

                        if (executionTree[current].Count == 0) // if leaf node
                        {
                            outputs.Add(current, actionArray[current].Invoke(objects, parameterArray[current])); // process input image
                        }
                        else
                        {
                            // compute outputs for each input-node of current node
                            HObject[] inputs = new HObject[executionTree[current].Count];

                            for (int j = 0; j < inputs.Length; j++)
                            {
                                inputs[j] = outputs[executionTree[current][j]][0];
                            }

                            watch.Reset();
                            watch.Start();
                            outputs.Add(current, actionArray[current].Invoke(inputs, parameterArray[current]));
                            watch.Stop();

                            #region thresholding and exceptionhandling
                            if (UseExcessRegionHandling != null && watch.ElapsedMilliseconds > ExecutionTimeThreshold)
                            {
                                throw new TimeThresholdException(watch.ElapsedMilliseconds.ToString() + "ms", current);
                            }
                            HTuple number;
                            if (UseRegionsCountThreshold)
                            {
                                HOperatorSet.CountObj(outputs[current][0], out number);
                                if (number > RegionsCountThreshold)
                                {
                                    if (type == ExcessRegionHandling.ThrowException)
                                    {
                                        throw new RegionException("Illegal Regions produced: " + number, current);
                                    }
                                    else if (type == ExcessRegionHandling.ContinuousSelectShape)
                                    {
                                        ContinuousSelectShape(outputs[current][0], number);
                                    }
                                    else if (type == ExcessRegionHandling.Union1)
                                    {
                                        HOperatorSet.Union1(outputs[current][0], out outputs[current][0]);
                                    }
                                }
                            }
                            if (UsePercentageOfPixelsThreshold)
                            {
                                // check if ROIs == image
                                HTuple rows, columns, area;
                                HOperatorSet.AreaCenter(outputs[current][0], out area, out rows, out columns);

                                var count = area.TupleSum() / ReferenceSet.PixelCount(imageIndex);
                                if (count > PercentageOfPixelsThreshold)
                                {
                                    throw new PixelPercentageException("Illegal Regions produced. Size (in %): " + count + " max size: " + PercentageOfPixelsThreshold, current);
                                }
                            }
                            #endregion


                        }
                    }
                }
            }
            catch (Exception e)
            {
                Dispose(outputs);
                if (e is RegionException || e is PixelPercentageException || e is TimeThresholdException) throw;
                throw new UnexpectedException(e.GetType().ToString() + " " + e.Message + " current: " + current, parameterArray, executionTree, current, columnNodeMap);
            }


            // free memory
            var ret = new HObject[outputs[outputNodes.First()].Length];
            for (int i = 0; i < outputs[outputNodes.First()].Length; i++)  // there only is one output node
            {
                if (outputs[outputNodes.First()][i] != null) ret[i] = outputs[outputNodes.First()][i].Clone();
            }

            Dispose(outputs);

            return ret;
        }*/
        #endregion

        #region memory cleanup

        private static void Dispose(HObject[][] hobjects)
        {
            if (hobjects != null)
                for (int i = 0; i < hobjects.Length; i++)
                {
                    if (hobjects[i] != null)
                        for (int j = 0; j < hobjects[i].Length; j++)
                        {
                            if (hobjects[i][j] != null)
                                hobjects[i][j].Dispose();
                        }
                }
        }

        private static void Dispose(Dictionary<float, HObject[]> hobjects)
        {
            foreach (var key in hobjects.Keys)
            {
                for (int i = 0; i < hobjects[key].Length; i++)
                {
                    if (hobjects[key][i] != null) hobjects[key][i].Dispose();
                }
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private static void Dispose(HObject[] hobjects)
        {
            if (hobjects != null)
                for (int i = 0; i < hobjects.Length; i++)
                {
                    if (hobjects[i] != null)
                        hobjects[i].Dispose();
                }

            //GC.Collect();
            //GC.WaitForPendingFinalizers();
        }
        #endregion

        private void ContinuousSelectShape(HObject regions, int count)
        {
            int selectShapeMin = 5;
            HTuple number = count;
            while (number > RegionsCountThreshold)
            {
                HOperatorSet.SelectShape(regions, out regions, "area", "and", selectShapeMin, int.MaxValue);
                selectShapeMin += 5;
                HOperatorSet.CountObj(regions, out number);
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        #region Methods for fitness function calculations (FScore, MCC, AUC, ...)
        /// <summary>
        /// Computes FScore (aka FFactor); hobjects[0] must hold the actual regions, hobjects[1] the reference region
        /// <param name="param">use this to specify beta</param>
        /// </summary>
        protected double ComputeFScore(HObject[] hobjects, ReferenceImage img)
        {
            var precision = ComputePrecision(hobjects, img);
            var recall = ComputeRecall(hobjects, img);
            var betaSquare = FitnessConfiguration.FScoreBetaSquare;

            if (recall > 0 || precision > 0)
                return (1 + betaSquare) * recall * precision / (recall + betaSquare * precision);
            else
                return 0;
        }

        /// <summary>
        /// Computes Recall / Sensitivity (== true positive rate for ROC-AUC)
        /// </summary>
        protected double ComputeRecall(HObject[] hobjects, ReferenceImage img)
        {
            HOperatorSet.Union1(hobjects[0], out hobjects[0]);

            return Collection.Recall(hobjects[1], hobjects[0]);
        }

        /// <summary>
        /// Computes precision
        /// </summary>
        protected double ComputePrecision(HObject[] hobjects, ReferenceImage img)
        {
            HOperatorSet.Union1(hobjects[0], out hobjects[0]);

            return Collection.Precision(hobjects[1], hobjects[0]);
        }


        /// <summary>
        /// The award for the most unfortunate name goes to...
        /// </summary>
        protected double ComputeRegionScoreLN(HObject[] hobjects, ReferenceImage img)
        {
            HObject obj;
            HTuple actual;
            HOperatorSet.Connection(hobjects[0], out obj); // seperate regions if union1 was used
            HOperatorSet.CountObj(obj, out actual);
            return 1 / Math.Log(Math.Max(2, Math.Abs(actual - img.RegionCount)), 2);
        }

        /// <summary>
        /// Compute area under the roc-curve (AUROC / AUC)
        /// </summary>
        protected static double ComputeAUC(List<PointD> points)
        {
            if (points.Count < 1) return 0;
            points.Add(new PointD(0, 0));
            points.Add(new PointD(1, 1));
            points = points.OrderBy(x => x.X).ToList();
            double sum = 0;
            for (int i = 0; i < points.Count - 1; i++)
            {
                sum += (points[i + 1].X - points[i].X) * ((points[i].Y + points[i + 1].Y) / 2);
            }

            return sum;
        }

        /// <summary>
        /// Computes the specificity (true negative rate). Specificity = 1 - FPR
        /// </summary>
        /// <param name="hobject"></param>
        /// <param name="imageIndex"></param>
        /// <returns></returns>
        protected double ComputeSpecificity(HObject[] hobject, ReferenceImage img)
        {
            var result = ComputeFPR(hobject, img);
            return 1 - result;
        }

        /// <summary>
        /// Computes the false positive rate (for AUC). FPR = 1 - TNR = 1 - specificity
        /// </summary>
        protected double ComputeFPR(HObject[] hobjects, ReferenceImage img)
        {
            HTuple rows, columns, area;
            double TN = 0, FP = 0; // N == negatives
            HObject intersection; // == true positives
                                  // compute intersection

            HObject reference = hobjects[1];
            HObject actual = hobjects[0];
            HOperatorSet.Union1(reference, out reference);
            HOperatorSet.Union1(actual, out actual);
            // true positives: intersection(0, 1)
            // false positives difference(1,intersection(0,1)) -- see def. HOperatorSet difference

            // actual and reference pixel counts
            HOperatorSet.AreaCenter(actual, out area, out rows, out columns);
            double actualPixelCount = 0;
            if (area.Length > 0)
                actualPixelCount = area;
            // actual and reference pixel counts
            HOperatorSet.AreaCenter(reference, out area, out rows, out columns);
            double referencePixelCount = 0;
            if (area.Length > 0)
                referencePixelCount = area;


            // compute false positives (FP)
            var intersectionPixelCount = 0.0;
            HOperatorSet.Intersection(actual, reference, out intersection);
            HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
            if (area.Length > 0) intersectionPixelCount = area;

            FP = actualPixelCount - intersectionPixelCount;

            TN = img.PixelCount - actualPixelCount - referencePixelCount + intersectionPixelCount; // area == intersection

            return FP / (TN + FP);
        }

        /*
                /// <summary>
                /// See ComputeMMC_with_savezones for more compact variant! Areas close to Reference regions are not penalized as much. This means that the TP and TN count like usual but FN and FP are treated more lightly.
                /// </summary>
                protected double CustomFitnessFunction(HObject[] hobjects, int imageIndex)
                {
                    HTuple rows, columns, area;
                    long TP = 0, TN = 0, FP = 0, FN = 0;
                    HObject difference; // == false positives (for precision), false negatives (for recall)
                    HObject intersection; // == true positives
                                          // compute intersection

                    HObject reference = hobjects[1];
                    HObject actual = hobjects[0];
                    HOperatorSet.Union1(reference, out reference);
                    HOperatorSet.Union1(actual, out actual);
                    // true positives: intersection(0, 1)
                    // false positives difference(1,intersection(0,1)) -- see def. HOperatorSet difference

                    //Inner/Mid/Outer are the edge parts of the region
                    HObject Inner, Mid, Outer;
                    HTuple InnerArea, MidArea, OuterArea;
                    double InnerPenaltyFactor = 0.2, MidPenaltyFactor = 0.5, OuterPenaltyFactor = 0.9;

                    //enlarge the regions to represent a "save-band" around the regions which do not suffer a great fitness penalty      
                    HOperatorSet.DilationCircle(reference, out Inner, 1.5);
                    HOperatorSet.Difference(reference, Inner, out Inner);
                    HOperatorSet.AreaCenter(Inner, out InnerArea, out rows, out columns);

                    HOperatorSet.DilationCircle(reference, out Mid, 2.5);
                    HOperatorSet.Difference(Inner, Mid, out Mid);
                    HOperatorSet.AreaCenter(Mid, out MidArea, out rows, out columns);

                    HOperatorSet.DilationCircle(reference, out Outer, 3.25);
                    HOperatorSet.Difference(Mid, Outer, out Outer);
                    HOperatorSet.AreaCenter(Outer, out OuterArea, out rows, out columns);

                    // actual and reference pixel counts
                    HOperatorSet.AreaCenter(actual, out area, out rows, out columns);
                    long actualPixelCount = 0;
                    if (area.Length > 0)
                        actualPixelCount = area;
                    HOperatorSet.AreaCenter(reference, out area, out rows, out columns);
                    long referencePixelCount = 0;
                    if (area.Length > 0)
                        referencePixelCount = area;

                    #region Standard MMC calculation
                    // compute true positives (TP)
                    HOperatorSet.Intersection(actual, reference, out intersection);
                    HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
                    if (area.Length > 0)
                        TP = area;

                    // compute false negatives (FN)
                    HOperatorSet.Difference(reference, intersection, out difference);
                    HOperatorSet.AreaCenter(difference, out area, out rows, out columns);
                    if (area.Length > 0)
                        FN = area;

                    // compute true negatives
                    TN = ReferenceSet[imageIndex].PixelCount - (actualPixelCount + referencePixelCount - TP); // TP == intersection

                    // compute false positives
                    //HOperatorSet.AreaCenter(actual, out area, out rows, out columns);
                    //if (area.Length > 0)
                    //   FP = area - TP;
                    FP = actualPixelCount - TP;

                    var TPFP = TP + FP;
                    var TPFN = TP + FN;
                    var TNFP = TN + FP;
                    var TNFN = TN + FN;

                    // compute MMC
                    if (TPFP == 0 || TPFN == 0 || TNFP == 0 || TNFN == 0) return 0;
                    //return (TP * TN - FP * FN) / Math.Sqrt((TP + FP) * (TP + FN) * (TN + FP) * (TN + FN));
                    var N = TN + TP + FN + FP;
                    var S = (double)(TP + FN) / N;
                    var P = (double)(TP + FP) / N;

                    //return ((TP * TN) - (FP * FN)) / Math.Sqrt(TPFP * TPFN * TNFP * TNFN); 

                    var StandardMMC = ((double)TP / N - S * P) / Math.Sqrt((P * S * (1 - S) * (1 - P)));
                    #endregion

                    #region MCC calculation for Inner
                    HTuple InnerTP = new HTuple(), InnerFN = new HTuple(), InnerFP = new HTuple(), InnerTN = new HTuple();
                    // InnerTP
                    HOperatorSet.Intersection(actual, Inner, out intersection);
                    HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
                    if (area.Length > 0)
                    {
                        InnerTP = area;
                    }
                    // InnerFN
                    HOperatorSet.Difference(reference, intersection, out difference);
                    HOperatorSet.AreaCenter(difference, out area, out rows, out columns);
                    if (area.Length > 0)
                    {
                        InnerFN = area;
                    }
                    // InnerTN
                    InnerTN = ReferenceSet[imageIndex].PixelCount - (actualPixelCount + referencePixelCount - InnerTP); // TP == intersection
                    // InnerFP
                    InnerFP = actualPixelCount - InnerTP;

                    TP = TP + InnerTP;
                    TN = TN + InnerTN;
                    FP = FP + InnerFP * InnerPenaltyFactor;
                    FN = FN + InnerFN * InnerPenaltyFactor;

                    var InnerMMC = (TP * TN - FP * FN) / Math.Sqrt((TP + FP) * (TP + FN) * (TN + FP) * (TN + FN));
                    #endregion

                    #region MMC calculation for Mid
                    HTuple MidTP = new HTuple(), MidFN = new HTuple(), MidFP = new HTuple(), MidTN = new HTuple();
                    // MidTP
                    HOperatorSet.Intersection(actual, Inner, out intersection);
                    HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
                    if (area.Length > 0)
                    {
                        MidTP = area;
                    }
                    // MidFN
                    HOperatorSet.Difference(reference, intersection, out difference);
                    HOperatorSet.AreaCenter(difference, out area, out rows, out columns);
                    if (area.Length > 0)
                    {
                        MidFN = area;
                    }
                    // MidTN
                    MidTN = ReferenceSet[imageIndex].PixelCount - (actualPixelCount + referencePixelCount - MidTP); // TP == intersection
                    // MidFP
                    MidFP = actualPixelCount - MidTP;

                    TP = TP + MidTP;
                    TN = TN + MidTN;
                    FP = FP + MidFP * MidPenaltyFactor;
                    FN = FN + MidFN * MidPenaltyFactor;

                    var MidMMC = (TP * TN - FP * FN) / Math.Sqrt((TP + FP) * (TP + FN) * (TN + FP) * (TN + FN));
                    #endregion

                    #region MMC calculation for Outer
                    HTuple OuterTP = new HTuple(), OuterFN = new HTuple(), OuterFP = new HTuple(), OuterTN = new HTuple();
                    // MidTP
                    HOperatorSet.Intersection(actual, Inner, out intersection);
                    HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
                    if (area.Length > 0)
                    {
                        OuterTP = area;
                    }
                    // MidFN
                    HOperatorSet.Difference(reference, intersection, out difference);
                    HOperatorSet.AreaCenter(difference, out area, out rows, out columns);
                    if (area.Length > 0)
                    {
                        OuterFN = area;
                    }
                    // MidTN
                    OuterTN = ReferenceSet[imageIndex].PixelCount - (actualPixelCount + referencePixelCount - OuterTP); // TP == intersection
                    // MidFP
                    OuterFP = actualPixelCount - OuterTP;

                    TP = TP + OuterTP;
                    TN = TN + OuterTN;
                    FP = FP + OuterFP * OuterPenaltyFactor;
                    FN = FN + OuterFN * OuterPenaltyFactor;

                    var OuterMMC = (TP * TN - FP * FN) / Math.Sqrt((TP + FP) * (TP + FN) * (TN + FP) * (TN + FN));
                    #endregion

                    return StandardMMC;
                }
        */

        /// <summary>
        /// FP in close proximity to the reference regions are not penalized as much. 
        /// This results in a higher fitness if areas close to the reference region are considered positives by the algorithm.
        /// Background: Non perfect marking of the references. 
        /// </summary>
        protected double ComputeMMC_with_savezones(HObject[] hobjects, ReferenceImage img)
        {
            try
            {
                HTuple rows, columns, area;
                long TP = 0, TN = 0, FP = 0, FN = 0, areaHelper = 0;
                HObject difference; // == false positives (for precision), false negatives (for recall)
                HObject intersection; // == true positives
                                      // compute intersection

                HObject reference = hobjects[1];
                HObject actual = hobjects[0];
                HOperatorSet.Union1(reference, out reference);
                HOperatorSet.Union1(actual, out actual);
                // true positives: intersection(0, 1)
                // false positives difference(1,intersection(0,1)) -- see def. HOperatorSet difference

                //Inner/Mid/Outer are the edge parts of the reference region
                HObject Inner, Mid, Outer, FalsePositivesRegion;
                //HTuple innerArea, midArea, outerArea;
                double InnerPenaltyFactor = 0.2, MidPenaltyFactor = 0.5, OuterPenaltyFactor = 0.9;

                //enlarge the regions to represent a "save-band" around the regions which do not suffer a great fitness penalty      
                HOperatorSet.DilationCircle(reference, out Inner, 1);
                HOperatorSet.Difference(Inner, reference, out Inner);
                //HOperatorSet.AreaCenter(Inner, out innerArea, out rows, out columns);

                HOperatorSet.DilationCircle(reference, out Mid, 2);
                HOperatorSet.Difference(Mid, Inner, out Mid);
                //HOperatorSet.AreaCenter(Mid, out midArea, out rows, out columns);

                HOperatorSet.DilationCircle(reference, out Outer, 3);
                HOperatorSet.Difference(Outer, Mid, out Outer);
                //HOperatorSet.AreaCenter(Outer, out outerArea, out rows, out columns);

                //long InnerArea = innerArea;
                //long MidArea = midArea;
                //long OuterArea = outerArea;

                // actual and reference pixel counts
                HOperatorSet.AreaCenter(actual, out area, out rows, out columns);
                long actualPixelCount = 0;
                if (area.Length > 0)
                    actualPixelCount = area;
                HOperatorSet.AreaCenter(reference, out area, out rows, out columns);
                long referencePixelCount = 0;
                if (area.Length > 0)
                    referencePixelCount = area;

                #region Standard MCC calculation
                // compute true positives (TP)
                HOperatorSet.Intersection(actual, reference, out intersection);
                HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
                if (area.Length > 0)
                    TP = area;

                // compute false negatives (FN)
                HOperatorSet.Difference(reference, intersection, out difference);
                HOperatorSet.AreaCenter(difference, out area, out rows, out columns);
                if (area.Length > 0)
                    FN = area;

                // compute true negatives
                TN = img.PixelCount - (actualPixelCount + referencePixelCount - TP); // TP == intersection

                // compute false positives
                FP = actualPixelCount - TP;
                #endregion


                // difference acutal reference != difference reference actual!
                HOperatorSet.Difference(actual, reference, out FalsePositivesRegion);
                HOperatorSet.AreaCenter(FalsePositivesRegion, out area, out rows, out columns);
                var test = area;    //FP and test should be equal then!!!!!

                #region Inner FP reduction calculation
                HOperatorSet.Intersection(Inner, FalsePositivesRegion, out Inner);
                HOperatorSet.AreaCenter(Inner, out area, out rows, out columns);
                if (area.Length > 0)
                    areaHelper = area;
                long FPReductionInner = (long)(InnerPenaltyFactor * areaHelper);
                #endregion

                #region Mid FP reduction calculation
                HOperatorSet.Intersection(Mid, FalsePositivesRegion, out Mid);
                HOperatorSet.AreaCenter(Mid, out area, out rows, out columns);
                if (area.Length > 0)
                    areaHelper = area;
                long FPReductionMid = (long)(MidPenaltyFactor * areaHelper);
                #endregion

                #region Outer FP reduction calculation
                HOperatorSet.Intersection(Outer, FalsePositivesRegion, out Outer);
                HOperatorSet.AreaCenter(Outer, out area, out rows, out columns);
                if (area.Length > 0)
                    areaHelper = area;
                long FPReductionOuter = (long)(OuterPenaltyFactor * areaHelper);
                #endregion

                var test2 = FP;     //FP old
                FP = FP - FPReductionInner - FPReductionMid - FPReductionOuter;

                #region older version MCC calculations
                /*
                #region MCC calculation for Inner
                //  HTuple InnerTP = new HTuple(), InnerFN = new HTuple(), InnerFP = new HTuple(), InnerTN = new HTuple();
                long InnerTP = 0, InnerFN = 0, InnerFP = 0, InnerTN = 0;

                // InnerTP
                HOperatorSet.Intersection(actual, Inner, out intersection);
                HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
                if (area.Length > 0)
                {
                    InnerTP = area;
                }
                // InnerFN
                HOperatorSet.Difference(reference, intersection, out difference);
                HOperatorSet.AreaCenter(difference, out area, out rows, out columns);
                if (area.Length > 0)
                {
                    InnerFN = area;
                }
                // InnerTN
                InnerTN = ReferenceSet[imageIndex].PixelCount - (InnerArea + referencePixelCount - InnerTP); // TP == intersection
                                                                                                             // InnerFP
                InnerFP = InnerArea - InnerTP;
                #endregion


                #region MCC calculation for Mid
                // HTuple MidTP = new HTuple(), MidFN = new HTuple(), MidFP = new HTuple(), MidTN = new HTuple();
                long MidTP = 0, MidFN = 0, MidFP = 0, MidTN = 0;

                // MidTP
                HOperatorSet.Intersection(actual, Mid, out intersection);
                HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
                if (area.Length > 0)
                {
                    MidTP = area;
                }
                // MidFN
                HOperatorSet.Difference(reference, intersection, out difference);
                HOperatorSet.AreaCenter(difference, out area, out rows, out columns);
                if (area.Length > 0)
                {
                    MidFN = area;
                }
                // MidTN
                MidTN = ReferenceSet[imageIndex].PixelCount - (MidArea + referencePixelCount - MidTP); // TP == intersection
                                                                                                       // MidFP
                MidFP = MidArea - MidTP;
                #endregion


                #region MCC calculation for Outer
                //  HTuple OuterTP = new HTuple(), OuterFN = new HTuple(), OuterFP = new HTuple(), OuterTN = new HTuple();
                long OuterTP = 0, OuterFN = 0, OuterFP = 0, OuterTN = 0;

                // OuterTP
                HOperatorSet.Intersection(actual, Outer, out intersection);
                HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
                if (area.Length > 0)
                {
                    OuterTP = area;
                }
                // OuterFN
                HOperatorSet.Difference(reference, intersection, out difference);
                HOperatorSet.AreaCenter(difference, out area, out rows, out columns);
                if (area.Length > 0)
                {
                    OuterFN = area;
                }
                // OuterTN
                OuterTN = ReferenceSet[imageIndex].PixelCount - (OuterArea + referencePixelCount - OuterTP); // TP == intersection
                                                                                                             // MidFP
                OuterFP = OuterArea - OuterTP;
                #endregion
              

                try
                {
                    TP = TP + InnerTP + MidTP + OuterTP;
                    TN = TN + InnerTN + MidTN + OuterTN;
                    FP = (long)(FP + InnerFP * InnerPenaltyFactor + MidFP * MidPenaltyFactor + OuterFP * OuterPenaltyFactor);
                    FN = (long)(FN + InnerFN * InnerPenaltyFactor + MidFN * MidPenaltyFactor + OuterFN * OuterPenaltyFactor);
                }
                catch (Exception e)
                {
                    throw e;
                }
                var TPFP = TP + FP;
                var TPFN = TP + FN;
                var TNFP = TN + FP;
                var TNFN = TN + FN;
                // condition for abortion of the calculation process (divison of 0)
                if (TPFP == 0 || TPFN == 0 || TNFP == 0 || TNFN == 0) return 0;
                var CustomMCCResult = (TP * TN - FP * FN) / (Math.Sqrt(TPFP) * Math.Sqrt(TPFN) * Math.Sqrt(TNFP) * Math.Sqrt(TNFN));                 //overflow of datatype when not separating the sqrt's
                // return (TP * TN - FP * FN) / Math.Sqrt((TP + FP) * (TP + FN) * (TN + FP) * (TN + FN));
                return CustomMCCResult;

                  */
                #endregion

                var TPFP = TP + FP;
                var TPFN = TP + FN;
                var TNFP = TN + FP;
                var TNFN = TN + FN;
                // condition for abortion of the calculation process (divison of 0)
                if (TPFP == 0 || TPFN == 0 || TNFP == 0 || TNFN == 0) return 0;
                var CustomMCCResult = (TP * TN - FP * FN) / (Math.Sqrt(TPFP) * Math.Sqrt(TPFN) * Math.Sqrt(TNFP) * Math.Sqrt(TNFN));                 //overflow of datatype when not separating the sqrt's
                // return (TP * TN - FP * FN) / Math.Sqrt((TP + FP) * (TP + FN) * (TN + FP) * (TN + FN));
                return CustomMCCResult;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Areas close to Reference regions are not penalized as much. This means that the TP and TN count like usual but FN and FP are treated more lightly at close proximity.
        /// Computes several MCC's and returns the maximum value received. These MCC's are the standard one and three others, where the region of the actual region is enlarged.
        /// </summary>
        protected double ComputeMMC_maximumOfSavezones(HObject[] hobjects, ReferenceImage img)
        {
            try
            {
                HTuple rows, columns, area;
                long TP = 0, TN = 0, FP = 0, FN = 0;
                HObject difference; // == false positives (for precision), false negatives (for recall)
                HObject intersection; // == true positives
                                      // compute intersection

                HObject reference = hobjects[1];
                HObject actual = hobjects[0];
                HOperatorSet.Union1(reference, out reference);
                HOperatorSet.Union1(actual, out actual);
                // true positives: intersection(0, 1)
                // false positives difference(1,intersection(0,1)) -- see def. HOperatorSet difference

                //Inner/Mid/Outer are the edge parts of the region
                HObject Inner, Mid, Outer;
                HTuple innerArea, midArea, outerArea;
                double InnerPenaltyFactor = 0.2, MidPenaltyFactor = 0.5, OuterPenaltyFactor = 0.9;

                //enlarge the regions to represent a "save-band" around the regions which do not suffer a great fitness penalty      
                HOperatorSet.DilationCircle(actual, out Inner, 1.5);
                //HOperatorSet.Difference(Inner, reference, out Inner);
                HOperatorSet.AreaCenter(Inner, out innerArea, out rows, out columns);

                HOperatorSet.DilationCircle(actual, out Mid, 2.5);
                //HOperatorSet.Difference(Mid, Inner, out Mid);
                HOperatorSet.AreaCenter(Mid, out midArea, out rows, out columns);

                HOperatorSet.DilationCircle(actual, out Outer, 3.25);
                //HOperatorSet.Difference(Outer, Mid, out Outer);
                HOperatorSet.AreaCenter(Outer, out outerArea, out rows, out columns);

                long InnerArea;
                long MidArea;
                long OuterArea;

                if (innerArea.Length == 0 || midArea.Length == 0 || outerArea.Length == 0)
                {
                    InnerArea = 0;
                    MidArea = 0;
                    OuterArea = 0;
                }
                else
                {
                    InnerArea = innerArea;
                    MidArea = midArea;
                    OuterArea = outerArea;
                }

                // actual and reference pixel counts
                HOperatorSet.AreaCenter(actual, out area, out rows, out columns);
                long actualPixelCount = 0;
                if (area.Length > 0)
                    actualPixelCount = area;

                HOperatorSet.AreaCenter(reference, out area, out rows, out columns);
                long referencePixelCount = 0;
                if (area.Length > 0)
                    referencePixelCount = area;

                #region Standard MCC calculation
                // compute true positives (TP)
                HOperatorSet.Intersection(actual, reference, out intersection);
                HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
                if (area.Length > 0)
                    TP = area;

                // compute false negatives (FN)
                HOperatorSet.Difference(reference, intersection, out difference);
                HOperatorSet.AreaCenter(difference, out area, out rows, out columns);
                if (area.Length > 0)
                    FN = area;

                // compute true negatives
                TN = img.PixelCount - (actualPixelCount + referencePixelCount - TP); // TP == intersection

                // compute false positives
                //HOperatorSet.AreaCenter(actual, out area, out rows, out columns);
                //if (area.Length > 0)
                //   FP = area - TP;
                FP = actualPixelCount - TP;
                #endregion


                #region MCC calculation for Inner
                //  HTuple InnerTP = new HTuple(), InnerFN = new HTuple(), InnerFP = new HTuple(), InnerTN = new HTuple();
                long InnerTP = 0, InnerFN = 0, InnerFP = 0, InnerTN = 0;
                //debug
                long InnerFPEnhanced = 0, InnerTNEnhanced = 0;

                // InnerTP
                HOperatorSet.Intersection(reference, Inner, out intersection);
                HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
                if (area.Length > 0)
                {
                    InnerTP = area;
                }
                // InnerFN
                HOperatorSet.Difference(reference, intersection, out difference);
                HOperatorSet.AreaCenter(difference, out area, out rows, out columns);
                if (area.Length > 0)
                {
                    InnerFN = area;
                }
                // InnerTN
                InnerTN = img.PixelCount - (InnerArea + referencePixelCount - InnerTP); // TP == intersection
                InnerTNEnhanced = (long)(InnerTN + InnerPenaltyFactor * (Math.Abs(TN - InnerTN)));

                // InnerFP
                InnerFP = InnerArea - InnerTP;
                InnerFPEnhanced = (long)(FP + InnerPenaltyFactor * (Math.Abs(InnerFP - FP)));
                #endregion


                #region MCC calculation for Mid
                // HTuple MidTP = new HTuple(), MidFN = new HTuple(), MidFP = new HTuple(), MidTN = new HTuple();
                long MidTP = 0, MidFN = 0, MidFP = 0, MidTN = 0;
                long MidFPEnhanced = 0, MidTNEnhanced = 0;

                // MidTP
                HOperatorSet.Intersection(reference, Inner, out intersection);
                HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
                if (area.Length > 0)
                {
                    MidTP = area;
                }
                // MidFN
                HOperatorSet.Difference(reference, intersection, out difference);
                HOperatorSet.AreaCenter(difference, out area, out rows, out columns);
                if (area.Length > 0)
                {
                    MidFN = area;
                }
                // MidTN
                MidTN = img.PixelCount - (MidArea + referencePixelCount - MidTP); // TP == intersection
                MidTNEnhanced = (long)(MidTN + MidPenaltyFactor * (Math.Abs(TN - MidTN)));

                // MidFP
                MidFP = MidArea - MidTP;
                MidFPEnhanced = (long)(FP + MidPenaltyFactor * (Math.Abs(MidFP - FP)));
                #endregion


                #region MCC calculation for Outer
                //  HTuple OuterTP = new HTuple(), OuterFN = new HTuple(), OuterFP = new HTuple(), OuterTN = new HTuple();
                long OuterTP = 0, OuterFN = 0, OuterFP = 0, OuterTN = 0;
                long OuterFPEnhanced = 0, OuterTNEnhanced = 0;

                // OuterTP
                HOperatorSet.Intersection(reference, Outer, out intersection);
                HOperatorSet.AreaCenter(intersection, out area, out rows, out columns);
                if (area.Length > 0)
                {
                    OuterTP = area;
                }
                // OuterFN
                HOperatorSet.Difference(reference, intersection, out difference);
                HOperatorSet.AreaCenter(difference, out area, out rows, out columns);
                if (area.Length > 0)
                {
                    OuterFN = area;
                }
                // OuterTN
                OuterTN = img.PixelCount - (OuterArea + referencePixelCount - OuterTP); // TP == intersection
                OuterTNEnhanced = (long)(OuterTN + OuterPenaltyFactor * (Math.Abs(TN - OuterTN)));

                // OuterFP
                OuterFP = OuterArea - OuterTP;
                OuterFPEnhanced = (long)(FP + OuterPenaltyFactor * (Math.Abs(OuterFP - FP)));
                #endregion

                double CustomMCCResult;
                #region calculateMCCs
                try
                {
                    var InnerTPFP = InnerTP + InnerFP;
                    var InnerTPFN = InnerTP + InnerFN;
                    var InnerTNFP = InnerTN + InnerFP;
                    var InnerTNFN = InnerTN + InnerFN;

                    var InnerTPFPEnh = InnerTP + InnerFPEnhanced;
                    var InnerTPFNEnh = InnerTP + InnerFN;
                    var InnerTNFPEnh = InnerTNEnhanced + InnerFPEnhanced;
                    var InnerTNFNEnh = InnerTNEnhanced + InnerFN;

                    var MidTPFP = MidTP + MidFP;
                    var MidTPFN = MidTP + MidFN;
                    var MidTNFP = MidTN + MidFP;
                    var MidTNFN = MidTN + MidFN;

                    var MidTPFPEnh = MidTP + MidFPEnhanced;
                    var MidTPFNEnh = MidTP + MidFN;
                    var MidTNFPEnh = MidTNEnhanced + MidFPEnhanced;
                    var MidTNFNEnh = MidTNEnhanced + MidFN;


                    var OuterTPFP = OuterTP + OuterFP;
                    var OuterTPFN = OuterTP + OuterFN;
                    var OuterTNFP = OuterTN + OuterFP;
                    var OuterTNFN = OuterTN + OuterFN;

                    var OuterTPFPEnh = OuterTP + OuterFPEnhanced;
                    var OuterTPFNEnh = OuterTP + OuterFN;
                    var OuterTNFPEnh = OuterTNEnhanced + OuterFPEnhanced;
                    var OuterTNFNEnh = OuterTNEnhanced + OuterFN;


                    var TPFP = TP + FP;
                    var TPFN = TP + FN;
                    var TNFP = TN + FP;
                    var TNFN = TN + FN;

                    // compute MMC
                    if (TPFP == 0 || TPFN == 0 || TNFP == 0 || TNFN == 0) return 0;
                    //return (TP * TN - FP * FN) / Math.Sqrt((TP + FP) * (TP + FN) * (TN + FP) * (TN + FN));
                    long N = TN + TP + FN + FP;
                    var S = (double)(TP + FN) / N;
                    var P = (double)(TP + FP) / N;

                    //return ((TP * TN) - (FP * FN)) / Math.Sqrt(TPFP * TPFN * TNFP * TNFN); 


                    // condition for abortion of the calculation process (divison of 0)
                    if (InnerTPFP == 0 || InnerTPFN == 0 || InnerTNFP == 0 || InnerTNFN == 0) return 0;
                    var InnerMCC = (InnerTP * InnerTN - InnerFP * InnerFN) / (Math.Sqrt(InnerTPFP) * Math.Sqrt(InnerTPFN) * Math.Sqrt(InnerTNFP) * Math.Sqrt(InnerTNFN));

                    // condition for abortion of the calculation process (divison of 0)
                    if (InnerTPFP == 0 || InnerTPFN == 0 || InnerTNFP == 0 || InnerTNFN == 0) return 0;
                    var InnerMCCEnhanced = (InnerTP * InnerTNEnhanced - InnerFPEnhanced * InnerFN) / (Math.Sqrt(InnerTPFPEnh) * Math.Sqrt(InnerTPFNEnh) * Math.Sqrt(InnerTNFPEnh) * Math.Sqrt(InnerTNFNEnh));

                    // condition for abortion of the calculation process (divison of 0)
                    if (MidTPFP == 0 || MidTPFN == 0 || MidTNFP == 0 || MidTNFN == 0) return 0;
                    var MidMCC = (MidTP * MidTN - MidFP * MidFN) / (Math.Sqrt(MidTPFP) * Math.Sqrt(MidTPFN) * Math.Sqrt(MidTNFP) * Math.Sqrt(MidTNFN));
                    // condition for abortion of the calculation process (divison of 0)
                    if (MidTPFP == 0 || MidTPFN == 0 || MidTNFP == 0 || MidTNFN == 0) return 0;
                    var MidMCCEnhanced = (MidTP * MidTNEnhanced - MidFPEnhanced * MidFN) / (Math.Sqrt(MidTPFPEnh) * Math.Sqrt(MidTPFNEnh) * Math.Sqrt(MidTNFPEnh) * Math.Sqrt(MidTNFNEnh));
                    // condition for abortion of the calculation process (divison of 0)
                    if (OuterTPFP == 0 || OuterTPFN == 0 || OuterTNFP == 0 || OuterTNFN == 0) return 0;
                    var OuterMCC = (OuterTP * OuterTN - OuterFP * OuterFN) / (Math.Sqrt(OuterTPFP) * Math.Sqrt(OuterTPFN) * Math.Sqrt(OuterTNFP) * Math.Sqrt(OuterTNFN));
                    // condition for abortion of the calculation process (divison of 0)
                    if (OuterTPFP == 0 || OuterTPFN == 0 || OuterTNFP == 0 || OuterTNFN == 0) return 0;
                    var OuterMCCEnhanced = (OuterTP * OuterTNEnhanced - OuterFPEnhanced * OuterFN) / (Math.Sqrt(OuterTPFPEnh) * Math.Sqrt(OuterTPFNEnh) * Math.Sqrt(OuterTNFPEnh) * Math.Sqrt(OuterTNFNEnh));

                    CustomMCCResult = ((double)TP / N - S * P) / Math.Sqrt((P * S * (1 - S) * (1 - P)));

                    var max = new[] { InnerMCCEnhanced, MidMCCEnhanced, OuterMCCEnhanced, CustomMCCResult }.Max();
                    CustomMCCResult = max;
                }
                catch (Exception)
                {
                    throw;
                }
                #endregion


                try
                {
                    TP = TP + InnerTP + MidTP + OuterTP;
                    TN = TN + InnerTN + MidTN + OuterTN;
                    FP = (long)(FP + InnerFP * InnerPenaltyFactor + MidFP * MidPenaltyFactor + OuterFP * OuterPenaltyFactor);
                    FN = (long)(FN + InnerFN * InnerPenaltyFactor + MidFN * MidPenaltyFactor + OuterFN * OuterPenaltyFactor);
                }
                catch (Exception)
                {
                    throw;
                }
                /*         var TPFP = TP + FP;
                         var TPFN = TP + FN;
                         var TNFP = TN + FP;
                         var TNFN = TN + FN;
                         // condition for abortion of the calculation process (divison of 0)
                         if (TPFP == 0 || TPFN == 0 || TNFP == 0 || TNFN == 0) return 0;
                         var CustomMCCResult = (TP * TN - FP * FN) / (Math.Sqrt(TPFP) * Math.Sqrt(TPFN) * Math.Sqrt(TNFP) * Math.Sqrt(TNFN));                 //overflow of datatype when not separating the sqrt's
                         // return (TP * TN - FP * FN) / Math.Sqrt((TP + FP) * (TP + FN) * (TN + FP) * (TN + FN));   */
                return CustomMCCResult;
            }
            catch (Exception)
            {
                throw;
            }
        }



        /// <summary>
        /// matthews correlation coefficient requires the fitness parameters to hold an image's pixel count
        /// </summary>
        protected double ComputeMCC(HObject[] hobjects, ReferenceImage img)
        {
            var mcc = Fitness.Collection.MCC(hobjects[1], hobjects[0], img.Height, img.Width);
            return mcc;
        }



        protected double ComputeJ(HObject[] hobjects, ReferenceImage img)
        {
            var recall = ComputeRecall(hobjects, img);
            HTuple area, row, column;
            HObject actual = hobjects[0];
            HObject reference = hobjects[1];
            HObject intersection;
            // actual and reference pixel counts
            HOperatorSet.AreaCenter(actual, out area, out row, out column);
            long actualPixelCount = 0;
            if (area.Length > 0)
                actualPixelCount = area;
            HOperatorSet.AreaCenter(reference, out area, out row, out column);
            long referencePixelCount = 0;
            if (area.Length > 0)
                referencePixelCount = area;


            // compute true positives (TP)
            double TP = 0;
            HOperatorSet.Intersection(actual, reference, out intersection);
            HOperatorSet.AreaCenter(intersection, out area, out row, out column);
            if (area.Length > 0)
                TP = area;

            // compute true negatives

            double TN = img.PixelCount - (actualPixelCount + referencePixelCount - TP); // TP == intersection

            // compute FP

            double FP = actualPixelCount - TP;


            return recall + TN / (TN + FP) - 1;
        }


        #endregion

        protected static double Positives(HObject actual, HObject reference)
        {
            HTuple area, row, column;
            HOperatorSet.AreaCenter(actual, out area, out row, out column);
            if (area.Length > 0)
                return area;
            return 0;
        }

        public override ICopyable Copy()
        {
            return new FitnessEvaluator((HalconFitnessConfiguration) FitnessConfiguration, CGPConfiguration, Decoder, Builder);
        }
    }
}
