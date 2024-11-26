using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVSobel : CVNode
    {
        public CVSobel() : base()
        {

        }
        public CVSobel(int xOrder, int yOrdner, int apertureSize) : base()
        {
            XOrder = xOrder;
            YOrder = yOrdner;
            ApertureSize = apertureSize;
        }

        public CVSobel(List<CVNode> children, float[] parameters) : base(children, parameters) // children not used, but needed for Activator.CreateInstance
        {
        }

        public int XOrder { get; set; }
        public int YOrder { get; set; }
        public int ApertureSize { get; set; }

        public override List<float>[] CGPParameterBounds
        {
            get
            {
                return GetCGPParameterBounds();
            }
        }

        public List<float>[] GetCGPParameterBounds()
        {
            return new List<float>[3]
            {
                    new List<float>() // order of derivative in x dir
                    {
                        1, 2
                    },
                    new List<float>() // order of derivative in y dir
                    {
                        1, 2
                    },
                    new List<float>() // aperture size
                    {
                        3, 5, 7
                    }
            };
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
            }
        }

        public override int CGPInputCount
        {
            get
            {
                return 1;
            }
        }

        private static UMat Sobel(UMat input, float[] parameter)
        {
            var output = new UMat();
            CvInvoke.Sobel(input, output, input.GetInputArray().GetDepth(), (int)parameter[0], (int)parameter[1], (int)parameter[2]);//XOrder, YOrder, ApertureSize);
            return output;
        }

        public override UMat Execute(UMat input)
        {
           output = Sobel(input, new float[] { XOrder, YOrder, ApertureSize });
           return output;
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { XOrder, YOrder, ApertureSize };
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            XOrder = (int)parameters[0];
            YOrder = (int)parameters[1];
            ApertureSize = (int)parameters[2];
        }
    }
}
