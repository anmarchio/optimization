using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HalconDotNet;

namespace Optimization.HPipeline.Fitness
{
    public class FileListReferenceSet : ReferenceSet
    {
        public FileListReferenceSet(string basePath, IEnumerable<string> imagepaths, Dictionary<string, IEnumerable<string>> labelpaths)
        {
            if (labelpaths.Count != imagepaths.Count())
            {
                throw new ApplicationException("images and labels mismatch! Did you assign the correct path for images and labels?");
            }
            ImagePaths = imagepaths.Select(x => Path.Combine(basePath, x)).ToList();
            LabelPaths = new Dictionary<string, List<string>>();
            foreach (var pair in labelpaths)
            {
                var p = Path.Combine(basePath, pair.Key);
                LabelPaths.Add(p, new List<string>());
                foreach (var value in pair.Value)
                {
                    LabelPaths[p].Add(Path.Combine(basePath, value));
                }
            }

            foreach(var x in ImagePaths)
            {
                if(!File.Exists(x)) throw new FileNotFoundException("File not found", x);
            }

            HObject image;
            HOperatorSet.ReadImage(out image, Path.Combine(basePath, imagepaths.First()));
            image.Dispose();
        }

        private List<string> ImagePaths { get; set; }
        private Dictionary<string, List<string>> LabelPaths { get; set; }

        public override ReferenceImage this[int i]
        {
            get
            {
                var imagePath = ImagePaths[i];
                var labelPaths = LabelPaths[imagePath];

                if (labelPaths.All(x => x.EndsWith(".hobj")))
                {
                    return new HObjectReferenceImage(imagePath, labelPaths, ImageResize);
                }
                else if (labelPaths.All(x => x.EndsWith(".png") || x.EndsWith(".bmp")))
                {
                    return new IndexedReferenceImage(imagePath, labelPaths, ImageResize);
                }

                throw new NotImplementedException("Only indexed images (all labels end with .png) or hobjects (.hobj) are supported.");
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
