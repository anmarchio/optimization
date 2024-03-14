using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Emgu.CV;
using Optimization.CVPipeline.OperatorNodes;
using Optimization.Pipeline;
using Optimization.Pipeline.Interfaces;

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
    public class CVInputNode : CVNode, IInputNode<UMat>
    {
        public CVInputNode() : base()
        {

        }

        public CVInputNode(List<CVNode> children, float[] parameters) : base(children, parameters)
        {

        }

        [XmlIgnore]
        public UMat Input
        {
            get;set;
        }

        public override UMat Execute(UMat input)
        {
            Input = input;
            return input;
        }

        public override UMat Output
        {
            get
            {
                return Input;
            }
        }

        public override UMat Execute()
        {
            try
            {
                return Input;
            }catch(Exception ex)
            {
                throw new OperatorException(this, ex);
            }
        }

        public override void ResetOutput()
        {
            // do nothing in order to avoid clearing the input image data.
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[0];
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            // do nothing
        }

        public override bool IsInputNode { get { return true; } }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.InputNode;
            }
        }

        public override List<float>[] CGPParameterBounds => new List<float>[0];

        public override int CGPInputCount
        {
            get
            {
                return 0;
            }
        }

        public float ProgramInputIdentifier
        {
            get;set;
        }

        //public override List<float>[] CGPParameterBounds => throw new NotImplementedException();
    }
}
