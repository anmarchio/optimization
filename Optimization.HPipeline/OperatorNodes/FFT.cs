using System;
using System.Collections.Generic;
using HalconDotNet;
using Optimization.HPipeline.Fitness.OperatorMaps;
using Optimization.Pipeline;

namespace Optimization.HPipeline.OperatorNodes
{
    [Serializable]
    public class FFT : HalconOperatorNode
    {   
        public  FFT(): base() { }
        public FFT(HalconOperatorNode child, float sigma1, float sigma2, int size, FftFilterType filterType) : base(child)
        {

            Initialize(sigma1, sigma2, size, filterType);
        }

        public FFT(IList<HalconOperatorNode> children, float sigma1, float sigma2, int size, FftFilterType filterType) : base(children)
        {
            Initialize(sigma1, sigma2, size, filterType);
        }

        public FFT(IList<HalconOperatorNode> children, float[] parameters) : base(children)
        {
            //string[] ffttypes = Enum.GetNames(typeof(FftFilterType));
            //(FftFilterType)Enum.Parse(typeof(FftFilterType), ffttypes[(int)parameters[3]])
            Initialize((float)parameters[0], (float)parameters[1], (int)parameters[2], FftFilterType.Gauss);
        }

    
        private void Initialize(float sigma1, float sigma2, int size, FftFilterType filterType)
        {
            Sigma1 = sigma1;
            Sigma2 = sigma2;
            Size = size;
            FilterType = filterType;
          
        }
        
        public float Sigma1 { get; set; }
        public float Sigma2 { get; set; }
        public int Size { get; set; }
        public FftFilterType FilterType { get; set; }

        public enum FftFilterType
        {
            Gauss, //Bandpass, //Lowpass, Highpass
        }


        private List<float>[] cgpParameterBounds = null;
        public override List<float>[] CGPParameterBounds
        {
            get
            { 
                if(cgpParameterBounds == null)
                {
                    cgpParameterBounds = new List<float>[4];
                    cgpParameterBounds[0] = new List<float>()
            {
                //0.7f, 0.8f, 0.9f, 1.0f, 1.1f, 1.2f, 1.5f, 2.0f, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f,
                //8.0f, 9.0f, 10.0f
                0.1f
            };
                    cgpParameterBounds[1] = new List<float>
            {
                //0.7f, 0.8f, 0.9f,
                //1.0f, 1.1f, 1.2f, 1.5f, 2.0f
                //, 3.0f, 4.0f, 5.0f, 6.0f, 7.0f, 8.0f, 9.0f, 10.0f
                0.2f
            };
                    cgpParameterBounds[2] = new List<float>
            {
                //3, 5, 7, 9, 11
                11
            };
                    // FftFilterType    
                    cgpParameterBounds[3] = new List<float>();
                    for (float i = 0; i < Enum.GetNames(typeof(FftFilterType)).Length; i++) cgpParameterBounds[3].Add(i);
                }
                return cgpParameterBounds;
            }
        }
        public override Func<HObject[], HTuple[], HObject[]> EvaluationFunction
        {
            get
            {
                return fft;
            }
        }

        private HObject[] fft(HObject[] arg1, HTuple[] arg2)
        {
            HObject[] o = new HObject[arg1.Length];
            double a2_0 = arg2[0].D;
            double a2_1 = arg2[1].D;
            float a = Convert.ToSingle(a2_0);
            float b = Convert.ToSingle(a2_1);
            int c = (int)arg2[2].D;
            if (FilterType == FftFilterType.Gauss)
            {
                CGPOperatorSet.GaussFFT(arg1[0], out o[0], a, b);
            }
            /* uncommented this due to inconsistent behavior: BandpassFFT returns an image, GaussFFT returns a region. Either unify behavior or split into two operators.
            else if (FilterType == FftFilterType.Bandpass)
            {
                CGPOperatorSet.BandpassFFT(arg1[0], out o[0], a, b, c);
            }*/ 
            return o;
        }

        public override HObject Execute(HObject input)
        {
            if (FilterType == FftFilterType.Gauss)
            {
                CGPOperatorSet.GaussFFT(input, out output, Sigma1, Sigma2);
            }
            /* see comment above
            else if (FilterType == FftFilterType.Bandpass)
            {
                CGPOperatorSet.BandpassFFT(input, out output, Sigma1, Sigma2, Size);
            }*/
            else
            {
                throw new NotImplementedException("finding this took me quite some time T_T; uncommented not implemented filters above");
            }

            if (output == null) throw new Exception("Something went wrong");

            return output;
        }

        public override void DisposeOutput()
        {
            if (output == null) return;
            output.Dispose();
            output = null;
        }



        public override float[] ToCGPNodeParameters()
        {
            return new float[] { (float)Sigma1, (float)Sigma2, (int)Size, (float)FilterType };
        }

     
        public override void FromCGPNodeParameters(float[] parameters)
        {
            Sigma1 = parameters[0];
            Sigma2 = parameters[1];
            Size = (int)parameters[2];
            FilterType = (FftFilterType) parameters[3];

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

        public override List<string> HalconFunctionCall()
        {
            throw new NotImplementedException();
        }
    }
}