using System;
using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.BgSegm;
using Optimization.HalconPipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVBackgroundSubtractorMOG : CVNode
    {

        public CVBackgroundSubtractorMOG(): base()
        {

        }

        public CVBackgroundSubtractorMOG(List<CVNode> children, float[] parameters)
        {
            History = parameters[0];
            NumMixtures = parameters[1];
            BackgroundRatio = parameters[2];
            NoiseSigma = parameters[3];
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
                return GetCGPParameterBounds();
            }
        }

        public List<float>[] GetCGPParameterBounds()
        {
            return new List<float>[]
            {
                    new List<float>()
                    {
                        200
                    }, // history
                    new List<float>()
                    {
                        2, 3, 4, 5
                    }, // nMixtures
                    new List<float>()
                    {
                        0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f
                    }, // backgroundRatio
                    new List<float>()
                    {
                        0
                    } // noiseSigma
            };
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            History = parameters[0];
            NumMixtures = parameters[1];
            BackgroundRatio = parameters[2];
            NoiseSigma = parameters[3];
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { History, NumMixtures, BackgroundRatio, NoiseSigma };
        }

        private BackgroundSubtractorMOG Subtractor { get; set; }

        public float History { get; set; } = 200;
        public float NumMixtures { get; set; } = 2;
        public float BackgroundRatio { get; set; } = 0.8f;
        public float NoiseSigma { get; set; } = 0;


        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.ImageToRegion;
            }
        }

        public override UMat Execute(UMat input)
        {
            
            if(Subtractor == null)
                Subtractor = new BackgroundSubtractorMOG((int)History, (int)NumMixtures, BackgroundRatio,
                    NoiseSigma);
            output = new UMat();
            Subtractor.Apply(input, output);

            return output;
        }
    }
}
