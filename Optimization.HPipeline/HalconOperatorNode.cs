using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using HalconDotNet;
using Optimization.HPipeline.Interfaces;
using Optimization.HPipeline.OperatorNodes;
using Optimization.Pipeline;
using Optimization.Pipeline.Interfaces;

namespace Optimization.HPipeline
{

    #region XmlInclude Attributes required to write Xml

    [XmlInclude(typeof(AnisotropicDiffusion))]
    [XmlInclude(typeof(AreaSizeThreshold))]
    [XmlInclude(typeof(AreaToRectangle))]
    [XmlInclude(typeof(CropRectangle))]
    [XmlInclude(typeof(AutoThreshold))]
    [XmlInclude(typeof(BandpassImage))]
    [XmlInclude(typeof(BinaryThreshold))]
    [XmlInclude(typeof(BinomialFilter))]
    [XmlInclude(typeof(CloseEdges))]
    [XmlInclude(typeof(Closing))]
    [XmlInclude(typeof(CoherenceEnhancingDiff))]
    [XmlInclude(typeof(Connection))]
    [XmlInclude(typeof(CropSmallestRectangle))]
    [XmlInclude(typeof(Dilation1))]
    [XmlInclude(typeof(DualRank))]
    [XmlInclude(typeof(DynThreshold))]
    [XmlInclude(typeof(EdgesImage))]
    [XmlInclude(typeof(EdgesSubPix))]
    [XmlInclude(typeof(EliminateMinMax))]
    [XmlInclude(typeof(EquHistoImage))]
    [XmlInclude(typeof(Erosion1))]
    [XmlInclude(typeof(ExpandGray))]
    [XmlInclude(typeof(FastThreshold))]
    //[XmlInclude(typeof(FFT))]
    [XmlInclude(typeof(GaussFilter))]
    [XmlInclude(typeof(GrayClosing))]
    [XmlInclude(typeof(GrayDilation))]
    [XmlInclude(typeof(GrayErosion))]
    [XmlInclude(typeof(GrayOpening))]
    [XmlInclude(typeof(GrayRangeRect))]
    [XmlInclude(typeof(HighpassImage))]
    [XmlInclude(typeof(HistoToThresh))]
    [XmlInclude(typeof(KirschAmp))]
    [XmlInclude(typeof(Laplace))]
    [XmlInclude(typeof(LaplaceOfGauss))]
    [XmlInclude(typeof(LocalThreshold))]
    [XmlInclude(typeof(MeanImage))]
    [XmlInclude(typeof(MedianImage))]
    [XmlInclude(typeof(MedianWeighted))]
    [XmlInclude(typeof(Opening))]
    [XmlInclude(typeof(RegionGrowing))]
    [XmlInclude(typeof(Roberts))]
    [XmlInclude(typeof(SelectShape))]
    [XmlInclude(typeof(SigmaImage))]
    [XmlInclude(typeof(SmoothImage))]
    [XmlInclude(typeof(SobelAmp))]
    [XmlInclude(typeof(SobelAmpEquHistoImage))]
    [XmlInclude(typeof(Threshold))]
    [XmlInclude(typeof(ThresholdAccessChannel))]
    [XmlInclude(typeof(Union1))]
    [XmlInclude(typeof(Union2))]
    [XmlInclude(typeof(VarThreshold))]
    [XmlInclude(typeof(ZeroCrossing))]
    [XmlInclude(typeof(HalconInputNode))]

    #endregion
    [Serializable]
    public abstract class HalconOperatorNode : CGPNode<HObject>, IOutputNode<HObject>, IHalconOperator, ILegacyParameterInformant<HObject[], HTuple[], HObject[]>, IHObjectComparable, IHalconCodeProducer
    {
        public HalconOperatorNode(HalconOperatorNode node) : base(node)
        {

        }

        public HalconOperatorNode() : base()
        {
        }

        public HalconOperatorNode(params HalconOperatorNode[] children) : base(children)
        {
        }


        public HalconOperatorNode(IList<HalconOperatorNode> children) : base(children != null ? children.ToArray() : null)
        {
        }


        public HalconOperatorNode(List<HalconOperatorNode> children, float[] parameters) : base(children != null ? children.ToArray() : null)
        {
            FromCGPNodeParameters(parameters);
        }


        public override int CGPParameterCount
        {
            get
            {
                if (CGPParameterBounds == null) return 0;
                return CGPParameterBounds.Count();
            }
        }

        /// <summary>
        /// This should only be called if the operator is of type ImageToImage
        /// </summary>
        protected override void ResizeOutputImage()
        {
            var input = Children.First() as HalconOperatorNode;
            HTuple width, height;
            HOperatorSet.GetImageSize(input.Output, out width, out height);
            if (!output.IsImageType("byte", "int2", "uint2", "real"))
            {
                using (var tmp = output.ConvertToStandardType())
                {
                    output.Dispose();
                    HOperatorSet.ZoomImageSize(tmp, out output, width, height, interpolation: "nearest_neighbor");
                }            
            }
            if (output.IsNullOrEmpty())
            {
                throw new Exception();
            }
        }
    

        public virtual Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                throw new NotSupportedException("this is no longer supported. old nodes implementing and overriding this function will still work, but future nodes should not implement it");
            }
        }

        public virtual void DisposeOutput()
        {
            if (output != null) output.Dispose();
            output = null;
        }


        public virtual bool OutputEquals(HObject hobject)
        {
            HTuple isEqual;
            HOperatorSet.CompareObj(hobject, Output, 0, out isEqual);
            return isEqual;
        }


        
        public override void ResetOutput()
        {
            if (output != null) output.Dispose();
            output = null;
        }

        public class HalconOperatorTypeEqualityComparer : EqualityComparer<HalconOperatorNode>
        {
            public override bool Equals(HalconOperatorNode x, HalconOperatorNode y)
            {
                return x.GetType() == y.GetType();
            }

            public override int GetHashCode(HalconOperatorNode obj)
            {
                return obj.GetType().GetHashCode();
            }
        }
       

        public class HalconOperatorEqualityComparer : EqualityComparer<HalconOperatorNode>
        {
            public override bool Equals(HalconOperatorNode x, HalconOperatorNode y)
            {
                if (x.GetType() != y.GetType()) return false;

                var properties = x.GetType().GetProperties().ToList().Where(z => z.CanRead && z.CanWrite);
                foreach (var prop in properties)
                {
                    var val1 = prop.GetValue(x);
                    var val2 = prop.GetValue(y);
                    if (val1 == null && val2 == null)
                        continue;
                    if (val1 == null ^ val2 == null)
                        return false;
                    if (val1.GetType().IsPrimitive)
                    {
                        if (val1 != val2) return false;
                    }
                }
                return true;
            }

            public override int GetHashCode(HalconOperatorNode obj)
            {
                return obj.GetType().GetHashCode();
            }
        }

        public override bool IsInputNode { get { return false; } }

        public abstract List<string> HalconFunctionCall();
    }
}
