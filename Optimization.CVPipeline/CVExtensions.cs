using System;
using System.Linq;
using Emgu.CV;
using Emgu.CV.Structure;
using Optimization.CVPipeline.CVCGP;
using Optimization.Fitness;

namespace Optimization.CVPipeline
{
    public static class CVExtensions
    {

        public static MCvScalar Red = new MCvScalar(0, 0 , 255);
        public static MCvScalar Green = new MCvScalar(0, 255, 0);
        public static MCvScalar Yellow = new MCvScalar(0, 255, 255);
        /// <summary>
        /// Assumes that both images have their ROIs set to 1 and everything else to 0
        /// </summary>
        /// <param name="arr1"></param>
        /// <param name="arr2"></param>
        /// <returns></returns>
        public static UMat Intersect(this UMat arr1, UMat arr2)
        {
            var dst = new UMat();
            CvInvoke.BitwiseAnd(arr1, arr2, dst);
            return dst;
        }

        public static double ComputeFitness(this UMat reference, UMat actual, FitnessFunction function)
        {
            if(function == FitnessFunction.MCC)
            {
                return CVCGPEvaluator.MCC(reference, actual);               
            }
            throw new NotImplementedException();
        }


        /// <summary>
        ///  Computes the difference of regions in arr1 and arr2 marked with value > 0, assumes 0 for background, 1 for foreground
        /// <param name="arr1">array containing set of pixels > 0</param>
        /// <param name="arr2">array containing set of pixels > 0</param>
        /// <returns>arr1 \ arr2
        /// note that this operator is not commutative:
        /// arr1 \ arr2 != arr2 \ arr1
        /// </returns>
        public static UMat Difference(this UMat arr1, UMat arr2)
        {
            var union = Union(arr1, arr2);
            var intersection = Intersect(arr1, arr2);
            var dst = new UMat();
            CvInvoke.BitwiseXor(arr1, intersection, dst, arr1);

            intersection.GetInputArray().Dispose();
            union.GetInputArray().Dispose();

            return dst;
        }

        public static UMat Union(this UMat arr1, UMat arr2)
        {
            var dst = new UMat();
            CvInvoke.BitwiseOr(arr1, arr2, dst);
            return dst;
        }

        public static int TruePositives(this UMat reference, UMat actual)
        {
            var intersection = reference.Intersect(actual);

            var tp = CvInvoke.CountNonZero(intersection);
            intersection.GetInputArray().Dispose();

            return tp;
        }

        public static int FalsePositives(this UMat reference, UMat actual)
        {
            var intersection = reference.Intersect(actual);
            var diff = actual.Difference(intersection);
            var fp = CvInvoke.CountNonZero(diff);

            diff.GetInputArray().Dispose();
            intersection.GetInputArray().Dispose();

            return fp;
        }

        public static UMat Invert(this UMat arr)
        {
            var inverse = new UMat();
            var nonzero = new UMat();
            CvInvoke.Threshold(arr, inverse, 0, 1, Emgu.CV.CvEnum.ThresholdType.BinaryInv);
            return inverse;
        }

        public static int TrueNegatives(this UMat reference, UMat actual)
        {
            var invertRef = reference.Invert();
            var invertAct = actual.Invert();
            var intersect = invertRef.Intersect(invertAct);
            var tn = CvInvoke.CountNonZero(intersect);

            invertRef.GetInputArray().Dispose();
            invertAct.GetInputArray().Dispose();
            intersect.GetInputArray().Dispose();

            return tn;
        }

        public static int FalseNegatives(this UMat reference, UMat actual)
        {
            var invertRef = reference.Invert();
            var invertAct = actual.Invert();
            var diff = invertAct.Difference(invertRef);
            var fn = CvInvoke.CountNonZero(diff);

            diff.GetInputArray().Dispose();
            invertAct.GetInputArray().Dispose();
            invertRef.GetInputArray().Dispose();

            return fn;
        }

        public static int Negatives(this UMat actual)
        {
            var inv = actual.Invert();
            var neg =  CvInvoke.CountNonZero(inv);

            inv.GetInputArray().Dispose();

            return neg;
        }

        public static int Positives(this UMat actual)
        {
            return CvInvoke.CountNonZero(actual);
        }

