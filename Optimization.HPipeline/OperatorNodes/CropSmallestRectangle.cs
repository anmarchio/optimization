using System;
using System.Collections.Generic;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class CropSmallestRectangle : HalconOperatorNode
    {
        public CropSmallestRectangle():base() { }

        public CropSmallestRectangle(int minGray, int maxGray) : base()
        {
            Initialize(minGray, maxGray);
        }
        
        private void Initialize(int minGray, int maxGray)
        {
            MinGray = minGray;
            MaxGray = maxGray;
        }

        public float MinGray { get; set; }
        public float MaxGray { get; set; }

 
        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if(cgpParameterBounds == null)
                {
                    cgpParameterBounds = new List<float>[2];
                    cgpParameterBounds[0] = new List<float>();
                    for (int i = 18; i < 22; i++) cgpParameterBounds[0].Add(i);
                    cgpParameterBounds[1] = new List<float>();
                    for (int i = 255; i < 256; i++) cgpParameterBounds[1].Add(i);
                }
                return cgpParameterBounds;
            }
        }
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return cropsmallestrectangle;
            }
        }

        private HObject[] cropsmallestrectangle(HObject[] arg1, HTuple[] arg2)
        {
            HObject[] o = new HObject[arg1.Length];
            HOperatorSet.GenEmptyObj(out o[0]);
            HObject region;
            HTuple row1, column1, row2, column2;
            HOperatorSet.Threshold(arg1[0], out region, arg2[0], arg2[1]);
            HOperatorSet.SmallestRectangle1(region, out row1, out column1, out row2, out column2);
            HOperatorSet.CropRectangle1(arg1[0], out o[0], row1, column1, row2, column2);
            return o;
        }

        public override HObject Execute(HObject input)
        {
            HObject region = null;
            HTuple row1, column1, row2, column2;
            try
            {
                HOperatorSet.Threshold(input, out region, MinGray, MaxGray);
                HOperatorSet.SmallestRectangle1(region, out row1, out column1, out row2, out column2);
                HOperatorSet.CropRectangle1(input, out output, row1, column1, row2, column2);
                return output;
            }
            finally
            {
                if (region != null) region.Dispose();
            }
        }

        public override void DisposeOutput()
        {
            if (output == null) return;
            output.Dispose();
            output = null;
        }


        public override float[] ToCGPNodeParameters()
        {
            return new float[] { MinGray, MaxGray };
        }

      
        public override void FromCGPNodeParameters(float[] parameters)
        {
            MinGray = parameters[0];
            MaxGray = parameters[1];
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
        /// Author:braml
        /// Generates halcon Code by exporting Execute functionality
        /// this specific Operator has 2 or more Input nodes. Leen suggested leaving this implementation empty for now
        /// </summary>
        /// <returns></returns>
        public override List<string> HalconFunctionCall()
        {
            /*
            List<string> lines = new List<string>();

            lines.Add(String.Format("threshold ()", ));
            lines.Add(String.Format("smallest_rectangle1 ()", ));
            lines.Add(String.Format("crop_rectangle1 ()", ));

            return lines;
            */

            throw new NotImplementedException();
        }
    }
}