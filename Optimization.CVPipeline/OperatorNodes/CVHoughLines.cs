using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    
    [Serializable]
    public class CVHoughLines : CVNode
    {
        public CVHoughLines() : base()
        {

        }

        public CVHoughLines(List<CVNode> children, float[] parameters) : base(children, parameters)
        {

        }

        public override int CGPInputCount
        {
            get
            {
                return 1;
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.RegionToRegion;
            }
        }

        public override List<float>[] CGPParameterBounds
        {
            get
            {
                return GetCGPParameterBounds();
            }
        }

        public List<float>[] GetCGPParameterBounds()
        {
            return new List<float>[0];
        }

        public int threshold
        {
            get;
            set;
        } = 100;

        public override UMat Execute(UMat input)
        {
            /*
            lines = List<float[ , ]>(180); limited to values below 180
            as seen in Internet example Code
            unsuitable return type of lines to expected UMat??
            But according to the documentation lines is a: Output vector of lines. Each line is represented by a two-element vector
             -> Not Useful */
            output = new UMat();
            CvInvoke.HoughLines(input, output, 1, 3.1415926535897932384626433832795 / 180, threshold);
            return output;
            
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            threshold = (int) parameters[0];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { threshold };
        }
    }
    /*
    static class Constants
    {
        public const double PI = 3.1415926535897932384626433832795;
    }
    */
}
