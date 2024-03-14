using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class MedianImage : HalconOperatorNode
    {
        public MedianImage() : base() { }

        public MedianImage(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            FromCGPNodeParameters(parameters);
        }

        public MedianImageMaskType MaskType { get; set; }
        public int Radius { get; set; } = 1;
        public MedianImageMargin Margin { get; set; }
        public enum MedianImageMaskType
        {
            circle, square
        }
        public enum MedianImageMargin
        {
            mirrored = 1,
            cyclic = 2,
            continued = 3, 
            //0, 30, 60, 90, 120, 150, 180, 210, 240, 255
            Zero = 0,
            Thirty = 30,
            Sixty = 60,
            Ninety = 90,
            OneTwenty = 120,
            OneFifty = 150,
            OneEighty = 180,
            TwoTen = 210,
            TwoForty = 240,
            TwoFifityFive = 255
        }
        public override int CGPInputCount
        {
            get
            {
                return 1;
            }
        }

        public override List<float>[] CGPParameterBounds
        {
            get
            {
                return new List<float>[]
                {
                    Enumerable.Range(1, 59).Select(x => (float)x).ToList(), // radius, used to be max value >4000, which causes horrendous execution times (and makes little sense)
                    Enum.GetValues(typeof(MedianImageMaskType)).Cast<MedianImageMaskType>().ToList().Select(x => (float)x).ToList(),
                    Enum.GetValues(typeof(MedianImageMargin)).Cast<MedianImageMargin>().ToList().Select(x => (float)x).ToList()
                };
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

            lines.Add($"get_image_size ({Children.First().OutputVariableName}, Width, Height)");
            lines.Add($"Width := Width/2");
            lines.Add($"Height := Height/2");

            lines.Add($"if ( Width >= {Radius.ToInvariantString()})");
            lines.Add($"radius := {Radius.ToInvariantString()}");
            lines.Add($"else");
            lines.Add($"radius := Width - 1");
            lines.Add($"endif");

            lines.Add($"if ( Height >= {Radius.ToInvariantString()})");
            lines.Add($"radius := {Radius.ToInvariantString()}");
            lines.Add($"else");
            lines.Add($"radius := Height - 1");
            lines.Add($"endif");

            lines.Add($"margin := {(int) Margin}");
            lines.Add($"if (margin == 0 or margin > 3)");
            lines.Add($"margin := margin");
            lines.Add($"endif");

            lines.Add($"median_image ({Children.First().OutputVariableName}, {OutputVariableName}, '{MaskType.ToString()}', radius, margin)");

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            HTuple width, height;
            HOperatorSet.GetImageSize(input, out width, out height);
            width = width.I / 2;
            height = height.I / 2;
            var radius = Radius < width.I ? Radius : width.I - 1;
            radius = radius < height.I ? radius : height.I - 1;
            HTuple margin = Margin.ToString();
            if (Margin == 0 || (int)Margin > 3) margin = (int)Margin;
            HOperatorSet.MedianImage(input, out output, MaskType.ToString(), radius, margin);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            Radius = (int)parameters[0];
            MaskType = (MedianImageMaskType)parameters[1];
            Margin = (MedianImageMargin)parameters[2];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { Radius, (float) MaskType, (float) Margin };
        }
    }
}
