using System;
using System.Collections.Generic;
using System.Linq;
using HalconDotNet;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class AnisotropicDiffusion : HalconOperatorNode
    {
        public AnisotropicDiffusion() : base()
        {
            DiffusionCoefficient = Mode.perona_malik;
            Contrast = 10;
            Theta = 1;
            Iterations = 10;
        }

        public AnisotropicDiffusion(List<HalconOperatorNode> children, float[] parameters) : base()
        {
            FromCGPNodeParameters(parameters);
        }

        public enum Mode
        {
            weickert, perona_malik, parabolic
        }

        public static Dictionary<Mode, string> Modes
        {
            get;
        } = new Dictionary<Mode, string>()
        {
            {Mode.weickert, "weickert" },
            {Mode.perona_malik, "perona-malik" }, // if only it weren't using '-'...
            {Mode.parabolic, "parabolic" }

        };

        public Mode DiffusionCoefficient { get; set; }

        public float Contrast { get; set; }

        public float Theta {get; set;}

        public float Iterations { get; set; }

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
                    new List<float>()
                    {
                       (float)Mode.weickert, (float)Mode.perona_malik, (float)Mode.parabolic
                    },
                    new List<float>()
                    {
                        2, 5, 10, 20, 50, 100
                    },
                    new List<float>()
                    {
                        0.5f, 1.0f, 3.0f
                    },
                    new List<float>()
                    {
                        1, 3, 10, 100, 500
                    }
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

            lines.Add($"anisotropic_diffusion ({Children.First().OutputVariableName}," +
                $" {OutputVariableName}, '{Modes[DiffusionCoefficient].ToString()}'," +
                $" {Contrast.ToString()}, {Theta.ToString()}, {Iterations.ToString()})");
            return lines;
        }

        public override HObject Execute(HObject input)
        {
            HOperatorSet.AnisotropicDiffusion(input, out output, Modes[DiffusionCoefficient], Contrast, Theta, Iterations);
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            DiffusionCoefficient = (Mode)parameters[0];
            Contrast = parameters[1];
            Theta = parameters[2];
            Iterations = parameters[3];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)DiffusionCoefficient, Contrast, Theta, Iterations };
        }
    }
}
