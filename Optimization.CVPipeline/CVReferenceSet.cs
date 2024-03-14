using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Emgu.CV;
using Extensions;
using Optimization.Data;
//using Microsoft.VisualBasic.Devices;

namespace Optimization.CVPipeline
{
    public class CVReferenceSet : DataSet<CVReferenceImage>
    {
        public CVReferenceSet(string directory, Emgu.CV.CvEnum.ImreadModes imgType, Size? resize = null)
        {
            var labelDir = Path.Combine(directory, "labels");
            var imageDir = Path.Combine(directory, "images");
            var regionDir = Path.Combine(directory, "regions");
            // Noch directorys erstellen? mit labelDir-imageDir-regionDir.createDirectory
            /*
            if (IndexedHObjectConverter.IsRegionMarkerData(directory) || IndexedHObjectConverter.IsOldRegionMarker(directory))
            {
                if (!IndexedHObjectConverter.IsIndexedColorData(directory))
                {
                    var converter = new IndexedHObjectConverter(directory, directory);
                    converter.ConvertHObjectToIndexed();
                }
            }*/            
            if (!Directory.Exists(imageDir)) throw new ArgumentException("directory must contain subdirectory 'images'");
            if (!Directory.Exists(labelDir)) throw new ArgumentException("directory must contain subdirectory 'labels'");
            ImagePaths = Directory.EnumerateFiles(imageDir).ToList();
            LabelPaths = Directory.EnumerateFiles(labelDir).ToList();

            ImgType = imgType;

            if (ImagePaths.Count != LabelPaths.Count) throw new Exception("number of labels and images not equal");

            FolderSize = (ulong)(new DirectoryInfo(directory)).DirSize();
        }

       /// <summary>
       /// imagepaths[i]'s label must be at labelpaths[i]
       /// </summary>
       /// <param name="basePath"></param>
       /// <param name="imagepaths"></param>
       /// <param name="labelpaths"></param>
        public CVReferenceSet(string basePath, IEnumerable<string> imagepaths, IEnumerable<string> labelPaths, Emgu.CV.CvEnum.ImreadModes imgType, ImageSize resize = null)
        {
            ImagePaths = imagepaths.Select(x => Path.Combine(basePath, x)).ToList();
            LabelPaths = labelPaths.Select(x => Path.Combine(basePath, x)).ToList();
            ImgType = imgType;
            if (!Directory.Exists(basePath)) throw new FileNotFoundException(string.Format("basePath: {0} not found.", basePath));

            if (ImagePaths.Any(x => !File.Exists(x))) throw new FileNotFoundException();
            if (LabelPaths.Any(x => !File.Exists(x))) throw new FileNotFoundException();
        }



        private ulong FolderSize { get; set; }

        protected List<string> ImagePaths { get; set; }
        protected List<string> LabelPaths { get; set; }

        protected Emgu.CV.CvEnum.ImreadModes ImgType
        {
            get;
            set;
        }

        public override CVReferenceImage this[int i]
        {
            get
            {
                var img = CvInvoke.Imread(ImagePaths[i], ImgType).GetUMat(Emgu.CV.CvEnum.AccessType.Fast);
                var label = CvInvoke.Imread(LabelPaths[i], ImgType).GetUMat(Emgu.CV.CvEnum.AccessType.Fast);
                var refImage = new CVReferenceImage(img, label, Path.GetFileName(ImagePaths[i]));

                return refImage;
            }
        }

        public override int Count
        {
            get
            {
                return ImagePaths.Count;
            }
        }

        public override bool FitsIntoMemory { get; set; } = false;
    }
}
