using System;

namespace Optimization.CVPipeline
{
    class Programm
    {

        //private static string DataSetPath = @"P:\80 Hiwis\Braml_Philipp\NittedCarbonFiber\CoronaII_Tubelight";

        static void Main()
        {
            Console.WriteLine("Welcome to the evaluationprogramm for the Prime.CVPipeline");
            Console.WriteLine("Please wait while images will be loaded");

            //Batch of images around 10-20
            //use CVReferenceSet(directory [DataSetPath]) for Images/Dataset
            //var dir = new CVReferenceSet(DataSetPath);
            //var train = new CVDataLoader(dir);

            //the following Code is already done for me in a ReferenceSet, except creating the directories??
            /*var labelDir = Path.Combine(directory, "labels");
            var imageDir = Path.Combine(directory, "images");
            var regionDir = Path.Combine(directory, "regions");
            labelDir.createDirectory();
            imageDir.createDirectory();
            regionDir.createDirectory();*/



            //Pipeline extract; from CommonCVPipeline
            //Need for a CvOperatorMap can be discussed further,because depending in the pipeline that will be used
            //it should be obious by name which operator map is going to be used
            //CVPipeline pipeline = CommonCVPipelines.SimplePipeline;

            //(possibly a CGPEvaluator)

            //(foreach(var pipeline in CommonCVPipeline.PipelineCollection)
            //for(... < num_img = Set.Count(); Or of reference set use the funtion CVRefernceImage(index) or really just count
            //result = pipeline.ExecuteSingle(img)
            // var dir = Path.Combine(CommonInformation.[TestResultDirectory]. " CvSimplePipelineOutput"
            // dir.createDirectory();
            // pipeline.WriteOutputs(img, dir); for this parameter maybe replace img with result
        }
    }
}
