using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVSobelX : CVNode
    {
        public CVSobelX() : base()
        {

        }
        public CVSobelX(int xOrder, int yOrdner, int apertureSize) : base()
        {
            XOrder = xOrder;
            ApertureSize = apertureSize;
        }

        public CVSobelX(List<CVNode> children, float[] parameters) : base(children, parameters) // children not used, but needed for Activator.CreateInstance
        {
        }

        public int XOrder { get; set; }
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
            return new List<float>[2]
            {
                    new List<float>() // order of derivative in x dir
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
            output = Sobel(input, new float[] { XOrder, 0, ApertureSize });
            return output;
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { XOrder, ApertureSize };
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            XOrder = (int)parameters[0];
            ApertureSize = (int)parameters[1];
        }
    }
}
