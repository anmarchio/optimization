using System;
using System.Collections.Generic;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVSobelY : CVNode
    {
        public CVSobelY() : base()
        {

        }
        public CVSobelY(int xOrder, int yOrdner, int apertureSize) : base()
        {
            yOrder = yOrder;
            ApertureSize = apertureSize;
        }

        public CVSobelY(List<CVNode> children, float[] parameters) : base(children, parameters) // children not used, but needed for Activator.CreateInstance
        {
        }

        public int yOrder { get; set; }
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
            output = Sobel(input, new float[] { 0, yOrder, ApertureSize });
            return output;
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { yOrder, ApertureSize };
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            yOrder = (int)parameters[0];
            ApertureSize = (int)parameters[1];
        }
    }
}
