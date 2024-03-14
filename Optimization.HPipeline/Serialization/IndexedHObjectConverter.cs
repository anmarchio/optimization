using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions;
using HalconDotNet;

namespace Optimization.HPipeline.Serialization
{
    public class IndexedHObjectConverter
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="directoryIn"> full path to directory</param>
        /// <param name="directoryOut"> full path to directory</param>
        /// <param name="config"> full path to config file</param>
        public IndexedHObjectConverter(string directoryIn, string directoryOut, string config = null)
        {
            DirectoryIn = directoryIn;
            DirectoryOut = directoryOut;

            if (!Directory.Exists(directoryIn)) throw new IOException("Directory not found: " + directoryIn);
            if (!Directory.Exists(directoryOut)) Directory.CreateDirectory(directoryOut);
            if (config != null && !File.Exists(config)) throw new IOException("could not find config file");
            else if (config != null)
            {
                throw new NotImplementedException("Configurations not yet supported"); // TO DO: Read Dictionary from config file
            }
        }

        public string DirectoryIn { get; set; }

        public string DirectoryOut { get; set; }


        /// <summary>
        /// Note that current, the int refers to the region index of the extracted HObject, which start at 1
        /// </summary>
        public Dictionary<int, string> IndexToLabel { get; set; } = new Dictionary<int, string>
        {
            {1, "foreground" },
            {255, "foreground" }
        };

        public Dictionary<string, int> LabelToIndex { get; set; } = new Dictionary<string, int>
        {
            { "foreground", 255 }
        };

        public Dictionary<int, HTuple> ColorPalette { get; set; } = new Dictionary<int, HTuple>();


        public static bool IsRegionMarkerData(string directoryIn)
        {
            var dirs = Directory.EnumerateDirectories(directoryIn);
            if (!dirs.Any(x => x.Equals(Path.Combine(directoryIn, "regions")))) return false;
            if (!dirs.Any(x => x.Equals(Path.Combine(directoryIn, "images")))) return false;
            return true;
        }


        public static bool IsOldRegionMarker(string directoryIn)
        {
            var dirs = Directory.EnumerateDirectories(directoryIn);
            if (!dirs.Any(x => x.Equals(Path.Combine(directoryIn, "regions")))) return false;
            return true;
        }

        public static bool IsIndexedColorData(string directoryIn)
        {
            var dirs = Directory.EnumerateDirectories(directoryIn);
            if (!dirs.Any(x => x.Equals(Path.Combine(directoryIn, "images")))) return false;
            if (!dirs.Any(x => x.Equals(Path.Combine(directoryIn, "labels")))) return false;
            return true;
        }


        public void ConvertImages()
        {
            if (IsRegionMarkerData(DirectoryIn) && IsIndexedColorData(DirectoryIn))
            {
                throw new Exception("Directory contains ambiguous data (Hobj and indexed)");
            }
            else if (IsRegionMarkerData(DirectoryIn))
            {
                ConvertHObjectToIndexed();
            }
            else if (IsIndexedColorData(DirectoryIn))
            {
                ConvertIndexedToHObject();
            }
            else
            {
                throw new NotImplementedException("Input directory folder structure does not match the supported folder structures");
            }
        }

        public void ConvertIndexedToHObject()
        {
            var imageDir = Path.Combine(DirectoryIn, "images");
            var labelDir = Path.Combine(DirectoryIn, "labels");
            var imageNames = Directory.EnumerateFiles(imageDir).Where(x => x.IsImageFilePath());
            var outRegionPath = Path.Combine(DirectoryOut, "regions");
            if (!Directory.Exists(outRegionPath)) Directory.CreateDirectory(outRegionPath);

            foreach (var imageName in imageNames)
            {
                var filename = Path.GetFileNameWithoutExtension(imageName);
                HObject image, labels, regions;
                HOperatorSet.ReadImage(out image, imageName);
                HOperatorSet.ReadImage(out labels, Path.Combine(labelDir, filename));
                //HTuple type;
                //HOperatorSet.GetImageType(labels, out type);
                HOperatorSet.LabelToRegion(labels, out regions); // returns each value as single region

                var imageRegionPath = Path.Combine(outRegionPath, Path.GetFileNameWithoutExtension(imageName));
                if (!Directory.Exists(imageRegionPath)) Directory.CreateDirectory(imageRegionPath);
                for (int i = 2; i <= regions.CountObj(); i++) // halcon starts counting at 1, cannot stress enough, how much this pisses me off
                                                              // thus starting at 2 ensures that the lowest value (assumes to be background) is not written as hobject
                {
                    if (IndexToLabel.ContainsKey(i - 1))
                    {
                        /* 
                        HTuple gval, rows, columns;
                        HOperatorSet.GetRegionPoints(regions.SelectObj(i), out rows, out columns);
                        HOperatorSet.GetGrayval(labels, rows.TupleSelect(1), columns.TupleSelect(1), out gval);
                        if (!ColorPalette.ContainsKey(i-1))
                            ColorPalette.Add(i - 1, gval);
                        */
                        regions.SelectObj(i).WriteObject(Path.Combine(imageRegionPath, IndexToLabel[i - 1] + "_" + (i - 1) + ".hobj"));
                    }
                }

                //HOperatorSet.WriteImage(image, "jpg", 0, Path.Combine(DirectoryOut, Path.GetFileName(imageName)));
                image.Dispose();
                regions.Dispose();
            }
        }


