using System.Drawing;
using System.IO;
using Emgu.CV;
using Optimization.Tests.TestImages;

namespace Optimization.CVPipeline.Tests
{
    public static class CommonCVImages
    {
        public static string IndexedImagesPath = Path.Combine(CommonImages.ImageFormatConversionDirectory, "Indexed");

        public static CVReferenceSet ReferenceSetCV { get; set; } = new CVReferenceSet(IndexedImagesPath, Emgu.CV.CvEnum.ImreadModes.Grayscale);

        public static CVReferenceSet ColorReferenceSetCV
        {
            get;
            set;
        } = new CVReferenceSet(IndexedImagesPath, Emgu.CV.CvEnum.ImreadModes.Color);

        public static UMat StandardFiberCV
        {
            get
            {
                if (!File.Exists(CommonImages.StandardFiberPath)) throw new FileNotFoundException("Could not find: " + CommonImages.StandardFiberPath);
                var img =  CvInvoke.Imread(CommonImages.StandardFiberPath).GetInputArray().GetUMat();
                if(CommonImages.ScaleDown) CvInvoke.Resize(img, img,
                    new Size(CommonImages.ScaleWidth, CommonImages.ScaleHeight));
                return img;
            }
        }              
        
    }
}
