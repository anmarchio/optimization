using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions;
using HalconDotNet;
using Optimization.Fitness;
using Optimization.Fitness.Interfaces;

namespace Optimization.HPipeline.Fitness
{
    /// <summary>
    /// I do not advise dealing with this class directly. Use ReferenceSet instead.
    /// </summary>
    public class ReferenceImage : IReference<HObject, HObject>, IDisposable
    {
        public enum RectangleType
        {
            AxisParallel, Minimal
        }

        protected ReferenceImage()
        {

        }

        /// <summary>
        /// DO NOT USE THIS CONSTRUCTOR -- apparently works with ReferenceSet, yet if used directory reference regions are empty. cannot be bothered to find out why.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="proc"></param>
        /// <param name="rectangleType"></param>
        internal ReferenceImage(string filename, string regionsFolder=null,
            LabelledReferenceSet.Processes proc = LabelledReferenceSet.Processes.rgb1ToGray,
            RectangleType rectangleType = RectangleType.AxisParallel,
            ImageSize imageResize = null)
        {
            Filename = filename;
            if(regionsFolder == null)
            {
                var parent = Directory.GetParent(filename);
                var relativeRegions = Path.Combine("regions", Path.GetFileNameWithoutExtension(filename));
                var relativeRegionsExt = Path.Combine("regions", Path.GetFileName(filename));
                if (parent.Name.Equals("images"))
                {
                    if (Directory.Exists(Path.Combine(parent.Parent.FullName, relativeRegionsExt)))
                        regionsFolder = Path.Combine(parent.Parent.FullName, relativeRegionsExt);
                    else
                        regionsFolder = Path.Combine(parent.Parent.FullName, relativeRegions);
                }
                else
                {
                    if (Directory.Exists(Path.Combine(parent.FullName, relativeRegionsExt)))
                        regionsFolder = Path.Combine(parent.FullName, relativeRegionsExt);
                    else
                        regionsFolder = Path.Combine(parent.FullName, relativeRegions);
                }
            }
            var region = RegionsConcatenation(regionsFolder);
            ReferenceRegions = region;
            HObject image = new HImage(filename);
            if(proc == LabelledReferenceSet.Processes.rgb1ToGray)
            {
                HObject tmp;
                HOperatorSet.Rgb1ToGray(image, out tmp);
                image.Dispose();
                image = tmp;
            }
            Image = image;
            Rectangle = rectangleType;

            if (imageResize != null)
            {
                HOperatorSet.ZoomImageSize(image, out image, imageResize.Width, imageResize.Width, "bicubic");
            }

            Initialize();
            InitializeRectangles();
            TryLoadLabels(filename);
        }

        private void TryLoadLabels(string regionsFolder)
        {
            Labels = new List<string>();

            if (Directory.Exists(regionsFolder))
            {
                var files = Directory.EnumerateFiles(regionsFolder).Where(x => x.EndsWith(".hobj"));

                foreach (var f in files)
                {
                    var name = Path.GetFileName(f);
                    var split = name.Split('_');
                    if (split.Length < 2) Labels.Add("Default");
                    else Labels.Add(split[0]);
                }
            }
        }

        public LabelledReferenceSet.Processes Process { get; protected set; }

        private HObject RegionsConcatenation(string path)
        {
            var filename = Path.GetFileName(path);

            //path = path.Replace(filename, "");
            var dir = path;// Path.Combine(path, "regions", filename);

            if (!Directory.Exists(dir))
            {
                HObject empty;
                HOperatorSet.GenEmptyObj(out empty);
                return empty;
            }
            var files = Directory.EnumerateFiles(dir);
            // files = files.Select(x => x.Replace("\\\\", "\\"));
            var regionPaths = files.Where(x => x.Contains(filename)).ToList();
            HObject o = null;
            if (regionPaths.Count() > 0)
            {

                HOperatorSet.ReadRegion(out o, regionPaths.First());
                HOperatorSet.FillUp(o, out o);
                for(int i = 1; i < regionPaths.Count;i++)
                {
                    HObject o2;
                    HOperatorSet.ReadRegion(out o2, regionPaths[i]);
                    HOperatorSet.FillUp(o2, out o2);
                    o = o.ConcatObj(o2);
                }
            }
            else
            {
                HOperatorSet.GenEmptyObj(out o);
            }
            return o;
        }

        protected void Initialize()
        {
            HTuple width, height;
            HOperatorSet.GetImageSize(Image, out width, out height);
            Width = width; Height = height;
            PixelCount = width * height;
            HObject union;
            HOperatorSet.Union1(ReferenceRegions, out union);
            HOperatorSet.FillUp(union, out union);
            UnionReferenceRegions = union;
        }

        public void Dispose()
        {
            if(Image!= null)
                Image.Dispose();
            if(ReferenceRegions != null)
                ReferenceRegions.Dispose();
            if (UnionReferenceRegions != null)
                UnionReferenceRegions.Dispose();
            if(smallestOuterRectangles != null)         
                foreach (var item in smallestOuterRectangles)
                    if (item.Value != null) item.Value.Dispose();

        }