        public static Dictionary<HObject, int> IndexedToHObject(string imagePath, string indexedImagePath)
        {

            var filename = Path.GetFileNameWithoutExtension(imagePath);
            HObject image = null, labels = null, regions;
            try
            {
                HOperatorSet.ReadImage(out image, imagePath);
                HOperatorSet.ReadImage(out labels, indexedImagePath);
                HOperatorSet.LabelToRegion(labels, out regions); // returns each value as single region
                var ret = new Dictionary<HObject, int>();
                for (int i = 2; i <= regions.CountObj(); i++) // halcon starts counting at 1, cannot stress enough, how much this pisses me off                                                            // thus starting at 2 ensures that the lowest value (assumes to be background) is not written as hobject
                {
                    ret.Add(regions.SelectObj(i), i - 1);
                }
                return ret;
            }
            finally
            {
                image.Dispose();
                labels.Dispose();
            }
        }

        /// <summary>
        /// Uses all regions specified by regionPaths to create an indexed image, using labelToIndex to determine which label corresponds to which value in the indexed image.
        /// region path names must follow the naming convention for labelled hobjects specified by RegionMarker, i.e. <label>_<rest>.hobj
        /// </summary>
        /// <param name="imagePath"></param>
        /// <param name="regionPaths"></param>
        /// <param name="labelToIndex"></param>
        /// <returns></returns>
        public static HObject HObjectsToIndexed(string imagePath, IEnumerable<string> regionPaths, Dictionary<string, int> labelToIndex, bool binary=true)
        {
            var labelRegionDict = new Dictionary<string, HObject>();
            foreach (var regionPath in regionPaths)
            {
                var label = Path.GetFileNameWithoutExtension(regionPath).Split('_')[0];
                HObject region = null;
                HOperatorSet.ReadRegion(out region, regionPath);
                if (!labelRegionDict.ContainsKey(label))
                {
                    labelRegionDict.Add(label, region);
                }
                else
                {
                    labelRegionDict[label] = labelRegionDict[label].ConcatObj(region);
                }
            }

            // we need to fill regions, as sometimes of the contour is stored. computing fitness on only the contour 
            // causes unfair low fitness
            foreach (var label in labelRegionDict.Keys.ToList())
            {
                HObject fill;
                HOperatorSet.FillUp(labelRegionDict[label], out fill);
                labelRegionDict[label] = fill;
            }

            HObject image;
            HTuple width, height;
            HOperatorSet.ReadImage(out image, imagePath);
            HOperatorSet.GetImageSize(image, out width, out height);
            // no regions stored for this image -> output empty label image       
            HObject labelImage = new HImage("byte", width, height);

            // to do: create image with pixel values corresponding to label
            foreach (var label in labelRegionDict.Keys)
            {
                /*
                if (ColorPalette.ContainsKey(LabelToIndex[label]))
                    HOperatorSet.OverpaintRegion(labelImage, labelRegionDict[label], ColorPalette[LabelToIndex[label]], "fill");

                else*/
                if (binary && !labelToIndex.ContainsKey(label))
                    labelToIndex[label] = labelToIndex["foreground"]; // treats everything that is not explicity labeled with an hobject as background
                HOperatorSet.OverpaintRegion(labelImage, labelRegionDict[label], labelToIndex[label], "fill");
            }
            return labelImage;
        }

