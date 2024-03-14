using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Optimization.CartesianGeneticProgramming;
using Optimization.CartesianGeneticProgramming.Interfaces;
using Optimization.Data;
using Optimization.EvolutionStrategy.Evaluators;
using Optimization.EvolutionStrategy.Interfaces;
using Optimization.Fitness;
using Optimization.Fitness.Interfaces;
using Optimization.Pipeline.Interfaces;
using Serilog;

namespace Optimization.CVPipeline.CVCGP
{
    public class CVCGPEvaluator : Evaluator<CVReferenceImage>
    {
        public CVCGPEvaluator(DataLoader<CVReferenceImage> trainDataLoader, DataLoader<CVReferenceImage> valDataLoader, ICGPDecoder decoder, CGPConfiguration configuration, FitnessConfiguration fitness)
            : base(trainDataLoader, valDataLoader, fitness)
        {
            Configuration = configuration;
            Decoder = decoder;
            OperatorDecoder = Configuration.OperatorMap as IOperatorEncoder;
            if (OperatorDecoder == null) throw new Exception();
            DecodingMap = configuration.OperatorMap as IDecodingMap<UMat[], float[], UMat>;
        }

        public IOperatorEncoder OperatorDecoder { get; set; }

        public ICGPDecoder Decoder { get; set; }

        public CGPConfiguration Configuration { get; private set; }

        protected IDecodingMap<UMat[], float[], UMat> DecodingMap { get; set; }

        public override ICopyable Copy()
        {
            return new CVCGPEvaluator(TrainDataLoader, ValidationDataLoader, Decoder, Configuration, FitnessConfiguration);
        }

        protected override void EvaluateBatch(IIndividual individual, List<CVReferenceImage> batch, params object[] additional)
        {
            var pipeline = new CVPipeline(individual.FloatVector, Configuration);

            base.EvaluateBatch(individual, batch, pipeline);
            var dispose = (bool)additional[0];

            if (dispose)
            {
                foreach (var item in batch)
                {
                    item.Image.GetInputArray().Dispose();
                    item.ReferenceImage.GetInputArray().Dispose();
                }
            }

            pipeline.ResetOutput();
        }

        protected override void EvaluateItem(IIndividual individual, CVReferenceImage item, params object[] additional)
        {
            var pipeline = additional[0] as CVPipeline;
            var fitness = FitnessConfiguration.FitnessFunctions.ToDictionary(x => x, x => (double?) 0.0);

            try
            {
                var output = pipeline.ExecuteSingle(item.Image);
                output = output.Binary();
                foreach(var fitFunc in FitnessConfiguration.FitnessFunctions)
                    fitness[fitFunc] += ComputeFitness(item.ReferenceImage, output, fitFunc);
            }
            catch (Exception ex)
            {
                /*
                Logger.PrintGrid(pipeline.ToCGPEncoding(Configuration.OperatorMap, Configuration),
                    Configuration, Logger.ExceptionGrids,
                    append:true, tag: null, printActiveNodes: true);*/
                Log.Error(ex, ex.Message);
            }
        }

        private double ComputeFitness(UMat reference, UMat actual, FitnessFunction fitFunc)
        {
            if(fitFunc == FitnessFunction.MCC)
            {
                return MCC(reference, actual);
            }

            throw new NotImplementedException("Unsupported Fitness: " + fitFunc.ToString());
        }

        public static double MCC(UMat reference, UMat actual)
        {
            //if (reference.Size.Height != actual.Size.Height || reference.Size.Width != actual.Size.Height)  -- moved to check this in CvNode
            //    CvInvoke.Resize(actual, actual, reference.Size, 0, 0, interpolation: Inter.Nearest);

            var tp = reference.TruePositives(actual);
            var tn = reference.TrueNegatives(actual);
            var fp = reference.FalsePositives(actual);
            var fn = reference.FalseNegatives(actual);
            return Collection.MCC(tp, tn, fp, fn);
        }
    }
}
