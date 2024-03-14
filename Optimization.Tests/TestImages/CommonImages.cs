using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extensions;

namespace Optimization.Tests.TestImages
{
    public static class CommonImages
    {

        private static string testImagesDirectory = null;

        public static bool ScaleDown { get; set; } = true;
        public static int ScaleWidth { get; set; } = 128;
        public static int ScaleHeight { get; set; } = 128;

        public static ImageSize Size { get; set; } = new ImageSize(ScaleWidth, ScaleHeight);

        public static string TestImagesDirectory
        {
            get
            {
                if (testImagesDirectory == null)
                {
                    string dir = CommonInformation.OptimizationTestsDirectory;
                    var fileInfo = new FileInfo(dir);
                    var children = fileInfo.Directory.Parent.EnumerateDirectories();
                    foreach (var childDirectory in children)
                    {
                        if (childDirectory.Name.Equals("TestImages"))
                            testImagesDirectory = childDirectory.FullName;
                    }

                    if (testImagesDirectory == null)
                        throw new Exception("Necessary Directory does not seem to exist relative to Test Directory (TestImages required).");
                }
                return testImagesDirectory;
            }
        }

        public static IEnumerable<string> EnumerateSampleImageDirectory()
        {
            string[] images = { "3", "4", "57", "65", "72" };

            for (int i = 0; i < images.Length; i++)
            {
                var path = Path.Combine(SampleImageDirectory, images[i]);
                var imagePath = Path.Combine(SampleImageDirectory, "Images", images[i]);
                if (File.Exists(path + ".jpg")) yield return path + ".jpg";
                else if (File.Exists(path + ".bmp")) yield return path + ".bmp";
                else if (File.Exists(imagePath + ".jpg")) yield return imagePath + ".jpg";
                else if (File.Exists(imagePath + ".bmp")) yield return imagePath + ".bmp";
                else throw new FileNotFoundException("The sample images folder seems to be missing.");
            }

        }


        public static string SampleImageDirectory
        {
            get
            {
                return Path.Combine(TestImagesDirectory, "SampleImage");
            }
        }

        public static string ImageFormatConversionDirectory
        {
            get
            {
                return Path.Combine(TestImagesDirectory, "ImageFormatConversion");
            }
        }

        public static string StandardFiberPath
        {
            get
            {
                return Path.Combine(CommonImages.TestImagesDirectory, "1.jpg");
            }
        }
    }
}
