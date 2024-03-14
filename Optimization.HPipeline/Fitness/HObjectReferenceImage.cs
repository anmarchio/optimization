using System.Collections.Generic;
using System.IO;
using System.Linq;
using Extensions;
using HalconDotNet;
using Optimization.HPipeline.OperatorNodes;

namespace Optimization.HPipeline.Fitness
{
    public class HObjectReferenceImage : ReferenceImage
    {
        public HObjectReferenceImage(string imagePath, List<string> labelPaths, ImageSize resize = null) : base()
        {
            Image = new HImage(imagePath);
            HObject concat, tmp;
            if (labelPaths.Count > 0)
                HOperatorSet.ReadObject(out concat, labelPaths.First());
            else
                HOperatorSet.GenEmptyRegion(out concat);
            foreach(var labelPath in labelPaths)
            {
                if (concat != null)
                {
                    HOperatorSet.ReadObject(out tmp, labelPath);
                    concat = concat.ConcatObj(tmp);
                }
                else
                    HOperatorSet.ReadObject(out concat, labelPath);                
            }

            HOperatorSet.Union1(concat, out tmp);
            ReferenceRegions = concat;
            UnionReferenceRegions = tmp;
            InitializeRectangles();
            Initialize();
            Filename = Path.GetFileName(imagePath);
        }
    }
}
