using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public sealed class DualRank : HalconOperatorNode
    {
        public DualRank() : base() { }

        public DualRank(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            FromCGPNodeParameters(parameters);
        }

        public DualRankMaskType MaskType { get; set; } = DualRankMaskType.square;
        public int Radius { get; set; } = 1;
        public int ModePercent { get; set; } = 20;
        public DualRankMargin Margin { get; set; } = DualRankMargin.mirrored;
        public enum DualRankMaskType
        {
            circle, square
        }
        public enum DualRankMargin
        {
            mirrored,
            cyclic,
            continued,
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

        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            {
                if (cgpParameterBounds == null)
                {
                    cgpParameterBounds = new List<float>[4];
                    for (int i = 0; i < cgpParameterBounds.Length; i++) cgpParameterBounds[i] = new List<float>();
                    cgpParameterBounds[0] = Enum.GetValues(typeof(DualRankMaskType)).Cast<DualRankMaskType>().ToList().Select(x => (float)x).ToList();
                    for (int i = 1; i <= 101; i++) { cgpParameterBounds[1].Add(i); } // radius
                    for (int i = 0; i <= 100; i++) { cgpParameterBounds[2].Add(i); } // modepercent
                    cgpParameterBounds[3] = Enum.GetValues(typeof(DualRankMargin)).Cast<DualRankMargin>().ToList().Select(x => (float)x).ToList();
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

            lines.Add($"mirrored := '{Margin.ToString()}'");
            lines.Add($"if (mirrored == '{DualRankMargin.continued.ToString()}' " +
                $"or mirrored == '{DualRankMargin.cyclic.ToString()}' " +
                $"or mirrored == '{DualRankMargin.mirrored.ToString()}' )");
            lines.Add($"dual_rank ({Children.First().OutputVariableName}, {OutputVariableName}," +
                $" '{MaskType.ToString()}', {Radius.ToString()}, {ModePercent.ToString()}," +
                $" mirrored)");
            lines.Add($"else");
            lines.Add($"dual_rank ({Children.First().OutputVariableName}, {OutputVariableName}," +
                $" '{MaskType.ToString()}', {Radius.ToString()}, {ModePercent.ToString()}," +
                $" {((int)Margin).ToString()})");
            lines.Add($"endif");

            return lines;
        }

        public override HObject Execute(HObject input)
        {
            if (Margin == DualRankMargin.continued || Margin == DualRankMargin.cyclic || Margin == DualRankMargin.mirrored)
                HOperatorSet.DualRank(input, out output, MaskType.ToString(), Radius, ModePercent, Margin.ToString());
            else
                HOperatorSet.DualRank(input, out output, MaskType.ToString(), Radius, ModePercent, (int)Margin);

            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            MaskType = (DualRankMaskType)parameters[0];
            Radius = (int)parameters[1];
            ModePercent = (int)parameters[2];
            Margin = (DualRankMargin)parameters[3];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float) MaskType, Radius, ModePercent, (float) Margin };
        }
    }
}
