using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using HalconDotNet;
using Optimization.HalconPipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class DynThreshold : HalconOperatorNode
    {
        public DynThreshold() : base() { }

        // static void HOperatorSet.DynThreshold(HObject origImage, HObject thresholdImage, out HObject regionDynThresh, HTuple offset, HTuple lightDark)
        /*
        public DynThreshold(HalconOperatorNode child, int offset, DynThresholdLightDark lightDark) : base(child)
        {
            Initialize(offset, lightDark);
        }

        public DynThreshold(IList<HalconOperatorNode> children, int offset, DynThresholdLightDark lightDark) : base(children)
        {
            Initialize(offset, lightDark);
        }

        public DynThreshold(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            Initialize((int)parameters[0], (DynThresholdLightDark)parameters[1]);
        }*/

        private void Initialize(int offset, DynThresholdLightDark lightDark)
        {
            Offset = offset;
            LightDark = lightDark;
        }

        public int Offset { get; set; } = 5;
        public DynThresholdLightDark LightDark { get; set; }
        public enum DynThresholdLightDark
        {
            dark, equal, light, not_equal
        }
        public override int CGPInputCount
        {
            get
            {
                return 2;
            }
        }

        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if (cgpParameterBounds == null)
                {
                    cgpParameterBounds = new List<float>[1];
                    cgpParameterBounds[0] = new List<float>();
                    for (float i = -255.01f; i < 255.0f; i += 5) cgpParameterBounds[0].Add(i);
                }
                return cgpParameterBounds;
            }
        }

        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.InputImageAndRegionToRegion;
            }
        }

        /// <summary>
        /// Author:braml
        /// Generates halcon Code by exporting Execute functionality
        /// </summary>
        /// <returns>List of strings that represent code to be executed as .hdev file</returns>
        public override List<string> HalconFunctionCall()
        {
            List<string> lines = new List<string>();

            lines.Add(GatherThresholdImageHalconText());
            
            lines.Add($"dyn_threshold (OrigImage," +
                $"{Children.First().OutputVariableName}, {OutputVariableName}, " +
                $"{Offset.ToString()}, {LightDark.ToString()})");

            return lines;
        }
        private string GatherThresholdImageHalconText()
        {
            string threshImageString = "copy_image(Image, OrigImage)";

            var thresh = Children.FirstOrDefault(x => x.IsOrOperatorType(OperatorType.ImageToImage));

            if (thresh == null)
                threshImageString = "throw(['DynThreshold expects one child of type OperatorType.ImageToImage that is not also a Threshold'])";
            

            return threshImageString;
        }

        private HObject GatherThresholdImage()
        {
            var thresh = Children.FirstOrDefault(x => x.IsOrOperatorType(OperatorType.ImageToImage));
            if (thresh == null) throw new Exception("The threshold image is something like the median filter (unclear if sobel works)");
            return (thresh as HalconOperatorNode).Output;
        }

        private HObject GatherOriginalImage()
        {
            var original = Children.FirstOrDefault(x => x.IsOrOperatorType(OperatorType.InputNode));
            if (original == null) throw new Exception("DynThreshold expects one child of type OperatorType.ImageToImage that is not also a Threshold");
            return (original as HalconOperatorNode).Output;
        }

        public override HObject Execute()
        {
            try
            {
                return Execute(GatherThresholdImage());
            }catch(Exception ex)
            {
                throw new OperatorException(this, ex);
            }
        }

        /// <summary>
        /// input is expected to correspond to parameter thresholdImage
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override HObject Execute(HObject input)
        {
            HOperatorSet.DynThreshold(GatherOriginalImage(), input, out output, Offset, LightDark.ToString());
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Offset = (int)parameters[0];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Offset };
        }
    }
}
