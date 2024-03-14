using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class BinomialFilter : HalconOperatorNode
    {
        public int MaskHeight { get; set; } = 3;
        public int MaskWidth { get; set; } = 3;

        public BinomialFilter(List<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            FromCGPNodeParameters(parameters);
        }
        public BinomialFilter() : base() { }

        public BinomialFilter(HalconOperatorNode child, int maskWidth, int maskHeight) : base(child)
        {

            Initialize(maskWidth, maskHeight);
        }        

        public void Initialize(int maskWidth, int maskHeight)
        {
            MaskWidth = maskWidth;
            MaskHeight = maskHeight;
        }

        public override int CGPInputCount
        {
            get
            {
                return 1;
            }
        }

        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if (cgpParameterBounds == null)
                {
                    cgpParameterBounds = new List<float>[2];
                    cgpParameterBounds[0] = new List<float>();
                    int[] primes = new int[] { 1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35, 37 };
                    for (int i = 0; i < primes.Length; i++) cgpParameterBounds[0].Add(primes[i]);
                    cgpParameterBounds[1] = new List<float>();
                    for (int i = 0; i < primes.Length; i++) cgpParameterBounds[1].Add(primes[i]);
                }
                return cgpParameterBounds;
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToImage;
            }
        }

        /// <summary>
        /// Author: braml
        /// Function to convert FunctionCall to halcon code for export into a hdev file
        /// </summary>
        /// <returns> List of strings that represent code to be executed as .hdev file</returns>
        public override List<string> HalconFunctionCall()
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format("binomial_filter({0}, {1}, {2}, {3})", Children.First().OutputVariableName, OutputVariableName,
                MaskWidth.ToString(), MaskHeight.ToString()));

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.BinomialFilter(input, out output, MaskWidth, MaskHeight);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            MaskWidth = (int)parameters[0];
            MaskHeight = (int)parameters[1];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { MaskWidth, MaskHeight };
        }
    }
}
