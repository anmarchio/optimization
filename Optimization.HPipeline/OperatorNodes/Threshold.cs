using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class Threshold : HalconThresholdNode
    {

        public Threshold() : base()
        {
            Initialize(5, 50);
        }
        public Threshold(HalconOperatorNode child, int min, int max) : base()
        {
            AddChild(child);   
            Initialize(min, max);
        }


        public Threshold(List<HalconOperatorNode> children, float[] parameters) : base(children, parameters)
        {
            Initialize((int)parameters[0], (int)parameters[1]);
        }


        private void Initialize(int min, int max)
        {
            //CGPParameterCount = 2;
            Min = min;
            Max = max;
            cgpParameterBounds = new List<float>[2];
            cgpParameterBounds[0] = new List<float>();
            for (int i = 0; i < 15; i++) cgpParameterBounds[0].Add(i * 5);
            cgpParameterBounds[1] = new List<float>();
            for (int i = 40; i < 52; i++) cgpParameterBounds[1].Add(i * 5);
        }

        public int Min
        {
            get; set;
        }
        public int Max
        {
            get; set;
        }
        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {              
                return cgpParameterBounds;
            }
        }
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return threshold;
            }
        }

        private HObject[] threshold(HObject[] arg1, HTuple[] arg2)
        {
            HObject[] objects = new HObject[arg1.Length];
            HOperatorSet.AbsImage(arg1[0], out objects[0]);
            HOperatorSet.Threshold(arg1[0], out objects[0], arg2[0], arg2[1]);
            return objects;
        }

        public override void DisposeOutput()
        {
            if (output == null) return;
            output.Dispose();
            output = null;
        }

        public override HObject Execute(HObject input)
        {
            using (var abs = base.Execute(input))
            {
                using (var conv = abs.ConvertToStandardType()) // else min max with[0, 255] does not make sense
                {
                    HOperatorSet.Threshold(conv, out output, Min, Math.Min(255, Min + Max));
                    return output;
                }
            }           
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Min, Max };
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
                return base.OperatorType | OperatorType.ImageToRegion | OperatorType.Threshold;
            }
        }

        /// <summary>
        /// Authors: mara, braml
        /// Function to convert FunctionCall to halcon code for export into a hdev file
        /// </summary>
        /// <returns></returns>
        public override List<string> HalconFunctionCall()
        {
            List<string> lines = new List<string>();
            //Abs export; after this call the image is located in OutputVariableName
            var absText = base.HalconFunctionCall();
            //Attach at the rigth position, at start
            lines.AddRange(absText);

            string convOutput = "conv_out";
            var convText = HObjectExtensions.ConvertStandardHalconText(OutputVariableName, convOutput);
            lines.AddRange(convText);

            lines.Add($"Max := min([255, {Min + Max}])");

            lines.Add($"threshold ({convOutput}, {OutputVariableName}, {Min}, {"Max"})");
            return lines;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Initialize((int)parameters[0], (int)parameters[1]);            
        }
    }
}