        private void ConvertOldRegionMarkerToNew()
        {
            var imageDir = Path.Combine(DirectoryIn, "images");
            var regionDir = Path.Combine(DirectoryIn, "regions");
            imageDir.CreateDirectory();

            var images = Directory.EnumerateFiles(DirectoryIn).Where(x => x.IsImageFilePath());
            var regions = Directory.EnumerateFiles(regionDir).Where(x => x.EndsWith(".hobj"));

            foreach (var imagePath in images)
            {
                var newImagePath = Path.Combine(imageDir, Path.GetFileName(imagePath));
                if (!File.Exists(newImagePath))
                    File.Move(imagePath, newImagePath);

                var newPath = Path.Combine(regionDir, Path.GetFileName(imagePath));
                newPath.CreateDirectory();

                foreach (var regionPath in regions.Where(x => Path.GetFileName(x).Contains(Path.GetFileName(imagePath))))
                {
                    var newRegionPath = Path.Combine(newPath, "foreground_" + Path.GetFileName(regionPath));
                    if (!File.Exists(newRegionPath))
                        File.Move(regionPath, newRegionPath);
                }
            }
        }

        public void ConvertHObjectToIndexed()
        {
            if (IsOldRegionMarker(DirectoryIn)) ConvertOldRegionMarkerToNew();
            var imageDir = Path.Combine(DirectoryIn, "images");

            var regionDir = Path.Combine(DirectoryIn, "regions");
            var labelDir = Path.Combine(DirectoryOut, "labels");
            var outImageDir = Path.Combine(DirectoryOut, "images");
            if (!Directory.Exists(labelDir)) Directory.CreateDirectory(labelDir);
            if (!Directory.Exists(outImageDir)) Directory.CreateDirectory(outImageDir);
            var imageNames = Directory.EnumerateFiles(imageDir).Where(x => x.IsImageFilePath());

            foreach (var imageName in imageNames)
            {
                var imageRegionsDir = Path.Combine(regionDir, Path.GetFileNameWithoutExtension(imageName));
                if (!Directory.Exists(imageRegionsDir)) // for backwards compatibility with old reagion marker saving regions in directories with .jpg extension
                    imageRegionsDir = Path.Combine(regionDir, Path.GetFileName(imageName));
                HObject image = null, labelImage = null;
                HOperatorSet.ReadImage(out image, imageName);
                HTuple width, height;
                HOperatorSet.GetImageSize(image, out width, out height);
                // no regions stored for this image -> output empty label image       
                // workaround for not being able to find a direct transfer of the color lookup table:
                if (Directory.Exists(imageRegionsDir))
                {
                    var regionPaths = Directory.EnumerateFiles(imageRegionsDir).Where(x => x.EndsWith(".hobj"));
                    labelImage = HObjectsToIndexed(imageName, regionPaths, LabelToIndex);
                    /*
                    var labelRegionDict = new Dictionary<string, HObject>();
                   
                    foreach (var regionPath in regionPaths)
                    {
                        var label = Path.GetFileNameWithoutExtension(regionPath).Split('_')[0];
                        HObject region = null;
                        HOperatorSet.ReadRegion(out region, regionPath);
                        if (!labelRegionDict.ContainsKey(label))
                        {
                            labelRegionDict.Add(label, region);
                        }
                        else
                        {
                            labelRegionDict[label] = labelRegionDict[label].ConcatObj(region);
                        }
                    }
                    // to do: create image with pixel values corresponding to label
                    foreach (var label in labelRegionDict.Keys)
                    {
                        /*
                        if (ColorPalette.ContainsKey(LabelToIndex[label]))
                            HOperatorSet.OverpaintRegion(labelImage, labelRegionDict[label], ColorPalette[LabelToIndex[label]], "fill");
                            
                        else
                        HOperatorSet.OverpaintRegion(labelImage, labelRegionDict[label], LabelToIndex[label], "fill");

                    }*/

                    HOperatorSet.WriteImage(image, Path.GetExtension(imageName).Replace(".", ""), 0, Path.Combine(outImageDir, Path.GetFileName(imageName)));
                    HOperatorSet.WriteImage(labelImage, "png", 0, Path.Combine(labelDir, Path.GetFileNameWithoutExtension(imageName)));
                }
            }
        }
    }
}
