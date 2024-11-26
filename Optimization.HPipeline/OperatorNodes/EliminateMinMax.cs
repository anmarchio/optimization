using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class EliminateMinMax : HalconOperatorNode
    {
        public EliminateMinMax() : base() { }
        public EliminateMinMax(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            FromCGPNodeParameters(parameters);
        }

        public int MaskWidth { get; set; } = 3;
        public int MaskHeight { get; set; } = 3;
        public int Gap { get; set; }
        public int Mode { get; set; }
        
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
                    cgpParameterBounds = new List<float>[4];
                    cgpParameterBounds[0] = new List<float>();
                    for(float i=3.0f; i <= 31; i = i + 2.0f) { cgpParameterBounds[0].Add(i); } // mask width
                    cgpParameterBounds[1] = new List<float>();
                    for (float i = 3.0f; i <= 31; i = i + 2.0f) { cgpParameterBounds[1].Add(i); } //mask height

                    cgpParameterBounds[2] = new List<float>();
                    for (float i = 1.0f; i <= 40; i++) { cgpParameterBounds[2].Add(i); }
                    cgpParameterBounds[3] = new List<float>()
                    {
                        1.0f, 2.0f, 3.0f
                    };
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

            //HTuple width, height;
            //lines.Add(String.Format("{0} := null", width.ToString()));
            //lines.Add(String.Format("{1} := null", height.ToString()));
            lines.Add(String.Format("get_image_size ({0}, Width, Height)", Children.First().OutputVariableName));
            lines.Add(String.Format("Min1 := min([Width, {0}])", MaskWidth.ToString()));
            lines.Add(String.Format("Min2 := min([Height, {0}])", MaskHeight.ToString()));
            lines.Add(String.Format("eliminate_min_max ({0}, {1}, {2}, {3}, {4}, {5})", Children.First().OutputVariableName, OutputVariableName, 
                "Min1", "Min2", Gap.ToString(), Mode.ToString()));

            return lines;

        }

        public override HObject Execute(HObject input)
        {
            HTuple width, height;
            HOperatorSet.GetImageSize(input, out width, out height);
            HOperatorSet.EliminateMinMax(input, out output, Math.Min(width, MaskWidth), Math.Min(height, MaskHeight), Gap, Mode);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            MaskWidth = (int)parameters[0];
            MaskHeight = (int)parameters[1];
            Gap = (int)parameters[2];
            Mode = (int)parameters[3];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { MaskWidth, MaskHeight, Gap, Mode };
        }
    }
}