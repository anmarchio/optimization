using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class Laplace : HalconOperatorNode
    {
        public Laplace() : base() { }

        public Laplace(HalconOperatorNode child, LaplaceResultType resultType, int maskSize, LaplaceFilterMask filterMask) : base(child)
        {
            Initialize(ResultType, MaskSize, FilterMask);
        }

        public Laplace(IList<HalconOperatorNode> children, LaplaceResultType resultType, int maskSize, LaplaceFilterMask filterMask) : base(children)
        {
            Initialize(ResultType, MaskSize, FilterMask);
        }

        public Laplace(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            Initialize((LaplaceResultType)parameters[0], (int)parameters[1], (LaplaceFilterMask)parameters[2]);
        }

        private void Initialize(LaplaceResultType resultType, int maskSize, LaplaceFilterMask filterMask)
        {
            ResultType = resultType;
            MaskSize = maskSize;
            FilterMask = filterMask;
        }

        public override int CGPInputCount
        {
            get
            {
                return 1;
            }
        }

        public LaplaceResultType ResultType { get; set; }
        public int MaskSize { get; set; } = 3;
        public LaplaceFilterMask FilterMask { get; set; }
        public enum LaplaceResultType
        {
            absolute, absolute_binomial, signed, signed_binomial, signed_clipped, signed_clipped_binomial
        }
        public enum LaplaceFilterMask
        {
            n_4, n_8, n_8_isotropic
        }

        public override List<float>[] CGPParameterBounds
        {
            get
            {
                {
                    return new List<float>[] {
                    new List<float>
                    {
                        3, 5, 7, 9, 11, 13, 15, 17, 19, 21, 23, 25, 27, 29, 31, 33, 35, 37, 39
                    }, // MaskSize
                    Enum.GetValues(typeof(LaplaceResultType)).Cast<LaplaceResultType>().ToList().Select(x => (float)x).ToList(),
                    Enum.GetValues(typeof(LaplaceFilterMask)).Cast<LaplaceFilterMask>().ToList().Select(x => (float)x).ToList()
                };
                }
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

            lines.Add($"mask := {MaskSize}");

            lines.Add($"if ({(!ResultType.ToString().Contains("binomial")).ToInvariantString().ToLower()})");
            lines.Add($"mask := min([13, {MaskSize}])");
            lines.Add($"endif");

            lines.Add($"laplace ({Children.First().OutputVariableName}, {OutputVariableName}," +
                $" '{ResultType.ToString()}', mask, '{FilterMask.ToString()}')");
            return lines;
        }

        public override HObject Execute(HObject input)
        {
            var mask = MaskSize;
            if (!ResultType.ToString().Contains("binomial")) mask = Math.Min(mask, 13);
            HOperatorSet.Laplace(input, out output, ResultType.ToString(), mask, FilterMask.ToString());
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            MaskSize = (int)parameters[0];
            ResultType = (LaplaceResultType)parameters[1];
            FilterMask = (LaplaceFilterMask)parameters[2];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { MaskSize, (float)ResultType, (float)FilterMask };
        }
    }
}
