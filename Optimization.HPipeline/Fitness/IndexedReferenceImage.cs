using System.Collections.Generic;
using System.IO;
using Extensions;
using HalconDotNet;
using Optimization.HPipeline.Serialization;

namespace Optimization.HPipeline.Fitness
{
    public class IndexedReferenceImage : ReferenceImage
    {
        public IndexedReferenceImage(string imagePath, List<string> labelPaths, ImageSize resize = null)
        {
            Image = new HImage(imagePath);
            HObject concat = null;
            foreach (var labelPath in labelPaths)
            {
                var regions = IndexedHObjectConverter.IndexedToHObject(imagePath, labelPath);
                foreach (var region in regions)
                {
                    if (concat == null)
                    {
                        concat = region.Key;
                        Labels.Add(region.Value.ToString());
                    }
                    else
                    {
                        concat = concat.ConcatObj(region.Key); // ConcatObj does not allocate new memory
                        Labels.Add(region.Value.ToString());
                    }
                }
            }
            if (concat == null)
                HOperatorSet.GenEmptyObj(out concat);
            ReferenceRegions = concat;
            HObject tmp;
            HOperatorSet.Union1(concat, out tmp);
            UnionReferenceRegions = tmp;
            InitializeRectangles();
            Initialize();
            Filename = Path.GetFileName(imagePath);
        }
    }
}