        protected void InitializeRectangles()
        {
            smallestOuterRectangles = new Dictionary<int, HObject>();
            smallestOuterRectangleSize = new Dictionary<int, int>();

            for (int i = 0; i < RegionCount; i++)
            {
                HObject rectangle;
                HTuple row, column, area;
                if (Rectangle == RectangleType.AxisParallel)
                {
                    HTuple row1, row2, column1, column2;
                    HOperatorSet.SmallestRectangle1(ReferenceRegions.SelectObj(i + 1), out row1, out column1, out row2, out column2);
                    HOperatorSet.GenRectangle1(out rectangle, row1, column1, row2, column2);
                }
                else
                {
                    HTuple phi, length1, length2;
                    HOperatorSet.SmallestRectangle2(ReferenceRegions.SelectObj(i + 1), out row, out column, out phi, out length1, out length2);
                    HOperatorSet.GenRectangle2(out rectangle, row, column, phi, length1, length2);
                }
                HOperatorSet.FillUp(rectangle, out rectangle);
                smallestOuterRectangles.Add(i, rectangle);
                HOperatorSet.AreaCenter(rectangle, out area, out row, out column);
                smallestOuterRectangleSize.Add(i, area.Length > 0 ? area.I : 0);
            }
        }
        
        private HObject RectanglesToHObject()
        {
            var r = smallestOuterRectangles[0];
            for (int i = 1; i < smallestOuterRectangles.Count; i++)
            {
                r = r.ConcatObj(smallestOuterRectangles[i]);
            }
            return r;
        }

        public string Filename { get; protected set; }

        public HObject Image { get; protected set; }

        public HObject ReferenceRegions { get; protected set; }
        public HObject UnionReferenceRegions { get; protected set; }
        public int RegionCount { get { return ReferenceRegions.CountObj(); } }
        public int PixelCount { get; protected set; }

        public RectangleType Rectangle = RectangleType.AxisParallel;

        public HObject this[int i]
        {
            get
            {
                return ReferenceRegions.SelectObj(i+1);
            }
        }


        private Dictionary<int, HObject> smallestOuterRectangles;
        public HObject SmallestOuterRectangle(int i)
        {
            return smallestOuterRectangles[i];
        }

        private Dictionary<int, int> smallestOuterRectangleSize;
        public int SmallestOuterRectangleSize(int i)
        {
            return smallestOuterRectangleSize[i];
        }

        public double ComputeFitness(object actual, FitnessFunction fitnessFunction)
        {
            var tmp = actual as HObject;

            switch (fitnessFunction)
            {
                case FitnessFunction.MCC:
                    return Collection.MCC(Reference, tmp, Height, Width);
                case FitnessFunction.IntersectionOverUnion:
                    return Collection.IntersectionOverUnion(tmp, Reference);
                case FitnessFunction.Accuracy:
                    return Collection.Accuracy(tmp, Reference, Height, Width);
                case FitnessFunction.Precision:
                    return Collection.Precision(Reference, tmp);
                case FitnessFunction.Recall:
                    return Collection.Recall(Reference, tmp);
                case FitnessFunction.Sensitivity: // same as recall
                    return Collection.Recall(Reference, tmp);
                default:
                    throw new NotSupportedException(fitnessFunction.ToString() + " is not supported by " + GetType().ToString());
            }

        }

        protected HObject rectangleUnion = null;
        public HObject RectangleUnion
        {
            get
            {
                if(rectangleUnion == null)
                {
                    HOperatorSet.Union1(RectanglesToHObject(), out rectangleUnion);
                }
                return rectangleUnion;
            }
        }

        protected int sumNonROIPixels = -1;
        public int SumNonROIPixels
        {
            get
            {
                if(sumNonROIPixels == -1)
                {
                    sumNonROIPixels = PixelCount - smallestOuterRectangleSize.Sum(x => x.Value);
                }
                return sumNonROIPixels;
            }
        }

        public HObject Input
        {
            get
            {
                return Image;
            }
        }

        public HObject Reference
        {
            get
            {
                return UnionReferenceRegions;
            }
        }

        public double PercentageOfPixels(HObject actual)
        {
            HTuple area, row, column;
            HObject actualUnion = null;
            try
            {
                HOperatorSet.Union1(actual, out actualUnion);
                HOperatorSet.AreaCenter(actualUnion, out area, out row, out column);
                if (area.TupleLength() == 0) return 0;
                return area.TupleSum() / (float)PixelCount;
            }
            finally
            {
                if (actualUnion != null) actualUnion.Dispose();
            }
        }


        /// <summary>
        /// Compute the intersection of region with all regions in this reference image until one is found. Returns the label of
        /// the reference region with which the intersection is not empty.
        /// </summary>
        /// <param name="region"></param>
        /// <returns>null if no intersection can be found, else the appropriate label</returns>
        public string LabelOf(HObject region)
        {
            for (int i = 0; i < ReferenceRegions.CountObj(); i++)
            {
                HObject intersection;
                HOperatorSet.Intersection(region, this[i], out intersection);

                HTuple area, row, column;
                HOperatorSet.AreaCenter(intersection, out area, out row, out column);
                if (area.Length > 0 && area > 0) return Labels[i];
            }
            return null;
        }

        public List<string> Labels { get; private set; } = new List<string>();
        public int Height { get; set; }
        public int Width { get; set; }

        public override string ToString()
        {
            return Filename;
        }
    }


}
