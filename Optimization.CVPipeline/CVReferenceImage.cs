using System;
using Emgu.CV;
using Optimization.CVPipeline.CVCGP;
using Optimization.Fitness;
using Optimization.Fitness.Interfaces;

namespace Optimization.CVPipeline
{
    public class CVReferenceImage : IReference<UMat, UMat>
    {
        public CVReferenceImage(UMat image, UMat reference, string name)
        {
            Image = image;
            ReferenceImage = reference;
            Name = name;
            Width = image.Size.Width;
            Height = image.Size.Height;
        }

        public string Name { get; set; }

        public UMat Input
        {
            get
            {
                return Image;
            }
        }

        public UMat Image { get; set; }
        

        public UMat Reference
        {
           get { return ReferenceImage; }
        }

        public double PercentageOfPixels(UMat actual)
        {
            return (float)actual.Positives() / (ReferenceImage.Size.Height * ReferenceImage.Size.Width);
        }

        public UMat ReferenceImage { get; set; }

        public double ComputeFitness(object actual, FitnessFunction function)
        {
            var tmp = actual as UMat;
            if (tmp == null) throw new NotSupportedException("CVReferenceSet expects UMats");
            if (!tmp.IsBinary()) tmp = tmp.Binary();
            
            if(function == FitnessFunction.MCC)
            {
                return CVCGPEvaluator.MCC(ReferenceImage, tmp);
            }
            throw new NotImplementedException("only mcc implemented so far");      
        }

        public int Height { get; set; }
        public int Width { get; set; }
    }
}
