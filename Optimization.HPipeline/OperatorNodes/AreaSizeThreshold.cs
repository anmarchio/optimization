using System;
using System.Collections.Generic;
using HalconDotNet;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.HalconPipeline;
using System.Diagnostics.Contracts;
using System.Linq;
using Newtonsoft.Json.Linq;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.Emit;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class AreaSizeThreshold : HalconThresholdNode
    {
        public AreaSizeThreshold() { }

        /// <summary>
        /// Splits the image into regions and thresholds them according to size
        /// </summary>
        /// <param name="child">Input</param>
        /// <param name="minGray">Min Gray Value</param>
        /// <param name="maxGray">Max Gray Value</param>
        /// <param name="minSize">Min Size</param>
        /// <param name="maxSize">Max Size</param>
        /// <param name="windowWidth">Window width</param>
        /// <param name="windowHeight">Window height</param>
        /*
        public AreaSizeThreshold(HalconOperatorNode child, int minGray, int maxGray, int minSize, int maxSize, int windowWidth, int windowHeight) : base(child)
        {

            Initialize(minGray, maxGray, minSize, maxSize, windowWidth, windowHeight);
        }

        public AreaSizeThreshold(IList<HalconOperatorNode> children, int minGray, int maxGray, int minSize, int maxSize, int windowWidth, int windowHeight) : base(children)
        {
            Initialize(minGray, maxGray, minSize, maxSize, windowWidth, windowHeight);
        }

        public AreaSizeThreshold(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            Initialize((int)parameters[0], (int)parameters[1], (int)parameters[2], (int)parameters[3], (int)parameters[4], (int)parameters[5]);
        }*/

     
        private void Initialize(int minGray, int maxGray, int minSize, int maxSize, int windowWidth, int windowHeight)
        {
            MinGray = minGray;
            MaxGray = maxGray;
            MinSize = minSize;
            MaxSize = maxSize;
            WindowWidth = windowHeight;
            WindowHeight = windowWidth;
            
        }

        public float MinGray { get; set; } = 15;
        public float MaxGray { get; set; } = 230;
        public float MinSize { get; set; } = 9000;
        public float MaxSize { get; set; } = 18000;
        public float WindowWidth { get; set; } = 160;
        public float WindowHeight { get; set; } = 160;


        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if(cgpParameterBounds == null)
                {
                    cgpParameterBounds = new List<float>[6];
                    cgpParameterBounds[0] = new List<float>();
                    cgpParameterBounds[1] = new List<float>();
                    cgpParameterBounds[2] = new List<float>();
                    cgpParameterBounds[3] = new List<float>();
                    cgpParameterBounds[4] = new List<float>();
                    cgpParameterBounds[5] = new List<float>();
                    for (int i = 15; i < 30; i++) cgpParameterBounds[0].Add(i);
                    for (int i = 230; i < 256; i++) cgpParameterBounds[1].Add(i);
                    for (int i = 9000; i < 11000; i = i + 1000) cgpParameterBounds[2].Add(i);
                    for (int i = 18000; i < 22000; i = i + 1000) cgpParameterBounds[3].Add(i);
                    for (int i = 160; i < 321; i = i + 10) cgpParameterBounds[4].Add(i);
                    for (int i = 160; i < 321; i = i + 10) cgpParameterBounds[5].Add(i);
                }
                return cgpParameterBounds;
            }
        }
        
        public override float[] ToCGPNodeParameters()
        {
            return new float[] { MinGray, MaxGray, MinSize, MaxSize, WindowWidth, WindowHeight };
        }
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return areasizethreshold;
            }
        }

        private HObject[] areasizethreshold(HObject[] arg1, HTuple[] arg2)
        {
            HObject[] o = new HObject[arg1.Length];
            CGPOperatorSet.AreaSizeThreshold(arg1[0], out o[0], arg2[0], arg2[1], arg2[2], arg2[3], arg2[4], arg2[5]);
            return o;
        }

        public override HObject Execute(HObject input)
        {
            using (var abs = base.Execute(input))
            {
                CGPOperatorSet.AreaSizeThreshold(abs, out output, MinGray, MaxGray, MinSize, MaxSize, WindowWidth, WindowHeight);
                return output;
            }
        }

        public override void DisposeOutput()
        {
            if (output == null) return;
            output.Dispose();
            output = null;
        }

      
        public override void FromCGPNodeParameters(float[] parameters)
        {
            MinGray = parameters[0];
            MaxGray = parameters[1];
            MinSize = parameters[2];
            MaxSize = parameters[3];
            WindowWidth = parameters[4];
            WindowHeight = parameters[5];
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
                return OperatorType.ImageToRegion;
            }
        }

        public override List<string> HalconFunctionCall()
        {

            List<string> lines = new List<string>();            

            lines.Add($"MinGray:= {MinGray.ToString()}");
            lines.Add($"MaxGray:= {MaxGray.ToString()}");
            lines.Add($"*AreaSizeThreshold");
            lines.Add($"abs_image({Children.First().OutputVariableName}, Image)");
            lines.Add($"");
            lines.Add($"gen_empty_region(FaultyRegion)");
            lines.Add($"gen_empty_region(TempRegion)");
            lines.Add($"");
            lines.Add($"get_image_size({Children.First().OutputVariableName}, Width, Height)");
            lines.Add($"");
            lines.Add($"I_W:= Width / {WindowWidth.ToString()}");
            lines.Add($"I_H:= Height / {WindowHeight.ToString()}");
            lines.Add($"");
            lines.Add($"for i := 0 to I_W by 1");
            lines.Add($"for j := 0 to I_H by 1");
            lines.Add($"Row1 := j * {WindowHeight.ToString()}");
            lines.Add($"Col1 := i * {WindowHeight.ToString()}");
            lines.Add($"Row2 := j * {WindowHeight.ToString()} + {WindowHeight.ToString()}");
            lines.Add($"Col2 := i * {WindowHeight.ToString()} + {WindowHeight.ToString()}");
            lines.Add($"");
            lines.Add($"if (Row2 > Height)");
            lines.Add($"Row2:= Height");
            lines.Add($"endif");
            lines.Add($"");
            lines.Add($"if (Col2 > Width)");
            lines.Add($"Col2:= Width");
            lines.Add($"endif");
            lines.Add($"");
            lines.Add($"if (Row1 > Height)");
            lines.Add($"Row1:= Height - 1");
            lines.Add($"endif");
            lines.Add($"");
            lines.Add($"if (Col1 > Width)");
            lines.Add($"Col1:= Width - 1");
            lines.Add($"endif");
            lines.Add($"");
            lines.Add($"crop_rectangle1({Children.First().OutputVariableName}, ImagePart, Row1, Col1, Row2, Col2)");
            lines.Add($"threshold(ImagePart, Threads, 40, 255)");
            lines.Add($"area_center(Threads, AreaSize, Row, Col)");
            lines.Add($"");
            lines.Add($"if (AreaSize < {MaxSize.ToString()} and AreaSize > {MinSize.ToString()})");
            lines.Add($"gen_rectangle1(TempRegion, Row1, Col1, Row2, Col2)");
            lines.Add($"union2(TempRegion, FaultyRegion, FaultyRegion)");
            lines.Add($"endif");
            lines.Add($"");
            lines.Add($"smallest_rectangle1(FaultyRegion, Row1, Col1, Row2, Col2)");
            lines.Add($"region_features(FaultyRegion, 'area', Value)");
            lines.Add($"endfor");
            lines.Add($"endfor");
            lines.Add($"");
            lines.Add($"count_obj(FaultyRegion, Number)");
            lines.Add($"if (Number > 0)");
            lines.Add($"{OutputVariableName}:= FaultyRegion");
            lines.Add($"else");
            lines.Add($"gen_empty_region({OutputVariableName})");
            lines.Add($"endif");
            return lines;
        }
    }
}