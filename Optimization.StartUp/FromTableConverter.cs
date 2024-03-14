using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using Extensions;
using Optimization.CartesianGeneticProgramming;
using Optimization.CartesianGeneticProgramming.Interfaces;
using Optimization.CVPipeline;
using Optimization.CVPipeline.CVCGP;
using Optimization.EvolutionStrategy;
using Optimization.HPipeline;
using Optimization.HPipeline.Fitness;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.Serialization;
using Optimization.Serialization.Interfaces;

namespace Optimization.StartUp
{
    public class FromTableConverter
    {
        
        public static CGPConfiguration CGPConfiguration(DataColumnCollection cgpColumn, DataRow cgpRow)
        {
            var rows = int.Parse(cgpRow[cgpColumn.IndexOfColumnName("rows")].ToString());
            var columns = int.Parse(cgpRow[cgpColumn.IndexOfColumnName("columns")].ToString());
            var levelsBack =  int.Parse(cgpRow[cgpColumn.IndexOfColumnName("levels_back")].ToString());
            var operatorSet = cgpRow[cgpColumn.IndexOfColumnName("operator_set")].ToString();
            int maxNumParameters = 3; // only uses default for fioperatormap, rest is initialized differently
            IOperatorMap operatorMap = null;
            if (operatorSet.Equals("ALL"))
            {
                var fastAll = CommonHalconPipelines.FastHalconOperatorNodes; // only removes a few that are known to cause issues
                operatorMap = new HalconOperatorMap(fastAll);
                maxNumParameters = fastAll.Max(x => x.CGPParameterCount);
            }
            else if (operatorSet.Equals("OpenCV"))
            {
                operatorMap = new CVOperatorMap(CommonCVPipelines.NodeCollection);
                maxNumParameters = CommonCVPipelines.NodeCollection.Max(x => x.CGPParameterCount);
            }
            else if (CommonHalconPipelines.HalconPipelineDictionary.ContainsKey(operatorSet))
            {
                var collection = CommonHalconPipelines.HalconPipelineDictionary[operatorSet].Nodes;
                operatorMap = new HalconOperatorMap(collection);
                maxNumParameters = collection.Max(x => x.CGPParameterCount);
            }
            else
                throw new NotSupportedException(operatorSet + " not currently supported");
            int operatorInputCount = 0;
            if (operatorMap.OperatorInputCount != null) operatorInputCount = operatorMap.OperatorInputCount.Values.Max();
            else operatorInputCount = 3; // for FiOperatorMap -- cannot compute by using max in this case
            var config = new CGPConfiguration(rows, columns, levelsBack, operatorInputCount, maxNumParameters , operatorMap, 1, 1);
            return config;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="column">result of batch run query</param>
        /// <param name="row">result of batch run query</param>
        /// <param name="config"></param>
        /// <param name="worker"></param>
        /// <returns></returns>
        public static BatchRun BatchRun(DataColumnCollection column, DataRow row,  CGPConfiguration config, IConnector connector, BackgroundWorker worker = null)
        {
            var batchID = row[column.IndexOfColumnName("id")].ToString();
            var generations = int.Parse(row[column.IndexOfColumnName("max_generations")].ToString());
            var iterations = int.Parse(row[column.IndexOfColumnName("iterations")].ToString());
            var seed = int.Parse(row[column.IndexOfColumnName("seed")].ToString());
            var type = row[column.IndexOfColumnName("evolution_strategy_type")].ToString();

            var saveDir = Path.Combine(Configuration.DjangoSavePath, batchID);

            if((config.OperatorMap as CVOperatorMap) != null)
            {
                var train = FromTableConverter.CVReferenceSet(column, row, "train", connector);
                var val = FromTableConverter.CVReferenceSet(column, row, "val", connector);
                var trainDL = new CVDataLoader(train);
                var valDL = new CVDataLoader(val);
                return CommonCVEvolutionStrategies.BuildStandardCVEvolutionStrategy(trainDL, valDL, config, generations, seed, iterations, saveDir, worker);
            }

            var refSet = ReferenceSet(column, row, "train", connector);
            var refSetVal = ReferenceSet(column, row, "val", connector);
            if(type.Equals("SelfAdaptive"))
                return CommonHalconEvolutionStrategies.SelfAdaptiveEvolutionStrategy(config, refSet, refSetVal, generations, iterations, saveDir, seed);
            else
                return CommonHalconEvolutionStrategies.BuildStandardCGPEvolutionStrategy(config, refSet, refSetVal, generations, iterations: iterations
                    , saveDirectory: saveDir, seed: seed);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="column">result of batch run query</param>
        /// <param name="row">result of batch run query</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ReferenceSet ReferenceSet(DataColumnCollection column, DataRow row, string type = "train")
        {
            var trainDataID = row[column.IndexOfColumnName(type + "_batch_data_id")].ToString();
            var refSet = new ReferenceSet(Path.Combine(Configuration.DjangoDataPath, trainDataID));
            return refSet;
        }

        public static ReferenceSet ReferenceSet(DataColumnCollection column, DataRow row, string type, IConnector connector)
        {
            var trainDataID = row[column.IndexOfColumnName(type + "_batch_data_id")].ToString();
            var batchData = connector.selectQuery(SQLQueryStrings.BatchDataImageIDs(trainDataID));

            var tpl = GetImageLabelPaths(batchData, connector);

            return new FileListReferenceSet(Configuration.DjangoMediaPath, imagepaths: tpl.Item1, labelpaths: tpl.Item2);
        }

        private static Tuple<List<string>, Dictionary<string, IEnumerable<string>>> GetImageLabelPaths(DataTable batchData, IConnector connector)
        {
            var labelPaths = new Dictionary<string, IEnumerable<string>>();
            var imagePaths = new List<string>();

            foreach (DataRow bDataRow in batchData.Rows)
            {
                var imageID = bDataRow[batchData.Columns.IndexOfColumnName("image_id")];
                var regionData = connector.selectQuery(SQLQueryStrings.RegionsForImage(imageID.ToString()));
                var imageData = connector.selectQuery(SQLQueryStrings.Image(imageID.ToString()));
                var imagePath = imageData.Rows[0][imageData.Columns.IndexOfColumnName("image")].ToString();
                imagePaths.Add(imagePath);
                labelPaths.Add(imagePath, new List<string>());
                foreach (DataRow r in regionData.Rows)
                {
                    ((List<string>)labelPaths[imagePath]).Add(r[regionData.Columns.IndexOfColumnName("file")].ToString());
                }
            }

            return new Tuple<List<string>, Dictionary<string, IEnumerable<string>>>(imagePaths, labelPaths);
        }

        public static CVReferenceSet CVReferenceSet(DataColumnCollection column, DataRow row, string type, IConnector connector)
        {
            var trainDataID = row[column.IndexOfColumnName(type + "_batch_data_id")].ToString();
            var batchData = connector.selectQuery(SQLQueryStrings.BatchDataImageIDs(trainDataID));
            var tpl = GetImageLabelPaths(batchData, connector);
            var labelPaths = new List<string>();
            if (tpl.Item2.Any(x => x.Value.Count() != 1)) throw new NotSupportedException("Must only use CV reference with one label for each image");
            foreach (var imgPath in tpl.Item1) labelPaths.Add(tpl.Item2[imgPath].First());

            var refSet = new CVReferenceSet(basePath: Configuration.DjangoMediaPath, imagepaths: tpl.Item1, labelPaths: labelPaths, imgType: Emgu.CV.CvEnum.ImreadModes.Grayscale);
            return refSet;
        }
    }
}
