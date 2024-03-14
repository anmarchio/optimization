using System;
using System.IO;
using HalconDotNet;
using NUnit.Framework;
using Optimization.HPipeline.Fitness;
using Optimization.Tests.Categories;
using Optimization.Tests.TestImages;

namespace Optimization.HPipeline.Tests
{

    [TestFixture]
    public class CommonHImages
    {
      
        public static ReferenceSet ReferenceSetHalcon
        {
            get
            {
                return new ReferenceSet(CommonHImages.SampleImageDirectory) {ImageResize = CommonImages.Size};
            }
        }

        
        public static ReferenceSet ReferenceSetHalconColor
        {
            get
            {
                return new ReferenceSet(CommonHImages.SampleImageDirectory, LabelledReferenceSet.Processes.none) {ImageResize = CommonImages.Size};
            }
        }

        public static string SampleImageDirectory
        {
            get
            {
                return Path.Combine(CommonImages.TestImagesDirectory, "SampleImage");
            }
        }

        public static string ImageFormatConversionDirectory
        {
            get
            {
                return Path.Combine(CommonImages.TestImagesDirectory, "ImageFormatConversion");
            }
        }


        public static HObject CrackDetection
        {
            get
            {
                HObject obj;
                HOperatorSet.ReadImage(out obj, Path.Combine(CommonImages.TestImagesDirectory, "CompareCrackDetectionHdev", "crackdetectionhdev.jpg"));
                if (CommonImages.ScaleDown) return DownScale(obj);
                return obj;
            }
        }

        public static HObject FiberOrientation
        {
            get
            {
                HObject obj;
                HOperatorSet.ReadImage(out obj, Path.Combine(CommonImages.TestImagesDirectory, "CompareFiberOrientationHdev", "fiberorientationhdev.jpg"));
                if (CommonImages.ScaleDown) return DownScale(obj);
                return obj;
            }
        }

        public static HObject NonWovenTwists
        {
            get
            {
                HObject obj;
                HOperatorSet.ReadImage(out obj, Path.Combine(CommonImages.TestImagesDirectory, "CompareNonWovenTwistsHdev", "nonwoventwistshdev.jpg"));
                if (CommonImages.ScaleDown) return DownScale(obj);
                return obj;
            }
        }

        private static HObject DownScale(HObject img)
        {
            HObject tmp;
            HOperatorSet.ZoomImageSize(img, out tmp, CommonImages.ScaleWidth, CommonImages.ScaleHeight, "constant");
            return tmp;
        }

        public static HObject StandardFiber
        {
            get
            {
                if (!File.Exists(CommonImages.StandardFiberPath)) throw new FileNotFoundException("Could not find: " + CommonImages.StandardFiberPath);
                HObject obj;
                HOperatorSet.ReadImage(out obj, CommonImages.StandardFiberPath);
                if (CommonImages.ScaleDown) return DownScale(obj);
                return obj;  
            }
        }
        

        [Test,ShortTest]
        public void CrackDetectionImageExists()
        {
            try
            {
                var tmp = CrackDetection;
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test,ShortTest]
        public void FiberOrientationImageExists()
        {
            try
            {
                var tmp = FiberOrientation;
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test,ShortTest]
        public void NonWovenTwistsImageExists()
        {
            try
            {
                var tmp = NonWovenTwists;
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

        [Test,ShortTest]
        public void StandardFiberImageExists()
        {
            try
            {
                var tmp = StandardFiber;
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        }

    }
}
