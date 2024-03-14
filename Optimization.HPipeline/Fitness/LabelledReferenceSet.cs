using System;
using System.IO;
using System.Linq;
using HalconDotNet;

namespace Optimization.HPipeline.Fitness
{ 
    /// <summary>
    /// Labelled reference sets provide a list of labelled hobjects for each image; List of LabelledReferenceImage(s)
    /// 
    /// DEPRECATED!!! THIS IS ONLY KEPT TO NOT BREAK EXISTING CODE. ALL THE LABELLED FUNCTIONALITY IS IN INTEGRATED IN REFERENCEIMAGE
    /// </summary>
public class LabelledReferenceSet :ReferenceSet
    {

        /// <summary>
        /// All images must be in some folder. All reference regions must be in a subfolder of that folder titled "regions". In the folder regions, all reference regions must be in a folder with the same title
        /// as the corresponding image. The names of each reference region must be "*_LABEL.hobj".
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="rectangleType"></param>
        public LabelledReferenceSet(string folder, Processes proc = Processes.rgb1ToGray, ReferenceImage.RectangleType rectangleType = ReferenceImage.RectangleType.AxisParallel)
        {
            RectangleType = rectangleType;
            Process = proc;
            var files = Directory.EnumerateFiles(folder).Where(x => x.Contains(".bmp")).ToArray();
            if (files.Length == 0) throw new ArgumentException("No images (.bmp) could be found in directory: " + folder);
            RegisterImages(files);
            Folder = folder;
        }

        public enum Processes
        {
            rgb1ToGray, none
        }

        public string Folder { get; set; }

        /// <summary>
        /// All images must be in some folder. All reference regions must be in a subfolder of that folder titles "regions". In the folder regions, all reference regions must be in a folder with the same title
        /// as the corresponding image. The names of each reference region must be "*_LABEL.hobj".
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="rectangleType"></param>
        public LabelledReferenceSet(string[] imagePaths, Processes proc = Processes.rgb1ToGray, ReferenceImage.RectangleType rectangleType = ReferenceImage.RectangleType.AxisParallel)
        {
            RectangleType = rectangleType;
            Process = proc;
            RegisterImages(imagePaths);
            Folder = Path.GetDirectoryName(imagePaths[0]);
        }

        
        /// <summary>
        /// Adds all the images specified by the paths to the list of images.
        /// </summary>
        /// <param name="paths"></param>
        private void RegisterImages(string[] paths)
        {
            if (paths == null || paths.Length == 0) throw new ArgumentNullException("at least one image path needs to be passed as argument");
            // this is to cause Halcon Version Error before anything important is being done
            // else halcon fails to load the first image of the reference set
            // gotta love dat halcon
            HObject r;
            HOperatorSet.ReadImage(out r, paths[0]);
            r.Dispose();

            //Images = new List<ReferenceImage>();
            //singleChannels = new List<Tuple<HObject, HObject, HObject>>();
            int j = 0;
            foreach (var s in paths)
            {
                //Images.Add(new ReferenceImage(s, null, Process, RectangleType));
                //HTuple width, height;
                //HOperatorSet.GetImageSize(Images[Images.Count-1].Image, out width, out height);
                j++;
            }
        }

        /*
        public override string ToString()
        {
            string s = Path.GetFileName(Folder) + ": ";

            foreach(var image in Images)
            {
                s += Path.GetFileName(image.Filename) + ";";
            }


                return s;
        }*/
    }
}
