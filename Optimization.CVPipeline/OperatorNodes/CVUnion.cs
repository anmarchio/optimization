using System;
using System.Collections.Generic;
using System.Linq;
using Emgu.CV;
using Optimization.Pipeline;

namespace Optimization.CVPipeline.OperatorNodes
{
    [Serializable]
    public class CVUnion : CVNode
    {
        public CVUnion() : base()
        {

        }

        public CVUnion(params CVNode[] children) : base(children)
        {

        }

        public CVUnion(List<CVNode> children, float[] parameters) : base(children, parameters)
        {

        }

        public override int CGPInputCount
        {
            get
            {
                return 2;
            }
        }

        public override List<float>[] CGPParameterBounds
        {
            get
            {
                return GetCGPParameterBounds;
            }
        }

        public List<float>[] GetCGPParameterBounds
        {
            get
            {
                return new List<float>[0];
            }
        }


        public override OperatorType OperatorType
        {
            get
            {
                return OperatorType.RegionToRegion;
            }
        }

        
        /// <summary>
        /// Note that input is ignored... this may cause unexpected behavior
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override UMat Execute(UMat input)
        {
            //UMat output = new UMat();
            if (output == null)
            {
                output = (Children.First() as CVNode).Output;
                for (int i = 1; i < Children.Count; i++)
                    output = output.Union((Children[i] as CVNode).Output);
            }
            if (!output.IsBinary())
                output = output.Binary();
            return output;
        }

        public override void FromCGPNodeParameters(float[] parameters)
        {
            return;
        }

        public override float[] ToCGPNodeParameters()
        {
            return new float[] { };
        }
    }
}
