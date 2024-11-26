using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class EdgesSubPix : HalconOperatorNode
    {
        public EdgesSubPix() : base() { }
        
        public EdgesSubPix(HalconOperatorNode child, EdgesSubPixFilterTypes Filter, float alpha, int low, int high) : base(child)
        {
            Initialize(Filter, Alpha, Low, High);
        }

        public EdgesSubPix(IList<HalconOperatorNode> children, EdgesSubPixFilterTypes Filter, float alpha, int low, int high) : base(children)
        {
            Initialize(Filter, Alpha, Low, High);
        }

        public EdgesSubPix(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            Initialize((EdgesSubPixFilterTypes)parameters[0], (float)parameters[1], (int)parameters[2], (int)parameters[3]);
        }
        
        private void Initialize(EdgesSubPixFilterTypes filter, float alpha, int low, int high)
        {
            Filter = filter;
            Alpha = alpha;
            Low = low;
            High = high;
        }

        public EdgesSubPixFilterTypes Filter { get; set; }
        public float Alpha { get; set; } = 0.1f;
        public int Low { get; set; } = 20;
        public int High { get; set; } = 40;
        public enum EdgesSubPixFilterTypes
        {
            canny, canny_junctions, deriche1,
            deriche1_junctions, deriche2, deriche2_junctions,
            lanser1, lanser1_junctions, lanser2, lanser2_junctions,
            mshen, mshen_junctions, shen, shen_junctions,
            sobel, sobel_fast, sobel_junctions
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
                    cgpParameterBounds = new List<float>[4];
                    cgpParameterBounds[0] = new List<float>();
                    for (float i = 0.1f; i <= 50.0f; i = i + 0.1f) cgpParameterBounds[0].Add(i); // alpha
                    cgpParameterBounds[1] = new List<float>();
                    for (int i = 5; i <= 120; i += 5) cgpParameterBounds[1].Add(i); // low
                    cgpParameterBounds[2] = new List<float>();
                    for (int i = 10; i <= 135; i += 5) cgpParameterBounds[2].Add(i); // high offset to low? does not quite look like it...
                    cgpParameterBounds[3] = Enum.GetValues(typeof(EdgesSubPixFilterTypes)).Cast<EdgesSubPixFilterTypes>().ToList().Select(x => (float)x).ToList();
                }
                return cgpParameterBounds;
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToXLDContData;
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

            lines.Add(String.Format("edges_sub_pix ({0}, {1}, {2}, {3}, {4}, {5})", Children.First().OutputVariableName, OutputVariableName,
                Filter.ToString(), Alpha.ToInvariantString(), Low.ToString(), High.ToString()));

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.EdgesSubPix(input, out output, Filter.ToString(), Alpha, Low, Low + High);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Alpha = parameters[0];
            Low = (int)parameters[1];
            High = (int)parameters[2];
            Filter = (EdgesSubPixFilterTypes) parameters[3];

        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Alpha, Low, High, (float)Filter};
        }
    }
}