        public static UMat Binary(this UMat image)
        {
            var dst = new UMat();
            CvInvoke.Threshold(image, dst, 0, 1, Emgu.CV.CvEnum.ThresholdType.Binary);
            return dst;
        }

        public static UMat ToGray(this UMat img, int channel = 0)
        {
            if (img.NumberOfChannels == 1) return img;
            var dst = new UMat();
            CvInvoke.ExtractChannel(img, dst, channel);
            dst.ConvertTo(dst, Emgu.CV.CvEnum.DepthType.Cv8U);
            return dst;
        }

        public static bool IsBinary(this UMat image)
        {
            IOutputArray hist= new UMat();
            if (image.NumberOfChannels > 1 || image.IsEmpty) return false; // used to be uncommented; unsure why...
            double[] minValues, maxValues;
            System.Drawing.Point[] minLocations, maxLocations;
            image.MinMax(out minValues, out maxValues, out minLocations, out maxLocations); // should be gray level image...
            var min = minValues.Min();
            var max = maxValues.Max();
            return (min == 0 || min == 1) && (max == 0 || max == 1);
        }

        public static UMat Inpaint(this UMat image, UMat reference = null, UMat actual = null)
        {

            var tmpImage = new Image<Bgr, byte>(image.GetInputArray().GetSize());
            image.GetInputArray().GetUMat().ToImage<Bgr, byte>().CopyTo(tmpImage);
            UMat tmpReference = null; if (reference != null) tmpReference = reference.GetInputArray().GetUMat();
            UMat tmpActual = null; if (actual != null) tmpActual = actual.GetInputArray().GetUMat();

            if (tmpReference != null)
                CvInvoke.Threshold(tmpReference, tmpReference, 0, 1, Emgu.CV.CvEnum.ThresholdType.Binary);
            if (tmpActual != null)
                CvInvoke.Threshold(tmpActual, tmpActual, 0, 1, Emgu.CV.CvEnum.ThresholdType.Binary);

            var referenceContours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            var actualContours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            var intersectionContours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            UMat intersection = null;
            if (tmpReference != null && tmpActual != null)
                tmpReference.Intersect(tmpActual).GetInputArray().GetUMat();

            if (tmpReference != null)
                CvInvoke.FindContours(tmpReference, referenceContours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            if (tmpActual != null)
                CvInvoke.FindContours(tmpActual, actualContours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);
            if (intersection != null)
                CvInvoke.FindContours(intersection, intersectionContours, null, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone);

            if (tmpActual != null)
            {
                CvInvoke.DrawContours(tmpImage, actualContours, -1, color: Red, thickness: 1);
                actualContours.Dispose();
                tmpActual.Dispose();
            }
            if (tmpReference != null)
            { 
                CvInvoke.DrawContours(tmpImage, referenceContours, -1, color: Green, thickness: 1);
                referenceContours.Dispose();
                tmpReference.Dispose();
            }
            if (intersection != null)
            {
                CvInvoke.DrawContours(tmpImage, intersectionContours, -1, color: Yellow, thickness: 1);
                intersectionContours.Dispose();
                intersection.Dispose();
            }
            return tmpImage.GetOutputArray().GetUMat();
        }

        public static void WriteImage(this Image<Bgr, byte> array, string filename)
        {
            if (!(filename.Contains(".bmp") || filename.Contains(".jpg") || filename.Contains(".tiff"))) throw new NotSupportedException("supported formats: .jpg, .bmp, .tiff");
            CvInvoke.Imwrite(filename, array);
        }

        public static void WriteImage(this UMat array, string filename)
        {
            if (!(filename.Contains(".bmp") || filename.Contains(".jpg") || filename.Contains(".tiff"))) throw new NotSupportedException("supported formats: .jpg, .bmp, .tiff");
            var bgr = array.GetInputArray().GetUMat().ToImage<Bgr, byte>();
            CvInvoke.Imwrite(filename, bgr);
        }

        public static void WriteImage(this UMat array, string filename, UMat reference = null, UMat actual = null)
        {
            var inpaint = Inpaint(array, reference, actual);
            WriteImage(inpaint, filename);
            inpaint.GetInputArray().Dispose();
        }

        public static bool DepthEquals(this UMat image1, UMat image2)
        {
            return image1.Depth.Equals(image2.Depth);
        }
    }
}
