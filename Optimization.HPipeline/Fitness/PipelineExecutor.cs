using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HalconDotNet;
using Optimization.Fitness.ErrorHandling;

namespace Optimization.HPipeline.Fitness
{
    public class PipelineExecutor
    {
        /// <summary>
        /// Constructor to set a number of different thresholds that the evolved pipeline must obey.
        /// </summary>
        /// <param name="executionTimeThreshold"></param>
        /// <param name="regionsCountThreshold"></param>
        /// <param name="percentageOfPixelsThreshold"></param>
        public PipelineExecutor(long? executionTimeThreshold = null, int? regionsCountThreshold = null, double? percentageOfPixelsThreshold = null)
        {
            if (executionTimeThreshold != null)
            {
                ExecutionTimeThreshold = (long)executionTimeThreshold;
                UseExecutionTimeThreshold = true;
            }
            if (regionsCountThreshold != null)
            {
                RegionsCountThreshold = (int)regionsCountThreshold;
                UseRegionsCountThreshold = true;
            }
            if (percentageOfPixelsThreshold != null)
            {
                PercentageOfPixelsThreshold = (double)percentageOfPixelsThreshold;
                UsePercentageOfPixelsThreshold = true;
            }
        }

        public int InvalidIndividualsEncountered = 0;

        public long ExecutionTimeThreshold { get; private set; }
        public double PercentageOfPixelsThreshold { get; private set; }
        public int RegionsCountThreshold { get; private set; }
        public bool UseExecutionTimeThreshold = false;
        public bool UsePercentageOfPixelsThreshold = false;
        public bool UseRegionsCountThreshold = false;

        /// <summary>
        /// Executes the operators of the pipeline and returns the final output.
        /// </summary>
        /// <param name="objects">Input image(s)</param>
        /// <param name="parameterArray">Stores the parameters for each operator (only for the active nodes!) identified by the nodenumber (float)</param>
        /// <param name="actionArray">Stores the execution function of each operator (only for the active nodes!) identified by the nodenumber (float)</param>
        /// <param name="executionTree">Contains the inputs for the active nodes (by nodenumber)</param>
        /// <param name="outputNodes">Nodes representing outputs aka the nodenumber of the last operator</param>
        /// <param name="imagePixelCount">Number of pixels of the input image</param>
        /// <param name="type">Type of the exception</param>
        /// <param name="columnNodeMap">Stores the nodenumbers of the active nodes for each column</param>
        /// <returns>Output (the actual region(s)) of the executed pipeline</returns>
        public HObject[] IterateColumns2(HObject[] objects, Dictionary<float, HTuple[]> parameterArray, Dictionary<float, Func<HObject[], HTuple[], HObject[]>> actionArray,
            Dictionary<float, List<float>> executionTree, List<float> outputNodes, double imagePixelCount, ExcessRegionHandling type, Dictionary<int, List<float>> columnNodeMap)
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

                        if (executionTree[current].Where(x => x >= 0).Count() == 0) // if leaf node
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
                            if (UseExecutionTimeThreshold && watch.ElapsedMilliseconds > ExecutionTimeThreshold)
                            {
                                goto InvalidIndividual;
                            }
                            HTuple number;
                            if (UseRegionsCountThreshold)
                            {
                                HOperatorSet.CountObj(outputs[current][0], out number);
                                if (number > RegionsCountThreshold)
                                {
                                    if (type == ExcessRegionHandling.ThrowException)
                                    {
                                        goto InvalidIndividual;
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

                                var count = area.TupleSum() / imagePixelCount;

                                if (count > PercentageOfPixelsThreshold)
                                {
                                    goto InvalidIndividual;
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
                //throw new UnexpectedException(e.GetType().ToString() + " " + e.Message + " current: " + current, parameterArray, executionTree, current, columnNodeMap);
                throw e;        
            
            }


            // free memory
            var ret = new HObject[outputs[outputNodes.First()].Length];
            for (int i = 0; i < outputs[outputNodes.First()].Length; i++)  // there only is one output node
            {
                if (outputs[outputNodes.First()][i] != null) ret[i] = outputs[outputNodes.First()][i].Clone();
            }

            Dispose(outputs);

            return ret;

        InvalidIndividual:
            InvalidIndividualsEncountered++;
            Dispose(outputs);
            return null;
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

            //GC.Collect();
            //GC.WaitForPendingFinalizers();
        }


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
    }
}
