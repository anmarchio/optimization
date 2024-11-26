using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class EdgesImage : HalconOperatorNode
    {

        public EdgesImage() : base()
        {
            Initialize(EdgesImageFilterType.canny, 0.1f, NMS.hvnms, 5, 10);
        }
        public EdgesImage(EdgesImageFilterType filter, float alpha, NMS nms, int low, int high)
        {
            Initialize(filter, alpha, nms, low, high);
        }
        public EdgesImage(IList<HalconOperatorNode> ignoreThis, float[] parameters) : base()
        {
            FromCGPNodeParameters(parameters);
        }

      
        private void Initialize(EdgesImageFilterType filter, float alpha, NMS nms, int low, int high)
        {
            //CGPParameterCount = 3;
            Alpha = alpha;
            NonMaximumSuppression = nms;
            Low = low;
            High = high;
            cgpParameterBounds = new List<float>[3];
            cgpParameterBounds[0] = new List<float>()
            {
                0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f, 1.1f, 1.2f, 1.3f
            };
            cgpParameterBounds[1] = new List<float>
            {
                5, 10, 15, 20, 25, 30, 40
            };
            cgpParameterBounds[2] = new List<float>
            {
                5, 10, 15, 20
            }; // used as offset to Low
        }
        private List<float>[] cgpParameterBounds;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if(cgpParameterBounds == null)
                {
                    cgpParameterBounds = new List<float>[3];
                    cgpParameterBounds[0] = new List<float>()
            {
                0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1.0f, 1.1f, 1.2f, 1.3f
            };
                    cgpParameterBounds[1] = new List<float>
            {
                5, 10, 15, 20, 25, 30, 40
            };
                    cgpParameterBounds[2] = new List<float>
            {
                5, 10, 15, 20
            }; // used as offset to Low
                }
                return cgpParameterBounds;
            }
        }

     
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return edgesImage;
            }
        }

        public enum EdgesImageFilterType
        {
            canny
        }
        public EdgesImageFilterType Filter { get;  set; }
        public float Alpha
        {
            get;  set;
        }

        public int Low { get;  set; }

        public int High { get;  set; }
        public enum NMS
        {
            nms, inms, hvnms, none
        }

        public NMS NonMaximumSuppression { get;  set; }

        private HObject[] edgesImage(HObject[] arg1, HTuple[] arg2)
        {
            HObject[] objects = new HObject[arg1.Length];
            HOperatorSet.EdgesImage(arg1[0], out objects[0], out objects[1], Filter.ToString(), arg2[0], NonMaximumSuppression.ToString(), arg2[1], arg2[1] + arg2[2]);
            return objects;
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
               return  OperatorType.ImageToImage;
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

            lines.Add($"edges_image ({Children.First().OutputVariableName}," +
                $" {OutputVariableName}, {"Tmp"}, '{Filter.ToString()}'," +
                $" {Alpha.ToInvariantString()}, '{NonMaximumSuppression.ToString()}'," +
                $" {Low.ToString()}, {High.ToString()})");

            return lines;
        }

        public override void DisposeOutput()
        {
            if (output != null) output.Dispose();
            output = null;
        }

        public override HObject Execute(HObject input)
        {
            HObject tmp = null;
            try
            {
                HOperatorSet.EdgesImage(input, out output, out tmp, Filter.ToString(), Alpha, NonMaximumSuppression.ToString(), Low, High);
                return Output;
            }
            finally
            {
                if(tmp != null)
                    tmp.Dispose();
            }
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Alpha, Low, High - Low };
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Initialize(EdgesImageFilterType.canny, parameters[0], NMS.nms, (int)parameters[1], (int)(parameters[1] + parameters[2]));
        }
    }
}
