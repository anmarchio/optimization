using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions;
using HalconDotNet;
using Optimization.Data;
using Optimization.HPipeline.Serialization;

//using Microsoft.VisualBasic.Devices;

namespace Optimization.HPipeline.Fitness
{
    /// <summary>
    /// Holds all images to be used by FitnessEvaluator.cs as reference images
    /// </summary>
    public class ReferenceSet : DataSet<ReferenceImage>
    {
        //public virtual List<ReferenceImage> Images { get; protected set; }

        private List<string> ImagePaths { get; set; }

        protected ReferenceSet()
        {

        }

        public ReferenceSet(string folder,
            LabelledReferenceSet.Processes proc = LabelledReferenceSet.Processes.rgb1ToGray,
            ReferenceImage.RectangleType rectangleType = ReferenceImage.RectangleType.AxisParallel)
        {
            if (IndexedHObjectConverter.IsIndexedColorData(folder))
            {
                //convert to appropriate place            
                if (!IndexedHObjectConverter.IsRegionMarkerData(folder))
                {
                    var converter = new IndexedHObjectConverter(folder, folder);
                    converter.ConvertIndexedToHObject();
                }
            }
            else if(!IndexedHObjectConverter.IsRegionMarkerData(folder) && !IndexedHObjectConverter.IsOldRegionMarker(folder))
            {
                throw new NotSupportedException("The input images are not in the supported folder structure");
            }

            RectangleType = rectangleType;
            Process = proc;
            string[] files = null;

            if (IndexedHObjectConverter.IsRegionMarkerData(folder))
                files = Directory.EnumerateFiles(Path.Combine(folder, "images")).Where(x => x.IsImageFilePath()).ToArray();
            else if (IndexedHObjectConverter.IsOldRegionMarker(folder))
                files = Directory.EnumerateFiles(folder).Where(x => x.IsImageFilePath()).ToArray();
            else throw new Exception("folder structure does not conform with RegionMarker or OldRegionMarker structure.");
            if (files.Length == 0) throw new ArgumentException("No images (.bmp, .jpg) could be found in directory: " + folder);

            ImagePaths = files.ToList();

            // this is to cause Halcon Version Error before anything important is being done
            // else halcon fails to load the first image of the reference set
            // gotta love dat halcon

            HObject r;
            HOperatorSet.ReadImage(out r, ImagePaths[0]);
            r.Dispose();

            //RegisterImages(files);

            DirectorySize = (ulong)new DirectoryInfo(folder).DirSize();
        }

        public ulong DirectorySize { get; set; }

        public LabelledReferenceSet.Processes Process { get; protected set; }

        public ReferenceImage.RectangleType RectangleType { get; protected set; }

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

        
        public override int Count
        {
            get { return ImagePaths.Count; }
        }

        public override ReferenceImage this[int key]
        {
            get
            {
                var refImage = new ReferenceImage(ImagePaths[key], null, Process, RectangleType, ImageResize);
                return refImage;
            }
        }

        public override bool FitsIntoMemory { get; set; } = false;

        /// <summary>
        /// Returns the minimum and maximum areas over all defects over all images
        /// </summary>
        /// <returns>Item1: min Item2: max</returns>
        public Tuple<int, int> GetMinAndMaxAreaSize()
        {
            int min = int.MaxValue, max = int.MinValue;

            for (int i = 0; i < Count; i++)
            {
                using (var image = this[i])
                {
                    HOperatorSet.AreaCenter(image.Reference, out HTuple area, out HTuple row, out HTuple column);
                    if (area.Length == 0) continue;
                    int tupleMin = area.TupleMin();
                    int tupleMax = area.TupleMax();
                    if (tupleMin < min) min = tupleMin;
                    if (tupleMax > max) max = tupleMax;
                }
            }

            // equality checks should only matter in case that all regions are empty
            // (which would be a dumb optimization problem anyway)
            return Tuple.Create(min == int.MaxValue ? 0 : min,
                max == int.MinValue ? int.MaxValue : max);
        }
    }
}
