using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class GaussFilter : HalconOperatorNode
    {
        public GaussFilter() : base() { }

        public GaussFilter(List<HalconOperatorNode> children, float[] parameters) : base(children, parameters)
        {

        }


        public float MaskSize { get; set; } = 5;

        public override List<float>[] CGPParameterBounds
        {
            get
            {
                return new List<float>[]
                {
                    new List<float> {3, 5, 7, 9, 11} // mask size
                };
            }
        }
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get { throw new NotImplementedException(); }
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.GaussFilter(input, out output, MaskSize);
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
            return new float[] { MaskSize };
        }


        public override void FromCGPNodeParameters(float[] parameters)
        {
            MaskSize = parameters[0];
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
        /// Workaround for imageandregiontoregion: search for input image
        /// </summary>
        /// <returns></returns>
        ///  /// <summary>
        /// Author: braml
        /// Function to convert FunctionCall to halcon code for export into a hdev file
        /// </summary>
        /// <returns> List of strings that represent code to be executed as .hdev file</returns>
        public override List<string> HalconFunctionCall()
        {
            List<string> lines = new List<string>();

            lines.Add(String.Format("gauss_filter ({0}, {1}, {2})",Children.First().OutputVariableName, OutputVariableName, MaskSize.ToString()));

            return lines;
        }
    }
}