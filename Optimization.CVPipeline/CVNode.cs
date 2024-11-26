using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Optimization.CVPipeline.OperatorNodes;
using Optimization.HalconPipeline;
using Optimization.HalconPipeline.Interfaces;
//using PRIME.Optimization.HPipeline;

namespace Optimization.CVPipeline
{
    #region XmlInclude Attributes required to write Xml
    [XmlInclude(typeof(CVAbsDiff))]
    [XmlInclude(typeof(CVAdaptiveThreshold))]
    [XmlInclude(typeof(CVAdd))]
    [XmlInclude(typeof(CVAddConst))]
    [XmlInclude(typeof(CVBackgroundSubtractorMOG))]
    [XmlInclude(typeof(CVBilateralFilter))]
    [XmlInclude(typeof(CVBinaryThreshold))]
    [XmlInclude(typeof(CVBlur))]
    [XmlInclude(typeof(CVBoxFilter))]
    [XmlInclude(typeof(CVCalcOpticalFlowFarneback))]
    [XmlInclude(typeof(CVCanny))]
    [XmlInclude(typeof(CVClose))]
    [XmlInclude(typeof(CVConnectedComponents))]
    [XmlInclude(typeof(CVConnectedComponentsWithStats))]
    [XmlInclude(typeof(CVCornerHarris))]
    [XmlInclude(typeof(CVDct))]
    [XmlInclude(typeof(CVDecolor))]
    [XmlInclude(typeof(CVDilate))]
    [XmlInclude(typeof(CVDivide))]
    [XmlInclude(typeof(CVDivideConst))]
    //[XmlInclude(typeof(CVEigen))]
    [XmlInclude(typeof(CVEdgePreservingFilter))]
    [XmlInclude(typeof(CVEqualizeHist))]
    [XmlInclude(typeof(CVErode))]
    [XmlInclude(typeof(CVExp))]
    [XmlInclude(typeof(CVFastNlMeansDenoisingColored))]
    [XmlInclude(typeof(CVFilter2D))]
    [XmlInclude(typeof(CVFilterSpeckles))]
    [XmlInclude(typeof(CVForwardFourier))]
    [XmlInclude(typeof(CVGaussianBlur))]
    [XmlInclude(typeof(CVGrabCut))]
    [XmlInclude(typeof(CVHoughCircles))]
    [XmlInclude(typeof(CVHoughLines))]
    [XmlInclude(typeof(CVInverseFourier))]
    [XmlInclude(typeof(CVInvert))]
    [XmlInclude(typeof(CVkMeans))]
    [XmlInclude(typeof(CVLaplacian))]
    [XmlInclude(typeof(CVLog))]
    [XmlInclude(typeof(CVMax))]
    [XmlInclude(typeof(CVMedianBlur))]
    [XmlInclude(typeof(CVMin))]
    [XmlInclude(typeof(CVMorphologyBlackHat))]
    [XmlInclude(typeof(CVMorphologyClose))]
    [XmlInclude(typeof(CVMorphologyDilate))]
    [XmlInclude(typeof(CVMorphologyErode))]
    [XmlInclude(typeof(CVMorphologyGradient))]
    [XmlInclude(typeof(CVMorphologyOpen))]
    [XmlInclude(typeof(CVMorphologyTopHat))]
    [XmlInclude(typeof(CVMultiply))]
    [XmlInclude(typeof(CVMultiplyConst))]
    [XmlInclude(typeof(CVNone))]
    [XmlInclude(typeof(CVNormalize))]
    [XmlInclude(typeof(CVOpen))]
    //[XmlInclude(typeof(CVPerspectiveTransform))]
    [XmlInclude(typeof(CVPow))]
    [XmlInclude(typeof(CVPyrDown))]
    [XmlInclude(typeof(CVPyrMeanShiftFiltering))]
    [XmlInclude(typeof(CVPyrUp))]
    [XmlInclude(typeof(CVResize))]
    [XmlInclude(typeof(CVResizeForFrame))]
    [XmlInclude(typeof(CVSobel))]
    [XmlInclude(typeof(CVSobelX))]
    [XmlInclude(typeof(CVSobelY))]
    [XmlInclude(typeof(CVSolve))]
    [XmlInclude(typeof(CVSolveCubic))]
    [XmlInclude(typeof(CVSolvePoly))]
    [XmlInclude(typeof(CVSqrt))]
    [XmlInclude(typeof(CVSubtract))]
    [XmlInclude(typeof(CVSubtractConst))]
    [XmlInclude(typeof(CVSwap))]
    [XmlInclude(typeof(CVThreshold))]
    [XmlInclude(typeof(CVUnion))]
    [XmlInclude(typeof(CVWatershed))]
    #endregion
    [Serializable]
    public abstract class CVNode : CGPNode<UMat>, IOutputNode<UMat>, IParameterInformant, IOutputDisposable
    {
        public CVNode() : base()
        {

        }

        public CVNode(params CVNode[] children) : base(children)
        {

        }

        public CVNode(List<CVNode> children, float[] parameters) : base(children != null ? children.ToArray() : null)
        {
            FromCGPNodeParameters(parameters);
        }


        public override int CGPParameterCount
        {
            get
            {
                if (CGPParameterBounds == null) return 0;
                return CGPParameterBounds.Length;
            }
        }

        protected override void ResizeOutputImage()
        {
            var input = (Children.First() as CVNode).Output;
            CvInvoke.Resize(output, output, input.Size, 0, 0, interpolation: Inter.Nearest);
        }

        public override void ResetOutput()
        {
            if (output != null)
                output.GetInputArray().Dispose();
            output = null;
        }

        public void DisposeOutput()
        {
            if (output != null) output.Dispose();
            output = null;
        }

        public override bool IsInputNode { get { return false; } }
    }

}
