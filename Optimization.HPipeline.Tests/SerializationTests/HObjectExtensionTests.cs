using System;
using System.IO;
using Extensions;
using HalconDotNet;
using NUnit.Framework;
using Optimization.HPipeline.Fitness;
using Optimization.Tests;
using Optimization.Tests.Categories;

namespace Optimization.HPipeline.Tests.SerializationTests
{
    [TestFixture]
    public class HObjectExtensionTests
    {
        [Test, ShortTest]
        public void Inpaint()
        {
            var refSet = new ReferenceSet(Path.Combine(CommonHImages.ImageFormatConversionDirectory, "Indexed"));
            var writeImages = true;

            var sq = CommonHalconPipelines.StatusQuo;
            var inpaintDir = Path.Combine(CommonInformation.TestResultsDirectory, "TestInpaint");
            inpaintDir.CreateDirectory();

            for(int i = 0; i < refSet.Count; i++)
            {
                try
                {
                    var img = refSet[i];
                    var result = sq.ExecuteSingle(img.Image);
                    //sq.WriteOutputs(img.Image, Path.Combine(CommonInformation.TestResultsDirectory, "TestInpaint", Path.GetFileNameWithoutExtension(img.Filename)));

                    var paint = img.Image.Inpaint(img.ReferenceRegions, result);
                    if (writeImages)
                        HOperatorSet.WriteImage(paint, "jpg", 0,
                            Path.Combine(inpaintDir, Path.GetFileNameWithoutExtension(img.Filename)));
                    paint = img.Image.Inpaint(img.ReferenceRegions, HObjectExtensions.Green);
                    if (writeImages)
                        HOperatorSet.WriteImage(paint, "jpg", 0,
                            Path.Combine(inpaintDir, "reference" + Path.GetFileNameWithoutExtension(img.Filename)));
                    paint = img.Image.Inpaint(result, HObjectExtensions.Yellow);
                    if (writeImages)
                        HOperatorSet.WriteImage(paint, "jpg", 0,
                            Path.Combine(inpaintDir, "actual" + Path.GetFileNameWithoutExtension(img.Filename)));
                }
                catch (Exception e)
                {
                    Assert.Warn("{0}, img: {1}, pipe: {2}", e, i, sq.Name);
                }
            }
        }
    }
}
