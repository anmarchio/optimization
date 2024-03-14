using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class GrayRangeRect : HalconOperatorNode
    {
        public GrayRangeRect() : base() { }
        public GrayRangeRect(HalconOperatorNode child, int maskWidth, int maskHeight) : base(child)
        {

            Initialize(maskWidth, maskHeight);
        }

        public GrayRangeRect(IList<HalconOperatorNode> children, int maskWidth, int maskHeight) : base(children)
        {
            Initialize(maskWidth, maskHeight);
        }

        public GrayRangeRect(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            Initialize((int)parameters[0], (int)parameters[1]);
        }

     
        private void Initialize(int maskWidth, int maskHeight)
        {
            MaskWidth = maskWidth;
            MaskHeight = maskHeight;
        }

        public int MaskHeight { get; set; } = 3;
        public int MaskWidth { get; set; } = 3;


        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if (cgpParameterBounds == null)
                {
                    cgpParameterBounds = new List<float>[2];
                    cgpParameterBounds[0] = new List<float>();
                    for (int i = 3; i <= 511; i++) cgpParameterBounds[0].Add(i);
                    cgpParameterBounds[1] = new List<float>();
                    for (int i = 3; i <= 511; i++) cgpParameterBounds[1].Add(i);
                }
                return cgpParameterBounds;
            }
        }
        /*
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return grayrangerect;
            }
        }

        private HObject[] grayrangerect(HObject[] arg1, HTuple[] arg2)
        {
            HObject[] o = new HObject[arg1.Length];
            HOperatorSet.GrayRangeRect(arg1[0], out o[0], arg2[0], arg2[1]);
            return o;
        }*/

        public override HObject Execute(HObject input)
        {
            HOperatorSet.GrayRangeRect(input, out output, MaskHeight, MaskWidth);
            return output;
        }

        public override void DisposeOutput()
        {
            if (output == null) return;
            output.Dispose();
            output = null;
        }

         public override float[] ToCGPNodeParameters()
        {
            return new float[] { MaskHeight, MaskWidth};
        }

     
        public override void FromCGPNodeParameters(float[] parameters)
        {
            MaskHeight = (int)parameters[0];
            MaskWidth = (int)parameters[1];
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

            lines.Add(string.Format("gray_range_rect({0}, {1}, {2}, {3})", Children.First().OutputVariableName, OutputVariableName,
                MaskHeight.ToString(), MaskWidth.ToString()));

            return lines;
        }
    }
}